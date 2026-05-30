using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class RegulatoryMappingEndpoints
{
    public static void MapComplianceCoreRegulatoryMappingEndpoints(this WebApplication app)
    {
        MapMappingRoutes(
            app.MapGroup("/api/regulatory-mappings")
                .WithTags("RegulatoryMappings")
                .RequireAuthorization(),
            string.Empty);
        MapMappingRoutes(
            app.MapGroup("/api/v1/regulatory-mappings")
                .WithTags("RegulatoryMappings")
                .RequireAuthorization(),
            "V1");

        var derivedFacts = app.MapGroup("/api/v1/derived-facts")
            .WithTags("DerivedFacts")
            .RequireAuthorization();

        derivedFacts.MapGet("/", async (
            Guid? regulatoryProgramId,
            Guid? rulePackId,
            Guid? citationId,
            Guid? complianceKeyId,
            Guid? materialKeyId,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryMappingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                regulatoryProgramId,
                rulePackId,
                citationId,
                complianceKeyId,
                materialKeyId,
                cancellationToken));
        })
        .WithName("ListDerivedFactsV1");

        derivedFacts.MapPost("/preview", async (
            DerivedFactPreviewRequest request,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryMappingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var items = await service.ListAsync(
                tenantId,
                request.RegulatoryProgramId,
                request.RulePackId,
                request.CitationId,
                request.ComplianceKeyId,
                request.MaterialKeyId,
                cancellationToken);
            var limited = request.Limit.HasValue && request.Limit.Value > 0
                ? items.Take(request.Limit.Value).ToList()
                : items;
            return Results.Ok(new DerivedFactPreviewResponse(
                DateTimeOffset.UtcNow,
                limited.Count,
                limited));
        })
        .WithName("PreviewDerivedFactsV1");
    }

    private static void MapMappingRoutes(RouteGroupBuilder mappings, string suffix)
    {
        mappings.MapGet("/", async (
            Guid? regulatoryProgramId,
            Guid? rulePackId,
            Guid? citationId,
            Guid? complianceKeyId,
            Guid? materialKeyId,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryMappingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                regulatoryProgramId,
                rulePackId,
                citationId,
                complianceKeyId,
                materialKeyId,
                cancellationToken));
        })
        .WithName($"ListRegulatoryMappings{suffix}");

        mappings.MapPost("/", async (
            CreateRegulatoryMappingRequest request,
            ComplianceCoreAuthorizationService authorization,
            RegulatoryMappingService service,
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
            return Results.Created($"/api/regulatory-mappings/{created.RegulatoryMappingId}", created);
        })
        .WithName($"CreateRegulatoryMapping{suffix}");
    }
}
