using System.IO.Compression;
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

public class StaffArrPersonExportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrPersonExport-{Guid.NewGuid():N}";

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task People_export_manifest_lists_formats()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export/manifest", token));
        response.EnsureSuccessStatusCode();
        var manifest = (await response.Content.ReadFromJsonAsync<PersonExportManifestResponse>())!;
        Assert.Equal("1", manifest.PackageVersion);
        Assert.Equal(3, manifest.Formats.Count);
        Assert.Contains(manifest.Formats, format => format.Key == "csv");
    }

    [Fact]
    public async Task People_export_v1_manifest_and_json_aliases_work()
    {
        var leadId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await SeedPersonAsync(leadId, "V1 Lead", "Person", "v1.lead.person@example.com");
        await SeedPersonAsync(memberId, "V1 Team", "Member", "v1.team.member@example.com", leadId);

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var manifestResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/people/export/manifest", token));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<PersonExportManifestResponse>())!;
        Assert.Equal("1", manifest.PackageVersion);
        Assert.Contains(manifest.Formats, x => x.Key == "json");

        var jsonResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/people/export?format=json", token));
        jsonResponse.EnsureSuccessStatusCode();
        var payload = (await jsonResponse.Content.ReadFromJsonAsync<PersonExportResponse>())!;
        var member = payload.People.Single(x => x.PersonId == memberId);
        Assert.Equal("v1.lead.person@example.com", member.ManagerEmail);
    }

    [Fact]
    public async Task People_export_json_includes_manager_email()
    {
        var leadId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        await SeedPersonAsync(leadId, "Lead", "Person", "lead.person@example.com");
        await SeedPersonAsync(memberId, "Team", "Member", "team.member@example.com", leadId);

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export?format=json", token));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<PersonExportResponse>())!;
        Assert.Equal(2, payload.PersonCount);

        var member = payload.People.Single(x => x.PersonId == memberId);
        Assert.Equal("lead.person@example.com", member.ManagerEmail);
    }

    [Fact]
    public async Task People_export_csv_matches_import_header()
    {
        await SeedPersonAsync(Guid.NewGuid(), "Export", "Target", "export.target@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "staffarr_admin");

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export?format=csv", token));
        response.EnsureSuccessStatusCode();
        var csv = await response.Content.ReadAsStringAsync();
        Assert.StartsWith(PeopleExportService.CsvHeader, csv, StringComparison.Ordinal);
        Assert.Contains("export.target@example.com", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task People_export_zip_contains_csv_and_manifest()
    {
        await SeedPersonAsync(Guid.NewGuid(), "Zip", "Export", "zip.export@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export", token));
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", response.Content.Headers.ContentType?.MediaType);

        using var archive = new ZipArchive(new MemoryStream(await response.Content.ReadAsByteArrayAsync()), ZipArchiveMode.Read);
        Assert.Contains(archive.Entries, entry => entry.Name == "people.csv");
        Assert.Contains(archive.Entries, entry => entry.Name == "manifest.json");
    }

    [Fact]
    public async Task People_export_filters_by_employment_status()
    {
        await SeedPersonAsync(Guid.NewGuid(), "Active", "User", "active.user@example.com", employmentStatus: "active");
        await SeedPersonAsync(Guid.NewGuid(), "Inactive", "User", "inactive.user@example.com", employmentStatus: "inactive");

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export?format=json&employmentStatus=inactive", token));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<PersonExportResponse>())!;
        Assert.Equal(1, payload.PersonCount);
        Assert.Equal("inactive.user@example.com", payload.People[0].PrimaryEmail);
    }

    [Fact]
    public async Task People_export_filters_by_org_unit()
    {
        var northSiteId = Guid.NewGuid();
        var southSiteId = Guid.NewGuid();
        await SeedOrgUnitAsync(northSiteId, "site", "North Site");
        await SeedOrgUnitAsync(southSiteId, "site", "South Site");
        await SeedPersonAsync(
            Guid.NewGuid(),
            "North",
            "Worker",
            "north.worker@example.com",
            primaryOrgUnitId: northSiteId);
        await SeedPersonAsync(
            Guid.NewGuid(),
            "South",
            "Worker",
            "south.worker@example.com",
            primaryOrgUnitId: southSiteId);

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/export?format=json&orgUnitId={northSiteId}", token));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<PersonExportResponse>())!;
        Assert.Equal(1, payload.PersonCount);
        Assert.Equal("north.worker@example.com", payload.People[0].PrimaryEmail);
        Assert.Equal(northSiteId, payload.People[0].PrimaryOrgUnitId);
    }

    [Fact]
    public async Task People_export_filters_by_employment_status_and_org_unit()
    {
        var northSiteId = Guid.NewGuid();
        var southSiteId = Guid.NewGuid();
        await SeedOrgUnitAsync(northSiteId, "site", "North Site");
        await SeedOrgUnitAsync(southSiteId, "site", "South Site");
        await SeedPersonAsync(
            Guid.NewGuid(),
            "North",
            "Active",
            "north.active@example.com",
            primaryOrgUnitId: northSiteId,
            employmentStatus: "active");
        await SeedPersonAsync(
            Guid.NewGuid(),
            "North",
            "Inactive",
            "north.inactive@example.com",
            primaryOrgUnitId: northSiteId,
            employmentStatus: "inactive");
        await SeedPersonAsync(
            Guid.NewGuid(),
            "South",
            "Active",
            "south.active@example.com",
            primaryOrgUnitId: southSiteId,
            employmentStatus: "active");

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/people/export?format=json&employmentStatus=active&orgUnitId={northSiteId}",
                token));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<PersonExportResponse>())!;
        Assert.Equal(1, payload.PersonCount);
        Assert.Equal("north.active@example.com", payload.People[0].PrimaryEmail);
    }

    [Fact]
    public async Task People_export_denied_for_non_writer_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export?format=json", token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task People_export_writes_audit_event()
    {
        await SeedPersonAsync(Guid.NewGuid(), "Audit", "Export", "audit.export@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export?format=json", token));
        response.EnsureSuccessStatusCode();

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var exportEvents = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "person.export");
        Assert.Equal(1, exportEvents);
    }

    private async Task SeedPersonAsync(
        Guid personId,
        string givenName,
        string familyName,
        string email,
        Guid? managerPersonId = null,
        string employmentStatus = "active",
        Guid? primaryOrgUnitId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = givenName,
            FamilyName = familyName,
            DisplayName = $"{givenName} {familyName}",
            PrimaryEmail = email,
            EmploymentStatus = employmentStatus,
            ManagerPersonId = managerPersonId,
            PrimaryOrgUnitId = primaryOrgUnitId,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedOrgUnitAsync(Guid orgUnitId, string unitType, string name)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.OrgUnits.Add(new OrgUnit
        {
            Id = orgUnitId,
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = unitType,
            Name = name,
            Status = "active",
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
