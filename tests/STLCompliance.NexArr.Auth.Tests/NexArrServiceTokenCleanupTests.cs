using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrServiceTokenCleanupTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;
    private string _workerToken = null!;

    public NexArrServiceTokenCleanupTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrServiceTokenCleanupTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Settings_requires_platform_admin()
    {
        await SeedDatabaseAsync();
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/service-token-cleanup/settings", tenantAdminToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/internal/service-token-cleanup/process-batch",
            new ProcessServiceTokenCleanupRequest(DateTimeOffset.UtcNow, 50));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_returns_expired_and_revoked_tokens_when_enabled()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            ServiceTokenCleanupWorkerService.ProcessCleanupActionScope);

        await EnableCleanupAsync(adminToken);
        var expiredTokenId = await SeedExpiredTokenAsync(daysAgo: 10);
        var revokedTokenId = await SeedRevokedTokenAsync(daysAgo: 40);

        var listRequest = Authorized(
            HttpMethod.Get,
            "/api/internal/service-token-cleanup/pending?batchSize=10",
            _workerToken);
        var listResponse = await _client.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingServiceTokenCleanupResponse>())!;

        Assert.Contains(pending.Items, x => x.TokenId == expiredTokenId && x.CleanupReason == "expired");
        Assert.Contains(pending.Items, x => x.TokenId == revokedTokenId && x.CleanupReason == "revoked");
    }

    [Fact]
    public async Task Process_batch_purges_eligible_tokens_and_records_run()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            ServiceTokenCleanupWorkerService.ProcessCleanupActionScope);

        await EnableCleanupAsync(adminToken);
        var expiredTokenId = await SeedExpiredTokenAsync(daysAgo: 10);
        var revokedTokenId = await SeedRevokedTokenAsync(daysAgo: 40);
        var activeTokenId = await SeedActiveTokenAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/service-token-cleanup/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessServiceTokenCleanupRequest(null, 50));
        var processResponse = await _client.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessServiceTokenCleanupResponse>())!;

        Assert.Equal(2, batch.PurgedCount);
        Assert.Contains(expiredTokenId, batch.PurgedTokenIds);
        Assert.Contains(revokedTokenId, batch.PurgedTokenIds);
        Assert.Equal(1, batch.ExpiredPurgeCount);
        Assert.Equal(1, batch.RevokedPurgeCount);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            Assert.False(await db.ServiceTokens.AnyAsync(x => x.Id == expiredTokenId));
            Assert.False(await db.ServiceTokens.AnyAsync(x => x.Id == revokedTokenId));
            Assert.True(await db.ServiceTokens.AnyAsync(x => x.Id == activeTokenId));
            Assert.True(await db.ServiceTokenCleanupRuns.AnyAsync());
        }

        var runsRequest = Authorized(
            HttpMethod.Get,
            "/api/platform-admin/service-token-cleanup/runs?limit=5",
            adminToken);
        var runsResponse = await _client.SendAsync(runsRequest);
        runsResponse.EnsureSuccessStatusCode();
        var runs = (await runsResponse.Content.ReadFromJsonAsync<ServiceTokenCleanupRunsResponse>())!;
        Assert.NotEmpty(runs.Items);
        Assert.True(runs.Items[0].PurgedCount >= 2);
    }

    private async Task EnableCleanupAsync(string adminToken)
    {
        var request = Authorized(
            HttpMethod.Put,
            "/api/platform-admin/service-token-cleanup/settings",
            adminToken);
        request.Content = JsonContent.Create(new UpsertServiceTokenCleanupSettingsRequest(
            true,
            7,
            30));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<ServiceClient> GetOrCreateServiceClientAsync(NexArrDbContext db)
    {
        var existing = await db.ServiceClients.FirstOrDefaultAsync();
        if (existing is not null)
        {
            return existing;
        }

        var now = DateTimeOffset.UtcNow;
        var client = new ServiceClient
        {
            Id = Guid.NewGuid(),
            ClientKey = $"cleanup-test-{Guid.NewGuid():N}",
            DisplayName = "Cleanup test client",
            SourceProductKey = "staffarr",
            AllowedProductKeys = "staffarr",
            IsActive = true,
            CreatedAt = now,
            ModifiedAt = now,
        };
        db.ServiceClients.Add(client);
        await db.SaveChangesAsync();
        return client;
    }

    private async Task<Guid> SeedExpiredTokenAsync(int daysAgo)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var client = await GetOrCreateServiceClientAsync(db);
        var now = DateTimeOffset.UtcNow;
        var tokenId = Guid.NewGuid();
        db.ServiceTokens.Add(new ServiceTokenRecord
        {
            Id = tokenId,
            ServiceClientId = client.Id,
            Jti = tokenId.ToString(),
            TokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"expired-{tokenId:N}"))),
            TenantId = PlatformSeeder.DemoTenantId,
            AllowedProductKeys = "staffarr",
            ExpiresAt = now.AddDays(-daysAgo),
            IssuedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now.AddDays(-daysAgo - 1),
        });
        await db.SaveChangesAsync();
        return tokenId;
    }

    private async Task<Guid> SeedRevokedTokenAsync(int daysAgo)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var client = await GetOrCreateServiceClientAsync(db);
        var now = DateTimeOffset.UtcNow;
        var tokenId = Guid.NewGuid();
        db.ServiceTokens.Add(new ServiceTokenRecord
        {
            Id = tokenId,
            ServiceClientId = client.Id,
            Jti = tokenId.ToString(),
            TokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"revoked-{tokenId:N}"))),
            TenantId = PlatformSeeder.DemoTenantId,
            AllowedProductKeys = "staffarr",
            ExpiresAt = now.AddDays(30),
            RevokedAt = now.AddDays(-daysAgo),
            IssuedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now.AddDays(-daysAgo - 5),
        });
        await db.SaveChangesAsync();
        return tokenId;
    }

    private async Task<Guid> SeedActiveTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var client = await GetOrCreateServiceClientAsync(db);
        var now = DateTimeOffset.UtcNow;
        var tokenId = Guid.NewGuid();
        db.ServiceTokens.Add(new ServiceTokenRecord
        {
            Id = tokenId,
            ServiceClientId = client.Id,
            Jti = tokenId.ToString(),
            TokenHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"active-{tokenId:N}"))),
            TenantId = PlatformSeeder.DemoTenantId,
            AllowedProductKeys = "staffarr",
            ExpiresAt = now.AddDays(30),
            IssuedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
        });
        await db.SaveChangesAsync();
        return tokenId;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"shared-worker-cleanup-{Guid.NewGuid():N}",
            "shared-worker cleanup test",
            "shared-worker",
            ["nexarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            null,
            ["nexarr"],
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
