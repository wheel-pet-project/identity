using Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Npgsql;
using ArgumentException = Core.Domain.SharedKernel.Exceptions.ArgumentException.ArgumentException;

namespace Api;

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
        catch (NpgsqlException ex)
        {
            logger.LogError("NpgsqlException handled: {exception}", ex);
            throw new RpcException(new Status(StatusCode.Unavailable, "Db unavailable, please try again later."));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("ArgumentException handled");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (DataConsistencyViolationException ex)
        {
            logger.LogCritical("DataConsistencyViolationException: {exception}", ex);
            throw new RpcException(new Status(StatusCode.Internal, "Entity invariant violation"));
        }
        catch (DomainRulesViolationException ex)
        {
            logger.LogWarning("DomainRulesViolationException handled");
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (Exception ex) when (ex is not RpcException)
        {
            logger.LogCritical(
                "[EXCEPTION] type: {type}, message: {description}, exception: {@exception}, inner exception: {@innerException}",
                ex.GetType().Name, ex.Message, ex, ex.InnerException);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}