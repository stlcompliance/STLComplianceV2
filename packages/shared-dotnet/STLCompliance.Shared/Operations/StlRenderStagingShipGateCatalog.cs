namespace STLCompliance.Shared.Operations;

using STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Render staging ship-gate validation inventory (W352): API health/ready probes, optional static sites,
/// local catalog CI gates, and live E2E probe conventions against deployed staging URLs.
/// </summary>
public static class StlRenderStagingShipGateCatalog
{
    public const string LiveModeEnvironmentVariable = "SHIP_GATE_RENDER_STAGING_LIVE";
    public const string OperatorRunbookDocRelativePath = "docs/operations/RENDER_STAGING_SHIP_GATE_V1.md";
    public const string ValidateScriptPs1RelativePath = "scripts/ops/render-staging-ship-gate-validate.ps1";
    public const string ValidateScriptShRelativePath = "scripts/ops/render-staging-ship-gate-validate.sh";
    public const string GitHubWorkflowFileName = "ship-gate-staging-render.yml";
    public const string GitHubWorkflowDisplayName = "Ship Gate Staging Render";

    public sealed record ApiProbeEntry(
        string ProductKey,
        string RenderApiServiceName,
        string SourceApiUrlEnvironmentVariable,
        string E2eApiUrlEnvironmentVariable);

    public sealed record StaticSiteProbeEntry(
        string SiteName,
        string SourceUrlEnvironmentVariable);

    public static readonly IReadOnlyList<ApiProbeEntry> ApiProbes =
    [
        new(StlProductDatabaseCatalog.NexArr, "nexarr-api", "RENDER_STAGING_NEXARR_API_URL", "E2E_NEXARR_URL"),
        new(StlProductDatabaseCatalog.StaffArr, "staffarr-api", "RENDER_STAGING_STAFFARR_API_URL", "E2E_STAFFARR_URL"),
        new(StlProductDatabaseCatalog.TrainArr, "trainarr-api", "RENDER_STAGING_TRAINARR_API_URL", "E2E_TRAINARR_URL"),
        new(StlProductDatabaseCatalog.MaintainArr, "maintainarr-api", "RENDER_STAGING_MAINTAINARR_API_URL", "E2E_MAINTAINARR_URL"),
        new(StlProductDatabaseCatalog.RoutArr, "routarr-api", "RENDER_STAGING_ROUTARR_API_URL", "E2E_ROUTARR_URL"),
        new(StlProductDatabaseCatalog.SupplyArr, "supplyarr-api", "RENDER_STAGING_SUPPLYARR_API_URL", "E2E_SUPPLYARR_URL"),
        new(StlProductDatabaseCatalog.CustomArr, "customarr-api", "RENDER_STAGING_CUSTOMARR_API_URL", "E2E_CUSTOMARR_URL"),
        new(StlProductDatabaseCatalog.OrdArr, "ordarr-api", "RENDER_STAGING_ORDARR_API_URL", "E2E_ORDARR_URL"),
        new(StlProductDatabaseCatalog.LedgArr, "ledgarr-api", "RENDER_STAGING_LEDGARR_API_URL", "E2E_LEDGARR_URL"),
        new(StlProductDatabaseCatalog.ComplianceCore, "compliancecore-api", "RENDER_STAGING_COMPLIANCECORE_API_URL", "E2E_COMPLIANCECORE_URL"),
        new(StlProductDatabaseCatalog.LoadArr, "loadarr-api", "RENDER_STAGING_LOADARR_API_URL", "E2E_LOADARR_URL"),
        new(StlProductDatabaseCatalog.RecordArr, "recordarr-api", "RENDER_STAGING_RECORDARR_API_URL", "E2E_RECORDARR_URL"),
        new(StlProductDatabaseCatalog.AssurArr, "assurarr-api", "RENDER_STAGING_ASSURARR_API_URL", "E2E_ASSURARR_URL"),
        new(StlProductDatabaseCatalog.ReportArr, "reportarr-api", "RENDER_STAGING_REPORTARR_API_URL", "E2E_REPORTARR_URL"),
    ];

    public static readonly IReadOnlyList<StaticSiteProbeEntry> OptionalStaticSiteProbes =
    [
        new("stlcompliancesite", "RENDER_STAGING_STLCOMPLIANCESITE_URL"),
        new("stlcompliancekb", "RENDER_STAGING_STLCOMPLIANCEKB_URL"),
        new("suite-frontend", "RENDER_STAGING_SUITE_FRONTEND_URL"),
        new("staffarr-frontend", "RENDER_STAGING_STAFFARR_FRONTEND_URL"),
        new("trainarr-frontend", "RENDER_STAGING_TRAINARR_FRONTEND_URL"),
        new("maintainarr-frontend", "RENDER_STAGING_MAINTAINARR_FRONTEND_URL"),
        new("routarr-frontend", "RENDER_STAGING_ROUTARR_FRONTEND_URL"),
        new("supplyarr-frontend", "RENDER_STAGING_SUPPLYARR_FRONTEND_URL"),
        new("compliancecore-frontend", "RENDER_STAGING_COMPLIANCECORE_FRONTEND_URL"),
        new("loadarr-frontend", "RENDER_STAGING_LOADARR_FRONTEND_URL"),
        new("assurarr-frontend", "RENDER_STAGING_ASSURARR_FRONTEND_URL"),
        new("fieldcompanion-frontend", "RENDER_STAGING_FIELDCOMPANION_FRONTEND_URL"),
    ];

    public static readonly IReadOnlyList<string> RequiredStagingApiUrlEnvironmentVariables =
        ApiProbes.Select(entry => entry.SourceApiUrlEnvironmentVariable).ToList();

    public static readonly IReadOnlyList<string> OptionalCredentialEnvironmentVariables =
    [
        StlLoadTestAuthDefaults.EmailEnvVar,
        StlLoadTestAuthDefaults.PasswordEnvVar,
        StlLoadTestAuthDefaults.TenantIdEnvVar,
    ];

    public const string LocalCatalogCiFilter = "Category=Ci&Area=RenderStagingShipGate";
    public const string RenderBlueprintCiFilter = "Category=Ci&Area=RenderBlueprint";
    public const string M13ShipGateCatalogCiFilter = "Category=E2e&Area=ShipGate";
    public const string OpenApiShipGateCiFilter = "Category=OpenApi&Area=ShipGate";
    public const string LiveStagingShipGateFilter = "Category=Live&Area=RenderStagingShipGate";
    public const string LiveEntitlementDenialFilter = "Category=Live&Area=AccessModel";

    public static ApiProbeEntry? TryGetApiProbe(string productKey) =>
        ApiProbes.FirstOrDefault(entry =>
            entry.ProductKey.Equals(productKey, StringComparison.OrdinalIgnoreCase));

    public static StaticSiteProbeEntry? TryGetStaticSiteProbe(string siteName) =>
        OptionalStaticSiteProbes.FirstOrDefault(entry =>
            entry.SiteName.Equals(siteName, StringComparison.OrdinalIgnoreCase));
}
