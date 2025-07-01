using Bwadl.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bwadl.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        // Register MediatR behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Register AutoMapper
        services.AddAutoMapper(assembly);

        return services;
    }
}
