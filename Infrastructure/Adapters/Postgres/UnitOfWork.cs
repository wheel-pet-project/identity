using Core.Domain.SharedKernel.Errors;
using Core.Ports.Postgres;
using FluentResults;
using Infrastructure.Settings;

namespace Infrastructure.Adapters.Postgres;

public class UnitOfWork(
    DbSession session, 
    PostgresRetryPolicy retryPolicy) 
    : IUnitOfWork
{
    public async Task BeginTransaction() =>
        await retryPolicy.ExecuteAsync(async () =>
            session.Transaction = await session.Connection.BeginTransactionAsync());

    public async Task<Result> Commit()
    {
        var result = Result.Ok();
        try
        {
            await retryPolicy.ExecuteAsync(() => session.Transaction!.CommitAsync());
        }
        catch
        {
            await retryPolicy.ExecuteAsync(() => session.Transaction!.RollbackAsync());
            result = Result.Fail(new TransactionFail("Transaction failed"));
        }
        finally
        {
            Dispose();
        }
        
        return result;
    }

    public void Dispose()
    {
        session.Transaction?.Dispose();
        session.Dispose();
        GC.SuppressFinalize(this);
    }
}