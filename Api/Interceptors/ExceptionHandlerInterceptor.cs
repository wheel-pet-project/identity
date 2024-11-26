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
            logger.LogError("NpgsqlException, name: {name}, exception: {exception}", nameof(ex), ex);

            throw new RpcException(new Status(StatusCode.Unavailable, "Db unavailable, please try again later."));
        }
        catch (Exception ex)
        {
            logger.LogCritical("Exception, name: {name}, message: {description}, inner exception: {@innerException}", 
                nameof(ex), ex.Message, ex.InnerException);

            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}