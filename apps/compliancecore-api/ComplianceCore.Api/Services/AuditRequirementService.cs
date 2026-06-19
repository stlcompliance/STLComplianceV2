using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class AuditRequirementService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<AuditRequirementMatrixResponse> MatrixByPackAsync(
        Guid tenantId,
        string packKey,
        CancellationToken cancellationToken = default)
    {
        var normalized = packKey.Trim().ToLowerInvariant();
        var requirements = (await QueryRequirementResponses(tenantId).ToListAsync(cancellationToken))
            .Where(x => x.Pack != null && x.Pack.PackKey == normalized)
            .ToList();

        return new AuditRequirementMatrixResponse(
            "pack",
            normalized,
            requirements.Select(x => FactRequirementService.MapResponse(x.Requirement, x.Definition, x.Pack, x.Citation)).ToList());
    }

    public async Task<AuditRequirementMatrixResponse> MatrixBySourceProductAsync(
        Guid tenantId,
        string sourceProduct,
        CancellationToken cancellationToken = default)
    {
        var normalized = sourceProduct.Trim();
        var requirements = (await QueryRequirementResponses(tenantId).ToListAsync(cancellationToken))
            .Where(x => x.Requirement.SourceProduct.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new AuditRequirementMatrixResponse(
            "source_product",
            normalized,
            requirements.Select(x => FactRequirementService.MapResponse(x.Requirement, x.Definition, x.Pack, x.Citation)).ToList());
    }

    public async Task<AuditRequirementMatrixResponse> MatrixByEntityAsync(
        Guid tenantId,
        string sourceEntity,
        CancellationToken cancellationToken = default)
    {
        var normalized = sourceEntity.Trim().ToLowerInvariant();
        var requirements = (await QueryRequirementResponses(tenantId).ToListAsync(cancellationToken))
            .Where(x => x.Requirement.SourceEntity.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new AuditRequirementMatrixResponse(
            "source_entity",
            normalized,
            requirements.Select(x => FactRequirementService.MapResponse(x.Requirement, x.Definition, x.Pack, x.Citation)).ToList());
    }

    public async Task<AuditRequirementMatrixResponse> MatrixByCitationAsync(
        Guid tenantId,
        string citationKey,
        CancellationToken cancellationToken = default)
    {
        var normalized = citationKey.Trim().ToLowerInvariant();
        var requirements = (await QueryRequirementResponses(tenantId).ToListAsync(cancellationToken))
            .Where(x => x.Citation != null && x.Citation.CitationKey == normalized)
            .ToList();

        return new AuditRequirementMatrixResponse(
            "citation",
            normalized,
            requirements.Select(x => FactRequirementService.MapResponse(x.Requirement, x.Definition, x.Pack, x.Citation)).ToList());
    }

    public async Task<EvidenceReferenceResponse> CreateEvidenceReferenceAsync(
        EvidenceReferenceCreateRequest request,
        string tokenSourceProduct,
        CancellationToken cancellationToken = default)
    {
        var sourceProduct = NormalizeSourceProduct(request.SourceProduct, tokenSourceProduct);
        if (string.IsNullOrWhiteSpace(request.DocumentUrl) && string.IsNullOrWhiteSpace(request.StorageKey))
        {
            throw new StlApiException(
                "evidence_references.validation",
                "Either document_url or storage_key is required.",
                400);
        }

        var existing = await db.EvidenceReferences.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId && x.EvidenceId == request.EvidenceId.Trim(),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new EvidenceReference
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                EvidenceId = request.EvidenceId.Trim(),
                CreatedAt = now
            };
            db.EvidenceReferences.Add(existing);
        }

        existing.FactKey = NormalizeFactKey(request.FactKey);
        existing.SourceProduct = sourceProduct;
        existing.SourceEntity = RequireTrimmed(request.SourceEntity, "source_entity");
        existing.SourceRecordId = RequireTrimmed(request.SourceRecordId, "source_record_id");
        existing.SourceField = RequireTrimmed(request.SourceField, "source_field");
        existing.DocumentType = RequireTrimmed(request.DocumentType, "document_type");
        existing.DocumentUrl = request.DocumentUrl?.Trim() ?? string.Empty;
        existing.StorageKey = request.StorageKey?.Trim() ?? string.Empty;
        existing.FileHash = RequireTrimmed(request.FileHash, "file_hash");
        existing.CapturedAt = request.CapturedAt;
        existing.EffectiveAt = request.EffectiveAt;
        existing.ExpiresAt = request.ExpiresAt;
        existing.CreatedByPersonId = request.CreatedByPersonId;
        existing.ReviewedByPersonId = request.ReviewedByPersonId;
        existing.ReviewStatus = string.IsNullOrWhiteSpace(request.ReviewStatus) ? "pending" : request.ReviewStatus.Trim().ToLowerInvariant();
        existing.Notes = request.Notes?.Trim() ?? string.Empty;

        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "evidence_reference.upsert",
            request.TenantId,
            null,
            "evidence_reference",
            existing.EvidenceId,
            "success",
            reasonCode: sourceProduct,
            cancellationToken: cancellationToken);

        return MapEvidence(existing);
    }

    public async Task<FactAssertionResponse> CreateFactAssertionAsync(
        FactAssertionCreateRequest request,
        string tokenSourceProduct,
        CancellationToken cancellationToken = default)
    {
        var sourceProduct = NormalizeSourceProduct(request.SourceProduct, tokenSourceProduct);
        var valueType = request.ValueType.Trim().ToLowerInvariant();
        if (!FactValueTypes.All.Contains(valueType))
        {
            throw new StlApiException("fact_assertions.invalid_value_type", "Value type is not supported.", 400);
        }

        EvidenceReference? evidence = null;
        if (!string.IsNullOrWhiteSpace(request.EvidenceId))
        {
            evidence = await db.EvidenceReferences.FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId && x.EvidenceId == request.EvidenceId.Trim(),
                cancellationToken);
            if (evidence is null)
            {
                throw new StlApiException("fact_assertions.evidence_not_found", "Evidence reference was not found.", 404);
            }
        }

        var assertion = new FactAssertion
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            FactKey = NormalizeFactKey(request.FactKey),
            SubjectKind = RequireTrimmed(request.SubjectKind, "subject_kind").ToLowerInvariant(),
            SubjectId = RequireTrimmed(request.SubjectId, "subject_id"),
            Value = RequireTrimmed(request.Value, "value"),
            ValueType = valueType,
            SourceProduct = sourceProduct,
            SourceRecordId = RequireTrimmed(request.SourceRecordId, "source_record_id"),
            EvidenceReferenceId = evidence?.Id,
            EvidenceId = evidence?.EvidenceId,
            AssertedAt = request.AssertedAt,
            EffectiveAt = request.EffectiveAt,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.FactAssertions.Add(assertion);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "fact_assertion.create",
            request.TenantId,
            null,
            "fact_assertion",
            assertion.Id.ToString(),
            "success",
            reasonCode: sourceProduct,
            cancellationToken: cancellationToken);

        return MapAssertion(assertion);
    }

    public async Task<AuditRequirementEvaluationResponse> EvaluateAsync(
        Guid tenantId,
        AuditRequirementEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        var packKey = request.PackKey.Trim().ToLowerInvariant();
        var subjectKind = RequireTrimmed(request.SubjectKind, "subject_kind").ToLowerInvariant();
        var subjectId = RequireTrimmed(request.SubjectId, "subject_id");
        var evaluatedAt = DateTimeOffset.UtcNow;

        var rows = (await QueryRequirementResponses(tenantId).ToListAsync(cancellationToken))
            .Where(x => x.Pack != null && x.Pack.PackKey == packKey)
            .OrderBy(x => x.Requirement.EvidenceKind)
            .ThenBy(x => x.Requirement.RequirementKey)
            .ToList();
        if (rows.Count == 0)
        {
            throw new StlApiException("audit_requirements.pack_not_found", "No audit requirements were found for the pack.", 404);
        }

        var factKeys = rows.Select(x => x.Definition.FactKey)
            .Concat(rows.SelectMany(x => FactRequirementContractRules.SplitCsv(x.Requirement.ExpectedValue)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var assertions = await db.FactAssertions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.SubjectKind == subjectKind
                && x.SubjectId == subjectId
                && factKeys.Contains(x.FactKey))
            .OrderByDescending(x => x.AssertedAt)
            .ToListAsync(cancellationToken);
        var latestAssertions = assertions
            .GroupBy(x => x.FactKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var componentResults = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var traces = new List<AuditTrace>();

        foreach (var row in rows.Where(x => !IsDerived(x.Requirement)))
        {
            var trace = BuildTrace(tenantId, packKey, subjectKind, subjectId, row, latestAssertions, componentResults, request, evaluatedAt);
            componentResults[row.Definition.FactKey] = string.Equals(trace.Result, "pass", StringComparison.OrdinalIgnoreCase)
                || string.Equals(trace.Result, "overridden", StringComparison.OrdinalIgnoreCase);
            traces.Add(trace);
        }

        foreach (var row in rows.Where(x => IsDerived(x.Requirement)))
        {
            var trace = BuildTrace(tenantId, packKey, subjectKind, subjectId, row, latestAssertions, componentResults, request, evaluatedAt);
            componentResults[row.Definition.FactKey] = string.Equals(trace.Result, "pass", StringComparison.OrdinalIgnoreCase)
                || string.Equals(trace.Result, "overridden", StringComparison.OrdinalIgnoreCase);
            traces.Add(trace);
        }

        db.AuditTraces.AddRange(traces);
        await db.SaveChangesAsync(cancellationToken);

        var overallResult = traces.Any(x => string.Equals(x.Result, "automatic_failure", StringComparison.OrdinalIgnoreCase))
            ? "block"
            : traces.All(x => string.Equals(x.Result, "pass", StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Result, "overridden", StringComparison.OrdinalIgnoreCase))
                ? "pass"
                : "fail";

        return new AuditRequirementEvaluationResponse(
            packKey,
            subjectKind,
            subjectId,
            overallResult,
            traces.Select(MapTrace).ToList(),
            evaluatedAt);
    }

    private IQueryable<RequirementProjection> QueryRequirementResponses(Guid tenantId) =>
        from requirement in db.FactRequirements.AsNoTracking()
        join definition in db.FactDefinitions.AsNoTracking() on requirement.FactDefinitionId equals definition.Id
        join pack in db.RulePacks.AsNoTracking() on requirement.RulePackId equals pack.Id into packJoin
        from pack in packJoin.DefaultIfEmpty()
        join citation in db.RegulatoryCitations.AsNoTracking() on requirement.CitationId equals citation.Id into citationJoin
        from citation in citationJoin.DefaultIfEmpty()
        where requirement.TenantId == tenantId && requirement.IsActive
        select new RequirementProjection(requirement, definition, pack, citation);

    private AuditTrace BuildTrace(
        Guid tenantId,
        string packKey,
        string subjectKind,
        string subjectId,
        RequirementProjection row,
        IReadOnlyDictionary<string, FactAssertion> assertions,
        IReadOnlyDictionary<string, bool> componentResults,
        AuditRequirementEvaluationRequest request,
        DateTimeOffset evaluatedAt)
    {
        assertions.TryGetValue(row.Definition.FactKey, out var assertion);
        var (passed, evaluatedValue) = EvaluateRequirement(row.Requirement, assertion, componentResults, evaluatedAt);
        var overrideReason = request.OverrideReasons?.GetValueOrDefault(row.Definition.FactKey) ?? string.Empty;
        var overrideUsed = !passed
            && !string.IsNullOrWhiteSpace(overrideReason)
            && row.Requirement.OverrideAllowed;
        var result = passed
            ? "pass"
            : overrideUsed
                ? "overridden"
                : row.Requirement.AutomaticFailureFlag
                    ? "automatic_failure"
                    : "fail";

        return new AuditTrace
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AuditTraceId = $"trace_{Guid.NewGuid():N}",
            PackKey = packKey,
            FactKey = row.Definition.FactKey,
            CitationKey = row.Citation?.CitationKey ?? string.Empty,
            SubjectKind = subjectKind,
            SubjectId = subjectId,
            EvaluatedValue = evaluatedValue,
            ExpectedValue = row.Requirement.ExpectedValue,
            Operator = row.Requirement.Operator,
            Result = result,
            FailureSeverity = row.Requirement.FailureSeverity,
            AutomaticFailureFlag = row.Requirement.AutomaticFailureFlag,
            OverrideUsed = overrideUsed,
            OverridePersonId = overrideUsed ? request.OverridePersonId : null,
            OverrideReason = overrideUsed ? overrideReason.Trim() : string.Empty,
            RemediationRequired = row.Requirement.RemediationRequired && !passed && !overrideUsed,
            EvaluatedAt = evaluatedAt
        };
    }

    private static (bool Passed, string EvaluatedValue) EvaluateRequirement(
        FactRequirement requirement,
        FactAssertion? assertion,
        IReadOnlyDictionary<string, bool> componentResults,
        DateTimeOffset now)
    {
        if (IsDerived(requirement) && !requirement.ExternallyAssertable)
        {
            var components = FactRequirementContractRules.SplitCsv(requirement.ExpectedValue);
            var passed = components.Count > 0
                && components.All(component => componentResults.TryGetValue(component, out var componentPassed) && componentPassed);
            return (passed, string.Join(',', components.Select(component =>
                $"{component}:{(componentResults.TryGetValue(component, out var componentPassed) && componentPassed ? "true" : "false")}")));
        }

        return requirement.Operator.ToLowerInvariant() switch
        {
            FactRequirementOperators.Exists => (assertion is not null, assertion is null ? string.Empty : "exists"),
            FactRequirementOperators.NotEmpty => (!string.IsNullOrWhiteSpace(assertion?.Value), assertion?.Value ?? string.Empty),
            FactRequirementOperators.Current => EvaluateCurrent(assertion, requirement.RetentionPeriod, now),
            FactRequirementOperators.Equal => EvaluateEquals(assertion, requirement.ExpectedValue),
            FactRequirementOperators.AllTrue => (false, "all_true requires derived_fact evidence_kind"),
            _ => (false, assertion?.Value ?? string.Empty)
        };
    }

    private static (bool Passed, string EvaluatedValue) EvaluateEquals(FactAssertion? assertion, string expected)
    {
        if (assertion is null)
        {
            return (false, string.Empty);
        }

        if (string.Equals(assertion.ValueType, FactValueTypes.Boolean, StringComparison.OrdinalIgnoreCase))
        {
            var actualBool = bool.TryParse(assertion.Value, out var parsed) && parsed;
            var expectedBool = bool.TryParse(expected, out var expectedParsed) && expectedParsed;
            return (actualBool == expectedBool, actualBool.ToString().ToLowerInvariant());
        }

        return (string.Equals(assertion.Value, expected, StringComparison.OrdinalIgnoreCase), assertion.Value);
    }

    private static (bool Passed, string EvaluatedValue) EvaluateCurrent(
        FactAssertion? assertion,
        string retentionPeriod,
        DateTimeOffset now)
    {
        if (assertion is null)
        {
            return (false, string.Empty);
        }

        var evaluated = RetentionWindowRules.EvaluateCurrent(
            assertion.AssertedAt,
            assertion.EffectiveAt,
            assertion.ExpiresAt,
            retentionPeriod,
            assertion.Value,
            now);

        if (evaluated.DaysRemaining.HasValue)
        {
            var days = Math.Abs(evaluated.DaysRemaining.Value);
            var suffix = days == 1 ? string.Empty : "s";
            var detail = evaluated.IsDueSoon
                ? $"warning window, due in {days} day{suffix}"
                : $"due in {days} day{suffix}";
            var evaluatedValue = evaluated.Passed
                ? $"current ({detail})"
                : $"expired {days} day{suffix} ago";

            return (evaluated.Passed, evaluatedValue);
        }

        return (evaluated.Passed, evaluated.EvaluatedValue);
    }

    private static bool IsDerived(FactRequirement requirement) =>
        string.Equals(requirement.EvidenceKind, FactRequirementEvidenceKinds.DerivedFact, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSourceProduct(string sourceProduct, string tokenSourceProduct)
    {
        var requested = RequireTrimmed(sourceProduct, "source_product");
        if (!string.Equals(requested, tokenSourceProduct, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.source_product_mismatch",
                "Source product must match the service token source product.",
                403);
        }

        if (!ComplianceCoreProductKeys.Canonical.TryGetValue(requested, out var canonical))
        {
            throw new StlApiException("source_product.unknown", "Source product is not recognized.", 400);
        }

        return canonical;
    }

    private static string NormalizeFactKey(string factKey) =>
        RequireTrimmed(factKey, "fact_key").ToLowerInvariant();

    private static string RequireTrimmed(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("audit_requirements.validation", $"{field} is required.", 400);
        }

        return value.Trim();
    }

    private static EvidenceReferenceResponse MapEvidence(EvidenceReference reference) =>
        new(
            reference.Id,
            reference.EvidenceId,
            reference.TenantId,
            reference.FactKey,
            reference.SourceProduct,
            reference.SourceEntity,
            reference.SourceRecordId,
            reference.SourceField,
            reference.DocumentType,
            reference.DocumentUrl,
            reference.StorageKey,
            reference.FileHash,
            reference.CapturedAt,
            reference.EffectiveAt,
            reference.ExpiresAt,
            reference.CreatedByPersonId,
            reference.ReviewedByPersonId,
            reference.ReviewStatus,
            reference.Notes);

    private static FactAssertionResponse MapAssertion(FactAssertion assertion) =>
        new(
            assertion.Id,
            assertion.TenantId,
            assertion.FactKey,
            assertion.SubjectKind,
            assertion.SubjectId,
            assertion.Value,
            assertion.ValueType,
            assertion.SourceProduct,
            assertion.SourceRecordId,
            assertion.EvidenceId,
            assertion.AssertedAt,
            assertion.EffectiveAt,
            assertion.ExpiresAt);

    private static AuditTraceResponse MapTrace(AuditTrace trace) =>
        new(
            trace.Id,
            trace.AuditTraceId,
            trace.PackKey,
            trace.FactKey,
            trace.CitationKey,
            trace.SubjectKind,
            trace.SubjectId,
            trace.EvaluatedValue,
            trace.ExpectedValue,
            trace.Operator,
            trace.Result,
            trace.FailureSeverity,
            trace.AutomaticFailureFlag,
            trace.OverrideUsed,
            trace.OverridePersonId,
            trace.OverrideReason,
            trace.RemediationRequired,
            trace.EvaluatedAt);

    private sealed record RequirementProjection(
        FactRequirement Requirement,
        FactDefinition Definition,
        RulePack? Pack,
        RegulatoryCitation? Citation);
}
