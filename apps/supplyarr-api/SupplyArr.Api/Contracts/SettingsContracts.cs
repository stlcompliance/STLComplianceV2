namespace SupplyArr.Api.Contracts;

public sealed record SupplyArrSettingManifestItem(
    string SettingKey,
    string Path,
    string Description);

public sealed record SupplyArrSettingsManifestResponse(
    IReadOnlyList<SupplyArrSettingManifestItem> Items);
