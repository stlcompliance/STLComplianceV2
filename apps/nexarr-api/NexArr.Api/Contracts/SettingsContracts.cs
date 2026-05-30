namespace NexArr.Api.Contracts;

public sealed record NexArrSettingManifestItem(
    string SettingKey,
    string EndpointPath,
    string Description);

public sealed record NexArrSettingsManifestResponse(
    IReadOnlyList<NexArrSettingManifestItem> Items);

