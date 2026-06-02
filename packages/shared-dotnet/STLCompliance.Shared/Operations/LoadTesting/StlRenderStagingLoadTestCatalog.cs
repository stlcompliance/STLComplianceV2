namespace STLCompliance.Shared.Operations.LoadTesting;

using STLCompliance.Shared.Operations;

/// <summary>
/// Render Blueprint API services and environment-variable conventions for staging load soak.
/// </summary>
public static class StlRenderStagingLoadTestCatalog
{
    public sealed record Entry(
        string ProductKey,
        string RenderApiServiceName,
        string SourceApiUrlEnvironmentVariable,
        string LoadTestBaseUrlEnvironmentVariable);

    public static readonly IReadOnlyList<Entry> All =
    [
        new(StlProductDatabaseCatalog.NexArr, "nexarr-api", "RENDER_STAGING_NEXARR_API_URL", "STL_NEXARR_BASE_URL"),
        new(StlProductDatabaseCatalog.StaffArr, "staffarr-api", "RENDER_STAGING_STAFFARR_API_URL", "STL_STAFFARR_BASE_URL"),
        new(StlProductDatabaseCatalog.TrainArr, "trainarr-api", "RENDER_STAGING_TRAINARR_API_URL", "STL_TRAINARR_BASE_URL"),
        new(StlProductDatabaseCatalog.MaintainArr, "maintainarr-api", "RENDER_STAGING_MAINTAINARR_API_URL", "STL_MAINTAINARR_BASE_URL"),
        new(StlProductDatabaseCatalog.RoutArr, "routarr-api", "RENDER_STAGING_ROUTARR_API_URL", "STL_ROUTARR_BASE_URL"),
        new(StlProductDatabaseCatalog.SupplyArr, "supplyarr-api", "RENDER_STAGING_SUPPLYARR_API_URL", "STL_SUPPLYARR_BASE_URL"),
        new(StlProductDatabaseCatalog.ComplianceCore, "compliancecore-api", "RENDER_STAGING_COMPLIANCECORE_API_URL", "STL_COMPLIANCECORE_BASE_URL"),
        new(StlProductDatabaseCatalog.LoadArr, "loadarr-api", "RENDER_STAGING_LOADARR_API_URL", "STL_LOADARR_BASE_URL"),
    ];

    public static Entry? TryGetEntry(string productKey) =>
        All.FirstOrDefault(entry =>
            entry.ProductKey.Equals(productKey, StringComparison.OrdinalIgnoreCase));
}
