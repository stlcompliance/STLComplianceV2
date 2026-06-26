using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LoadArr.Api.Data;
using LoadArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace STLCompliance.LoadArr.Auth.Tests;

public sealed class LoadArrIntegrationAuthTests : IAsyncLifetime
{
    private WebApplicationFactory<global::LoadArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"LoadArrIntegrationAuth-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::LoadArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<LoadArrDbContext>(services);
                services.AddDbContext<LoadArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Integration_items_allows_warehouse_manager_after_non_loadarr_launch_context()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "warehouse_manager");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/items", token));

        response.EnsureSuccessStatusCode();
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);
        Assert.Equal(0, payload!.RootElement.GetProperty("total").GetInt32());
        Assert.Equal(0, payload.RootElement.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task Integration_items_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "tenant_member");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/items", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Integration_item_create_denies_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "warehouse_supervisor");
        using var request = Authorized(HttpMethod.Post, "/api/v1/integrations/items", token);
        request.Content = JsonContent.Create(new
        {
            supplyarrItemId = "item-1",
            itemCode = "SKU-1",
            itemNameSnapshot = "Widget",
            unitOfMeasureSnapshot = "each"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Integration_items_reject_platform_admin_without_loadarr_role()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "tenant_member", isPlatformAdmin: true);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/items", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string CreateLoadArrAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey, bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<LoadArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            Guid.Parse("22222222-2222-2222-2222-222222222201"),
            Guid.Parse("33333333-3333-3333-3333-333333333301"),
            "warehouse.user@demo.stl",
            "Warehouse User",
            Guid.Parse("11111111-1111-1111-1111-111111111101"),
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin);
        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
