namespace STLCompliance.Shared.Operations;

/// <summary>
/// M13 ship-gate checklist: OpenAPI parity products, ordinary-product access probes, and tenant-isolation minimums.
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

    /// <summary>
    /// Ordinary product APIs whose <c>/api/me</c> surfaces must remain accessible when the JWT lacks only the
    /// historical launchable-product claim name.
    /// </summary>
    public static readonly IReadOnlyList<M13OrdinaryProductAccessProbe> ProductApiOrdinaryAccessProbes =
    [
        new("staffarr", "/api/me"),
        new("trainarr", "/api/me"),
        new("maintainarr", "/api/me"),
        new("routarr", "/api/me"),
        new("supplyarr", "/api/me"),
        new("reportarr", "/api/me"),
    ];

    /// <summary>NexArr launch authority probe path.</summary>
    public const string NexArrLaunchContextPath = "/api/v1/launch/context";

    public const string NexArrDeniedLaunchProductKey = "nonexistent-product";

    /// <summary>Compliance Core studio hydration stays platform-admin-only.</summary>
    public const string ComplianceCoreStudioMePath = "/api/me";

    /// <summary>Minimum integration facts in <c>TenantIsolationFlowTests</c> (W95/W96 + NexArr).</summary>
    public const int MinimumTenantIsolationIntegrationTests = 11;

    /// <summary>
    /// Minimum integration facts in <c>AccessModelFlowTests</c>
    /// (ordinary product me surfaces + Compliance Core studio denial + NexArr launch checks).
    /// </summary>
    public const int MinimumAccessModelIntegrationTests = 8;
}

/// <param name="ProductKey">Catalog product key and test host selector.</param>
/// <param name="Path">Representative route that must remain accessible after launch (typically <c>/api/me</c>).</param>
public sealed record M13OrdinaryProductAccessProbe(
    string ProductKey,
    string Path);
