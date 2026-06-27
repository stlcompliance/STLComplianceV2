using System.Net;
using System.Net.Http.Json;
using AssurArr.Api.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.Shared.Integration;
using AssurArrHandoffSessionResponse = AssurArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.AssurArr.Api.Tests;

public sealed class AssurArrAuthEndpointsTests : IClassFixture<WebApplicationFactory<global::AssurArr.Api.Program>>
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";
    private const string ServiceToken = "assurarr-handoff-service-token";
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid SessionId = Guid.Parse("11111111-1111-1111-1111-111111111113");

    private readonly HttpClient _client;

    public AssurArrAuthEndpointsTests(WebApplicationFactory<global::AssurArr.Api.Program> factory)
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
    public async Task Handoff_redeem_routes_return_session_for_launchable_assurarr_user(string path)
    {
        var response = await _client.PostAsJsonAsync(path, new RedeemHandoffRequest("  assurarr-ok  "));
        response.EnsureSuccessStatusCode();

        var session = (await response.Content.ReadFromJsonAsync<AssurArrHandoffSessionResponse>())!;
        Assert.Equal(UserId.ToString(), session.UserId);
        Assert.Equal(UserId.ToString(), session.PersonId);
        Assert.Equal("admin@example.test", session.Email);
        Assert.Equal("Demo Admin", session.DisplayName);
        Assert.Equal(TenantId.ToString(), session.TenantId);
        Assert.Equal("demo-stl", session.TenantSlug);
        Assert.Equal("STL Demo Tenant", session.TenantDisplayName);
        Assert.Equal(SessionId.ToString(), session.SessionId);
        Assert.Equal("quality_manager", session.TenantRoleKey);
        Assert.True(session.IsPlatformAdmin);
        Assert.Equal(new[] { "assurarr", "staffarr" }, session.LaunchableProductKeys);
        Assert.Equal("dark", session.ThemePreference);
        Assert.Equal("http://localhost:5183/app/assurarr", session.CallbackUrl);
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
    public async Task Handoff_redeem_rejects_non_assurarr_target_product()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new RedeemHandoffRequest("assurarr-wrong-product"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("handoff.product_mismatch", body);
    }

    [Fact]
    public async Task Handoff_redeem_rejects_when_assurarr_is_not_launchable()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new RedeemHandoffRequest("assurarr-not-available"));

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
                "assurarr-ok" => CreateResponse("assurarr", ["assurarr", "staffarr"]),
                "assurarr-wrong-product" => CreateResponse("trainarr", ["assurarr"]),
                "assurarr-not-available" => CreateResponse("assurarr", ["staffarr"]),
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
                    "quality_manager",
                    true,
                    launchableProductKeys,
                    "dark",
                    45,
                    "http://localhost:5183/app/assurarr"))
            };
        }
    }
}
