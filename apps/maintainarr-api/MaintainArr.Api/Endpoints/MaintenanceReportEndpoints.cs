using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MaintenanceReportEndpoints
{
    public static void MapMaintainArrMaintenanceReportEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("MaintenanceReports").RequireAuthorization();

            group.MapGet("/summary", async (
                string? lifecycleStatus,
                MaintainArrAuthorizationService authorization,
                MaintenanceReportService reportService,
                IMaintainArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireMaintenanceReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var summary = await reportService.GetSummaryAsync(tenantId, lifecycleStatus, cancellationToken);
                await audit.WriteAsync(
                    "maintainarr.reports.maintenance.summary",
                    tenantId,
                    actorUserId,
                    "maintenance_report",
                    null,
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(summary);
            })
            .WithName($"GetMaintainArrMaintenanceReportSummary{nameSuffix}");

        group.MapGet("/summary/export", async (
            string? lifecycleStatus,
            MaintainArrAuthorizationService authorization,
            MaintenanceReportService reportService,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintenanceReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, lifecycleStatus, cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.maintenance.export",
                tenantId,
                actorUserId,
                "maintenance_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportMaintainArrMaintenanceReportSummary{nameSuffix}");

        group.MapGet("/assets/{assetId:guid}", async (
            Guid assetId,
            MaintainArrAuthorizationService authorization,
            MaintenanceReportService reportService,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintenanceReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetAssetDetailAsync(tenantId, assetId, cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.maintenance.asset.detail",
                tenantId,
                actorUserId,
                "maintenance_report",
                assetId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetMaintainArrMaintenanceReportAssetDetail{nameSuffix}");

        group.MapGet("/work-orders/{workOrderId:guid}", async (
            Guid workOrderId,
            MaintainArrAuthorizationService authorization,
            MaintenanceReportService reportService,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintenanceReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetWorkOrderDetailAsync(tenantId, workOrderId, cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.maintenance.work_order.detail",
                tenantId,
                actorUserId,
                "maintenance_report",
                workOrderId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetMaintainArrMaintenanceReportWorkOrderDetail{nameSuffix}");

        group.MapGet("/defects/{defectId:guid}", async (
            Guid defectId,
            MaintainArrAuthorizationService authorization,
            MaintenanceReportService reportService,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintenanceReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetDefectDetailAsync(tenantId, defectId, cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.maintenance.defect.detail",
                tenantId,
                actorUserId,
                "maintenance_report",
                defectId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetMaintainArrMaintenanceReportDefectDetail{nameSuffix}");

        group.MapGet("/inspection-runs/{inspectionRunId:guid}", async (
            Guid inspectionRunId,
            MaintainArrAuthorizationService authorization,
            MaintenanceReportService reportService,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintenanceReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetInspectionRunDetailAsync(tenantId, inspectionRunId, cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.maintenance.inspection_run.detail",
                tenantId,
                actorUserId,
                "maintenance_report",
                inspectionRunId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetMaintainArrMaintenanceReportInspectionRunDetail{nameSuffix}");

        group.MapGet("/pm-schedules/{pmScheduleId:guid}", async (
            Guid pmScheduleId,
            MaintainArrAuthorizationService authorization,
            MaintenanceReportService reportService,
            IMaintainArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintenanceReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetPmScheduleDetailAsync(tenantId, pmScheduleId, cancellationToken);
            await audit.WriteAsync(
                "maintainarr.reports.maintenance.pm_schedule.detail",
                tenantId,
                actorUserId,
                "maintenance_report",
                pmScheduleId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetMaintainArrMaintenanceReportPmScheduleDetail{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/reports/maintenance"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/reports/maintenance"), "V1");
    }
}
