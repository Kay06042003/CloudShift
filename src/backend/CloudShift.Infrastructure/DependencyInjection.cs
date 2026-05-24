using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Application.Common.Interfaces;
using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Infrastructure.Messaging;
using CloudShift.Infrastructure.Data;
using CloudShift.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudShift.Infrastructure;

/// <summary>
/// Extension method to register all Infrastructure-layer services (EF Core, Repositories, etc.)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repository registrations
        services.AddScoped<IAppProfileRepository, AppProfileRepository>();
        services.AddScoped<IProjectMappingRepository, ProjectMappingRepository>();
        services.AddScoped<IMigrationJobRepository, MigrationJobRepository>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }
}

