using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class RecruitingEndpoints
{
    public static void MapStaffArrRecruitingEndpoints(this WebApplication app)
    {
        var recruiting = app.MapGroup("/api/v1/recruiting").WithTags("Recruiting").RequireAuthorization();

        recruiting.MapGet("/requisitions", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListRequisitionsAsync(context.User.GetTenantId(), cancellationToken));
        })
        .WithName("ListStaffArrRecruitingRequisitionsV1");

        recruiting.MapPost("/requisitions", async (
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
            return Results.Created($"/api/v1/recruiting/requisitions/{created.Id}", created);
        })
        .WithName("CreateStaffArrRecruitingRequisitionV1");

        recruiting.MapPatch("/requisitions/{requisitionId:guid}", async (
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
        .WithName("UpdateStaffArrRecruitingRequisitionV1");

        recruiting.MapPost("/requisitions/{requisitionId:guid}/archive", async (
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
        .WithName("ArchiveStaffArrRecruitingRequisitionV1");

        recruiting.MapGet("/candidates", async (
            Guid? requisitionId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListCandidatesAsync(context.User.GetTenantId(), requisitionId, cancellationToken));
        })
        .WithName("ListStaffArrRecruitingCandidatesV1");

        recruiting.MapPost("/candidates", async (
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
            return Results.Created($"/api/v1/recruiting/candidates/{created.Id}", created);
        })
        .WithName("CreateStaffArrRecruitingCandidateV1");

        recruiting.MapPatch("/candidates/{candidateId:guid}", async (
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
        .WithName("UpdateStaffArrRecruitingCandidateV1");

        recruiting.MapPost("/candidates/{candidateId:guid}/archive", async (
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
        .WithName("ArchiveStaffArrRecruitingCandidateV1");

        recruiting.MapPost("/candidates/from-submission/{submissionId:guid}", async (
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
        .WithName("ConvertStaffArrRecruitingApplicationSubmissionV1");

        recruiting.MapGet("/candidates/{candidateId:guid}/interview-stages", async (
            Guid candidateId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListInterviewStagesAsync(context.User.GetTenantId(), candidateId, cancellationToken));
        })
        .WithName("ListStaffArrRecruitingInterviewStagesV1");

        recruiting.MapPost("/interview-stages", async (
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
            return Results.Created($"/api/v1/recruiting/interview-stages/{created.Id}", created);
        })
        .WithName("CreateStaffArrRecruitingInterviewStageV1");

        recruiting.MapPatch("/interview-stages/{stageId:guid}", async (
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
        .WithName("UpdateStaffArrRecruitingInterviewStageV1");

        recruiting.MapPost("/interview-stages/{stageId:guid}/archive", async (
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
        .WithName("ArchiveStaffArrRecruitingInterviewStageV1");

        recruiting.MapGet("/offers", async (
            Guid? candidateId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RecruitingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListOffersAsync(context.User.GetTenantId(), candidateId, cancellationToken));
        })
        .WithName("ListStaffArrRecruitingOffersV1");

        recruiting.MapPost("/offers", async (
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
            return Results.Created($"/api/v1/recruiting/offers/{created.Id}", created);
        })
        .WithName("CreateStaffArrRecruitingOfferV1");

        recruiting.MapPatch("/offers/{offerId:guid}", async (
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
        .WithName("UpdateStaffArrRecruitingOfferV1");

        recruiting.MapPost("/offers/{offerId:guid}/archive", async (
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
        .WithName("ArchiveStaffArrRecruitingOfferV1");
    }
}
