using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DispatchReportEndpoints
{
    public static void MapRoutArrDispatchReportEndpoints(this WebApplication app)
    {
        MapGroup(app, "/api/reports/dispatch", string.Empty);
        MapGroup(app, "/api/v1/reports/dispatch", "V1");
    }

    private static void MapGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("DispatchReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            DispatchReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.dispatch.summary",
                tenantId,
                actorUserId,
                "dispatch_report",
                summary.Scope,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetRoutArrDispatchReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            DispatchReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.dispatch.export",
                tenantId,
                actorUserId,
                "dispatch_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportRoutArrDispatchReportSummary{routeNameSuffix}");

        group.MapGet("/alerts", async (
            string? scope,
            int? limit,
            RoutArrAuthorizationService authorization,
            DispatchReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var alerts = await reportService.GetAlertsAsync(tenantId, scope, limit, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.dispatch.alerts",
                tenantId,
                actorUserId,
                "dispatch_report",
                scope,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(alerts);
        })
        .WithName($"GetRoutArrDispatchReportAlerts{routeNameSuffix}");

        group.MapGet("/time-summary", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            DispatchReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetTimeSummaryAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.dispatch.time_summary",
                tenantId,
                actorUserId,
                "dispatch_report",
                summary.Scope,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetRoutArrDispatchTimeSummary{routeNameSuffix}");

        group.MapGet("/time-summary/export", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            DispatchReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportTimeSummaryCsvAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.dispatch.time_summary.export",
                tenantId,
                actorUserId,
                "dispatch_report",
                "time_summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportRoutArrDispatchTimeSummary{routeNameSuffix}");

        group.MapGet("/trips/{tripId:guid}", async (
            Guid tripId,
            RoutArrAuthorizationService authorization,
            DispatchReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetTripDetailAsync(tenantId, tripId, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.dispatch.trip.detail",
                tenantId,
                actorUserId,
                "dispatch_report",
                tripId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetRoutArrDispatchReportTripDetail{routeNameSuffix}");

        group.MapGet("/exceptions/{exceptionId:guid}", async (
            Guid exceptionId,
            RoutArrAuthorizationService authorization,
            DispatchReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetExceptionDetailAsync(tenantId, exceptionId, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.dispatch.exception.detail",
                tenantId,
                actorUserId,
                "dispatch_report",
                exceptionId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetRoutArrDispatchReportExceptionDetail{routeNameSuffix}");
    }
}
