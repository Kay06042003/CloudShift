using CloudShift.Application.ProjectMappings.Models;

namespace CloudShift.Application.ProjectMappings.DTOs;

/// <summary>
/// Outbound DTO representing a Project Mapping.
/// The <see cref="FilterConfig"/> is deserialized from the stored JSON for type-safe consumption by clients.
/// </summary>
public sealed record ProjectMappingDto(
    Guid Id,
    Guid UserId,
    string Name,
    Guid SourceProfileId,
    string SourceProviderName,
    Guid DestProfileId,
    string DestProviderName,
    string SourcePath,
    string DestPath,
    FilterConfig FilterConfig,
    string ConflictResolutionRule,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
