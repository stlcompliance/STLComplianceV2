using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierDirectoryEndpoints
{
    public static void MapSupplyArrSupplierDirectoryEndpoints(this WebApplication app)
    {
        MapPartyGroup(app, "/api/parties", null);
        MapPartyGroup(app, "/api/v1/parties", null);
        MapPartyGroup(app, "/api/vendors", "vendor");
        MapPartyGroup(app, "/api/v1/vendors", "vendor");
        MapPartyGroup(app, "/api/dealers", "dealer");
        MapPartyGroup(app, "/api/v1/dealers", "dealer");
        MapSupplierDirectoryGroup(app, "/api/suppliers");
        MapSupplierDirectoryGroup(app, "/api/v1/suppliers");
        MapPartyGroup(app, "/api/external-parties", null);
        MapPartyGroup(app, "/api/v1/external-parties", null);
        MapPartyGroup(app, "/api/customers", "customer");
        MapPartyGroup(app, "/api/v1/customers", "customer");
        MapContactsGroup(app, "/api/contacts");
        MapContactsGroup(app, "/api/v1/contacts");
    }

    public static void MapSupplyArrPartyRegistryEndpoints(this WebApplication app) =>
        app.MapSupplyArrSupplierDirectoryEndpoints();

    private static void MapPartyGroup(WebApplication app, string routePrefix, string? fixedPartyType)
    {
        var group = app.MapGroup(routePrefix).WithTags("ExternalParties").RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListExternalPartiesAsync(tenantId, fixedPartyType, cancellationToken));
        })
        .WithName($"ListParties{RouteSuffix(routePrefix)}");

        group.MapGet("/metadata", (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service) =>
        {
            authorization.RequirePartiesRead(context.User);
            return Results.Ok(service.GetExternalPartyMetadata());
        })
        .WithName($"GetPartyMetadata{RouteSuffix(routePrefix)}");

        group.MapGet("/{partyId:guid}", async (
            Guid partyId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            var party = await service.GetExternalPartyAsync(tenantId, partyId, cancellationToken);
            if (fixedPartyType is not null
                && !MatchesFixedPartyType(party.PartyType, fixedPartyType))
            {
                return Results.NotFound();
            }

            return Results.Ok(party);
        })
        .WithName($"GetParty{RouteSuffix(routePrefix)}");

        group.MapPost("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
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

                var created = await service.CreateExternalPartyAsync(tenantId, actorUserId, request, cancellationToken);
                return Results.Created($"{routePrefix}/{created.PartyId}", created);
            }

            var typedRequest = await context.Request.ReadFromJsonAsync<CreateTypedExternalPartyRequest>(cancellationToken);
            if (typedRequest is null)
            {
                return Results.BadRequest();
            }

            var typedCreated = await service.CreateTypedExternalPartyAsync(
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
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateExternalPartyAsync(tenantId, actorUserId, partyId, request, cancellationToken);
            if (fixedPartyType is not null
                && !MatchesFixedPartyType(updated.PartyType, fixedPartyType))
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
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateExternalPartyApprovalStatusAsync(
                tenantId,
                actorUserId,
                partyId,
                request,
                cancellationToken);
            if (fixedPartyType is not null
                && !MatchesFixedPartyType(updated.PartyType, fixedPartyType))
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
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateExternalPartyStatusAsync(
                tenantId,
                actorUserId,
                partyId,
                request,
                cancellationToken);
            if (fixedPartyType is not null
                && !MatchesFixedPartyType(updated.PartyType, fixedPartyType))
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
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();

            if (fixedPartyType is not null)
            {
                var party = await service.GetExternalPartyAsync(tenantId, partyId, cancellationToken);
                if (!MatchesFixedPartyType(party.PartyType, fixedPartyType))
                {
                    return Results.NotFound();
                }
            }

            var contact = await service.AddExternalPartyContactAsync(
                tenantId,
                actorUserId,
                partyId,
                request,
                cancellationToken);
            return Results.Created($"{routePrefix}/{partyId}/contacts/{contact.ContactId}", contact);
        })
        .WithName($"CreatePartyContact{RouteSuffix(routePrefix)}");
    }

    private static void MapSupplierDirectoryGroup(WebApplication app, string routePrefix)
    {
        var group = app.MapGroup(routePrefix).WithTags("SupplierDirectory").RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListSuppliersAsync(tenantId, cancellationToken));
        })
        .WithName($"ListSupplierDirectory{RouteSuffix(routePrefix)}");

        group.MapGet("/metadata", (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service) =>
        {
            authorization.RequirePartiesRead(context.User);
            return Results.Ok(service.GetSupplierMetadata());
        })
        .WithName($"GetSupplierDirectoryMetadata{RouteSuffix(routePrefix)}");

        group.MapGet("/{supplierId:guid}", async (
            Guid supplierId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSupplierAsync(tenantId, supplierId, cancellationToken));
        })
        .WithName($"GetSupplierDirectoryItem{RouteSuffix(routePrefix)}");

        group.MapPost("/", async (
            CreateSupplierRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateSupplierAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"{routePrefix}/{created.SupplierId}", created);
        })
        .WithName($"CreateSupplierDirectoryItem{RouteSuffix(routePrefix)}");

        group.MapPut("/{supplierId:guid}", async (
            Guid supplierId,
            UpdateSupplierRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateSupplierAsync(tenantId, actorUserId, supplierId, request, cancellationToken));
        })
        .WithName($"UpdateSupplierDirectoryItem{RouteSuffix(routePrefix)}");

        group.MapPatch("/{supplierId:guid}/approval-status", async (
            Guid supplierId,
            UpdateSupplierApprovalStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateSupplierApprovalStatusAsync(tenantId, actorUserId, supplierId, request, cancellationToken));
        })
        .WithName($"UpdateSupplierDirectoryApprovalStatus{RouteSuffix(routePrefix)}");

        group.MapPatch("/{supplierId:guid}/status", async (
            Guid supplierId,
            UpdateSupplierStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateSupplierStatusAsync(tenantId, actorUserId, supplierId, request, cancellationToken));
        })
        .WithName($"UpdateSupplierDirectoryStatus{RouteSuffix(routePrefix)}");

        group.MapPost("/{supplierId:guid}/contacts", async (
            Guid supplierId,
            CreateSupplierContactRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var contact = await service.AddSupplierContactAsync(tenantId, actorUserId, supplierId, request, cancellationToken);
            return Results.Created($"{routePrefix}/{supplierId}/contacts/{contact.ContactId}", contact);
        })
        .WithName($"CreateSupplierDirectoryContact{RouteSuffix(routePrefix)}");
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
        var group = app.MapGroup(routePrefix).WithTags("ExternalParties").RequireAuthorization();

        group.MapGet("/", async (
            Guid? partyId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesRead(context.User);
            var tenantId = context.User.GetTenantId();
            if (partyId.HasValue)
            {
                var party = await service.GetExternalPartyAsync(tenantId, partyId.Value, cancellationToken);
                return Results.Ok(party.Contacts);
            }

            var parties = await service.ListExternalPartiesAsync(tenantId, null, cancellationToken);
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
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartiesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var contact = await service.AddExternalPartyContactAsync(
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

    private static bool MatchesFixedPartyType(string actualType, string fixedPartyType)
    {
        if (string.Equals(actualType, fixedPartyType, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(actualType, "supplier", StringComparison.OrdinalIgnoreCase)
            && (string.Equals(fixedPartyType, "vendor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fixedPartyType, "dealer", StringComparison.OrdinalIgnoreCase));
    }
}
