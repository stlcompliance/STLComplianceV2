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
            return $"Product API source ({product}) requires caller context or a future product fetch extension.";
        }

        if (string.Equals(source.SourceType, FactSourceTypes.ProductMirror, StringComparison.Ordinal))
        {
            var product = string.IsNullOrWhiteSpace(source.ProductKey) ? "product" : source.ProductKey;
            return $"Product mirror source ({product}) has no ingested fact for this tenant and scope.";
        }

        return "No active resolver for this source type.";
    }

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
