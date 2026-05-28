using Microsoft.Extensions.Options;
using TrainArr.Api.Contracts;
using TrainArr.Api.Options;

namespace TrainArr.Api.Services;

public sealed class IntegrationProbeService(
    IHttpClientFactory httpClientFactory,
    IOptions<StaffArrClientOptions> staffArrOptions,
    IOptions<ComplianceCoreClientOptions> complianceCoreOptions)
{
    public const string HttpClientName = "TrainArrIntegrationProbe";

    public async Task<IntegrationProbesResponse> ProbeAsync(CancellationToken cancellationToken = default)
    {
        var probedAt = DateTimeOffset.UtcNow;
        var staffArr = staffArrOptions.Value;
        var complianceCore = complianceCoreOptions.Value;

        var items = new List<IntegrationProbeItem>
        {
            await ProbeHealthAsync(
                "staffarr",
                "StaffArr",
                staffArr.BaseUrl,
                probedAt,
                cancellationToken),
            await ProbeHealthAsync(
                "compliancecore",
                "Compliance Core",
                complianceCore.BaseUrl,
                probedAt,
                cancellationToken),
        };

        return new IntegrationProbesResponse(probedAt, items);
    }

    private async Task<IntegrationProbeItem> ProbeHealthAsync(
        string integrationKey,
        string displayName,
        string baseUrl,
        DateTimeOffset probedAt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return new IntegrationProbeItem(
                integrationKey,
                displayName,
                "misconfigured",
                null,
                "Base URL is not configured.",
                probedAt);
        }

        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), "health"));
            using var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new IntegrationProbeItem(
                    integrationKey,
                    displayName,
                    "reachable",
                    (int)response.StatusCode,
                    null,
                    probedAt);
            }

            return new IntegrationProbeItem(
                integrationKey,
                displayName,
                "unreachable",
                (int)response.StatusCode,
                $"Health probe returned HTTP {(int)response.StatusCode}.",
                probedAt);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new IntegrationProbeItem(
                integrationKey,
                displayName,
                "unreachable",
                null,
                Truncate(ex.Message, 256),
                probedAt);
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
