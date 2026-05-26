using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CloudShift.Application.Common.Interfaces;
using CloudShift.Application.ProjectMappings.Models;
using CloudShift.Domain.Enums;
using Microsoft.Extensions.Logging;
using Polly;

namespace CloudShift.Worker.MigrationPlugins;

public sealed class GoogleDriveOneDriveMigrationPlugin : IProviderMigrationPlugin
{
    private const string GoogleFolderMimeType = "application/vnd.google-apps.folder";
    private const long LargeFileThresholdBytes = 50L * 1024L * 1024L;
    private const int ChunkSizeBytes = 10 * 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ITokenProtector _tokenProtector;
    private readonly ILogger<GoogleDriveOneDriveMigrationPlugin> _logger;

    public GoogleDriveOneDriveMigrationPlugin(
        HttpClient httpClient,
        ITokenProtector tokenProtector,
        ILogger<GoogleDriveOneDriveMigrationPlugin> logger)
    {
        _httpClient = httpClient;
        _tokenProtector = tokenProtector;
        _logger = logger;
    }

    public bool CanHandle(ProviderType sourceProvider, ProviderType destinationProvider)
    {
        return sourceProvider == ProviderType.GoogleDrive
            && destinationProvider == ProviderType.OneDrive;
    }

    public async Task<IReadOnlyList<MigrationFileItem>> BuildFilePlanAsync(
        MigrationRouteContext context,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var sourceToken = _tokenProtector.Unprotect(context.SourceProfile.EncryptedAccessToken);
        var rootFolderId = string.IsNullOrWhiteSpace(context.Mapping.SourcePath)
            ? "root"
            : context.Mapping.SourcePath.Trim();
        var files = new List<MigrationFileItem>();

        _logger.LogInformation(
            "Started Google Drive file discovery. JobId: {JobId}, SourceItemId: {SourceItemId}, SourceProvider: {SourceProvider}, DestinationProvider: {DestinationProvider}",
            context.Job.Id,
            rootFolderId,
            context.SourceProfile.Provider,
            context.DestinationProfile.Provider);

        await ListGoogleFolderAsync(
            sourceToken,
            rootFolderId,
            context.Mapping.DestPath,
            context.FilterConfig,
            files,
            cancellationToken);

        stopwatch.Stop();
        _logger.LogInformation(
            "Completed Google Drive file discovery. JobId: {JobId}, TotalItems: {TotalItems}, ElapsedMs: {ElapsedMs}",
            context.Job.Id,
            files.Count,
            stopwatch.ElapsedMilliseconds);

        return files;
    }

    public async Task TransferFileAsync(
        MigrationRouteContext context,
        MigrationFileItem file,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var sourceToken = _tokenProtector.Unprotect(context.SourceProfile.EncryptedAccessToken);
        var destinationToken = _tokenProtector.Unprotect(context.DestinationProfile.EncryptedAccessToken);

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["JobId"] = context.Job.Id,
            ["SourceProvider"] = context.SourceProfile.Provider,
            ["DestinationProvider"] = context.DestinationProfile.Provider,
            ["SourceItemId"] = file.SourceItemId,
            ["ItemType"] = "File",
            ["FileSizeBytes"] = file.SizeBytes
        });

        _logger.LogInformation(
            "Started file transfer. DestinationItemId: {DestinationItemId}",
            file.DestinationRelativePath);

        if (IsSkipConflictRule(context.Mapping.ConflictResolutionRule)
            && await OneDriveFileExistsAsync(destinationToken, file.DestinationRelativePath, cancellationToken))
        {
            _logger.LogWarning(
                "Skipped file transfer because destination item already exists. DestinationItemId: {DestinationItemId}",
                file.DestinationRelativePath);
            return;
        }

        await using var sourceStream = await OpenGoogleDriveDownloadStreamAsync(
            sourceToken,
            file.SourceItemId,
            cancellationToken);

        if (file.SizeBytes > LargeFileThresholdBytes || !IsOverwriteConflictRule(context.Mapping.ConflictResolutionRule))
        {
            await UploadOneDriveFileWithUploadSessionAsync(
                destinationToken,
                file,
                sourceStream,
                context.Mapping.ConflictResolutionRule,
                cancellationToken);
        }
        else
        {
            await UploadSmallOneDriveFileAsync(destinationToken, file, sourceStream, cancellationToken);
        }

        stopwatch.Stop();
        _logger.LogInformation(
            "Completed file transfer. DestinationItemId: {DestinationItemId}, ElapsedMs: {ElapsedMs}",
            file.DestinationRelativePath,
            stopwatch.ElapsedMilliseconds);
    }

    private async Task ListGoogleFolderAsync(
        string accessToken,
        string folderId,
        string destinationPath,
        FilterConfig filterConfig,
        List<MigrationFileItem> files,
        CancellationToken cancellationToken)
    {
        string? pageToken = null;

        do
        {
            var url = BuildGoogleListUrl(folderId, pageToken);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await SendCloudRequestAsync(request, cancellationToken);
            var payload = await response.Content.ReadFromJsonAsync<GoogleFileListResponse>(JsonOptions, cancellationToken)
                ?? new GoogleFileListResponse();

            foreach (var item in payload.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (filterConfig.SkipHiddenFiles && item.Name.StartsWith(".", StringComparison.Ordinal))
                {
                    continue;
                }

                if (item.MimeType == GoogleFolderMimeType)
                {
                    await ListGoogleFolderAsync(
                        accessToken,
                        item.Id,
                        CombineGraphPath(destinationPath, item.Name),
                        filterConfig,
                        files,
                        cancellationToken);

                    continue;
                }

                var sizeBytes = ParseSize(item.Size);
                if (!ShouldInclude(item.Name, sizeBytes, item.ModifiedTime, filterConfig))
                {
                    continue;
                }

                files.Add(new MigrationFileItem(
                    item.Id,
                    item.Name,
                    CombineGraphPath(destinationPath, item.Name),
                    sizeBytes,
                    item.ModifiedTime));
            }

            pageToken = payload.NextPageToken;
        }
        while (!string.IsNullOrWhiteSpace(pageToken));
    }

    private async Task<Stream> OpenGoogleDriveDownloadStreamAsync(
        string accessToken,
        string fileId,
        CancellationToken cancellationToken)
    {
        var url = $"https://www.googleapis.com/drive/v3/files/{Uri.EscapeDataString(fileId)}?alt=media&supportsAllDrives=true";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowCloudHttpExceptionAsync(response, "Google Drive download failed", cancellationToken);
        }

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private async Task UploadSmallOneDriveFileAsync(
        string accessToken,
        MigrationFileItem file,
        Stream sourceStream,
        CancellationToken cancellationToken)
    {
        var url = BuildGraphContentUrl(file.DestinationRelativePath);
        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StreamContent(sourceStream);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var response = await SendCloudRequestAsync(request, cancellationToken);
        _logger.LogDebug(
            "Uploaded small OneDrive file. DestinationItemId: {DestinationItemId}, StatusCode: {StatusCode}",
            file.DestinationRelativePath,
            response.StatusCode);
    }

    private async Task UploadOneDriveFileWithUploadSessionAsync(
        string accessToken,
        MigrationFileItem file,
        Stream sourceStream,
        string conflictResolutionRule,
        CancellationToken cancellationToken)
    {
        var uploadUrl = await CreateOneDriveUploadSessionAsync(
            accessToken,
            file,
            conflictResolutionRule,
            cancellationToken);
        var buffer = new byte[ChunkSizeBytes];
        long offset = 0;

        while (offset < file.SizeBytes)
        {
            var bytesRead = await ReadChunkAsync(sourceStream, buffer, cancellationToken);
            if (bytesRead == 0)
            {
                throw new EndOfStreamException(
                    $"Source stream ended before expected file size for '{file.SourceItemId}'.");
            }

            using var content = new ByteArrayContent(buffer, 0, bytesRead);
            content.Headers.ContentRange = new ContentRangeHeaderValue(offset, offset + bytesRead - 1, file.SizeBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            using var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
            {
                Content = content
            };

            using var response = await SendCloudRequestAsync(request, cancellationToken);
            _logger.LogDebug(
                "Uploaded OneDrive chunk. DestinationItemId: {DestinationItemId}, Offset: {Offset}, BytesRead: {BytesRead}, StatusCode: {StatusCode}",
                file.DestinationRelativePath,
                offset,
                bytesRead,
                response.StatusCode);

            offset += bytesRead;
        }
    }

    private async Task<string> CreateOneDriveUploadSessionAsync(
        string accessToken,
        MigrationFileItem file,
        string conflictResolutionRule,
        CancellationToken cancellationToken)
    {
        var url = BuildGraphCreateUploadSessionUrl(file.DestinationRelativePath);
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new
        {
            item = new Dictionary<string, object>
            {
                ["@microsoft.graph.conflictBehavior"] = ToGraphConflictBehavior(conflictResolutionRule),
                ["name"] = file.Name
            }
        });

        using var response = await SendCloudRequestAsync(request, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<OneDriveUploadSessionResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Microsoft Graph did not return an upload session.");

        if (string.IsNullOrWhiteSpace(payload.UploadUrl))
        {
            throw new InvalidOperationException("Microsoft Graph upload session did not include an upload URL.");
        }

        return payload.UploadUrl;
    }

    private async Task<bool> OneDriveFileExistsAsync(
        string accessToken,
        string relativePath,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildGraphMetadataUrl(relativePath));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowCloudHttpExceptionAsync(response, "Microsoft Graph metadata lookup failed", cancellationToken);
        }

        return true;
    }

    private async Task<HttpResponseMessage> SendCloudRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowCloudHttpExceptionAsync(response, "Cloud provider request failed", cancellationToken);
        }

        return response;
    }

    private static async Task ThrowCloudHttpExceptionAsync(
        HttpResponseMessage response,
        string message,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var safeBody = body.Length > 512 ? body[..512] : body;

        throw new HttpRequestException(
            $"{message}. StatusCode: {(int)response.StatusCode}. Response: {safeBody}",
            null,
            response.StatusCode);
    }

    private static string BuildGoogleListUrl(string folderId, string? pageToken)
    {
        var query = Uri.EscapeDataString($"'{folderId.Replace("'", "\\'", StringComparison.Ordinal)}' in parents and trashed = false");
        var fields = Uri.EscapeDataString("nextPageToken,files(id,name,mimeType,size,modifiedTime)");
        var builder = new StringBuilder(
            $"https://www.googleapis.com/drive/v3/files?q={query}&fields={fields}&pageSize=1000&supportsAllDrives=true&includeItemsFromAllDrives=true");

        if (!string.IsNullOrWhiteSpace(pageToken))
        {
            builder.Append("&pageToken=").Append(Uri.EscapeDataString(pageToken));
        }

        return builder.ToString();
    }

    private static string BuildGraphContentUrl(string relativePath)
    {
        return $"https://graph.microsoft.com/v1.0/me/drive/root:/{EscapeGraphPath(relativePath)}:/content";
    }

    private static string BuildGraphCreateUploadSessionUrl(string relativePath)
    {
        return $"https://graph.microsoft.com/v1.0/me/drive/root:/{EscapeGraphPath(relativePath)}:/createUploadSession";
    }

    private static string BuildGraphMetadataUrl(string relativePath)
    {
        return $"https://graph.microsoft.com/v1.0/me/drive/root:/{EscapeGraphPath(relativePath)}";
    }

    private static string EscapeGraphPath(string relativePath)
    {
        return string.Join(
            '/',
            relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));
    }

    private static string CombineGraphPath(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left))
        {
            return right.Trim('/');
        }

        return $"{left.Trim('/')}/{right.Trim('/')}";
    }

    private static bool ShouldInclude(
        string fileName,
        long sizeBytes,
        DateTimeOffset? modifiedAt,
        FilterConfig filterConfig)
    {
        if (filterConfig.SkipHiddenFiles && fileName.StartsWith(".", StringComparison.Ordinal))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName);
        var includeExtensions = NormalizeExtensions(filterConfig.IncludeExtensions);
        var excludeExtensions = NormalizeExtensions(filterConfig.ExcludeExtensions);

        if (includeExtensions.Count > 0 && !includeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        if (excludeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        if (filterConfig.MinSizeMB is not null && sizeBytes < ToBytes(filterConfig.MinSizeMB.Value))
        {
            return false;
        }

        if (filterConfig.MaxSizeMB is not null && sizeBytes > ToBytes(filterConfig.MaxSizeMB.Value))
        {
            return false;
        }

        if (modifiedAt is not null && filterConfig.ModifiedAfter is not null && modifiedAt < filterConfig.ModifiedAfter.Value)
        {
            return false;
        }

        if (modifiedAt is not null && filterConfig.ModifiedBefore is not null && modifiedAt > filterConfig.ModifiedBefore.Value)
        {
            return false;
        }

        if (filterConfig.IncludeNamePatterns.Count > 0)
        {
            return filterConfig.IncludeNamePatterns.Any(pattern =>
                fileName.Contains(pattern.Trim('*'), StringComparison.OrdinalIgnoreCase));
        }

        return true;
    }

    private static bool IsSkipConflictRule(string conflictResolutionRule)
    {
        return conflictResolutionRule.Equals("Skip", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOverwriteConflictRule(string conflictResolutionRule)
    {
        return conflictResolutionRule.Equals("Overwrite", StringComparison.OrdinalIgnoreCase);
    }

    private static string ToGraphConflictBehavior(string conflictResolutionRule)
    {
        if (conflictResolutionRule.Equals("Rename", StringComparison.OrdinalIgnoreCase))
        {
            return "rename";
        }

        if (conflictResolutionRule.Equals("Skip", StringComparison.OrdinalIgnoreCase))
        {
            return "fail";
        }

        return "replace";
    }

    private static HashSet<string> NormalizeExtensions(IEnumerable<string> extensions)
    {
        return extensions
            .Where(extension => !string.IsNullOrWhiteSpace(extension))
            .Select(extension => extension.Trim().StartsWith(".", StringComparison.Ordinal)
                ? extension.Trim()
                : $".{extension.Trim()}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static long ToBytes(double megabytes)
    {
        return Convert.ToInt64(megabytes * 1024 * 1024);
    }

    private static long ParseSize(string? size)
    {
        return long.TryParse(size, out var parsed) ? parsed : 0;
    }

    private static async Task<int> ReadChunkAsync(
        Stream stream,
        byte[] buffer,
        CancellationToken cancellationToken)
    {
        var totalRead = 0;

        while (totalRead < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(
                buffer.AsMemory(totalRead, buffer.Length - totalRead),
                cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            totalRead += bytesRead;
        }

        return totalRead;
    }

    private sealed class GoogleFileListResponse
    {
        public string? NextPageToken { get; set; }
        public List<GoogleDriveFile> Files { get; set; } = new();
    }

    private sealed class GoogleDriveFile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string? Size { get; set; }
        public DateTimeOffset? ModifiedTime { get; set; }
    }

    private sealed class OneDriveUploadSessionResponse
    {
        public string UploadUrl { get; set; } = string.Empty;
    }
}
