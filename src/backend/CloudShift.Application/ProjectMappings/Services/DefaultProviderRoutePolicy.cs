using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Domain.Enums;

namespace CloudShift.Application.ProjectMappings.Services;

public sealed class DefaultProviderRoutePolicy : IProviderRoutePolicy
{
    public bool CanMigrate(ProviderType sourceProvider, ProviderType destinationProvider)
    {
        return sourceProvider == ProviderType.GoogleDrive
            && destinationProvider == ProviderType.OneDrive;
    }
}
