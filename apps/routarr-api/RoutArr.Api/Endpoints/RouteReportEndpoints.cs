using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class RouteReportEndpoints
{
    public static void MapRoutArrRouteReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports/routes")
            .WithTags("RouteReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            RouteReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.routes.summary",
                tenantId,
                actorUserId,
                "route_report",
                summary.Scope,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName("GetRoutArrRouteReportSummary");

        group.MapGet("/summary/export", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            RouteReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.routes.export",
                tenantId,
                actorUserId,
                "route_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportRoutArrRouteReportSummary");

        group.MapGet("/{routeId:guid}", async (
            Guid routeId,
            RoutArrAuthorizationService authorization,
            RouteReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetRouteDetailAsync(tenantId, routeId, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.routes.route.detail",
                tenantId,
                actorUserId,
                "route_report",
                routeId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName("GetRoutArrRouteReportRouteDetail");

        group.MapGet("/stops/{stopId:guid}", async (
            Guid stopId,
            RoutArrAuthorizationService authorization,
            RouteReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetStopDetailAsync(tenantId, stopId, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.routes.stop.detail",
                tenantId,
                actorUserId,
                "route_report",
                stopId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName("GetRoutArrRouteReportStopDetail");
    }
}
