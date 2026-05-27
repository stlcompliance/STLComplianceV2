using STLCompliance.Shared.Operations;

namespace STLCompliance.Dr.Tests;

/// <summary>
/// Optional live Render staging DR drill (pg_dump + restore + validation) per product database.
/// Skipped unless DR_RENDER_STAGING_LIVE=1 and all RENDER_STAGING_*_DATABASE_URL values are set.
/// </summary>
[Trait("Category", "Dr")]
[Trait("Category", "Live")]
public sealed class RenderStagingDrillLiveTests
{
    public static IEnumerable<object[]> AllProductDatabases =>
        StlProductDatabaseCatalog.All.Select(database => new object[] { database });

    [SkippableTheory]
    [MemberData(nameof(AllProductDatabases))]
    public async Task Product_database_staging_snapshot_drill_succeeds(string productDatabase)
    {
        Skip.IfNot(RenderStagingLiveProbe.LiveModeEnabled, "Set DR_RENDER_STAGING_LIVE=1 to run Render staging DR drill.");

        Skip.IfNot(
            RenderStagingLiveProbe.HasDatabaseUrl(productDatabase),
            $"Missing staging database URL for '{productDatabase}'.");

        Skip.IfNot(RenderStagingLiveProbe.PgToolsAvailable(), "pg_dump/pg_restore/psql are not available on PATH.");

        var validation = await RenderStagingDrillLiveRunner.RunProductDatabaseDrillAsync(productDatabase);

        Assert.True(validation.IsValid, $"[{productDatabase}] {string.Join("; ", validation.Errors)}");
        Assert.True(validation.MigrationHistoryCount > 0);
        Assert.True(validation.PlatformMetadataTableExists);
    }
}

internal static class RenderStagingLiveProbe
{
    public static bool LiveModeEnabled => StlRenderStagingDrillSupport.LiveModeEnabled;

    public static bool HasDatabaseUrl(string productDatabase)
    {
        var entry = StlRenderStagingDrillCatalog.TryGetEntry(productDatabase);
        if (entry is null)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(entry.SourceDatabaseUrlEnvironmentVariable));
    }

    public static bool PgToolsAvailable()
    {
        return CommandAvailable("pg_dump") && CommandAvailable("pg_restore") && CommandAvailable("psql");
    }

    private static bool CommandAvailable(string command)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(directory, command);
            if (File.Exists(candidate))
            {
                return true;
            }

            if (OperatingSystem.IsWindows() && File.Exists(candidate + ".exe"))
            {
                return true;
            }
        }

        return false;
    }
}
