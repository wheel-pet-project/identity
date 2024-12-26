using System.Data;
using System.Reflection;
using Core.Domain.SharedKernel;
using Dapper;
using JsonNet.ContractResolvers;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        using var session = new DbSession(connection);
        
        var outboxEventsModels = await session.Connection.QueryAsync<OutboxEventModel>(QuerySql);
        
        var jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new PrivateSetterContractResolver()
        };
        
        var eventModels = outboxEventsModels.ToList();
        if (eventModels.Any())
        {
            session.Transaction = session.Connection.BeginTransaction();
            try
            {
                foreach (var domainEvent in eventModels.Select(ev => JsonConvert
                                 .DeserializeObject<DomainEvent>(ev.Content, jsonSettings)).OfType<DomainEvent>())
                {
                    await mediator.Publish(domainEvent, context.CancellationToken);
                    
                    var command = new CommandDefinition(MarkAsProcessedSql,
                        new { ProcessedOnUtc = DateTime.UtcNow, domainEvent.EventId }, 
                        session.Transaction);
                    
                    await session.Connection.ExecuteAsync(command);
                }
                
                session.Transaction.Commit();
            }
            catch
            {
                session.Transaction.Rollback();
            }
        }
    }
}