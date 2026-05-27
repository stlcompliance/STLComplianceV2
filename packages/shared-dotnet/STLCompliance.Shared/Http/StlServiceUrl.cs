namespace STLCompliance.Shared.Http;

/// <summary>
/// Normalizes service base URLs for Render private-network host:port values and full http(s) URLs.
/// </summary>
public static class StlServiceUrl
{
    public static string NormalizeHttpBaseUrl(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var trimmed = raw.Trim().TrimEnd('/');
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return $"http://{trimmed}";
    }
}
