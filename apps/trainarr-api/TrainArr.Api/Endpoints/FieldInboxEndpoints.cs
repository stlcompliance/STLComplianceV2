using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class FieldInboxEndpoints
{
    public static void MapTrainArrFieldInboxEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/field-inbox")
            .WithTags("FieldInbox")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid? staffarrPersonId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            FieldInboxService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentsRead(context.User, staffarrPersonId);
            var tenantId = context.User.GetTenantId();
            var effectivePersonId = staffarrPersonId;
            if (effectivePersonId is null
                && !context.User.IsPlatformAdmin()
                && string.Equals(context.User.GetTenantRoleKey(), "tenant_member", StringComparison.OrdinalIgnoreCase))
            {
                effectivePersonId = context.User.GetPersonId();
            }

            return Results.Ok(await service.GetAsync(tenantId, effectivePersonId, cancellationToken));
        })
        .WithName("GetTrainArrFieldInbox");
    }
}
