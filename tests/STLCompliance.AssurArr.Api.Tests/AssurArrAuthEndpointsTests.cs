using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
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
    public async Task Handoff_redeem_routes_return_session_for_assurarr_target_user(string path)
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
        Assert.Contains("assurarr", session.LaunchableProductKeys);
        Assert.Contains("ledgarr", session.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", session.LaunchableProductKeys);
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
    public async Task Handoff_redeem_allows_non_assurarr_launch_context_when_target_is_assurarr()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new RedeemHandoffRequest("assurarr-not-available"));

        response.EnsureSuccessStatusCode();

        var session = (await response.Content.ReadFromJsonAsync<AssurArrHandoffSessionResponse>())!;
        Assert.Equal(UserId.ToString(), session.UserId);
        Assert.Equal("quality_manager", session.TenantRoleKey);
        Assert.Contains("assurarr", session.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", session.LaunchableProductKeys);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
    }

    [Fact]
    public async Task Session_bootstrap_returns_fixed_suite_launch_context_without_product_access_flag()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/session");
        request.Headers.Add("X-STL-Test-TenantRoleKey", "quality_manager");
        request.Headers.Add("X-STL-Test-LaunchableProductKeys", "staffarr");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var session = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
        Assert.Equal("assurarr", session["productKey"]!.GetValue<string>());
        Assert.False(session.ContainsKey("hasAssurArrAccess"));
        Assert.Contains(
            session["launchableProductKeys"]!.AsArray(),
            item => string.Equals(item?.GetValue<string>(), "assurarr", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            session["launchableProductKeys"]!.AsArray(),
            item => string.Equals(item?.GetValue<string>(), "compliancecore", StringComparison.OrdinalIgnoreCase));
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
