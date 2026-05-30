namespace MaintainArr.Api.Contracts;

public sealed record MaintainArrSettingManifestItem(
    string SettingKey,
    string Path,
    string Description);

public sealed record MaintainArrSettingsManifestResponse(
    IReadOnlyList<MaintainArrSettingManifestItem> Items);
