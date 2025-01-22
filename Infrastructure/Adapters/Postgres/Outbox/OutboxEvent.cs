namespace Infrastructure.Adapters.Postgres.Outbox;

public class OutboxEvent
{
    public required Guid EventId { get; init; }
    
    public required string Type { get; init; }
    
    public required string Content { get; init; }
    
    public required DateTime OccurredOnUtc { get; init; }

    public DateTime? ProcessedOnUtc { get; init; }
}