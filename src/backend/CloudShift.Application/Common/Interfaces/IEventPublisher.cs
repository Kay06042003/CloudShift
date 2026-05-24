using CloudShift.Domain.Messages;

namespace CloudShift.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the message broker (currently MassTransit / RabbitMQ).
/// Lives in Application so handlers can publish events without taking a
/// direct dependency on the MassTransit NuGet packages.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a <see cref="MigrationJobStartedEvent"/> to the message broker.
    /// </summary>
    Task PublishAsync(MigrationJobStartedEvent @event, CancellationToken cancellationToken = default);
}
