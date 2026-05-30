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
using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;
using STLCompliance.Shared.Auth;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrDriverPortalTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _dispatcherToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"DriverPortalNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"DriverPortalRoutArr-{Guid.NewGuid():N}";

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
    public async Task Driver_portal_schedule_and_execution_lifecycle()
    {
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var now = DateTimeOffset.UtcNow;

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Driver portal trip",
            "Assigned to demo admin person",
            "VEH-DP-1",
            now.AddHours(1),
            now.AddHours(5),
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", _dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId, DriverDisplayName: "Demo Driver"));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var scheduleResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/driver-portal/schedule", driverToken));
        scheduleResponse.EnsureSuccessStatusCode();
        var schedule = (await scheduleResponse.Content.ReadFromJsonAsync<DriverPortalScheduleResponse>())!;
        Assert.Contains(schedule.TodayTrips, x => x.TripId == trip.TripId && x.CanDispatch);

        var dispatchResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/dispatch", driverToken));
        dispatchResponse.EnsureSuccessStatusCode();
        var dispatched = (await dispatchResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal("dispatched", dispatched.DispatchStatus);

        var settingsRequest = Authorized(HttpMethod.Put, "/api/trip-execution-settings", _dispatcherToken);
        settingsRequest.Content = JsonContent.Create(new UpsertTripExecutionSettingsRequest(
            RequirePreTripDvirBeforeStart: false,
            RequirePostTripDvirBeforeComplete: false,
            RequireDeliveryProofBeforeComplete: false,
            RequirePickupProofBeforeStart: false,
            BlockTripStartOnDvirFail: true,
            BlockTripCompleteOnDvirFail: true,
            RequirePickupProofPhotoBeforeStart: false,
            RequireDeliveryProofPhotoBeforeComplete: false,
            RequireDeliverySignatureBeforeComplete: false,
            RequirePreTripDvirPhotoBeforeStart: false,
            RequirePostTripDvirPhotoBeforeComplete: false));
        (await _routarrClient.SendAsync(settingsRequest)).EnsureSuccessStatusCode();

        var startResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/start", driverToken));
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal("in_progress", started.DispatchStatus);

        var completeResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/complete", driverToken));
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal("completed", completed.DispatchStatus);
        Assert.Null(completed.ClosedAt);

        var scheduleAfterComplete = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/driver-portal/schedule", driverToken));
        scheduleAfterComplete.EnsureSuccessStatusCode();
        var afterComplete = (await scheduleAfterComplete.Content.ReadFromJsonAsync<DriverPortalScheduleResponse>())!;
        var pendingClose = afterComplete.TodayTrips.Single(x => x.TripId == trip.TripId);
        Assert.True(pendingClose.CanClose);
        Assert.False(pendingClose.CanComplete);

        var closeResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/close", driverToken));
        closeResponse.EnsureSuccessStatusCode();
        var closed = (await closeResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal("completed", closed.DispatchStatus);
        Assert.NotNull(closed.ClosedAt);

        var scheduleAfterClose = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/driver-portal/schedule", driverToken));
        scheduleAfterClose.EnsureSuccessStatusCode();
        var afterClose = (await scheduleAfterClose.Content.ReadFromJsonAsync<DriverPortalScheduleResponse>())!;
        Assert.DoesNotContain(afterClose.TodayTrips, x => x.TripId == trip.TripId);
        Assert.DoesNotContain(afterClose.UpcomingTrips, x => x.TripId == trip.TripId);

        var executionResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/trips/{trip.TripId}/execution", _dispatcherToken));
        executionResponse.EnsureSuccessStatusCode();
        var execution = (await executionResponse.Content.ReadFromJsonAsync<TripExecutionSummaryResponse>())!;
        Assert.Equal("completed", execution.DispatchStatus);
        Assert.NotNull(execution.ClosedAt);
    }

    [Fact]
    public async Task Driver_portal_reports_exception_on_assigned_trip()
    {
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var now = DateTimeOffset.UtcNow;

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Exception report trip",
            "Driver exception slice",
            "VEH-EX-1",
            now.AddHours(1),
            now.AddHours(4),
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", _dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var reportRequest = Authorized(
            HttpMethod.Post,
            $"/api/driver-portal/trips/{trip.TripId}/exceptions",
            driverToken);
        reportRequest.Content = JsonContent.Create(new DriverPortalReportExceptionRequest(
            "Traffic on I-40",
            "Heavy congestion past mile 12",
            DriverPortalExceptionRules.TrafficDelay));
        var reportResponse = await _routarrClient.SendAsync(reportRequest);
        reportResponse.EnsureSuccessStatusCode();
        var reported = (await reportResponse.Content.ReadFromJsonAsync<DispatchExceptionSummaryResponse>())!;
        Assert.Equal(trip.TripId, reported.TripId);
        Assert.Equal(DispatchExceptionCategories.Delay, reported.Category);
        Assert.Equal(DispatchExceptionStatuses.Open, reported.Status);
        Assert.StartsWith("[Driver-reported]", reported.Description, StringComparison.Ordinal);

        var queueResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/exceptions?status=open", _dispatcherToken));
        queueResponse.EnsureSuccessStatusCode();
        var queue = (await queueResponse.Content.ReadFromJsonAsync<DispatchExceptionListResponse>())!;
        Assert.Contains(queue.Items, x => x.ExceptionId == reported.ExceptionId);

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var outbox = await db.IntegrationOutboxEvents
            .AsNoTracking()
            .SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.EventKind == RoutArrIntegrationOutboxEventKinds.ExceptionCreated
                && x.RelatedEntityId == reported.ExceptionId);
        Assert.Equal("dispatch_exception", outbox.RelatedEntityType);
    }

    [Fact]
    public async Task Driver_portal_rejects_execution_for_unassigned_trip()
    {
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var otherDriverId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Other driver trip",
            "Not for demo admin",
            null,
            now.AddHours(2),
            now.AddHours(6),
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", _dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(otherDriverId));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var dispatchRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/status", _dispatcherToken);
        dispatchRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        (await _routarrClient.SendAsync(dispatchRequest)).EnsureSuccessStatusCode();

        var startResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/driver-portal/trips/{trip.TripId}/start", driverToken));
        Assert.Equal(HttpStatusCode.Forbidden, startResponse.StatusCode);
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
            $"{productKey}-driver-portal",
            "driver portal test",
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
