using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Api.Interceptors;

public class TracingInterceptor : Interceptor
{
    private readonly ActivitySource _activitySource = new("DrivingLicense");

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        using var activity = _activitySource
            .StartActivity($"handling {request.GetType().Name}")!
            .SetTag("correlation-id",
                context.RequestHeaders.FirstOrDefault(x =>
                        x.Key.Equals("X-Correlation-Id", StringComparison.InvariantCultureIgnoreCase))
                    ?.Value ??
                "without correlation id");

        return await continuation(request, context);
    }
}