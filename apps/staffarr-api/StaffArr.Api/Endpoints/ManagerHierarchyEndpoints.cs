using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class ManagerHierarchyEndpoints
{
    public static void MapStaffArrManagerHierarchyEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/people/{personId:guid}", Suffix: string.Empty),
            (Route: "/api/v1/people/{personId:guid}", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
                .WithTags("ManagerHierarchy")
                .RequireAuthorization();

            group.MapPut("/manager", async (
                Guid personId,
                UpdatePersonManagerRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                ManagerHierarchyService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePeopleWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var updated = await service.UpdateManagerAsync(
                    tenantId,
                    actorUserId,
                    personId,
                    request,
                    cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"UpdatePersonManager{suffix}");

            group.MapGet("/manager-chain", async (
                Guid personId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                ManagerHierarchyService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePeopleRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetManagerChainAsync(tenantId, personId, cancellationToken));
            })
            .WithName($"GetManagerChain{suffix}");

            group.MapGet("/subordinates", async (
                Guid personId,
                bool? includeIndirect,
                int? limit,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                ManagerHierarchyService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePeopleRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetSubordinatesAsync(
                    tenantId,
                    personId,
                    includeIndirect ?? true,
                    limit ?? 200,
                    cancellationToken));
            })
            .WithName($"ListSubordinates{suffix}");

            group.MapGet("/subordinates/{subordinatePersonId:guid}", async (
                Guid personId,
                Guid subordinatePersonId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                ManagerHierarchyService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePeopleRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetSubordinateDetailAsync(
                    tenantId,
                    personId,
                    subordinatePersonId,
                    cancellationToken));
            })
            .WithName($"GetSubordinateDetail{suffix}");
        }
    }
}
