using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using CloudShift.Application.AppProfiles.DTOs;
using CloudShift.Application.AppProfiles.Exceptions;
using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Application.Common.Interfaces;
using CloudShift.Domain.Entities;
using CloudShift.Domain.Enums;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Polly;

namespace CloudShift.Infrastructure.Auth;

public sealed class CloudOAuthService : ICloudOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenProtector _tokenProtector;
    private readonly ILogger<CloudOAuthService> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _tokenRetryPolicy;

    public CloudOAuthService(
        HttpClient httpClient,
        ITokenProtector tokenProtector,
        ILogger<CloudOAuthService> logger)
    {
        _httpClient = httpClient;
        _tokenProtector = tokenProtector;
        _logger = logger;
        _retryPolicy = CreateRetryPolicy();
        _tokenRetryPolicy = CreateTokenRetryPolicy();
    }

    public string BuildAuthorizationUrl(OAuthProviderApp providerApp, string state)
    {
        return providerApp.Provider switch
        {
            ProviderType.GoogleDrive => BuildGoogleAuthorizationUrl(providerApp, state),
            ProviderType.OneDrive => BuildMicrosoftAuthorizationUrl(providerApp, state),
            _ => throw new NotSupportedException($"Provider '{providerApp.Provider}' is not supported.")
        };
    }

    public async Task<CloudOAuthTokenResult> ExchangeCodeAsync(
        OAuthProviderApp providerApp,
        string code,
        CancellationToken cancellationToken = default)
    {
        return providerApp.Provider switch
        {
            ProviderType.GoogleDrive => await ExchangeGoogleCodeAsync(providerApp, code, cancellationToken),
            ProviderType.OneDrive => await ExchangeMicrosoftCodeAsync(providerApp, code, cancellationToken),
            _ => throw new NotSupportedException($"Provider '{providerApp.Provider}' is not supported.")
        };
    }

    private static string BuildGoogleAuthorizationUrl(OAuthProviderApp providerApp, string state)
    {
        RequireConfigured(providerApp);

        return QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", new Dictionary<string, string?>
        {
            ["client_id"] = providerApp.ClientId,
            ["redirect_uri"] = providerApp.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = providerApp.Scopes,
            ["access_type"] = "offline",
            ["prompt"] = "consent select_account",
            ["state"] = state
        });
    }

    private static string BuildMicrosoftAuthorizationUrl(OAuthProviderApp providerApp, string state)
    {
        RequireConfigured(providerApp);
        var tenantId = string.IsNullOrWhiteSpace(providerApp.TenantId) ? "common" : providerApp.TenantId;

        return QueryHelpers.AddQueryString($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize", new Dictionary<string, string?>
        {
            ["client_id"] = providerApp.ClientId,
            ["redirect_uri"] = providerApp.RedirectUri,
            ["response_type"] = "code",
            ["response_mode"] = "query",
            ["scope"] = providerApp.Scopes,
            ["prompt"] = "select_account",
            ["state"] = state
        });
    }

    private async Task<CloudOAuthTokenResult> ExchangeGoogleCodeAsync(OAuthProviderApp providerApp, string code, CancellationToken cancellationToken)
    {
        RequireConfigured(providerApp);
        var clientSecret = _tokenProtector.Unprotect(providerApp.EncryptedClientSecret);
        using var tokenResponse = await SendTokenRequestAsync(
            "https://oauth2.googleapis.com/token",
            new Dictionary<string, string>
            {
                ["client_id"] = providerApp.ClientId,
                ["client_secret"] = clientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = providerApp.RedirectUri
            },
            cancellationToken);

        using var tokenJson = await ReadJsonAsync(tokenResponse, cancellationToken);
        var accessToken = GetRequiredString(tokenJson.RootElement, "access_token");
        var refreshToken = GetRequiredString(tokenJson.RootElement, "refresh_token");
        var expiresAt = DateTime.UtcNow.AddSeconds(GetOptionalInt(tokenJson.RootElement, "expires_in", 3600));
        var grantedScopes = TryGetString(tokenJson.RootElement, "scope") ?? providerApp.Scopes;
        var account = await GetGoogleAccountAsync(accessToken, cancellationToken);

        return new CloudOAuthTokenResult(account.ExternalAccountId, account.Email, accessToken, refreshToken, expiresAt, grantedScopes);
    }

    private async Task<CloudOAuthTokenResult> ExchangeMicrosoftCodeAsync(OAuthProviderApp providerApp, string code, CancellationToken cancellationToken)
    {
        RequireConfigured(providerApp);
        var clientSecret = _tokenProtector.Unprotect(providerApp.EncryptedClientSecret);
        var tenantId = string.IsNullOrWhiteSpace(providerApp.TenantId) ? "common" : providerApp.TenantId;
        using var tokenResponse = await SendTokenRequestAsync(
            $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
            new Dictionary<string, string>
            {
                ["client_id"] = providerApp.ClientId,
                ["client_secret"] = clientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = providerApp.RedirectUri
            },
            cancellationToken);

        using var tokenJson = await ReadJsonAsync(tokenResponse, cancellationToken);
        var accessToken = GetRequiredString(tokenJson.RootElement, "access_token");
        var refreshToken = GetRequiredString(tokenJson.RootElement, "refresh_token");
        var expiresAt = DateTime.UtcNow.AddSeconds(GetOptionalInt(tokenJson.RootElement, "expires_in", 3600));
        var grantedScopes = TryGetString(tokenJson.RootElement, "scope") ?? providerApp.Scopes;
        var account = await GetMicrosoftAccountAsync(accessToken, cancellationToken);

        return new CloudOAuthTokenResult(account.ExternalAccountId, account.Email, accessToken, refreshToken, expiresAt, grantedScopes);
    }

    private async Task<HttpResponseMessage> SendTokenRequestAsync(
        string uri,
        Dictionary<string, string> form,
        CancellationToken cancellationToken)
    {
        var response = await _tokenRetryPolicy.ExecuteAsync(
            ct => _httpClient.PostAsync(uri, new FormUrlEncodedContent(form), ct),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
        return response;
    }

    private async Task<CloudAccountIdentity> GetGoogleAccountAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var response = await _retryPolicy.ExecuteAsync(
            async ct =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://openidconnect.googleapis.com/v1/userinfo");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return await _httpClient.SendAsync(request, ct);
            },
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        using var json = await ReadJsonAsync(response, cancellationToken);

        return new CloudAccountIdentity(
            GetRequiredString(json.RootElement, "sub"),
            GetRequiredString(json.RootElement, "email"));
    }

    private async Task<CloudAccountIdentity> GetMicrosoftAccountAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var response = await _retryPolicy.ExecuteAsync(
            async ct =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me?$select=id,mail,userPrincipalName");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return await _httpClient.SendAsync(request, ct);
            },
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        using var json = await ReadJsonAsync(response, cancellationToken);

        var email = TryGetString(json.RootElement, "mail")
            ?? TryGetString(json.RootElement, "userPrincipalName")
            ?? throw new InvalidOperationException("Microsoft Graph did not return an account email.");
        var externalAccountId = GetRequiredString(json.RootElement, "id");

        return new CloudAccountIdentity(externalAccountId, email);
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var providerError = TryReadOAuthError(body);

        _logger.LogWarning(
            "OAuth provider returned unsuccessful response. StatusCode: {StatusCode}, ProviderError: {ProviderError}, ProviderErrorDescription: {ProviderErrorDescription}",
            response.StatusCode,
            providerError.Error,
            providerError.Description);

        throw new CloudOAuthException(
            $"OAuth provider returned {(int)response.StatusCode}.",
            response.StatusCode,
            providerError.Error,
            providerError.Description);
    }

    private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>(ex => ex.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Unauthorized)
            .OrResult(response => response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Unauthorized)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, delay, attempt, _) =>
                {
                    _logger.LogWarning(
                        outcome.Exception,
                        "Retrying OAuth HTTP call. Attempt: {Attempt}, Delay: {Delay}, StatusCode: {StatusCode}",
                        attempt,
                        delay,
                        outcome.Result?.StatusCode);
                });
    }

    private IAsyncPolicy<HttpResponseMessage> CreateTokenRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>(ex => ex.StatusCode is HttpStatusCode.TooManyRequests)
            .OrResult(response => response.StatusCode is HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, delay, attempt, _) =>
                {
                    _logger.LogWarning(
                        outcome.Exception,
                        "Retrying OAuth token HTTP call. Attempt: {Attempt}, Delay: {Delay}, StatusCode: {StatusCode}",
                        attempt,
                        delay,
                        outcome.Result?.StatusCode);
                });
    }

    private static (string? Error, string? Description) TryReadOAuthError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return (null, null);
        }

        try
        {
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;

            return (
                TryGetString(root, "error"),
                TryGetString(root, "error_description"));
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    private static void RequireConfigured(OAuthProviderApp providerApp)
    {
        if (string.IsNullOrWhiteSpace(providerApp.ClientId)
            || string.IsNullOrWhiteSpace(providerApp.EncryptedClientSecret)
            || string.IsNullOrWhiteSpace(providerApp.RedirectUri)
            || string.IsNullOrWhiteSpace(providerApp.Scopes))
        {
            throw new InvalidOperationException($"OAuth provider app '{providerApp.Id}' is not configured.");
        }
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        return TryGetString(element, propertyName)
            ?? throw new InvalidOperationException($"OAuth response is missing '{propertyName}'.");
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static int GetOptionalInt(JsonElement element, string propertyName, int defaultValue)
    {
        return element.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var result)
            ? result
            : defaultValue;
    }

    private sealed record CloudAccountIdentity(string ExternalAccountId, string Email);
}
