using System.Diagnostics;
using System.Net;
using System.Text.Json;
using CloudShift.Application.ProjectMappings.Models;
using CloudShift.Domain.Entities;
using CloudShift.Domain.Enums;
using CloudShift.Domain.Messages;
using CloudShift.Infrastructure.Data;
using CloudShift.Worker.MigrationPlugins;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

namespace CloudShift.Worker.Consumers;

public sealed class MigrationJobConsumer : IConsumer<MigrationJobStartedEvent>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _dbContext;
    private readonly ProviderMigrationPluginResolver _pluginResolver;
    private readonly ILogger<MigrationJobConsumer> _logger;

    public MigrationJobConsumer(
        ApplicationDbContext dbContext,
        ProviderMigrationPluginResolver pluginResolver,
        ILogger<MigrationJobConsumer> logger)
    {
        _dbContext = dbContext;
        _pluginResolver = pluginResolver;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MigrationJobStartedEvent> context)
    {
        var cancellationToken = context.CancellationToken;
        var jobId = context.Message.JobId;
        var stopwatch = Stopwatch.StartNew();

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["JobId"] = jobId,
            ["BatchId"] = context.MessageId
        });

        _logger.LogInformation("Received migration job event. MappingId: {MappingId}", context.Message.MappingId);

        var job = await LoadJobAsync(jobId, cancellationToken);
        if (job is null)
        {
            _logger.LogWarning("Skipped migration job because it was not found.");
            return;
        }

        if (!TryBuildRouteContext(job, out var routeContext, out var validationError))
        {
            await MarkJobFailedAsync(job, validationError, cancellationToken);
            return;
        }

        var plugin = _pluginResolver.Resolve(
            routeContext.SourceProfile.Provider,
            routeContext.DestinationProfile.Provider);
        var retryPolicy = CreateHttpRetryPolicy(job.Id);

        try
        {
            await ProcessJobAsync(job, routeContext, plugin, retryPolicy, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            job.Status = JobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Migration job failed. MappingId: {MappingId}, ElapsedMs: {ElapsedMs}",
                job.ProjectMappingId,
                stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task ProcessJobAsync(
        MigrationJob job,
        MigrationRouteContext routeContext,
        IProviderMigrationPlugin plugin,
        IAsyncPolicy retryPolicy,
        CancellationToken cancellationToken)
    {
        var plannedFiles = await retryPolicy.ExecuteAsync(
            ct => plugin.BuildFilePlanAsync(routeContext, ct),
            cancellationToken);

        job.Status = JobStatus.Processing;
        job.StartedAt ??= DateTime.UtcNow;
        job.TotalItems = plannedFiles.Count;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Started migration job processing. UserId: {UserId}, SourceProvider: {SourceProvider}, DestinationProvider: {DestinationProvider}, TotalItems: {TotalItems}",
            routeContext.Mapping.UserId,
            routeContext.SourceProfile.Provider,
            routeContext.DestinationProfile.Provider,
            plannedFiles.Count);

        var failedItems = 0;

        foreach (var file in plannedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var transferLog = await CreateTransferLogAsync(job.Id, file, cancellationToken);

            try
            {
                await retryPolicy.ExecuteAsync(
                    ct => plugin.TransferFileAsync(routeContext, file, ct),
                    cancellationToken);

                transferLog.Status = FileTransferStatus.Success;
                transferLog.TransferredAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "Transferred migration file. SourceItemId: {SourceItemId}, DestinationItemId: {DestinationItemId}, FileSizeBytes: {FileSizeBytes}",
                    file.SourceItemId,
                    file.DestinationRelativePath,
                    file.SizeBytes);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failedItems++;
                transferLog.Status = FileTransferStatus.Failed;
                transferLog.ErrorMessage = ex.Message;
                transferLog.TransferredAt = DateTime.UtcNow;

                _logger.LogError(
                    ex,
                    "Failed to transfer migration file. SourceItemId: {SourceItemId}, DestinationItemId: {DestinationItemId}, FileSizeBytes: {FileSizeBytes}",
                    file.SourceItemId,
                    file.DestinationRelativePath,
                    file.SizeBytes);
            }
            finally
            {
                job.ProcessedItems++;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        job.Status = failedItems == 0 ? JobStatus.Completed : JobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Completed migration job. TotalItems: {TotalItems}, ProcessedItems: {ProcessedItems}, FailedItems: {FailedItems}, FinalStatus: {Status}",
            job.TotalItems,
            job.ProcessedItems,
            failedItems,
            job.Status);
    }

    private async Task<MigrationJob?> LoadJobAsync(Guid jobId, CancellationToken cancellationToken)
    {
        return await _dbContext.MigrationJobs
            .Include(j => j.ProjectMapping)
                .ThenInclude(m => m!.SourceProfile)
            .Include(j => j.ProjectMapping)
                .ThenInclude(m => m!.DestProfile)
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
    }

    private static bool TryBuildRouteContext(
        MigrationJob job,
        out MigrationRouteContext context,
        out string validationError)
    {
        context = null!;

        if (job.ProjectMapping is null)
        {
            validationError = "Migration job has no project mapping.";
            return false;
        }

        if (job.ProjectMapping.SourceProfile is null || job.ProjectMapping.DestProfile is null)
        {
            validationError = "Migration job mapping has no source or destination profile.";
            return false;
        }

        var filterConfig = JsonSerializer.Deserialize<FilterConfig>(
            job.ProjectMapping.FilterConfigJson,
            JsonOptions) ?? new FilterConfig();

        context = new MigrationRouteContext(
            job,
            job.ProjectMapping,
            job.ProjectMapping.SourceProfile,
            job.ProjectMapping.DestProfile,
            filterConfig);

        validationError = string.Empty;
        return true;
    }

    private async Task<FileTransferLog> CreateTransferLogAsync(
        Guid jobId,
        MigrationFileItem file,
        CancellationToken cancellationToken)
    {
        var transferLog = new FileTransferLog
        {
            MigrationJobId = jobId,
            FileName = file.Name,
            SourceFileId = file.SourceItemId,
            Size = file.SizeBytes,
            Status = FileTransferStatus.Pending
        };

        await _dbContext.FileTransferLogs.AddAsync(transferLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return transferLog;
    }

    private async Task MarkJobFailedAsync(
        MigrationJob job,
        string reason,
        CancellationToken cancellationToken)
    {
        job.Status = JobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogError(
            "Marked migration job failed because it cannot be processed. Reason: {Reason}",
            reason);
    }

    private IAsyncPolicy CreateHttpRetryPolicy(Guid jobId)
    {
        return Policy
            .Handle<HttpRequestException>(IsTransientCloudHttpError)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, delay, attempt, _) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Retrying cloud migration operation. JobId: {JobId}, Attempt: {Attempt}, DelayMs: {DelayMs}",
                        jobId,
                        attempt,
                        delay.TotalMilliseconds);
                });
    }

    private static bool IsTransientCloudHttpError(HttpRequestException exception)
    {
        return exception.StatusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.Unauthorized
            or HttpStatusCode.RequestTimeout
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
    }
}
