using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Wrap;

namespace Infrastructure.Settings.Polly;

public class RetryPolicy : IDapperRetryPolicy
{
    private readonly ILogger<RetryPolicy> _logger;
    private readonly AsyncPolicyWrap _policyWrap;
    
    private const int TransientErrorRetries = 10;

    public RetryPolicy(ILogger<RetryPolicy> logger)
    {
        _logger = logger;
        // var retryPolicy = Policy.Handle<Exception>()
        //     .WaitAndRetry(
        //         retryCount: TransientErrorRetries,
        //         sleepDurationProvider: attempt => TimeSpan.FromSeconds(1),
        //         onRetry: LogRetryAction);
        // var circuitBreakerPolicy = Policy.Handle<Exception>()
        //     .CircuitBreaker(3, TimeSpan.FromSeconds(10));
        // _policyWrap = retryPolicy.Wrap(circuitBreakerPolicy);

        var retryPolicy = Policy.Handle<NpgsqlException>()
            .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(1),
            onRetry: (exception, timeSpan, retryCount, context) => logger.LogWarning($"Retrying to db after {timeSpan.TotalSeconds} seconds. Attempt: {retryCount}"));

        var circuitBreakerPolicy = Policy.Handle<NpgsqlException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(2),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(1));

        _policyWrap = retryPolicy.WrapAsync(circuitBreakerPolicy);
    }

    private void LogBreakAction(Exception exception, TimeSpan timeSpan)
    {
        _logger.LogWarning(
            exception,
            $"CurcuitBreak: {((NpgsqlException)exception).ErrorCode}");
    }
    private void LogRetryAction(Exception exception, TimeSpan sleepTime, int reattemptCount, Context context) =>
        _logger.LogWarning(
            exception,
            $"Transient DB Failure while executing query, error number: {((NpgsqlException)exception).ErrorCode} reattempt number: {reattemptCount}");
    
    public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action) => 
        await _policyWrap.ExecuteAsync(action.Invoke);
}