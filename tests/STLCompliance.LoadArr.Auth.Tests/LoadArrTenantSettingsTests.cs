using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LoadArr.Api.Data;
using LoadArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace STLCompliance.LoadArr.Auth.Tests;

public sealed class LoadArrTenantSettingsTests : IAsyncLifetime
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
        var loadArrDbName = $"LoadArrTenantSettings-{Guid.NewGuid():N}";

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
    public async Task Tenant_settings_get_seeds_defaults_and_audit_without_internal_ids()
    {
        var readToken = CreateLoadArrAccessToken(["loadarr"], "warehouse_manager");
        var adminToken = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/loadarr/tenant-settings", readToken));
        response.EnsureSuccessStatusCode();

        var settingsResponse = await ReadJsonObjectAsync(response);
        var settings = settingsResponse["settings"]!.AsObject();

        Assert.False(settingsResponse.ContainsKey("tenantId"));
        Assert.Equal(1, settingsResponse["version"]!.GetValue<int>());
        Assert.False(string.IsNullOrWhiteSpace(settingsResponse["rowVersion"]!.GetValue<string>()));
        Assert.Equal(13, settings.Count);
        Assert.True(settings["receiving"]!["allowPurchaseOrderReceiving"]!.GetValue<bool>());
        Assert.True(settings["warehouseOperatingModel"]!["enableReceiving"]!.GetValue<bool>());

        var auditResponse = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/loadarr/tenant-settings/audit", adminToken));
        auditResponse.EnsureSuccessStatusCode();

        var audit = await ReadJsonObjectAsync(auditResponse);
        var firstEntry = audit["items"]!.AsArray()[0]!.AsObject();

        Assert.Equal(1, audit["total"]!.GetValue<int>());
        Assert.False(firstEntry.ContainsKey("id"));
        Assert.False(firstEntry.ContainsKey("tenantId"));
        Assert.Equal("all", firstEntry["sectionKey"]!.GetValue<string>());
        Assert.Equal("seed", firstEntry["changeSource"]!.GetValue<string>());
    }

    [Fact]
    public async Task Tenant_settings_get_allows_warehouse_manager_after_non_loadarr_launch_context()
    {
        var readToken = CreateLoadArrAccessToken(["nexarr"], "warehouse_manager");

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/loadarr/tenant-settings", readToken));
        response.EnsureSuccessStatusCode();

        var settingsResponse = await ReadJsonObjectAsync(response);
        Assert.Equal(1, settingsResponse["version"]!.GetValue<int>());
    }

    [Fact]
    public async Task Session_bootstrap_allows_warehouse_manager_after_non_loadarr_launch_context()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "warehouse_manager");

        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/session", token));
        response.EnsureSuccessStatusCode();

        var session = await ReadJsonObjectAsync(response);
        Assert.Equal("loadarr", session["productKey"]!.GetValue<string>());
        Assert.True(session["hasLoadArrAccess"]!.GetValue<bool>());
        Assert.Contains(
            session["launchableProductKeys"]!.AsArray(),
            item => string.Equals(item?.GetValue<string>(), "nexarr", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Tenant_settings_put_rejects_blocking_validation_errors()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");
        var current = await GetCurrentSettingsAsync(token);
        var settings = current["settings"]!.DeepClone().AsObject();
        settings["receiving"]!["overReceiptTolerancePercent"] = 101;

        var response = await _loadarrClient.SendAsync(
            AuthorizedJson(
                HttpMethod.Put,
                "/api/v1/loadarr/tenant-settings",
                token,
                new
                {
                    rowVersion = current["rowVersion"]!.GetValue<string>(),
                    settings,
                    reason = "Invalid tolerance should fail."
                }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("loadarr.settings.validation_failed", body["errorCode"]!.GetValue<string>());
        Assert.Contains(
            body["validation"]!["errors"]!.AsArray(),
            error => error?["code"]?.GetValue<string>() == "loadarr.settings.receiving.overReceiptTolerancePercent.out_of_range");
    }

    [Fact]
    public async Task Tenant_settings_put_requires_warning_acknowledgement_for_risky_changes()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");
        var current = await GetCurrentSettingsAsync(token);
        var settings = current["settings"]!.DeepClone().AsObject();
        settings["inventoryControl"]!["allowNegativeInventory"] = true;

        var blocked = await _loadarrClient.SendAsync(
            AuthorizedJson(
                HttpMethod.Put,
                "/api/v1/loadarr/tenant-settings",
                token,
                new
                {
                    rowVersion = current["rowVersion"]!.GetValue<string>(),
                    settings,
                    reason = "Exercise warning gate."
                }));

        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);

        var saved = await _loadarrClient.SendAsync(
            AuthorizedJson(
                HttpMethod.Put,
                "/api/v1/loadarr/tenant-settings",
                token,
                new
                {
                    rowVersion = current["rowVersion"]!.GetValue<string>(),
                    settings,
                    reason = "Temporary recovery policy.",
                    warningsAcknowledged = new[] { "loadarr.settings.inventory.negative_inventory" }
                }));
        saved.EnsureSuccessStatusCode();

        var body = await ReadJsonObjectAsync(saved);
        Assert.Equal(2, body["version"]!.GetValue<int>());
        Assert.True(body["settings"]!["inventoryControl"]!["allowNegativeInventory"]!.GetValue<bool>());
    }

    [Fact]
    public async Task Tenant_settings_section_reset_restores_defaults_and_versions()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");
        var current = await GetCurrentSettingsAsync(token);
        var settings = current["settings"]!.DeepClone().AsObject();
        settings["receiving"]!["allowOverReceipt"] = false;
        settings["receiving"]!["overReceiptTolerancePercent"] = 0;

        var changed = await _loadarrClient.SendAsync(
            AuthorizedJson(
                HttpMethod.Put,
                "/api/v1/loadarr/tenant-settings",
                token,
                new
                {
                    rowVersion = current["rowVersion"]!.GetValue<string>(),
                    settings,
                    reason = "Disable over-receipt for a policy test."
                }));
        changed.EnsureSuccessStatusCode();
        var changedBody = await ReadJsonObjectAsync(changed);

        var reset = await _loadarrClient.SendAsync(
            AuthorizedJson(
                HttpMethod.Post,
                "/api/v1/loadarr/tenant-settings/receiving/reset",
                token,
                new
                {
                    rowVersion = changedBody["rowVersion"]!.GetValue<string>(),
                    reason = "Restore receiving defaults."
                }));
        reset.EnsureSuccessStatusCode();

        var resetBody = await ReadJsonObjectAsync(reset);
        Assert.Equal(3, resetBody["version"]!.GetValue<int>());
        Assert.True(resetBody["settings"]!["receiving"]!["allowOverReceipt"]!.GetValue<bool>());
        Assert.Equal(5m, resetBody["settings"]!["receiving"]!["overReceiptTolerancePercent"]!.GetValue<decimal>());
    }

    [Fact]
    public async Task Tenant_settings_put_rejects_stale_row_versions()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");
        var current = await GetCurrentSettingsAsync(token);
        var settings = current["settings"]!.DeepClone().AsObject();
        settings["receiving"]!["requireVendorPackingSlip"] = true;

        var saved = await _loadarrClient.SendAsync(
            AuthorizedJson(
                HttpMethod.Put,
                "/api/v1/loadarr/tenant-settings",
                token,
                new
                {
                    rowVersion = current["rowVersion"]!.GetValue<string>(),
                    settings,
                    reason = "First save."
                }));
        saved.EnsureSuccessStatusCode();

        settings["receiving"]!["requireBolOrPod"] = true;
        var stale = await _loadarrClient.SendAsync(
            AuthorizedJson(
                HttpMethod.Put,
                "/api/v1/loadarr/tenant-settings",
                token,
                new
                {
                    rowVersion = current["rowVersion"]!.GetValue<string>(),
                    settings,
                    reason = "Second save with stale row version."
                }));

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var body = await ReadJsonObjectAsync(stale);
        Assert.Equal("loadarr.settings.concurrency_conflict", body["errorCode"]!.GetValue<string>());
    }

    private async Task<JsonObject> GetCurrentSettingsAsync(string token)
    {
        var response = await _loadarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/loadarr/tenant-settings", token));
        response.EnsureSuccessStatusCode();
        return await ReadJsonObjectAsync(response);
    }

    private string CreateLoadArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey,
        bool isPlatformAdmin = false)
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
            tenantRoleKey,
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

    private static HttpRequestMessage AuthorizedJson<TBody>(
        HttpMethod method,
        string path,
        string token,
        TBody body)
    {
        var request = Authorized(method, path, token);
        request.Content = JsonContent.Create(body);
        return request;
    }

    private static async Task<JsonObject> ReadJsonObjectAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?.AsObject()
            ?? throw new InvalidOperationException("Expected a JSON object response.");
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
