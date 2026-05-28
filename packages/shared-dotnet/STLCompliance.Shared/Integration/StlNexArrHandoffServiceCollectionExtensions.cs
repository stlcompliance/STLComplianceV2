using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.Shared.Http;

namespace STLCompliance.Shared.Integration;

public static class StlNexArrHandoffServiceCollectionExtensions
{
    public const string NexArrBaseUrlConfigurationKey = "NexArr:BaseUrl";

    public static IServiceCollection AddStlNexArrHandoffClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        void ConfigureNexArrClient(HttpClient client)
        {
            var baseUrl = configuration[NexArrBaseUrlConfigurationKey]
                ?? configuration["NexArr__BaseUrl"]
                ?? throw new InvalidOperationException("NexArr:BaseUrl is not configured.");
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(baseUrl).TrimEnd('/') + "/");
        }

        services.AddHttpClient<StlNexArrHandoffClient>((_, client) => ConfigureNexArrClient(client));
        services.AddHttpClient<StlNexArrLaunchClient>((_, client) => ConfigureNexArrClient(client));

        return services;
    }
}
