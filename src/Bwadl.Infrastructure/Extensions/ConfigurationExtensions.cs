using Bwadl.Infrastructure.Configuration;
using Bwadl.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bwadl.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfigurationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the configuration service
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        
        // Register strongly-typed configuration options
        services.Configure<ApplicationOptions>(configuration.GetSection(ApplicationOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.Configure<MessageBusOptions>(configuration.GetSection(MessageBusOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
        services.Configure<FeatureOptions>(configuration.GetSection(FeatureOptions.SectionName));
        
        return services;
    }

    public static T GetOptions<T>(this IConfiguration configuration, string sectionName) where T : new()
    {
        var options = new T();
        configuration.GetSection(sectionName).Bind(options);
        return options;
    }

    public static T GetRequiredOptions<T>(this IConfiguration configuration, string sectionName) where T : new()
    {
        var options = configuration.GetOptions<T>(sectionName);
        
        // Basic validation - check if any properties are set
        var properties = typeof(T).GetProperties();
        var hasAnyValue = properties.Any(p => 
        {
            var value = p.GetValue(options);
            return value switch
            {
                string str => !string.IsNullOrEmpty(str),
                null => false,
                _ => true
            };
        });
        
        if (!hasAnyValue)
        {
            throw new InvalidOperationException($"Required configuration section '{sectionName}' is missing or empty");
        }
        
        return options;
    }
}
