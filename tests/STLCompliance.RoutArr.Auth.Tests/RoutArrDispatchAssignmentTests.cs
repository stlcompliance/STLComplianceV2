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

public sealed class RoutArrDispatchAssignmentTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrAssignNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrAssign-{Guid.NewGuid():N}";

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
    public async Task Assignment_preview_blocks_driver_with_unavailable_window()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var driverPersonId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;
        var tripStart = now.AddHours(2);
        var tripEnd = now.AddHours(6);

        var trip = await CreateTripAsync(dispatcherToken, tripStart, tripEnd);

        var availabilityRequest = Authorized(HttpMethod.Post, "/api/driver-availability", dispatcherToken);
        availabilityRequest.Content = JsonContent.Create(new CreateDriverAvailabilityRequest(
            driverPersonId,
            "unavailable",
            tripStart.AddHours(-1),
            tripEnd.AddHours(1),
            "PTO",
            null));
        (await _routarrClient.SendAsync(availabilityRequest)).EnsureSuccessStatusCode();

        var previewRequest = Authorized(HttpMethod.Post, "/api/dispatch/assignments/preview", dispatcherToken);
        previewRequest.Content = JsonContent.Create(new DispatchAssignmentPreviewRequest(
            trip.TripId,
            "driver",
            driverPersonId,
            null));
        var previewResponse = await _routarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<DispatchAssignmentPreviewResponse>())!;

        Assert.False(preview.CanAssign);
        Assert.True(preview.HasBlockingConflicts);
        Assert.Single(preview.BlockingDriverAvailability);
        Assert.Empty(preview.OverlappingTrips);

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId));
        var assignResponse = await _routarrClient.SendAsync(assignRequest);
        Assert.Equal(HttpStatusCode.Conflict, assignResponse.StatusCode);

        assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId, IgnoreAvailabilityConflicts: true));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Assign_vehicle_detects_equipment_conflict_and_allows_override()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var vehicleRefKey = $"vehicle-{Guid.NewGuid():N}";
        var now = DateTimeOffset.UtcNow;
        var tripStart = now.AddHours(2);
        var tripEnd = now.AddHours(6);

        var trip = await CreateTripAsync(dispatcherToken, tripStart, tripEnd, vehicleRefKey: null);

        var equipmentRequest = Authorized(HttpMethod.Post, "/api/equipment-availability", dispatcherToken);
        equipmentRequest.Content = JsonContent.Create(new CreateEquipmentAvailabilityRequest(
            vehicleRefKey,
            "unavailable",
            tripStart.AddHours(-1),
            tripEnd.AddHours(1),
            "Maintenance",
            null));
        (await _routarrClient.SendAsync(equipmentRequest)).EnsureSuccessStatusCode();

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-vehicle", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripVehicleRequest(vehicleRefKey));
        var blocked = await _routarrClient.SendAsync(assignRequest);
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);

        assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-vehicle", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripVehicleRequest(vehicleRefKey, IgnoreAvailabilityConflicts: true));
        var assignResponse = await _routarrClient.SendAsync(assignRequest);
        assignResponse.EnsureSuccessStatusCode();
        var assigned = (await assignResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal(vehicleRefKey, assigned.VehicleRefKey);
    }

    [Fact]
    public async Task Assignment_preview_detects_overlapping_driver_trips()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var driverPersonId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;
        var tripStart = now.AddHours(2);
        var tripEnd = now.AddHours(6);

        var firstTrip = await CreateTripAsync(dispatcherToken, tripStart, tripEnd);
        await AssignDriverAsync(dispatcherToken, firstTrip.TripId, driverPersonId);

        var secondTrip = await CreateTripAsync(dispatcherToken, tripStart.AddHours(1), tripEnd.AddHours(1));

        var previewRequest = Authorized(HttpMethod.Post, "/api/dispatch/assignments/preview", dispatcherToken);
        previewRequest.Content = JsonContent.Create(new DispatchAssignmentPreviewRequest(
            secondTrip.TripId,
            "driver",
            driverPersonId,
            null));
        var previewResponse = await _routarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<DispatchAssignmentPreviewResponse>())!;

        Assert.True(preview.HasBlockingConflicts);
        Assert.Single(preview.OverlappingTrips);
        Assert.Equal(firstTrip.TripId, preview.OverlappingTrips[0].TripId);
    }

    private async Task<TripDetailResponse> CreateTripAsync(
        string dispatcherToken,
        DateTimeOffset tripStart,
        DateTimeOffset tripEnd,
        string? vehicleRefKey = null)
    {
        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Assignment trip",
            "Drag-drop assignment test",
            vehicleRefKey,
            tripStart,
            tripEnd,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        return (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
    }

    private async Task AssignDriverAsync(string dispatcherToken, Guid tripId, string driverPersonId)
    {
        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{tripId}/assign-driver", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var statusRequest = Authorized(HttpMethod.Patch, $"/api/trips/{tripId}/status", dispatcherToken);
        statusRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("assigned"));
        (await _routarrClient.SendAsync(statusRequest)).EnsureSuccessStatusCode();
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
            $"{productKey}-assign-test",
            $"{productKey} Assign Test",
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
