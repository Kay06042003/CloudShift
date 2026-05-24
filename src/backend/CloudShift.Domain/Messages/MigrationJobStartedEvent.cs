using System;
using CloudShift.Domain.Enums;

namespace CloudShift.Domain.Messages;

/// <summary>
/// Published to RabbitMQ when a migration job is created and queued.
/// The Worker Service consumes this to begin the actual file transfer.
/// </summary>
public class MigrationJobStartedEvent
{
    public Guid JobId { get; set; }
    public Guid MappingId { get; set; }
    public JobType JobType { get; set; }
    public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
}
