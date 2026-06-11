using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class AuditTimelineService(StaffArrDbContext db)
{
    public async Task<PagedResult<StaffArrAuditEventExportItem>> ListAuditTimelineAsync(
        Guid tenantId,
        AuditTimelineFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 25,
            > 100 => 100,
            _ => pageSize,
        };

        var query = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        query = ApplyAuditEventFilters(query, filter);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StaffArrAuditEventExportItem(
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

        return new PagedResult<StaffArrAuditEventExportItem>(
            items,
            page,
            pageSize,
            totalCount,
            page * pageSize < totalCount);
    }

    private static IQueryable<StaffArrAuditEvent> ApplyAuditEventFilters(
        IQueryable<StaffArrAuditEvent> query,
        AuditTimelineFilter filter)
    {
        query = ApplyOccurredAtFilter(query, filter.From, filter.To);

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            var action = filter.Action.Trim().ToLowerInvariant();
            query = query.Where(x => x.Action.ToLower() == action);
        }

        if (!string.IsNullOrWhiteSpace(filter.Result))
        {
            var result = filter.Result.Trim().ToLowerInvariant();
            query = query.Where(x => x.Result.ToLower() == result);
        }

        if (!string.IsNullOrWhiteSpace(filter.TargetType))
        {
            var targetType = filter.TargetType.Trim().ToLowerInvariant();
            query = query.Where(x => x.TargetType.ToLower() == targetType);
        }

        if (filter.ActorUserId is Guid actorUserId)
        {
            query = query.Where(x => x.ActorUserId == actorUserId);
        }

        return query;
    }

    private static IQueryable<StaffArrAuditEvent> ApplyOccurredAtFilter(
        IQueryable<StaffArrAuditEvent> query,
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
            throw new StlApiException(
                "audit_timeline.invalid_date_range",
                "The 'from' date must be before or equal to the 'to' date.",
                400);
        }
    }
}
