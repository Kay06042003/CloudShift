using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CloudShift.Application.Common.Interfaces;
using CloudShift.Domain.Entities;
using CloudShift.Domain.Enums;
using CloudShift.Domain.Messages;
using CloudShift.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

namespace CloudShift.Worker.Consumers;

public class MigrationJobConsumer : IConsumer<MigrationJobStartedEvent>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICloudStorageProvider _cloudStorageProvider;
    private readonly ILogger<MigrationJobConsumer> _logger;

    public MigrationJobConsumer(
        ApplicationDbContext dbContext,
        ICloudStorageProvider cloudStorageProvider,
        ILogger<MigrationJobConsumer> logger)
    {
        _dbContext = dbContext;
        _cloudStorageProvider = cloudStorageProvider;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MigrationJobStartedEvent> context)
    {
        var cancellationToken = context.CancellationToken;
        var jobId = context.Message.JobId;
        _logger.LogInformation("Received MigrationJobStartedEvent for JobId: {JobId}", jobId);

        var job = await _dbContext.MigrationJobs
            .Include(j => j.ProjectMapping)
                .ThenInclude(m => m!.SourceProfile)
            .Include(j => j.ProjectMapping)
                .ThenInclude(m => m!.DestProfile)
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job is null)
        {
            _logger.LogWarning("MigrationJob with JobId: {JobId} not found.", jobId);
            return;
        }

        if (job.ProjectMapping is null)
        {
            _logger.LogError("MigrationJob {JobId} has no ProjectMapping loaded.", jobId);
            job.Status = JobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var retryPolicy = CreateHttpRetryPolicy(jobId);
        var plannedFiles = await GetFullJobFilePlanAsync(job.ProjectMapping, cancellationToken);

        job.Status = JobStatus.Processing;
        job.StartedAt ??= DateTime.UtcNow;
        job.TotalItems = plannedFiles.Count;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var failedItems = 0;

        foreach (var file in plannedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transferLog = new FileTransferLog
            {
                MigrationJobId = job.Id,
                FileName = file.FileName,
                SourceFileId = file.SourceFileId,
                Size = file.Size,
                Status = FileTransferStatus.Pending
            };

            await _dbContext.FileTransferLogs.AddAsync(transferLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                await using var sourceStream = await OpenSourceStreamAsync(file, cancellationToken);

                await retryPolicy.ExecuteAsync(
                    ct => _cloudStorageProvider.TransferFileAsync(
                        file.SourcePath,
                        file.DestPath,
                        sourceStream,
                        ct),
                    cancellationToken);

                transferLog.Status = FileTransferStatus.Success;
                transferLog.TransferredAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "Transferred file {FileName} for JobId: {JobId}",
                    file.FileName,
                    job.Id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failedItems++;
                transferLog.Status = FileTransferStatus.Failed;
                transferLog.ErrorMessage = ex.Message;
                transferLog.TransferredAt = DateTime.UtcNow;

                _logger.LogError(
                    ex,
                    "Failed to transfer file {FileName} for JobId: {JobId}. SourcePath: {SourcePath}, DestPath: {DestPath}",
                    file.FileName,
                    job.Id,
                    file.SourcePath,
                    file.DestPath);
            }
            finally
            {
                job.ProcessedItems++;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        job.Status = failedItems == 0 ? JobStatus.Completed : JobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Completed MigrationJob {JobId}. TotalItems: {TotalItems}, ProcessedItems: {ProcessedItems}, FailedItems: {FailedItems}, FinalStatus: {Status}",
            job.Id,
            job.TotalItems,
            job.ProcessedItems,
            failedItems,
            job.Status);
    }

    private IAsyncPolicy CreateHttpRetryPolicy(Guid jobId)
    {
        return Policy
            .Handle<HttpRequestException>(IsTransientCloudHttpError)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, delay, attempt, _) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Retrying cloud transfer for JobId: {JobId}. Attempt: {Attempt}, Delay: {Delay}",
                        jobId,
                        attempt,
                        delay);
                });
    }

    private static bool IsTransientCloudHttpError(HttpRequestException exception)
    {
        return exception.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Unauthorized;
    }

    private static Task<IReadOnlyList<PlannedTransferFile>> GetFullJobFilePlanAsync(
        ProjectMapping mapping,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TODO: Replace with Google Drive recursive listing using mapping.FilterConfigJson.
        // This placeholder keeps orchestration, retry, and item logging testable now.
        IReadOnlyList<PlannedTransferFile> files =
        [
            new(
                SourceFileId: mapping.SourcePath,
                FileName: string.IsNullOrWhiteSpace(mapping.SourcePath) ? "root-placeholder" : mapping.SourcePath,
                SourcePath: mapping.SourcePath,
                DestPath: mapping.DestPath,
                Size: 0)
        ];

        return Task.FromResult(files);
    }

    private static Task<Stream> OpenSourceStreamAsync(
        PlannedTransferFile file,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TODO: Replace with Google Drive SDK media download stream.
        // Do not materialize the full file in RAM or on disk here.
        Stream sourceStream = Stream.Null;
        return Task.FromResult(sourceStream);
    }

    private sealed record PlannedTransferFile(
        string SourceFileId,
        string FileName,
        string SourcePath,
        string DestPath,
        long Size);
}
