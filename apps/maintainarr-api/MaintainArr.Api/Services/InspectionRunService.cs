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
    IMaintainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedPassFailValues = new(StringComparer.OrdinalIgnoreCase)
    {
        InspectionAnswerPassFailValues.Pass,
        InspectionAnswerPassFailValues.Fail,
        InspectionAnswerPassFailValues.Na,
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
        CancellationToken cancellationToken = default)
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

        if (request.Answers.Count == 0)
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

        var failed = false;

        foreach (var item in checklistItems)
        {
            answers.TryGetValue(item.Id, out var answer);

            if (item.IsRequired && !HasAnswer(item, answer))
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
                item.IsRequired,
                item.SortOrder))
            .ToList();

        var answerEntities = await db.InspectionRunAnswers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionRunId == run.Id)
            .ToListAsync(cancellationToken);

        var itemKeys = checklistItemEntities.ToDictionary(x => x.Id, x => x.ItemKey);

        var answers = answerEntities
            .Select(answer => new InspectionRunAnswerResponse(
                answer.Id,
                answer.ChecklistItemId,
                itemKeys.GetValueOrDefault(answer.ChecklistItemId, string.Empty),
                answer.PassFailValue,
                answer.NumericValue,
                answer.TextValue,
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

    private static (string? PassFailValue, decimal? NumericValue, string? TextValue) NormalizeAnswer(
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

            return (normalized, null, null);
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

            return (null, input.NumericValue.Value, null);
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

        return (null, null, text);
    }

    private static bool HasAnswer(InspectionChecklistItem item, InspectionRunAnswer? answer)
    {
        if (answer is null)
        {
            return false;
        }

        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.PassFail, StringComparison.OrdinalIgnoreCase))
        {
            return !string.IsNullOrWhiteSpace(answer.PassFailValue);
        }

        if (string.Equals(item.ItemType, InspectionChecklistItemTypes.Numeric, StringComparison.OrdinalIgnoreCase))
        {
            return answer.NumericValue.HasValue;
        }

        return !string.IsNullOrWhiteSpace(answer.TextValue);
    }

    private static bool IsFailedAnswer(InspectionChecklistItem item, InspectionRunAnswer answer)
    {
        if (!string.Equals(item.ItemType, InspectionChecklistItemTypes.PassFail, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(answer.PassFailValue, InspectionAnswerPassFailValues.Fail, StringComparison.OrdinalIgnoreCase);
    }
}
