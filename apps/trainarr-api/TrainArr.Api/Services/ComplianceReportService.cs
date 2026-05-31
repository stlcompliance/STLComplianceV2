using System.Text;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class ComplianceReportService(TrainArrDbContext db)
{
    private const int RecentLimit = 25;

    public async Task<ComplianceReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        bool attentionOnly,
        CancellationToken cancellationToken = default)
    {
        var citationCount = await db.TrainingCitationAttachments
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId, cancellationToken);

        var requirementCount = await db.TrainingRulePackRequirements
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId, cancellationToken);

        var remediations = await db.StaffarrIncidentRemediations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var openRemediations = remediations.Count(x =>
            !string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(x.Status, "cancelled", StringComparison.OrdinalIgnoreCase));

        var attentionCount = openRemediations + (citationCount == 0 ? 1 : 0);
        var filtered = remediations
            .Where(x => !attentionOnly || !string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase))
            .Take(RecentLimit)
            .Select(x => new ComplianceReportRemediationItem(
                x.Id,
                x.StaffarrPersonId,
                x.ReasonCategoryKey,
                x.Status,
                x.CreatedAt,
                x.UpdatedAt))
            .ToList();

        return new ComplianceReportSummaryResponse(
            citationCount,
            requirementCount,
            openRemediations,
            remediations.Count,
            attentionCount,
            filtered);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        bool attentionOnly,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, attentionOnly, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("remediationId,staffarrPersonId,reasonCategoryKey,status,createdAt,updatedAt");

        foreach (var item in summary.RecentRemediations)
        {
            builder.Append(CsvEscape(item.RemediationId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.StaffarrPersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.ReasonCategoryKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.Status));
            builder.Append(',');
            builder.Append(CsvEscape(item.CreatedAt.ToString("O")));
            builder.AppendLine(CsvEscape(item.UpdatedAt.ToString("O")));
        }

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        return new CsvExportResult(
            "text/csv",
            $"trainarr-compliance-report-{timestamp}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<TrainingGapReportResponse> GetGapReportAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var assignments = await db.TrainingAssignments
            .AsNoTracking()
            .Include(x => x.TrainingDefinition)
            .Include(x => x.QualificationIssue)
            .Where(x => x.TenantId == tenantId
                && AssignmentDueReminderRules.OpenAssignmentStatuses.Contains(x.Status))
            .OrderBy(x => x.DueAt ?? DateTimeOffset.MaxValue)
            .Take(25)
            .ToListAsync(cancellationToken);

        var personQualificationKeys = assignments
            .Select(x => (x.StaffarrPersonId, x.TrainingDefinition.QualificationKey))
            .Where(x => !string.IsNullOrWhiteSpace(x.QualificationKey))
            .Distinct()
            .ToList();

        var personIds = personQualificationKeys.Select(x => x.StaffarrPersonId).Distinct().ToList();
        var qualificationKeys = personQualificationKeys.Select(x => x.QualificationKey).Distinct().ToList();
        var issuedQualifications = await db.QualificationIssues
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && personIds.Contains(x.StaffarrPersonId)
                && qualificationKeys.Contains(x.QualificationKey)
                && x.Status == "issued"
                && (x.ExpiresAt == null || x.ExpiresAt > DateTimeOffset.UtcNow))
            .Select(x => new { x.StaffarrPersonId, x.QualificationKey })
            .ToListAsync(cancellationToken);

        var issuedLookup = issuedQualifications
            .Select(x => (x.StaffarrPersonId, x.QualificationKey))
            .ToHashSet();

        var gaps = assignments
            .Where(x => !issuedLookup.Contains((x.StaffarrPersonId, x.TrainingDefinition.QualificationKey)))
            .Select(x => new TrainingGapReportItem(
                x.Id,
                x.StaffarrPersonId,
                x.TrainingDefinitionId,
                x.TrainingDefinition.DefinitionKey,
                x.TrainingDefinition.Name,
                x.TrainingDefinition.QualificationKey,
                x.TrainingDefinition.QualificationName,
                x.Status,
                x.DueAt,
                "missing_issued_qualification",
                $"No current issued qualification exists for {x.TrainingDefinition.QualificationKey}."))
            .ToList();

        return new TrainingGapReportResponse(DateTimeOffset.UtcNow, gaps.Count, gaps);
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
