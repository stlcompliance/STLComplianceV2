namespace SupplyArr.Api.Services;

public static class ForgivingSearchNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var chars = value.Trim().ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray();
        return new string(chars);
    }

    public static IReadOnlyList<string> Tokenize(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return query
            .Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Normalize)
            .Where(token => token.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public static int ScoreMatch(string? haystack, string query)
    {
        var normalizedHaystack = Normalize(haystack);
        var normalizedQuery = Normalize(query);
        if (normalizedHaystack.Length == 0 || normalizedQuery.Length == 0)
        {
            return 0;
        }

        if (normalizedHaystack == normalizedQuery)
        {
            return 100;
        }

        if (normalizedHaystack.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 85;
        }

        var tokens = Tokenize(query);
        if (tokens.Count == 0)
        {
            return 0;
        }

        var matchedTokens = tokens.Count(token => normalizedHaystack.Contains(token, StringComparison.Ordinal));
        if (matchedTokens == tokens.Count)
        {
            return 65 + (5 * matchedTokens);
        }

        if (matchedTokens > 0)
        {
            return 35 + (5 * matchedTokens);
        }

        return 0;
    }

    public static string BuildHaystack(params string?[] values) =>
        Normalize(string.Join(' ', values.Where(value => !string.IsNullOrWhiteSpace(value))));
}
