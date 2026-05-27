using System.Security.Cryptography;
using System.Text;
using STLCompliance.Shared.Integration;

namespace NexArr.Api.Services;

public static class IntegrationBootstrapSecretValidator
{
    public static bool IsValid(IConfiguration configuration, string? providedSecret)
    {
        var configuredSecret = configuration[StlIntegrationTokenProvisioner.BootstrapSecretConfigurationKey];
        if (string.IsNullOrWhiteSpace(configuredSecret) || string.IsNullOrWhiteSpace(providedSecret))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(configuredSecret);
        var providedBytes = Encoding.UTF8.GetBytes(providedSecret);
        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
