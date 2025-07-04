namespace Bwadl.Shared.Configuration;

public class CacheOptions
{
    public const string SectionName = "Cache";
    
    public string Provider { get; set; } = "Memory";
    public int DefaultExpirationMinutes { get; set; } = 15;
    public RedisOptions Redis { get; set; } = new();
}

public class RedisOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Database { get; set; } = 0;
    public string KeyPrefix { get; set; } = string.Empty;
}
