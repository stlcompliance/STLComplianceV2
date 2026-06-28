using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CustomArr.Api.Data;
using CustomArr.Api.Services;
using STLCompliance.Shared.Integration;
using CustomArrHandoffSessionResponse = CustomArr.Api.Data.CustomArrHandoffSessionResponse;

namespace STLCompliance.CustomArr.Api.Tests;

public sealed class CustomArrAuthEndpointsTests : IAsyncLifetime
{
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DemoUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DemoPersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private WebApplicationFactory<global::CustomArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"CustomArrAuth-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::CustomArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("Handoff:ServiceToken", "customarr-handoff-service-token");
            builder.UseSetting("NexArr:BaseUrl", "http://localhost");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<CustomArrDbContext>(services);
                services.AddDbContext<CustomArrDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => new FakeNexArrHandoffHandler());
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
    public async Task Session_bootstrap_allows_users_after_non_customarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/session", token));
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.Equal("customarr", root.GetProperty("productKey").GetString());
        Assert.False(root.TryGetProperty("hasCustomArrAccess", out _));
        Assert.Contains(
            root.GetProperty("launchableProductKeys").EnumerateArray().Select(item => item.GetString()),
            value => string.Equals(value, "customarr", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            root.GetProperty("launchableProductKeys").EnumerateArray().Select(item => item.GetString()),
            value => string.Equals(value, "compliancecore", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handoff_redeem_allows_non_customarr_launch_context_when_target_is_customarr()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new StlNexArrRedeemHandoffRequest("customarr-not-available", null));
        response.EnsureSuccessStatusCode();

        var session = (await response.Content.ReadFromJsonAsync<CustomArrHandoffSessionResponse>())!;
        Assert.Equal(DemoUserId.ToString(), session.UserId);
        Assert.Equal("customarr_manager", session.TenantRoleKey);
        Assert.Contains("customarr", session.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", session.LaunchableProductKeys);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
    }

    [Fact]
    public async Task Customer_quick_create_rejects_platform_admin_without_customarr_role()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);
        using var request = Authorized(HttpMethod.Post, "/api/v1/integrations/references/customer/quick-create", token);
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
        request.Content = JsonContent.Create(new
        {
            referenceType = "customer",
            values = new Dictionary<string, string>
            {
                ["legalName"] = "Platform Admin Prospect",
                ["displayName"] = "Platform Admin Prospect"
            }
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Customer_quick_create_allows_customarr_manager_after_non_customarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "customarr_manager");
        using var request = Authorized(HttpMethod.Post, "/api/v1/integrations/references/customer/quick-create", token);
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
        request.Content = JsonContent.Create(new
        {
            referenceType = "customer",
            values = new Dictionary<string, string>
            {
                ["legalName"] = "Contoso Field Services",
                ["displayName"] = "Contoso"
            }
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    private string CreateAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey = "tenant_member", bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<CustomArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            DemoUserId,
            DemoPersonId,
            "customarr.user@demo.stl",
            "CustomArr User",
            DemoTenantId,
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

    private sealed class FakeNexArrHandoffHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/launch/handoff/redeem", request.RequestUri?.AbsolutePath);

            var payload = await request.Content!.ReadFromJsonAsync<StlNexArrRedeemHandoffRequest>(cancellationToken);
            Assert.NotNull(payload);
            Assert.Equal("customarr-handoff-service-token", payload!.ServiceToken);

            return payload.HandoffCode switch
            {
                "customarr-not-available" => CreateResponse("customarr", ["nexarr"]),
                _ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
                {
                    Content = JsonContent.Create(new { error = "unknown handoff code" })
                }
            };
        }

        private static HttpResponseMessage CreateResponse(string targetProductKey, IReadOnlyList<string> launchableProductKeys)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new StlNexArrHandoffRedeemedResponse(
                    DemoUserId,
                    "customarr.user@demo.stl",
                    "CustomArr User",
                    DemoTenantId,
                    "demo-stl",
                    "STL Demo Tenant",
                    targetProductKey,
                    Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    "customarr_manager",
                    false,
                    launchableProductKeys,
                    "dark",
                    45,
                    "http://localhost:5186/app/customarr"))
            };
        }
    }
}
