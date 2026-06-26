using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MaintainArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using STLCompliance.Shared.Integration;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrReferenceIntegrationAuthTests : IAsyncLifetime
{
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _maintainarrClient = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"MaintainArrReferenceTypes-{Guid.NewGuid():N}";

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<global::MaintainArr.Api.Data.MaintainArrDbContext>(services);
                services.AddDbContext<global::MaintainArr.Api.Data.MaintainArrDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Reference_types_catalog_allows_asset_reader()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", token));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ReferenceTypeDescriptor[]>();
        Assert.NotNull(payload);
        Assert.Contains(payload!, item => item.ReferenceType == "asset");
    }

    [Fact]
    public async Task Reference_types_catalog_rejects_unrelated_launched_role()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "routarr_driver");

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reference_types_catalog_rejects_platform_admin_without_maintainarr_role()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "routarr_driver", isPlatformAdmin: true);

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/reference-types", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        bool isPlatformAdmin = false)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin);
        return accessToken;
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
