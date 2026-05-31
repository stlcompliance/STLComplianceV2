using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformAdminEndpoints
{
    public static void MapPlatformAdminEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string suffix)
        {
            group.MapGet("/dashboard", async (
            HttpContext context,
            PlatformAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetDashboardAsync(context.User, cancellationToken));
        })
        .WithName($"PlatformAdminDashboard{suffix}");

            group.MapGet("/launch-diagnostics", async (
            HttpContext context,
            PlatformAdminService service,
            Guid? tenantId,
            string? productKey,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetLaunchDiagnosticsAsync(
                context.User,
                tenantId,
                productKey,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"PlatformAdminLaunchDiagnostics{suffix}");

            group.MapGet("/launch-attempts", async (
            HttpContext context,
            PlatformAdminService service,
            Guid? tenantId,
            Guid? userId,
            string? productKey,
            Guid? correlationId,
            DateTimeOffset? fromUtc,
            DateTimeOffset? toUtc,
            string? result,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            var attempts = await service.GetLaunchAttemptsAsync(
                context.User,
                tenantId,
                userId,
                productKey,
                correlationId,
                fromUtc,
                toUtc,
                result,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken);
            return Results.Ok(attempts);
        })
        .WithName($"PlatformAdminLaunchAttempts{suffix}");

            group.MapGet("/overview/tenants", async (
            HttpContext context,
            PlatformAdminService service,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetTenantOverviewAsync(
                context.User,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"PlatformAdminTenantOverview{suffix}");

            group.MapGet("/overview/products", async (
            HttpContext context,
            PlatformAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetProductOverviewAsync(context.User, cancellationToken));
        })
        .WithName($"PlatformAdminProductOverview{suffix}");

            group.MapGet("/product-manifests", async (
            HttpContext context,
            ProductManifestService service,
            Guid? tenantId,
            string? productKey,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListAsync(
                context.User,
                tenantId,
                productKey,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"PlatformAdminProductManifests{suffix}");

            group.MapPost("/users", async (
            CreatePlatformUserRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var created = await service.CreateUserAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/platform-admin/users/{created.UserId}", created);
        })
        .WithName($"PlatformAdminCreateUser{suffix}");

            group.MapPost("/users/invite", async (
            InvitePlatformUserRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var invited = await service.InviteUserAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/platform-admin/users/{invited.UserId}", invited);
        })
        .WithName($"PlatformAdminInviteUser{suffix}");

            group.MapGet("/users", async (
            HttpContext context,
            PlatformUserAdminService service,
            string? search,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListUsersAsync(
                context.User,
                search,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken));
        })
        .WithName($"PlatformAdminListUsers{suffix}");

            group.MapGet("/users/{userId:guid}", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetUserAsync(context.User, userId, cancellationToken));
        })
        .WithName($"PlatformAdminGetUser{suffix}");

            group.MapGet("/users/{userId:guid}/access-history", async (
            Guid userId,
            HttpContext context,
            PlatformAdminService service,
            DateTimeOffset? fromUtc,
            DateTimeOffset? toUtc,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetUserAccessHistoryAsync(
                context.User,
                userId,
                fromUtc,
                toUtc,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken));
        })
        .WithName($"PlatformAdminUserAccessHistory{suffix}");

            group.MapGet("/users/{userId:guid}/login-history", async (
            Guid userId,
            HttpContext context,
            PlatformAdminService service,
            DateTimeOffset? fromUtc,
            DateTimeOffset? toUtc,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetUserLoginHistoryAsync(
                context.User,
                userId,
                fromUtc,
                toUtc,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken));
        })
        .WithName($"PlatformAdminUserLoginHistory{suffix}");

            group.MapGet("/users/{userId:guid}/launch-history", async (
            Guid userId,
            HttpContext context,
            PlatformAdminService service,
            DateTimeOffset? fromUtc,
            DateTimeOffset? toUtc,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetUserLaunchHistoryAsync(
                context.User,
                userId,
                fromUtc,
                toUtc,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken));
        })
        .WithName($"PlatformAdminUserLaunchHistory{suffix}");

            group.MapGet("/users/{userId:guid}/identity-audit-history", async (
            Guid userId,
            HttpContext context,
            PlatformAdminService service,
            DateTimeOffset? fromUtc,
            DateTimeOffset? toUtc,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetUserIdentityAuditHistoryAsync(
                context.User,
                userId,
                fromUtc,
                toUtc,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken));
        })
        .WithName($"PlatformAdminUserIdentityAuditHistory{suffix}");

            group.MapPatch("/users/{userId:guid}", async (
            Guid userId,
            UpdatePlatformUserRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpdateUserAsync(context.User, userId, request, cancellationToken));
        })
        .WithName($"PlatformAdminUpdateUser{suffix}");

            group.MapPost("/users/{userId:guid}/enable", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.EnableUserAsync(context.User, userId, cancellationToken));
        })
        .WithName($"PlatformAdminEnableUser{suffix}");

            group.MapPost("/users/{userId:guid}/disable", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var confirmationToken = context.Request.Headers["X-Admin-Confirm"].ToString();
            return Results.Ok(await service.DisableUserAsync(context.User, userId, confirmationToken, cancellationToken));
        })
        .WithName($"PlatformAdminDisableUser{suffix}");

            group.MapPost("/users/{userId:guid}/lock", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var confirmationToken = context.Request.Headers["X-Admin-Confirm"].ToString();
            return Results.Ok(await service.LockUserAsync(context.User, userId, confirmationToken, cancellationToken));
        })
        .WithName($"PlatformAdminLockUser{suffix}");

            group.MapPost("/users/{userId:guid}/unlock", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var confirmationToken = context.Request.Headers["X-Admin-Confirm"].ToString();
            return Results.Ok(await service.UnlockUserAsync(context.User, userId, confirmationToken, cancellationToken));
        })
        .WithName($"PlatformAdminUnlockUser{suffix}");

            group.MapPost("/users/{userId:guid}/reset-password", async (
            Guid userId,
            AdminResetUserPasswordRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var confirmationToken = context.Request.Headers["X-Admin-Confirm"].ToString();
            return Results.Ok(await service.ResetUserPasswordAsync(context.User, userId, request, confirmationToken, cancellationToken));
        })
        .WithName($"PlatformAdminResetUserPassword{suffix}");

            group.MapPost("/users/{userId:guid}/mfa", async (
            Guid userId,
            SetPlatformUserMfaRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var confirmationToken = context.Request.Headers["X-Admin-Confirm"].ToString();
            return Results.Ok(await service.SetUserMfaAsync(context.User, userId, request, confirmationToken, cancellationToken));
        })
        .WithName($"PlatformAdminSetUserMfa{suffix}");

            group.MapGet("/users/{userId:guid}/tenant-memberships", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListTenantMembershipsAsync(context.User, userId, cancellationToken));
        })
        .WithName($"PlatformAdminListUserTenantMemberships{suffix}");

            group.MapGet("/users/{userId:guid}/sessions", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListUserSessionsAsync(context.User, userId, cancellationToken));
        })
        .WithName($"PlatformAdminListUserSessions{suffix}");

            group.MapPost("/users/{userId:guid}/sessions/{sessionId:guid}/revoke", async (
            Guid userId,
            Guid sessionId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RevokeUserSessionAsync(context.User, userId, sessionId, cancellationToken));
        })
        .WithName($"PlatformAdminRevokeUserSession{suffix}");

            group.MapPost("/users/{userId:guid}/tenant-memberships", async (
            Guid userId,
            AssignPlatformUserTenantMembershipRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.AssignTenantMembershipAsync(context.User, userId, request, cancellationToken));
        })
        .WithName($"PlatformAdminAssignUserTenantMembership{suffix}");

            group.MapDelete("/users/{userId:guid}/tenant-memberships/{tenantId:guid}", async (
            Guid userId,
            Guid tenantId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RemoveTenantMembershipAsync(context.User, userId, tenantId, cancellationToken));
        })
        .WithName($"PlatformAdminRemoveUserTenantMembership{suffix}");

            group.MapGet("/users/{userId:guid}/roles", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListRolesAsync(context.User, userId, cancellationToken));
        })
        .WithName($"PlatformAdminListUserRoles{suffix}");

            group.MapPost("/users/{userId:guid}/roles", async (
            Guid userId,
            AssignPlatformUserRoleRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var confirmationToken = context.Request.Headers["X-Admin-Confirm"].ToString();
            return Results.Ok(await service.AssignRoleAsync(
                context.User,
                userId,
                request,
                confirmationToken,
                cancellationToken));
        })
        .WithName($"PlatformAdminAssignUserRole{suffix}");

            group.MapDelete("/users/{userId:guid}/roles/{roleKey}", async (
            Guid userId,
            string roleKey,
            Guid? tenantId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var confirmationToken = context.Request.Headers["X-Admin-Confirm"].ToString();
            return Results.Ok(await service.RemoveRoleAsync(
                context.User,
                userId,
                roleKey,
                tenantId,
                confirmationToken,
                cancellationToken));
        })
        .WithName($"PlatformAdminRemoveUserRole{suffix}");

            group.MapGet("/users/{userId:guid}/external-identity-mappings", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListExternalIdentityProviderMappingsAsync(context.User, userId, cancellationToken));
        })
        .WithName($"PlatformAdminListUserExternalIdentityMappings{suffix}");

            group.MapPut("/users/{userId:guid}/external-identity-mappings", async (
            Guid userId,
            UpsertPlatformUserExternalIdentityProviderMappingRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpsertExternalIdentityProviderMappingAsync(context.User, userId, request, cancellationToken));
        })
        .WithName($"PlatformAdminUpsertUserExternalIdentityMapping{suffix}");

            group.MapDelete("/users/{userId:guid}/external-identity-mappings/{mappingId:guid}", async (
            Guid userId,
            Guid mappingId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RemoveExternalIdentityProviderMappingAsync(context.User, userId, mappingId, cancellationToken));
        })
        .WithName($"PlatformAdminRemoveUserExternalIdentityMapping{suffix}");
        }

        MapRoutes(
            app.MapGroup("/api/platform-admin")
                .WithTags("PlatformAdmin")
                .RequireAuthorization(),
            string.Empty);

        MapRoutes(
            app.MapGroup("/api/v1/platform-admin")
                .WithTags("PlatformAdmin")
                .RequireAuthorization(),
            "V1");
    }
}
