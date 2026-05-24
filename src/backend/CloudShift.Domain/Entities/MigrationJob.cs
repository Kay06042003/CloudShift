using System;
using System.Collections.Generic;
using CloudShift.Domain.Enums;

namespace CloudShift.Domain.Entities;

public class MigrationJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectMappingId { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public JobType JobType { get; set; } = JobType.Full;
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProjectMapping? ProjectMapping { get; set; }
    public ICollection<FileTransferLog> FileTransferLogs { get; set; } = new List<FileTransferLog>();
}
