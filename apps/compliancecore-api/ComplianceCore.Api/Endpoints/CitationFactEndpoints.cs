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
            string? sourceProduct,
            string? sourceEntity,
            string? factKey,
            ComplianceCoreAuthorizationService authorization,
            FactRequirementService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                rulePackId,
                citationId,
                sourceProduct,
                sourceEntity,
                factKey,
                cancellationToken));
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

        citations.MapGet("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryCitationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, id, cancellationToken));
        })
        .WithName($"GetRegulatoryCitation{nameSuffix}");

        citations.MapPatch("/{id:guid}", async (
            Guid id,
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
                id,
                request,
                cancellationToken));
        })
        .WithName($"UpdateRegulatoryCitation{nameSuffix}");

        citations.MapGet("/{id:guid}/history", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryCitationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListHistoryAsync(tenantId, id, cancellationToken));
        })
        .WithName($"ListRegulatoryCitationHistory{nameSuffix}");

        citations.MapGet("/{id:guid}/rules", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryCitationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListRuleLinksAsync(tenantId, id, cancellationToken));
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

        factDefinitions.MapGet("/{key}", async (
            string key,
            ComplianceCoreAuthorizationService authorization,
            FactDefinitionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetByKeyAsync(tenantId, key, cancellationToken));
        })
        .WithName($"GetFactDefinition{nameSuffix}");

        factDefinitions.MapPatch("/{key}", async (
            string key,
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
                key,
                request,
                cancellationToken));
        })
        .WithName($"UpdateFactDefinition{nameSuffix}");

        factDefinitions.MapGet("/{key}/usage", async (
            string key,
            ComplianceCoreAuthorizationService authorization,
            FactDefinitionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetUsageByKeyAsync(tenantId, key, cancellationToken));
        })
        .WithName($"GetFactDefinitionUsage{nameSuffix}");

        factDefinitions.MapGet("/{key}/history", async (
            string key,
            ComplianceCoreAuthorizationService authorization,
            FactDefinitionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListHistoryByKeyAsync(tenantId, key, cancellationToken));
        })
        .WithName($"ListFactDefinitionHistory{nameSuffix}");

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
