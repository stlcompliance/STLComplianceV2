using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class AuditHistoryEndpoints
{
    public static void MapSupplyArrAuditHistoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/audit-history")
            .WithTags("AuditHistory")
            .RequireAuthorization();

        group.MapGet("/", async (
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
                "supplyarr.audit.history.read",
                tenantId,
                actor,
                "audit_history",
                null,
                "success",
                reasonCode: $"result_count:{response.Items.Count}",
                cancellationToken: cancellationToken);
            return Results.Ok(response);
        })
        .WithName("ListSupplyArrAuditHistory");
    }
}
