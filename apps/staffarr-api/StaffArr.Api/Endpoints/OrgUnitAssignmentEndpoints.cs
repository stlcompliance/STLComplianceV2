using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class OrgUnitAssignmentEndpoints
{
    public static void MapStaffArrOrgUnitAssignmentEndpoints(this WebApplication app)
    {
        var assignments = app.MapGroup("/api/people/{personId:guid}/org-assignments")
            .WithTags("OrgUnitAssignments")
            .RequireAuthorization();

        assignments.MapGet("/", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            OrgUnitAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListByPersonAsync(tenantId, personId, cancellationToken));
        })
        .WithName("ListPersonOrgAssignments");

        assignments.MapPost("/", async (
            Guid personId,
            CreateOrgUnitAssignmentRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            OrgUnitAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, personId, request, cancellationToken);
            return Results.Created($"/api/people/{personId}/org-assignments/{created.AssignmentId}", created);
        })
        .WithName("CreatePersonOrgAssignment");

        assignments.MapPut("/{assignmentId:guid}", async (
            Guid personId,
            Guid assignmentId,
            UpdateOrgUnitAssignmentRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            OrgUnitAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateAsync(
                tenantId,
                actorUserId,
                personId,
                assignmentId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdatePersonOrgAssignment");

        assignments.MapPatch("/{assignmentId:guid}/status", async (
            Guid personId,
            Guid assignmentId,
            UpdateOrgUnitAssignmentStatusRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            OrgUnitAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateStatusAsync(
                tenantId,
                actorUserId,
                personId,
                assignmentId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdatePersonOrgAssignmentStatus");
    }
}
