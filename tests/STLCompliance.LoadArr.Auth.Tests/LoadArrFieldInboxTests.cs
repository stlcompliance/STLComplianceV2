using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LoadArr.Api.Data;
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
    public async Task Field_inbox_returns_owner_aligned_receiving_tasks_with_loadarr_deep_links()
    {
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
    }

    [Fact]
    public async Task Field_inbox_v1_alias_returns_owner_aligned_receiving_tasks()
    {
        var token = CreateLoadArrAccessToken(["loadarr"]);

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/field-inbox", token));
        response.EnsureSuccessStatusCode();

        var inbox = (await response.Content.ReadFromJsonAsync<FieldInboxResponse>())!;
        Assert.Equal(2, inbox.Summary.TotalCount);
        Assert.All(inbox.Items, item => Assert.Equal("loadarr", item.ProductKey));
    }

    [Fact]
    public async Task Field_inbox_allows_warehouse_manager_after_non_loadarr_launch_context()
    {
        var token = CreateLoadArrAccessToken(["nexarr"]);

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));
        response.EnsureSuccessStatusCode();

        var inbox = (await response.Content.ReadFromJsonAsync<FieldInboxResponse>())!;
        Assert.Equal(2, inbox.Summary.TotalCount);
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
    public async Task Field_inbox_requires_authentication()
    {
        var response = await _loadarrClient.GetAsync("/api/field-inbox");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private string CreateLoadArrAccessToken(IReadOnlyList<string> entitlements, bool isPlatformAdmin = false)
    {
        using var scope = _loadarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<LoadArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            DemoUserId,
            DemoPersonId,
            "warehouse.user@demo.stl",
            "Warehouse User",
            DemoTenantId,
            Guid.NewGuid(),
            "warehouse_manager",
            entitlements,
            isPlatformAdmin);
        return accessToken;
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
