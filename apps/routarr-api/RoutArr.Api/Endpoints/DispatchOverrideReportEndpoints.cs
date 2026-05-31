using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DispatchOverrideReportEndpoints
{
    public static void MapRoutArrDispatchOverrideReportEndpoints(this WebApplication app)
    {
        MapGroup(app, "/api/reports/dispatch-overrides", string.Empty);
        MapGroup(app, "/api/v1/reports/dispatch-overrides", "V1");
    }

    private static void MapGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("DispatchOverrideReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            DispatchOverrideReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.dispatch_overrides.summary",
                tenantId,
                actorUserId,
                "dispatch_override_report",
                summary.Scope,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetRoutArrDispatchOverrideReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            DispatchOverrideReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.dispatch_overrides.export",
                tenantId,
                actorUserId,
                "dispatch_override_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportRoutArrDispatchOverrideReportSummary{routeNameSuffix}");
    }
}
