using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class MePortalEndpoints
{
    public static void MapStaffArrMePortalEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/me"), string.Empty, "/api/me");
        MapRoutes(app.MapGroup("/api/v1/me"), "V1", "/api/v1/me");
    }

    private static void MapRoutes(RouteGroupBuilder me, string nameSuffix, string pathPrefix)
    {
        me = me.WithTags("Me").RequireAuthorization();

        me.MapGet("/portal", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            MePortalService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSelfServicePortalAccess(context.User);
            return Results.Ok(await service.GetSummaryAsync(context.User, cancellationToken));
        })
        .WithName($"StaffArrGetMePortalSummary{nameSuffix}");

        me.MapGet("/subordinates", async (
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ManagerHierarchyService managerHierarchyService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSelfServicePortalAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var personId = context.User.GetPersonId();
            return Results.Ok(await managerHierarchyService.GetSubordinatesAsync(
                tenantId,
                personId,
                includeIndirect: false,
                limit ?? 50,
                cancellationToken));
        })
        .WithName($"StaffArrListMyDirectReports{nameSuffix}");

        me.MapGet("/update-requests", async (
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelUpdateRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSelfServicePortalAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var personId = context.User.GetPersonId();
            return Results.Ok(await service.ListForPersonAsync(
                tenantId,
                personId,
                limit ?? 25,
                cancellationToken));
        })
        .WithName($"StaffArrListMyPersonnelUpdateRequests{nameSuffix}");

        me.MapPost("/update-requests", async (
            SubmitPersonnelUpdateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelUpdateRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSelfServicePortalAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var personId = context.User.GetPersonId();
            var actorUserId = context.User.GetUserId();
            var created = await service.SubmitAsync(
                tenantId,
                personId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"{pathPrefix}/update-requests/{created.RequestId}", created);
        })
        .WithName($"StaffArrSubmitPersonnelUpdateRequest{nameSuffix}");

        me.MapGet("/update-requests/{requestId:guid}", async (
            Guid requestId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelUpdateRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSelfServicePortalAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var personId = context.User.GetPersonId();
            var record = await service.GetByIdAsync(tenantId, requestId, cancellationToken);
            if (record.PersonId != personId)
            {
                authorization.RequirePeopleRead(context.User);
            }

            return Results.Ok(record);
        })
        .WithName($"StaffArrGetMyPersonnelUpdateRequest{nameSuffix}");

        me.MapGet("/team", async (
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            MyTeamService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireManagerTeamAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var personId = context.User.GetPersonId();
            return Results.Ok(await service.GetDashboardAsync(
                tenantId,
                personId,
                limit ?? 50,
                cancellationToken));
        })
        .WithName($"StaffArrGetMyTeamDashboard{nameSuffix}");

        me.MapPost("/team/update-requests/{requestId:guid}/review", async (
            Guid requestId,
            ReviewPersonnelUpdateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelUpdateRequestService service,
            ManagerHierarchyService managerHierarchy,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireManagerTeamAccess(context.User);
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
        .WithName($"StaffArrReviewMyTeamPersonnelUpdateRequest{nameSuffix}");

        me.MapGet("/incidents", async (
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSelfServicePortalAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var personId = context.User.GetPersonId();
            var incidents = await service.ListIncidentsAsync(tenantId, personId, cancellationToken);
            if (limit is > 0 and var take)
            {
                incidents = incidents.Take(take).ToList();
            }

            return Results.Ok(incidents);
        })
        .WithName($"StaffArrListMyPersonnelIncidents{nameSuffix}");

        me.MapPost("/incidents", async (
            SubmitSelfReportedPersonnelIncidentRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSelfServicePortalAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var personId = context.User.GetPersonId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateSelfReportAsync(
                tenantId,
                personId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"{pathPrefix}/incidents/{created.IncidentId}", created);
        })
        .WithName($"StaffArrSubmitSelfReportedPersonnelIncident{nameSuffix}");

        me.MapGet("/incidents/{incidentId:guid}", async (
            Guid incidentId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSelfServicePortalAccess(context.User);
            var tenantId = context.User.GetTenantId();
            var personId = context.User.GetPersonId();
            var detail = await service.GetIncidentAsync(tenantId, incidentId, cancellationToken);
            if (detail.PersonId != personId)
            {
                authorization.RequireIncidentsRead(context.User, detail.PersonId);
            }

            return Results.Ok(detail);
        })
        .WithName($"StaffArrGetMyPersonnelIncident{nameSuffix}");
    }
}
