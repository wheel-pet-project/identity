using FluentResults;

namespace Core.Ports.Postgres;

public interface IUnitOfWork : IDisposable
{
    Task BeginTransaction();

    Task<Result> Commit();
}