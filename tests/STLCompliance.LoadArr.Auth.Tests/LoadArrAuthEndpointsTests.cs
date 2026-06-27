using System.Net;
using System.Net.Http.Json;
using LoadArr.Api.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.Shared.Integration;
using LoadArrHandoffSessionResponse = LoadArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.LoadArr.Auth.Tests;

public sealed class LoadArrAuthEndpointsTests : IClassFixture<WebApplicationFactory<global::LoadArr.Api.Program>>
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";
    private const string ServiceToken = "loadarr-handoff-service-token";
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid SessionId = Guid.Parse("11111111-1111-1111-1111-111111111113");

    private readonly HttpClient _client;

    public LoadArrAuthEndpointsTests(WebApplicationFactory<global::LoadArr.Api.Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("ConnectionStrings:Database", string.Empty);
                builder.UseSetting("DATABASE_URL", string.Empty);
                builder.UseSetting("Auth:SigningKey", SigningKey);
                builder.UseSetting("Handoff:ServiceToken", ServiceToken);
                builder.UseSetting("NexArr:BaseUrl", "http://localhost");
                builder.ConfigureServices(services =>
                {
                    services.AddHttpClient<StlNexArrHandoffClient>()
                        .ConfigurePrimaryHttpMessageHandler(() => new FakeNexArrHandoffHandler());
                });
            })
            .CreateClient();
    }

    [Theory]
    [InlineData("/api/auth/handoff/redeem")]
    [InlineData("/api/auth/nexarr/redeem")]
    [InlineData("/api/v1/auth/handoff/redeem")]
    [InlineData("/api/v1/auth/nexarr/redeem")]
    public async Task Handoff_redeem_routes_return_session_for_launchable_loadarr_user(string path)
    {
        var response = await _client.PostAsJsonAsync(path, new RedeemHandoffRequest("  loadarr-ok  "));
        response.EnsureSuccessStatusCode();

        var session = (await response.Content.ReadFromJsonAsync<LoadArrHandoffSessionResponse>())!;
        Assert.Equal(UserId.ToString(), session.UserId);
        Assert.Equal(UserId.ToString(), session.PersonId);
        Assert.Equal("admin@example.test", session.Email);
        Assert.Equal("Demo Admin", session.DisplayName);
        Assert.Equal(TenantId.ToString(), session.TenantId);
        Assert.Equal("demo-stl", session.TenantSlug);
        Assert.Equal("STL Demo Tenant", session.TenantDisplayName);
        Assert.Equal(SessionId.ToString(), session.SessionId);
        Assert.Equal("warehouse_manager", session.TenantRoleKey);
        Assert.True(session.IsPlatformAdmin);
        Assert.Equal(new[] { "loadarr", "nexarr" }, session.LaunchableProductKeys);
        Assert.Equal("dark", session.ThemePreference);
        Assert.Equal("http://localhost:5182/app/loadarr", session.CallbackUrl);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
    }

    [Fact]
    public async Task Handoff_redeem_rejects_missing_code()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/handoff/redeem", new RedeemHandoffRequest(" "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("handoff.code_missing", body);
    }

    [Fact]
    public async Task Handoff_redeem_rejects_non_loadarr_target_product()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new RedeemHandoffRequest("loadarr-wrong-product"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("handoff.product_mismatch", body);
    }

    [Fact]
    public async Task Handoff_redeem_rejects_when_loadarr_is_not_launchable()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new RedeemHandoffRequest("loadarr-not-available"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("handoff.not_available", body);
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
            Assert.Equal(ServiceToken, payload!.ServiceToken);

            return payload.HandoffCode switch
            {
                "loadarr-ok" => CreateResponse("loadarr", ["loadarr", "nexarr"]),
                "loadarr-wrong-product" => CreateResponse("trainarr", ["loadarr"]),
                "loadarr-not-available" => CreateResponse("loadarr", ["nexarr"]),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = JsonContent.Create(new { error = "unknown handoff code" })
                }
            };
        }

        private static HttpResponseMessage CreateResponse(string targetProductKey, IReadOnlyList<string> launchableProductKeys)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new StlNexArrHandoffRedeemedResponse(
                    UserId,
                    "admin@example.test",
                    "Demo Admin",
                    TenantId,
                    "demo-stl",
                    "STL Demo Tenant",
                    targetProductKey,
                    SessionId,
                    "warehouse_manager",
                    true,
                    launchableProductKeys,
                    "dark",
                    45,
                    "http://localhost:5182/app/loadarr"))
            };
        }
    }
}
