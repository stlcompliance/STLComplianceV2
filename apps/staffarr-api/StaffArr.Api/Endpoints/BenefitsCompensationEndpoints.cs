using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class BenefitsCompensationEndpoints
{
    public static void MapStaffArrBenefitsCompensationEndpoints(this WebApplication app)
    {
        var benefits = app.MapGroup("/api/v1/benefits-compensation").WithTags("Benefits and compensation").RequireAuthorization();

        benefits.MapGet("/enrollments", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListBenefitEnrollmentsAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        benefits.MapPost("/enrollments", async (UpsertBenefitEnrollmentRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertBenefitEnrollmentAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        benefits.MapPatch("/enrollments/{id:guid}", async (Guid id, UpsertBenefitEnrollmentRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertBenefitEnrollmentAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        benefits.MapGet("/dependents", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListDependentsAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        benefits.MapPost("/dependents", async (UpsertBenefitDependentRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertDependentAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        benefits.MapPatch("/dependents/{id:guid}", async (Guid id, UpsertBenefitDependentRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertDependentAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        benefits.MapGet("/beneficiaries", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListBeneficiariesAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        benefits.MapPost("/beneficiaries", async (UpsertBenefitBeneficiaryRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertBeneficiaryAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        benefits.MapPatch("/beneficiaries/{id:guid}", async (Guid id, UpsertBenefitBeneficiaryRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertBeneficiaryAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        benefits.MapGet("/compensation-profiles", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListCompensationProfilesAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        benefits.MapPost("/compensation-profiles", async (UpsertCompensationProfileRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertCompensationProfileAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        benefits.MapPatch("/compensation-profiles/{id:guid}", async (Guid id, UpsertCompensationProfileRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertCompensationProfileAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        benefits.MapGet("/change-requests", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListCompensationChangeRequestsAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        benefits.MapPost("/change-requests", async (CreateCompensationChangeRequestRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.CreateCompensationChangeRequestAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken));
        });
        benefits.MapPost("/change-requests/{id:guid}/review", async (Guid id, ReviewCompensationChangeRequest request, HttpContext context, StaffArrAuthorizationService authorization, BenefitsCompensationService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.ReviewCompensationChangeRequestAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });
    }
}
