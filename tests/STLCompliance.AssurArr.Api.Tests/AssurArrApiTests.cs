using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using AssurArr.Api.Contracts;

namespace STLCompliance.AssurArr.Api.Tests;

public sealed class AssurArrApiTests(WebApplicationFactory<global::AssurArr.Api.Program> factory)
    : IClassFixture<WebApplicationFactory<global::AssurArr.Api.Program>>
{
    private readonly HttpClient _client = factory
        .WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
        })
        .CreateClient();

    [Fact]
    public async Task Dashboard_includes_seeded_quality_counts()
    {
        var response = await _client.GetAsync("/api/v1/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dashboard = await response.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(dashboard);
        Assert.Contains(dashboard!.Cards, card => card.Key == "nonconformances" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "holds" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "scars" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "capa-effectiveness");
        Assert.Contains(dashboard.Cards, card => card.Key == "recently-released-holds");
    }

    [Fact]
    public async Task Can_create_and_list_nonconformance_records()
    {
        var title = $"Test nonconformance {Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/nonconformances",
            new CreateAssurArrNonconformanceRequest(
                title,
                "Created from automated test coverage.",
                "high",
                "receiving",
                "failed_inspection",
                "loadarr",
                "loadarr:receiving:test",
                ["loadarr:inventory:test"],
                null,
                null,
                null,
                null,
                null,
                false,
                null,
                null,
                [],
                DateTimeOffset.UtcNow.AddDays(2)));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<AssurArrNonconformanceResponse>();
        Assert.NotNull(created);
        Assert.Equal(title, created!.Title);

        var detailResponse = await _client.GetAsync($"/api/v1/nonconformances/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<AssurArrNonconformanceResponse>();
        Assert.NotNull(detail);
        Assert.Equal(created.Id, detail!.Id);
        Assert.Equal(title, detail.Title);

        var listResponse = await _client.GetAsync("/api/v1/nonconformances");
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<List<AssurArrNonconformanceResponse>>();
        Assert.NotNull(list);
        Assert.Contains(list!, item => item.Title == title);
    }

    [Fact]
    public async Task Can_create_root_cause_analyses_for_nonconformances()
    {
        var nonconformanceTitle = $"Test nonconformance for root cause {Guid.NewGuid():N}";
        var nonconformanceResponse = await _client.PostAsJsonAsync(
            "/api/v1/nonconformances",
            new CreateAssurArrNonconformanceRequest(
                nonconformanceTitle,
                "Created for automated root cause coverage.",
                "high",
                "internal_process",
                "process_failure",
                "assurarr",
                "assurarr:workflow:test",
                ["assurarr:object:test"],
                null,
                null,
                null,
                null,
                null,
                false,
                null,
                null,
                [],
                DateTimeOffset.UtcNow.AddDays(2)));

        Assert.Equal(HttpStatusCode.OK, nonconformanceResponse.StatusCode);
        var nonconformance = await nonconformanceResponse.Content.ReadFromJsonAsync<AssurArrNonconformanceResponse>();
        Assert.NotNull(nonconformance);

        var rootCauseTitle = $"Test root cause {Guid.NewGuid():N}";
        var rootCauseResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/root-cause-analyses",
            new CreateAssurArrRootCauseAnalysisRequest(
                rootCauseTitle,
                "Automated coverage for root cause analysis creation.",
                nonconformance!.Id,
                "in_progress",
                "five_whys",
                "process",
                "assurarr",
                nonconformance.SourceObjectRef,
                nonconformance.AffectedObjectRefs.ToArray(),
                null,
                ["recordarr:doc:root-cause-test"],
                "Process checklist was incomplete.",
                ["missing checklist", "insufficient review"],
                null,
                null,
                ["recordarr:doc:root-cause-evidence"]));

        Assert.Equal(HttpStatusCode.OK, rootCauseResponse.StatusCode);
        var rootCause = await rootCauseResponse.Content.ReadFromJsonAsync<AssurArrRootCauseAnalysisResponse>();
        Assert.NotNull(rootCause);
        Assert.Equal(rootCauseTitle, rootCause!.Title);
        Assert.Equal(nonconformance.Id, rootCause.NonconformanceId);

        var rootCauseListResponse = await _client.GetAsync($"/api/v1/nonconformances/{nonconformance.Id}/root-cause-analyses");
        Assert.Equal(HttpStatusCode.OK, rootCauseListResponse.StatusCode);
        var rootCauses = await rootCauseListResponse.Content.ReadFromJsonAsync<List<AssurArrRootCauseAnalysisResponse>>();
        Assert.NotNull(rootCauses);
        Assert.Contains(rootCauses!, item => item.Id == rootCause.Id);

        var nonconformanceDetailResponse = await _client.GetAsync($"/api/v1/nonconformances/{nonconformance.Id}");
        Assert.Equal(HttpStatusCode.OK, nonconformanceDetailResponse.StatusCode);
        var nonconformanceDetail = await nonconformanceDetailResponse.Content.ReadFromJsonAsync<AssurArrNonconformanceResponse>();
        Assert.NotNull(nonconformanceDetail);
        Assert.Equal(rootCause.Number, nonconformanceDetail!.RootCauseRef);
        Assert.Equal("investigation", nonconformanceDetail.Status);
    }

    [Fact]
    public async Task Can_request_approve_and_reject_hold_releases()
    {
        var approvalHoldTitle = $"Test approval hold {Guid.NewGuid():N}";
        var approvalHoldResponse = await _client.PostAsJsonAsync(
            "/api/v1/holds",
            new CreateAssurArrQualityHoldRequest(
                approvalHoldTitle,
                "Created for hold release approval coverage.",
                "moderate",
                "inventory",
                "full",
                "loadarr",
                "loadarr:inventory:test",
                ["loadarr:inventory:test"],
                null,
                "Needs release review",
                null,
                null,
                null,
                null,
                null));

        Assert.Equal(HttpStatusCode.OK, approvalHoldResponse.StatusCode);
        var approvalHold = await approvalHoldResponse.Content.ReadFromJsonAsync<AssurArrQualityHoldResponse>();
        Assert.NotNull(approvalHold);

        var approvalHoldDetailResponse = await _client.GetAsync($"/api/v1/holds/{approvalHold!.Id}");
        Assert.Equal(HttpStatusCode.OK, approvalHoldDetailResponse.StatusCode);
        var approvalHoldDetail = await approvalHoldDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityHoldResponse>();
        Assert.NotNull(approvalHoldDetail);
        Assert.Equal(approvalHold.Id, approvalHoldDetail!.Id);
        Assert.Equal(approvalHold.Number, approvalHoldDetail.Number);

        var approvalReleaseResponse = await _client.PostAsJsonAsync(
            $"/api/v1/integrations/holds/{approvalHold!.Id}/release-requests",
            new CreateAssurArrQualityReleaseRequest(
                $"Release {approvalHold.Number}",
                "Release request created for automated coverage.",
                "none",
                "loadarr",
                "loadarr:inventory:test",
                ["loadarr:inventory:test"],
                null,
                approvalHold.Number,
                "full",
                null,
                DateTimeOffset.UtcNow,
                "Release requirements met.",
                null,
                ["recordarr:doc:release-evidence"],
                "Release request notes"));

        Assert.Equal(HttpStatusCode.OK, approvalReleaseResponse.StatusCode);
        var approvalRelease = await approvalReleaseResponse.Content.ReadFromJsonAsync<AssurArrQualityReleaseResponse>();
        Assert.NotNull(approvalRelease);
        Assert.Equal("requested", approvalRelease!.Status);
        Assert.Equal(approvalHold.Number, approvalRelease.HoldRef);

        var holdsAfterRequestResponse = await _client.GetAsync("/api/v1/holds");
        holdsAfterRequestResponse.EnsureSuccessStatusCode();
        var holdsAfterRequest = await holdsAfterRequestResponse.Content.ReadFromJsonAsync<List<AssurArrQualityHoldResponse>>();
        Assert.NotNull(holdsAfterRequest);
        var requestedHold = holdsAfterRequest!.Single(item => item.Id == approvalHold.Id);
        Assert.Equal("release_pending", requestedHold.Status);
        Assert.NotEmpty(requestedHold.ReleaseRequirements);

        var releaseApprovalResponse = await _client.PostAsJsonAsync(
            $"/api/v1/integrations/holds/{approvalHold.Id}/release",
            new UpdateAssurArrStatusRequest("executed", "Release approved."));

        Assert.Equal(HttpStatusCode.OK, releaseApprovalResponse.StatusCode);
        var approvedRelease = await releaseApprovalResponse.Content.ReadFromJsonAsync<AssurArrQualityReleaseResponse>();
        Assert.NotNull(approvedRelease);
        Assert.Equal("approved", approvedRelease!.Status);

        var holdsAfterReleaseResponse = await _client.GetAsync("/api/v1/holds");
        holdsAfterReleaseResponse.EnsureSuccessStatusCode();
        var holdsAfterRelease = await holdsAfterReleaseResponse.Content.ReadFromJsonAsync<List<AssurArrQualityHoldResponse>>();
        Assert.NotNull(holdsAfterRelease);
        Assert.Equal("released", holdsAfterRelease!.Single(item => item.Id == approvalHold.Id).Status);

        var rejectHoldTitle = $"Test reject hold {Guid.NewGuid():N}";
        var rejectHoldResponse = await _client.PostAsJsonAsync(
            "/api/v1/holds",
            new CreateAssurArrQualityHoldRequest(
                rejectHoldTitle,
                "Created for hold release rejection coverage.",
                "moderate",
                "inventory",
                "full",
                "loadarr",
                "loadarr:inventory:test",
                ["loadarr:inventory:test"],
                null,
                "Needs release review",
                null,
                null,
                null,
                null,
                null));

        Assert.Equal(HttpStatusCode.OK, rejectHoldResponse.StatusCode);
        var rejectHold = await rejectHoldResponse.Content.ReadFromJsonAsync<AssurArrQualityHoldResponse>();
        Assert.NotNull(rejectHold);

        var rejectReleaseResponse = await _client.PostAsJsonAsync(
            $"/api/v1/integrations/holds/{rejectHold!.Id}/release-requests",
            new CreateAssurArrQualityReleaseRequest(
                $"Release {rejectHold.Number}",
                "Release request created for rejection coverage.",
                "none",
                "loadarr",
                "loadarr:inventory:test",
                ["loadarr:inventory:test"],
                null,
                rejectHold.Number,
                "full",
                null,
                DateTimeOffset.UtcNow,
                "Release requirements met.",
                null,
                ["recordarr:doc:release-evidence"],
                "Release request notes"));

        Assert.Equal(HttpStatusCode.OK, rejectReleaseResponse.StatusCode);

        var rejectionResponse = await _client.PostAsJsonAsync(
            $"/api/v1/integrations/holds/{rejectHold.Id}/reject",
            new UpdateAssurArrStatusRequest("rejected", "Release rejected."));

        Assert.Equal(HttpStatusCode.OK, rejectionResponse.StatusCode);
        var rejectedRelease = await rejectionResponse.Content.ReadFromJsonAsync<AssurArrQualityReleaseResponse>();
        Assert.NotNull(rejectedRelease);
        Assert.Equal("rejected", rejectedRelease!.Status);

        var holdsAfterRejectResponse = await _client.GetAsync("/api/v1/holds");
        holdsAfterRejectResponse.EnsureSuccessStatusCode();
        var holdsAfterReject = await holdsAfterRejectResponse.Content.ReadFromJsonAsync<List<AssurArrQualityHoldResponse>>();
        Assert.NotNull(holdsAfterReject);
        Assert.Equal("rejected", holdsAfterReject!.Single(item => item.Id == rejectHold.Id).Status);
    }

    [Fact]
    public async Task Can_create_quality_review_and_release_records()
    {
        var reviewTitle = $"Test quality review {Guid.NewGuid():N}";
        var reviewResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/quality-reviews",
            new CreateAssurArrQualityReviewRequest(
                reviewTitle,
                "Automated coverage for the quality review workflow.",
                "moderate",
                "hold_release",
                "assurarr",
                "HOLD-000001",
                ["loadarr:inventory:test"],
                null,
                "HOLD-000001",
                null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(2),
                "Review evidence before release.",
                ["recordarr:doc:test"],
                ["recordarr:doc:test"],
                "Review notes"));

        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);

        var review = await reviewResponse.Content.ReadFromJsonAsync<AssurArrQualityReviewResponse>();
        Assert.NotNull(review);
        Assert.Equal(reviewTitle, review!.Title);

        var reviewDetailResponse = await _client.GetAsync($"/api/v1/integrations/quality-reviews/{review.Id}");
        Assert.Equal(HttpStatusCode.OK, reviewDetailResponse.StatusCode);
        var reviewDetail = await reviewDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityReviewResponse>();
        Assert.NotNull(reviewDetail);
        Assert.Equal(review.Id, reviewDetail!.Id);
        Assert.Equal(review.Number, reviewDetail.Number);

        var releaseTitle = $"Test quality release {Guid.NewGuid():N}";
        var releaseResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/quality-releases",
            new CreateAssurArrQualityReleaseRequest(
                releaseTitle,
                "Automated coverage for the quality release workflow.",
                "low",
                "assurarr",
                "HOLD-000001",
                ["loadarr:inventory:test"],
                null,
                "HOLD-000001",
                "full",
                null,
                DateTimeOffset.UtcNow,
                "Inspection evidence retained in RecordArr.",
                DateTimeOffset.UtcNow.AddDays(1),
                ["recordarr:doc:test"],
                "Release notes"));

        Assert.Equal(HttpStatusCode.OK, releaseResponse.StatusCode);

        var release = await releaseResponse.Content.ReadFromJsonAsync<AssurArrQualityReleaseResponse>();
        Assert.NotNull(release);
        Assert.Equal(releaseTitle, release!.Title);

        var releaseDetailResponse = await _client.GetAsync($"/api/v1/integrations/quality-releases/{release.Id}");
        Assert.Equal(HttpStatusCode.OK, releaseDetailResponse.StatusCode);
        var releaseDetail = await releaseDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityReleaseResponse>();
        Assert.NotNull(releaseDetail);
        Assert.Equal(release.Id, releaseDetail!.Id);
        Assert.Equal(release.Number, releaseDetail.Number);

        var listResponse = await _client.GetAsync("/api/v1/integrations/quality-reviews");
        listResponse.EnsureSuccessStatusCode();
        var reviews = await listResponse.Content.ReadFromJsonAsync<List<AssurArrQualityReviewResponse>>();
        Assert.NotNull(reviews);
        Assert.Contains(reviews!, item => item.Title == reviewTitle);
    }

    [Fact]
    public async Task Can_create_capa_actions_and_verification_plans()
    {
        var capaTitle = $"Test CAPA {Guid.NewGuid():N}";
        var capaResponse = await _client.PostAsJsonAsync(
            "/api/v1/capas",
            new CreateAssurArrCapaRequest(
                capaTitle,
                "Automated coverage for CAPA actions.",
                "high",
                "corrective_and_preventive",
                "manual",
                "assurarr",
                "workflow:capa:test",
                ["loadarr:inventory:test"],
                null,
                null,
                "Awaiting analysis",
                DateTimeOffset.UtcNow.AddDays(7),
                ["NCR-000001"],
                ["FIND-000001"],
                []));

        Assert.Equal(HttpStatusCode.OK, capaResponse.StatusCode);
        var capa = await capaResponse.Content.ReadFromJsonAsync<AssurArrCapaResponse>();
        Assert.NotNull(capa);

        var capaDetailResponse = await _client.GetAsync($"/api/v1/capas/{capa!.Id}");
        Assert.Equal(HttpStatusCode.OK, capaDetailResponse.StatusCode);
        var capaDetail = await capaDetailResponse.Content.ReadFromJsonAsync<AssurArrCapaResponse>();
        Assert.NotNull(capaDetail);
        Assert.Equal(capa.Id, capaDetail!.Id);
        Assert.Equal(capa.Number, capaDetail.Number);

        var actionTitle = $"Test CAPA action {Guid.NewGuid():N}";
        var actionResponse = await _client.PostAsJsonAsync(
            $"/api/v1/capas/{capa!.Id}/actions",
            new CreateAssurArrCapaActionRequest(
                actionTitle,
                "Automated coverage for CAPA action records.",
                "update_work_instruction",
                null,
                "loadarr:receiving",
                "loadarr:action:test",
                "loadarr",
                "loadarr:workflow:test",
                DateTimeOffset.UtcNow.AddDays(3),
                true,
                ["recordarr:doc:test"],
                ["blocker:test"],
                "Action notes"));

        Assert.Equal(HttpStatusCode.OK, actionResponse.StatusCode);
        var action = await actionResponse.Content.ReadFromJsonAsync<AssurArrCapaActionResponse>();
        Assert.NotNull(action);
        Assert.Equal(actionTitle, action!.Title);

        var blockerTitle = $"Test CAPA blocker {Guid.NewGuid():N}";
        var blockerResponse = await _client.PostAsJsonAsync(
            $"/api/v1/capas/{capa!.Id}/actions/{action.Id}/blockers",
            new CreateAssurArrCapaActionBlockerRequest(
                "waiting_supplier",
                "supplyarr",
                "supplyarr:supplier:test",
                blockerTitle,
                "Automated coverage for CAPA action blockers."));

        Assert.Equal(HttpStatusCode.OK, blockerResponse.StatusCode);
        var blocker = await blockerResponse.Content.ReadFromJsonAsync<AssurArrCapaActionBlockerResponse>();
        Assert.NotNull(blocker);
        Assert.Equal(blockerTitle, blocker!.Title);
        Assert.Equal("active", blocker.Status);

        var blockerListResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}/actions/{action.Id}/blockers");
        blockerListResponse.EnsureSuccessStatusCode();
        var blockers = await blockerListResponse.Content.ReadFromJsonAsync<List<AssurArrCapaActionBlockerResponse>>();
        Assert.NotNull(blockers);
        Assert.Contains(blockers!, item => item.Title == blockerTitle);

        var resolveBlockerResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/actions/{action.Id}/blockers/{blocker.Id}/status",
            new UpdateAssurArrCapaActionBlockerStatusRequest("resolved", null, DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.OK, resolveBlockerResponse.StatusCode);

        var capaRootCauseResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/status",
            new UpdateAssurArrStatusRequest("root_cause", "Root cause analysis in progress."));

        Assert.Equal(HttpStatusCode.OK, capaRootCauseResponse.StatusCode);

        var capaActionPlanResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/status",
            new UpdateAssurArrStatusRequest("action_plan", "Action plan defined."));

        Assert.Equal(HttpStatusCode.OK, capaActionPlanResponse.StatusCode);

        var capaImplementationResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/status",
            new UpdateAssurArrStatusRequest("implementation", "Actions in progress."));

        Assert.Equal(HttpStatusCode.OK, capaImplementationResponse.StatusCode);

        var capaVerificationResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/status",
            new UpdateAssurArrStatusRequest("verification", "Ready for effectiveness verification."));

        Assert.Equal(HttpStatusCode.OK, capaVerificationResponse.StatusCode);

        var verificationTitle = $"Test verification plan {Guid.NewGuid():N}";
        var verificationResponse = await _client.PostAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/verification-plans",
            new CreateAssurArrVerificationPlanRequest(
                verificationTitle,
                "Automated coverage for verification plans.",
                "audit",
                "No missing release signatures in sampled receipts.",
                5,
                14,
                ["record", "photo"],
                null,
                DateTimeOffset.UtcNow.AddDays(14)));

        Assert.Equal(HttpStatusCode.OK, verificationResponse.StatusCode);
        var verification = await verificationResponse.Content.ReadFromJsonAsync<AssurArrVerificationPlanResponse>();
        Assert.NotNull(verification);
        Assert.Equal(verificationTitle, verification!.Title);

        var actionListResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}/actions");
        actionListResponse.EnsureSuccessStatusCode();
        var actions = await actionListResponse.Content.ReadFromJsonAsync<List<AssurArrCapaActionResponse>>();
        Assert.NotNull(actions);
        Assert.Contains(actions!, item => item.Title == actionTitle);

        var verificationListResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}/verification-plans");
        verificationListResponse.EnsureSuccessStatusCode();
        var verifications = await verificationListResponse.Content.ReadFromJsonAsync<List<AssurArrVerificationPlanResponse>>();
        Assert.NotNull(verifications);
        Assert.Contains(verifications!, item => item.Title == verificationTitle);

        var effectivenessResponse = await _client.PostAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/effectiveness-verifications",
            new CreateAssurArrEffectivenessVerificationRequest(
                verification.Id,
                "scheduled",
                null,
                null,
                "Initial effectiveness check scheduled after action completion.",
                ["recordarr:doc:test"],
                ["actions_completed=1", "open_nc_count=0"],
                false,
                true,
                null));

        Assert.Equal(HttpStatusCode.OK, effectivenessResponse.StatusCode);
        var effectiveness = await effectivenessResponse.Content.ReadFromJsonAsync<AssurArrEffectivenessVerificationResponse>();
        Assert.NotNull(effectiveness);
        Assert.Equal(verification.Id, effectiveness!.VerificationPlanId);
        Assert.Equal(capa.Id, effectiveness.CapaId);

        var effectivenessListResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}/effectiveness-verifications");
        effectivenessListResponse.EnsureSuccessStatusCode();
        var effectivenessVerifications = await effectivenessListResponse.Content.ReadFromJsonAsync<List<AssurArrEffectivenessVerificationResponse>>();
        Assert.NotNull(effectivenessVerifications);
        Assert.Contains(effectivenessVerifications!, item => item.Id == effectiveness.Id);

        var effectivenessStatusResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/effectiveness-verifications/{effectiveness.Id}/status",
            new UpdateAssurArrEffectivenessVerificationStatusRequest(
                "effective",
                "Verification confirmed the corrective action was effective.",
                false,
                true,
                null));

        Assert.Equal(HttpStatusCode.OK, effectivenessStatusResponse.StatusCode);
        var updatedEffectiveness = await effectivenessStatusResponse.Content.ReadFromJsonAsync<AssurArrEffectivenessVerificationResponse>();
        Assert.NotNull(updatedEffectiveness);
        Assert.Equal("effective", updatedEffectiveness!.Status);

        var capaAfterEffectivenessResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}");
        capaAfterEffectivenessResponse.EnsureSuccessStatusCode();
        var capaAfterEffectiveness = await capaAfterEffectivenessResponse.Content.ReadFromJsonAsync<AssurArrCapaResponse>();
        Assert.NotNull(capaAfterEffectiveness);
        Assert.Equal("closed", capaAfterEffectiveness!.Status);
        Assert.Contains(capaAfterEffectiveness.EffectivenessVerificationRefs, reference => reference == effectiveness.Number);
    }

    [Fact]
    public async Task Can_create_audit_checklists_and_items()
    {
        var auditTitle = $"Test audit {Guid.NewGuid():N}";
        var auditResponse = await _client.PostAsJsonAsync(
            "/api/v1/audits",
            new CreateAssurArrQualityAuditRequest(
                auditTitle,
                "Automated coverage for audit checklists.",
                "moderate",
                "internal",
                "receiving review",
                "assurarr",
                "workflow:audit:test",
                ["loadarr:location:test"],
                null,
                [],
                null,
                null,
                null,
                "supplyarr:supplier:test",
                null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(1),
                []));

        Assert.Equal(HttpStatusCode.OK, auditResponse.StatusCode);
        var audit = await auditResponse.Content.ReadFromJsonAsync<AssurArrQualityAuditResponse>();
        Assert.NotNull(audit);

        var auditDetailResponse = await _client.GetAsync($"/api/v1/audits/{audit!.Id}");
        Assert.Equal(HttpStatusCode.OK, auditDetailResponse.StatusCode);
        var auditDetail = await auditDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityAuditResponse>();
        Assert.NotNull(auditDetail);
        Assert.Equal(audit.Id, auditDetail!.Id);
        Assert.Equal(audit.Number, auditDetail.Number);

        var checklistTitle = $"Test checklist {Guid.NewGuid():N}";
        var checklistResponse = await _client.PostAsJsonAsync(
            $"/api/v1/audits/{audit!.Id}/checklists",
            new CreateAssurArrQualityAuditChecklistRequest(
                checklistTitle,
                "Automated coverage for audit checklist creation.",
                "draft"));

        Assert.Equal(HttpStatusCode.OK, checklistResponse.StatusCode);
        var checklist = await checklistResponse.Content.ReadFromJsonAsync<AssurArrQualityAuditChecklistResponse>();
        Assert.NotNull(checklist);
        Assert.Equal(checklistTitle, checklist!.Title);

        var itemPrompt = $"Check release signature {Guid.NewGuid():N}";
        var itemResponse = await _client.PostAsJsonAsync(
            $"/api/v1/audits/{audit.Id}/checklists/{checklist.Id}/items",
            new CreateAssurArrQualityAuditChecklistItemRequest(
                1,
                itemPrompt,
                "Confirm the signoff before closing the audit.",
                "recordarr:req:release-signoff",
                "pass_fail",
                true,
                "pass",
                "pass",
                false,
                null,
                ["recordarr:doc:test"],
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.OK, itemResponse.StatusCode);
        var item = await itemResponse.Content.ReadFromJsonAsync<AssurArrQualityAuditChecklistItemResponse>();
        Assert.NotNull(item);
        Assert.Equal(itemPrompt, item!.Prompt);

        var findingTitle = $"Test finding {Guid.NewGuid():N}";
        var findingResponse = await _client.PostAsJsonAsync(
            "/api/v1/findings",
            new CreateAssurArrAuditFindingRequest(
                findingTitle,
                "Automated coverage for finding creation.",
                "moderate",
                "major_nonconformance",
                "assurarr",
                "workflow:finding:test",
                ["loadarr:inventory:test"],
                null,
                audit.Number,
                null,
                null,
                DateTimeOffset.UtcNow.AddDays(4)));

        Assert.Equal(HttpStatusCode.OK, findingResponse.StatusCode);
        var finding = await findingResponse.Content.ReadFromJsonAsync<AssurArrAuditFindingResponse>();
        Assert.NotNull(finding);

        var findingDetailResponse = await _client.GetAsync($"/api/v1/findings/{finding!.Id}");
        Assert.Equal(HttpStatusCode.OK, findingDetailResponse.StatusCode);
        var findingDetail = await findingDetailResponse.Content.ReadFromJsonAsync<AssurArrAuditFindingResponse>();
        Assert.NotNull(findingDetail);
        Assert.Equal(finding.Id, findingDetail!.Id);
        Assert.Equal(finding.Number, findingDetail.Number);

        var checklistListResponse = await _client.GetAsync($"/api/v1/audits/{audit.Id}/checklists");
        checklistListResponse.EnsureSuccessStatusCode();
        var checklists = await checklistListResponse.Content.ReadFromJsonAsync<List<AssurArrQualityAuditChecklistResponse>>();
        Assert.NotNull(checklists);
        Assert.Contains(checklists!, entry => entry.Title == checklistTitle);

        var itemListResponse = await _client.GetAsync($"/api/v1/audits/{audit.Id}/checklists/{checklist.Id}/items");
        itemListResponse.EnsureSuccessStatusCode();
        var items = await itemListResponse.Content.ReadFromJsonAsync<List<AssurArrQualityAuditChecklistItemResponse>>();
        Assert.NotNull(items);
        Assert.Contains(items!, entry => entry.Prompt == itemPrompt);

        var responseUpdate = await _client.PatchAsJsonAsync(
            $"/api/v1/audits/{audit.Id}/checklists/{checklist.Id}/items/{item.Id}/response",
            new UpdateAssurArrQualityAuditChecklistItemResponseRequest(
                "pass",
                "pass",
                false,
                null,
                ["recordarr:doc:test"],
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.OK, responseUpdate.StatusCode);
    }

    [Fact]
    public async Task Can_create_supplier_quality_issue_and_customer_complaint_records()
    {
        var supplierTitle = $"Test supplier quality issue {Guid.NewGuid():N}";
        var supplierResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/supplier-quality-issues",
            new CreateAssurArrSupplierQualityIssueRequest(
                supplierTitle,
                "Automated coverage for supplier quality issues.",
                "high",
                "damaged_received",
                "loadarr",
                "loadarr:receipt:test",
                ["loadarr:receipt:test"],
                ["supplyarr:po:test"],
                ["supplyarr:item:test"],
                "supplyarr:supplier:test",
                "NCR-000001",
                "SCAR-000001",
                ["HOLD-000001"],
                ["recordarr:doc:test"],
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.OK, supplierResponse.StatusCode);
        var supplierIssue = await supplierResponse.Content.ReadFromJsonAsync<AssurArrSupplierQualityIssueResponse>();
        Assert.NotNull(supplierIssue);
        Assert.Equal(supplierTitle, supplierIssue!.Title);

        var supplierDetailResponse = await _client.GetAsync($"/api/v1/integrations/supplier-quality-issues/{supplierIssue.Id}");
        Assert.Equal(HttpStatusCode.OK, supplierDetailResponse.StatusCode);
        var supplierDetail = await supplierDetailResponse.Content.ReadFromJsonAsync<AssurArrSupplierQualityIssueResponse>();
        Assert.NotNull(supplierDetail);
        Assert.Equal(supplierIssue.Id, supplierDetail!.Id);
        Assert.Equal(supplierIssue.Number, supplierDetail.Number);

        var complaintTitle = $"Test complaint case {Guid.NewGuid():N}";
        var complaintResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/customer-complaint-quality-cases",
            new CreateAssurArrCustomerComplaintQualityCaseRequest(
                complaintTitle,
                "Automated coverage for customer complaint quality cases.",
                "high",
                "delivery_quality",
                "routarr",
                "routarr:shipment:test",
                ["ordarr:order:test"],
                ["routarr:shipment:test"],
                ["loadarr:item:test"],
                ["maintainarr:asset:test"],
                "customarr:customer:test",
                "Jordan Lee, logistics manager",
                "customarr:location:test",
                "NCR-000001",
                ["HOLD-000001"],
                ["CAPA-000001"],
                ["recordarr:doc:response-test"],
                ["recordarr:doc:test"],
                null,
                DateTimeOffset.UtcNow,
                null,
                DateTimeOffset.UtcNow.AddDays(4)));

        Assert.Equal(HttpStatusCode.OK, complaintResponse.StatusCode);
        var complaint = await complaintResponse.Content.ReadFromJsonAsync<AssurArrCustomerComplaintQualityCaseResponse>();
        Assert.NotNull(complaint);
        Assert.Equal(complaintTitle, complaint!.Title);

        var complaintDetailResponse = await _client.GetAsync($"/api/v1/integrations/customer-complaint-quality-cases/{complaint.Id}");
        Assert.Equal(HttpStatusCode.OK, complaintDetailResponse.StatusCode);
        var complaintDetail = await complaintDetailResponse.Content.ReadFromJsonAsync<AssurArrCustomerComplaintQualityCaseResponse>();
        Assert.NotNull(complaintDetail);
        Assert.Equal(complaint.Id, complaintDetail!.Id);
        Assert.Equal(complaint.Number, complaintDetail.Number);

        var supplierList = await _client.GetAsync("/api/v1/integrations/supplier-quality-issues");
        supplierList.EnsureSuccessStatusCode();
        var supplierIssues = await supplierList.Content.ReadFromJsonAsync<List<AssurArrSupplierQualityIssueResponse>>();
        Assert.NotNull(supplierIssues);
        Assert.Contains(supplierIssues!, item => item.Title == supplierTitle);

        var complaintList = await _client.GetAsync("/api/v1/integrations/customer-complaint-quality-cases");
        complaintList.EnsureSuccessStatusCode();
        var complaintCases = await complaintList.Content.ReadFromJsonAsync<List<AssurArrCustomerComplaintQualityCaseResponse>>();
        Assert.NotNull(complaintCases);
        Assert.Contains(complaintCases!, item => item.Title == complaintTitle);
    }

    [Fact]
    public async Task Can_create_and_read_scar_records()
    {
        var title = $"Test SCAR {Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/scars",
            new CreateAssurArrSupplierCorrectiveActionRequest(
                title,
                "Automated coverage for SCAR detail reads.",
                "high",
                "assurarr",
                "SQA-000001",
                ["loadarr:receipt:test", "supplyarr:po:test"],
                "supplyarr:supplier:test",
                "NCR-000001",
                "CAPA-000001",
                null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(7),
                ["recordarr:doc:test"],
                null,
                DateTimeOffset.UtcNow,
                "accepted",
                "CAPA-000001",
                ["recordarr:doc:test"],
                null));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var scar = await createResponse.Content.ReadFromJsonAsync<AssurArrSupplierCorrectiveActionRequestResponse>();
        Assert.NotNull(scar);
        Assert.Equal(title, scar!.Title);

        var detailResponse = await _client.GetAsync($"/api/v1/integrations/scars/{scar.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<AssurArrSupplierCorrectiveActionRequestResponse>();
        Assert.NotNull(detail);
        Assert.Equal(scar.Id, detail!.Id);
        Assert.Equal(scar.Number, detail.Number);

        var listResponse = await _client.GetAsync("/api/v1/integrations/scars");
        listResponse.EnsureSuccessStatusCode();
        var scars = await listResponse.Content.ReadFromJsonAsync<List<AssurArrSupplierCorrectiveActionRequestResponse>>();
        Assert.NotNull(scars);
        Assert.Contains(scars!, item => item.Title == title);
    }

    [Fact]
    public async Task Can_create_and_update_scar_records()
    {
        var title = $"Test SCAR {Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/scars",
            new CreateAssurArrSupplierCorrectiveActionRequest(
                title,
                "Automated coverage for supplier corrective action requests.",
                "high",
                "assurarr",
                "SQA-000001",
                ["loadarr:receipt:test", "supplyarr:po:test"],
                "supplyarr:supplier:test",
                "NCR-000001",
                "CAPA-000001",
                null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(3),
                ["recordarr:doc:response-test"],
                null,
                null,
                null,
                "CAPA-000001",
                ["recordarr:doc:test"],
                null));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<AssurArrSupplierCorrectiveActionRequestResponse>();
        Assert.NotNull(created);
        Assert.Equal(title, created!.Title);

        var listResponse = await _client.GetAsync("/api/v1/integrations/scars");
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<List<AssurArrSupplierCorrectiveActionRequestResponse>>();
        Assert.NotNull(list);
        Assert.Contains(list!, item => item.Title == title);

        var updateResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/scars/{created.Id}/status",
            new UpdateAssurArrStatusRequest("sent", "Ready for supplier transmission."));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    [Fact]
    public async Task Can_create_containment_action_and_disposition_records()
    {
        var containmentTitle = $"Test containment action {Guid.NewGuid():N}";
        var containmentResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/containment-actions",
            new CreateAssurArrContainmentActionRequest(
                containmentTitle,
                "Automated coverage for containment actions.",
                "high",
                "quarantine",
                "loadarr",
                "loadarr:inventory:test",
                ["loadarr:inventory:test"],
                "NCR-000001",
                null,
                null,
                "loadarr:receiving:action:test",
                DateTimeOffset.UtcNow.AddDays(1),
                true,
                ["recordarr:doc:test"],
                "Containment notes"));

        Assert.Equal(HttpStatusCode.OK, containmentResponse.StatusCode);
        var containment = await containmentResponse.Content.ReadFromJsonAsync<AssurArrContainmentActionResponse>();
        Assert.NotNull(containment);
        Assert.Equal(containmentTitle, containment!.Title);

        var dispositionTitle = $"Test disposition {Guid.NewGuid():N}";
        var dispositionResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/dispositions",
            new CreateAssurArrDispositionRequest(
                dispositionTitle,
                "Automated coverage for disposition records.",
                "moderate",
                "conditional_release",
                "assurarr",
                "NCR-000001",
                ["loadarr:inventory:test"],
                "NCR-000001",
                null,
                DateTimeOffset.UtcNow,
                null,
                null,
                "Inspection evidence pending.",
                ["Complete inspection"],
                "loadarr",
                "loadarr:inventory:test",
                ["recordarr:doc:test"],
                "Disposition notes"));

        Assert.Equal(HttpStatusCode.OK, dispositionResponse.StatusCode);
        var disposition = await dispositionResponse.Content.ReadFromJsonAsync<AssurArrDispositionResponse>();
        Assert.NotNull(disposition);
        Assert.Equal(dispositionTitle, disposition!.Title);

        var containmentList = await _client.GetAsync("/api/v1/integrations/containment-actions");
        containmentList.EnsureSuccessStatusCode();
        var containmentActions = await containmentList.Content.ReadFromJsonAsync<List<AssurArrContainmentActionResponse>>();
        Assert.NotNull(containmentActions);
        Assert.Contains(containmentActions!, item => item.Title == containmentTitle);

        var dispositionList = await _client.GetAsync("/api/v1/integrations/dispositions");
        dispositionList.EnsureSuccessStatusCode();
        var dispositions = await dispositionList.Content.ReadFromJsonAsync<List<AssurArrDispositionResponse>>();
        Assert.NotNull(dispositions);
        Assert.Contains(dispositions!, item => item.Title == dispositionTitle);
    }

    [Fact]
    public async Task Can_create_and_lookup_quality_status_checks()
    {
        var targetProduct = $"target-{Guid.NewGuid():N}";
        var targetObjectId = $"object-{Guid.NewGuid():N}";
        var title = $"Test quality status {Guid.NewGuid():N}";

        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/quality-status-checks",
            new CreateAssurArrQualityStatusSnapshotRequest(
                targetProduct,
                $"{targetProduct}:{targetObjectId}",
                "warning",
                "moderate",
                title,
                "Automated coverage for quality status checks.",
                "assurarr",
                "NCR-000001",
                [$"{targetProduct}:{targetObjectId}"],
                null,
                ["HOLD-000001"],
                ["NCR-000001"],
                ["CAPA-000001"],
                ["FIND-000001"],
                DateTimeOffset.UtcNow.AddDays(2)));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(created);
        Assert.Equal(targetProduct, created!.TargetProduct);

        var lookupResponse = await _client.GetAsync($"/api/v1/integrations/quality-status/{targetProduct}/{targetObjectId}");
        Assert.Equal(HttpStatusCode.OK, lookupResponse.StatusCode);

        var lookup = await lookupResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(lookup);
        Assert.Equal(targetProduct, lookup!.TargetProduct);

        var listResponse = await _client.GetAsync("/api/v1/integrations/quality-status");
        listResponse.EnsureSuccessStatusCode();
        var statuses = await listResponse.Content.ReadFromJsonAsync<List<AssurArrQualityStatusSnapshotResponse>>();
        Assert.NotNull(statuses);
        Assert.Contains(statuses!, item => item.TargetProduct == targetProduct);
    }

    [Fact]
    public async Task Can_create_scorecard_metrics_and_read_scorecard_detail()
    {
        var targetRef = $"loadarr:site:{Guid.NewGuid():N}";
        var scorecardTitle = $"Test scorecard {Guid.NewGuid():N}";

        var scorecardResponse = await _client.PostAsJsonAsync(
            "/api/v1/scorecards",
            new CreateAssurArrQualityScorecardRequest(
                "site",
                targetRef,
                DateTimeOffset.UtcNow.AddDays(-7),
                DateTimeOffset.UtcNow,
                92,
                "acceptable",
                "stable",
                scorecardTitle,
                "Automated coverage for quality scorecards.",
                "low",
                "assurarr",
                targetRef,
                [$"{targetRef}"],
                null,
                []));

        Assert.Equal(HttpStatusCode.OK, scorecardResponse.StatusCode);
        var scorecard = await scorecardResponse.Content.ReadFromJsonAsync<AssurArrQualityScorecardResponse>();
        Assert.NotNull(scorecard);
        Assert.Equal(scorecardTitle, scorecard!.Title);

        var metricKey = $"metric-{Guid.NewGuid():N}";
        var metricResponse = await _client.PostAsJsonAsync(
            $"/api/v1/scorecards/{scorecard.Id}/metrics",
            new CreateAssurArrQualityMetricRequest(
                metricKey,
                "Open nonconformance count",
                "Count of nonconformances that are not closed or canceled.",
                "nonconformance",
                3,
                3,
                0,
                "count",
                0,
                2,
                5,
                "warning",
                ["assurarr", "loadarr"]));

        Assert.Equal(HttpStatusCode.OK, metricResponse.StatusCode);
        var metric = await metricResponse.Content.ReadFromJsonAsync<AssurArrQualityMetricResponse>();
        Assert.NotNull(metric);
        Assert.Equal(metricKey, metric!.MetricKey);

        var detailResponse = await _client.GetAsync($"/api/v1/scorecards/{scorecard.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<AssurArrQualityScorecardResponse>();
        Assert.NotNull(detail);
        Assert.Contains(metricKey, detail!.MetricRefs);

        var metricListResponse = await _client.GetAsync($"/api/v1/scorecards/{scorecard.Id}/metrics");
        metricListResponse.EnsureSuccessStatusCode();
        var metrics = await metricListResponse.Content.ReadFromJsonAsync<List<AssurArrQualityMetricResponse>>();
        Assert.NotNull(metrics);
        Assert.Contains(metrics!, item => item.MetricKey == metricKey);
    }

    [Fact]
    public async Task Can_create_and_lookup_quality_risk_profiles()
    {
        var targetType = "process";
        var targetRef = $"assurarr:process:{Guid.NewGuid():N}";

        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/risk-profiles",
            new CreateAssurArrQualityRiskProfileRequest(
                targetType,
                targetRef,
                "high",
                ["recurring defect", "supplier instability"],
                5,
                2,
                1,
                DateTimeOffset.UtcNow.AddDays(-1),
                ["monitor trend", "verify training refresh"],
                DateTimeOffset.UtcNow,
                null));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<AssurArrQualityRiskProfileResponse>();
        Assert.NotNull(created);
        Assert.Equal(targetType, created!.TargetType);
        Assert.Equal(targetRef, created.TargetRef);

        var lookupResponse = await _client.GetAsync($"/api/v1/integrations/risk-profiles/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, lookupResponse.StatusCode);

        var lookup = await lookupResponse.Content.ReadFromJsonAsync<AssurArrQualityRiskProfileResponse>();
        Assert.NotNull(lookup);
        Assert.Equal(created.Id, lookup!.Id);

        var listResponse = await _client.GetAsync("/api/v1/integrations/risk-profiles");
        listResponse.EnsureSuccessStatusCode();
        var profiles = await listResponse.Content.ReadFromJsonAsync<List<AssurArrQualityRiskProfileResponse>>();
        Assert.NotNull(profiles);
        Assert.Contains(profiles!, item => item.TargetType == targetType && item.TargetRef == targetRef);
    }
}
