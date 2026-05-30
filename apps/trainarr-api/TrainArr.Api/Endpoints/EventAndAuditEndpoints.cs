using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Data;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class EventAndAuditEndpoints
{
    public static void MapTrainArrEventAndAuditEndpoints(this WebApplication app)
    {
        static void MapEventRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("EventAndAudit").RequireAuthorization();

            group.MapGet("/events", async (
                int? limit,
                TrainArrAuthorizationService authorization,
                TrainingEventProcessingService processingService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireEventProcessingSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await processingService.ListRecentAsync(tenantId, limit, cancellationToken));
            })
            .WithName($"ListTrainArrEvents{nameSuffix}");

            group.MapGet("/audit", async (
                DateTimeOffset? from,
                DateTimeOffset? to,
                string? action,
                string? result,
                string? targetType,
                Guid? actorUserId,
                int? page,
                int? pageSize,
                TrainArrAuthorizationService authorization,
                TrainArrDbContext db,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAuditPackageRead(context.User);
                var tenantId = context.User.GetTenantId();
                var pageValue = Math.Max(page ?? 1, 1);
                var pageSizeValue = Math.Clamp(pageSize ?? 25, 1, 200);

                var query = db.AuditEvents
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId);

                if (from.HasValue)
                {
                    query = query.Where(x => x.OccurredAt >= from.Value);
                }

                if (to.HasValue)
                {
                    query = query.Where(x => x.OccurredAt <= to.Value);
                }

                if (!string.IsNullOrWhiteSpace(action))
                {
                    query = query.Where(x => x.Action == action);
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    query = query.Where(x => x.Result == result);
                }

                if (!string.IsNullOrWhiteSpace(targetType))
                {
                    query = query.Where(x => x.TargetType == targetType);
                }

                if (actorUserId.HasValue)
                {
                    query = query.Where(x => x.ActorUserId == actorUserId.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);
                var items = await query
                    .OrderByDescending(x => x.OccurredAt)
                    .ThenByDescending(x => x.Id)
                    .Skip((pageValue - 1) * pageSizeValue)
                    .Take(pageSizeValue)
                    .Select(x => new TrainArrAuditTimelineItem(
                        x.Id,
                        x.TenantId,
                        x.ActorUserId,
                        x.Action,
                        x.TargetType,
                        x.TargetId,
                        x.Result,
                        x.ReasonCode,
                        x.CorrelationId,
                        x.OccurredAt))
                    .ToListAsync(cancellationToken);

                return Results.Ok(new TrainArrAuditTimelineResponse(
                    items,
                    pageValue,
                    pageSizeValue,
                    totalCount,
                    pageValue * pageSizeValue < totalCount));
            })
            .WithName($"ListTrainArrAudit{nameSuffix}");
        }

        MapEventRoutes(app.MapGroup("/api"), string.Empty);
        MapEventRoutes(app.MapGroup("/api/v1"), "V1");
    }

    private sealed record TrainArrAuditTimelineResponse(
        IReadOnlyList<TrainArrAuditTimelineItem> Items,
        int Page,
        int PageSize,
        int TotalCount,
        bool HasNextPage);

    private sealed record TrainArrAuditTimelineItem(
        Guid AuditEventId,
        Guid TenantId,
        Guid? ActorUserId,
        string Action,
        string TargetType,
        string? TargetId,
        string Result,
        string? ReasonCode,
        Guid CorrelationId,
        DateTimeOffset OccurredAt);
}
