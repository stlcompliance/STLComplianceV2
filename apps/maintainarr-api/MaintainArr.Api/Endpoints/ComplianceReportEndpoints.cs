using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class ComplianceReportEndpoints
{
    public static void MapMaintainArrComplianceReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports/compliance")
            .WithTags("ComplianceReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            bool? attentionOnly,
            string? siteRef,
            ComplianceReportService reportService,
            MaintainArrAuthorizationService authorization,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                attentionOnly,
                siteRef,
                cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.compliance.summary",
                tenantId,
                actorUserId,
                "compliance_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName("GetMaintainArrComplianceReportSummary");

        group.MapGet("/inspection-templates/{inspectionTemplateId:guid}", async (
            Guid inspectionTemplateId,
            ComplianceReportService reportService,
            MaintainArrAuthorizationService authorization,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetTemplateDetailAsync(
                tenantId,
                inspectionTemplateId,
                cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.compliance.template.detail",
                tenantId,
                actorUserId,
                "compliance_report",
                inspectionTemplateId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName("GetMaintainArrComplianceReportTemplateDetail");

        group.MapGet("/summary/export", async (
            bool? attentionOnly,
            string? siteRef,
            ComplianceReportService reportService,
            MaintainArrAuthorizationService authorization,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var (contentType, fileName, content) = await reportService.ExportSummaryCsvAsync(
                tenantId,
                attentionOnly,
                siteRef,
                cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.compliance.export",
                tenantId,
                actorUserId,
                "compliance_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(content, contentType, fileName);
        })
        .WithName("ExportMaintainArrComplianceReportSummary");
    }
}
