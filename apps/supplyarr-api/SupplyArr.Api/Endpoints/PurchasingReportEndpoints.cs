using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PurchasingReportEndpoints
{
    public static void MapSupplyArrPurchasingReportEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("PurchasingReports").RequireAuthorization();

            group.MapGet("/summary", async (
                bool? openDocumentsOnly,
                Guid? supplierId,
                Guid? vendorPartyId,
                SupplyArrAuthorizationService authorization,
                PurchasingReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePurchasingReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var summary = await reportService.GetSummaryAsync(
                    tenantId,
                    openDocumentsOnly,
                    supplierId ?? vendorPartyId,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.purchasing.summary",
                    tenantId,
                    actorUserId,
                    "purchasing_report",
                    null,
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(summary);
            })
            .WithName($"GetSupplyArrPurchasingReportSummary{nameSuffix}");

            group.MapGet("/summary/export", async (
                bool? openDocumentsOnly,
                Guid? supplierId,
                Guid? vendorPartyId,
                SupplyArrAuthorizationService authorization,
                PurchasingReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePurchasingReportExport(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var export = await reportService.ExportSummaryCsvAsync(
                    tenantId,
                    openDocumentsOnly,
                    supplierId ?? vendorPartyId,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.purchasing.export",
                    tenantId,
                    actorUserId,
                    "purchasing_report",
                    "summary",
                    "success",
                    cancellationToken: cancellationToken);
                return Results.File(export.Content, export.ContentType, export.FileName);
            })
            .WithName($"ExportSupplyArrPurchasingReportSummary{nameSuffix}");

            group.MapGet("/purchase-requests/{purchaseRequestId:guid}", async (
                Guid purchaseRequestId,
                SupplyArrAuthorizationService authorization,
                PurchasingReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePurchasingReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await reportService.GetPurchaseRequestDetailAsync(
                    tenantId,
                    purchaseRequestId,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.purchasing.purchase_request_detail",
                    tenantId,
                    actorUserId,
                    "purchasing_report",
                    purchaseRequestId.ToString(),
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(detail);
            })
            .WithName($"GetSupplyArrPurchasingPurchaseRequestDetail{nameSuffix}");

            group.MapGet("/purchase-orders/{purchaseOrderId:guid}", async (
                Guid purchaseOrderId,
                SupplyArrAuthorizationService authorization,
                PurchasingReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePurchasingReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await reportService.GetPurchaseOrderDetailAsync(
                    tenantId,
                    purchaseOrderId,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.purchasing.purchase_order_detail",
                    tenantId,
                    actorUserId,
                    "purchasing_report",
                    purchaseOrderId.ToString(),
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(detail);
            })
            .WithName($"GetSupplyArrPurchasingPurchaseOrderDetail{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/reports/purchasing"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/reports/purchasing"), "V1");
    }
}
