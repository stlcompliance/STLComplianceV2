using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class RoleTemplateEndpoints
{
    public static void MapStaffArrRoleTemplateEndpoints(this WebApplication app)
    {
        var roleTemplates = app.MapGroup("/api/roles")
            .WithTags("RoleTemplates")
            .RequireAuthorization();

        roleTemplates.MapGet("/", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListRoleTemplatesAsync(tenantId, cancellationToken));
        })
        .WithName("ListRoleTemplates");

        roleTemplates.MapPost("/", async (
            CreateRoleTemplateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateRoleTemplateAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"/api/roles/{created.RoleTemplateId}", created);
        })
        .WithName("CreateRoleTemplate");

        roleTemplates.MapPut("/{roleTemplateId:guid}", async (
            Guid roleTemplateId,
            UpdateRoleTemplateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateRoleTemplateAsync(
                tenantId,
                actorUserId,
                roleTemplateId,
                request,
                cancellationToken));
        })
        .WithName("UpdateRoleTemplate");

        var permissionTemplates = app.MapGroup("/api/permissions")
            .WithTags("PermissionTemplates")
            .RequireAuthorization();

        permissionTemplates.MapGet("/", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPermissionTemplatesAsync(tenantId, cancellationToken));
        })
        .WithName("ListPermissionTemplates");

        permissionTemplates.MapPost("/", async (
            UpsertPermissionTemplateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertPermissionTemplateAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertPermissionTemplate");

        var personRoleAssignments = app.MapGroup("/api/people/{personId:guid}/role-assignments")
            .WithTags("PersonRoleAssignments")
            .RequireAuthorization();

        personRoleAssignments.MapGet("/", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPersonRoleAssignmentsAsync(tenantId, personId, cancellationToken));
        })
        .WithName("ListPersonRoleAssignments");

        personRoleAssignments.MapPost("/", async (
            Guid personId,
            CreatePersonRoleAssignmentRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreatePersonRoleAssignmentAsync(
                tenantId,
                actorUserId,
                personId,
                request,
                cancellationToken);
            return Results.Created($"/api/people/{personId}/role-assignments/{created.AssignmentId}", created);
        })
        .WithName("CreatePersonRoleAssignment");

        personRoleAssignments.MapPatch("/{assignmentId:guid}/status", async (
            Guid personId,
            Guid assignmentId,
            UpdatePersonRoleAssignmentStatusRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdatePersonRoleAssignmentStatusAsync(
                tenantId,
                actorUserId,
                personId,
                assignmentId,
                request,
                cancellationToken));
        })
        .WithName("UpdatePersonRoleAssignmentStatus");
    }
}
