using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Services;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;
using STLCompliance.Shared.Auth;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrDispatchOverrideReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _dispatcherToken = null!;
    private Guid _tripId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"DispatchOverrideReportNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"DispatchOverrideReportRoutArr-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var serviceToken = await IssueServiceTokenAsync(adminToken, "routarr");

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", serviceToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _routarrClient = _routarrFactory.CreateClient();
        _dispatcherToken = await RedeemRoutArrTokenAsync();
        _tripId = await SeedOverrideAuditAsync();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Dispatch_override_report_summary_returns_scoped_overrides()
    {
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/dispatch-overrides/summary?scope=daily", _dispatcherToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<DispatchOverrideReportSummaryResponse>())!;
        Assert.True(summary.TotalOverrideCount >= 1);
        Assert.Contains(summary.OverrideKindCounts, x =>
            string.Equals(x.Key, "availability", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(summary.RecentOverrides, x =>
            x.TargetId == _tripId.ToString()
            && x.OverrideKinds.Contains("availability"));
    }

    [Fact]
    public async Task Dispatch_override_report_export_returns_csv()
    {
        var managerToken = CreateRoutArrAccessToken(["routarr"], "routarr_manager");
        var response = await _routarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/reports/dispatch-overrides/summary/export?scope=daily",
                managerToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("occurredAt,action", csv, StringComparison.Ordinal);
        Assert.Contains("override:availability", csv, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Dispatch_override_report_denies_driver_role()
    {
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver");
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/dispatch-overrides/summary", driverToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> SeedOverrideAuditAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var driverPersonId = Guid.NewGuid().ToString();
        var tripStart = now.AddHours(2);
        var tripEnd = now.AddHours(6);

        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Override report trip",
            "Dispatch override report seed",
            null,
            tripStart,
            tripEnd,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        var trip = (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var availabilityRequest = Authorized(HttpMethod.Post, "/api/driver-availability", _dispatcherToken);
        availabilityRequest.Content = JsonContent.Create(new CreateDriverAvailabilityRequest(
            driverPersonId,
            "unavailable",
            tripStart.AddHours(-1),
            tripEnd.AddHours(1),
            "PTO",
            null));
        (await _routarrClient.SendAsync(availabilityRequest)).EnsureSuccessStatusCode();

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", _dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId,
            IgnoreAvailabilityConflicts: true));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        return trip.TripId;
    }

    private string CreateRoutArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        Guid? userIdOverride = null)
    {
        using var scope = _routarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RoutArrTokenService>();
        var userId = userIdOverride ?? PlatformSeeder.DemoAdminUserId;
        var (token, _) = tokenService.CreateAccessToken(
            userId,
            userId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return token;
    }

    private async Task<string> RedeemRoutArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _routarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new RoutArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<RoutArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("routarr", "http://localhost:5180/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-override-report",
            "override report test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return login.AccessToken;
    }

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        foreach (var descriptor in services
                     .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
                     .ToList())
        {
            services.Remove(descriptor);
        }
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
