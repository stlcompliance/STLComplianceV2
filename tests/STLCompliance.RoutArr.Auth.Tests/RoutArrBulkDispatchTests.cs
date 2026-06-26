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

public sealed class RoutArrBulkDispatchTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrBulkNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrBulk-{Guid.NewGuid():N}";

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
    public async Task Bulk_preview_detects_driver_availability_conflict()
    {
        var dispatcherToken = CreateRoutArrAccessToken(["routarr"], "tenant_admin");
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

        var previewRequest = Authorized(HttpMethod.Post, "/api/dispatch/bulk/preview", dispatcherToken);
        previewRequest.Content = JsonContent.Create(new BulkDispatchPreviewRequest([
            new BulkDispatchActionItem(trip.TripId, driverPersonId, null, null),
        ]));
        var previewResponse = await _routarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<BulkDispatchPreviewResponse>())!;

        Assert.Equal(1, preview.Summary.Total);
        Assert.Equal(0, preview.Summary.CanApplyCount);
        Assert.Equal(1, preview.Summary.BlockedCount);
        Assert.False(preview.Items[0].CanApply);
        Assert.NotNull(preview.Items[0].DriverPreview);
        Assert.True(preview.Items[0].DriverPreview!.HasBlockingConflicts);
    }

    [Fact]
    public async Task Bulk_apply_assigns_driver_with_override_and_status()
    {
        var dispatcherToken = CreateRoutArrAccessToken(["routarr"], "tenant_admin");
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

        var applyRequest = Authorized(HttpMethod.Post, "/api/dispatch/bulk/apply", dispatcherToken);
        applyRequest.Content = JsonContent.Create(new BulkDispatchApplyRequest([
            new BulkDispatchActionItem(trip.TripId, driverPersonId, null, "assigned"),
        ], IgnoreAvailabilityConflicts: true));
        var applyResponse = await _routarrClient.SendAsync(applyRequest);
        applyResponse.EnsureSuccessStatusCode();
        var applied = (await applyResponse.Content.ReadFromJsonAsync<BulkDispatchApplyResponse>())!;

        Assert.Equal(1, applied.Summary.SuccessCount);
        Assert.Equal("assigned", applied.Results[0].Trip!.DispatchStatus);
        Assert.Equal(driverPersonId, applied.Results[0].Trip!.AssignedDriverPersonId);
    }

    [Fact]
    public async Task Bulk_preview_detects_intra_batch_driver_overlap()
    {
        var dispatcherToken = CreateRoutArrAccessToken(["routarr"], "tenant_admin");
        var driverPersonId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;
        var tripStart = now.AddHours(2);
        var tripEnd = now.AddHours(6);

        var firstTrip = await CreateTripAsync(dispatcherToken, tripStart, tripEnd);
        var secondTrip = await CreateTripAsync(dispatcherToken, tripStart.AddHours(1), tripEnd.AddHours(1));

        var previewRequest = Authorized(HttpMethod.Post, "/api/dispatch/bulk/preview", dispatcherToken);
        previewRequest.Content = JsonContent.Create(new BulkDispatchPreviewRequest([
            new BulkDispatchActionItem(firstTrip.TripId, driverPersonId, null, null),
            new BulkDispatchActionItem(secondTrip.TripId, driverPersonId, null, null),
        ]));
        var previewResponse = await _routarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<BulkDispatchPreviewResponse>())!;

        Assert.True(preview.Items[0].CanApply);
        Assert.False(preview.Items[1].CanApply);
        Assert.Single(preview.Items[1].DriverPreview!.OverlappingTrips);
        Assert.Equal(firstTrip.TripId, preview.Items[1].DriverPreview!.OverlappingTrips[0].TripId);
    }

    [Fact]
    public async Task Bulk_apply_without_override_fails_blocked_item()
    {
        var dispatcherToken = CreateRoutArrAccessToken(["routarr"], "tenant_admin");
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

        var applyRequest = Authorized(HttpMethod.Post, "/api/dispatch/bulk/apply", dispatcherToken);
        applyRequest.Content = JsonContent.Create(new BulkDispatchApplyRequest([
            new BulkDispatchActionItem(trip.TripId, driverPersonId, null, null),
        ]));
        var applyResponse = await _routarrClient.SendAsync(applyRequest);
        applyResponse.EnsureSuccessStatusCode();
        var applied = (await applyResponse.Content.ReadFromJsonAsync<BulkDispatchApplyResponse>())!;

        Assert.Equal(0, applied.Summary.SuccessCount);
        Assert.Equal(1, applied.Summary.FailureCount);
        Assert.Equal("dispatch.assignment_blocked", applied.Results[0].ErrorCode);
    }

    private async Task<TripDetailResponse> CreateTripAsync(
        string dispatcherToken,
        DateTimeOffset tripStart,
        DateTimeOffset tripEnd,
        string? vehicleRefKey = null)
    {
        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Bulk dispatch trip",
            "Bulk dispatch test",
            vehicleRefKey,
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
            $"{productKey}-bulk-test",
            $"{productKey} Bulk Test",
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
