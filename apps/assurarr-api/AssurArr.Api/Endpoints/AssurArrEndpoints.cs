using AssurArr.Api.Contracts;
using AssurArr.Api.Services;

namespace AssurArr.Api.Endpoints;

public static class AssurArrEndpoints
{
    public static void MapAssurArrEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1").WithTags("AssurArr");
        var integrationGroup = app.MapGroup("/api/v1/integrations").WithTags("AssurArr Integrations");

        group.MapGet("/dashboard", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetDashboardAsync(cancellationToken)))
            .WithName("GetAssurArrDashboard");

        group.MapGet("/nonconformances", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListNonconformancesAsync(cancellationToken)))
            .WithName("ListAssurArrNonconformances");

        group.MapGet("/nonconformances/{id:guid}", async (Guid id, AssurArrQualityService service, CancellationToken cancellationToken) =>
            await service.GetNonconformanceAsync(id, cancellationToken) is { } response
                ? Results.Ok(response)
                : Results.NotFound())
            .WithName("GetAssurArrNonconformance");

        group.MapPost("/nonconformances", async (CreateAssurArrNonconformanceRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateNonconformanceAsync(request, cancellationToken)))
            .WithName("CreateAssurArrNonconformance");

        group.MapPatch("/nonconformances/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateNonconformanceStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrNonconformanceStatus");

        group.MapGet("/holds", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListQualityHoldsAsync(cancellationToken)))
            .WithName("ListAssurArrQualityHolds");

        group.MapPost("/holds", async (CreateAssurArrQualityHoldRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateQualityHoldAsync(request, cancellationToken)))
            .WithName("CreateAssurArrQualityHold");

        group.MapPatch("/holds/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateQualityHoldStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrQualityHoldStatus");

        group.MapGet("/capas", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListCapasAsync(cancellationToken)))
            .WithName("ListAssurArrCapas");

        group.MapPost("/capas", async (CreateAssurArrCapaRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateCapaAsync(request, cancellationToken)))
            .WithName("CreateAssurArrCapa");

        group.MapPatch("/capas/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateCapaStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrCapaStatus");

        group.MapGet("/audits", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAuditsAsync(cancellationToken)))
            .WithName("ListAssurArrAudits");

        group.MapPost("/audits", async (CreateAssurArrQualityAuditRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateAuditAsync(request, cancellationToken)))
            .WithName("CreateAssurArrAudit");

        group.MapPatch("/audits/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateAuditStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrAuditStatus");

        group.MapGet("/findings", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListFindingsAsync(cancellationToken)))
            .WithName("ListAssurArrFindings");

        group.MapPost("/findings", async (CreateAssurArrAuditFindingRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateFindingAsync(request, cancellationToken)))
            .WithName("CreateAssurArrFinding");

        group.MapPatch("/findings/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateFindingStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrFindingStatus");

        group.MapGet("/status-snapshots", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListStatusSnapshotsAsync(cancellationToken)))
            .WithName("ListAssurArrQualityStatusSnapshots");

        group.MapPost("/status-snapshots", async (CreateAssurArrQualityStatusSnapshotRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateStatusSnapshotAsync(request, cancellationToken)))
            .WithName("CreateAssurArrQualityStatusSnapshot");

        group.MapGet("/scorecards", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListScorecardsAsync(cancellationToken)))
            .WithName("ListAssurArrQualityScorecards");

        group.MapPost("/scorecards", async (CreateAssurArrQualityScorecardRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateScorecardAsync(request, cancellationToken)))
            .WithName("CreateAssurArrQualityScorecard");

        group.MapGet("/history", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetDashboardAsync(cancellationToken)))
            .WithName("GetAssurArrHistory");

        integrationGroup.MapGet("/quality-reviews", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListQualityReviewsAsync(cancellationToken)))
            .WithName("ListAssurArrQualityReviews");

        integrationGroup.MapPost("/quality-reviews", async (CreateAssurArrQualityReviewRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateQualityReviewAsync(request, cancellationToken)))
            .WithName("CreateAssurArrQualityReview");

        integrationGroup.MapPatch("/quality-reviews/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateQualityReviewStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrQualityReviewStatus");

        integrationGroup.MapGet("/quality-releases", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListQualityReleasesAsync(cancellationToken)))
            .WithName("ListAssurArrQualityReleases");

        integrationGroup.MapPost("/quality-releases", async (CreateAssurArrQualityReleaseRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateQualityReleaseAsync(request, cancellationToken)))
            .WithName("CreateAssurArrQualityRelease");

        integrationGroup.MapPatch("/quality-releases/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateQualityReleaseStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrQualityReleaseStatus");
    }
}
