using System.Text;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;

namespace StaffArr.Api.Services;

public sealed class CertificationReportService(StaffArrDbContext db)
{
    private const int RecentLimit = 50;
    private static readonly TimeSpan ExpiringSoonWindow = TimeSpan.FromDays(30);

    public async Task<CertificationReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        bool missingOnly,
        bool expiringOnly,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiringThreshold = now.Add(ExpiringSoonWindow);

        var people = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EmploymentStatus == "active")
            .Select(x => new { x.Id, x.DisplayName })
            .ToListAsync(cancellationToken);
        var peopleById = people.ToDictionary(x => x.Id, x => x.DisplayName);

        var definitions = await db.CertificationDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "active")
            .Select(x => new { x.Id, x.CertificationKey, x.Name })
            .ToListAsync(cancellationToken);

        var certifications = await db.PersonCertifications
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && peopleById.Keys.Contains(x.PersonId))
            .OrderByDescending(x => x.GrantedAt)
            .ToListAsync(cancellationToken);

        var activeCount = certifications.Count(x => x.Status == "active");
        var expiredCount = certifications.Count(x => x.Status == "expired");
        var expiringSoonCount = certifications.Count(x =>
            x.Status == "active"
            && x.ExpiresAt is not null
            && x.ExpiresAt >= now
            && x.ExpiresAt <= expiringThreshold);

        var latestByPersonAndDefinition = certifications
            .GroupBy(x => new { x.PersonId, x.CertificationDefinitionId })
            .ToDictionary(x => x.Key, x => x.OrderByDescending(c => c.GrantedAt).First());

        var missingCount = 0;
        foreach (var person in people)
        {
            foreach (var definition in definitions)
            {
                if (!latestByPersonAndDefinition.TryGetValue(
                        new { PersonId = person.Id, CertificationDefinitionId = definition.Id },
                        out var latest))
                {
                    missingCount++;
                    continue;
                }

                if (latest.Status != "active")
                {
                    missingCount++;
                }
            }
        }

        var definitionById = definitions.ToDictionary(x => x.Id, x => x);
        var recent = certifications
            .Where(x => !expiringOnly
                || (x.Status == "active"
                    && x.ExpiresAt is not null
                    && x.ExpiresAt >= now
                    && x.ExpiresAt <= expiringThreshold))
            .Where(x => !missingOnly || x.Status != "active")
            .Take(RecentLimit)
            .Select(x =>
            {
                definitionById.TryGetValue(x.CertificationDefinitionId, out var definition);
                peopleById.TryGetValue(x.PersonId, out var displayName);
                return new CertificationReportSummaryItem(
                    x.Id,
                    x.PersonId,
                    displayName ?? x.PersonId.ToString(),
                    definition?.CertificationKey ?? x.CertificationDefinitionId.ToString(),
                    definition?.Name ?? x.CertificationDefinitionId.ToString(),
                    x.Status,
                    x.GrantedAt,
                    x.ExpiresAt);
            })
            .ToList();

        return new CertificationReportSummaryResponse(
            people.Count,
            activeCount,
            expiringSoonCount,
            expiredCount,
            missingCount,
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        bool missingOnly,
        bool expiringOnly,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, missingOnly, expiringOnly, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "personCertificationId,personId,personDisplayName,certificationKey,certificationName,status,grantedAt,expiresAt");

        foreach (var item in summary.RecentCertifications)
        {
            builder.Append(CsvEscape(item.PersonCertificationId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.PersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.PersonDisplayName));
            builder.Append(',');
            builder.Append(CsvEscape(item.CertificationKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.CertificationName));
            builder.Append(',');
            builder.Append(CsvEscape(item.Status));
            builder.Append(',');
            builder.Append(CsvEscape(item.GrantedAt.ToString("O")));
            builder.AppendLine(CsvEscape(item.ExpiresAt?.ToString("O") ?? string.Empty));
        }

        return new CsvExportResult(
            "text/csv",
            $"staffarr-certification-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
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
