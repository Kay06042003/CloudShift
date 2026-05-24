using CloudShift.Domain.Enums;

namespace CloudShift.Application.AppProfiles.DTOs;

/// <summary>
/// Read-only DTO representing an App Profile returned to the client.
/// Tokens are intentionally excluded for security.
/// </summary>
public sealed record AppProfileDto(
    Guid Id,
    Guid UserId,
    ProviderType Provider,
    string ProviderName,
    string Email,
    DateTime ExpiresAt,
    DateTime CreatedAt
);
