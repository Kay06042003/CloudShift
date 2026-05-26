using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Domain.Enums;
using MediatR;

namespace CloudShift.Application.ProjectMappings.Commands;

/// <summary>
/// CQRS Command: Queues a new <c>MigrationJob</c> for the given mapping
/// and publishes a <c>MigrationJobStartedEvent</c> to RabbitMQ.
/// Returns the newly created <see cref="MigrationJobDto"/>.
/// </summary>
public sealed record StartMigrationJobCommand(
    Guid UserId,
    Guid MappingId,
    JobType JobType = JobType.Full
) : IRequest<MigrationJobDto>;
