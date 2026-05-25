using CloudShift.Domain.Enums;

namespace CloudShift.Domain.Entities;

public class OAuthProviderApp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ProviderType Provider { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string EncryptedClientSecret { get; set; } = string.Empty;
    public string TenantId { get; set; } = "common";
    public string RedirectUri { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<AppProfile> AppProfiles { get; set; } = new List<AppProfile>();
}
