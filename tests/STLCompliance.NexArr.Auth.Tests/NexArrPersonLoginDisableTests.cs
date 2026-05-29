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

public class NexArrPersonLoginDisableTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;
    private string _staffarrToken = null!;

    public NexArrPersonLoginDisableTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrPersonLoginDisableTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Person_login_disable_rejects_missing_service_token()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/internal/person-login-disable",
            new PersonLoginDisableRequest(
                PlatformSeeder.DemoTenantId,
                Guid.NewGuid(),
                PlatformSeeder.DemoTenantAdminUserId,
                "Offboarding"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Person_login_disable_rejects_wrong_source_product()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var workerToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["nexarr"],
            PersonLoginDisableService.DisableLoginActionScope);

        var request = Authorized(
            HttpMethod.Post,
            "/api/internal/person-login-disable",
            workerToken);
        request.Content = JsonContent.Create(new PersonLoginDisableRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            PlatformSeeder.DemoTenantAdminUserId,
            "Offboarding"));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Person_login_disable_disables_user_and_revokes_sessions()
    {
        await SeedDatabaseAsync();
        await SeedActiveSessionsAsync(PlatformSeeder.DemoTenantAdminUserId);
        _staffarrToken = await IssueStaffarrLoginDisableTokenAsync();

        var staffarrPersonId = Guid.NewGuid();
        var disableRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/person-login-disable",
            _staffarrToken);
        disableRequest.Content = JsonContent.Create(new PersonLoginDisableRequest(
            PlatformSeeder.DemoTenantId,
            staffarrPersonId,
            PlatformSeeder.DemoTenantAdminUserId,
            "Workforce offboarding"));
        var disableResponse = await _client.SendAsync(disableRequest);
        disableResponse.EnsureSuccessStatusCode();
        var result = (await disableResponse.Content.ReadFromJsonAsync<PersonLoginDisableResponse>())!;

        Assert.Equal(PlatformSeeder.DemoTenantAdminUserId, result.ExternalUserId);
        Assert.False(result.WasAlreadyDisabled);
        Assert.True(result.SessionsRevokedCount >= 1);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == PlatformSeeder.DemoTenantAdminUserId);
            Assert.False(user.IsActive);

            var sessions = await db.UserSessions
                .Where(s => s.UserId == PlatformSeeder.DemoTenantAdminUserId)
                .ToListAsync();
            Assert.NotEmpty(sessions);
            Assert.All(sessions, s => Assert.NotNull(s.RevokedAt));

            var outboxEvent = await db.PlatformOutboxEvents
                .FirstOrDefaultAsync(x => x.EventType == PlatformOutboxEventKinds.UserDisabled);
            Assert.NotNull(outboxEvent);
            Assert.Equal(PlatformSeeder.DemoTenantId, outboxEvent.TenantId);
        }

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Person_login_disable_is_idempotent_for_already_disabled_user()
    {
        await SeedDatabaseAsync();
        _staffarrToken = await IssueStaffarrLoginDisableTokenAsync();

        var first = await DisableLoginAsync(PlatformSeeder.DemoTenantAdminUserId);
        Assert.False(first.WasAlreadyDisabled);

        var second = await DisableLoginAsync(PlatformSeeder.DemoTenantAdminUserId);
        Assert.True(second.WasAlreadyDisabled);
        Assert.Equal(0, second.SessionsRevokedCount);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var disabledEvents = await db.PlatformOutboxEvents
            .CountAsync(x => x.EventType == PlatformOutboxEventKinds.UserDisabled);
        Assert.Equal(1, disabledEvents);
    }

    private async Task<PersonLoginDisableResponse> DisableLoginAsync(Guid externalUserId)
    {
        var request = Authorized(
            HttpMethod.Post,
            "/api/internal/person-login-disable",
            _staffarrToken);
        request.Content = JsonContent.Create(new PersonLoginDisableRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            externalUserId,
            "Offboarding"));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PersonLoginDisableResponse>())!;
    }

    private async Task<string> IssueStaffarrLoginDisableTokenAsync()
    {
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        return await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            ["nexarr"],
            PersonLoginDisableService.DisableLoginActionScope,
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

    private async Task SeedActiveSessionsAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.UserSessions.Add(new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenHash = Guid.NewGuid().ToString("N"),
            ActiveTenantId = PlatformSeeder.DemoTenantId,
            ExpiresAt = now.AddDays(7),
            CreatedAt = now,
        });
        await db.SaveChangesAsync();
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
