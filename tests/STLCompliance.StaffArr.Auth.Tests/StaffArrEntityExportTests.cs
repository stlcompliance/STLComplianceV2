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

public sealed class StaffArrEntityExportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;
    private string _adminToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrEntityExports-{Guid.NewGuid():N}";

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
    public async Task Entity_export_manifest_lists_three_entities()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", _adminToken));
        response.EnsureSuccessStatusCode();

        var manifest = (await response.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Equal("2", manifest.PackageVersion);
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

        var peopleResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/people?employmentStatus=active", _adminToken));
        peopleResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", peopleResponse.Content.Headers.ContentType?.MediaType);
        var peopleCsv = await peopleResponse.Content.ReadAsStringAsync();
        Assert.Contains(StaffArrEntityBulkExportService.PeopleCsvHeader, peopleCsv, StringComparison.Ordinal);
        Assert.Contains("Export Worker", peopleCsv, StringComparison.Ordinal);

        var incidentsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/personnel-incidents?status=open", _adminToken));
        incidentsResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", incidentsResponse.Content.Headers.ContentType?.MediaType);
        var incidentsCsv = await incidentsResponse.Content.ReadAsStringAsync();
        Assert.Contains(StaffArrEntityBulkExportService.IncidentsCsvHeader, incidentsCsv, StringComparison.Ordinal);
        Assert.Contains("Entity export test incident", incidentsCsv, StringComparison.Ordinal);

        var certificationsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/person-certifications", _adminToken));
        certificationsResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", certificationsResponse.Content.Headers.ContentType?.MediaType);
        var certificationsCsv = await certificationsResponse.Content.ReadAsStringAsync();
        Assert.Contains(StaffArrEntityBulkExportService.CertificationsCsvHeader, certificationsCsv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Supervisor_cannot_export_manifest()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", supervisorToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_without_staffarr_role_cannot_export_manifest()
    {
        var platformAdminToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            isPlatformAdmin: true);

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", platformAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task SeedWorkforceDataAsync()
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var personId = Guid.NewGuid();
        var orgUnitId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.OrgUnits.Add(new OrgUnit
        {
            Id = orgUnitId,
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = "team",
            Name = "Entity Export Test Team",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Export",
            FamilyName = "Worker",
            DisplayName = "Export Worker",
            PrimaryEmail = "export.worker@demo.stl",
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

        db.PersonCertifications.Add(new PersonCertification
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = personId,
            CertificationDefinitionId = certificationDefinitionId,
            SourceType = "manual",
            Status = "active",
            GrantedAt = now.AddMonths(-3),
            ExpiresAt = now.AddDays(10),
            GrantedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PersonnelIncidents.Add(new PersonnelIncident
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = personId,
            ReasonCategoryKey = "safety",
            Severity = "high",
            Status = "open",
            Title = "Entity export test incident",
            Description = "Seeded for StaffArr entity export tests.",
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
        Guid? personId = null,
        bool isPlatformAdmin = false)
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
            isPlatformAdmin);

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
