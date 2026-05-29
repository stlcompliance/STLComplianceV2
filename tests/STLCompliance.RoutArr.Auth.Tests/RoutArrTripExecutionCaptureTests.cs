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
using STLCompliance.Shared.Integration;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrTripExecutionCaptureTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _dispatcherToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TripExecCaptureNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"TripExecCaptureRoutArr-{Guid.NewGuid():N}";

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
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Dispatcher_upserts_trip_execution_settings()
    {
        var putRequest = Authorized(HttpMethod.Put, "/api/trip-execution-settings", _dispatcherToken);
        putRequest.Content = JsonContent.Create(new UpsertTripExecutionSettingsRequest(
            RequirePreTripDvirBeforeStart: true,
            RequirePostTripDvirBeforeComplete: true,
            RequireDeliveryProofBeforeComplete: true,
            RequirePickupProofBeforeStart: false,
            BlockTripStartOnDvirFail: true,
            BlockTripCompleteOnDvirFail: true,
            RequirePickupProofPhotoBeforeStart: true,
            RequireDeliveryProofPhotoBeforeComplete: false,
            RequireDeliverySignatureBeforeComplete: true,
            RequirePreTripDvirPhotoBeforeStart: false,
            RequirePostTripDvirPhotoBeforeComplete: false));
        var putResponse = await _routarrClient.SendAsync(putRequest);
        putResponse.EnsureSuccessStatusCode();
        var saved = (await putResponse.Content.ReadFromJsonAsync<TripExecutionSettingsResponse>())!;
        Assert.True(saved.RequirePostTripDvirBeforeComplete);
        Assert.True(saved.RequireDeliveryProofBeforeComplete);
        Assert.True(saved.RequirePickupProofPhotoBeforeStart);
        Assert.True(saved.RequireDeliverySignatureBeforeComplete);

        var getResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/trip-execution-settings", _dispatcherToken));
        getResponse.EnsureSuccessStatusCode();
        var loaded = (await getResponse.Content.ReadFromJsonAsync<TripExecutionSettingsResponse>())!;
        Assert.True(loaded.RequirePostTripDvirBeforeComplete);
    }

    [Fact]
    public async Task Driver_start_blocked_until_pre_trip_dvir_submitted()
    {
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var trip = await CreateDispatchedTripAsync(driverPersonId);

        var startResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/start", driverToken));
        Assert.Equal(HttpStatusCode.Conflict, startResponse.StatusCode);

        var dvirRequest = Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/dvir", driverToken);
        dvirRequest.Content = JsonContent.Create(new SubmitTripDvirRequest(
            "pre_trip",
            trip.VehicleRefKey,
            "pass",
            1000,
            null));
        (await _routarrClient.SendAsync(dvirRequest)).EnsureSuccessStatusCode();

        var startAgain = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/start", driverToken));
        startAgain.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Driver_capture_readiness_lists_blockers()
    {
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var trip = await CreateDispatchedTripAsync(driverPersonId);

        var readinessResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/driver-portal/trips/{trip.TripId}/capture-readiness", driverToken));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<TripCaptureReadinessResponse>())!;
        Assert.False(readiness.CanStartTrip);
        Assert.Contains(readiness.Items, x => x.Key == "pre_trip_dvir" && !x.Satisfied);
    }

    [Fact]
    public async Task Dvir_fail_without_defect_notes_returns_400()
    {
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var trip = await CreateDispatchedTripAsync(PlatformSeeder.DemoAdminUserId.ToString());

        var dvirRequest = Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/dvir", driverToken);
        dvirRequest.Content = JsonContent.Create(new SubmitTripDvirRequest(
            "pre_trip",
            trip.VehicleRefKey,
            "fail",
            null,
            null));
        var response = await _routarrClient.SendAsync(dvirRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Dispatcher_reads_trip_capture_readiness_via_trips_api()
    {
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();
        var trip = await CreateDispatchedTripAsync(driverPersonId);

        var readinessResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/trips/{trip.TripId}/capture-readiness", _dispatcherToken));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<TripCaptureReadinessResponse>())!;
        Assert.Equal(trip.TripId, readiness.TripId);
        Assert.False(readiness.CanStartTrip);
        Assert.Contains(readiness.Items, x => x.Key == "pre_trip_dvir" && !x.Satisfied);
    }

    [Fact]
    public async Task Trip_audit_trail_lists_status_and_proof_events()
    {
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var trip = await CreateDispatchedTripAsync(driverPersonId);

        var proofRequest = Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/proofs", driverToken);
        proofRequest.Content = JsonContent.Create(new CreateTripProofRequest(
            "pickup",
            trip.VehicleRefKey,
            "BOL-1",
            "Audit trail test",
            null));
        (await _routarrClient.SendAsync(proofRequest)).EnsureSuccessStatusCode();

        var auditResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/trips/{trip.TripId}/audit-trail?limit=20", _dispatcherToken));
        auditResponse.EnsureSuccessStatusCode();
        var audit = (await auditResponse.Content.ReadFromJsonAsync<TripAuditTrailResponse>())!;
        Assert.Equal(trip.TripId, audit.TripId);
        Assert.Contains(audit.Entries, x => x.Action == "trip.status");
        Assert.Contains(audit.Entries, x => x.Action == "trip_proof.create");
    }

    private async Task<TripDetailResponse> CreateDispatchedTripAsync(string driverPersonId)
    {
        var now = DateTimeOffset.UtcNow;
        var createRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Capture depth trip",
            "Worker 257",
            "VEH-W257",
            now.AddHours(1),
            now.AddHours(5),
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", _dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var dispatchRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/status", _dispatcherToken);
        dispatchRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        (await _routarrClient.SendAsync(dispatchRequest)).EnsureSuccessStatusCode();

        return trip;
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
            $"{productKey}-trip-exec-capture",
            "trip exec capture test",
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
