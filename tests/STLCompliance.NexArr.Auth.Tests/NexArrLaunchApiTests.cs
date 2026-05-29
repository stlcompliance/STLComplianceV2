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
        Assert.Equal("staffarr", catalog.CurrentProductKey);
        Assert.NotEmpty(catalog.Products);

        var staffarr = catalog.Products.FirstOrDefault(x => x.ProductKey == "staffarr");
        Assert.NotNull(staffarr);
        Assert.True(staffarr.IsCurrentProduct);
        Assert.Equal("/launch/staffarr", staffarr.LaunchUrl);

        Assert.DoesNotContain(catalog.Products, x => x.ProductKey == "shared-worker");
        Assert.DoesNotContain(catalog.Products, x => x.ProductKey == "nexarr-worker");
    }

    [Fact]
    public async Task Handoff_create_and_redeem_with_service_token_succeeds()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        const string callbackUrl = "http://localhost:5173/app/staffarr";

        var handoffRequest = Authorized(HttpMethod.Post, "/api/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", callbackUrl));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(handoff.HandoffCode));
        Assert.Contains("handoff=", handoff.LaunchUrl);

        var serviceToken = await IssueServiceTokenAsync(token, "staffarr");

        var redeemRequest = Authorized(HttpMethod.Post, "/api/launch/handoff/redeem", token);
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
    public async Task Handoff_create_rejects_disallowed_callback()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", "https://evil.example/callback"));
        var handoffResponse = await _client.SendAsync(handoffRequest);

        Assert.Equal(HttpStatusCode.Forbidden, handoffResponse.StatusCode);
    }

    [Fact]
    public async Task Handoff_redeem_without_service_token_denied_for_tenant_admin()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/launch/handoff", adminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var redeemRequest = Authorized(HttpMethod.Post, "/api/launch/handoff/redeem", tenantAdminToken);
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
