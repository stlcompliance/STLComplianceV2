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

            definitions.MapPost("/", async (
                UpsertCertificationDefinitionRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                CertificationService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireCertificationManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.UpsertDefinitionAsync(tenantId, actorUserId, request, cancellationToken);
                return Results.Ok(created);
            })
            .WithName($"UpsertCertificationDefinition{suffix}");
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

        personCertifications.MapPost("/", async (
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
            return Results.Created($"/api/people/{personId}/certifications/{created.PersonCertificationId}", created);
        })
        .WithName("GrantPersonCertification");

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
        .WithName("UpdatePersonCertification");
    }
}
