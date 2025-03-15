using Core.Domain.AccountAggregate.DomainEvents;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using Core.Ports.Kafka;
using From.IdentityKafkaEvents;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Infrastructure.Adapters.Kafka;

public class KafkaProducer(
    ITopicProducerProvider topicProducerProvider,
    IOptions<KafkaTopicsConfiguration> kafkaTopicsConfiguration)
    : IMessageBus
{
    private readonly KafkaTopicsConfiguration _configuration = kafkaTopicsConfiguration.Value;
    public async Task Publish(AccountCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var producer = topicProducerProvider.GetProducer<string, AccountCreated>(
            new Uri($"topic:{_configuration.AccountCreatedTopic}"));
        
        await producer.Produce(domainEvent.EventId.ToString(),
            new AccountCreated(domainEvent.EventId, domainEvent.AccountId, domainEvent.Email, domainEvent.Phone,
                "someurl:" + domainEvent.ConfirmationToken),
            Pipe.Execute<KafkaSendContext<string, AccountCreated>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }

    public async Task Publish(PasswordRecoverTokenCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var producer = topicProducerProvider.GetProducer<string, PasswordRecoverTokenCreated>(
            new Uri($"topic:{_configuration.PasswordRecoverTokenCreatedTopic}"));
        
        await producer.Produce(domainEvent.EventId.ToString(),
            new PasswordRecoverTokenCreated(domainEvent.EventId, domainEvent.AccountId,
                "someurl:" + domainEvent.RecoverToken),
            Pipe.Execute<KafkaSendContext<string, PasswordRecoverTokenCreated>>(ctx =>
                ctx.MessageId = domainEvent.EventId), cancellationToken);
    }
}