namespace Infrastructure.Settings.Polly;

public interface IDapperRetryPolicy
{
    Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action);
}