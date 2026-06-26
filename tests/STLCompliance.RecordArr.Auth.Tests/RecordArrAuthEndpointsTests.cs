using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RecordArr.Api.Services;
using RecordArr.Api.Endpoints;

namespace STLCompliance.RecordArr.Auth.Tests;

public sealed class RecordArrAuthEndpointsTests : IAsyncLifetime
{
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DemoUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DemoPersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private WebApplicationFactory<global::RecordArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";

        _factory = new WebApplicationFactory<global::RecordArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
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
    public async Task Session_bootstrap_allows_users_after_non_recordarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/session", token));
        response.EnsureSuccessStatusCode();

        var session = await ReadJsonObjectAsync(response);
        Assert.Equal("recordarr", session["productKey"]!.GetValue<string>());
        Assert.True(session["hasRecordArrAccess"]!.GetValue<bool>());
        Assert.Contains(
            session["launchableProductKeys"]!.AsArray(),
            item => string.Equals(item?.GetValue<string>(), "nexarr", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Seed_record_rejects_platform_admin_from_other_tenant()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/records/rec-bol-001", token));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Created_record_allows_same_tenant_owner_after_non_recordarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);
        using var createRequest = Authorized(HttpMethod.Post, "/api/v1/workspace/records", token);
        createRequest.Content = JsonContent.Create(new WorkspaceEndpoints.CreateRecordRequest(
            "Driver packet",
            "Seeded by auth regression test.",
            "document",
            "operations",
            "packet",
            "driver_packet",
            "internal",
            "routarr",
            "trip",
            "trip-100",
            "Trip 100",
            DemoPersonId.ToString(),
            "packet.txt",
            "text/plain",
            Convert.ToBase64String("hello recordarr"u8.ToArray())));

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await ReadJsonObjectAsync(createResponse);
        var recordId = created["recordId"]!.GetValue<string>();

        var getResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/workspace/records/{recordId}", token));
        getResponse.EnsureSuccessStatusCode();
    }

    private string CreateAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey = "tenant_member", bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RecordArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            DemoUserId,
            DemoPersonId,
            "recordarr.user@demo.stl",
            "RecordArr User",
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

    private static async Task<JsonObject> ReadJsonObjectAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?.AsObject()
            ?? throw new InvalidOperationException("Expected a JSON object response.");
    }
}
