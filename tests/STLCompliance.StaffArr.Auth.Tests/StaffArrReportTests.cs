using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;
    private string _adminToken = null!;
    private Guid _personId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrReports-{Guid.NewGuid():N}";

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
        _adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "staffarr_admin");
        await SeedWorkforceDataAsync();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Personnel_report_summary_returns_aggregates()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/personnel/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<PersonnelReportSummaryResponse>())!;
        Assert.True(summary.TotalPeople >= 1);
        Assert.True(summary.ActiveCount >= 1);
    }

    [Fact]
    public async Task Readiness_report_summary_returns_aggregates()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/readiness/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<ReadinessReportSummaryResponse>())!;
        Assert.Equal(1, summary.TotalRollups);
        Assert.True(summary.TotalMembers >= 1);
    }

    [Fact]
    public async Task Certification_report_summary_returns_aggregates()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/certifications/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<CertificationReportSummaryResponse>())!;
        Assert.True(summary.TotalPeople >= 1);
        Assert.True(summary.ActiveCertificationCount >= 1);
        Assert.True(summary.ExpiringSoonCount >= 1);
    }

    [Fact]
    public async Task Incident_report_summary_returns_aggregates()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/incidents/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<IncidentReportSummaryResponse>())!;
        Assert.Equal(1, summary.TotalIncidents);
        Assert.Equal(1, summary.OpenCount);
    }

    [Fact]
    public async Task Readiness_alerts_include_expiring_certifications_overrides_and_open_incidents()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/readiness/alerts", _adminToken));
        response.EnsureSuccessStatusCode();

        var alerts = await response.Content.ReadFromJsonAsync<IReadOnlyList<ReadinessReportAlertResponse>>();
        Assert.NotNull(alerts);
        Assert.Contains(alerts!, alert => alert.AlertType == "certification_expiring");
        Assert.Contains(alerts, alert => alert.AlertType == "override_expiring");
        Assert.Contains(alerts, alert => alert.AlertType == "open_incident");
    }

    [Fact]
    public async Task Entity_export_manifest_lists_three_entities()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", _adminToken));
        response.EnsureSuccessStatusCode();

        var manifest = (await response.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Equal(3, manifest.Entities.Count);
        Assert.Contains(manifest.Entities, entity => entity.EntityKey == "people");
    }

    [Fact]
    public async Task Entity_export_v1_manifest_and_csv_aliases_work()
    {
        var manifestResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/manifest", _adminToken));
        manifestResponse.EnsureSuccessStatusCode();

        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Equal(3, manifest.Entities.Count);
        Assert.Contains(manifest.Entities, entity =>
            entity.EntityKey == "people"
            && entity.ExportPath == "/api/v1/exports/people");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "personnel"
            && report.ExportPath == "/api/v1/reports/personnel/summary/export");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "certifications"
            && report.ExportPath == "/api/v1/reports/certifications/summary/export");

        var peopleResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/people?employmentStatus=active", _adminToken));
        peopleResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", peopleResponse.Content.Headers.ContentType?.MediaType);
        var peopleCsv = await peopleResponse.Content.ReadAsStringAsync();
        Assert.Contains(StaffArrEntityBulkExportService.PeopleCsvHeader, peopleCsv, StringComparison.Ordinal);
        Assert.Contains("Report Worker", peopleCsv, StringComparison.Ordinal);

        var incidentsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/personnel-incidents?status=open", _adminToken));
        incidentsResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", incidentsResponse.Content.Headers.ContentType?.MediaType);
        var incidentsCsv = await incidentsResponse.Content.ReadAsStringAsync();
        Assert.Contains(StaffArrEntityBulkExportService.IncidentsCsvHeader, incidentsCsv, StringComparison.Ordinal);
        Assert.Contains("Report test incident", incidentsCsv, StringComparison.Ordinal);

        var certificationsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/person-certifications", _adminToken));
        certificationsResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", certificationsResponse.Content.Headers.ContentType?.MediaType);
        var certificationsCsv = await certificationsResponse.Content.ReadAsStringAsync();
        Assert.Contains(StaffArrEntityBulkExportService.CertificationsCsvHeader, certificationsCsv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Supervisor_can_read_personnel_report_but_cannot_export_manifest()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");

        var readResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/personnel/summary", supervisorToken));
        readResponse.EnsureSuccessStatusCode();

        var exportResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", supervisorToken));
        Assert.Equal(HttpStatusCode.Forbidden, exportResponse.StatusCode);
    }

    [Fact]
    public async Task Staffarr_v1_feature_aliases_are_available()
    {
        var reportsIndexResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports", _adminToken));
        reportsIndexResponse.EnsureSuccessStatusCode();
        var reportsIndexJson = await reportsIndexResponse.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/reports/certifications", reportsIndexJson, StringComparison.Ordinal);

        var certificationsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/certifications", _adminToken));
        certificationsResponse.EnsureSuccessStatusCode();

        var sitesResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/sites", _adminToken));
        sitesResponse.EnsureSuccessStatusCode();

        var hierarchyResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/hierarchy?personId={_personId:D}", _adminToken));
        hierarchyResponse.EnsureSuccessStatusCode();

        var documentsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/documents?personId={_personId:D}", _adminToken));
        documentsResponse.EnsureSuccessStatusCode();

        var onboardingResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/onboarding?personId={_personId:D}", _adminToken));
        onboardingResponse.EnsureSuccessStatusCode();

        var integrationsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations", _adminToken));
        integrationsResponse.EnsureSuccessStatusCode();
        var integrationsJson = JsonDocument.Parse(await integrationsResponse.Content.ReadAsStringAsync());
        var items = integrationsJson.RootElement.GetProperty("items");
        Assert.Contains(items.EnumerateArray(), item => item.GetProperty("key").GetString() == "person-history");
        Assert.Contains(items.EnumerateArray(), item => item.GetProperty("key").GetString() == "procurement-approval-authority");
        Assert.Contains(items.EnumerateArray(), item => item.GetProperty("key").GetString() == "permission-check");
        Assert.Contains(items.EnumerateArray(), item => item.GetProperty("key").GetString() == "readiness-rollups-teams");
        Assert.Contains(items.EnumerateArray(), item => item.GetProperty("key").GetString() == "readiness-rollups-sites");
        Assert.Contains(items.EnumerateArray(), item => item.GetProperty("key").GetString() == "readiness-rollups-departments");
    }

    [Fact]
    public async Task Staffarr_v1_report_groups_expose_summaries_and_exports()
    {
        var personnelResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/personnel/summary", _adminToken));
        personnelResponse.EnsureSuccessStatusCode();
        var personnel = (await personnelResponse.Content.ReadFromJsonAsync<PersonnelReportSummaryResponse>())!;
        Assert.True(personnel.TotalPeople >= 1);

        var readinessResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/readiness/summary", _adminToken));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<ReadinessReportSummaryResponse>())!;
        Assert.Equal(1, readiness.TotalRollups);

        var incidentsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/incidents/summary?openOnly=true", _adminToken));
        incidentsResponse.EnsureSuccessStatusCode();
        var incidents = (await incidentsResponse.Content.ReadFromJsonAsync<IncidentReportSummaryResponse>())!;
        Assert.Equal(1, incidents.TotalIncidents);
        Assert.Equal(1, incidents.OpenCount);

        var certificationsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/certifications/summary?expiringOnly=true", _adminToken));
        certificationsResponse.EnsureSuccessStatusCode();
        var certifications = (await certificationsResponse.Content.ReadFromJsonAsync<CertificationReportSummaryResponse>())!;
        Assert.True(certifications.ExpiringSoonCount >= 1);

        var exportResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/personnel/summary/export", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);

        var certificationExportResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/certifications/summary/export?expiringOnly=true", _adminToken));
        certificationExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", certificationExportResponse.Content.Headers.ContentType?.MediaType);
    }

    private async Task SeedWorkforceDataAsync()
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        _personId = Guid.NewGuid();
        var orgUnitId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.OrgUnits.Add(new OrgUnit
        {
            Id = orgUnitId,
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = "team",
            Name = "Reports Test Team",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.People.Add(new StaffPerson
        {
            Id = _personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Report",
            FamilyName = "Worker",
            DisplayName = "Report Worker",
            PrimaryEmail = "report.worker@demo.stl",
            EmploymentStatus = "active",
            PrimaryOrgUnitId = orgUnitId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        var certificationDefinitionId = Guid.NewGuid();
        db.CertificationDefinitions.Add(new CertificationDefinition
        {
            Id = certificationDefinitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            CertificationKey = "hazmat-cert",
            Name = "Hazmat Certification",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.ReadinessRollups.Add(new ReadinessRollup
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ScopeType = "team",
            OrgUnitId = orgUnitId,
            OrgUnitName = "Reports Test Team",
            TotalMembers = 1,
            ReadyCount = 0,
            NotReadyCount = 1,
            OverrideCount = 0,
            ReadyPercent = 0m,
            ComputedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PersonCertifications.Add(new PersonCertification
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = _personId,
            CertificationDefinitionId = certificationDefinitionId,
            SourceType = "manual",
            Status = "active",
            GrantedAt = now.AddMonths(-3),
            ExpiresAt = now.AddDays(10),
            GrantedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PersonReadinessOverrides.Add(new PersonReadinessOverride
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = _personId,
            Status = "active",
            Reason = "Temporary assignment allowance",
            GrantedAt = now.AddDays(-2),
            ExpiresAt = now.AddDays(7),
            GrantedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PersonnelIncidents.Add(new PersonnelIncident
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = _personId,
            ReasonCategoryKey = "safety",
            Severity = "high",
            Status = "open",
            Title = "Report test incident",
            Description = "Seeded for StaffArr report tests.",
            OccurredAt = now,
            ReportedAt = now,
            ReportedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
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
