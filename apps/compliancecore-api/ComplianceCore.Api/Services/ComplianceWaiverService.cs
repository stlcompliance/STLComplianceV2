using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ComplianceWaiverService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public const string ExpireBatchActionScope = "compliancecore.waivers.expire_batch";

    public async Task<IReadOnlyList<ComplianceWaiverResponse>> ListAsync(
        Guid tenantId,
        string? status,
        string? packKey,
        string? scopeKey,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit ?? 50, 1, 200);
        var query = db.ComplianceWaivers.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(packKey))
        {
            var normalizedPackKey = packKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.PackKey == normalizedPackKey);
        }

        if (!string.IsNullOrWhiteSpace(scopeKey))
        {
            var normalizedScopeKey = ComplianceWaiverRules.NormalizeScopeKey(scopeKey);
            query = query.Where(x => x.SubjectScopeKey == normalizedScopeKey);
        }

        var rows = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        return rows.Select(MapResponse).ToList();
    }

    public async Task<ComplianceWaiverResponse> GetAsync(
        Guid tenantId,
        Guid waiverId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, waiverId, cancellationToken);
        return MapResponse(entity);
    }

    public async Task<ComplianceWaiverResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateComplianceWaiverRequest request,
        CancellationToken cancellationToken = default)
    {
        var waiverKey = ComplianceWaiverRules.NormalizeWaiverKey(request.WaiverKey);
        var scopeKey = ComplianceWaiverRules.NormalizeScopeKey(request.SubjectScopeKey);
        var ruleKey = ComplianceWaiverRules.NormalizeOptionalRuleKey(request.RuleKey);
        var gateKey = ComplianceWaiverRules.NormalizeOptionalGateKey(request.GateKey);
        var reasonCode = NormalizeReasonCode(request.ReasonCode);
        var explanation = NormalizeExplanation(request.Explanation);

        var exists = await db.ComplianceWaivers.AnyAsync(
            x => x.TenantId == tenantId && x.WaiverKey == waiverKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "waivers.duplicate",
                "A compliance waiver with this key already exists.",
                409);
        }

        var rulePack = await db.RulePacks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.RulePackId && x.IsActive,
            cancellationToken)
            ?? throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);

        await ValidateWaiverRuleTargetAsync(rulePack, ruleKey, cancellationToken);

        ValidateEffectiveWindow(request.EffectiveAt, request.ExpiresAt);

        var now = DateTimeOffset.UtcNow;
        var entity = new ComplianceWaiver
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WaiverKey = waiverKey,
            RulePackId = rulePack.Id,
            PackKey = rulePack.PackKey,
            RuleKey = ruleKey,
            GateKey = gateKey,
            SubjectScopeKey = scopeKey,
            ReasonCode = reasonCode,
            Explanation = explanation,
            Status = WaiverStatuses.Pending,
            EffectiveAt = request.EffectiveAt,
            ExpiresAt = request.ExpiresAt,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ComplianceWaivers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "waiver.create",
            tenantId,
            actorUserId,
            "compliance_waiver",
            entity.Id.ToString(),
            entity.Status,
            reasonCode: waiverKey,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<ComplianceWaiverResponse> ApproveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid waiverId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, waiverId, cancellationToken);
        if (!string.Equals(entity.Status, WaiverStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "waivers.invalid_status",
                "Only pending waivers can be approved.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = WaiverStatuses.Approved;
        entity.ApprovedByUserId = actorUserId;
        entity.ApprovedAt = now;
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "waiver.approve",
            tenantId,
            actorUserId,
            "compliance_waiver",
            entity.Id.ToString(),
            entity.Status,
            reasonCode: entity.WaiverKey,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<ComplianceWaiverResponse> RejectAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid waiverId,
        RejectComplianceWaiverRequest? request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, waiverId, cancellationToken);
        if (!string.Equals(entity.Status, WaiverStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "waivers.invalid_status",
                "Only pending waivers can be rejected.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = WaiverStatuses.Rejected;
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "waiver.reject",
            tenantId,
            actorUserId,
            "compliance_waiver",
            entity.Id.ToString(),
            entity.Status,
            reasonCode: request?.Notes ?? entity.WaiverKey,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<ComplianceWaiverResponse> RevokeAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid waiverId,
        RevokeComplianceWaiverRequest? request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, waiverId, cancellationToken);
        if (!string.Equals(entity.Status, WaiverStatuses.Approved, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "waivers.invalid_status",
                "Only approved waivers can be revoked.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = WaiverStatuses.Revoked;
        entity.RevokedByUserId = actorUserId;
        entity.RevokedAt = now;
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "waiver.revoke",
            tenantId,
            actorUserId,
            "compliance_waiver",
            entity.Id.ToString(),
            entity.Status,
            reasonCode: request?.Notes ?? entity.WaiverKey,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<ComplianceWaiverResponse> RenewAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid waiverId,
        RenewComplianceWaiverRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, waiverId, cancellationToken);
        if (!string.Equals(entity.Status, WaiverStatuses.Expired, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(entity.Status, WaiverStatuses.Approved, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "waivers.invalid_status",
                "Only approved or expired waivers can be renewed.",
                409);
        }

        ValidateEffectiveWindow(request.EffectiveAt, request.ExpiresAt);

        var now = DateTimeOffset.UtcNow;
        entity.Status = WaiverStatuses.Approved;
        entity.EffectiveAt = request.EffectiveAt;
        entity.ExpiresAt = request.ExpiresAt;
        entity.RevokedByUserId = null;
        entity.RevokedAt = null;
        entity.ApprovedByUserId = actorUserId;
        entity.ApprovedAt = now;
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "waiver.renew",
            tenantId,
            actorUserId,
            "compliance_waiver",
            entity.Id.ToString(),
            entity.Status,
            reasonCode: request.Notes ?? entity.WaiverKey,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<ProcessExpiredWaiversResponse> ProcessExpiredBatchAsync(
        ProcessExpiredWaiversRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = ComplianceWaiverRules.NormalizeBatchSize(request.BatchSize);

        var query = db.ComplianceWaivers.Where(x =>
            x.Status == WaiverStatuses.Approved
            && x.ExpiresAt.HasValue
            && x.ExpiresAt.Value <= asOf);

        if (request.TenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == request.TenantId.Value);
        }

        var expired = await query
            .OrderBy(x => x.ExpiresAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var expiredKeys = new List<string>();
        foreach (var waiver in expired)
        {
            if (!ComplianceWaiverRules.ShouldExpireForBatch(waiver.Status, waiver.ExpiresAt, asOf))
            {
                continue;
            }

            waiver.Status = WaiverStatuses.Expired;
            waiver.UpdatedAt = asOf;
            expiredKeys.Add(waiver.WaiverKey);

            await auditService.WriteAsync(
                "waiver.expire",
                waiver.TenantId,
                actorUserId: null,
                "compliance_waiver",
                waiver.Id.ToString(),
                WaiverStatuses.Expired,
                reasonCode: waiver.WaiverKey,
                cancellationToken: cancellationToken);
        }

        if (expired.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return new ProcessExpiredWaiversResponse(asOf, batchSize, expired.Count, expiredKeys);
    }

    public async Task<ComplianceWaiver?> TryFindActiveWaiverAsync(
        Guid tenantId,
        Guid rulePackId,
        string? ruleKey,
        string? gateKey,
        IReadOnlyDictionary<string, string>? context,
        CancellationToken cancellationToken = default)
    {
        var asOf = DateTimeOffset.UtcNow;
        var scopeKey = ResolveScopeKey(context);

        var candidates = await db.ComplianceWaivers
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.RulePackId == rulePackId
                && x.Status == WaiverStatuses.Approved
                && x.EffectiveAt <= asOf
                && (!x.ExpiresAt.HasValue || x.ExpiresAt > asOf))
            .OrderByDescending(x => x.ApprovedAt)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault(waiver =>
            ComplianceWaiverRules.MatchesEvaluationScope(waiver, rulePackId, ruleKey, gateKey, scopeKey));
    }

    public async Task<(string Outcome, string ReasonCode, string Message, Guid? WaiverId, string? WaiverKey)>
        ApplyWaiverIfEligibleAsync(
            Guid tenantId,
            Guid rulePackId,
            string packKey,
            string mappedOutcome,
            string mappedReasonCode,
            string mappedMessage,
            IReadOnlyList<RuleEvaluationItemResponse> ruleResults,
            string? gateKey,
            IReadOnlyDictionary<string, string>? context,
            CancellationToken cancellationToken)
    {
        if (string.Equals(mappedOutcome, ComplianceEvaluationOutcomes.Allow, StringComparison.OrdinalIgnoreCase))
        {
            return (mappedOutcome, mappedReasonCode, mappedMessage, null, null);
        }

        if (ComplianceWaiverRules.HasNonWaivableRuleFailure(ruleResults))
        {
            return (mappedOutcome, mappedReasonCode, mappedMessage, null, null);
        }

        var failedRuleKeys = ruleResults
            .Where(item => !string.Equals(item.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.RuleKey)
            .ToList();

        ComplianceWaiver? waiver = null;
        foreach (var failedRuleKey in failedRuleKeys)
        {
            waiver = await TryFindActiveWaiverAsync(
                tenantId,
                rulePackId,
                failedRuleKey,
                gateKey,
                context,
                cancellationToken);
            if (waiver is not null)
            {
                break;
            }
        }

        waiver ??= await TryFindActiveWaiverAsync(
            tenantId,
            rulePackId,
            ruleKey: null,
            gateKey,
            context,
            cancellationToken);

        if (waiver is null)
        {
            return (mappedOutcome, mappedReasonCode, mappedMessage, null, null);
        }

        return (
            ComplianceEvaluationOutcomes.Waived,
            "compliance_waiver_applied",
            $"Compliance waiver '{waiver.WaiverKey}' allows continuation for rule pack '{packKey}'. Original outcome was {mappedOutcome}.",
            waiver.Id,
            waiver.WaiverKey);
    }

    private async Task<ComplianceWaiver> LoadAsync(
        Guid tenantId,
        Guid waiverId,
        CancellationToken cancellationToken)
    {
        var entity = await db.ComplianceWaivers.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == waiverId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("waivers.not_found", "Compliance waiver was not found.", 404);
        }

        return entity;
    }

    private Task ValidateWaiverRuleTargetAsync(
        RulePack rulePack,
        string? ruleKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ruleKey) || string.IsNullOrWhiteSpace(rulePack.RuleContentJson))
        {
            return Task.CompletedTask;
        }

        var content = RuleEvaluator.ParseContent(rulePack.RuleContentJson);
        var normalizedRuleKey = ComplianceWaiverRules.NormalizeOptionalRuleKey(ruleKey)!;
        var matchingRule = content.Rules.FirstOrDefault(rule =>
            string.Equals(rule.RuleKey.Trim(), normalizedRuleKey, StringComparison.OrdinalIgnoreCase));

        if (matchingRule is null)
        {
            throw new StlApiException(
                "waivers.rule_not_found",
                $"Rule '{normalizedRuleKey}' was not found in rule pack content.",
                400);
        }

        if (matchingRule.NonWaivable)
        {
            throw new StlApiException(
                "waivers.non_waivable_rule",
                $"Rule '{normalizedRuleKey}' is marked non-waivable and cannot be targeted by a compliance waiver.",
                409);
        }

        return Task.CompletedTask;
    }

    private static string ResolveScopeKey(IReadOnlyDictionary<string, string>? context)
    {
        if (context is null || context.Count == 0)
        {
            return "tenant";
        }

        return ProductFactMirrorRules.ResolveScopeKeyFromContext(context);
    }

    private static string NormalizeReasonCode(string reasonCode)
    {
        var normalized = reasonCode.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 64)
        {
            throw new StlApiException(
                "waivers.validation",
                "Reason code must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeExplanation(string explanation)
    {
        var normalized = explanation.Trim();
        if (normalized.Length is < 8 or > 2000)
        {
            throw new StlApiException(
                "waivers.validation",
                "Explanation must be between 8 and 2000 characters.",
                400);
        }

        return normalized;
    }

    private static void ValidateEffectiveWindow(DateTimeOffset effectiveAt, DateTimeOffset? expiresAt)
    {
        if (expiresAt.HasValue && expiresAt.Value <= effectiveAt)
        {
            throw new StlApiException(
                "waivers.validation",
                "Expiration must be after the effective date.",
                400);
        }
    }

    private static ComplianceWaiverResponse MapResponse(ComplianceWaiver entity) =>
        new(
            entity.Id,
            entity.WaiverKey,
            entity.RulePackId,
            entity.PackKey,
            entity.RuleKey,
            entity.GateKey,
            entity.SubjectScopeKey,
            entity.ReasonCode,
            entity.Explanation,
            entity.Status,
            entity.EffectiveAt,
            entity.ExpiresAt,
            entity.CreatedByUserId,
            entity.ApprovedByUserId,
            entity.ApprovedAt,
            entity.RevokedByUserId,
            entity.RevokedAt,
            entity.CreatedAt,
            entity.UpdatedAt);
}
