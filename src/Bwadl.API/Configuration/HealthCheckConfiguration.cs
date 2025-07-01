using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bwadl.API.Configuration;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
            .AddCheck("database", () => HealthCheckResult.Healthy("Database is available"));

        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30);
            options.AddHealthCheckEndpoint("Bwadl API", "/health");
        })
        .AddInMemoryStorage();

        return services;
    }

    public static IApplicationBuilder UseHealthCheckConfiguration(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
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
                        duration = x.Value.Duration.ToString()
                    }),
                    totalDuration = report.TotalDuration.ToString()
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
        });

        app.UseHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
        });

        return app;
    }
}
