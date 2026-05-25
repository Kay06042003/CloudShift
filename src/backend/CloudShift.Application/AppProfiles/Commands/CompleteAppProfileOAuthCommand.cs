using CloudShift.Application.AppProfiles.DTOs;
using CloudShift.Domain.Enums;
using MediatR;

namespace CloudShift.Application.AppProfiles.Commands;

public sealed record CompleteAppProfileOAuthCommand(
    Guid UserId,
    Guid? ProviderAppId,
    ProviderType Provider,
    string ExternalAccountId,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string GrantedScopes
) : IRequest<AppProfileDto>;
