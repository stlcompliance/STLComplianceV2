using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportArr.Api.Services;

namespace STLCompliance.ReportArr.Auth.Tests;

public sealed class ReportArrAuthEndpointsTests : IAsyncLifetime
{
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DemoUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DemoPersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private WebApplicationFactory<global::ReportArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";

        _factory = new WebApplicationFactory<global::ReportArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("RecordArr:BaseUrl", "http://recordarr.test");
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
    public async Task Session_bootstrap_allows_users_after_non_reportarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/session", token));
        response.EnsureSuccessStatusCode();

        var session = await ReadJsonObjectAsync(response);
        Assert.Equal("reportarr", session["productKey"]!.GetValue<string>());
        Assert.True(session["hasReportArrAccess"]!.GetValue<bool>());
        Assert.Contains(
            session["launchableProductKeys"]!.AsArray(),
            item => string.Equals(item?.GetValue<string>(), "nexarr", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Me_allows_users_after_non_reportarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/me", token));
        response.EnsureSuccessStatusCode();

        var me = await ReadJsonObjectAsync(response);
        Assert.Equal("reportarr", me["productKey"]!.GetValue<string>());
        Assert.True(me["hasReportArrAccess"]!.GetValue<bool>());
        Assert.Contains(
            me["launchableProductKeys"]!.AsArray(),
            item => string.Equals(item?.GetValue<string>(), "nexarr", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Report_access_policies_reject_platform_admin_without_reportarr_role()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/report-access-policies", token));

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Report_access_policies_allow_reportarr_admin_after_non_reportarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "reportarr_admin");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/report-access-policies", token));
        response.EnsureSuccessStatusCode();

        _ = await ReadJsonArrayAsync(response);
    }

    [Fact]
    public async Task Audit_scopes_reject_platform_admin_without_reporting_role()
    {
        var token = CreateAccessToken(["nexarr", "compliancecore"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/audit-scopes", token));

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Audit_scopes_allow_compliance_reporter_after_non_reportarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr", "compliancecore"], tenantRoleKey: "compliance_reporter");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/audit-scopes", token));
        response.EnsureSuccessStatusCode();

        _ = await ReadJsonArrayAsync(response);
    }

    private string CreateAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey = "tenant_member", bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ReportArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            DemoUserId,
            DemoPersonId,
            "reportarr.user@demo.stl",
            "ReportArr User",
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

    private static async Task<JsonArray> ReadJsonArrayAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?.AsArray()
            ?? throw new InvalidOperationException("Expected a JSON array response.");
    }
}
