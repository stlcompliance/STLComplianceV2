namespace STLCompliance.Shared.Operations;

/// <summary>
/// Canonical local Vite preview ports and handoff launch paths for browser E2E (docker-compose e2e profile).
/// </summary>
public sealed record StlE2eFrontendEndpoint(
    string ProductKey,
    int Port,
    string DefaultBaseUrl,
    string LaunchPath = "/launch");

public static class StlE2eFrontendCatalog
{
    public const string Suite = "suite";

    public const int SuitePort = 5174;
    public const string SuiteDefaultBaseUrl = "http://localhost:5174";

    public const string PlaywrightTenantChromeHandoffSpec =
        StlE2ePlaywrightSpecCatalog.ProductHandoffTenantChromeSpec;

    public const int CompanionPort = 5181;
    public const string CompanionDefaultBaseUrl = "http://localhost:5181";

    public static readonly StlE2eFrontendEndpoint CompanionFrontend =
        new("companion", CompanionPort, CompanionDefaultBaseUrl);

    public static readonly StlE2eFrontendEndpoint SuiteFrontend =
        new(Suite, SuitePort, SuiteDefaultBaseUrl, LaunchPath: "/login");

    public static readonly IReadOnlyList<StlE2eFrontendEndpoint> ProductHandoffFrontends =
    [
        new("staffarr", 5175, "http://localhost:5175"),
        new("trainarr", 5176, "http://localhost:5176"),
        new("compliancecore", 5177, "http://localhost:5177"),
        new("maintainarr", 5178, "http://localhost:5178"),
        new("supplyarr", 5179, "http://localhost:5179"),
        new("routarr", 5180, "http://localhost:5180"),
    ];

    public static readonly IReadOnlyList<StlE2eFrontendEndpoint> All =
        [SuiteFrontend, CompanionFrontend, ..ProductHandoffFrontends];

    public static bool IsKnownHandoffProduct(string productKey) =>
        ProductHandoffFrontends.Any(
            e => string.Equals(e.ProductKey, productKey, StringComparison.OrdinalIgnoreCase));

    public static StlE2eFrontendEndpoint? TryGetHandoffFrontend(string productKey) =>
        ProductHandoffFrontends.FirstOrDefault(
            e => string.Equals(e.ProductKey, productKey, StringComparison.OrdinalIgnoreCase));
}
