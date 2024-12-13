using Core.Application.UseCases;
using FluentResults;
using FluentValidation;
using MediatR;

namespace Api.PipelineBehaviours;

public class ValidationPipelineBehaviour<TRequest, TResponse>(
    ILogger<ValidationPipelineBehaviour<TRequest, TResponse>> logger,
    IValidator<TRequest> validator)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : BaseRequest 
    where TResponse : ResultBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            logger.LogInformation(
                $"[VALIDATION] [{request.CorrelationId}] -> FAILURE: {string.Join(' ', result.Errors.Select(x => x.ErrorMessage))}");
            throw new ValidationException(result.Errors);
        }
            
        logger.LogInformation($"[VALIDATION] [{request.CorrelationId}] -> SUCCESS");
        
        var response = await next();
        
        return response;
    }
}