using Npgsql;

namespace STLCompliance.Shared.Operations;

public static class StlDrRestoreDrillSupport
{
    public const string DefaultDrillSuffix = "_dr_restore_drill";

    public static string GetDrillDatabaseName(string productDatabase, string drillSuffix = DefaultDrillSuffix) =>
        $"{productDatabase}{drillSuffix}";

    public static string BuildAdminConnectionString(
        string host,
        int port,
        string username,
        string password,
        string database) =>
        new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = username,
            Password = password,
            Database = database,
        }.ConnectionString;

    public static string BuildDatabaseConnectionString(
        StlDrRestoreDrillPlan plan,
        string database) =>
        BuildAdminConnectionString(plan.Host, plan.Port, plan.Username, plan.Password, database);

    public static IReadOnlyList<string> ResolveBackupFiles(StlDrRestoreDrillPlan plan)
    {
        if (string.IsNullOrWhiteSpace(plan.BackupDirectory))
        {
            throw new InvalidOperationException("BackupDirectory is required.");
        }

        if (!Directory.Exists(plan.BackupDirectory))
        {
            throw new DirectoryNotFoundException($"Backup directory not found: {plan.BackupDirectory}");
        }

        var resolved = new List<string>();
        foreach (var database in plan.TargetDatabases)
        {
            if (!StlProductDatabaseCatalog.IsKnownProductDatabase(database))
            {
                throw new InvalidOperationException($"Unknown product database '{database}'.");
            }

            resolved.Add(ResolveBackupPath(plan.BackupDirectory, database));
        }

        return resolved;
    }

    public static string ResolveBackupPath(string backupDirectory, string productDatabase)
    {
        var customFormat = Path.Combine(backupDirectory, $"{productDatabase}.custom");
        if (File.Exists(customFormat))
        {
            return customFormat;
        }

        var dumpFormat = Path.Combine(backupDirectory, $"{productDatabase}.dump");
        if (File.Exists(dumpFormat))
        {
            return dumpFormat;
        }

        var sqlFormat = Path.Combine(backupDirectory, $"{productDatabase}.sql");
        if (File.Exists(sqlFormat))
        {
            return sqlFormat;
        }

        throw new FileNotFoundException(
            $"No backup found for '{productDatabase}'. Expected one of: {productDatabase}.custom, {productDatabase}.dump, {productDatabase}.sql",
            sqlFormat);
    }

    public static bool IsCustomFormatBackup(string backupPath)
    {
        var extension = Path.GetExtension(backupPath);
        return extension.Equals(".custom", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".dump", StringComparison.OrdinalIgnoreCase);
    }
}
