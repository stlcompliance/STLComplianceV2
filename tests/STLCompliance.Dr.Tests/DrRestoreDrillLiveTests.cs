using System.Diagnostics;
using STLCompliance.Shared.Operations;

namespace STLCompliance.Dr.Tests;

/// <summary>
/// Optional live DR restore drill against docker-compose Postgres.
/// Skipped unless DR_LIVE=1 (or E2E_LIVE=1) and postgres container is reachable.
/// </summary>
[Trait("Category", "Dr")]
[Trait("Category", "Live")]
public sealed class DrRestoreDrillLiveTests
{
    [SkippableFact]
    public async Task NexArr_backup_restore_and_validation_succeeds_against_live_postgres()
    {
        Skip.IfNot(DrLiveProbe.LiveModeEnabled, "Set DR_LIVE=1 or E2E_LIVE=1 to run live DR restore drill.");

        var container = DrLiveProbe.ResolvePostgresContainer();
        Skip.IfNot(await DrLiveProbe.IsPostgresContainerAvailableAsync(container), $"Postgres container '{container}' is unavailable.");

        var backupDirectory = Path.Combine(Path.GetTempPath(), $"stl-dr-live-{Guid.NewGuid():N}");
        Directory.CreateDirectory(backupDirectory);

        var backupPath = Path.Combine(backupDirectory, $"{StlProductDatabaseCatalog.NexArr}.custom");
        var drillDatabase = $"{StlDrRestoreDrillSupport.GetDrillDatabaseName(StlProductDatabaseCatalog.NexArr)}_{Guid.NewGuid():N}";

        try
        {
            await RunDockerPgDumpAsync(container, StlProductDatabaseCatalog.NexArr, backupPath);
            Assert.True(File.Exists(backupPath));

            await DropDrillDatabaseAsync(container, drillDatabase);
            await RunDockerPsqlAsync(container, "postgres", $"CREATE DATABASE {drillDatabase};");
            await RunDockerPgRestoreAsync(container, drillDatabase, backupPath);

            var connectionString = StlDrRestoreDrillSupport.BuildAdminConnectionString(
                "localhost",
                5432,
                "stl",
                "stl_dev_password",
                drillDatabase);

            var validation = await StlDrRestoreDrillValidator.ValidateRestoredDatabaseAsync(connectionString);
            Assert.True(validation.IsValid, string.Join("; ", validation.Errors));
            Assert.True(validation.MigrationHistoryCount > 0);
            Assert.True(validation.PlatformMetadataTableExists);
        }
        finally
        {
            await DropDrillDatabaseAsync(container, drillDatabase);
            if (Directory.Exists(backupDirectory))
            {
                Directory.Delete(backupDirectory, recursive: true);
            }
        }
    }

    private static async Task DropDrillDatabaseAsync(string container, string drillDatabase)
    {
        var terminateSql =
            $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{drillDatabase}' AND pid <> pg_backend_pid();";
        await RunDockerPsqlAsync(container, "postgres", terminateSql);
        await RunDockerPsqlAsync(container, "postgres", $"DROP DATABASE IF EXISTS {drillDatabase};");
    }

    private static Task RunDockerPgDumpAsync(string container, string database, string outputPath)
    {
        var arguments = $"exec -e PGPASSWORD=stl_dev_password {container} pg_dump -Fc -h localhost -U stl -d {database}";
        return RunProcessCaptureToFileAsync("docker", arguments, outputPath);
    }

    private static async Task RunDockerPgRestoreAsync(string container, string targetDatabase, string backupPath)
    {
        var containerBackup = $"/tmp/{Path.GetFileName(backupPath)}";
        await RunProcessAsync("docker", $"cp \"{backupPath}\" {container}:{containerBackup}");
        await RunProcessAsync(
            "docker",
            $"exec -e PGPASSWORD=stl_dev_password {container} pg_restore --no-owner --no-privileges --dbname={targetDatabase} -h localhost -U stl {containerBackup}");
    }

    private static Task RunDockerPsqlAsync(string container, string database, string sql) =>
        RunProcessAsync(
            "docker",
            $"exec -e PGPASSWORD=stl_dev_password {container} psql -v ON_ERROR_STOP=1 -h localhost -U stl -d {database} -c \"{sql}\"");

    private static async Task RunProcessCaptureToFileAsync(string fileName, string arguments, string outputPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {fileName}.");

        await using var outputStream = File.Create(outputPath);
        await process.StandardOutput.BaseStream.CopyToAsync(outputStream);
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{fileName} {arguments} failed: {error}");
        }
    }

    private static async Task RunProcessAsync(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {fileName}.");

        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{fileName} {arguments} failed: {error}");
        }
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
            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"exec {container} pg_isready -U stl",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
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
