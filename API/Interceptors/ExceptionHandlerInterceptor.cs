using Domain.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;
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
            logger.LogWarning("""
                              Application Exception. 
                              Title: {title}, description: {description}
                              """, ex.Title, ex.Description);
        
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Title));
        }
        catch (DomainException ex)
        {
            logger.LogError("""
                            Domain exception. 
                            Title: {title}, description: {description}
                            """, ex.Title, ex.Description);
        
            throw new RpcException(new Status(StatusCode.Internal, 
                "Internal server error"));
        }
        // catch (Exception ex)
        // {
        //     throw new RpcException(new Status(StatusCode.Internal, 
        //         "Internal server error"));
        // }
    }
}