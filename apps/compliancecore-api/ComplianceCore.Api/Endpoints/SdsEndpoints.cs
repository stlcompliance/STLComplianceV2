using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class SdsEndpoints
{
    public static void MapComplianceCoreSdsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sds")
            .WithTags("SdsReferences")
            .RequireAuthorization();

        group.MapGet("/", async (
            bool? includeInactive,
            ComplianceCoreAuthorizationService authorization,
            SdsReferenceService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireKeysRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, includeInactive ?? false, cancellationToken));
        })
        .WithName("ListSdsReferences");

        group.MapGet("/{sdsReferenceId:guid}", async (
            Guid sdsReferenceId,
            ComplianceCoreAuthorizationService authorization,
            SdsReferenceService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireKeysRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, sdsReferenceId, cancellationToken));
        })
        .WithName("GetSdsReference");

        group.MapPost("/", async (
            CreateSdsReferenceRequest request,
            ComplianceCoreAuthorizationService authorization,
            SdsReferenceService service,
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
            return Results.Created($"/api/sds/{created.SdsReferenceId}", created);
        })
        .WithName("CreateSdsReference");

        group.MapPatch("/{sdsReferenceId:guid}", async (
            Guid sdsReferenceId,
            UpdateSdsReferenceRequest request,
            ComplianceCoreAuthorizationService authorization,
            SdsReferenceService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireKeysManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                context.User.GetUserId(),
                sdsReferenceId,
                request,
                cancellationToken));
        })
        .WithName("UpdateSdsReference");
    }
}
