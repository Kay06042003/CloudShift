using CloudShift.Application.AppProfiles.Commands;
using CloudShift.Application.AppProfiles.DTOs;
using CloudShift.Application.AppProfiles.Exceptions;
using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Application.AppProfiles.Queries;
using CloudShift.Application.OAuthProviderApps.Interfaces;
using CloudShift.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CloudShift.Api.Controllers;

/// <summary>
/// Manages App Profiles (connected cloud storage accounts).
/// </summary>
[ApiController]
[Route("api/app-profiles")]
[Produces("application/json")]
public sealed class AppProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICloudOAuthService _cloudOAuthService;
    private readonly IOAuthStateProtector _oauthStateProtector;
    private readonly IOAuthProviderAppRepository _providerAppRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppProfileController> _logger;

    public AppProfileController(
        IMediator mediator,
        ICloudOAuthService cloudOAuthService,
        IOAuthStateProtector oauthStateProtector,
        IOAuthProviderAppRepository providerAppRepository,
        IConfiguration configuration,
        ILogger<AppProfileController> logger)
    {
        _mediator = mediator;
        _cloudOAuthService = cloudOAuthService;
        _oauthStateProtector = oauthStateProtector;
        _providerAppRepository = providerAppRepository;
        _configuration = configuration;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────
    // GET /api/app-profiles/oauth/provider-apps/{providerAppId}/authorize?userId={userId}
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts OAuth authorization-code flow using a customer-owned provider app configuration.
    /// </summary>
    [HttpGet("oauth/provider-apps/{providerAppId:guid}/authorize")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Authorize(
        [FromRoute] Guid providerAppId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest("userId is required.");
        }

        var providerApp = await _providerAppRepository.GetByIdAsync(providerAppId, cancellationToken);
        if (providerApp is null || providerApp.UserId != userId)
        {
            return NotFound(new { error = $"OAuth provider app '{providerAppId}' was not found." });
        }

        if (!providerApp.IsActive)
        {
            return BadRequest("OAuth provider app is inactive.");
        }

        var state = _oauthStateProtector.Protect(userId, providerApp.Id, providerApp.Provider);
        string authorizationUrl;
        try
        {
            authorizationUrl = _cloudOAuthService.BuildAuthorizationUrl(providerApp, state);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Custom OAuth provider app is not configured. ProviderAppId: {ProviderAppId}", providerApp.Id);
            return BadRequest(ex.Message);
        }

        _logger.LogInformation(
            "Starting OAuth authorization. UserId: {UserId}, Provider: {Provider}, ProviderAppId: {ProviderAppId}",
            userId,
            providerApp.Provider,
            providerApp.Id);

        return Redirect(authorizationUrl);
    }

    // GET /api/app-profiles/oauth/{provider}/callback?code={code}&state={state}

    /// <summary>
    /// Receives OAuth callback, exchanges authorization code server-side, encrypts tokens, and stores the profile.
    /// </summary>
    [HttpGet("oauth/{provider}/callback")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Callback(
        [FromRoute] string provider,
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("OAuth provider returned an error. Provider: {Provider}, Error: {Error}", provider, error);
            return Redirect(BuildFrontendRedirect("error", error));
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        {
            return BadRequest("OAuth callback requires code and state.");
        }

        if (!TryParseProvider(provider, out var providerType))
        {
            return BadRequest($"Unsupported provider '{provider}'.");
        }

        OAuthState oauthState;
        try
        {
            oauthState = _oauthStateProtector.Unprotect(state);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Security.Cryptography.CryptographicException)
        {
            _logger.LogWarning(ex, "Invalid OAuth state received for provider {Provider}.", provider);
            return BadRequest("OAuth state is invalid or expired.");
        }

        if (oauthState.Provider != providerType)
        {
            return BadRequest("OAuth state provider mismatch.");
        }

        var providerApp = await _providerAppRepository.GetByIdAsync(oauthState.ProviderAppId, cancellationToken);
        if (providerApp is null || providerApp.UserId != oauthState.UserId)
        {
            return BadRequest("OAuth provider app is invalid.");
        }

        if (providerApp.Provider != providerType)
        {
            return BadRequest("OAuth provider app does not match callback provider.");
        }

        CloudOAuthTokenResult tokenResult;
        try
        {
            tokenResult = await _cloudOAuthService.ExchangeCodeAsync(providerApp, code, cancellationToken);
        }
        catch (CloudOAuthException ex)
        {
            _logger.LogWarning(
                ex,
                "OAuth token exchange failed. UserId: {UserId}, Provider: {Provider}, ProviderAppId: {ProviderAppId}, StatusCode: {StatusCode}, ProviderError: {ProviderError}",
                oauthState.UserId,
                providerType,
                providerApp.Id,
                ex.StatusCode,
                ex.ProviderError);

            return Redirect(BuildFrontendRedirect("error", ex.ProviderError ?? "provider_error"));
        }

        await _mediator.Send(
            new CompleteAppProfileOAuthCommand(
                oauthState.UserId,
                providerApp.Id,
                providerType,
                tokenResult.ExternalAccountId,
                tokenResult.Email,
                tokenResult.AccessToken,
                tokenResult.RefreshToken,
                tokenResult.ExpiresAt,
                tokenResult.GrantedScopes),
            cancellationToken);

        _logger.LogInformation(
            "OAuth callback completed. UserId: {UserId}, Provider: {Provider}, ProviderAppId: {ProviderAppId}, Email: {Email}",
            oauthState.UserId,
            providerType,
            providerApp.Id,
            tokenResult.Email);

        return Redirect(BuildFrontendRedirect("success"));
    }

    // ─────────────────────────────────────────────────────────────────
    // GET /api/app-profiles?userId={userId}
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all App Profiles for the specified user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AppProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppProfiles(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetAppProfilesQuery(userId);
        var profiles = await _mediator.Send(query, cancellationToken);

        _logger.LogInformation(
            "Returned app profiles. UserId: {UserId}, Count: {Count}",
            userId,
            profiles.Count);

        return Ok(profiles);
    }

    // ─────────────────────────────────────────────────────────────────
    // POST /api/app-profiles/{id}/test-connection
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Performs a mock connection health check for the specified App Profile.
    /// Always returns healthy (true) — replace with real OAuth token validation later.
    /// </summary>
    [HttpPost("{id:guid}/test-connection")]
    [ProducesResponseType(typeof(ConnectionTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult TestConnection(Guid id)
    {
        // TODO: Replace with real token introspection / API ping in a future sprint.
        _logger.LogInformation(
            "Tested app profile connection. ProfileId: {ProfileId}, IsHealthy: {IsHealthy}",
            id,
            true);

        return Ok(new ConnectionTestResult(
            ProfileId: id,
            IsHealthy: true,
            Message: "Connection successful (mocked)",
            TestedAt: DateTime.UtcNow
        ));
    }

    private string BuildFrontendRedirect(string status, string? reason = null)
    {
        var baseUrl = _configuration["Frontend:OAuthCompleteUrl"] ?? "http://localhost:4200/app-profiles";
        var query = $"oauth={Uri.EscapeDataString(status)}";

        if (!string.IsNullOrWhiteSpace(reason))
        {
            query += $"&oauthReason={Uri.EscapeDataString(reason)}";
        }

        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{baseUrl}{separator}{query}";
    }

    private static bool TryParseProvider(string provider, out ProviderType providerType)
    {
        providerType = provider.Trim().ToLowerInvariant() switch
        {
            "google" or "google-drive" or "googledrive" => ProviderType.GoogleDrive,
            "microsoft" or "onedrive" or "one-drive" => ProviderType.OneDrive,
            _ => default
        };

        return providerType is ProviderType.GoogleDrive or ProviderType.OneDrive;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Local response records (small enough to live next to the controller)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Result of a connection health check.</summary>
public sealed record ConnectionTestResult(
    Guid ProfileId,
    bool IsHealthy,
    string Message,
    DateTime TestedAt
);
