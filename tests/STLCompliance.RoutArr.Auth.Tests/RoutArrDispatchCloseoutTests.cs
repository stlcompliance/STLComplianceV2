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
using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;
using STLCompliance.Shared.Auth;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrDispatchCloseoutTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrCloseoutNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrCloseout-{Guid.NewGuid():N}";

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
    public async Task Closeout_summary_lists_open_planned_trip()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var now = DateTimeOffset.UtcNow;
        await CreateTripAsync(dispatcherToken, now, now.AddHours(4));

        var summaryRequest = Authorized(HttpMethod.Get, "/api/dispatch/closeout/summary?scope=daily", dispatcherToken);
        var summaryResponse = await _routarrClient.SendAsync(summaryRequest);
        summaryResponse.EnsureSuccessStatusCode();
        var summary = (await summaryResponse.Content.ReadFromJsonAsync<DispatchCloseoutSummaryResponse>())!;

        Assert.True(summary.Counts.OpenTrips >= 1);
        Assert.Contains(summary.OpenTrips, x => x.DispatchStatus == TripDispatchStatuses.Planned);
    }

    [Fact]
    public async Task Closeout_preview_blocks_planned_trip_on_complete_disposition()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var now = DateTimeOffset.UtcNow;
        await CreateTripAsync(dispatcherToken, now, now.AddHours(4));

        var previewRequest = Authorized(HttpMethod.Post, "/api/dispatch/closeout/preview", dispatcherToken);
        previewRequest.Content = JsonContent.Create(new DispatchCloseoutRequest(
            "daily",
            DispatchCloseoutRules.TripDispositionComplete,
            DispatchCloseoutRules.StopDispositionSkip));
        var previewResponse = await _routarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<DispatchCloseoutPreviewResponse>())!;

        Assert.Contains(
            preview.TripActions,
            x => x.CurrentDispatchStatus == TripDispatchStatuses.Planned && !x.CanApply);
    }

    [Fact]
    public async Task Closeout_apply_cancels_planned_trip_and_skips_pending_stop()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var now = DateTimeOffset.UtcNow;
        var trip = await CreateTripAsync(dispatcherToken, now, now.AddHours(4));

        var createRouteRequest = Authorized(HttpMethod.Post, "/api/routes", dispatcherToken);
        createRouteRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "Closeout route",
            "EOD test",
            trip.TripId,
            [
                new CreateRouteStopRequest("stop-a", "Stop A", "123 Main", "pickup", 1, null),
            ]));
        (await _routarrClient.SendAsync(createRouteRequest)).EnsureSuccessStatusCode();

        var applyRequest = Authorized(HttpMethod.Post, "/api/dispatch/closeout/apply", dispatcherToken);
        applyRequest.Content = JsonContent.Create(new DispatchCloseoutRequest(
            "daily",
            DispatchCloseoutRules.TripDispositionCancel,
            DispatchCloseoutRules.StopDispositionSkip));
        var applyResponse = await _routarrClient.SendAsync(applyRequest);
        applyResponse.EnsureSuccessStatusCode();
        var applied = (await applyResponse.Content.ReadFromJsonAsync<DispatchCloseoutApplyResponse>())!;

        Assert.Contains(applied.TripResults, x => x.TripId == trip.TripId && x.Applied);
        Assert.Contains(applied.StopResults, x => x.Applied && x.FinalStopStatus == RouteStopStatuses.Skipped);

        var tripRequest = Authorized(HttpMethod.Get, $"/api/trips/{trip.TripId}", dispatcherToken);
        var tripResponse = await _routarrClient.SendAsync(tripRequest);
        tripResponse.EnsureSuccessStatusCode();
        var updatedTrip = (await tripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal(TripDispatchStatuses.Cancelled, updatedTrip.DispatchStatus);
    }

    [Fact]
    public async Task Closeout_apply_completes_in_progress_trip()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var driverPersonId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;
        var trip = await CreateTripAsync(dispatcherToken, now, now.AddHours(4));

        await AssignDriverAsync(dispatcherToken, trip.TripId, driverPersonId);
        await UpdateTripStatusAsync(dispatcherToken, trip.TripId, TripDispatchStatuses.Dispatched);
        await UpdateTripStatusAsync(dispatcherToken, trip.TripId, TripDispatchStatuses.InProgress);

        var applyRequest = Authorized(HttpMethod.Post, "/api/dispatch/closeout/apply", dispatcherToken);
        applyRequest.Content = JsonContent.Create(new DispatchCloseoutRequest(
            "daily",
            DispatchCloseoutRules.TripDispositionComplete,
            DispatchCloseoutRules.StopDispositionSkip));
        var applyResponse = await _routarrClient.SendAsync(applyRequest);
        applyResponse.EnsureSuccessStatusCode();
        var applied = (await applyResponse.Content.ReadFromJsonAsync<DispatchCloseoutApplyResponse>())!;

        Assert.Contains(applied.TripResults, x => x.TripId == trip.TripId && x.Applied);

        var tripRequest = Authorized(HttpMethod.Get, $"/api/trips/{trip.TripId}", dispatcherToken);
        var tripResponse = await _routarrClient.SendAsync(tripRequest);
        tripResponse.EnsureSuccessStatusCode();
        var updatedTrip = (await tripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal(TripDispatchStatuses.Completed, updatedTrip.DispatchStatus);
    }

    private async Task AssignDriverAsync(string token, Guid tripId, string driverPersonId)
    {
        var request = Authorized(HttpMethod.Patch, $"/api/trips/{tripId}/assign-driver", token);
        request.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId, false));
        (await _routarrClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task UpdateTripStatusAsync(string token, Guid tripId, string status)
    {
        var request = Authorized(HttpMethod.Patch, $"/api/trips/{tripId}/status", token);
        request.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest(status));
        (await _routarrClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task<TripDetailResponse> CreateTripAsync(
        string dispatcherToken,
        DateTimeOffset tripStart,
        DateTimeOffset tripEnd)
    {
        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Closeout trip",
            "Closeout test",
            null,
            tripStart,
            tripEnd,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        return (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
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
            $"{productKey}-closeout-test",
            $"{productKey} Closeout Test",
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
