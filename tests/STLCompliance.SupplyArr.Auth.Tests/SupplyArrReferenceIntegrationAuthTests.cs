using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using STLCompliance.Shared.Integration;
using SupplyArr.Api.Data;
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrReferenceIntegrationAuthTests : IAsyncLifetime
{
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _supplyarrClient = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"SupplyArrReferenceTypes-{Guid.NewGuid():N}";

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Reference_types_catalog_allows_supplyarr_reader()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "tenant_member");

        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", token));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ReferenceTypeDescriptor[]>();
        Assert.NotNull(payload);
        Assert.Contains(payload!, item => item.ReferenceType == "part");
        Assert.Contains(payload!, item => item.ReferenceType == "vendor");
    }

    [Fact]
    public async Task Reference_types_catalog_rejects_unrelated_launched_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "routarr_driver");

        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reference_types_catalog_rejects_platform_admin_without_supplyarr_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "routarr_driver", isPlatformAdmin: true);

        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Party_quick_create_schema_disables_platform_admin_without_supplyarr_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "routarr_driver", isPlatformAdmin: true);

        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/references/vendor/quick-create-schema", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string CreateSupplyArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        bool isPlatformAdmin = false)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<SupplyArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin);
        return token;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>)
                || d.ServiceType == typeof(TContext))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }
}
