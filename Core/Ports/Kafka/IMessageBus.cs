using Core.Domain.ConfirmationTokenAggregate.DomainEvents;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;

namespace Core.Ports.Kafka;

public interface IMessageBus
{
    Task Publish(ConfirmationTokenCreatedDomainEvent domainEvent, CancellationToken cancellationToken);
    
    Task Publish(PasswordRecoverTokenCreatedDomainEvent domainEvent, CancellationToken cancellationToken);
}