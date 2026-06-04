using System.IO.Compression;
using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class AuditPackageService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public AuditPackageManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Sections:
            [
                new("audit_events", "audit_events.json", "Audit events", "Tenant-scoped Compliance Core audit trail."),
                new("findings", "findings.json", "Findings", "Compliance findings from evaluations and workflow gates."),
                new("evaluation_runs", "evaluation_runs.json", "Evaluation runs", "Deterministic rule evaluation run history."),
                new("workflow_gate_checks", "workflow_gate_checks.json", "Workflow gate checks", "Workflow gate outcomes with applied waiver audit trail."),
                new("waivers", "waivers.json", "Waivers", "Compliance waivers and exception approvals."),
                new("rule_packs", "rule_packs.json", "Rule packs", "Rule pack metadata and publication status."),
            ]);

    public async Task<AuditPackageExportResponse> BuildExportAsync(
        Guid tenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var package = await MaterializeExportAsync(tenantId, from, to, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildDateRangeReasonCode(from, to),
            cancellationToken: cancellationToken);

        return package;
    }

    public async Task<byte[]> ExportZipAsync(
        Guid tenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var package = await MaterializeExportAsync(tenantId, from, to, cancellationToken);
        var zipBytes = await CreateZipBytesAsync(package, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildDateRangeReasonCode(from, to),
            cancellationToken: cancellationToken);

        return zipBytes;
    }

    public async Task<AuditPackageExportResponse> MaterializeExportAsync(
        Guid tenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var package = await LoadPackageDataAsync(tenantId, from, to, cancellationToken);
        return package with { PackageId = Guid.NewGuid() };
    }

    public async Task<byte[]> CreateZipBytesAsync(
        AuditPackageExportResponse package,
        CancellationToken cancellationToken = default)
    {
        await using var memory = new MemoryStream();
        await WriteZipAsync(package, memory, cancellationToken);

        return memory.ToArray();
    }

    public async Task WriteZipAsync(
        AuditPackageExportResponse package,
        Stream output,
        CancellationToken cancellationToken = default)
    {
        await using var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);

        await WriteJsonEntryAsync(archive, "manifest.json", new
        {
            package.PackageId,
            package.TenantId,
            package.GeneratedAt,
            package.DateRange,
            package.Counts,
            PackageVersion = "1",
        }, cancellationToken);

        await WriteJsonEntryAsync(archive, "audit_events.json", package.AuditEvents, cancellationToken);
        await WriteJsonEntryAsync(archive, "findings.json", package.Findings, cancellationToken);
        await WriteJsonEntryAsync(archive, "evaluation_runs.json", package.EvaluationRuns, cancellationToken);
        await WriteJsonEntryAsync(archive, "workflow_gate_checks.json", package.WorkflowGateChecks, cancellationToken);
        await WriteJsonEntryAsync(archive, "waivers.json", package.Waivers, cancellationToken);
        await WriteJsonEntryAsync(archive, "rule_packs.json", package.RulePacks, cancellationToken);
    }

    private async Task<AuditPackageExportResponse> LoadPackageDataAsync(
        Guid tenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var auditEventsQuery = db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);
        auditEventsQuery = ApplyOccurredAtFilter(auditEventsQuery, from, to);

        var findingsQuery = db.ComplianceFindings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            findingsQuery = findingsQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            findingsQuery = findingsQuery.Where(x => x.CreatedAt <= to);
        }

        var evaluationsQuery = db.RuleEvaluationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            evaluationsQuery = evaluationsQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            evaluationsQuery = evaluationsQuery.Where(x => x.CreatedAt <= to);
        }

        var gateChecksQuery = db.WorkflowGateCheckResults
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            gateChecksQuery = gateChecksQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            gateChecksQuery = gateChecksQuery.Where(x => x.CreatedAt <= to);
        }

        var waiversQuery = db.ComplianceWaivers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            waiversQuery = waiversQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            waiversQuery = waiversQuery.Where(x => x.CreatedAt <= to);
        }

        var auditEvents = await auditEventsQuery
            .OrderBy(x => x.OccurredAt)
            .Select(x => new AuditEventExportItem(
                x.Id,
                x.ActorUserId,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.ReasonCode,
                x.CorrelationId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        var findings = await findingsQuery
            .OrderBy(x => x.CreatedAt)
            .Join(
                db.RulePacks.AsNoTracking(),
                finding => finding.RulePackId,
                pack => pack.Id,
                (finding, pack) => new AuditPackageFindingItem(
                    finding.Id,
                    finding.RulePackId,
                    pack.PackKey,
                    finding.RuleEvaluationRunId,
                    finding.FindingKey,
                    finding.Severity,
                    finding.Status,
                    finding.RuleKey,
                    finding.FactKey,
                    finding.Title,
                    finding.Message,
                    finding.ReasonCode,
                    finding.CreatedAt))
            .ToListAsync(cancellationToken);

        var evaluationRuns = await evaluationsQuery
            .OrderBy(x => x.CreatedAt)
            .Join(
                db.RulePacks.AsNoTracking(),
                run => run.RulePackId,
                pack => pack.Id,
                (run, pack) => new AuditPackageEvaluationRunItem(
                    run.Id,
                    run.RulePackId,
                    pack.PackKey,
                    run.ActorUserId,
                    run.Status,
                    run.OverallResult,
                    run.FactInputsJson,
                    run.RuleResultsJson,
                    run.AppliedWaiverId,
                    run.AppliedWaiverKey,
                    run.CreatedAt))
            .ToListAsync(cancellationToken);

        var workflowGateChecks = await gateChecksQuery
            .OrderBy(x => x.CreatedAt)
            .Join(
                db.WorkflowGateDefinitions.AsNoTracking(),
                check => check.WorkflowGateDefinitionId,
                gate => gate.Id,
                (check, gate) => new { check, gate })
            .Join(
                db.RulePacks.AsNoTracking(),
                item => item.gate.RulePackId,
                pack => pack.Id,
                (item, pack) => new AuditPackageWorkflowGateCheckItem(
                    item.check.Id,
                    item.check.GateKey,
                    pack.Id,
                    pack.PackKey,
                    item.check.RuleEvaluationRunId,
                    item.check.Outcome,
                    item.check.ReasonCode,
                    item.check.Message,
                    item.check.AppliedWaiverId,
                    item.check.AppliedWaiverKey,
                    item.check.CreatedAt))
            .ToListAsync(cancellationToken);

        var waivers = await waiversQuery
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AuditPackageWaiverItem(
                x.Id,
                x.WaiverKey,
                x.RulePackId,
                x.PackKey,
                x.RuleKey,
                x.GateKey,
                x.SubjectScopeKey,
                x.ReasonCode,
                x.Explanation,
                x.Status,
                x.EffectiveAt,
                x.ExpiresAt,
                x.ApprovedByUserId,
                x.ApprovedAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        var rulePacks = await db.RulePacks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.PackKey)
            .ThenBy(x => x.VersionNumber)
            .Join(
                db.RegulatoryPrograms.AsNoTracking(),
                pack => pack.RegulatoryProgramId,
                program => program.Id,
                (pack, program) => new AuditPackageRulePackItem(
                    pack.Id,
                    pack.PackKey,
                    pack.Label,
                    pack.Description,
                    pack.VersionNumber,
                    pack.Status,
                    pack.IsActive,
                    pack.RegulatoryProgramId,
                    program.ProgramKey,
                    pack.RuleContentJson != null,
                    pack.CreatedAt,
                    pack.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new AuditPackageExportResponse(
            PackageId: Guid.Empty,
            TenantId: tenantId,
            GeneratedAt: DateTimeOffset.UtcNow,
            DateRange: from is null && to is null
                ? null
                : new AuditPackageDateRangeResponse(from, to),
            Counts: new AuditPackageCountsResponse(
                auditEvents.Count,
                findings.Count,
                evaluationRuns.Count,
                workflowGateChecks.Count,
                waivers.Count,
                rulePacks.Count),
            AuditEvents: auditEvents,
            Findings: findings,
            EvaluationRuns: evaluationRuns,
            WorkflowGateChecks: workflowGateChecks,
            Waivers: waivers,
            RulePacks: rulePacks);
    }

    private static IQueryable<Entities.ComplianceCoreAuditEvent> ApplyOccurredAtFilter(
        IQueryable<Entities.ComplianceCoreAuditEvent> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(x => x.OccurredAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.OccurredAt <= to);
        }

        return query;
    }

    private static void ValidateDateRange(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from is not null && to is not null && from > to)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "audit_package.invalid_date_range",
                "The 'from' date must be before or equal to the 'to' date.",
                400);
        }
    }

    private static string? BuildDateRangeReasonCode(DateTimeOffset? from, DateTimeOffset? to) =>
        from is null && to is null ? "all_time" : "date_filtered";

    private static async Task WriteJsonEntryAsync<T>(
        ZipArchive archive,
        string entryName,
        T payload,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        await JsonSerializer.SerializeAsync(entryStream, payload, JsonOptions, cancellationToken);
    }
}
