using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Api.Interceptors;

public class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Start handling request with correlation id: {correlationId}",
            context.RequestHeaders
                .FirstOrDefault(x => x.Key.Equals("X-Correlation-Id", StringComparison.InvariantCultureIgnoreCase))
                ?.Value ?? "_");

        var response = await continuation(request, context);

        logger.LogInformation("Handling ended, status code: {statusCode}, message: {message}",
            context.Status.StatusCode, context.Status.Detail);

        return response;
    }
}