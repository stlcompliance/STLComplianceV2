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

        people.MapGet("/{personId:guid}/trainarr-training-history", async (
            Guid personId,
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            TrainarrPersonTrainingHistoryService trainarrHistoryService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonHistoryRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await trainarrHistoryService.GetForPersonAsync(
                tenantId,
                actorUserId,
                personId,
                limit,
                cancellationToken));
        })
        .WithName("GetStaffPersonTrainarrTrainingHistory");

        people.MapPost("/", async (
            CreateStaffPersonRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/people/{created.PersonId}", created);
        })
        .WithName("CreateStaffPerson");

        people.MapPut("/{personId:guid}", async (
            Guid personId,
            UpdateStaffPersonRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                personId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpdateStaffPerson");

        people.MapPatch("/{personId:guid}/employment-status", async (
            Guid personId,
            UpdatePersonEmploymentStatusRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateEmploymentStatusAsync(
                tenantId,
                personId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpdateStaffPersonEmploymentStatus");

        people.MapPost("/import", async (
            BulkPersonImportRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleBulkImportService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ImportAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("BulkImportStaffPeople");
    }
}
