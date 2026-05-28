using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class VendorRestrictionEndpoints
{
    public static void MapSupplyArrVendorRestrictionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/vendor-restrictions")
            .WithTags("VendorRestrictions")
            .RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorRestrictionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName("ListVendorRestrictions");

        group.MapGet("/{restrictionId:guid}", async (
            Guid restrictionId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorRestrictionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, restrictionId, cancellationToken));
        })
        .WithName("GetVendorRestriction");

        group.MapPost("/{restrictionId:guid}/lift", async (
            Guid restrictionId,
            LiftVendorRestrictionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorRestrictionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.LiftAsync(
                tenantId,
                actorUserId,
                restrictionId,
                request,
                cancellationToken));
        })
        .WithName("LiftVendorRestriction");

        group.MapPut("/{restrictionId:guid}", async (
            Guid restrictionId,
            UpdateVendorRestrictionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorRestrictionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                actorUserId,
                restrictionId,
                request,
                cancellationToken));
        })
        .WithName("UpdateVendorRestriction");

        var partyGroup = app.MapGroup("/api/parties/{partyId:guid}/vendor-restrictions")
            .WithTags("VendorRestrictions")
            .RequireAuthorization();

        partyGroup.MapGet("/", async (
            Guid partyId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorRestrictionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListByPartyAsync(tenantId, partyId, cancellationToken));
        })
        .WithName("ListVendorRestrictionsByParty");

        partyGroup.MapPost("/", async (
            Guid partyId,
            CreateVendorRestrictionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorRestrictionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CreateAsync(
                tenantId,
                actorUserId,
                partyId,
                request,
                cancellationToken));
        })
        .WithName("CreateVendorRestrictionForParty");

        partyGroup.MapGet("/enforcement", async (
            Guid partyId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorRestrictionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetEnforcementAsync(tenantId, partyId, cancellationToken));
        })
        .WithName("GetVendorRestrictionEnforcement");
    }
}
