using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed record ProductFactApiFetchResult(
    bool Succeeded,
    string? ErrorMessage,
    string? StringValue,
    bool? BooleanValue,
    decimal? NumberValue,
    string? DateValue);

public sealed class ProductFactApiFetcher(
    IHttpClientFactory httpClientFactory,
    IOptions<ProductApiIntegrationOptions> options)
{
    public ProductFactApiFetchResult FetchSnapshot(
        FactDefinition definition,
        FactSourceApiSyncConfig config)
    {
        if (!config.HasSnapshotValue)
        {
            return new ProductFactApiFetchResult(false, "Snapshot value is not configured.", null, null, null, null);
        }

        return definition.ValueType.ToLowerInvariant() switch
        {
            FactValueTypes.Boolean => config.BooleanValue is not null && bool.TryParse(config.BooleanValue, out var boolean)
                ? new ProductFactApiFetchResult(true, null, null, boolean, null, null)
                : new ProductFactApiFetchResult(false, "booleanValue is invalid.", null, null, null, null),
            FactValueTypes.Number => config.NumberValue is not null
                ? new ProductFactApiFetchResult(true, null, null, null, config.NumberValue, null)
                : new ProductFactApiFetchResult(false, "numberValue is invalid.", null, null, null, null),
            FactValueTypes.Date => !string.IsNullOrWhiteSpace(config.DateValue)
                && DateOnly.TryParse(config.DateValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                ? new ProductFactApiFetchResult(true, null, null, null, null, config.DateValue)
                : new ProductFactApiFetchResult(false, "dateValue is invalid.", null, null, null, null),
            _ => !string.IsNullOrWhiteSpace(config.StringValue)
                ? new ProductFactApiFetchResult(true, null, config.StringValue, null, null, null)
                : new ProductFactApiFetchResult(false, "stringValue is required.", null, null, null, null),
        };
    }

    public async Task<ProductFactApiFetchResult> FetchFromProductApiAsync(
        Guid tenantId,
        FactDefinition definition,
        FactSource source,
        FactSourceApiSyncConfig config,
        CancellationToken cancellationToken)
    {
        if (!config.HasHttpFetch)
        {
            return new ProductFactApiFetchResult(
                false,
                "fetchRelativePath is not configured for this source.",
                null,
                null,
                null,
                null);
        }

        var productKey = string.IsNullOrWhiteSpace(source.ProductKey)
            ? "unknown"
            : source.ProductKey.Trim().ToLowerInvariant();

        if (!options.Value.Products.TryGetValue(productKey, out var connection)
            || string.IsNullOrWhiteSpace(connection.BaseUrl))
        {
            return new ProductFactApiFetchResult(
                false,
                $"Product API base URL is not configured for '{productKey}'.",
                null,
                null,
                null,
                null);
        }

        if (string.IsNullOrWhiteSpace(connection.ServiceToken))
        {
            return new ProductFactApiFetchResult(
                false,
                $"Product API service token is not configured for '{productKey}'.",
                null,
                null,
                null,
                null);
        }

        var relativePath = config.FetchRelativePath!
            .Replace("{tenantId}", tenantId.ToString("D"), StringComparison.OrdinalIgnoreCase)
            .Replace("{factKey}", definition.FactKey, StringComparison.OrdinalIgnoreCase)
            .Replace("{scopeKey}", config.ScopeKey, StringComparison.OrdinalIgnoreCase);

        if (!relativePath.StartsWith('/'))
        {
            relativePath = "/" + relativePath;
        }

        var baseUrl = connection.BaseUrl.TrimEnd('/');
        var requestUri = $"{baseUrl}{relativePath}";

        try
        {
            using var client = httpClientFactory.CreateClient(nameof(ProductFactApiFetcher));
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connection.ServiceToken);

            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new ProductFactApiFetchResult(
                    false,
                    $"Product API returned {(int)response.StatusCode}: {TrimError(body)}",
                    null,
                    null,
                    null,
                    null);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ParseFetchedValue(definition.ValueType, document.RootElement);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new ProductFactApiFetchResult(false, TrimError(ex.Message), null, null, null, null);
        }
    }

    private static ProductFactApiFetchResult ParseFetchedValue(string valueType, JsonElement root)
    {
        if (root.TryGetProperty("value", out var wrapped))
        {
            root = wrapped;
        }

        return valueType.ToLowerInvariant() switch
        {
            FactValueTypes.Boolean => root.ValueKind is JsonValueKind.True or JsonValueKind.False
                ? new ProductFactApiFetchResult(true, null, null, root.GetBoolean(), null, null)
                : new ProductFactApiFetchResult(false, "Fetched value is not a boolean.", null, null, null, null),
            FactValueTypes.Number => root.ValueKind == JsonValueKind.Number
                ? new ProductFactApiFetchResult(true, null, null, null, root.GetDecimal(), null)
                : new ProductFactApiFetchResult(false, "Fetched value is not a number.", null, null, null, null),
            FactValueTypes.Date => root.ValueKind == JsonValueKind.String
                && DateOnly.TryParse(root.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? new ProductFactApiFetchResult(true, null, null, null, null, date.ToString("O"))
                : new ProductFactApiFetchResult(false, "Fetched value is not a date string.", null, null, null, null),
            _ => root.ValueKind == JsonValueKind.String
                ? new ProductFactApiFetchResult(true, null, root.GetString(), null, null, null)
                : new ProductFactApiFetchResult(false, "Fetched value is not a string.", null, null, null, null),
        };
    }

    private static string TrimError(string message) =>
        message.Length <= 512 ? message : message[..512];
}
