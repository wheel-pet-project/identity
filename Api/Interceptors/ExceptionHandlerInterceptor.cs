using Domain.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Polly.CircuitBreaker;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace API.Interceptors;

public class ExceptionHandlerInterceptor(ILogger<ExceptionHandlerInterceptor> logger) 
    : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (ApplicationException ex)
        {
            logger.LogError("Application Exception, description: {description}", ex.Description);

            throw new RpcException(new Status(StatusCode.DataLoss, ex.Title));
        }
        catch (DomainException ex)
        {
            logger.LogError("Domain exception, description: {description}", ex.Description);

            throw new RpcException(new Status(StatusCode.Internal,
                "Internal server error"));
        }
        catch (IsolatedCircuitException ex)
        {
            logger.LogError("Db unavailable");

            throw new RpcException(new Status(StatusCode.Unavailable,
                "Db unavailable"));
        }
        catch (BrokenCircuitException ex)
        {
            logger.LogError("BrokenCircuitException");

            throw new RpcException(new Status(StatusCode.Unavailable,
                "Db error"));
        }
    }
}