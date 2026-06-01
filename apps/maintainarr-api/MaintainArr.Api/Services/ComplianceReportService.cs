using System.Text;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class ComplianceReportService(MaintainArrDbContext db)
{
    private const int AttentionLimit = 50;
    private const int TemplateSummaryLimit = 100;

    private static readonly string[] OpenDefectStatuses =
    [
        DefectStatuses.Open,
        DefectStatuses.Acknowledged,
        DefectStatuses.InRepair,
    ];

    public async Task<ComplianceReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        bool? attentionOnly,
        string? siteRef,
        CancellationToken cancellationToken = default)
    {
        var generatedAt = DateTimeOffset.UtcNow;
        var assetIds = await ResolveAssetFilterAsync(tenantId, siteRef, cancellationToken);

        var inspectionTotals = await BuildInspectionTotalsAsync(tenantId, assetIds, cancellationToken);
        var defectTotals = await BuildDefectTotalsAsync(tenantId, assetIds, cancellationToken);
        var pmTotals = await BuildPmAdherenceTotalsAsync(tenantId, assetIds, cancellationToken);
        var mirrors = await db.ComplianceRegulatoryKeyMirrors
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var templateSummaries = await BuildTemplateSummariesAsync(
            tenantId,
            assetIds,
            mirrors,
            cancellationToken);

        var regulatoryGroups = BuildRegulatoryKeyGroups(mirrors, templateSummaries);
        var attentionItems = await BuildAttentionItemsAsync(
            tenantId,
            assetIds,
            cancellationToken);

        if (attentionOnly == true)
        {
            templateSummaries = templateSummaries
                .Where(x => x.RequiresAttention)
                .ToList();
            var attentionAssetIds = attentionItems.Select(x => x.AssetId).ToHashSet();
            regulatoryGroups = regulatoryGroups
                .Where(x => x.OpenComplianceIssueCount > 0)
                .ToList();
        }

        var defectSeverityCounts = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && OpenDefectStatuses.Contains(x.Status)
                && (assetIds == null || assetIds.Contains(x.AssetId)))
            .GroupBy(x => x.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new ComplianceReportSummaryResponse(
            generatedAt,
            inspectionTotals,
            defectTotals,
            pmTotals,
            mirrors.Count,
            regulatoryGroups,
            templateSummaries,
            attentionItems,
            defectSeverityCounts
                .OrderByDescending(x => x.Count)
                .Select(x => new ComplianceReportCountItem(x.Severity, x.Count))
                .ToList());
    }

    public async Task<ComplianceReportTemplateDetailResponse> GetTemplateDetailAsync(
        Guid tenantId,
        Guid inspectionTemplateId,
        CancellationToken cancellationToken = default)
    {
        var template = await db.InspectionTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == inspectionTemplateId, cancellationToken);
        if (template is null)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "inspection_templates.not_found",
                "Inspection template was not found.",
                404);
        }

        var runs = await db.InspectionRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InspectionTemplateId == inspectionTemplateId)
            .Select(x => new { x.Status, x.Result, x.CompletedAt })
            .ToListAsync(cancellationToken);

        var failedAnswers = await db.InspectionRunAnswers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.PassFailValue == InspectionAnswerPassFailValues.Fail
                && db.InspectionRuns.Any(r =>
                    r.TenantId == tenantId
                    && r.Id == x.InspectionRunId
                    && r.InspectionTemplateId == inspectionTemplateId))
            .CountAsync(cancellationToken);

        var completed = runs.Count(x => x.Status == InspectionRunStatuses.Completed);
        var passed = runs.Count(x =>
            x.Status == InspectionRunStatuses.Completed
            && x.Result == InspectionRunResults.Passed);
        var failed = runs.Count(x =>
            x.Status == InspectionRunStatuses.Completed
            && x.Result == InspectionRunResults.Failed);

        var inspectionTotals = new ComplianceReportInspectionTotals(
            runs.Count,
            completed,
            passed,
            failed,
            runs.Count(x => x.Status == InspectionRunStatuses.InProgress),
            failedAnswers,
            completed == 0 ? 0m : Math.Round((decimal)passed / completed * 100m, 2));

        var mirrors = await db.ComplianceRegulatoryKeyMirrors
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.SubjectType == ComplianceRegulatoryKeyMirrorSubjectTypes.InspectionTemplate
                && x.SubjectId == inspectionTemplateId)
            .ToListAsync(cancellationToken);

        var regulatoryKeys = mirrors
            .GroupBy(x => new { x.ComplianceKey, x.MaterialKey })
            .Select(g => new ComplianceReportRegulatoryKeyGroup(
                g.Key.ComplianceKey,
                g.Key.MaterialKey,
                g.Count(),
                g.Count(),
                failed))
            .ToList();

        var resultCounts = runs
            .Where(x => x.Result is not null)
            .GroupBy(x => x.Result!)
            .Select(g => new ComplianceReportCountItem(g.Key, g.Count()))
            .ToList();

        return new ComplianceReportTemplateDetailResponse(
            template.Id,
            template.TemplateKey,
            template.Name,
            inspectionTotals,
            regulatoryKeys,
            resultCounts);
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        bool? attentionOnly,
        string? siteRef,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, attentionOnly, siteRef, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("section,key,value");
        builder.AppendLine($"inspection,total_runs,{summary.InspectionTotals.TotalRuns}");
        builder.AppendLine($"inspection,failed_runs,{summary.InspectionTotals.FailedRuns}");
        builder.AppendLine($"inspection,pass_rate_percent,{summary.InspectionTotals.PassRatePercent:F2}");
        builder.AppendLine($"defects,open_critical,{summary.DefectTotals.OpenCriticalCount}");
        builder.AppendLine($"defects,open_high,{summary.DefectTotals.OpenHighCount}");
        builder.AppendLine($"pm,overdue,{summary.PmAdherenceTotals.OverdueCount}");
        builder.AppendLine($"pm,adherence_percent,{summary.PmAdherenceTotals.AdherencePercent:F2}");
        builder.AppendLine($"regulatory,mirror_count,{summary.RegulatoryKeyMirrorCount}");

        builder.AppendLine();
        builder.AppendLine("compliance_key,material_key,linked_subjects,open_issues");
        foreach (var group in summary.RegulatoryKeyGroups)
        {
            builder.AppendLine(
                $"{CsvEscape(group.ComplianceKey)},{CsvEscape(group.MaterialKey ?? "")},{group.LinkedSubjectCount},{group.OpenComplianceIssueCount}");
        }

        builder.AppendLine();
        builder.AppendLine("template_key,template_name,regulatory_keys,failed_runs,requires_attention");
        foreach (var template in summary.TemplateSummaries)
        {
            builder.AppendLine(
                $"{CsvEscape(template.TemplateKey)},{CsvEscape(template.TemplateName)},{template.RegulatoryKeyCount},{template.FailedRunCount},{template.RequiresAttention}");
        }

        return (
            "text/csv",
            $"maintainarr-compliance-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<IReadOnlyList<ComplianceAlertResponse>> ListAlertsAsync(
        Guid tenantId,
        string? siteRef,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var safeLimit = Math.Clamp(limit ?? 100, 1, 500);
        var assetIds = await ResolveAssetFilterAsync(tenantId, siteRef, cancellationToken);
        var attentionItems = await BuildAttentionItemsAsync(tenantId, assetIds, cancellationToken);

        return attentionItems
            .Select(item => new ComplianceAlertResponse(
                item.AssetId,
                item.AssetTag,
                item.AssetName,
                item.SiteRef,
                item.IssueType,
                AlertSeverity(item.IssueType),
                item.Message))
            .Take(safeLimit)
            .ToList();
    }

    private async Task<List<Guid>?> ResolveAssetFilterAsync(
        Guid tenantId,
        string? siteRef,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(siteRef))
        {
            return null;
        }

        var normalized = siteRef.Trim();
        return await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SiteRef == normalized)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    private async Task<ComplianceReportInspectionTotals> BuildInspectionTotalsAsync(
        Guid tenantId,
        List<Guid>? assetIds,
        CancellationToken cancellationToken)
    {
        var runsQuery = db.InspectionRuns.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (assetIds is not null)
        {
            runsQuery = runsQuery.Where(x => assetIds.Contains(x.AssetId));
        }

        var runs = await runsQuery
            .Select(x => new { x.Id, x.Status, x.Result })
            .ToListAsync(cancellationToken);

        var runIds = runs.Select(x => x.Id).ToList();
        var failedAnswers = runIds.Count == 0
            ? 0
            : await db.InspectionRunAnswers
                .AsNoTracking()
                .CountAsync(
                    x => x.TenantId == tenantId
                        && runIds.Contains(x.InspectionRunId)
                        && x.PassFailValue == InspectionAnswerPassFailValues.Fail,
                    cancellationToken);

        var completed = runs.Count(x => x.Status == InspectionRunStatuses.Completed);
        var passed = runs.Count(x =>
            x.Status == InspectionRunStatuses.Completed && x.Result == InspectionRunResults.Passed);
        var failed = runs.Count(x =>
            x.Status == InspectionRunStatuses.Completed && x.Result == InspectionRunResults.Failed);

        return new ComplianceReportInspectionTotals(
            runs.Count,
            completed,
            passed,
            failed,
            runs.Count(x => x.Status == InspectionRunStatuses.InProgress),
            failedAnswers,
            completed == 0 ? 0m : Math.Round((decimal)passed / completed * 100m, 2));
    }

    private async Task<ComplianceReportDefectTotals> BuildDefectTotalsAsync(
        Guid tenantId,
        List<Guid>? assetIds,
        CancellationToken cancellationToken)
    {
        var query = db.Defects.AsNoTracking()
            .Where(x => x.TenantId == tenantId && OpenDefectStatuses.Contains(x.Status));
        if (assetIds is not null)
        {
            query = query.Where(x => assetIds.Contains(x.AssetId));
        }

        var defects = await query
            .Select(x => new { x.Severity, x.Source })
            .ToListAsync(cancellationToken);

        return new ComplianceReportDefectTotals(
            defects.Count,
            defects.Count(x => string.Equals(x.Severity, DefectSeverities.Critical, StringComparison.OrdinalIgnoreCase)),
            defects.Count(x => string.Equals(x.Severity, DefectSeverities.High, StringComparison.OrdinalIgnoreCase)),
            defects.Count(x =>
                string.Equals(x.Source, DefectSources.InspectionAuto, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Source, DefectSources.InspectionManual, StringComparison.OrdinalIgnoreCase)),
            defects.Count(x => string.Equals(x.Source, DefectSources.Manual, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task<ComplianceReportPmAdherenceTotals> BuildPmAdherenceTotalsAsync(
        Guid tenantId,
        List<Guid>? assetIds,
        CancellationToken cancellationToken)
    {
        var query = db.PmSchedules.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "active");
        if (assetIds is not null)
        {
            query = query.Where(x => assetIds.Contains(x.AssetId));
        }

        var schedules = await query.Select(x => x.DueStatus).ToListAsync(cancellationToken);
        var overdue = schedules.Count(x =>
            string.Equals(x, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase));
        var due = schedules.Count(x => string.Equals(x, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase));
        var scheduled = schedules.Count(x =>
            string.Equals(x, PmDueStatuses.Scheduled, StringComparison.OrdinalIgnoreCase));
        var adherent = schedules.Count - overdue;
        var percent = schedules.Count == 0
            ? 100m
            : Math.Round((decimal)adherent / schedules.Count * 100m, 2);

        return new ComplianceReportPmAdherenceTotals(
            schedules.Count,
            overdue,
            due,
            scheduled,
            percent);
    }

    private async Task<IReadOnlyList<ComplianceReportTemplateSummaryItem>> BuildTemplateSummariesAsync(
        Guid tenantId,
        List<Guid>? assetIds,
        IReadOnlyList<ComplianceRegulatoryKeyMirror> mirrors,
        CancellationToken cancellationToken)
    {
        var mirroredTemplateIds = mirrors
            .Where(x => x.SubjectType == ComplianceRegulatoryKeyMirrorSubjectTypes.InspectionTemplate)
            .Select(x => x.SubjectId)
            .ToHashSet();

        var templates = await db.InspectionTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && (x.Status == InspectionTemplateStatuses.Active || mirroredTemplateIds.Contains(x.Id)))
            .OrderBy(x => x.TemplateKey)
            .Take(TemplateSummaryLimit)
            .ToListAsync(cancellationToken);

        var runsQuery = db.InspectionRuns.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (assetIds is not null)
        {
            runsQuery = runsQuery.Where(x => assetIds.Contains(x.AssetId));
        }

        var runStats = await runsQuery
            .GroupBy(x => x.InspectionTemplateId)
            .Select(g => new
            {
                TemplateId = g.Key,
                CompletedCount = g.Count(x => x.Status == InspectionRunStatuses.Completed),
                FailedCount = g.Count(x =>
                    x.Status == InspectionRunStatuses.Completed && x.Result == InspectionRunResults.Failed),
                LastFailedAt = g
                    .Where(x => x.Result == InspectionRunResults.Failed && x.CompletedAt != null)
                    .Max(x => x.CompletedAt),
            })
            .ToListAsync(cancellationToken);

        var statsByTemplate = runStats.ToDictionary(x => x.TemplateId);

        return templates
            .Select(template =>
            {
                statsByTemplate.TryGetValue(template.Id, out var stats);
                var keyCount = mirrors.Count(x =>
                    x.SubjectType == ComplianceRegulatoryKeyMirrorSubjectTypes.InspectionTemplate
                    && x.SubjectId == template.Id);
                var failedCount = stats?.FailedCount ?? 0;
                var requiresAttention = failedCount > 0 || keyCount == 0;

                return new ComplianceReportTemplateSummaryItem(
                    template.Id,
                    template.TemplateKey,
                    template.Name,
                    keyCount,
                    stats?.CompletedCount ?? 0,
                    failedCount,
                    stats?.LastFailedAt,
                    requiresAttention);
            })
            .ToList();
    }

    private static IReadOnlyList<ComplianceReportRegulatoryKeyGroup> BuildRegulatoryKeyGroups(
        IReadOnlyList<ComplianceRegulatoryKeyMirror> mirrors,
        IReadOnlyList<ComplianceReportTemplateSummaryItem> templateSummaries)
    {
        var attentionByTemplate = templateSummaries
            .Where(x => x.RequiresAttention)
            .ToDictionary(x => x.InspectionTemplateId, x => x.FailedRunCount);

        return mirrors
            .GroupBy(x => new { x.ComplianceKey, x.MaterialKey })
            .Select(g =>
            {
                var templateIds = g
                    .Where(x => x.SubjectType == ComplianceRegulatoryKeyMirrorSubjectTypes.InspectionTemplate)
                    .Select(x => x.SubjectId)
                    .Distinct()
                    .ToList();
                var openIssues = templateIds.Sum(id =>
                    attentionByTemplate.TryGetValue(id, out var failed) ? Math.Max(failed, 1) : 0);

                return new ComplianceReportRegulatoryKeyGroup(
                    g.Key.ComplianceKey,
                    g.Key.MaterialKey,
                    g.Count(),
                    templateIds.Count,
                    openIssues);
            })
            .OrderByDescending(x => x.OpenComplianceIssueCount)
            .ThenBy(x => x.ComplianceKey)
            .ToList();
    }

    private async Task<IReadOnlyList<ComplianceReportAttentionItem>> BuildAttentionItemsAsync(
        Guid tenantId,
        List<Guid>? assetIds,
        CancellationToken cancellationToken)
    {
        var items = new List<ComplianceReportAttentionItem>();

        var assetsQuery = db.Assets.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (assetIds is not null)
        {
            assetsQuery = assetsQuery.Where(x => assetIds.Contains(x.Id));
        }

        var assets = await assetsQuery
            .Select(x => new { x.Id, x.AssetTag, x.Name, x.SiteRef })
            .ToListAsync(cancellationToken);
        var assetMap = assets.ToDictionary(x => x.Id);

        var failedInspections = await db.InspectionRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Result == InspectionRunResults.Failed
                && (assetIds == null || assetIds.Contains(x.AssetId)))
            .OrderByDescending(x => x.CompletedAt)
            .Take(AttentionLimit)
            .Select(x => new { x.AssetId, x.CompletedAt })
            .ToListAsync(cancellationToken);

        foreach (var run in failedInspections)
        {
            if (!assetMap.TryGetValue(run.AssetId, out var asset))
            {
                continue;
            }

            items.Add(new ComplianceReportAttentionItem(
                asset.Id,
                asset.AssetTag,
                asset.Name,
                asset.SiteRef,
                "failed_inspection",
                $"Failed inspection{(run.CompletedAt is null ? "" : $" at {run.CompletedAt:u}")}"));
        }

        var overduePm = await db.PmSchedules
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Status == "active"
                && x.DueStatus == PmDueStatuses.Overdue
                && (assetIds == null || assetIds.Contains(x.AssetId)))
            .OrderBy(x => x.NextDueAt)
            .Take(AttentionLimit)
            .Select(x => new { x.AssetId, x.Name, x.NextDueAt })
            .ToListAsync(cancellationToken);

        foreach (var pm in overduePm)
        {
            if (!assetMap.TryGetValue(pm.AssetId, out var asset))
            {
                continue;
            }

            items.Add(new ComplianceReportAttentionItem(
                asset.Id,
                asset.AssetTag,
                asset.Name,
                asset.SiteRef,
                "overdue_pm",
                $"Overdue PM: {pm.Name} (due {pm.NextDueAt:u})"));
        }

        var criticalDefects = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && OpenDefectStatuses.Contains(x.Status)
                && x.Severity == DefectSeverities.Critical
                && (assetIds == null || assetIds.Contains(x.AssetId)))
            .OrderByDescending(x => x.CreatedAt)
            .Take(AttentionLimit)
            .Select(x => new { x.AssetId, x.Title })
            .ToListAsync(cancellationToken);

        foreach (var defect in criticalDefects)
        {
            if (!assetMap.TryGetValue(defect.AssetId, out var asset))
            {
                continue;
            }

            items.Add(new ComplianceReportAttentionItem(
                asset.Id,
                asset.AssetTag,
                asset.Name,
                asset.SiteRef,
                "critical_defect",
                $"Critical defect: {defect.Title}"));
        }

        return items
            .Take(AttentionLimit)
            .ToList();
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }

    private static string AlertSeverity(string issueType) => issueType switch
    {
        "critical_defect" => "critical",
        "failed_inspection" => "high",
        "overdue_pm" => "high",
        _ => "medium"
    };
}
