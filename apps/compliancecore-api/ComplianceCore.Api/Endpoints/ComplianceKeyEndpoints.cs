using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class ComplianceKeyEndpoints
{
    public static void MapComplianceCoreComplianceKeyEndpoints(this WebApplication app)
    {
        var keys = app.MapGroup("/api/compliance-keys")
            .WithTags("ComplianceKeys")
            .RequireAuthorization();

        keys.MapGet("/", async (
            ComplianceCoreAuthorizationService authorization,
            ComplianceKeyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireKeysRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListComplianceKeys");

        keys.MapPost("/", async (
            CreateComplianceKeyRequest request,
            ComplianceCoreAuthorizationService authorization,
            ComplianceKeyService service,
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
            return Results.Created($"/api/compliance-keys/{created.ComplianceKeyId}", created);
        })
        .WithName("CreateComplianceKey");
    }
}
