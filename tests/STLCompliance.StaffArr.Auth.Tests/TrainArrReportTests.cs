using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _trainarrClient = null!;
    private string _adminToken = null!;
    private Guid _seedPersonId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"TrainArrReports-{Guid.NewGuid():N}";

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();
        _adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        await SeedTrainingDataAsync();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Assignment_report_summary_returns_aggregates()
    {
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/assignments/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<AssignmentReportSummaryResponse>())!;
        Assert.True(summary.TotalAssignments >= 1);
        Assert.True(summary.OpenAssignments >= 1);
    }

    [Fact]
    public async Task Qualification_report_summary_returns_aggregates()
    {
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/qualifications/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<QualificationReportSummaryResponse>())!;
        Assert.Equal(1, summary.TotalQualifications);
        Assert.Equal(1, summary.IssuedCount);
    }

    [Fact]
    public async Task Compliance_report_summary_returns_aggregates()
    {
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/compliance/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<ComplianceReportSummaryResponse>())!;
        Assert.Equal(1, summary.CitationAttachmentCount);
        Assert.Equal(1, summary.RulePackRequirementCount);
        Assert.Equal(1, summary.OpenRemediationCount);
    }

    [Fact]
    public async Task Command_center_dashboard_returns_training_operational_snapshot()
    {
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/dashboard", _adminToken));
        response.EnsureSuccessStatusCode();

        var dashboard = (await response.Content.ReadFromJsonAsync<TrainArrCommandCenterResponse>())!;
        Assert.True(dashboard.Assignments.TotalAssignments >= 2);
        Assert.Equal(1, dashboard.Qualifications.ExpiringWithin30Days);
        Assert.Equal(1, dashboard.FailedEvaluationCount);
        Assert.Equal(1, dashboard.RemediationBacklogCount);
        Assert.Equal(1, dashboard.ProgramsNeedingReviewCount);
        Assert.Equal(1, dashboard.UnqualifiedAssignmentRiskCount);
        Assert.InRange(dashboard.AuditReadinessScore, 0, 99);
        Assert.Contains(dashboard.Risks, risk => risk.RiskKey == "overdue_assignments");
        Assert.Contains(dashboard.Risks, risk => risk.RiskKey == "unqualified_assignment_risks");
    }

    [Fact]
    public async Task Personal_training_dashboard_returns_self_assignments_qualifications_inbox_and_history()
    {
        var personToken = CreateTrainArrAccessToken(
            ["trainarr"],
            tenantRoleKey: "tenant_member",
            personId: _seedPersonId);

        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/me/training", personToken));
        response.EnsureSuccessStatusCode();

        var dashboard = (await response.Content.ReadFromJsonAsync<PersonalTrainingDashboardResponse>())!;
        Assert.Equal(_seedPersonId, dashboard.StaffarrPersonId);
        Assert.Equal(2, dashboard.Summary.ActiveAssignmentCount);
        Assert.Equal(1, dashboard.Summary.OverdueAssignmentCount);
        Assert.Equal(1, dashboard.Summary.QualificationCount);
        Assert.Equal(1, dashboard.Summary.ExpiringQualificationCount);
        Assert.Equal(2, dashboard.Summary.FieldInboxCount);
        Assert.Single(dashboard.RecentHistory);
        Assert.Contains(dashboard.AssignedTraining, assignment => assignment.TrainingDefinitionName == "Hazmat Awareness");
        Assert.Contains(dashboard.Qualifications, qualification => qualification.QualificationKey == "hazmat_endorsement");
    }

    [Fact]
    public async Task Entity_export_manifest_lists_three_entities()
    {
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", _adminToken));
        response.EnsureSuccessStatusCode();

        var manifest = (await response.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Equal(3, manifest.Entities.Count);
        Assert.Contains(manifest.Entities, entity => entity.EntityKey == "training_assignments");
    }

    [Fact]
    public async Task Entity_export_v1_manifest_and_csv_aliases_work()
    {
        var manifestResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/manifest", _adminToken));
        manifestResponse.EnsureSuccessStatusCode();

        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Equal(3, manifest.Entities.Count);
        Assert.Contains(manifest.Entities, entity =>
            entity.EntityKey == "training_assignments"
            && entity.ExportPath == "/api/v1/exports/training-assignments");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "assignments"
            && report.ExportPath == "/api/v1/reports/assignments/summary/export");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "programs_without_citation"
            && report.ExportPath == "/api/v1/reports/compliance/programs-without-citation/export");

        var assignmentsResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/training-assignments?status=assigned", _adminToken));
        assignmentsResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", assignmentsResponse.Content.Headers.ContentType?.MediaType);
        var assignmentsCsv = await assignmentsResponse.Content.ReadAsStringAsync();
        Assert.Contains(TrainArrEntityBulkExportService.AssignmentsCsvHeader, assignmentsCsv, StringComparison.Ordinal);
        Assert.Contains("hazmat_awareness", assignmentsCsv, StringComparison.Ordinal);

        var qualificationsResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/qualification-issues?status=issued", _adminToken));
        qualificationsResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", qualificationsResponse.Content.Headers.ContentType?.MediaType);
        var qualificationsCsv = await qualificationsResponse.Content.ReadAsStringAsync();
        Assert.Contains(TrainArrEntityBulkExportService.QualificationsCsvHeader, qualificationsCsv, StringComparison.Ordinal);
        Assert.Contains("hazmat_endorsement", qualificationsCsv, StringComparison.Ordinal);

        var definitionsResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/training-definitions", _adminToken));
        definitionsResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", definitionsResponse.Content.Headers.ContentType?.MediaType);
        var definitionsCsv = await definitionsResponse.Content.ReadAsStringAsync();
        Assert.Contains(TrainArrEntityBulkExportService.TrainingDefinitionsCsvHeader, definitionsCsv, StringComparison.Ordinal);
        Assert.Contains("forklift_operator_training", definitionsCsv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Assignment_report_export_returns_csv()
    {
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/assignments/summary/export", _adminToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Reports_v1_aliases_match_assignment_and_qualification_summaries()
    {
        var assignmentLegacyResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/assignments/summary", _adminToken));
        assignmentLegacyResponse.EnsureSuccessStatusCode();
        var assignmentLegacy = (await assignmentLegacyResponse.Content.ReadFromJsonAsync<AssignmentReportSummaryResponse>())!;

        var assignmentV1Response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/assignments/summary", _adminToken));
        assignmentV1Response.EnsureSuccessStatusCode();
        var assignmentV1 = (await assignmentV1Response.Content.ReadFromJsonAsync<AssignmentReportSummaryResponse>())!;
        Assert.Equal(assignmentLegacy.TotalAssignments, assignmentV1.TotalAssignments);
        Assert.Equal(assignmentLegacy.OpenAssignments, assignmentV1.OpenAssignments);

        var qualificationLegacyResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/qualifications/summary", _adminToken));
        qualificationLegacyResponse.EnsureSuccessStatusCode();
        var qualificationLegacy = (await qualificationLegacyResponse.Content.ReadFromJsonAsync<QualificationReportSummaryResponse>())!;

        var qualificationV1Response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/qualifications/summary", _adminToken));
        qualificationV1Response.EnsureSuccessStatusCode();
        var qualificationV1 = (await qualificationV1Response.Content.ReadFromJsonAsync<QualificationReportSummaryResponse>())!;
        Assert.Equal(qualificationLegacy.TotalQualifications, qualificationV1.TotalQualifications);
        Assert.Equal(qualificationLegacy.IssuedCount, qualificationV1.IssuedCount);

        var complianceLegacyResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/compliance/summary", _adminToken));
        complianceLegacyResponse.EnsureSuccessStatusCode();
        var complianceLegacy = (await complianceLegacyResponse.Content.ReadFromJsonAsync<ComplianceReportSummaryResponse>())!;

        var complianceV1Response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/compliance/summary", _adminToken));
        complianceV1Response.EnsureSuccessStatusCode();
        var complianceV1 = (await complianceV1Response.Content.ReadFromJsonAsync<ComplianceReportSummaryResponse>())!;
        Assert.Equal(complianceLegacy.CitationAttachmentCount, complianceV1.CitationAttachmentCount);
        Assert.Equal(complianceLegacy.RulePackRequirementCount, complianceV1.RulePackRequirementCount);
    }

    [Fact]
    public async Task Reports_v1_expose_overdue_expiring_and_gap_reports()
    {
        var overdueResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/assignments/overdue", _adminToken));
        overdueResponse.EnsureSuccessStatusCode();
        var overdue = (await overdueResponse.Content.ReadFromJsonAsync<AssignmentOverdueReportResponse>())!;
        Assert.True(overdue.TotalOverdueAssignments >= 1);
        Assert.All(overdue.Items, item => Assert.True(item.IsOverdue));

        var expiringResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/qualifications/expiring?windowDays=30", _adminToken));
        expiringResponse.EnsureSuccessStatusCode();
        var expiring = (await expiringResponse.Content.ReadFromJsonAsync<QualificationExpiringReportResponse>())!;
        Assert.Equal(30, expiring.WindowDays);
        Assert.Equal(1, expiring.TotalExpiringQualifications);
        Assert.Contains(expiring.Items, item => item.QualificationKey == "hazmat_endorsement");

        var gapsResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/compliance/gaps", _adminToken));
        gapsResponse.EnsureSuccessStatusCode();
        var gaps = (await gapsResponse.Content.ReadFromJsonAsync<TrainingGapReportResponse>())!;
        Assert.Equal(1, gaps.TotalGaps);
        Assert.Contains(gaps.Items, item =>
            item.QualificationKey == "forklift_operator"
            && item.GapReasonCode == "missing_issued_qualification");

        var programsWithoutCitationResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/compliance/programs-without-citation", _adminToken));
        programsWithoutCitationResponse.EnsureSuccessStatusCode();
        var programsWithoutCitation = (await programsWithoutCitationResponse.Content.ReadFromJsonAsync<ProgramCitationGapReportResponse>())!;
        Assert.True(programsWithoutCitation.TotalPrograms >= 2);
        Assert.Equal(2, programsWithoutCitation.ProgramsMissingCitationCount);
        Assert.Contains(programsWithoutCitation.Items, item => item.ProgramKey == "hazmat_program");
        Assert.Contains(programsWithoutCitation.Items, item => item.ProgramKey == "draft_pit_review");

        var programsWithoutCitationExportResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/compliance/programs-without-citation/export", _adminToken));
        programsWithoutCitationExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", programsWithoutCitationExportResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Readiness_alerts_include_required_training_risk_signals()
    {
        var legacyResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/readiness/alerts", _adminToken));
        legacyResponse.EnsureSuccessStatusCode();
        var legacyAlerts = await legacyResponse.Content.ReadFromJsonAsync<List<TrainArrReadinessAlertResponse>>();
        Assert.NotNull(legacyAlerts);
        Assert.Contains(legacyAlerts!, x => x.AlertType == "overdue_training_assignment");
        Assert.Contains(legacyAlerts!, x => x.AlertType == "expiring_qualification");
        Assert.Contains(legacyAlerts!, x => x.AlertType == "failed_training_evaluation");
        Assert.Contains(legacyAlerts!, x => x.AlertType == "open_training_remediation");

        var v1Response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/readiness/alerts", _adminToken));
        v1Response.EnsureSuccessStatusCode();
        var v1Alerts = await v1Response.Content.ReadFromJsonAsync<List<TrainArrReadinessAlertResponse>>();
        Assert.NotNull(v1Alerts);
        Assert.Equal(legacyAlerts!.Count, v1Alerts!.Count);
    }

    [Fact]
    public async Task Reports_index_v1_lists_available_report_groups()
    {
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports", _adminToken));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ReportIndexResponse>();
        Assert.NotNull(payload);
        Assert.Contains(payload!.Items, item => item.Key == "assignments");
        Assert.Contains(payload.Items, item => item.Key == "qualifications");
        Assert.Contains(payload.Items, item => item.Key == "compliance");
        Assert.Contains(payload.Items, item => item.Key == "dashboard" && item.Path == "/api/v1/dashboard");
        Assert.Contains(payload.Items, item => item.Key == "compliance_gap_programs_without_citation");
    }

    [Fact]
    public async Task Training_definitions_v1_create_and_list_catalog_entries()
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/v1/training-definitions", _adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"v1-definition-{Guid.NewGuid():N}"[..24],
            "Versioned Definition",
            "Created through the documented v1 training definition catalog.",
            "v1_qualification",
            "V1 Qualification"));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.StartsWith("/api/v1/training-definitions/", createResponse.Headers.Location?.OriginalString);
        var created = (await createResponse.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        Assert.Equal("Versioned Definition", created.Name);

        var listResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/training-definitions", _adminToken));
        listResponse.EnsureSuccessStatusCode();
        var definitions = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingDefinitionResponse>>())!;
        Assert.Contains(definitions, definition => definition.TrainingDefinitionId == created.TrainingDefinitionId);
    }

    [Fact]
    public async Task Assignment_report_summary_denies_unauthorized_role()
    {
        var token = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "supplyarr_admin");
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/assignments/summary", token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task SeedTrainingDataAsync()
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var personId = Guid.NewGuid();
        _seedPersonId = personId;
        var definitionId = Guid.NewGuid();
        var gapDefinitionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var gapAssignmentId = Guid.NewGuid();
        var programId = Guid.NewGuid();

        db.TrainingDefinitions.Add(new TrainingDefinition
        {
            Id = definitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = "hazmat_awareness",
            Name = "Hazmat Awareness",
            Description = "Seeded for report test.",
            QualificationKey = "hazmat_endorsement",
            QualificationName = "Hazmat Endorsement",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingDefinitions.Add(new TrainingDefinition
        {
            Id = gapDefinitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = "forklift_operator_training",
            Name = "Forklift Operator Training",
            Description = "Seeded gap report definition.",
            QualificationKey = "forklift_operator",
            QualificationName = "Forklift Operator",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingPrograms.Add(new TrainingProgram
        {
            Id = programId,
            TenantId = PlatformSeeder.DemoTenantId,
            ProgramKey = "hazmat_program",
            Name = "Hazmat Program",
            Description = "Seeded program",
            Status = "published",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingPrograms.Add(new TrainingProgram
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ProgramKey = "draft_pit_review",
            Name = "Draft PIT Review",
            Description = "Seeded draft program needing review.",
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = assignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            TrainingDefinitionId = definitionId,
            AssignmentReason = "manual",
            Status = "assigned",
            DueAt = now.AddDays(-1),
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = gapAssignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            TrainingDefinitionId = gapDefinitionId,
            AssignmentReason = "equipment_assignment",
            Status = "assigned",
            DueAt = now.AddDays(5),
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingEvaluations.Add(new TrainingEvaluation
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingAssignmentId = gapAssignmentId,
            Result = "fail",
            Score = 60,
            Notes = "Seeded failed evaluation for command center dashboard.",
            EvaluatorUserId = PlatformSeeder.DemoAdminUserId,
            EvaluatedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.QualificationIssues.Add(new QualificationIssue
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingAssignmentId = assignmentId,
            StaffarrPersonId = personId,
            QualificationKey = "hazmat_endorsement",
            QualificationName = "Hazmat Endorsement",
            GrantPublicationId = Guid.NewGuid(),
            Status = "issued",
            IssuedAt = now,
            ExpiresAt = now.AddDays(20),
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingCitationAttachments.Add(new TrainingCitationAttachment
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            EntityType = "training_definition",
            EntityId = definitionId,
            ComplianceCoreCitationId = Guid.NewGuid(),
            CitationKey = "49-cfr-172",
            CitationVersion = 1,
            CreatedAt = now,
        });

        db.TrainingRulePackRequirements.Add(new TrainingRulePackRequirement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            EntityType = "training_definition",
            EntityId = definitionId,
            RulePackKey = "hazmat_pack",
            KnownVersionNumber = 1,
            KnownStatus = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.StaffarrIncidentRemediations.Add(new StaffarrIncidentRemediation
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrIncidentId = Guid.NewGuid(),
            StaffarrPersonId = personId,
            ReasonCategoryKey = "training_compliance",
            Severity = "medium",
            Title = "Training gap",
            Description = "Needs remediation assignment.",
            OccurredAt = now,
            ReportedAt = now,
            Status = "intake_received",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PersonTrainingHistoryEntries.Add(new PersonTrainingHistoryEntry
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            SourceDomainEventId = Guid.NewGuid(),
            EventKind = TrainingDomainEventKinds.AssignmentCreated,
            Summary = "Training assignment created for dashboard seed.",
            RelatedEntityType = "training_assignment",
            RelatedEntityId = assignmentId,
            OccurredAt = now,
            CreatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private sealed record ReportIndexResponse(IReadOnlyList<ReportIndexItem> Items);

    private sealed record ReportIndexItem(string Key, string Path, string Description);
}
