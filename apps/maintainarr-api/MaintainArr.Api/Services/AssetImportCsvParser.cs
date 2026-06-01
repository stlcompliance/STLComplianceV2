using MaintainArr.Api.Contracts;

namespace MaintainArr.Api.Services;

public static class AssetImportCsvParser
{
    private static readonly string[] RequiredHeaders =
    [
        "assettag",
        "name",
    ];

    public static IReadOnlyList<AssetImportRowRequest> Parse(string csvText)
    {
        var lines = csvText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();

        if (lines.Count < 2)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "imports.csv.invalid",
                "CSV must include a header row and at least one asset row.",
                400);
        }

        var headers = SplitCsvLine(lines[0]).Select(h => h.Trim()).ToList();
        var normalizedHeaders = headers.Select(h => h.ToLowerInvariant()).ToList();
        foreach (var required in RequiredHeaders)
        {
            if (!normalizedHeaders.Contains(required))
            {
                throw new STLCompliance.Shared.Contracts.StlApiException(
                    "imports.csv.invalid",
                    $"CSV header must include {required}.",
                    400);
            }
        }

        var rows = new List<AssetImportRowRequest>();
        for (var lineIndex = 1; lineIndex < lines.Count; lineIndex++)
        {
            var values = SplitCsvLine(lines[lineIndex]);
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count; i++)
            {
                map[headers[i]] = i < values.Count ? values[i].Trim() : string.Empty;
            }

            var valueMap = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in map)
            {
                var normalizedKey = NormalizeHeaderToFieldKey(pair.Key);
                if (string.Equals(normalizedKey, "assetTag", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalizedKey, "name", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalizedKey, "description", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalizedKey, "lifecycleStatus", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                valueMap[normalizedKey] = string.IsNullOrWhiteSpace(pair.Value) ? null : pair.Value.Trim();
            }

            var legacyClass = map.GetValueOrDefault("assetClassKey") ?? map.GetValueOrDefault("assetClass") ?? string.Empty;
            var legacyType = map.GetValueOrDefault("assetTypeKey") ?? map.GetValueOrDefault("assetType") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(legacyClass))
            {
                valueMap["assetClass"] = legacyClass;
            }
            if (!string.IsNullOrWhiteSpace(legacyType))
            {
                valueMap["assetType"] = legacyType;
            }
            if (map.TryGetValue("siteRef", out var siteRefValue) && !string.IsNullOrWhiteSpace(siteRefValue))
            {
                valueMap["siteId"] = siteRefValue.Trim();
            }

            rows.Add(new AssetImportRowRequest
            {
                AssetTag = map.GetValueOrDefault("assetTag")?.Trim() ?? string.Empty,
                Name = map.GetValueOrDefault("name")?.Trim() ?? string.Empty,
                Description = map.GetValueOrDefault("description")?.Trim() ?? string.Empty,
                LifecycleStatus = string.IsNullOrWhiteSpace(map.GetValueOrDefault("lifecycleStatus"))
                    ? "in_service"
                    : map["lifecycleStatus"].Trim(),
                AssetClassKey = legacyClass,
                AssetTypeKey = legacyType,
                SiteRef = map.GetValueOrDefault("siteRef"),
                Values = valueMap,
            });
        }

        return rows;
    }

    private static string NormalizeHeaderToFieldKey(string header)
    {
        var normalized = header.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (normalized.Contains('_'))
        {
            var parts = normalized.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return parts[0].ToLowerInvariant() + string.Concat(parts.Skip(1).Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
        }

        return normalized;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        values.Add(current.ToString());
        return values;
    }
}
