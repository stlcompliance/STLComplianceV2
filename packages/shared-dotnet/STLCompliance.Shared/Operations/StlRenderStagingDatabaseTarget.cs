namespace STLCompliance.Shared.Operations;

/// <summary>
/// One Render managed Postgres instance used for staging snapshot fetch and restore drill.
/// </summary>
public sealed record StlRenderStagingDatabaseTarget(
    string ProductDatabase,
    string Host,
    int Port,
    string Username,
    string Password,
    string Database)
{
    public string AdminConnectionString =>
        StlDrRestoreDrillSupport.BuildAdminConnectionString(Host, Port, Username, Password, "postgres");

    public string DatabaseConnectionString =>
        StlDrRestoreDrillSupport.BuildAdminConnectionString(Host, Port, Username, Password, Database);

    public StlDrRestoreDrillPlan ToDrRestoreDrillPlan(string backupDirectory, string drillSuffix) =>
        new(Host, Port, Username, Password, backupDirectory, drillSuffix, [ProductDatabase]);
}
