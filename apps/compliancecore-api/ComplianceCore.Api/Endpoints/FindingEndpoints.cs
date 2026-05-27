using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class FindingEndpoints
{
    public static void MapComplianceCoreFindingEndpoints(this WebApplication app)
    {
        var findings = app.MapGroup("/api/findings")
            .WithTags("Findings")
            .RequireAuthorization();

        findings.MapGet("/", async (
            Guid? rulePackId,
            Guid? evaluationRunId,
            string? status,
            ComplianceCoreAuthorizationService authorization,
            ComplianceFindingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                rulePackId,
                evaluationRunId,
                status,
                cancellationToken));
        })
        .WithName("ListComplianceFindings");

        findings.MapPost("/", async (
            CreateComplianceFindingRequest request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceFindingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/findings/{created.FindingId}", created);
        })
        .WithName("CreateComplianceFinding");

        findings.MapPatch("/{findingId:guid}/status", async (
            Guid findingId,
            UpdateComplianceFindingStatusRequest request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceFindingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.UpdateStatusAsync(
                tenantId,
                findingId,
                context.User.GetUserId(),
                request,
                cancellationToken));
        })
        .WithName("UpdateComplianceFindingStatus");
    }
}
