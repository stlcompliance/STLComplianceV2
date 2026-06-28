using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Operations;
using LoadArr.Api.Settings;

namespace LoadArr.Api.Endpoints;

public static class LoadArrAdminEndpoints
{
    public static void MapLoadArrAdminEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/admin"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/admin"), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder admin, string suffix)
    {
        admin = admin.WithTags("Admin").RequireAuthorization();

        admin.MapGet("/permissions", (HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireTenantSettingsRead(context.User);
            _ = context.User.GetTenantId();
            var permissions = StlLoadArrPermissionCatalog.All
                .Select(item => new LoadArrPermissionCatalogItemResponse(
                    "loadarr",
                    item.PermissionKey,
                    item.Label,
                    item.Description,
                    item.Scope,
                    item.Sensitivity,
                    item.Status))
                .ToList();

            return Results.Ok(new LoadArrPermissionCatalogResponse(permissions));
        })
        .ExcludeFromDescription()
        .WithName($"GetLoadArrPermissionCatalog{suffix}");
    }
}

public sealed record LoadArrPermissionCatalogResponse(
    IReadOnlyList<LoadArrPermissionCatalogItemResponse> Permissions);

public sealed record LoadArrPermissionCatalogItemResponse(
    string ProductKey,
    string PermissionKey,
    string Label,
    string? Description,
    string Scope,
    string Sensitivity,
    string Status);
