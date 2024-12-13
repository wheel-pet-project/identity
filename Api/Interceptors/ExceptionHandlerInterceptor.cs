using Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Npgsql;

namespace Api.Interceptors;

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
        catch (NpgsqlException ex)
        {
            logger.LogError("[EXCEPTION] NpgsqlException: {exception}", ex);

            throw new RpcException(new Status(StatusCode.Unavailable, "Db unavailable, please try again later."));
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("[EXCEPTION] ValidationException handled");

            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(' ', ex.Errors.Select(x => x.ErrorMessage))));
        }
        catch (DataConsistencyViolationException ex)
        {
            logger.LogCritical("[EXCEPTION] DataConsistencyViolationException: {exception}", ex);

            throw new RpcException(new Status(StatusCode.Internal, "Entity invariant violation"));
        }
        catch (Exception ex) when(ex is not RpcException)
        {
            logger.LogCritical(
                "[EXCEPTION] type: {type}, message: {description}, exception: {@exception}, inner exception: {@innerException}",
                ex.GetType().Name, ex.Message, ex, ex.InnerException);
            
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}