using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class EntityExportEndpoints
{
    public static void MapMaintainArrEntityExportEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Group: app.MapGroup("/api/exports"), ExportBasePath: "/api/exports", ReportBasePath: "/api/reports"),
            (Group: app.MapGroup("/api/v1/exports"), ExportBasePath: "/api/v1/exports", ReportBasePath: "/api/v1/reports"),
        };

        foreach (var (exports, exportBasePath, reportBasePath) in groups)
        {
            exports.WithTags("Exports").RequireAuthorization();

            exports.MapGet("/manifest", (
                MaintainArrAuthorizationService authorization,
                EntityBulkExportService service,
                HttpContext context) =>
            {
                authorization.RequireEntityExport(context.User);
                return Results.Ok(service.GetManifest(exportBasePath, reportBasePath));
            });

            exports.MapGet("/assets", async (
                string? lifecycleStatus,
                MaintainArrAuthorizationService authorization,
                EntityBulkExportService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireEntityExport(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var export = await service.ExportAssetsCsvAsync(
                    tenantId,
                    actorUserId,
                    lifecycleStatus,
                    cancellationToken);
                return Results.File(export.Content, export.ContentType, export.FileName);
            });

            exports.MapGet("/work-orders", async (
                string? status,
                Guid? assetId,
                MaintainArrAuthorizationService authorization,
                EntityBulkExportService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireEntityExport(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var export = await service.ExportWorkOrdersCsvAsync(
                    tenantId,
                    actorUserId,
                    status,
                    assetId,
                    cancellationToken);
                return Results.File(export.Content, export.ContentType, export.FileName);
            });

            exports.MapGet("/inspection-runs", async (
                string? status,
                Guid? assetId,
                MaintainArrAuthorizationService authorization,
                EntityBulkExportService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireEntityExport(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var export = await service.ExportInspectionRunsCsvAsync(
                    tenantId,
                    actorUserId,
                    status,
                    assetId,
                    cancellationToken);
                return Results.File(export.Content, export.ContentType, export.FileName);
            });
        }
    }
}
