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

public sealed class RoutArrTripProofDvirTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _dispatcherToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TripProofDvirNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"TripProofDvirRoutArr-{Guid.NewGuid():N}";

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
    public async Task Driver_captures_proof_and_dvir_dispatcher_reads_execution()
    {
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var now = DateTimeOffset.UtcNow;

        var trip = await CreateAssignedTripAsync(driverPersonId, now);

        var proofRequest = Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/proofs", driverToken);
        proofRequest.Content = JsonContent.Create(new CreateTripProofRequest(
            "pickup",
            trip.VehicleRefKey,
            "BOL-1001",
            "Signed at dock",
            null));
        var proofResponse = await _routarrClient.SendAsync(proofRequest);
        proofResponse.EnsureSuccessStatusCode();
        var proof = (await proofResponse.Content.ReadFromJsonAsync<TripProofRecordResponse>())!;
        Assert.Equal("pickup", proof.ProofType);
        Assert.Equal("BOL-1001", proof.ReferenceKey);

        var preDvirRequest = Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/dvir", driverToken);
        preDvirRequest.Content = JsonContent.Create(new SubmitTripDvirRequest(
            "pre_trip",
            trip.VehicleRefKey,
            "pass",
            42100,
            null));
        (await _routarrClient.SendAsync(preDvirRequest)).EnsureSuccessStatusCode();

        var postDvirRequest = Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/dvir", driverToken);
        postDvirRequest.Content = JsonContent.Create(new SubmitTripDvirRequest(
            "post_trip",
            trip.VehicleRefKey,
            "conditional",
            42155,
            "Minor tire wear noted"));
        (await _routarrClient.SendAsync(postDvirRequest)).EnsureSuccessStatusCode();

        var dispatcherSummaryResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/trips/{trip.TripId}/execution", _dispatcherToken));
        dispatcherSummaryResponse.EnsureSuccessStatusCode();
        var summary = (await dispatcherSummaryResponse.Content.ReadFromJsonAsync<TripExecutionSummaryResponse>())!;
        Assert.True(summary.HasPreTripDvir);
        Assert.True(summary.HasPostTripDvir);
        Assert.Single(summary.Proofs);
        Assert.Equal(2, summary.DvirInspections.Count);

        var listProofsResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/trips/{trip.TripId}/proofs", _dispatcherToken));
        listProofsResponse.EnsureSuccessStatusCode();
        var proofList = (await listProofsResponse.Content.ReadFromJsonAsync<TripProofListResponse>())!;
        Assert.Single(proofList.Items);
    }

    [Fact]
    public async Task Driver_proof_write_rejected_for_unassigned_trip()
    {
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var otherDriverId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;
        var trip = await CreateAssignedTripAsync(otherDriverId, now);

        var proofRequest = Authorized(HttpMethod.Post, $"/api/trips/{trip.TripId}/proofs", driverToken);
        proofRequest.Content = JsonContent.Create(new CreateTripProofRequest("delivery", null, "POD-1", null, null));
        var proofResponse = await _routarrClient.SendAsync(proofRequest);
        Assert.Equal(HttpStatusCode.Forbidden, proofResponse.StatusCode);
    }

    [Fact]
    public async Task Driver_schedule_includes_proof_and_dvir_flags()
    {
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var now = DateTimeOffset.UtcNow;
        var trip = await CreateAssignedTripAsync(driverPersonId, now);

        var proofRequest = Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/proofs", driverToken);
        proofRequest.Content = JsonContent.Create(new CreateTripProofRequest("pickup", null, "REF-1", null, null));
        (await _routarrClient.SendAsync(proofRequest)).EnsureSuccessStatusCode();

        var dvirRequest = Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/dvir", driverToken);
        dvirRequest.Content = JsonContent.Create(new SubmitTripDvirRequest("pre_trip", null, "pass", null, null));
        (await _routarrClient.SendAsync(dvirRequest)).EnsureSuccessStatusCode();

        var scheduleResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/driver-portal/schedule", driverToken));
        scheduleResponse.EnsureSuccessStatusCode();
        var schedule = (await scheduleResponse.Content.ReadFromJsonAsync<DriverPortalScheduleResponse>())!;
        var row = schedule.TodayTrips.Concat(schedule.UpcomingTrips).First(x => x.TripId == trip.TripId);
        Assert.Equal(1, row.ProofCount);
        Assert.True(row.HasPreTripDvir);
        Assert.False(row.HasPostTripDvir);
    }

    private async Task<TripDetailResponse> CreateAssignedTripAsync(string driverPersonId, DateTimeOffset now)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Proof DVIR trip",
            "Worker 217 test",
            "VEH-W217",
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
            $"{productKey}-proof-dvir",
            "proof dvir test",
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
