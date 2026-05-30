namespace StaffArr.Api.Contracts;

public sealed record StaffArrSettingManifestItem(
    string SettingKey,
    string EndpointPath,
    string Description);

public sealed record StaffArrSettingsManifestResponse(
    IReadOnlyList<StaffArrSettingManifestItem> Items);

