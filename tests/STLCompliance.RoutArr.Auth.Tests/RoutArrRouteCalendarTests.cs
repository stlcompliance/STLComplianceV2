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

public sealed class RoutArrRouteCalendarTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrCalendarNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrCalendar-{Guid.NewGuid():N}";

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
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Route_calendar_returns_empty_days_for_empty_tenant()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/calendar", dispatcherToken));
        response.EnsureSuccessStatusCode();
        var calendar = (await response.Content.ReadFromJsonAsync<RouteCalendarResponse>())!;

        Assert.Equal("daily", calendar.Scope);
        Assert.Single(calendar.Days);
        Assert.Empty(calendar.Days[0].Events);
        Assert.Equal(0, calendar.Summary.TripCount);
        Assert.Equal(0, calendar.Summary.RouteCount);
        Assert.Equal(0, calendar.Summary.StopCount);
    }

    [Fact]
    public async Task Route_calendar_buckets_trips_routes_and_stops_by_day()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var now = DateTimeOffset.UtcNow;
        var dayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);

        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Calendar trip",
            "Scheduled for calendar view",
            "VEH-200",
            dayStart.AddHours(9),
            dayStart.AddHours(17),
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        var trip = (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var createRouteRequest = Authorized(HttpMethod.Post, "/api/routes", dispatcherToken);
        createRouteRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "Calendar route",
            "Linked route for calendar",
            trip.TripId,
            [
                new CreateRouteStopRequest("stop-cal-1", "Pickup yard", "North depot", "pickup", 1, dayStart.AddHours(10)),
            ]));
        (await _routarrClient.SendAsync(createRouteRequest)).EnsureSuccessStatusCode();

        var calendarResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/calendar", dispatcherToken));
        calendarResponse.EnsureSuccessStatusCode();
        var calendar = (await calendarResponse.Content.ReadFromJsonAsync<RouteCalendarResponse>())!;

        Assert.True(calendar.Summary.TripCount >= 1);
        Assert.True(calendar.Summary.RouteCount >= 1);
        Assert.True(calendar.Summary.StopCount >= 1);

        var today = calendar.Days.FirstOrDefault(x => x.Date == dayStart);
        Assert.NotNull(today);
        Assert.Contains(today!.Events, x => x.EventType == "trip" && x.EntityId == trip.TripId);
        Assert.Contains(today.Events, x => x.EventType == "route");
        Assert.Contains(today.Events, x => x.EventType == "stop" && x.Label == "Pickup yard");
    }

    [Fact]
    public async Task Route_calendar_weekly_scope_returns_seven_days()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/calendar?scope=weekly", dispatcherToken));
        response.EnsureSuccessStatusCode();
        var calendar = (await response.Content.ReadFromJsonAsync<RouteCalendarResponse>())!;

        Assert.Equal("weekly", calendar.Scope);
        Assert.Equal(7, calendar.Days.Count);
        Assert.Equal(calendar.WindowStart.AddDays(7), calendar.WindowEnd);
    }

    [Fact]
    public async Task Route_calendar_custom_date_range_is_accepted()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var start = "2026-06-01";
        var end = "2026-06-04";

        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/dispatch/calendar?start={start}&end={end}", dispatcherToken));
        response.EnsureSuccessStatusCode();
        var calendar = (await response.Content.ReadFromJsonAsync<RouteCalendarResponse>())!;

        Assert.Equal("custom", calendar.Scope);
        Assert.Equal(3, calendar.Days.Count);
    }

    [Fact]
    public async Task Route_calendar_requires_authentication()
    {
        var response = await _routarrClient.GetAsync("/api/dispatch/calendar");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Route_calendar_requires_routarr_entitlement()
    {
        var token = CreateRoutArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/calendar", token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
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
            $"{productKey}-calendar-test",
            $"{productKey} Calendar Test",
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

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>)
                || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
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
