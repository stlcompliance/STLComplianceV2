using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class EventAndAuditEndpoints
{
    public static void MapStaffArrEventAndAuditEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("EventAndAudit").RequireAuthorization();

            group.MapGet("/events", async (
                Guid personId,
                int? page,
                int? pageSize,
                StaffArrAuthorizationService authorization,
                PersonnelHistoryService historyService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePersonHistoryRead(context.User, personId);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await historyService.ListPersonHistoryAsync(
                    tenantId,
                    personId,
                    page ?? 1,
                    pageSize ?? 50,
                    cancellationToken));
            })
            .WithName($"ListStaffArrEvents{nameSuffix}");

            group.MapGet("/audit", async (
                DateTimeOffset? from,
                DateTimeOffset? to,
                string? action,
                string? result,
                string? targetType,
                Guid? actorUserId,
                int? page,
                int? pageSize,
                StaffArrAuthorizationService authorization,
                AuditTimelineService auditService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAuditTimelineRead(context.User);
                var tenantId = context.User.GetTenantId();
                var filter = new AuditTimelineFilter(from, to, action, result, targetType, actorUserId);
                return Results.Ok(await auditService.ListAuditTimelineAsync(
                    tenantId,
                    filter,
                    page ?? 1,
                    pageSize ?? 25,
                    cancellationToken));
            })
            .WithName($"ListStaffArrAudit{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1"), "V1");
    }
}
