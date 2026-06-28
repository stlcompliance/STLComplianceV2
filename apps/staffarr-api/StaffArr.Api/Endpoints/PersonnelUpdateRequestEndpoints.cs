using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class PersonnelUpdateRequestEndpoints
{
    public static void MapStaffArrPersonnelUpdateRequestEndpoints(this WebApplication app)
    {
        var requests = app.MapGroup("/api/personnel-update-requests")
            .WithTags("PersonnelUpdateRequests")
            .RequireAuthorization();

        requests.MapGet("/", async (
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelUpdateRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPendingAsync(tenantId, limit ?? 50, cancellationToken));
        })
        .WithName("StaffArrListPendingPersonnelUpdateRequests");

        requests.MapGet("/{requestId:guid}", async (
            Guid requestId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelUpdateRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetByIdAsync(tenantId, requestId, cancellationToken));
        })
        .WithName("StaffArrGetPersonnelUpdateRequest");

        requests.MapPost("/{requestId:guid}/review", async (
            Guid requestId,
            ReviewPersonnelUpdateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelUpdateRequestService service,
            ManagerHierarchyService managerHierarchy,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStaffArrLaunchContext(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var existing = await service.GetByIdAsync(tenantId, requestId, cancellationToken);
            await authorization.RequirePersonnelUpdateRequestReviewAsync(
                context.User,
                existing.PersonId,
                managerHierarchy,
                cancellationToken);

            var reviewed = await service.ReviewAsync(
                tenantId,
                requestId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(reviewed);
        })
        .WithName("StaffArrReviewPersonnelUpdateRequest");
    }
}
