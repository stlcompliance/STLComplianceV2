using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class ProofDvirReportEndpoints
{
    public static void MapRoutArrProofDvirReportEndpoints(this WebApplication app)
    {
        MapGroup(app, "/api/reports/proof-dvir", string.Empty);
        MapGroup(app, "/api/v1/reports/proof-dvir", "V1");
    }

    private static void MapGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("ProofDvirReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            ProofDvirReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.proof_dvir.summary",
                tenantId,
                actorUserId,
                "proof_dvir_report",
                summary.Scope,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetRoutArrProofDvirReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? scope,
            RoutArrAuthorizationService authorization,
            ProofDvirReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, scope, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.proof_dvir.export",
                tenantId,
                actorUserId,
                "proof_dvir_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportRoutArrProofDvirReportSummary{routeNameSuffix}");

        group.MapGet("/trips/{tripId:guid}", async (
            Guid tripId,
            RoutArrAuthorizationService authorization,
            ProofDvirReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetTripDetailAsync(tenantId, tripId, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.proof_dvir.trip.detail",
                tenantId,
                actorUserId,
                "proof_dvir_report",
                tripId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetRoutArrProofDvirReportTripDetail{routeNameSuffix}");

        group.MapGet("/proofs/{proofId:guid}", async (
            Guid proofId,
            RoutArrAuthorizationService authorization,
            ProofDvirReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetProofDetailAsync(tenantId, proofId, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.proof_dvir.proof.detail",
                tenantId,
                actorUserId,
                "proof_dvir_report",
                proofId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetRoutArrProofDvirReportProofDetail{routeNameSuffix}");

        group.MapPost("/trips/{tripId:guid}/proofs/{proofId:guid}/reject", async (
            Guid tripId,
            Guid proofId,
            RejectTripProofRequest request,
            HttpContext context,
            TripProofDvirService proofDvirService,
            CancellationToken cancellationToken) =>
            Results.Ok(await proofDvirService.RejectProofAsync(context.User, tripId, proofId, request, cancellationToken)))
        .WithName($"RejectRoutArrTripProof{routeNameSuffix}");

        group.MapPatch("/trips/{tripId:guid}/proofs/{proofId:guid}", async (
            Guid tripId,
            Guid proofId,
            CorrectTripProofRequest request,
            HttpContext context,
            TripProofDvirService proofDvirService,
            CancellationToken cancellationToken) =>
            Results.Ok(await proofDvirService.CorrectProofAsync(context.User, tripId, proofId, request, cancellationToken)))
        .WithName($"CorrectRoutArrTripProof{routeNameSuffix}");

        group.MapGet("/dvir/{dvirId:guid}", async (
            Guid dvirId,
            RoutArrAuthorizationService authorization,
            ProofDvirReportService reportService,
            IRoutArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await reportService.GetDvirDetailAsync(tenantId, dvirId, cancellationToken);
            await audit.WriteAsync(
                "routarr.reports.proof_dvir.dvir.detail",
                tenantId,
                actorUserId,
                "proof_dvir_report",
                dvirId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(detail);
        })
        .WithName($"GetRoutArrProofDvirReportDvirDetail{routeNameSuffix}");
    }
}
