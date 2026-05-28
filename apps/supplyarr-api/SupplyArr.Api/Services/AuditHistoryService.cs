using System.Text;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class AuditHistoryService(SupplyArrDbContext db)
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 100;

    public async Task<AuditHistoryListResponse> ListAsync(
        Guid tenantId,
        int? limit,
        string? cursor,
        string? action,
        string? targetType,
        string? targetId,
        Guid? actorUserId,
        string? result,
        DateTimeOffset? fromOccurredAt,
        DateTimeOffset? toOccurredAt,
        CancellationToken cancellationToken = default)
    {
        var effectiveLimit = Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);
        var query = db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(action))
        {
            var normalizedAction = action.Trim().ToLowerInvariant();
            query = query.Where(x => x.Action.ToLower().Contains(normalizedAction));
        }

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            var normalizedTargetType = targetType.Trim().ToLowerInvariant();
            query = query.Where(x => x.TargetType.ToLower() == normalizedTargetType);
        }

        if (!string.IsNullOrWhiteSpace(targetId))
        {
            query = query.Where(x => x.TargetId == targetId.Trim());
        }

        if (actorUserId is not null)
        {
            query = query.Where(x => x.ActorUserId == actorUserId);
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            var normalizedResult = result.Trim().ToLowerInvariant();
            query = query.Where(x => x.Result.ToLower() == normalizedResult);
        }

        if (fromOccurredAt is not null)
        {
            query = query.Where(x => x.OccurredAt >= fromOccurredAt.Value);
        }

        if (toOccurredAt is not null)
        {
            query = query.Where(x => x.OccurredAt <= toOccurredAt.Value);
        }

        if (TryDecodeCursor(cursor, out var cursorOccurredAt, out var cursorId))
        {
            query = query.Where(x =>
                x.OccurredAt < cursorOccurredAt
                || (x.OccurredAt == cursorOccurredAt && x.Id.CompareTo(cursorId) < 0));
        }

        var rows = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Take(effectiveLimit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > effectiveLimit;
        if (hasMore)
        {
            rows = rows.Take(effectiveLimit).ToList();
        }

        string? nextCursor = null;
        if (hasMore && rows.Count > 0)
        {
            var last = rows[^1];
            nextCursor = EncodeCursor(last.OccurredAt, last.Id);
        }

        return new AuditHistoryListResponse(
            rows.Select(Map).ToList(),
            nextCursor,
            hasMore);
    }

    private static AuditHistoryItemResponse Map(SupplyArrAuditEvent entity) =>
        new(
            entity.Id,
            entity.ActorUserId,
            entity.Action,
            entity.TargetType,
            entity.TargetId,
            entity.Result,
            entity.ReasonCode,
            entity.CorrelationId,
            entity.OccurredAt);

    private static string EncodeCursor(DateTimeOffset occurredAt, Guid id) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{occurredAt:O}|{id:D}"));

    private static bool TryDecodeCursor(string? cursor, out DateTimeOffset occurredAt, out Guid id)
    {
        occurredAt = default;
        id = default;
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return false;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var separatorIndex = decoded.LastIndexOf('|');
            if (separatorIndex <= 0
                || !DateTimeOffset.TryParse(decoded[..separatorIndex], out occurredAt)
                || !Guid.TryParse(decoded[(separatorIndex + 1)..], out id))
            {
                throw new StlApiException("audit_history.invalid_cursor", "Audit history cursor is invalid.", 400);
            }

            return true;
        }
        catch (FormatException)
        {
            throw new StlApiException("audit_history.invalid_cursor", "Audit history cursor is invalid.", 400);
        }
    }
}
