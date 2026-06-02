namespace STLCompliance.Shared.Operations;

/// <summary>
/// Render Blueprint database services and environment-variable conventions for staging DR drills.
/// </summary>
public static class StlRenderStagingDrillCatalog
{
    public sealed record Entry(
        string ProductDatabase,
        string RenderDatabaseServiceName,
        string SourceDatabaseUrlEnvironmentVariable);

    public static readonly IReadOnlyList<Entry> All =
    [
        new(StlProductDatabaseCatalog.NexArr, "nexarr-db", "RENDER_STAGING_NEXARR_DATABASE_URL"),
        new(StlProductDatabaseCatalog.StaffArr, "staffarr-db", "RENDER_STAGING_STAFFARR_DATABASE_URL"),
        new(StlProductDatabaseCatalog.TrainArr, "trainarr-db", "RENDER_STAGING_TRAINARR_DATABASE_URL"),
        new(StlProductDatabaseCatalog.MaintainArr, "maintainarr-db", "RENDER_STAGING_MAINTAINARR_DATABASE_URL"),
        new(StlProductDatabaseCatalog.RoutArr, "routarr-db", "RENDER_STAGING_ROUTARR_DATABASE_URL"),
        new(StlProductDatabaseCatalog.SupplyArr, "supplyarr-db", "RENDER_STAGING_SUPPLYARR_DATABASE_URL"),
        new(StlProductDatabaseCatalog.ComplianceCore, "compliancecore-db", "RENDER_STAGING_COMPLIANCECORE_DATABASE_URL"),
        new(StlProductDatabaseCatalog.LoadArr, "loadarr-db", "RENDER_STAGING_LOADARR_DATABASE_URL"),
    ];

    public static Entry? TryGetEntry(string productDatabase) =>
        All.FirstOrDefault(entry =>
            entry.ProductDatabase.Equals(productDatabase, StringComparison.OrdinalIgnoreCase));
}
