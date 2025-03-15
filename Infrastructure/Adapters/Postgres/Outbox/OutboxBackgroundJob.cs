using System.Collections.Concurrent;
using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.AlreadyHaveThisState;
using Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using Dapper;
using JsonNet.ContractResolvers;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Quartz;

namespace Infrastructure.Adapters.Postgres.Outbox;

[DisallowConcurrentExecution]
public class OutboxBackgroundJob(
    NpgsqlDataSource dataSource,
    IMediator mediator,
    ILogger<OutboxBackgroundJob> logger)
    : IJob
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new PrivateSetterContractResolver()
    };

    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var outboxEvents = (await connection.QueryAsync<OutboxEvent>(QuerySql, transaction)).AsList().AsReadOnly();

        if (outboxEvents.Count > 0)
        {
            var updateQueue = new ConcurrentQueue<Guid>();

            var domainEvents = outboxEvents
                .Select(ev => JsonConvert.DeserializeObject<DomainEvent>(ev.Content, _jsonSerializerSettings))
                .OfType<DomainEvent>()
                .AsList()
                .AsReadOnly();

            var publishTasks = domainEvents
                .Select(domainEvent => PublishToMediator(domainEvent, updateQueue,
                    jobExecutionContext.CancellationToken))
                .ToList()
                .AsReadOnly();

            await Task.WhenAll(publishTasks);

            var updateList = updateQueue.ToList();
            var paramNames = string.Join(",", updateList.Select((_, i) => $"(@EventId{i}, @ProcessedOnUtc{i})"));
            var formattedSql = string.Format(UpdateSql, paramNames);

            var processedTime = DateTime.UtcNow;
            var parameters = new DynamicParameters();
            for (var i = 0; i < updateList.Count; i++)
            {
                parameters.Add($"EventId{i}", updateList[i]);
                parameters.Add($"ProcessedOnUtc{i}", processedTime);
            }

            await connection.ExecuteAsync(formattedSql, parameters, transaction);

            await transaction.CommitAsync();
        }

        return;

        async Task PublishToMediator(
            DomainEvent @event,
            ConcurrentQueue<Guid> updateQueue,
            CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Publish(@event, cancellationToken);
                updateQueue.Enqueue(@event.EventId);
            }
            catch (AlreadyHaveThisStateException)
            {
                updateQueue.Enqueue(@event.EventId);
            }
            catch (Exception e)
            {
                logger.LogError("Failed of processing outbox events and save update, exception: {e}", e);
            }
        }
    }

    private const string QuerySql =
        """
        SELECT event_id AS EventId, 
               content AS Content
        FROM outbox
        WHERE processed_on_utc IS NULL
        ORDER BY occurred_on_utc
        LIMIT 50
        FOR UPDATE SKIP LOCKED 
        """;

    private const string UpdateSql =
        """
        UPDATE outbox
        SET processed_on_utc = new.processed_on_utc
        FROM (VALUES 
            {0}) AS new(event_id, processed_on_utc)
        WHERE outbox.event_id = new.event_id::uuid
        """;
}