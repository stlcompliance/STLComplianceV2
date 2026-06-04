using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class InspectionRunService(
    MaintainArrDbContext db,
    AssetService assetService,
    DefectService defectService,
    PmOccurrenceService pmOccurrences,
    IMaintainArrAuditService audit,
    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueue)
{
    private static readonly HashSet<string> AllowedPassFailValues = new(StringComparer.OrdinalIgnoreCase)
    {
        InspectionAnswerPassFailValues.Pass,
        InspectionAnswerPassFailValues.Fail,
        InspectionAnswerPassFailValues.Na,
    };

    private static readonly HashSet<string> AllowedYesNoValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "yes",
        "no",
    };

    public async Task<IReadOnlyList<InspectionRunSummaryResponse>> ListAsync(
        Guid tenantId,
        Guid actorUserId,
        bool viewAll,
        CancellationToken cancellationToken = default)
    {
        var query = db.InspectionRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!viewAll)
        {
            query = query.Where(x => x.StartedByUserId == actorUserId);
        }

        var runs = await query
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

        if (runs.Count == 0)
        {
            return [];
        }

        return await MapSummariesAsync(tenantId, runs, cancellationToken);
    }

    public async Task<InspectionRunDetailResponse> GetAsync(
        Guid tenantId,
        Guid inspectionRunId,
        CancellationToken cancellationToken = default)
    {
        var run = await GetRunEntityAsync(tenantId, inspectionRunId, cancellationToken);
        return await MapDetailAsync(tenantId, run, cancellationToken);
    }

    public async Task<InspectionRunDetailResponse> StartAsync(
        Guid tenantId,
        Guid actorUserId,
        StartInspectionRunRequest request,
        CancellationToken cancellationToken = default,
        Guid? pmScheduleId = null)
    {
        var asset = await assetService.GetAsync(tenantId, request.AssetId, cancellationToken);
        if (!string.Equals(asset.LifecycleStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inspection_run.asset_not_active",
                "Inspections can only be started for active assets.",
                400);
        }

        var template = await db.InspectionTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.InspectionTemplateId,
                cancellationToken);

        if (template is null)
        {
            throw new StlApiException("inspection_template.not_found", "Inspection template was not found.", 404);
        }

        if (!string.Equals(template.Status, InspectionTemplateStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inspection_run.template_not_active",
                "Inspections can only use active templates.",
                400);
        }

        await EnsureTemplateAppliesToAssetTypeAsync(tenantId, template.Id, asset.AssetTypeId, cancellationToken);

        var checklistItems = await db.InspectionChecklistItems
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == template.Id)
            .ToListAsync(cancellationToken);

        if (checklistItems.Count == 0)
        {
            throw new StlApiException(
                "inspection_run.template_missing_checklist",
                "Active templates require checklist items before running inspections.",
                400);
        }

        var inProgressExists = await db.InspectionRuns.AnyAsync(
            x => x.TenantId == tenantId
                && x.AssetId == request.AssetId
                && x.InspectionTemplateId == template.Id
                && x.Status == InspectionRunStatuses.InProgress,
            cancellationToken);

        if (inProgressExists)
        {
            throw new StlApiException(
                "inspection_run.already_in_progress",
                "An inspection run is already in progress for this asset and template.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new InspectionRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = request.AssetId,
            InspectionTemplateId = template.Id,
            PmScheduleId = pmScheduleId,
            TemplateVersion = template.Version,
            Status = InspectionRunStatuses.InProgress,
            StartedByUserId = actorUserId,
            StartedAt = now,
            UpdatedAt = now,
        };

        db.InspectionRuns.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "inspection_run.start",
            tenantId,
            actorUserId,
            "inspection_run",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await platformOutboxEnqueue.TryEnqueueInspectionRunEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.InspectionStarted,
            entity,
            asset,
            actorUserId,
            now,
            $"Inspection {template.TemplateKey} started for asset {asset.AssetTag}.",
            cancellationToken: cancellationToken);

        if (pmScheduleId.HasValue)
        {
            var schedule = await LoadPmScheduleAsync(tenantId, pmScheduleId.Value, cancellationToken);
            var pmAsset = await db.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == schedule.AssetId, cancellationToken);
            if (pmAsset is null)
            {
                return await MapDetailAsync(tenantId, entity, cancellationToken);
            }

            await pmOccurrences.MarkInspectionGeneratedAsync(
                schedule,
                entity.Id.ToString("D"),
                now,
                cancellationToken);

            await platformOutboxEnqueue.TryEnqueuePmOccurrenceEventAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.PmOccurrenceInspectionGenerated,
                schedule,
                pmAsset,
                actorUserId,
                now,
                $"PM occurrence generated inspection {entity.Id} for asset {asset.AssetTag}.",
                eventResult: entity.Id.ToString("D"),
                idempotencyDiscriminator: entity.Id.ToString("D"),
                cancellationToken: cancellationToken);
        }

        return await MapDetailAsync(tenantId, entity, cancellationToken);
    }

    public async Task<InspectionRunDetailResponse> SubmitAnswersAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid inspectionRunId,
        SubmitInspectionRunAnswersRequest request,
        CancellationToken cancellationToken = default)
    {
        var run = await GetRunForWriteAsync(tenantId, inspectionRunId, cancellationToken);
        if (!string.Equals(run.Status, InspectionRunStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inspection_run.not_in_progress",
                "Answers can only be submitted for in-progress inspection runs.",
                400);
        }

        var checklistItems = await db.InspectionChecklistItems
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == run.InspectionTemplateId)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (request.Answers.Count == 0
            && checklistItems.Values.Any(item => !IsEvidenceOnlyItem(item.ItemType)))
        {
            throw new StlApiException(
                "inspection_run.answers_required",
                "At least one answer is required.",
                400);
        }

        var existingAnswers = await db.InspectionRunAnswers
            .Where(x => x.TenantId == tenantId && x.InspectionRunId == inspectionRunId)
            .ToDictionaryAsync(x => x.ChecklistItemId, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        foreach (var input in request.Answers)
        {
            if (!checklistItems.TryGetValue(input.ChecklistItemId, out var checklistItem))
            {
                throw new StlApiException(
                    "inspection_run.invalid_checklist_item",
                    "One or more checklist items do not belong to this inspection template.",
                    400);
            }

            var normalized = NormalizeAnswer(checklistItem, input);

            if (existingAnswers.TryGetValue(input.ChecklistItemId, out var existing))
            {
                existing.PassFailValue = normalized.PassFailValue;
                existing.NumericValue = normalized.NumericValue;
                existing.TextValue = normalized.TextValue;
                existing.SelectedOptionsJson = SerializeStringList(normalized.SelectedOptions);
                existing.AnsweredAt = now;
                existing.AnsweredByUserId = actorUserId;
            }
            else
            {
                var answer = new InspectionRunAnswer
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    InspectionRunId = inspectionRunId,
                    ChecklistItemId = input.ChecklistItemId,
                    PassFailValue = normalized.PassFailValue,
                    NumericValue = normalized.NumericValue,
                    TextValue = normalized.TextValue,
                    SelectedOptionsJson = SerializeStringList(normalized.SelectedOptions),
                    AnsweredAt = now,
                    AnsweredByUserId = actorUserId,
                };
                db.InspectionRunAnswers.Add(answer);
                existingAnswers[input.ChecklistItemId] = answer;
            }
        }

        run.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "inspection_run.answers.submit",
            tenantId,
            actorUserId,
            "inspection_run",
            run.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        var asset = await assetService.GetAsync(tenantId, run.AssetId, cancellationToken);
        await platformOutboxEnqueue.TryEnqueueInspectionRunEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.InspectionAnswerSubmitted,
            run,
            asset,
            actorUserId,
            now,
            $"Inspection answers submitted for asset {asset.AssetTag}.",
            eventResult: $"{request.Answers.Count}",
            cancellationToken: cancellationToken);

        return await MapDetailAsync(tenantId, run, cancellationToken);
    }

    public async Task<InspectionRunDetailResponse> CompleteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid inspectionRunId,
        CancellationToken cancellationToken = default)
    {
        var run = await GetRunForWriteAsync(tenantId, inspectionRunId, cancellationToken);
        if (!string.Equals(run.Status, InspectionRunStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inspection_run.not_in_progress",
                "Only in-progress inspection runs can be completed.",
                400);
        }

        var checklistItems = await db.InspectionChecklistItems
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == run.InspectionTemplateId)
            .ToListAsync(cancellationToken);

        var answers = await db.InspectionRunAnswers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionRunId == inspectionRunId)
            .ToDictionaryAsync(x => x.ChecklistItemId, cancellationToken);

        var evidenceChecklistItemIds = await db.InspectionRunEvidence
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionRunId == inspectionRunId && x.ChecklistItemId.HasValue)
            .Select(x => x.ChecklistItemId!.Value)
            .ToListAsync(cancellationToken);

        var failed = false;

        foreach (var item in checklistItems)
        {
            answers.TryGetValue(item.Id, out var answer);

            if (item.IsRequired && !HasAnswer(item, answer, evidenceChecklistItemIds))
            {
                throw new StlApiException(
                    "inspection_run.missing_required_answers",
                    "All required checklist items must be answered before completing the inspection.",
                    400);
            }

            if (answer is not null && IsFailedAnswer(item, answer))
            {
                failed = true;
            }
        }

        var now = DateTimeOffset.UtcNow;
        run.Status = InspectionRunStatuses.Completed;
        run.Result = failed ? InspectionRunResults.Failed : InspectionRunResults.Passed;
        run.CompletedAt = now;
        run.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "inspection_run.complete",
            tenantId,
            actorUserId,
            "inspection_run",
            run.Id.ToString(),
            run.Result ?? "Succeeded",
            cancellationToken: cancellationToken);

        var asset = await assetService.GetAsync(tenantId, run.AssetId, cancellationToken);
        await platformOutboxEnqueue.TryEnqueueInspectionRunEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.InspectionCompleted,
            run,
            asset,
            actorUserId,
            now,
            $"Inspection completed for asset {asset.AssetTag} with result {run.Result}.",
            eventResult: run.Result,
            cancellationToken: cancellationToken);

        if (failed)
        {
            await platformOutboxEnqueue.TryEnqueueInspectionRunEventAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.InspectionFailed,
                run,
                asset,
                actorUserId,
                now,
                $"Inspection failed for asset {asset.AssetTag}.",
                eventResult: run.Result,
                cancellationToken: cancellationToken);
        }

        if (failed)
        {
            await defectService.AutoCreateFromCompletedRunAsync(
                tenantId,
                actorUserId,
                inspectionRunId,
                cancellationToken);
        }

        return await MapDetailAsync(tenantId, run, cancellationToken);
    }

    private async Task EnsureTemplateAppliesToAssetTypeAsync(
        Guid tenantId,
        Guid inspectionTemplateId,
        Guid assetTypeId,
        CancellationToken cancellationToken)
    {
        var linkedTypeIds = await db.InspectionTemplateAssetTypes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == inspectionTemplateId)
            .Select(x => x.AssetTypeId)
            .ToListAsync(cancellationToken);

        if (linkedTypeIds.Count == 0)
        {
            return;
        }

        if (!linkedTypeIds.Contains(assetTypeId))
        {
            throw new StlApiException(
                "inspection_run.template_asset_type_mismatch",
                "This inspection template is not linked to the asset type.",
                400);
        }
    }

    private async Task<InspectionRun> GetRunEntityAsync(
        Guid tenantId,
        Guid inspectionRunId,
        CancellationToken cancellationToken)
    {
        var run = await db.InspectionRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == inspectionRunId, cancellationToken);

        if (run is null)
        {
            throw new StlApiException("inspection_run.not_found", "Inspection run was not found.", 404);
        }

        return run;
    }

    private async Task<InspectionRun> GetRunForWriteAsync(
        Guid tenantId,
        Guid inspectionRunId,
        CancellationToken cancellationToken)
    {
        var run = await db.InspectionRuns.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == inspectionRunId,
            cancellationToken);

        if (run is null)
        {
            throw new StlApiException("inspection_run.not_found", "Inspection run was not found.", 404);
        }

        return run;
    }

    private async Task<PmSchedule> LoadPmScheduleAsync(
        Guid tenantId,
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

        return schedule;
    }

    private async Task<IReadOnlyList<InspectionRunSummaryResponse>> MapSummariesAsync(
        Guid tenantId,
        IReadOnlyList<InspectionRun> runs,
        CancellationToken cancellationToken)
    {
        var runIds = runs.Select(x => x.Id).ToList();
        var assetIds = runs.Select(x => x.AssetId).Distinct().ToList();
        var templateIds = runs.Select(x => x.InspectionTemplateId).Distinct().ToList();

        var assets = await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var templates = await db.InspectionTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && templateIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var answerCounts = await db.InspectionRunAnswers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && runIds.Contains(x.InspectionRunId))
            .GroupBy(x => x.InspectionRunId)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var requiredCounts = await db.InspectionChecklistItems
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && templateIds.Contains(x.InspectionTemplateId) && x.IsRequired)
            .GroupBy(x => x.InspectionTemplateId)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        return runs
            .Select(run =>
            {
                assets.TryGetValue(run.AssetId, out var asset);
                templates.TryGetValue(run.InspectionTemplateId, out var template);

                return new InspectionRunSummaryResponse(
                    run.Id,
                    run.AssetId,
                    asset?.AssetTag ?? string.Empty,
                    asset?.Name ?? string.Empty,
                    run.InspectionTemplateId,
                    run.PmScheduleId,
                    template?.TemplateKey ?? string.Empty,
                    template?.Name ?? string.Empty,
                    run.TemplateVersion,
                    run.Status,
                    run.Result,
                    run.StartedByUserId,
                    run.StartedAt,
                    run.CompletedAt,
                    answerCounts.GetValueOrDefault(run.Id),
                    requiredCounts.GetValueOrDefault(run.InspectionTemplateId));
            })
            .ToList();
    }

    private async Task<InspectionRunDetailResponse> MapDetailAsync(
        Guid tenantId,
        InspectionRun run,
        CancellationToken cancellationToken)
    {
        var asset = await assetService.GetAsync(tenantId, run.AssetId, cancellationToken);

        var template = await db.InspectionTemplates
            .AsNoTracking()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == run.InspectionTemplateId, cancellationToken);

        var categories = await db.InspectionTemplateCategories
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == run.InspectionTemplateId)
            .ToDictionaryAsync(x => x.Id, x => x.CategoryKey, cancellationToken);

        var checklistItemEntities = await db.InspectionChecklistItems
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == run.InspectionTemplateId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ItemKey)
            .ToListAsync(cancellationToken);

        var checklistItems = checklistItemEntities
            .Select(item => new InspectionRunChecklistItemSnapshot(
                item.Id,
                item.CategoryId,
                item.CategoryId.HasValue && categories.TryGetValue(item.CategoryId.Value, out var key) ? key : null,
                item.ItemKey,
                item.Prompt,
                item.ItemType,
                DeserializeStringList(item.ControlledOptionsJson),
                item.AcceptableRangeMin,
                item.AcceptableRangeMax,
                item.UnitOfMeasure,
                item.IsRequired,
                item.SortOrder))
            .ToList();

        var answerEntities = await db.InspectionRunAnswers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionRunId == run.Id)
            .ToListAsync(cancellationToken);

        var itemMap = checklistItemEntities.ToDictionary(x => x.Id);

        var answers = answerEntities
            .Select(answer => new InspectionRunAnswerResponse(
                answer.Id,
                answer.ChecklistItemId,
                itemMap.GetValueOrDefault(answer.ChecklistItemId)?.ItemKey ?? string.Empty,
                answer.PassFailValue,
                answer.NumericValue,
                answer.TextValue,
                DeserializeStringList(answer.SelectedOptionsJson),
                itemMap.GetValueOrDefault(answer.ChecklistItemId)?.UnitOfMeasure,
                answer.AnsweredAt,
                answer.AnsweredByUserId))
            .OrderBy(x => x.ItemKey)
            .ToList();

        return new InspectionRunDetailResponse(
            run.Id,
            run.AssetId,
            asset.AssetTag,
            asset.Name,
            run.InspectionTemplateId,
            run.PmScheduleId,
            template.TemplateKey,
            template.Name,
            run.TemplateVersion,
            run.Status,
            run.Result,
            run.StartedByUserId,
            run.StartedAt,
            run.CompletedAt,
            run.UpdatedAt,
            checklistItems,
            answers);
    }

    private static (string? PassFailValue, decimal? NumericValue, string? TextValue, IReadOnlyList<string> SelectedOptions) NormalizeAnswer(
        InspectionChecklistItem checklistItem,
        InspectionRunAnswerInput input)
    {
        if (string.Equals(checklistItem.ItemType, InspectionChecklistItemTypes.PassFail, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(input.PassFailValue))
            {
                throw new StlApiException(
                    "inspection_run.invalid_pass_fail_answer",
                    "Pass/fail checklist items require a pass, fail, or na value.",
                    400);
            }

            var normalized = input.PassFailValue.Trim().ToLowerInvariant();
            if (!AllowedPassFailValues.Contains(normalized))
            {
                throw new StlApiException(
                    "inspection_run.invalid_pass_fail_answer",
                    "Pass/fail value must be pass, fail, or na.",
                    400);
            }

            return (normalized, null, null, []);
        }

        if (string.Equals(checklistItem.ItemType, InspectionChecklistItemTypes.YesNo, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(input.TextValue))
            {
                throw new StlApiException(
                    "inspection_run.invalid_yes_no_answer",
                    "Yes/no checklist items require a yes or no value.",
                    400);
            }

            var normalized = input.TextValue.Trim().ToLowerInvariant();
            if (!AllowedYesNoValues.Contains(normalized))
            {
                throw new StlApiException(
                    "inspection_run.invalid_yes_no_answer",
                    "Yes/no value must be yes or no.",
                    400);
            }

            return (null, null, normalized, []);
        }

        if (string.Equals(checklistItem.ItemType, InspectionChecklistItemTypes.Select, StringComparison.OrdinalIgnoreCase))
        {
            var selectedOptions = NormalizeSelectedOptions(checklistItem, input.SelectedOptions, allowMultiple: false);
            return (null, null, null, selectedOptions);
        }

        if (string.Equals(checklistItem.ItemType, InspectionChecklistItemTypes.MultiSelect, StringComparison.OrdinalIgnoreCase))
        {
            var selectedOptions = NormalizeSelectedOptions(checklistItem, input.SelectedOptions, allowMultiple: true);
            return (null, null, null, selectedOptions);
        }

        if (string.Equals(checklistItem.ItemType, InspectionChecklistItemTypes.Photo, StringComparison.OrdinalIgnoreCase)
            || string.Equals(checklistItem.ItemType, InspectionChecklistItemTypes.Signature, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inspection_run.evidence_required",
                "Evidence checklist items must be completed through the inspection evidence panel.",
                400);
        }

        if (string.Equals(checklistItem.ItemType, InspectionChecklistItemTypes.MeterReading, StringComparison.OrdinalIgnoreCase))
        {
            if (!input.NumericValue.HasValue)
            {
                throw new StlApiException(
                    "inspection_run.invalid_meter_reading_answer",
                    "Meter reading checklist items require a numeric value.",
                    400);
            }

            return (null, input.NumericValue.Value, null, []);
        }

        if (string.Equals(checklistItem.ItemType, InspectionChecklistItemTypes.Numeric, StringComparison.OrdinalIgnoreCase))
        {
            if (!input.NumericValue.HasValue)
            {
                throw new StlApiException(
                    "inspection_run.invalid_numeric_answer",
                    "Numeric checklist items require a numeric value.",
                    400);
            }

            return (null, input.NumericValue.Value, null, []);
        }

        if (string.IsNullOrWhiteSpace(input.TextValue))
        {
            throw new StlApiException(
                "inspection_run.invalid_text_answer",
                "Text checklist items require a text value.",
                400);
        }

        var text = input.TextValue.Trim();
        if (text.Length > 512)
        {
            throw new StlApiException(
                "inspection_run.invalid_text_answer",
                "Text answers must be 512 characters or fewer.",
                400);
        }

        return (null, null, text, []);
    }

    private static bool HasAnswer(InspectionChecklistItem item, InspectionRunAnswer? answer, IReadOnlyCollection<Guid> evidenceChecklistItemIds)
    {
        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.Photo, StringComparison.OrdinalIgnoreCase)
            || string.Equals(item.ItemType, InspectionChecklistItemTypes.Signature, StringComparison.OrdinalIgnoreCase))
        {
            return evidenceChecklistItemIds.Contains(item.Id);
        }

        if (answer is null)
        {
            return false;
        }

        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.PassFail, StringComparison.OrdinalIgnoreCase))
        {
            return !string.IsNullOrWhiteSpace(answer.PassFailValue);
        }

        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.YesNo, StringComparison.OrdinalIgnoreCase))
        {
            return !string.IsNullOrWhiteSpace(answer.TextValue);
        }

        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.Select, StringComparison.OrdinalIgnoreCase)
            || string.Equals(item.ItemType, InspectionChecklistItemTypes.MultiSelect, StringComparison.OrdinalIgnoreCase))
        {
            return DeserializeStringList(answer.SelectedOptionsJson).Count > 0;
        }

        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.Numeric, StringComparison.OrdinalIgnoreCase))
        {
            return answer.NumericValue.HasValue;
        }

        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.MeterReading, StringComparison.OrdinalIgnoreCase))
        {
            return answer.NumericValue.HasValue;
        }

        return !string.IsNullOrWhiteSpace(answer.TextValue);
    }

    private static bool IsFailedAnswer(InspectionChecklistItem item, InspectionRunAnswer answer)
    {
        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.YesNo, StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(answer.TextValue, "no", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.PassFail, StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(answer.PassFailValue, InspectionAnswerPassFailValues.Fail, StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.MeterReading, StringComparison.OrdinalIgnoreCase))
        {
            if (!answer.NumericValue.HasValue)
            {
                return true;
            }

            if (item.AcceptableRangeMin.HasValue && answer.NumericValue.Value < item.AcceptableRangeMin.Value)
            {
                return true;
            }

            if (item.AcceptableRangeMax.HasValue && answer.NumericValue.Value > item.AcceptableRangeMax.Value)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsEvidenceOnlyItem(string itemType) =>
        string.Equals(itemType, InspectionChecklistItemTypes.Photo, StringComparison.OrdinalIgnoreCase)
        || string.Equals(itemType, InspectionChecklistItemTypes.Signature, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<string> NormalizeSelectedOptions(
        InspectionChecklistItem checklistItem,
        IReadOnlyList<string>? selectedOptions,
        bool allowMultiple)
    {
        var controlledOptions = DeserializeStringList(checklistItem.ControlledOptionsJson);
        if (controlledOptions.Count == 0)
        {
            throw new StlApiException(
                "inspection_run.invalid_select_answer",
                "Selectable checklist items require controlled options.",
                400);
        }

        var normalized = (selectedOptions ?? [])
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
        {
            throw new StlApiException(
                "inspection_run.invalid_select_answer",
                allowMultiple
                    ? "Multi-select checklist items require at least one selected option."
                    : "Select checklist items require exactly one selected option.",
                400);
        }

        if (!allowMultiple && normalized.Count != 1)
        {
            throw new StlApiException(
                "inspection_run.invalid_select_answer",
                "Select checklist items require exactly one selected option.",
                400);
        }

        if (allowMultiple && normalized.Count > 50)
        {
            throw new StlApiException(
                "inspection_run.invalid_select_answer",
                "Multi-select checklist items can have at most 50 selected options.",
                400);
        }

        var controlledOptionMap = controlledOptions.ToDictionary(
            option => option,
            option => option,
            StringComparer.OrdinalIgnoreCase);
        var invalidOption = normalized.FirstOrDefault(option => !controlledOptionMap.ContainsKey(option));
        if (invalidOption is not null)
        {
            throw new StlApiException(
                "inspection_run.invalid_select_answer",
                $"Selected option '{invalidOption}' is not available for this checklist item.",
                400);
        }

        return normalized.Select(option => controlledOptionMap[option]).ToList();
    }

    private static string SerializeStringList(IReadOnlyList<string> values) =>
        JsonSerializer.Serialize(values);

    private static IReadOnlyList<string> DeserializeStringList(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<string[]>(json) ?? [];
}
