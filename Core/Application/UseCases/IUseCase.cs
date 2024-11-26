using FluentResults;

namespace Core.Application.UseCases;

public interface IUseCase<TRequest, TResponse>
{
    Task<Result<TResponse>> Execute(TRequest request);
}