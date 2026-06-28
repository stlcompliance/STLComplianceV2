using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrTenantSettingsRulesTests
{
    [Fact]
    public void Defaults_match_canonical_maintainarr_contract()
    {
        var defaults = MaintainArrTenantSettingsDefaults.Create();

        Assert.Equal(MaintainArrTenantSettingsService.CurrentSchemaVersion, defaults.SchemaVersion);
        Assert.Equal("mixed", defaults.Operating.MaintenanceOperatingMode);
        Assert.Equal("controlled", defaults.Operating.MaintenanceStrictness);
        Assert.Equal("auto", defaults.Assets.AssetNumberingMode);
        Assert.Equal("AST", defaults.Assets.AssetNumberPrefix);
        Assert.Equal("active", defaults.Assets.DefaultAssetStatus);
        Assert.Equal("WO", defaults.WorkOrders.WorkOrderNumberPrefix);
        Assert.True(defaults.Defects.AutoCreateWorkOrderFromDefect);
        Assert.True(defaults.OutOfService.EnableOutOfServiceStatus);
        Assert.Equal(14, defaults.PreventiveMaintenance.PmGenerateDaysAhead);
        Assert.Equal(7, defaults.PreventiveMaintenance.PmGracePeriodDays);
        Assert.Equal("both", defaults.Labor.LaborTimeEntryMode);
        Assert.Equal("request_only", defaults.Parts.PartsReservationMode);
        Assert.Equal("warn", defaults.Compliance.ComplianceCheckMode);
        Assert.False(defaults.Evidence.SendCompletedPacketsToRecordArr);
        Assert.False(defaults.Integrations.EnableRecordArrDocumentPackets);
        Assert.False(defaults.Ui.ShowInternalIds);
    }

    [Fact]
    public void Normalize_applies_schema_version_enum_casing_and_prefix_rules()
    {
        var defaults = MaintainArrTenantSettingsDefaults.Create();
        var settings = defaults with
        {
            SchemaVersion = -1,
            Operating = defaults.Operating with
            {
                MaintenanceOperatingMode = " FACILITY ",
                MaintenanceStrictness = " STRICT "
            },
            Assets = defaults.Assets with
            {
                AssetNumberPrefix = " unit-1 "
            },
            WorkOrders = defaults.WorkOrders with
            {
                WorkOrderNumberPrefix = " mx_wo ",
                DefaultPriority = " HIGH "
            }
        };

        var normalized = MaintainArrTenantSettingsValidator.Normalize(settings);

        Assert.Equal(MaintainArrTenantSettingsService.CurrentSchemaVersion, normalized.SchemaVersion);
        Assert.Equal("facility", normalized.Operating.MaintenanceOperatingMode);
        Assert.Equal("strict", normalized.Operating.MaintenanceStrictness);
        Assert.Equal("UNIT-1", normalized.Assets.AssetNumberPrefix);
        Assert.Equal("MX_WO", normalized.WorkOrders.WorkOrderNumberPrefix);
        Assert.Equal("high", normalized.WorkOrders.DefaultPriority);
    }

    [Fact]
    public void Normalize_rejects_invalid_enum_values()
    {
        var defaults = MaintainArrTenantSettingsDefaults.Create();
        var settings = defaults with
        {
            Compliance = defaults.Compliance with
            {
                ComplianceCheckMode = "silent"
            }
        };

        var ex = Assert.Throws<StlApiException>(() =>
            MaintainArrTenantSettingsValidator.Normalize(settings));

        Assert.Equal("settings.invalid_enum", ex.Code);
    }

    [Fact]
    public void Normalize_rejects_out_of_range_numbers()
    {
        var defaults = MaintainArrTenantSettingsDefaults.Create();
        var settings = defaults with
        {
            Scheduling = defaults.Scheduling with
            {
                DefaultScheduleDurationMinutes = 4
            }
        };

        var ex = Assert.Throws<StlApiException>(() =>
            MaintainArrTenantSettingsValidator.Normalize(settings));

        Assert.Equal("settings.numeric_out_of_range", ex.Code);
    }

    [Fact]
    public void Normalize_rejects_unsupported_labor_rounding()
    {
        var defaults = MaintainArrTenantSettingsDefaults.Create();
        var settings = defaults with
        {
            Labor = defaults.Labor with
            {
                RoundLaborMinutesTo = 7
            }
        };

        var ex = Assert.Throws<StlApiException>(() =>
            MaintainArrTenantSettingsValidator.Normalize(settings));

        Assert.Equal("settings.invalid_labor_rounding", ex.Code);
    }

    [Fact]
    public async Task Upsert_persists_normalized_settings_and_audit_diff()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        var service = new MaintainArrTenantSettingsService(db);
        var tenantId = Guid.NewGuid();

        var initial = await service.GetOrCreateAsync(tenantId, "actor-1");
        var changed = initial.Settings with
        {
            Assets = initial.Settings.Assets with
            {
                AssetNumberingMode = "manual",
                AssetNumberPrefix = " unit "
            },
            Ui = initial.Settings.Ui with
            {
                ShowInternalIds = true
            }
        };

        var response = await service.UpsertAsync(
            tenantId,
            "actor-2",
            new UpsertMaintainArrTenantSettingsRequest(changed, "Tenant maintenance policy update."));

        Assert.Equal("manual", response.Settings.Assets.AssetNumberingMode);
        Assert.Equal("UNIT", response.Settings.Assets.AssetNumberPrefix);
        Assert.True(response.Settings.Ui.ShowInternalIds);
        Assert.Equal(1, await db.MaintainArrTenantSettings.CountAsync(x => x.TenantId == tenantId));

        var audit = await service.ListAuditAsync(tenantId, 10);
        var item = Assert.Single(audit.Items);
        Assert.Equal("Tenant maintenance policy update.", item.ChangeReason);
        Assert.Contains(item.Changes, change =>
            change.Path == "assets.assetNumberingMode"
            && change.Before == "auto"
            && change.After == "manual");
        Assert.Contains(item.Changes, change =>
            change.Path == "assets.assetNumberPrefix"
            && change.Before == "AST"
            && change.After == "UNIT");
        Assert.Contains(item.Changes, change =>
            change.Path == "ui.showInternalIds"
            && change.Before == "false"
            && change.After == "true");
    }

    private static MaintainArrDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MaintainArrDbContext>()
            .UseInMemoryDatabase($"maintainarr-tenant-settings-{Guid.NewGuid():N}")
            .Options;

        return new MaintainArrDbContext(options);
    }
}
