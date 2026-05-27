using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Endpoints;

public static class PeopleEndpoints
{
    public static void MapStaffArrPeopleEndpoints(this WebApplication app)
    {
        var people = app.MapGroup("/api/people").WithTags("People").RequireAuthorization();

        people.MapGet("/", async (
            string? query,
            Guid? orgUnitId,
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await service.ListAsync(tenantId, query, orgUnitId, limit ?? 50, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListStaffPeople");

        people.MapGet("/{personId:guid}", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStaffArrEntitlement(context.User);
            var tenantId = context.User.GetTenantId();
            var currentPersonId = context.User.GetPersonId();
            if (currentPersonId != personId)
            {
                authorization.RequirePeopleRead(context.User);
            }

            return Results.Ok(await service.GetByIdAsync(tenantId, personId, cancellationToken));
        })
        .WithName("GetStaffPerson");

        people.MapGet("/{personId:guid}/timeline", async (
            Guid personId,
            int? page,
            int? pageSize,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonTimelineService timelineService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonTimelineRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await timelineService.ListPersonTimelineAsync(
                tenantId,
                personId,
                page ?? 1,
                pageSize ?? 50,
                cancellationToken));
        })
        .WithName("GetPersonTimeline");

        people.MapPost("/", async (
            CreateStaffPersonRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(tenantId, request, cancellationToken);
            return Results.Created($"/api/people/{created.PersonId}", created);
        })
        .WithName("CreateStaffPerson");
    }
}
