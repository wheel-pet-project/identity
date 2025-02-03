using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace Infrastructure.Settings;

public class PostgresRetryPolicy(ILogger<PostgresRetryPolicy> logger)
{
    private readonly AsyncRetryPolicy _policy = Policy.Handle<NpgsqlException>()
        .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromSeconds(1),
                retryCount: 3,
                fastFirst: true),
            onRetry: (_, time, retryCount, _) => logger.LogWarning(
                "Retrying connection to db after: {time}, reattempt number: {attemptCount}", time, retryCount));

    public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action) =>
        await _policy.ExecuteAsync(action.Invoke);

    public async Task ExecuteAsync(Func<Task> action) =>
        await _policy.ExecuteAsync(action.Invoke);
}