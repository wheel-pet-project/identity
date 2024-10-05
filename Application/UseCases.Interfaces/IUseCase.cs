namespace Application.UseCases.Interfaces;

public interface IUseCase<TRequest, TResponse>
{
    Task<TResponse> Execute(TRequest request);
}