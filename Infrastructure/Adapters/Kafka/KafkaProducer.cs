using Core.Domain.ConfirmationTokenAggregate.DomainEvents;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using Core.Ports.Kafka;
using MassTransit;
using NotificationKafkaMessages;

namespace Infrastructure.Adapters.Kafka;

public class KafkaProducer(
    ITopicProducer<string, SendConfirmationEmailMessage> sendConfirmationEmailProducer,
    ITopicProducer<string, SendRecoverPasswordEmailMessage> passwordRecoverTokenCreatedProducer) 
    : IMessageBus
{
    public async Task Publish(ConfirmationTokenCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await sendConfirmationEmailProducer.Produce(key: domainEvent.EventId.ToString(),
            new SendConfirmationEmailMessage(domainEvent.Email, "someurl:" + domainEvent.ConfirmationToken),
            cancellationToken);
    }

    public async Task Publish(PasswordRecoverTokenCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await passwordRecoverTokenCreatedProducer.Produce(key: domainEvent.EventId.ToString(),
            new SendRecoverPasswordEmailMessage(domainEvent.Email, "someurl:" + domainEvent.RecoverToken),
            cancellationToken);
    }
}