using LoadArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace LoadArr.Api.Endpoints;

public static class FieldInboxEndpoints
{
    public static void MapLoadArrFieldInboxEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("FieldInbox").RequireAuthorization();

            group.MapGet("/", (HttpContext context, FieldInboxService service) =>
            {
                _ = context.User.GetTenantId();
                return Results.Ok(service.Get());
            })
            .WithName($"GetLoadArrFieldInbox{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/field-inbox"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/field-inbox"), "V1");
    }
}
