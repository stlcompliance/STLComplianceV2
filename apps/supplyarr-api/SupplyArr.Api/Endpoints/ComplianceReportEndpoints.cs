using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ComplianceReportEndpoints
{
    public static void MapSupplyArrComplianceReportEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("ComplianceReports").RequireAuthorization();

            group.MapGet("/summary", async (
                bool? attentionOnly,
                string? partyType,
                Guid? externalPartyId,
                string? reviewStatus,
                SupplyArrAuthorizationService authorization,
                ComplianceReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireComplianceReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var summary = await reportService.GetSummaryAsync(
                    tenantId,
                    attentionOnly,
                    partyType,
                    externalPartyId,
                    reviewStatus,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.compliance.summary",
                    tenantId,
                    actorUserId,
                    "compliance_report",
                    null,
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(summary);
            })
            .WithName($"GetSupplyArrComplianceReportSummary{nameSuffix}");

            group.MapGet("/summary/export", async (
                bool? attentionOnly,
                string? partyType,
                Guid? externalPartyId,
                string? reviewStatus,
                SupplyArrAuthorizationService authorization,
                ComplianceReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireComplianceReportExport(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var export = await reportService.ExportSummaryCsvAsync(
                    tenantId,
                    attentionOnly,
                    partyType,
                    externalPartyId,
                    reviewStatus,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.compliance.export",
                    tenantId,
                    actorUserId,
                    "compliance_report",
                    "summary",
                    "success",
                    cancellationToken: cancellationToken);
                return Results.File(export.Content, export.ContentType, export.FileName);
            })
            .WithName($"ExportSupplyArrComplianceReportSummary{nameSuffix}");

            group.MapGet("/parties/{externalPartyId:guid}", async (
                Guid externalPartyId,
                SupplyArrAuthorizationService authorization,
                ComplianceReportService reportService,
                ISupplyArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireComplianceReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await reportService.GetPartyDetailAsync(
                    tenantId,
                    externalPartyId,
                    cancellationToken);
                await audit.WriteAsync(
                    "supplyarr.reports.compliance.party_detail",
                    tenantId,
                    actorUserId,
                    "compliance_report",
                    externalPartyId.ToString(),
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(detail);
            })
            .WithName($"GetSupplyArrCompliancePartyDetail{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/reports/compliance"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/reports/compliance"), "V1");
    }
}
