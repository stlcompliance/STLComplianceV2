using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class StaffArrReportEndpoints
{
    public static void MapStaffArrPersonnelReportEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/reports/personnel", Suffix: string.Empty),
            (Route: "/api/v1/reports/personnel", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
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
            .WithName($"GetStaffArrPersonnelReportSummary{suffix}");

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
            .WithName($"ExportStaffArrPersonnelReportSummary{suffix}");
        }
    }

    public static void MapStaffArrReadinessReportEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/reports/readiness", Suffix: string.Empty),
            (Route: "/api/v1/reports/readiness", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
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
            .WithName($"GetStaffArrReadinessReportSummary{suffix}");

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
            .WithName($"ExportStaffArrReadinessReportSummary{suffix}");

            group.MapGet("/alerts", async (
                StaffArrAuthorizationService authorization,
                ReadinessReportService reportService,
                IStaffArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireReadinessReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var alerts = await reportService.ListAlertsAsync(tenantId, cancellationToken);
                await audit.WriteAsync(
                    "staffarr.reports.readiness.alerts",
                    tenantId,
                    actorUserId,
                    "readiness_report",
                    null,
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(alerts);
            })
            .WithName($"GetStaffArrReadinessAlerts{suffix}");
        }
    }

    public static void MapStaffArrIncidentReportEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/reports/incidents", Suffix: string.Empty),
            (Route: "/api/v1/reports/incidents", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
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
            .WithName($"GetStaffArrIncidentReportSummary{suffix}");

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
            .WithName($"ExportStaffArrIncidentReportSummary{suffix}");
        }
    }

    public static void MapStaffArrCertificationReportEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/reports/certifications", Suffix: string.Empty),
            (Route: "/api/v1/reports/certifications", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
                .WithTags("CertificationReports")
                .RequireAuthorization();

            group.MapGet("/summary", async (
                bool? missingOnly,
                bool? expiringOnly,
                StaffArrAuthorizationService authorization,
                CertificationReportService reportService,
                IStaffArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireReadinessReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var summary = await reportService.GetSummaryAsync(
                    tenantId,
                    missingOnly ?? false,
                    expiringOnly ?? false,
                    cancellationToken);
                await audit.WriteAsync(
                    "staffarr.reports.certifications.summary",
                    tenantId,
                    actorUserId,
                    "certification_report",
                    null,
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(summary);
            })
            .WithName($"GetStaffArrCertificationReportSummary{suffix}");

            group.MapGet("/summary/export", async (
                bool? missingOnly,
                bool? expiringOnly,
                StaffArrAuthorizationService authorization,
                CertificationReportService reportService,
                IStaffArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireReadinessReportExport(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var export = await reportService.ExportSummaryCsvAsync(
                    tenantId,
                    missingOnly ?? false,
                    expiringOnly ?? false,
                    cancellationToken);
                await audit.WriteAsync(
                    "staffarr.reports.certifications.export",
                    tenantId,
                    actorUserId,
                    "certification_report",
                    "summary",
                    "success",
                    cancellationToken: cancellationToken);
                return Results.File(export.Content, export.ContentType, export.FileName);
            })
            .WithName($"ExportStaffArrCertificationReportSummary{suffix}");
        }
    }

    public static void MapStaffArrEntityExportEndpoints(this WebApplication app)
    {
        MapEntityExportRoutes(app.MapGroup("/api/exports"), string.Empty, "/api/exports", "/api/reports");
        MapEntityExportRoutes(app.MapGroup("/api/v1/exports"), "V1", "/api/v1/exports", "/api/v1/reports");
    }

    private static void MapEntityExportRoutes(
        RouteGroupBuilder group,
        string suffix,
        string exportBasePath,
        string reportBasePath)
    {
        group.WithTags("EntityExports").RequireAuthorization();
        group.MapGet("/manifest", (
            StaffArrAuthorizationService authorization,
            StaffArrEntityBulkExportService exportService,
            HttpContext context) =>
        {
            authorization.RequirePersonnelReportExport(context.User);
            return Results.Ok(exportService.GetManifest(exportBasePath, reportBasePath));
        })
        .WithName($"GetStaffArrEntityExportManifest{suffix}");

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
        .WithName($"ExportStaffArrPeopleCsv{suffix}");

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
        .WithName($"ExportStaffArrPersonnelIncidentsCsv{suffix}");

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
        .WithName($"ExportStaffArrPersonCertificationsCsv{suffix}");
    }
}
