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

public class StaffArrPersonBulkImportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrPersonBulkImport-{Guid.NewGuid():N}";

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
    public async Task Bulk_import_creates_multiple_people()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var request = Authorized(HttpMethod.Post, "/api/people/import", token);
        request.Content = JsonContent.Create(new BulkPersonImportRequest(
        [
            new BulkPersonImportRowRequest("Lead", "Person", "lead.person@example.com", "active", null, null, null, "Supervisor"),
            new BulkPersonImportRowRequest("Team", "Member", "team.member@example.com", "active", null, null, "lead.person@example.com", "Technician"),
        ]));

        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<BulkPersonImportResponse>())!;

        Assert.Equal(2, payload.CreatedCount);
        Assert.Equal(0, payload.ErrorCount);
        Assert.Equal(2, payload.Results.Count(r => r.Status == "created"));
        Assert.Contains(payload.Results, r => r.PrimaryEmail == "team.member@example.com" && r.PersonId.HasValue);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var member = await db.People.SingleAsync(p => p.PrimaryEmail == "team.member@example.com");
        var lead = await db.People.SingleAsync(p => p.PrimaryEmail == "lead.person@example.com");
        Assert.Equal(lead.Id, member.ManagerPersonId);
    }

    [Fact]
    public async Task Bulk_import_denied_for_non_writer_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var request = Authorized(HttpMethod.Post, "/api/people/import", token);
        request.Content = JsonContent.Create(new BulkPersonImportRequest(
        [
            new BulkPersonImportRowRequest("Denied", "Import", "denied.import@example.com"),
        ]));

        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Bulk_import_reports_duplicate_email_within_batch()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var request = Authorized(HttpMethod.Post, "/api/people/import", token);
        request.Content = JsonContent.Create(new BulkPersonImportRequest(
        [
            new BulkPersonImportRowRequest("First", "Row", "duplicate@example.com"),
            new BulkPersonImportRowRequest("Second", "Row", "duplicate@example.com"),
        ]));

        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<BulkPersonImportResponse>())!;

        Assert.Equal(1, payload.CreatedCount);
        Assert.Equal(1, payload.ErrorCount);
        Assert.Contains(payload.Results, r => r.Status == "error" && r.ErrorCode == "people.email_conflict");
    }

    [Fact]
    public async Task Bulk_import_reports_existing_tenant_email_conflict()
    {
        await SeedPersonAsync(Guid.NewGuid(), "Existing", "User", "existing.user@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var request = Authorized(HttpMethod.Post, "/api/people/import", token);
        request.Content = JsonContent.Create(new BulkPersonImportRequest(
        [
            new BulkPersonImportRowRequest("New", "User", "existing.user@example.com"),
        ]));

        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<BulkPersonImportResponse>())!;

        Assert.Equal(0, payload.CreatedCount);
        Assert.Equal(1, payload.ErrorCount);
        Assert.Contains(payload.Results, r => r.ErrorCode == "people.email_conflict");
    }

    [Fact]
    public async Task Bulk_import_dry_run_validates_without_persisting()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "staffarr_admin");
        var request = Authorized(HttpMethod.Post, "/api/people/import", token);
        request.Content = JsonContent.Create(new BulkPersonImportRequest(
        [
            new BulkPersonImportRowRequest("Dry", "Run", "dry.run@example.com"),
        ],
        DryRun: true));

        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<BulkPersonImportResponse>())!;

        Assert.True(payload.DryRun);
        Assert.Equal(1, payload.ValidatedCount);
        Assert.Equal(0, payload.CreatedCount);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        Assert.False(await db.People.AnyAsync(p => p.PrimaryEmail == "dry.run@example.com"));
    }

    [Fact]
    public async Task Bulk_import_writes_batch_audit_event()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var request = Authorized(HttpMethod.Post, "/api/people/import", token);
        request.Content = JsonContent.Create(new BulkPersonImportRequest(
        [
            new BulkPersonImportRowRequest("Audit", "One", "audit.one@example.com"),
            new BulkPersonImportRowRequest("Audit", "Two", "audit.two@example.com"),
        ]));

        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var batchEvents = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "person.import.batch");
        Assert.Equal(1, batchEvents);
    }

    private async Task SeedPersonAsync(Guid personId, string givenName, string familyName, string email)
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
            EmploymentStatus = "active",
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
