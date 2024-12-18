namespace Infrastructure.Adapters.Postgres.Outbox;

public class OutboxEventModel
{
    public Guid EventId { get; init; }
    
    public string Type { get; init; }
    
    public string Content { get; init; }
    
    public DateTime OccurredOnUtc { get; init; }
    
    public DateTime ProcessedOnUtc { get; init; }
}