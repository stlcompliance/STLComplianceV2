using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class CitationFactEndpoints
{
    public static void MapComplianceCoreCitationFactEndpoints(this WebApplication app)
    {
        MapCitationRoutes(
            app.MapGroup("/api/citations")
                .WithTags("Citations")
                .RequireAuthorization(),
            string.Empty);
        MapCitationRoutes(
            app.MapGroup("/api/v1/citations")
                .WithTags("Citations")
                .RequireAuthorization(),
            "V1Citations");

        MapFactDefinitionRoutes(
            app.MapGroup("/api/fact-definitions")
                .WithTags("FactCatalog")
                .RequireAuthorization(),
            string.Empty);
        MapFactDefinitionRoutes(
            app.MapGroup("/api/v1/facts")
                .WithTags("FactCatalog")
                .RequireAuthorization(),
            "V1Facts");

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

    private static void MapCitationRoutes(RouteGroupBuilder citations, string nameSuffix)
    {
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
        .WithName($"ListRegulatoryCitations{nameSuffix}");

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
        .WithName($"CreateRegulatoryCitation{nameSuffix}");

        citations.MapGet("/{citationId:guid}", async (
            Guid citationId,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryCitationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, citationId, cancellationToken));
        })
        .WithName($"GetRegulatoryCitation{nameSuffix}");

        citations.MapPatch("/{citationId:guid}", async (
            Guid citationId,
            UpdateRegulatoryCitationRequest request,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryCitationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                context.User.GetUserId(),
                citationId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateRegulatoryCitation{nameSuffix}");

        citations.MapGet("/{citationId:guid}/history", async (
            Guid citationId,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryCitationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListHistoryAsync(tenantId, citationId, cancellationToken));
        })
        .WithName($"ListRegulatoryCitationHistory{nameSuffix}");

        citations.MapGet("/{citationId:guid}/rules", async (
            Guid citationId,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryCitationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListRuleLinksAsync(tenantId, citationId, cancellationToken));
        })
        .WithName($"ListRegulatoryCitationRules{nameSuffix}");
    }

    private static void MapFactDefinitionRoutes(RouteGroupBuilder factDefinitions, string nameSuffix)
    {
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
        .WithName($"ListFactDefinitions{nameSuffix}");

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
        .WithName($"CreateFactDefinition{nameSuffix}");

        factDefinitions.MapGet("/{factKey}", async (
            string factKey,
            ComplianceCoreAuthorizationService authorization,
            FactDefinitionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetByKeyAsync(tenantId, factKey, cancellationToken));
        })
        .WithName($"GetFactDefinition{nameSuffix}");

        factDefinitions.MapPatch("/{factKey}", async (
            string factKey,
            UpdateFactDefinitionRequest request,
            ComplianceCoreAuthorizationService authorization,
            FactDefinitionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.UpdateByKeyAsync(
                tenantId,
                context.User.GetUserId(),
                factKey,
                request,
                cancellationToken));
        })
        .WithName($"UpdateFactDefinition{nameSuffix}");

        factDefinitions.MapGet("/{factKey}/usage", async (
            string factKey,
            ComplianceCoreAuthorizationService authorization,
            FactDefinitionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetUsageByKeyAsync(tenantId, factKey, cancellationToken));
        })
        .WithName($"GetFactDefinitionUsage{nameSuffix}");

        factDefinitions.MapPost("/validate-payload", async (
            ValidateFactPayloadRequest request,
            ComplianceCoreAuthorizationService authorization,
            FactDefinitionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ValidatePayloadAsync(tenantId, request, cancellationToken));
        })
        .WithName($"ValidateFactPayload{nameSuffix}");
    }
}
