using CloudShift.Domain.Entities;

namespace CloudShift.Application.ProjectMappings.Interfaces;

/// <summary>
/// Persistence contract for <see cref="ProjectMapping"/> aggregates.
/// Defined in Application to keep the domain layer free from infrastructure concerns.
/// </summary>
public interface IProjectMappingRepository
{
    /// <summary>Persists a new mapping and returns it with the generated Id.</summary>
    Task<ProjectMapping> AddAsync(ProjectMapping mapping, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all mappings belonging to the given user,
    /// with <see cref="ProjectMapping.SourceProfile"/> and <see cref="ProjectMapping.DestProfile"/> eagerly loaded.
    /// </summary>
    Task<IReadOnlyList<ProjectMapping>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns a single mapping by primary key, including navigation properties, or null if not found.</summary>
    Task<ProjectMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
