using CloudShift.Domain.Entities;

namespace CloudShift.Application.ProjectMappings.Interfaces;

/// <summary>
/// Persistence contract for <see cref="MigrationJob"/> records.
/// </summary>
public interface IMigrationJobRepository
{
    /// <summary>Persists a new job record and returns it with the generated Id.</summary>
    Task<MigrationJob> AddAsync(MigrationJob job, CancellationToken cancellationToken = default);

    /// <summary>Returns a single job by primary key, or null if not found.</summary>
    Task<MigrationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all jobs owned by a user through their project mappings.</summary>
    Task<IReadOnlyList<MigrationJob>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
