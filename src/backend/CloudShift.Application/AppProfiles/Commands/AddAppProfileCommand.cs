using CloudShift.Application.AppProfiles.DTOs;
using MediatR;

namespace CloudShift.Application.AppProfiles.Commands;

/// <summary>
/// CQRS Command: Creates a new App Profile in the system.
/// Returns the newly created profile's DTO.
/// </summary>
public sealed record AddAppProfileCommand(
    Guid UserId,
    CloudShift.Domain.Enums.ProviderType Provider,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
) : IRequest<AppProfileDto>;
