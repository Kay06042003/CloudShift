using System;
using System.Threading.Tasks;
using CloudShift.Domain.Enums;
using CloudShift.Domain.Messages;
using CloudShift.Infrastructure.Data;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CloudShift.Worker.Consumers;

public class MigrationJobConsumer : IConsumer<MigrationJobStartedEvent>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MigrationJobConsumer> _logger;

    public MigrationJobConsumer(ApplicationDbContext dbContext, ILogger<MigrationJobConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MigrationJobStartedEvent> context)
    {
        var jobId = context.Message.JobId;
        _logger.LogInformation("Received MigrationJobStartedEvent for JobId: {JobId}", jobId);

        var job = await _dbContext.MigrationJobs.FindAsync(new object[] { jobId }, context.CancellationToken);

        if (job != null)
        {
            job.Status = JobStatus.Processing;
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Updated Job Status to Processing for JobId: {JobId}", jobId);
        }
        else
        {
            _logger.LogWarning("MigrationJob with JobId: {JobId} not found.", jobId);
        }
    }
}
