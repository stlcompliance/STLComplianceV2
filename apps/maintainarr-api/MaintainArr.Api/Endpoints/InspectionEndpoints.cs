using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class InspectionEndpoints
{
    public static void MapMaintainArrInspectionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/inspections").WithTags("Inspections").RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionRunService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var viewAll = authorization.CanViewAllInspectionRuns(context.User);
            return Results.Ok(await service.ListAsync(tenantId, actorUserId, viewAll, cancellationToken));
        })
        .WithName("ListInspectionRuns");

        group.MapGet("/{inspectionRunId:guid}", async (
            Guid inspectionRunId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionRunService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, inspectionRunId, cancellationToken);
            authorization.RequireInspectionRunAccess(context.User, detail.StartedByUserId);
            return Results.Ok(detail);
        })
        .WithName("GetInspectionRun");

        group.MapPost("/", async (
            StartInspectionRunRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionRunService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsExecute(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.StartAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/inspections/{created.InspectionRunId}", created);
        })
        .WithName("StartInspectionRun");

        group.MapPut("/{inspectionRunId:guid}/answers", async (
            Guid inspectionRunId,
            SubmitInspectionRunAnswersRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionRunService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsExecute(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var existing = await service.GetAsync(tenantId, inspectionRunId, cancellationToken);
            authorization.RequireInspectionRunAccess(context.User, existing.StartedByUserId);
            var updated = await service.SubmitAnswersAsync(
                tenantId,
                actorUserId,
                inspectionRunId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("SubmitInspectionRunAnswers");

        group.MapPost("/{inspectionRunId:guid}/complete", async (
            Guid inspectionRunId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionRunService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsExecute(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var existing = await service.GetAsync(tenantId, inspectionRunId, cancellationToken);
            authorization.RequireInspectionRunAccess(context.User, existing.StartedByUserId);
            var completed = await service.CompleteAsync(tenantId, actorUserId, inspectionRunId, cancellationToken);
            return Results.Ok(completed);
        })
        .WithName("CompleteInspectionRun");

        group.MapGet("/{inspectionRunId:guid}/voice-guidance", async (
            Guid inspectionRunId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionRunService runService,
            InspectionVoiceGuidanceService voiceService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInspectionsExecute(context.User);
            var tenantId = context.User.GetTenantId();
            var existing = await runService.GetAsync(tenantId, inspectionRunId, cancellationToken);
            authorization.RequireInspectionRunAccess(context.User, existing.StartedByUserId);
            return Results.Ok(await voiceService.GetGuidanceAsync(tenantId, inspectionRunId, cancellationToken));
        })
        .WithName("GetInspectionVoiceGuidance");

        group.MapPost("/voice/normalize-numeric", (
            NormalizeVoiceNumericRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            InspectionVoiceGuidanceService voiceService) =>
        {
            authorization.RequireInspectionsExecute(context.User);
            var result = voiceService.NormalizeNumeric(request.Transcript);
            return Results.Ok(new NormalizeVoiceNumericResponse(
                result.Value,
                result.NormalizedText,
                result.Understood));
        })
        .WithName("NormalizeInspectionVoiceNumeric");
    }
}
