using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class PersonLookupEndpoints
{
    public static void MapStaffArrPersonLookupEndpoints(this WebApplication app)
    {
        var lookup = app.MapGroup("/api/person-lookup")
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
        .WithName("GetPersonLookupByQuery");

        var personLookup = app.MapGroup("/api/people/{personId:guid}/lookup")
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
        .WithName("GetPersonLookup");
    }
}
