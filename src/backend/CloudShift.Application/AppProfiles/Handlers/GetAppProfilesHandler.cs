using CloudShift.Application.AppProfiles.DTOs;
using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Application.AppProfiles.Queries;
using CloudShift.Domain.Entities;
using MediatR;

namespace CloudShift.Application.AppProfiles.Handlers;

/// <summary>
/// Handles <see cref="GetAppProfilesQuery"/>.
/// Returns all profiles for a given user, mapped to DTOs.
/// </summary>
public sealed class GetAppProfilesHandler : IRequestHandler<GetAppProfilesQuery, IReadOnlyList<AppProfileDto>>
{
    private readonly IAppProfileRepository _repository;

    public GetAppProfilesHandler(IAppProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AppProfileDto>> Handle(GetAppProfilesQuery query, CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetByUserIdAsync(query.UserId, cancellationToken);

        return profiles.Select(MapToDto).ToList();
    }

    private static AppProfileDto MapToDto(AppProfile p) => new(
        p.Id,
        p.UserId,
        p.Provider,
        p.Provider.ToString(),
        p.Email,
        p.ExpiresAt,
        p.CreatedAt
    );
}
