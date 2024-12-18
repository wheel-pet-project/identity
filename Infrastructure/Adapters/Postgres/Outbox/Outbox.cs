using Core.Domain.SharedKernel;
using Core.Ports.Postgres;
using Dapper;
using Newtonsoft.Json;

namespace Infrastructure.Adapters.Postgres.Outbox;

public class Outbox(DbSession session) : IOutbox
{
    public async Task PublishDomainEvents(IAggregate aggregate)
    {
        var sql = @"
INSERT INTO outbox (event_id, type, content, occurred_on_utc, processed_on_utc) 
VALUES (@EventId, @Type, @Content, @OccurredOnUtc, null);";

        var jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        
        var outboxEventModels = aggregate.DomainEvents.Select(e => new OutboxEventModel
        {
            EventId = e.EventId,
            Type = e.GetType().Name,
            Content = JsonConvert.SerializeObject(e, jsonSerializerSettings),
            OccurredOnUtc = DateTime.UtcNow
        });
        
        foreach (var eventModel in outboxEventModels)
        {
            var command = new CommandDefinition(sql, new
            {
                eventModel.EventId,
                eventModel.Type,
                eventModel.Content,
                eventModel.OccurredOnUtc
            }, session.Transaction);
            
            await session.Connection.ExecuteAsync(command);
        }
        
        aggregate.ClearDomainEvents();
    }
}