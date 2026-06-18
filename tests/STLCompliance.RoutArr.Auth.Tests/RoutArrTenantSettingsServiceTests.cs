using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrTenantSettingsServiceTests
{
    [Fact]
    public async Task GetEditable_creates_tenant_settings_with_all_groups()
    {
        await using var db = CreateDb();
        var service = CreateService(db);

        var response = await service.GetEditableAsync(Guid.NewGuid(), "person-admin");

        Assert.Contains(response.Groups, x => x.GroupKey == "general");
        Assert.Contains(response.Groups, x => x.GroupKey == "dispatchBoard");
        Assert.Contains(response.Groups, x => x.GroupKey == "closeout");
        Assert.Contains(
            response.Groups.Single(x => x.GroupKey == "general").Fields,
            x => x.SettingKey == "defaultOperatingTimezone" && string.Equals(Convert.ToString(x.Value), "America/Chicago", StringComparison.Ordinal));
        Assert.Equal(21, response.Groups.Count);
        Assert.True(await db.RoutArrTenantSettings.AnyAsync(x => x.TenantId == response.TenantId));
    }

    [Fact]
    public async Task ValidateGroup_blocks_invalid_auto_tender_without_routing_guide()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var tenantId = Guid.NewGuid();
        await service.GetEditableAsync(tenantId, "person-admin");

        var result = await service.ValidateGroupAsync(
            tenantId,
            new ValidateRoutArrTenantSettingGroupRequest(
                "tendering",
                new Dictionary<string, JsonElement>
                {
                    ["defaultTenderMethod"] = JsonValue("auto_tender"),
                }),
            "person-admin");

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, x => x.FieldPath == "tendering.defaultTenderMethod");
    }

    [Fact]
    public async Task GetEffective_applies_scope_precedence_from_site_to_trip()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var tenantId = Guid.NewGuid();
        var actorPersonId = "person-admin";
        await service.GetEditableAsync(tenantId, actorPersonId);
        db.ChangeTracker.Clear();

        var siteScope = new RoutArrSettingsScopeReference(
            "site",
            "staffarr",
            "location",
            "site_stl",
            "St. Louis terminal",
            "active");
        var tripScope = new RoutArrSettingsScopeReference(
            "trip",
            "routarr",
            "trip",
            "trip_123",
            "TRIP-123",
            "planned");

        await service.CreateOverrideAsync(
            tenantId,
            actorPersonId,
            new CreateRoutArrTenantSettingOverrideRequest(
                siteScope,
                "general",
                "defaultCurrency",
                JsonValue("CAD"),
                "Regional cross-border work"));
        db.ChangeTracker.Clear();

        await service.CreateOverrideAsync(
            tenantId,
            actorPersonId,
            new CreateRoutArrTenantSettingOverrideRequest(
                tripScope,
                "general",
                "defaultCurrency",
                JsonValue("MXN"),
                "Specific trip contract"));
        db.ChangeTracker.Clear();

        var effective = await service.GetEffectiveAsync(tenantId, [siteScope, tripScope], actorPersonId);
        var currency = effective.Groups.Single(x => x.GroupKey == "general")
            .Fields.Single(x => x.SettingKey == "defaultCurrency");

        Assert.Equal("MXN", Convert.ToString(currency.Value));
        Assert.Equal("tripOverride", currency.EffectiveSource);
    }

    [Fact]
    public async Task UpdateGroup_creates_audit_entry_and_settings_event()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var tenantId = Guid.NewGuid();
        var actorPersonId = "person-admin";
        var editable = await service.GetEditableAsync(tenantId, actorPersonId);

        await service.UpdateGroupAsync(
            tenantId,
            actorPersonId,
            "general",
            new UpdateRoutArrTenantSettingGroupRequest(
                editable.Version,
                new Dictionary<string, JsonElement>
                {
                    ["defaultCurrency"] = JsonValue("CAD"),
                },
                "Canadian operations"));

        var audit = await db.RoutArrTenantSettingAuditEntries.SingleAsync(x => x.TenantId == tenantId);
        Assert.Equal("updated", audit.Action);
        Assert.Equal("general", audit.SettingGroup);
        Assert.Equal("defaultCurrency", audit.ChangedKeys);

        var outbox = await db.IntegrationOutboxEvents.SingleAsync(x =>
            x.TenantId == tenantId
            && x.EventKind == RoutArrIntegrationOutboxEventKinds.TenantSettingsUpdated);
        Assert.Equal("routarr_tenant_settings", outbox.RelatedEntityType);
    }

    [Fact]
    public async Task UpdateGroup_replaces_multi_select_rows()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var tenantId = Guid.NewGuid();
        var actorPersonId = "person-admin";
        var editable = await service.GetEditableAsync(tenantId, actorPersonId);

        var updated = await service.UpdateGroupAsync(
            tenantId,
            actorPersonId,
            "demand",
            new UpdateRoutArrTenantSettingGroupRequest(
                editable.Version,
                new Dictionary<string, JsonElement>
                {
                    ["requiredDemandFields"] = JsonValue(new[] { "origin", "destination", "customer" }),
                },
                "Require customer references for planning"));

        await service.UpdateGroupAsync(
            tenantId,
            actorPersonId,
            "demand",
            new UpdateRoutArrTenantSettingGroupRequest(
                updated.Version,
                new Dictionary<string, JsonElement>
                {
                    ["requiredDemandFields"] = JsonValue(new[] { "origin" }),
                },
                "Relax planning intake fields"));

        var listItems = await db.RoutArrTenantSettingListItems
            .Where(x => x.TenantId == tenantId
                && x.SettingGroup == "demand"
                && x.SettingKey == "requiredDemandFields")
            .OrderBy(x => x.SortOrder)
            .Select(x => x.ItemKey)
            .ToListAsync();

        Assert.Equal(["origin"], listItems);
    }

    [Fact]
    public async Task UpdateGroup_rejects_stale_expected_version()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var tenantId = Guid.NewGuid();
        await service.GetEditableAsync(tenantId, "person-admin");

        var exception = await Assert.ThrowsAsync<StlApiException>(() =>
            service.UpdateGroupAsync(
                tenantId,
                "person-admin",
                "general",
                new UpdateRoutArrTenantSettingGroupRequest(
                    99,
                    new Dictionary<string, JsonElement>
                    {
                        ["defaultCurrency"] = JsonValue("CAD"),
                    })));

        Assert.Equal("routarr.settings.concurrency_conflict", exception.Code);
    }

    private static RoutArrTenantSettingsService CreateService(RoutArrDbContext db)
    {
        var integrationSettings = new IntegrationEventSettingsService(db, new NoopAuditService());
        var outbox = new IntegrationOutboxEnqueueService(db, integrationSettings);
        return new RoutArrTenantSettingsService(db, outbox);
    }

    private static RoutArrDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<RoutArrDbContext>()
            .UseInMemoryDatabase($"routarr-tenant-settings-{Guid.NewGuid():N}")
            .Options;
        return new RoutArrDbContext(options);
    }

    private static JsonElement JsonValue<T>(T value) =>
        JsonSerializer.SerializeToElement(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private sealed class NoopAuditService : IRoutArrAuditService
    {
        public Task<RoutArrAuditWriteResult> WriteAsync(
            string action,
            Guid tenantId,
            Guid? actorUserId,
            string targetType,
            string? targetId,
            string result,
            string? reasonCode = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new RoutArrAuditWriteResult(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                action,
                result,
                reasonCode));
    }
}
