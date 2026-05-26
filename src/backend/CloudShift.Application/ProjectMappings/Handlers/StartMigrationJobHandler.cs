using CloudShift.Application.Common.Interfaces;
using CloudShift.Application.ProjectMappings.Commands;
using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Domain.Entities;
using CloudShift.Domain.Enums;
using CloudShift.Domain.Messages;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CloudShift.Application.ProjectMappings.Handlers;

public sealed class StartMigrationJobHandler : IRequestHandler<StartMigrationJobCommand, MigrationJobDto>
{
    private readonly IProjectMappingRepository _mappingRepository;
    private readonly IMigrationJobRepository _jobRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IProviderRoutePolicy _routePolicy;
    private readonly ILogger<StartMigrationJobHandler> _logger;

    public StartMigrationJobHandler(
        IProjectMappingRepository mappingRepository,
        IMigrationJobRepository jobRepository,
        IEventPublisher eventPublisher,
        IProviderRoutePolicy routePolicy,
        ILogger<StartMigrationJobHandler> logger)
    {
        _mappingRepository = mappingRepository;
        _jobRepository = jobRepository;
        _eventPublisher = eventPublisher;
        _routePolicy = routePolicy;
        _logger = logger;
    }

    public async Task<MigrationJobDto> Handle(
        StartMigrationJobCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting migration job. UserId: {UserId}, MappingId: {MappingId}, JobType: {JobType}",
            command.UserId,
            command.MappingId,
            command.JobType);

        var mapping = await _mappingRepository.GetByIdAsync(command.MappingId, cancellationToken)
            ?? throw new KeyNotFoundException($"ProjectMapping '{command.MappingId}' was not found.");

        if (mapping.UserId != command.UserId)
        {
            _logger.LogWarning(
                "Rejected migration job start because mapping is not owned by the user. UserId: {UserId}, MappingId: {MappingId}, MappingOwnerId: {MappingOwnerId}",
                command.UserId,
                command.MappingId,
                mapping.UserId);

            throw new UnauthorizedAccessException("The mapping is not owned by the requested user.");
        }

        if (mapping.SourceProfile is null || mapping.DestProfile is null)
        {
            throw new InvalidOperationException("The mapping must include source and destination profiles.");
        }

        if (!_routePolicy.CanMigrate(mapping.SourceProfile.Provider, mapping.DestProfile.Provider))
        {
            throw new InvalidOperationException(
                $"Migration route '{mapping.SourceProfile.Provider}' to '{mapping.DestProfile.Provider}' is not supported.");
        }

        var job = new MigrationJob
        {
            ProjectMappingId = mapping.Id,
            Status = JobStatus.Queued,
            JobType = command.JobType,
            TotalItems = 0,
            ProcessedItems = 0,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _jobRepository.AddAsync(job, cancellationToken);

        await _eventPublisher.PublishAsync(new MigrationJobStartedEvent
        {
            JobId = saved.Id,
            MappingId = mapping.Id,
            JobType = command.JobType,
            EnqueuedAt = DateTime.UtcNow
        }, cancellationToken);

        _logger.LogInformation(
            "Queued migration job. JobId: {JobId}, UserId: {UserId}, MappingId: {MappingId}, SourceProvider: {SourceProvider}, DestinationProvider: {DestinationProvider}",
            saved.Id,
            command.UserId,
            mapping.Id,
            mapping.SourceProfile.Provider,
            mapping.DestProfile.Provider);

        return new MigrationJobDto(
            saved.Id,
            saved.ProjectMappingId,
            saved.Status,
            saved.Status.ToString(),
            saved.JobType,
            saved.JobType.ToString(),
            saved.TotalItems,
            saved.ProcessedItems,
            saved.StartedAt,
            saved.CompletedAt,
            saved.CreatedAt);
    }
}
