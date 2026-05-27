namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// SLO targets for M13 load-test scenarios.
/// Product-owner targets (V1) are active by default; engineering defaults remain for harness development.
/// </summary>
public static class StlLoadTestSloCatalog
{
    public const string EngineeringDefaultsProfile = "engineering-defaults";
    public const string ProductOwnerProfile = "product-owner";

    public const string ActiveProfileEnvVar = "STL_LOAD_SLO_PROFILE";

    public const string ApiHealthLivenessKey = "api-health-liveness";
    public const string ApiHealthReadyKey = "api-health-ready";
    public const string NexArrPlatformHealthKey = "nexarr-platform-health";
    public const string NexArrAuthMeKey = "nexarr-auth-me";
    public const string ProductAuthHandoffMeKey = "product-auth-handoff-me";
    public const string TrainArrQualificationCheckKey = "trainarr-qualification-check";
    public const string RoutArrDispatchWorkflowGateKey = "routarr-dispatch-workflow-gate";

    public static readonly IReadOnlyList<StlLoadTestSloTarget> EngineeringDefaults =
    [
        new(
            ApiHealthLivenessKey,
            "All product APIs /health liveness",
            P95LatencyMsMax: 500,
            ErrorRateMax: 0.01,
            MinRequestCount: 50,
            Notes: "Engineering placeholder — lightweight liveness probes against all seven APIs."),
        new(
            ApiHealthReadyKey,
            "All product APIs /health/ready readiness",
            P95LatencyMsMax: 2000,
            ErrorRateMax: 0.02,
            MinRequestCount: 50,
            Notes: "Engineering placeholder — readiness includes database checks."),
        new(
            NexArrPlatformHealthKey,
            "NexArr GET /api/platform/health aggregation",
            P95LatencyMsMax: 5000,
            ErrorRateMax: 0.05,
            MinRequestCount: 20,
            Notes: "Engineering placeholder — aggregates downstream /health/ready probes."),
        new(
            NexArrAuthMeKey,
            "NexArr POST /api/auth/login + GET /api/me",
            P95LatencyMsMax: 1500,
            ErrorRateMax: 0.02,
            MinRequestCount: 30,
            Notes: "Engineering placeholder — authenticated NexArr session bootstrap under load."),
        new(
            ProductAuthHandoffMeKey,
            "Cross-product NexArr login → handoff → redeem → GET /api/me",
            P95LatencyMsMax: 8000,
            ErrorRateMax: 0.05,
            MinRequestCount: 12,
            Notes: "Engineering placeholder — full product handoff bootstrap for all six product APIs."),
        new(
            TrainArrQualificationCheckKey,
            "TrainArr handoff → POST /api/qualification-checks",
            P95LatencyMsMax: 12000,
            ErrorRateMax: 0.05,
            MinRequestCount: 8,
            Notes: "Engineering placeholder — cross-product qualification authorization check."),
        new(
            RoutArrDispatchWorkflowGateKey,
            "RoutArr handoff → create trip → POST /api/dispatch-workflow-gates/check",
            P95LatencyMsMax: 15000,
            ErrorRateMax: 0.05,
            MinRequestCount: 6,
            Notes: "Engineering placeholder — dispatch workflow gate with Compliance Core."),
    ];

    public static readonly IReadOnlyList<StlLoadTestSloTarget> ProductOwnerTargets =
    [
        new(
            ApiHealthLivenessKey,
            "All product APIs /health liveness",
            P95LatencyMsMax: 400,
            ErrorRateMax: 0.005,
            MinRequestCount: 50,
            Notes: "PO V1 — docs/operations/PRODUCT_OWNER_LOAD_SLO_V1.md"),
        new(
            ApiHealthReadyKey,
            "All product APIs /health/ready readiness",
            P95LatencyMsMax: 1500,
            ErrorRateMax: 0.01,
            MinRequestCount: 50,
            Notes: "PO V1 — docs/operations/PRODUCT_OWNER_LOAD_SLO_V1.md"),
        new(
            NexArrPlatformHealthKey,
            "NexArr GET /api/platform/health aggregation",
            P95LatencyMsMax: 4000,
            ErrorRateMax: 0.03,
            MinRequestCount: 20,
            Notes: "PO V1 — docs/operations/PRODUCT_OWNER_LOAD_SLO_V1.md"),
        new(
            NexArrAuthMeKey,
            "NexArr POST /api/auth/login + GET /api/me",
            P95LatencyMsMax: 1200,
            ErrorRateMax: 0.01,
            MinRequestCount: 30,
            Notes: "PO V1 — docs/operations/PRODUCT_OWNER_LOAD_SLO_V1.md"),
        new(
            ProductAuthHandoffMeKey,
            "Cross-product NexArr login → handoff → redeem → GET /api/me",
            P95LatencyMsMax: 6000,
            ErrorRateMax: 0.03,
            MinRequestCount: 12,
            Notes: "PO V1 — docs/operations/PRODUCT_OWNER_LOAD_SLO_V1.md"),
        new(
            TrainArrQualificationCheckKey,
            "TrainArr handoff → POST /api/qualification-checks",
            P95LatencyMsMax: 10000,
            ErrorRateMax: 0.04,
            MinRequestCount: 10,
            Notes: "PO V1 — TrainArr qualification authorization journey."),
        new(
            RoutArrDispatchWorkflowGateKey,
            "RoutArr handoff → create trip → POST /api/dispatch-workflow-gates/check",
            P95LatencyMsMax: 12000,
            ErrorRateMax: 0.04,
            MinRequestCount: 8,
            Notes: "PO V1 — RoutArr dispatch workflow gate journey."),
    ];

    public static string ActiveProfile
    {
        get
        {
            var value = Environment.GetEnvironmentVariable(ActiveProfileEnvVar);
            return string.Equals(value, EngineeringDefaultsProfile, StringComparison.OrdinalIgnoreCase)
                ? EngineeringDefaultsProfile
                : ProductOwnerProfile;
        }
    }

    public static IReadOnlyList<StlLoadTestSloTarget> GetActiveTargets() =>
        ActiveProfile == EngineeringDefaultsProfile
            ? EngineeringDefaults
            : ProductOwnerTargets;

    public static StlLoadTestSloTarget GetByScenarioKey(string scenarioKey) =>
        GetActiveTargets().FirstOrDefault(s => s.ScenarioKey.Equals(scenarioKey, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Unknown load-test scenario '{scenarioKey}' for profile '{ActiveProfile}'.");
}
