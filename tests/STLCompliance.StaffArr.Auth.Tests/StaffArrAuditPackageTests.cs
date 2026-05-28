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
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrAuditPackageTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrAuditPackage-{Guid.NewGuid():N}";

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
    public async Task Audit_package_manifest_lists_sections()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/manifest", supervisorToken));
        response.EnsureSuccessStatusCode();
        var manifest = (await response.Content.ReadFromJsonAsync<AuditPackageManifestResponse>())!;
        Assert.Equal("1", manifest.PackageVersion);
        Assert.Equal(7, manifest.Sections.Count);
        Assert.Contains(manifest.Sections, section => section.Key == "audit_events");
        Assert.Contains(manifest.Sections, section => section.FileName == "training_blockers.json");
    }

    [Fact]
    public async Task Audit_package_export_zip_contains_json_files()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        await SeedWorkforceDataAsync();

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export", adminToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", response.Content.Headers.ContentType?.MediaType);

        var zipBytes = await response.Content.ReadAsByteArrayAsync();
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        Assert.Equal(8, archive.Entries.Count);
        Assert.Contains(archive.Entries, entry => entry.Name == "manifest.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "people.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "permission_history.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "training_blockers.json");
    }

    [Fact]
    public async Task Audit_package_export_json_returns_structured_package()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "staffarr_admin");
        await SeedWorkforceDataAsync();

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=json", adminToken));
        response.EnsureSuccessStatusCode();
        var package = (await response.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.NotEqual(Guid.Empty, package.PackageId);
        Assert.Equal(PlatformSeeder.DemoTenantId, package.TenantId);
        Assert.Equal(1, package.Counts.People);
        Assert.Equal(1, package.Counts.PersonnelIncidents);
        Assert.Equal("Demo Worker", package.People[0].DisplayName);
    }

    [Fact]
    public async Task Audit_package_export_writes_audit_event()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var beforeCount = await CountAuditPackageExportEventsAsync();

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=json", adminToken));
        response.EnsureSuccessStatusCode();

        var afterCount = await CountAuditPackageExportEventsAsync();
        Assert.Equal(beforeCount + 1, afterCount);
    }

    [Fact]
    public async Task Audit_package_export_denies_supervisor()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export", supervisorToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Audit_package_export_rejects_invalid_date_range()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var from = DateTimeOffset.UtcNow;
        var to = from.AddDays(-1);
        var response = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit-packages/export?format=json&from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}",
                adminToken));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Audit_package_timeline_returns_paged_audit_events()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        await SeedAuditEventsWithDatesAsync();

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/timeline?page=1&pageSize=10", supervisorToken));
        response.EnsureSuccessStatusCode();
        var timeline = (await response.Content.ReadFromJsonAsync<PagedResult<StaffArrAuditEventExportItem>>())!;
        Assert.Equal(2, timeline.TotalCount);
        Assert.Equal(2, timeline.Items.Count);
        Assert.Equal("org_unit.update", timeline.Items[0].Action);
    }

    [Fact]
    public async Task Audit_package_timeline_date_filter_limits_results()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        await SeedAuditEventsWithDatesAsync();

        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 23, 59, 59, TimeSpan.Zero);
        var response = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit-packages/timeline?from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}",
                supervisorToken));
        response.EnsureSuccessStatusCode();
        var timeline = (await response.Content.ReadFromJsonAsync<PagedResult<StaffArrAuditEventExportItem>>())!;
        Assert.Equal(1, timeline.TotalCount);
        Assert.Equal("org_unit.create", timeline.Items[0].Action);
    }

    [Fact]
    public async Task Audit_package_export_date_filter_limits_audit_events()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        await SeedAuditEventsWithDatesAsync();

        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 23, 59, 59, TimeSpan.Zero);
        var response = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit-packages/export?format=json&from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}",
                adminToken));
        response.EnsureSuccessStatusCode();
        var package = (await response.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.Equal(1, package.Counts.AuditEvents);
        Assert.Equal("org_unit.create", package.AuditEvents[0].Action);
    }

    private async Task SeedWorkforceDataAsync()
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var personId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Demo",
            FamilyName = "Worker",
            DisplayName = "Demo Worker",
            PrimaryEmail = "worker@demo.stl",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PersonnelIncidents.Add(new PersonnelIncident
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = personId,
            ReasonCategoryKey = "safety",
            Severity = "medium",
            Status = "open",
            Title = "Audit package test incident",
            Description = "Seeded for audit package export test.",
            OccurredAt = now,
            ReportedAt = now,
            ReportedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private async Task SeedAuditEventsWithDatesAsync()
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        db.AuditEvents.AddRange(
            new StaffArrAuditEvent
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                Action = "org_unit.create",
                TargetType = "org_unit",
                TargetId = Guid.NewGuid().ToString(),
                Result = "success",
                CorrelationId = Guid.NewGuid(),
                OccurredAt = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero),
            },
            new StaffArrAuditEvent
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                Action = "org_unit.update",
                TargetType = "org_unit",
                TargetId = Guid.NewGuid().ToString(),
                Result = "success",
                CorrelationId = Guid.NewGuid(),
                OccurredAt = new DateTimeOffset(2026, 2, 1, 12, 0, 0, TimeSpan.Zero),
            });
        await db.SaveChangesAsync();
    }

    private async Task<int> CountAuditPackageExportEventsAsync()
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        return await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "audit_package.export");
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
