namespace Application.Application.Interfaces;

public interface IQueryUseCase<TRequest, TResponse>
{
    Task<TResponse> Execute(TRequest request, CancellationToken ct = default);
}