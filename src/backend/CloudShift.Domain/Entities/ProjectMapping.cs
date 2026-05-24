using System;
using System.Collections.Generic;

namespace CloudShift.Domain.Entities;

public class ProjectMapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid SourceProfileId { get; set; }
    public Guid DestProfileId { get; set; }

    /// <summary>Human-readable label for this mapping configuration.</summary>
    public string Name { get; set; } = string.Empty;

    public string SourcePath { get; set; } = string.Empty;
    public string DestPath { get; set; } = string.Empty;

    /// <summary>
    /// Serialized JSON representation of the strongly-typed <c>FilterConfig</c>.
    /// Always handle this through the Application layer's DTO/model, never raw JSON in business logic.
    /// </summary>
    public string FilterConfigJson { get; set; } = "{}";

    /// <summary>
    /// Rule that determines how to handle filename collisions at the destination.
    /// Stored as a string so it's human-readable in the DB.
    /// </summary>
    public string ConflictResolutionRule { get; set; } = "Skip";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public AppProfile? SourceProfile { get; set; }
    public AppProfile? DestProfile { get; set; }

    public ICollection<MigrationJob> MigrationJobs { get; set; } = new List<MigrationJob>();
}
