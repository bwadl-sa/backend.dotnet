using Bwadl.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Bwadl.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only cache GET operations (queries)
        if (!typeof(TRequest).Name.Contains("Query"))
        {
            return await next();
        }

        var cacheKey = GenerateCacheKey(request);
        
        _logger.LogInformation("Checking cache for key: {CacheKey}", cacheKey);
        
        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
        
        var response = await next();
        
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15), cancellationToken);
        _logger.LogInformation("Cached response for key: {CacheKey}", cacheKey);
        
        return response;
    }

    private static string GenerateCacheKey(TRequest request)
    {
        var requestName = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request);
        return $"{requestName}:{requestJson.GetHashCode()}";
    }
}
