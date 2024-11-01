using FluentResults;

namespace Application.Application.Interfaces;

public interface IUseCase<TRequest, TResponse>
{
    Task<Result<TResponse>> Execute(TRequest request);
}