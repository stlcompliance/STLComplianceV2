using System.Text;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;

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

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
