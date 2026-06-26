using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class FieldInboxEndpoints
{
    public static void MapStaffArrFieldInboxEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/field-inbox")
            .WithTags("FieldInbox")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid? personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            FieldInboxService service,
            CancellationToken cancellationToken) =>
        {
            var effectivePersonId = personId;
            if (effectivePersonId is null
                && !MatchesIncidentManagerRole(context.User.GetTenantRoleKey()))
            {
                effectivePersonId = context.User.GetPersonId();
            }

            authorization.RequireIncidentsRead(context.User, effectivePersonId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, effectivePersonId, cancellationToken));
        })
        .WithName("GetStaffArrFieldInbox");
    }

    private static bool MatchesIncidentManagerRole(string roleKey) =>
        roleKey.Equals("tenant_admin", StringComparison.OrdinalIgnoreCase)
        || roleKey.Equals("staffarr_admin", StringComparison.OrdinalIgnoreCase)
        || roleKey.Equals("hr_admin", StringComparison.OrdinalIgnoreCase)
        || roleKey.Equals("supervisor", StringComparison.OrdinalIgnoreCase);
}
