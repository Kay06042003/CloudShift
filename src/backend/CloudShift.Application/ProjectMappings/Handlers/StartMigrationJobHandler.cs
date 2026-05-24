using CloudShift.Application.Common.Interfaces;
using CloudShift.Application.ProjectMappings.Commands;
using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Domain.Entities;
using CloudShift.Domain.Messages;
using MediatR;

namespace CloudShift.Application.ProjectMappings.Handlers;

/// <summary>
/// Handles <see cref="StartMigrationJobCommand"/>.
/// <list type="number">
///   <item>Verifies the <c>ProjectMapping</c> exists.</item>
///   <item>Creates a <c>MigrationJob</c> row with status <c>Queued</c>.</item>
///   <item>Publishes <see cref="MigrationJobStartedEvent"/> to RabbitMQ via <see cref="IEventPublisher"/>.</item>
/// </list>
/// </summary>
public sealed class StartMigrationJobHandler : IRequestHandler<StartMigrationJobCommand, MigrationJobDto>
{
    private readonly IProjectMappingRepository _mappingRepository;
    private readonly IMigrationJobRepository   _jobRepository;
    private readonly IEventPublisher           _eventPublisher;

    public StartMigrationJobHandler(
        IProjectMappingRepository mappingRepository,
        IMigrationJobRepository jobRepository,
        IEventPublisher eventPublisher)
    {
        _mappingRepository = mappingRepository;
        _jobRepository     = jobRepository;
        _eventPublisher    = eventPublisher;
    }

    public async Task<MigrationJobDto> Handle(
        StartMigrationJobCommand command,
        CancellationToken cancellationToken)
    {
        // ── 1. Validate mapping exists ─────────────────────────────────────────
        var mapping = await _mappingRepository.GetByIdAsync(command.MappingId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"ProjectMapping '{command.MappingId}' was not found.");

        // ── 2. Create MigrationJob (status = Queued) ───────────────────────────
        var job = new MigrationJob
        {
            ProjectMappingId = mapping.Id,
            Status           = Domain.Enums.JobStatus.Queued,
            JobType          = command.JobType,
            TotalItems       = 0,
            ProcessedItems   = 0,
            CreatedAt        = DateTime.UtcNow
        };

        var saved = await _jobRepository.AddAsync(job, cancellationToken);

        // ── 3. Publish event to RabbitMQ via abstraction ───────────────────────
        await _eventPublisher.PublishAsync(new MigrationJobStartedEvent
        {
            JobId      = saved.Id,
            MappingId  = mapping.Id,
            JobType    = command.JobType,
            EnqueuedAt = DateTime.UtcNow
        }, cancellationToken);

        // ── 4. Return DTO ──────────────────────────────────────────────────────
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
            saved.CreatedAt
        );
    }
}
