using MaintainArr.Api.Contracts;

namespace MaintainArr.Api.Services;

public static class AssetImportCsvParser
{
    private static readonly string[] RequiredHeaders =
    [
        "assetclasskey",
        "assettypekey",
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

        var headers = lines[0].Split(',').Select(h => h.Trim().ToLowerInvariant()).ToList();
        foreach (var required in RequiredHeaders)
        {
            if (!headers.Contains(required))
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

            rows.Add(new AssetImportRowRequest(
                map["assetclasskey"],
                map["assettypekey"],
                map["assettag"],
                map["name"],
                map.GetValueOrDefault("description") ?? string.Empty,
                string.IsNullOrWhiteSpace(map.GetValueOrDefault("siteref")) ? null : map["siteref"],
                string.IsNullOrWhiteSpace(map.GetValueOrDefault("lifecyclestatus"))
                    ? "active"
                    : map["lifecyclestatus"]!));
        }

        return rows;
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
