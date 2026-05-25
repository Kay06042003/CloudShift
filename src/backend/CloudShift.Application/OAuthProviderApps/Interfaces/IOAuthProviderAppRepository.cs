using CloudShift.Domain.Entities;

namespace CloudShift.Application.OAuthProviderApps.Interfaces;

public interface IOAuthProviderAppRepository
{
    Task<OAuthProviderApp> AddAsync(OAuthProviderApp app, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OAuthProviderApp>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<OAuthProviderApp?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OAuthProviderApp?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<int> CountLinkedProfilesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OAuthProviderApp> UpdateAsync(OAuthProviderApp app, CancellationToken cancellationToken = default);

    Task DeleteAsync(OAuthProviderApp app, CancellationToken cancellationToken = default);
}
