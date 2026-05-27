using System.Diagnostics;
using STLCompliance.Shared.Operations;

namespace STLCompliance.Dr.Tests;

/// <summary>
/// Live Render staging snapshot fetch + restore drill for one product database.
/// </summary>
internal static class RenderStagingDrillLiveRunner
{
    public static async Task<StlDrRestoreValidationResult> RunProductDatabaseDrillAsync(
        string productDatabase,
        CancellationToken cancellationToken = default)
    {
        var plan = StlRenderStagingDrillSupport.ResolvePlanFromEnvironment(
            productDatabases: [productDatabase]);

        var target = plan.Targets.Single();
        var backupDirectory = Path.Combine(Path.GetTempPath(), $"stl-render-staging-live-{Guid.NewGuid():N}");
        Directory.CreateDirectory(backupDirectory);

        var backupPath = Path.Combine(backupDirectory, $"{productDatabase}.custom");
        var drillDatabase = $"{StlDrRestoreDrillSupport.GetDrillDatabaseName(productDatabase)}_{Guid.NewGuid():N}";

        try
        {
            await RunPgDumpAsync(target, backupPath, cancellationToken);
            await DropDrillDatabaseAsync(target, drillDatabase, cancellationToken);
            await RunPsqlAsync(target, "postgres", $"CREATE DATABASE \"{drillDatabase}\";", cancellationToken);
            await RunPgRestoreAsync(target, drillDatabase, backupPath, cancellationToken);

            var connectionString = StlDrRestoreDrillSupport.BuildAdminConnectionString(
                target.Host,
                target.Port,
                target.Username,
                target.Password,
                drillDatabase);

            return await StlDrRestoreDrillValidator.ValidateRestoredDatabaseAsync(connectionString, cancellationToken);
        }
        finally
        {
            await DropDrillDatabaseAsync(target, drillDatabase, cancellationToken);
            if (Directory.Exists(backupDirectory))
            {
                Directory.Delete(backupDirectory, recursive: true);
            }
        }
    }

    private static Task RunPgDumpAsync(
        StlRenderStagingDatabaseTarget target,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var arguments =
            $"-Fc -h {target.Host} -p {target.Port} -U {target.Username} -d {target.Database} -f \"{outputPath}\"";
        return RunPgToolAsync("pg_dump", arguments, target.Password, cancellationToken);
    }

    private static Task RunPgRestoreAsync(
        StlRenderStagingDatabaseTarget target,
        string drillDatabase,
        string backupPath,
        CancellationToken cancellationToken)
    {
        var arguments =
            $"--no-owner --no-privileges --dbname={drillDatabase} -h {target.Host} -p {target.Port} -U {target.Username} \"{backupPath}\"";
        return RunPgToolAsync("pg_restore", arguments, target.Password, cancellationToken);
    }

    private static Task RunPsqlAsync(
        StlRenderStagingDatabaseTarget target,
        string database,
        string sql,
        CancellationToken cancellationToken) =>
        RunPgToolAsync(
            "psql",
            $"-v ON_ERROR_STOP=1 -h {target.Host} -p {target.Port} -U {target.Username} -d {database} -c \"{sql}\"",
            target.Password,
            cancellationToken);

    private static async Task DropDrillDatabaseAsync(
        StlRenderStagingDatabaseTarget target,
        string drillDatabase,
        CancellationToken cancellationToken)
    {
        var terminateSql =
            $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{drillDatabase}' AND pid <> pg_backend_pid();";
        await RunPsqlAsync(target, "postgres", terminateSql, cancellationToken);
        await RunPsqlAsync(target, "postgres", $"DROP DATABASE IF EXISTS \"{drillDatabase}\";", cancellationToken);
    }

    private static async Task RunPgToolAsync(
        string tool,
        string arguments,
        string password,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = tool,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.Environment["PGPASSWORD"] = password;

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {tool}.");

        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{tool} failed: {error}");
        }
    }
}
