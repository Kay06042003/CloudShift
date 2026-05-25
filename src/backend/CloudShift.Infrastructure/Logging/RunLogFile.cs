namespace CloudShift.Infrastructure.Logging;

public static class RunLogFile
{
    private const string SolutionFileName = "CloudShift.sln";

    public static string Prepare(string contentRootPath, bool truncateExisting)
    {
        var repositoryRoot = FindRepositoryRoot(contentRootPath)
            ?? FindRepositoryRoot(Directory.GetCurrentDirectory())
            ?? contentRootPath;

        var logDirectory = Path.Combine(repositoryRoot, "logs");
        Directory.CreateDirectory(logDirectory);

        var logFilePath = Path.Combine(logDirectory, "cloudshift.log");
        if (truncateExisting)
        {
            TryTruncate(logFilePath);
        }

        return logFilePath;
    }

    private static string? FindRepositoryRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static void TryTruncate(string logFilePath)
    {
        try
        {
            using var stream = new FileStream(
                logFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.ReadWrite);

            stream.SetLength(0);
        }
        catch (IOException)
        {
            // Another CloudShift process is already writing this run log.
        }
        catch (UnauthorizedAccessException)
        {
            // Let Serilog surface the write failure through its self-log/console path.
        }
    }
}
