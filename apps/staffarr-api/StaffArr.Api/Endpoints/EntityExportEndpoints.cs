using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class EntityExportEndpoints
{
    public static void MapStaffArrEntityExportEndpoints(this WebApplication app)
    {
        MapEntityExportRoutes(app.MapGroup("/api/exports"), string.Empty, "/api/exports");
        MapEntityExportRoutes(app.MapGroup("/api/v1/exports"), "V1", "/api/v1/exports");
    }

    private static void MapEntityExportRoutes(
        RouteGroupBuilder group,
        string suffix,
        string exportBasePath)
    {
        group.WithTags("EntityExports").RequireAuthorization();
        group.MapGet("/manifest", (
            StaffArrAuthorizationService authorization,
            StaffArrEntityBulkExportService exportService,
            HttpContext context) =>
        {
            authorization.RequireEntityExport(context.User);
            return Results.Ok(exportService.GetManifest(exportBasePath));
        })
        .WithName($"GetStaffArrEntityExportManifest{suffix}");

        group.MapGet("/people", async (
            string? employmentStatus,
            StaffArrAuthorizationService authorization,
            StaffArrEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEntityExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportPeopleCsvAsync(
                tenantId,
                actorUserId,
                employmentStatus,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportStaffArrPeopleCsv{suffix}");

        group.MapGet("/personnel-incidents", async (
            string? status,
            StaffArrAuthorizationService authorization,
            StaffArrEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEntityExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportPersonnelIncidentsCsvAsync(
                tenantId,
                actorUserId,
                status,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportStaffArrPersonnelIncidentsCsv{suffix}");

        group.MapGet("/person-certifications", async (
            string? status,
            StaffArrAuthorizationService authorization,
            StaffArrEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEntityExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportPersonCertificationsCsvAsync(
                tenantId,
                actorUserId,
                status,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportStaffArrPersonCertificationsCsv{suffix}");
    }
}
