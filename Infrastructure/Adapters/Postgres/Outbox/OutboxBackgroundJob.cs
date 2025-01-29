using System.Data.Common;
using Core.Domain.SharedKernel;
using Dapper;
using JsonNet.ContractResolvers;
using MediatR;
using Newtonsoft.Json;
using Quartz;

namespace Infrastructure.Adapters.Postgres.Outbox;

[DisallowConcurrentExecution]
public class OutboxBackgroundJob(
    DbDataSource dataSource, 
    IMediator mediator) 
    : IJob
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new PrivateSetterContractResolver()
    };
    
    public async Task Execute(IJobExecutionContext context)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        
        var outboxEventsSequence = await connection.QueryAsync<OutboxEvent>(_querySql);
    
        var events = outboxEventsSequence.AsList().AsReadOnly();
        if (events.Count > 0)
        {
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                foreach (var domainEvent in events.Select(ev =>
                                 JsonConvert.DeserializeObject<DomainEvent>(ev.Content, _jsonSerializerSettings))
                             .OfType<DomainEvent>().AsList().AsReadOnly())
                {
                    await mediator.Publish(domainEvent, context.CancellationToken);

                    var command = new CommandDefinition(_markAsProcessedSql,
                        new { ProcessedOnUtc = DateTime.UtcNow, domainEvent.EventId },
                        transaction);

                    await connection.ExecuteAsync(command);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
            }
        }
    }
    
    private readonly string _querySql =
        """
        SELECT event_id AS EventId, type AS Type, content AS Content, 
               occurred_on_utc AS OccurredOnUtc, processed_on_utc AS ProcessedOnUtc
        FROM outbox
        WHERE processed_on_utc IS NULL
        ORDER BY occurred_on_utc
        LIMIT 50
        """;

    private readonly string _markAsProcessedSql =
        """
        UPDATE outbox
        SET processed_on_utc = @ProcessedOnUtc
        WHERE event_id = @EventId
        """;
}