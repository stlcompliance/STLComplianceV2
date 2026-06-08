using MaintainArr.Api.Contracts;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed partial class DefectService
{
    public async Task<DefectDetailResponse> CreateDraftAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertDefectDraftRequest request,
        CancellationToken cancellationToken = default,
        string? actorPersonId = null)
    {
        await EnsureActiveAssetAsync(tenantId, request.AssetId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var severity = NormalizeSeverityValue(request.Severity);
        var priority = NormalizePriorityForStorage(request.Priority, MapSeverityToPriority(severity));
        var reportSource = NormalizeReportSource(request.ReportSource) ?? DefectSources.Manual;

        var entity = new Defect
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = request.AssetId,
            Title = NormalizeTextValue(request.Title, 256) ?? string.Empty,
            Description = NormalizeTextValue(request.Description, 1024) ?? string.Empty,
            Severity = severity,
            Priority = priority,
            DefectType = NormalizeOptionalValue(request.DefectType, 128),
            ReportSource = reportSource,
            SourceType = NormalizeOptionalValue(request.SourceType, 64) ?? DefectSources.Manual,
            SourceReferenceId = NormalizeOptionalValue(request.SourceReferenceId, 128),
            IncidentReferenceId = NormalizeOptionalValue(request.IncidentReferenceId, 128),
            Status = DefectStatuses.Draft,
            Source = reportSource,
            ReportedByUserId = actorUserId,
            ReportedByPersonId = NormalizePersonReference(actorPersonId),
            DiscoveredByPersonId = NormalizePersonReference(request.DiscoveredByPersonId),
            CreatedByPersonId = NormalizePersonReference(actorPersonId),
            UpdatedByPersonId = NormalizePersonReference(actorPersonId),
            ReportedAt = request.ReportedAt ?? now,
            DiscoveredAt = request.DiscoveredAt ?? request.ReportedAt ?? now,
            IsSafetyCritical = request.IsSafetyCritical ?? false,
            IsComplianceImpacting = request.IsComplianceImpacting ?? false,
            IsOperabilityImpacting = request.IsOperabilityImpacting ?? false,
            FailureMode = NormalizeOptionalValue(request.FailureMode, 128),
            SystemKey = NormalizeOptionalValue(request.SystemKey, 128),
            ComponentKey = NormalizeOptionalValue(request.ComponentKey, 128),
            Symptom = NormalizeOptionalValue(request.Symptom, 256),
            SidePosition = NormalizeOptionalValue(request.SidePosition, 64),
            OperatingCondition = NormalizeOptionalValue(request.OperatingCondition, 128),
            DeferralCode = NormalizeOptionalValue(request.DeferralCode, 64),
            ReadinessNotes = NormalizeOptionalValue(request.ReadinessNotes, 1024),
            CorrectiveAction = NormalizeOptionalValue(request.CorrectiveAction, 1024),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Defects.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "defect.draft_create",
            tenantId,
            actorUserId,
            "defect",
            entity.Id.ToString(),
            DefectStatuses.Draft,
            cancellationToken: cancellationToken);

        return await MapDetailAsync(tenantId, entity, cancellationToken);
    }

    public async Task<DefectDetailResponse> UpdateDraftAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid defectId,
        UpsertDefectDraftRequest request,
        CancellationToken cancellationToken = default,
        string? actorPersonId = null)
    {
        var defect = await GetDraftDefectForUpdateAsync(tenantId, defectId, cancellationToken);
        ApplyDraftRequest(defect, request, actorPersonId);
        defect.UpdatedAt = DateTimeOffset.UtcNow;
        defect.UpdatedByPersonId = NormalizePersonReference(actorPersonId);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "defect.draft_update",
            tenantId,
            actorUserId,
            "defect",
            defect.Id.ToString(),
            DefectStatuses.Draft,
            cancellationToken: cancellationToken);

        return await MapDetailAsync(tenantId, defect, cancellationToken);
    }

    public async Task<DefectValidationResponse> ValidateDraftAsync(
        Guid tenantId,
        Guid defectId,
        CancellationToken cancellationToken = default)
    {
        var defect = await GetDraftDefectAsync(tenantId, defectId, cancellationToken);
        var findings = await BuildValidationFindingsAsync(tenantId, defect, cancellationToken);
        return new DefectValidationResponse(
            findings.All(f => !string.Equals(f.Severity, "blocker", StringComparison.OrdinalIgnoreCase)),
            findings);
    }

    public async Task<IReadOnlyList<DefectDuplicateMatchResponse>> CheckDuplicateDraftAsync(
        Guid tenantId,
        Guid defectId,
        CancellationToken cancellationToken = default)
    {
        var defect = await GetDraftDefectAsync(tenantId, defectId, cancellationToken);
        return await BuildDuplicateMatchesAsync(tenantId, defect, cancellationToken);
    }

    public async Task<DefectDraftPreviewResponse> PreviewDraftAsync(
        Guid tenantId,
        Guid defectId,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var defect = await GetDraftDefectAsync(tenantId, defectId, cancellationToken);
        var validation = await BuildValidationFindingsAsync(tenantId, defect, cancellationToken);
        var duplicates = await BuildDuplicateMatchesAsync(tenantId, defect, cancellationToken);
        AssetReadinessResponse? assetReadiness = null;
        try
        {
            assetReadiness = await assetReadinessService.GetAsync(tenantId, defect.AssetId, cancellationToken);
        }
        catch (StlApiException)
        {
            assetReadiness = null;
        }
        var canSubmit = validation.All(f => !string.Equals(f.Severity, "blocker", StringComparison.OrdinalIgnoreCase));

        return new DefectDraftPreviewResponse(
            await MapDetailAsync(tenantId, defect, cancellationToken),
            validation,
            duplicates,
            assetReadiness,
            canSubmit,
            canSubmit,
            canSubmit);
    }

    public async Task<DefectSubmissionResponse> SubmitAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid defectId,
        SubmitDefectRequest request,
        CancellationToken cancellationToken = default,
        string? actorPersonId = null)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var defect = await GetDraftDefectForUpdateAsync(tenantId, defectId, cancellationToken);
        await EnsureActiveAssetAsync(tenantId, defect.AssetId, cancellationToken);

        var validation = await BuildValidationFindingsAsync(tenantId, defect, cancellationToken);
        var blockers = validation
            .Where(f => string.Equals(f.Severity, "blocker", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (blockers.Count > 0)
        {
            throw new StlApiException(
                "defect.submit_blocked",
                "Defect draft still has validation blockers.",
                409,
                new Dictionary<string, object?>
                {
                    ["findings"] = blockers,
                });
        }

        var duplicateMatches = await BuildDuplicateMatchesAsync(tenantId, defect, cancellationToken);
        if (duplicateMatches.Count > 0)
        {
            await audit.WriteAsync(
                "defect.duplicate_override",
                tenantId,
                actorUserId,
                "defect",
                defect.Id.ToString(),
                duplicateMatches.Count.ToString(),
                cancellationToken: cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        defect.Status = DefectStatuses.Open;
        defect.ReportedByPersonId ??= NormalizePersonReference(actorPersonId);
        defect.CreatedByPersonId ??= NormalizePersonReference(actorPersonId);
        defect.UpdatedByPersonId = NormalizePersonReference(actorPersonId);
        defect.ReportedAt ??= now;
        defect.DiscoveredAt ??= defect.ReportedAt;
        defect.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        WorkOrderDetailResponse? workOrder = null;
        AssetQualityHoldResponse? assetQualityHold = null;

        if (request.CreateWorkOrder)
        {
            workOrder = await workOrderService.CreateFromDefectAsync(
                tenantId,
                actorUserId,
                defect.Id,
                new CreateWorkOrderFromDefectRequest(
                    request.WorkOrderTitle,
                    request.WorkOrderDescription,
                    request.WorkOrderPriority,
                    NormalizePersonReference(request.WorkOrderAssignedTechnicianPersonId),
                    request.WorkOrderDraftPlanJson,
                    request.WorkOrderPlannedStartAt,
                    request.WorkOrderPlannedDueAt),
                cancellationToken);
        }

        if (request.MarkAssetNotReady)
        {
            var holdTitle = NormalizeOptionalValue(request.HoldTitle, 256)
                ?? $"Defect {defect.Title} requires asset hold";
            var holdDescription = NormalizeOptionalValue(request.HoldDescription, 1024)
                ?? defect.Description;
            var holdSeverity = NormalizeOptionalValue(request.HoldSeverity, 32)
                ?? defect.Severity;

            assetQualityHold = await assetQualityHoldService.CreateAsync(
                tenantId,
                actorUserId,
                new CreateAssetQualityHoldRequest(
                    defect.AssetId,
                    NormalizeOptionalValue(request.HoldType, 64) ?? "defect_hold",
                    NormalizeOptionalValue(request.HoldSourceProduct, 64) ?? "maintainarr",
                    NormalizeOptionalValue(request.HoldSourceObjectRef, 128) ?? defect.Id.ToString("D"),
                    holdTitle,
                    holdDescription,
                    holdSeverity,
                    NormalizePersonReference(request.HoldCreatedByPersonId) ?? NormalizePersonReference(actorPersonId)),
                cancellationToken);
        }

        await audit.WriteAsync(
            "defect.submit",
            tenantId,
            actorUserId,
            "defect",
            defect.Id.ToString(),
            defect.Status,
            cancellationToken: cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new DefectSubmissionResponse(
            await GetAsync(tenantId, defect.Id, cancellationToken),
            workOrder,
            assetQualityHold);
    }

    private async Task<Defect> GetDraftDefectAsync(
        Guid tenantId,
        Guid defectId,
        CancellationToken cancellationToken)
    {
        var defect = await db.Defects
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == defectId, cancellationToken);

        if (defect is null)
        {
            throw new StlApiException("defect.not_found", "Defect was not found.", 404);
        }

        if (!string.Equals(defect.Status, DefectStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "defect.not_draft",
                "Only draft defects can be edited with draft actions.",
                409);
        }

        return defect;
    }

    private async Task<Defect> GetDraftDefectForUpdateAsync(
        Guid tenantId,
        Guid defectId,
        CancellationToken cancellationToken)
    {
        return await GetDraftDefectAsync(tenantId, defectId, cancellationToken);
    }

    private async Task<IReadOnlyList<DefectValidationFindingResponse>> BuildValidationFindingsAsync(
        Guid tenantId,
        Defect defect,
        CancellationToken cancellationToken)
    {
        var findings = new List<DefectValidationFindingResponse>();

        if (defect.AssetId == Guid.Empty)
        {
            findings.Add(new DefectValidationFindingResponse(
                "asset",
                "blocker",
                "defect.asset_required",
                "Asset is required.",
                "assetId",
                "basics",
                "maintainarr"));
        }
        else
        {
            var asset = await db.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == defect.AssetId, cancellationToken);

            if (asset is null)
            {
                findings.Add(new DefectValidationFindingResponse(
                    "asset",
                    "blocker",
                    "defect.asset_not_found",
                    "Selected asset was not found.",
                    "assetId",
                    "basics",
                    "maintainarr"));
            }
            else if (!string.Equals(asset.LifecycleStatus, "active", StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new DefectValidationFindingResponse(
                    "asset",
                    "blocker",
                    "defect.asset_not_active",
                    "Defects can only be submitted for active assets.",
                    "assetId",
                    "basics",
                    "maintainarr"));
            }
        }

        if (string.IsNullOrWhiteSpace(defect.Title))
        {
            findings.Add(new DefectValidationFindingResponse(
                "basics",
                "blocker",
                "defect.title_required",
                "Defect title is required.",
                "title",
                "basics",
                "maintainarr"));
        }
        else if (defect.Title.Trim().Length > 256)
        {
            findings.Add(new DefectValidationFindingResponse(
                "basics",
                "blocker",
                "defect.title_too_long",
                "Defect title must be 256 characters or fewer.",
                "title",
                "basics",
                "maintainarr"));
        }

        if (!DefectSeverities.All.Contains(defect.Severity))
        {
            findings.Add(new DefectValidationFindingResponse(
                "classification",
                "blocker",
                "defect.invalid_severity",
                "Severity must be low, medium, high, or critical.",
                "severity",
                "classification",
                "maintainarr"));
        }

        if (!string.IsNullOrWhiteSpace(defect.Priority)
            && !IsValidPriority(defect.Priority))
        {
            findings.Add(new DefectValidationFindingResponse(
                "classification",
                "blocker",
                "defect.invalid_priority",
                "Priority must be low, medium, high, or urgent.",
                "priority",
                "classification",
                "maintainarr"));
        }

        if (!string.IsNullOrWhiteSpace(defect.SourceType)
            && string.IsNullOrWhiteSpace(defect.SourceReferenceId))
        {
            findings.Add(new DefectValidationFindingResponse(
                "source",
                "warning",
                "defect.source_reference_missing",
                "Source reference id is recommended when a source type is selected.",
                "sourceReferenceId",
                "related",
                "maintainarr"));
        }

        if (defect.Description.Length > 1024)
        {
            findings.Add(new DefectValidationFindingResponse(
                "details",
                "blocker",
                "defect.description_too_long",
                "Description must be 1024 characters or fewer.",
                "description",
                "details",
                "maintainarr"));
        }

        return findings;
    }

    private async Task<IReadOnlyList<DefectDuplicateMatchResponse>> BuildDuplicateMatchesAsync(
        Guid tenantId,
        Defect defect,
        CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeComparisonKey(defect.Title);
        var normalizedSourceType = NormalizeComparisonKey(defect.SourceType);
        var normalizedSourceReferenceId = NormalizeComparisonKey(defect.SourceReferenceId);

        var candidates = await db.Defects
            .AsNoTracking()
            .Include(x => x.Asset)
            .Where(x =>
                x.TenantId == tenantId
                && x.Id != defect.Id
                && !string.Equals(x.Status, DefectStatuses.Closed, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.Status, DefectStatuses.Resolved, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var matches = new List<DefectDuplicateMatchResponse>();
        foreach (var candidate in candidates)
        {
            var sameAsset = candidate.AssetId == defect.AssetId;
            var candidateTitle = NormalizeComparisonKey(candidate.Title);
            var candidateSourceType = NormalizeComparisonKey(candidate.SourceType);
            var candidateSourceReferenceId = NormalizeComparisonKey(candidate.SourceReferenceId);

            var similarityScore = 0;
            var matchReason = string.Empty;

            if (!string.IsNullOrWhiteSpace(normalizedSourceType)
                && normalizedSourceType == candidateSourceType
                && !string.IsNullOrWhiteSpace(normalizedSourceReferenceId)
                && normalizedSourceReferenceId == candidateSourceReferenceId)
            {
                similarityScore = 100;
                matchReason = "Same source reference";
            }
            else if (sameAsset && !string.IsNullOrWhiteSpace(normalizedTitle) && candidateTitle == normalizedTitle)
            {
                similarityScore = 95;
                matchReason = "Same asset and identical title";
            }
            else if (sameAsset
                && !string.IsNullOrWhiteSpace(normalizedTitle)
                && candidateTitle.Contains(normalizedTitle, StringComparison.Ordinal))
            {
                similarityScore = 80;
                matchReason = "Same asset and similar title";
            }
            else if (!string.IsNullOrWhiteSpace(normalizedTitle)
                && normalizedTitle.Contains(candidateTitle, StringComparison.Ordinal)
                && sameAsset)
            {
                similarityScore = 75;
                matchReason = "Same asset and related title";
            }

            if (similarityScore == 0)
            {
                continue;
            }

            matches.Add(new DefectDuplicateMatchResponse(
                candidate.Id,
                candidate.Title,
                candidate.Status,
                candidate.Severity,
                candidate.Asset?.AssetTag ?? string.Empty,
                candidate.Asset?.Name ?? string.Empty,
                matchReason,
                similarityScore));
        }

        return matches
            .OrderByDescending(x => x.SimilarityScore)
            .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }

    private void ApplyDraftRequest(
        Defect defect,
        UpsertDefectDraftRequest request,
        string? actorPersonId)
    {
        if (request.AssetId != Guid.Empty && request.AssetId != defect.AssetId)
        {
            defect.AssetId = request.AssetId;
        }

        if (request.Title is not null)
        {
            defect.Title = NormalizeTextValue(request.Title, 256) ?? string.Empty;
        }

        if (request.Description is not null)
        {
            defect.Description = NormalizeTextValue(request.Description, 1024) ?? string.Empty;
        }

        if (request.Severity is not null)
        {
            defect.Severity = NormalizeSeverityValue(request.Severity);
            defect.Priority = NormalizePriorityForStorage(request.Priority, MapSeverityToPriority(defect.Severity));
        }
        else if (request.Priority is not null)
        {
            defect.Priority = NormalizePriorityForStorage(request.Priority, defect.Priority);
        }

        if (request.DefectType is not null)
        {
            defect.DefectType = NormalizeOptionalValue(request.DefectType, 128);
        }

        if (request.ReportSource is not null)
        {
            var reportSource = NormalizeReportSource(request.ReportSource) ?? DefectSources.Manual;
            defect.ReportSource = reportSource;
            defect.Source = reportSource;
        }

        if (request.SourceType is not null)
        {
            defect.SourceType = NormalizeOptionalValue(request.SourceType, 64);
        }

        if (request.SourceReferenceId is not null)
        {
            defect.SourceReferenceId = NormalizeOptionalValue(request.SourceReferenceId, 128);
        }

        if (request.IncidentReferenceId is not null)
        {
            defect.IncidentReferenceId = NormalizeOptionalValue(request.IncidentReferenceId, 128);
        }

        if (request.ReportedAt.HasValue)
        {
            defect.ReportedAt = request.ReportedAt;
        }

        if (request.DiscoveredAt.HasValue)
        {
            defect.DiscoveredAt = request.DiscoveredAt;
        }

        if (request.ReportedByPersonId is not null)
        {
            defect.ReportedByPersonId = NormalizePersonReference(request.ReportedByPersonId);
        }
        else if (string.IsNullOrWhiteSpace(defect.ReportedByPersonId))
        {
            defect.ReportedByPersonId = NormalizePersonReference(actorPersonId);
        }

        if (request.DiscoveredByPersonId is not null)
        {
            defect.DiscoveredByPersonId = NormalizePersonReference(request.DiscoveredByPersonId);
        }

        if (request.FailureMode is not null)
        {
            defect.FailureMode = NormalizeOptionalValue(request.FailureMode, 128);
        }

        if (request.SystemKey is not null)
        {
            defect.SystemKey = NormalizeOptionalValue(request.SystemKey, 128);
        }

        if (request.ComponentKey is not null)
        {
            defect.ComponentKey = NormalizeOptionalValue(request.ComponentKey, 128);
        }

        if (request.Symptom is not null)
        {
            defect.Symptom = NormalizeOptionalValue(request.Symptom, 256);
        }

        if (request.SidePosition is not null)
        {
            defect.SidePosition = NormalizeOptionalValue(request.SidePosition, 64);
        }

        if (request.OperatingCondition is not null)
        {
            defect.OperatingCondition = NormalizeOptionalValue(request.OperatingCondition, 128);
        }

        if (request.DeferralCode is not null)
        {
            defect.DeferralCode = NormalizeOptionalValue(request.DeferralCode, 64);
        }

        if (request.IsSafetyCritical.HasValue)
        {
            defect.IsSafetyCritical = request.IsSafetyCritical.Value;
        }

        if (request.IsComplianceImpacting.HasValue)
        {
            defect.IsComplianceImpacting = request.IsComplianceImpacting.Value;
        }

        if (request.IsOperabilityImpacting.HasValue)
        {
            defect.IsOperabilityImpacting = request.IsOperabilityImpacting.Value;
        }

        if (request.ReadinessNotes is not null)
        {
            defect.ReadinessNotes = NormalizeOptionalValue(request.ReadinessNotes, 1024);
        }

        if (request.CorrectiveAction is not null)
        {
            defect.CorrectiveAction = NormalizeOptionalValue(request.CorrectiveAction, 1024);
        }

        if (string.IsNullOrWhiteSpace(defect.SourceType))
        {
            defect.SourceType = DefectSources.Manual;
        }

        if (string.IsNullOrWhiteSpace(defect.ReportSource))
        {
            defect.ReportSource = DefectSources.Manual;
            defect.Source = DefectSources.Manual;
        }

        defect.Priority = NormalizePriorityForStorage(defect.Priority, MapSeverityToPriority(defect.Severity));
    }

    private static string NormalizeSeverityValue(string? severity)
    {
        if (string.IsNullOrWhiteSpace(severity))
        {
            return DefectSeverities.Medium;
        }

        ValidateSeverity(severity);
        return NormalizeSeverity(severity);
    }

    private static bool IsValidPriority(string priority) =>
        string.Equals(NormalizePriority(priority), WorkOrderPriorities.Low, StringComparison.OrdinalIgnoreCase)
        || string.Equals(NormalizePriority(priority), WorkOrderPriorities.Medium, StringComparison.OrdinalIgnoreCase)
        || string.Equals(NormalizePriority(priority), WorkOrderPriorities.High, StringComparison.OrdinalIgnoreCase)
        || string.Equals(NormalizePriority(priority), WorkOrderPriorities.Urgent, StringComparison.OrdinalIgnoreCase);

    private static string NormalizePriorityForStorage(string? priority, string fallback)
    {
        if (string.IsNullOrWhiteSpace(priority))
        {
            return fallback;
        }

        var normalized = NormalizePriority(priority);
        if (IsValidPriority(normalized))
        {
            return normalized;
        }

        return priority.Trim();
    }

    private static string? NormalizeReportSource(string? reportSource)
    {
        if (string.IsNullOrWhiteSpace(reportSource))
        {
            return null;
        }

        return NormalizeOptionalValue(reportSource, 64);
    }

    private static string? NormalizePersonReference(string? personId)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return null;
        }

        return NormalizeOptionalValue(personId, 128);
    }

    private static string? NormalizeOptionalValue(string? value, int maxLength)
    {
        if (value is null)
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException(
                "defect.validation",
                $"Value must be {maxLength} characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string? NormalizeTextValue(string? value, int maxLength)
    {
        if (value is null)
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException(
                "defect.validation",
                $"Value must be {maxLength} characters or fewer.",
                400);
        }

        return normalized;
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

    private static string MapSeverityToPriority(string severity) =>
        severity.Trim().ToLowerInvariant() switch
        {
            DefectSeverities.Critical => WorkOrderPriorities.Urgent,
            DefectSeverities.High => WorkOrderPriorities.High,
            DefectSeverities.Low => WorkOrderPriorities.Low,
            _ => WorkOrderPriorities.Medium,
        };

    private static string NormalizeComparisonKey(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
}
