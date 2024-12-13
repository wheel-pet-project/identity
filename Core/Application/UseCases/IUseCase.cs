using FluentResults;

namespace Core.Application.UseCases;

public interface IUseCase<in TRequest, TResponse>
{
    Task<Result<TResponse>> Execute(TRequest request);
}