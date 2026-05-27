using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ComplianceFindingService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<ComplianceFindingResponse>> ListAsync(
        Guid tenantId,
        Guid? rulePackId = null,
        Guid? evaluationRunId = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.ComplianceFindings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (rulePackId.HasValue)
        {
            query = query.Where(x => x.RulePackId == rulePackId.Value);
        }

        if (evaluationRunId.HasValue)
        {
            query = query.Where(x => x.RuleEvaluationRunId == evaluationRunId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        var findings = await query
            .OrderByDescending(x => x.CreatedAt)
            .Join(
                db.RulePacks.AsNoTracking(),
                finding => finding.RulePackId,
                pack => pack.Id,
                (finding, pack) => new { finding, pack })
            .ToListAsync(cancellationToken);

        return findings.Select(x => MapResponse(x.finding, x.pack.PackKey)).ToList();
    }

    public async Task<ComplianceFindingResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateComplianceFindingRequest request,
        CancellationToken cancellationToken = default)
    {
        var rulePack = await db.RulePacks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.RulePackId && x.IsActive,
            cancellationToken);

        if (rulePack is null)
        {
            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
        }

        if (request.RuleEvaluationRunId.HasValue)
        {
            var runExists = await db.RuleEvaluationRuns.AnyAsync(
                x => x.TenantId == tenantId && x.Id == request.RuleEvaluationRunId.Value,
                cancellationToken);

            if (!runExists)
            {
                throw new StlApiException("rule_evaluation.not_found", "Rule evaluation run was not found.", 404);
            }
        }

        var severity = NormalizeSeverity(request.Severity);
        var findingKey = NormalizeFindingKey(request.FindingKey);

        var finding = new ComplianceFinding
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RulePackId = request.RulePackId,
            RuleEvaluationRunId = request.RuleEvaluationRunId,
            FindingKey = findingKey,
            Severity = severity,
            Status = FindingStatuses.Open,
            RuleKey = request.RuleKey?.Trim(),
            FactKey = request.FactKey?.Trim(),
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            ReasonCode = request.ReasonCode.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.ComplianceFindings.Add(finding);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "findings.create",
            tenantId,
            actorUserId,
            "compliance_finding",
            finding.Id.ToString(),
            "success",
            reasonCode: finding.ReasonCode,
            cancellationToken: cancellationToken);

        return MapResponse(finding, rulePack.PackKey);
    }

    public async Task<ComplianceFindingResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid findingId,
        Guid? actorUserId,
        UpdateComplianceFindingStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var finding = await db.ComplianceFindings.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == findingId,
            cancellationToken);

        if (finding is null)
        {
            throw new StlApiException("findings.not_found", "Finding was not found.", 404);
        }

        var status = NormalizeStatus(request.Status);
        finding.Status = status;
        await db.SaveChangesAsync(cancellationToken);

        var packKey = await db.RulePacks
            .AsNoTracking()
            .Where(x => x.Id == finding.RulePackId)
            .Select(x => x.PackKey)
            .FirstAsync(cancellationToken);

        await auditService.WriteAsync(
            "findings.update_status",
            tenantId,
            actorUserId,
            "compliance_finding",
            finding.Id.ToString(),
            "success",
            reasonCode: status,
            cancellationToken: cancellationToken);

        return MapResponse(finding, packKey);
    }

    public async Task<IReadOnlyList<ComplianceFindingResponse>> EmitFromEvaluationAsync(
        Guid tenantId,
        Guid rulePackId,
        string packKey,
        Guid evaluationRunId,
        string evaluationResult,
        IReadOnlyList<string> unresolvedFactKeys,
        IReadOnlyList<RuleEvaluationItemResponse> ruleResults,
        CancellationToken cancellationToken = default)
    {
        var emitted = new List<ComplianceFinding>();
        var now = DateTimeOffset.UtcNow;
        var runSuffix = evaluationRunId.ToString("N")[..8];

        foreach (var rule in ruleResults.Where(item =>
                     !string.Equals(item.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase)))
        {
            emitted.Add(new ComplianceFinding
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RulePackId = rulePackId,
                RuleEvaluationRunId = evaluationRunId,
                FindingKey = $"{packKey}_{rule.RuleKey}_{runSuffix}",
                Severity = FindingSeverities.Block,
                Status = FindingStatuses.Open,
                RuleKey = rule.RuleKey,
                Title = rule.Label,
                Message = rule.Message,
                ReasonCode = "rule_failed",
                CreatedAt = now,
            });
        }

        foreach (var factKey in unresolvedFactKeys)
        {
            emitted.Add(new ComplianceFinding
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RulePackId = rulePackId,
                RuleEvaluationRunId = evaluationRunId,
                FindingKey = $"{packKey}_{factKey}_unresolved_{runSuffix}",
                Severity = FindingSeverities.Warn,
                Status = FindingStatuses.Open,
                FactKey = factKey,
                Title = "Unresolved fact",
                Message = $"Required fact '{factKey}' could not be resolved.",
                ReasonCode = "fact_unresolved",
                CreatedAt = now,
            });
        }

        if (emitted.Count == 0 &&
            string.Equals(evaluationResult, RuleEvaluationResults.Fail, StringComparison.OrdinalIgnoreCase))
        {
            emitted.Add(new ComplianceFinding
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RulePackId = rulePackId,
                RuleEvaluationRunId = evaluationRunId,
                FindingKey = $"{packKey}_evaluation_fail_{runSuffix}",
                Severity = FindingSeverities.Block,
                Status = FindingStatuses.Open,
                Title = "Rule evaluation failed",
                Message = "Rule evaluation did not pass.",
                ReasonCode = "rule_evaluation_failed",
                CreatedAt = now,
            });
        }

        if (emitted.Count == 0)
        {
            return [];
        }

        db.ComplianceFindings.AddRange(emitted);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "findings.emit_from_evaluation",
            tenantId,
            actorUserId: null,
            "rule_evaluation_run",
            evaluationRunId.ToString(),
            "success",
            reasonCode: $"{emitted.Count}_findings",
            cancellationToken: cancellationToken);

        return emitted.Select(finding => MapResponse(finding, packKey)).ToList();
    }

    private static ComplianceFindingResponse MapResponse(ComplianceFinding finding, string packKey) =>
        new(
            finding.Id,
            finding.RulePackId,
            packKey,
            finding.RuleEvaluationRunId,
            finding.FindingKey,
            finding.Severity,
            finding.Status,
            finding.RuleKey,
            finding.FactKey,
            finding.Title,
            finding.Message,
            finding.ReasonCode,
            finding.CreatedAt);

    private static string NormalizeSeverity(string severity)
    {
        var normalized = severity.Trim().ToLowerInvariant();
        return normalized is FindingSeverities.Warn or FindingSeverities.Block
            ? normalized
            : throw new StlApiException("findings.validation", "Severity must be warn or block.", 400);
    }

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        return normalized is FindingStatuses.Open
            or FindingStatuses.Acknowledged
            or FindingStatuses.Resolved
            ? normalized
            : throw new StlApiException("findings.validation", "Status must be open, acknowledged, or resolved.", 400);
    }

    private static string NormalizeFindingKey(string findingKey)
    {
        var normalized = findingKey.Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            throw new StlApiException("findings.validation", "Finding key is required.", 400);
        }

        return normalized;
    }
}
