using System.Text.Json;

namespace RoutArr.Api.Contracts;

public sealed record RoutArrSettingOptionResponse(
    string Value,
    string Label);

public sealed record RoutArrSettingFieldResponse(
    string SettingKey,
    string Label,
    string ValueKind,
    object? Value,
    object? PlatformDefaultValue,
    string EffectiveSource,
    string HelpText,
    IReadOnlyList<RoutArrSettingOptionResponse> Options);

public sealed record RoutArrSettingGroupResponse(
    string GroupKey,
    string Label,
    string Description,
    IReadOnlyList<RoutArrSettingFieldResponse> Fields,
    DateTimeOffset? LastUpdatedAt,
    string? LastUpdatedByPersonId);

public sealed record RoutArrTenantSettingsResponse(
    Guid TenantId,
    int Version,
    DateTimeOffset EffectiveAt,
    IReadOnlyList<RoutArrSettingGroupResponse> Groups,
    IReadOnlyList<RoutArrTenantSettingOverrideResponse> Overrides);

public sealed record RoutArrSettingDefinitionResponse(
    string GroupKey,
    string SettingKey,
    string Label,
    string ValueKind,
    object? PlatformDefaultValue,
    string HelpText,
    IReadOnlyList<RoutArrSettingOptionResponse> Options);

public sealed record RoutArrSettingGroupDefinitionResponse(
    string GroupKey,
    string Label,
    string Description,
    IReadOnlyList<RoutArrSettingDefinitionResponse> Fields);

public sealed record RoutArrTenantSettingsOptionsResponse(
    IReadOnlyList<RoutArrSettingGroupDefinitionResponse> Groups,
    IReadOnlyList<RoutArrSettingOptionResponse> ScopeTypes,
    IReadOnlyList<RoutArrSettingOptionResponse> Permissions);

public sealed record RoutArrSettingValidationIssue(
    string FieldPath,
    string Message,
    string Severity);

public sealed record RoutArrSettingsValidationResponse(
    bool IsValid,
    IReadOnlyList<RoutArrSettingValidationIssue> Issues);

public sealed record UpdateRoutArrTenantSettingGroupRequest(
    int? ExpectedVersion,
    IReadOnlyDictionary<string, JsonElement> Values,
    string? Reason = null);

public sealed record ValidateRoutArrTenantSettingGroupRequest(
    string SettingGroup,
    IReadOnlyDictionary<string, JsonElement> Values);

public sealed record ResetRoutArrTenantSettingGroupRequest(
    int? ExpectedVersion,
    string? Reason = null);

public sealed record RoutArrSettingsScopeReference(
    string ScopeType,
    string SourceProduct,
    string EntityType,
    string StableId,
    string DisplayLabelSnapshot,
    string? StatusSnapshot = null,
    DateTimeOffset? SnapshotAt = null);

public sealed record PreviewRoutArrEffectiveSettingsRequest(
    IReadOnlyList<RoutArrSettingsScopeReference> Scopes);

public sealed record CreateRoutArrTenantSettingOverrideRequest(
    RoutArrSettingsScopeReference Scope,
    string SettingGroup,
    string SettingKey,
    JsonElement Value,
    string Reason,
    bool IsEmergencyOverride = false);

public sealed record UpdateRoutArrTenantSettingOverrideRequest(
    int? ExpectedVersion,
    JsonElement Value,
    string Reason,
    bool IsEmergencyOverride = false);

public sealed record RoutArrTenantSettingOverrideResponse(
    string OverrideKey,
    int Version,
    RoutArrSettingsScopeReference Scope,
    string SettingGroup,
    string SettingKey,
    string ValueKind,
    object? Value,
    bool IsEmergencyOverride,
    string Reason,
    DateTimeOffset UpdatedAt,
    string UpdatedByPersonId);

public sealed record RoutArrTenantSettingAuditEntryResponse(
    string AuditKey,
    string Action,
    string SettingGroup,
    IReadOnlyList<string> ChangedKeys,
    string ChangedByPersonId,
    DateTimeOffset ChangedAt,
    int PreviousVersion,
    int NewVersion,
    string? AffectedScopeType,
    string? AffectedScopeRef,
    string Summary);

public sealed record RoutArrTenantSettingAuditHistoryResponse(
    IReadOnlyList<RoutArrTenantSettingAuditEntryResponse> Items);
