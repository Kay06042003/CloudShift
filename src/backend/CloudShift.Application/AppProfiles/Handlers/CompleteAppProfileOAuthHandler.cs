using CloudShift.Application.AppProfiles.Commands;
using CloudShift.Application.AppProfiles.DTOs;
using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Application.Common.Interfaces;
using CloudShift.Domain.Entities;
using MediatR;

namespace CloudShift.Application.AppProfiles.Handlers;

public sealed class CompleteAppProfileOAuthHandler : IRequestHandler<CompleteAppProfileOAuthCommand, AppProfileDto>
{
    private readonly IAppProfileRepository _repository;
    private readonly ITokenProtector _tokenProtector;

    public CompleteAppProfileOAuthHandler(
        IAppProfileRepository repository,
        ITokenProtector tokenProtector)
    {
        _repository = repository;
        _tokenProtector = tokenProtector;
    }

    public async Task<AppProfileDto> Handle(CompleteAppProfileOAuthCommand command, CancellationToken cancellationToken)
    {
        var profile = new AppProfile
        {
            UserId = command.UserId,
            ProviderAppId = command.ProviderAppId,
            Provider = command.Provider,
            ExternalAccountId = command.ExternalAccountId,
            Email = command.Email,
            EncryptedAccessToken = _tokenProtector.Protect(command.AccessToken),
            EncryptedRefreshToken = _tokenProtector.Protect(command.RefreshToken),
            GrantedScopes = command.GrantedScopes,
            ExpiresAt = command.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _repository.AddAsync(profile, cancellationToken);

        return new AppProfileDto(
            saved.Id,
            saved.UserId,
            saved.ProviderAppId,
            saved.Provider,
            saved.Provider.ToString(),
            saved.ExternalAccountId,
            saved.Email,
            saved.ExpiresAt,
            saved.CreatedAt);
    }
}
