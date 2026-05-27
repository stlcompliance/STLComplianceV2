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

public sealed class RoutArrDispatchBoardTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrDispatchNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrDispatch-{Guid.NewGuid():N}";

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
    public async Task Dispatch_board_returns_zero_counts_for_empty_tenant()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/board", dispatcherToken));
        response.EnsureSuccessStatusCode();
        var board = (await response.Content.ReadFromJsonAsync<DispatchBoardResponse>())!;

        Assert.Equal("daily", board.Scope);
        Assert.Equal(0, board.Trips.TotalCount);
        Assert.Equal(0, board.Routes.TotalCount);
        Assert.Equal(0, board.Stops.TotalCount);
        Assert.Equal(0, board.WorkQueue.UnassignedDriverTripCount);
        Assert.Empty(board.AssignedTrips);
        Assert.Empty(board.ActiveTrips);
    }

    [Fact]
    public async Task Dispatch_board_reflects_trips_routes_stops_and_work_queue()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var driverPersonId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Board test trip",
            "Late and active board trip",
            "VEH-100",
            now.AddHours(-2),
            now.AddMinutes(90),
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        var trip = (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var dispatchRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/status", dispatcherToken);
        dispatchRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        (await _routarrClient.SendAsync(dispatchRequest)).EnsureSuccessStatusCode();

        var startRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/status", dispatcherToken);
        startRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("in_progress"));
        (await _routarrClient.SendAsync(startRequest)).EnsureSuccessStatusCode();

        var createRouteRequest = Authorized(HttpMethod.Post, "/api/routes", dispatcherToken);
        createRouteRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "Board route",
            "Linked route",
            trip.TripId,
            [
                new CreateRouteStopRequest("stop-1", "Pickup", "North yard", "pickup", 1, now.AddHours(1)),
            ]));
        (await _routarrClient.SendAsync(createRouteRequest)).EnsureSuccessStatusCode();

        var unlinkedRouteRequest = Authorized(HttpMethod.Post, "/api/routes", dispatcherToken);
        unlinkedRouteRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "Unlinked route",
            "Needs trip link",
            null,
            [
                new CreateRouteStopRequest("stop-u1", "Depot", "Main depot", "depot", 1, null),
            ]));
        (await _routarrClient.SendAsync(unlinkedRouteRequest)).EnsureSuccessStatusCode();

        var boardResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/board", dispatcherToken));
        boardResponse.EnsureSuccessStatusCode();
        var board = (await boardResponse.Content.ReadFromJsonAsync<DispatchBoardResponse>())!;

        Assert.True(board.Trips.TotalCount >= 1);
        Assert.True(board.Trips.InProgressCount >= 1);
        Assert.True(board.Trips.AtRiskCount >= 1);
        Assert.True(board.Routes.TotalCount >= 2);
        Assert.True(board.Stops.TotalCount >= 2);
        Assert.Equal(1, board.WorkQueue.UnlinkedRouteCount);
        Assert.NotEmpty(board.AssignedTrips);
        Assert.NotEmpty(board.ActiveTrips);
        Assert.Contains(board.ActiveTrips, x => x.TripId == trip.TripId && x.IsAtRisk);
    }

    [Fact]
    public async Task Dispatch_board_weekly_scope_is_accepted()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/board?scope=weekly", dispatcherToken));
        response.EnsureSuccessStatusCode();
        var board = (await response.Content.ReadFromJsonAsync<DispatchBoardResponse>())!;

        Assert.Equal("weekly", board.Scope);
        Assert.Equal(board.WindowStart.AddDays(7), board.WindowEnd);
    }

    [Fact]
    public async Task Dispatch_board_requires_authentication()
    {
        var response = await _routarrClient.GetAsync("/api/dispatch/board");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Dispatch_board_requires_routarr_entitlement()
    {
        var token = CreateRoutArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/board", token));
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
            $"{productKey}-dispatch-test",
            $"{productKey} Dispatch Test",
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
