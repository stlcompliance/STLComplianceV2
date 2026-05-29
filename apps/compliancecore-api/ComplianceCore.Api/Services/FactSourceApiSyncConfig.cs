using System.Globalization;
using System.Text.Json;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed record FactSourceApiSyncConfig(
    string ScopeKey,
    string? FetchRelativePath,
    string? BooleanValue,
    string? StringValue,
    decimal? NumberValue,
    string? DateValue)
{
    public bool HasSnapshotValue =>
        BooleanValue is not null
        || !string.IsNullOrWhiteSpace(StringValue)
        || NumberValue is not null
        || !string.IsNullOrWhiteSpace(DateValue);

    public bool HasHttpFetch =>
        !string.IsNullOrWhiteSpace(FetchRelativePath);
}

public static class FactSourceApiSyncConfigParser
{
    public static FactSourceApiSyncConfig Parse(string configJson, string defaultScopeKey)
    {
        if (string.IsNullOrWhiteSpace(configJson) || configJson.Trim() == "{}")
        {
            return new FactSourceApiSyncConfig(
                FactSourceSyncRules.NormalizeScopeKey(defaultScopeKey),
                null,
                null,
                null,
                null,
                null);
        }

        try
        {
            using var document = JsonDocument.Parse(configJson);
            var root = document.RootElement;
            var scopeKey = root.TryGetProperty("scopeKey", out var scopeElement) && scopeElement.ValueKind == JsonValueKind.String
                ? FactSourceSyncRules.NormalizeScopeKey(scopeElement.GetString())
                : FactSourceSyncRules.NormalizeScopeKey(defaultScopeKey);

            var fetchPath = root.TryGetProperty("fetchRelativePath", out var pathElement) && pathElement.ValueKind == JsonValueKind.String
                ? pathElement.GetString()?.Trim()
                : null;

            bool? booleanValue = null;
            if (root.TryGetProperty("booleanValue", out var booleanElement)
                && booleanElement.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                booleanValue = booleanElement.GetBoolean();
            }

            string? stringValue = null;
            if (root.TryGetProperty("stringValue", out var stringElement) && stringElement.ValueKind == JsonValueKind.String)
            {
                stringValue = stringElement.GetString();
            }

            decimal? numberValue = null;
            if (root.TryGetProperty("numberValue", out var numberElement) && numberElement.ValueKind == JsonValueKind.Number)
            {
                numberValue = numberElement.GetDecimal();
            }

            string? dateValue = null;
            if (root.TryGetProperty("dateValue", out var dateElement) && dateElement.ValueKind == JsonValueKind.String)
            {
                dateValue = dateElement.GetString();
            }

            return new FactSourceApiSyncConfig(
                scopeKey,
                fetchPath,
                booleanValue?.ToString(),
                stringValue,
                numberValue,
                dateValue);
        }
        catch (JsonException ex)
        {
            throw new StlApiException(
                "fact_sources.validation",
                $"Product API config JSON is invalid: {ex.Message}",
                400);
        }
    }

    public static void ValidateForSourceType(string sourceType, string valueType, string configJson, string defaultScopeKey)
    {
        if (!string.Equals(sourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal))
        {
            return;
        }

        var config = Parse(configJson, defaultScopeKey);
        if (config.HasSnapshotValue)
        {
            ValidateSnapshotValue(valueType, config);
            return;
        }

        if (config.HasHttpFetch)
        {
            if (config.FetchRelativePath!.Length > 512)
            {
                throw new StlApiException(
                    "fact_sources.validation",
                    "fetchRelativePath must be 512 characters or fewer.",
                    400);
            }

            return;
        }

        throw new StlApiException(
            "fact_sources.validation",
            "Product API sources require fetchRelativePath or a snapshot value (booleanValue, stringValue, numberValue, dateValue) in configJson.",
            400);
    }

    private static void ValidateSnapshotValue(string valueType, FactSourceApiSyncConfig config)
    {
        switch (valueType.ToLowerInvariant())
        {
            case FactValueTypes.Boolean:
                if (config.BooleanValue is null || !bool.TryParse(config.BooleanValue, out _))
                {
                    throw new StlApiException(
                        "fact_sources.validation",
                        "Product API snapshot config requires booleanValue for boolean facts.",
                        400);
                }

                break;
            case FactValueTypes.Number:
                if (config.NumberValue is null)
                {
                    throw new StlApiException(
                        "fact_sources.validation",
                        "Product API snapshot config requires numberValue for number facts.",
                        400);
                }

                break;
            case FactValueTypes.Date:
                if (string.IsNullOrWhiteSpace(config.DateValue)
                    || !DateOnly.TryParse(config.DateValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    throw new StlApiException(
                        "fact_sources.validation",
                        "Product API snapshot config requires dateValue (ISO date) for date facts.",
                        400);
                }

                break;
            default:
                if (string.IsNullOrWhiteSpace(config.StringValue))
                {
                    throw new StlApiException(
                        "fact_sources.validation",
                        "Product API snapshot config requires stringValue for string facts.",
                        400);
                }

                break;
        }
    }
}
