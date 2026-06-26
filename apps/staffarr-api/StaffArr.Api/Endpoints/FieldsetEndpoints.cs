using StaffArr.Api.Services;

namespace StaffArr.Api.Endpoints;

public static class FieldsetEndpoints
{
    public static void MapStaffArrFieldsetEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/fieldsets").WithTags("Fieldsets").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/fieldsets").WithTags("Fieldsets").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/employment-applications/builder", (
            HttpContext context,
            StaffArrAuthorizationService authorization) =>
        {
            authorization.RequireTenantSettingsView(context.User);
            return Results.Ok(StaffArrControlledFieldCatalog.GetEmploymentApplicationBuilderCatalog());
        })
        .WithName($"GetStaffArrEmploymentApplicationBuilderCatalog{nameSuffix}");

        group.MapGet("/people/profile", (
            HttpContext context,
            StaffArrAuthorizationService authorization) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(StaffArrControlledFieldCatalog.GetPeopleProfileFieldset());
        })
        .WithName($"GetStaffArrPeopleProfileFieldset{nameSuffix}");

        group.MapGet("/personnel-incidents/create", (
            HttpContext context,
            StaffArrAuthorizationService authorization) =>
        {
            authorization.RequireIncidentsManageWrite(context.User);
            return Results.Ok(StaffArrControlledFieldCatalog.GetPersonnelIncidentCreateFieldset());
        })
        .WithName($"GetStaffArrPersonnelIncidentCreateFieldset{nameSuffix}");

        group.MapGet("/hrm/{module}", (
            string module,
            HttpContext context,
            StaffArrAuthorizationService authorization) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(StaffArrControlledFieldCatalog.GetHrmProgramFieldset(module));
        })
        .WithName($"GetStaffArrHrmFieldset{nameSuffix}");
    }
}
