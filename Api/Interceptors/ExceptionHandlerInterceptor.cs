using Domain.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Npgsql;
using ApplicationException = Application.Exceptions.ApplicationException;

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
        catch (ApplicationException ex)
        {
            logger.LogError("ApplicationException, name: {name}, description: {description}, inner exception: {@innerException}", 
                nameof(ex), ex.Description, ex.InnerException);

            throw new RpcException(new Status(StatusCode.DataLoss, ex.Title));
        }
        catch (DomainException ex)
        {
            logger.LogCritical("DomainException, name: {name}, description: {description}, inner exception: {@innerException}", 
                nameof(ex), ex.Description, ex.InnerException);

            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
        catch (NpgsqlException ex)
        {
            logger.LogError("NpgsqlException, name: {name}, exception: {exception}", nameof(ex), ex);

            throw new RpcException(new Status(StatusCode.Unavailable, "Db unavailable, please try again later."));
        }
    }
}