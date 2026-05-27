using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class WorkflowGateEndpoints
{
    public static void MapComplianceCoreWorkflowGateEndpoints(this WebApplication app)
    {
        var gates = app.MapGroup("/api/workflow-gates")
            .WithTags("WorkflowGates")
            .RequireAuthorization();

        gates.MapGet("/", async (
            ComplianceCoreAuthorizationService authorization,
            WorkflowGateService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListDefinitionsAsync(tenantId, cancellationToken));
        })
        .WithName("ListWorkflowGates");

        gates.MapPost("/", async (
            CreateWorkflowGateDefinitionRequest request,
            ComplianceCoreAuthorizationService authorization,
            WorkflowGateService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkflowGatesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateDefinitionAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/workflow-gates/{created.WorkflowGateId}", created);
        })
        .WithName("CreateWorkflowGate");

        gates.MapPost("/check", async (
            WorkflowGateCheckRequest request,
            ComplianceCoreAuthorizationService authorization,
            WorkflowGateService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkflowGateCheck(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.CheckForUserAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken));
        })
        .WithName("CheckWorkflowGate");

        gates.MapPost("/check/batch", async (
            WorkflowGateBatchCheckRequest request,
            ComplianceCoreAuthorizationService authorization,
            WorkflowGateService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkflowGateCheck(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.CheckBatchForUserAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken));
        })
        .WithName("CheckWorkflowGateBatch");

        gates.MapPost("/seed/dispatch", async (
            ComplianceCoreAuthorizationService authorization,
            DispatchWorkflowGateSeedService seedService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkflowGatesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var gates = await seedService.EnsureDispatchGatesAsync(
                tenantId,
                context.User.GetUserId(),
                cancellationToken);
            return Results.Ok(gates);
        })
        .WithName("SeedDispatchWorkflowGates");
    }
}
