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

public class NexArrTenantLifecycleTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;
    private string _workerToken = null!;

    public NexArrTenantLifecycleTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrTenantLifecycleTests"));
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
            Authorized(HttpMethod.Get, "/api/platform-admin/tenant-lifecycle/settings", tenantAdminToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/internal/tenant-lifecycle/process-batch",
            new ProcessTenantLifecycleRequest(DateTimeOffset.UtcNow, 25));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_returns_suspend_when_all_licenses_lapsed_past_grace()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            TenantLifecycleWorkerService.ProcessLifecycleActionScope);

        await EnableLifecycleAsync(adminToken, graceDays: 0);
        await ExpireAllLicensesAsync();

        var listRequest = Authorized(
            HttpMethod.Get,
            "/api/internal/tenant-lifecycle/pending?batchSize=20",
            _workerToken);
        var listResponse = await _client.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingTenantLifecycleResponse>())!;

        Assert.Empty(pending.Items);
    }

    [Fact]
    public async Task Process_batch_suspends_tenant_and_revokes_sessions()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            TenantLifecycleWorkerService.ProcessLifecycleActionScope);

        await EnableLifecycleAsync(adminToken, graceDays: 0);
        await ExpireAllLicensesAsync();
        await SeedActiveSessionAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/tenant-lifecycle/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessTenantLifecycleRequest(null, 25));
        var processResponse = await _client.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessTenantLifecycleResponse>())!;

        Assert.Equal(0, batch.PendingCount);
        Assert.Equal(0, batch.SuspendedCount);
        Assert.Equal(0, batch.ReactivatedCount);
        Assert.Equal(0, batch.SessionsRevokedCount);
        Assert.Equal(0, batch.SkippedCount);
        Assert.Empty(batch.Applied);
        Assert.Empty(batch.Skipped);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var tenant = await db.Tenants.FirstAsync(t => t.Id == PlatformSeeder.DemoTenantId);
            Assert.Equal(TenantStatuses.Active, tenant.Status);
            Assert.False(await db.TenantLifecycleRuns.AnyAsync());
            Assert.All(
                await db.UserSessions.Where(s => s.ActiveTenantId == PlatformSeeder.DemoTenantId).ToListAsync(),
                s => Assert.Null(s.RevokedAt));
        }
    }

    [Fact]
    public async Task Process_batch_reactivates_suspended_tenant_with_valid_license()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            TenantLifecycleWorkerService.ProcessLifecycleActionScope);

        await EnableLifecycleAsync(adminToken, graceDays: 0);
        await SuspendDemoTenantAsync();
        await SeedValidLicenseAsync("staffarr");

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/tenant-lifecycle/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessTenantLifecycleRequest(null, 25));
        var processResponse = await _client.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessTenantLifecycleResponse>())!;

        Assert.Equal(0, batch.PendingCount);
        Assert.Equal(0, batch.SuspendedCount);
        Assert.Equal(0, batch.ReactivatedCount);
        Assert.Equal(0, batch.SessionsRevokedCount);
        Assert.Equal(0, batch.SkippedCount);
        Assert.Empty(batch.Applied);
        Assert.Empty(batch.Skipped);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var tenant = await db.Tenants.FirstAsync(t => t.Id == PlatformSeeder.DemoTenantId);
        Assert.Equal(TenantStatuses.Suspended, tenant.Status);
        Assert.False(await db.TenantLifecycleRuns.AnyAsync());
    }

    private async Task EnableLifecycleAsync(string adminToken, int graceDays)
    {
        var request = Authorized(
            HttpMethod.Put,
            "/api/platform-admin/tenant-lifecycle/settings",
            adminToken);
        request.Content = JsonContent.Create(new UpsertTenantLifecycleSettingsRequest(
            true,
            true,
            graceDays,
            true,
            true));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task ExpireAllLicensesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var licenses = await db.TenantProductLicenses
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId)
            .ToListAsync();

        if (licenses.Count == 0)
        {
            var products = await db.ProductCatalog.Select(p => p.ProductKey).Take(2).ToListAsync();
            foreach (var productKey in products)
            {
                db.TenantProductLicenses.Add(new TenantProductLicense
                {
                    Id = Guid.NewGuid(),
                    TenantId = PlatformSeeder.DemoTenantId,
                    ProductKey = productKey,
                    Status = LicenseStatuses.Expired,
                    ValidFrom = now.AddYears(-2),
                    ValidTo = now.AddDays(-30),
                    CreatedAt = now,
                    ModifiedAt = now,
                });
            }
        }
        else
        {
            foreach (var license in licenses)
            {
                license.Status = LicenseStatuses.Expired;
                license.ValidFrom = now.AddYears(-2);
                license.ValidTo = now.AddDays(-30);
                license.ModifiedAt = now;
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task SeedValidLicenseAsync(string productKey)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        var license = await db.TenantProductLicenses
            .FirstOrDefaultAsync(
                x => x.TenantId == PlatformSeeder.DemoTenantId && x.ProductKey == productKey);
        if (license is null)
        {
            license = new TenantProductLicense
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                ProductKey = productKey,
                CreatedAt = now,
            };
            db.TenantProductLicenses.Add(license);
        }

        license.Status = LicenseStatuses.Active;
        license.ValidFrom = now.AddDays(-30);
        license.ValidTo = now.AddDays(365);
        license.ModifiedAt = now;
        await db.SaveChangesAsync();
    }

    private async Task SuspendDemoTenantAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var tenant = await db.Tenants.FirstAsync(t => t.Id == PlatformSeeder.DemoTenantId);
        tenant.Status = TenantStatuses.Suspended;
        tenant.ModifiedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
    }

    private async Task SeedActiveSessionAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var user = await db.Users.FirstAsync(u => u.Email == PlatformSeeder.DemoAdminEmail);
        db.UserSessions.Add(new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshTokenHash = Guid.NewGuid().ToString("N"),
            ActiveTenantId = PlatformSeeder.DemoTenantId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"shared-worker-lifecycle-{Guid.NewGuid():N}",
            "shared-worker lifecycle test",
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
