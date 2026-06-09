using MaintainArr.Api.Contracts;
using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;

namespace MaintainArr.Api.Services.Recalls;

public sealed class RecallRegistry(
    IEnumerable<RecallProviderBase> providers,
    IOptions<RecallOptions> options)
{
    private readonly RecallOptions _options = options.Value;
    private readonly IReadOnlyDictionary<string, RecallProviderBase> _providers = providers
        .GroupBy(provider => provider.ProviderKey, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

    public RecallProviderBase? GetProvider(string providerKey) =>
        _providers.TryGetValue(providerKey, out var provider) ? provider : null;

    public IReadOnlyList<RecallProviderSummaryResponse> GetProviders()
    {
        var results = new List<RecallProviderSummaryResponse>();
        foreach (var provider in _providers.Values.OrderBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            results.Add(new RecallProviderSummaryResponse(
                provider.ProviderKey,
                provider.DisplayName,
                provider is NhtsaRecallProvider
                    ? "Automated vehicle recall lookup through NHTSA campaign data."
                    : "Manual recall entry and tenant-curated campaigns.",
                provider.SourceOfTruth,
                provider.Enabled ? "healthy" : "disabled",
                provider.ProviderKey is "nhtsa",
                provider.ProviderKey is "nhtsa",
                provider.ProviderKey is "manual",
                null,
                null,
                null));
        }

        results.AddRange(new[]
        {
            new RecallProviderSummaryResponse(
                "transport_canada",
                "Transport Canada",
                "Placeholder provider for Canadian recall lookups.",
                "Transport Canada",
                _options.Enabled && _options.TransportCanadaEnabled ? "planned" : "disabled",
                true,
                true,
                false,
                null,
                null,
                "Not yet implemented."),
            new RecallProviderSummaryResponse(
                "oem_recall_provider",
                "OEM recall provider",
                "Disabled-first slot for manufacturer recall feeds.",
                "OEM / manufacturer",
                "disabled",
                true,
                true,
                false,
                null,
                null,
                "Not yet implemented."),
            new RecallProviderSummaryResponse(
                "paid_open_recall_provider",
                "Paid recall provider",
                "Disabled-first slot for a commercial VIN verification provider.",
                "Paid provider",
                "disabled",
                true,
                true,
                false,
                null,
                null,
                "Not yet implemented."),
            new RecallProviderSummaryResponse(
                "tenant_uploaded_recall_report",
                "Tenant uploaded recall report",
                "Import slot for manufacturer and dealer recall reports.",
                "Tenant uploaded",
                "available",
                false,
                false,
                true,
                null,
                null,
                null),
            new RecallProviderSummaryResponse(
                "manual_verification",
                "Manual verification",
                "Human-reviewed verification evidence and notes.",
                "MaintainArr users",
                "available",
                false,
                false,
                true,
                null,
                null,
                null),
        });

        return results
            .OrderBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<IReadOnlyList<RecallProviderHealthResponse>> GetProviderHealthAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<RecallProviderHealthResponse>();
        foreach (var provider in _providers.Values.OrderBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            results.Add(await provider.HealthCheckAsync(cancellationToken));
        }

        results.AddRange(new[]
        {
            new RecallProviderHealthResponse(
                "transport_canada",
                _options.Enabled && _options.TransportCanadaEnabled ? "planned" : "disabled",
                "Transport Canada provider is not implemented yet.",
                DateTimeOffset.UtcNow,
                null),
            new RecallProviderHealthResponse(
                "oem_recall_provider",
                "disabled",
                "OEM provider is not implemented yet.",
                DateTimeOffset.UtcNow,
                null),
            new RecallProviderHealthResponse(
                "paid_open_recall_provider",
                "disabled",
                "Paid recall provider is not implemented yet.",
                DateTimeOffset.UtcNow,
                null),
            new RecallProviderHealthResponse(
                "tenant_uploaded_recall_report",
                "available",
                "Tenant upload slot is available.",
                DateTimeOffset.UtcNow,
                null),
            new RecallProviderHealthResponse(
                "manual_verification",
                "available",
                "Manual verification slot is available.",
                DateTimeOffset.UtcNow,
                null),
        });

        return results
            .OrderBy(provider => provider.ProviderKey, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
