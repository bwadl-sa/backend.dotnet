using Bwadl.Application.Common.Interfaces;
using Bwadl.Domain.Interfaces;
using Bwadl.Infrastructure.Caching;
using Bwadl.Infrastructure.Configuration;
using Bwadl.Infrastructure.Data.Repositories;
using Bwadl.Infrastructure.ExternalServices;
using Bwadl.Infrastructure.Extensions;
using Bwadl.Infrastructure.Messaging;
using Bwadl.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bwadl.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var logger = Log.ForContext(typeof(DependencyInjection));
        logger.Information("Registering Infrastructure services");

        // Configuration Services
        services.AddConfigurationServices(configuration);

        // Repositories
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        
        // External Services
        services.AddHttpClient<IEmailService, EnhancedEmailService>();
        
        // Messaging
        services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
        
        // Caching - register memory cache first, then our cache service
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, RedisService>();
        
        // Security
        services.AddSingleton<ISecretManager, SecretManager>();

        logger.Information("Infrastructure services registered successfully");
        return services;
    }
}
