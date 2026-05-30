using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
