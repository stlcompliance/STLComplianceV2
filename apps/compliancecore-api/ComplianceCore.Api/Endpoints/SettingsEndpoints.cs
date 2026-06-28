using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;

namespace ComplianceCore.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapComplianceCoreSettingsEndpoints(this WebApplication app)
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
                ComplianceCoreAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireFactSourceSyncWorkerSettingsManage(context.User);

                var response = new ComplianceCoreSettingsManifestResponse(
                [
                    new("fact_source_sync_worker_settings", "/api/v1/fact-source-sync-worker-settings", "Fact source sync worker cadence and controls."),
                    new("m12_analytics_worker_settings", "/api/v1/m12-analytics-worker-settings", "Compliance analytics worker settings."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
