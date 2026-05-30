using MaintainArr.Api.Services;
using MaintainArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class EventAndAuditEndpoints
{
    public static void MapMaintainArrEventAndAuditEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api", Suffix: string.Empty),
            (Route: "/api/v1", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
                .WithTags("EventAndAudit")
                .RequireAuthorization();

            group.MapGet("/events", async (
                int? limit,
                MaintainArrAuthorizationService authorization,
                MaintenancePlatformEventProcessingService eventService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePlatformEventSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await eventService.ListRecentAsync(tenantId, limit, cancellationToken));
            })
            .WithName($"ListMaintainArrEvents{suffix}");

            group.MapGet("/audit", async (
                DateTimeOffset? from,
                DateTimeOffset? to,
                string? action,
                string? result,
                string? targetType,
                Guid? actorUserId,
                int? page,
                int? pageSize,
                MaintainArrAuthorizationService authorization,
                AuditPackageService auditService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAuditPackageRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await auditService.ListAuditTimelineAsync(
                    tenantId,
                    new AuditPackageFilter(from, to, action, result, targetType, actorUserId),
                    page ?? 1,
                    pageSize ?? 25,
                    cancellationToken));
            })
            .WithName($"ListMaintainArrAuditTimeline{suffix}");
        }
    }
}
