using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class TheoreticalSituationService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TheoreticalSituationResponse> CreateAsync(
        Guid tenantId,
        Guid personId,
        CreateTheoreticalSituationRequest request,
        CancellationToken cancellationToken = default)
    {
        var kind = NormalizeSituationKind(request.SituationKind);
        var option = SituationKindOptions.First(x => x.Key == kind);
        var now = DateTimeOffset.UtcNow;
        var situation = new TheoreticalSituation
        {
            SituationId = Guid.NewGuid(),
            TenantId = tenantId,
            CreatedByPersonId = personId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? option.Label : request.Title.Trim(),
            SituationKind = kind,
            Status = TheoreticalSituationStatuses.Draft,
            EvaluationMode = TheoreticalEvaluationModes.WhatIf,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.TheoreticalSituations.Add(situation);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "theoretical_situation.create",
            tenantId,
            personId,
            "theoretical_situation",
            situation.SituationId.ToString(),
            "success",
            reasonCode: kind,
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, situation.SituationId, cancellationToken);
    }

    public async Task<IReadOnlyList<TheoreticalSituationListItemResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var situations = await db.TheoreticalSituations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status != TheoreticalSituationStatuses.Archived)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);
        var ids = situations.Select(x => x.SituationId).ToList();
        var evaluations = await db.TheoreticalSituationEvaluations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.SituationId))
            .GroupBy(x => x.SituationId)
            .Select(x => new { SituationId = x.Key, Result = x.OrderByDescending(e => e.EvaluatedAt).First().Result })
            .ToListAsync(cancellationToken);
        var latest = evaluations.ToDictionary(x => x.SituationId, x => x.Result);

        return situations.Select(x => new TheoreticalSituationListItemResponse(
            x.SituationId,
            x.Title,
            x.SituationKind,
            x.Status,
            x.SavedAsTemplate,
            x.CreatedAt,
            x.UpdatedAt,
            latest.GetValueOrDefault(x.SituationId))).ToList();
    }

    public async Task<TheoreticalSituationResponse> GetAsync(
        Guid tenantId,
        Guid situationId,
        CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken);
        var context = await db.TheoreticalSituationContexts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .OrderBy(x => x.ContextKey)
            .Select(x => MapContext(x))
            .ToListAsync(cancellationToken);
        var facts = await db.TheoreticalSituationFacts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId && x.Active)
            .OrderBy(x => x.RequirementKey)
            .ThenBy(x => x.FactKey)
            .Select(x => MapFact(x))
            .ToListAsync(cancellationToken);
        var incidents = await db.TheoreticalSituationIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => MapIncident(x))
            .ToListAsync(cancellationToken);
        var latest = await db.TheoreticalSituationEvaluations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .OrderByDescending(x => x.EvaluatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new TheoreticalSituationResponse(
            situation.SituationId,
            situation.TenantId,
            situation.CreatedByPersonId,
            situation.Title,
            situation.SituationKind,
            situation.Status,
            situation.EvaluationMode,
            situation.SavedAsTemplate,
            situation.CreatedAt,
            situation.UpdatedAt,
            context,
            facts,
            incidents,
            latest is null ? null : await MapEvaluationAsync(latest, cancellationToken));
    }

    public async Task<TheoreticalSituationResponse> UpdateAsync(
        Guid tenantId,
        Guid situationId,
        UpdateTheoreticalSituationRequest request,
        CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken, tracking: true);
        if (!string.IsNullOrWhiteSpace(request.SituationKind))
        {
            situation.SituationKind = NormalizeSituationKind(request.SituationKind);
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            situation.Title = request.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToLowerInvariant();
            if (status is not (TheoreticalSituationStatuses.Draft or TheoreticalSituationStatuses.Evaluated or TheoreticalSituationStatuses.Template or TheoreticalSituationStatuses.Archived))
            {
                throw new StlApiException("theoretical_situations.invalid_status", "Situation status is not supported.", 400);
            }

            situation.Status = status;
        }

        situation.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(tenantId, situationId, cancellationToken);
    }

    public async Task DeleteAsync(Guid tenantId, Guid situationId, CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken, tracking: true);
        situation.Status = TheoreticalSituationStatuses.Archived;
        situation.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<TheoreticalOptionResponse>> GetSituationKindsAsync() =>
        Task.FromResult<IReadOnlyList<TheoreticalOptionResponse>>(SituationKindOptions);

    public Task<IReadOnlyList<TheoreticalContextFieldResponse>> GetContextFieldsAsync(string? situationKind = null)
    {
        var kind = string.IsNullOrWhiteSpace(situationKind) ? null : NormalizeSituationKind(situationKind);
        var fields = ContextFields
            .Where(x => kind is null || x.SituationKinds.Count == 0 || x.SituationKinds.Contains(kind))
            .ToList();
        return Task.FromResult<IReadOnlyList<TheoreticalContextFieldResponse>>(fields);
    }

    public Task<IReadOnlyList<TheoreticalOptionResponse>> GetContextValuesAsync(string contextKey)
    {
        var field = ContextFields.FirstOrDefault(x => string.Equals(x.ContextKey, contextKey, StringComparison.OrdinalIgnoreCase));
        if (field is null)
        {
            throw new StlApiException("theoretical_situations.context_not_found", "Context field is not supported.", 404);
        }

        return Task.FromResult(field.Values);
    }

    public async Task<IReadOnlyList<TheoreticalOptionResponse>> GetDocumentTypesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var optionKeys = await db.ComplianceEvidenceOptions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Active && x.DocumentTypeKey != string.Empty)
            .Select(x => new { Key = x.DocumentTypeKey, Label = x.OptionLabel })
            .ToListAsync(cancellationToken);
        var references = await db.DocumentReferences.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Active)
            .Select(x => new { Key = x.StableKey, x.Label })
            .ToListAsync(cancellationToken);

        return optionKeys.Concat(references)
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => new TheoreticalOptionResponse(x.Key, PreferLabel(x.First().Label, x.Key), "Document type or document reference known to Compliance Core.", "document"))
            .OrderBy(x => x.Label)
            .ToList();
    }

    public Task<IReadOnlyList<TheoreticalOptionResponse>> GetEvidenceStatesAsync() =>
        Task.FromResult<IReadOnlyList<TheoreticalOptionResponse>>(EvidenceStateOptions);

    public async Task<IReadOnlyList<TheoreticalOptionResponse>> GetExceptionExemptionOptionsAsync(
        Guid tenantId,
        string? requirementKey = null,
        string? packKey = null,
        string? citationKey = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.ComplianceExceptionExemptions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Active);

        if (!string.IsNullOrWhiteSpace(packKey))
        {
            query = query.Where(x => x.PackKey == packKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(citationKey))
        {
            query = query.Where(x => x.CitationKey == citationKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(requirementKey))
        {
            var normalized = NormalizeOptionalKey(requirementKey);
            query = query.Where(x => x.ApplicabilityKey == normalized || x.ConditionLogicJson.Contains(normalized));
        }

        return await query
            .OrderBy(x => x.Label)
            .Select(x => new TheoreticalOptionResponse(
                x.Key,
                x.Label,
                x.Description == string.Empty ? x.EffectType : x.Description,
                "exception_exemption"))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TheoreticalEvidenceOptionResponse>> GetEvidenceOptionsAsync(
        Guid tenantId,
        string? requirementKey = null,
        CancellationToken cancellationToken = default)
    {
        var query =
            from option in db.ComplianceEvidenceOptions.AsNoTracking()
            join groupRow in db.ComplianceEvidenceOptionGroups.AsNoTracking()
                on option.EvidenceOptionGroupId equals groupRow.EvidenceOptionGroupId
            where option.TenantId == tenantId && option.Active && groupRow.Active
            select new { option, groupRow };

        if (!string.IsNullOrWhiteSpace(requirementKey))
        {
            var normalized = NormalizeKey(requirementKey);
            query = query.Where(x => x.groupRow.RequirementKey == normalized);
        }

        return await query
            .OrderBy(x => x.groupRow.RequirementKey)
            .ThenBy(x => x.option.Priority)
            .Select(x => new TheoreticalEvidenceOptionResponse(
                x.option.EvidenceOptionId,
                x.option.OptionKey,
                x.option.OptionLabel,
                x.groupRow.LogicType,
                x.groupRow.RequirementKey,
                x.groupRow.FactKey,
                x.option.EvidenceKind,
                x.option.TargetKind,
                x.option.SourceProduct,
                x.option.SourceEntity,
                x.option.Required))
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<TheoreticalOptionResponse>> GetIncidentOptionsAsync() =>
        Task.FromResult<IReadOnlyList<TheoreticalOptionResponse>>(IncidentOptions);

    public Task<IReadOnlyList<TheoreticalOptionResponse>> GetMaterialClassesAsync() =>
        Task.FromResult<IReadOnlyList<TheoreticalOptionResponse>>(MaterialClassOptions);

    public async Task<IReadOnlyList<TheoreticalOptionResponse>> GetSystemOptionsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await ReferenceOptionsAsync(db.SystemReferences.AsNoTracking().Where(x => x.TenantId == tenantId && x.Active), "system", cancellationToken);

    public async Task<IReadOnlyList<TheoreticalOptionResponse>> GetPartOptionsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await ReferenceOptionsAsync(db.PartReferences.AsNoTracking().Where(x => x.TenantId == tenantId && x.Active), "part", cancellationToken);

    public async Task<IReadOnlyList<TheoreticalOptionResponse>> GetAssetOptionsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await ReferenceOptionsAsync(db.AssetReferences.AsNoTracking().Where(x => x.TenantId == tenantId && x.Active), "asset", cancellationToken);

    public async Task<TheoreticalNextContextResponse> GetNextContextAsync(
        Guid tenantId,
        Guid situationId,
        CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken);
        var existing = await ContextDictionaryAsync(tenantId, situationId, cancellationToken);
        var fields = ContextFields
            .Where(x => x.SituationKinds.Count == 0 || x.SituationKinds.Contains(situation.SituationKind))
            .Where(x => x.Required && !existing.ContainsKey(x.ContextKey))
            .Take(2)
            .ToList();

        return new TheoreticalNextContextResponse(
            fields,
            fields.Count == 0,
            fields.Count == 0
                ? "Enough structured context is present to resolve applicability."
                : "Answer the next focused context question before resolving applicability.");
    }

    public async Task<IReadOnlyList<TheoreticalSituationContextResponse>> UpsertContextAsync(
        Guid tenantId,
        Guid situationId,
        TheoreticalSituationContextRequest request,
        CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken, tracking: true);
        var now = DateTimeOffset.UtcNow;
        foreach (var value in request.Values)
        {
            var field = ContextFields.FirstOrDefault(x => string.Equals(x.ContextKey, value.ContextKey, StringComparison.OrdinalIgnoreCase));
            if (field is null || (field.SituationKinds.Count > 0 && !field.SituationKinds.Contains(situation.SituationKind)))
            {
                throw new StlApiException("theoretical_situations.context_not_supported", "Context field is not supported for this situation.", 400);
            }

            var option = field.Values.FirstOrDefault(x => string.Equals(x.Key, value.ContextValueKey, StringComparison.OrdinalIgnoreCase));
            if (option is null)
            {
                throw new StlApiException("theoretical_situations.context_value_not_supported", "Context value is not supported.", 400);
            }

            var context = await db.TheoreticalSituationContexts.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.SituationId == situationId && x.ContextKey == field.ContextKey,
                cancellationToken);
            if (context is null)
            {
                context = new TheoreticalSituationContext
                {
                    ContextId = Guid.NewGuid(),
                    TenantId = tenantId,
                    SituationId = situationId,
                    ContextKey = field.ContextKey,
                    CreatedAt = now
                };
                db.TheoreticalSituationContexts.Add(context);
            }

            context.ContextLabel = field.Label;
            context.ContextValueKey = option.Key;
            context.ContextValueLabel = option.Label;
            context.ControlledVocabularyType = field.ControlledVocabularyType;
            context.Confidence = 1m;
        }

        situation.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return await db.TheoreticalSituationContexts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .OrderBy(x => x.ContextKey)
            .Select(x => MapContext(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TheoreticalSituationFactResponse>> UpsertFactsAsync(
        Guid tenantId,
        Guid situationId,
        TheoreticalSituationFactRequest request,
        CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken, tracking: true);
        var now = DateTimeOffset.UtcNow;
        foreach (var value in request.Facts)
        {
            var factKey = NormalizeKey(value.FactKey);
            var state = NormalizeSimulatedState(value.SimulatedState);
            var requirementKey = NormalizeOptionalKey(value.RequirementKey);
            var evidenceOptionKey = NormalizeOptionalKey(value.EvidenceOptionKey);
            var existing = await db.TheoreticalSituationFacts.FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.SituationId == situationId
                    && x.FactKey == factKey
                    && x.RequirementKey == requirementKey
                    && x.EvidenceOptionKey == evidenceOptionKey,
                cancellationToken);
            if (existing is null)
            {
                existing = new TheoreticalSituationFact
                {
                    SituationFactId = Guid.NewGuid(),
                    TenantId = tenantId,
                    SituationId = situationId,
                    FactKey = factKey,
                    RequirementKey = requirementKey,
                    EvidenceOptionKey = evidenceOptionKey,
                    CreatedAt = now
                };
                db.TheoreticalSituationFacts.Add(existing);
            }

            existing.CitationKey = NormalizeOptionalKey(value.CitationKey);
            existing.PackKey = NormalizeOptionalKey(value.PackKey);
            existing.SimulatedValue = value.SimulatedValue?.Trim() ?? string.Empty;
            existing.ValueType = NormalizeValueType(value.ValueType);
            existing.SimulatedState = state;
            existing.EvidenceKind = NormalizeEvidenceKind(value.EvidenceKind);
            existing.TargetKind = NormalizeTargetKind(value.TargetKind);
            existing.Active = true;
        }

        situation.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return await db.TheoreticalSituationFacts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId && x.Active)
            .OrderBy(x => x.RequirementKey)
            .ThenBy(x => x.FactKey)
            .Select(x => MapFact(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TheoreticalSituationIncidentResponse>> UpsertIncidentsAsync(
        Guid tenantId,
        Guid situationId,
        TheoreticalSituationIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken, tracking: true);
        var now = DateTimeOffset.UtcNow;
        db.TheoreticalSituationIncidents.RemoveRange(db.TheoreticalSituationIncidents.Where(x => x.TenantId == tenantId && x.SituationId == situationId));
        foreach (var value in request.Incidents)
        {
            if (!IncidentOptions.Any(x => string.Equals(x.Key, value.IncidentTypeKey, StringComparison.OrdinalIgnoreCase)))
            {
                throw new StlApiException("theoretical_situations.incident_not_supported", "Incident type is not supported.", 400);
            }

            db.TheoreticalSituationIncidents.Add(new TheoreticalSituationIncident
            {
                SituationIncidentId = Guid.NewGuid(),
                TenantId = tenantId,
                SituationId = situationId,
                IncidentTypeKey = NormalizeKey(value.IncidentTypeKey),
                SeverityKey = NormalizeOptionalKey(value.SeverityKey),
                InvolvedSubjectKind = NormalizeOptionalKey(value.InvolvedSubjectKind),
                InvolvedSubjectState = NormalizeOptionalKey(value.InvolvedSubjectState),
                TriggerKey = NormalizeOptionalKey(value.TriggerKey),
                TriggerValue = NormalizeOptionalKey(value.TriggerValue),
                ReportabilityState = NormalizeOptionalKey(value.ReportabilityState),
                RemediationState = NormalizeOptionalKey(value.RemediationState),
                CreatedAt = now
            });
        }

        situation.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return await db.TheoreticalSituationIncidents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => MapIncident(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TheoreticalApplicabilityResultResponse>> ResolveApplicabilityAsync(
        Guid tenantId,
        Guid situationId,
        CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken, tracking: true);
        var context = await ContextDictionaryAsync(tenantId, situationId, cancellationToken);
        var incidents = await db.TheoreticalSituationIncidents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .ToListAsync(cancellationToken);
        var rows = await QueryRequirementUniverse(tenantId).ToListAsync(cancellationToken);

        db.TheoreticalApplicabilityResults.RemoveRange(db.TheoreticalApplicabilityResults.Where(x => x.TenantId == tenantId && x.SituationId == situationId));
        var now = DateTimeOffset.UtcNow;
        var results = rows.Select(row => BuildApplicability(tenantId, situation.SituationId, situation.SituationKind, context, incidents, row, now)).ToList();
        db.TheoreticalApplicabilityResults.AddRange(results);
        situation.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return results
            .OrderBy(x => x.UserVisiblePriority)
            .ThenByDescending(x => x.ApplicabilityScore)
            .Select(MapApplicability)
            .ToList();
    }

    public async Task<IReadOnlyList<TheoreticalApplicabilityResultResponse>> GetApplicabilityResultsAsync(
        Guid tenantId,
        Guid situationId,
        CancellationToken cancellationToken = default)
    {
        await RequireSituationAsync(tenantId, situationId, cancellationToken);
        return await db.TheoreticalApplicabilityResults.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .OrderBy(x => x.UserVisiblePriority)
            .ThenByDescending(x => x.ApplicabilityScore)
            .Select(x => MapApplicability(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TheoreticalSituationEvaluationResponse> EvaluateAsync(
        Guid tenantId,
        Guid situationId,
        Guid personId,
        TheoreticalEvaluateRequest request,
        CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken, tracking: true);
        if (!await db.TheoreticalApplicabilityResults.AnyAsync(x => x.TenantId == tenantId && x.SituationId == situationId, cancellationToken))
        {
            await ResolveApplicabilityAsync(tenantId, situationId, cancellationToken);
        }

        var applicability = await db.TheoreticalApplicabilityResults.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .ToListAsync(cancellationToken);
        var applicableKeys = applicability
            .Where(x => x.ApplicabilityBand is TheoreticalApplicabilityBands.Primary or TheoreticalApplicabilityBands.Likely
                || (request.IncludePossible && x.ApplicabilityBand == TheoreticalApplicabilityBands.Possible))
            .Select(x => new RequirementKey(x.PackKey, x.CitationKey))
            .ToList();
        var rows = (await QueryRequirementUniverse(tenantId).ToListAsync(cancellationToken))
            .Where(row => applicableKeys.Any(key => key.Matches(row.Pack?.PackKey ?? string.Empty, row.Citation?.CitationKey ?? string.Empty)))
            .OrderBy(row => IsDerived(row.Requirement) ? 1 : 0)
            .ThenBy(row => row.Requirement.RequirementKey)
            .ToList();
        var facts = await db.TheoreticalSituationFacts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId && x.Active)
            .ToListAsync(cancellationToken);
        var optionGroups = await db.ComplianceEvidenceOptionGroups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Active)
            .ToListAsync(cancellationToken);
        var options = await db.ComplianceEvidenceOptions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Active)
            .ToListAsync(cancellationToken);
        var exceptionExemptions = await db.ComplianceExceptionExemptions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Active)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var componentResults = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var details = new List<TheoreticalSituationEvaluationDetail>();
        foreach (var row in rows)
        {
            var detail = BuildEvaluationDetail(tenantId, row, facts, optionGroups, options, exceptionExemptions, componentResults, now);
            componentResults[row.Definition.FactKey] = IsPositiveResult(detail.Result);
            details.Add(detail);
        }

        var result = DetermineOverallResult(details);
        var evaluation = new TheoreticalSituationEvaluation
        {
            EvaluationId = Guid.NewGuid(),
            TenantId = tenantId,
            SituationId = situationId,
            EvaluatedAt = now,
            EvaluatedByPersonId = personId,
            Result = result,
            Summary = BuildSummary(result, situation.SituationKind, details),
            PrimaryProgramsJson = Serialize(applicability.Where(x => x.ApplicabilityBand == TheoreticalApplicabilityBands.Primary).Select(x => x.ProgramKey).Distinct().ToList()),
            LikelyProgramsJson = Serialize(applicability.Where(x => x.ApplicabilityBand == TheoreticalApplicabilityBands.Likely).Select(x => x.ProgramKey).Distinct().ToList()),
            EdgeCasesJson = Serialize(applicability.Where(x => x.EdgeCase).OrderBy(x => x.UserVisiblePriority).Select(x => x.EdgeCaseReason).Where(x => x != string.Empty).Distinct().Take(8).ToList()),
            PassCount = details.Count(x => x.Result == TheoreticalEvaluationResults.Compliant),
            FailCount = details.Count(x => x.Result == TheoreticalEvaluationResults.NotCompliant),
            WarningCount = details.Count(x => x.Result == TheoreticalEvaluationResults.AllowedWithWarning),
            BlockedCount = details.Count(x => x.Result is TheoreticalEvaluationResults.Blocked or TheoreticalEvaluationResults.OverrideNotAllowed),
            NotApplicableCount = details.Count(x => x.Result == TheoreticalEvaluationResults.NotApplicable),
            UnknownCount = details.Count(x => x.Result == TheoreticalEvaluationResults.InsufficientInformation),
            OverrideAvailableCount = details.Count(x => x.Result == TheoreticalEvaluationResults.AllowedWithOverride),
            OverrideBlockedCount = details.Count(x => x.Result == TheoreticalEvaluationResults.OverrideNotAllowed)
        };

        foreach (var detail in details)
        {
            detail.EvaluationId = evaluation.EvaluationId;
        }

        db.TheoreticalSituationEvaluations.Add(evaluation);
        db.TheoreticalSituationEvaluationDetails.AddRange(details);
        situation.Status = TheoreticalSituationStatuses.Evaluated;
        situation.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "theoretical_situation.evaluated",
            tenantId,
            personId,
            "theoretical_situation",
            situationId.ToString(),
            "success",
            reasonCode: result,
            cancellationToken: cancellationToken);

        return await MapEvaluationAsync(evaluation, cancellationToken);
    }

    public async Task<IReadOnlyList<TheoreticalSituationEvaluationResponse>> ListEvaluationsAsync(
        Guid tenantId,
        Guid situationId,
        CancellationToken cancellationToken = default)
    {
        await RequireSituationAsync(tenantId, situationId, cancellationToken);
        var evaluations = await db.TheoreticalSituationEvaluations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .OrderByDescending(x => x.EvaluatedAt)
            .ToListAsync(cancellationToken);
        var result = new List<TheoreticalSituationEvaluationResponse>();
        foreach (var evaluation in evaluations)
        {
            result.Add(await MapEvaluationAsync(evaluation, cancellationToken));
        }

        return result;
    }

    public async Task<TheoreticalSituationEvaluationResponse> GetEvaluationAsync(
        Guid tenantId,
        Guid situationId,
        Guid evaluationId,
        CancellationToken cancellationToken = default)
    {
        await RequireSituationAsync(tenantId, situationId, cancellationToken);
        var evaluation = await db.TheoreticalSituationEvaluations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SituationId == situationId && x.EvaluationId == evaluationId, cancellationToken)
            ?? throw new StlApiException("theoretical_situations.evaluation_not_found", "Theoretical evaluation was not found.", 404);
        return await MapEvaluationAsync(evaluation, cancellationToken);
    }

    public async Task<TheoreticalSituationResponse> SaveTemplateAsync(
        Guid tenantId,
        Guid situationId,
        CancellationToken cancellationToken = default)
    {
        var situation = await RequireSituationAsync(tenantId, situationId, cancellationToken, tracking: true);
        situation.SavedAsTemplate = true;
        situation.Status = TheoreticalSituationStatuses.Template;
        situation.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(tenantId, situationId, cancellationToken);
    }

    public async Task<TheoreticalSituationResponse> CreateFromTemplateAsync(
        Guid tenantId,
        Guid personId,
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await RequireSituationAsync(tenantId, templateId, cancellationToken);
        if (!template.SavedAsTemplate)
        {
            throw new StlApiException("theoretical_situations.not_template", "The selected situation is not saved as a template.", 400);
        }

        var now = DateTimeOffset.UtcNow;
        var situation = new TheoreticalSituation
        {
            SituationId = Guid.NewGuid(),
            TenantId = tenantId,
            CreatedByPersonId = personId,
            Title = $"{template.Title} copy",
            SituationKind = template.SituationKind,
            Status = TheoreticalSituationStatuses.Draft,
            EvaluationMode = template.EvaluationMode,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.TheoreticalSituations.Add(situation);

        var contexts = await db.TheoreticalSituationContexts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == templateId)
            .ToListAsync(cancellationToken);
        db.TheoreticalSituationContexts.AddRange(contexts.Select(x => new TheoreticalSituationContext
        {
            ContextId = Guid.NewGuid(),
            TenantId = tenantId,
            SituationId = situation.SituationId,
            ContextKey = x.ContextKey,
            ContextLabel = x.ContextLabel,
            ContextValueKey = x.ContextValueKey,
            ContextValueLabel = x.ContextValueLabel,
            ControlledVocabularyType = x.ControlledVocabularyType,
            Confidence = x.Confidence,
            CreatedAt = now
        }));

        var facts = await db.TheoreticalSituationFacts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == templateId && x.Active)
            .ToListAsync(cancellationToken);
        db.TheoreticalSituationFacts.AddRange(facts.Select(x => new TheoreticalSituationFact
        {
            SituationFactId = Guid.NewGuid(),
            TenantId = tenantId,
            SituationId = situation.SituationId,
            FactKey = x.FactKey,
            RequirementKey = x.RequirementKey,
            CitationKey = x.CitationKey,
            PackKey = x.PackKey,
            SimulatedValue = x.SimulatedValue,
            ValueType = x.ValueType,
            SimulatedState = x.SimulatedState,
            EvidenceOptionKey = x.EvidenceOptionKey,
            EvidenceKind = x.EvidenceKind,
            TargetKind = x.TargetKind,
            Active = true,
            CreatedAt = now
        }));

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(tenantId, situation.SituationId, cancellationToken);
    }

    private TheoreticalApplicabilityResult BuildApplicability(
        Guid tenantId,
        Guid situationId,
        string situationKind,
        IReadOnlyDictionary<string, string> context,
        IReadOnlyList<TheoreticalSituationIncident> incidents,
        RequirementProjection row,
        DateTimeOffset now)
    {
        var kind = SituationKindProfiles.GetValueOrDefault(situationKind) ?? SituationKindProfiles["custom_theoretical_situation"];
        var haystack = BuildSearchText(row);
        var score = 0m;
        var reasons = new List<string>();
        var exclusions = new List<string>();
        var missing = new List<string>();
        var edgeCase = false;
        var edgeCaseReason = string.Empty;

        if (kind.Keywords.Any(keyword => haystack.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.28m;
            reasons.Add("Situation kind matches requirement language.");
        }

        if (kind.SubjectTypes.Any(subject => row.Requirement.SourceEntity.Contains(subject, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.18m;
            reasons.Add("Subject type matches the source entity.");
        }

        if (kind.Products.Any(product => row.Requirement.SourceProduct.Contains(product, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.16m;
            reasons.Add("Source product/domain matches the situation.");
        }

        if (kind.EvidenceKinds.Contains(row.Requirement.EvidenceKind, StringComparer.OrdinalIgnoreCase))
        {
            score += 0.12m;
            reasons.Add("Evidence kind matches the situation.");
        }

        if (context.TryGetValue("commercial_motor_vehicle_operation", out var cmv) && cmv == "yes"
            && ContainsAny(haystack, "fmcsa", "motor carrier", "commercial motor", "driver", "vehicle", "dispatch"))
        {
            score += 0.15m;
            reasons.Add("Commercial motor vehicle context matches the rule universe.");
        }
        else if (!context.ContainsKey("commercial_motor_vehicle_operation") && kind.RequiresCmvContext)
        {
            missing.Add("commercial_motor_vehicle_operation");
        }

        if (context.TryGetValue("hazmat_involved", out var hazmat))
        {
            if (hazmat == "yes" && ContainsAny(haystack, "hazmat", "hazardous", "phmsa", "placard", "shipping paper", "material"))
            {
                score += 0.24m;
                reasons.Add("Hazmat context matches hazardous-material requirements.");
            }
            else if (hazmat == "no" && ContainsAny(haystack, "hazmat", "hazardous", "phmsa", "placard"))
            {
                edgeCase = true;
                edgeCaseReason = "Hazmat rules are an edge case because hazmat was not selected.";
            }
        }
        else if (kind.RequiresHazmatContext)
        {
            missing.Add("hazmat_involved");
        }

        if (context.TryGetValue("mining_site_work", out var mining) && mining == "yes"
            && ContainsAny(haystack, "mine", "mining", "msha"))
        {
            score += 0.22m;
            reasons.Add("Mining-site context matches an edge regulatory domain.");
        }
        else if (ContainsAny(haystack, "mine", "mining", "msha"))
        {
            edgeCase = true;
            edgeCaseReason = "Mining operations could add requirements.";
        }

        if (context.TryGetValue("operation_mode", out var mode))
        {
            if (mode == "highway_motor_carrier" && ContainsAny(haystack, "driver", "vehicle", "fmcsa", "motor carrier", "dispatch"))
            {
                score += 0.12m;
                reasons.Add("Highway motor-carrier context matches the source domain.");
            }
            else if (mode != "highway_motor_carrier" && ContainsAny(haystack, "fmcsa", "motor carrier"))
            {
                exclusions.Add("Selected operation mode is outside highway motor-carrier work.");
            }
        }

        if (context.TryGetValue("document_type", out var documentType)
            && (row.Requirement.RequiredDocumentType.Contains(documentType, StringComparison.OrdinalIgnoreCase)
                || haystack.Contains(documentType, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.18m;
            reasons.Add("Selected document type matches the requirement.");
        }

        if (context.TryGetValue("material_class", out var materialClass)
            && materialClass != "unknown"
            && ContainsAny(haystack, materialClass.Replace('_', ' '), "hazmat", "hazardous", "material"))
        {
            score += 0.14m;
            reasons.Add("Material class context matches the requirement.");
        }

        if (incidents.Count > 0 && ContainsAny(haystack, "incident", "accident", "testing", "report", "release"))
        {
            score += 0.2m;
            reasons.Add("Incident context matches incident-triggered requirements.");
        }

        if (!row.Pack?.IsActive ?? true)
        {
            exclusions.Add("Rule pack is inactive.");
        }

        string band;
        if (exclusions.Count > 0)
        {
            band = TheoreticalApplicabilityBands.NotApplicable;
            score = Math.Min(score, 0.2m);
        }
        else if (missing.Count > 0 && score < 0.55m)
        {
            band = TheoreticalApplicabilityBands.InsufficientContext;
            score = Math.Max(score, 0.35m);
        }
        else if (edgeCase)
        {
            band = TheoreticalApplicabilityBands.EdgeCase;
            score = Math.Max(score, 0.4m);
        }
        else if (score >= 0.68m)
        {
            band = TheoreticalApplicabilityBands.Primary;
        }
        else if (score >= 0.5m)
        {
            band = TheoreticalApplicabilityBands.Likely;
        }
        else if (score >= 0.32m)
        {
            band = TheoreticalApplicabilityBands.Possible;
        }
        else
        {
            band = TheoreticalApplicabilityBands.NotApplicable;
        }

        return new TheoreticalApplicabilityResult
        {
            ApplicabilityResultId = Guid.NewGuid(),
            TenantId = tenantId,
            SituationId = situationId,
            ProgramKey = row.Program?.ProgramKey ?? string.Empty,
            PackKey = row.Pack?.PackKey ?? string.Empty,
            CitationKey = row.Citation?.CitationKey ?? string.Empty,
            ApplicabilityScore = Math.Min(1m, score),
            ApplicabilityBand = band,
            MatchReasonsJson = Serialize(reasons),
            MissingContextJson = Serialize(missing.Take(2).ToList()),
            ExclusionReasonsJson = Serialize(exclusions),
            EdgeCase = edgeCase || band == TheoreticalApplicabilityBands.EdgeCase,
            EdgeCaseReason = edgeCaseReason,
            UserVisiblePriority = band switch
            {
                TheoreticalApplicabilityBands.Primary => 10,
                TheoreticalApplicabilityBands.Likely => 30,
                TheoreticalApplicabilityBands.Possible => 60,
                TheoreticalApplicabilityBands.EdgeCase => 90,
                _ => 120
            },
            CreatedAt = now
        };
    }

    private TheoreticalSituationEvaluationDetail BuildEvaluationDetail(
        Guid tenantId,
        RequirementProjection row,
        IReadOnlyList<TheoreticalSituationFact> facts,
        IReadOnlyList<ComplianceEvidenceOptionGroup> optionGroups,
        IReadOnlyList<ComplianceEvidenceOption> options,
        IReadOnlyList<ComplianceExceptionExemption> exceptionExemptions,
        IReadOnlyDictionary<string, bool> componentResults,
        DateTimeOffset now)
    {
        var fact = facts.FirstOrDefault(x => x.RequirementKey == row.Requirement.RequirementKey)
            ?? facts.FirstOrDefault(x => x.FactKey == row.Definition.FactKey);
        var group = optionGroups.FirstOrDefault(x =>
            string.Equals(x.RequirementKey, row.Requirement.RequirementKey, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(x.FactKey) && string.Equals(x.FactKey, row.Definition.FactKey, StringComparison.OrdinalIgnoreCase)));
        var groupOptions = group is null
            ? new List<ComplianceEvidenceOption>()
            : options.Where(x => x.EvidenceOptionGroupId == group.EvidenceOptionGroupId).OrderBy(x => x.Priority).ToList();

        var hasExceptionState = fact is not null && TheoreticalSimulatedStates.ExceptionStates.Contains(fact.SimulatedState);
        var normalFact = hasExceptionState ? CloneFactWithState(fact!, TheoreticalSimulatedStates.Missing) : fact;
        var normalFacts = hasExceptionState
            ? facts.Select(item => item.SituationFactId == fact!.SituationFactId ? normalFact! : item).ToList()
            : facts;

        var normalEvaluation = groupOptions.Count > 0
            ? EvaluateEvidenceOptions(row, group!, groupOptions, normalFacts)
            : EvaluateSingleFact(row, normalFact, componentResults, now);
        var exception = hasExceptionState ? FindApplicableException(row, fact!, exceptionExemptions, now) : null;
        var evaluation = hasExceptionState
            ? ApplyExceptionExemption(row, fact!, normalEvaluation, exception, now)
            : normalEvaluation;
        var exceptionConsidered = hasExceptionState && fact!.SimulatedState != TheoreticalSimulatedStates.NoExceptionClaimed;
        var exceptionApplies = exceptionConsidered && (evaluation.Result is TheoreticalEvaluationResults.Compliant or TheoreticalEvaluationResults.NotApplicable);
        var proofRequired = exceptionConsidered && exception?.RequiredEvidenceOptionGroupId is not null;
        var proofPresentedAsValid = exceptionConsidered &&
                                    (fact!.SimulatedState is TheoreticalSimulatedStates.KnownExceptionApplies
                                        or TheoreticalSimulatedStates.ExemptionValid
                                        or TheoreticalSimulatedStates.SpecialPermitValid
                                        or TheoreticalSimulatedStates.AlternateCompliancePathSelected);

        return new TheoreticalSituationEvaluationDetail
        {
            DetailId = Guid.NewGuid(),
            TenantId = tenantId,
            RequirementKey = row.Requirement.RequirementKey,
            FactKey = row.Definition.FactKey,
            CitationKey = row.Citation?.CitationKey ?? string.Empty,
            PackKey = row.Pack?.PackKey ?? string.Empty,
            AuditQuestion = PreferLabel(row.Requirement.AuditQuestion, row.Requirement.Label),
            SimulatedState = evaluation.State,
            ExpectedValue = row.Requirement.ExpectedValue,
            ActualValue = evaluation.ActualValue,
            Operator = row.Requirement.Operator,
            Result = evaluation.Result,
            FailureSeverity = row.Requirement.FailureSeverity,
            AutomaticFailureFlag = row.Requirement.AutomaticFailureFlag,
            OverrideAllowed = row.Requirement.OverrideAllowed,
            OverridePermission = row.Requirement.OverridePermission,
            RemediationRequired = row.Requirement.RemediationRequired && IsNegativeResult(evaluation.Result),
            NormalRuleResult = normalEvaluation.Result,
            ExceptionExemptionKey = exception?.Key ?? (exceptionConsidered ? fact?.SimulatedValue ?? string.Empty : string.Empty),
            ExceptionExemptionType = exception?.Type ?? ExceptionTypeForState(fact?.SimulatedState ?? string.Empty),
            ExceptionExemptionLabel = exception?.Label ?? LabelForExceptionState(fact?.SimulatedState ?? string.Empty),
            ExceptionExemptionConsidered = exceptionConsidered,
            ExceptionExemptionApplies = exceptionApplies,
            ExceptionExemptionProofRequired = proofRequired,
            ExceptionExemptionProofValid = proofPresentedAsValid &&
                                           (evaluation.Result is not TheoreticalEvaluationResults.Blocked and not TheoreticalEvaluationResults.NotCompliant),
            ResultBeforeException = normalEvaluation.Result,
            ResultAfterException = evaluation.Result,
            FinalComplianceResult = evaluation.Result,
            Explanation = evaluation.Explanation,
            SuggestedNextAction = evaluation.NextAction,
            VisiblePriority = evaluation.Result switch
            {
                TheoreticalEvaluationResults.Blocked => 10,
                TheoreticalEvaluationResults.OverrideNotAllowed => 15,
                TheoreticalEvaluationResults.NotCompliant => 20,
                TheoreticalEvaluationResults.InsufficientInformation => 30,
                TheoreticalEvaluationResults.AllowedWithOverride => 35,
                _ => 60
            }
        };
    }

    private static RequirementEvaluation ApplyExceptionExemption(
        RequirementProjection row,
        TheoreticalSituationFact fact,
        RequirementEvaluation normalEvaluation,
        ComplianceExceptionExemption? exception,
        DateTimeOffset now)
    {
        if (fact.SimulatedState == TheoreticalSimulatedStates.NoExceptionClaimed)
        {
            return normalEvaluation with
            {
                State = fact.SimulatedState,
                Explanation = $"No exception or exemption was claimed. Normal rule result remains {FormatResult(normalEvaluation.Result)}. {normalEvaluation.Explanation}",
                NextAction = normalEvaluation.NextAction
            };
        }

        if (fact.SimulatedState == TheoreticalSimulatedStates.ExceptionUnknown)
        {
            return Unknown(
                $"Normal rule result is {FormatResult(normalEvaluation.Result)}, but a possible exception/exemption is unknown.",
                "Select no exception, choose a known legal exception/exemption, or provide the required proof state.") with
                {
                    State = fact.SimulatedState,
                    ActualValue = fact.SimulatedValue
                };
        }

        if (exception is not null && exception.ExpiresAt is not null && exception.ExpiresAt < now)
        {
            return Fail(row, fact.SimulatedState, "Selected exception/exemption exists but is expired. Requirement remains failed.", "Renew the authorization or satisfy the normal requirement.");
        }

        if (exception is not null && !IsInScope(exception, row))
        {
            return Fail(row, fact.SimulatedState, "Selected exception/exemption is outside the covered subject, product, entity, citation, or pack scope. Requirement remains failed.", "Select an in-scope authorization or satisfy the normal requirement.");
        }

        return fact.SimulatedState switch
        {
            TheoreticalSimulatedStates.KnownExceptionApplies => ApplySuccessfulLegalRelief(
                exception,
                normalEvaluation,
                fact,
                "The selected regulatory exception applies to this simulated requirement."),
            TheoreticalSimulatedStates.ExemptionValid => ApplySuccessfulLegalRelief(
                exception,
                normalEvaluation,
                fact,
                "The selected exemption, waiver, or variance is valid and in scope."),
            TheoreticalSimulatedStates.SpecialPermitValid => ApplySuccessfulLegalRelief(
                exception,
                normalEvaluation,
                fact,
                "The selected special permit or agency approval is valid and in scope."),
            TheoreticalSimulatedStates.AlternateCompliancePathSelected => ApplySuccessfulLegalRelief(
                exception,
                normalEvaluation,
                fact,
                "The selected alternate compliance path satisfies the requirement."),
            TheoreticalSimulatedStates.ExemptionExpired => Fail(
                row,
                fact.SimulatedState,
                "Exemption selected, but it is expired. Requirement remains failed.",
                "Renew the exemption or satisfy the normal requirement."),
            TheoreticalSimulatedStates.ExemptionMissingProof => Fail(
                row,
                fact.SimulatedState,
                "Exemption selected, but required proof is missing or invalid. Requirement remains failed.",
                "Attach current proof scoped to this subject before relying on the exemption."),
            TheoreticalSimulatedStates.SpecialPermitOutsideScope => Fail(
                row,
                fact.SimulatedState,
                "Special permit selected, but the simulated situation is outside the permit scope. Requirement remains failed.",
                "Select an in-scope permit or satisfy the normal requirement."),
            _ => normalEvaluation
        };
    }

    private static RequirementEvaluation ApplySuccessfulLegalRelief(
        ComplianceExceptionExemption? exception,
        RequirementEvaluation normalEvaluation,
        TheoreticalSituationFact fact,
        string reason)
    {
        var effect = exception?.EffectType ?? EffectTypeForState(fact.SimulatedState);
        var finalResult = effect == ComplianceExceptionExemptionEffectTypes.MakesRequirementNotApplicable
            ? TheoreticalEvaluationResults.NotApplicable
            : TheoreticalEvaluationResults.Compliant;
        var finalLanguage = finalResult == TheoreticalEvaluationResults.NotApplicable
            ? "the normal requirement is not applicable"
            : "the requirement is satisfied through the legal alternate path";
        var proof = exception?.RequiredEvidenceOptionGroupId is null
            ? "No separate proof group is configured."
            : "Required proof is selected as present and valid.";

        return new RequirementEvaluation(
            finalResult,
            fact.SimulatedState,
            string.IsNullOrWhiteSpace(fact.SimulatedValue) ? "legal_relief_selected" : fact.SimulatedValue,
            $"Normal rule result: {FormatResult(normalEvaluation.Result)}. {reason} {proof} Final result: {finalLanguage}.",
            "Keep the legal basis, proof, scope, and expiration evidence with the audit trace.");
    }

    private static ComplianceExceptionExemption? FindApplicableException(
        RequirementProjection row,
        TheoreticalSituationFact fact,
        IReadOnlyList<ComplianceExceptionExemption> exceptions,
        DateTimeOffset now)
    {
        var selectedKey = fact.SimulatedValue.Trim();
        var candidates = exceptions
            .Where(item => string.IsNullOrWhiteSpace(selectedKey) || string.Equals(item.Key, selectedKey, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(item.PackKey) || string.Equals(item.PackKey, row.Pack?.PackKey, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(item.CitationKey) || string.Equals(item.CitationKey, row.Citation?.CitationKey, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(item.ApplicabilityKey) || string.Equals(item.ApplicabilityKey, row.Requirement.ApplicabilityKey, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(item.AppliesToSourceProduct) || string.Equals(item.AppliesToSourceProduct, row.Requirement.SourceProduct, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(item.AppliesToSourceEntity) || string.Equals(item.AppliesToSourceEntity, row.Requirement.SourceEntity, StringComparison.OrdinalIgnoreCase))
            .Where(item => item.EffectiveAt is null || item.EffectiveAt <= now)
            .OrderBy(item => item.ExpiresAt is null ? 1 : 0)
            .ThenBy(item => item.ExpiresAt)
            .ToList();

        return candidates.FirstOrDefault();
    }

    private static bool IsInScope(ComplianceExceptionExemption exception, RequirementProjection row) =>
        (string.IsNullOrWhiteSpace(exception.PackKey) || string.Equals(exception.PackKey, row.Pack?.PackKey, StringComparison.OrdinalIgnoreCase)) &&
        (string.IsNullOrWhiteSpace(exception.CitationKey) || string.Equals(exception.CitationKey, row.Citation?.CitationKey, StringComparison.OrdinalIgnoreCase)) &&
        (string.IsNullOrWhiteSpace(exception.ApplicabilityKey) || string.Equals(exception.ApplicabilityKey, row.Requirement.ApplicabilityKey, StringComparison.OrdinalIgnoreCase)) &&
        (string.IsNullOrWhiteSpace(exception.AppliesToSourceProduct) || string.Equals(exception.AppliesToSourceProduct, row.Requirement.SourceProduct, StringComparison.OrdinalIgnoreCase)) &&
        (string.IsNullOrWhiteSpace(exception.AppliesToSourceEntity) || string.Equals(exception.AppliesToSourceEntity, row.Requirement.SourceEntity, StringComparison.OrdinalIgnoreCase));

    private static TheoreticalSituationFact CloneFactWithState(TheoreticalSituationFact fact, string state) =>
        new()
        {
            SituationFactId = fact.SituationFactId,
            TenantId = fact.TenantId,
            SituationId = fact.SituationId,
            FactKey = fact.FactKey,
            RequirementKey = fact.RequirementKey,
            CitationKey = fact.CitationKey,
            PackKey = fact.PackKey,
            SimulatedValue = fact.SimulatedValue,
            ValueType = fact.ValueType,
            SimulatedState = state,
            EvidenceOptionKey = fact.EvidenceOptionKey,
            EvidenceKind = fact.EvidenceKind,
            TargetKind = fact.TargetKind,
            Active = fact.Active,
            CreatedAt = fact.CreatedAt
        };

    private static string ExceptionTypeForState(string state) =>
        state switch
        {
            TheoreticalSimulatedStates.KnownExceptionApplies => ComplianceExceptionExemptionTypes.RegulatoryException,
            TheoreticalSimulatedStates.ExemptionValid or TheoreticalSimulatedStates.ExemptionExpired or TheoreticalSimulatedStates.ExemptionMissingProof => ComplianceExceptionExemptionTypes.RegulatoryExemption,
            TheoreticalSimulatedStates.SpecialPermitValid or TheoreticalSimulatedStates.SpecialPermitOutsideScope => ComplianceExceptionExemptionTypes.SpecialPermit,
            TheoreticalSimulatedStates.AlternateCompliancePathSelected => ComplianceExceptionExemptionTypes.AlternateCompliancePath,
            _ => string.Empty
        };

    private static string EffectTypeForState(string state) =>
        state switch
        {
            TheoreticalSimulatedStates.KnownExceptionApplies => ComplianceExceptionExemptionEffectTypes.MakesRequirementNotApplicable,
            TheoreticalSimulatedStates.AlternateCompliancePathSelected => ComplianceExceptionExemptionEffectTypes.AllowsAlternateEvidence,
            TheoreticalSimulatedStates.SpecialPermitValid => ComplianceExceptionExemptionEffectTypes.AuthorizesOtherwiseBlockedAction,
            _ => ComplianceExceptionExemptionEffectTypes.AuthorizesOtherwiseBlockedAction
        };

    private static string LabelForExceptionState(string state) =>
        state switch
        {
            TheoreticalSimulatedStates.KnownExceptionApplies => "Known exception applies",
            TheoreticalSimulatedStates.ExemptionValid => "Exemption, waiver, or variance valid",
            TheoreticalSimulatedStates.ExemptionExpired => "Exemption expired",
            TheoreticalSimulatedStates.ExemptionMissingProof => "Exemption missing proof",
            TheoreticalSimulatedStates.SpecialPermitValid => "Special permit valid",
            TheoreticalSimulatedStates.SpecialPermitOutsideScope => "Special permit outside scope",
            TheoreticalSimulatedStates.AlternateCompliancePathSelected => "Alternate compliance path selected",
            TheoreticalSimulatedStates.ExceptionUnknown => "Possible exception/exemption unknown",
            _ => string.Empty
        };

    private static string FormatResult(string result) =>
        result.Replace('_', ' ');

    private static RequirementEvaluation EvaluateEvidenceOptions(
        RequirementProjection row,
        ComplianceEvidenceOptionGroup group,
        IReadOnlyList<ComplianceEvidenceOption> options,
        IReadOnlyList<TheoreticalSituationFact> facts)
    {
        var relevantFacts = facts
            .Where(x => string.Equals(x.RequirementKey, row.Requirement.RequirementKey, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.FactKey, row.Definition.FactKey, StringComparison.OrdinalIgnoreCase))
            .ToList();
        bool IsPass(ComplianceEvidenceOption option)
        {
            var fact = relevantFacts.FirstOrDefault(x => string.Equals(x.EvidenceOptionKey, option.OptionKey, StringComparison.OrdinalIgnoreCase));
            return fact is not null && TheoreticalSimulatedStates.Passing.Contains(fact.SimulatedState);
        }

        var passCount = options.Count(IsPass);
        var selectedCount = relevantFacts.Count(x => !string.IsNullOrWhiteSpace(x.EvidenceOptionKey) && x.SimulatedState != TheoreticalSimulatedStates.Unknown);
        var anyFailure = relevantFacts.Any(x => x.SimulatedState is TheoreticalSimulatedStates.Invalid or TheoreticalSimulatedStates.Expired or TheoreticalSimulatedStates.Incomplete or TheoreticalSimulatedStates.Missing);
        var state = relevantFacts.FirstOrDefault()?.SimulatedState ?? TheoreticalSimulatedStates.Unknown;
        var logic = group.LogicType.ToLowerInvariant();

        if (logic == EvidenceOptionLogicTypes.AnyOf && passCount > 0)
        {
            return Pass(state, "At least one acceptable evidence path is satisfied.", "No action needed for this simulated requirement.");
        }

        if (logic == EvidenceOptionLogicTypes.AllOf && passCount == options.Count)
        {
            return Pass(state, "All required evidence bundle components are satisfied.", "No action needed for this simulated requirement.");
        }

        if (logic == EvidenceOptionLogicTypes.OneOf)
        {
            if (passCount == 1)
            {
                return Pass(state, "Exactly one mutually exclusive evidence path is satisfied.", "No action needed for this simulated requirement.");
            }

            if (passCount > 1)
            {
                return Fail(row, state, "More than one mutually exclusive evidence path was selected.", "Choose exactly one evidence path.");
            }
        }

        if (logic == EvidenceOptionLogicTypes.Derived && passCount == options.Count)
        {
            return Pass(state, "Derived evidence path is satisfied by its component facts.", "No action needed for this simulated requirement.");
        }

        if (selectedCount == 0 || relevantFacts.Any(x => x.SimulatedState == TheoreticalSimulatedStates.Unknown))
        {
            return Unknown("No complete evidence-path state was selected.", "Select a structured state for the relevant evidence path.");
        }

        return anyFailure
            ? Fail(row, state, "The selected evidence path does not satisfy the requirement.", "Provide valid alternate evidence or remediate the failed item.")
            : Unknown("The evidence-path selection is incomplete.", "Select the missing evidence state.");
    }

    private static RequirementEvaluation EvaluateSingleFact(
        RequirementProjection row,
        TheoreticalSituationFact? fact,
        IReadOnlyDictionary<string, bool> componentResults,
        DateTimeOffset now)
    {
        if (fact is null)
        {
            return Unknown("No simulated fact or evidence state was supplied.", "Select a structured evidence state.");
        }

        if (fact.SimulatedState == TheoreticalSimulatedStates.NotApplicable)
        {
            return new RequirementEvaluation(
                TheoreticalEvaluationResults.NotApplicable,
                fact.SimulatedState,
                fact.SimulatedValue,
                "The requirement was marked not applicable for this hypothetical situation.",
                "Review applicability if this was unexpected.");
        }

        if (fact.SimulatedState == TheoreticalSimulatedStates.OverrideRequested)
        {
            return row.Requirement.OverrideAllowed
                ? new RequirementEvaluation(
                    TheoreticalEvaluationResults.AllowedWithOverride,
                    fact.SimulatedState,
                    fact.SimulatedValue,
                    "The simulated failure is eligible for an override.",
                    "Route to the configured override approver before acting.")
                : new RequirementEvaluation(
                    TheoreticalEvaluationResults.OverrideNotAllowed,
                    fact.SimulatedState,
                    fact.SimulatedValue,
                    "An override was requested, but this requirement is not override-eligible.",
                    "Remediate the requirement instead of overriding it.");
        }

        if (TheoreticalSimulatedStates.Passing.Contains(fact.SimulatedState))
        {
            if (row.Requirement.Operator == FactRequirementOperators.AllTrue && IsDerived(row.Requirement))
            {
                var components = FactRequirementContractRules.SplitCsv(row.Requirement.ExpectedValue);
                var derivedPassed = components.Count > 0 && components.All(component => componentResults.TryGetValue(component, out var componentPassed) && componentPassed);
                return derivedPassed
                    ? Pass(fact.SimulatedState, "All component facts in the derived rollup are satisfied.", "No action needed for this simulated requirement.")
                    : Fail(row, fact.SimulatedState, "One or more component facts in the derived rollup fail.", "Remediate the component fact before relying on the derived rollup.");
            }

            var value = string.IsNullOrWhiteSpace(fact.SimulatedValue) ? row.Requirement.ExpectedValue : fact.SimulatedValue;
            var operatorPassed = row.Requirement.Operator.ToLowerInvariant() switch
            {
                FactRequirementOperators.Exists => true,
                FactRequirementOperators.NotEmpty => !string.IsNullOrWhiteSpace(value),
                FactRequirementOperators.Current => fact.SimulatedState != TheoreticalSimulatedStates.Expired && IsCurrent(value, now),
                FactRequirementOperators.Equal => ValuesEqual(value, row.Requirement.ExpectedValue, row.Requirement.ValueType),
                _ => true
            };
            return operatorPassed
                ? Pass(fact.SimulatedState, "The simulated fact satisfies the requirement.", "No action needed for this simulated requirement.", value)
                : Fail(row, fact.SimulatedState, "The simulated fact value does not satisfy the expected value.", "Change the simulated value or remediate the requirement.", value);
        }

        return fact.SimulatedState switch
        {
            TheoreticalSimulatedStates.Unknown => Unknown("The simulated evidence state is unknown.", "Select a known state or gather more information."),
            TheoreticalSimulatedStates.Missing => Fail(row, fact.SimulatedState, "Required evidence is missing.", "Add valid evidence or select an acceptable alternate path."),
            TheoreticalSimulatedStates.Expired => Fail(row, fact.SimulatedState, "Evidence exists but is expired.", "Update the evidence before relying on this situation."),
            TheoreticalSimulatedStates.Invalid => Fail(row, fact.SimulatedState, "Evidence exists but is invalid.", "Replace or correct the evidence before relying on this situation."),
            TheoreticalSimulatedStates.Incomplete => Fail(row, fact.SimulatedState, "Evidence exists but is incomplete.", "Complete the missing evidence fields."),
            _ => Unknown("The simulated evidence state is not enough to evaluate the requirement.", "Select a supported structured state.")
        };
    }

    private IQueryable<RequirementProjection> QueryRequirementUniverse(Guid tenantId) =>
        from requirement in db.FactRequirements.AsNoTracking()
        join definition in db.FactDefinitions.AsNoTracking() on requirement.FactDefinitionId equals definition.Id
        join pack in db.RulePacks.AsNoTracking() on requirement.RulePackId equals pack.Id into packJoin
        from pack in packJoin.DefaultIfEmpty()
        join citation in db.RegulatoryCitations.AsNoTracking() on requirement.CitationId equals citation.Id into citationJoin
        from citation in citationJoin.DefaultIfEmpty()
        join program in db.RegulatoryPrograms.AsNoTracking() on pack.RegulatoryProgramId equals program.Id into programJoin
        from program in programJoin.DefaultIfEmpty()
        where requirement.TenantId == tenantId
            && requirement.IsActive
            && (pack == null || (pack.IsActive && pack.Status != RulePackStatuses.Archived))
        select new RequirementProjection(requirement, definition, pack, citation, program);

    private async Task<TheoreticalSituation> RequireSituationAsync(
        Guid tenantId,
        Guid situationId,
        CancellationToken cancellationToken,
        bool tracking = false)
    {
        var query = tracking ? db.TheoreticalSituations : db.TheoreticalSituations.AsNoTracking();
        return await query.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.SituationId == situationId && x.Status != TheoreticalSituationStatuses.Archived,
            cancellationToken)
            ?? throw new StlApiException("theoretical_situations.not_found", "Theoretical situation was not found.", 404);
    }

    private async Task<IReadOnlyDictionary<string, string>> ContextDictionaryAsync(
        Guid tenantId,
        Guid situationId,
        CancellationToken cancellationToken) =>
        await db.TheoreticalSituationContexts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SituationId == situationId)
            .ToDictionaryAsync(x => x.ContextKey, x => x.ContextValueKey, StringComparer.OrdinalIgnoreCase, cancellationToken);

    private async Task<TheoreticalSituationEvaluationResponse> MapEvaluationAsync(
        TheoreticalSituationEvaluation evaluation,
        CancellationToken cancellationToken)
    {
        var details = await db.TheoreticalSituationEvaluationDetails.AsNoTracking()
            .Where(x => x.TenantId == evaluation.TenantId && x.EvaluationId == evaluation.EvaluationId)
            .OrderBy(x => x.VisiblePriority)
            .ThenBy(x => x.RequirementKey)
            .Select(x => new TheoreticalSituationEvaluationDetailResponse(
                x.DetailId,
                x.RequirementKey,
                x.FactKey,
                x.CitationKey,
                x.PackKey,
                x.AuditQuestion,
                x.SimulatedState,
                x.ExpectedValue,
                x.ActualValue,
                x.Operator,
                x.Result,
                x.FailureSeverity,
                x.AutomaticFailureFlag,
                x.OverrideAllowed,
                x.OverridePermission,
                x.RemediationRequired,
                x.NormalRuleResult,
                x.ExceptionExemptionKey,
                x.ExceptionExemptionType,
                x.ExceptionExemptionLabel,
                x.ExceptionExemptionConsidered,
                x.ExceptionExemptionApplies,
                x.ExceptionExemptionProofRequired,
                x.ExceptionExemptionProofValid,
                x.ResultBeforeException,
                x.ResultAfterException,
                x.FinalComplianceResult,
                x.Explanation,
                x.SuggestedNextAction,
                x.VisiblePriority))
            .ToListAsync(cancellationToken);

        return new TheoreticalSituationEvaluationResponse(
            evaluation.EvaluationId,
            evaluation.SituationId,
            evaluation.EvaluatedAt,
            evaluation.EvaluatedByPersonId,
            evaluation.Result,
            evaluation.Summary,
            DeserializeList(evaluation.PrimaryProgramsJson),
            DeserializeList(evaluation.LikelyProgramsJson),
            DeserializeList(evaluation.EdgeCasesJson),
            evaluation.PassCount,
            evaluation.FailCount,
            evaluation.WarningCount,
            evaluation.BlockedCount,
            evaluation.NotApplicableCount,
            evaluation.UnknownCount,
            evaluation.OverrideAvailableCount,
            evaluation.OverrideBlockedCount,
            details);
    }

    private static TheoreticalApplicabilityResultResponse MapApplicability(TheoreticalApplicabilityResult result) =>
        new(
            result.ApplicabilityResultId,
            result.ProgramKey,
            result.PackKey,
            result.CitationKey,
            result.ApplicabilityScore,
            result.ApplicabilityBand,
            DeserializeList(result.MatchReasonsJson),
            DeserializeList(result.MissingContextJson),
            DeserializeList(result.ExclusionReasonsJson),
            result.EdgeCase,
            result.EdgeCaseReason,
            result.UserVisiblePriority,
            result.CreatedAt);

    private static TheoreticalSituationContextResponse MapContext(TheoreticalSituationContext context) =>
        new(
            context.ContextId,
            context.ContextKey,
            context.ContextLabel,
            context.ContextValueKey,
            context.ContextValueLabel,
            context.ControlledVocabularyType,
            context.Confidence,
            context.CreatedAt);

    private static TheoreticalSituationFactResponse MapFact(TheoreticalSituationFact fact) =>
        new(
            fact.SituationFactId,
            fact.FactKey,
            fact.RequirementKey,
            fact.CitationKey,
            fact.PackKey,
            fact.SimulatedValue,
            fact.ValueType,
            fact.SimulatedState,
            fact.EvidenceOptionKey,
            fact.EvidenceKind,
            fact.TargetKind,
            fact.Active,
            fact.CreatedAt);

    private static TheoreticalSituationIncidentResponse MapIncident(TheoreticalSituationIncident incident) =>
        new(
            incident.SituationIncidentId,
            incident.IncidentTypeKey,
            incident.SeverityKey,
            incident.InvolvedSubjectKind,
            incident.InvolvedSubjectState,
            incident.TriggerKey,
            incident.TriggerValue,
            incident.ReportabilityState,
            incident.RemediationState,
            incident.CreatedAt);

    private static async Task<IReadOnlyList<TheoreticalOptionResponse>> ReferenceOptionsAsync<TEntity>(
        IQueryable<TEntity> query,
        string category,
        CancellationToken cancellationToken)
        where TEntity : ProductObjectReferenceBase =>
        await query
            .OrderBy(x => x.Label)
            .Select(x => new TheoreticalOptionResponse(x.StableKey, x.Label, x.Description, category))
            .ToListAsync(cancellationToken);

    private static string DetermineOverallResult(IReadOnlyList<TheoreticalSituationEvaluationDetail> details)
    {
        if (details.Count == 0)
        {
            return TheoreticalEvaluationResults.InsufficientInformation;
        }

        if (details.Any(x => x.Result is TheoreticalEvaluationResults.Blocked or TheoreticalEvaluationResults.OverrideNotAllowed))
        {
            return TheoreticalEvaluationResults.Blocked;
        }

        if (details.Any(x => x.Result == TheoreticalEvaluationResults.AllowedWithOverride))
        {
            return TheoreticalEvaluationResults.AllowedWithOverride;
        }

        if (details.Any(x => x.Result == TheoreticalEvaluationResults.NotCompliant))
        {
            return TheoreticalEvaluationResults.NotCompliant;
        }

        if (details.Any(x => x.Result == TheoreticalEvaluationResults.InsufficientInformation))
        {
            return TheoreticalEvaluationResults.InsufficientInformation;
        }

        if (details.Any(x => x.Result == TheoreticalEvaluationResults.AllowedWithWarning))
        {
            return TheoreticalEvaluationResults.AllowedWithWarning;
        }

        return details.All(x => x.Result == TheoreticalEvaluationResults.NotApplicable)
            ? TheoreticalEvaluationResults.NotApplicable
            : TheoreticalEvaluationResults.Compliant;
    }

    private static string BuildSummary(
        string result,
        string situationKind,
        IReadOnlyList<TheoreticalSituationEvaluationDetail> details)
    {
        var label = SituationKindOptions.FirstOrDefault(x => x.Key == situationKind)?.Label ?? "Situation";
        return result switch
        {
            TheoreticalEvaluationResults.Compliant => $"Compliant - {label.ToLowerInvariant()} appears allowed in this hypothetical situation.",
            TheoreticalEvaluationResults.Blocked => "Not compliant - the simulated situation should be blocked.",
            TheoreticalEvaluationResults.NotCompliant => "Not compliant - one or more required facts or evidence paths fail.",
            TheoreticalEvaluationResults.AllowedWithOverride => "Allowed with override - approval is required before relying on this outcome.",
            TheoreticalEvaluationResults.InsufficientInformation => "Insufficient information - answer the remaining structured evidence questions.",
            TheoreticalEvaluationResults.NotApplicable => "Not applicable - no active primary requirements matched the simulated context.",
            _ => details.Count == 0 ? "No applicable requirements were found." : "The simulated situation completed with warnings."
        };
    }

    private static RequirementEvaluation Pass(string state, string explanation, string nextAction, string actualValue = "true") =>
        new(TheoreticalEvaluationResults.Compliant, state, actualValue, explanation, nextAction);

    private static RequirementEvaluation Fail(
        RequirementProjection row,
        string state,
        string explanation,
        string nextAction,
        string actualValue = "false")
    {
        var result = row.Requirement.AutomaticFailureFlag || !row.Requirement.OverrideAllowed
            ? TheoreticalEvaluationResults.Blocked
            : row.Requirement.FailureSeverity is FactRequirementFailureSeverities.Info or FactRequirementFailureSeverities.Minor
                ? TheoreticalEvaluationResults.AllowedWithWarning
                : TheoreticalEvaluationResults.NotCompliant;
        return new RequirementEvaluation(result, state, actualValue, explanation, nextAction);
    }

    private static RequirementEvaluation Unknown(string explanation, string nextAction) =>
        new(TheoreticalEvaluationResults.InsufficientInformation, TheoreticalSimulatedStates.Unknown, string.Empty, explanation, nextAction);

    private static bool IsPositiveResult(string result) =>
        result is TheoreticalEvaluationResults.Compliant or TheoreticalEvaluationResults.AllowedWithOverride or TheoreticalEvaluationResults.AllowedWithWarning;

    private static bool IsNegativeResult(string result) =>
        result is TheoreticalEvaluationResults.Blocked or TheoreticalEvaluationResults.NotCompliant or TheoreticalEvaluationResults.OverrideNotAllowed;

    private static bool IsDerived(FactRequirement requirement) =>
        string.Equals(requirement.EvidenceKind, FactRequirementEvidenceKinds.DerivedFact, StringComparison.OrdinalIgnoreCase);

    private static bool ValuesEqual(string actual, string expected, string valueType)
    {
        if (string.Equals(valueType, FactValueTypes.Boolean, StringComparison.OrdinalIgnoreCase))
        {
            return bool.TryParse(actual, out var actualBool)
                && bool.TryParse(expected, out var expectedBool)
                && actualBool == expectedBool;
        }

        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCurrent(string value, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (DateTimeOffset.TryParse(value, out var instant))
        {
            return instant >= now;
        }

        return !DateOnly.TryParse(value, out var date) || date >= DateOnly.FromDateTime(now.UtcDateTime);
    }

    private static bool ContainsAny(string value, params string[] terms) =>
        terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));

    private static string BuildSearchText(RequirementProjection row) =>
        string.Join(' ', new[]
        {
            row.Requirement.RequirementKey,
            row.Requirement.Label,
            row.Requirement.Description,
            row.Requirement.AuditQuestion,
            row.Requirement.SourceProduct,
            row.Requirement.SourceEntity,
            row.Requirement.SourceFieldOrRecordType,
            row.Requirement.EvidenceKind,
            row.Requirement.RequiredDocumentType,
            row.Definition.FactKey,
            row.Definition.Label,
            row.Pack?.PackKey ?? string.Empty,
            row.Pack?.Label ?? string.Empty,
            row.Program?.ProgramKey ?? string.Empty,
            row.Program?.Label ?? string.Empty,
            row.Citation?.CitationKey ?? string.Empty,
            row.Citation?.Label ?? string.Empty
        }).ToLowerInvariant();

    private static string NormalizeSituationKind(string value)
    {
        var key = NormalizeKey(value);
        if (!SituationKindOptions.Any(x => x.Key == key))
        {
            throw new StlApiException("theoretical_situations.invalid_kind", "Situation kind is not supported.", 400);
        }

        return key;
    }

    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("theoretical_situations.validation", "A controlled key is required.", 400);
        }

        return value.Trim().ToLowerInvariant();
    }

    private static string NormalizeOptionalKey(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();

    private static string NormalizeSimulatedState(string value)
    {
        var state = NormalizeKey(value);
        if (!TheoreticalSimulatedStates.All.Contains(state))
        {
            throw new StlApiException("theoretical_situations.invalid_state", "Simulated state is not supported.", 400);
        }

        return state;
    }

    private static string NormalizeValueType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return FactValueTypes.Boolean;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!FactValueTypes.All.Contains(normalized))
        {
            throw new StlApiException("theoretical_situations.invalid_value_type", "Value type is not supported.", 400);
        }

        return normalized;
    }

    private static string NormalizeEvidenceKind(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return FactRequirementEvidenceKinds.ProductRecord;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!FactRequirementEvidenceKinds.All.Contains(normalized))
        {
            throw new StlApiException("theoretical_situations.invalid_evidence_kind", "Evidence kind is not supported.", 400);
        }

        return normalized;
    }

    private static string NormalizeTargetKind(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return EvidenceOptionTargetKinds.ProductRecord;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!EvidenceOptionTargetKinds.All.Contains(normalized))
        {
            throw new StlApiException("theoretical_situations.invalid_target_kind", "Target kind is not supported.", 400);
        }

        return normalized;
    }

    private static string PreferLabel(string label, string fallback) =>
        string.IsNullOrWhiteSpace(label) ? fallback.Replace('_', ' ') : label;

    private static string Serialize(IReadOnlyList<string> values) =>
        JsonSerializer.Serialize(values, JsonOptions);

    private static IReadOnlyList<string> DeserializeList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<string>>(json, JsonOptions) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static readonly IReadOnlyList<TheoreticalOptionResponse> SituationKindOptions = new List<TheoreticalOptionResponse>
    {
        new("driver_person_documentation", "Driver/person documentation", "Evaluate personnel and driver document sufficiency.", "driver"),
        new("driver_dispatch_readiness", "Driver dispatch readiness", "Evaluate whether a driver can be dispatched.", "driver"),
        new("driver_qualification", "Driver qualification", "Evaluate driver qualification file and related requirements.", "driver"),
        new("driver_medical_qualification", "Driver medical qualification", "Evaluate medical qualification evidence.", "driver"),
        new("cdl_endorsement_eligibility", "CDL/endorsement eligibility", "Evaluate CDL, endorsement, and restriction states.", "driver"),
        new("drug_alcohol_program_state", "Drug and alcohol program state", "Evaluate controlled-substances and alcohol program state.", "driver"),
        new("entry_level_driver_training", "Entry-level driver training", "Evaluate ELDT training requirements.", "training"),
        new("hazmat_employee_training", "Hazmat employee training", "Evaluate hazmat employee training requirements.", "training"),
        new("vehicle_dispatch_readiness", "Vehicle dispatch readiness", "Evaluate whether a vehicle can be dispatched.", "vehicle"),
        new("vehicle_inspection_repair_maintenance", "Vehicle inspection/repair/maintenance", "Evaluate vehicle inspection, defect, repair, and maintenance facts.", "vehicle"),
        new("dvir_defect_correction", "DVIR/defect correction", "Evaluate DVIR defect correction state.", "vehicle"),
        new("annual_inspection", "Annual inspection", "Evaluate annual inspection evidence.", "vehicle"),
        new("roadside_inspection_correction", "Roadside inspection correction", "Evaluate roadside inspection correction state.", "vehicle"),
        new("out_of_service_readiness", "Out-of-service readiness", "Evaluate out-of-service readiness.", "vehicle"),
        new("cargo_securement", "Cargo securement", "Evaluate cargo securement facts.", "shipment"),
        new("shipment_load_transportability", "Shipment/load transportability", "Evaluate whether a shipment or load is transportable.", "shipment"),
        new("hazmat_applicability", "Hazmat applicability", "Evaluate whether hazmat rules apply.", "hazmat"),
        new("hazmat_classification", "Hazmat classification", "Evaluate hazmat classification evidence.", "hazmat"),
        new("hazmat_shipping_papers", "Hazmat shipping papers", "Evaluate shipping paper sufficiency.", "hazmat"),
        new("hazmat_marking", "Hazmat marking", "Evaluate marking requirements.", "hazmat"),
        new("hazmat_labeling", "Hazmat labeling", "Evaluate labeling requirements.", "hazmat"),
        new("hazmat_placarding", "Hazmat placarding", "Evaluate placarding requirements.", "hazmat"),
        new("hazmat_packaging", "Hazmat packaging", "Evaluate packaging requirements.", "hazmat"),
        new("hazmat_loading_unloading_segregation", "Hazmat loading/unloading/segregation", "Evaluate loading and segregation requirements.", "hazmat"),
        new("hazmat_incident_reporting", "Hazmat incident reporting", "Evaluate hazmat incident reporting triggers.", "incident"),
        new("accident_post_accident_testing", "Accident/post-accident testing", "Evaluate post-accident testing triggers.", "incident"),
        new("motor_carrier_authority_registration", "Motor carrier authority/registration", "Evaluate carrier authority and registration state.", "registration"),
        new("insurance_financial_responsibility", "Insurance/financial responsibility", "Evaluate financial responsibility facts.", "registration"),
        new("audit_sample", "Audit sample", "Evaluate whether a sample would fail an audit.", "audit"),
        new("evidence_sufficiency", "Evidence sufficiency", "Evaluate whether evidence satisfies a requirement.", "audit"),
        new("incident_event_outcome", "Incident/event outcome", "Evaluate incident or event outcome.", "incident"),
        new("custom_theoretical_situation", "Custom theoretical situation", "Evaluate a controlled custom situation.", "custom")
    };

    private static readonly IReadOnlyList<TheoreticalOptionResponse> EvidenceStateOptions = new List<TheoreticalOptionResponse>
    {
        new(TheoreticalSimulatedStates.Valid, "Exists and valid", "Evidence or fact is present and valid.", "evidence_state"),
        new(TheoreticalSimulatedStates.Invalid, "Exists but invalid", "Evidence or fact exists but is invalid.", "evidence_state"),
        new(TheoreticalSimulatedStates.Expired, "Exists but expired", "Evidence or fact exists but is expired.", "evidence_state"),
        new(TheoreticalSimulatedStates.Incomplete, "Exists but incomplete", "Evidence or fact exists but is incomplete.", "evidence_state"),
        new(TheoreticalSimulatedStates.Missing, "Does not exist", "Required evidence or fact is missing.", "evidence_state"),
        new(TheoreticalSimulatedStates.Unknown, "Unknown", "State is unknown.", "evidence_state"),
        new(TheoreticalSimulatedStates.NotApplicable, "Not applicable", "Requirement does not apply in this context.", "evidence_state"),
        new(TheoreticalSimulatedStates.AlternateEvidence, "Satisfied by alternate evidence", "An acceptable alternate path satisfies the requirement.", "evidence_state"),
        new(TheoreticalSimulatedStates.SystemFact, "Satisfied by system fact", "A system fact satisfies the requirement.", "evidence_state"),
        new(TheoreticalSimulatedStates.ExternalRegistry, "Satisfied by external registry", "External registry verification satisfies the requirement.", "evidence_state"),
        new(TheoreticalSimulatedStates.Derived, "Satisfied by derived fact", "A derived rollup satisfies the requirement.", "evidence_state"),
        new(TheoreticalSimulatedStates.OverrideRequested, "Override requested", "An override is requested for the simulated failure.", "evidence_state"),
        new(TheoreticalSimulatedStates.NoExceptionClaimed, "No exception/exemption claimed", "Normal requirement logic controls.", "exception_exemption_state"),
        new(TheoreticalSimulatedStates.ExceptionUnknown, "Possible exception/exemption unknown", "Potential legal relief is unknown and must be clarified.", "exception_exemption_state"),
        new(TheoreticalSimulatedStates.KnownExceptionApplies, "Known exception applies", "A built-in regulatory exception applies.", "exception_exemption_state"),
        new(TheoreticalSimulatedStates.ExemptionValid, "Exemption/waiver/variance exists and valid", "Legal relief exists, is current, and is scoped to the situation.", "exception_exemption_state"),
        new(TheoreticalSimulatedStates.ExemptionExpired, "Exemption exists but expired", "Legal relief exists but is no longer current.", "exception_exemption_state"),
        new(TheoreticalSimulatedStates.ExemptionMissingProof, "Exemption exists but missing proof", "Legal relief is claimed but proof is missing or invalid.", "exception_exemption_state"),
        new(TheoreticalSimulatedStates.SpecialPermitValid, "Special permit exists and valid", "Special permit or agency approval is current and in scope.", "exception_exemption_state"),
        new(TheoreticalSimulatedStates.SpecialPermitOutsideScope, "Special permit exists but outside scope", "Special permit does not cover the simulated material, person, route, equipment, date, or operation.", "exception_exemption_state"),
        new(TheoreticalSimulatedStates.AlternateCompliancePathSelected, "Alternate compliance path selected", "A legal alternate compliance path is selected.", "exception_exemption_state")
    };

    private static readonly IReadOnlyList<TheoreticalOptionResponse> MaterialClassOptions = new List<TheoreticalOptionResponse>
    {
        new("class_1_explosives", "Class 1 Explosives", "Hazard Class 1.", "hazmat_class"),
        new("class_2_gas", "Class 2 Gas", "Hazard Class 2.", "hazmat_class"),
        new("class_3_flammable_liquid", "Class 3 Flammable liquid", "Hazard Class 3.", "hazmat_class"),
        new("class_4_flammable_solid", "Class 4 Flammable solid", "Hazard Class 4.", "hazmat_class"),
        new("class_5_oxidizer_organic_peroxide", "Class 5 Oxidizer/organic peroxide", "Hazard Class 5.", "hazmat_class"),
        new("class_6_poison_toxic_infectious", "Class 6 Poison/toxic/infectious substance", "Hazard Class 6.", "hazmat_class"),
        new("class_7_radioactive", "Class 7 Radioactive", "Hazard Class 7.", "hazmat_class"),
        new("class_8_corrosive", "Class 8 Corrosive", "Hazard Class 8.", "hazmat_class"),
        new("class_9_miscellaneous", "Class 9 Miscellaneous", "Hazard Class 9.", "hazmat_class"),
        new("combustible_liquid", "Combustible liquid where applicable", "Combustible liquid context.", "hazmat_class"),
        new("orm_limited_quantity_material_of_trade", "ORM/limited quantity/material of trade where applicable", "Limited-quantity or material-of-trade context.", "hazmat_class"),
        new("not_regulated", "Not regulated", "Material is not regulated.", "hazmat_class"),
        new("unknown", "Unknown", "Material class is unknown.", "hazmat_class")
    };

    private static readonly IReadOnlyList<TheoreticalOptionResponse> IncidentOptions = new List<TheoreticalOptionResponse>
    {
        new("accident", "Accident", "Crash or accident event.", "incident"),
        new("hazmat_release", "Hazmat release", "Hazardous-material release event.", "incident"),
        new("roadside_inspection", "Roadside inspection", "Roadside inspection or enforcement stop.", "incident"),
        new("injury", "Injury", "Injury event.", "incident"),
        new("fatality", "Fatality", "Fatality event.", "incident"),
        new("tow_away", "Tow-away", "Tow-away crash event.", "incident"),
        new("training_failure", "Training failure", "Training or evaluation failure.", "incident")
    };

    private static readonly IReadOnlyList<TheoreticalContextFieldResponse> ContextFields = new List<TheoreticalContextFieldResponse>
    {
        Field("subject_type", "Subject", "segmented", "subject_type", true, new[] { "driver_person_documentation", "driver_dispatch_readiness", "driver_qualification", "driver_medical_qualification", "cdl_endorsement_eligibility", "drug_alcohol_program_state", "entry_level_driver_training", "hazmat_employee_training" },
            Option("driver", "Driver"), Option("person", "Person"), Option("training_record", "Training record"), Option("unknown", "Unknown")),
        Field("commercial_motor_vehicle_operation", "Commercial motor vehicle operation", "yes_no_unknown", "boolean_unknown", true, Array.Empty<string>(),
            Option("yes", "Yes"), Option("no", "No"), Option("unknown", "Unknown")),
        Field("operation_scope", "Operation scope", "segmented", "operation_scope", true, Array.Empty<string>(),
            Option("interstate", "Interstate"), Option("intrastate", "Intrastate"), Option("both", "Both"), Option("unknown", "Unknown")),
        Field("operation_mode", "Operation mode", "select", "operation_mode", true, Array.Empty<string>(),
            Option("highway_motor_carrier", "Highway motor carrier"), Option("pipeline", "Pipeline"), Option("rail", "Rail"), Option("transit", "Transit"), Option("aviation", "Aviation"), Option("maritime", "Maritime"), Option("unknown", "Unknown")),
        Field("cdl_required", "CDL required", "yes_no_unknown", "boolean_unknown", false, new[] { "driver_dispatch_readiness", "driver_qualification", "cdl_endorsement_eligibility" },
            Option("yes", "Yes"), Option("no", "No"), Option("unknown", "Unknown")),
        Field("hazmat_involved", "Hazmat involved", "yes_no_unknown", "boolean_unknown", false, Array.Empty<string>(),
            Option("yes", "Yes"), Option("no", "No"), Option("unknown", "Unknown")),
        Field("placarding_required", "Placarding required", "yes_no_unknown", "boolean_unknown", false, new[] { "hazmat_applicability", "hazmat_placarding", "shipment_load_transportability" },
            Option("yes", "Yes"), Option("no", "No"), Option("unknown", "Unknown")),
        Field("passenger_transport", "Passenger transport", "yes_no_unknown", "boolean_unknown", false, Array.Empty<string>(),
            Option("yes", "Yes"), Option("no", "No"), Option("unknown", "Unknown")),
        Field("intermodal_equipment", "Intermodal equipment", "yes_no_unknown", "boolean_unknown", false, Array.Empty<string>(),
            Option("yes", "Yes"), Option("no", "No"), Option("unknown", "Unknown")),
        Field("mining_site_work", "Mining-site work", "yes_no_unknown", "boolean_unknown", false, Array.Empty<string>(),
            Option("yes", "Yes"), Option("no", "No"), Option("unknown", "Unknown")),
        Field("party_role", "Party role", "select", "party_role", false, Array.Empty<string>(),
            Option("private_carrier", "Private carrier"), Option("for_hire_carrier", "For-hire carrier"), Option("shipper", "Shipper"), Option("offeror", "Offeror"), Option("consignee", "Consignee"), Option("unknown", "Unknown")),
        Field("question_type", "Question type", "segmented", "question_type", true, Array.Empty<string>(),
            Option("audit_evidence", "Audit/evidence"), Option("dispatch", "Dispatch"), Option("incident", "Incident"), Option("training", "Training"), Option("registration", "Registration")),
        Field("document_type", "Document type", "select", "document_type", false, new[] { "evidence_sufficiency", "audit_sample", "driver_person_documentation" },
            Option("driver_qualification_application", "Driver qualification application"), Option("medical_certificate", "Medical certificate"), Option("annual_inspection", "Annual inspection"), Option("road_test_certificate", "Road test certificate"), Option("shipping_paper", "Shipping paper")),
        Field("material_class", "Material class", "select", "hazmat_class", false, new[] { "hazmat_applicability", "hazmat_classification", "hazmat_shipping_papers", "hazmat_marking", "hazmat_labeling", "hazmat_placarding", "hazmat_packaging", "hazmat_loading_unloading_segregation", "shipment_load_transportability" },
            MaterialClassOptions.ToArray())
    };

    private static TheoreticalContextFieldResponse Field(
        string key,
        string label,
        string controlType,
        string vocabulary,
        bool required,
        IReadOnlyList<string> situationKinds,
        params TheoreticalOptionResponse[] values) =>
        new(key, label, controlType, vocabulary, required, situationKinds, values);

    private static TheoreticalOptionResponse Option(string key, string label) =>
        new(key, label, string.Empty, "context");

    private static readonly IReadOnlyDictionary<string, SituationKindProfile> SituationKindProfiles =
        SituationKindOptions.ToDictionary(
            x => x.Key,
            x =>
            {
                var category = x.Category;
                return new SituationKindProfile(
                    x.Key,
                    category switch
                    {
                        "driver" => new[] { "driver", "person" },
                        "vehicle" => new[] { "vehicle", "asset", "inspection" },
                        "shipment" => new[] { "shipment", "load", "cargo" },
                        "hazmat" => new[] { "material", "shipment", "hazmat" },
                        "incident" => new[] { "incident", "accident", "event" },
                        "training" => new[] { "training", "person" },
                        _ => new[] { category }
                    },
                    category switch
                    {
                        "driver" => new[] { ComplianceCoreProductKeys.StaffArr, ComplianceCoreProductKeys.RoutArr, ComplianceCoreProductKeys.TrainArr },
                        "vehicle" => new[] { ComplianceCoreProductKeys.MaintainArr, ComplianceCoreProductKeys.RoutArr },
                        "shipment" => new[] { ComplianceCoreProductKeys.RoutArr, ComplianceCoreProductKeys.SupplyArr },
                        "hazmat" => new[] { ComplianceCoreProductKeys.SupplyArr, ComplianceCoreProductKeys.RoutArr, ComplianceCoreProductKeys.TrainArr },
                        "incident" => new[] { ComplianceCoreProductKeys.StaffArr, ComplianceCoreProductKeys.RoutArr, ComplianceCoreProductKeys.SupplyArr },
                        "training" => new[] { ComplianceCoreProductKeys.TrainArr, ComplianceCoreProductKeys.StaffArr },
                        _ => new[] { ComplianceCoreProductKeys.ComplianceCore }
                    },
                    category switch
                    {
                        "audit" => FactRequirementEvidenceKinds.All.ToArray(),
                        "hazmat" => new[] { FactRequirementEvidenceKinds.DocumentRecord, FactRequirementEvidenceKinds.ProductRecord, FactRequirementEvidenceKinds.SystemFact, FactRequirementEvidenceKinds.ExternalRegistry, FactRequirementEvidenceKinds.DerivedFact },
                        _ => FactRequirementEvidenceKinds.All.ToArray()
                    },
                    x.Key.Split('_').Concat(new[] { x.Label.ToLowerInvariant() }).ToArray(),
                    category is "driver" or "vehicle" or "shipment",
                    category is "hazmat");
            });

    private sealed record SituationKindProfile(
        string Key,
        IReadOnlyList<string> SubjectTypes,
        IReadOnlyList<string> Products,
        IReadOnlyList<string> EvidenceKinds,
        IReadOnlyList<string> Keywords,
        bool RequiresCmvContext,
        bool RequiresHazmatContext);

    private sealed record RequirementProjection(
        FactRequirement Requirement,
        FactDefinition Definition,
        RulePack? Pack,
        RegulatoryCitation? Citation,
        RegulatoryProgram? Program);

    private sealed record RequirementKey(string PackKey, string CitationKey)
    {
        public bool Matches(string packKey, string citationKey) =>
            string.Equals(PackKey, packKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(CitationKey, citationKey, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record RequirementEvaluation(
        string Result,
        string State,
        string ActualValue,
        string Explanation,
        string NextAction);
}
