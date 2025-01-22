using System.Collections.Immutable;
using Core.Domain.SharedKernel;
using Core.Ports.Postgres;
using Dapper;
using Newtonsoft.Json;

namespace Infrastructure.Adapters.Postgres.Outbox;

public class Outbox(DbSession session) : IOutbox
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
    
    public async Task PublishDomainEvents(IAggregate aggregate)
    {
        var outboxEvents = aggregate.DomainEvents.Select(e => new OutboxEvent
        {
            EventId = e.EventId,
            Type = e.GetType().Name,
            Content = JsonConvert.SerializeObject(e, _jsonSerializerSettings),
            OccurredOnUtc = DateTime.UtcNow
        }).AsList().AsReadOnly();
        
        foreach (var @event in outboxEvents)
        {
            var command = new CommandDefinition(_sql, new
            {
                @event.EventId,
                @event.Type,
                @event.Content,
                @event.OccurredOnUtc
            }, session.Transaction);
            
            await session.Connection.ExecuteAsync(command);
        }
    }
    
    private readonly string _sql = 
        """
        INSERT INTO outbox (event_id, type, content, occurred_on_utc, processed_on_utc) 
        VALUES (@EventId, @Type, @Content, @OccurredOnUtc, null);
        """;
}