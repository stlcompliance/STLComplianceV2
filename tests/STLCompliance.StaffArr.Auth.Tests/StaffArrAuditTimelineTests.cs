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

public sealed class StaffArrAuditTimelineTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrAuditTimeline-{Guid.NewGuid():N}";

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
    public async Task Audit_timeline_returns_paged_audit_events()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        await SeedAuditEventsWithDatesAsync();

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit?page=1&pageSize=10", supervisorToken));
        response.EnsureSuccessStatusCode();
        var timeline = (await response.Content.ReadFromJsonAsync<PagedResult<StaffArrAuditEventExportItem>>())!;
        Assert.Equal(2, timeline.TotalCount);
        Assert.Equal(2, timeline.Items.Count);
        Assert.Equal("org_unit.update", timeline.Items[0].Action);
    }

    [Fact]
    public async Task Audit_timeline_date_filter_limits_results()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        await SeedAuditEventsWithDatesAsync();

        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 23, 59, 59, TimeSpan.Zero);
        var response = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit?from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}",
                supervisorToken));
        response.EnsureSuccessStatusCode();
        var timeline = (await response.Content.ReadFromJsonAsync<PagedResult<StaffArrAuditEventExportItem>>())!;
        Assert.Equal(1, timeline.TotalCount);
        Assert.Equal("org_unit.create", timeline.Items[0].Action);
    }

    [Fact]
    public async Task Audit_timeline_action_filter_limits_results()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        await SeedActionFilteredAuditEventsAsync();

        var response = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/audit?action=w228.test.failure&pageSize=20",
                supervisorToken));
        response.EnsureSuccessStatusCode();
        var timeline =
            (await response.Content.ReadFromJsonAsync<PagedResult<StaffArrAuditEventExportItem>>())!;
        Assert.All(timeline.Items, item => Assert.Equal("w228.test.failure", item.Action));
    }

    [Fact]
    public async Task Audit_timeline_v1_alias_matches_legacy_route()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        await SeedActionFilteredAuditEventsAsync();

        var legacyResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit?page=1&pageSize=10", token));
        legacyResponse.EnsureSuccessStatusCode();
        var legacy = (await legacyResponse.Content.ReadFromJsonAsync<PagedResult<StaffArrAuditEventExportItem>>())!;

        var v1Response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit?page=1&pageSize=10", token));
        v1Response.EnsureSuccessStatusCode();
        var v1 = (await v1Response.Content.ReadFromJsonAsync<PagedResult<StaffArrAuditEventExportItem>>())!;

        Assert.Equal(legacy.TotalCount, v1.TotalCount);
        Assert.Equal(legacy.Items[0].Action, v1.Items[0].Action);
    }

    [Fact]
    public async Task Audit_timeline_rejects_invalid_date_range()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var from = DateTimeOffset.UtcNow;
        var to = from.AddDays(-1);
        var response = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit?from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}",
                adminToken));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_without_staffarr_role_cannot_read_audit_timeline()
    {
        await SeedAuditEventsWithDatesAsync();
        var platformAdminToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            isPlatformAdmin: true);

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit?page=1&pageSize=10", platformAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task SeedActionFilteredAuditEventsAsync()
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.AuditEvents.AddRange(
            new StaffArrAuditEvent
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                Action = "w228.test.success",
                TargetType = "person",
                Result = "success",
                CorrelationId = Guid.NewGuid(),
                OccurredAt = now,
            },
            new StaffArrAuditEvent
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                Action = "w228.test.failure",
                TargetType = "person",
                Result = "failure",
                CorrelationId = Guid.NewGuid(),
                OccurredAt = now,
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
