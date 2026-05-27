using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class MaterialKeyEndpoints
{
    public static void MapComplianceCoreMaterialKeyEndpoints(this WebApplication app)
    {
        var keys = app.MapGroup("/api/material-keys")
            .WithTags("MaterialKeys")
            .RequireAuthorization();

        keys.MapGet("/", async (
            ComplianceCoreAuthorizationService authorization,
            MaterialKeyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireKeysRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListMaterialKeys");

        keys.MapPost("/", async (
            CreateMaterialKeyRequest request,
            ComplianceCoreAuthorizationService authorization,
            MaterialKeyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireKeysManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/material-keys/{created.MaterialKeyId}", created);
        })
        .WithName("CreateMaterialKey");
    }
}
