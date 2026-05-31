using MaintainArr.Api.Contracts;

using MaintainArr.Api.Data;

using MaintainArr.Api.Entities;

using Microsoft.EntityFrameworkCore;

using STLCompliance.Shared.Contracts;



namespace MaintainArr.Api.Services;



public sealed class DefectService(

    MaintainArrDbContext db,

    IMaintainArrAuditService audit,

    AssetDowntimeService assetDowntimeService,

    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueue)

{

    public async Task<IReadOnlyList<DefectSummaryResponse>> ListAsync(

        Guid tenantId,

        bool viewAll,

        Guid? actorUserId,

        Guid? assetId = null,

        Guid? inspectionRunId = null,

        string? status = null,

        CancellationToken cancellationToken = default)

    {

        var query = db.Defects

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId);



        if (!viewAll && actorUserId.HasValue)

        {

            query = query.Where(x => x.ReportedByUserId == actorUserId.Value);

        }



        if (assetId.HasValue)

        {

            query = query.Where(x => x.AssetId == assetId.Value);

        }



        if (inspectionRunId.HasValue)

        {

            query = query.Where(x => x.InspectionRunId == inspectionRunId.Value);

        }



        if (!string.IsNullOrWhiteSpace(status))

        {

            query = query.Where(x => x.Status == status);

        }



        var defects = await query

            .OrderByDescending(x => x.CreatedAt)

            .ToListAsync(cancellationToken);



        return await MapSummariesAsync(tenantId, defects, cancellationToken);

    }



    public async Task<DefectDetailResponse> GetAsync(

        Guid tenantId,

        Guid defectId,

        CancellationToken cancellationToken = default)

    {

        var defect = await GetDefectEntityAsync(tenantId, defectId, cancellationToken);

        return await MapDetailAsync(tenantId, defect, cancellationToken);

    }



    public async Task<DefectDetailResponse> CreateManualAsync(

        Guid tenantId,

        Guid actorUserId,

        CreateDefectRequest request,

        CancellationToken cancellationToken = default)

    {

        await EnsureActiveAssetAsync(tenantId, request.AssetId, cancellationToken);

        ValidateSeverity(request.Severity);

        ValidateTitle(request.Title);



        var now = DateTimeOffset.UtcNow;

        var entity = new Defect

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            AssetId = request.AssetId,

            Title = request.Title.Trim(),

            Description = request.Description?.Trim() ?? string.Empty,

            Severity = NormalizeSeverity(request.Severity),

            Status = DefectStatuses.Open,

            Source = DefectSources.Manual,

            ReportedByUserId = actorUserId,

            CreatedAt = now,

            UpdatedAt = now,

        };



        db.Defects.Add(entity);

        await db.SaveChangesAsync(cancellationToken);

        DowntimeFollowUpResponse? downtimeFollowUp = null;
        var asset = await db.Assets
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == entity.AssetId, cancellationToken);
        if (string.Equals(entity.Severity, DefectSeverities.Critical, StringComparison.OrdinalIgnoreCase))
        {
            if (asset is not null
                && string.Equals(asset.LifecycleStatus, "active", StringComparison.OrdinalIgnoreCase))
            {
                asset.LifecycleStatus = "out_of_service";
                asset.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }

            if (asset is not null)
            {
                downtimeFollowUp = await assetDowntimeService.TryOpenCriticalDefectOutOfServiceDowntimeAsync(
                    tenantId,
                    actorUserId,
                    entity.Id,
                    asset.Id,
                    asset.AssetTag,
                    asset.Name,
                    cancellationToken);
            }
        }

        await audit.WriteAsync(

            "defect.create",

            tenantId,

            actorUserId,

            "defect",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);

        if (asset is not null)
        {
            await platformOutboxEnqueue.TryEnqueueDefectEventAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.DefectCreated,
                entity,
                asset,
                actorUserId,
                now,
                $"Defect {entity.Title} created for asset {asset.AssetTag}.",
                eventResult: entity.Severity,
                cancellationToken: cancellationToken);
        }



        return await MapDetailAsync(tenantId, entity, cancellationToken, downtimeFollowUp);

    }



    public async Task<CreateDefectsFromInspectionRunResponse> CreateFromInspectionRunAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid inspectionRunId,

        CreateDefectsFromInspectionRunRequest request,

        string source,

        CancellationToken cancellationToken = default)

    {

        var run = await db.InspectionRuns

            .AsNoTracking()

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == inspectionRunId, cancellationToken);



        if (run is null)

        {

            throw new StlApiException("inspection_run.not_found", "Inspection run was not found.", 404);

        }



        if (!string.Equals(run.Status, InspectionRunStatuses.Completed, StringComparison.OrdinalIgnoreCase))

        {

            throw new StlApiException(

                "defect.inspection_run_not_completed",

                "Defects can only be created from completed inspection runs.",

                400);

        }



        var created = new List<DefectSummaryResponse>();

        var existing = new List<DefectSummaryResponse>();



        var failedItemIds = await GetFailedChecklistItemIdsAsync(tenantId, inspectionRunId, cancellationToken);

        var targetItemIds = ResolveTargetChecklistItemIds(failedItemIds, request.ChecklistItemIds);



        foreach (var checklistItemId in targetItemIds)

        {

            var result = await CreateOrGetInspectionDefectAsync(

                tenantId,

                actorUserId,

                run,

                checklistItemId,

                source,

                cancellationToken);



            if (result.IsNew)

            {

                created.Add(result.Summary);

            }

            else

            {

                existing.Add(result.Summary);

            }

        }



        return new CreateDefectsFromInspectionRunResponse(inspectionRunId, created, existing);

    }



    public async Task AutoCreateFromCompletedRunAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid inspectionRunId,

        CancellationToken cancellationToken = default)

    {

        var failedItemIds = await GetFailedChecklistItemIdsAsync(tenantId, inspectionRunId, cancellationToken);

        if (failedItemIds.Count == 0)

        {

            return;

        }



        var run = await db.InspectionRuns

            .AsNoTracking()

            .FirstAsync(x => x.TenantId == tenantId && x.Id == inspectionRunId, cancellationToken);



        foreach (var checklistItemId in failedItemIds)

        {

            await CreateOrGetInspectionDefectAsync(

                tenantId,

                actorUserId,

                run,

                checklistItemId,

                DefectSources.InspectionAuto,

                cancellationToken);

        }

    }



    public async Task<DefectDetailResponse> UpdateStatusAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid defectId,

        UpdateDefectStatusRequest request,

        CancellationToken cancellationToken = default)

    {

        var status = request.Status?.Trim() ?? string.Empty;

        if (!DefectStatuses.All.Contains(status))

        {

            throw new StlApiException(

                "defect.invalid_status",

                "Status must be open, acknowledged, in_repair, resolved, or closed.",

                400);

        }



        var defect = await db.Defects.FirstOrDefaultAsync(

            x => x.TenantId == tenantId && x.Id == defectId,

            cancellationToken);



        if (defect is null)

        {

            throw new StlApiException("defect.not_found", "Defect was not found.", 404);

        }



        var now = DateTimeOffset.UtcNow;

        defect.Status = status.ToLowerInvariant();

        defect.UpdatedAt = now;



        if (string.Equals(defect.Status, DefectStatuses.Resolved, StringComparison.OrdinalIgnoreCase)

            || string.Equals(defect.Status, DefectStatuses.Closed, StringComparison.OrdinalIgnoreCase))

        {

            defect.ResolvedAt ??= now;

        }

        else

        {

            defect.ResolvedAt = null;

        }



        await db.SaveChangesAsync(cancellationToken);



        await audit.WriteAsync(

            "defect.status_update",

            tenantId,

            actorUserId,

            "defect",

            defect.Id.ToString(),

            defect.Status,

            cancellationToken: cancellationToken);

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == defect.AssetId, cancellationToken);
        if (asset is not null)
        {
            var eventKind = defect.Status switch
            {
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



        return await MapDetailAsync(tenantId, defect, cancellationToken);

    }



    private async Task<(bool IsNew, DefectSummaryResponse Summary)> CreateOrGetInspectionDefectAsync(

        Guid tenantId,

        Guid actorUserId,

        InspectionRun run,

        Guid checklistItemId,

        string source,

        CancellationToken cancellationToken)

    {

        var existing = await db.Defects

            .AsNoTracking()

            .FirstOrDefaultAsync(

                x => x.TenantId == tenantId

                    && x.InspectionRunId == run.Id

                    && x.ChecklistItemId == checklistItemId,

                cancellationToken);



        if (existing is not null)

        {

            var summaries = await MapSummariesAsync(tenantId, [existing], cancellationToken);

            return (false, summaries[0]);

        }



        var item = await db.InspectionChecklistItems

            .AsNoTracking()

            .FirstOrDefaultAsync(

                x => x.TenantId == tenantId && x.Id == checklistItemId,

                cancellationToken);



        if (item is null)

        {

            throw new StlApiException("defect.checklist_item_not_found", "Checklist item was not found.", 404);

        }



        var now = DateTimeOffset.UtcNow;

        var entity = new Defect

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            AssetId = run.AssetId,

            InspectionRunId = run.Id,

            ChecklistItemId = checklistItemId,

            Title = $"Failed: {item.Prompt}",

            Description = $"Defect opened from inspection run {run.Id} for checklist item {item.ItemKey}.",

            Severity = DefectSeverities.Medium,

            Status = DefectStatuses.Open,

            Source = source,

            ReportedByUserId = actorUserId,

            CreatedAt = now,

            UpdatedAt = now,

        };



        db.Defects.Add(entity);

        await db.SaveChangesAsync(cancellationToken);



        await audit.WriteAsync(

            "defect.create_from_inspection",

            tenantId,

            actorUserId,

            "defect",

            entity.Id.ToString(),

            source,

            cancellationToken: cancellationToken);

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == entity.AssetId, cancellationToken);
        if (asset is not null)
        {
            await platformOutboxEnqueue.TryEnqueueDefectEventAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.DefectCreated,
                entity,
                asset,
                actorUserId,
                now,
                $"Defect {entity.Title} created from inspection for asset {asset.AssetTag}.",
                eventResult: entity.Severity,
                cancellationToken: cancellationToken);
        }



        var createdSummaries = await MapSummariesAsync(tenantId, [entity], cancellationToken);

        return (true, createdSummaries[0]);

    }



    private async Task<IReadOnlyList<Guid>> GetFailedChecklistItemIdsAsync(

        Guid tenantId,

        Guid inspectionRunId,

        CancellationToken cancellationToken)

    {

        var run = await db.InspectionRuns

            .AsNoTracking()

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == inspectionRunId, cancellationToken);



        if (run is null)

        {

            return [];

        }



        var checklistItems = await db.InspectionChecklistItems

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == run.InspectionTemplateId)

            .ToListAsync(cancellationToken);



        var answers = await db.InspectionRunAnswers

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.InspectionRunId == inspectionRunId)

            .ToDictionaryAsync(x => x.ChecklistItemId, cancellationToken);



        return checklistItems

            .Where(item => answers.TryGetValue(item.Id, out var answer)

                && string.Equals(item.ItemType, InspectionChecklistItemTypes.PassFail, StringComparison.OrdinalIgnoreCase)

                && string.Equals(answer.PassFailValue, InspectionAnswerPassFailValues.Fail, StringComparison.OrdinalIgnoreCase))

            .Select(item => item.Id)

            .ToList();

    }



    private static IReadOnlyList<Guid> ResolveTargetChecklistItemIds(

        IReadOnlyList<Guid> failedItemIds,

        IReadOnlyList<Guid>? requestedItemIds)

    {

        if (requestedItemIds is null || requestedItemIds.Count == 0)

        {

            return failedItemIds;

        }



        var failedSet = failedItemIds.ToHashSet();

        var invalid = requestedItemIds.Where(id => !failedSet.Contains(id)).ToList();

        if (invalid.Count > 0)

        {

            throw new StlApiException(

                "defect.invalid_checklist_items",

                "All checklist items must have a fail answer on the completed inspection run.",

                400);

        }



        return requestedItemIds.Distinct().ToList();

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

                "Defects can only be created for active assets.",

                400);

        }

    }



    private async Task<Defect> GetDefectEntityAsync(

        Guid tenantId,

        Guid defectId,

        CancellationToken cancellationToken)

    {

        var defect = await db.Defects

            .AsNoTracking()

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == defectId, cancellationToken);



        if (defect is null)

        {

            throw new StlApiException("defect.not_found", "Defect was not found.", 404);

        }



        return defect;

    }



    private async Task<IReadOnlyList<DefectSummaryResponse>> MapSummariesAsync(

        Guid tenantId,

        IReadOnlyList<Defect> defects,

        CancellationToken cancellationToken)

    {

        if (defects.Count == 0)

        {

            return [];

        }



        var assetIds = defects.Select(x => x.AssetId).Distinct().ToList();

        var assets = await db.Assets

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.Id))

            .ToDictionaryAsync(x => x.Id, cancellationToken);



        var checklistItemIds = defects

            .Where(x => x.ChecklistItemId.HasValue)

            .Select(x => x.ChecklistItemId!.Value)

            .Distinct()

            .ToList();



        var checklistItems = checklistItemIds.Count == 0

            ? new Dictionary<Guid, InspectionChecklistItem>()

            : await db.InspectionChecklistItems

                .AsNoTracking()

                .Where(x => x.TenantId == tenantId && checklistItemIds.Contains(x.Id))

                .ToDictionaryAsync(x => x.Id, cancellationToken);



        var defectIds = defects.Select(x => x.Id).ToList();

        var evidenceCounts = defectIds.Count == 0

            ? new Dictionary<Guid, int>()

            : await db.DefectEvidence

                .AsNoTracking()

                .Where(x => x.TenantId == tenantId && defectIds.Contains(x.DefectId))

                .GroupBy(x => x.DefectId)

                .Select(g => new { DefectId = g.Key, Count = g.Count() })

                .ToDictionaryAsync(x => x.DefectId, x => x.Count, cancellationToken);



        return defects

            .Select(defect =>

            {

                assets.TryGetValue(defect.AssetId, out var asset);

                string? itemKey = null;

                if (defect.ChecklistItemId.HasValue)

                {

                    checklistItems.TryGetValue(defect.ChecklistItemId.Value, out var item);

                    itemKey = item?.ItemKey;

                }



                return new DefectSummaryResponse(

                    defect.Id,

                    defect.AssetId,

                    asset?.AssetTag ?? string.Empty,

                    asset?.Name ?? string.Empty,

                    defect.InspectionRunId,

                    defect.ChecklistItemId,

                    itemKey,

                    defect.Title,

                    defect.Severity,

                    defect.Status,

                    defect.Source,

                    defect.ReportedByUserId,

                    defect.CreatedAt,

                    defect.UpdatedAt,

                    defect.ResolvedAt,

                    evidenceCounts.GetValueOrDefault(defect.Id, 0));

            })

            .ToList();

    }



    private async Task<DefectDetailResponse> MapDetailAsync(

        Guid tenantId,

        Defect defect,

        CancellationToken cancellationToken,

        DowntimeFollowUpResponse? downtimeFollowUp = null)

    {

        var summaries = await MapSummariesAsync(tenantId, [defect], cancellationToken);

        var summary = summaries[0];



        InspectionChecklistItem? checklistItem = null;

        if (defect.ChecklistItemId.HasValue)

        {

            checklistItem = await db.InspectionChecklistItems

                .AsNoTracking()

                .FirstOrDefaultAsync(

                    x => x.TenantId == tenantId && x.Id == defect.ChecklistItemId.Value,

                    cancellationToken);

        }



        return new DefectDetailResponse(

            summary.DefectId,

            summary.AssetId,

            summary.AssetTag,

            summary.AssetName,

            summary.InspectionRunId,

            summary.ChecklistItemId,

            summary.ChecklistItemKey,

            checklistItem?.Prompt,

            defect.Title,

            defect.Description,

            defect.Severity,

            defect.Status,

            defect.Source,

            defect.ReportedByUserId,

            defect.CreatedAt,

            defect.UpdatedAt,

            defect.ResolvedAt,

            summary.EvidenceCount,

            downtimeFollowUp);

    }



    private static void ValidateTitle(string title)

    {

        if (string.IsNullOrWhiteSpace(title))

        {

            throw new StlApiException("defect.title_required", "Defect title is required.", 400);

        }



        if (title.Trim().Length > 256)

        {

            throw new StlApiException("defect.title_too_long", "Defect title must be 256 characters or fewer.", 400);

        }

    }



    private static void ValidateSeverity(string severity)

    {

        if (!DefectSeverities.All.Contains(severity))

        {

            throw new StlApiException(

                "defect.invalid_severity",

                "Severity must be low, medium, high, or critical.",

                400);

        }

    }



    private static string NormalizeSeverity(string severity) => severity.Trim().ToLowerInvariant();

}

