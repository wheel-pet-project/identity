using Core.Domain.SharedKernel.Exceptions.AlreadyHaveThisState;
using Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Npgsql;
using ArgumentException = Core.Domain.SharedKernel.Exceptions.ArgumentException.ArgumentException;

namespace Api.Interceptors;

public class ExceptionHandlerInterceptor(ILogger<ExceptionHandlerInterceptor> logger)
    : Interceptor
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
        catch (NpgsqlException e)
        {
            logger.LogError("NpgsqlException handled: {@exception}", e);
            throw new RpcException(new Status(StatusCode.Unavailable, "Db unavailable, please try again later."));
        }
        catch (ArgumentException e)
        {
            logger.LogWarning("ArgumentException handled: {@exception}", e);
            throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message));
        }
        catch (DataConsistencyViolationException e)
        {
            logger.LogCritical("DataConsistencyViolationException: {@exception}", e);
            throw new RpcException(new Status(StatusCode.Internal, "Entity invariant violation"));
        }
        catch (DomainRulesViolationException e)
        {
            logger.LogWarning("DomainRulesViolationException handled: {@exception}", e);
            throw new RpcException(new Status(StatusCode.FailedPrecondition, e.Message));
        }
        catch (AlreadyHaveThisStateException)
        {
            logger.LogWarning("AlreadyHaveThisStateException handled");
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Resource already have this state"));
        }
        catch (Exception e) when (e is not RpcException)
        {
            logger.LogCritical(
                "[EXCEPTION] type: {type}, eception: {@exception}",
                e.GetType().Name, e);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}