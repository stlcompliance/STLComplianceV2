using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class OrgUnitEndpoints
{
    public static void MapStaffArrOrgUnitEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/org-units").WithTags("OrgUnits").RequireAuthorization();

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
        .WithName("ListOrgUnits");
    }
}
