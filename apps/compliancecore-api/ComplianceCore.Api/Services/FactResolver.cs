using System.Globalization;
using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public static class FactResolver
{
    public static bool TryReadStaticValue(
        string valueType,
        string configJson,
        out JsonElement value,
        out string? error)
    {
        value = default;
        error = null;

        try
        {
            using var document = JsonDocument.Parse(configJson);
            var root = document.RootElement.Clone();

            return valueType.ToLowerInvariant() switch
            {
                FactValueTypes.Boolean => TryReadBoolean(root, out value, out error),
                FactValueTypes.Number => TryReadNumber(root, out value, out error),
                FactValueTypes.Date => TryReadDate(root, out value, out error),
                _ => TryReadString(root, out value, out error),
            };
        }
        catch (JsonException)
        {
            error = "Config JSON is not valid.";
            return false;
        }
    }

    public static ResolvedFactValue? TryResolveFromSource(
        FactDefinition definition,
        FactSource source,
        IReadOnlyDictionary<string, string>? context)
    {
        if (string.Equals(source.SourceType, FactSourceTypes.StaticConfig, StringComparison.Ordinal))
        {
            if (!TryReadStaticValue(definition.ValueType, source.ConfigJson, out var value, out _))
            {
                return null;
            }

            return new ResolvedFactValue(
                definition.FactKey,
                definition.ValueType,
                value,
                source.SourceType,
                source.SourceKey,
                FromContext: false);
        }

        if (string.Equals(source.SourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal))
        {
            if (context is not null
                && context.TryGetValue(definition.FactKey, out var contextValue)
                && TryParseContextValue(definition.ValueType, contextValue, out var parsed))
            {
                return new ResolvedFactValue(
                    definition.FactKey,
                    definition.ValueType,
                    parsed,
                    source.SourceType,
                    source.SourceKey,
                    FromContext: true);
            }

            return null;
        }

        if (string.Equals(source.SourceType, FactSourceTypes.Calculated, StringComparison.Ordinal))
        {
            return TryResolveCalculatedValue(definition, source, context);
        }

        if (string.Equals(source.SourceType, FactSourceTypes.ReportGenerated, StringComparison.Ordinal))
        {
            if (context is not null
                && context.TryGetValue(definition.FactKey, out var contextValue)
                && TryParseContextValue(definition.ValueType, contextValue, out var parsed))
            {
                return new ResolvedFactValue(
                    definition.FactKey,
                    definition.ValueType,
                    parsed,
                    source.SourceType,
                    source.SourceKey,
                    FromContext: true);
            }

            return null;
        }

        return null;
    }

    public static bool CanSourceResolve(
        FactDefinition definition,
        FactSource source,
        IReadOnlyDictionary<string, string>? context)
    {
        if (string.Equals(source.SourceType, FactSourceTypes.StaticConfig, StringComparison.Ordinal))
        {
            return TryReadStaticValue(definition.ValueType, source.ConfigJson, out _, out _);
        }

        if (string.Equals(source.SourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal))
        {
            if (context is not null
                && context.ContainsKey(definition.FactKey)
                && TryParseContextValue(definition.ValueType, context[definition.FactKey], out _))
            {
                return true;
            }

            return false;
        }

        if (string.Equals(source.SourceType, FactSourceTypes.Calculated, StringComparison.Ordinal))
        {
            return CanResolveCalculatedValue(definition, source, context);
        }

        if (string.Equals(source.SourceType, FactSourceTypes.ReportGenerated, StringComparison.Ordinal))
        {
            if (context is not null
                && context.ContainsKey(definition.FactKey)
                && TryParseContextValue(definition.ValueType, context[definition.FactKey], out _))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    public static string DescribeValidationGap(FactDefinition definition, FactSource source)
    {
        if (string.Equals(source.SourceType, FactSourceTypes.StaticConfig, StringComparison.Ordinal))
        {
            if (!TryReadStaticValue(definition.ValueType, source.ConfigJson, out _, out var error))
            {
                return error ?? "Static config source is misconfigured.";
            }

            return string.Empty;
        }

        if (string.Equals(source.SourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal))
        {
            var product = string.IsNullOrWhiteSpace(source.ProductKey) ? "product" : source.ProductKey;
            return $"Product API source ({product}) requires caller context, a successful background sync cache, or sync configuration.";
        }

        if (string.Equals(source.SourceType, FactSourceTypes.ReportGenerated, StringComparison.Ordinal))
        {
            var product = string.IsNullOrWhiteSpace(source.ProductKey) ? "product" : source.ProductKey;
            return $"Generated report source ({product}) requires caller context, a successful background sync cache, or generated report sync configuration.";
        }

        if (string.Equals(source.SourceType, FactSourceTypes.Calculated, StringComparison.Ordinal))
        {
            return "Calculated fact source requires prerequisite fact keys and current fact values for the calculation.";
        }

        if (string.Equals(source.SourceType, FactSourceTypes.ProductMirror, StringComparison.Ordinal))
        {
            var product = string.IsNullOrWhiteSpace(source.ProductKey) ? "product" : source.ProductKey;
            return $"Product mirror source ({product}) has no ingested fact for this tenant and scope.";
        }

        return "No active resolver for this source type.";
    }

    private static ResolvedFactValue? TryResolveCalculatedValue(
        FactDefinition definition,
        FactSource source,
        IReadOnlyDictionary<string, string>? context)
    {
        if (!TryReadCalculatedConfig(source.ConfigJson, out var config, out _))
        {
            return null;
        }

        if (context is null)
        {
            return null;
        }

        if (!TryResolveCalculatedBoolean(config, context, out var calculatedValue))
        {
            return null;
        }

        return new ResolvedFactValue(
            definition.FactKey,
            definition.ValueType,
            JsonSerializer.SerializeToElement(calculatedValue),
            source.SourceType,
            source.SourceKey,
            FromContext: false);
    }

    private static bool CanResolveCalculatedValue(
        FactDefinition definition,
        FactSource source,
        IReadOnlyDictionary<string, string>? context)
    {
        if (!TryReadCalculatedConfig(source.ConfigJson, out var config, out _))
        {
            return false;
        }

        if (context is null)
        {
            return false;
        }

        return TryResolveCalculatedBoolean(config, context, out _);
    }

    private static bool TryReadCalculatedConfig(
        string configJson,
        out CalculatedFactSourceConfig config,
        out string? error)
    {
        config = new CalculatedFactSourceConfig(Array.Empty<string>(), "all_true");
        error = null;

        try
        {
            using var document = JsonDocument.Parse(configJson);
            var root = document.RootElement;

            var calculationMode = "all_true";
            if (root.TryGetProperty("calculationMode", out var modeElement)
                && modeElement.ValueKind == JsonValueKind.String)
            {
                var normalizedMode = modeElement.GetString()?.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(normalizedMode))
                {
                    if (normalizedMode is not "all_true"
                        and not "any_true"
                        and not "all_false"
                        and not "any_false")
                    {
                        error = "Calculated fact sources only support calculationMode values of all_true, any_true, all_false, or any_false.";
                        return false;
                    }

                    calculationMode = normalizedMode;
                }
            }

            if (!root.TryGetProperty("sourceFactKeys", out var keysElement)
                || keysElement.ValueKind != JsonValueKind.Array)
            {
                error = "Calculated fact sources require a sourceFactKeys array.";
                return false;
            }

            var keys = new List<string>();
            foreach (var item in keysElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var key = item.GetString()?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (!keys.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    keys.Add(key);
                }
            }

            if (keys.Count == 0)
            {
                error = "Calculated fact sources require at least one sourceFactKeys entry.";
                return false;
            }

            config = new CalculatedFactSourceConfig(keys, calculationMode);
            return true;
        }
        catch (JsonException)
        {
            error = "Calculated fact source config is invalid.";
            return false;
        }
    }

    private static bool TryResolveCalculatedBoolean(
        CalculatedFactSourceConfig config,
        IReadOnlyDictionary<string, string> context,
        out bool value)
    {
        value = false;
        var anyMissing = false;
        var anyFalse = false;
        var anyTrue = false;
        var allFalse = true;
        var allTrue = true;

        foreach (var factKey in config.SourceFactKeys)
        {
            if (!context.TryGetValue(factKey, out var raw))
            {
                anyMissing = true;
                continue;
            }

            if (!bool.TryParse(raw, out var parsed))
            {
                anyMissing = true;
                continue;
            }

            anyTrue |= parsed;
            anyFalse |= !parsed;
            allTrue &= parsed;
            allFalse &= !parsed;
        }

        if (anyMissing)
        {
            return false;
        }

        value = config.CalculationMode.ToLowerInvariant() switch
        {
            "any_true" => anyTrue,
            "all_false" => allFalse,
            "any_false" => anyFalse,
            _ => allTrue,
        };
        return true;
    }

    private sealed record CalculatedFactSourceConfig(
        IReadOnlyList<string> SourceFactKeys,
        string CalculationMode);

    private static bool TryReadBoolean(JsonElement root, out JsonElement value, out string? error)
    {
        if (root.TryGetProperty("booleanValue", out var element)
            && (element.ValueKind is JsonValueKind.True or JsonValueKind.False))
        {
            value = element.Clone();
            error = null;
            return true;
        }

        value = default;
        error = "Static config requires booleanValue for boolean facts.";
        return false;
    }

    private static bool TryReadNumber(JsonElement root, out JsonElement value, out string? error)
    {
        if (root.TryGetProperty("numberValue", out var element)
            && element.ValueKind is JsonValueKind.Number)
        {
            value = element.Clone();
            error = null;
            return true;
        }

        value = default;
        error = "Static config requires numberValue for number facts.";
        return false;
    }

    private static bool TryReadDate(JsonElement root, out JsonElement value, out string? error)
    {
        if (root.TryGetProperty("dateValue", out var element)
            && element.ValueKind is JsonValueKind.String
            && DateOnly.TryParse(element.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            value = element.Clone();
            error = null;
            return true;
        }

        value = default;
        error = "Static config requires dateValue (ISO date) for date facts.";
        return false;
    }

    private static bool TryReadString(JsonElement root, out JsonElement value, out string? error)
    {
        if (root.TryGetProperty("stringValue", out var element)
            && element.ValueKind is JsonValueKind.String)
        {
            value = element.Clone();
            error = null;
            return true;
        }

        value = default;
        error = "Static config requires stringValue for string facts.";
        return false;
    }

    private static bool TryParseContextValue(string valueType, string raw, out JsonElement value)
    {
        value = default;

        try
        {
            return valueType.ToLowerInvariant() switch
            {
                FactValueTypes.Boolean => bool.TryParse(raw, out var boolean)
                    && SetRaw(JsonSerializer.SerializeToElement(boolean), out value),
                FactValueTypes.Number => decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
                    && SetRaw(JsonSerializer.SerializeToElement(number), out value),
                FactValueTypes.Date => DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                    && SetRaw(JsonSerializer.SerializeToElement(date.ToString("O")), out value),
                _ => SetRaw(JsonSerializer.SerializeToElement(raw), out value),
            };
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool SetRaw(JsonElement element, out JsonElement value)
    {
        value = element;
        return true;
    }
}
