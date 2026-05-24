using CloudShift.Application.AppProfiles.Commands;
using CloudShift.Application.AppProfiles.DTOs;
using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Domain.Entities;
using MediatR;

namespace CloudShift.Application.AppProfiles.Handlers;

/// <summary>
/// Handles <see cref="AddAppProfileCommand"/>.
/// Persists the new profile and maps it back to a DTO.
/// NOTE: OAuth flow is mocked — the caller supplies tokens directly.
/// </summary>
public sealed class AddAppProfileHandler : IRequestHandler<AddAppProfileCommand, AppProfileDto>
{
    private readonly IAppProfileRepository _repository;

    public AddAppProfileHandler(IAppProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppProfileDto> Handle(AddAppProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = new AppProfile
        {
            UserId       = command.UserId,
            Provider     = command.Provider,
            Email        = command.Email,
            AccessToken  = command.AccessToken,
            RefreshToken = command.RefreshToken,
            ExpiresAt    = command.ExpiresAt,
            CreatedAt    = DateTime.UtcNow
        };

        var saved = await _repository.AddAsync(profile, cancellationToken);

        return MapToDto(saved);
    }

    private static AppProfileDto MapToDto(AppProfile p) => new(
        p.Id,
        p.UserId,
        p.Provider,
        p.Provider.ToString(),   // human-readable provider name
        p.Email,
        p.ExpiresAt,
        p.CreatedAt
    );
}
