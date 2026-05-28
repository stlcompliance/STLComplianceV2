using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class PersonnelHistoryEndpoints
{
    public static void MapStaffArrPersonnelHistoryEndpoints(this WebApplication app)
    {
        var history = app.MapGroup("/api/person-history")
            .WithTags("PersonnelHistory")
            .RequireAuthorization();

        history.MapGet("/", async (
            Guid personId,
            int? page,
            int? pageSize,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelHistoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonHistoryRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPersonHistoryAsync(
                tenantId,
                personId,
                page ?? 1,
                pageSize ?? 50,
                cancellationToken));
        })
        .WithName("ListPersonHistoryByQuery");

        history.MapGet("/summary", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelHistoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonHistoryRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, personId, cancellationToken));
        })
        .WithName("GetPersonHistorySummaryByQuery");

        var nested = app.MapGroup("/api/people/{personId:guid}/person-history")
            .WithTags("PersonnelHistory")
            .RequireAuthorization();

        nested.MapGet("/", async (
            Guid personId,
            int? page,
            int? pageSize,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelHistoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonHistoryRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPersonHistoryAsync(
                tenantId,
                personId,
                page ?? 1,
                pageSize ?? 50,
                cancellationToken));
        })
        .WithName("ListPersonHistoryNested");

        nested.MapGet("/summary", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelHistoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonHistoryRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, personId, cancellationToken));
        })
        .WithName("GetPersonHistorySummaryNested");
    }
}
