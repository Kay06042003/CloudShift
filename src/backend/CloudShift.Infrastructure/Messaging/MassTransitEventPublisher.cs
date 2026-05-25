using CloudShift.Application.Common.Interfaces;
using CloudShift.Domain.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CloudShift.Infrastructure.Messaging;

/// <summary>
/// MassTransit implementation of <see cref="IEventPublisher"/>.
/// Wraps <see cref="IPublishEndpoint"/> so the Application layer
/// has no direct dependency on the MassTransit NuGet packages.
/// </summary>
public sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventPublisher> _logger;

    public MassTransitEventPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<MassTransitEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync(MigrationJobStartedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Publishing MigrationJobStartedEvent. JobId: {JobId}, MappingId: {MappingId}, JobType: {JobType}",
            @event.JobId,
            @event.MappingId,
            @event.JobType);

        await _publishEndpoint.Publish(@event, cancellationToken);
    }
}
