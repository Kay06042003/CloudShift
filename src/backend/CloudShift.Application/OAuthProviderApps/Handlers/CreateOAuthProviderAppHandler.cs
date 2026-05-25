using CloudShift.Application.Common.Interfaces;
using CloudShift.Application.OAuthProviderApps.Commands;
using CloudShift.Application.OAuthProviderApps.DTOs;
using CloudShift.Application.OAuthProviderApps.Interfaces;
using CloudShift.Domain.Entities;
using CloudShift.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CloudShift.Application.OAuthProviderApps.Handlers;

public sealed class CreateOAuthProviderAppHandler : IRequestHandler<CreateOAuthProviderAppCommand, OAuthProviderAppDto>
{
    private readonly IOAuthProviderAppRepository _repository;
    private readonly ITokenProtector _tokenProtector;
    private readonly ILogger<CreateOAuthProviderAppHandler> _logger;

    public CreateOAuthProviderAppHandler(
        IOAuthProviderAppRepository repository,
        ITokenProtector tokenProtector,
        ILogger<CreateOAuthProviderAppHandler> logger)
    {
        _repository = repository;
        _tokenProtector = tokenProtector;
        _logger = logger;
    }

    public async Task<OAuthProviderAppDto> Handle(CreateOAuthProviderAppCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating OAuth provider app. UserId: {UserId}, Provider: {Provider}",
            command.UserId,
            command.Provider);

        var now = DateTime.UtcNow;
        var app = new OAuthProviderApp
        {
            UserId = command.UserId,
            Provider = command.Provider,
            Name = command.Name.Trim(),
            ClientId = command.ClientId.Trim(),
            EncryptedClientSecret = _tokenProtector.Protect(command.ClientSecret),
            TenantId = NormalizeTenantId(command.Provider, command.TenantId),
            RedirectUri = command.RedirectUri.Trim(),
            Scopes = NormalizeScopes(command.Provider, command.Scopes),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var saved = await _repository.AddAsync(app, cancellationToken);

        _logger.LogInformation(
            "Created OAuth provider app. ProviderAppId: {ProviderAppId}, UserId: {UserId}, Provider: {Provider}",
            saved.Id,
            saved.UserId,
            saved.Provider);

        return MapToDto(saved);
    }

    internal static OAuthProviderAppDto MapToDto(OAuthProviderApp app)
    {
        return new OAuthProviderAppDto(
            app.Id,
            app.UserId,
            app.Provider,
            app.Provider.ToString(),
            app.Name,
            app.ClientId,
            app.TenantId,
            app.RedirectUri,
            app.Scopes,
            app.IsActive,
            app.AppProfiles.Count,
            app.CreatedAt,
            app.UpdatedAt);
    }

    internal static string NormalizeTenantId(ProviderType provider, string tenantId)
    {
        if (provider == ProviderType.OneDrive)
        {
            return string.IsNullOrWhiteSpace(tenantId) ? "common" : tenantId.Trim();
        }

        return string.Empty;
    }

    internal static string NormalizeScopes(ProviderType provider, string? scopes)
    {
        if (!string.IsNullOrWhiteSpace(scopes))
        {
            return scopes.Trim();
        }

        return provider switch
        {
            ProviderType.GoogleDrive => "openid email profile https://www.googleapis.com/auth/drive",
            ProviderType.OneDrive => "offline_access User.Read Files.ReadWrite",
            _ => string.Empty
        };
    }
}
