using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class WorkOrderService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit,
    MaintenanceNotificationEnqueueService notificationEnqueueService,
    TechnicianRefService technicianRefService,
    AssetDowntimeService assetDowntimeService,
    TrainArrQualificationCheckClient trainArrQualificationCheckClient,
    ComplianceCoreWorkOrderGateClient complianceCoreWorkOrderGateClient,
    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueue)
{
    public async Task<IReadOnlyList<WorkOrderSummaryResponse>> ListAsync(
        Guid tenantId,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        Guid? assetId = null,
        Guid? defectId = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!viewAll && actorUserId.HasValue)
        {
            var personId = actorPersonId?.Trim();
            query = query.Where(x =>
                x.CreatedByUserId == actorUserId.Value
                || (personId != null
                    && x.AssignedTechnicianPersonId != null
                    && x.AssignedTechnicianPersonId == personId));
        }

        if (assetId.HasValue)
        {
            query = query.Where(x => x.AssetId == assetId.Value);
        }

        if (defectId.HasValue)
        {
            query = query.Where(x => x.DefectId == defectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var workOrders = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return await MapSummariesAsync(tenantId, workOrders, cancellationToken);
    }

    public async Task<WorkOrderDetailResponse> GetAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetWorkOrderEntityAsync(tenantId, workOrderId, cancellationToken);
        return await MapDetailAsync(tenantId, workOrder, cancellationToken);
    }

    public async Task<WorkOrderDetailResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateWorkOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureActiveAssetAsync(tenantId, request.AssetId, cancellationToken);
        ValidateTitle(request.Title);
        ValidatePriority(request.Priority);
        ValidateAssignedPersonId(request.AssignedTechnicianPersonId);
        await EnsureAssignedTechnicianQualifiedAsync(
            tenantId,
            request.AssignedTechnicianPersonId,
            cancellationToken);

        if (request.PmScheduleId.HasValue)
        {
            await EnsurePmScheduleForAssetAsync(
                tenantId,
                request.AssetId,
                request.PmScheduleId.Value,
                cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = request.AssetId,
            PmScheduleId = request.PmScheduleId,
            WorkOrderNumber = await GenerateWorkOrderNumberAsync(tenantId, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Priority = NormalizePriority(request.Priority),
            Status = WorkOrderStatuses.Open,
            Source = request.PmScheduleId.HasValue ? WorkOrderSources.PmSchedule : WorkOrderSources.Manual,
            AssignedTechnicianPersonId = NormalizeAssignedPersonId(request.AssignedTechnicianPersonId),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.WorkOrders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order.create",
            tenantId,
            actorUserId,
            "work_order",
            entity.Id.ToString(),
            entity.Source,
            cancellationToken: cancellationToken);

        await notificationEnqueueService.TryEnqueueAsync(
            tenantId,
            MaintenanceNotificationEventKinds.WorkOrderCreated,
            entity.AssetId,
            "work_order",
            entity.Id,
            cancellationToken);

        await MirrorAssignedTechnicianAsync(
            tenantId,
            actorUserId,
            entity.AssignedTechnicianPersonId,
            cancellationToken);

        await EnqueueWorkOrderLifecycleEventsAsync(
            tenantId,
            actorUserId,
            entity,
            now,
            emitCreated: true,
            emitAssigned: !string.IsNullOrWhiteSpace(entity.AssignedTechnicianPersonId),
            cancellationToken);

        return await MapDetailAsync(tenantId, entity, cancellationToken);
    }

    public async Task<WorkOrderDetailResponse> CreateFromDefectAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid defectId,
        CreateWorkOrderFromDefectRequest request,
        CancellationToken cancellationToken = default)
    {
        var defect = await db.Defects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == defectId, cancellationToken);

        if (defect is null)
        {
            throw new StlApiException("defect.not_found", "Defect was not found.", 404);
        }

        if (string.Equals(defect.Status, DefectStatuses.Closed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(defect.Status, DefectStatuses.Resolved, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "work_order.defect_closed",
                "Work orders cannot be created from resolved or closed defects.",
                400);
        }

        var existing = await db.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.DefectId == defectId
                    && WorkOrderStatuses.Active.Contains(x.Status),
                cancellationToken);

        if (existing is not null)
        {
            return await MapDetailAsync(tenantId, existing, cancellationToken);
        }

        var priority = string.IsNullOrWhiteSpace(request.Priority)
            ? MapDefectSeverityToPriority(defect.Severity)
            : request.Priority;

        ValidatePriority(priority);
        ValidateAssignedPersonId(request.AssignedTechnicianPersonId);
        await EnsureAssignedTechnicianQualifiedAsync(
            tenantId,
            request.AssignedTechnicianPersonId,
            cancellationToken);

        var title = string.IsNullOrWhiteSpace(request.Title)
            ? $"Repair: {defect.Title}"
            : request.Title.Trim();
        ValidateTitle(title);

        var description = request.Description?.Trim() ?? defect.Description;

        var now = DateTimeOffset.UtcNow;
        var entity = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = defect.AssetId,
            DefectId = defectId,
            WorkOrderNumber = await GenerateWorkOrderNumberAsync(tenantId, cancellationToken),
            Title = title,
            Description = description,
            Priority = NormalizePriority(priority),
            Status = WorkOrderStatuses.Open,
            Source = WorkOrderSources.Defect,
            AssignedTechnicianPersonId = NormalizeAssignedPersonId(request.AssignedTechnicianPersonId),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.WorkOrders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order.create_from_defect",
            tenantId,
            actorUserId,
            "work_order",
            entity.Id.ToString(),
            defectId.ToString(),
            cancellationToken: cancellationToken);

        await notificationEnqueueService.TryEnqueueAsync(
            tenantId,
            MaintenanceNotificationEventKinds.WorkOrderCreated,
            entity.AssetId,
            "work_order",
            entity.Id,
            cancellationToken);

        await EnqueueWorkOrderLifecycleEventsAsync(
            tenantId,
            actorUserId,
            entity,
            now,
            emitCreated: true,
            emitAssigned: !string.IsNullOrWhiteSpace(entity.AssignedTechnicianPersonId),
            cancellationToken);

        return await MapDetailAsync(tenantId, entity, cancellationToken);
    }

    public async Task<PmWorkOrderGenerationResult> EnsureForDuePmScheduleAsync(
        Guid pmScheduleId,
        string dueStatus,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (!PmWorkOrderGenerationRules.ShouldEnsureWorkOrder(dueStatus))
        {
            throw new StlApiException(
                "work_order.pm_schedule_not_due",
                "Work orders are only generated for due or overdue PM schedules.",
                400);
        }

        var schedule = await db.PmSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == pmScheduleId, cancellationToken);

        if (schedule is null)
        {
            throw new StlApiException("pm_schedule.not_found", "PM schedule was not found.", 404);
        }

        if (!PmDueScanRules.IsScannableScheduleStatus(schedule.Status))
        {
            throw new StlApiException(
                "work_order.pm_schedule_not_active",
                "Work orders cannot be generated for inactive PM schedules.",
                409);
        }

        var existing = await db.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == schedule.TenantId
                    && x.PmScheduleId == pmScheduleId
                    && WorkOrderStatuses.Active.Contains(x.Status),
                cancellationToken);

        if (existing is not null)
        {
            return new PmWorkOrderGenerationResult(existing.Id, existing.WorkOrderNumber, LinkedExisting: true);
        }

        await EnsureActiveAssetAsync(schedule.TenantId, schedule.AssetId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TenantId = schedule.TenantId,
            AssetId = schedule.AssetId,
            PmScheduleId = pmScheduleId,
            WorkOrderNumber = await GenerateWorkOrderNumberAsync(schedule.TenantId, cancellationToken),
            Title = PmWorkOrderGenerationRules.BuildTitle(schedule.Name),
            Description = PmWorkOrderGenerationRules.BuildDescription(
                schedule.Name,
                schedule.Description,
                schedule.NextDueAt),
            Priority = PmWorkOrderGenerationRules.MapDueStatusToPriority(dueStatus),
            Status = WorkOrderStatuses.Open,
            Source = WorkOrderSources.PmSchedule,
            AssignedTechnicianPersonId = null,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.WorkOrders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order.create_from_pm_schedule",
            schedule.TenantId,
            actorUserId,
            "work_order",
            entity.Id.ToString(),
            pmScheduleId.ToString(),
            cancellationToken: cancellationToken);

        await notificationEnqueueService.TryEnqueueAsync(
            schedule.TenantId,
            MaintenanceNotificationEventKinds.WorkOrderCreated,
            entity.AssetId,
            "work_order",
            entity.Id,
            cancellationToken);

        await EnqueueWorkOrderLifecycleEventsAsync(
            schedule.TenantId,
            actorUserId,
            entity,
            now,
            emitCreated: true,
            emitAssigned: false,
            cancellationToken);

        return new PmWorkOrderGenerationResult(entity.Id, entity.WorkOrderNumber, LinkedExisting: false);
    }

    public async Task<WorkOrderDetailResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        UpdateWorkOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await db.WorkOrders
            .Include(x => x.Asset)
                .ThenInclude(x => x.AssetType)
            .FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken);

        if (workOrder is null)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        if (!WorkOrderStatuses.Active.Contains(workOrder.Status))
        {
            throw new StlApiException(
                "work_order.not_editable",
                "Only open or in-progress work orders can be updated.",
                400);
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            ValidateTitle(request.Title);
            workOrder.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            workOrder.Description = request.Description.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            ValidatePriority(request.Priority);
            workOrder.Priority = NormalizePriority(request.Priority);
        }

        var previousAssignedTechnicianPersonId = workOrder.AssignedTechnicianPersonId;
        if (request.AssignedTechnicianPersonId is not null)
        {
            ValidateAssignedPersonId(request.AssignedTechnicianPersonId);
            await EnsureAssignedTechnicianQualifiedAsync(
                tenantId,
                request.AssignedTechnicianPersonId,
                cancellationToken);
            workOrder.AssignedTechnicianPersonId = NormalizeAssignedPersonId(request.AssignedTechnicianPersonId);
        }

        workOrder.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order.update",
            tenantId,
            actorUserId,
            "work_order",
            workOrder.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await MirrorAssignedTechnicianAsync(
            tenantId,
            actorUserId,
            workOrder.AssignedTechnicianPersonId,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId)
            && !string.Equals(
                previousAssignedTechnicianPersonId,
                workOrder.AssignedTechnicianPersonId,
                StringComparison.Ordinal))
        {
            await EnqueueWorkOrderLifecycleEventsAsync(
                tenantId,
                actorUserId,
                workOrder,
                workOrder.UpdatedAt,
                emitCreated: false,
                emitAssigned: true,
                cancellationToken);
        }

        return await MapDetailAsync(tenantId, workOrder, cancellationToken);
    }

    public async Task<WorkOrderDetailResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        UpdateWorkOrderStatusRequest request,
        bool canCloseAny,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var status = request.Status?.Trim() ?? string.Empty;
        if (!WorkOrderStatuses.All.Contains(status))
        {
            throw new StlApiException(
                "work_order.invalid_status",
                "Status must be open, in_progress, completed, or cancelled.",
                400);
        }

        var workOrder = await db.WorkOrders.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken);

        if (workOrder is null)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        var normalized = status.ToLowerInvariant();
        if (!WorkOrderStatusRules.CanTransition(workOrder.Status, normalized))
        {
            throw new StlApiException(
                "work_order.invalid_transition",
                $"Cannot transition work order from {workOrder.Status} to {normalized}.",
                400);
        }

        if (!canCloseAny)
        {
            EnsureTechnicianCanTransition(workOrder, normalized, actorUserId, actorPersonId);
        }

        await EnsureComplianceCoreAllowsStatusTransitionAsync(
            workOrder,
            normalized,
            actorPersonId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        workOrder.Status = normalized;
        workOrder.UpdatedAt = now;

        if (string.Equals(normalized, WorkOrderStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            workOrder.StartedAt ??= now;
        }

        if (string.Equals(normalized, WorkOrderStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            workOrder.CompletedAt ??= now;
            workOrder.StartedAt ??= now;
        }

        if (string.Equals(normalized, WorkOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            workOrder.CancelledAt ??= now;
        }

        await db.SaveChangesAsync(cancellationToken);

        DowntimeFollowUpResponse? downtimeFollowUp = null;
        if (string.Equals(normalized, WorkOrderStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            var asset = await db.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrder.AssetId, cancellationToken);
            if (asset is not null)
            {
                downtimeFollowUp = await assetDowntimeService.TryOpenWorkOrderRepairDowntimeAsync(
                    tenantId,
                    actorUserId,
                    workOrder.Id,
                    asset.Id,
                    asset.AssetTag,
                    asset.Name,
                    cancellationToken);
            }
        }

        await audit.WriteAsync(
            "work_order.status_update",
            tenantId,
            actorUserId,
            "work_order",
            workOrder.Id.ToString(),
            workOrder.Status,
            cancellationToken: cancellationToken);

        await EnqueueWorkOrderStatusEventAsync(
            tenantId,
            actorUserId,
            workOrder,
            normalized,
            now,
            cancellationToken);

        return await MapDetailAsync(tenantId, workOrder, cancellationToken, downtimeFollowUp);
    }

    private async Task EnqueueWorkOrderLifecycleEventsAsync(
        Guid tenantId,
        Guid actorUserId,
        WorkOrder workOrder,
        DateTimeOffset occurredAt,
        bool emitCreated,
        bool emitAssigned,
        CancellationToken cancellationToken)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrder.AssetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        if (emitCreated)
        {
            await platformOutboxEnqueue.TryEnqueueWorkOrderEventAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.WorkOrderCreated,
                workOrder,
                asset,
                actorUserId,
                occurredAt,
                $"Work order {workOrder.WorkOrderNumber} created for asset {asset.AssetTag}.",
                eventResult: workOrder.Source,
                cancellationToken: cancellationToken);
        }

        if (emitAssigned && !string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId))
        {
            await platformOutboxEnqueue.TryEnqueueWorkOrderEventAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.WorkOrderAssigned,
                workOrder,
                asset,
                actorUserId,
                occurredAt,
                $"Work order {workOrder.WorkOrderNumber} assigned for asset {asset.AssetTag}.",
                eventResult: workOrder.AssignedTechnicianPersonId,
                idempotencyDiscriminator: workOrder.AssignedTechnicianPersonId,
                cancellationToken: cancellationToken);
        }
    }

    private async Task EnqueueWorkOrderStatusEventAsync(
        Guid tenantId,
        Guid actorUserId,
        WorkOrder workOrder,
        string status,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var eventKind = status switch
        {
            WorkOrderStatuses.InProgress => MaintenancePlatformOutboxEventKinds.WorkOrderStarted,
            WorkOrderStatuses.Completed => MaintenancePlatformOutboxEventKinds.WorkOrderCompleted,
            _ => null,
        };

        if (eventKind is null)
        {
            return;
        }

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrder.AssetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        await platformOutboxEnqueue.TryEnqueueWorkOrderEventAsync(
            tenantId,
            eventKind,
            workOrder,
            asset,
            actorUserId,
            occurredAt,
            $"Work order {workOrder.WorkOrderNumber} changed to {status} for asset {asset.AssetTag}.",
            eventResult: status,
            cancellationToken: cancellationToken);
    }

    private async Task EnsureComplianceCoreAllowsStatusTransitionAsync(
        WorkOrder workOrder,
        string toStatus,
        string? actorPersonId,
        CancellationToken cancellationToken)
    {
        if (!complianceCoreWorkOrderGateClient.IsConfigured
            || !IsComplianceCoreGatedStatus(toStatus))
        {
            return;
        }

        var asset = workOrder.Asset
            ?? await db.Assets
                .AsNoTracking()
                .Include(x => x.AssetType)
                .FirstOrDefaultAsync(
                    x => x.TenantId == workOrder.TenantId && x.Id == workOrder.AssetId,
                    cancellationToken)
            ?? throw new StlApiException(
                "work_order.asset_not_found",
                "Work order asset was not found.",
                404);
        var assetTypeKey = asset.AssetType?.TypeKey ?? string.Empty;

        var context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["product"] = "maintainarr",
            ["action"] = "work_order_status_transition",
            ["fromStatus"] = workOrder.Status,
            ["from_status"] = workOrder.Status,
            ["toStatus"] = toStatus,
            ["to_status"] = toStatus,
            ["workOrderId"] = workOrder.Id.ToString("D"),
            ["work_order_id"] = workOrder.Id.ToString("D"),
            ["workOrderNumber"] = workOrder.WorkOrderNumber,
            ["work_order_number"] = workOrder.WorkOrderNumber,
            ["workOrderPriority"] = workOrder.Priority,
            ["work_order_priority"] = workOrder.Priority,
            ["workOrderSource"] = workOrder.Source,
            ["work_order_source"] = workOrder.Source,
            ["assetId"] = workOrder.AssetId.ToString("D"),
            ["asset_id"] = workOrder.AssetId.ToString("D"),
            ["assetTag"] = asset.AssetTag,
            ["asset_tag"] = asset.AssetTag,
            ["assetTypeKey"] = assetTypeKey,
            ["asset_type_key"] = assetTypeKey,
        };

        if (!string.IsNullOrWhiteSpace(actorPersonId))
        {
            context["actorPersonId"] = actorPersonId.Trim();
            context["actor_person_id"] = actorPersonId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId))
        {
            context["assignedTechnicianPersonId"] = workOrder.AssignedTechnicianPersonId;
            context["assigned_technician_person_id"] = workOrder.AssignedTechnicianPersonId;
        }

        if (workOrder.DefectId.HasValue)
        {
            context["defectId"] = workOrder.DefectId.Value.ToString("D");
            context["defect_id"] = workOrder.DefectId.Value.ToString("D");
        }

        if (workOrder.PmScheduleId.HasValue)
        {
            context["pmScheduleId"] = workOrder.PmScheduleId.Value.ToString("D");
            context["pm_schedule_id"] = workOrder.PmScheduleId.Value.ToString("D");
        }

        var result = await complianceCoreWorkOrderGateClient.CheckWorkOrderAsync(
            workOrder.TenantId,
            workOrder.Id,
            workOrder.AssetId,
            asset.AssetTag,
            context,
            cancellationToken);

        if (result is null || IsPermissiveComplianceCoreGateOutcome(result.Outcome))
        {
            return;
        }

        throw new StlApiException(
            "work_order.compliancecore_gate_blocked",
            result.Message,
            409,
            new Dictionary<string, object?>
            {
                ["outcome"] = result.Outcome,
                ["reasonCode"] = result.ReasonCode,
                ["checkResultId"] = result.CheckResultId,
                ["traceId"] = result.TraceId,
                ["appliedWaiverId"] = result.AppliedWaiverId,
                ["appliedWaiverKey"] = result.AppliedWaiverKey,
            });
    }

    private static bool IsComplianceCoreGatedStatus(string status) =>
        string.Equals(status, WorkOrderStatuses.InProgress, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, WorkOrderStatuses.Completed, StringComparison.OrdinalIgnoreCase);

    private static bool IsPermissiveComplianceCoreGateOutcome(string outcome) =>
        string.Equals(outcome, "allow", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "warn", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "waived", StringComparison.OrdinalIgnoreCase);

    private static void EnsureTechnicianCanTransition(
        WorkOrder workOrder,
        string toStatus,
        Guid actorUserId,
        string? actorPersonId)
    {
        var personId = actorPersonId?.Trim();
        var isCreator = workOrder.CreatedByUserId == actorUserId;
        var isAssignee = personId is not null
            && !string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId)
            && string.Equals(workOrder.AssignedTechnicianPersonId, personId, StringComparison.Ordinal);

        if (string.Equals(toStatus, WorkOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.forbidden",
                "Only managers can cancel work orders.",
                403);
        }

        if (string.Equals(toStatus, WorkOrderStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            if (isCreator || isAssignee || string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId))
            {
                return;
            }

            throw new StlApiException(
                "auth.forbidden",
                "You can only start work orders you created, are assigned to, or that are unassigned.",
                403);
        }

        if (string.Equals(toStatus, WorkOrderStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            if (isAssignee || (isCreator && string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId)))
            {
                return;
            }

            throw new StlApiException(
                "auth.forbidden",
                "You can only complete work orders assigned to you or that you created when unassigned.",
                403);
        }
    }

    private async Task<string> GenerateWorkOrderNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var number = $"WO-{DateTimeOffset.UtcNow:yyyyMMdd}-{suffix}";
            var exists = await db.WorkOrders
                .AsNoTracking()
                .AnyAsync(x => x.TenantId == tenantId && x.WorkOrderNumber == number, cancellationToken);
            if (!exists)
            {
                return number;
            }
        }

        return $"WO-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..24];
    }

    private async Task EnsureActiveAssetAsync(Guid tenantId, Guid assetId, CancellationToken cancellationToken)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);

        if (asset is null)
        {
            throw new StlApiException("asset.not_found", "Asset was not found.", 404);
        }

        if (!string.Equals(asset.LifecycleStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "asset.not_active",
                "Work orders can only be created for active assets.",
                400);
        }
    }

    private async Task EnsurePmScheduleForAssetAsync(
        Guid tenantId,
        Guid assetId,
        Guid pmScheduleId,
        CancellationToken cancellationToken)
    {
        var schedule = await db.PmSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == pmScheduleId, cancellationToken);

        if (schedule is null)
        {
            throw new StlApiException("pm_schedule.not_found", "PM schedule was not found.", 404);
        }

        if (schedule.AssetId != assetId)
        {
            throw new StlApiException(
                "work_order.pm_schedule_asset_mismatch",
                "PM schedule does not belong to the selected asset.",
                400);
        }
    }

    private async Task<WorkOrder> GetWorkOrderEntityAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken);

        if (workOrder is null)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        return workOrder;
    }

    private async Task<IReadOnlyList<WorkOrderSummaryResponse>> MapSummariesAsync(
        Guid tenantId,
        IReadOnlyList<WorkOrder> workOrders,
        CancellationToken cancellationToken)
    {
        if (workOrders.Count == 0)
        {
            return [];
        }

        var assetIds = workOrders.Select(x => x.AssetId).Distinct().ToList();
        var assets = await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return workOrders
            .Select(workOrder =>
            {
                assets.TryGetValue(workOrder.AssetId, out var asset);
                return new WorkOrderSummaryResponse(
                    workOrder.Id,
                    workOrder.WorkOrderNumber,
                    workOrder.AssetId,
                    asset?.AssetTag ?? string.Empty,
                    asset?.Name ?? string.Empty,
                    workOrder.DefectId,
                    workOrder.PmScheduleId,
                    workOrder.Title,
                    workOrder.Priority,
                    workOrder.Status,
                    workOrder.Source,
                    workOrder.AssignedTechnicianPersonId,
                    workOrder.CreatedByUserId,
                    workOrder.CreatedAt,
                    workOrder.UpdatedAt,
                    workOrder.StartedAt,
                    workOrder.CompletedAt,
                    workOrder.CancelledAt);
            })
            .ToList();
    }

    private async Task<WorkOrderDetailResponse> MapDetailAsync(
        Guid tenantId,
        WorkOrder workOrder,
        CancellationToken cancellationToken,
        DowntimeFollowUpResponse? downtimeFollowUp = null)
    {
        var summaries = await MapSummariesAsync(tenantId, [workOrder], cancellationToken);
        var summary = summaries[0];

        string? defectTitle = null;
        if (workOrder.DefectId.HasValue)
        {
            var defect = await db.Defects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrder.DefectId.Value, cancellationToken);
            defectTitle = defect?.Title;
        }

        string? pmScheduleName = null;
        if (workOrder.PmScheduleId.HasValue)
        {
            var schedule = await db.PmSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrder.PmScheduleId.Value, cancellationToken);
            pmScheduleName = schedule?.Name;
        }

        return new WorkOrderDetailResponse(
            summary.WorkOrderId,
            summary.WorkOrderNumber,
            summary.AssetId,
            summary.AssetTag,
            summary.AssetName,
            summary.DefectId,
            defectTitle,
            summary.PmScheduleId,
            pmScheduleName,
            workOrder.Title,
            workOrder.Description,
            workOrder.Priority,
            workOrder.Status,
            workOrder.Source,
            workOrder.AssignedTechnicianPersonId,
            workOrder.CreatedByUserId,
            workOrder.CreatedAt,
            workOrder.UpdatedAt,
            workOrder.StartedAt,
            workOrder.CompletedAt,
            workOrder.CancelledAt,
            downtimeFollowUp);
    }

    private static string MapDefectSeverityToPriority(string severity) =>
        severity.Trim().ToLowerInvariant() switch
        {
            DefectSeverities.Critical => WorkOrderPriorities.Urgent,
            DefectSeverities.High => WorkOrderPriorities.High,
            DefectSeverities.Low => WorkOrderPriorities.Low,
            _ => WorkOrderPriorities.Medium,
        };

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new StlApiException("work_order.title_required", "Work order title is required.", 400);
        }

        if (title.Trim().Length > 256)
        {
            throw new StlApiException("work_order.title_too_long", "Work order title must be 256 characters or fewer.", 400);
        }
    }

    private static void ValidatePriority(string priority)
    {
        if (!WorkOrderPriorities.All.Contains(priority))
        {
            throw new StlApiException(
                "work_order.invalid_priority",
                "Priority must be low, medium, high, or urgent.",
                400);
        }
    }

    private static void ValidateAssignedPersonId(string? personId)
    {
        if (personId is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(personId))
        {
            return;
        }

        if (personId.Trim().Length > 128)
        {
            throw new StlApiException(
                "work_order.assigned_person_too_long",
                "Assigned technician person id must be 128 characters or fewer.",
                400);
        }
    }

    private static string NormalizePriority(string priority) => priority.Trim().ToLowerInvariant();

    private static string? NormalizeAssignedPersonId(string? personId)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return null;
        }

        return personId.Trim();
    }

    private async Task EnsureAssignedTechnicianQualifiedAsync(
        Guid tenantId,
        string? assignedTechnicianPersonId,
        CancellationToken cancellationToken)
    {
        if (!trainArrQualificationCheckClient.IsConfigured || string.IsNullOrWhiteSpace(assignedTechnicianPersonId))
        {
            return;
        }

        if (!Guid.TryParse(assignedTechnicianPersonId.Trim(), out var staffarrPersonId))
        {
            throw new StlApiException(
                "work_order.technician_person_id_invalid",
                "Assigned technician person id must be a StaffArr person GUID when TrainArr qualification checks are enabled.",
                400);
        }

        var check = await trainArrQualificationCheckClient.CheckTechnicianAsync(
            tenantId,
            staffarrPersonId,
            cancellationToken);

        if (check is null || string.Equals(check.Outcome, "allow", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        throw new StlApiException(
            "work_order.technician_qualification_blocked",
            $"TrainArr technician qualification check returned {check.Outcome}: {check.Message}",
            409);
    }

    private async Task MirrorAssignedTechnicianAsync(
        Guid tenantId,
        Guid actorUserId,
        string? assignedTechnicianPersonId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(assignedTechnicianPersonId))
        {
            return;
        }

        await technicianRefService.UpsertFromAssignmentAsync(
            tenantId,
            actorUserId,
            assignedTechnicianPersonId,
            null,
            cancellationToken);
    }
}
