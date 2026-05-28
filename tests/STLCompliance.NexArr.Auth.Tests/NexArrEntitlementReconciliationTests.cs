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

public class NexArrEntitlementReconciliationTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;
    private string _workerToken = null!;

    public NexArrEntitlementReconciliationTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrEntitlementReconciliationTests"));
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
            Authorized(HttpMethod.Get, "/api/platform-admin/entitlement-reconciliation/settings", tenantAdminToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/internal/entitlement-reconciliation/process-batch",
            new ProcessEntitlementReconciliationRequest(DateTimeOffset.UtcNow, 50));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_returns_stale_entitlement_drift_when_enabled()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            EntitlementReconciliationWorkerService.ProcessReconciliationActionScope);

        await EnableReconciliationAsync(adminToken);
        await SeedExpiredLicenseAsync("staffarr");

        var listRequest = Authorized(
            HttpMethod.Get,
            "/api/internal/entitlement-reconciliation/pending?batchSize=20",
            _workerToken);
        var listResponse = await _client.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingEntitlementReconciliationResponse>())!;

        Assert.Contains(
            pending.Items,
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.ProductKey == "staffarr"
                && x.DriftKind == "stale_entitlement");
    }

    [Fact]
    public async Task Process_batch_revokes_stale_entitlement_and_records_run()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            EntitlementReconciliationWorkerService.ProcessReconciliationActionScope);

        await EnableReconciliationAsync(adminToken);
        await SeedExpiredLicenseAsync("staffarr");

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/entitlement-reconciliation/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessEntitlementReconciliationRequest(null, 50));
        var processResponse = await _client.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessEntitlementReconciliationResponse>())!;

        Assert.True(batch.RevokedCount >= 1);
        Assert.Contains(
            batch.Applied,
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.ProductKey == "staffarr"
                && x.DriftKind == "stale_entitlement");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var entitlement = await db.Entitlements.FirstAsync(
                e => e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "staffarr");
            Assert.Equal(EntitlementStatuses.Revoked, entitlement.Status);
            Assert.True(await db.EntitlementReconciliationRuns.AnyAsync());
        }

        var runsRequest = Authorized(
            HttpMethod.Get,
            "/api/platform-admin/entitlement-reconciliation/runs?limit=5",
            adminToken);
        var runsResponse = await _client.SendAsync(runsRequest);
        runsResponse.EnsureSuccessStatusCode();
        var runs = (await runsResponse.Content.ReadFromJsonAsync<EntitlementReconciliationRunsResponse>())!;
        Assert.NotEmpty(runs.Items);
        Assert.True(runs.Items[0].RevokedCount >= 1);
    }

    [Fact]
    public async Task Process_batch_grants_missing_entitlement_from_valid_license()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            EntitlementReconciliationWorkerService.ProcessReconciliationActionScope);

        await EnableReconciliationAsync(adminToken);
        await SeedValidLicenseWithoutEntitlementAsync("supplyarr");

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/entitlement-reconciliation/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessEntitlementReconciliationRequest(null, 50));
        var processResponse = await _client.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessEntitlementReconciliationResponse>())!;

        Assert.True(batch.GrantedCount >= 1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var entitlement = await db.Entitlements.FirstAsync(
            e => e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "supplyarr");
        Assert.Equal(EntitlementStatuses.Active, entitlement.Status);
    }

    private async Task EnableReconciliationAsync(string adminToken)
    {
        var request = Authorized(
            HttpMethod.Put,
            "/api/platform-admin/entitlement-reconciliation/settings",
            adminToken);
        request.Content = JsonContent.Create(new UpsertEntitlementReconciliationSettingsRequest(
            true,
            true,
            true));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task SeedExpiredLicenseAsync(string productKey)
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

        license.Status = LicenseStatuses.Expired;
        license.ValidFrom = now.AddYears(-2);
        license.ValidTo = now.AddDays(-1);
        license.ModifiedAt = now;
        await db.SaveChangesAsync();
    }

    private async Task SeedValidLicenseWithoutEntitlementAsync(string productKey)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        var entitlement = await db.Entitlements
            .FirstOrDefaultAsync(
                e => e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == productKey);
        if (entitlement is not null)
        {
            db.Entitlements.Remove(entitlement);
        }

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

    private async Task<string> IssueServiceTokenAsync(string adminToken, string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"shared-worker-reconcile-{Guid.NewGuid():N}",
            "shared-worker reconcile test",
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
