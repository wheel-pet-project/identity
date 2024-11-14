namespace Infrastructure.Settings.Polly;

public interface IPostgresRetryPolicy
{
    Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action);

    Task ExecuteAsync(Func<Task> action);
    
    Task ExecuteAsync(Action action);
}