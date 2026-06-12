using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PartyRegistryEndpoints
{
    public static void MapSupplyArrPartyRegistryEndpoints(this WebApplication app)
    {
        MapPartyGroup(app, "/api/parties", null);
        MapPartyGroup(app, "/api/v1/parties", null);
        MapPartyGroup(app, "/api/vendors", "vendor");
        MapPartyGroup(app, "/api/v1/vendors", "vendor");
        MapPartyGroup(app, "/api/dealers", "dealer");
        MapPartyGroup(app, "/api/v1/dealers", "dealer");
        MapPartyGroup(app, "/api/suppliers", "supplier");
        MapPartyGroup(app, "/api/v1/suppliers", "supplier");
        MapPartyGroup(app, "/api/external-parties", null);
        MapPartyGroup(app, "/api/v1/external-parties", null);
        MapPartyGroup(app, "/api/customers", "customer");
        MapPartyGroup(app, "/api/v1/customers", "customer");
        MapContactsGroup(app, "/api/contacts");
        MapContactsGroup(app, "/api/v1/contacts");
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

        group.MapGet("/metadata", (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service) =>
        {
            authorization.RequirePartiesRead(context.User);
            return Results.Ok(service.GetMetadata());
        })
        .WithName($"GetPartyMetadata{RouteSuffix(routePrefix)}");

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
            "/api/v1/vendors" => "VendorsV1",
            "/api/dealers" => "Dealers",
            "/api/v1/dealers" => "DealersV1",
            "/api/suppliers" => "Suppliers",
            "/api/v1/suppliers" => "SuppliersV1",
            "/api/external-parties" => "ExternalParties",
            "/api/v1/external-parties" => "ExternalPartiesV1",
            "/api/customers" => "Customers",
            "/api/v1/customers" => "CustomersV1",
            "/api/v1/parties" => "AllV1",
            _ => "All"
        };

    private static void MapContactsGroup(WebApplication app, string routePrefix)
    {
        var group = app.MapGroup(routePrefix).WithTags("PartyRegistry").RequireAuthorization();

        group.MapGet("/", async (
            Guid? partyId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            if (partyId.HasValue)
            {
                var party = await service.GetAsync(tenantId, partyId.Value, cancellationToken);
                return Results.Ok(party.Contacts);
            }

            var parties = await service.ListAsync(tenantId, null, cancellationToken);
            var contacts = parties
                .SelectMany(p => p.Contacts)
                .ToList();
            return Results.Ok(contacts);
        })
        .WithName($"ListContacts{ContactsRouteSuffix(routePrefix)}");

        group.MapPost("/", async (
            CreateExternalPartyContactRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ExternalPartyService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var contact = await service.AddContactAsync(
                tenantId,
                actorUserId,
                request.PartyId,
                new CreatePartyContactRequest(
                    request.ContactName,
                    request.Email,
                    request.Phone,
                    request.RoleLabel,
                    request.IsPrimary),
                cancellationToken);
            return Results.Created($"{routePrefix}/{contact.ContactId}", contact);
        })
        .WithName($"CreateContact{ContactsRouteSuffix(routePrefix)}");
    }

    private static string ContactsRouteSuffix(string routePrefix) =>
        routePrefix.Contains("/v1/", StringComparison.OrdinalIgnoreCase) ? "V1" : string.Empty;
}
