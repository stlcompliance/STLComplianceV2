using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class AuditEndpoints
{
    public static void MapAuditEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/audit")
            .WithTags("Audit")
            .RequireAuthorization();

        group.MapGet("/events", async (
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            Guid? tenantId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var timeline = await service.ListAuditTimelineAsync(
                new PlatformAuditPackageFilter(
                    TenantId: tenantId,
                    From: from,
                    To: to,
                    Action: action,
                    Result: result,
                    TargetType: targetType,
                    ActorUserId: actorUserId),
                page ?? 1,
                pageSize ?? 25,
                cancellationToken);
            return Results.Ok(timeline);
        })
        .WithName("GetAuditEventsV1");

        group.MapGet("/events/{id:guid}", async (
            Guid id,
            HttpContext context,
            PlatformAuthorizationService authorization,
            NexArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var item = await db.AuditEvents.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PlatformAuditEventExportItem(
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
                .FirstOrDefaultAsync(cancellationToken);

            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("GetAuditEventByIdV1");

        group.MapGet("/tenants/{tenantid:guid}", async (
            Guid tenantid,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var timeline = await service.ListAuditTimelineAsync(
                new PlatformAuditPackageFilter(
                    TenantId: tenantid,
                    From: from,
                    To: to,
                    Action: action,
                    Result: result,
                    TargetType: targetType,
                    ActorUserId: actorUserId),
                page ?? 1,
                pageSize ?? 25,
                cancellationToken);
            return Results.Ok(timeline);
        })
        .WithName("GetTenantAuditEventsV1");

        group.MapGet("/users/{personid:guid}", async (
            Guid personid,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var timeline = await service.ListAuditTimelineAsync(
                new PlatformAuditPackageFilter(
                    From: from,
                    To: to,
                    Action: action,
                    Result: result,
                    TargetType: targetType,
                    ActorUserId: personid),
                page ?? 1,
                pageSize ?? 25,
                cancellationToken);
            return Results.Ok(timeline);
        })
        .WithName("GetUserAuditEventsV1");

        group.MapGet("/products/{productcode}", async (
            string productcode,
            HttpContext context,
            PlatformAuthorizationService authorization,
            NexArrDbContext db,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var normalizedProductCode = productcode.Trim().ToLowerInvariant();
            var resolvedPage = page is null or < 1 ? 1 : page.Value;
            var resolvedPageSize = pageSize switch
            {
                null or < 1 => 25,
                > 100 => 100,
                _ => pageSize.Value
            };

            var query = db.AuditEvents.AsNoTracking().Where(x =>
                x.TargetId != null
                && x.TargetId.ToLower() == normalizedProductCode);

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.OccurredAt)
                .Skip((resolvedPage - 1) * resolvedPageSize)
                .Take(resolvedPageSize)
                .Select(x => new PlatformAuditEventExportItem(
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

            return Results.Ok(new PagedResult<PlatformAuditEventExportItem>(
                items,
                resolvedPage,
                resolvedPageSize,
                total,
                resolvedPage * resolvedPageSize < total));
        })
        .WithName("GetProductAuditEventsV1");
    }
}
