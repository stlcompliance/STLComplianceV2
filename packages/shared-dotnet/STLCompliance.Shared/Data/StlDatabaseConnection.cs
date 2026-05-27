using Microsoft.Extensions.Configuration;
using Npgsql;

namespace STLCompliance.Shared.Data;

/// <summary>
/// Resolves PostgreSQL connection strings from Render/Heroku-style URIs and local ADO.NET settings.
/// </summary>
public static class StlDatabaseConnection
{
    public static string? Resolve(IConfiguration configuration) =>
        Normalize(configuration.GetConnectionString("Database") ?? configuration["DATABASE_URL"]);

    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        if (IsPostgresUri(trimmed))
        {
            return ConvertPostgresUri(trimmed);
        }

        return trimmed;
    }

    private static bool IsPostgresUri(string value) =>
        value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

    private static string ConvertPostgresUri(string uriValue)
    {
        var normalized = uriValue;
        if (normalized.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            normalized = string.Concat("postgresql://", normalized.AsSpan("postgres://".Length));
        }

        var databaseUri = new Uri(normalized);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
            Database = databaseUri.AbsolutePath.TrimStart('/'),
        };

        if (!string.IsNullOrEmpty(databaseUri.UserInfo))
        {
            var parts = databaseUri.UserInfo.Split(':', 2);
            builder.Username = Uri.UnescapeDataString(parts[0]);
            if (parts.Length > 1)
            {
                builder.Password = Uri.UnescapeDataString(parts[1]);
            }
        }

        if (!string.IsNullOrEmpty(databaseUri.Query))
        {
            var query = databaseUri.Query.TrimStart('?');
            foreach (var segment in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = segment.Split('=', 2);
                if (pair.Length == 2)
                {
                    builder[pair[0]] = Uri.UnescapeDataString(pair[1]);
                }
            }
        }

        return builder.ConnectionString;
    }
}
