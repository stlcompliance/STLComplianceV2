using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public enum RoutArrTenantSettingValueKind
{
    Boolean = 0,
    Integer = 1,
    Decimal = 2,
    Text = 3,
    Enum = 4,
    Time = 5,
    DurationMinutes = 6,
    MultiSelect = 7,
}

public enum RoutArrTenantSettingScopeType
{
    Tenant = 0,
    Site = 1,
    Terminal = 2,
    Customer = 3,
    Carrier = 4,
    Lane = 5,
    RouteType = 6,
    ServiceType = 7,
    Demand = 8,
    Trip = 9,
}

public sealed class RoutArrTenantSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public int Version { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedByPersonId { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; }

    public string UpdatedByPersonId { get; set; } = string.Empty;

    public ICollection<RoutArrTenantSettingValue> Values { get; set; } = [];

    public ICollection<RoutArrTenantSettingListItem> ListItems { get; set; } = [];

    public ICollection<RoutArrTenantSettingOverride> Overrides { get; set; } = [];
}

public sealed class RoutArrTenantSettingValue : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TenantSettingsId { get; set; }

    public string SettingGroup { get; set; } = string.Empty;

    public string SettingKey { get; set; } = string.Empty;

    public RoutArrTenantSettingValueKind ValueKind { get; set; }

    public bool? BooleanValue { get; set; }

    public int? IntegerValue { get; set; }

    public decimal? DecimalValue { get; set; }

    public string? TextValue { get; set; }

    public string? EnumValue { get; set; }

    public TimeOnly? TimeValue { get; set; }

    public int? DurationMinutesValue { get; set; }

    public bool IsTenantConfigured { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public RoutArrTenantSettings TenantSettings { get; set; } = null!;
}

public sealed class RoutArrTenantSettingListItem : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TenantSettingsId { get; set; }

    public string SettingGroup { get; set; } = string.Empty;

    public string SettingKey { get; set; } = string.Empty;

    public string ItemKey { get; set; } = string.Empty;

    public string DisplayLabel { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsTenantConfigured { get; set; }

    public RoutArrTenantSettings TenantSettings { get; set; } = null!;
}

public sealed class RoutArrTenantSettingOverride : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TenantSettingsId { get; set; }

    public string PublicKey { get; set; } = string.Empty;

    public RoutArrTenantSettingScopeType ScopeType { get; set; }

    public string ScopeSourceProduct { get; set; } = string.Empty;

    public string ScopeEntityType { get; set; } = string.Empty;

    public string ScopeStableId { get; set; } = string.Empty;

    public string ScopeDisplayLabelSnapshot { get; set; } = string.Empty;

    public string ScopeStatusSnapshot { get; set; } = string.Empty;

    public DateTimeOffset ScopeSnapshotAt { get; set; }

    public string SettingGroup { get; set; } = string.Empty;

    public string SettingKey { get; set; } = string.Empty;

    public RoutArrTenantSettingValueKind ValueKind { get; set; }

    public bool? BooleanValue { get; set; }

    public int? IntegerValue { get; set; }

    public decimal? DecimalValue { get; set; }

    public string? TextValue { get; set; }

    public string? EnumValue { get; set; }

    public TimeOnly? TimeValue { get; set; }

    public int? DurationMinutesValue { get; set; }

    public bool IsEmergencyOverride { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedByPersonId { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; }

    public string UpdatedByPersonId { get; set; } = string.Empty;

    public RoutArrTenantSettings TenantSettings { get; set; } = null!;

    public ICollection<RoutArrTenantSettingOverrideListItem> ListItems { get; set; } = [];
}

public sealed class RoutArrTenantSettingOverrideListItem : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid OverrideId { get; set; }

    public string ItemKey { get; set; } = string.Empty;

    public string DisplayLabel { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public RoutArrTenantSettingOverride Override { get; set; } = null!;
}

public sealed class RoutArrTenantSettingAuditEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string PublicKey { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string SettingGroup { get; set; } = string.Empty;

    public string ChangedKeys { get; set; } = string.Empty;

    public string ChangedByPersonId { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; }

    public int PreviousVersion { get; set; }

    public int NewVersion { get; set; }

    public string? AffectedScopeType { get; set; }

    public string? AffectedScopeRef { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string? PreviousSummary { get; set; }

    public string? NewSummary { get; set; }
}
