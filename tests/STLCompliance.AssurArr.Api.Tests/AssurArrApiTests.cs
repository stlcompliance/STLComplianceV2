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
        Assert.Contains(dashboard.Cards, card => card.Key == "critical-nonconformances" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "holds" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "scars" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "audit-findings" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "repeat-issues" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "capa-effectiveness");
        Assert.Contains(dashboard.Cards, card => card.Key == "overdue-capas" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "recently-released-holds");
        Assert.Contains(dashboard.Cards, card => card.Key == "risk-by-site" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "risk-by-supplier" && card.Count >= 1);
        Assert.Contains(dashboard.Cards, card => card.Key == "risk-by-process" && card.Count >= 1);
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
        Assert.Contains(created.EventLog, eventType => eventType == "assurarr.nonconformance.created");

        var createdDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        createdDashboardResponse.EnsureSuccessStatusCode();
        var createdDashboard = await createdDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(createdDashboard);
        Assert.Contains(createdDashboard!.RecentEvents, entry => entry.EventType == "assurarr.nonconformance.created" && entry.SubjectId == created.Id);

        var initialStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/loadarr/test");
        initialStatusResponse.EnsureSuccessStatusCode();
        var initialStatus = await initialStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(initialStatus);
        Assert.Equal("warning", initialStatus!.QualityStatus);
        Assert.Contains(initialStatus.OpenNonconformanceRefs, item => item == created.Number);

        var containmentResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/nonconformances/{created.Id}/status-updates",
            new UpdateAssurArrStatusRequest("containment", "Containment underway."));

        Assert.Equal(HttpStatusCode.OK, containmentResponse.StatusCode);

        var containmentStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/loadarr/test");
        containmentStatusResponse.EnsureSuccessStatusCode();
        var containmentStatus = await containmentStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(containmentStatus);
        Assert.Equal("on_hold", containmentStatus!.QualityStatus);
        Assert.Contains(containmentStatus.OpenNonconformanceRefs, item => item == created.Number);

        var containmentDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        containmentDashboardResponse.EnsureSuccessStatusCode();
        var containmentDashboard = await containmentDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(containmentDashboard);
        Assert.Contains(containmentDashboard!.RecentEvents, entry => entry.EventType == "assurarr.nonconformance.status_changed" && entry.SubjectId == created.Id);

        var detailResponse = await _client.GetAsync($"/api/v1/integrations/nonconformances/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<AssurArrNonconformanceResponse>();
        Assert.NotNull(detail);
        Assert.Equal(created.Id, detail!.Id);
        Assert.Equal(title, detail.Title);
        Assert.Contains(detail.EventLog, eventType => eventType == "assurarr.nonconformance.status_changed");

        var listResponse = await _client.GetAsync("/api/v1/integrations/nonconformances");
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
            "/api/v1/integrations/nonconformances",
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
        Assert.Contains(nonconformance!.EventLog, eventType => eventType == "assurarr.nonconformance.created");

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

        var rootCauseStartedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        rootCauseStartedDashboardResponse.EnsureSuccessStatusCode();
        var rootCauseStartedDashboard = await rootCauseStartedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(rootCauseStartedDashboard);
        Assert.Contains(rootCauseStartedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.root_cause.started" && entry.SubjectId == rootCause.Id);

        var rootCauseDetailResponse = await _client.GetAsync($"/api/v1/nonconformances/{nonconformance.Id}/root-cause-analyses/{rootCause.Id}");
        Assert.Equal(HttpStatusCode.OK, rootCauseDetailResponse.StatusCode);
        var rootCauseDetail = await rootCauseDetailResponse.Content.ReadFromJsonAsync<AssurArrRootCauseAnalysisResponse>();
        Assert.NotNull(rootCauseDetail);
        Assert.Equal(rootCause.Id, rootCauseDetail!.Id);
        Assert.Equal(rootCause.Number, rootCauseDetail.Number);

        var rootCauseListResponse = await _client.GetAsync($"/api/v1/nonconformances/{nonconformance.Id}/root-cause-analyses");
        Assert.Equal(HttpStatusCode.OK, rootCauseListResponse.StatusCode);
        var rootCauses = await rootCauseListResponse.Content.ReadFromJsonAsync<List<AssurArrRootCauseAnalysisResponse>>();
        Assert.NotNull(rootCauses);
        Assert.Contains(rootCauses!, item => item.Id == rootCause.Id);

        var completedRootCauseTitle = $"Test completed root cause {Guid.NewGuid():N}";
        var completedRootCauseResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/root-cause-analyses",
            new CreateAssurArrRootCauseAnalysisRequest(
                completedRootCauseTitle,
                "Automated coverage for completed root cause analysis creation.",
                nonconformance!.Id,
                "completed",
                "fishbone",
                "process",
                "assurarr",
                nonconformance.SourceObjectRef,
                nonconformance.AffectedObjectRefs.ToArray(),
                null,
                ["recordarr:doc:completed-root-cause-test"],
                "Completed review of process checklist.",
                ["missing checklist", "insufficient review"],
                null,
                DateTimeOffset.UtcNow,
                ["recordarr:doc:completed-root-cause-evidence"]));

        Assert.Equal(HttpStatusCode.OK, completedRootCauseResponse.StatusCode);
        var completedRootCause = await completedRootCauseResponse.Content.ReadFromJsonAsync<AssurArrRootCauseAnalysisResponse>();
        Assert.NotNull(completedRootCause);
        Assert.Equal(completedRootCauseTitle, completedRootCause!.Title);

        var rootCauseCompletedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        rootCauseCompletedDashboardResponse.EnsureSuccessStatusCode();
        var rootCauseCompletedDashboard = await rootCauseCompletedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(rootCauseCompletedDashboard);
        Assert.Contains(rootCauseCompletedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.root_cause.completed" && entry.SubjectId == completedRootCause.Id);

        var nonconformanceDetailResponse = await _client.GetAsync($"/api/v1/integrations/nonconformances/{nonconformance.Id}");
        Assert.Equal(HttpStatusCode.OK, nonconformanceDetailResponse.StatusCode);
        var nonconformanceDetail = await nonconformanceDetailResponse.Content.ReadFromJsonAsync<AssurArrNonconformanceResponse>();
        Assert.NotNull(nonconformanceDetail);
        Assert.Equal(rootCause.Number, nonconformanceDetail!.RootCauseRef);
        Assert.Equal("investigation", nonconformanceDetail.Status);
        Assert.Contains(nonconformanceDetail.EventLog, eventType => eventType == "assurarr.nonconformance.status_changed");

        var dispositionPendingResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/nonconformances/{nonconformance.Id}/status-updates",
            new UpdateAssurArrStatusRequest("disposition_pending", "Ready for disposition review."));

        Assert.Equal(HttpStatusCode.OK, dispositionPendingResponse.StatusCode);

        var correctiveActionResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/nonconformances/{nonconformance.Id}/status-updates",
            new UpdateAssurArrStatusRequest("corrective_action", "Corrective action underway."));

        Assert.Equal(HttpStatusCode.OK, correctiveActionResponse.StatusCode);

        var verificationResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/nonconformances/{nonconformance.Id}/status-updates",
            new UpdateAssurArrStatusRequest("verification", "Verification complete."));

        Assert.Equal(HttpStatusCode.OK, verificationResponse.StatusCode);

        var closedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/nonconformances/{nonconformance.Id}/status-updates",
            new UpdateAssurArrStatusRequest("closed", "Nonconformance closed for test coverage."));

        Assert.Equal(HttpStatusCode.OK, closedResponse.StatusCode);

        var nonconformanceDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        nonconformanceDashboardResponse.EnsureSuccessStatusCode();
        var nonconformanceDashboard = await nonconformanceDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(nonconformanceDashboard);
        Assert.Contains(nonconformanceDashboard!.RecentEvents, entry => entry.EventType == "assurarr.nonconformance.status_changed" && entry.SubjectId == nonconformance.Id);
        Assert.Contains(nonconformanceDashboard.RecentEvents, entry => entry.EventType == "assurarr.nonconformance.closed");
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
        Assert.Contains(approvalHold!.EventLog, eventType => eventType == "assurarr.hold.placed");

        var initialHoldStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/loadarr/test");
        initialHoldStatusResponse.EnsureSuccessStatusCode();
        var initialHoldStatus = await initialHoldStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(initialHoldStatus);
        Assert.Equal("on_hold", initialHoldStatus!.QualityStatus);
        Assert.Contains(initialHoldStatus.ActiveHoldRefs, item => item == approvalHold.Number);

        var approvalHoldDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        approvalHoldDashboardResponse.EnsureSuccessStatusCode();
        var approvalHoldDashboard = await approvalHoldDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(approvalHoldDashboard);
        Assert.Contains(approvalHoldDashboard!.RecentEvents, entry => entry.EventType == "assurarr.hold.placed" && entry.SubjectId == approvalHold.Id);

        var approvalHoldDetailResponse = await _client.GetAsync($"/api/v1/integrations/holds/{approvalHold!.Id}");
        Assert.Equal(HttpStatusCode.OK, approvalHoldDetailResponse.StatusCode);
        var approvalHoldDetail = await approvalHoldDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityHoldResponse>();
        Assert.NotNull(approvalHoldDetail);
        Assert.Equal(approvalHold.Id, approvalHoldDetail!.Id);
        Assert.Equal(approvalHold.Number, approvalHoldDetail.Number);
        Assert.Contains(approvalHoldDetail.EventLog, eventType => eventType == "assurarr.hold.placed");

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

        var releaseRequestedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        releaseRequestedDashboardResponse.EnsureSuccessStatusCode();
        var releaseRequestedDashboard = await releaseRequestedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(releaseRequestedDashboard);
        Assert.Contains(releaseRequestedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.hold.release_requested" && entry.SubjectId == approvalHold.Id);

        var holdsAfterRequestResponse = await _client.GetAsync("/api/v1/integrations/holds");
        holdsAfterRequestResponse.EnsureSuccessStatusCode();
        var holdsAfterRequest = await holdsAfterRequestResponse.Content.ReadFromJsonAsync<List<AssurArrQualityHoldResponse>>();
        Assert.NotNull(holdsAfterRequest);
        var requestedHold = holdsAfterRequest!.Single(item => item.Id == approvalHold.Id);
        Assert.Equal("release_pending", requestedHold.Status);
        Assert.NotEmpty(requestedHold.ReleaseRequirements);

        var approvalHoldAfterRequestResponse = await _client.GetAsync($"/api/v1/integrations/holds/{approvalHold.Id}");
        approvalHoldAfterRequestResponse.EnsureSuccessStatusCode();
        var approvalHoldAfterRequest = await approvalHoldAfterRequestResponse.Content.ReadFromJsonAsync<AssurArrQualityHoldResponse>();
        Assert.NotNull(approvalHoldAfterRequest);
        Assert.Contains(approvalHoldAfterRequest!.EventLog, eventType => eventType == "assurarr.hold.release_requested");

        var pendingHoldStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/loadarr/test");
        pendingHoldStatusResponse.EnsureSuccessStatusCode();
        var pendingHoldStatus = await pendingHoldStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(pendingHoldStatus);
        Assert.Equal("conditional_release", pendingHoldStatus!.QualityStatus);
        Assert.Contains(pendingHoldStatus.ActiveHoldRefs, item => item == approvalHold.Number);

        var releaseApprovalResponse = await _client.PostAsJsonAsync(
            $"/api/v1/integrations/holds/{approvalHold.Id}/release",
            new UpdateAssurArrStatusRequest("executed", "Release approved."));

        Assert.Equal(HttpStatusCode.OK, releaseApprovalResponse.StatusCode);
        var approvedRelease = await releaseApprovalResponse.Content.ReadFromJsonAsync<AssurArrQualityReleaseResponse>();
        Assert.NotNull(approvedRelease);
        Assert.Equal("approved", approvedRelease!.Status);

        var holdsAfterReleaseResponse = await _client.GetAsync("/api/v1/integrations/holds");
        holdsAfterReleaseResponse.EnsureSuccessStatusCode();
        var holdsAfterRelease = await holdsAfterReleaseResponse.Content.ReadFromJsonAsync<List<AssurArrQualityHoldResponse>>();
        Assert.NotNull(holdsAfterRelease);
        Assert.Equal("released", holdsAfterRelease!.Single(item => item.Id == approvalHold.Id).Status);

        var approvalHoldAfterReleaseResponse = await _client.GetAsync($"/api/v1/integrations/holds/{approvalHold.Id}");
        approvalHoldAfterReleaseResponse.EnsureSuccessStatusCode();
        var approvalHoldAfterRelease = await approvalHoldAfterReleaseResponse.Content.ReadFromJsonAsync<AssurArrQualityHoldResponse>();
        Assert.NotNull(approvalHoldAfterRelease);
        Assert.Contains(approvalHoldAfterRelease!.EventLog, eventType => eventType == "assurarr.hold.released");

        var releaseDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        releaseDashboardResponse.EnsureSuccessStatusCode();
        var releaseDashboard = await releaseDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(releaseDashboard);
        Assert.Contains(releaseDashboard!.RecentEvents, entry => entry.EventType == "assurarr.hold.released" && entry.SubjectId == approvalHold.Id);

        var releasedHoldStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/loadarr/test");
        releasedHoldStatusResponse.EnsureSuccessStatusCode();
        var releasedHoldStatus = await releasedHoldStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(releasedHoldStatus);
        Assert.Equal("acceptable", releasedHoldStatus!.QualityStatus);
        Assert.DoesNotContain(releasedHoldStatus.ActiveHoldRefs, item => item == approvalHold.Number);

        var cancelHoldTitle = $"Test cancel hold {Guid.NewGuid():N}";
        var cancelHoldResponse = await _client.PostAsJsonAsync(
            "/api/v1/holds",
            new CreateAssurArrQualityHoldRequest(
                cancelHoldTitle,
                "Created for hold cancellation coverage.",
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

        Assert.Equal(HttpStatusCode.OK, cancelHoldResponse.StatusCode);
        var cancelHold = await cancelHoldResponse.Content.ReadFromJsonAsync<AssurArrQualityHoldResponse>();
        Assert.NotNull(cancelHold);
        Assert.Contains(cancelHold!.EventLog, eventType => eventType == "assurarr.hold.placed");

        var cancelInitialStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/loadarr/test");
        cancelInitialStatusResponse.EnsureSuccessStatusCode();
        var cancelInitialStatus = await cancelInitialStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(cancelInitialStatus);
        Assert.Equal("on_hold", cancelInitialStatus!.QualityStatus);
        Assert.Contains(cancelInitialStatus.ActiveHoldRefs, item => item == cancelHold.Number);

        var cancelHoldStatusResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/holds/{cancelHold!.Id}/status",
            new UpdateAssurArrStatusRequest("canceled", "Hold no longer needed."));

        Assert.Equal(HttpStatusCode.OK, cancelHoldStatusResponse.StatusCode);

        var cancelHoldDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        cancelHoldDashboardResponse.EnsureSuccessStatusCode();
        var cancelHoldDashboard = await cancelHoldDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(cancelHoldDashboard);
        Assert.Contains(cancelHoldDashboard!.RecentEvents, entry => entry.EventType == "assurarr.hold.status_changed" && entry.SubjectId == cancelHold.Id);
        Assert.Contains(cancelHoldDashboard!.RecentEvents, entry => entry.EventType == "assurarr.hold.canceled" && entry.SubjectId == cancelHold.Id);
        Assert.Contains(cancelHoldDashboard.RecentEvents, entry => entry.EventType == "assurarr.hold.status_changed" && entry.SubjectId == cancelHold.Id);

        var cancelHoldDetailResponse = await _client.GetAsync($"/api/v1/integrations/holds/{cancelHold.Id}");
        cancelHoldDetailResponse.EnsureSuccessStatusCode();
        var cancelHoldDetail = await cancelHoldDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityHoldResponse>();
        Assert.NotNull(cancelHoldDetail);
        Assert.Contains(cancelHoldDetail!.EventLog, eventType => eventType == "assurarr.hold.canceled");

        var canceledHoldStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/loadarr/test");
        canceledHoldStatusResponse.EnsureSuccessStatusCode();
        var canceledHoldStatus = await canceledHoldStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(canceledHoldStatus);
        Assert.Equal("unknown", canceledHoldStatus!.QualityStatus);
        Assert.DoesNotContain(canceledHoldStatus.ActiveHoldRefs, item => item == cancelHold.Number);

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
        Assert.Contains(rejectHold!.EventLog, eventType => eventType == "assurarr.hold.placed");

        var rejectInitialStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/loadarr/test");
        rejectInitialStatusResponse.EnsureSuccessStatusCode();
        var rejectInitialStatus = await rejectInitialStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(rejectInitialStatus);
        Assert.Equal("on_hold", rejectInitialStatus!.QualityStatus);
        Assert.Contains(rejectInitialStatus.ActiveHoldRefs, item => item == rejectHold.Number);

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

        var rejectedHoldStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/loadarr/test");
        rejectedHoldStatusResponse.EnsureSuccessStatusCode();
        var rejectedHoldStatus = await rejectedHoldStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(rejectedHoldStatus);
        Assert.Equal("unknown", rejectedHoldStatus!.QualityStatus);
        Assert.DoesNotContain(rejectedHoldStatus.ActiveHoldRefs, item => item == rejectHold.Number);

        var rejectedHoldDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        rejectedHoldDashboardResponse.EnsureSuccessStatusCode();
        var rejectedHoldDashboard = await rejectedHoldDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(rejectedHoldDashboard);
        Assert.Contains(rejectedHoldDashboard!.RecentEvents, entry => entry.EventType == "assurarr.hold.rejected" && entry.SubjectId == rejectHold.Id);

        var holdsAfterRejectResponse = await _client.GetAsync("/api/v1/integrations/holds");
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

        var reviewRequestedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        reviewRequestedDashboardResponse.EnsureSuccessStatusCode();
        var reviewRequestedDashboard = await reviewRequestedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(reviewRequestedDashboard);
        Assert.Contains(reviewRequestedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.quality_review.requested" && entry.SubjectId == review.Id);

        var reviewDetailResponse = await _client.GetAsync($"/api/v1/integrations/quality-reviews/{review.Id}");
        Assert.Equal(HttpStatusCode.OK, reviewDetailResponse.StatusCode);
        var reviewDetail = await reviewDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityReviewResponse>();
        Assert.NotNull(reviewDetail);
        Assert.Equal(review.Id, reviewDetail!.Id);
        Assert.Equal(review.Number, reviewDetail.Number);

        var reviewInProgressResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/quality-reviews/{review.Id}/status",
            new UpdateAssurArrStatusRequest("in_review", "Review in progress."));

        Assert.Equal(HttpStatusCode.OK, reviewInProgressResponse.StatusCode);

        var approvedReviewResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/quality-reviews/{review.Id}/status",
            new UpdateAssurArrStatusRequest("approved", "Review approved."));

        Assert.Equal(HttpStatusCode.OK, approvedReviewResponse.StatusCode);

        var approvedReviewDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        approvedReviewDashboardResponse.EnsureSuccessStatusCode();
        var approvedReviewDashboard = await approvedReviewDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(approvedReviewDashboard);
        Assert.Contains(approvedReviewDashboard!.RecentEvents, entry => entry.EventType == "assurarr.quality_review.approved");

        var rejectedReviewTitle = $"Test quality review reject {Guid.NewGuid():N}";
        var rejectedReviewResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/quality-reviews",
            new CreateAssurArrQualityReviewRequest(
                rejectedReviewTitle,
                "Automated coverage for rejected quality review workflow.",
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
                "Reject review notes"));

        Assert.Equal(HttpStatusCode.OK, rejectedReviewResponse.StatusCode);
        var rejectedReview = await rejectedReviewResponse.Content.ReadFromJsonAsync<AssurArrQualityReviewResponse>();
        Assert.NotNull(rejectedReview);

        var rejectedReviewInProgressResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/quality-reviews/{rejectedReview!.Id}/status",
            new UpdateAssurArrStatusRequest("in_review", "Review in progress."));

        Assert.Equal(HttpStatusCode.OK, rejectedReviewInProgressResponse.StatusCode);

        var rejectedReviewStatusResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/quality-reviews/{rejectedReview.Id}/status",
            new UpdateAssurArrStatusRequest("rejected", "Review rejected."));

        Assert.Equal(HttpStatusCode.OK, rejectedReviewStatusResponse.StatusCode);

        var rejectedReviewDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        rejectedReviewDashboardResponse.EnsureSuccessStatusCode();
        var rejectedReviewDashboard = await rejectedReviewDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(rejectedReviewDashboard);
        Assert.Contains(rejectedReviewDashboard!.RecentEvents, entry => entry.EventType == "assurarr.quality_review.rejected" && entry.SubjectId == rejectedReview!.Id);
        Assert.Contains(rejectedReviewDashboard.RecentEvents, entry => entry.EventType == "assurarr.quality_review.requested" && entry.SubjectId == rejectedReview.Id);

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

        var releaseRequestedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        releaseRequestedDashboardResponse.EnsureSuccessStatusCode();
        var releaseRequestedDashboard = await releaseRequestedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(releaseRequestedDashboard);
        Assert.Contains(releaseRequestedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.quality_release.requested" && entry.SubjectId == release.Id);

        var releaseDetailResponse = await _client.GetAsync($"/api/v1/integrations/quality-releases/{release.Id}");
        Assert.Equal(HttpStatusCode.OK, releaseDetailResponse.StatusCode);
        var releaseDetail = await releaseDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityReleaseResponse>();
        Assert.NotNull(releaseDetail);
        Assert.Equal(release.Id, releaseDetail!.Id);
        Assert.Equal(release.Number, releaseDetail.Number);

        var releasePendingResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/quality-releases/{release.Id}/status",
            new UpdateAssurArrStatusRequest("pending_review", "Release under review."));

        Assert.Equal(HttpStatusCode.OK, releasePendingResponse.StatusCode);

        var releaseApprovedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/quality-releases/{release.Id}/status",
            new UpdateAssurArrStatusRequest("approved", "Release approved."));

        Assert.Equal(HttpStatusCode.OK, releaseApprovedResponse.StatusCode);

        var releaseExecutedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/quality-releases/{release.Id}/status",
            new UpdateAssurArrStatusRequest("executed", "Release executed."));

        Assert.Equal(HttpStatusCode.OK, releaseExecutedResponse.StatusCode);

        var releaseDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        releaseDashboardResponse.EnsureSuccessStatusCode();
        var releaseDashboard = await releaseDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(releaseDashboard);
        Assert.Contains(releaseDashboard!.RecentEvents, entry => entry.EventType == "assurarr.quality_release.approved");
        Assert.Contains(releaseDashboard.RecentEvents, entry => entry.EventType == "assurarr.quality_release.executed");

        var rejectedReleaseTitle = $"Test rejected quality release {Guid.NewGuid():N}";
        var rejectedReleaseResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/quality-releases",
            new CreateAssurArrQualityReleaseRequest(
                rejectedReleaseTitle,
                "Automated coverage for rejected quality release workflow.",
                "low",
                "assurarr",
                "HOLD-000001",
                ["loadarr:inventory:test"],
                null,
                "HOLD-000001",
                "full",
                null,
                DateTimeOffset.UtcNow,
                "Rejected release review evidence.",
                DateTimeOffset.UtcNow.AddDays(1),
                ["recordarr:doc:test"],
                "Rejected release notes"));

        Assert.Equal(HttpStatusCode.OK, rejectedReleaseResponse.StatusCode);
        var rejectedRelease = await rejectedReleaseResponse.Content.ReadFromJsonAsync<AssurArrQualityReleaseResponse>();
        Assert.NotNull(rejectedRelease);

        var rejectedReleasePendingResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/quality-releases/{rejectedRelease!.Id}/status",
            new UpdateAssurArrStatusRequest("pending_review", "Release pending review."));

        Assert.Equal(HttpStatusCode.OK, rejectedReleasePendingResponse.StatusCode);

        var rejectedReleaseStatusResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/quality-releases/{rejectedRelease!.Id}/status",
            new UpdateAssurArrStatusRequest("rejected", "Release rejected."));

        Assert.Equal(HttpStatusCode.OK, rejectedReleaseStatusResponse.StatusCode);

        var rejectedReleaseDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        rejectedReleaseDashboardResponse.EnsureSuccessStatusCode();
        var rejectedReleaseDashboard = await rejectedReleaseDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(rejectedReleaseDashboard);
        Assert.Contains(rejectedReleaseDashboard!.RecentEvents, entry => entry.EventType == "assurarr.quality_release.rejected" && entry.SubjectId == rejectedRelease.Id);

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
            "/api/v1/integrations/capas",
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
        Assert.Contains(capa!.EventLog, eventType => eventType == "assurarr.capa.created");

        var capaCreatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        capaCreatedDashboardResponse.EnsureSuccessStatusCode();
        var capaCreatedDashboard = await capaCreatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(capaCreatedDashboard);
        Assert.Contains(capaCreatedDashboard!.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.created" && eventItem.SubjectId == capa.Id);

        var capaDetailResponse = await _client.GetAsync($"/api/v1/integrations/capas/{capa!.Id}");
        Assert.Equal(HttpStatusCode.OK, capaDetailResponse.StatusCode);
        var capaDetail = await capaDetailResponse.Content.ReadFromJsonAsync<AssurArrCapaResponse>();
        Assert.NotNull(capaDetail);
        Assert.Equal(capa.Id, capaDetail!.Id);
        Assert.Equal(capa.Number, capaDetail.Number);
        Assert.Contains(capaDetail.EventLog, eventType => eventType == "assurarr.capa.created");

        var actionTitle = $"Test CAPA action {Guid.NewGuid():N}";
        var actionResponse = await _client.PostAsJsonAsync(
            $"/api/v1/integrations/capas/{capa!.Id}/actions",
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

        var capaAfterActionResponse = await _client.GetAsync($"/api/v1/integrations/capas/{capa.Id}");
        Assert.Equal(HttpStatusCode.OK, capaAfterActionResponse.StatusCode);
        var capaAfterAction = await capaAfterActionResponse.Content.ReadFromJsonAsync<AssurArrCapaResponse>();
        Assert.NotNull(capaAfterAction);
        Assert.Contains(capaAfterAction!.EventLog, eventType => eventType == "assurarr.capa.action_assigned");

        var actionDetailResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}/actions/{action.Id}");
        Assert.Equal(HttpStatusCode.OK, actionDetailResponse.StatusCode);
        var actionDetail = await actionDetailResponse.Content.ReadFromJsonAsync<AssurArrCapaActionResponse>();
        Assert.NotNull(actionDetail);
        Assert.Equal(action.Id, actionDetail!.Id);
        Assert.Equal(action.Number, actionDetail.Number);

        var actionAssignedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        actionAssignedDashboardResponse.EnsureSuccessStatusCode();
        var actionAssignedDashboard = await actionAssignedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(actionAssignedDashboard);
        Assert.Contains(actionAssignedDashboard!.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.action_assigned");

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

        var blockerDetailResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}/actions/{action.Id}/blockers/{blocker.Id}");
        Assert.Equal(HttpStatusCode.OK, blockerDetailResponse.StatusCode);
        var blockerDetail = await blockerDetailResponse.Content.ReadFromJsonAsync<AssurArrCapaActionBlockerResponse>();
        Assert.NotNull(blockerDetail);
        Assert.Equal(blocker.Id, blockerDetail!.Id);
        Assert.Equal(blocker.Number, blockerDetail.Number);

        var blockerListResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}/actions/{action.Id}/blockers");
        blockerListResponse.EnsureSuccessStatusCode();
        var blockers = await blockerListResponse.Content.ReadFromJsonAsync<List<AssurArrCapaActionBlockerResponse>>();
        Assert.NotNull(blockers);
        Assert.Contains(blockers!, item => item.Title == blockerTitle);

        var resolveBlockerResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/actions/{action.Id}/blockers/{blocker.Id}/status",
            new UpdateAssurArrCapaActionBlockerStatusRequest("resolved", null, DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.OK, resolveBlockerResponse.StatusCode);

        var completeActionResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/actions/{action.Id}/status",
            new UpdateAssurArrCapaActionStatusRequest(
                "completed",
                null,
                DateTimeOffset.UtcNow,
                null,
                null,
                "Action completed and ready for verification."));

        Assert.Equal(HttpStatusCode.OK, completeActionResponse.StatusCode);

        var verifyActionResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/actions/{action.Id}/status",
            new UpdateAssurArrCapaActionStatusRequest(
                "verified",
                null,
                null,
                null,
                DateTimeOffset.UtcNow,
                "Action verified by quality reviewer."));

        Assert.Equal(HttpStatusCode.OK, verifyActionResponse.StatusCode);

        var dashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        dashboardResponse.EnsureSuccessStatusCode();
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(dashboard);
        Assert.Contains(dashboard!.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.action_completed");
        Assert.Contains(dashboard.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.action_verified");

        var capaRootCauseResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/status",
            new UpdateAssurArrStatusRequest("root_cause", "Root cause analysis in progress."));

        Assert.Equal(HttpStatusCode.OK, capaRootCauseResponse.StatusCode);

        var capaActionPlanResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{capa.Id}/status",
            new UpdateAssurArrStatusRequest("action_plan", "Action plan defined."));

        Assert.Equal(HttpStatusCode.OK, capaActionPlanResponse.StatusCode);

        var capaActionPlanDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        capaActionPlanDashboardResponse.EnsureSuccessStatusCode();
        var capaActionPlanDashboard = await capaActionPlanDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(capaActionPlanDashboard);
        Assert.Contains(capaActionPlanDashboard!.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.root_cause_completed");
        Assert.Contains(capaActionPlanDashboard.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.action_plan_created");

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
            $"/api/v1/integrations/capas/{capa.Id}/verification",
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

        var verificationDetailResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}/verification-plans/{verification.Id}");
        Assert.Equal(HttpStatusCode.OK, verificationDetailResponse.StatusCode);
        var verificationDetail = await verificationDetailResponse.Content.ReadFromJsonAsync<AssurArrVerificationPlanResponse>();
        Assert.NotNull(verificationDetail);
        Assert.Equal(verification.Id, verificationDetail!.Id);
        Assert.Equal(verification.Number, verificationDetail.Number);

        var capaVerificationDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        capaVerificationDashboardResponse.EnsureSuccessStatusCode();
        var capaVerificationDashboard = await capaVerificationDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(capaVerificationDashboard);
        Assert.Contains(capaVerificationDashboard!.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.verification_started");

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

        var effectivenessDetailResponse = await _client.GetAsync($"/api/v1/capas/{capa.Id}/effectiveness-verifications/{effectiveness.Id}");
        Assert.Equal(HttpStatusCode.OK, effectivenessDetailResponse.StatusCode);
        var effectivenessDetail = await effectivenessDetailResponse.Content.ReadFromJsonAsync<AssurArrEffectivenessVerificationResponse>();
        Assert.NotNull(effectivenessDetail);
        Assert.Equal(effectiveness.Id, effectivenessDetail!.Id);
        Assert.Equal(effectiveness.Number, effectivenessDetail.Number);

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

        var capaClosedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        capaClosedDashboardResponse.EnsureSuccessStatusCode();
        var capaClosedDashboard = await capaClosedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(capaClosedDashboard);
        Assert.Contains(capaClosedDashboard!.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.verified_effective");
        Assert.Contains(capaClosedDashboard.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.closed");

        var reopenedCapaTitle = $"Test reopened CAPA {Guid.NewGuid():N}";
        var reopenedCapaResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/capas",
            new CreateAssurArrCapaRequest(
                reopenedCapaTitle,
                "Automated coverage for ineffective CAPA verification.",
                "high",
                "corrective_and_preventive",
                "manual",
                "assurarr",
                "workflow:capa:reopen:test",
                ["loadarr:inventory:test"],
                null,
                null,
                "Awaiting verification retry",
                DateTimeOffset.UtcNow.AddDays(7),
                ["NCR-000001"],
                ["FIND-000001"],
                []));

        Assert.Equal(HttpStatusCode.OK, reopenedCapaResponse.StatusCode);
        var reopenedCapa = await reopenedCapaResponse.Content.ReadFromJsonAsync<AssurArrCapaResponse>();
        Assert.NotNull(reopenedCapa);

        var reopenedRootCauseResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{reopenedCapa!.Id}/status",
            new UpdateAssurArrStatusRequest("root_cause", "Reopened CAPA root cause review started."));

        Assert.Equal(HttpStatusCode.OK, reopenedRootCauseResponse.StatusCode);

        var reopenedActionPlanResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{reopenedCapa.Id}/status",
            new UpdateAssurArrStatusRequest("action_plan", "Reopened CAPA action plan defined."));

        Assert.Equal(HttpStatusCode.OK, reopenedActionPlanResponse.StatusCode);

        var reopenedImplementationResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{reopenedCapa.Id}/status",
            new UpdateAssurArrStatusRequest("implementation", "Reopened CAPA actions underway."));

        Assert.Equal(HttpStatusCode.OK, reopenedImplementationResponse.StatusCode);

        var reopenedVerificationStageResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{reopenedCapa.Id}/status",
            new UpdateAssurArrStatusRequest("verification", "Reopened CAPA ready for verification."));

        Assert.Equal(HttpStatusCode.OK, reopenedVerificationStageResponse.StatusCode);

        var reopenedVerificationResponse = await _client.PostAsJsonAsync(
            $"/api/v1/capas/{reopenedCapa!.Id}/effectiveness-verifications",
            new CreateAssurArrEffectivenessVerificationRequest(
                null,
                "scheduled",
                null,
                null,
                "Reverification scheduled after reopened CAPA.",
                ["recordarr:doc:test"],
                ["actions_completed=0", "open_nc_count=1"],
                true,
                true,
                reopenedCapa.Number));

        Assert.Equal(HttpStatusCode.OK, reopenedVerificationResponse.StatusCode);
        var reopenedVerification = await reopenedVerificationResponse.Content.ReadFromJsonAsync<AssurArrEffectivenessVerificationResponse>();
        Assert.NotNull(reopenedVerification);

        var reopenedVerificationStatusResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/capas/{reopenedCapa.Id}/effectiveness-verifications/{reopenedVerification!.Id}/status",
            new UpdateAssurArrEffectivenessVerificationStatusRequest(
                "ineffective",
                "Verification found the corrective action ineffective.",
                true,
                true,
                reopenedCapa.Id.ToString()));

        Assert.Equal(HttpStatusCode.OK, reopenedVerificationStatusResponse.StatusCode);

        var reopenedCapaResponseAfterVerification = await _client.GetAsync($"/api/v1/integrations/capas/{reopenedCapa.Id}");
        reopenedCapaResponseAfterVerification.EnsureSuccessStatusCode();
        var reopenedCapaAfterVerification = await reopenedCapaResponseAfterVerification.Content.ReadFromJsonAsync<AssurArrCapaResponse>();
        Assert.NotNull(reopenedCapaAfterVerification);
        Assert.Equal("ineffective", reopenedCapaAfterVerification!.Status);

        var reopenedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        reopenedDashboardResponse.EnsureSuccessStatusCode();
        var reopenedDashboard = await reopenedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(reopenedDashboard);
        Assert.Contains(reopenedDashboard!.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.reopened");
        Assert.Contains(reopenedDashboard.RecentEvents, eventItem => eventItem.EventType == "assurarr.capa.verified_ineffective");
    }

    [Fact]
    public async Task Can_create_audit_checklists_and_items()
    {
        var auditTitle = $"Test audit {Guid.NewGuid():N}";
        var auditResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/audits",
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

        var auditCreatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        auditCreatedDashboardResponse.EnsureSuccessStatusCode();
        var auditCreatedDashboard = await auditCreatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(auditCreatedDashboard);
        Assert.Contains(auditCreatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.audit.created" && entry.SubjectId == audit.Id);

        var auditDetailResponse = await _client.GetAsync($"/api/v1/audits/{audit!.Id}");
        Assert.Equal(HttpStatusCode.OK, auditDetailResponse.StatusCode);
        var auditDetail = await auditDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityAuditResponse>();
        Assert.NotNull(auditDetail);
        Assert.Equal(audit.Id, auditDetail!.Id);
        Assert.Equal(audit.Number, auditDetail.Number);

        var startAuditResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/audits/{audit.Id}/status",
            new UpdateAssurArrStatusRequest("in_progress", "Audit execution started."));

        Assert.Equal(HttpStatusCode.OK, startAuditResponse.StatusCode);

        var startedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        startedDashboardResponse.EnsureSuccessStatusCode();
        var startedDashboard = await startedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(startedDashboard);
        Assert.Contains(startedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.audit.status_changed" && entry.SubjectId == audit.Id);
        Assert.Contains(startedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.audit.started" && entry.SubjectId == audit.Id);

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

        var checklistCreatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        checklistCreatedDashboardResponse.EnsureSuccessStatusCode();
        var checklistCreatedDashboard = await checklistCreatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(checklistCreatedDashboard);
        Assert.Contains(checklistCreatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.audit.checklist_created" && entry.SubjectId == audit.Id);

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

        var checklistDetailResponse = await _client.GetAsync($"/api/v1/audits/{audit.Id}/checklists/{checklist.Id}");
        Assert.Equal(HttpStatusCode.OK, checklistDetailResponse.StatusCode);
        var checklistDetail = await checklistDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityAuditChecklistResponse>();
        Assert.NotNull(checklistDetail);
        Assert.Equal(checklist.Id, checklistDetail!.Id);
        Assert.Equal(checklist.Number, checklistDetail.Number);

        var itemDetailResponse = await _client.GetAsync($"/api/v1/audits/{audit.Id}/checklists/{checklist.Id}/items/{item.Id}");
        Assert.Equal(HttpStatusCode.OK, itemDetailResponse.StatusCode);
        var itemDetail = await itemDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityAuditChecklistItemResponse>();
        Assert.NotNull(itemDetail);
        Assert.Equal(item.Id, itemDetail!.Id);
        Assert.Equal(item.Number, itemDetail.Number);

        var itemCreatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        itemCreatedDashboardResponse.EnsureSuccessStatusCode();
        var itemCreatedDashboard = await itemCreatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(itemCreatedDashboard);
        Assert.Contains(itemCreatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.audit.checklist.item_created" && entry.SubjectId == checklist.Id);

        var findingTitle = $"Test finding {Guid.NewGuid():N}";
        var findingResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/findings",
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

        var findingCreatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        findingCreatedDashboardResponse.EnsureSuccessStatusCode();
        var findingCreatedDashboard = await findingCreatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(findingCreatedDashboard);
        Assert.Contains(findingCreatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.finding.created" && entry.SubjectId == finding.Id);
        Assert.Contains(findingCreatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.audit.finding_created" && entry.SubjectId == finding.Id);

        var acceptFindingResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/findings/{finding.Id}/status",
            new UpdateAssurArrStatusRequest("accepted", "Finding accepted for follow-up."));

        Assert.Equal(HttpStatusCode.OK, acceptFindingResponse.StatusCode);

        var escalateFindingResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/findings/{finding.Id}/status",
            new UpdateAssurArrStatusRequest("nonconformance_created", "Finding escalated to a nonconformance."));

        Assert.Equal(HttpStatusCode.OK, escalateFindingResponse.StatusCode);

        var findingStatusDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        findingStatusDashboardResponse.EnsureSuccessStatusCode();
        var findingStatusDashboard = await findingStatusDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(findingStatusDashboard);
        Assert.Contains(findingStatusDashboard!.RecentEvents, entry => entry.EventType == "assurarr.finding.status_changed" && entry.SubjectId == finding.Id);
        Assert.Contains(findingStatusDashboard!.RecentEvents, entry => entry.EventType == "assurarr.finding.accepted" && entry.SubjectId == finding.Id);
        Assert.Contains(findingStatusDashboard.RecentEvents, entry => entry.EventType == "assurarr.finding.nonconformance_created" && entry.SubjectId == finding.Id);

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

        var itemAnsweredDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        itemAnsweredDashboardResponse.EnsureSuccessStatusCode();
        var itemAnsweredDashboard = await itemAnsweredDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(itemAnsweredDashboard);
        Assert.Contains(itemAnsweredDashboard!.RecentEvents, entry => entry.EventType == "assurarr.audit.checklist.item_answered");

        var reviewAuditResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/audits/{audit.Id}/status",
            new UpdateAssurArrStatusRequest("findings_review", "Audit findings reviewed."));

        Assert.Equal(HttpStatusCode.OK, reviewAuditResponse.StatusCode);

        var closeFindingResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/findings/{finding.Id}/status",
            new UpdateAssurArrStatusRequest("closed", "Finding closed after escalation."));

        Assert.Equal(HttpStatusCode.OK, closeFindingResponse.StatusCode);

        var closeAuditResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/audits/{audit.Id}/status",
            new UpdateAssurArrStatusRequest("closed", "Audit completed after checklist review."));

        Assert.Equal(HttpStatusCode.OK, closeAuditResponse.StatusCode);

        var dashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        dashboardResponse.EnsureSuccessStatusCode();
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(dashboard);
        Assert.Contains(dashboard.RecentEvents, entry => entry.EventType == "assurarr.finding.closed");
        Assert.Contains(dashboard.RecentEvents, entry => entry.EventType == "assurarr.audit.closed");
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

        var supplierCreatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        supplierCreatedDashboardResponse.EnsureSuccessStatusCode();
        var supplierCreatedDashboard = await supplierCreatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(supplierCreatedDashboard);
        Assert.Contains(supplierCreatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.supplier_quality_issue.created" && entry.SubjectId == supplierIssue.Id);

        var supplierDetailResponse = await _client.GetAsync($"/api/v1/integrations/supplier-quality-issues/{supplierIssue.Id}");
        Assert.Equal(HttpStatusCode.OK, supplierDetailResponse.StatusCode);
        var supplierDetail = await supplierDetailResponse.Content.ReadFromJsonAsync<AssurArrSupplierQualityIssueResponse>();
        Assert.NotNull(supplierDetail);
        Assert.Equal(supplierIssue.Id, supplierDetail!.Id);
        Assert.Equal(supplierIssue.Number, supplierDetail.Number);

        var supplierInitialStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/supplyarr/test");
        supplierInitialStatusResponse.EnsureSuccessStatusCode();
        var supplierInitialStatus = await supplierInitialStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(supplierInitialStatus);
        Assert.Equal("warning", supplierInitialStatus!.QualityStatus);
        Assert.Equal("loadarr:receipt:test", supplierInitialStatus.TargetObjectRef);
        Assert.Contains(supplierInitialStatus.OpenNonconformanceRefs, item => item == "NCR-000001");

        var supplierNotifiedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/supplier-quality-issues/{supplierIssue.Id}/status",
            new UpdateAssurArrStatusRequest("supplier_notified", "Supplier notified."));

        Assert.Equal(HttpStatusCode.OK, supplierNotifiedResponse.StatusCode);

        var supplierResponsePending = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/supplier-quality-issues/{supplierIssue.Id}/status",
            new UpdateAssurArrStatusRequest("response_pending", "Supplier response requested."));

        Assert.Equal(HttpStatusCode.OK, supplierResponsePending.StatusCode);

        var supplierResolvedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/supplier-quality-issues/{supplierIssue.Id}/status",
            new UpdateAssurArrStatusRequest("resolved", "Supplier issue resolved."));

        Assert.Equal(HttpStatusCode.OK, supplierResolvedResponse.StatusCode);

        var supplierClosedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/supplier-quality-issues/{supplierIssue.Id}/status",
            new UpdateAssurArrStatusRequest("closed", "Supplier issue closed."));

        Assert.Equal(HttpStatusCode.OK, supplierClosedResponse.StatusCode);

        var supplierClosedStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/supplyarr/test");
        supplierClosedStatusResponse.EnsureSuccessStatusCode();
        var supplierClosedStatus = await supplierClosedStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(supplierClosedStatus);
        Assert.Equal("acceptable", supplierClosedStatus!.QualityStatus);
        Assert.Empty(supplierClosedStatus.OpenNonconformanceRefs);

        var supplierDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        supplierDashboardResponse.EnsureSuccessStatusCode();
        var supplierDashboard = await supplierDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(supplierDashboard);
        Assert.Contains(supplierDashboard!.RecentEvents, entry => entry.EventType == "assurarr.supplier_quality_issue.status_changed" && entry.SubjectId == supplierIssue.Id);
        Assert.Contains(supplierDashboard.RecentEvents, entry => entry.EventType == "assurarr.supplier_quality_issue.closed" && entry.SubjectId == supplierIssue.Id);

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

        var complaintCreatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        complaintCreatedDashboardResponse.EnsureSuccessStatusCode();
        var complaintCreatedDashboard = await complaintCreatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(complaintCreatedDashboard);
        Assert.Contains(complaintCreatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.customer_complaint.created" && entry.SubjectId == complaint.Id);

        var complaintDetailResponse = await _client.GetAsync($"/api/v1/integrations/customer-complaint-quality-cases/{complaint.Id}");
        Assert.Equal(HttpStatusCode.OK, complaintDetailResponse.StatusCode);
        var complaintDetail = await complaintDetailResponse.Content.ReadFromJsonAsync<AssurArrCustomerComplaintQualityCaseResponse>();
        Assert.NotNull(complaintDetail);
        Assert.Equal(complaint.Id, complaintDetail!.Id);
        Assert.Equal(complaint.Number, complaintDetail.Number);

        var complaintInitialStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/customarr/test");
        complaintInitialStatusResponse.EnsureSuccessStatusCode();
        var complaintInitialStatus = await complaintInitialStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(complaintInitialStatus);
        Assert.Equal("warning", complaintInitialStatus!.QualityStatus);
        Assert.Equal("routarr:shipment:test", complaintInitialStatus.TargetObjectRef);
        Assert.Contains(complaintInitialStatus.OpenNonconformanceRefs, item => item == "NCR-000001");
        Assert.Contains(complaintInitialStatus.OpenCapaRefs, item => item == "CAPA-000001");

        var complaintTriageResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/customer-complaint-quality-cases/{complaint.Id}/status",
            new UpdateAssurArrStatusRequest("triage", "Complaint triaged."));

        Assert.Equal(HttpStatusCode.OK, complaintTriageResponse.StatusCode);

        var complaintResponsePending = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/customer-complaint-quality-cases/{complaint.Id}/status",
            new UpdateAssurArrStatusRequest("response_pending", "Customer response prepared."));

        Assert.Equal(HttpStatusCode.OK, complaintResponsePending.StatusCode);

        var complaintPendingStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/customarr/test");
        complaintPendingStatusResponse.EnsureSuccessStatusCode();
        var complaintPendingStatus = await complaintPendingStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(complaintPendingStatus);
        Assert.Equal("under_review", complaintPendingStatus!.QualityStatus);
        Assert.Contains(complaintPendingStatus.OpenCapaRefs, item => item == "CAPA-000001");

        var complaintClosedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/customer-complaint-quality-cases/{complaint.Id}/status",
            new UpdateAssurArrStatusRequest("closed", "Complaint closed after response."));

        Assert.Equal(HttpStatusCode.OK, complaintClosedResponse.StatusCode);

        var complaintClosedStatusResponse = await _client.GetAsync("/api/v1/integrations/quality-status/customarr/test");
        complaintClosedStatusResponse.EnsureSuccessStatusCode();
        var complaintClosedStatus = await complaintClosedStatusResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(complaintClosedStatus);
        Assert.Equal("acceptable", complaintClosedStatus!.QualityStatus);
        Assert.Empty(complaintClosedStatus.OpenNonconformanceRefs);
        Assert.Empty(complaintClosedStatus.OpenCapaRefs);

        var complaintDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        complaintDashboardResponse.EnsureSuccessStatusCode();
        var complaintDashboard = await complaintDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(complaintDashboard);
        Assert.Contains(complaintDashboard!.RecentEvents, entry => entry.EventType == "assurarr.customer_complaint.status_changed" && entry.SubjectId == complaint.Id);
        Assert.Contains(complaintDashboard!.RecentEvents, entry => entry.EventType == "assurarr.customer_complaint.response_sent" && entry.SubjectId == complaint.Id);
        Assert.Contains(complaintDashboard.RecentEvents, entry => entry.EventType == "assurarr.customer_complaint.closed" && entry.SubjectId == complaint.Id);

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

        var rejectedTitle = $"Test rejected SCAR {Guid.NewGuid():N}";
        var rejectedCreateResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/scars",
            new CreateAssurArrSupplierCorrectiveActionRequest(
                rejectedTitle,
                "Automated coverage for rejected SCARs.",
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

        Assert.Equal(HttpStatusCode.OK, rejectedCreateResponse.StatusCode);
        var rejectedScar = await rejectedCreateResponse.Content.ReadFromJsonAsync<AssurArrSupplierCorrectiveActionRequestResponse>();
        Assert.NotNull(rejectedScar);

        var rejectedScarSentResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/scars/{rejectedScar!.Id}/status",
            new UpdateAssurArrStatusRequest("sent", "Supplier response sent."));

        Assert.Equal(HttpStatusCode.OK, rejectedScarSentResponse.StatusCode);

        var rejectedScarResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/scars/{rejectedScar!.Id}/status",
            new UpdateAssurArrStatusRequest("rejected", "Supplier response rejected."));

        Assert.Equal(HttpStatusCode.OK, rejectedScarResponse.StatusCode);

        var rejectedScarDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        rejectedScarDashboardResponse.EnsureSuccessStatusCode();
        var rejectedScarDashboard = await rejectedScarDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(rejectedScarDashboard);
        Assert.Contains(rejectedScarDashboard!.RecentEvents, entry => entry.EventType == "assurarr.scar.rejected" && entry.SubjectId == rejectedScar.Id);
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

        var createdDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        createdDashboardResponse.EnsureSuccessStatusCode();
        var createdDashboard = await createdDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(createdDashboard);
        Assert.Contains(createdDashboard!.RecentEvents, entry => entry.EventType == "assurarr.scar.created" && entry.SubjectId == created.Id);

        var listResponse = await _client.GetAsync("/api/v1/integrations/scars");
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<List<AssurArrSupplierCorrectiveActionRequestResponse>>();
        Assert.NotNull(list);
        Assert.Contains(list!, item => item.Title == title);

        var sentResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/scars/{created.Id}/status",
            new UpdateAssurArrStatusRequest("sent", "Ready for supplier transmission."));

        Assert.Equal(HttpStatusCode.OK, sentResponse.StatusCode);

        var sentDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        sentDashboardResponse.EnsureSuccessStatusCode();
        var sentDashboard = await sentDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(sentDashboard);
        Assert.Contains(sentDashboard!.RecentEvents, entry => entry.EventType == "assurarr.scar.sent" && entry.SubjectId == created.Id);

        var responseReceivedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/scars/{created.Id}/status",
            new UpdateAssurArrStatusRequest("response_received", "Supplier response received."));

        Assert.Equal(HttpStatusCode.OK, responseReceivedResponse.StatusCode);

        var responseReceivedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        responseReceivedDashboardResponse.EnsureSuccessStatusCode();
        var responseReceivedDashboard = await responseReceivedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(responseReceivedDashboard);
        Assert.Contains(responseReceivedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.scar.response_received" && entry.SubjectId == created.Id);

        var acceptedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/scars/{created.Id}/status",
            new UpdateAssurArrStatusRequest("accepted", "Supplier response accepted."));

        Assert.Equal(HttpStatusCode.OK, acceptedResponse.StatusCode);

        var acceptedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        acceptedDashboardResponse.EnsureSuccessStatusCode();
        var acceptedDashboard = await acceptedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(acceptedDashboard);
        Assert.Contains(acceptedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.scar.accepted" && entry.SubjectId == created.Id);

        var closedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/scars/{created.Id}/status",
            new UpdateAssurArrStatusRequest("closed", "Supplier corrective action request closed."));

        Assert.Equal(HttpStatusCode.OK, closedResponse.StatusCode);

        var closedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        closedDashboardResponse.EnsureSuccessStatusCode();
        var closedDashboard = await closedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(closedDashboard);
        Assert.Contains(closedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.scar.closed" && entry.SubjectId == created.Id);
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

        var containmentCreatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        containmentCreatedDashboardResponse.EnsureSuccessStatusCode();
        var containmentCreatedDashboard = await containmentCreatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(containmentCreatedDashboard);
        Assert.Contains(containmentCreatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.containment.created" && entry.SubjectId == containment.Id);

        var containmentDetailResponse = await _client.GetAsync($"/api/v1/integrations/containment-actions/{containment.Id}");
        Assert.Equal(HttpStatusCode.OK, containmentDetailResponse.StatusCode);
        var containmentDetail = await containmentDetailResponse.Content.ReadFromJsonAsync<AssurArrContainmentActionResponse>();
        Assert.NotNull(containmentDetail);
        Assert.Equal(containment.Id, containmentDetail!.Id);
        Assert.Equal(containment.Number, containmentDetail.Number);

        var containmentAssignedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/containment-actions/{containment.Id}/status",
            new UpdateAssurArrStatusRequest("assigned", "Containment assigned."));

        Assert.Equal(HttpStatusCode.OK, containmentAssignedResponse.StatusCode);

        var containmentAssignedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        containmentAssignedDashboardResponse.EnsureSuccessStatusCode();
        var containmentAssignedDashboard = await containmentAssignedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(containmentAssignedDashboard);
        Assert.Contains(containmentAssignedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.containment.assigned" && entry.SubjectId == containment.Id);

        var containmentInProgressResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/containment-actions/{containment.Id}/status",
            new UpdateAssurArrStatusRequest("in_progress", "Containment in progress."));

        Assert.Equal(HttpStatusCode.OK, containmentInProgressResponse.StatusCode);

        var containmentCompletedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/containment-actions/{containment.Id}/status",
            new UpdateAssurArrStatusRequest("completed", "Containment completed."));

        Assert.Equal(HttpStatusCode.OK, containmentCompletedResponse.StatusCode);

        var containmentCompletedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        containmentCompletedDashboardResponse.EnsureSuccessStatusCode();
        var containmentCompletedDashboard = await containmentCompletedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(containmentCompletedDashboard);
        Assert.Contains(containmentCompletedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.containment.completed" && entry.SubjectId == containment.Id);

        var containmentVerifiedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/containment-actions/{containment.Id}/status",
            new UpdateAssurArrStatusRequest("verified", "Containment verified."));

        Assert.Equal(HttpStatusCode.OK, containmentVerifiedResponse.StatusCode);

        var containmentVerifiedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        containmentVerifiedDashboardResponse.EnsureSuccessStatusCode();
        var containmentVerifiedDashboard = await containmentVerifiedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(containmentVerifiedDashboard);
        Assert.Contains(containmentVerifiedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.containment.verified" && entry.SubjectId == containment.Id);

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

        var dispositionProposedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        dispositionProposedDashboardResponse.EnsureSuccessStatusCode();
        var dispositionProposedDashboard = await dispositionProposedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(dispositionProposedDashboard);
        Assert.Contains(dispositionProposedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.disposition.proposed" && entry.SubjectId == disposition.Id);

        var dispositionDetailResponse = await _client.GetAsync($"/api/v1/integrations/dispositions/{disposition.Id}");
        Assert.Equal(HttpStatusCode.OK, dispositionDetailResponse.StatusCode);
        var dispositionDetail = await dispositionDetailResponse.Content.ReadFromJsonAsync<AssurArrDispositionResponse>();
        Assert.NotNull(dispositionDetail);
        Assert.Equal(disposition.Id, dispositionDetail!.Id);
        Assert.Equal(disposition.Number, dispositionDetail.Number);

        var dispositionApprovedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/dispositions/{disposition.Id}/status",
            new UpdateAssurArrStatusRequest("approved", "Disposition approved."));

        Assert.Equal(HttpStatusCode.OK, dispositionApprovedResponse.StatusCode);

        var dispositionApprovedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        dispositionApprovedDashboardResponse.EnsureSuccessStatusCode();
        var dispositionApprovedDashboard = await dispositionApprovedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(dispositionApprovedDashboard);
        Assert.Contains(dispositionApprovedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.disposition.approved" && entry.SubjectId == disposition.Id);

        var dispositionExecutedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/dispositions/{disposition.Id}/status",
            new UpdateAssurArrStatusRequest("executed", "Disposition executed."));

        Assert.Equal(HttpStatusCode.OK, dispositionExecutedResponse.StatusCode);

        var dispositionExecutedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        dispositionExecutedDashboardResponse.EnsureSuccessStatusCode();
        var dispositionExecutedDashboard = await dispositionExecutedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(dispositionExecutedDashboard);
        Assert.Contains(dispositionExecutedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.disposition.executed" && entry.SubjectId == disposition.Id);

        var rejectedDispositionTitle = $"Test rejected disposition {Guid.NewGuid():N}";
        var rejectedDispositionResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/dispositions",
            new CreateAssurArrDispositionRequest(
                rejectedDispositionTitle,
                "Automated coverage for rejected dispositions.",
                "moderate",
                "reject",
                "assurarr",
                "NCR-000001",
                ["loadarr:inventory:test"],
                "NCR-000001",
                null,
                DateTimeOffset.UtcNow,
                null,
                null,
                "Disposition rejected for test coverage.",
                ["Return to supplier"],
                "loadarr",
                "loadarr:inventory:test",
                ["recordarr:doc:test"],
                "Disposition notes"));

        Assert.Equal(HttpStatusCode.OK, rejectedDispositionResponse.StatusCode);
        var rejectedDisposition = await rejectedDispositionResponse.Content.ReadFromJsonAsync<AssurArrDispositionResponse>();
        Assert.NotNull(rejectedDisposition);

        var dispositionRejectedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/dispositions/{rejectedDisposition.Id}/status",
            new UpdateAssurArrStatusRequest("rejected", "Disposition rejected."));

        Assert.Equal(HttpStatusCode.OK, dispositionRejectedResponse.StatusCode);

        var dispositionRejectedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        dispositionRejectedDashboardResponse.EnsureSuccessStatusCode();
        var dispositionRejectedDashboard = await dispositionRejectedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(dispositionRejectedDashboard);
        Assert.Contains(dispositionRejectedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.disposition.rejected" && entry.SubjectId == rejectedDisposition.Id);

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
        Assert.Contains(created.EventLog, eventType => eventType == "assurarr.quality_status.changed");
        Assert.Contains(created.EventLog, eventType => eventType == "assurarr.quality_status.published");

        var createdDetailResponse = await _client.GetAsync($"/api/v1/status-snapshots/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, createdDetailResponse.StatusCode);
        var createdDetail = await createdDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(createdDetail);
        Assert.Equal(created.Id, createdDetail!.Id);
        Assert.Equal(created.Number, createdDetail.Number);
        Assert.Contains(createdDetail.EventLog, eventType => eventType == "assurarr.quality_status.published");

        var lookupResponse = await _client.GetAsync($"/api/v1/integrations/quality-status/{targetProduct}/{targetObjectId}");
        Assert.Equal(HttpStatusCode.OK, lookupResponse.StatusCode);

        var lookup = await lookupResponse.Content.ReadFromJsonAsync<AssurArrQualityStatusSnapshotResponse>();
        Assert.NotNull(lookup);
        Assert.Equal(targetProduct, lookup!.TargetProduct);
        Assert.Contains(lookup.EventLog, eventType => eventType == "assurarr.quality_status.changed");

        var listResponse = await _client.GetAsync("/api/v1/integrations/quality-status");
        listResponse.EnsureSuccessStatusCode();
        var statuses = await listResponse.Content.ReadFromJsonAsync<List<AssurArrQualityStatusSnapshotResponse>>();
        Assert.NotNull(statuses);
        Assert.Contains(statuses!, item => item.TargetProduct == targetProduct);

        var dashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        dashboardResponse.EnsureSuccessStatusCode();
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(dashboard);
        Assert.Contains(dashboard!.RecentEvents, eventItem => eventItem.EventType == "assurarr.quality_status.changed" && eventItem.SubjectId == created.Id);
        Assert.Contains(dashboard.RecentEvents, eventItem => eventItem.EventType == "assurarr.quality_status.published" && eventItem.SubjectId == created.Id);
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
        Assert.Contains(scorecard.EventLog, eventType => eventType == "assurarr.scorecard.generated");

        var scorecardGeneratedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        scorecardGeneratedDashboardResponse.EnsureSuccessStatusCode();
        var scorecardGeneratedDashboard = await scorecardGeneratedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(scorecardGeneratedDashboard);
        Assert.Contains(scorecardGeneratedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.scorecard.generated");

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

        var metricDetailResponse = await _client.GetAsync($"/api/v1/scorecards/{scorecard.Id}/metrics/{metric.Id}");
        Assert.Equal(HttpStatusCode.OK, metricDetailResponse.StatusCode);
        var metricDetail = await metricDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityMetricResponse>();
        Assert.NotNull(metricDetail);
        Assert.Equal(metric.Id, metricDetail!.Id);
        Assert.Equal(metric.MetricKey, metricDetail.MetricKey);

        var metricCalculatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        metricCalculatedDashboardResponse.EnsureSuccessStatusCode();
        var metricCalculatedDashboard = await metricCalculatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(metricCalculatedDashboard);
        Assert.Contains(metricCalculatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.metric.calculated");

        var detailResponse = await _client.GetAsync($"/api/v1/scorecards/{scorecard.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<AssurArrQualityScorecardResponse>();
        Assert.NotNull(detail);
        Assert.Contains(metricKey, detail!.MetricRefs);
        Assert.Contains(detail.EventLog, eventType => eventType == "assurarr.metric.calculated");

        var metricListResponse = await _client.GetAsync($"/api/v1/scorecards/{scorecard.Id}/metrics");
        metricListResponse.EnsureSuccessStatusCode();
        var metrics = await metricListResponse.Content.ReadFromJsonAsync<List<AssurArrQualityMetricResponse>>();
        Assert.NotNull(metrics);
        Assert.Contains(metrics!, item => item.MetricKey == metricKey);

        var scorecardListResponse = await _client.GetAsync("/api/v1/integrations/scorecards");
        scorecardListResponse.EnsureSuccessStatusCode();
        var scorecards = await scorecardListResponse.Content.ReadFromJsonAsync<List<AssurArrQualityScorecardResponse>>();
        Assert.NotNull(scorecards);
        Assert.Contains(scorecards!, item => item.Id == scorecard.Id);

        var reviewByPersonId = Guid.NewGuid();
        var reviewResponse = await _client.PostAsJsonAsync(
            $"/api/v1/integrations/scorecards/{scorecard.Id}/review",
            new ReviewAssurArrQualityScorecardRequest(reviewByPersonId, DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);
        var reviewed = await reviewResponse.Content.ReadFromJsonAsync<AssurArrQualityScorecardResponse>();
        Assert.NotNull(reviewed);
        Assert.Equal(reviewByPersonId, reviewed!.ReviewedByPersonId);
        Assert.NotNull(reviewed.ReviewedAt);
        Assert.Contains(reviewed.EventLog, eventType => eventType == "assurarr.scorecard.reviewed");

        var reviewedDetailResponse = await _client.GetAsync($"/api/v1/scorecards/{scorecard.Id}");
        reviewedDetailResponse.EnsureSuccessStatusCode();
        var reviewedDetail = await reviewedDetailResponse.Content.ReadFromJsonAsync<AssurArrQualityScorecardResponse>();
        Assert.NotNull(reviewedDetail);
        Assert.Equal(reviewByPersonId, reviewedDetail!.ReviewedByPersonId);
        Assert.Contains(reviewedDetail.EventLog, eventType => eventType == "assurarr.scorecard.reviewed");

        var reviewedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        reviewedDashboardResponse.EnsureSuccessStatusCode();
        var reviewedDashboard = await reviewedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(reviewedDashboard);
        Assert.Contains(reviewedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.scorecard.reviewed" && entry.SubjectId == scorecard.Id);
    }

    [Fact]
    public async Task Can_recalculate_supplier_and_customer_quality_metrics_from_workflows()
    {
        var supplierRef = $"supplyarr:supplier:{Guid.NewGuid():N}";
        var supplierScorecardResponse = await _client.PostAsJsonAsync(
            "/api/v1/scorecards",
            new CreateAssurArrQualityScorecardRequest(
                "supplier",
                supplierRef,
                DateTimeOffset.UtcNow.AddDays(-7),
                DateTimeOffset.UtcNow,
                88,
                "warning",
                "stable",
                $"Supplier scorecard {Guid.NewGuid():N}",
                "Automated supplier metric coverage.",
                "moderate",
                "assurarr",
                supplierRef,
                [supplierRef],
                null,
                []));

        Assert.Equal(HttpStatusCode.OK, supplierScorecardResponse.StatusCode);
        var supplierScorecard = await supplierScorecardResponse.Content.ReadFromJsonAsync<AssurArrQualityScorecardResponse>();
        Assert.NotNull(supplierScorecard);

        var supplierIssueResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/supplier-quality-issues",
            new CreateAssurArrSupplierQualityIssueRequest(
                $"Supplier metric issue {Guid.NewGuid():N}",
                "Automated coverage for supplier metric calculation.",
                "high",
                "damaged_received",
                "loadarr",
                $"loadarr:receipt:{Guid.NewGuid():N}",
                [$"loadarr:receipt:{Guid.NewGuid():N}"],
                [],
                [],
                supplierRef,
                null,
                null,
                [],
                [],
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.OK, supplierIssueResponse.StatusCode);
        var supplierIssue = await supplierIssueResponse.Content.ReadFromJsonAsync<AssurArrSupplierQualityIssueResponse>();
        Assert.NotNull(supplierIssue);

        var supplierMetricsResponse = await _client.GetAsync($"/api/v1/scorecards/{supplierScorecard!.Id}/metrics");
        supplierMetricsResponse.EnsureSuccessStatusCode();
        var supplierMetrics = await supplierMetricsResponse.Content.ReadFromJsonAsync<List<AssurArrQualityMetricResponse>>();
        Assert.NotNull(supplierMetrics);
        var supplierMetric = Assert.Single(supplierMetrics!, item => item.MetricKey == "supplier-quality-issue-count");
        Assert.Equal(1, supplierMetric.Value);
        Assert.Equal("warning", supplierMetric.Status);

        var supplierNotifiedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/supplier-quality-issues/{supplierIssue!.Id}/status",
            new UpdateAssurArrStatusRequest("supplier_notified", "Supplier notified for metric verification."));

        Assert.Equal(HttpStatusCode.OK, supplierNotifiedResponse.StatusCode);

        var supplierResponsePendingResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/supplier-quality-issues/{supplierIssue.Id}/status",
            new UpdateAssurArrStatusRequest("response_pending", "Supplier response pending for metric verification."));

        Assert.Equal(HttpStatusCode.OK, supplierResponsePendingResponse.StatusCode);

        var supplierResolvedResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/supplier-quality-issues/{supplierIssue.Id}/status",
            new UpdateAssurArrStatusRequest("resolved", "Supplier issue resolved for metric verification."));

        Assert.Equal(HttpStatusCode.OK, supplierResolvedResponse.StatusCode);

        var supplierCloseResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/supplier-quality-issues/{supplierIssue.Id}/status",
            new UpdateAssurArrStatusRequest("closed", "Supplier issue closed for metric verification."));

        Assert.Equal(HttpStatusCode.OK, supplierCloseResponse.StatusCode);

        var supplierMetricsAfterCloseResponse = await _client.GetAsync($"/api/v1/scorecards/{supplierScorecard.Id}/metrics");
        supplierMetricsAfterCloseResponse.EnsureSuccessStatusCode();
        var supplierMetricsAfterClose = await supplierMetricsAfterCloseResponse.Content.ReadFromJsonAsync<List<AssurArrQualityMetricResponse>>();
        Assert.NotNull(supplierMetricsAfterClose);
        var supplierMetricAfterClose = Assert.Single(supplierMetricsAfterClose!, item => item.MetricKey == "supplier-quality-issue-count");
        Assert.Equal(0, supplierMetricAfterClose.Value);
        Assert.Equal("acceptable", supplierMetricAfterClose.Status);

        var customerRef = $"customarr:customer:{Guid.NewGuid():N}";
        var customerScorecardResponse = await _client.PostAsJsonAsync(
            "/api/v1/scorecards",
            new CreateAssurArrQualityScorecardRequest(
                "customer",
                customerRef,
                DateTimeOffset.UtcNow.AddDays(-7),
                DateTimeOffset.UtcNow,
                88,
                "warning",
                "stable",
                $"Customer scorecard {Guid.NewGuid():N}",
                "Automated customer metric coverage.",
                "moderate",
                "assurarr",
                customerRef,
                [customerRef],
                null,
                []));

        Assert.Equal(HttpStatusCode.OK, customerScorecardResponse.StatusCode);
        var customerScorecard = await customerScorecardResponse.Content.ReadFromJsonAsync<AssurArrQualityScorecardResponse>();
        Assert.NotNull(customerScorecard);

        var customerComplaintResponse = await _client.PostAsJsonAsync(
            "/api/v1/integrations/customer-complaint-quality-cases",
            new CreateAssurArrCustomerComplaintQualityCaseRequest(
                $"Customer metric complaint {Guid.NewGuid():N}",
                "Automated coverage for customer metric calculation.",
                "high",
                "delivery_quality",
                "routarr",
                $"routarr:shipment:{Guid.NewGuid():N}",
                [],
                [],
                [],
                [],
                customerRef,
                "Jordan Lee, logistics manager",
                null,
                null,
                [],
                [],
                null,
                [],
                null,
                DateTimeOffset.UtcNow,
                null,
                DateTimeOffset.UtcNow.AddDays(4)));

        Assert.Equal(HttpStatusCode.OK, customerComplaintResponse.StatusCode);
        var customerComplaint = await customerComplaintResponse.Content.ReadFromJsonAsync<AssurArrCustomerComplaintQualityCaseResponse>();
        Assert.NotNull(customerComplaint);

        var customerMetricsResponse = await _client.GetAsync($"/api/v1/scorecards/{customerScorecard!.Id}/metrics");
        customerMetricsResponse.EnsureSuccessStatusCode();
        var customerMetrics = await customerMetricsResponse.Content.ReadFromJsonAsync<List<AssurArrQualityMetricResponse>>();
        Assert.NotNull(customerMetrics);
        var customerMetric = Assert.Single(customerMetrics!, item => item.MetricKey == "customer-complaint-count");
        Assert.Equal(1, customerMetric.Value);
        Assert.Equal("warning", customerMetric.Status);

        var customerTriageResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/customer-complaint-quality-cases/{customerComplaint!.Id}/status",
            new UpdateAssurArrStatusRequest("triage", "Customer complaint triaged for metric verification."));

        Assert.Equal(HttpStatusCode.OK, customerTriageResponse.StatusCode);

        var customerResponsePendingResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/customer-complaint-quality-cases/{customerComplaint.Id}/status",
            new UpdateAssurArrStatusRequest("response_pending", "Customer response pending for metric verification."));

        Assert.Equal(HttpStatusCode.OK, customerResponsePendingResponse.StatusCode);

        var customerCloseResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/integrations/customer-complaint-quality-cases/{customerComplaint.Id}/status",
            new UpdateAssurArrStatusRequest("closed", "Customer complaint closed for metric verification."));

        Assert.Equal(HttpStatusCode.OK, customerCloseResponse.StatusCode);

        var customerMetricsAfterCloseResponse = await _client.GetAsync($"/api/v1/scorecards/{customerScorecard.Id}/metrics");
        customerMetricsAfterCloseResponse.EnsureSuccessStatusCode();
        var customerMetricsAfterClose = await customerMetricsAfterCloseResponse.Content.ReadFromJsonAsync<List<AssurArrQualityMetricResponse>>();
        Assert.NotNull(customerMetricsAfterClose);
        var customerMetricAfterClose = Assert.Single(customerMetricsAfterClose!, item => item.MetricKey == "customer-complaint-count");
        Assert.Equal(0, customerMetricAfterClose.Value);
        Assert.Equal("acceptable", customerMetricAfterClose.Status);
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
        Assert.Contains(created.EventLog, eventType => eventType == "assurarr.risk_profile.updated");

        var riskProfileUpdatedDashboardResponse = await _client.GetAsync("/api/v1/dashboard");
        riskProfileUpdatedDashboardResponse.EnsureSuccessStatusCode();
        var riskProfileUpdatedDashboard = await riskProfileUpdatedDashboardResponse.Content.ReadFromJsonAsync<AssurArrDashboardResponse>();
        Assert.NotNull(riskProfileUpdatedDashboard);
        Assert.Contains(riskProfileUpdatedDashboard!.RecentEvents, entry => entry.EventType == "assurarr.risk_profile.updated");

        var lookupResponse = await _client.GetAsync($"/api/v1/integrations/risk-profiles/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, lookupResponse.StatusCode);

        var lookup = await lookupResponse.Content.ReadFromJsonAsync<AssurArrQualityRiskProfileResponse>();
        Assert.NotNull(lookup);
        Assert.Equal(created.Id, lookup!.Id);
        Assert.Contains(lookup.EventLog, eventType => eventType == "assurarr.risk_profile.updated");

        var listResponse = await _client.GetAsync("/api/v1/integrations/risk-profiles");
        listResponse.EnsureSuccessStatusCode();
        var profiles = await listResponse.Content.ReadFromJsonAsync<List<AssurArrQualityRiskProfileResponse>>();
        Assert.NotNull(profiles);
        Assert.Contains(profiles!, item => item.TargetType == targetType && item.TargetRef == targetRef);
    }
}
