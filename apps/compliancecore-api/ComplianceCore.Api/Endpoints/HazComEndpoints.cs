using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class HazComEndpoints
{
    public static void MapComplianceCoreHazComEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/hazcom")
            .WithTags("HazComReferences")
            .RequireAuthorization();

        group.MapGet("/", async (
            bool? includeInactive,
            ComplianceCoreAuthorizationService authorization,
            HazComReferenceService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireKeysRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, includeInactive ?? false, cancellationToken));
        })
        .WithName("ListHazComReferences");

        group.MapGet("/{hazComReferenceId:guid}", async (
            Guid hazComReferenceId,
            ComplianceCoreAuthorizationService authorization,
            HazComReferenceService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireKeysRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, hazComReferenceId, cancellationToken));
        })
        .WithName("GetHazComReference");

        group.MapPost("/", async (
            CreateHazComReferenceRequest request,
            ComplianceCoreAuthorizationService authorization,
            HazComReferenceService service,
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
            return Results.Created($"/api/hazcom/{created.HazComReferenceId}", created);
        })
        .WithName("CreateHazComReference");

        group.MapPatch("/{hazComReferenceId:guid}", async (
            Guid hazComReferenceId,
            UpdateHazComReferenceRequest request,
            ComplianceCoreAuthorizationService authorization,
            HazComReferenceService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireKeysManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                context.User.GetUserId(),
                hazComReferenceId,
                request,
                cancellationToken));
        })
        .WithName("UpdateHazComReference");
    }
}
