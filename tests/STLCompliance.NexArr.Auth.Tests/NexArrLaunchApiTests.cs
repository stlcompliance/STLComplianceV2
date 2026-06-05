using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrLaunchApiTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrLaunchApiTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<NexArrDbContext>)
                        || d.ServiceType == typeof(NexArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<NexArrDbContext>(options =>
                    options.UseInMemoryDatabase("NexArrLaunchTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Launch_context_requires_authentication()
    {
        var response = await _client.GetAsync("/api/launch/context?productKey=staffarr");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Launch_catalog_requires_authentication()
    {
        var response = await _client.GetAsync("/api/launch/catalog");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Launch_catalog_v1_requires_authentication()
    {
        var response = await _client.GetAsync("/api/v1/launch/catalog");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Launch_context_v1_requires_authentication()
    {
        var response = await _client.GetAsync("/api/v1/launch/context?productKey=staffarr");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Settings_manifest_v1_requires_platform_admin()
    {
        await SeedDatabaseAsync();
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var forbiddenResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", tenantAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var manifestResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", platformAdminToken));
        manifestResponse.EnsureSuccessStatusCode();

        var manifest = await manifestResponse.Content.ReadFromJsonAsync<NexArrSettingsManifestResponse>();
        Assert.NotNull(manifest);
        Assert.Contains(manifest.Items, x => x.SettingKey == "platform_service_token_cleanup_settings");
        Assert.Contains(manifest.Items, x => x.SettingKey == "platform_outbox_publisher_settings");
        Assert.Contains(manifest.Items, x => x.SettingKey == "platform_entitlement_reconciliation_settings");
    }

    [Fact]
    public async Task Config_manifest_v1_matches_settings_manifest_for_platform_admin()
    {
        await SeedDatabaseAsync();
        var platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var configResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/config", platformAdminToken));
        configResponse.EnsureSuccessStatusCode();
        var configManifest = (await configResponse.Content.ReadFromJsonAsync<NexArrSettingsManifestResponse>())!;

        var settingsResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", platformAdminToken));
        settingsResponse.EnsureSuccessStatusCode();
        var settingsManifest = (await settingsResponse.Content.ReadFromJsonAsync<NexArrSettingsManifestResponse>())!;

        Assert.Equal(settingsManifest.Items.Count, configManifest.Items.Count);
        foreach (var item in settingsManifest.Items)
        {
            Assert.Contains(configManifest.Items, x => x.SettingKey == item.SettingKey);
        }
    }

    [Fact]
    public async Task Platform_admin_gets_launch_context_for_entitled_product()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/launch/context?productKey=staffarr", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var context = await response.Content.ReadFromJsonAsync<LaunchContextResponse>();
        Assert.NotNull(context);
        Assert.True(context.CanLaunch);
        Assert.Equal("staffarr", context.ProductKey);
        Assert.Contains("5175", context.BaseLaunchUrl);
    }

    [Fact]
    public async Task Platform_admin_gets_launch_context_v1_for_entitled_product()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/launch/context?productKey=staffarr", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var context = await response.Content.ReadFromJsonAsync<LaunchContextResponse>();
        Assert.NotNull(context);
        Assert.True(context.CanLaunch);
        Assert.Equal("staffarr", context.ProductKey);
        Assert.Contains("5175", context.BaseLaunchUrl);
    }

    [Fact]
    public async Task Launch_context_denied_without_product_entitlement()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/launch/context?productKey=nonexistent-product", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Launch_catalog_returns_entitled_launchable_products_with_current_indicator()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/launch/catalog?currentProductKey=staffarr", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var catalog = await response.Content.ReadFromJsonAsync<LaunchCatalogResponse>();
        Assert.NotNull(catalog);
        Assert.Equal(PlatformSeeder.DemoTenantId, catalog.TenantId);
        Assert.Equal(PlatformSeeder.DemoTenantAdminUserId, catalog.UserId);
        Assert.Equal(PlatformSeeder.DemoTenantAdminEmail, catalog.UserEmail);
        Assert.False(string.IsNullOrWhiteSpace(catalog.UserDisplayName));
        Assert.Equal("staffarr", catalog.CurrentProductKey);
        Assert.False(string.IsNullOrWhiteSpace(catalog.CatalogVersion));
        Assert.True(catalog.CacheExpiresAt > catalog.GeneratedAt);
        Assert.NotEmpty(catalog.Products);

        var staffarr = catalog.Products.FirstOrDefault(x => x.ProductKey == "staffarr");
        Assert.NotNull(staffarr);
        Assert.True(staffarr.IsCurrentProduct);
        Assert.Equal("workforce", staffarr.ProductCategory);
        Assert.Equal("People Operations", staffarr.ProductOwner);
        Assert.Equal("stl:staffarr:api", staffarr.ServiceAudience);
        Assert.Equal("/launch/staffarr", staffarr.LaunchUrl);
        Assert.Contains(catalog.Products, x => x.ProductKey == "loadarr");
        Assert.Contains(catalog.Products, x => x.ProductKey == "recordarr");
        Assert.Contains(catalog.Products, x => x.ProductKey == "reportarr");
        Assert.Contains(catalog.Products, x => x.ProductKey == "assurarr");
        Assert.Contains(catalog.Products, x => x.ProductKey == "fieldcompanion");

        Assert.DoesNotContain(catalog.Products, x => x.ProductKey == "shared-worker");
        Assert.DoesNotContain(catalog.Products, x => x.ProductKey == "nexarr-worker");
    }

    [Fact]
    public async Task Launch_catalog_version_changes_after_entitlement_revocation()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var firstResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/launch/catalog?currentProductKey=staffarr", token));
        firstResponse.EnsureSuccessStatusCode();
        var firstCatalog = (await firstResponse.Content.ReadFromJsonAsync<LaunchCatalogResponse>())!;
        Assert.Contains(firstCatalog.Products, x => x.ProductKey == "staffarr");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var entitlement = await db.Entitlements.SingleAsync(e =>
                e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "staffarr");
            entitlement.Status = EntitlementStatuses.Revoked;
            entitlement.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }

        var secondResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/launch/catalog?currentProductKey=staffarr", token));
        secondResponse.EnsureSuccessStatusCode();
        var secondCatalog = (await secondResponse.Content.ReadFromJsonAsync<LaunchCatalogResponse>())!;

        Assert.NotEqual(firstCatalog.CatalogVersion, secondCatalog.CatalogVersion);
        Assert.DoesNotContain(secondCatalog.Products, x => x.ProductKey == "staffarr");
    }

    [Fact]
    public async Task Launch_catalog_v1_returns_entitled_launchable_products_with_current_indicator()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/launch/catalog?currentProductKey=staffarr", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var catalog = await response.Content.ReadFromJsonAsync<LaunchCatalogResponse>();
        Assert.NotNull(catalog);
        Assert.Equal(PlatformSeeder.DemoTenantId, catalog.TenantId);
        Assert.Equal(PlatformSeeder.DemoTenantAdminUserId, catalog.UserId);
        Assert.Equal("staffarr", catalog.CurrentProductKey);
        Assert.False(string.IsNullOrWhiteSpace(catalog.CatalogVersion));
        Assert.NotEmpty(catalog.Products);

        var staffarr = catalog.Products.FirstOrDefault(x => x.ProductKey == "staffarr");
        Assert.NotNull(staffarr);
        Assert.True(staffarr.IsCurrentProduct);
        Assert.Equal("workforce", staffarr.ProductCategory);
        Assert.Equal("/launch/staffarr", staffarr.LaunchUrl);
    }

    [Fact]
    public async Task Handoff_create_and_redeem_with_service_token_succeeds()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        const string callbackUrl = "http://localhost:5173/app/staffarr";

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", callbackUrl));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(handoff.HandoffCode));
        Assert.Contains("handoff=", handoff.LaunchUrl);

        var serviceToken = await IssueServiceTokenAsync(token, "staffarr");

        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", token);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, serviceToken));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        redeemResponse.EnsureSuccessStatusCode();
        var redeemed = await redeemResponse.Content.ReadFromJsonAsync<HandoffRedeemedResponse>();
        Assert.NotNull(redeemed);
        Assert.Equal(PlatformSeeder.DemoAdminUserId, redeemed.UserId);
        Assert.Equal("staffarr", redeemed.TargetProductKey);
        Assert.Equal("STL Demo Tenant", redeemed.TenantDisplayName);
        Assert.Equal("demo-stl", redeemed.TenantSlug);
        Assert.Equal(callbackUrl, redeemed.CallbackUrl);
    }

    [Fact]
    public async Task Handoff_create_and_redeem_enqueue_platform_launch_events()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        const string callbackUrl = "http://localhost:5173/app/staffarr";

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", callbackUrl));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var serviceToken = await IssueServiceTokenAsync(token, "staffarr");
        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", token);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, serviceToken));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        redeemResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var outboxEvents = await db.PlatformOutboxEvents
            .AsNoTracking()
            .Where(x => x.PayloadJson.Contains(handoff.HandoffId.ToString()))
            .ToListAsync();

        var launchEvent = Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.LaunchSucceeded);
        Assert.Equal(PlatformOutboxEventStatuses.Pending, launchEvent.ProcessingStatus);
        Assert.Equal(PlatformSeeder.DemoTenantId, launchEvent.TenantId);
        Assert.Equal("staffarr", launchEvent.ProductCode);
        Assert.Contains("\"callbackConfigured\":\"true\"", launchEvent.PayloadJson);
        Assert.DoesNotContain(handoff.HandoffCode, launchEvent.PayloadJson);

        var redeemedEvent = Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.HandoffRedeemed);
        Assert.Equal(PlatformOutboxEventStatuses.Pending, redeemedEvent.ProcessingStatus);
        Assert.Equal(PlatformSeeder.DemoTenantId, redeemedEvent.TenantId);
        Assert.Equal("staffarr", redeemedEvent.ProductCode);
        Assert.Contains("\"tenantRoleKey\":\"platform_admin\"", redeemedEvent.PayloadJson);
        Assert.DoesNotContain(handoff.HandoffCode, redeemedEvent.PayloadJson);
        Assert.DoesNotContain(serviceToken, redeemedEvent.PayloadJson);
    }

    [Fact]
    public async Task Launch_and_handoff_failures_enqueue_platform_events()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var blockedHandoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        blockedHandoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", "https://evil.example/callback"));
        var blockedHandoffResponse = await _client.SendAsync(blockedHandoffRequest);
        Assert.Equal(HttpStatusCode.Forbidden, blockedHandoffResponse.StatusCode);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var serviceToken = await IssueServiceTokenAsync(token, "staffarr");
        var firstRedeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", token);
        firstRedeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, serviceToken));
        (await _client.SendAsync(firstRedeemRequest)).EnsureSuccessStatusCode();

        var secondRedeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", token);
        secondRedeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, serviceToken));
        var secondRedeemResponse = await _client.SendAsync(secondRedeemRequest);
        Assert.Equal(HttpStatusCode.Conflict, secondRedeemResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();

        var launchFailedEvent = await db.PlatformOutboxEvents
            .AsNoTracking()
            .SingleAsync(x =>
                x.EventType == PlatformOutboxEventKinds.LaunchFailed
                && x.ProductCode == "staffarr"
                && x.PayloadJson.Contains("callback_not_allowed"));
        Assert.Equal(PlatformOutboxEventStatuses.Pending, launchFailedEvent.ProcessingStatus);
        Assert.DoesNotContain("https://evil.example", launchFailedEvent.PayloadJson);

        var handoffFailedEvent = await db.PlatformOutboxEvents
            .AsNoTracking()
            .SingleAsync(x =>
                x.EventType == PlatformOutboxEventKinds.HandoffFailed
                && x.PayloadJson.Contains(handoff.HandoffId.ToString())
                && x.PayloadJson.Contains("already_redeemed"));
        Assert.Equal(PlatformOutboxEventStatuses.Pending, handoffFailedEvent.ProcessingStatus);
        Assert.DoesNotContain(handoff.HandoffCode, handoffFailedEvent.PayloadJson);
        Assert.DoesNotContain(serviceToken, handoffFailedEvent.PayloadJson);
    }

    [Fact]
    public async Task Handoff_create_v1_and_redeem_with_service_token_succeeds()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        const string callbackUrl = "http://localhost:5173/app/staffarr";

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", callbackUrl));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(handoff.HandoffCode));

        var serviceToken = await IssueServiceTokenAsync(token, "staffarr");

        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", token);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, serviceToken));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        redeemResponse.EnsureSuccessStatusCode();
        var redeemed = await redeemResponse.Content.ReadFromJsonAsync<HandoffRedeemedResponse>();
        Assert.NotNull(redeemed);
        Assert.Equal(PlatformSeeder.DemoAdminUserId, redeemed.UserId);
        Assert.Equal("staffarr", redeemed.TargetProductKey);
    }

    [Fact]
    public async Task Handoff_redeem_v1_with_service_token_succeeds()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        const string callbackUrl = "http://localhost:5173/app/staffarr";

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", callbackUrl));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var serviceToken = await IssueServiceTokenAsync(token, "staffarr");

        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", token);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, serviceToken));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        redeemResponse.EnsureSuccessStatusCode();
        var redeemed = await redeemResponse.Content.ReadFromJsonAsync<HandoffRedeemedResponse>();
        Assert.NotNull(redeemed);
        Assert.Equal(PlatformSeeder.DemoAdminUserId, redeemed.UserId);
        Assert.Equal("staffarr", redeemed.TargetProductKey);
    }

    [Fact]
    public async Task Handoff_redeem_launch_v1_alias_with_service_token_succeeds()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        const string callbackUrl = "http://localhost:5173/app/staffarr";

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", callbackUrl));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var serviceToken = await IssueServiceTokenAsync(token, "staffarr");

        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff/redeem", token);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, serviceToken));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        redeemResponse.EnsureSuccessStatusCode();
        var redeemed = await redeemResponse.Content.ReadFromJsonAsync<HandoffRedeemedResponse>();
        Assert.NotNull(redeemed);
        Assert.Equal(PlatformSeeder.DemoAdminUserId, redeemed.UserId);
        Assert.Equal("staffarr", redeemed.TargetProductKey);
    }

    [Fact]
    public async Task Handoff_create_rejects_disallowed_callback()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", "https://evil.example/callback"));
        var handoffResponse = await _client.SendAsync(handoffRequest);

        Assert.Equal(HttpStatusCode.Forbidden, handoffResponse.StatusCode);
    }

    [Fact]
    public async Task Handoff_redeem_without_service_token_denied_for_tenant_admin()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", adminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", tenantAdminToken);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, null));
        var redeemResponse = await _client.SendAsync(redeemRequest);

        Assert.Equal(HttpStatusCode.Forbidden, redeemResponse.StatusCode);
    }

    [Fact]
    public async Task Callback_validate_returns_allowed_for_seeded_origin()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/launch/callback/validate", token);
        request.Content = JsonContent.Create(new ValidateCallbackRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr",
            PlatformSeeder.DemoTenantId));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var validation = await response.Content.ReadFromJsonAsync<ValidateCallbackResponse>();

        Assert.NotNull(validation);
        Assert.True(validation.IsAllowed);
    }

    [Fact]
    public async Task Callback_validate_returns_denied_for_unknown_origin()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/launch/callback/validate", token);
        request.Content = JsonContent.Create(new ValidateCallbackRequest(
            "staffarr",
            "https://evil.example/callback",
            PlatformSeeder.DemoTenantId));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var validation = await response.Content.ReadFromJsonAsync<ValidateCallbackResponse>();

        Assert.NotNull(validation);
        Assert.False(validation.IsAllowed);
        Assert.Equal("callback_not_allowed", validation.ReasonCode);
    }

    [Fact]
    public async Task Callback_validate_v1_returns_allowed_for_seeded_origin()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/v1/launch/callback/validate", token);
        request.Content = JsonContent.Create(new ValidateCallbackRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr",
            PlatformSeeder.DemoTenantId));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var validation = await response.Content.ReadFromJsonAsync<ValidateCallbackResponse>();

        Assert.NotNull(validation);
        Assert.True(validation.IsAllowed);
    }

    [Fact]
    public async Task Launch_validate_v1_returns_launchable_result_for_entitled_product()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/v1/launch/validate", token);
        request.Content = JsonContent.Create(new ValidateLaunchRequest("staffarr", PlatformSeeder.DemoTenantId));
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var validation = await response.Content.ReadFromJsonAsync<ValidateLaunchResponse>();
        Assert.NotNull(validation);
        Assert.Equal("staffarr", validation.ProductKey);
        Assert.True(validation.CanLaunch);
        Assert.Null(validation.ReasonCode);
        Assert.NotNull(validation.LaunchUrl);
    }

    [Fact]
    public async Task Launch_validate_returns_reason_for_missing_product()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/launch/validate", token);
        request.Content = JsonContent.Create(new ValidateLaunchRequest("missing-product", PlatformSeeder.DemoTenantId));
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var validation = await response.Content.ReadFromJsonAsync<ValidateLaunchResponse>();
        Assert.NotNull(validation);
        Assert.False(validation.CanLaunch);
        Assert.Equal("product_not_found", validation.ReasonCode);
        Assert.Null(validation.LaunchUrl);
    }

    [Fact]
    public async Task Tenant_admin_cannot_create_callback_allowlist_entry()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/launch/callback-allowlist", token);
        request.Content = JsonContent.Create(new CreateCallbackAllowlistEntryRequest(
            "staffarr",
            null,
            "https://blocked.example",
            "origin"));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_cannot_create_callback_allowlist_entry_v1()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/v1/launch/callback-allowlist", token);
        request.Content = JsonContent.Create(new CreateCallbackAllowlistEntryRequest(
            "staffarr",
            null,
            "https://blocked.example",
            "origin"));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_can_list_callback_allowlist_v1_for_product()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/launch/callback-allowlist?productKey=staffarr", token));
        response.EnsureSuccessStatusCode();

        var entries = await response.Content.ReadFromJsonAsync<IReadOnlyList<CallbackAllowlistEntryResponse>>();
        Assert.NotNull(entries);
        Assert.NotEmpty(entries);
        Assert.Contains(entries, x => x.ProductKey == "staffarr");
    }

    [Fact]
    public async Task Platform_admin_can_delete_callback_allowlist_entry_v1()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var urlPattern = $"https://temp-delete-{Guid.NewGuid():N}.example";

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/launch/callback-allowlist", token);
        createRequest.Content = JsonContent.Create(new CreateCallbackAllowlistEntryRequest(
            "staffarr",
            null,
            urlPattern,
            "origin"));
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<CallbackAllowlistEntryResponse>())!;

        var deleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/launch/callback-allowlist/{created.EntryId}", token));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/launch/callback-allowlist?productKey=staffarr", token));
        listResponse.EnsureSuccessStatusCode();
        var entries = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<CallbackAllowlistEntryResponse>>())!;
        Assert.DoesNotContain(entries, x => x.EntryId == created.EntryId);
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-launch-test",
            $"{productKey} Launch Test",
            productKey,
            [productKey]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task SeedDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }
}
