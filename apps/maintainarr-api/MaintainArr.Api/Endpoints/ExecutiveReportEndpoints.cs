using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class ExecutiveReportEndpoints
{
    public static void MapMaintainArrExecutiveReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports/executive")
            .WithTags("ExecutiveReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ExecutiveReportService reportService,
            MaintainArrAuthorizationService authorization,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireExecutiveReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.executive.summary",
                tenantId,
                actorUserId,
                "executive_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName("GetMaintainArrExecutiveReportSummary");

        group.MapGet("/summary/export", async (
            ExecutiveReportService reportService,
            MaintainArrAuthorizationService authorization,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireExecutiveReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var (contentType, fileName, content) = await reportService.ExportSummaryCsvAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.executive.export",
                tenantId,
                actorUserId,
                "executive_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(content, contentType, fileName);
        })
        .WithName("ExportMaintainArrExecutiveReportSummary");
    }
}
