using CloudShift.Application.Common.Interfaces;
using CloudShift.Domain.Messages;
using MassTransit;

namespace CloudShift.Infrastructure.Messaging;

/// <summary>
/// MassTransit implementation of <see cref="IEventPublisher"/>.
/// Wraps <see cref="IPublishEndpoint"/> so the Application layer
/// has no direct dependency on the MassTransit NuGet packages.
/// </summary>
public sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync(MigrationJobStartedEvent @event, CancellationToken cancellationToken = default)
        => _publishEndpoint.Publish(@event, cancellationToken);
}
