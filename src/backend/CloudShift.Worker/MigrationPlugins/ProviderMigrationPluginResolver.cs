using CloudShift.Domain.Enums;

namespace CloudShift.Worker.MigrationPlugins;

public sealed class ProviderMigrationPluginResolver
{
    private readonly IEnumerable<IProviderMigrationPlugin> _plugins;

    public ProviderMigrationPluginResolver(IEnumerable<IProviderMigrationPlugin> plugins)
    {
        _plugins = plugins;
    }

    public IProviderMigrationPlugin Resolve(ProviderType sourceProvider, ProviderType destinationProvider)
    {
        return _plugins.FirstOrDefault(plugin => plugin.CanHandle(sourceProvider, destinationProvider))
            ?? throw new NotSupportedException(
                $"No migration plugin is registered for '{sourceProvider}' to '{destinationProvider}'.");
    }
}
