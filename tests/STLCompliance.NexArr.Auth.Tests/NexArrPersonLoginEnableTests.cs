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

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrPersonLoginEnableTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;
    private string _staffarrToken = null!;

    public NexArrPersonLoginEnableTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrPersonLoginEnableTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Person_login_enable_enables_user_and_enqueues_outbox()
    {
        await SeedDatabaseAsync();
        await DisableTenantAdminAsync();
        _staffarrToken = await IssueStaffarrLoginEnableTokenAsync();

        var enableRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/person-login-enable",
            _staffarrToken);
        enableRequest.Content = JsonContent.Create(new PersonLoginEnableRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            PlatformSeeder.DemoTenantAdminUserId,
            "Rehire"));
        var enableResponse = await _client.SendAsync(enableRequest);
        enableResponse.EnsureSuccessStatusCode();
        var result = (await enableResponse.Content.ReadFromJsonAsync<PersonLoginEnableResponse>())!;
        Assert.False(result.WasAlreadyEnabled);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == PlatformSeeder.DemoTenantAdminUserId);
            Assert.True(user.IsActive);

            var outboxEvent = await db.PlatformOutboxEvents
                .FirstOrDefaultAsync(x => x.EventType == PlatformOutboxEventKinds.UserEnabled);
            Assert.NotNull(outboxEvent);
            Assert.Equal(PlatformSeeder.DemoTenantId, outboxEvent.TenantId);
        }

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        loginResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Platform_admin_enable_user_enqueues_outbox_without_tenant()
    {
        await SeedDatabaseAsync();
        await DisableTenantAdminAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var enableRequest = Authorized(
            HttpMethod.Post,
            $"/api/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/enable",
            adminToken);
        var enableResponse = await _client.SendAsync(enableRequest);
        enableResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var outboxEvent = await db.PlatformOutboxEvents
            .FirstOrDefaultAsync(x => x.EventType == PlatformOutboxEventKinds.UserEnabled);
        Assert.NotNull(outboxEvent);
        Assert.Null(outboxEvent.TenantId);
    }

    [Fact]
    public async Task Person_login_enable_is_idempotent_for_outbox()
    {
        await SeedDatabaseAsync();
        await DisableTenantAdminAsync();
        _staffarrToken = await IssueStaffarrLoginEnableTokenAsync();

        var first = await EnableLoginAsync();
        Assert.False(first.WasAlreadyEnabled);
        var second = await EnableLoginAsync();
        Assert.True(second.WasAlreadyEnabled);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var enabledEvents = await db.PlatformOutboxEvents
            .CountAsync(x => x.EventType == PlatformOutboxEventKinds.UserEnabled);
        Assert.Equal(1, enabledEvents);
    }

    private async Task<PersonLoginEnableResponse> EnableLoginAsync()
    {
        var request = Authorized(
            HttpMethod.Post,
            "/api/internal/person-login-enable",
            _staffarrToken);
        request.Content = JsonContent.Create(new PersonLoginEnableRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            PlatformSeeder.DemoTenantAdminUserId,
            null));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PersonLoginEnableResponse>())!;
    }

    private async Task DisableTenantAdminAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var user = await db.Users.FirstAsync(u => u.Id == PlatformSeeder.DemoTenantAdminUserId);
        user.IsActive = false;
        await db.SaveChangesAsync();
    }

    private async Task<string> IssueStaffarrLoginEnableTokenAsync()
    {
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        return await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            ["nexarr"],
            PersonLoginEnableService.EnableLoginActionScope,
            PlatformSeeder.DemoTenantId);
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope,
        Guid? tenantId = null)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"integration-{sourceProduct}-{Guid.NewGuid():N}",
            $"{sourceProduct} integration test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            tenantId,
            allowedProducts,
            actionScope,
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
