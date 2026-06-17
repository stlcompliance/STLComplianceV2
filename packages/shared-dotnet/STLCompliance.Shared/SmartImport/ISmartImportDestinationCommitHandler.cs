using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace STLCompliance.Shared.SmartImport;

public interface ISmartImportDestinationCommitHandler
{
    string ProductKey { get; }

    Task<SmartImportDestinationCommitResponse> CommitAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default);
}

public static class SmartImportDestinationCommitResponses
{
    public static SmartImportDestinationCommitResponse Committed(
        string resultEntityId,
        string displayName,
        IReadOnlyList<SmartImportDestinationLink>? links = null) =>
        new(
            Status: SmartImportStatuses.Committed,
            ResultEntityId: resultEntityId,
            DisplayName: displayName,
            Links: links ?? [],
            Retryable: false,
            ErrorCode: null,
            ErrorMessage: null);

    public static SmartImportDestinationCommitResponse ReviewRequired(
        string errorCode,
        string errorMessage,
        bool retryable = false) =>
        new(
            Status: SmartImportStatuses.ReviewRequired,
            ResultEntityId: null,
            DisplayName: null,
            Links: [],
            Retryable: retryable,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage);

    public static bool IsCreateOperation(string operation) =>
        string.IsNullOrWhiteSpace(operation)
        || string.Equals(operation, "create", StringComparison.OrdinalIgnoreCase);
}

public static class SmartImportPayloadReader
{
    public static string? GetString(JsonElement payload, params string[] keys)
    {
        foreach (var key in keys)
        {
            foreach (var root in CandidateObjects(payload))
            {
                if (TryGetProperty(root, key, out var element) && TryConvertToString(element, out var value))
                {
                    return value;
                }
            }
        }

        return null;
    }

    public static Guid? GetGuid(JsonElement payload, params string[] keys)
    {
        var value = GetString(payload, keys);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }

    public static int GetInt(JsonElement payload, int defaultValue, params string[] keys)
    {
        var value = GetString(payload, keys);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : defaultValue;
    }

    public static decimal? GetDecimal(JsonElement payload, params string[] keys)
    {
        var value = GetString(payload, keys);
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    public static bool GetBool(JsonElement payload, bool defaultValue, params string[] keys)
    {
        var value = GetString(payload, keys);
        return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    public static DateTimeOffset? GetDateTimeOffset(JsonElement payload, params string[] keys)
    {
        var value = GetString(payload, keys);
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var parsed)
            ? parsed
            : null;
    }

    public static string DisplayName(JsonElement payload, string fallback) =>
        FirstNonEmpty(
            GetString(payload, "displayName", "name", "title", "label", "legalName"),
            GetString(payload, "assetTag", "assetId", "unitNumber", "vin"),
            SourceFileName(payload),
            fallback);

    public static string SourceFileName(JsonElement payload) =>
        GetString(payload, "fileName", "sourceFile", "sourceFilename", "originalFilename") ?? string.Empty;

    public static string? RecordArrRecordId(JsonElement payload, string? fallback = null) =>
        FirstNonEmptyOrNull(GetString(payload, "recordArrRecordId", "recordarrRecordId", "recordId"), fallback);

    public static string? RecordArrFileId(JsonElement payload) =>
        GetString(payload, "recordArrFileId", "recordarrFileId", "fileId");

    public static string ShortId(Guid id) => id.ToString("N")[..12];

    public static string SlugKey(string? value, string fallback, int maxLength = 64)
    {
        var source = string.IsNullOrWhiteSpace(value) ? fallback : value;
        var builder = new StringBuilder();
        var previousSeparator = false;
        foreach (var character in source.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousSeparator = false;
                continue;
            }

            if (!previousSeparator)
            {
                builder.Append('_');
                previousSeparator = true;
            }
        }

        var slug = builder.ToString().Trim('_');
        if (slug.Length == 0)
        {
            slug = fallback;
        }

        return Truncate(slug, maxLength);
    }

    public static string Truncate(string? value, int maxLength)
    {
        var normalized = (value ?? string.Empty).Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    public static string FirstNonEmpty(params string?[] values) =>
        FirstNonEmptyOrNull(values) ?? string.Empty;

    public static string Sha256Hex(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string? FirstNonEmptyOrNull(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static IEnumerable<JsonElement> CandidateObjects(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        if (TryGetProperty(payload, "proposedFields", out var proposedFields)
            && proposedFields.ValueKind == JsonValueKind.Object)
        {
            yield return proposedFields;
        }

        yield return payload;

        if (TryGetProperty(payload, "source", out var source)
            && source.ValueKind == JsonValueKind.Object)
        {
            yield return source;
        }
    }

    private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement value)
    {
        var normalizedPropertyName = NormalizePropertyName(propertyName);
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                || (normalizedPropertyName.Length > 0
                    && string.Equals(NormalizePropertyName(property.Name), normalizedPropertyName, StringComparison.OrdinalIgnoreCase)))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string NormalizePropertyName(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }

    private static bool TryConvertToString(JsonElement element, out string value)
    {
        value = string.Empty;
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                value = element.GetString()?.Trim() ?? string.Empty;
                return value.Length > 0;
            case JsonValueKind.Number:
                value = element.GetRawText();
                return value.Length > 0;
            case JsonValueKind.True:
                value = bool.TrueString;
                return true;
            case JsonValueKind.False:
                value = bool.FalseString;
                return true;
            default:
                return false;
        }
    }
}
