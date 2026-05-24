using CloudShift.Domain.Entities;

namespace CloudShift.Application.AppProfiles.Interfaces;

/// <summary>
/// Persistence contract for App Profile data.
/// Lives in Application so the domain layer stays infrastructure-free.
/// </summary>
public interface IAppProfileRepository
{
    /// <summary>Persists a new profile and returns it with the generated Id.</summary>
    Task<AppProfile> AddAsync(AppProfile profile, CancellationToken cancellationToken = default);

    /// <summary>Returns all profiles owned by the specified user.</summary>
    Task<IReadOnlyList<AppProfile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns a single profile by its primary key, or null if not found.</summary>
    Task<AppProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
