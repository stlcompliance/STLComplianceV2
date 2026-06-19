using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class RecruitingEndpoints
{
    public static void MapStaffArrHiringEndpoints(this WebApplication app)
    {
        var hiring = app.MapGroup("/api/v1/hiring").WithTags("Hiring").RequireAuthorization();

        hiring.MapGet("/requisitions", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListRequisitionsAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName("ListStaffArrHiringRequisitionsV1");

        hiring.MapPost("/requisitions", async (
            UpsertRecruitingRequisitionRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var created = await service.UpsertRequisitionAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                null,
                request,
                cancellationToken);
            return Results.Created($"/api/v1/hiring/requisitions/{created.Id}", created);
        })
        .WithName("CreateStaffArrHiringRequisitionV1");

        hiring.MapPatch("/requisitions/{requisitionId:guid}", async (
            Guid requisitionId,
            UpsertRecruitingRequisitionRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertRequisitionAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                requisitionId,
                request,
                cancellationToken));
        })
        .WithName("UpdateStaffArrHiringRequisitionV1");

        hiring.MapPost("/requisitions/{requisitionId:guid}/archive", async (
            Guid requisitionId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.ArchiveRequisitionAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                requisitionId,
                cancellationToken));
        })
        .WithName("ArchiveStaffArrHiringRequisitionV1");

        hiring.MapGet("/candidates", async (
            Guid? requisitionId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListCandidatesAsync(context.User.GetTenantId(), requisitionId, cancellationToken));
        })
        .WithName("ListStaffArrHiringCandidatesV1");

        hiring.MapPost("/candidates", async (
            UpsertRecruitingCandidateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var created = await service.UpsertCandidateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                null,
                request,
                cancellationToken);
            return Results.Created($"/api/v1/hiring/candidates/{created.Id}", created);
        })
        .WithName("CreateStaffArrHiringCandidateV1");

        hiring.MapPatch("/candidates/{candidateId:guid}", async (
            Guid candidateId,
            UpsertRecruitingCandidateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertCandidateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                candidateId,
                request,
                cancellationToken));
        })
        .WithName("UpdateStaffArrHiringCandidateV1");

        hiring.MapPost("/candidates/{candidateId:guid}/archive", async (
            Guid candidateId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.ArchiveCandidateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                candidateId,
                cancellationToken));
        })
        .WithName("ArchiveStaffArrHiringCandidateV1");

        hiring.MapPost("/submissions/{submissionId:guid}/candidates", async (
            Guid submissionId,
            Guid? requisitionId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var created = await service.ConvertSubmissionToCandidateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                submissionId,
                requisitionId,
                cancellationToken);
            return Results.Ok(created);
        })
        .WithName("CreateStaffArrHiringCandidateFromSubmissionV1");

        hiring.MapPost("/candidates/{candidateId:guid}/hire", async (
            Guid candidateId,
            CreateStaffPersonRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var hired = await service.HireCandidateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                candidateId,
                request,
                cancellationToken);
            return Results.Ok(hired);
        })
        .WithName("HireStaffArrCandidateV1");

        hiring.MapGet("/candidates/{candidateId:guid}/interview-stages", async (
            Guid candidateId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListInterviewStagesAsync(context.User.GetTenantId(), candidateId, cancellationToken));
        })
        .WithName("ListStaffArrHiringInterviewStagesV1");

        hiring.MapPost("/interview-stages", async (
            UpsertRecruitingInterviewStageRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var created = await service.UpsertInterviewStageAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                null,
                request,
                cancellationToken);
            return Results.Created($"/api/v1/hiring/interview-stages/{created.Id}", created);
        })
        .WithName("CreateStaffArrHiringInterviewStageV1");

        hiring.MapPatch("/interview-stages/{stageId:guid}", async (
            Guid stageId,
            UpsertRecruitingInterviewStageRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertInterviewStageAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                stageId,
                request,
                cancellationToken));
        })
        .WithName("UpdateStaffArrHiringInterviewStageV1");

        hiring.MapPost("/interview-stages/{stageId:guid}/archive", async (
            Guid stageId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.ArchiveInterviewStageAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                stageId,
                cancellationToken));
        })
        .WithName("ArchiveStaffArrHiringInterviewStageV1");

        hiring.MapGet("/offers", async (
            Guid? candidateId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListOffersAsync(context.User.GetTenantId(), candidateId, cancellationToken));
        })
        .WithName("ListStaffArrHiringOffersV1");

        hiring.MapPost("/offers", async (
            UpsertRecruitingOfferRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var created = await service.UpsertOfferAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                null,
                request,
                cancellationToken);
            return Results.Created($"/api/v1/hiring/offers/{created.Id}", created);
        })
        .WithName("CreateStaffArrHiringOfferV1");

        hiring.MapPatch("/offers/{offerId:guid}", async (
            Guid offerId,
            UpsertRecruitingOfferRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertOfferAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                offerId,
                request,
                cancellationToken));
        })
        .WithName("UpdateStaffArrHiringOfferV1");

        hiring.MapPost("/offers/{offerId:guid}/archive", async (
            Guid offerId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.ArchiveOfferAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                offerId,
                cancellationToken));
        })
        .WithName("ArchiveStaffArrHiringOfferV1");
    }
}
