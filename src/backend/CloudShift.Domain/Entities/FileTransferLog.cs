using System;
using CloudShift.Domain.Enums;

namespace CloudShift.Domain.Entities;

public class FileTransferLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MigrationJobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string SourceFileId { get; set; } = string.Empty;
    public long Size { get; set; }
    public FileTransferStatus Status { get; set; } = FileTransferStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? TransferredAt { get; set; }

    public MigrationJob? MigrationJob { get; set; }
}
