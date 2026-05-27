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

public sealed class RoutArrDriverAvailabilityTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrAvailNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrAvail-{Guid.NewGuid():N}";

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

                services.AddHttpClient<NexArrHandoffClient>()
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
    public async Task Driver_availability_panel_returns_empty_summary_for_empty_tenant()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/driver-availability", dispatcherToken));
        response.EnsureSuccessStatusCode();
        var panel = (await response.Content.ReadFromJsonAsync<DriverAvailabilityPanelResponse>())!;

        Assert.Equal("daily", panel.Scope);
        Assert.Equal(0, panel.Summary.RecordCount);
        Assert.Equal(0, panel.Summary.ConflictCount);
        Assert.Empty(panel.Records);
    }

    [Fact]
    public async Task Driver_availability_detects_conflict_with_assigned_trip()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var driverPersonId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;
        var tripStart = now.AddHours(2);
        var tripEnd = now.AddHours(6);

        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Conflict trip",
            "Driver has overlapping assignment",
            null,
            tripStart,
            tripEnd,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        var trip = (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var statusRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/status", dispatcherToken);
        statusRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("assigned"));
        (await _routarrClient.SendAsync(statusRequest)).EnsureSuccessStatusCode();

        var createAvailabilityRequest = Authorized(HttpMethod.Post, "/api/driver-availability", dispatcherToken);
        createAvailabilityRequest.Content = JsonContent.Create(new CreateDriverAvailabilityRequest(
            driverPersonId,
            "unavailable",
            tripStart.AddHours(-1),
            tripEnd.AddHours(1),
            "PTO",
            null));
        var createAvailabilityResponse = await _routarrClient.SendAsync(createAvailabilityRequest);
        createAvailabilityResponse.EnsureSuccessStatusCode();
        var availability = (await createAvailabilityResponse.Content.ReadFromJsonAsync<DriverAvailabilityDetailResponse>())!;

        Assert.True(availability.HasConflict);
        Assert.Single(availability.ConflictingTrips);
        Assert.Equal(trip.TripId, availability.ConflictingTrips[0].TripId);

        var panelResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/driver-availability", dispatcherToken));
        panelResponse.EnsureSuccessStatusCode();
        var panel = (await panelResponse.Content.ReadFromJsonAsync<DriverAvailabilityPanelResponse>())!;

        Assert.Equal(1, panel.Summary.RecordCount);
        Assert.Equal(1, panel.Summary.ConflictCount);
        Assert.True(panel.Records[0].HasConflict);
    }

    [Fact]
    public async Task Driver_can_create_own_availability_record()
    {
        var otherPersonId = Guid.NewGuid().ToString();
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var now = DateTimeOffset.UtcNow;

        var createRequest = Authorized(HttpMethod.Post, "/api/driver-availability", driverToken);
        createRequest.Content = JsonContent.Create(new CreateDriverAvailabilityRequest(
            otherPersonId,
            "unavailable",
            now.AddDays(1),
            now.AddDays(1).AddHours(8),
            "Doctor",
            null));

        var response = await _routarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using var scope = _routarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RoutArrTokenService>();
        var userId = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var driverPersonId = personGuid.ToString();
        var (ownPersonToken, _) = tokenService.CreateAccessToken(
            userId,
            personGuid,
            "driver@example.com",
            "Driver",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            "routarr_driver",
            ["routarr"],
            isPlatformAdmin: false);

        createRequest = Authorized(HttpMethod.Post, "/api/driver-availability", ownPersonToken);
        createRequest.Content = JsonContent.Create(new CreateDriverAvailabilityRequest(
            driverPersonId,
            "unavailable",
            now.AddDays(1),
            now.AddDays(1).AddHours(8),
            "Doctor",
            null));
        response = await _routarrClient.SendAsync(createRequest);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Driver_availability_requires_authentication_and_entitlement()
    {
        var unauthenticated = await _routarrClient.GetAsync("/api/dispatch/driver-availability");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);

        var noEntitlementToken = CreateRoutArrAccessToken([]);
        var forbidden = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/driver-availability", noEntitlementToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    private async Task<string> RedeemRoutArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/handoff/redeem")
        {
            Content = JsonContent.Create(new RoutArrRedeemRequest(handoffCode)),
        };
        var redeemResponse = await _routarrClient.SendAsync(redeemRequest);
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
            $"{productKey}-avail-test",
            $"{productKey} Avail Test",
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
