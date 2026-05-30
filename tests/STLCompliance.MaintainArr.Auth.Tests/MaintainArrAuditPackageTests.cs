using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrAuditPackageTests : IAsyncLifetime
{
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"MaintainArrAuditPackage-{Guid.NewGuid():N}";

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Audit_package_manifest_lists_sections_v2()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], tenantRoleKey: "maintainarr_manager");
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/manifest", token));
        response.EnsureSuccessStatusCode();
        var manifest = (await response.Content.ReadFromJsonAsync<AuditPackageManifestResponse>())!;
        Assert.Equal("2", manifest.PackageVersion);
        Assert.Contains(manifest.Sections, section => section.FileName == "audit_events.csv");
        Assert.Equal(7, manifest.Sections.Count);
    }

    [Fact]
    public async Task Audit_package_export_zip_contains_csv_and_json()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], tenantRoleKey: "maintainarr_admin");
        await SeedAuditEventAsync("work_order.create", "success");

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export", token));
        response.EnsureSuccessStatusCode();

        var zipBytes = await response.Content.ReadAsByteArrayAsync();
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        Assert.Equal(8, archive.Entries.Count);
        Assert.Contains(archive.Entries, entry => entry.Name == "audit_events.csv");
        Assert.Contains(archive.Entries, entry => entry.Name == "assets.json");
    }

    [Fact]
    public async Task Filter_options_and_summary_reflect_action_filter()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], tenantRoleKey: "maintainarr_admin");
        await SeedAuditEventAsync("work_order.create", "success");
        await SeedAuditEventAsync("defect.create", "failed");

        var optionsResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/filter-options", token));
        optionsResponse.EnsureSuccessStatusCode();
        var options = (await optionsResponse.Content.ReadFromJsonAsync<AuditPackageFilterOptionsResponse>())!;
        Assert.Contains("work_order.create", options.Actions);

        var summaryResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/summary?action=work_order.create", token));
        summaryResponse.EnsureSuccessStatusCode();
        var summary = (await summaryResponse.Content.ReadFromJsonAsync<AuditPackageExportSummaryResponse>())!;
        Assert.Equal(1, summary.Counts.AuditEvents);
        Assert.Equal("work_order.create", summary.Filters.Action);
    }

    [Fact]
    public async Task Export_csv_returns_csv_content_type()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], tenantRoleKey: "tenant_admin");
        await SeedAuditEventAsync("asset.create", "success");

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=csv", token));
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Audit_package_timeline_respects_action_filter()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], tenantRoleKey: "maintainarr_manager");
        await SeedAuditEventAsync("work_order.create", "success");
        await SeedAuditEventAsync("defect.create", "failed");

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/timeline?action=defect.create&pageSize=20", token));
        response.EnsureSuccessStatusCode();
        var timeline = (await response.Content.ReadFromJsonAsync<PagedResult<AuditEventExportItem>>())!;
        Assert.All(timeline.Items, item => Assert.Equal("defect.create", item.Action));
        Assert.Equal(1, timeline.TotalCount);
    }

    [Fact]
    public async Task Audit_package_v1_aliases_manifest_summary_timeline_and_export_work()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], tenantRoleKey: "maintainarr_admin");
        await SeedAuditEventAsync("work_order.create", "success");
        await SeedAuditEventAsync("defect.create", "failed");

        var manifestResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages/manifest", token));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<AuditPackageManifestResponse>())!;
        Assert.Equal("2", manifest.PackageVersion);

        var summaryResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages/summary?action=work_order.create", token));
        summaryResponse.EnsureSuccessStatusCode();
        var summary = (await summaryResponse.Content.ReadFromJsonAsync<AuditPackageExportSummaryResponse>())!;
        Assert.Equal(1, summary.Counts.AuditEvents);

        var timelineResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages/timeline?action=defect.create&pageSize=20", token));
        timelineResponse.EnsureSuccessStatusCode();
        var timeline = (await timelineResponse.Content.ReadFromJsonAsync<PagedResult<AuditEventExportItem>>())!;
        Assert.All(timeline.Items, item => Assert.Equal("defect.create", item.Action));

        var exportResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages/export?format=csv", token));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Audit_v1_alias_matches_audit_package_timeline()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], tenantRoleKey: "maintainarr_admin");
        await SeedAuditEventAsync("work_order.create", "success");
        await SeedAuditEventAsync("defect.create", "failed");

        var timelineResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages/timeline?action=defect.create&pageSize=20", token));
        timelineResponse.EnsureSuccessStatusCode();
        var timeline = (await timelineResponse.Content.ReadFromJsonAsync<PagedResult<AuditEventExportItem>>())!;

        var auditAliasResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit?action=defect.create&pageSize=20", token));
        auditAliasResponse.EnsureSuccessStatusCode();
        var auditAlias = (await auditAliasResponse.Content.ReadFromJsonAsync<PagedResult<AuditEventExportItem>>())!;

        Assert.Equal(timeline.TotalCount, auditAlias.TotalCount);
        Assert.Equal(timeline.Items.Count, auditAlias.Items.Count);
        Assert.All(auditAlias.Items, item => Assert.Equal("defect.create", item.Action));
    }

    private async Task SeedAuditEventAsync(string action, string result)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        db.AuditEvents.Add(new MaintainArrAuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ActorUserId = PlatformSeeder.DemoAdminUserId,
            Action = action,
            TargetType = "test",
            TargetId = Guid.NewGuid().ToString(),
            Result = result,
            CorrelationId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
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
