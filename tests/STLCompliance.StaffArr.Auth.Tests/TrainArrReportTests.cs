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
        var definitionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
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
}
