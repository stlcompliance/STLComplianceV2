using System.Text;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class PersonnelReportService(StaffArrDbContext db)
{
    private const int RecentLimit = 25;

    public async Task<PersonnelReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? employmentStatus,
        CancellationToken cancellationToken = default)
    {
        var people = await db.People
            .AsNoTracking()
            .Include(x => x.PrimaryOrgUnit)
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var filtered = people
            .Where(x => MatchesEmploymentStatusFilter(x, employmentStatus))
            .ToList();

        var activeCount = people.Count(x =>
            string.Equals(x.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase));
        var inactiveCount = people.Count(x =>
            string.Equals(x.EmploymentStatus, "inactive", StringComparison.OrdinalIgnoreCase));
        var onLeaveCount = people.Count(x =>
            string.Equals(x.EmploymentStatus, "leave", StringComparison.OrdinalIgnoreCase));
        var total = people.Count;
        var activePercent = total == 0 ? 0m : Math.Round(activeCount * 100m / total, 1);

        var recent = filtered
            .OrderByDescending(x => x.UpdatedAt)
            .Take(RecentLimit)
            .Select(x => new PersonnelReportSummaryItem(
                x.Id,
                x.DisplayName,
                x.PrimaryEmail,
                x.EmploymentStatus,
                x.PrimaryOrgUnit?.Name,
                x.JobTitle,
                x.UpdatedAt))
            .ToList();

        return new PersonnelReportSummaryResponse(
            filtered.Count,
            activeCount,
            inactiveCount,
            onLeaveCount,
            activePercent,
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? employmentStatus,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, employmentStatus, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "personId,displayName,primaryEmail,employmentStatus,primaryOrgUnitName,jobTitle,updatedAt");

        foreach (var item in summary.RecentPeople)
        {
            builder.Append(CsvEscape(item.PersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.DisplayName));
            builder.Append(',');
            builder.Append(CsvEscape(item.PrimaryEmail));
            builder.Append(',');
            builder.Append(CsvEscape(item.EmploymentStatus));
            builder.Append(',');
            builder.Append(CsvEscape(item.PrimaryOrgUnitName ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.JobTitle ?? string.Empty));
            builder.AppendLine(CsvEscape(item.UpdatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"staffarr-personnel-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static bool MatchesEmploymentStatusFilter(StaffPerson person, string? employmentStatus)
    {
        if (string.IsNullOrWhiteSpace(employmentStatus)
            || string.Equals(employmentStatus, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(
            person.EmploymentStatus,
            employmentStatus.Trim(),
            StringComparison.OrdinalIgnoreCase);
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
