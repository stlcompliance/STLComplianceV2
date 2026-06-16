using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.Scheduling;

namespace MaintainArr.Api.Services;

public sealed class MaintainArrSchedulingService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit,
    MaintenancePlatformOutboxEnqueueService platformOutbox)
{
    private static readonly IReadOnlySet<string> TerminalStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        WorkOrderStatuses.Completed,
        WorkOrderStatuses.Closed,
        WorkOrderStatuses.Cancelled,
        WorkOrderStatuses.Canceled,
        WorkOrderStatuses.Rejected,
    };

    private static readonly IReadOnlySet<string> ScheduledStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        WorkOrderStatuses.Scheduled,
        WorkOrderStatuses.Assigned,
        WorkOrderStatuses.InProgress,
        WorkOrderStatuses.Paused,
        WorkOrderStatuses.CompletedPendingReview,
    };

    public async Task<StlSchedulingBoardResponse> ListUnscheduledAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var workOrders = await LoadWorkOrdersAsync(tenantId, cancellationToken);
        var items = workOrders
            .Where(row => !IsTerminal(row.WorkOrder.Status) && !IsScheduled(row.WorkOrder))
            .Select(row => MapItem(row.WorkOrder, row.Asset, row.Blockers, "unscheduled"))
            .ToList();

        return new StlSchedulingBoardResponse(
            tenantId,
            StlProductKeys.MaintainArr,
            DateTimeOffset.UtcNow,
            "live",
            items,
            []);
    }

    public async Task<StlSchedulingBoardResponse> ListScheduledAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var workOrders = await LoadWorkOrdersAsync(tenantId, cancellationToken);
        var resources = await ListResourcesAsync(tenantId, cancellationToken);
        var items = workOrders
            .Where(row => !IsTerminal(row.WorkOrder.Status) && IsScheduled(row.WorkOrder))
            .Select(row => MapItem(row.WorkOrder, row.Asset, row.Blockers, "scheduled"))
            .ToList();

        return new StlSchedulingBoardResponse(
            tenantId,
            StlProductKeys.MaintainArr,
            DateTimeOffset.UtcNow,
            "live",
            items,
            resources);
    }

    public async Task<IReadOnlyList<StlSchedulingResourceLane>> ListResourcesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.StaffPersonRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.DisplayNameSnapshot)
            .Select(x => new StlSchedulingResourceLane(
                StlProductKeys.MaintainArr,
                "technician",
                x.StaffarrPersonId,
                x.DisplayNameSnapshot,
                x.PrimarySiteSnapshot,
                NormalizeResourceStatus(x.ActiveStatusSnapshot),
                x.PrimarySiteSnapshot,
                null))
            .ToListAsync(cancellationToken);
    }

    public async Task<StlSchedulingValidationResponse> ValidateAsync(
        Guid tenantId,
        StlSchedulingRequest request,
        bool canOverride,
        CancellationToken cancellationToken = default)
    {
        var blockers = new List<StlSchedulingConflict>();
        var warnings = new List<StlSchedulingConflict>();
        var missingPermissions = new List<string>();

        if (request.TenantId != Guid.Empty && request.TenantId != tenantId)
        {
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.Permission,
                "tenant_mismatch",
                "blocked",
                "Scheduling request tenant does not match the authenticated tenant."));
        }

        if (!string.Equals(request.ProductKey, StlProductKeys.MaintainArr, StringComparison.Ordinal))
        {
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.MissingFacts,
                "wrong_product",
                "blocked",
                "MaintainArr can only schedule MaintainArr-owned work."));
        }

        if (!string.Equals(request.ItemType, "workOrder", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.ItemType, "work_order", StringComparison.OrdinalIgnoreCase))
        {
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.MissingFacts,
                "unsupported_item_type",
                "blocked",
                "MaintainArr scheduling currently supports work orders."));
        }

        if (!Guid.TryParse(request.ItemId, out var workOrderId))
        {
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.MissingFacts,
                "invalid_item_id",
                "blocked",
                "Work order ID must be a GUID."));
            return BuildValidation(blockers, warnings, missingPermissions, request.CorrelationId);
        }

        var row = await LoadWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        if (row is null)
        {
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.MissingFacts,
                "work_order_not_found",
                "blocked",
                "Work order was not found in MaintainArr."));
            return BuildValidation(blockers, warnings, missingPermissions, request.CorrelationId);
        }

        if (IsTerminal(row.WorkOrder.Status))
        {
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.OrderStatus,
                "terminal_work_order_status",
                "blocked",
                $"Work order {row.WorkOrder.WorkOrderNumber} is {row.WorkOrder.Status} and cannot be scheduled."));
        }

        if (request.RequestedStart.HasValue != request.RequestedEnd.HasValue)
        {
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.MissingFacts,
                "incomplete_scheduled_window",
                "blocked",
                "Scheduling requires both start and end times."));
        }

        if (request.RequestedStart.HasValue
            && request.RequestedEnd.HasValue
            && request.RequestedStart.Value >= request.RequestedEnd.Value)
        {
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.MissingFacts,
                "invalid_scheduled_window",
                "blocked",
                "Scheduled start must be before scheduled end."));
        }

        ValidateResourceAssignments(request, blockers);
        await ValidateResourceAssignmentsAsync(tenantId, request, blockers, warnings, cancellationToken);
        ValidateLocationAssignments(request, blockers);
        ValidateAssetReadiness(row.Asset, request, canOverride, blockers, warnings);
        ValidateWorkOrderBlockers(row.Blockers, request, canOverride, blockers, warnings);

        if (request.Override?.Requested == true
            && !canOverride)
        {
            missingPermissions.Add(StlSchedulingPermissionKeys.Override(StlProductKeys.MaintainArr));
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.Permission,
                "override_permission_required",
                "blocked",
                "Override requires maintainarr.scheduling.override permission."));
        }

        return BuildValidation(blockers, warnings, missingPermissions, request.CorrelationId);
    }

    public async Task<StlSchedulingMutationResponse> ScheduleAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        StlSchedulingRequest request,
        bool canOverride,
        bool isReschedule,
        CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(tenantId, request, canOverride, cancellationToken);
        var row = await LoadWorkOrderAsync(tenantId, Guid.Parse(request.ItemId), cancellationToken)
            ?? throw new StlApiException("maintainarr.scheduling.work_order_not_found", "Work order was not found.", 404);

        if (!validation.Allowed)
        {
            return new StlSchedulingMutationResponse(
                "blocked",
                MapItem(row.WorkOrder, row.Asset, row.Blockers, IsScheduled(row.WorkOrder) ? "scheduled" : "unscheduled"),
                validation,
                null);
        }

        var now = DateTimeOffset.UtcNow;
        var assignment = request.ResourceAssignments.FirstOrDefault(IsPersonResource);
        row.WorkOrder.PlannedStartAt = request.RequestedStart;
        row.WorkOrder.PlannedDueAt = request.RequestedEnd;
        row.WorkOrder.AssignedTechnicianPersonId = assignment?.ResourceId ?? row.WorkOrder.AssignedTechnicianPersonId;
        row.WorkOrder.Status = WorkOrderStatuses.Scheduled;
        row.WorkOrder.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var eventKind = isReschedule
            ? MaintenancePlatformOutboxEventKinds.WorkOrderRescheduled
            : MaintenancePlatformOutboxEventKinds.WorkOrderScheduled;
        var eventId = await platformOutbox.TryEnqueueWorkOrderEventAsync(
            tenantId,
            eventKind,
            row.WorkOrder,
            row.Asset,
            actorUserId,
            now,
            isReschedule
                ? $"Work order {row.WorkOrder.WorkOrderNumber} was rescheduled."
                : $"Work order {row.WorkOrder.WorkOrderNumber} was scheduled.",
            eventResult: "scheduled",
            idempotencyDiscriminator: request.IdempotencyKey,
            cancellationToken);

        await audit.WriteAsync(
            isReschedule ? "maintainarr.scheduling.reschedule" : "maintainarr.scheduling.schedule",
            tenantId,
            actorUserId,
            actorPersonId,
            "work_order",
            row.WorkOrder.Id.ToString("D"),
            "success",
            reasonCode: request.Reason,
            cancellationToken: cancellationToken);

        var refreshed = await LoadWorkOrderAsync(tenantId, row.WorkOrder.Id, cancellationToken) ?? row;
        return new StlSchedulingMutationResponse(
            isReschedule ? "rescheduled" : "scheduled",
            MapItem(refreshed.WorkOrder, refreshed.Asset, refreshed.Blockers, "scheduled"),
            validation,
            eventId);
    }

    public async Task<StlSchedulingMutationResponse> UnscheduleAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        StlSchedulingRequest request,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadWorkOrderFromRequestAsync(tenantId, request, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        row.WorkOrder.PlannedStartAt = null;
        row.WorkOrder.PlannedDueAt = null;
        row.WorkOrder.AssignedTechnicianPersonId = null;
        row.WorkOrder.Status = WorkOrderStatuses.Planned;
        row.WorkOrder.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var validation = new StlSchedulingValidationResponse(
            StlSchedulingValidationStatuses.Allowed,
            true,
            [],
            [],
            [],
            request.CorrelationId);

        var eventId = await platformOutbox.TryEnqueueWorkOrderEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.WorkOrderUnscheduled,
            row.WorkOrder,
            row.Asset,
            actorUserId,
            now,
            $"Work order {row.WorkOrder.WorkOrderNumber} was unscheduled.",
            eventResult: "unscheduled",
            idempotencyDiscriminator: request.IdempotencyKey,
            cancellationToken);

        await audit.WriteAsync(
            "maintainarr.scheduling.unschedule",
            tenantId,
            actorUserId,
            actorPersonId,
            "work_order",
            row.WorkOrder.Id.ToString("D"),
            "success",
            reasonCode: request.Reason,
            cancellationToken: cancellationToken);

        return new StlSchedulingMutationResponse(
            "unscheduled",
            MapItem(row.WorkOrder, row.Asset, row.Blockers, "unscheduled"),
            validation,
            eventId);
    }

    public async Task<StlSchedulingMutationResponse> CancelAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        StlSchedulingRequest request,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadWorkOrderFromRequestAsync(tenantId, request, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        row.WorkOrder.Status = WorkOrderStatuses.Cancelled;
        row.WorkOrder.CancelledAt = now;
        row.WorkOrder.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var validation = new StlSchedulingValidationResponse(
            StlSchedulingValidationStatuses.Allowed,
            true,
            [],
            [],
            [],
            request.CorrelationId);

        var eventId = await platformOutbox.TryEnqueueWorkOrderEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.WorkOrderCanceled,
            row.WorkOrder,
            row.Asset,
            actorUserId,
            now,
            $"Work order {row.WorkOrder.WorkOrderNumber} was cancelled.",
            eventResult: "cancelled",
            idempotencyDiscriminator: request.IdempotencyKey,
            cancellationToken);

        await audit.WriteAsync(
            "maintainarr.scheduling.cancel",
            tenantId,
            actorUserId,
            actorPersonId,
            "work_order",
            row.WorkOrder.Id.ToString("D"),
            "success",
            reasonCode: request.Reason,
            cancellationToken: cancellationToken);

        return new StlSchedulingMutationResponse(
            "cancelled",
            MapItem(row.WorkOrder, row.Asset, row.Blockers, "cancelled"),
            validation,
            eventId);
    }

    public async Task<StlSchedulingMutationResponse> CompleteAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        StlSchedulingRequest request,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadWorkOrderFromRequestAsync(tenantId, request, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        row.WorkOrder.Status = WorkOrderStatuses.Completed;
        row.WorkOrder.CompletedAt = now;
        row.WorkOrder.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var validation = new StlSchedulingValidationResponse(
            StlSchedulingValidationStatuses.Allowed,
            true,
            [],
            [],
            [],
            request.CorrelationId);

        var eventId = await platformOutbox.TryEnqueueWorkOrderEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.WorkOrderCompleted,
            row.WorkOrder,
            row.Asset,
            actorUserId,
            now,
            $"Work order {row.WorkOrder.WorkOrderNumber} was completed from scheduling.",
            eventResult: "completed",
            idempotencyDiscriminator: request.IdempotencyKey,
            cancellationToken);

        await audit.WriteAsync(
            "maintainarr.scheduling.complete",
            tenantId,
            actorUserId,
            actorPersonId,
            "work_order",
            row.WorkOrder.Id.ToString("D"),
            "success",
            reasonCode: request.Reason,
            cancellationToken: cancellationToken);

        return new StlSchedulingMutationResponse(
            "completed",
            MapItem(row.WorkOrder, row.Asset, row.Blockers, "completed"),
            validation,
            eventId);
    }

    private async Task<WorkOrderSchedulingRow> LoadWorkOrderFromRequestAsync(
        Guid tenantId,
        StlSchedulingRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ItemId, out var workOrderId))
        {
            throw new StlApiException("maintainarr.scheduling.invalid_item_id", "Work order ID must be a GUID.", 400);
        }

        return await LoadWorkOrderAsync(tenantId, workOrderId, cancellationToken)
            ?? throw new StlApiException("maintainarr.scheduling.work_order_not_found", "Work order was not found.", 404);
    }

    private async Task<IReadOnlyList<WorkOrderSchedulingRow>> LoadWorkOrdersAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var workOrders = await db.WorkOrders
            .AsNoTracking()
            .Include(x => x.Asset)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        var workOrderIds = workOrders.Select(x => x.Id).ToList();
        var blockers = await db.WorkOrderBlockers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && workOrderIds.Contains(x.WorkOrderId))
            .ToListAsync(cancellationToken);

        return workOrders
            .Select(workOrder => new WorkOrderSchedulingRow(
                workOrder,
                workOrder.Asset,
                blockers.Where(blocker => blocker.WorkOrderId == workOrder.Id).ToList()))
            .ToList();
    }

    private async Task<WorkOrderSchedulingRow?> LoadWorkOrderAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken);

        if (workOrder is null)
        {
            return null;
        }

        var blockers = await db.WorkOrderBlockers
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .ToListAsync(cancellationToken);

        return new WorkOrderSchedulingRow(workOrder, workOrder.Asset, blockers);
    }

    private static StlSchedulingDisplayItem MapItem(
        WorkOrder workOrder,
        Asset asset,
        IReadOnlyList<WorkOrderBlocker> blockers,
        string scheduleStatus)
    {
        var activeBlockers = blockers
            .Where(blocker => IsActiveBlocker(blocker.Status))
            .Select(blocker => BuildConflict(
                ClassifyBlocker(blocker),
                $"work_order_blocker_{blocker.Id:N}",
                blocker.Severity,
                $"{blocker.Title}: {blocker.Description}",
                blocker.SourceProduct,
                "work_order_blocker",
                blocker.Id.ToString("D"),
                !string.IsNullOrWhiteSpace(blocker.OverrideReason)))
            .ToList();

        IReadOnlyList<StlSchedulingResourceAssignment> assignedResources = string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId)
            ? []
            : [
                new StlSchedulingResourceAssignment(
                    "technician",
                    workOrder.AssignedTechnicianPersonId,
                    StlProductKeys.StaffArr,
                    workOrder.AssignedTechnicianPersonId,
                    "primary_technician")
            ];

        IReadOnlyList<StlSchedulingResourceAssignment> resourceNeeds = assignedResources.Count == 0
            ? [
                new StlSchedulingResourceAssignment(
                    "technician",
                    "unassigned",
                    StlProductKeys.StaffArr,
                    "Technician",
                    "primary_technician")
            ]
            : [];

        var allowedActions = AllowedActionsFor(workOrder).ToList();

        return new StlSchedulingDisplayItem(
            StlProductKeys.MaintainArr,
            "workOrder",
            workOrder.Id.ToString("D"),
            workOrder.WorkOrderNumber,
            $"{workOrder.Title} | {asset.AssetTag}",
            workOrder.Status,
            scheduleStatus,
            workOrder.Priority,
            null,
            null,
            workOrder.PlannedStartAt.HasValue || workOrder.PlannedDueAt.HasValue
                ? new StlSchedulingWindow(workOrder.PlannedStartAt, workOrder.PlannedDueAt, "UTC")
                : null,
            null,
            workOrder.OriginType == WorkOrderOriginTypes.CustomerRequest ? workOrder.OriginRef : null,
            asset.StaffarrSiteOrgUnitId?.ToString("D"),
            workOrder.StaffarrLocationId,
            resourceNeeds,
            assignedResources,
            activeBlockers,
            [],
            BuildSourceRefs(workOrder),
            $"/work-orders/{workOrder.Id:D}",
            allowedActions,
            new Dictionary<string, bool>
            {
                [StlSchedulingActions.Schedule] = allowedActions.Contains(StlSchedulingActions.Schedule),
                [StlSchedulingActions.Reschedule] = allowedActions.Contains(StlSchedulingActions.Reschedule),
                [StlSchedulingActions.Unschedule] = allowedActions.Contains(StlSchedulingActions.Unschedule),
                [StlSchedulingActions.Cancel] = allowedActions.Contains(StlSchedulingActions.Cancel),
                [StlSchedulingActions.Complete] = allowedActions.Contains(StlSchedulingActions.Complete),
            },
            "live");
    }

    private static IReadOnlyList<StlSchedulingSourceReference> BuildSourceRefs(WorkOrder workOrder)
    {
        var refs = new List<StlSchedulingSourceReference>
        {
            new(StlProductKeys.MaintainArr, "workOrder", workOrder.Id.ToString("D"), workOrder.WorkOrderNumber),
            new(StlProductKeys.MaintainArr, "asset", workOrder.AssetId.ToString("D")),
        };

        if (workOrder.DefectId is Guid defectId)
        {
            refs.Add(new StlSchedulingSourceReference(StlProductKeys.MaintainArr, "defect", defectId.ToString("D")));
        }

        if (workOrder.PmScheduleId is Guid pmScheduleId)
        {
            refs.Add(new StlSchedulingSourceReference(StlProductKeys.MaintainArr, "pmSchedule", pmScheduleId.ToString("D")));
        }

        if (!string.IsNullOrWhiteSpace(workOrder.OriginRef))
        {
            refs.Add(new StlSchedulingSourceReference(
                StlProductKeys.MaintainArr,
                workOrder.OriginType,
                workOrder.OriginRef));
        }

        return refs;
    }

    private static IEnumerable<string> AllowedActionsFor(WorkOrder workOrder)
    {
        if (IsTerminal(workOrder.Status))
        {
            yield return StlSchedulingActions.View;
            yield break;
        }

        yield return StlSchedulingActions.View;
        if (IsScheduled(workOrder))
        {
            yield return StlSchedulingActions.Reschedule;
            yield return StlSchedulingActions.Unschedule;
            yield return StlSchedulingActions.Cancel;
            yield return StlSchedulingActions.Complete;
        }
        else
        {
            yield return StlSchedulingActions.Schedule;
            yield return StlSchedulingActions.Cancel;
        }
    }

    private static StlSchedulingValidationResponse BuildValidation(
        IReadOnlyList<StlSchedulingConflict> blockers,
        IReadOnlyList<StlSchedulingConflict> warnings,
        IReadOnlyList<string> missingPermissions,
        Guid correlationId)
    {
        var status = blockers.Count > 0
            ? blockers.Any(x => x.ConflictType == StlSchedulingConflictTypes.MissingFacts)
                ? StlSchedulingValidationStatuses.MissingFacts
                : blockers.Any(x => x.ConflictType == StlSchedulingConflictTypes.Permission)
                    ? StlSchedulingValidationStatuses.MissingPermissions
                    : StlSchedulingValidationStatuses.Blocked
            : warnings.Count > 0
                ? StlSchedulingValidationStatuses.Warning
                : StlSchedulingValidationStatuses.Allowed;

        return new StlSchedulingValidationResponse(
            status,
            blockers.Count == 0,
            blockers,
            warnings,
            missingPermissions,
            correlationId);
    }

    private static StlSchedulingConflict BuildConflict(
        string conflictType,
        string code,
        string severity,
        string message,
        string? sourceProductKey = null,
        string? sourceObjectType = null,
        string? sourceObjectId = null,
        bool overrideAllowed = false) =>
        new(
            conflictType,
            code,
            severity,
            message,
            sourceProductKey ?? StlProductKeys.MaintainArr,
            sourceObjectType,
            sourceObjectId,
            overrideAllowed);

    private async Task ValidateResourceAssignmentsAsync(
        Guid tenantId,
        StlSchedulingRequest request,
        ICollection<StlSchedulingConflict> blockers,
        ICollection<StlSchedulingConflict> warnings,
        CancellationToken cancellationToken)
    {
        var personAssignments = request.ResourceAssignments.Where(IsPersonResource).ToList();
        if (personAssignments.Count == 0)
        {
            warnings.Add(BuildConflict(
                StlSchedulingConflictTypes.Resource,
                "technician_not_assigned",
                "warning",
                "No technician was assigned. MaintainArr will keep the work order schedulable but unassigned."));
            return;
        }

        var personIds = personAssignments.Select(x => x.ResourceId).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var refs = await db.StaffPersonRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && personIds.Contains(x.StaffarrPersonId))
            .ToListAsync(cancellationToken);

        foreach (var assignment in personAssignments)
        {
            var personRef = refs.FirstOrDefault(x => string.Equals(x.StaffarrPersonId, assignment.ResourceId, StringComparison.OrdinalIgnoreCase));
            if (personRef is null)
            {
                blockers.Add(BuildConflict(
                    StlSchedulingConflictTypes.MissingFacts,
                    "person_not_found",
                    "blocked",
                    $"StaffArr person {assignment.ResourceId} was not found in the MaintainArr technician mirror.",
                    StlProductKeys.StaffArr,
                    "person",
                    assignment.ResourceId));
                continue;
            }

            if (IsInactiveStatus(personRef.ActiveStatusSnapshot))
            {
                blockers.Add(BuildConflict(
                    StlSchedulingConflictTypes.Resource,
                    "inactive_person",
                    "blocked",
                    $"{personRef.DisplayNameSnapshot} is inactive and cannot be scheduled.",
                    StlProductKeys.StaffArr,
                    "person",
                    personRef.StaffarrPersonId));
            }
        }
    }

    private static void ValidateResourceAssignments(
        StlSchedulingRequest request,
        ICollection<StlSchedulingConflict> blockers)
    {
        if (request.ResourceAssignments.Any(x => string.IsNullOrWhiteSpace(x.ResourceId)))
        {
            blockers.Add(BuildConflict(
                StlSchedulingConflictTypes.MissingFacts,
                "resource_id_required",
                "blocked",
                "Resource assignments require a resource ID."));
        }
    }

    private static void ValidateLocationAssignments(
        StlSchedulingRequest request,
        ICollection<StlSchedulingConflict> blockers)
    {
        foreach (var location in request.LocationAssignments)
        {
            if (string.IsNullOrWhiteSpace(location.LocationId) && string.IsNullOrWhiteSpace(location.SiteId))
            {
                blockers.Add(BuildConflict(
                    StlSchedulingConflictTypes.Location,
                    "location_ref_required",
                    "blocked",
                    "Location assignments require a site or location reference.",
                    StlProductKeys.StaffArr));
            }

            if (IsInactiveStatus(location.Status))
            {
                blockers.Add(BuildConflict(
                    StlSchedulingConflictTypes.Location,
                    "inactive_location",
                    "blocked",
                    $"{location.DisplayName ?? location.LocationId ?? location.SiteId} is inactive or closed.",
                    StlProductKeys.StaffArr,
                    "location",
                    location.LocationId ?? location.SiteId));
            }
        }
    }

    private static void ValidateAssetReadiness(
        Asset asset,
        StlSchedulingRequest request,
        bool canOverride,
        ICollection<StlSchedulingConflict> blockers,
        ICollection<StlSchedulingConflict> warnings)
    {
        if (!IsInactiveStatus(asset.LifecycleStatus)
            && !string.Equals(asset.LifecycleStatus, "out_of_service", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var conflict = BuildConflict(
            StlSchedulingConflictTypes.AssetReadiness,
            "asset_not_ready",
            "blocked",
            $"Asset {asset.AssetTag} is {asset.LifecycleStatus}.",
            StlProductKeys.MaintainArr,
            "asset",
            asset.Id.ToString("D"),
            overrideAllowed: true);

        if (request.Override?.Requested == true
            && canOverride
            && !string.IsNullOrWhiteSpace(request.Override.Reason))
        {
            warnings.Add(conflict with { Severity = "warning" });
            return;
        }

        blockers.Add(conflict);
    }

    private static void ValidateWorkOrderBlockers(
        IReadOnlyList<WorkOrderBlocker> blockers,
        StlSchedulingRequest request,
        bool canOverride,
        ICollection<StlSchedulingConflict> validationBlockers,
        ICollection<StlSchedulingConflict> warnings)
    {
        foreach (var blocker in blockers.Where(x => IsActiveBlocker(x.Status)))
        {
            var conflict = BuildConflict(
                ClassifyBlocker(blocker),
                $"work_order_blocker_{blocker.Id:N}",
                blocker.Severity,
                $"{blocker.Title}: {blocker.Description}",
                blocker.SourceProduct,
                "work_order_blocker",
                blocker.Id.ToString("D"),
                overrideAllowed: true);

            if (request.Override?.Requested == true
                && canOverride
                && !string.IsNullOrWhiteSpace(request.Override.Reason))
            {
                warnings.Add(conflict with { Severity = "warning" });
                continue;
            }

            validationBlockers.Add(conflict);
        }
    }

    private static string ClassifyBlocker(WorkOrderBlocker blocker)
    {
        if (blocker.BlockerType.Contains("compliance", StringComparison.OrdinalIgnoreCase)
            || string.Equals(blocker.SourceProduct, StlProductKeys.ComplianceCore, StringComparison.OrdinalIgnoreCase))
        {
            return StlSchedulingConflictTypes.Compliance;
        }

        if (blocker.BlockerType.Contains("qualification", StringComparison.OrdinalIgnoreCase)
            || string.Equals(blocker.SourceProduct, StlProductKeys.TrainArr, StringComparison.OrdinalIgnoreCase))
        {
            return StlSchedulingConflictTypes.Qualification;
        }

        if (blocker.BlockerType.Contains("asset", StringComparison.OrdinalIgnoreCase))
        {
            return StlSchedulingConflictTypes.AssetReadiness;
        }

        return StlSchedulingConflictTypes.Resource;
    }

    private static bool IsScheduled(WorkOrder workOrder) =>
        ScheduledStatuses.Contains(workOrder.Status)
        || workOrder.PlannedStartAt.HasValue
        || workOrder.PlannedDueAt.HasValue;

    private static bool IsTerminal(string status) => TerminalStatuses.Contains(status);

    private static bool IsActiveBlocker(string status) =>
        string.Equals(status, "active", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "open", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "blocked", StringComparison.OrdinalIgnoreCase);

    private static bool IsPersonResource(StlSchedulingResourceAssignment assignment) =>
        string.Equals(assignment.ResourceType, "person", StringComparison.OrdinalIgnoreCase)
        || string.Equals(assignment.ResourceType, "technician", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeResourceStatus(string? status) =>
        string.IsNullOrWhiteSpace(status) ? "unknown" : status.Trim().ToLowerInvariant();

    private static bool IsInactiveStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        return status.Equals("inactive", StringComparison.OrdinalIgnoreCase)
            || status.Equals("closed", StringComparison.OrdinalIgnoreCase)
            || status.Equals("terminated", StringComparison.OrdinalIgnoreCase)
            || status.Equals("retired", StringComparison.OrdinalIgnoreCase)
            || status.Equals("out_of_service", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record WorkOrderSchedulingRow(
        WorkOrder WorkOrder,
        Asset Asset,
        IReadOnlyList<WorkOrderBlocker> Blockers);
}
