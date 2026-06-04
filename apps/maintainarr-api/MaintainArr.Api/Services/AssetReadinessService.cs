using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetReadinessService(
    MaintainArrDbContext db,
    ComplianceCoreAssetReadinessGateClient complianceCoreAssetReadinessGate,
    IMaintainArrAuditService audit)
{
    public const int DefaultMaterializedReadStalenessHours = AssetStatusRollupDefaults.StalenessHours;

    public async Task<AssetReadinessResponse> GetAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default,
        Guid? actorUserId = null)
    {
        var asOf = DateTimeOffset.UtcNow;
        var materialized = await TryGetMaterializedDetailAsync(
            tenantId,
            assetId,
            asOf,
            DefaultMaterializedReadStalenessHours,
            cancellationToken);
        if (materialized is not null)
        {
            return await AttachDecisionMetadataAsync(
                tenantId,
                actorUserId,
                materialized,
                "asset_status_rollup",
                "asset_readiness.read",
                DefaultMaterializedReadStalenessHours,
                cancellationToken);
        }

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
        if (asset is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        var context = await LoadReadinessContextAsync(tenantId, [assetId], cancellationToken);
        var response = BuildDetailResponse(asset, context);
        return await AttachDecisionMetadataAsync(
            tenantId,
            actorUserId,
            response,
            "live_query",
            "asset_readiness.read",
            null,
            cancellationToken);
    }

    public async Task<AssetReadinessResponse> GetByDispatchRefAsync(
        Guid tenantId,
        Guid? assetId,
        string? vehicleRefKey,
        string? assetTag,
        CancellationToken cancellationToken = default,
        Guid? actorUserId = null)
    {
        var asset = await ResolveAssetForDispatchAsync(tenantId, assetId, vehicleRefKey, assetTag, cancellationToken);
        var context = await LoadReadinessContextAsync(tenantId, [asset.Id], cancellationToken);
        var response = BuildDetailResponse(asset, context);
        return await AttachDecisionMetadataAsync(
            tenantId,
            actorUserId,
            response,
            "live_query",
            "asset_readiness.dispatch_gate",
            null,
            cancellationToken);
    }

    public async Task<AssetReadinessHistoryResponse> ListHistoryAsync(
        Guid tenantId,
        Guid assetId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit ?? 15, 1, 100);

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
        if (asset is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        var query = db.AssetStatusHistory
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .OrderByDescending(x => x.ChangedAt)
            .ThenByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Take(normalizedLimit)
            .Select(x => new AssetReadinessHistoryItemResponse(
                x.Id,
                x.StatusFieldKey,
                x.StatusValueKey,
                x.Notes,
                x.ChangedByPersonId,
                x.ChangedAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AssetReadinessHistoryResponse(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            totalCount,
            normalizedLimit,
            items);
    }

    public async Task<Asset> ResolveAssetForDispatchAsync(
        Guid tenantId,
        Guid? assetId,
        string? vehicleRefKey,
        string? assetTag,
        CancellationToken cancellationToken = default)
    {
        var normalizedTag = NormalizeOptionalKey(assetTag);
        var normalizedVehicleRef = NormalizeOptionalKey(vehicleRefKey);

        if (assetId is null
            && string.IsNullOrWhiteSpace(normalizedTag)
            && string.IsNullOrWhiteSpace(normalizedVehicleRef))
        {
            throw new StlApiException(
                "asset_readiness.ref_required",
                "Asset id, vehicle reference key, or asset tag is required.",
                400);
        }

        if (assetId is Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new StlApiException(
                    "asset_readiness.ref_required",
                    "Asset id must be a non-empty GUID when provided.",
                    400);
            }

            var byAssetId = await db.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken);
            if (byAssetId is null)
            {
                throw new StlApiException("assets.not_found", "Asset was not found.", 404);
            }

            return byAssetId;
        }

        if (!string.IsNullOrWhiteSpace(normalizedTag))
        {
            var byTag = await db.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.AssetTag == normalizedTag,
                    cancellationToken);
            if (byTag is null)
            {
                throw new StlApiException("assets.not_found", "Asset was not found.", 404);
            }

            return byTag;
        }

        if (Guid.TryParse(normalizedVehicleRef, out var parsedAssetId))
        {
            var byId = await db.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == parsedAssetId, cancellationToken);
            if (byId is not null)
            {
                return byId;
            }
        }

        var byVehicleRefTag = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.AssetTag == normalizedVehicleRef,
                cancellationToken);
        if (byVehicleRefTag is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        return byVehicleRefTag;
    }

    private static string? NormalizeOptionalKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    public async Task<IReadOnlyList<AssetReadinessSummaryResponse>> ListFleetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var asOf = DateTimeOffset.UtcNow;
        var materialized = await TryListMaterializedFleetAsync(
            tenantId,
            asOf,
            DefaultMaterializedReadStalenessHours,
            cancellationToken);
        if (materialized.Count > 0)
        {
            return materialized;
        }

        var assets = await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.AssetTag)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        if (assets.Count == 0)
        {
            return [];
        }

        var assetIds = assets.Select(x => x.Id).ToList();
        var context = await LoadReadinessContextAsync(tenantId, assetIds, cancellationToken);

        return assets
            .Select(asset =>
            {
                var blockers = BuildBlockers(asset.Id, context);
                var primaryMessage = blockers.Count > 0 ? blockers[0].Message : null;
                var isReady = AssetReadinessRules.IsReady(blockers.Count);
                return new AssetReadinessSummaryResponse(
                    asset.Id,
                    asset.AssetTag,
                    asset.Name,
                    asset.LifecycleStatus,
                    AssetReadinessRules.ResolveReadinessStatus(isReady),
                    blockers.Count,
                    primaryMessage);
            })
            .ToList();
    }

    private async Task<AssetReadinessResponse?> TryGetMaterializedDetailAsync(
        Guid tenantId,
        Guid assetId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken)
    {
        var rollup = await db.AssetStatusRollups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId, cancellationToken);

        if (rollup is null || AssetStatusRollupRules.IsStale(rollup.ComputedAt, asOfUtc, stalenessHours))
        {
            return null;
        }

        return new AssetReadinessResponse(
            rollup.AssetId,
            rollup.AssetTag,
            rollup.AssetName,
            rollup.LifecycleStatus,
            rollup.ReadinessStatus,
            rollup.ReadinessBasis,
            rollup.ComputedAt,
            [],
            new AssetReadinessSignalCountsResponse(
                rollup.OpenCriticalDefectCount,
                rollup.OpenHighDefectCount,
                rollup.ActiveWorkOrderCount,
                rollup.PmDueCount,
                rollup.PmOverdueCount,
                rollup.FailedInspectionCount));
    }

    private async Task<IReadOnlyList<AssetReadinessSummaryResponse>> TryListMaterializedFleetAsync(
        Guid tenantId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken)
    {
        var rollups = await db.AssetStatusRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.AssetTag)
            .ThenBy(x => x.AssetName)
            .ToListAsync(cancellationToken);

        if (rollups.Count == 0)
        {
            return [];
        }

        if (rollups.Any(x => AssetStatusRollupRules.IsStale(x.ComputedAt, asOfUtc, stalenessHours)))
        {
            return [];
        }

        return rollups
            .Select(rollup => new AssetReadinessSummaryResponse(
                rollup.AssetId,
                rollup.AssetTag,
                rollup.AssetName,
                rollup.LifecycleStatus,
                rollup.ReadinessStatus,
                rollup.BlockerCount,
                rollup.PrimaryBlockerMessage))
            .ToList();
    }

    private static AssetReadinessResponse BuildDetailResponse(Asset asset, AssetReadinessContext context)
    {
        var blockers = BuildBlockers(asset.Id, context);
        var signals = BuildSignalCounts(asset.Id, context);
        var isReady = AssetReadinessRules.IsReady(blockers.Count);

        return new AssetReadinessResponse(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.LifecycleStatus,
            AssetReadinessRules.ResolveReadinessStatus(isReady),
            AssetReadinessRules.ResolveReadinessBasis(isReady),
            DateTimeOffset.UtcNow,
            blockers,
            signals);
    }

    private async Task<AssetReadinessResponse> AttachDecisionMetadataAsync(
        Guid tenantId,
        Guid? actorUserId,
        AssetReadinessResponse response,
        string dataSource,
        string auditAction,
        int? stalenessThresholdHours,
        CancellationToken cancellationToken)
    {
        var evaluatedResponse = await ApplyComplianceCoreGateAsync(
            tenantId,
            response,
            dataSource,
            auditAction,
            cancellationToken);

        var auditEventId = await audit.WriteAsync(
            auditAction,
            tenantId,
            actorUserId,
            "asset",
            evaluatedResponse.AssetId.ToString(),
            evaluatedResponse.ReadinessStatus,
            reasonCode: evaluatedResponse.ReadinessBasis,
            cancellationToken: cancellationToken);

        return evaluatedResponse with
        {
            Dispatchability = BuildDispatchabilitySummary(evaluatedResponse),
            Confidence = new AssetReadinessConfidenceResponse(
                dataSource,
                "fresh",
                stalenessThresholdHours,
                evaluatedResponse.CalculatedAt),
            AuditSnapshot = new AssetReadinessAuditSnapshotResponse(
                auditEventId,
                "asset_readiness_decision",
                DateTimeOffset.UtcNow),
            ComplianceCoreReferences = evaluatedResponse.ComplianceCoreReferences ?? [],
        };
    }

    private async Task<AssetReadinessResponse> ApplyComplianceCoreGateAsync(
        Guid tenantId,
        AssetReadinessResponse response,
        string dataSource,
        string auditAction,
        CancellationToken cancellationToken)
    {
        var result = await complianceCoreAssetReadinessGate.CheckAssetReadinessAsync(
            tenantId,
            response.AssetId,
            response.AssetTag,
            response.AssetName,
            new Dictionary<string, string>
            {
                ["product"] = "maintainarr",
                ["action"] = "asset_readiness",
                ["audit_action"] = auditAction,
                ["data_source"] = dataSource,
                ["asset_id"] = response.AssetId.ToString("D"),
                ["assetId"] = response.AssetId.ToString("D"),
                ["asset_tag"] = response.AssetTag,
                ["assetTag"] = response.AssetTag,
                ["lifecycle_status"] = response.LifecycleStatus,
                ["lifecycleStatus"] = response.LifecycleStatus,
                ["local_readiness_status"] = response.ReadinessStatus,
                ["localReadinessStatus"] = response.ReadinessStatus,
                ["local_readiness_basis"] = response.ReadinessBasis,
                ["localReadinessBasis"] = response.ReadinessBasis,
                ["local_blocker_count"] = response.Blockers.Count.ToString(),
                ["localBlockerCount"] = response.Blockers.Count.ToString()
            },
            cancellationToken);
        if (result is null)
        {
            return response;
        }

        var references = (response.ComplianceCoreReferences ?? [])
            .Append(new AssetReadinessComplianceCoreReferenceResponse(
                "check_result",
                result.CheckResultId.ToString("D"),
                result.Outcome,
                $"/compliancecore/check-results/{result.CheckResultId:D}"))
            .ToList();

        if (IsPermissiveComplianceCoreGateOutcome(result.Outcome))
        {
            return response with { ComplianceCoreReferences = references };
        }

        var blockers = response.Blockers
            .Append(new AssetReadinessBlockerResponse(
                "compliancecore_rule",
                string.IsNullOrWhiteSpace(result.Message)
                    ? "Compliance Core rules prevent this asset from being dispatched."
                    : result.Message,
                "compliancecore_check",
                result.CheckResultId.ToString("D"),
                result.AppliedWaiverId?.ToString("D") ?? result.AppliedWaiverKey))
            .ToList();

        return response with
        {
            ReadinessStatus = "not_ready",
            ReadinessBasis = "compliancecore",
            Blockers = blockers,
            ComplianceCoreReferences = references,
        };
    }

    private static AssetReadinessDispatchabilitySummaryResponse BuildDispatchabilitySummary(
        AssetReadinessResponse response)
    {
        var primaryBlocker = response.Blockers.FirstOrDefault();
        var isDispatchable = string.Equals(
            response.ReadinessStatus,
            "ready",
            StringComparison.OrdinalIgnoreCase);

        return new AssetReadinessDispatchabilitySummaryResponse(
            isDispatchable,
            isDispatchable ? "allow" : "block",
            isDispatchable
                ? "asset_maintenance_clear"
                : primaryBlocker?.BlockerType ?? response.ReadinessBasis,
            isDispatchable
                ? "Asset is dispatchable from MaintainArr maintenance readiness."
                : primaryBlocker?.Message ?? "Asset is not dispatchable from MaintainArr maintenance readiness.",
            response.Blockers.Count,
            primaryBlocker?.BlockerType,
            primaryBlocker?.Message);
    }

    private static AssetReadinessSignalCountsResponse BuildSignalCounts(Guid assetId, AssetReadinessContext context)
    {
        var defects = context.DefectsByAsset.GetValueOrDefault(assetId, []);
        var workOrders = context.WorkOrdersByAsset.GetValueOrDefault(assetId, []);
        var pmSchedules = context.PmSchedulesByAsset.GetValueOrDefault(assetId, []);
        var latestFailedInspection = context.LatestFailedInspectionByAsset.ContainsKey(assetId);

        return new AssetReadinessSignalCountsResponse(
            defects.Count(x =>
                AssetReadinessRules.IsOpenDefectStatus(x.Status)
                && string.Equals(x.Severity, DefectSeverities.Critical, StringComparison.OrdinalIgnoreCase)),
            defects.Count(x =>
                AssetReadinessRules.IsOpenDefectStatus(x.Status)
                && string.Equals(x.Severity, DefectSeverities.High, StringComparison.OrdinalIgnoreCase)),
            workOrders.Count(x => AssetReadinessRules.IsActiveWorkOrderStatus(x.Status)),
            pmSchedules.Count(x =>
                AssetReadinessRules.IsActivePmScheduleStatus(x.Status)
                && string.Equals(x.DueStatus, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase)),
            pmSchedules.Count(x =>
                AssetReadinessRules.IsActivePmScheduleStatus(x.Status)
                && string.Equals(x.DueStatus, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase)),
            latestFailedInspection ? 1 : 0);
    }

    private static List<AssetReadinessBlockerResponse> BuildBlockers(Guid assetId, AssetReadinessContext context)
    {
        var blockers = new List<AssetReadinessBlockerResponse>();

        foreach (var defect in context.DefectsByAsset.GetValueOrDefault(assetId, []))
        {
            if (!AssetReadinessRules.IsOpenDefectStatus(defect.Status)
                || !AssetReadinessRules.IsBlockingDefectSeverity(defect.Severity))
            {
                continue;
            }

            var severityLabel = string.Equals(defect.Severity, DefectSeverities.Critical, StringComparison.OrdinalIgnoreCase)
                ? "Critical"
                : "High";
            blockers.Add(new AssetReadinessBlockerResponse(
                string.Equals(defect.Severity, DefectSeverities.Critical, StringComparison.OrdinalIgnoreCase)
                    ? "critical_defect"
                    : "high_defect",
                $"{severityLabel} defect open: {defect.Title}",
                "defect",
                defect.Id.ToString(),
                defect.InspectionRunId?.ToString()));
        }

        foreach (var workOrder in context.WorkOrdersByAsset.GetValueOrDefault(assetId, []))
        {
            if (!AssetReadinessRules.IsActiveWorkOrderStatus(workOrder.Status))
            {
                continue;
            }

            blockers.Add(new AssetReadinessBlockerResponse(
                "active_work_order",
                $"Active work order {workOrder.WorkOrderNumber}: {workOrder.Title}",
                "work_order",
                workOrder.Id.ToString(),
                workOrder.DefectId?.ToString() ?? workOrder.PmScheduleId?.ToString()));
        }

        foreach (var blocker in context.WorkOrderBlockersByAsset.GetValueOrDefault(assetId, []))
        {
            if (!string.Equals(blocker.Status, "active", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            blockers.Add(new AssetReadinessBlockerResponse(
                blocker.BlockerType,
                blocker.Title,
                "work_order_blocker",
                blocker.BlockerId.ToString(),
                blocker.WorkOrderId.ToString()));
        }

        foreach (var hold in context.AssetQualityHoldsByAsset.GetValueOrDefault(assetId, []))
        {
            if (!string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            blockers.Add(new AssetReadinessBlockerResponse(
                "quality_hold",
                hold.Title,
                "asset_quality_hold",
                hold.Id.ToString(),
                hold.SourceObjectRef));
        }

        foreach (var schedule in context.PmSchedulesByAsset.GetValueOrDefault(assetId, []))
        {
            if (!AssetReadinessRules.IsActivePmScheduleStatus(schedule.Status)
                || !AssetReadinessRules.IsBlockingPmDueStatus(schedule.DueStatus))
            {
                continue;
            }

            var blockerType = string.Equals(schedule.DueStatus, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase)
                ? "pm_overdue"
                : "pm_due";
            var statusLabel = blockerType == "pm_overdue" ? "overdue" : "due";
            blockers.Add(new AssetReadinessBlockerResponse(
                blockerType,
                $"Preventive maintenance {statusLabel}: {schedule.Name}",
                "pm_schedule",
                schedule.Id.ToString(),
                null));
        }

        if (context.LatestFailedInspectionByAsset.TryGetValue(assetId, out var failedRun))
        {
            blockers.Add(new AssetReadinessBlockerResponse(
                "failed_inspection",
                $"Latest inspection failed: {failedRun.TemplateName}",
                "inspection_run",
                failedRun.RunId.ToString(),
                null));
        }

        return blockers
            .OrderBy(x => BlockerSortOrder(x.BlockerType))
            .ThenBy(x => x.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int BlockerSortOrder(string blockerType) => blockerType switch
    {
        "parts" => 0,
        "labor" => 0,
        "qualification" => 0,
        "safety" => 0,
        "compliance" => 0,
        "approval" => 0,
        "vendor" => 0,
        "quality_hold" => 0,
        "asset_unavailable" => 0,
        "location_unavailable" => 0,
        "system" => 0,
        "document" => 1,
        "compliancecore_rule" => 0,
        "critical_defect" => 0,
        "high_defect" => 1,
        "failed_inspection" => 2,
        "pm_overdue" => 3,
        "pm_due" => 4,
        "active_work_order" => 5,
        _ => 99,
    };

    private static bool IsPermissiveComplianceCoreGateOutcome(string outcome) =>
        string.Equals(outcome, "allow", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "warn", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "waived", StringComparison.OrdinalIgnoreCase);

    private async Task<AssetReadinessContext> LoadReadinessContextAsync(
        Guid tenantId,
        IReadOnlyList<Guid> assetIds,
        CancellationToken cancellationToken)
    {
        var defects = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);

        var workOrders = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);

        var workOrderBlockers = await (
            from blocker in db.WorkOrderBlockers.AsNoTracking()
            join workOrder in db.WorkOrders.AsNoTracking()
                on blocker.WorkOrderId equals workOrder.Id
            where blocker.TenantId == tenantId
                && workOrder.TenantId == tenantId
                && assetIds.Contains(workOrder.AssetId)
            select new WorkOrderBlockerSnapshot(
                workOrder.AssetId,
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
                blocker.OverrideReason))
            .ToListAsync(cancellationToken);

        var qualityHolds = await db.AssetQualityHolds
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);

        var pmSchedules = await db.PmSchedules
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);

        var completedRuns = await (
            from run in db.InspectionRuns.AsNoTracking()
            join template in db.InspectionTemplates.AsNoTracking()
                on run.InspectionTemplateId equals template.Id
            where run.TenantId == tenantId
                && assetIds.Contains(run.AssetId)
                && template.TenantId == tenantId
                && run.Status == InspectionRunStatuses.Completed
                && run.CompletedAt != null
            select new FailedInspectionSnapshot(
                run.AssetId,
                run.Id,
                template.Name,
                run.Result,
                run.CompletedAt!.Value))
            .ToListAsync(cancellationToken);

        var latestFailedInspectionByAsset = completedRuns
            .GroupBy(x => x.AssetId)
            .Select(group => group.OrderByDescending(x => x.CompletedAt).First())
            .Where(x => AssetReadinessRules.IsFailedInspectionResult(x.Result))
            .ToDictionary(x => x.AssetId);

        return new AssetReadinessContext(
            defects.GroupBy(x => x.AssetId).ToDictionary(group => group.Key, group => group.ToList()),
            workOrders.GroupBy(x => x.AssetId).ToDictionary(group => group.Key, group => group.ToList()),
            workOrderBlockers.GroupBy(x => x.AssetId).ToDictionary(group => group.Key, group => group.ToList()),
            qualityHolds.GroupBy(x => x.AssetId).ToDictionary(group => group.Key, group => group.ToList()),
            pmSchedules.GroupBy(x => x.AssetId).ToDictionary(group => group.Key, group => group.ToList()),
            latestFailedInspectionByAsset);
    }

    private sealed record AssetReadinessContext(
        IReadOnlyDictionary<Guid, List<Defect>> DefectsByAsset,
        IReadOnlyDictionary<Guid, List<WorkOrder>> WorkOrdersByAsset,
        IReadOnlyDictionary<Guid, List<WorkOrderBlockerSnapshot>> WorkOrderBlockersByAsset,
        IReadOnlyDictionary<Guid, List<AssetQualityHold>> AssetQualityHoldsByAsset,
        IReadOnlyDictionary<Guid, List<PmSchedule>> PmSchedulesByAsset,
        IReadOnlyDictionary<Guid, FailedInspectionSnapshot> LatestFailedInspectionByAsset);

    private sealed record WorkOrderBlockerSnapshot(
        Guid AssetId,
        Guid BlockerId,
        Guid WorkOrderId,
        string BlockerType,
        string SourceProduct,
        string? SourceObjectRef,
        string Title,
        string Description,
        string Severity,
        string Status,
        string? RequiredAction,
        DateTimeOffset CreatedAt,
        string? CreatedByPersonId,
        DateTimeOffset? ResolvedAt,
        string? ResolvedByPersonId,
        string? OverrideReason);

    private sealed record FailedInspectionSnapshot(
        Guid AssetId,
        Guid RunId,
        string TemplateName,
        string? Result,
        DateTimeOffset CompletedAt);
}
