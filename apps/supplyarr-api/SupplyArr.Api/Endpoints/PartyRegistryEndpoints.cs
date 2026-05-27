using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PartyRegistryEndpoints
{
    public static void MapSupplyArrPartyRegistryEndpoints(this WebApplication app)
    {
        MapPartyGroup(app, "/api/parties", null);
        MapPartyGroup(app, "/api/vendors", "vendor");
        MapPartyGroup(app, "/api/dealers", "dealer");
        MapPartyGroup(app, "/api/suppliers", "supplier");
    }

    private static void MapPartyGroup(WebApplication app, string routePrefix, string? fixedPartyType)
    {
        var group = app.MapGroup(routePrefix).WithTags("PartyRegistry").RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, fixedPartyType, cancellationToken));
        })
        .WithName($"ListParties{RouteSuffix(routePrefix)}");

        group.MapGet("/{partyId:guid}", async (
            Guid partyId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            var party = await service.GetAsync(tenantId, partyId, cancellationToken);
            if (fixedPartyType is not null
                && !string.Equals(party.PartyType, fixedPartyType, StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound();
            }

            return Results.Ok(party);
        })
        .WithName($"GetParty{RouteSuffix(routePrefix)}");

        group.MapPost("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();

            if (fixedPartyType is null)
            {
                var request = await context.Request.ReadFromJsonAsync<CreateExternalPartyRequest>(cancellationToken);
                if (request is null)
                {
                    return Results.BadRequest();
                }

                var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
                return Results.Created($"{routePrefix}/{created.PartyId}", created);
            }

            var typedRequest = await context.Request.ReadFromJsonAsync<CreateTypedExternalPartyRequest>(cancellationToken);
            if (typedRequest is null)
            {
                return Results.BadRequest();
            }

            var typedCreated = await service.CreateTypedAsync(
                tenantId,
                actorUserId,
                fixedPartyType,
                typedRequest,
                cancellationToken);
            return Results.Created($"{routePrefix}/{typedCreated.PartyId}", typedCreated);
        })
        .WithName($"CreateParty{RouteSuffix(routePrefix)}");

        group.MapPut("/{partyId:guid}", async (
            Guid partyId,
            UpdateExternalPartyRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateAsync(tenantId, actorUserId, partyId, request, cancellationToken);
            if (fixedPartyType is not null
                && !string.Equals(updated.PartyType, fixedPartyType, StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound();
            }

            return Results.Ok(updated);
        })
        .WithName($"UpdateParty{RouteSuffix(routePrefix)}");

        group.MapPatch("/{partyId:guid}/approval-status", async (
            Guid partyId,
            UpdateExternalPartyApprovalStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateApprovalStatusAsync(
                tenantId,
                actorUserId,
                partyId,
                request,
                cancellationToken);
            if (fixedPartyType is not null
                && !string.Equals(updated.PartyType, fixedPartyType, StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound();
            }

            return Results.Ok(updated);
        })
        .WithName($"UpdatePartyApprovalStatus{RouteSuffix(routePrefix)}");

        group.MapPatch("/{partyId:guid}/status", async (
            Guid partyId,
            UpdateExternalPartyStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateStatusAsync(
                tenantId,
                actorUserId,
                partyId,
                request,
                cancellationToken);
            if (fixedPartyType is not null
                && !string.Equals(updated.PartyType, fixedPartyType, StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound();
            }

            return Results.Ok(updated);
        })
        .WithName($"UpdatePartyStatus{RouteSuffix(routePrefix)}");

        group.MapPost("/{partyId:guid}/contacts", async (
            Guid partyId,
            CreatePartyContactRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();

            if (fixedPartyType is not null)
            {
                var party = await service.GetAsync(tenantId, partyId, cancellationToken);
                if (!string.Equals(party.PartyType, fixedPartyType, StringComparison.OrdinalIgnoreCase))
                {
                    return Results.NotFound();
                }
            }

            var contact = await service.AddContactAsync(
                tenantId,
                actorUserId,
                partyId,
                request,
                cancellationToken);
            return Results.Created($"{routePrefix}/{partyId}/contacts/{contact.ContactId}", contact);
        })
        .WithName($"CreatePartyContact{RouteSuffix(routePrefix)}");
    }

    private static string RouteSuffix(string routePrefix) =>
        routePrefix switch
        {
            "/api/vendors" => "Vendors",
            "/api/dealers" => "Dealers",
            "/api/suppliers" => "Suppliers",
            _ => "All"
        };
}
