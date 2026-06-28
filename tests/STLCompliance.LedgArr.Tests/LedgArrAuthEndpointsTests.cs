using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LedgArr.Api.Data;
using LedgArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace STLCompliance.LedgArr.Tests;

public sealed class LedgArrAuthEndpointsTests : IAsyncLifetime
{
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DemoUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DemoPersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid DemoLegalEntityId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private WebApplicationFactory<global::LedgArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"LedgArrAuth-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::LedgArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("StaffArr:BaseUrl", "http://staffarr.test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<LedgArrDbContext>(services);
                services.AddDbContext<LedgArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LedgArrDbContext>();
        db.FinancialLegalEntities.Add(new FinancialLegalEntity
        {
            Id = DemoLegalEntityId,
            TenantId = DemoTenantId,
            EntityCode = "US-CORP",
            DisplayName = "US Controller Entity",
            EntityType = "company",
            BaseCurrencyCode = "USD",
            Status = "active"
        });
        db.SaveChanges();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Session_bootstrap_allows_users_after_non_ledgarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/session", token));
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.Equal("ledgarr", root.GetProperty("productKey").GetString());
        Assert.Contains(
            root.GetProperty("launchableProductKeys").EnumerateArray().Select(item => item.GetString()),
            value => string.Equals(value, "ledgarr", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            root.GetProperty("launchableProductKeys").EnumerateArray().Select(item => item.GetString()),
            value => string.Equals(value, "fieldcompanion", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            root.GetProperty("launchableProductKeys").EnumerateArray().Select(item => item.GetString()),
            value => string.Equals(value, "compliancecore", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Workspace_summary_rejects_plain_tenant_member()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/summary", token));

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_summary_allows_controller_after_non_ledgarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "controller");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/summary", token));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Posting_rule_create_allows_controller()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "controller");
        using var request = Authorized(HttpMethod.Post, "/api/v1/ledgarr/posting-rules", token);
        request.Content = JsonContent.Create(new CreatePostingRuleRequest(
            "manual_adjustment",
            "Manual adjustment bridge",
            "active",
            [new CreatePostingRuleLineRequest("expense", "5000", "debit")]));

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Manual adjustment bridge", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Payroll_calendars_reject_platform_admin_without_finance_role()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/payroll/calendars", token));

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Payroll_calendar_create_allows_controller()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "controller");
        using var request = Authorized(HttpMethod.Post, "/api/v1/payroll/calendars", token);
        request.Content = JsonContent.Create(new CreatePayrollCalendarRequest(
            DemoLegalEntityId,
            "Biweekly Controller Payroll",
            "biweekly",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 14),
            new DateOnly(2026, 1, 16),
            new DateOnly(2025, 12, 30),
            "America/Chicago",
            "active"));

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    private string CreateAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey = "tenant_member", bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<LedgArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            DemoUserId,
            DemoPersonId,
            "ledgarr.user@demo.stl",
            "LedgArr User",
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
}
