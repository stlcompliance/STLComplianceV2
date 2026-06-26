using STLCompliance.Shared.Integration;
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

public sealed class RoutArrActiveTripsTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _dispatcherToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ActiveTripsNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"ActiveTripsRoutArr-{Guid.NewGuid():N}";

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
        _dispatcherToken = CreateRoutArrAccessToken(["routarr"], "tenant_admin");
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Active_trips_returns_dispatched_trip_with_late_flag()
    {
        var now = DateTimeOffset.UtcNow;
        var createRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Active map trip",
            "Late dispatched",
            "VEH-200",
            now.AddHours(-3),
            now.AddMinutes(-30),
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", _dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            "11111111-1111-1111-1111-111111111111"));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var dispatchRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/status", _dispatcherToken);
        dispatchRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        (await _routarrClient.SendAsync(dispatchRequest)).EnsureSuccessStatusCode();

        var activeResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/active-trips?scope=daily", _dispatcherToken));
        activeResponse.EnsureSuccessStatusCode();
        var active = (await activeResponse.Content.ReadFromJsonAsync<ActiveTripsResponse>())!;

        Assert.True(active.Summary.TotalCount >= 1);
        var row = Assert.Single(active.Items, x => x.TripId == trip.TripId);
        Assert.Equal("dispatched", row.DispatchStatus);
        Assert.True(row.IsLate);
        Assert.Equal("VEH-200", row.VehicleRefKey);
        Assert.InRange(row.TimelineOffsetPercent, 0, 100);
        Assert.InRange(row.TimelineWidthPercent, 6, 100);
    }

    [Fact]
    public async Task Active_trips_attention_filter_excludes_on_track()
    {
        var now = DateTimeOffset.UtcNow;

        var onTrackRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        onTrackRequest.Content = JsonContent.Create(new CreateTripRequest(
            "On track active",
            "Future dispatched",
            "VEH-300",
            now.AddHours(2),
            now.AddHours(6),
            null));
        var onTrackResponse = await _routarrClient.SendAsync(onTrackRequest);
        onTrackResponse.EnsureSuccessStatusCode();
        var onTrackTrip = (await onTrackResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignOnTrack = Authorized(HttpMethod.Patch, $"/api/trips/{onTrackTrip.TripId}/assign-driver", _dispatcherToken);
        assignOnTrack.Content = JsonContent.Create(new AssignTripDriverRequest(
            "11111111-1111-1111-1111-111111111111"));
        (await _routarrClient.SendAsync(assignOnTrack)).EnsureSuccessStatusCode();

        var dispatchOnTrack = Authorized(HttpMethod.Patch, $"/api/trips/{onTrackTrip.TripId}/status", _dispatcherToken);
        dispatchOnTrack.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        (await _routarrClient.SendAsync(dispatchOnTrack)).EnsureSuccessStatusCode();

        var lateRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        lateRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Late active",
            "Late dispatched",
            "VEH-301",
            now.AddHours(-3),
            now.AddMinutes(-30),
            null));
        var lateResponse = await _routarrClient.SendAsync(lateRequest);
        lateResponse.EnsureSuccessStatusCode();
        var lateTrip = (await lateResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignLate = Authorized(HttpMethod.Patch, $"/api/trips/{lateTrip.TripId}/assign-driver", _dispatcherToken);
        assignLate.Content = JsonContent.Create(new AssignTripDriverRequest(
            "22222222-2222-2222-2222-222222222222",
            DriverDisplayName: "Late Driver"));
        (await _routarrClient.SendAsync(assignLate)).EnsureSuccessStatusCode();

        var dispatchLate = Authorized(HttpMethod.Patch, $"/api/trips/{lateTrip.TripId}/status", _dispatcherToken);
        dispatchLate.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        (await _routarrClient.SendAsync(dispatchLate)).EnsureSuccessStatusCode();

        var filteredResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/active-trips?scope=daily&attentionOnly=true", _dispatcherToken));
        filteredResponse.EnsureSuccessStatusCode();
        var filtered = (await filteredResponse.Content.ReadFromJsonAsync<ActiveTripsResponse>())!;

        Assert.Contains(filtered.Items, x => x.TripId == lateTrip.TripId);
        Assert.DoesNotContain(filtered.Items, x => x.TripId == onTrackTrip.TripId);
        Assert.Equal("Late Driver", Assert.Single(filtered.Items, x => x.TripId == lateTrip.TripId).AssignedDriverDisplayName);
    }

    [Fact]
    public async Task Active_trips_includes_open_exception_count()
    {
        var now = DateTimeOffset.UtcNow;
        var createRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Exception active trip",
            "Has exception",
            null,
            now.AddHours(-2),
            now.AddHours(2),
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", _dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            "11111111-1111-1111-1111-111111111111"));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var dispatchRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/status", _dispatcherToken);
        dispatchRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        (await _routarrClient.SendAsync(dispatchRequest)).EnsureSuccessStatusCode();

        var exceptionRequest = Authorized(HttpMethod.Post, "/api/dispatch/exceptions", _dispatcherToken);
        exceptionRequest.Content = JsonContent.Create(new CreateDispatchExceptionRequest(
            "Delay on active trip",
            "Traffic delay",
            "delay",
            trip.TripId,
            null,
            null));
        (await _routarrClient.SendAsync(exceptionRequest)).EnsureSuccessStatusCode();

        var activeResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/active-trips?scope=daily", _dispatcherToken));
        activeResponse.EnsureSuccessStatusCode();
        var active = (await activeResponse.Content.ReadFromJsonAsync<ActiveTripsResponse>())!;

        var row = Assert.Single(active.Items, x => x.TripId == trip.TripId);
        Assert.Equal(1, row.OpenExceptionCount);
        Assert.True(active.Summary.OpenExceptionCount >= 1);
    }

    [Fact]
    public async Task Active_trips_empty_when_no_dispatched_trips()
    {
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/active-trips?scope=daily", _dispatcherToken));
        response.EnsureSuccessStatusCode();
        var active = (await response.Content.ReadFromJsonAsync<ActiveTripsResponse>())!;
        Assert.Equal("daily", active.Scope);
        Assert.NotNull(active.Items);
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
            $"{productKey}-active-trips",
            "active trips test",
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
