using Bwadl.Domain.Interfaces;
using Bwadl.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Bwadl.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register repositories
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        return services;
    }
}
