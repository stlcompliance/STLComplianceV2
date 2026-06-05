using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace NexArr.Api.Services;

public static class FieldCompanionScanPayloadParser
{
    private static readonly (string ProductKey, string ResourceType, string PathPrefix)[] DeepLinkPathPatterns =
    [
        ("trainarr", "assignment", "/assignments/"),
        ("maintainarr", "work-order", "/work-orders/"),
        ("maintainarr", "inspection", "/inspections/"),
        ("routarr", "trip", "/trips/"),
        ("supplyarr", "receiving", "/receiving/"),
        ("staffarr", "incident", "/incidents/"),
    ];

    public static bool TryExtractTaskKey(string scannedValue, out string taskKey, out string? errorMessage)
    {
        taskKey = string.Empty;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(scannedValue))
        {
            errorMessage = "Scan payload is empty.";
            return false;
        }

        var trimmed = scannedValue.Trim();

        if (FieldCompanionFieldTaskKeyParser.TryParse(trimmed, out _))
        {
            taskKey = trimmed;
            return true;
        }

        if (trimmed.StartsWith("stl-field-task:", StringComparison.OrdinalIgnoreCase))
        {
            var candidate = trimmed["stl-field-task:".Length..].Trim();
            if (FieldCompanionFieldTaskKeyParser.TryParse(candidate, out _))
            {
                taskKey = candidate;
                return true;
            }
        }

        if (TryParseJsonTaskKey(trimmed, out taskKey))
        {
            return true;
        }

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri)
            && TryParseUriTaskKey(absoluteUri, out taskKey))
        {
            return true;
        }

        if (trimmed.StartsWith("/", StringComparison.Ordinal)
            && TryParseRelativePath(trimmed, out taskKey))
        {
            return true;
        }

        errorMessage = "Scan payload is not a recognized field task reference.";
        return false;
    }

    private static bool TryParseJsonTaskKey(string trimmed, out string taskKey)
    {
        taskKey = string.Empty;
        if (!trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            if (!document.RootElement.TryGetProperty("taskKey", out var property))
            {
                return false;
            }

            var candidate = property.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(candidate) || !FieldCompanionFieldTaskKeyParser.TryParse(candidate, out _))
            {
                return false;
            }

            taskKey = candidate;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryParseUriTaskKey(Uri uri, out string taskKey)
    {
        taskKey = string.Empty;

        var query = QueryHelpers.ParseQuery(uri.Query);
        query.TryGetValue("taskKey", out var queryValues);
        var fromQuery = queryValues.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(fromQuery) && FieldCompanionFieldTaskKeyParser.TryParse(fromQuery, out _))
        {
            taskKey = fromQuery;
            return true;
        }

        return TryParseRelativePath(uri.AbsolutePath, out taskKey);
    }

    private static bool TryParseRelativePath(string path, out string taskKey)
    {
        taskKey = string.Empty;
        var normalized = path.Split('?', 2)[0].Trim();
        if (!normalized.StartsWith("/", StringComparison.Ordinal))
        {
            normalized = $"/{normalized}";
        }

        foreach (var pattern in DeepLinkPathPatterns)
        {
            if (!normalized.StartsWith(pattern.PathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var remainder = normalized[pattern.PathPrefix.Length..].Trim('/');
            var idSegment = remainder.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            if (string.IsNullOrWhiteSpace(idSegment) || !Guid.TryParse(idSegment, out var resourceId))
            {
                continue;
            }

            taskKey = $"{pattern.ProductKey}:{pattern.ResourceType}:{resourceId:D}";
            return true;
        }

        return false;
    }
}
