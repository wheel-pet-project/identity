using Core.Domain.SharedKernel;
using Dapper;
using MediatR;
using Newtonsoft.Json;
using Quartz;

namespace Infrastructure.Adapters.Postgres.Outbox;

public class OutboxBackgroundJob(
    DbSession session,
    IMediator mediator) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var querySql = @"
SELECT event_id, type, content, occurred_on_utc, processed_on_utc
FROM outbox
LIMIT 50";

        var markAsProcessedSql = @"
UPDATE outbox
SET processed_on_utc = @ProcessedOnUtc
WHERE event_id = @EventId";
        
        var outboxEventsModels = await session.Connection.QueryAsync<OutboxEventModel>(querySql);

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        
        var eventModels = outboxEventsModels.ToList();
        if (eventModels.Any())
        {
            foreach (var @event in eventModels)
            {
                var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(@event.Content, jsonSettings);
                if (domainEvent is not null)
                {
                    await mediator.Publish(domainEvent, context.CancellationToken);

                    var command = new CommandDefinition(markAsProcessedSql,
                        new { ProcessedOnUtc = DateTime.UtcNow, @event.EventId }, 
                        session.Transaction);
                
                    await session.Connection.ExecuteAsync(command);
                }
            }
        }
        
        session.Dispose();
    }
}