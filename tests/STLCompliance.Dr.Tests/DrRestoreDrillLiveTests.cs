using STLCompliance.Shared.Operations;

namespace STLCompliance.Dr.Tests;

/// <summary>
/// Optional live DR restore drill against docker-compose Postgres for all seven product databases.
/// Skipped unless DR_LIVE=1 (or E2E_LIVE=1) and postgres container is reachable.
/// </summary>
[Trait("Category", "Dr")]
[Trait("Category", "Live")]
public sealed class DrRestoreDrillLiveTests
{
    public static IEnumerable<object[]> AllProductDatabases =>
        StlProductDatabaseCatalog.All.Select(database => new object[] { database });

    [SkippableTheory]
    [MemberData(nameof(AllProductDatabases))]
    public async Task Product_database_backup_restore_and_validation_succeeds(string productDatabase)
    {
        Skip.IfNot(DrLiveProbe.LiveModeEnabled, "Set DR_LIVE=1 or E2E_LIVE=1 to run live DR restore drill.");

        var container = DrLiveProbe.ResolvePostgresContainer();
        Skip.IfNot(await DrLiveProbe.IsPostgresContainerAvailableAsync(container), $"Postgres container '{container}' is unavailable.");

        var validation = await DrRestoreDrillLiveRunner.RunProductDatabaseDrillAsync(productDatabase, container);

        Assert.True(validation.IsValid, $"[{productDatabase}] {string.Join("; ", validation.Errors)}");
        Assert.True(validation.MigrationHistoryCount > 0);
        Assert.True(validation.PlatformMetadataTableExists);
    }
}

internal static class DrLiveProbe
{
    private const string DefaultPostgresContainer = "stlcompliancev2-postgres-1";

    public static bool LiveModeEnabled =>
        string.Equals(Environment.GetEnvironmentVariable("DR_LIVE"), "1", StringComparison.Ordinal)
        || string.Equals(Environment.GetEnvironmentVariable("DR_LIVE"), "true", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable("E2E_LIVE"), "1", StringComparison.Ordinal)
        || string.Equals(Environment.GetEnvironmentVariable("E2E_LIVE"), "true", StringComparison.OrdinalIgnoreCase);

    public static string ResolvePostgresContainer() =>
        Environment.GetEnvironmentVariable("DR_POSTGRES_CONTAINER") ?? DefaultPostgresContainer;

    public static async Task<bool> IsPostgresContainerAvailableAsync(string container)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"exec {container} pg_isready -U stl",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process is null)
            {
                return false;
            }

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
