using CloudShift.Application.Common.Interfaces;
using CloudShift.Application.OAuthProviderApps.Commands;
using CloudShift.Application.OAuthProviderApps.DTOs;
using CloudShift.Application.OAuthProviderApps.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CloudShift.Application.OAuthProviderApps.Handlers;

public sealed class UpdateOAuthProviderAppHandler : IRequestHandler<UpdateOAuthProviderAppCommand, OAuthProviderAppDto>
{
    private readonly IOAuthProviderAppRepository _repository;
    private readonly ITokenProtector _tokenProtector;
    private readonly ILogger<UpdateOAuthProviderAppHandler> _logger;

    public UpdateOAuthProviderAppHandler(
        IOAuthProviderAppRepository repository,
        ITokenProtector tokenProtector,
        ILogger<UpdateOAuthProviderAppHandler> logger)
    {
        _repository = repository;
        _tokenProtector = tokenProtector;
        _logger = logger;
    }

    public async Task<OAuthProviderAppDto> Handle(UpdateOAuthProviderAppCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating OAuth provider app. ProviderAppId: {ProviderAppId}, UserId: {UserId}, Provider: {Provider}",
            command.Id,
            command.UserId,
            command.Provider);

        var app = await _repository.GetForUpdateAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"OAuth provider app '{command.Id}' was not found.");

        if (app.UserId != command.UserId)
        {
            throw new UnauthorizedAccessException("OAuth provider app does not belong to the current user.");
        }

        var linkedProfileCount = app.AppProfiles.Count;
        if (linkedProfileCount > 0 && app.Provider != command.Provider)
        {
            _logger.LogWarning(
                "Rejected OAuth provider app provider change because profiles are linked. ProviderAppId: {ProviderAppId}, UserId: {UserId}, LinkedProfileCount: {LinkedProfileCount}",
                app.Id,
                app.UserId,
                linkedProfileCount);

            throw new InvalidOperationException("Cannot change provider while app profiles are linked to this OAuth app.");
        }

        app.Provider = command.Provider;
        app.Name = command.Name.Trim();
        app.ClientId = command.ClientId.Trim();
        if (!string.IsNullOrWhiteSpace(command.ClientSecret))
        {
            app.EncryptedClientSecret = _tokenProtector.Protect(command.ClientSecret);
        }

        app.TenantId = CreateOAuthProviderAppHandler.NormalizeTenantId(command.Provider, command.TenantId);
        app.RedirectUri = command.RedirectUri.Trim();
        app.Scopes = CreateOAuthProviderAppHandler.NormalizeScopes(command.Provider, command.Scopes);
        app.IsActive = command.IsActive;
        app.UpdatedAt = DateTime.UtcNow;

        var saved = await _repository.UpdateAsync(app, cancellationToken);

        _logger.LogInformation(
            "Updated OAuth provider app. ProviderAppId: {ProviderAppId}, UserId: {UserId}, Provider: {Provider}, LinkedProfileCount: {LinkedProfileCount}, IsActive: {IsActive}",
            saved.Id,
            saved.UserId,
            saved.Provider,
            linkedProfileCount,
            saved.IsActive);

        return CreateOAuthProviderAppHandler.MapToDto(saved);
    }
}
