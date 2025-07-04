using Microsoft.OpenApi.Models;

namespace Bwadl.API.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Bwadl API V1",
                Version = "v1",
                Description = "Clean Architecture API - Version 1 (Basic CRUD Operations)",
                Contact = new OpenApiContact
                {
                    Name = "Development Team",
                    Email = "dev@bwadl.com"
                }
            });

            c.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "Bwadl API V2",
                Version = "v2",
                Description = "Clean Architecture API - Version 2 (Enhanced with Pagination and Metadata)",
                Contact = new OpenApiContact
                {
                    Name = "Development Team",
                    Email = "dev@bwadl.com"
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bwadl API V1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "Bwadl API V2");
            c.RoutePrefix = "swagger"; // Change from empty to swagger
            c.DocumentTitle = "Bwadl API Documentation";
        });

        return app;
    }
}
