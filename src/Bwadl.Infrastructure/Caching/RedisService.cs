using Bwadl.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Bwadl.Infrastructure.Caching;

public class RedisService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger = Log.ForContext<RedisService>();

    public RedisService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _logger.Information("Getting cache value for key: {Key}", key);
        
        await Task.Delay(1, cancellationToken); // Simulate async operation
        
        if (_cache.TryGetValue(key, out var cached) && cached is T result)
        {
            _logger.Information("Cache hit for key: {Key}", key);
            return result;
        }
        
        _logger.Information("Cache miss for key: {Key}", key);
        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        _logger.Information("Setting cache value for key: {Key}", key);
        
        await Task.Delay(1, cancellationToken); // Simulate async operation
        
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        
        _cache.Set(key, value, options);
        
        _logger.Information("Cache value set successfully for key: {Key}", key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.Information("Removing cache value for key: {Key}", key);
        
        await Task.Delay(1, cancellationToken); // Simulate async operation
        
        _cache.Remove(key);
        
        _logger.Information("Cache value removed successfully for key: {Key}", key);
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        _logger.Information("Pattern-based cache removal not implemented for memory cache");
        // Memory cache doesn't support pattern-based removal easily
        return Task.CompletedTask;
    }
}
