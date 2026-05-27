namespace STLCompliance.Shared.Operations;

/// <summary>
/// Operator plan for fetching Render staging snapshots and running restore drills.
/// </summary>
public sealed record StlRenderStagingDrillPlan(
    string BackupDirectory,
    IReadOnlyList<StlRenderStagingDatabaseTarget> Targets,
    string DrillSuffix = StlDrRestoreDrillSupport.DefaultDrillSuffix,
    bool SkipSnapshotFetch = false)
{
    public static StlRenderStagingDrillPlan FromTargets(
        string backupDirectory,
        IReadOnlyList<StlRenderStagingDatabaseTarget> targets,
        string drillSuffix = StlDrRestoreDrillSupport.DefaultDrillSuffix,
        bool skipSnapshotFetch = false) =>
        new(backupDirectory, targets, drillSuffix, skipSnapshotFetch);
}
