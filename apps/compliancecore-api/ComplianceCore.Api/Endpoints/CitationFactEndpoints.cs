using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class CitationFactEndpoints
{
    public static void MapComplianceCoreCitationFactEndpoints(this WebApplication app)
    {
        var citations = app.MapGroup("/api/citations")
            .WithTags("Citations")
            .RequireAuthorization();

        citations.MapGet("/", async (
            Guid? regulatoryProgramId,
            Guid? rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryCitationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, regulatoryProgramId, rulePackId, cancellationToken));
        })
        .WithName("ListRegulatoryCitations");

        citations.MapPost("/", async (
            CreateRegulatoryCitationRequest request,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryCitationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/citations/{created.CitationId}", created);
        })
        .WithName("CreateRegulatoryCitation");

        var factDefinitions = app.MapGroup("/api/fact-definitions")
            .WithTags("FactCatalog")
            .RequireAuthorization();

        factDefinitions.MapGet("/", async (
            ComplianceCoreAuthorizationService authorization,
            FactDefinitionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListFactDefinitions");

        factDefinitions.MapPost("/", async (
            CreateFactDefinitionRequest request,
            ComplianceCoreAuthorizationService authorization,
            FactDefinitionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/fact-definitions/{created.FactDefinitionId}", created);
        })
        .WithName("CreateFactDefinition");

        var factRequirements = app.MapGroup("/api/fact-requirements")
            .WithTags("FactCatalog")
            .RequireAuthorization();

        factRequirements.MapGet("/", async (
            Guid? rulePackId,
            Guid? citationId,
            ComplianceCoreAuthorizationService authorization,
            FactRequirementService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, rulePackId, citationId, cancellationToken));
        })
        .WithName("ListFactRequirements");

        factRequirements.MapPost("/", async (
            CreateFactRequirementRequest request,
            ComplianceCoreAuthorizationService authorization,
            FactRequirementService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/fact-requirements/{created.FactRequirementId}", created);
        })
        .WithName("CreateFactRequirement");
    }
}
