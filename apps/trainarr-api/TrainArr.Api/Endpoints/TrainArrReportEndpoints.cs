using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainArrReportEndpoints
{
    public static void MapTrainArrReportIndexEndpoints(this WebApplication app)
    {
        var routes = new[] { "/api/reports", "/api/v1/reports" };
        foreach (var route in routes)
        {
            var group = app.MapGroup(route)
                .WithTags("Reports")
                .RequireAuthorization();

            group.MapGet("/", (
                TrainArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireAssignmentReportRead(context.User);
                var items = new[]
                {
                    new { key = "assignments", path = $"{route}/assignments", description = "Training assignment summaries and exports." },
                    new { key = "qualifications", path = $"{route}/qualifications", description = "Qualification issue and status summaries." },
                    new { key = "compliance", path = $"{route}/compliance", description = "Compliance coverage and remediation summaries." },
                };
                return Results.Ok(new { items });
            })
            .WithName(route.Contains("/v1/") ? "GetTrainArrReportsIndexV1" : "GetTrainArrReportsIndex");
        }
    }

    public static void MapTrainArrAssignmentReportEndpoints(this WebApplication app)
    {
        var routes = new[]
        {
            (Route: "/api/reports/assignments", Suffix: string.Empty),
            (Route: "/api/v1/reports/assignments", Suffix: "V1"),
        };
        foreach (var (route, suffix) in routes)
        {
            var group = app.MapGroup(route)
                .WithTags("AssignmentReports")
                .RequireAuthorization();

            group.MapGet("/summary", async (
            string? status,
            bool? overdueOnly,
            TrainArrAuthorizationService authorization,
            AssignmentReportService reportService,
            ITrainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                status,
                overdueOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "trainarr.reports.assignments.summary",
                tenantId,
                actorUserId,
                "assignment_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetTrainArrAssignmentReportSummary{suffix}");

            group.MapGet("/summary/export", async (
            string? status,
            bool? overdueOnly,
            TrainArrAuthorizationService authorization,
            AssignmentReportService reportService,
            ITrainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                status,
                overdueOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "trainarr.reports.assignments.export",
                tenantId,
                actorUserId,
                "assignment_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportTrainArrAssignmentReportSummary{suffix}");
        }
    }

    public static void MapTrainArrQualificationReportEndpoints(this WebApplication app)
    {
        var routes = new[]
        {
            (Route: "/api/reports/qualifications", Suffix: string.Empty),
            (Route: "/api/v1/reports/qualifications", Suffix: "V1"),
        };
        foreach (var (route, suffix) in routes)
        {
            var group = app.MapGroup(route)
                .WithTags("QualificationReports")
                .RequireAuthorization();

            group.MapGet("/summary", async (
            string? status,
            TrainArrAuthorizationService authorization,
            QualificationReportService reportService,
            ITrainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, status, cancellationToken);
            await audit.WriteAsync(
                "trainarr.reports.qualifications.summary",
                tenantId,
                actorUserId,
                "qualification_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetTrainArrQualificationReportSummary{suffix}");

            group.MapGet("/summary/export", async (
            string? status,
            TrainArrAuthorizationService authorization,
            QualificationReportService reportService,
            ITrainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, status, cancellationToken);
            await audit.WriteAsync(
                "trainarr.reports.qualifications.export",
                tenantId,
                actorUserId,
                "qualification_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportTrainArrQualificationReportSummary{suffix}");
        }
    }

    public static void MapTrainArrComplianceReportEndpoints(this WebApplication app)
    {
        var routes = new[]
        {
            (Route: "/api/reports/compliance", Suffix: string.Empty),
            (Route: "/api/v1/reports/compliance", Suffix: "V1"),
        };
        foreach (var (route, suffix) in routes)
        {
            var group = app.MapGroup(route)
                .WithTags("ComplianceReports")
                .RequireAuthorization();

            group.MapGet("/summary", async (
            bool? attentionOnly,
            TrainArrAuthorizationService authorization,
            ComplianceReportService reportService,
            ITrainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                attentionOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "trainarr.reports.compliance.summary",
                tenantId,
                actorUserId,
                "compliance_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetTrainArrComplianceReportSummary{suffix}");

            group.MapGet("/summary/export", async (
            bool? attentionOnly,
            TrainArrAuthorizationService authorization,
            ComplianceReportService reportService,
            ITrainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                attentionOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "trainarr.reports.compliance.export",
                tenantId,
                actorUserId,
                "compliance_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportTrainArrComplianceReportSummary{suffix}");
        }
    }

    public static void MapTrainArrEntityExportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/exports")
            .WithTags("EntityExports")
            .RequireAuthorization();

        group.MapGet("/manifest", (
            TrainArrAuthorizationService authorization,
            TrainArrEntityBulkExportService exportService,
            HttpContext context) =>
        {
            authorization.RequireAssignmentReportExport(context.User);
            return Results.Ok(exportService.GetManifest());
        })
        .WithName("GetTrainArrEntityExportManifest");

        group.MapGet("/training-assignments", async (
            string? status,
            TrainArrAuthorizationService authorization,
            TrainArrEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportTrainingAssignmentsCsvAsync(
                tenantId,
                actorUserId,
                status,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportTrainArrTrainingAssignmentsCsv");

        group.MapGet("/qualification-issues", async (
            string? status,
            TrainArrAuthorizationService authorization,
            TrainArrEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportQualificationIssuesCsvAsync(
                tenantId,
                actorUserId,
                status,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportTrainArrQualificationIssuesCsv");

        group.MapGet("/training-definitions", async (
            TrainArrAuthorizationService authorization,
            TrainArrEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportTrainingDefinitionsCsvAsync(
                tenantId,
                actorUserId,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportTrainArrTrainingDefinitionsCsv");
    }
}
