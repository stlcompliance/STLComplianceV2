namespace STLCompliance.Shared.Operations.LoadTesting;

public sealed record StlRenderStagingLoadTestEndpointTarget(
    string ProductKey,
    string BaseUrl,
    string LoadTestBaseUrlEnvironmentVariable);
