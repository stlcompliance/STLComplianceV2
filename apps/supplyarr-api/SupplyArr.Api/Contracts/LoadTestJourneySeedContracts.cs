namespace SupplyArr.Api.Contracts;

public sealed record LoadTestJourneySeedResponse(
    Guid DemandRefId,
    string DemandRefSource,
    string SourceRefKey,
    string Title,
    bool DemandRefCreated,
    bool SettingsEnsured);
