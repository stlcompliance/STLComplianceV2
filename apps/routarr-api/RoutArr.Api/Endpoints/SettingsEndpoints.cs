using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace RoutArr.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapRoutArrSettingsEndpoints(this WebApplication app)
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
                RoutArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireNotificationSettingsManage(context.User);

                var response = new RoutArrSettingsManifestResponse(
                [
                    new("routarr_tenant_settings", "/api/v1/tenant-settings/editable", "First-class RoutArr tenant settings across dispatch, demand, planning, tendering, visibility, exceptions, documents, integrations, overrides, and closeout."),
                    new("routarr_tenant_setting_options", "/api/v1/tenant-settings/options", "Allowed values and metadata for the RoutArr tenant settings UI."),
                    new("routarr_tenant_setting_audit", "/api/v1/tenant-settings/audit", "Audit history for material RoutArr tenant setting changes."),
                    new("routarr_tenant_setting_overrides", "/api/v1/tenant-settings/overrides", "Scoped RoutArr tenant setting overrides with typed references and audit."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
