namespace CloudShift.Worker.MigrationPlugins;

public sealed record MigrationFileItem(
    string SourceItemId,
    string Name,
    string DestinationRelativePath,
    long SizeBytes,
    DateTimeOffset? ModifiedAt);
