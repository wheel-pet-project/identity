using Core.Domain.ConfirmationTokenAggregate.DomainEvents;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using Core.Ports.Kafka;
using MassTransit;
using NotificationKafkaEvents;

namespace Infrastructure.Adapters.Kafka;

public class KafkaProducer(
    ITopicProducer<string, ConfirmationTokenCreated> sendConfirmationEmailProducer,
    ITopicProducer<string, PasswordRecoverTokenCreated> passwordRecoverTokenCreatedProducer) 
    : IMessageBus
{
    public async Task Publish(ConfirmationTokenCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await sendConfirmationEmailProducer.Produce(domainEvent.EventId.ToString(), 
            new ConfirmationTokenCreated(domainEvent.EventId, domainEvent.Email, 
                "someurl:" + domainEvent.ConfirmationToken),
            Pipe.Execute<KafkaSendContext<string, ConfirmationTokenCreated>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }

    public async Task Publish(PasswordRecoverTokenCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await passwordRecoverTokenCreatedProducer.Produce(key: domainEvent.EventId.ToString(),
            new PasswordRecoverTokenCreated(domainEvent.EventId, domainEvent.Email,
                "someurl:" + domainEvent.RecoverToken),
            Pipe.Execute<KafkaSendContext<string, PasswordRecoverTokenCreated>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }
}