using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class PersonLookupEndpoints
{
    public static void MapStaffArrPersonLookupEndpoints(this WebApplication app)
    {
        var lookupGroups = new[]
        {
            (Route: "/api/person-lookup", Suffix: string.Empty),
            (Route: "/api/v1/person-lookup", Suffix: "V1")
        };

        foreach (var (route, suffix) in lookupGroups)
        {
            var lookup = app.MapGroup(route)
                .WithTags("PersonLookup")
                .RequireAuthorization();

            lookup.MapGet("/", async (
                Guid? personId,
                string? email,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PersonLookupService service,
                CancellationToken cancellationToken) =>
            {
                if (personId is null && string.IsNullOrWhiteSpace(email))
                {
                    return Results.BadRequest(new
                    {
                        code = "person_lookup.validation",
                        message = "Provide personId or email query parameter."
                    });
                }

                if (personId is Guid requestedPersonId)
                {
                    if (requestedPersonId == Guid.Empty)
                    {
                        return Results.BadRequest(new
                        {
                            code = "person_lookup.validation",
                            message = "personId must be a valid identifier."
                        });
                    }

                    authorization.RequirePersonLookupRead(context.User, requestedPersonId);
                }
                else
                {
                    authorization.RequirePeopleRead(context.User);
                }

                var tenantId = context.User.GetTenantId();
                var result = personId is Guid personGuid
                    ? await service.GetByPersonIdAsync(tenantId, personGuid, cancellationToken)
                    : await service.GetByEmailAsync(tenantId, email!, cancellationToken);

                if (personId is null)
                {
                    authorization.RequirePersonLookupRead(context.User, result.PersonId);
                }

                return Results.Ok(result);
            })
            .WithName($"GetPersonLookupByQuery{suffix}");
        }

        var personLookupGroups = new[]
        {
            (Route: "/api/people/{personId:guid}/lookup", Suffix: string.Empty),
            (Route: "/api/v1/people/{personId:guid}/lookup", Suffix: "V1")
        };

        foreach (var (route, suffix) in personLookupGroups)
        {
            var personLookup = app.MapGroup(route)
                .WithTags("PersonLookup")
                .RequireAuthorization();

            personLookup.MapGet("/", async (
                Guid personId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PersonLookupService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePersonLookupRead(context.User, personId);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetByPersonIdAsync(tenantId, personId, cancellationToken));
            })
            .WithName($"GetPersonLookup{suffix}");
        }
    }
}
