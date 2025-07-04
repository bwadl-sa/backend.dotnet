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
            options.SetEvaluationTimeInSeconds(15); // More frequent updates
            options.SetMinimumSecondsBetweenFailureNotifications(60);
            // Use only the health-api endpoint
            options.AddHealthCheckEndpoint("Bwadl API", "/health-api");
            options.SetHeaderText("Bwadl API Health Dashboard");
        })
        .AddInMemoryStorage(); // This will reset the storage

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
        
        // Health checks API endpoint for UI
        app.UseHealthChecks("/health-api", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        // Health Checks UI with explicit configuration
        app.UseHealthChecksUI(config =>
        {
            config.UIPath = "/health-ui";
            config.ApiPath = "/health-api";
            config.WebhookPath = "/health-webhooks";
            config.ResourcesPath = "/health-ui-resources";
            config.AsideMenuOpened = true;
            config.PageTitle = "Bwadl API Health Checks";
        });

        return app;
    }
}
