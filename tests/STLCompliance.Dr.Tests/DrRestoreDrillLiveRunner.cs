using System.Diagnostics;
using STLCompliance.Shared.Operations;

namespace STLCompliance.Dr.Tests;

/// <summary>
/// Live postgres backup → drill database restore → validation for one product database.
/// </summary>
internal static class DrRestoreDrillLiveRunner
{
    public static async Task<StlDrRestoreValidationResult> RunProductDatabaseDrillAsync(
        string productDatabase,
        string postgresContainer,
        string host = "localhost",
        int port = 5432,
        string username = "stl",
        string password = "stl_dev_password")
    {
        if (!StlProductDatabaseCatalog.IsKnownProductDatabase(productDatabase))
        {
            throw new ArgumentException($"Unknown product database '{productDatabase}'.", nameof(productDatabase));
        }

        var backupDirectory = Path.Combine(Path.GetTempPath(), $"stl-dr-live-{Guid.NewGuid():N}");
        Directory.CreateDirectory(backupDirectory);

        var backupPath = Path.Combine(backupDirectory, $"{productDatabase}.custom");
        var drillDatabase = $"{StlDrRestoreDrillSupport.GetDrillDatabaseName(productDatabase)}_{Guid.NewGuid():N}";

        try
        {
            await RunDockerPgDumpAsync(postgresContainer, productDatabase, backupPath);
            if (!File.Exists(backupPath))
            {
                throw new InvalidOperationException($"pg_dump did not create backup at '{backupPath}'.");
            }

            await DropDrillDatabaseAsync(postgresContainer, drillDatabase);
            await RunDockerPsqlAsync(postgresContainer, "postgres", $"CREATE DATABASE {drillDatabase};");
            await RunDockerPgRestoreAsync(postgresContainer, drillDatabase, backupPath);

            var connectionString = StlDrRestoreDrillSupport.BuildAdminConnectionString(
                host,
                port,
                username,
                password,
                drillDatabase);

            return await StlDrRestoreDrillValidator.ValidateRestoredDatabaseAsync(connectionString);
        }
        finally
        {
            await DropDrillDatabaseAsync(postgresContainer, drillDatabase);
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
