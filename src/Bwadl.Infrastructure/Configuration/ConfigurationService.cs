using Bwadl.Application.Common.Interfaces;
using Bwadl.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bwadl.Infrastructure.Configuration;

public interface IConfigurationService
{
    Task<T> GetOptionsAsync<T>(CancellationToken cancellationToken = default) where T : class, new();
    Task<string> GetSecretConfigurationAsync(string key, CancellationToken cancellationToken = default);
    Task<SecurityOptions> GetSecurityOptionsWithSecretsAsync(CancellationToken cancellationToken = default);
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ISecretManager _secretManager;
    private readonly IServiceProvider _serviceProvider;

    public ConfigurationService(
        IConfiguration configuration,
        ISecretManager secretManager,
        IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _secretManager = secretManager;
        _serviceProvider = serviceProvider;
    }

    public Task<T> GetOptionsAsync<T>(CancellationToken cancellationToken = default) where T : class, new()
    {
        // Use IOptions<T> if available, otherwise bind from configuration
        var options = _serviceProvider.GetService(typeof(IOptions<T>)) as IOptions<T>;
        if (options != null)
        {
            return Task.FromResult(options.Value);
        }

        var result = new T();
        var sectionName = GetSectionName<T>();
        _configuration.GetSection(sectionName).Bind(result);
        return Task.FromResult(result);
    }

    public async Task<string> GetSecretConfigurationAsync(string key, CancellationToken cancellationToken = default)
    {
        // First try configuration (for development)
        var configValue = _configuration[key];
        if (!string.IsNullOrEmpty(configValue))
        {
            return configValue;
        }

        // Then try secret manager (for production)
        return await _secretManager.GetSecretAsync(key, cancellationToken);
    }

    public async Task<SecurityOptions> GetSecurityOptionsWithSecretsAsync(CancellationToken cancellationToken = default)
    {
        var securityOptions = await GetOptionsAsync<SecurityOptions>(cancellationToken);
        
        // Retrieve JWT secret from secret manager
        securityOptions.Jwt.SecretKey = await GetSecretConfigurationAsync("Jwt:SecretKey", cancellationToken);
        
        return securityOptions;
    }

    private static string GetSectionName<T>()
    {
        var type = typeof(T);
        
        // Look for a SectionName constant
        var sectionNameField = type.GetField("SectionName");
        if (sectionNameField?.GetValue(null) is string sectionName)
        {
            return sectionName;
        }
        
        // Fallback to class name without "Options" suffix
        var typeName = type.Name;
        return typeName.EndsWith("Options") 
            ? typeName[..^7] // Remove "Options" suffix
            : typeName;
    }
}
