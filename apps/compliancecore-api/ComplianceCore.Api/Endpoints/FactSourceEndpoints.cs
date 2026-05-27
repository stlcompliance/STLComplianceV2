using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class FactSourceEndpoints
{
    public static void MapComplianceCoreFactSourceEndpoints(this WebApplication app)
    {
        var factSources = app.MapGroup("/api/fact-sources")
            .WithTags("FactSources")
            .RequireAuthorization();

        factSources.MapGet("/", async (
            Guid? factDefinitionId,
            ComplianceCoreAuthorizationService authorization,
            FactSourceService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, factDefinitionId, cancellationToken));
        })
        .WithName("ListFactSources");

        factSources.MapPost("/", async (
            CreateFactSourceRequest request,
            ComplianceCoreAuthorizationService authorization,
            FactSourceService service,
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
            return Results.Created($"/api/fact-sources/{created.FactSourceId}", created);
        })
        .WithName("CreateFactSource");
    }
}
