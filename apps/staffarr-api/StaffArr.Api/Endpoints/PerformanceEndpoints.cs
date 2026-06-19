using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class PerformanceEndpoints
{
    public static void MapStaffArrPerformanceEndpoints(this WebApplication app)
    {
        var performance = app.MapGroup("/api/v1/performance").WithTags("Performance").RequireAuthorization();

        performance.MapGet("/cycles", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListReviewCyclesAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        performance.MapGet("/cycles/{id:guid}", async (Guid id, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.GetReviewCycleAsync(context.User.GetTenantId(), id, cancellationToken));
        });
        performance.MapPost("/cycles", async (UpsertPerformanceReviewCycleRequest request, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertReviewCycleAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        performance.MapPatch("/cycles/{id:guid}", async (Guid id, UpsertPerformanceReviewCycleRequest request, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertReviewCycleAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        performance.MapGet("/goals", async (Guid? personId, Guid? cycleId, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListGoalsAsync(context.User.GetTenantId(), personId, cycleId, cancellationToken));
        });
        performance.MapPost("/goals", async (UpsertPerformanceGoalRequest request, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertGoalAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        performance.MapPatch("/goals/{id:guid}", async (Guid id, UpsertPerformanceGoalRequest request, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertGoalAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        performance.MapGet("/competencies", async (Guid? personId, Guid? cycleId, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListCompetenciesAsync(context.User.GetTenantId(), personId, cycleId, cancellationToken));
        });
        performance.MapPost("/competencies", async (UpsertPerformanceCompetencyAssessmentRequest request, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertCompetencyAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        performance.MapPatch("/competencies/{id:guid}", async (Guid id, UpsertPerformanceCompetencyAssessmentRequest request, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertCompetencyAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });

        performance.MapGet("/feedback", async (Guid? personId, Guid? cycleId, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListFeedbackAsync(context.User.GetTenantId(), personId, cycleId, cancellationToken));
        });
        performance.MapPost("/feedback", async (CreatePerformanceFeedbackEntryRequest request, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.CreateFeedbackAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken));
        });

        performance.MapGet("/pip", async (Guid? personId, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            return Results.Ok(await service.ListImprovementPlansAsync(context.User.GetTenantId(), personId, cancellationToken));
        });
        performance.MapPost("/pip", async (UpsertPerformanceImprovementPlanRequest request, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertImprovementPlanAsync(context.User.GetTenantId(), context.User.GetUserId(), null, request, cancellationToken));
        });
        performance.MapPatch("/pip/{id:guid}", async (Guid id, UpsertPerformanceImprovementPlanRequest request, HttpContext context, StaffArrAuthorizationService authorization, PerformanceService service, CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            return Results.Ok(await service.UpsertImprovementPlanAsync(context.User.GetTenantId(), context.User.GetUserId(), id, request, cancellationToken));
        });
    }
}
