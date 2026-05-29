using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class StaffArrReportEndpoints
{
    public static void MapStaffArrPersonnelReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports/personnel")
            .WithTags("PersonnelReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? employmentStatus,
            StaffArrAuthorizationService authorization,
            PersonnelReportService reportService,
            IStaffArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                employmentStatus,
                cancellationToken);
            await audit.WriteAsync(
                "staffarr.reports.personnel.summary",
                tenantId,
                actorUserId,
                "personnel_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName("GetStaffArrPersonnelReportSummary");

        group.MapGet("/summary/export", async (
            string? employmentStatus,
            StaffArrAuthorizationService authorization,
            PersonnelReportService reportService,
            IStaffArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                employmentStatus,
                cancellationToken);
            await audit.WriteAsync(
                "staffarr.reports.personnel.export",
                tenantId,
                actorUserId,
                "personnel_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportStaffArrPersonnelReportSummary");
    }

    public static void MapStaffArrReadinessReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports/readiness")
            .WithTags("ReadinessReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? scopeType,
            bool? attentionOnly,
            StaffArrAuthorizationService authorization,
            ReadinessReportService reportService,
            IStaffArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                scopeType,
                attentionOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "staffarr.reports.readiness.summary",
                tenantId,
                actorUserId,
                "readiness_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName("GetStaffArrReadinessReportSummary");

        group.MapGet("/summary/export", async (
            string? scopeType,
            bool? attentionOnly,
            StaffArrAuthorizationService authorization,
            ReadinessReportService reportService,
            IStaffArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                scopeType,
                attentionOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "staffarr.reports.readiness.export",
                tenantId,
                actorUserId,
                "readiness_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportStaffArrReadinessReportSummary");
    }

    public static void MapStaffArrIncidentReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports/incidents")
            .WithTags("IncidentReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? status,
            string? severity,
            bool? openOnly,
            StaffArrAuthorizationService authorization,
            IncidentReportService reportService,
            IStaffArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                status,
                severity,
                openOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "staffarr.reports.incidents.summary",
                tenantId,
                actorUserId,
                "incident_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName("GetStaffArrIncidentReportSummary");

        group.MapGet("/summary/export", async (
            string? status,
            string? severity,
            bool? openOnly,
            StaffArrAuthorizationService authorization,
            IncidentReportService reportService,
            IStaffArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                status,
                severity,
                openOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "staffarr.reports.incidents.export",
                tenantId,
                actorUserId,
                "incident_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportStaffArrIncidentReportSummary");
    }

    public static void MapStaffArrEntityExportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/exports")
            .WithTags("EntityExports")
            .RequireAuthorization();

        group.MapGet("/manifest", (
            StaffArrAuthorizationService authorization,
            StaffArrEntityBulkExportService exportService,
            HttpContext context) =>
        {
            authorization.RequirePersonnelReportExport(context.User);
            return Results.Ok(exportService.GetManifest());
        })
        .WithName("GetStaffArrEntityExportManifest");

        group.MapGet("/people", async (
            string? employmentStatus,
            StaffArrAuthorizationService authorization,
            StaffArrEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportPeopleCsvAsync(
                tenantId,
                actorUserId,
                employmentStatus,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportStaffArrPeopleCsv");

        group.MapGet("/personnel-incidents", async (
            string? status,
            StaffArrAuthorizationService authorization,
            StaffArrEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportPersonnelIncidentsCsvAsync(
                tenantId,
                actorUserId,
                status,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportStaffArrPersonnelIncidentsCsv");

        group.MapGet("/person-certifications", async (
            string? status,
            StaffArrAuthorizationService authorization,
            StaffArrEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonnelReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportPersonCertificationsCsvAsync(
                tenantId,
                actorUserId,
                status,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportStaffArrPersonCertificationsCsv");
    }
}
