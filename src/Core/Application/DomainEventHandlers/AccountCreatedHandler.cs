using Core.Domain.AccountAggregate.DomainEvents;
using Core.Ports.Kafka;
using MediatR;

namespace Core.Application.DomainEventHandlers;

public class AccountCreatedHandler(IMessageBus messageBus)
    : INotificationHandler<AccountCreatedDomainEvent>
{
    public async Task Handle(AccountCreatedDomainEvent @event, CancellationToken cancellationToken)
    {
        await messageBus.Publish(@event, cancellationToken);
    }
}