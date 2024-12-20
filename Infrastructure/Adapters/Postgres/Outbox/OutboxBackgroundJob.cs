using System.Data;
using Core.Domain.SharedKernel;
using Dapper;
using MediatR;
using Newtonsoft.Json;
using Quartz;

namespace Infrastructure.Adapters.Postgres.Outbox;

public class OutboxBackgroundJob(
    IDbConnection connection,
    IMediator mediator) : IJob
{
    private const string QuerySql = @"
    SELECT event_id AS EventId, type AS Type, content AS Content, occurred_on_utc AS OccurredOnUtc, 
        processed_on_utc AS ProcessedOnUtc
    FROM outbox
    WHERE processed_on_utc IS NULL
    LIMIT 50";

    private const string MarkAsProcessedSql = @"
    UPDATE outbox
    SET processed_on_utc = @ProcessedOnUtc
    WHERE event_id = @EventId";
    
    public async Task Execute(IJobExecutionContext context)
    {
        connection.Open();
        var outboxEventsModels = await connection.QueryAsync<OutboxEventModel>(QuerySql);
        
        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        
        var eventModels = outboxEventsModels.ToList();
        if (eventModels.Any())
        {
            var transaction = connection.BeginTransaction();
            try
            {
                foreach (var domainEvent in eventModels.Select(ev => JsonConvert
                                 .DeserializeObject<DomainEvent>(ev.Content, jsonSettings)).OfType<DomainEvent>())
                {
                    await mediator.Publish(domainEvent, context.CancellationToken);
                    
                    var command = new CommandDefinition(MarkAsProcessedSql,
                        new { ProcessedOnUtc = DateTime.UtcNow, domainEvent.EventId }, 
                        transaction);
                    
                    await connection.ExecuteAsync(command);
                }
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
        }
        
        connection.Dispose();
    }
}