using CloudShift.Application.ProjectMappings.Models;

namespace CloudShift.Application.ProjectMappings.DTOs;

/// <summary>
/// Request payload for creating a new Project Mapping.
/// </summary>
public sealed class CreateProjectMappingRequest
{
    /// <summary>ID of the user who owns this mapping.</summary>
    public Guid UserId { get; set; }

    /// <summary>Human-readable name / label for this migration configuration.</summary>
    public required string Name { get; set; }

    /// <summary>AppProfile that acts as the migration source (e.g. a Google Drive account).</summary>
    public Guid SourceProfileId { get; set; }

    /// <summary>AppProfile that acts as the migration destination (e.g. a OneDrive account).</summary>
    public Guid DestProfileId { get; set; }

    /// <summary>Folder path within the source to migrate from. Empty string = root.</summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>Folder path within the destination to migrate to. Empty string = root.</summary>
    public string DestPath { get; set; } = string.Empty;

    /// <summary>
    /// Optional filter rules. If omitted, all files will be migrated.
    /// </summary>
    public FilterConfig? FilterConfig { get; set; }

    /// <summary>
    /// How to handle filename collisions at the destination.
    /// Allowed values: "Skip" | "Overwrite" | "Rename".
    /// Defaults to "Skip".
    /// </summary>
    public string ConflictResolutionRule { get; set; } = "Skip";
}
