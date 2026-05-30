namespace RoutArr.Api.Contracts;

public sealed record RoutArrSettingManifestItem(
    string SettingKey,
    string EndpointPath,
    string Description);

public sealed record RoutArrSettingsManifestResponse(
    IReadOnlyList<RoutArrSettingManifestItem> Items);

