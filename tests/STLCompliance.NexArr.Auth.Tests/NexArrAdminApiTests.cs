using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrAdminApiTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrAdminApiTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrAdminTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Tenants_list_requires_authentication()
    {
        var response = await _client.GetAsync("/api/tenants");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_create_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/tenants", token);
        request.Content = JsonContent.Create(new CreateTenantRequest("acme-corp", "Acme Corporation"));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var tenant = await response.Content.ReadFromJsonAsync<TenantDetailResponse>();
        Assert.NotNull(tenant);
        Assert.Equal("acme-corp", tenant.Slug);
    }

    [Fact]
    public async Task Tenant_admin_cannot_create_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/tenants", token);
        request.Content = JsonContent.Create(new CreateTenantRequest("blocked-corp", "Blocked Corporation"));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_can_read_own_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/tenants/{PlatformSeeder.DemoTenantId}", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tenant = await response.Content.ReadFromJsonAsync<TenantDetailResponse>();
        Assert.NotNull(tenant);
        Assert.Equal(PlatformSeeder.DemoTenantId, tenant.TenantId);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_other_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var otherTenantId = Guid.Parse("99999999-9999-9999-9999-999999999901");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/tenants/{otherTenantId}", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_create_product()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/products", token);
        request.Content = JsonContent.Create(new CreateProductRequest("companion", "Companion App", 80));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_grant_entitlement()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createTenant = Authorized(HttpMethod.Post, "/api/tenants", token);
        createTenant.Content = JsonContent.Create(new CreateTenantRequest("grant-test", "Grant Test Tenant"));
        var tenantResponse = await _client.SendAsync(createTenant);
        tenantResponse.EnsureSuccessStatusCode();
        var tenant = (await tenantResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;

        var grantRequest = Authorized(HttpMethod.Post, "/api/entitlements", token);
        grantRequest.Content = JsonContent.Create(new GrantEntitlementRequest(tenant.TenantId, "staffarr"));
        var grantResponse = await _client.SendAsync(grantRequest);

        Assert.Equal(HttpStatusCode.Created, grantResponse.StatusCode);
        var entitlement = await grantResponse.Content.ReadFromJsonAsync<EntitlementDetailResponse>();
        Assert.NotNull(entitlement);
        Assert.Equal("Active", entitlement.Status);
    }

    [Fact]
    public async Task Tenant_admin_cannot_grant_entitlement_for_other_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var otherTenantId = Guid.Parse("99999999-9999-9999-9999-999999999901");

        var grantRequest = Authorized(HttpMethod.Post, "/api/entitlements", token);
        grantRequest.Content = JsonContent.Create(new GrantEntitlementRequest(otherTenantId, "staffarr"));
        var response = await _client.SendAsync(grantRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Service_token_issue_and_validate_succeeds()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "staffarr-worker",
            "StaffArr Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", token);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "read",
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;

        var validateRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        validateRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(issued.AccessToken));
        var validateResponse = await _client.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var validation = await validateResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>();

        Assert.NotNull(validation);
        Assert.True(validation.IsValid);
        Assert.Equal(issued.TokenId, validation.TokenId);
        Assert.Equal(PlatformSeeder.DemoTenantId, validation.TenantId);
    }

    [Fact]
    public async Task Revoked_service_token_fails_validation()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "trainarr-worker",
            "TrainArr Worker",
            "trainarr",
            ["trainarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", token);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            null,
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;

        var revokeRequest = Authorized(HttpMethod.Post, $"/api/service-tokens/{issued.TokenId}/revoke", token);
        var revokeResponse = await _client.SendAsync(revokeRequest);
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var validateRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        validateRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(issued.AccessToken));
        var validateResponse = await _client.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var validation = await validateResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>();

        Assert.NotNull(validation);
        Assert.False(validation.IsValid);
        Assert.Equal("token_revoked", validation.ReasonCode);
    }

    [Fact]
    public async Task Tenant_admin_cannot_register_service_client()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "blocked-worker",
            "Blocked Worker",
            "staffarr",
            ["staffarr"]));
        var response = await _client.SendAsync(registerRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
