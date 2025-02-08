using Core.Domain.AccountAggregate.DomainEvents;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using Core.Ports.Kafka;
using From.IdentityKafkaEvents;
using MassTransit;

namespace Infrastructure.Adapters.Kafka;

public class KafkaProducer(
    ITopicProducer<string, AccountCreated> sendConfirmationEmailProducer,
    ITopicProducer<string, PasswordRecoverTokenCreated> passwordRecoverTokenCreatedProducer) 
    : IMessageBus
{
    public async Task Publish(AccountCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await sendConfirmationEmailProducer.Produce(domainEvent.EventId.ToString(), 
            new AccountCreated(domainEvent.EventId, domainEvent.AccountId, domainEvent.Email, domainEvent.Phone, 
                "someurl:" + domainEvent.ConfirmationToken),
            Pipe.Execute<KafkaSendContext<string, AccountCreated>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }

    public async Task Publish(PasswordRecoverTokenCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await passwordRecoverTokenCreatedProducer.Produce(key: domainEvent.EventId.ToString(),
            new PasswordRecoverTokenCreated(domainEvent.EventId, domainEvent.AccountId,
                "someurl:" + domainEvent.RecoverToken),
            Pipe.Execute<KafkaSendContext<string, PasswordRecoverTokenCreated>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }
}