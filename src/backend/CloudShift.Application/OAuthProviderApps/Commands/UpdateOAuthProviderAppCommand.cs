using CloudShift.Application.OAuthProviderApps.DTOs;
using CloudShift.Domain.Enums;
using MediatR;

namespace CloudShift.Application.OAuthProviderApps.Commands;

public sealed record UpdateOAuthProviderAppCommand(
    Guid Id,
    Guid UserId,
    ProviderType Provider,
    string Name,
    string ClientId,
    string? ClientSecret,
    string TenantId,
    string RedirectUri,
    string? Scopes,
    bool IsActive
) : IRequest<OAuthProviderAppDto>;
