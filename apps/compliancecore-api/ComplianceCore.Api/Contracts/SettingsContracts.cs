namespace ComplianceCore.Api.Contracts;

public sealed record ComplianceCoreSettingManifestItem(
    string SettingKey,
    string EndpointPath,
    string Description);

public sealed record ComplianceCoreSettingsManifestResponse(
    IReadOnlyList<ComplianceCoreSettingManifestItem> Items);

