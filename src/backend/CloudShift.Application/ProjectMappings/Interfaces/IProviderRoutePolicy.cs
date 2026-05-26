using CloudShift.Domain.Enums;

namespace CloudShift.Application.ProjectMappings.Interfaces;

public interface IProviderRoutePolicy
{
    bool CanMigrate(ProviderType sourceProvider, ProviderType destinationProvider);
}
