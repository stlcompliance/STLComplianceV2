using LoadArr.Api.Services;
using LoadArr.Api.Settings;
using STLCompliance.Shared.Auth;

namespace LoadArr.Api.Endpoints;

public static class FieldInboxEndpoints
{
    public static void MapLoadArrFieldInboxEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("FieldInbox").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                FieldInboxService service,
                LoadArrAuthorizationService authorization,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                authorization.RequireWorkspaceRead(context.User);
                return Results.Ok(await service.GetAsync(tenantId, cancellationToken));
            })
            .WithName($"GetLoadArrFieldInbox{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/field-inbox"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/field-inbox"), "V1");
    }
}
