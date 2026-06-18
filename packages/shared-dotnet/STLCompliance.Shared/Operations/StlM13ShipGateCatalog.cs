namespace STLCompliance.Shared.Operations;

/// <summary>
/// M13 ship-gate checklist: OpenAPI parity products, entitlement-denial probes, and tenant-isolation minimums.
/// Integration tests reference this catalog so CI matrices stay explicit and auditable.
/// </summary>
public static class StlM13ShipGateCatalog
{
    public const string OpenApiParityTestProject = "STLCompliance.OpenApi.Tests";
    public const string E2eIntegrationTestProject = "STLCompliance.E2E";

    /// <summary>Product keys with checked-in OpenAPI snapshots (W92).</summary>
    public static readonly IReadOnlyList<string> OpenApiProductKeys =
    [
        "nexarr",
        "staffarr",
        "trainarr",
        "maintainarr",
        "routarr",
        "supplyarr",
        "customarr",
        "compliancecore",
        "loadarr",
        "reportarr",
        "assurarr",
    ];

    /// <summary>Path fragments every product OpenAPI document must expose.</summary>
    public static readonly IReadOnlyList<string> RequiredOpenApiPathFragments =
    [
        "/health",
        "/api/",
    ];

    /// <summary>Product APIs that must reject <c>/api/me</c> when the JWT lacks the product entitlement.</summary>
    public static readonly IReadOnlyList<M13EntitlementDenialProbe> ProductApiEntitlementDenialProbes =
    [
        new("staffarr", "staffarr", "/api/me"),
        new("trainarr", "trainarr", "/api/me"),
        new("maintainarr", "maintainarr", "/api/me"),
        new("routarr", "routarr", "/api/me"),
        new("supplyarr", "supplyarr", "/api/me"),
        new("customarr", "customarr", "/api/me"),
        new("reportarr", "reportarr", "/api/me"),
        new("compliancecore", "compliancecore", "/api/me"),
    ];

    /// <summary>NexArr launch authority denial when the product is unknown or not entitled.</summary>
    public const string NexArrLaunchContextPath = "/api/v1/launch/context";

    public const string NexArrDeniedLaunchProductKey = "nonexistent-product";

    /// <summary>Minimum integration facts in <c>TenantIsolationFlowTests</c> (W95/W96 + NexArr).</summary>
    public const int MinimumTenantIsolationIntegrationTests = 11;

    /// <summary>
    /// Minimum integration facts in <c>EntitlementDenialFlowTests</c>
    /// (seven product APIs + NexArr launch denial).
    /// </summary>
    public const int MinimumEntitlementDenialIntegrationTests = 8;
}

/// <param name="ProductKey">Catalog product key and test host selector.</param>
/// <param name="RequiredEntitlement">Entitlement claim that must be present on the JWT.</param>
/// <param name="DenialPath">Representative route that enforces entitlement (typically <c>/api/me</c>).</param>
public sealed record M13EntitlementDenialProbe(
    string ProductKey,
    string RequiredEntitlement,
    string DenialPath);
