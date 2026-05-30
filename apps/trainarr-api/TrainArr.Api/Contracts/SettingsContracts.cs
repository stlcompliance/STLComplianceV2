namespace TrainArr.Api.Contracts;

public sealed record TrainArrSettingManifestItem(
    string SettingKey,
    string EndpointPath,
    string Description);

public sealed record TrainArrSettingsManifestResponse(
    IReadOnlyList<TrainArrSettingManifestItem> Items);

