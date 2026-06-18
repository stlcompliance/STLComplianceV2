using TrainArr.Api.Contracts;
using TrainArr.Api.Services;

namespace TrainArr.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapTrainArrSettingsEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/settings"),
            app.MapGroup("/api/v1/settings"),
            app.MapGroup("/api/config"),
            app.MapGroup("/api/v1/config"),
        };

        foreach (var group in groups)
        {
            group.WithTags("Settings").RequireAuthorization();

            group.MapGet("/", (
                TrainArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireTenantSettingsRead(context.User);

                var response = new TrainArrSettingsManifestResponse(
                [
                    new("trainarr_tenant_settings", "/api/v1/tenant-settings/trainarr", "Canonical tenant-scoped TrainArr posture and default behavior."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
