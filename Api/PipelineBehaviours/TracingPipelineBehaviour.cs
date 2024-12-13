using System.Diagnostics;
using Core.Application.UseCases;
using MediatR;

namespace Api.PipelineBehaviours;

public class TracingPipelineBehaviour<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : BaseRequest
{
    private readonly ActivitySource _activitySource = new("Identity");

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var activity = _activitySource
            .StartActivity($"[START] handling {request.GetType().Name}")!
            .SetTag("CorrelationId", request.CorrelationId);

        var response = await next();
        
        return response;
    }
}