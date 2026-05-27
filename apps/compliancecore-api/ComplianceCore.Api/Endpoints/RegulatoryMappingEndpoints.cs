using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class RegulatoryMappingEndpoints
{
    public static void MapComplianceCoreRegulatoryMappingEndpoints(this WebApplication app)
    {
        var mappings = app.MapGroup("/api/regulatory-mappings")
            .WithTags("RegulatoryMappings")
            .RequireAuthorization();

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
        .WithName("ListRegulatoryMappings");

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
        .WithName("CreateRegulatoryMapping");
    }
}
