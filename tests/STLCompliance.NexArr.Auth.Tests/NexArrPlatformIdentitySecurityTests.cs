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

public sealed class NexArrPlatformIdentitySecurityTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrPlatformIdentitySecurityTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrPlatformIdentitySecurityTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Platform_identity_resolve_reports_mfa_state()
    {
        await SeedDatabaseAsync();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var user = await db.Users.Include(x => x.Credential).FirstAsync(x => x.Id == PlatformSeeder.DemoTenantAdminUserId);
            user.Credential!.IsMfaEnabled = true;
            user.Credential.MfaSecret = "TESTSECRET";
            await db.SaveChangesAsync();
        }

        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var readToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            PlatformIdentityIntegrationService.ReadIdentityActionScope,
            PlatformSeeder.DemoTenantId);

        var request = Authorized(
            HttpMethod.Get,
            $"/api/internal/platform-identities/{PlatformSeeder.DemoTenantAdminUserId}?tenantId={PlatformSeeder.DemoTenantId}",
            readToken);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var identity = (await response.Content.ReadFromJsonAsync<PlatformIdentityResponse>())!;
        Assert.True(identity.IsMfaEnabled);
    }

    [Fact]
    public async Task StaffArr_can_request_password_reset_and_reset_mfa()
    {
        await SeedDatabaseAsync();
        await SeedActiveSessionAsync(PlatformSeeder.DemoTenantAdminUserId);
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var passwordResetToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            PlatformIdentitySecurityService.RequestPasswordResetActionScope,
            PlatformSeeder.DemoTenantId);
        var mfaResetToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            PlatformIdentitySecurityService.ResetMfaActionScope,
            PlatformSeeder.DemoTenantId);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var user = await db.Users.Include(x => x.Credential).FirstAsync(x => x.Id == PlatformSeeder.DemoTenantAdminUserId);
            user.Credential!.IsMfaEnabled = true;
            user.Credential.MfaSecret = "TESTSECRET";
            user.Credential.MfaRecoveryCodeHashesJson = "[\"abc\"]";
            await db.SaveChangesAsync();
        }

        var passwordResetRequest = Authorized(
            HttpMethod.Post,
            $"/api/internal/platform-identities/{PlatformSeeder.DemoTenantAdminUserId}/password-reset",
            passwordResetToken);
        passwordResetRequest.Content = JsonContent.Create(new RequestPlatformIdentityPasswordResetRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            PlatformSeeder.DemoTenantAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            "Admin initiated reset"));
        var passwordResetResponse = await _client.SendAsync(passwordResetRequest);
        passwordResetResponse.EnsureSuccessStatusCode();
        var passwordResetPayload =
            (await passwordResetResponse.Content.ReadFromJsonAsync<RequestPlatformIdentityPasswordResetResponse>())!;
        Assert.Equal(PlatformSeeder.DemoTenantAdminUserId, passwordResetPayload.ExternalUserId);

        var mfaResetRequest = Authorized(
            HttpMethod.Post,
            $"/api/internal/platform-identities/{PlatformSeeder.DemoTenantAdminUserId}/mfa-reset",
            mfaResetToken);
        mfaResetRequest.Content = JsonContent.Create(new ResetPlatformIdentityMfaRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            PlatformSeeder.DemoTenantAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            "Admin initiated MFA reset"));
        var mfaResetResponse = await _client.SendAsync(mfaResetRequest);
        mfaResetResponse.EnsureSuccessStatusCode();
        var mfaResetPayload = (await mfaResetResponse.Content.ReadFromJsonAsync<ResetPlatformIdentityMfaResponse>())!;
        Assert.True(mfaResetPayload.WasMfaEnabled);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var resetTokens = await verifyDb.PasswordResetTokens
            .Where(x => x.UserId == PlatformSeeder.DemoTenantAdminUserId)
            .ToListAsync();
        Assert.NotEmpty(resetTokens);

        var sessions = await verifyDb.UserSessions
            .Where(x => x.UserId == PlatformSeeder.DemoTenantAdminUserId)
            .ToListAsync();
        Assert.NotEmpty(sessions);
        Assert.All(sessions, session => Assert.NotNull(session.RevokedAt));
    }

    private async Task SeedActiveSessionAsync(Guid userId)
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string actionScope,
        Guid tenantId)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"identity-security-{sourceProduct}-{Guid.NewGuid():N}",
            $"{sourceProduct} identity security integration test",
            sourceProduct,
            ["nexarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            tenantId,
            ["nexarr"],
            actionScope,
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!.AccessToken;
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
        return (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!.AccessToken;
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
