namespace Infrastructure.Settings;

public class KafkaSettings
{
    public required IReadOnlyList<string> BootstrapServers { get; init; }
}