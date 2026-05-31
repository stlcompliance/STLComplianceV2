using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class EntityExportEndpoints
{
    public static void MapRoutArrEntityExportEndpoints(this WebApplication app)
    {
        MapEntityExportRoutes(app.MapGroup("/api/exports"), string.Empty, "/api/exports", "/api/reports");
        MapEntityExportRoutes(app.MapGroup("/api/v1/exports"), "V1", "/api/v1/exports", "/api/v1/reports");
    }

    private static void MapEntityExportRoutes(
        RouteGroupBuilder exports,
        string suffix,
        string exportBasePath,
        string reportBasePath)
    {
        exports.WithTags("Exports").RequireAuthorization();
        exports.MapGet("/manifest", (
            RoutArrAuthorizationService authorization,
            RoutArrEntityBulkExportService service,
            HttpContext context) =>
        {
            authorization.RequireEntityExport(context.User);
            return Results.Ok(service.GetManifest(exportBasePath, reportBasePath));
        })
        .WithName($"GetRoutArrEntityExportManifest{suffix}");

        exports.MapGet("/trips", async (
            string? dispatchStatus,
            RoutArrAuthorizationService authorization,
            RoutArrEntityBulkExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEntityExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await service.ExportTripsCsvAsync(
                tenantId,
                actorUserId,
                dispatchStatus,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportRoutArrTripsCsv{suffix}");

        exports.MapGet("/routes", async (
            string? routeStatus,
            RoutArrAuthorizationService authorization,
            RoutArrEntityBulkExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEntityExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await service.ExportRoutesCsvAsync(
                tenantId,
                actorUserId,
                routeStatus,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportRoutArrRoutesCsv{suffix}");

        exports.MapGet("/dispatch-exceptions", async (
            string? status,
            RoutArrAuthorizationService authorization,
            RoutArrEntityBulkExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEntityExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await service.ExportDispatchExceptionsCsvAsync(
                tenantId,
                actorUserId,
                status,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportRoutArrDispatchExceptionsCsv{suffix}");
    }
}
