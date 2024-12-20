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
        SELECT event_id AS EventId, type AS Type, content AS Content, occurred_on_utc AS OccurredOnUtc, 
               processed_on_utc AS ProcessedOnUtc
        FROM outbox
        WHERE processed_on_utc IS NULL
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
            session.Transaction = session.Connection.BeginTransaction();
            foreach (var domainEvent in eventModels.Select(ev => 
                         JsonConvert.DeserializeObject<DomainEvent>(ev.Content, jsonSettings)).OfType<DomainEvent>())
            {
                await mediator.Publish(domainEvent, context.CancellationToken);
                    
                var command = new CommandDefinition(markAsProcessedSql,
                    new { ProcessedOnUtc = DateTime.UtcNow, domainEvent.EventId }, 
                    session.Transaction);
                    
                await session.Connection.ExecuteAsync(command);
            }
            session.Transaction.Commit();
        }
        
        session.Dispose();
    }
}