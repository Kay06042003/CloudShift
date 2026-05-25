using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Application.Common.Interfaces;
using CloudShift.Application.OAuthProviderApps.Interfaces;
using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Infrastructure.Auth;
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

        services.AddDataProtection();
        services.AddHttpClient<ICloudOAuthService, CloudOAuthService>();

        // Repository registrations
        services.AddScoped<IAppProfileRepository, AppProfileRepository>();
        services.AddScoped<IOAuthProviderAppRepository, OAuthProviderAppRepository>();
        services.AddScoped<IProjectMappingRepository, ProjectMappingRepository>();
        services.AddScoped<IMigrationJobRepository, MigrationJobRepository>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        services.AddScoped<ITokenProtector, DataProtectionTokenProtector>();
        services.AddScoped<IOAuthStateProtector, DataProtectionOAuthStateProtector>();

        return services;
    }
}
