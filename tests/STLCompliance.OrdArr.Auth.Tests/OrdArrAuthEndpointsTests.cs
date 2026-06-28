using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrdArr.Api.Data;
using OrdArr.Api.Services;
using STLCompliance.Shared.Integration;
using OrdArrHandoffSessionResponse = OrdArr.Api.Data.OrdArrHandoffSessionResponse;

namespace STLCompliance.OrdArr.Auth.Tests;

public sealed class OrdArrAuthEndpointsTests : IAsyncLifetime
{
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DemoUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DemoPersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private WebApplicationFactory<global::OrdArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"OrdArrAuth-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::OrdArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("Handoff:ServiceToken", "ordarr-handoff-service-token");
            builder.UseSetting("NexArr:BaseUrl", "http://localhost");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<OrdArrDbContext>(services);
                services.AddDbContext<OrdArrDbContext>(options => options.UseInMemoryDatabase(dbName));
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
    public async Task Session_bootstrap_allows_users_after_non_ordarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/session", token));
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.Equal("ordarr", root.GetProperty("productKey").GetString());
        Assert.False(root.TryGetProperty("hasOrdArrAccess", out _));
        Assert.Contains(
            root.GetProperty("launchableProductKeys").EnumerateArray().Select(item => item.GetString()),
            value => string.Equals(value, "ordarr", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            root.GetProperty("launchableProductKeys").EnumerateArray().Select(item => item.GetString()),
            value => string.Equals(value, "compliancecore", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handoff_redeem_allows_non_ordarr_launch_context_when_target_is_ordarr()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new StlNexArrRedeemHandoffRequest("ordarr-not-available", null));
        response.EnsureSuccessStatusCode();

        var session = (await response.Content.ReadFromJsonAsync<OrdArrHandoffSessionResponse>())!;
        Assert.Equal(DemoUserId.ToString(), session.UserId);
        Assert.Equal("ordarr-ops", session.TenantRoleKey);
        Assert.Contains("ordarr", session.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", session.LaunchableProductKeys);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
    }

    [Fact]
    public async Task Workspace_summary_rejects_plain_tenant_member()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/summary", token));

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_summary_allows_ordarr_ops_after_non_ordarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "ordarr-ops");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/summary", token));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Completion_packet_updates_are_authorized_and_persisted()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "ordarr-ops");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/orders/", token);
        createRequest.Headers.Add("Idempotency-Key", $"auth-test-order-{Guid.NewGuid():N}");
        createRequest.Content = JsonContent.Create(new OrdArrCreateOrderRequest(
            new STLCompliance.Shared.Integration.StlProductObjectReference("customarr", "customer", "cust-auth-packet", "CUST-AUTH-PACKET"),
            "Auth Packet Customer",
            "customer_order",
            "person-ordarr-owner",
            "Verifies completion packet coordination."));

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        using var createDocument = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var orderId = createDocument.RootElement.GetProperty("orderId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(orderId));

        var approveRequest = Authorized(HttpMethod.Post, $"/api/v1/orders/{orderId}/approve", token);
        approveRequest.Headers.Add("Idempotency-Key", $"auth-test-approve-{Guid.NewGuid():N}");
        approveRequest.Content = JsonContent.Create(new OrdArrAcceptOrderRequest(null, null, null, "Approved for packet testing"));

        var approveResponse = await _client.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();

        var packetRequest = Authorized(HttpMethod.Post, $"/api/v1/orders/{orderId}/completion-packets", token);
        packetRequest.Headers.Add("Idempotency-Key", $"auth-test-packet-{Guid.NewGuid():N}");
        packetRequest.Content = JsonContent.Create(new OrdArrCompletionPacketRequest("completion"));

        var packetResponse = await _client.SendAsync(packetRequest);
        packetResponse.EnsureSuccessStatusCode();

        using var packetDocument = JsonDocument.Parse(await packetResponse.Content.ReadAsStringAsync());
        var packetRoot = packetDocument.RootElement;
        Assert.Equal("ready", packetRoot.GetProperty("completionState").GetString());
        Assert.Equal("not_ready", packetRoot.GetProperty("financialPacketState").GetString());
        Assert.Equal(1, packetRoot.GetProperty("completionPackets").GetArrayLength());
        Assert.Equal("ready", packetRoot.GetProperty("completionPackets")[0].GetProperty("status").GetString());
    }

    [Fact]
    public async Task Create_order_rejects_platform_admin_without_ordarr_role()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);
        using var request = Authorized(HttpMethod.Post, "/api/v1/orders/", token);
        request.Headers.Add("Idempotency-Key", $"auth-test-{Guid.NewGuid():N}");
        request.Content = JsonContent.Create(new OrdArrCreateOrderRequest(
            new STLCompliance.Shared.Integration.StlProductObjectReference("customarr", "customer", "cust-auth", "CUST-AUTH"),
            "Auth Test Customer",
            "customer_order",
            "person-ordarr-owner",
            "Verifies OrdArr authorization."));

        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string CreateAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey = "tenant_member", bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<OrdArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            DemoUserId,
            DemoPersonId,
            "ordarr.user@demo.stl",
            "OrdArr User",
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
            Assert.Equal("ordarr-handoff-service-token", payload!.ServiceToken);

            return payload.HandoffCode switch
            {
                "ordarr-not-available" => CreateResponse("ordarr", ["nexarr"]),
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
                    "ordarr.user@demo.stl",
                    "OrdArr User",
                    DemoTenantId,
                    "demo-stl",
                    "STL Demo Tenant",
                    targetProductKey,
                    Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    "ordarr-ops",
                    false,
                    launchableProductKeys,
                    "dark",
                    45,
                    "http://localhost:5187/app/ordarr"))
            };
        }
    }
}
