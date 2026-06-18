using STLCompliance.Shared.Data;

namespace LoadArr.Api.Settings;

public sealed class LoadArrTenantSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public int Version { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public string SettingsJson { get; set; } = string.Empty;

    public string NormalizedSnapshotJson { get; set; } = string.Empty;

    public string RowVersion { get; set; } = Guid.NewGuid().ToString("N");

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByPersonId { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public string? UpdatedByPersonId { get; set; }

    public string? UpdatedByDisplayNameSnapshot { get; set; }
}

public sealed class LoadArrTenantSettingAuditEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid SettingsId { get; set; }

    public int SettingsVersionBefore { get; set; }

    public int SettingsVersionAfter { get; set; }

    public string SectionKey { get; set; } = string.Empty;

    public string? ChangedByPersonId { get; set; }

    public string? ChangedByDisplayNameSnapshot { get; set; }

    public DateTimeOffset ChangedAt { get; set; }

    public string? Reason { get; set; }

    public string ChangeSource { get; set; } = LoadArrTenantSettingChangeSources.Api;

    public string BeforeSummaryJson { get; set; } = string.Empty;

    public string AfterSummaryJson { get; set; } = string.Empty;

    public string ChangedFieldsJson { get; set; } = "[]";

    public string WarningsAcknowledgedJson { get; set; } = "[]";
}

public static class LoadArrTenantSettingChangeSources
{
    public const string Ui = "ui";

    public const string Api = "api";

    public const string Seed = "seed";

    public const string Migration = "migration";

    public const string System = "system";
}

