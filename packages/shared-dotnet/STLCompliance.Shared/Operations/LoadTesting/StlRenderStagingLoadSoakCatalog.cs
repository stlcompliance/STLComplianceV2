namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Default soak settings for Render staging k6 runs against product-owner SLOs.
/// </summary>
public static class StlRenderStagingLoadSoakCatalog
{
    public const int DefaultVirtualUsers = 5;
    public const string DefaultDuration = "30s";

    public static readonly IReadOnlyList<string> DefaultScenarioKeys =
    [
        StlLoadTestSloCatalog.ApiHealthLivenessKey,
        StlLoadTestSloCatalog.ApiHealthReadyKey,
        StlLoadTestSloCatalog.NexArrPlatformHealthKey,
        StlLoadTestSloCatalog.NexArrAuthMeKey,
        StlLoadTestSloCatalog.ProductAuthHandoffMeKey,
        StlLoadTestSloCatalog.TrainArrQualificationCheckKey,
        StlLoadTestSloCatalog.RoutArrDispatchWorkflowGateKey,
    ];

    public static StlLoadTestSloTarget ResolveSoakSloTarget(string scenarioKey) =>
        StlLoadTestSloCatalog.GetByScenarioKey(scenarioKey);
}
