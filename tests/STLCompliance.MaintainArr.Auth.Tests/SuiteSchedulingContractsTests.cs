using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.Scheduling;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class SuiteSchedulingContractsTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ActorUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void Event_envelope_accepts_canonical_created_event()
    {
        var envelope = new StlIntegrationEventEnvelope(
            Guid.NewGuid(),
            StlSuiteEventCatalog.MaintainArr.WorkOrderScheduled,
            StlProductKeys.MaintainArr,
            TenantId,
            "workOrder",
            "wo-1001",
            7,
            DateTimeOffset.UtcNow,
            "person-100",
            StlIntegrationEventActorTypes.Person,
            StlProductKeys.MaintainArr,
            Guid.NewGuid(),
            null,
            StlIntegrationEventEnvelopeRules.BuildIdempotencyKey(
                StlProductKeys.MaintainArr,
                "schedule",
                TenantId,
                "workOrder",
                "wo-1001"),
            "1.0",
            new Dictionary<string, object?>
            {
                ["workOrderId"] = "wo-1001",
                ["scheduledStart"] = "2026-06-18T14:00:00Z",
            });

        Assert.Empty(StlIntegrationEventEnvelopeRules.Validate(envelope));
    }

    [Fact]
    public void Event_envelope_rejects_wrong_prefix_missing_schema_and_sensitive_payload()
    {
        var envelope = new StlIntegrationEventEnvelope(
            Guid.NewGuid(),
            StlSuiteEventCatalog.OrdArr.OrderCreated,
            StlProductKeys.MaintainArr,
            TenantId,
            "workOrder",
            "wo-1001",
            null,
            DateTimeOffset.UtcNow,
            null,
            StlIntegrationEventActorTypes.Service,
            StlProductKeys.MaintainArr,
            Guid.NewGuid(),
            null,
            string.Empty,
            string.Empty,
            new Dictionary<string, object?>
            {
                ["nested"] = new Dictionary<string, object?>
                {
                    ["serviceToken"] = "do-not-emit",
                },
            });

        var errors = StlIntegrationEventEnvelopeRules.Validate(envelope);

        Assert.Contains(errors, error => error.Contains("eventType prefix", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, error => error.Contains("idempotencyKey", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, error => error.Contains("schemaVersion", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, error => error.Contains("payload.nested.serviceToken", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task MaintainArr_schedule_sets_local_work_order_and_emits_idempotent_event()
    {
        await using var db = CreateDb();
        var workOrder = await SeedWorkOrderAsync(db);
        var service = BuildSchedulingService(db);
        var request = BuildScheduleRequest(workOrder.Id, "schedule-once");

        var first = await service.ScheduleAsync(TenantId, ActorUserId, "person-100", request, canOverride: false, isReschedule: false);
        var second = await service.ScheduleAsync(TenantId, ActorUserId, "person-100", request, canOverride: false, isReschedule: false);

        var refreshed = await db.WorkOrders.SingleAsync(x => x.Id == workOrder.Id);
        var outbox = await db.MaintenancePlatformOutboxEvents
            .Where(x => x.RelatedEntityId == workOrder.Id)
            .ToListAsync();

        Assert.Equal("scheduled", first.Status);
        Assert.Equal("scheduled", second.Status);
        Assert.Equal(WorkOrderStatuses.Scheduled, refreshed.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-06-18T14:00:00Z"), refreshed.PlannedStartAt);
        Assert.Equal("person-active", refreshed.AssignedTechnicianPersonId);
        Assert.Single(outbox);
        Assert.Equal(MaintenancePlatformOutboxEventKinds.WorkOrderScheduled, outbox[0].EventKind);
    }

    [Fact]
    public async Task MaintainArr_validation_blocks_inactive_staffarr_person()
    {
        await using var db = CreateDb();
        var workOrder = await SeedWorkOrderAsync(db, activePersonStatus: "inactive");
        var service = BuildSchedulingService(db);

        var validation = await service.ValidateAsync(
            TenantId,
            BuildScheduleRequest(workOrder.Id, "inactive-person"),
            canOverride: false);

        Assert.False(validation.Allowed);
        Assert.Contains(validation.Blockers, blocker => blocker.Code == "inactive_person");
    }

    [Fact]
    public async Task MaintainArr_validation_blocks_inactive_location()
    {
        await using var db = CreateDb();
        var workOrder = await SeedWorkOrderAsync(db);
        var request = BuildScheduleRequest(workOrder.Id, "closed-location") with
        {
            LocationAssignments =
            [
                new StlSchedulingLocationAssignment(
                    SiteId: null,
                    LocationId: "dock-9",
                    SourceProductKey: StlProductKeys.StaffArr,
                    DisplayName: "Dock 9",
                    Status: "closed")
            ],
        };

        var validation = await BuildSchedulingService(db).ValidateAsync(TenantId, request, canOverride: false);

        Assert.False(validation.Allowed);
        Assert.Contains(validation.Blockers, blocker => blocker.Code == "inactive_location");
    }

    [Fact]
    public async Task MaintainArr_validation_blocks_asset_not_ready_without_override()
    {
        await using var db = CreateDb();
        var workOrder = await SeedWorkOrderAsync(db, assetLifecycleStatus: "out_of_service");
        var service = BuildSchedulingService(db);

        var blocked = await service.ValidateAsync(
            TenantId,
            BuildScheduleRequest(workOrder.Id, "asset-blocked"),
            canOverride: false);
        var overridden = await service.ValidateAsync(
            TenantId,
            BuildScheduleRequest(workOrder.Id, "asset-override") with
            {
                Override = new StlSchedulingOverrideRequest(true, "Manager accepted temporary readiness risk.", ["asset_not_ready"]),
            },
            canOverride: true);

        Assert.False(blocked.Allowed);
        Assert.Contains(blocked.Blockers, blocker => blocker.Code == "asset_not_ready");
        Assert.True(overridden.Allowed);
        Assert.Contains(overridden.Warnings, warning => warning.Code == "asset_not_ready");
    }

    private static MaintainArrDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<MaintainArrDbContext>()
            .UseInMemoryDatabase($"maintainarr-scheduling-{Guid.NewGuid():N}")
            .Options;
        return new MaintainArrDbContext(options);
    }

    private static MaintainArrSchedulingService BuildSchedulingService(MaintainArrDbContext db)
    {
        var correlation = new CorrelationIdAccessor();
        correlation.Set(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
        var audit = new MaintainArrAuditService(db, correlation);
        var settings = new MaintenancePlatformEventSettingsService(db, audit);
        var processing = new MaintenancePlatformEventProcessingService(db, settings, audit);
        var outbox = new MaintenancePlatformOutboxEnqueueService(db, settings, processing);
        return new MaintainArrSchedulingService(db, audit, outbox, new MaintainArrTenantSettingsService(db));
    }

    private static async Task<WorkOrder> SeedWorkOrderAsync(
        MaintainArrDbContext db,
        string activePersonStatus = "active",
        string assetLifecycleStatus = "active")
    {
        var now = DateTimeOffset.UtcNow;
        var assetId = Guid.NewGuid();
        var workOrderId = Guid.NewGuid();
        db.Assets.Add(new Asset
        {
            Id = assetId,
            TenantId = TenantId,
            AssetTypeId = Guid.NewGuid(),
            AssetTag = "TRK-100",
            Name = "Truck 100",
            Description = "Scheduling test asset",
            LifecycleStatus = assetLifecycleStatus,
            StaffarrSiteNameSnapshot = "North yard",
            CreatedAt = now,
            UpdatedAt = now,
        });
        var workOrder = new WorkOrder
        {
            Id = workOrderId,
            TenantId = TenantId,
            AssetId = assetId,
            WorkOrderNumber = "WO-1001",
            Title = "Replace sensor",
            Description = "Replace trailer door sensor",
            Priority = WorkOrderPriorities.High,
            Status = WorkOrderStatuses.Planned,
            Source = WorkOrderSources.Manual,
            WorkOrderType = WorkOrderTypes.Corrective,
            OriginType = WorkOrderOriginTypes.Manual,
            CreatedByUserId = ActorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.WorkOrders.Add(workOrder);
        db.StaffPersonRefs.Add(new MaintainArrStaffPersonRef
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            StaffarrPersonId = "person-active",
            DisplayNameSnapshot = "Avery Tech",
            ActiveStatusSnapshot = activePersonStatus,
            PrimarySiteSnapshot = "North yard",
            LastSeenAt = now,
        });
        await db.SaveChangesAsync();
        return workOrder;
    }

    private static StlSchedulingRequest BuildScheduleRequest(Guid workOrderId, string idempotencyKey) =>
        new(
            TenantId,
            StlProductKeys.MaintainArr,
            "workOrder",
            workOrderId.ToString("D"),
            DateTimeOffset.Parse("2026-06-18T14:00:00Z"),
            DateTimeOffset.Parse("2026-06-18T16:00:00Z"),
            "UTC",
            [
                new StlSchedulingResourceAssignment(
                    "technician",
                    "person-active",
                    StlProductKeys.StaffArr,
                    "Avery Tech",
                    "primary_technician")
            ],
            [],
            [],
            "test",
            Guid.NewGuid(),
            idempotencyKey,
            [
                new StlSchedulingSourceReference(
                    StlProductKeys.MaintainArr,
                    "workOrder",
                    workOrderId.ToString("D"),
                    "WO-1001")
            ],
            null,
            false);
}
