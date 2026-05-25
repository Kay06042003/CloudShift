using CloudShift.Application.OAuthProviderApps.DTOs;
using CloudShift.Domain.Enums;
using MediatR;

namespace CloudShift.Application.OAuthProviderApps.Commands;

public sealed record CreateOAuthProviderAppCommand(
    Guid UserId,
    ProviderType Provider,
    string Name,
    string ClientId,
    string ClientSecret,
    string TenantId,
    string RedirectUri,
    string? Scopes
) : IRequest<OAuthProviderAppDto>;
