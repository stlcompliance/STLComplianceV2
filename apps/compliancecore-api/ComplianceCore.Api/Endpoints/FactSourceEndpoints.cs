using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class FactSourceEndpoints
{
    public static void MapComplianceCoreFactSourceEndpoints(this WebApplication app)
    {
        MapRoutes(
            app.MapGroup("/api/fact-sources")
                .WithTags("FactSources")
                .RequireAuthorization(),
            string.Empty,
            "/api/fact-sources");
        MapRoutes(
            app.MapGroup("/api/v1/fact-sources")
                .WithTags("FactSources")
                .RequireAuthorization(),
            "V1",
            "/api/v1/fact-sources");
    }

    private static void MapRoutes(RouteGroupBuilder factSources, string suffix, string routePrefix)
    {
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
        .WithName($"ListFactSources{suffix}");

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
            return Results.Created($"{routePrefix}/{created.FactSourceId}", created);
        })
        .WithName($"CreateFactSource{suffix}");

        factSources.MapPatch("/{factSourceId:guid}", async (
            Guid factSourceId,
            UpdateFactSourceRequest request,
            ComplianceCoreAuthorizationService authorization,
            FactSourceService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                context.User.GetUserId(),
                factSourceId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateFactSource{suffix}");
    }
}
