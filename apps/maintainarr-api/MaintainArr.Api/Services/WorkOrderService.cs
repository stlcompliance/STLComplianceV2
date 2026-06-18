using System.Text.Json;
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
    PmOccurrenceService pmOccurrences,
    AssetDowntimeService assetDowntimeService,
    AssetReadinessService assetReadinessService,
    WorkOrderDiscussionService discussionService,
    TrainArrQualificationCheckClient trainArrQualificationCheckClient,
    ComplianceCoreWorkOrderGateClient complianceCoreWorkOrderGateClient,
    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueue,
    MaintainArrTenantSettingsService tenantSettings)
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
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        if (request.DefectId.HasValue && request.PmScheduleId.HasValue)
        {
            throw new StlApiException(
                "work_order.multiple_sources",
                "Choose either a defect or a PM schedule as the work order source, not both.",
                400);
        }

        if (request.DefectId.HasValue)
        {
            return await CreateFromDefectAsync(
                tenantId,
                actorUserId,
                request.DefectId.Value,
                new CreateWorkOrderFromDefectRequest(
                    request.Title,
                    request.Description,
                    ResolveRequestedPriority(request.Priority, settings),
                    request.AssignedTechnicianPersonId,
                    request.DraftPlanJson,
                    request.PlannedStartAt,
                    request.PlannedDueAt),
                cancellationToken);
        }

        EnsureAssetPolicy(request.AssetId, settings);
        var asset = await EnsureActiveAssetAsync(tenantId, request.AssetId, cancellationToken);
        ValidateTitle(request.Title);
        var priority = ResolveRequestedPriority(request.Priority, settings);
        ValidatePriority(priority);
        ValidateAssignedPersonId(request.AssignedTechnicianPersonId);
        EnsureAssignedTechnicianPolicy(request.AssignedTechnicianPersonId, settings);
        var qualificationSnapshot = await CheckAssignedTechnicianQualificationAsync(
            tenantId,
            request.AssignedTechnicianPersonId,
            settings,
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
            Priority = NormalizePriority(priority),
            Status = WorkOrderStatuses.Open,
            Source = ResolveSource(request),
            WorkOrderType = ResolveWorkOrderType(request),
            OriginType = ResolveOriginType(request),
            OriginRef = ResolveOriginRef(request),
            StaffarrLocationId = NormalizeLocationRef(asset.SiteRef),
            DraftPlanJson = NormalizeDraftPlanJson(request.DraftPlanJson),
            PlannedStartAt = request.PlannedStartAt,
            PlannedDueAt = request.PlannedDueAt,
            RequiredQualificationRefsJson = SerializeStringList(
                qualificationSnapshot is null ? [] : [qualificationSnapshot.QualificationKey]),
            QualificationCheckResultsJson = SerializeQualificationCheckResults(qualificationSnapshot),
            AssignedTechnicianPersonId = NormalizeAssignedPersonId(request.AssignedTechnicianPersonId),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.WorkOrders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await UpsertTechnicianAssignmentAsync(
            tenantId,
            actorUserId,
            entity.Id,
            entity.AssignedTechnicianPersonId,
            qualificationSnapshot,
            now,
            cancellationToken);

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

        await discussionService.RecordTimelineEventAsync(
            tenantId,
            entity.Id,
            "maintainarr.work_order.created",
            now,
            null,
            null,
            $"Work order {entity.WorkOrderNumber} was created.",
            "maintainarr",
            entity.Id.ToString("D"),
            null,
            SerializeWorkOrderSnapshot(entity),
            cancellationToken);

        return await MapDetailAsync(tenantId, entity, cancellationToken);
    }

    public async Task<WorkOrderDetailResponse> CreateDraftAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateWorkOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        return await UpsertDraftAsync(tenantId, actorUserId, null, request, cancellationToken);
    }

    public async Task<WorkOrderDetailResponse> UpdateDraftAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        CreateWorkOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        return await UpsertDraftAsync(tenantId, actorUserId, workOrderId, request, cancellationToken);
    }

    public async Task<WorkOrderValidationResponse> ValidateDraftAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetDraftWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        return await ValidateDraftEntityAsync(tenantId, workOrder, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkOrderDuplicateMatchResponse>> CheckDuplicateDraftAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetDraftWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        return await CheckDuplicateDraftAsync(tenantId, workOrder, cancellationToken);
    }

    public async Task<WorkOrderPreviewResponse> PreviewDraftAsync(
        Guid tenantId,
        Guid workOrderId,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetDraftWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        var validation = await ValidateDraftEntityAsync(tenantId, workOrder, cancellationToken);
        var duplicates = await CheckDuplicateDraftAsync(tenantId, workOrder, cancellationToken);
        var assetReadiness = await assetReadinessService.GetAsync(tenantId, workOrder.AssetId, cancellationToken);
        var complianceFindings = await CheckComplianceGateFindingsAsync(
            workOrder,
            WorkOrderStatuses.Draft,
            "work_order_preview",
            actorPersonId,
            cancellationToken);
        var findings = validation.Findings.Concat(complianceFindings).ToList();
        var hasBlockers = findings.Any(x => string.Equals(x.Severity, "blocker", StringComparison.OrdinalIgnoreCase))
            || assetReadiness.Blockers.Count > 0;

        foreach (var blocker in assetReadiness.Blockers)
        {
            findings.Add(new WorkOrderFindingResponse(
                "readiness",
                "warning",
                blocker.BlockerType,
                blocker.Message,
                Source: blocker.SourceEntityType));
        }

        return new WorkOrderPreviewResponse(
            await MapDetailAsync(tenantId, workOrder, cancellationToken),
            findings,
            duplicates,
            assetReadiness,
            !hasBlockers,
            !hasBlockers,
            !hasBlockers);
    }

    public async Task<WorkOrderDetailResponse> OpenDraftAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetDraftWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        await EnsureOpenableDraftAsync(workOrder, actorPersonId, WorkOrderStatuses.Open, cancellationToken);

        var updated = await UpdateStatusAsync(
            tenantId,
            actorUserId,
            workOrderId,
            new UpdateWorkOrderStatusRequest(WorkOrderStatuses.Open),
            canCloseAny: false,
            actorPersonId,
            cancellationToken);
        workOrder.Status = updated.Status;
        workOrder.UpdatedAt = updated.UpdatedAt;
        workOrder.StartedAt = updated.StartedAt;
        workOrder.CompletedAt = updated.CompletedAt;
        workOrder.CancelledAt = updated.CancelledAt;

        await notificationEnqueueService.TryEnqueueAsync(
            tenantId,
            MaintenanceNotificationEventKinds.WorkOrderCreated,
            workOrder.AssetId,
            "work_order",
            workOrder.Id,
            cancellationToken);

        await EnqueueWorkOrderLifecycleEventsAsync(
            tenantId,
            actorUserId,
            workOrder,
            DateTimeOffset.UtcNow,
            emitCreated: true,
            emitAssigned: !string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId),
            cancellationToken);

        await discussionService.RecordTimelineEventAsync(
            tenantId,
            workOrder.Id,
            "maintainarr.work_order.created",
            DateTimeOffset.UtcNow,
            null,
            null,
            $"Work order {workOrder.WorkOrderNumber} was opened from draft.",
            "maintainarr",
            workOrder.Id.ToString("D"),
            null,
            SerializeWorkOrderSnapshot(workOrder),
            cancellationToken);

        return updated;
    }

    public async Task<WorkOrderDetailResponse> ScheduleDraftAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetDraftWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        await EnsureOpenableDraftAsync(workOrder, actorPersonId, WorkOrderStatuses.Scheduled, cancellationToken);

        var updated = await UpdateStatusAsync(
            tenantId,
            actorUserId,
            workOrderId,
            new UpdateWorkOrderStatusRequest(WorkOrderStatuses.Scheduled),
            canCloseAny: false,
            actorPersonId,
            cancellationToken);
        workOrder.Status = updated.Status;
        workOrder.UpdatedAt = updated.UpdatedAt;
        workOrder.StartedAt = updated.StartedAt;
        workOrder.CompletedAt = updated.CompletedAt;
        workOrder.CancelledAt = updated.CancelledAt;

        await notificationEnqueueService.TryEnqueueAsync(
            tenantId,
            MaintenanceNotificationEventKinds.WorkOrderCreated,
            workOrder.AssetId,
            "work_order",
            workOrder.Id,
            cancellationToken);

        await EnqueueWorkOrderLifecycleEventsAsync(
            tenantId,
            actorUserId,
            workOrder,
            DateTimeOffset.UtcNow,
            emitCreated: true,
            emitAssigned: !string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId),
            cancellationToken);

        await discussionService.RecordTimelineEventAsync(
            tenantId,
            workOrder.Id,
            "maintainarr.work_order.scheduled",
            DateTimeOffset.UtcNow,
            null,
            null,
            $"Work order {workOrder.WorkOrderNumber} was scheduled from draft.",
            "maintainarr",
            workOrder.Id.ToString("D"),
            null,
            SerializeWorkOrderSnapshot(workOrder),
            cancellationToken);

        return updated;
    }

    public async Task<WorkOrderDetailResponse> StartDraftAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetDraftWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        await EnsureOpenableDraftAsync(workOrder, actorPersonId, WorkOrderStatuses.InProgress, cancellationToken);

        var updated = await UpdateStatusAsync(
            tenantId,
            actorUserId,
            workOrderId,
            new UpdateWorkOrderStatusRequest(WorkOrderStatuses.InProgress),
            canCloseAny: false,
            actorPersonId,
            cancellationToken);
        workOrder.Status = updated.Status;
        workOrder.UpdatedAt = updated.UpdatedAt;
        workOrder.StartedAt = updated.StartedAt;
        workOrder.CompletedAt = updated.CompletedAt;
        workOrder.CancelledAt = updated.CancelledAt;

        await notificationEnqueueService.TryEnqueueAsync(
            tenantId,
            MaintenanceNotificationEventKinds.WorkOrderCreated,
            workOrder.AssetId,
            "work_order",
            workOrder.Id,
            cancellationToken);

        await EnqueueWorkOrderLifecycleEventsAsync(
            tenantId,
            actorUserId,
            workOrder,
            DateTimeOffset.UtcNow,
            emitCreated: true,
            emitAssigned: !string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId),
            cancellationToken);

        await discussionService.RecordTimelineEventAsync(
            tenantId,
            workOrder.Id,
            "maintainarr.work_order.started",
            DateTimeOffset.UtcNow,
            null,
            null,
            $"Work order {workOrder.WorkOrderNumber} was started from draft.",
            "maintainarr",
            workOrder.Id.ToString("D"),
            null,
            SerializeWorkOrderSnapshot(workOrder),
            cancellationToken);

        return updated;
    }

    public async Task<WorkOrderDetailResponse> CreateFromDefectAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid defectId,
        CreateWorkOrderFromDefectRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
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
        EnsureAssignedTechnicianPolicy(request.AssignedTechnicianPersonId, settings);
        var qualificationSnapshot = await CheckAssignedTechnicianQualificationAsync(
            tenantId,
            request.AssignedTechnicianPersonId,
            settings,
            cancellationToken);

        var asset = await EnsureActiveAssetAsync(tenantId, defect.AssetId, cancellationToken);

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
            WorkOrderType = WorkOrderTypes.DefectRepair,
            OriginType = WorkOrderOriginTypes.Defect,
            OriginRef = defectId.ToString("D"),
            StaffarrLocationId = NormalizeLocationRef(asset.SiteRef),
            DraftPlanJson = NormalizeDraftPlanJson(request.DraftPlanJson),
            PlannedStartAt = request.PlannedStartAt,
            PlannedDueAt = request.PlannedDueAt,
            RequiredQualificationRefsJson = SerializeStringList(
                qualificationSnapshot is null ? [] : [qualificationSnapshot.QualificationKey]),
            QualificationCheckResultsJson = SerializeQualificationCheckResults(qualificationSnapshot),
            AssignedTechnicianPersonId = NormalizeAssignedPersonId(request.AssignedTechnicianPersonId),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.WorkOrders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await UpsertTechnicianAssignmentAsync(
            tenantId,
            actorUserId,
            entity.Id,
            entity.AssignedTechnicianPersonId,
            qualificationSnapshot,
            now,
            cancellationToken);

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

        var settings = await tenantSettings.LoadEffectiveSettingsAsync(schedule.TenantId, cancellationToken);
        if (!settings.PreventiveMaintenance.PmAutoGenerateWorkOrders)
        {
            throw new StlApiException(
                "work_order.pm_generation_disabled",
                "PM work order generation is disabled by MaintainArr tenant settings.",
                409);
        }

        if (!PmDueScanRules.IsScannableScheduleStatus(schedule.Status))
        {
            throw new StlApiException(
                "work_order.pm_schedule_not_active",
                "Work orders cannot be generated for inactive PM schedules.",
                409);
        }

        var program = await db.PmProgramSchedules
            .AsNoTracking()
            .Where(x => x.PmScheduleId == pmScheduleId && x.PmProgram.Status == PmProgramStatuses.Active)
            .Select(x => x.PmProgram)
            .FirstOrDefaultAsync(cancellationToken);

        if (program is not null && !program.AutoGenerateWorkOrder)
        {
            throw new StlApiException(
                "work_order.pm_program_work_order_generation_disabled",
                "Work order generation is disabled for the linked PM program.",
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
            await pmOccurrences.MarkWorkOrderGeneratedAsync(
                schedule,
                existing.Id.ToString("D"),
                DateTimeOffset.UtcNow,
                cancellationToken);
            return new PmWorkOrderGenerationResult(existing.Id, existing.WorkOrderNumber, LinkedExisting: true);
        }

        var asset = await EnsureActiveAssetAsync(schedule.TenantId, schedule.AssetId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TenantId = schedule.TenantId,
            AssetId = schedule.AssetId,
            PmScheduleId = pmScheduleId,
            TemplateRef = program?.DefaultWorkOrderTemplateRef,
            WorkOrderNumber = await GenerateWorkOrderNumberAsync(schedule.TenantId, cancellationToken),
            Title = PmWorkOrderGenerationRules.BuildTitle(schedule.Name),
            Description = PmWorkOrderGenerationRules.BuildDescription(
                schedule.Name,
                schedule.Description,
                schedule.NextDueAt),
            Priority = PmWorkOrderGenerationRules.MapDueStatusToPriority(dueStatus),
            Status = WorkOrderStatuses.Open,
            Source = WorkOrderSources.PmSchedule,
            WorkOrderType = WorkOrderTypes.Preventive,
            OriginType = WorkOrderOriginTypes.PmDue,
            OriginRef = pmScheduleId.ToString("D"),
            StaffarrLocationId = NormalizeLocationRef(asset.SiteRef),
            RequiredQualificationRefsJson = "[]",
            QualificationCheckResultsJson = "[]",
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

        await EnqueuePmOccurrenceGeneratedEventAsync(
            schedule.TenantId,
            actorUserId,
            schedule.Id,
            entity,
            now,
            cancellationToken);

        await pmOccurrences.MarkWorkOrderGeneratedAsync(
            schedule,
            entity.Id.ToString("D"),
            now,
            cancellationToken);

        await discussionService.RecordTimelineEventAsync(
            schedule.TenantId,
            entity.Id,
            "maintainarr.work_order.created",
            now,
            null,
            null,
            $"Work order {entity.WorkOrderNumber} was created from PM schedule {schedule.ScheduleKey}.",
            "maintainarr",
            entity.Id.ToString("D"),
            null,
            SerializeWorkOrderSnapshot(entity),
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
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
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

        if (!WorkOrderStatuses.Active.Contains(workOrder.Status)
            && !string.Equals(workOrder.Status, WorkOrderStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "work_order.not_editable",
                "Only draft, open, or in-progress work orders can be updated.",
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
        WorkOrderQualificationCheckResultResponse? qualificationSnapshot = null;
        if (request.AssignedTechnicianPersonId is not null)
        {
            ValidateAssignedPersonId(request.AssignedTechnicianPersonId);
            EnsureAssignedTechnicianPolicy(request.AssignedTechnicianPersonId, settings);
            qualificationSnapshot = await CheckAssignedTechnicianQualificationAsync(
                tenantId,
                request.AssignedTechnicianPersonId,
                settings,
                cancellationToken);
            workOrder.AssignedTechnicianPersonId = NormalizeAssignedPersonId(request.AssignedTechnicianPersonId);
            workOrder.RequiredQualificationRefsJson = SerializeStringList(
                qualificationSnapshot is null ? [] : [qualificationSnapshot.QualificationKey]);
            workOrder.QualificationCheckResultsJson = SerializeQualificationCheckResults(qualificationSnapshot);
        }

        if (request.DraftPlanJson is not null)
        {
            workOrder.DraftPlanJson = request.DraftPlanJson.Trim();
        }

        if (request.PlannedStartAt.HasValue)
        {
            workOrder.PlannedStartAt = request.PlannedStartAt;
        }

        if (request.PlannedDueAt.HasValue)
        {
            workOrder.PlannedDueAt = request.PlannedDueAt;
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

        await UpsertTechnicianAssignmentAsync(
            tenantId,
            actorUserId,
            workOrder.Id,
            workOrder.AssignedTechnicianPersonId,
            qualificationSnapshot,
            workOrder.UpdatedAt,
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

        await discussionService.RecordTimelineEventAsync(
            tenantId,
            workOrder.Id,
            "maintainarr.work_order.updated",
            workOrder.UpdatedAt,
            null,
            null,
            $"Work order {workOrder.WorkOrderNumber} was updated.",
            "maintainarr",
            workOrder.Id.ToString("D"),
            null,
            SerializeWorkOrderSnapshot(workOrder),
            cancellationToken);

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
                "Status must be one of the supported work order lifecycle values.",
                400);
        }

        var workOrder = await db.WorkOrders.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken);

        if (workOrder is null)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        var normalized = status.ToLowerInvariant();
        if (!WorkOrderStatusRules.CanTransition(workOrder.Status, normalized))
        {
            throw new StlApiException(
                "work_order.invalid_transition",
                $"Cannot transition work order from {workOrder.Status} to {normalized}.",
                400);
        }

        EnsureReopenAllowed(workOrder.Status, normalized, settings);
        await EnsureWorkOrderClosurePolicyAsync(workOrder, normalized, settings, cancellationToken);

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

        if (string.Equals(normalized, WorkOrderStatuses.CompletedPendingReview, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, WorkOrderStatuses.Completed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, WorkOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            workOrder.CompletedAt ??= now;
            workOrder.StartedAt ??= now;
        }

        if (string.Equals(normalized, WorkOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, WorkOrderStatuses.Canceled, StringComparison.OrdinalIgnoreCase))
        {
            workOrder.CancelledAt ??= now;
        }

        await db.SaveChangesAsync(cancellationToken);

        if (workOrder.DefectId.HasValue)
        {
            var defectStatus = normalized switch
            {
                WorkOrderStatuses.InProgress => DefectStatuses.InRepair,
                WorkOrderStatuses.Completed => DefectStatuses.Resolved,
                _ => null,
            };

            if (defectStatus is not null)
            {
                await SyncLinkedDefectStatusAsync(
                    tenantId,
                    actorUserId,
                    workOrder.DefectId.Value,
                    defectStatus,
                    cancellationToken);
            }
        }

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

        if (string.Equals(normalized, WorkOrderStatuses.Completed, StringComparison.OrdinalIgnoreCase)
            && workOrder.PmScheduleId.HasValue)
        {
            await EnqueuePmOccurrenceCompletedEventAsync(
                tenantId,
                actorUserId,
                workOrder.PmScheduleId.Value,
                workOrder,
                now,
                cancellationToken);
        }

        await discussionService.RecordTimelineEventAsync(
            tenantId,
            workOrder.Id,
            GetStatusTimelineEventType(normalized),
            now,
            actorPersonId,
            null,
            $"Work order {workOrder.WorkOrderNumber} changed to {normalized}.",
            "maintainarr",
            workOrder.Id.ToString("D"),
            null,
            SerializeWorkOrderSnapshot(workOrder),
            cancellationToken);

        return await MapDetailAsync(tenantId, workOrder, cancellationToken, downtimeFollowUp);
    }

    public async Task<WorkOrderBlockerResponse> CreateBlockerAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        CreateWorkOrderBlockerRequest request,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await db.WorkOrders
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken);

        if (workOrder is null)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        var blockerType = NormalizeRequiredValue(request.BlockerType, "work_order.blocker_type_required");
        ValidateBlockerType(blockerType);
        var sourceProduct = NormalizeRequiredValue(request.SourceProduct, "work_order.blocker_source_product_required");
        ValidateSourceProduct(sourceProduct);
        var sourceObjectRef = NormalizeOptionalValue(request.SourceObjectRef);
        var title = NormalizeRequiredValue(request.Title, "work_order.blocker_title_required");
        ValidateBlockerTitle(title);
        var description = NormalizeRequiredValue(request.Description, "work_order.blocker_description_required");
        ValidateBlockerDescription(description);
        var severity = NormalizeRequiredValue(request.Severity, "work_order.blocker_severity_required");
        ValidateBlockerSeverity(severity);
        var status = NormalizeOptionalValue(request.Status) ?? "active";
        ValidateBlockerStatus(status);
        var requiredAction = NormalizeOptionalValue(request.RequiredAction);
        ValidateBlockerRequiredAction(requiredAction);
        var createdByPersonId = NormalizeOptionalValue(request.CreatedByPersonId);
        ValidateOptionalPersonId(createdByPersonId);

        var existing = await db.WorkOrderBlockers
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.WorkOrderId == workOrderId
                    && x.SourceProduct == sourceProduct
                    && x.SourceObjectRef == sourceObjectRef,
                cancellationToken);
        var previousStatus = existing?.Status;

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new WorkOrderBlocker
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkOrderId = workOrderId,
                BlockerType = blockerType,
                SourceProduct = sourceProduct,
                SourceObjectRef = sourceObjectRef,
                Title = title,
                Description = description,
                Severity = severity,
                Status = status,
                RequiredAction = requiredAction,
                CreatedAt = now,
                CreatedByPersonId = createdByPersonId,
            };
            db.WorkOrderBlockers.Add(existing);
        }
        else
        {
            existing.BlockerType = blockerType;
            existing.SourceProduct = sourceProduct;
            existing.SourceObjectRef = sourceObjectRef;
            existing.Title = title;
            existing.Description = description;
            existing.Severity = severity;
            existing.Status = status;
            existing.RequiredAction = requiredAction;
            if (existing.CreatedByPersonId is null)
            {
                existing.CreatedByPersonId = createdByPersonId;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order.blocker_upsert",
            tenantId,
            actorUserId,
            "work_order_blocker",
            existing.Id.ToString(),
            existing.Status,
            cancellationToken: cancellationToken);

        await discussionService.RecordTimelineEventAsync(
            tenantId,
            workOrderId,
            string.Equals(status, "active", StringComparison.OrdinalIgnoreCase)
                ? "maintainarr.work_order.blocked"
                : "maintainarr.work_order.blocker_updated",
            now,
            null,
            null,
            $"{title} - {description}",
            sourceProduct,
            sourceObjectRef,
            null,
            SerializeBlockerSnapshot(existing),
            cancellationToken);

        await EnqueueWorkOrderBlockerEventAsync(
            tenantId,
            actorUserId,
            workOrder,
            existing,
            previousStatus,
            status,
            now,
            cancellationToken);

        return MapBlockerResponse(existing);
    }

    public async Task<WorkOrderCloseoutResponse> CreateCloseoutAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        CreateWorkOrderCloseoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        var workOrder = await db.WorkOrders
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken);

        if (workOrder is null)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        var completionSummary = NormalizeRequiredValue(request.CompletionSummary, "work_order.closeout_summary_required");
        ValidateCloseoutCompletionSummary(completionSummary);
        var rootCause = NormalizeOptionalValue(request.RootCause);
        ValidateCloseoutRootCause(rootCause);
        var correctiveAction = NormalizeOptionalValue(request.CorrectiveAction);
        ValidateCloseoutCorrectiveAction(correctiveAction);
        var preventiveActionRecommendation = NormalizeOptionalValue(request.PreventiveActionRecommendation);
        ValidateCloseoutPreventiveAction(preventiveActionRecommendation);
        var returnToServiceByPersonId = NormalizeOptionalValue(request.ReturnToServiceByPersonId);
        ValidateOptionalPersonId(returnToServiceByPersonId);
        var supervisorReviewedByPersonId = NormalizeOptionalValue(request.SupervisorReviewedByPersonId);
        ValidateOptionalPersonId(supervisorReviewedByPersonId);
        var complianceReviewedByPersonId = NormalizeOptionalValue(request.ComplianceReviewedByPersonId);
        ValidateOptionalPersonId(complianceReviewedByPersonId);
        var qualityReviewedByPersonId = NormalizeOptionalValue(request.QualityReviewedByPersonId);
        ValidateOptionalPersonId(qualityReviewedByPersonId);
        var unresolvedDefectRefs = NormalizeOptionalValue(request.UnresolvedDefectRefs);
        var followUpWorkOrderRefs = NormalizeOptionalValue(request.FollowUpWorkOrderRefs);
        var customerImpactSummary = NormalizeOptionalValue(request.CustomerImpactSummary);
        var downtimeSummary = NormalizeOptionalValue(request.DowntimeSummary);
        var finalAssetReadinessStatus = NormalizeOptionalValue(request.FinalAssetReadinessStatus);
        var finalStatus = NormalizeOptionalValue(request.FinalStatus);
        var now = DateTimeOffset.UtcNow;

        var closeout = await db.WorkOrderCloseouts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId, cancellationToken);

        var evidenceRecordRefs = request.EvidenceRecordRefs is null
            ? DeserializeGuidList(closeout?.EvidenceRecordRefsJson)
            : NormalizeGuidList(request.EvidenceRecordRefs);
        var permitRecordRefs = request.PermitRecordRefs is null
            ? DeserializeGuidList(closeout?.PermitRecordRefsJson)
            : NormalizeGuidList(request.PermitRecordRefs);
        await EnsureCloseoutEvidenceRecordRefsAsync(
            tenantId,
            workOrderId,
            evidenceRecordRefs,
            cancellationToken);

        if (closeout is null)
        {
            closeout = new WorkOrderCloseout
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkOrderId = workOrderId,
                CreatedAt = now,
                CreatedByPersonId = returnToServiceByPersonId ?? supervisorReviewedByPersonId ?? complianceReviewedByPersonId ?? qualityReviewedByPersonId,
            };
            db.WorkOrderCloseouts.Add(closeout);
        }

        closeout.CompletionSummary = completionSummary;
        closeout.RootCause = rootCause;
        closeout.CorrectiveAction = correctiveAction;
        closeout.PreventiveActionRecommendation = preventiveActionRecommendation;
        closeout.AssetReturnedToService = request.AssetReturnedToService;
        closeout.ReturnToServiceAt = request.AssetReturnedToService
            ? request.ReturnToServiceAt ?? now
            : request.ReturnToServiceAt;
        closeout.ReturnToServiceByPersonId = returnToServiceByPersonId;
        closeout.PostRepairInspectionRequired = request.PostRepairInspectionRequired;
        closeout.PostRepairInspectionRef = request.PostRepairInspectionRef;
        closeout.SupervisorReviewRequired = request.SupervisorReviewRequired;
        closeout.SupervisorReviewedByPersonId = supervisorReviewedByPersonId;
        closeout.SupervisorReviewedAt = request.SupervisorReviewedAt;
        closeout.ComplianceReviewRequired = request.ComplianceReviewRequired;
        closeout.ComplianceReviewedByPersonId = complianceReviewedByPersonId;
        closeout.ComplianceReviewedAt = request.ComplianceReviewedAt;
        closeout.QualityReviewRequired = request.QualityReviewRequired;
        closeout.QualityReviewedByPersonId = qualityReviewedByPersonId;
        closeout.QualityReviewedAt = request.QualityReviewedAt;
        closeout.EvidenceAccepted = request.EvidenceAccepted;
        closeout.UnresolvedDefectRefs = unresolvedDefectRefs;
        closeout.FollowUpWorkOrderRefs = followUpWorkOrderRefs;
        closeout.CustomerImpactSummary = customerImpactSummary;
        closeout.DowntimeSummary = downtimeSummary;
        closeout.FinalAssetReadinessStatus = finalAssetReadinessStatus;
        closeout.FinalStatus = NormalizeCloseoutFinalStatus(
            finalStatus ?? (request.AssetReturnedToService ? WorkOrderStatuses.Closed : WorkOrderStatuses.CompletedPendingReview));
        closeout.PermitRecordRefsJson = SerializeGuidList(permitRecordRefs);
        closeout.EvidenceRecordRefsJson = SerializeGuidList(evidenceRecordRefs);

        await EnsureReturnToServicePolicyAsync(settings, workOrder, closeout, cancellationToken);

        await UpsertReturnToServiceAsync(
            tenantId,
            workOrder,
            closeout,
            permitRecordRefs,
            evidenceRecordRefs,
            cancellationToken);

        await UpsertPermitRefsAsync(
            tenantId,
            workOrder,
            closeout,
            permitRecordRefs,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        if (workOrder.DefectId.HasValue && string.Equals(closeout.FinalStatus, WorkOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            await SyncLinkedDefectStatusAsync(
                tenantId,
                actorUserId,
                workOrder.DefectId.Value,
                DefectStatuses.Closed,
                cancellationToken);
        }

        await audit.WriteAsync(
            "work_order.closeout",
            tenantId,
            actorUserId,
            "work_order_closeout",
            closeout.Id.ToString(),
            closeout.FinalStatus,
            cancellationToken: cancellationToken);

        await discussionService.RecordTimelineEventAsync(
            tenantId,
            workOrderId,
            string.Equals(closeout.FinalStatus, WorkOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase)
                ? "maintainarr.work_order.closed"
                : "maintainarr.work_order.closeout_updated",
            now,
            null,
            null,
            $"Work order {workOrder.WorkOrderNumber} closeout updated.",
            "maintainarr",
            closeout.Id.ToString("D"),
            null,
            SerializeCloseoutSnapshot(closeout),
            cancellationToken);

        return MapCloseoutResponse(closeout);
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
            WorkOrderStatuses.Requested or WorkOrderStatuses.Open => MaintenancePlatformOutboxEventKinds.WorkOrderRequested,
            WorkOrderStatuses.Triage => MaintenancePlatformOutboxEventKinds.WorkOrderAssigned,
            WorkOrderStatuses.Approved => MaintenancePlatformOutboxEventKinds.WorkOrderApproved,
            WorkOrderStatuses.Rejected => MaintenancePlatformOutboxEventKinds.WorkOrderRejected,
            WorkOrderStatuses.Planned => MaintenancePlatformOutboxEventKinds.WorkOrderPlanned,
            WorkOrderStatuses.Scheduled => MaintenancePlatformOutboxEventKinds.WorkOrderAssigned,
            WorkOrderStatuses.Assigned => MaintenancePlatformOutboxEventKinds.WorkOrderAssigned,
            WorkOrderStatuses.InProgress => MaintenancePlatformOutboxEventKinds.WorkOrderStarted,
            WorkOrderStatuses.Paused => MaintenancePlatformOutboxEventKinds.WorkOrderPaused,
            WorkOrderStatuses.Completed => MaintenancePlatformOutboxEventKinds.WorkOrderCompleted,
            WorkOrderStatuses.CompletedPendingReview => MaintenancePlatformOutboxEventKinds.WorkOrderCompleted,
            WorkOrderStatuses.Closed => MaintenancePlatformOutboxEventKinds.WorkOrderClosed,
            WorkOrderStatuses.Cancelled or WorkOrderStatuses.Canceled => MaintenancePlatformOutboxEventKinds.WorkOrderCanceled,
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

    private async Task EnqueuePmOccurrenceGeneratedEventAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmScheduleId,
        WorkOrder workOrder,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var schedule = await db.PmSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == pmScheduleId, cancellationToken);
        if (schedule is null)
        {
            return;
        }

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == schedule.AssetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        await pmOccurrences.MarkWorkOrderGeneratedAsync(
            schedule,
            workOrder.Id.ToString("D"),
            occurredAt,
            cancellationToken);

        await platformOutboxEnqueue.TryEnqueuePmOccurrenceEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.PmOccurrenceWorkOrderGenerated,
            schedule,
            asset,
            actorUserId,
            occurredAt,
            $"PM occurrence {schedule.ScheduleKey} generated work order {workOrder.WorkOrderNumber} for asset {asset.AssetTag}.",
            eventResult: workOrder.Id.ToString("D"),
            idempotencyDiscriminator: workOrder.Id.ToString("D"),
            cancellationToken: cancellationToken);
    }

    private async Task EnqueuePmOccurrenceCompletedEventAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmScheduleId,
        WorkOrder workOrder,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var schedule = await db.PmSchedules
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == pmScheduleId, cancellationToken);
        if (schedule is null)
        {
            return;
        }

        var previousDueStatus = schedule.DueStatus;
        schedule.LastCompletedAt = occurredAt;
        schedule.DueStatus = PmDueStatuses.Completed;
        schedule.UpdatedAt = occurredAt;
        await db.SaveChangesAsync(cancellationToken);

        await pmOccurrences.MarkCompletedAsync(
            schedule,
            workOrder.Id.ToString("D"),
            occurredAt,
            cancellationToken);

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == schedule.AssetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        await platformOutboxEnqueue.TryEnqueuePmOccurrenceEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.PmOccurrenceCompleted,
            schedule,
            asset,
            actorUserId,
            occurredAt,
            $"PM occurrence {schedule.ScheduleKey} completed via work order {workOrder.WorkOrderNumber} for asset {asset.AssetTag}.",
            eventResult: workOrder.Status,
            idempotencyDiscriminator: workOrder.Id.ToString("D"),
            cancellationToken: cancellationToken);

        if (!string.Equals(previousDueStatus, PmDueStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            await discussionService.RecordTimelineEventAsync(
                tenantId,
                workOrder.Id,
                "maintainarr.pm_occurrence.completed",
                occurredAt,
                null,
                null,
                $"PM occurrence {schedule.ScheduleKey} completed.",
                "maintainarr",
                schedule.Id.ToString("D"),
                null,
                JsonSerializer.Serialize(new
                {
                    schedule.Id,
                    schedule.ScheduleKey,
                    schedule.Name,
                    schedule.AssetId,
                    schedule.DueStatus,
                    schedule.LastCompletedAt,
                }),
                cancellationToken);
        }
    }

    private async Task EnsureComplianceCoreAllowsStatusTransitionAsync(
        WorkOrder workOrder,
        string toStatus,
        string? actorPersonId,
        CancellationToken cancellationToken)
    {
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(workOrder.TenantId, cancellationToken);
        if (!complianceCoreWorkOrderGateClient.IsConfigured
            || !settings.Compliance.EnableComplianceCoreChecks
            || !string.Equals(settings.Compliance.ComplianceCheckMode, "block", StringComparison.OrdinalIgnoreCase)
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
        || string.Equals(status, WorkOrderStatuses.CompletedPendingReview, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, WorkOrderStatuses.Completed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, WorkOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase);

    private static bool IsPermissiveComplianceCoreGateOutcome(string outcome) =>
        string.Equals(outcome, "allow", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "warn", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "waived", StringComparison.OrdinalIgnoreCase);

    private static bool IsFinalWorkOrderStatus(string status) =>
        string.Equals(status, WorkOrderStatuses.Completed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, WorkOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, WorkOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, WorkOrderStatuses.Canceled, StringComparison.OrdinalIgnoreCase);

    private static bool IsClosureStatus(string status) =>
        string.Equals(status, WorkOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase);

    private static void EnsureReopenAllowed(
        string fromStatus,
        string toStatus,
        MaintainArrTenantSettingsDto settings)
    {
        if (settings.WorkOrders.AllowReopenClosedWorkOrders)
        {
            return;
        }

        if (IsFinalWorkOrderStatus(fromStatus) && !IsFinalWorkOrderStatus(toStatus))
        {
            throw new StlApiException(
                "work_order.reopen_disabled",
                "Reopening closed work orders is disabled by MaintainArr tenant settings.",
                409);
        }
    }

    private async Task EnsureWorkOrderClosurePolicyAsync(
        WorkOrder workOrder,
        string toStatus,
        MaintainArrTenantSettingsDto settings,
        CancellationToken cancellationToken)
    {
        if (!IsClosureStatus(toStatus))
        {
            return;
        }

        if (settings.WorkOrders.RequireResolutionNotesBeforeClose)
        {
            var hasCloseoutSummary = await db.WorkOrderCloseouts
                .AsNoTracking()
                .AnyAsync(
                    x => x.TenantId == workOrder.TenantId
                        && x.WorkOrderId == workOrder.Id
                        && x.CompletionSummary != string.Empty,
                    cancellationToken);

            if (!hasCloseoutSummary)
            {
                throw new StlApiException(
                    "work_order.resolution_notes_required",
                    "Resolution notes are required before closing this work order.",
                    409);
            }
        }

        if (settings.Labor.EnableLaborTracking
            && (settings.WorkOrders.RequireLaborBeforeClose || settings.Labor.RequireLaborOnWorkOrderClose))
        {
            var hasLabor = await db.WorkOrderLaborEntries
                .AsNoTracking()
                .AnyAsync(
                    x => x.TenantId == workOrder.TenantId
                        && x.WorkOrderId == workOrder.Id
                        && !string.Equals(x.Status, WorkOrderLaborStatuses.Rejected, StringComparison.OrdinalIgnoreCase),
                    cancellationToken);

            if (!hasLabor)
            {
                throw new StlApiException(
                    "work_order.labor_required",
                    "Labor must be logged before closing this work order.",
                    409);
            }
        }

        if (settings.WorkOrders.RequirePartsBeforeClose)
        {
            var hasOpenPartsDemand = await db.WorkOrderPartsDemandLines
                .AsNoTracking()
                .AnyAsync(
                    x => x.TenantId == workOrder.TenantId
                        && x.WorkOrderId == workOrder.Id
                        && !string.Equals(x.Status, WorkOrderPartsDemandStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(x.ProcurementStatus, WorkOrderPartsDemandProcurementStatuses.Fulfilled, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(x.ProcurementStatus, WorkOrderPartsDemandProcurementStatuses.ReceivedComplete, StringComparison.OrdinalIgnoreCase),
                    cancellationToken);

            if (hasOpenPartsDemand)
            {
                throw new StlApiException(
                    "work_order.parts_required",
                    "Parts demand lines must be fulfilled or cancelled before closing this work order.",
                    409);
            }
        }
    }

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

        if (string.Equals(toStatus, WorkOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)
            || string.Equals(toStatus, WorkOrderStatuses.Canceled, StringComparison.OrdinalIgnoreCase)
            || string.Equals(toStatus, WorkOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.forbidden",
                "Only managers can close or cancel work orders.",
                403);
        }

        if (string.Equals(toStatus, WorkOrderStatuses.InProgress, StringComparison.OrdinalIgnoreCase)
            || string.Equals(toStatus, WorkOrderStatuses.Paused, StringComparison.OrdinalIgnoreCase)
            || string.Equals(toStatus, WorkOrderStatuses.Blocked, StringComparison.OrdinalIgnoreCase)
            || string.Equals(toStatus, WorkOrderStatuses.CompletedPendingReview, StringComparison.OrdinalIgnoreCase))
        {
            if (isCreator || isAssignee || string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId))
            {
                return;
            }

            throw new StlApiException(
                "auth.forbidden",
                "You can only move work orders into execution states when you created them, are assigned, or are unassigned.",
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
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        var prefix = string.IsNullOrWhiteSpace(settings.WorkOrders.WorkOrderNumberPrefix)
            ? "WO"
            : settings.WorkOrders.WorkOrderNumberPrefix.Trim().ToUpperInvariant();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var number = $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMdd}-{suffix}";
            var exists = await db.WorkOrders
                .AsNoTracking()
                .AnyAsync(x => x.TenantId == tenantId && x.WorkOrderNumber == number, cancellationToken);
            if (!exists)
            {
                return number;
            }
        }

        return $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..Math.Min(64, prefix.Length + 42)];
    }

    private async Task<Asset> EnsureActiveAssetAsync(Guid tenantId, Guid assetId, CancellationToken cancellationToken)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);

        if (asset is null)
        {
            throw new StlApiException("asset.not_found", "Asset was not found.", 404);
        }

        var assetStatus = await db.AssetStatusHistory
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId && x.StatusFieldKey == "assetStatus")
            .OrderByDescending(x => x.ChangedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => x.StatusValueKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.Equals(assetStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            return asset;
        }

        var lifecycleIsActive = string.Equals(asset.LifecycleStatus, "active", StringComparison.OrdinalIgnoreCase)
            || string.Equals(asset.LifecycleStatus, "in_service", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(assetStatus) && lifecycleIsActive)
        {
            return asset;
        }

        throw new StlApiException(
            "asset.not_active",
            "Work orders can only be created for active assets.",
            400);
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
                    workOrder.TemplateRef,
                    workOrder.Title,
                    workOrder.Priority,
                    workOrder.Status,
                    workOrder.Source,
                    "maintainarr",
                    workOrder.Id.ToString("D"),
                    workOrder.WorkOrderType,
                    workOrder.OriginType,
                    workOrder.OriginRef,
                    asset?.StaffarrSiteOrgUnitId,
                    workOrder.StaffarrLocationId,
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
        var asset = await db.Assets
            .AsNoTracking()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == workOrder.AssetId, cancellationToken);

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

        var blockers = await db.WorkOrderBlockers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrder.Id)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new WorkOrderBlockerResponse(
                x.Id,
                x.WorkOrderId,
                x.BlockerType,
                x.SourceProduct,
                x.SourceObjectRef,
                x.Title,
                x.Description,
                x.Severity,
                x.Status,
                x.RequiredAction,
                x.CreatedAt,
                x.CreatedByPersonId,
                x.ResolvedAt,
                x.ResolvedByPersonId,
                x.OverrideReason))
            .ToListAsync(cancellationToken);

        var closeout = await db.WorkOrderCloseouts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkOrderId == workOrder.Id, cancellationToken);

        var permitRefs = await db.MaintenancePermitRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrder.Id)
            .OrderByDescending(x => x.ValidFrom)
            .ThenByDescending(x => x.Id)
            .Select(x => new MaintenancePermitRefResponse(
                x.Id,
                x.WorkOrderId,
                x.PermitType,
                x.SourceProduct,
                x.SourceObjectRef,
                x.RecordRef,
                x.StatusSnapshot,
                x.ApprovedByPersonId,
                x.ValidFrom,
                x.ValidTo))
            .ToListAsync(cancellationToken);

        var returnToService = await db.ReturnToServices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkOrderId == workOrder.Id, cancellationToken);

        var vendorWorkRefs = await db.MaintenanceVendorWorks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrder.Id)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var technicianAssignments = await db.WorkOrderTechnicianAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrder.Id)
            .OrderByDescending(x => x.AssignedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new WorkOrderTechnicianAssignmentResponse(
                x.Id,
                x.WorkOrderId,
                x.PersonId,
                x.AssignmentRole,
                x.Status,
                x.AssignedAt,
                x.AssignedByPersonId,
                x.AcceptedAt,
                x.CompletedAt,
                DeserializeStringList(x.RequiredQualificationRefsJson),
                DeserializeQualificationCheckResults(x.QualificationCheckSnapshotJson)))
            .ToListAsync(cancellationToken);

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
            workOrder.TemplateRef,
            workOrder.Title,
            workOrder.Description,
            workOrder.Priority,
            workOrder.Status,
            workOrder.Source,
            "maintainarr",
            workOrder.Id.ToString("D"),
            workOrder.WorkOrderType,
            workOrder.OriginType,
            workOrder.OriginRef,
            asset.StaffarrSiteOrgUnitId,
            workOrder.StaffarrLocationId,
            string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId)
                ? Array.Empty<string>()
                : [workOrder.AssignedTechnicianPersonId],
            null,
            DeserializeStringList(workOrder.RequiredQualificationRefsJson),
            DeserializeQualificationCheckResults(workOrder.QualificationCheckResultsJson),
            technicianAssignments,
            permitRefs,
            returnToService is null ? null : MapReturnToServiceResponse(returnToService),
            vendorWorkRefs,
            workOrder.AssignedTechnicianPersonId,
            workOrder.CreatedByUserId,
            workOrder.CreatedAt,
            workOrder.UpdatedAt,
            workOrder.StartedAt,
            workOrder.CompletedAt,
            workOrder.CancelledAt,
            workOrder.DraftPlanJson,
            workOrder.PlannedStartAt,
            workOrder.PlannedDueAt,
            downtimeFollowUp,
            blockers,
            closeout is null ? null : MapCloseoutResponse(closeout));
    }

    private static WorkOrderBlockerResponse MapBlockerResponse(WorkOrderBlocker blocker) =>
        new(
            blocker.Id,
            blocker.WorkOrderId,
            blocker.BlockerType,
            blocker.SourceProduct,
            blocker.SourceObjectRef,
            blocker.Title,
            blocker.Description,
            blocker.Severity,
            blocker.Status,
            blocker.RequiredAction,
            blocker.CreatedAt,
            blocker.CreatedByPersonId,
            blocker.ResolvedAt,
            blocker.ResolvedByPersonId,
            blocker.OverrideReason);

    private static WorkOrderCloseoutResponse MapCloseoutResponse(WorkOrderCloseout closeout) =>
        new(
            closeout.Id,
            closeout.WorkOrderId,
            closeout.CompletionSummary,
            closeout.RootCause,
            closeout.CorrectiveAction,
            closeout.PreventiveActionRecommendation,
            closeout.AssetReturnedToService,
            closeout.ReturnToServiceAt,
            closeout.ReturnToServiceByPersonId,
            closeout.PostRepairInspectionRequired,
            closeout.PostRepairInspectionRef,
            closeout.SupervisorReviewRequired,
            closeout.SupervisorReviewedByPersonId,
            closeout.SupervisorReviewedAt,
            closeout.ComplianceReviewRequired,
            closeout.ComplianceReviewedByPersonId,
            closeout.ComplianceReviewedAt,
            closeout.QualityReviewRequired,
            closeout.QualityReviewedByPersonId,
            closeout.QualityReviewedAt,
            closeout.EvidenceAccepted,
            closeout.UnresolvedDefectRefs,
            closeout.FollowUpWorkOrderRefs,
            closeout.CustomerImpactSummary,
            closeout.DowntimeSummary,
            closeout.FinalAssetReadinessStatus,
            closeout.FinalStatus,
            DeserializeGuidList(closeout.PermitRecordRefsJson),
            DeserializeGuidList(closeout.EvidenceRecordRefsJson),
            closeout.CreatedAt,
            closeout.CreatedByPersonId);

    private static ReturnToServiceResponse MapReturnToServiceResponse(ReturnToService returnToService) =>
        new(
            returnToService.Id,
            returnToService.WorkOrderId,
            returnToService.AssetId,
            returnToService.Status,
            DeserializeStringList(returnToService.RequiredChecksJson),
            DeserializeStringList(returnToService.CompletedChecksJson),
            returnToService.FinalInspectionRef,
            returnToService.ApprovedByPersonId,
            returnToService.ApprovedAt,
            returnToService.RejectionReason,
            returnToService.FinalReadinessStatus,
            DeserializeGuidList(returnToService.RecordRefsJson));

    private static string SerializeWorkOrderSnapshot(WorkOrder workOrder) =>
        JsonSerializer.Serialize(new
        {
            workOrder.Id,
            workOrder.WorkOrderNumber,
            workOrder.AssetId,
            workOrder.DefectId,
            workOrder.PmScheduleId,
            workOrder.TemplateRef,
            workOrder.Title,
            workOrder.Description,
            workOrder.Priority,
            workOrder.Status,
            workOrder.Source,
            workOrder.WorkOrderType,
            workOrder.OriginType,
            workOrder.OriginRef,
            workOrder.StaffarrLocationId,
            workOrder.AssignedTechnicianPersonId,
            RequiredQualificationRefs = DeserializeStringList(workOrder.RequiredQualificationRefsJson),
            QualificationCheckResults = DeserializeQualificationCheckResults(workOrder.QualificationCheckResultsJson),
            workOrder.CreatedAt,
            workOrder.UpdatedAt,
            workOrder.StartedAt,
            workOrder.CompletedAt,
            workOrder.CancelledAt,
        });

    private static string SerializeBlockerSnapshot(WorkOrderBlocker blocker) =>
        JsonSerializer.Serialize(new
        {
            blocker.Id,
            blocker.WorkOrderId,
            blocker.BlockerType,
            blocker.SourceProduct,
            blocker.SourceObjectRef,
            blocker.Title,
            blocker.Description,
            blocker.Severity,
            blocker.Status,
            blocker.RequiredAction,
            blocker.CreatedAt,
            blocker.CreatedByPersonId,
            blocker.ResolvedAt,
            blocker.ResolvedByPersonId,
            blocker.OverrideReason,
        });

    private static string SerializeCloseoutSnapshot(WorkOrderCloseout closeout) =>
        JsonSerializer.Serialize(new
        {
            closeout.Id,
            closeout.WorkOrderId,
            closeout.CompletionSummary,
            closeout.RootCause,
            closeout.CorrectiveAction,
            closeout.PreventiveActionRecommendation,
            closeout.AssetReturnedToService,
            closeout.ReturnToServiceAt,
            closeout.ReturnToServiceByPersonId,
            closeout.PostRepairInspectionRequired,
            closeout.PostRepairInspectionRef,
            closeout.SupervisorReviewRequired,
            closeout.SupervisorReviewedByPersonId,
            closeout.SupervisorReviewedAt,
            closeout.ComplianceReviewRequired,
            closeout.ComplianceReviewedByPersonId,
            closeout.ComplianceReviewedAt,
            closeout.QualityReviewRequired,
            closeout.QualityReviewedByPersonId,
            closeout.QualityReviewedAt,
            closeout.EvidenceAccepted,
            closeout.UnresolvedDefectRefs,
            closeout.FollowUpWorkOrderRefs,
            closeout.CustomerImpactSummary,
            closeout.DowntimeSummary,
            closeout.FinalAssetReadinessStatus,
            closeout.FinalStatus,
            PermitRecordRefs = DeserializeGuidList(closeout.PermitRecordRefsJson),
            EvidenceRecordRefs = DeserializeGuidList(closeout.EvidenceRecordRefsJson),
        });

    private static IReadOnlyList<Guid> NormalizeGuidList(IEnumerable<Guid>? refs) =>
        refs?.Distinct().ToArray() ?? Array.Empty<Guid>();

    private static IReadOnlyList<Guid> DeserializeGuidList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<Guid>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json)?.ToArray() ?? Array.Empty<Guid>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid serialized GUID list.", ex);
        }
    }

    private static string SerializeGuidList(IReadOnlyList<Guid> refs) =>
        JsonSerializer.Serialize(refs);

    private async Task UpsertReturnToServiceAsync(
        Guid tenantId,
        WorkOrder workOrder,
        WorkOrderCloseout closeout,
        IReadOnlyList<Guid> permitRecordRefs,
        IReadOnlyList<Guid> evidenceRecordRefs,
        CancellationToken cancellationToken)
    {
        var entity = await db.ReturnToServices
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkOrderId == workOrder.Id, cancellationToken);

        if (entity is null)
        {
            entity = new ReturnToService
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkOrderId = workOrder.Id,
                AssetId = workOrder.AssetId,
            };
            db.ReturnToServices.Add(entity);
        }

        entity.AssetId = workOrder.AssetId;
        entity.Status = DetermineReturnToServiceStatus(closeout);
        entity.RequiredChecksJson = SerializeStringList(BuildRequiredReturnToServiceChecks(closeout));
        entity.CompletedChecksJson = SerializeStringList(BuildCompletedReturnToServiceChecks(closeout));
        entity.FinalInspectionRef = closeout.PostRepairInspectionRef;
        entity.ApprovedByPersonId = closeout.ReturnToServiceByPersonId;
        entity.ApprovedAt = closeout.ReturnToServiceAt;
        entity.RejectionReason = !closeout.AssetReturnedToService && string.Equals(entity.Status, ReturnToServiceStatuses.Rejected, StringComparison.OrdinalIgnoreCase)
            ? closeout.CompletionSummary
            : null;
        entity.FinalReadinessStatus = closeout.FinalAssetReadinessStatus;
        entity.RecordRefsJson = SerializeGuidList(permitRecordRefs.Concat(evidenceRecordRefs).ToArray());
    }

    private async Task UpsertPermitRefsAsync(
        Guid tenantId,
        WorkOrder workOrder,
        WorkOrderCloseout closeout,
        IReadOnlyList<Guid> permitRecordRefs,
        CancellationToken cancellationToken)
    {
        var existing = await db.MaintenancePermitRefs
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrder.Id)
            .ToListAsync(cancellationToken);

        var desiredRefs = new HashSet<Guid>(permitRecordRefs);
        var sourceObjectRef = workOrder.Id.ToString("D");
        foreach (var permitRef in existing.Where(x =>
            !string.IsNullOrWhiteSpace(x.RecordRef)
            && Guid.TryParse(x.RecordRef, out var parsed)
            && !desiredRefs.Contains(parsed)).ToList())
        {
            db.MaintenancePermitRefs.Remove(permitRef);
        }

        foreach (var permitRecordRef in permitRecordRefs)
        {
            var entity = existing.FirstOrDefault(x => string.Equals(x.RecordRef, permitRecordRef.ToString("D"), StringComparison.OrdinalIgnoreCase));
            if (entity is null)
            {
                entity = new MaintenancePermitRef
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WorkOrderId = workOrder.Id,
                };
                db.MaintenancePermitRefs.Add(entity);
            }

            entity.PermitType = MaintenancePermitTypes.Other;
            entity.SourceProduct = "maintainarr";
            entity.SourceObjectRef = sourceObjectRef;
            entity.RecordRef = permitRecordRef.ToString("D");
            entity.StatusSnapshot = closeout.FinalStatus;
            entity.ApprovedByPersonId = closeout.ReturnToServiceByPersonId ?? closeout.SupervisorReviewedByPersonId ?? closeout.ComplianceReviewedByPersonId ?? closeout.QualityReviewedByPersonId;
            entity.ValidFrom = closeout.ReturnToServiceAt;
            entity.ValidTo = null;
        }
    }

    private async Task EnsureReturnToServicePolicyAsync(
        MaintainArrTenantSettingsDto settings,
        WorkOrder workOrder,
        WorkOrderCloseout closeout,
        CancellationToken cancellationToken)
    {
        if (!settings.OutOfService.EnableOutOfServiceStatus || !closeout.AssetReturnedToService)
        {
            return;
        }

        if ((settings.OutOfService.RequireSupervisorApprovalForRTS
                || settings.Evidence.RequireSupervisorSignatureOnRTS)
            && (string.IsNullOrWhiteSpace(closeout.SupervisorReviewedByPersonId)
                || closeout.SupervisorReviewedAt is null))
        {
            throw new StlApiException(
                "work_order.rts_supervisor_required",
                "Supervisor approval is required before returning the asset to service.",
                409);
        }

        if (settings.OutOfService.RequireInspectionBeforeRTS && closeout.PostRepairInspectionRef is null)
        {
            throw new StlApiException(
                "work_order.rts_inspection_required",
                "Post-repair inspection is required before returning the asset to service.",
                409);
        }

        var openDefects = await db.Defects
            .AsNoTracking()
            .Where(x =>
                x.TenantId == workOrder.TenantId
                && x.AssetId == workOrder.AssetId
                && !string.Equals(x.Status, DefectStatuses.Resolved, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.Status, DefectStatuses.Closed, StringComparison.OrdinalIgnoreCase))
            .Select(x => new { x.Id, x.Severity })
            .ToListAsync(cancellationToken);

        if (settings.OutOfService.RequireAllCriticalDefectsClosedBeforeRTS
            && openDefects.Any(x => string.Equals(x.Severity, DefectSeverities.Critical, StringComparison.OrdinalIgnoreCase)))
        {
            throw new StlApiException(
                "work_order.rts_critical_defects_open",
                "Critical defects must be resolved or closed before returning the asset to service.",
                409);
        }

        if (!settings.OutOfService.AllowRTSWithOpenMinorDefects && openDefects.Count > 0)
        {
            throw new StlApiException(
                "work_order.rts_open_defects",
                "Open defects must be resolved or closed before returning the asset to service.",
                409);
        }
    }

    private static string DetermineReturnToServiceStatus(WorkOrderCloseout closeout)
    {
        if (closeout.AssetReturnedToService)
        {
            return ReturnToServiceStatuses.Approved;
        }

        if (string.Equals(closeout.FinalStatus, WorkOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            return ReturnToServiceStatuses.NotRequired;
        }

        if (string.Equals(closeout.FinalStatus, WorkOrderStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            return ReturnToServiceStatuses.Rejected;
        }

        return ReturnToServiceStatuses.Pending;
    }

    private static IReadOnlyList<string> BuildRequiredReturnToServiceChecks(WorkOrderCloseout closeout)
    {
        var checks = new List<string>();
        if (closeout.PostRepairInspectionRequired)
        {
            checks.Add("post_repair_inspection");
        }

        if (closeout.SupervisorReviewRequired)
        {
            checks.Add("supervisor_review");
        }

        if (closeout.ComplianceReviewRequired)
        {
            checks.Add("compliance_review");
        }

        if (closeout.QualityReviewRequired)
        {
            checks.Add("quality_review");
        }

        return checks;
    }

    private static IReadOnlyList<string> BuildCompletedReturnToServiceChecks(WorkOrderCloseout closeout)
    {
        var checks = new List<string>();
        if (closeout.PostRepairInspectionRef.HasValue)
        {
            checks.Add("post_repair_inspection");
        }

        if (!string.IsNullOrWhiteSpace(closeout.SupervisorReviewedByPersonId) || closeout.SupervisorReviewedAt.HasValue)
        {
            checks.Add("supervisor_review");
        }

        if (!string.IsNullOrWhiteSpace(closeout.ComplianceReviewedByPersonId) || closeout.ComplianceReviewedAt.HasValue)
        {
            checks.Add("compliance_review");
        }

        if (!string.IsNullOrWhiteSpace(closeout.QualityReviewedByPersonId) || closeout.QualityReviewedAt.HasValue)
        {
            checks.Add("quality_review");
        }

        return checks;
    }

    private static string SerializeStringList(IReadOnlyList<string> values) =>
        JsonSerializer.Serialize(values);

    private async Task EnsureCloseoutEvidenceRecordRefsAsync(
        Guid tenantId,
        Guid workOrderId,
        IReadOnlyList<Guid> evidenceRecordRefs,
        CancellationToken cancellationToken)
    {
        if (evidenceRecordRefs.Count == 0)
        {
            return;
        }

        var validRefs = await db.WorkOrderEvidence
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId && evidenceRecordRefs.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (validRefs.Count != evidenceRecordRefs.Count)
        {
            throw new StlApiException(
                "work_order.closeout_evidence_invalid",
                "All closeout evidence references must belong to the same work order.",
                400);
        }
    }

    private static string GetStatusTimelineEventType(string status) =>
        status.ToLowerInvariant() switch
        {
            WorkOrderStatuses.Requested or WorkOrderStatuses.Open => "maintainarr.work_order.requested",
            WorkOrderStatuses.Triage => "maintainarr.work_order.triaged",
            WorkOrderStatuses.Approved => "maintainarr.work_order.approved",
            WorkOrderStatuses.Rejected => "maintainarr.work_order.rejected",
            WorkOrderStatuses.Planned => "maintainarr.work_order.planned",
            WorkOrderStatuses.Scheduled => "maintainarr.work_order.scheduled",
            WorkOrderStatuses.Assigned => "maintainarr.work_order.assigned",
            WorkOrderStatuses.InProgress => "maintainarr.work_order.started",
            WorkOrderStatuses.Paused => "maintainarr.work_order.paused",
            WorkOrderStatuses.Blocked => "maintainarr.work_order.blocked",
            WorkOrderStatuses.CompletedPendingReview => "maintainarr.work_order.completed",
            WorkOrderStatuses.Completed => "maintainarr.work_order.completed",
            WorkOrderStatuses.Closed => "maintainarr.work_order.closed",
            WorkOrderStatuses.Cancelled => "maintainarr.work_order.canceled",
            WorkOrderStatuses.Canceled => "maintainarr.work_order.canceled",
            _ => "maintainarr.work_order.updated",
        };

    private static string NormalizeCloseoutFinalStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        return normalized switch
        {
            "review_pending" => WorkOrderStatuses.CompletedPendingReview,
            "cancelled" => WorkOrderStatuses.Canceled,
            _ => normalized,
        };
    }

    private async Task EnqueueWorkOrderBlockerEventAsync(
        Guid tenantId,
        Guid actorUserId,
        WorkOrder workOrder,
        WorkOrderBlocker blocker,
        string? previousStatus,
        string currentStatus,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var previousIsActive = string.Equals(previousStatus, "active", StringComparison.OrdinalIgnoreCase);
        var currentIsActive = string.Equals(currentStatus, "active", StringComparison.OrdinalIgnoreCase);

        if (previousIsActive == currentIsActive)
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

        var eventKind = currentIsActive
            ? MaintenancePlatformOutboxEventKinds.WorkOrderBlocked
            : MaintenancePlatformOutboxEventKinds.WorkOrderUnblocked;

        var summary = currentIsActive
            ? $"Work order {workOrder.WorkOrderNumber} blocked by {blocker.Title}."
            : $"Work order {workOrder.WorkOrderNumber} unblocked by {blocker.Title}.";

        await platformOutboxEnqueue.TryEnqueueWorkOrderEventAsync(
            tenantId,
            eventKind,
            workOrder,
            asset,
            actorUserId,
            occurredAt,
            summary,
            eventResult: currentStatus,
            idempotencyDiscriminator: $"{blocker.Id:D}:{currentStatus.ToLowerInvariant()}",
            cancellationToken: cancellationToken);
    }

    private async Task SyncLinkedDefectStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid defectId,
        string targetDefectStatus,
        CancellationToken cancellationToken)
    {
        var defect = await db.Defects.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == defectId,
            cancellationToken);

        if (defect is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetDefectStatus)
            || string.Equals(defect.Status, targetDefectStatus, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        defect.Status = targetDefectStatus;
        defect.UpdatedAt = now;
        if (string.Equals(targetDefectStatus, DefectStatuses.Resolved, StringComparison.OrdinalIgnoreCase)
            || string.Equals(targetDefectStatus, DefectStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            defect.ResolvedAt ??= now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "defect.status_update_from_work_order",
            tenantId,
            actorUserId,
            "defect",
            defect.Id.ToString(),
            defect.Status,
            cancellationToken: cancellationToken);

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == defect.AssetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        var eventKind = defect.Status switch
        {
            DefectStatuses.InRepair => null,
            DefectStatuses.Resolved => MaintenancePlatformOutboxEventKinds.DefectRepaired,
            DefectStatuses.Closed => MaintenancePlatformOutboxEventKinds.DefectClosed,
            _ => null,
        };

        if (eventKind is not null)
        {
            await platformOutboxEnqueue.TryEnqueueDefectEventAsync(
                tenantId,
                eventKind,
                defect,
                asset,
                actorUserId,
                now,
                $"Defect {defect.Title} changed to {defect.Status} for asset {asset.AssetTag}.",
                eventResult: defect.Status,
                cancellationToken: cancellationToken);
        }
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
        var normalized = NormalizePriority(priority);
        if (!WorkOrderPriorities.All.Contains(normalized))
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

    private static string ResolveRequestedPriority(string? priority, MaintainArrTenantSettingsDto settings) =>
        string.IsNullOrWhiteSpace(priority)
            ? settings.WorkOrders.DefaultPriority
            : priority.Trim();

    private static void EnsureAssetPolicy(Guid assetId, MaintainArrTenantSettingsDto settings)
    {
        if (settings.WorkOrders.RequireAssetOnWorkOrder && assetId == Guid.Empty)
        {
            throw new StlApiException(
                "work_order.asset_required",
                "An asset is required by MaintainArr tenant settings.",
                400);
        }
    }

    private static void EnsureAssignedTechnicianPolicy(string? assignedTechnicianPersonId, MaintainArrTenantSettingsDto settings)
    {
        if (!settings.WorkOrders.AllowUnassignedWorkOrders && string.IsNullOrWhiteSpace(assignedTechnicianPersonId))
        {
            throw new StlApiException(
                "work_order.assigned_technician_required",
                "Assigned technician is required by MaintainArr tenant settings.",
                400);
        }
    }

    private static string NormalizePriority(string priority)
    {
        var normalized = priority.Trim().ToLowerInvariant();
        return normalized switch
        {
            "normal" => WorkOrderPriorities.Medium,
            "emergency" => WorkOrderPriorities.Urgent,
            _ => normalized,
        };
    }

    private static string NormalizeRequiredValue(string? value, string code)
    {
        var normalized = NormalizeOptionalValue(value);
        if (normalized is null)
        {
            throw new StlApiException(code, "A value is required.", 400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static void ValidateBlockerType(string blockerType)
    {
        if (!WorkOrderBlockerTypes.All.Contains(blockerType))
        {
            throw new StlApiException(
                "work_order.invalid_blocker_type",
                "Blocker type is not recognized.",
                400);
        }
    }

    private static void ValidateSourceProduct(string sourceProduct)
    {
        if (sourceProduct.Length > 64)
        {
            throw new StlApiException(
                "work_order.blocker_source_product_too_long",
                "Blocker source product must be 64 characters or fewer.",
                400);
        }
    }

    private static void ValidateBlockerTitle(string title)
    {
        if (title.Length > 256)
        {
            throw new StlApiException(
                "work_order.blocker_title_too_long",
                "Blocker title must be 256 characters or fewer.",
                400);
        }
    }

    private static void ValidateBlockerDescription(string description)
    {
        if (description.Length > 1024)
        {
            throw new StlApiException(
                "work_order.blocker_description_too_long",
                "Blocker description must be 1024 characters or fewer.",
                400);
        }
    }

    private static void ValidateBlockerSeverity(string severity)
    {
        if (!WorkOrderBlockerSeverities.All.Contains(severity))
        {
            throw new StlApiException(
                "work_order.invalid_blocker_severity",
                "Blocker severity must be low, moderate, high, or critical.",
                400);
        }
    }

    private static void ValidateBlockerStatus(string status)
    {
        if (!WorkOrderBlockerStatuses.All.Contains(status))
        {
            throw new StlApiException(
                "work_order.invalid_blocker_status",
                "Blocker status must be active, resolved, overridden, or canceled.",
                400);
        }
    }

    private static void ValidateBlockerRequiredAction(string? requiredAction)
    {
        if (requiredAction is not null && requiredAction.Length > 512)
        {
            throw new StlApiException(
                "work_order.blocker_required_action_too_long",
                "Blocker required action must be 512 characters or fewer.",
                400);
        }
    }

    private static void ValidateCloseoutCompletionSummary(string summary)
    {
        if (summary.Length > 1024)
        {
            throw new StlApiException(
                "work_order.closeout_summary_too_long",
                "Closeout completion summary must be 1024 characters or fewer.",
                400);
        }
    }

    private static void ValidateCloseoutRootCause(string? rootCause)
    {
        if (rootCause is null)
        {
            return;
        }

        if (!WorkOrderCloseoutRootCauses.All.Contains(rootCause))
        {
            throw new StlApiException(
                "work_order.invalid_closeout_root_cause",
                "Closeout root cause is not recognized.",
                400);
        }
    }

    private static void ValidateCloseoutCorrectiveAction(string? correctiveAction)
    {
        if (correctiveAction is not null && correctiveAction.Length > 1024)
        {
            throw new StlApiException(
                "work_order.closeout_corrective_action_too_long",
                "Closeout corrective action must be 1024 characters or fewer.",
                400);
        }
    }

    private static void ValidateCloseoutPreventiveAction(string? preventiveActionRecommendation)
    {
        if (preventiveActionRecommendation is not null && preventiveActionRecommendation.Length > 1024)
        {
            throw new StlApiException(
                "work_order.closeout_preventive_action_too_long",
                "Closeout preventive action recommendation must be 1024 characters or fewer.",
                400);
        }
    }

    private static void ValidateOptionalPersonId(string? personId)
    {
        if (personId is null)
        {
            return;
        }

        if (personId.Length > 128)
        {
            throw new StlApiException(
                "work_order.person_id_too_long",
                "Person id must be 128 characters or fewer.",
                400);
        }
    }

    private static string? NormalizeAssignedPersonId(string? personId)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return null;
        }

        return personId.Trim();
    }

    private async Task<WorkOrderQualificationCheckResultResponse?> CheckAssignedTechnicianQualificationAsync(
        Guid tenantId,
        string? assignedTechnicianPersonId,
        MaintainArrTenantSettingsDto settings,
        CancellationToken cancellationToken)
    {
        var check = await GetAssignedTechnicianQualificationCheckResultAsync(
            tenantId,
            assignedTechnicianPersonId,
            settings,
            cancellationToken);

        if (check is null || IsPermissiveQualificationOutcome(check.Outcome))
        {
            return check;
        }

        throw new StlApiException(
            "work_order.technician_qualification_blocked",
            $"TrainArr technician qualification check returned {check.Outcome}: {check.Message}",
            409);
    }

    private async Task<WorkOrderQualificationCheckResultResponse?> GetAssignedTechnicianQualificationCheckResultAsync(
        Guid tenantId,
        string? assignedTechnicianPersonId,
        MaintainArrTenantSettingsDto settings,
        CancellationToken cancellationToken)
    {
        if (!settings.Integrations.EnableTrainArrQualificationChecks
            || !settings.Scheduling.RespectTrainArrQualifications
            || !trainArrQualificationCheckClient.IsConfigured
            || string.IsNullOrWhiteSpace(assignedTechnicianPersonId))
        {
            return null;
        }

        if (!Guid.TryParse(assignedTechnicianPersonId.Trim(), out var staffarrPersonId))
        {
            return new WorkOrderQualificationCheckResultResponse(
                null,
                assignedTechnicianPersonId.Trim(),
                trainArrQualificationCheckClient.TechnicianQualificationKey,
                "invalid",
                "invalid_person_id",
                "Assigned technician person id must be a StaffArr person GUID when TrainArr qualification checks are enabled.");
        }

        var check = await trainArrQualificationCheckClient.CheckTechnicianAsync(
            tenantId,
            staffarrPersonId,
            cancellationToken);

        return check is null
            ? null
            : new WorkOrderQualificationCheckResultResponse(
                check.CheckId,
                staffarrPersonId.ToString("D"),
                check.QualificationKey,
                check.Outcome,
                check.ReasonCode,
                check.Message);
    }

    private async Task<WorkOrderDetailResponse> UpsertDraftAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid? workOrderId,
        CreateWorkOrderRequest request,
        CancellationToken cancellationToken)
    {
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        EnsureAssetPolicy(request.AssetId, settings);
        var asset = await EnsureActiveAssetAsync(tenantId, request.AssetId, cancellationToken);
        var draftPlanJson = NormalizeDraftPlanJson(request.DraftPlanJson);
        var assignedTechnicianPersonId = NormalizeAssignedPersonId(request.AssignedTechnicianPersonId);
        var title = request.Title?.Trim() ?? string.Empty;
        var description = request.Description?.Trim() ?? string.Empty;
        var priority = string.IsNullOrWhiteSpace(request.Priority) ? string.Empty : NormalizePriority(request.Priority);

        if (request.DefectId.HasValue && request.PmScheduleId.HasValue)
        {
            throw new StlApiException(
                "work_order.multiple_sources",
                "Choose either a defect or a PM schedule as the work order source, not both.",
                400);
        }

        if (request.DefectId.HasValue)
        {
            var defect = await db.Defects
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Id == request.DefectId.Value,
                    cancellationToken);

            if (defect is null)
            {
                throw new StlApiException("defect.not_found", "Defect was not found.", 404);
            }

            if (defect.AssetId != request.AssetId)
            {
                throw new StlApiException(
                    "work_order.defect_asset_mismatch",
                    "Defect does not belong to the selected asset.",
                    400);
            }

            if (string.Equals(defect.Status, DefectStatuses.Closed, StringComparison.OrdinalIgnoreCase)
                || string.Equals(defect.Status, DefectStatuses.Resolved, StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "work_order.defect_closed",
                    "Work orders cannot be created from resolved or closed defects.",
                    400);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = $"Repair: {defect.Title}";
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                description = defect.Description;
            }

            if (string.IsNullOrWhiteSpace(priority))
            {
                priority = MapDefectSeverityToPriority(defect.Severity);
            }
        }
        else if (request.PmScheduleId.HasValue)
        {
            var schedule = await db.PmSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Id == request.PmScheduleId.Value,
                    cancellationToken);

            if (schedule is null)
            {
                throw new StlApiException("pm_schedule.not_found", "PM schedule was not found.", 404);
            }

            if (schedule.AssetId != request.AssetId)
            {
                throw new StlApiException(
                    "work_order.pm_schedule_asset_mismatch",
                    "PM schedule does not belong to the selected asset.",
                    400);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = PmWorkOrderGenerationRules.BuildTitle(schedule.Name);
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                description = PmWorkOrderGenerationRules.BuildDescription(
                    schedule.Name,
                    schedule.Description,
                    schedule.NextDueAt);
            }

            if (string.IsNullOrWhiteSpace(priority))
            {
                priority = PmWorkOrderGenerationRules.MapDueStatusToPriority(schedule.DueStatus);
            }
        }

        if (string.IsNullOrWhiteSpace(priority))
        {
            priority = NormalizePriority(settings.WorkOrders.DefaultPriority);
        }

        var workOrderType = DetermineDraftWorkOrderType(draftPlanJson, request.DefectId, request.PmScheduleId);
        var source = ResolveSource(request);
        var originType = ResolveOriginType(request);
        var originRef = ResolveOriginRef(request);
        var qualificationSnapshot = await GetAssignedTechnicianQualificationCheckResultAsync(
            tenantId,
            assignedTechnicianPersonId,
            settings,
            cancellationToken);

        WorkOrder workOrder;
        if (workOrderId.HasValue)
        {
            workOrder = await db.WorkOrders
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Id == workOrderId.Value,
                    cancellationToken)
                ?? throw new StlApiException("work_order.not_found", "Work order was not found.", 404);

            if (!string.Equals(workOrder.Status, WorkOrderStatuses.Draft, StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "work_order.not_draft",
                    "Only draft work orders can be updated in the create wizard.",
                    409);
            }
        }
        else
        {
            workOrder = new WorkOrder
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkOrderNumber = await GenerateWorkOrderNumberAsync(tenantId, cancellationToken),
                CreatedByUserId = actorUserId,
                CreatedAt = now,
                Status = WorkOrderStatuses.Draft,
                RequiredQualificationRefsJson = "[]",
                QualificationCheckResultsJson = "[]",
            };
            db.WorkOrders.Add(workOrder);
        }

        workOrder.AssetId = request.AssetId;
        workOrder.DefectId = request.DefectId;
        workOrder.PmScheduleId = request.PmScheduleId;
        workOrder.Title = title;
        workOrder.Description = description;
        workOrder.Priority = priority;
        workOrder.Source = source;
        workOrder.WorkOrderType = workOrderType;
        workOrder.OriginType = originType;
        workOrder.OriginRef = originRef;
        workOrder.StaffarrLocationId = NormalizeLocationRef(asset.SiteRef);
        workOrder.DraftPlanJson = draftPlanJson;
        workOrder.PlannedStartAt = request.PlannedStartAt;
        workOrder.PlannedDueAt = request.PlannedDueAt;
        workOrder.AssignedTechnicianPersonId = assignedTechnicianPersonId;
        workOrder.RequiredQualificationRefsJson = SerializeStringList(
            qualificationSnapshot is null ? [] : [qualificationSnapshot.QualificationKey]);
        workOrder.QualificationCheckResultsJson = SerializeQualificationCheckResults(qualificationSnapshot);
        workOrder.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId))
        {
            await MirrorAssignedTechnicianAsync(
                tenantId,
                actorUserId,
                workOrder.AssignedTechnicianPersonId,
                cancellationToken);

            await UpsertTechnicianAssignmentAsync(
                tenantId,
                actorUserId,
                workOrder.Id,
                workOrder.AssignedTechnicianPersonId,
                qualificationSnapshot,
                now,
                cancellationToken);
        }

        return await MapDetailAsync(tenantId, workOrder, cancellationToken);
    }

    private async Task<WorkOrder> GetDraftWorkOrderAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders
            .AsNoTracking()
            .Include(x => x.Asset)
                .ThenInclude(x => x.AssetType)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken);

        if (workOrder is null)
        {
            throw new StlApiException("work_order.not_found", "Work order was not found.", 404);
        }

        if (!string.Equals(workOrder.Status, WorkOrderStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "work_order.not_draft",
                "Only draft work orders can be edited in the create wizard.",
                409);
        }

        return workOrder;
    }

    private async Task<WorkOrderValidationResponse> ValidateDraftEntityAsync(
        Guid tenantId,
        WorkOrder workOrder,
        CancellationToken cancellationToken)
    {
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        var findings = new List<WorkOrderFindingResponse>();

        ValidateTitleIfNeeded(findings, workOrder.Title);
        ValidatePriorityIfNeeded(findings, workOrder.Priority);
        if (!settings.WorkOrders.AllowUnassignedWorkOrders
            && string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId))
        {
            AddValidationFindingAsync(
                findings,
                "assignment",
                "blocker",
                "work_order.assigned_technician_required",
                "Assigned technician is required by MaintainArr tenant settings.",
                fieldKey: "assignedTechnicianPersonId",
                sectionKey: "assignment",
                source: "maintainarr");
        }

        if (string.IsNullOrWhiteSpace(GetDraftPlanValue(workOrder.DraftPlanJson, "workOrderType")))
        {
            AddValidationFindingAsync(
                findings,
                "basics",
                "blocker",
                "work_order.type_required",
                "Work order type is required.",
                fieldKey: "workOrderType",
                sectionKey: "basics",
                source: "maintainarr");
        }

        var scopeSummary = GetDraftPlanValue(workOrder.DraftPlanJson, "scopeSummary");
        if (!string.IsNullOrWhiteSpace(scopeSummary) && scopeSummary.Length > 1024)
        {
            AddValidationFindingAsync(
                findings,
                "scope",
                "blocker",
                "work_order.scope_summary_too_long",
                "Scope summary must be 1024 characters or fewer.",
                fieldKey: "scopeSummary",
                sectionKey: "scope",
                source: "maintainarr");
        }

        var notes = GetDraftPlanValue(workOrder.DraftPlanJson, "notes");
        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > 2048)
        {
            AddValidationFindingAsync(
                findings,
                "documents",
                "blocker",
                "work_order.notes_too_long",
                "Notes must be 2048 characters or fewer.",
                fieldKey: "notes",
                sectionKey: "documents",
                source: "maintainarr");
        }

        if (workOrder.PlannedStartAt.HasValue
            && workOrder.PlannedDueAt.HasValue
            && workOrder.PlannedDueAt.Value < workOrder.PlannedStartAt.Value)
        {
            AddValidationFindingAsync(
                findings,
                "scheduling",
                "blocker",
                "work_order.planned_due_before_start",
                "Planned due date must be on or after the planned start date.",
                fieldKey: "plannedDueAt",
                sectionKey: "scheduling",
                source: "maintainarr");
        }

        if (workOrder.DefectId.HasValue)
        {
            var defect = await db.Defects
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Id == workOrder.DefectId.Value,
                    cancellationToken);

            if (defect is null)
            {
                AddValidationFindingAsync(
                    findings,
                    "source",
                    "blocker",
                    "defect.not_found",
                    "Linked defect was not found.",
                    fieldKey: "defectId",
                    sectionKey: "source",
                    source: "maintainarr");
            }
            else
            {
                if (string.Equals(defect.Status, DefectStatuses.Closed, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(defect.Status, DefectStatuses.Resolved, StringComparison.OrdinalIgnoreCase))
                {
                    AddValidationFindingAsync(
                        findings,
                        "source",
                        "blocker",
                        "work_order.defect_closed",
                        "Work orders cannot be opened from resolved or closed defects.",
                        fieldKey: "defectId",
                        sectionKey: "source",
                        source: "maintainarr");
                }

                if (defect.AssetId != workOrder.AssetId)
                {
                    AddValidationFindingAsync(
                        findings,
                        "source",
                        "blocker",
                        "work_order.defect_asset_mismatch",
                        "Linked defect does not belong to the selected asset.",
                        fieldKey: "defectId",
                        sectionKey: "source",
                        source: "maintainarr");
                }
            }
        }

        if (workOrder.DefectId.HasValue && workOrder.PmScheduleId.HasValue)
        {
            AddValidationFindingAsync(
                findings,
                "source",
                "blocker",
                "work_order.multiple_sources",
                "Choose either a defect or a PM schedule as the source, not both.",
                fieldKey: "defectId",
                sectionKey: "source",
                source: "maintainarr");
        }

        if (workOrder.PmScheduleId.HasValue)
        {
            var schedule = await db.PmSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Id == workOrder.PmScheduleId.Value,
                    cancellationToken);

            if (schedule is null)
            {
                AddValidationFindingAsync(
                    findings,
                    "source",
                    "blocker",
                    "pm_schedule.not_found",
                    "Linked PM schedule was not found.",
                    fieldKey: "pmScheduleId",
                    sectionKey: "source",
                    source: "maintainarr");
            }
            else if (schedule.AssetId != workOrder.AssetId)
            {
                AddValidationFindingAsync(
                    findings,
                    "source",
                    "blocker",
                    "work_order.pm_schedule_asset_mismatch",
                    "Linked PM schedule does not belong to the selected asset.",
                    fieldKey: "pmScheduleId",
                    sectionKey: "source",
                    source: "maintainarr");
            }
        }

        var qualificationFinding = await TryBuildTechnicianQualificationFindingAsync(
            tenantId,
            workOrder,
            settings,
            cancellationToken);
        if (qualificationFinding is not null)
        {
            findings.Add(qualificationFinding);
        }

        return new WorkOrderValidationResponse(
            findings.All(f => !string.Equals(f.Severity, "blocker", StringComparison.OrdinalIgnoreCase)),
            findings);
    }

    private async Task<IReadOnlyList<WorkOrderDuplicateMatchResponse>> CheckDuplicateDraftAsync(
        Guid tenantId,
        WorkOrder workOrder,
        CancellationToken cancellationToken)
    {
        var asset = workOrder.Asset
            ?? await db.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrder.AssetId, cancellationToken);

        if (asset is null)
        {
            return [];
        }

        var normalizedTitle = NormalizeComparisonKey(workOrder.Title);
        var candidates = await db.WorkOrders
            .AsNoTracking()
            .Include(x => x.Asset)
            .Where(x =>
                x.TenantId == tenantId
                && x.Id != workOrder.Id
                && (string.Equals(x.Status, WorkOrderStatuses.Draft, StringComparison.OrdinalIgnoreCase)
                    || WorkOrderStatuses.Active.Contains(x.Status)))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var matches = new List<WorkOrderDuplicateMatchResponse>();
        foreach (var candidate in candidates)
        {
            var sameAsset = candidate.AssetId == workOrder.AssetId;
            var sameDefect = workOrder.DefectId.HasValue
                && candidate.DefectId.HasValue
                && candidate.DefectId == workOrder.DefectId;
            var samePmSchedule = workOrder.PmScheduleId.HasValue
                && candidate.PmScheduleId.HasValue
                && candidate.PmScheduleId == workOrder.PmScheduleId;
            var candidateTitle = NormalizeComparisonKey(candidate.Title);

            var similarityScore = 0;
            var matchReason = string.Empty;

            if (sameDefect)
            {
                similarityScore = 100;
                matchReason = "Same defect";
            }
            else if (samePmSchedule)
            {
                similarityScore = 100;
                matchReason = "Same PM schedule";
            }
            else if (sameAsset && !string.IsNullOrWhiteSpace(normalizedTitle) && string.Equals(candidateTitle, normalizedTitle, StringComparison.Ordinal))
            {
                similarityScore = 95;
                matchReason = "Same asset and identical title";
            }
            else if (sameAsset && !string.IsNullOrWhiteSpace(normalizedTitle) && candidateTitle.Contains(normalizedTitle, StringComparison.Ordinal))
            {
                similarityScore = 80;
                matchReason = "Same asset and similar title";
            }

            if (similarityScore == 0)
            {
                continue;
            }

            matches.Add(new WorkOrderDuplicateMatchResponse(
                candidate.Id,
                candidate.WorkOrderNumber,
                candidate.Title,
                candidate.Status,
                candidate.Asset?.AssetTag ?? string.Empty,
                candidate.Asset?.Name ?? string.Empty,
                matchReason,
                similarityScore));
        }

        return matches
            .OrderByDescending(x => x.SimilarityScore)
            .ThenBy(x => x.WorkOrderNumber)
            .Take(5)
            .ToList();
    }

    private async Task<IReadOnlyList<WorkOrderFindingResponse>> CheckComplianceGateFindingsAsync(
        WorkOrder workOrder,
        string toStatus,
        string? actionKey,
        string? actorPersonId,
        CancellationToken cancellationToken)
    {
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(workOrder.TenantId, cancellationToken);
        if (!complianceCoreWorkOrderGateClient.IsConfigured || !settings.Compliance.EnableComplianceCoreChecks)
        {
            return [];
        }

        var asset = workOrder.Asset
            ?? await db.Assets
                .AsNoTracking()
                .Include(x => x.AssetType)
                .FirstOrDefaultAsync(
                    x => x.TenantId == workOrder.TenantId && x.Id == workOrder.AssetId,
                    cancellationToken);

        if (asset is null)
        {
                return
                [
                    new WorkOrderFindingResponse(
                        "compliance",
                        "blocker",
                        "work_order.asset_not_found",
                        "Work order asset was not found.",
                        Source: "compliancecore")
                ];
        }

        var assetTypeKey = asset.AssetType?.TypeKey ?? string.Empty;
        var context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["product"] = "maintainarr",
            ["action"] = string.IsNullOrWhiteSpace(actionKey) ? "work_order_preview" : actionKey.Trim().ToLowerInvariant(),
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
            if (result is not null && string.Equals(result.Outcome, "warn", StringComparison.OrdinalIgnoreCase))
            {
                return
                [
                    new WorkOrderFindingResponse(
                        "compliance",
                        "warning",
                        result.ReasonCode,
                        result.Message,
                        Source: "compliancecore")
                ];
            }

            return [];
        }

        var severity = string.Equals(settings.Compliance.ComplianceCheckMode, "block", StringComparison.OrdinalIgnoreCase)
            ? "blocker"
            : "warning";

        return
        [
            new WorkOrderFindingResponse(
                "compliance",
                severity,
                result.ReasonCode,
                result.Message,
                Source: "compliancecore")
        ];
    }

    private async Task EnsureOpenableDraftAsync(
        WorkOrder workOrder,
        string? actorPersonId,
        string targetStatus,
        CancellationToken cancellationToken)
    {
        var validation = await ValidateDraftEntityAsync(workOrder.TenantId, workOrder, cancellationToken);
        var readiness = await assetReadinessService.GetAsync(workOrder.TenantId, workOrder.AssetId, cancellationToken);
        var complianceFindings = await CheckComplianceGateFindingsAsync(
            workOrder,
            targetStatus,
            "work_order_draft_action",
            actorPersonId,
            cancellationToken);

        if (validation.Findings.Any(x => string.Equals(x.Severity, "blocker", StringComparison.OrdinalIgnoreCase)))
        {
            throw new StlApiException(
                "work_order.draft_not_ready",
                "Draft work order still has validation blockers.",
                409,
                new Dictionary<string, object?>
                {
                    ["findings"] = validation.Findings,
                });
        }

        if (readiness.Blockers.Count > 0)
        {
            throw new StlApiException(
                "work_order.readiness_blocked",
                "Asset readiness still has blockers.",
                409,
                new Dictionary<string, object?>
                {
                    ["findings"] = readiness.Blockers,
                });
        }

        if (complianceFindings.Any(x => string.Equals(x.Severity, "blocker", StringComparison.OrdinalIgnoreCase)))
        {
            throw new StlApiException(
                "work_order.compliance_blocked",
                "Compliance Core blocked the requested action.",
                409,
                new Dictionary<string, object?>
                {
                    ["findings"] = complianceFindings,
                });
        }
    }

    private static string? NormalizeDraftPlanJson(string? draftPlanJson)
    {
        if (string.IsNullOrWhiteSpace(draftPlanJson))
        {
            return null;
        }

        return draftPlanJson.Trim();
    }

    private static string? GetDraftPlanValue(string? draftPlanJson, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(draftPlanJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(draftPlanJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (!document.RootElement.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            return property.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                JsonValueKind.String => NormalizeOptionalValue(property.GetString()),
                _ => NormalizeOptionalValue(property.ToString()),
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string DetermineDraftSource(Guid? defectId, Guid? pmScheduleId) =>
        defectId.HasValue
            ? WorkOrderSources.Defect
            : pmScheduleId.HasValue
                ? WorkOrderSources.PmSchedule
                : WorkOrderSources.Manual;

    private static string ResolveSource(CreateWorkOrderRequest request) =>
        !string.IsNullOrWhiteSpace(request.Source)
            ? request.Source.Trim()
            : request.PmScheduleId.HasValue
                ? WorkOrderSources.PmSchedule
                : request.DefectId.HasValue
                    ? WorkOrderSources.Defect
                    : WorkOrderSources.Manual;

    private static string ResolveWorkOrderType(CreateWorkOrderRequest request) =>
        !string.IsNullOrWhiteSpace(request.WorkOrderType)
            ? request.WorkOrderType.Trim()
            : request.PmScheduleId.HasValue
                ? WorkOrderTypes.Preventive
                : request.DefectId.HasValue
                    ? WorkOrderTypes.DefectRepair
                    : WorkOrderTypes.OperatorRequest;

    private static string ResolveOriginType(CreateWorkOrderRequest request) =>
        !string.IsNullOrWhiteSpace(request.OriginType)
            ? request.OriginType.Trim()
            : request.PmScheduleId.HasValue
                ? WorkOrderOriginTypes.PmDue
                : request.DefectId.HasValue
                    ? WorkOrderOriginTypes.Defect
                    : WorkOrderOriginTypes.Manual;

    private static string? ResolveOriginRef(CreateWorkOrderRequest request) =>
        !string.IsNullOrWhiteSpace(request.OriginRef)
            ? request.OriginRef.Trim()
            : request.PmScheduleId?.ToString("D");

    private static string DetermineDraftOriginType(Guid? defectId, Guid? pmScheduleId) =>
        defectId.HasValue
            ? WorkOrderOriginTypes.Defect
            : pmScheduleId.HasValue
                ? WorkOrderOriginTypes.PmDue
                : WorkOrderOriginTypes.Manual;

    private static string? DetermineDraftOriginRef(Guid? defectId, Guid? pmScheduleId) =>
        defectId.HasValue
            ? defectId.Value.ToString("D")
            : pmScheduleId.HasValue
                ? pmScheduleId.Value.ToString("D")
                : null;

    private static string DetermineDraftWorkOrderType(
        string? draftPlanJson,
        Guid? defectId,
        Guid? pmScheduleId)
    {
        var explicitType = GetDraftPlanValue(draftPlanJson, "workOrderType");
        if (!string.IsNullOrWhiteSpace(explicitType))
        {
            return explicitType;
        }

        if (defectId.HasValue)
        {
            return WorkOrderTypes.DefectRepair;
        }

        if (pmScheduleId.HasValue)
        {
            return WorkOrderTypes.Preventive;
        }

        return WorkOrderTypes.Corrective;
    }

    private async Task<WorkOrderFindingResponse?> TryBuildTechnicianQualificationFindingAsync(
        Guid tenantId,
        WorkOrder workOrder,
        MaintainArrTenantSettingsDto settings,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workOrder.AssignedTechnicianPersonId))
        {
            return null;
        }

        var snapshot = DeserializeQualificationCheckResults(workOrder.QualificationCheckResultsJson).FirstOrDefault();
        snapshot ??= await GetAssignedTechnicianQualificationCheckResultAsync(
            tenantId,
            workOrder.AssignedTechnicianPersonId,
            settings,
            cancellationToken);

        if (snapshot is null || IsPermissiveQualificationOutcome(snapshot.Outcome))
        {
            if (snapshot is not null && string.Equals(snapshot.Outcome, "warn", StringComparison.OrdinalIgnoreCase))
            {
                return new WorkOrderFindingResponse(
                    "qualification",
                    "warning",
                    snapshot.ReasonCode,
                    snapshot.Message,
                    FieldKey: "assignedTechnicianPersonId",
                    SectionKey: "assignment",
                    Source: "trainarr");
            }

            return null;
        }

        return new WorkOrderFindingResponse(
            "qualification",
            "blocker",
            snapshot.ReasonCode,
            snapshot.Message,
            FieldKey: "assignedTechnicianPersonId",
            SectionKey: "assignment",
            Source: "trainarr");
    }

    private static bool IsPermissiveQualificationOutcome(string outcome) =>
        string.Equals(outcome, "allow", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "warn", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "waived", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeComparisonKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant()
            .Replace('_', ' ')
            .Replace('-', ' ');
        return string.Join(' ', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static void AddValidationFindingAsync(
        ICollection<WorkOrderFindingResponse> findings,
        string category,
        string severity,
        string code,
        string message,
        string? fieldKey = null,
        string? sectionKey = null,
        string? source = null)
    {
        findings.Add(new WorkOrderFindingResponse(category, severity, code, message, fieldKey, sectionKey, source));
    }

    private static void ValidateTitleIfNeeded(ICollection<WorkOrderFindingResponse> findings, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            AddValidationFindingAsync(
                findings,
                "basics",
                "blocker",
                "work_order.title_required",
                "Work order title is required.",
                fieldKey: "title",
                sectionKey: "basics",
                source: "maintainarr");
            return;
        }

        if (title.Trim().Length > 256)
        {
            AddValidationFindingAsync(
                findings,
                "basics",
                "blocker",
                "work_order.title_too_long",
                "Work order title must be 256 characters or fewer.",
                fieldKey: "title",
                sectionKey: "basics",
                source: "maintainarr");
        }
    }

    private static void ValidatePriorityIfNeeded(ICollection<WorkOrderFindingResponse> findings, string priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
        {
            AddValidationFindingAsync(
                findings,
                "basics",
                "blocker",
                "work_order.priority_required",
                "Priority is required.",
                fieldKey: "priority",
                sectionKey: "basics",
                source: "maintainarr");
            return;
        }

        if (!WorkOrderPriorities.All.Contains(NormalizePriority(priority)))
        {
            AddValidationFindingAsync(
                findings,
                "basics",
                "blocker",
                "work_order.invalid_priority",
                "Priority must be low, medium, high, or urgent.",
                fieldKey: "priority",
                sectionKey: "basics",
                source: "maintainarr");
        }
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

    private async Task UpsertTechnicianAssignmentAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid workOrderId,
        string? personId,
        WorkOrderQualificationCheckResultResponse? qualificationSnapshot,
        DateTimeOffset assignedAt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return;
        }

        var normalizedPersonId = personId.Trim();
        var assignment = await db.WorkOrderTechnicianAssignments
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.WorkOrderId == workOrderId
                    && x.PersonId == normalizedPersonId
                    && x.AssignmentRole == WorkOrderTechnicianAssignmentRoles.Primary,
                cancellationToken);

        if (assignment is null)
        {
            assignment = new WorkOrderTechnicianAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkOrderId = workOrderId,
                PersonId = normalizedPersonId,
                AssignmentRole = WorkOrderTechnicianAssignmentRoles.Primary,
                AssignedAt = assignedAt,
            };
            db.WorkOrderTechnicianAssignments.Add(assignment);
        }

        assignment.Status = WorkOrderTechnicianAssignmentStatuses.Assigned;
        assignment.AssignedAt = assignedAt;
        assignment.AssignedByPersonId = actorUserId?.ToString("D");
        assignment.RequiredQualificationRefsJson = SerializeStringList(
            qualificationSnapshot is null ? [] : [qualificationSnapshot.QualificationKey]);
        assignment.QualificationCheckSnapshotJson = SerializeQualificationCheckResults(qualificationSnapshot);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeLocationRef(string? locationRef)
    {
        if (string.IsNullOrWhiteSpace(locationRef))
        {
            return null;
        }

        return locationRef.Trim();
    }

    private static IReadOnlyList<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json)?.ToArray() ?? Array.Empty<string>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid serialized string list.", ex);
        }
    }

    private static string SerializeQualificationCheckResults(WorkOrderQualificationCheckResultResponse? snapshot) =>
        snapshot is null ? "[]" : JsonSerializer.Serialize(new[] { snapshot });

    private static IReadOnlyList<WorkOrderQualificationCheckResultResponse> DeserializeQualificationCheckResults(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<WorkOrderQualificationCheckResultResponse>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<WorkOrderQualificationCheckResultResponse>>(json)?.ToArray()
                ?? Array.Empty<WorkOrderQualificationCheckResultResponse>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid serialized qualification check results.", ex);
        }
    }

    private static class WorkOrderBlockerTypes
    {
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "parts",
            "labor",
            "qualification",
            "safety",
            "compliance",
            "approval",
            "vendor",
            "quality_hold",
            "document",
            "asset_unavailable",
            "location_unavailable",
            "system",
        };
    }

    private static class WorkOrderBlockerSeverities
    {
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "low",
            "moderate",
            "high",
            "critical",
        };
    }

    private static class WorkOrderBlockerStatuses
    {
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "active",
            "resolved",
            "overridden",
            "canceled",
        };
    }

    private static class WorkOrderCloseoutRootCauses
    {
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "wear",
            "abuse",
            "operator_error",
            "maintenance_error",
            "part_failure",
            "design_issue",
            "environmental",
            "unknown",
            "other",
        };
    }
}
