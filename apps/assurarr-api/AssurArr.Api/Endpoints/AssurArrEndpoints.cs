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

        group.MapGet("/holds/{id:guid}", async (Guid id, AssurArrQualityService service, CancellationToken cancellationToken) =>
            await service.GetQualityHoldAsync(id, cancellationToken) is { } response
                ? Results.Ok(response)
                : Results.NotFound())
            .WithName("GetAssurArrQualityHold");

        group.MapPost("/holds", async (CreateAssurArrQualityHoldRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateQualityHoldAsync(request, cancellationToken)))
            .WithName("CreateAssurArrQualityHold");

        group.MapPatch("/holds/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateQualityHoldStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrQualityHoldStatus");

        integrationGroup.MapPost("/holds/{holdId:guid}/release-requests", async (Guid holdId, CreateAssurArrQualityReleaseRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.RequestHoldReleaseAsync(holdId, request, cancellationToken)))
            .WithName("CreateAssurArrHoldReleaseRequest");

        integrationGroup.MapPost("/holds/{holdId:guid}/release", async (Guid holdId, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ApproveHoldReleaseAsync(holdId, request, cancellationToken)))
            .WithName("ApproveAssurArrHoldRelease");

        integrationGroup.MapPost("/holds/{holdId:guid}/reject", async (Guid holdId, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.RejectHoldReleaseAsync(holdId, request, cancellationToken)))
            .WithName("RejectAssurArrHoldRelease");

        group.MapGet("/capas", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListCapasAsync(cancellationToken)))
            .WithName("ListAssurArrCapas");

        group.MapGet("/capas/{id:guid}", async (Guid id, AssurArrQualityService service, CancellationToken cancellationToken) =>
            await service.GetCapaAsync(id, cancellationToken) is { } response
                ? Results.Ok(response)
                : Results.NotFound())
            .WithName("GetAssurArrCapa");

        group.MapPost("/capas", async (CreateAssurArrCapaRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateCapaAsync(request, cancellationToken)))
            .WithName("CreateAssurArrCapa");

        group.MapPatch("/capas/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateCapaStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrCapaStatus");

        group.MapGet("/capas/{capaId:guid}/actions", async (Guid capaId, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListCapaActionsAsync(capaId, cancellationToken)))
            .WithName("ListAssurArrCapaActions");

        group.MapPost("/capas/{capaId:guid}/actions", async (Guid capaId, CreateAssurArrCapaActionRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateCapaActionAsync(capaId, request, cancellationToken)))
            .WithName("CreateAssurArrCapaAction");

        group.MapPatch("/capas/{capaId:guid}/actions/{actionId:guid}/status", async (Guid capaId, Guid actionId, UpdateAssurArrCapaActionStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateCapaActionStatusAsync(capaId, actionId, request, cancellationToken)))
            .WithName("UpdateAssurArrCapaActionStatus");

        group.MapGet("/capas/{capaId:guid}/actions/{actionId:guid}/blockers", async (Guid capaId, Guid actionId, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListCapaActionBlockersAsync(capaId, actionId, cancellationToken)))
            .WithName("ListAssurArrCapaActionBlockers");

        group.MapPost("/capas/{capaId:guid}/actions/{actionId:guid}/blockers", async (Guid capaId, Guid actionId, CreateAssurArrCapaActionBlockerRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateCapaActionBlockerAsync(capaId, actionId, request, cancellationToken)))
            .WithName("CreateAssurArrCapaActionBlocker");

        group.MapPatch("/capas/{capaId:guid}/actions/{actionId:guid}/blockers/{blockerId:guid}/status", async (Guid capaId, Guid actionId, Guid blockerId, UpdateAssurArrCapaActionBlockerStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateCapaActionBlockerStatusAsync(capaId, actionId, blockerId, request, cancellationToken)))
            .WithName("UpdateAssurArrCapaActionBlockerStatus");

        group.MapGet("/capas/{capaId:guid}/verification-plans", async (Guid capaId, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListVerificationPlansAsync(capaId, cancellationToken)))
            .WithName("ListAssurArrVerificationPlans");

        group.MapPost("/capas/{capaId:guid}/verification-plans", async (Guid capaId, CreateAssurArrVerificationPlanRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateVerificationPlanAsync(capaId, request, cancellationToken)))
            .WithName("CreateAssurArrVerificationPlan");

        group.MapPatch("/capas/{capaId:guid}/verification-plans/{verificationPlanId:guid}/status", async (Guid capaId, Guid verificationPlanId, UpdateAssurArrVerificationPlanStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateVerificationPlanStatusAsync(capaId, verificationPlanId, request, cancellationToken)))
            .WithName("UpdateAssurArrVerificationPlanStatus");

        group.MapGet("/audits", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAuditsAsync(cancellationToken)))
            .WithName("ListAssurArrAudits");

        group.MapPost("/audits", async (CreateAssurArrQualityAuditRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateAuditAsync(request, cancellationToken)))
            .WithName("CreateAssurArrAudit");

        group.MapPatch("/audits/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateAuditStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrAuditStatus");

        group.MapGet("/audits/{auditId:guid}/checklists", async (Guid auditId, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAuditChecklistsAsync(auditId, cancellationToken)))
            .WithName("ListAssurArrAuditChecklists");

        group.MapPost("/audits/{auditId:guid}/checklists", async (Guid auditId, CreateAssurArrQualityAuditChecklistRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateAuditChecklistAsync(auditId, request, cancellationToken)))
            .WithName("CreateAssurArrAuditChecklist");

        group.MapPatch("/audits/{auditId:guid}/checklists/{checklistId:guid}/status", async (Guid auditId, Guid checklistId, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateAuditChecklistStatusAsync(auditId, checklistId, request, cancellationToken)))
            .WithName("UpdateAssurArrAuditChecklistStatus");

        group.MapGet("/audits/{auditId:guid}/checklists/{checklistId:guid}/items", async (Guid auditId, Guid checklistId, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAuditChecklistItemsAsync(auditId, checklistId, cancellationToken)))
            .WithName("ListAssurArrAuditChecklistItems");

        group.MapPost("/audits/{auditId:guid}/checklists/{checklistId:guid}/items", async (Guid auditId, Guid checklistId, CreateAssurArrQualityAuditChecklistItemRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateAuditChecklistItemAsync(auditId, checklistId, request, cancellationToken)))
            .WithName("CreateAssurArrAuditChecklistItem");

        group.MapPatch("/audits/{auditId:guid}/checklists/{checklistId:guid}/items/{itemId:guid}/response", async (Guid auditId, Guid checklistId, Guid itemId, UpdateAssurArrQualityAuditChecklistItemResponseRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateAuditChecklistItemResponseAsync(auditId, checklistId, itemId, request, cancellationToken)))
            .WithName("UpdateAssurArrAuditChecklistItemResponse");

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

        integrationGroup.MapGet("/quality-status", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListQualityStatusAsync(cancellationToken)))
            .WithName("ListAssurArrQualityStatus");

        integrationGroup.MapGet("/quality-status/{targetProduct}/{targetObjectId}", async (string targetProduct, string targetObjectId, AssurArrQualityService service, CancellationToken cancellationToken) =>
            await service.GetQualityStatusAsync(targetProduct, targetObjectId, cancellationToken) is { } response
                ? Results.Ok(response)
                : Results.NotFound())
            .WithName("GetAssurArrQualityStatus");

        integrationGroup.MapPost("/quality-status-checks", async (CreateAssurArrQualityStatusSnapshotRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateQualityStatusCheckAsync(request, cancellationToken)))
            .WithName("CreateAssurArrQualityStatusCheck");

        integrationGroup.MapGet("/scorecards", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListScorecardsAsync(cancellationToken)))
            .WithName("ListAssurArrQualityScorecardsIntegration");

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

        integrationGroup.MapGet("/containment-actions", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListContainmentActionsAsync(cancellationToken)))
            .WithName("ListAssurArrContainmentActions");

        integrationGroup.MapPost("/containment-actions", async (CreateAssurArrContainmentActionRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateContainmentActionAsync(request, cancellationToken)))
            .WithName("CreateAssurArrContainmentAction");

        integrationGroup.MapPatch("/containment-actions/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateContainmentActionStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrContainmentActionStatus");

        integrationGroup.MapGet("/dispositions", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListDispositionsAsync(cancellationToken)))
            .WithName("ListAssurArrDispositions");

        integrationGroup.MapPost("/dispositions", async (CreateAssurArrDispositionRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateDispositionAsync(request, cancellationToken)))
            .WithName("CreateAssurArrDisposition");

        integrationGroup.MapPatch("/dispositions/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateDispositionStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrDispositionStatus");

        integrationGroup.MapGet("/supplier-quality-issues", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListSupplierQualityIssuesAsync(cancellationToken)))
            .WithName("ListAssurArrSupplierQualityIssues");

        integrationGroup.MapPost("/supplier-quality-issues", async (CreateAssurArrSupplierQualityIssueRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateSupplierQualityIssueAsync(request, cancellationToken)))
            .WithName("CreateAssurArrSupplierQualityIssue");

        integrationGroup.MapPatch("/supplier-quality-issues/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateSupplierQualityIssueStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrSupplierQualityIssueStatus");

        integrationGroup.MapGet("/scars", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListScarsAsync(cancellationToken)))
            .WithName("ListAssurArrSupplierCorrectiveActionRequests");

        integrationGroup.MapPost("/scars", async (CreateAssurArrSupplierCorrectiveActionRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateScarAsync(request, cancellationToken)))
            .WithName("CreateAssurArrSupplierCorrectiveActionRequest");

        integrationGroup.MapPatch("/scars/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateScarStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrSupplierCorrectiveActionRequestStatus");

        integrationGroup.MapGet("/customer-complaint-quality-cases", async (AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListCustomerComplaintQualityCasesAsync(cancellationToken)))
            .WithName("ListAssurArrCustomerComplaintQualityCases");

        integrationGroup.MapPost("/customer-complaint-quality-cases", async (CreateAssurArrCustomerComplaintQualityCaseRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateCustomerComplaintQualityCaseAsync(request, cancellationToken)))
            .WithName("CreateAssurArrCustomerComplaintQualityCase");

        integrationGroup.MapPatch("/customer-complaint-quality-cases/{id:guid}/status", async (Guid id, UpdateAssurArrStatusRequest request, AssurArrQualityService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateCustomerComplaintQualityCaseStatusAsync(id, request, cancellationToken)))
            .WithName("UpdateAssurArrCustomerComplaintQualityCaseStatus");
    }
}
