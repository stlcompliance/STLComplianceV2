using Npgsql;

namespace STLCompliance.Shared.Operations;

public static class StlRenderStagingDrillSupport
{
    public const string DefaultBackupDirectoryEnvironmentVariable = "RENDER_STAGING_SNAPSHOT_DIRECTORY";
    public const string LiveModeEnvironmentVariable = "DR_RENDER_STAGING_LIVE";

    public static bool LiveModeEnabled =>
        string.Equals(Environment.GetEnvironmentVariable(LiveModeEnvironmentVariable), "1", StringComparison.Ordinal)
        || string.Equals(Environment.GetEnvironmentVariable(LiveModeEnvironmentVariable), "true", StringComparison.OrdinalIgnoreCase);

    public static StlRenderStagingDatabaseTarget ParseDatabaseUrl(string productDatabase, string databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            throw new ArgumentException("Database URL is required.", nameof(databaseUrl));
        }

        if (!StlProductDatabaseCatalog.IsKnownProductDatabase(productDatabase))
        {
            throw new ArgumentException($"Unknown product database '{productDatabase}'.", nameof(productDatabase));
        }

        var builder = ParseConnectionStringBuilder(databaseUrl);
        if (string.IsNullOrWhiteSpace(builder.Host))
        {
            throw new InvalidOperationException($"Database URL for '{productDatabase}' is missing Host.");
        }

        if (string.IsNullOrWhiteSpace(builder.Username))
        {
            throw new InvalidOperationException($"Database URL for '{productDatabase}' is missing Username.");
        }

        if (string.IsNullOrWhiteSpace(builder.Password))
        {
            throw new InvalidOperationException($"Database URL for '{productDatabase}' is missing Password.");
        }

        if (string.IsNullOrWhiteSpace(builder.Database))
        {
            throw new InvalidOperationException($"Database URL for '{productDatabase}' is missing Database.");
        }

        return new StlRenderStagingDatabaseTarget(
            productDatabase,
            builder.Host,
            builder.Port,
            builder.Username,
            builder.Password!,
            builder.Database);
    }

    public static IReadOnlyList<StlRenderStagingDatabaseTarget> ResolveTargetsFromEnvironment(
        IReadOnlyList<string>? productDatabases = null)
    {
        var selected = productDatabases is { Count: > 0 }
            ? productDatabases
            : StlProductDatabaseCatalog.All;

        var targets = new List<StlRenderStagingDatabaseTarget>();
        var missing = new List<string>();

        foreach (var database in selected)
        {
            var entry = StlRenderStagingDrillCatalog.TryGetEntry(database)
                ?? throw new InvalidOperationException($"No Render staging catalog entry for '{database}'.");

            var databaseUrl = Environment.GetEnvironmentVariable(entry.SourceDatabaseUrlEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(databaseUrl))
            {
                missing.Add(entry.SourceDatabaseUrlEnvironmentVariable);
                continue;
            }

            targets.Add(ParseDatabaseUrl(entry.ProductDatabase, databaseUrl));
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing staging database URL environment variables: {string.Join(", ", missing)}");
        }

        return targets;
    }

    public static StlRenderStagingDrillPlan ResolvePlanFromEnvironment(
        string? backupDirectory = null,
        IReadOnlyList<string>? productDatabases = null,
        bool skipSnapshotFetch = false)
    {
        var directory = backupDirectory
            ?? Environment.GetEnvironmentVariable(DefaultBackupDirectoryEnvironmentVariable)
            ?? Path.Combine(Path.GetTempPath(), $"stl-render-staging-{DateTime.UtcNow:yyyyMMdd-HHmmss}");

        var targets = ResolveTargetsFromEnvironment(productDatabases);
        return StlRenderStagingDrillPlan.FromTargets(directory, targets, skipSnapshotFetch: skipSnapshotFetch);
    }

    public static IReadOnlyList<string> ValidateBackupDirectory(StlRenderStagingDrillPlan plan)
    {
        if (string.IsNullOrWhiteSpace(plan.BackupDirectory))
        {
            throw new InvalidOperationException("BackupDirectory is required.");
        }

        if (!Directory.Exists(plan.BackupDirectory))
        {
            throw new DirectoryNotFoundException($"Backup directory not found: {plan.BackupDirectory}");
        }

        var missing = new List<string>();
        foreach (var target in plan.Targets)
        {
            try
            {
                _ = StlDrRestoreDrillSupport.ResolveBackupPath(plan.BackupDirectory, target.ProductDatabase);
            }
            catch (FileNotFoundException)
            {
                missing.Add(target.ProductDatabase);
            }
        }

        return missing;
    }

    private static NpgsqlConnectionStringBuilder ParseConnectionStringBuilder(string databaseUrl)
    {
        if (databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
                Database = uri.AbsolutePath.TrimStart('/'),
            };

            if (uri.Host.Contains("render.com", StringComparison.OrdinalIgnoreCase))
            {
                builder.SslMode = SslMode.Require;
            }

            return builder;
        }

        return new NpgsqlConnectionStringBuilder(databaseUrl);
    }
}
