using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LoadArr.Api.Data;
using LoadArr.Api.Endpoints;
using LoadArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.LoadArr.Auth.Tests;

public sealed class LoadArrFieldInboxTests : IAsyncLifetime
{
    private const string FrontendBaseUrl = "https://loadarr-frontend.test";
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid SecondaryTenantId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    private static readonly Guid DemoUserId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    private static readonly Guid DemoPersonId = Guid.Parse("33333333-3333-3333-3333-333333333301");

    private WebApplicationFactory<global::LoadArr.Api.Program> _loadarrFactory = null!;
    private HttpClient _loadarrClient = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var loadArrDbName = $"LoadArrFieldInbox-{Guid.NewGuid():N}";

        _loadarrFactory = new WebApplicationFactory<global::LoadArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("LoadArr:FrontendBaseUrl", FrontendBaseUrl);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<LoadArrDbContext>(services);
                services.AddDbContext<LoadArrDbContext>(options => options.UseInMemoryDatabase(loadArrDbName));
            });
        });

        _loadarrClient = _loadarrFactory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _loadarrClient.Dispose();
        await _loadarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Field_inbox_returns_empty_when_tenant_has_no_actionable_receiving_sessions()
    {
        var token = CreateLoadArrAccessToken(["loadarr"]);

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));
        response.EnsureSuccessStatusCode();

        var inbox = (await response.Content.ReadFromJsonAsync<FieldInboxResponse>())!;

        Assert.Equal(0, inbox.Summary.TotalCount);
        Assert.Empty(inbox.Items);
    }

    [Fact]
    public async Task Field_inbox_returns_persisted_tenant_scoped_receiving_tasks_with_loadarr_deep_links()
    {
        await SeedReceivingSessionAsync(
            DemoTenantId,
            CreateReceivingSession("recv-24018", "RCV-24018", "open", "PO-24018", "2026-06-27T08:00:00Z"));
        await SeedReceivingSessionAsync(
            DemoTenantId,
            CreateReceivingSession(
                "recv-8834",
                "RCV-8834",
                "inspection_required",
                "PO-8834",
                "2026-06-27T07:30:00Z"));
        await SeedReceivingSessionAsync(
            DemoTenantId,
            CreateReceivingSession("recv-persisted-complete", "RCV-0001", "completed", "PO-0001", "2026-06-27T07:00:00Z"));
        await SeedReceivingSessionAsync(
            SecondaryTenantId,
            CreateReceivingSession("recv-other-tenant", "RCV-SECONDARY", "open", "PO-SECONDARY", "2026-06-27T06:30:00Z"));

        var token = CreateLoadArrAccessToken(["loadarr"]);

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));
        response.EnsureSuccessStatusCode();

        var inbox = (await response.Content.ReadFromJsonAsync<FieldInboxResponse>())!;

        Assert.Equal(2, inbox.Summary.TotalCount);
        Assert.All(inbox.Items, item =>
        {
            Assert.Equal("loadarr", item.ProductKey);
            Assert.Equal("receiving", item.TaskType);
            Assert.True(Guid.TryParse(item.TaskKey.Split(':').LastOrDefault(), out _));
        });

        var openTask = inbox.Items.Single(item => item.Title == "RCV-24018");
        Assert.Equal("open", openTask.Status);
        Assert.Contains("/work/receiving/recv-24018", openTask.DeepLinkPath, StringComparison.Ordinal);
        Assert.Contains("taskKey=loadarr%3Areceiving%3A", openTask.DeepLinkPath, StringComparison.Ordinal);
        Assert.Equal($"{FrontendBaseUrl}{openTask.DeepLinkPath}", openTask.DeepLinkUrl);

        var blockedTask = inbox.Items.Single(item => item.Title == "RCV-8834");
        Assert.Equal("inspection_required", blockedTask.Status);
        Assert.Equal("Compliance inspection required before receiving can complete", blockedTask.BlockedReason);
        Assert.DoesNotContain(inbox.Items, item => item.Title == "RCV-SECONDARY");
    }

    [Fact]
    public async Task Field_inbox_v1_alias_returns_only_secondary_tenant_tasks()
    {
        await SeedReceivingSessionAsync(
            DemoTenantId,
            CreateReceivingSession("recv-demo-tenant", "RCV-DEMO", "open", "PO-DEMO", "2026-06-27T08:00:00Z"));
        await SeedReceivingSessionAsync(
            SecondaryTenantId,
            CreateReceivingSession("recv-secondary-tenant", "RCV-SECONDARY", "open", "PO-SECONDARY", "2026-06-27T09:00:00Z"));

        var token = CreateLoadArrAccessToken(["loadarr"], tenantId: SecondaryTenantId);

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/field-inbox", token));
        response.EnsureSuccessStatusCode();

        var inbox = (await response.Content.ReadFromJsonAsync<FieldInboxResponse>())!;
        Assert.Equal(1, inbox.Summary.TotalCount);
        Assert.Collection(
            inbox.Items,
            item =>
            {
                Assert.Equal("loadarr", item.ProductKey);
                Assert.Equal("RCV-SECONDARY", item.Title);
            });
    }

    [Fact]
    public async Task Field_inbox_allows_warehouse_manager_after_non_loadarr_launch_context()
    {
        await SeedReceivingSessionAsync(
            DemoTenantId,
            CreateReceivingSession("recv-nexarr-launch", "RCV-24018", "open", "PO-24018", "2026-06-27T08:00:00Z"));

        var token = CreateLoadArrAccessToken(["nexarr"]);

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));
        response.EnsureSuccessStatusCode();

        var inbox = (await response.Content.ReadFromJsonAsync<FieldInboxResponse>())!;
        Assert.Equal(1, inbox.Summary.TotalCount);
    }

    [Fact]
    public async Task Field_inbox_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], tenantRoleKey: "tenant_member");

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Field_inbox_rejects_platform_admin_without_loadarr_role()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Permission_catalog_allows_warehouse_manager_after_non_loadarr_launch_context()
    {
        var token = CreateLoadArrAccessToken(["nexarr"]);

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/admin/permissions", token));
        response.EnsureSuccessStatusCode();

        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);

        var permissions = payload!.RootElement.GetProperty("permissions");
        Assert.True(permissions.GetArrayLength() > 0);
        Assert.All(permissions.EnumerateArray(), item =>
            Assert.Equal("loadarr", item.GetProperty("productKey").GetString()));
    }

    [Fact]
    public async Task Permission_catalog_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], tenantRoleKey: "tenant_member");

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/admin/permissions", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Permission_catalog_rejects_platform_admin_without_loadarr_role()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/admin/permissions", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Field_inbox_requires_authentication()
    {
        var response = await _loadarrClient.GetAsync("/api/field-inbox");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private string CreateLoadArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "warehouse_manager",
        bool isPlatformAdmin = false,
        Guid? tenantId = null)
    {
        using var scope = _loadarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<LoadArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            DemoUserId,
            DemoPersonId,
            "warehouse.user@demo.stl",
            "Warehouse User",
            tenantId ?? DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin);
        return accessToken;
    }

    private async Task SeedReceivingSessionAsync(Guid tenantId, LoadArrReceivingSessionResponse session)
    {
        await using var scope = _loadarrFactory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<LoadArrOperationalWorkflowStore>();
        await store.SaveReceivingSessionAsync(tenantId, session, cancellationToken: CancellationToken.None);
    }

    private static LoadArrReceivingSessionResponse CreateReceivingSession(
        string sessionId,
        string receivingNumber,
        string status,
        string sourceObjectId,
        string startedAtUtc)
    {
        return new LoadArrReceivingSessionResponse(
            sessionId,
            receivingNumber,
            "purchase_order",
            status,
            "staff-site-stl-north",
            "STL North Yard",
            "supplyarr",
            "purchase_order",
            sourceObjectId,
            "Midwest Freight Supply",
            "person-inventory-clerk",
            null,
            startedAtUtc,
            null,
            new[]
            {
                new LoadArrReceivingLineResponse(
                    $"line-{sessionId}",
                    "SUP-VALVE-KIT-A",
                    "Valve repair kit A",
                    4m,
                    4m,
                    "each",
                    "loc-dock-01",
                    "Receiving Dock 1",
                    "L2405-77",
                    null,
                    "pending_inspection",
                    "ready_to_complete",
                    null,
                    "Received and staged for putaway")
            });
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
