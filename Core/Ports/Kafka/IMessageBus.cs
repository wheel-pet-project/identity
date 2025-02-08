using Core.Domain.AccountAggregate.DomainEvents;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;

namespace Core.Ports.Kafka;

public interface IMessageBus
{
    Task Publish(AccountCreatedDomainEvent domainEvent, CancellationToken cancellationToken);
    
    Task Publish(PasswordRecoverTokenCreatedDomainEvent domainEvent, CancellationToken cancellationToken);
}