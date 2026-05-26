using CloudShift.Application.Common.Behaviors;
using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Application.ProjectMappings.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CloudShift.Application;

/// <summary>
/// Extension method to register all Application-layer services (MediatR handlers, etc.)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Scans this assembly and registers all IRequestHandler<,> implementations automatically
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
        });

        services.AddSingleton<IProviderRoutePolicy, DefaultProviderRoutePolicy>();

        return services;
    }
}
