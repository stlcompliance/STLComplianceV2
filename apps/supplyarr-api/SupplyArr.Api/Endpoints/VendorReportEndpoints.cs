using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class VendorReportEndpoints
{
    public static void MapSupplyArrVendorReportEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("VendorReports").RequireAuthorization();

            group.MapGet("/summary", async (
                string? approvalStatus,
                bool? activeOnly,
                SupplyArrAuthorizationService authorization,
                VendorReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var summary = await reportService.GetSummaryAsync(
                    tenantId,
                    approvalStatus,
                    activeOnly,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.vendor.summary",
                    tenantId,
                    actorUserId,
                    "vendor_report",
                    null,
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(summary);
            })
            .WithName($"GetSupplyArrVendorReportSummary{nameSuffix}");

            group.MapGet("/{vendorPartyId:guid}", async (
                Guid vendorPartyId,
                SupplyArrAuthorizationService authorization,
                VendorReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await reportService.GetDetailAsync(tenantId, vendorPartyId, cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.vendor.detail",
                    tenantId,
                    actorUserId,
                    "vendor_report",
                    vendorPartyId.ToString(),
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(detail);
            })
            .WithName($"GetSupplyArrVendorReportDetail{nameSuffix}");

            group.MapGet("/summary/export", async (
                string? approvalStatus,
                bool? activeOnly,
                SupplyArrAuthorizationService authorization,
                VendorReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorReportExport(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var export = await reportService.ExportSummaryCsvAsync(
                    tenantId,
                    approvalStatus,
                    activeOnly,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.vendor.export",
                    tenantId,
                    actorUserId,
                    "vendor_report",
                    "summary",
                    "success",
                    cancellationToken: cancellationToken);
                return Results.File(export.Content, export.ContentType, export.FileName);
            })
            .WithName($"ExportSupplyArrVendorReportSummary{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/reports/vendors"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/reports/vendors"), "V1");
    }
}
