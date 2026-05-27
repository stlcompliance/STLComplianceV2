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
                builder.Configuration.AddInMemoryCollection(
                    tokens.Select(static pair => new KeyValuePair<string, string?>(pair.Key, pair.Value)));
            }
        }
        finally
        {
            loggerFactory.Dispose();
        }
    }
}
