using Core.Application.UseCases;
using FluentResults;
using MediatR;

namespace Api.PipelineBehaviours;

public class LoggingPipelineBehaviour<TRequest, TResponse>(
    ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger) 
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : BaseRequest 
    where TResponse : IResultBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation($"[START] [{request.CorrelationId}] handling {request.GetType().Name}");
        
        var response = await next();
        var responseStatus = response.IsSuccess
            ? "SUCCESS"
            : $"FAILURE: {string.Join(',', response.Errors.Select(x => x.Message))}";
        
        logger.LogInformation(
            $"[END] [{request.CorrelationId}] handling {request.GetType().Name} -> {responseStatus}");
        
        return response;
    }
}