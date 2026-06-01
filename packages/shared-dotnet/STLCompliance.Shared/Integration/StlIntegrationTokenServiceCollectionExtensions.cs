using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace STLCompliance.Shared.Integration;

public static class StlIntegrationTokenServiceCollectionExtensions
{
    public static void AddStlIntegrationTokenProvisioning(this IHostApplicationBuilder builder)
    {
        if (!StlIntegrationTokenProvisioner.IsAutoProvisionEnabled(builder.Configuration))
        {
            return;
        }

        var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        var logger = loggerFactory.CreateLogger("StlIntegrationTokenProvisioning");

        try
        {
            var tokens = StlIntegrationTokenProvisioner.ProvisionSynchronously(builder.Configuration, logger);
            if (tokens.Count > 0)
            {
                var provisionedConfiguration = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                foreach (var (key, value) in tokens)
                {
                    foreach (var lookupKey in StlIntegrationTokenProvisioner.ExpandConfigurationKeys(key))
                    {
                        provisionedConfiguration[lookupKey] = value;
                    }
                }

                builder.Configuration.AddInMemoryCollection(provisionedConfiguration);
            }
        }
        finally
        {
            loggerFactory.Dispose();
        }
    }
}
