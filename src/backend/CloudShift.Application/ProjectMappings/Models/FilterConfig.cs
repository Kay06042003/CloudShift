namespace CloudShift.Application.ProjectMappings.Models;

/// <summary>
/// Strongly-typed representation of the migration filter rules.
/// This class is serialised to JSON and stored in <c>ProjectMapping.FilterConfigJson</c>.
/// Business logic MUST use this class — never read or write the raw JSON string directly.
/// </summary>
public sealed class FilterConfig
{
    /// <summary>
    /// Whitelist of file extensions to include (e.g. [".pdf", ".docx"]).
    /// Empty list = include all extensions.
    /// </summary>
    public List<string> IncludeExtensions { get; set; } = new();

    /// <summary>
    /// Blacklist of file extensions to exclude (e.g. [".tmp", ".log"]).
    /// Applied after <see cref="IncludeExtensions"/>.
    /// </summary>
    public List<string> ExcludeExtensions { get; set; } = new();

    /// <summary>Maximum file size to migrate, in megabytes. Null = no limit.</summary>
    public double? MaxSizeMB { get; set; }

    /// <summary>Minimum file size to migrate, in megabytes. Null = no minimum.</summary>
    public double? MinSizeMB { get; set; }

    /// <summary>Only migrate files modified after this UTC date. Null = no constraint.</summary>
    public DateTime? ModifiedAfter { get; set; }

    /// <summary>Only migrate files modified before this UTC date. Null = no constraint.</summary>
    public DateTime? ModifiedBefore { get; set; }

    /// <summary>
    /// Glob-style filename patterns to include (e.g. ["Report_*", "Invoice_*"]).
    /// Empty list = include all filenames.
    /// </summary>
    public List<string> IncludeNamePatterns { get; set; } = new();
}
