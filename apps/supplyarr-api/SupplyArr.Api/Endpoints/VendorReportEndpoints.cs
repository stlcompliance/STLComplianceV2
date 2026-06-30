using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierReportEndpoints
{
    public static void MapSupplyArrSupplierReportEndpoints(this WebApplication app)
    {
        static void MapSupplierRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("SupplierReports").RequireAuthorization();

            group.MapGet("/summary", async (
                string? approvalStatus,
                bool? activeOnly,
                SupplyArrAuthorizationService authorization,
                SupplierReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var summary = await reportService.GetSupplierSummaryAsync(
                    tenantId,
                    approvalStatus,
                    activeOnly,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.supplier.summary",
                    tenantId,
                    actorUserId,
                    "supplier_report",
                    null,
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(summary);
            })
            .WithName($"GetSupplyArrSupplierReportSummary{nameSuffix}");

            group.MapGet("/{supplierId:guid}", async (
                Guid supplierId,
                SupplyArrAuthorizationService authorization,
                SupplierReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await reportService.GetSupplierDetailAsync(tenantId, supplierId, cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.supplier.detail",
                    tenantId,
                    actorUserId,
                    "supplier_report",
                    supplierId.ToString(),
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(detail);
            })
            .WithName($"GetSupplyArrSupplierReportDetail{nameSuffix}");

            group.MapGet("/summary/export", async (
                string? approvalStatus,
                bool? activeOnly,
                SupplyArrAuthorizationService authorization,
                SupplierReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorReportExport(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var export = await reportService.ExportSupplierSummaryCsvAsync(
                    tenantId,
                    approvalStatus,
                    activeOnly,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.supplier.export",
                    tenantId,
                    actorUserId,
                    "supplier_report",
                    "summary",
                    "success",
                    cancellationToken: cancellationToken);
                return Results.File(export.Content, export.ContentType, export.FileName);
            })
            .WithName($"ExportSupplyArrSupplierReportSummary{nameSuffix}");
        }

        static void MapVendorAliasRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("VendorReports").RequireAuthorization();

            group.MapGet("/summary", async (
                string? approvalStatus,
                bool? activeOnly,
                SupplyArrAuthorizationService authorization,
                SupplierReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var summary = await reportService.GetVendorSummaryAsync(
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
                SupplierReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await reportService.GetVendorDetailAsync(tenantId, vendorPartyId, cancellationToken);
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
                SupplierReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorReportExport(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var export = await reportService.ExportVendorSummaryCsvAsync(
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

        MapSupplierRoutes(app.MapGroup("/api/reports/suppliers"), string.Empty);
        MapSupplierRoutes(app.MapGroup("/api/v1/reports/suppliers"), "V1");
        MapVendorAliasRoutes(app.MapGroup("/api/reports/vendors"), string.Empty);
        MapVendorAliasRoutes(app.MapGroup("/api/v1/reports/vendors"), "V1");
    }
}

public static class VendorReportEndpoints
{
    public static void MapSupplyArrVendorReportEndpoints(this WebApplication app)
        => app.MapSupplyArrSupplierReportEndpoints();
}
