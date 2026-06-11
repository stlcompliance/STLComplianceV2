using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace StaffArr.Api.Endpoints;

public static class RoleManagementEndpoints
{
    public static void MapStaffArrRoleManagementEndpoints(this WebApplication app)
    {
        var roles = app.MapGroup("/api/v1/roles")
            .WithTags("RoleManagement")
            .RequireAuthorization();

        roles.MapGet("/", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            return Results.Ok(await service.ListRolesAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName("ListStaffRolesV1");

        roles.MapPost("/", async (
            CreateStaffRoleRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            var created = await service.CreateRoleAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId(),
                request,
                cancellationToken);
            return Results.Created($"/api/v1/roles/{created.RoleId}", created);
        })
        .WithName("CreateStaffRoleV1");

        roles.MapGet("/{roleId:guid}", async (
            Guid roleId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            return Results.Ok(await service.GetRoleAsync(context.User.GetTenantId(), roleId, cancellationToken));
        })
        .WithName("GetStaffRoleV1");

        roles.MapPut("/{roleId:guid}", async (
            Guid roleId,
            UpdateStaffRoleRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            return Results.Ok(await service.UpdateRoleAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId(),
                roleId,
                request,
                cancellationToken));
        })
        .WithName("UpdateStaffRoleV1");

        roles.MapPost("/{roleId:guid}/archive", async (
            Guid roleId,
            ArchiveStaffRoleRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            return Results.Ok(await service.ArchiveRoleAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId(),
                roleId,
                request.Reason,
                cancellationToken));
        })
        .WithName("ArchiveStaffRoleV1");

        roles.MapPost("/{roleId:guid}/clone", async (
            Guid roleId,
            CloneStaffRoleRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            var clone = await service.CloneRoleAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId(),
                roleId,
                request,
                cancellationToken);
            return Results.Created($"/api/v1/roles/{clone.RoleId}", clone);
        })
        .WithName("CloneStaffRoleV1");

        roles.MapGet("/{roleId:guid}/permissions", async (
            Guid roleId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            return Results.Ok(await service.GetRolePermissionsAsync(context.User.GetTenantId(), roleId, cancellationToken));
        })
        .WithName("GetStaffRolePermissionsV1");

        roles.MapPut("/{roleId:guid}/permissions", async (
            Guid roleId,
            SetStaffRolePermissionsRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            return Results.Ok(await service.SetRolePermissionsAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId(),
                roleId,
                request,
                cancellationToken));
        })
        .WithName("SetStaffRolePermissionsV1");

        roles.MapGet("/{roleId:guid}/scopes", async (
            Guid roleId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            return Results.Ok(await service.GetRoleScopesAsync(context.User.GetTenantId(), roleId, cancellationToken));
        })
        .WithName("GetStaffRoleScopesV1");

        roles.MapPut("/{roleId:guid}/scopes", async (
            Guid roleId,
            SetStaffRoleScopesRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            return Results.Ok(await service.SetRoleScopesAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId(),
                roleId,
                request,
                cancellationToken));
        })
        .WithName("SetStaffRoleScopesV1");

        var personRoles = app.MapGroup("/api/v1/people/{personId:guid}/roles")
            .WithTags("RoleManagement")
            .RequireAuthorization();

        personRoles.MapGet("/", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            return Results.Ok(await service.GetPersonRolesAsync(context.User.GetTenantId(), personId, cancellationToken));
        })
        .WithName("GetPersonRolesV1");

        personRoles.MapPut("/", async (
            Guid personId,
            SetStaffPersonRolesRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            return Results.Ok(await service.SetPersonRolesAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId(),
                personId,
                request,
                cancellationToken));
        })
        .WithName("SetPersonRolesV1");

        var permissions = app.MapGroup("/api/v1/permissions")
            .WithTags("RoleManagement")
            .RequireAuthorization();

        permissions.MapGet("/catalog", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            return Results.Ok(await service.GetPermissionCatalogsAsync(
                context.User.GetTenantId(),
                context.User.GetEntitlements(),
                cancellationToken));
        })
        .WithName("GetPermissionCatalogV1");

        permissions.MapPost("/catalog/refresh", async (
            RefreshPermissionCatalogRequest? request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            return Results.Ok(await service.RefreshPermissionCatalogsAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId(),
                request?.ProductKey,
                context.User.GetEntitlements(),
                cancellationToken));
        })
        .WithName("RefreshPermissionCatalogV1");

        permissions.MapPost("/evaluate", async (
            PermissionEvaluateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            if (request.TenantId != context.User.GetTenantId())
            {
                throw new StlApiException("permission_evaluate.validation", "tenantId must match the current tenant.", 400);
            }

            return Results.Ok(await service.EvaluatePermissionAsync(request, cancellationToken));
        })
        .WithName("EvaluatePermissionV1");

        var integrations = app.MapGroup("/api/v1/integrations/permissions")
            .WithTags("RoleManagementIntegration");

        integrations.MapGet("/catalog", async (
            Guid tenantId,
            string? productKey,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            var sourceProduct = ValidatePermissionCatalogReadServiceToken(tokenValidator, context, tenantId);
            var effectiveProductKey = string.IsNullOrWhiteSpace(productKey) ? sourceProduct : productKey;
            return Results.Ok(await service.GetPermissionCatalogsAsync(
                tenantId,
                [effectiveProductKey],
                cancellationToken));
        })
        .WithName("IntegrationGetPermissionCatalogV1");

        integrations.MapPost("/evaluate", async (
            PermissionEvaluateRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            RoleManagementService service,
            CancellationToken cancellationToken) =>
        {
            ValidatePermissionEvaluateServiceToken(tokenValidator, context, request);
            return Results.Ok(await service.EvaluatePermissionAsync(request, cancellationToken));
        })
        .WithName("IntegrationEvaluatePermissionV1");
    }

    private static string ValidatePermissionCatalogReadServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());
        var preview = tokenValidator.TryValidate(bearer)
            ?? throw new StlApiException(
                "auth.service_token_invalid",
                "Service token is invalid.",
                401);

        var source = preview.SourceProductKey?.Trim().ToLowerInvariant();
        if (source is not "maintainarr"
            and not "routarr"
            and not "supplyarr"
            and not "trainarr"
            and not "compliancecore"
            and not "loadarr"
            and not "staffarr")
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for permission catalog reads.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = source,
                RequiredTargetProduct = "staffarr",
                TenantId = tenantId,
                RequiredActionScope = RoleManagementService.PermissionCatalogReadActionScope
            });

        return source;
    }

    private static void ValidatePermissionEvaluateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        PermissionEvaluateRequest request)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());
        var preview = tokenValidator.TryValidate(bearer)
            ?? throw new StlApiException(
                "auth.service_token_invalid",
                "Service token is invalid.",
                401);

        var source = preview.SourceProductKey?.Trim().ToLowerInvariant();
        var requestProduct = request.ProductKey.Trim().ToLowerInvariant();
        if (source is not "maintainarr"
            and not "routarr"
            and not "supplyarr"
            and not "trainarr"
            and not "compliancecore"
            and not "loadarr"
            and not "staffarr")
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for permission evaluation.",
                403);
        }

        if (!string.Equals(source, requestProduct, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product must match the requested product key.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = source,
                RequiredTargetProduct = "staffarr",
                TenantId = request.TenantId,
                RequiredActionScope = IntegrationEndpoints.PermissionCheckReadActionScope
            });
    }
}
