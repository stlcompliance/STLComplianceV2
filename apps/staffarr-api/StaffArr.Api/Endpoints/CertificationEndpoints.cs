using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class CertificationEndpoints
{
    public static void MapStaffArrCertificationEndpoints(this WebApplication app)
    {
        var definitionRoutes = new[]
        {
            (Route: "/api/certifications", Suffix: string.Empty),
            (Route: "/api/v1/certifications", Suffix: "V1")
        };

        foreach (var (route, suffix) in definitionRoutes)
        {
            var definitions = app.MapGroup(route)
                .WithTags("Certifications")
                .RequireAuthorization();

            definitions.MapGet("/", async (
                HttpContext context,
                StaffArrAuthorizationService authorization,
                CertificationService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireCertificationRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListDefinitionsAsync(tenantId, cancellationToken));
            })
            .WithName($"ListCertificationDefinitions{suffix}");
        }

        var personCertifications = app.MapGroup("/api/people/{personId:guid}/certifications")
            .WithTags("PersonCertifications")
            .RequireAuthorization();

        personCertifications.MapGet("/", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            CertificationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireCertificationRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPersonCertificationsAsync(tenantId, personId, cancellationToken));
        })
        .WithName("ListPersonCertifications");
    }
}
