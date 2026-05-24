using CloudShift.Application.AppProfiles.DTOs;
using MediatR;

namespace CloudShift.Application.AppProfiles.Queries;

/// <summary>
/// CQRS Query: Returns all App Profiles for a given user.
/// </summary>
public sealed record GetAppProfilesQuery(Guid UserId) : IRequest<IReadOnlyList<AppProfileDto>>;
