using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using Core.Ports.Kafka;
using MediatR;

namespace Core.Application.DomainEventHandlers;

public class PasswordRecoverTokenCreatedHandler(IMessageBus messageBus) 
    : INotificationHandler<PasswordRecoverTokenCreatedDomainEvent>
{
    public async Task Handle(PasswordRecoverTokenCreatedDomainEvent notification, 
        CancellationToken cancellationToken) => 
        await messageBus.Publish(notification, cancellationToken);
}