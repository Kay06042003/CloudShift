using CloudShift.Domain.Enums;

namespace CloudShift.Application.OAuthProviderApps.DTOs;

public sealed record OAuthProviderAppDto(
    Guid Id,
    Guid UserId,
    ProviderType Provider,
    string ProviderName,
    string Name,
    string ClientId,
    string TenantId,
    string RedirectUri,
    string Scopes,
    bool IsActive,
    int LinkedProfileCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
