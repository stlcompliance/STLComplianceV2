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

            definitions.MapGet(string.Empty, async (
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

        var personCertificationRoutes = new[]
        {
            (Route: "/api/people/{personId:guid}/certifications", Suffix: string.Empty),
            (Route: "/api/v1/people/{personId:guid}/certifications", Suffix: "V1")
        };

        foreach (var (route, suffix) in personCertificationRoutes)
        {
            var personCertifications = app.MapGroup(route)
                .WithTags("PersonCertifications")
                .RequireAuthorization();

            personCertifications.MapGet(string.Empty, async (
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
            .WithName($"ListPersonCertifications{suffix}");

            personCertifications.MapPost(string.Empty, async (
                Guid personId,
                GrantPersonCertificationRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                CertificationService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireCertificationManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.GrantManualCertificationAsync(
                    tenantId,
                    actorUserId,
                    personId,
                    request,
                    cancellationToken);
                return Results.Created($"{route}/{created.PersonCertificationId}", created);
            })
            .WithName($"GrantPersonCertification{suffix}");

            personCertifications.MapPatch("/{personCertificationId:guid}", async (
                Guid personId,
                Guid personCertificationId,
                UpdatePersonCertificationRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                CertificationService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireCertificationManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.UpdatePersonCertificationAsync(
                    tenantId,
                    actorUserId,
                    personId,
                    personCertificationId,
                    request,
                    cancellationToken));
            })
            .WithName($"UpdatePersonCertification{suffix}");
        }
    }
}
