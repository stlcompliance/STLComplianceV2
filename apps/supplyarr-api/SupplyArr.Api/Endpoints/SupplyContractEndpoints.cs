using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplyContractEndpoints
{
    public static void MapSupplyArrContractEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/contracts/records")
            .WithTags("Contracts")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid? supplierId,
            Guid? vendorPartyId,
            string? status,
            int? limit,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyContractService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, supplierId, vendorPartyId, status, limit, cancellationToken));
        })
        .WithName("ListContractRecordsV1");

        group.MapPost("/", async (
            CreateSupplyContractRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyContractService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/v1/contracts/records/{created.ContractId}", created);
        })
        .WithName("CreateContractRecordV1");

        group.MapGet("/{contractId:guid}", async (
            Guid contractId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplyContractService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, contractId, cancellationToken));
        })
        .WithName("GetContractRecordV1");
    }
}
