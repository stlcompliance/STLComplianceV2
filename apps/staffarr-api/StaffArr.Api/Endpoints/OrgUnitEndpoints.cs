using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class OrgUnitEndpoints
{
    public static void MapStaffArrOrgUnitEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/org-units", Suffix: string.Empty),
            (Route: "/api/v1/org-units", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route).WithTags("OrgUnits").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                OrgUnitService service,
                [AsParameters] OrgUnitQuery request,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                if (string.Equals(request.Type, "site", StringComparison.OrdinalIgnoreCase))
                {
                    authorization.RequireSiteRead(context.User, projection);
                }
                else
                {
                    authorization.RequireOrganizationRead(context.User, projection);
                }

                return Results.Ok(await service.ListAsync(
                    tenantId,
                    includeArchived: request.IncludeArchived,
                    request.Search,
                    request.Type,
                    cancellationToken));
            })
            .WithName($"ListOrgUnits{suffix}");

            group.MapGet("/tree", async (
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                OrgUnitService service,
                [AsParameters] OrgUnitQuery request,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                if (string.Equals(request.Type, "site", StringComparison.OrdinalIgnoreCase))
                {
                    authorization.RequireSiteRead(context.User, projection);
                }
                else
                {
                    authorization.RequireOrganizationRead(context.User, projection);
                }

                return Results.Ok(await service.ListTreeAsync(
                    tenantId,
                    includeArchived: request.IncludeArchived,
                    request.Search,
                    request.Type,
                    cancellationToken));
            })
            .WithName($"ListOrgUnitTree{suffix}");

            group.MapGet("/{orgUnitId:guid}", async (
                Guid orgUnitId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var orgUnit = await service.GetAsync(tenantId, orgUnitId, cancellationToken);
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                if (string.Equals(orgUnit.UnitType, "site", StringComparison.OrdinalIgnoreCase))
                {
                    authorization.RequireSiteRead(context.User, projection);
                }
                else
                {
                    authorization.RequireOrganizationRead(context.User, projection);
                }

                return Results.Ok(orgUnit);
            })
            .WithName($"GetOrgUnit{suffix}");

            group.MapPost("/", async (
                CreateOrgUnitRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                if (string.Equals(request.UnitType, "site", StringComparison.OrdinalIgnoreCase))
                {
                    authorization.RequireSiteCreate(context.User, projection);
                }
                else
                {
                    authorization.RequireOrganizationCreate(context.User, projection);
                }

                var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
                return Results.Created($"{route}/{created.OrgUnitId}", created);
            })
            .WithName($"CreateOrgUnit{suffix}");

            group.MapPut("/{orgUnitId:guid}", async (
                Guid orgUnitId,
                UpdateOrgUnitRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var current = await service.GetAsync(tenantId, orgUnitId, cancellationToken);
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                if (string.Equals(current.UnitType, "site", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(request.UnitType, "site", StringComparison.OrdinalIgnoreCase))
                {
                    authorization.RequireSiteUpdate(context.User, projection);
                }
                else
                {
                    authorization.RequireOrganizationUpdate(context.User, projection);
                }

                var updated = await service.UpdateAsync(tenantId, actorUserId, orgUnitId, request, cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"UpdateOrgUnit{suffix}");

            group.MapPatch("/{orgUnitId:guid}/status", async (
                Guid orgUnitId,
                UpdateOrgUnitStatusRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var current = await service.GetAsync(tenantId, orgUnitId, cancellationToken);
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                if (string.Equals(current.UnitType, "site", StringComparison.OrdinalIgnoreCase))
                {
                    authorization.RequireSiteUpdate(context.User, projection);
                }
                else
                {
                    authorization.RequireOrganizationUpdate(context.User, projection);
                }

                var updated = await service.UpdateStatusAsync(tenantId, actorUserId, orgUnitId, request, cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"UpdateOrgUnitStatus{suffix}");

            group.MapPost("/{orgUnitId:guid}/archive", async (
                Guid orgUnitId,
                ArchiveOrgUnitRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var current = await service.GetAsync(tenantId, orgUnitId, cancellationToken);
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                if (string.Equals(current.UnitType, "site", StringComparison.OrdinalIgnoreCase))
                {
                    authorization.RequireSiteArchive(context.User, projection);
                }
                else
                {
                    authorization.RequireOrganizationArchive(context.User, projection);
                }

                var updated = await service.ArchiveAsync(tenantId, actorUserId, orgUnitId, request, cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"ArchiveOrgUnit{suffix}");
        }
    }

    private sealed record OrgUnitQuery(
        bool IncludeArchived = false,
        string? Search = null,
        string? Type = null);
}
