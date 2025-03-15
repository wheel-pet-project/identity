namespace Infrastructure.Adapters.Kafka;

public class KafkaTopicsConfiguration
{
    public required string AccountCreatedTopic { get; set; }
    public required string PasswordRecoverTokenCreatedTopic { get; set; }
}