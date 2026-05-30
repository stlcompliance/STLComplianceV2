using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class OrgUnitEndpoints
{
    public static void MapStaffArrOrgUnitEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/org-units", Suffix: string.Empty),
            (Route: "/api/v1/org-units", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route).WithTags("OrgUnits").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                StaffArrAuthorizationService authorization,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePeopleRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
            })
            .WithName($"ListOrgUnits{suffix}");

            group.MapPost("/", async (
                CreateOrgUnitRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePeopleWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
                return Results.Created($"{route}/{created.OrgUnitId}", created);
            })
            .WithName($"CreateOrgUnit{suffix}");

            group.MapPut("/{orgUnitId:guid}", async (
                Guid orgUnitId,
                UpdateOrgUnitRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePeopleWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var updated = await service.UpdateAsync(tenantId, actorUserId, orgUnitId, request, cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"UpdateOrgUnit{suffix}");

            group.MapPatch("/{orgUnitId:guid}/status", async (
                Guid orgUnitId,
                UpdateOrgUnitStatusRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePeopleWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var updated = await service.UpdateStatusAsync(tenantId, actorUserId, orgUnitId, request, cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"UpdateOrgUnitStatus{suffix}");
        }
    }
}
