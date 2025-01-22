using Core.Domain.ConfirmationTokenAggregate.DomainEvents;
using Core.Ports.Kafka;
using MediatR;

namespace Core.Application.DomainEventHandlers;

public class ConfirmationTokenCreatedHandler(IMessageBus messageBus) 
    : INotificationHandler<ConfirmationTokenCreatedDomainEvent>
{
    public async Task Handle(ConfirmationTokenCreatedDomainEvent @event, 
        CancellationToken cancellationToken)
    {
        await messageBus.Publish(@event, cancellationToken);
    }
}