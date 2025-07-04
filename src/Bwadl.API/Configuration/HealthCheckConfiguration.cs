using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Bwadl.API.Configuration;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
            .AddCheck("database", () => HealthCheckResult.Healthy("Database is available"))
            .AddCheck("memory", () =>
            {
                var allocated = GC.GetTotalMemory(false);
                var data = new Dictionary<string, object>()
                {
                    { "allocated", allocated },
                    { "gen0", GC.CollectionCount(0) },
                    { "gen1", GC.CollectionCount(1) },
                    { "gen2", GC.CollectionCount(2) }
                };
                return HealthCheckResult.Healthy("Memory usage is normal", data);
            });

        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30);
            options.SetMinimumSecondsBetweenFailureNotifications(60);
            options.AddHealthCheckEndpoint("Bwadl API", "http://localhost:5232/health");
            options.AddHealthCheckEndpoint("Bwadl API Detailed", "http://localhost:5232/health/detailed");
        })
        .AddInMemoryStorage();

        return services;
    }

    public static IApplicationBuilder UseHealthCheckConfiguration(this IApplicationBuilder app)
    {        
        // Basic health check endpoint
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });
        
        // Detailed health check endpoint  
        app.UseHealthChecks("/health/detailed", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(x => new
                    {
                        name = x.Key,
                        status = x.Value.Status.ToString(),
                        description = x.Value.Description,
                        duration = x.Value.Duration.ToString(),
                        data = x.Value.Data
                    }),
                    totalDuration = report.TotalDuration.ToString()
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
        });
        
        // Health Checks UI with explicit configuration
        app.UseHealthChecksUI(config =>
        {
            config.UIPath = "/healthchecks-ui";
            config.ApiPath = "/healthchecks-api";
            config.AsideMenuOpened = true;
            config.PageTitle = "Bwadl API Health Checks";
        });

        return app;
    }
}
