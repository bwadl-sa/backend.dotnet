using MediatR;
using Microsoft.Extensions.Logging;
using Polly;

namespace Bwadl.Application.Common.Behaviors;

public class ResiliencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ResiliencyBehavior<TRequest, TResponse>> _logger;

    public ResiliencyBehavior(ILogger<ResiliencyBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} for {RequestName} after {Delay}ms due to: {Exception}",
                        retryCount, requestName, timespan.TotalMilliseconds, exception.Message);
                });

        return await retryPolicy.ExecuteAsync(async () =>
        {
            return await next();
        });
    }
}
