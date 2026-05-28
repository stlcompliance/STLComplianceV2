using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using STLCompliance.Shared.Operations.LoadTesting;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreLoadTestJourneySeedTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreJourneySeed-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Load_test_journey_seed_is_idempotent_and_creates_rule_pack_and_dispatch_gates()
    {
        var adminToken = CreateAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var firstResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlLoadTestJourneySeedCatalog.SeedEndpointPath, adminToken));
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<LoadTestJourneySeedResponse>())!;
        Assert.Equal(StlLoadTestJourneySeedCatalog.RulePackKey, first.RulePackKey);
        Assert.True(first.RulePackCreated);
        Assert.True(first.RuleContentEnsured);
        Assert.True(first.DriverLicenseFactEnsured);
        Assert.Contains("dispatch_driver_qualification", first.DispatchGateKeys);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
            var pack = await db.RulePacks.SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.PackKey == StlLoadTestJourneySeedCatalog.RulePackKey);
            Assert.False(string.IsNullOrWhiteSpace(pack.RuleContentJson));
            Assert.Equal(3, await db.WorkflowGateDefinitions.CountAsync(x => x.TenantId == PlatformSeeder.DemoTenantId));
        }

        var secondResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlLoadTestJourneySeedCatalog.SeedEndpointPath, adminToken));
        secondResponse.EnsureSuccessStatusCode();
        var second = (await secondResponse.Content.ReadFromJsonAsync<LoadTestJourneySeedResponse>())!;
        Assert.False(second.RulePackCreated);
        Assert.False(second.RuleContentEnsured);
        Assert.Equal(0, second.DispatchGatesCreated);
    }

    [Fact]
    public async Task Load_test_journey_seed_denied_for_read_only_role()
    {
        var token = CreateAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlLoadTestJourneySeedCatalog.SeedEndpointPath, token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string CreateAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
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
