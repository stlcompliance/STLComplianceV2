using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class PeopleExportEndpoints
{
    public static void MapStaffArrPeopleExportEndpoints(this WebApplication app)
    {
        var exports = app.MapGroup("/api/people/export")
            .WithTags("People")
            .RequireAuthorization();

        exports.MapGet("/manifest", (
            StaffArrAuthorizationService authorization,
            PeopleExportService service,
            HttpContext context) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(service.GetManifest());
        })
        .WithName("GetStaffArrPeopleExportManifest");

        exports.MapGet("/", async (
            string? format,
            string? employmentStatus,
            Guid? orgUnitId,
            StaffArrAuthorizationService authorization,
            PeopleExportService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();

            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Ok(await service.BuildExportAsync(
                    tenantId,
                    actorUserId,
                    employmentStatus,
                    orgUnitId,
                    cancellationToken));
            }

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                var csv = await service.ExportCsvAsync(
                    tenantId,
                    actorUserId,
                    employmentStatus,
                    orgUnitId,
                    cancellationToken);
                return Results.Text(csv, "text/csv");
            }

            var zipBytes = await service.ExportZipAsync(
                tenantId,
                actorUserId,
                employmentStatus,
                orgUnitId,
                cancellationToken);
            return Results.File(
                zipBytes,
                "application/zip",
                $"staffarr-people-export-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        })
        .WithName("ExportStaffArrPeople");
    }
}
