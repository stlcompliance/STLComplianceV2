using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class ComplianceCoreReportEndpoints
{
    public static void MapComplianceCoreFindingsReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports/findings")
            .WithTags("FindingsReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? status,
            string? severity,
            bool? openOnly,
            ComplianceCoreAuthorizationService authorization,
            FindingsReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                status,
                severity,
                openOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.findings.summary",
                tenantId,
                actorUserId,
                "findings_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName("GetComplianceCoreFindingsReportSummary");

        group.MapGet("/summary/export", async (
            string? status,
            string? severity,
            bool? openOnly,
            ComplianceCoreAuthorizationService authorization,
            FindingsReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                status,
                severity,
                openOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.findings.export",
                tenantId,
                actorUserId,
                "findings_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportComplianceCoreFindingsReportSummary");
    }

    public static void MapComplianceCoreOperatorReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports/operator")
            .WithTags("OperatorReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            bool? attentionOnly,
            ComplianceCoreAuthorizationService authorization,
            OperatorReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOperatorReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                attentionOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.operator.summary",
                tenantId,
                actorUserId,
                "operator_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName("GetComplianceCoreOperatorReportSummary");

        group.MapGet("/summary/export", async (
            bool? attentionOnly,
            ComplianceCoreAuthorizationService authorization,
            OperatorReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOperatorReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                attentionOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.operator.export",
                tenantId,
                actorUserId,
                "operator_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportComplianceCoreOperatorReportSummary");
    }

    public static void MapComplianceCoreEntityExportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/exports")
            .WithTags("EntityExports")
            .RequireAuthorization();

        group.MapGet("/manifest", (
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreEntityBulkExportService exportService,
            HttpContext context) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            return Results.Ok(exportService.GetManifest());
        })
        .WithName("GetComplianceCoreEntityExportManifest");

        group.MapGet("/findings", async (
            string? status,
            bool? openOnly,
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportFindingsCsvAsync(
                tenantId,
                actorUserId,
                status,
                openOnly,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportComplianceCoreFindingsCsv");

        group.MapGet("/evaluations", async (
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportEvaluationsCsvAsync(
                tenantId,
                actorUserId,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportComplianceCoreEvaluationsCsv");

        group.MapGet("/rule-packs", async (
            string? status,
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportRulePacksCsvAsync(
                tenantId,
                actorUserId,
                status,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportComplianceCoreRulePacksCsv");
    }
}
