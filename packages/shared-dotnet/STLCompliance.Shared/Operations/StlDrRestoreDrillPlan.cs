namespace STLCompliance.Shared.Operations;

/// <summary>
/// Operator plan for a disaster-recovery restore drill against staging or local Postgres.
/// </summary>
public sealed record StlDrRestoreDrillPlan(
    string Host,
    int Port,
    string Username,
    string Password,
    string BackupDirectory,
    string DrillSuffix = StlDrRestoreDrillSupport.DefaultDrillSuffix,
    IReadOnlyList<string>? Databases = null)
{
    public IReadOnlyList<string> TargetDatabases =>
        Databases is { Count: > 0 } selected
            ? selected
            : StlProductDatabaseCatalog.All;

    public string AdminConnectionString =>
        StlDrRestoreDrillSupport.BuildAdminConnectionString(Host, Port, Username, Password, "postgres");
}
