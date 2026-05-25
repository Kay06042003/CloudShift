using System;
using CloudShift.Domain.Enums;

namespace CloudShift.Domain.Entities;

public class AppProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? ProviderAppId { get; set; }
    public ProviderType Provider { get; set; }
    public string ExternalAccountId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EncryptedAccessToken { get; set; } = string.Empty;
    public string EncryptedRefreshToken { get; set; } = string.Empty;
    public string GrantedScopes { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public OAuthProviderApp? ProviderApp { get; set; }
}
