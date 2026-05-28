using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PartsInventoryReportEndpoints
{
    public static void MapSupplyArrPartsInventoryReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports/parts-inventory")
            .WithTags("PartsInventoryReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? partStatus,
            bool? activePartsOnly,
            bool? belowReorderOnly,
            Guid? inventoryLocationId,
            SupplyArrAuthorizationService authorization,
            PartsInventoryReportService reportService,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsInventoryReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                partStatus,
                activePartsOnly,
                belowReorderOnly,
                inventoryLocationId,
                cancellationToken);
            await audit.WriteAsync(
                "supplyarr.reports.parts_inventory.summary",
                tenantId,
                actorUserId,
                "parts_inventory_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName("GetSupplyArrPartsInventoryReportSummary");

        group.MapGet("/summary/export", async (
            string? partStatus,
            bool? activePartsOnly,
            bool? belowReorderOnly,
            Guid? inventoryLocationId,
            SupplyArrAuthorizationService authorization,
            PartsInventoryReportService reportService,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsInventoryReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportPartsSummaryCsvAsync(
                tenantId,
                partStatus,
                activePartsOnly,
                belowReorderOnly,
                inventoryLocationId,
                cancellationToken);
            await audit.WriteAsync(
                "supplyarr.reports.parts_inventory.export",
                tenantId,
                actorUserId,
                "parts_inventory_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportSupplyArrPartsInventoryReportSummary");

        group.MapGet("/parts/{partId:guid}", async (
            Guid partId,
            SupplyArrAuthorizationService authorization,
            PartsInventoryReportService reportService,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsInventoryReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetPartDetailAsync(tenantId, partId, cancellationToken);
            await audit.WriteAsync(
                "supplyarr.reports.parts_inventory.part_detail",
                tenantId,
                actorUserId,
                "parts_inventory_report",
                partId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName("GetSupplyArrPartsInventoryPartDetail");

        group.MapGet("/locations/{inventoryLocationId:guid}", async (
            Guid inventoryLocationId,
            SupplyArrAuthorizationService authorization,
            PartsInventoryReportService reportService,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsInventoryReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetLocationDetailAsync(
                tenantId,
                inventoryLocationId,
                cancellationToken);
            await audit.WriteAsync(
                "supplyarr.reports.parts_inventory.location_detail",
                tenantId,
                actorUserId,
                "parts_inventory_report",
                inventoryLocationId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName("GetSupplyArrPartsInventoryLocationDetail");
    }
}
