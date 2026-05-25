using CloudShift.Application.OAuthProviderApps.Commands;
using CloudShift.Application.OAuthProviderApps.DTOs;
using CloudShift.Application.OAuthProviderApps.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CloudShift.Application.OAuthProviderApps.Handlers;

public sealed class DeleteOAuthProviderAppHandler : IRequestHandler<DeleteOAuthProviderAppCommand, DeleteOAuthProviderAppResult>
{
    private readonly IOAuthProviderAppRepository _repository;
    private readonly ILogger<DeleteOAuthProviderAppHandler> _logger;

    public DeleteOAuthProviderAppHandler(
        IOAuthProviderAppRepository repository,
        ILogger<DeleteOAuthProviderAppHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DeleteOAuthProviderAppResult> Handle(DeleteOAuthProviderAppCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting OAuth provider app. ProviderAppId: {ProviderAppId}, UserId: {UserId}",
            command.Id,
            command.UserId);

        var app = await _repository.GetForUpdateAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"OAuth provider app '{command.Id}' was not found.");

        if (app.UserId != command.UserId)
        {
            throw new UnauthorizedAccessException("OAuth provider app does not belong to the current user.");
        }

        var linkedProfileCount = app.AppProfiles.Count;
        if (linkedProfileCount > 0)
        {
            app.IsActive = false;
            app.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(app, cancellationToken);

            _logger.LogWarning(
                "Deactivated OAuth provider app instead of deleting because profiles are linked. ProviderAppId: {ProviderAppId}, UserId: {UserId}, LinkedProfileCount: {LinkedProfileCount}",
                app.Id,
                app.UserId,
                linkedProfileCount);

            return new DeleteOAuthProviderAppResult(
                app.Id,
                app.UserId,
                Deleted: false,
                Deactivated: true,
                linkedProfileCount,
                "OAuth app has linked profiles, so it was deactivated instead of deleted.");
        }

        await _repository.DeleteAsync(app, cancellationToken);

        _logger.LogInformation(
            "Deleted OAuth provider app. ProviderAppId: {ProviderAppId}, UserId: {UserId}",
            app.Id,
            app.UserId);

        return new DeleteOAuthProviderAppResult(
            app.Id,
            app.UserId,
            Deleted: true,
            Deactivated: false,
            linkedProfileCount,
            "OAuth app was deleted.");
    }
}
