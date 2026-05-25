using CloudShift.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CloudShift.Application.OAuthProviderApps.DTOs;

public sealed class UpdateOAuthProviderAppRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public ProviderType Provider { get; set; }

    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    public string? ClientSecret { get; set; }

    public string TenantId { get; set; } = "common";

    [Required]
    public string RedirectUri { get; set; } = string.Empty;

    public string? Scopes { get; set; }

    public bool IsActive { get; set; } = true;
}
