using CloudShift.Domain.Enums;

namespace CloudShift.Application.ProjectMappings.DTOs;

/// <summary>
/// Outbound DTO for a newly created (or queued) Migration Job.
/// </summary>
public sealed record MigrationJobDto(
    Guid Id,
    Guid ProjectMappingId,
    JobStatus Status,
    string StatusName,
    JobType JobType,
    string JobTypeName,
    int TotalItems,
    int ProcessedItems,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt
);
