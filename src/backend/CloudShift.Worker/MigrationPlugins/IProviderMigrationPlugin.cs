using CloudShift.Domain.Enums;

namespace CloudShift.Worker.MigrationPlugins;

public interface IProviderMigrationPlugin
{
    bool CanHandle(ProviderType sourceProvider, ProviderType destinationProvider);

    Task<IReadOnlyList<MigrationFileItem>> BuildFilePlanAsync(
        MigrationRouteContext context,
        CancellationToken cancellationToken);

    Task TransferFileAsync(
        MigrationRouteContext context,
        MigrationFileItem file,
        CancellationToken cancellationToken);
}
