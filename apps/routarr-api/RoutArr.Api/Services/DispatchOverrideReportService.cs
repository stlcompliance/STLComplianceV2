using System.Text;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class DispatchOverrideReportService(RoutArrDbContext db)
{
    private const int RecentOverrideLimit = 50;

    public async Task<DispatchOverrideReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var normalizedScope = DispatchOverrideReportRules.NormalizeScope(scope);
        var now = DateTimeOffset.UtcNow;
        var (windowStart, windowEnd) = DispatchOverrideReportRules.GetWindow(normalizedScope, now);

        var auditEvents = await db.AuditEvents
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.OccurredAt >= windowStart
                && x.OccurredAt < windowEnd
                && DispatchOverrideReportRules.AssignmentActions.Contains(x.Action))
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(cancellationToken);

        var overrideEntries = auditEvents
            .Where(x => DispatchOverrideReportRules.IsOverrideAuditEntry(x.Action, x.Result))
            .Select(MapEntry)
            .ToList();

        var overrideKindCounts = overrideEntries
            .SelectMany(x => x.OverrideKinds)
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(g => new DispatchOverrideReportCountItem(g.Key, g.Count()))
            .OrderBy(x => x.Key)
            .ToList();

        return new DispatchOverrideReportSummaryResponse(
            now,
            normalizedScope,
            windowStart,
            windowEnd,
            overrideEntries.Count,
            overrideEntries.Count(x => string.Equals(x.Action, "trip.assign_driver", StringComparison.OrdinalIgnoreCase)),
            overrideEntries.Count(x => string.Equals(x.Action, "trip.assign_vehicle", StringComparison.OrdinalIgnoreCase)),
            overrideKindCounts,
            overrideEntries.Take(RecentOverrideLimit).ToList());
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, scope, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "occurredAt,action,targetType,targetId,overrideKinds,result,actorUserId,auditEventId");

        foreach (var entry in summary.RecentOverrides)
        {
            builder.Append(entry.OccurredAt.ToString("O"));
            builder.Append(',');
            builder.Append(CsvEscape(entry.Action));
            builder.Append(',');
            builder.Append(CsvEscape(entry.TargetType));
            builder.Append(',');
            builder.Append(CsvEscape(entry.TargetId ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(string.Join(';', entry.OverrideKinds)));
            builder.Append(',');
            builder.Append(CsvEscape(entry.Result));
            builder.Append(',');
            builder.Append(entry.ActorUserId?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.AppendLine(entry.AuditEventId.ToString());
        }

        var fileName = $"routarr-dispatch-override-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static DispatchOverrideReportEntry MapEntry(RoutArrAuditEvent entity) =>
        new(
            entity.Id,
            entity.ActorUserId,
            entity.Action,
            entity.TargetType,
            entity.TargetId,
            entity.Result,
            DispatchOverrideReportRules.ParseOverrideKinds(entity.Result),
            entity.OccurredAt);

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
