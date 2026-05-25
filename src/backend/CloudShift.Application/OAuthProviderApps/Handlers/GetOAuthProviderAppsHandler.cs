using CloudShift.Application.OAuthProviderApps.DTOs;
using CloudShift.Application.OAuthProviderApps.Interfaces;
using CloudShift.Application.OAuthProviderApps.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CloudShift.Application.OAuthProviderApps.Handlers;

public sealed class GetOAuthProviderAppsHandler : IRequestHandler<GetOAuthProviderAppsQuery, IReadOnlyList<OAuthProviderAppDto>>
{
    private readonly IOAuthProviderAppRepository _repository;
    private readonly ILogger<GetOAuthProviderAppsHandler> _logger;

    public GetOAuthProviderAppsHandler(
        IOAuthProviderAppRepository repository,
        ILogger<GetOAuthProviderAppsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OAuthProviderAppDto>> Handle(GetOAuthProviderAppsQuery query, CancellationToken cancellationToken)
    {
        var apps = await _repository.GetByUserIdAsync(query.UserId, cancellationToken);
        _logger.LogInformation(
            "Returned OAuth provider apps. UserId: {UserId}, Count: {Count}",
            query.UserId,
            apps.Count);

        return apps.Select(CreateOAuthProviderAppHandler.MapToDto).ToList();
    }
}
