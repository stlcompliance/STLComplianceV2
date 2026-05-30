using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class EventAndAuditEndpoints
{
    public static void MapRoutArrEventAndAuditEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("EventAndAudit").RequireAuthorization();

            group.MapGet("/events", async (
                int? limit,
                RoutArrAuthorizationService authorization,
                IntegrationEventProcessingService processingService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIntegrationEventSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await processingService.ListRecentAsync(tenantId, limit, cancellationToken));
            })
            .WithName($"ListRoutArrEvents{nameSuffix}");

            group.MapGet("/audit", async (
                DateTimeOffset? from,
                DateTimeOffset? to,
                string? action,
                string? result,
                string? targetType,
                Guid? actorUserId,
                int? page,
                int? pageSize,
                RoutArrAuthorizationService authorization,
                AuditPackageService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAuditPackageRead(context.User);
                var tenantId = context.User.GetTenantId();
                var filter = new AuditPackageFilter(from, to, action, result, targetType, actorUserId);
                return Results.Ok(await service.ListAuditTimelineAsync(
                    tenantId,
                    filter,
                    page ?? 1,
                    pageSize ?? 25,
                    cancellationToken));
            })
            .WithName($"ListRoutArrAudit{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1"), "V1");
    }
}
