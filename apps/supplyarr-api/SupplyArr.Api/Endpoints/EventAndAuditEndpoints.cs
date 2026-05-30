using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class EventAndAuditEndpoints
{
    public static void MapSupplyArrEventAndAuditEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("EventAndAudit").RequireAuthorization();

            group.MapGet("/events", async (
                int? limit,
                SupplyArrAuthorizationService authorization,
                IntegrationEventProcessingService processingService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIntegrationEventSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await processingService.ListRecentOutboxAsync(tenantId, limit, cancellationToken));
            })
            .WithName($"ListSupplyArrEvents{nameSuffix}");

            group.MapGet("/audit", async (
                int? limit,
                string? cursor,
                string? action,
                string? targetType,
                string? targetId,
                Guid? actorUserId,
                string? result,
                DateTimeOffset? fromOccurredAt,
                DateTimeOffset? toOccurredAt,
                SupplyArrAuthorizationService authorization,
                AuditHistoryService auditHistoryService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAuditHistoryRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actor = context.User.GetUserId();
                var response = await auditHistoryService.ListAsync(
                    tenantId,
                    limit,
                    cursor,
                    action,
                    targetType,
                    targetId,
                    actorUserId,
                    result,
                    fromOccurredAt,
                    toOccurredAt,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.audit.read",
                    tenantId,
                    actor,
                    "audit",
                    null,
                    "success",
                    reasonCode: $"result_count:{response.Items.Count}",
                    cancellationToken: cancellationToken);
                return Results.Ok(response);
            })
            .WithName($"ListSupplyArrAudit{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1"), "V1");
    }
}
