using StaffArr.Api.Contracts;
using StaffArr.Api.Services;

namespace StaffArr.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapStaffArrSettingsEndpoints(this WebApplication app)
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
                StaffArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireWorkerAdminSettingsManage(context.User);

                var response = new StaffArrSettingsManifestResponse(
                [
                    new("certification_expiration_settings", "/api/worker-admin/certification-expiration/settings", "Certification expiration worker cadence and controls."),
                    new("readiness_rollup_settings", "/api/worker-admin/readiness-rollup/settings", "Readiness rollup worker settings."),
                    new("permission_projection_settings", "/api/worker-admin/permission-projection/settings", "Permission projection worker settings."),
                    new("personnel_history_rollup_settings", "/api/worker-admin/personnel-history-rollup/settings", "Personnel history rollup worker settings."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
