using StaffArr.Api.Contracts;
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

        exports.MapGet("/preset", async (
            StaffArrAuthorizationService authorization,
            PersonExportPresetService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var preset = await service.GetAsync(tenantId, cancellationToken);
            return preset is null ? Results.NotFound() : Results.Ok(preset);
        })
        .WithName("GetStaffArrPeopleExportPreset");

        exports.MapPut("/preset", async (
            UpsertPersonExportPresetRequest request,
            StaffArrAuthorizationService authorization,
            PersonExportPresetService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var preset = await service.UpsertAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Ok(preset);
        })
        .WithName("UpsertStaffArrPeopleExportPreset");

        exports.MapGet("/schedule", async (
            StaffArrAuthorizationService authorization,
            PersonExportScheduleService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetStaffArrPeopleExportSchedule");

        exports.MapPut("/schedule", async (
            UpsertPersonExportScheduleRequest request,
            StaffArrAuthorizationService authorization,
            PersonExportScheduleService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var schedule = await service.UpsertAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Ok(schedule);
        })
        .WithName("UpsertStaffArrPeopleExportSchedule");

        exports.MapGet("/delivery-notifications", async (
            int? limit,
            StaffArrAuthorizationService authorization,
            PersonExportDeliveryNotificationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListRecentAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListStaffArrPeopleExportDeliveryNotifications");
    }
}
