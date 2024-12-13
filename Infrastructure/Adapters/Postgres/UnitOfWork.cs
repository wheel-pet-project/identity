using Core.Domain.SharedKernel.Errors;
using Core.Ports.Postgres;
using FluentResults;
using Infrastructure.Settings;

namespace Infrastructure.Adapters.Postgres;

public class UnitOfWork(DbSession session, PostgresRetryPolicy retryPolicy) : IUnitOfWork
{
    public async Task BeginTransaction() =>
        await retryPolicy.ExecuteAsync(() => session.Transaction = session.Connection.BeginTransaction());

    public async Task<Result> Commit()
    {
        var result = Result.Ok();
        try
        {
            await retryPolicy.ExecuteAsync(() => session.Transaction.Commit());
        }
        catch
        {
            await retryPolicy.ExecuteAsync(() => session.Transaction.Rollback());
            result = Result.Fail(new TransactionFail("Transaction failed"));
        }
        finally
        {
            Dispose();
        }
        
        return result;
    }

    public void Dispose() => session.Transaction.Dispose();
}