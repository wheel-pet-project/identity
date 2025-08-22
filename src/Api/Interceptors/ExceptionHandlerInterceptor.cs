using Core.Domain.SharedKernel.Exceptions.InternalExceptions;
using Core.Domain.SharedKernel.Exceptions.PublicExceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Api.Interceptors;

public class ExceptionHandlerInterceptor(ILogger<ExceptionHandlerInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> next)
    {
        try
        {
            return await next(request, context);
        }
        catch (PublicException e)
        {
            logger.LogWarning("PublicException handled: {@exception}", e);
            throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message));
        }
        catch (InternalException e)
        {
            logger.LogCritical("DataConsistencyViolationException: {@exception}", e);
            throw new RpcException(new Status(StatusCode.Internal, "Internal error"));
        }
        catch (Exception e) when (e is not RpcException)
        {
            logger.LogCritical("Exception: {@exception}", e);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}