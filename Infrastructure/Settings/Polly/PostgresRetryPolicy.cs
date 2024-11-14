using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Wrap;

namespace Infrastructure.Settings.Polly;

public class PostgresRetryPolicy(ILogger<PostgresRetryPolicy> logger) : IPostgresRetryPolicy
{
    public readonly AsyncRetryPolicy Policy = global::Polly.Policy.Handle<NpgsqlException>()
        .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3, fastFirst: true), 
            onRetry: (_, time, retryCount, _) => logger.LogWarning(
                "Retrying to db after: {time}, reattempt number: {attemptCount}", time, retryCount));
    
    public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action) => 
        await Policy.ExecuteAsync(action.Invoke);
    
    public async Task ExecuteAsync(Func<Task> action) => 
        await Policy.ExecuteAsync(action.Invoke);
    
    public async Task ExecuteAsync(Action action) => 
        await Policy.ExecuteAsync(() =>
        {
            action.Invoke();
            return Task.CompletedTask;
        });
}