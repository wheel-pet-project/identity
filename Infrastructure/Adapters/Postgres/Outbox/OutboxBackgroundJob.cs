using System.Collections.Concurrent;
using System.Data;
using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.InternalExceptions;
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
        var outboxEvents = (await connection.QueryAsync<OutboxEvent>(QuerySql, transaction))
            .AsList()
            .AsReadOnly();

        if (outboxEvents.Count > 0)
        {
            var updateQueue = new ConcurrentQueue<EventUpdate>();

            var domainEvents = outboxEvents
                .Select(ev =>
                    JsonConvert.DeserializeObject<DomainEvent>(ev.Content, _jsonSerializerSettings))
                .OfType<DomainEvent>()
                .AsList()
                .AsReadOnly();

            var publishTasks = domainEvents
                .Select(domainEvent => PublishToMediator(domainEvent, updateQueue,
                    jobExecutionContext.CancellationToken))
                .ToList()
                .AsReadOnly();

            await Task.WhenAll(publishTasks);

            var updates = updateQueue.ToList();
            var formattedSql = FormatSql(updates);
            var parameters = GenerateDynamicParameters(updates);

            await connection.ExecuteAsync(formattedSql, parameters, transaction);

            await transaction.CommitAsync();
        }

        async Task PublishToMediator(
            DomainEvent @event,
            ConcurrentQueue<EventUpdate> updateQueue,
            CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Publish(@event, cancellationToken);
                updateQueue.Enqueue(new EventUpdate(@event.EventId, DateTime.UtcNow));
            }
            catch (AlreadyHaveThisStateException)
            {
                updateQueue.Enqueue(new EventUpdate(@event.EventId, DateTime.UtcNow));
            }
            catch (Exception e)
            {
                updateQueue.Enqueue(new EventUpdate(@event.EventId));
                logger.LogError(
                    "Failed of processing outbox events and save update, exception: {e}", e);
            }
        }
    }

    private string FormatSql(List<EventUpdate> updates)
    {
        var paramNames = string.Join(",", updates.Select((_, i) => $"(@EventId{i}, @ProcessedOnUtc{i})"));
        var formattedSql = string.Format(UpdateSql, paramNames);

        return formattedSql;
    }

    private DynamicParameters GenerateDynamicParameters(List<EventUpdate> updates)
    {
        var parameters = new DynamicParameters();
        for (var i = 0; i < updates.Count; i++)
        {
            parameters.Add($"EventId{i}", updates[i].EventId);
            parameters.Add($"ProcessedOnUtc{i}", (object?)updates[i].ProcessedOnUtc ?? DBNull.Value, DbType.DateTime);
        }

        return parameters;
    }

    private class EventUpdate(Guid eventId, DateTime? processedOnUtc = null)
    {
        public Guid EventId { get; } = eventId;
        public DateTime? ProcessedOnUtc { get; } = processedOnUtc;
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