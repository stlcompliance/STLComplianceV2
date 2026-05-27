using System.Globalization;
using System.Text;

namespace ComplianceCore.Api.Csv;

public static class CsvText
{
    public static string WriteRow(params string?[] fields)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < fields.Length; index++)
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            builder.Append(Escape(fields[index] ?? string.Empty));
        }

        return builder.ToString();
    }

    public static IReadOnlyList<string> ParseRow(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (inQuotes)
            {
                if (character == '"')
                {
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(character);
                }

                continue;
            }

            if (character == '"')
            {
                inQuotes = true;
                continue;
            }

            if (character == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        fields.Add(current.ToString());
        return fields;
    }

    public static IReadOnlyList<IReadOnlyDictionary<string, string>> ParseTable(
        string csv,
        string fileName,
        IReadOnlyList<string> expectedHeaders)
    {
        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length == 0)
        {
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        if (headerFields.Count != expectedHeaders.Count ||
            !headerFields.SequenceEqual(expectedHeaders, StringComparer.OrdinalIgnoreCase))
        {
            throw new CsvParseException(
                fileName,
                1,
                $"Header must be: {string.Join(",", expectedHeaders)}");
        }

        var rows = new List<IReadOnlyDictionary<string, string>>();
        for (var lineNumber = 1; lineNumber < lines.Length; lineNumber++)
        {
            var fields = ParseRow(lines[lineNumber]);
            if (fields.Count == 1 && string.IsNullOrWhiteSpace(fields[0]))
            {
                continue;
            }

            if (fields.Count != expectedHeaders.Count)
            {
                throw new CsvParseException(
                    fileName,
                    lineNumber + 1,
                    $"Expected {expectedHeaders.Count} columns but found {fields.Count}.");
            }

            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < expectedHeaders.Count; index++)
            {
                row[expectedHeaders[index]] = fields[index].Trim();
            }

            rows.Add(row);
        }

        return rows;
    }

    public static string BuildTable(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string?>> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine(WriteRow(headers.Cast<string?>().ToArray()));
        foreach (var row in rows)
        {
            builder.AppendLine(WriteRow(row.ToArray()));
        }

        return builder.ToString();
    }

    public static bool ParseBool(string value, string fileName, int lineNumber, string column)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        throw new CsvParseException(fileName, lineNumber, $"Column '{column}' must be true or false.");
    }

    public static int ParseInt(string value, string fileName, int lineNumber, string column)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new CsvParseException(fileName, lineNumber, $"Column '{column}' must be an integer.");
        }

        return parsed;
    }

    public static DateOnly? ParseDateOnly(string value, string fileName, int lineNumber, string column)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        throw new CsvParseException(fileName, lineNumber, $"Column '{column}' must be a date (yyyy-MM-dd).");
    }

    private static string Escape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}

public sealed class CsvParseException(string fileName, int lineNumber, string message) : Exception(message)
{
    public string FileName { get; } = fileName;

    public int LineNumber { get; } = lineNumber;
}
