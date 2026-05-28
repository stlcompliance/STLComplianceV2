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

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrTripCompletionRollupWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _sharedWorkerToRoutarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TripCompletionRollupNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"TripCompletionRollupRoutArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToRoutarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["routarr"],
            TripCompletionRollupWorkerService.ProcessTripCompletionRollupsActionScope);

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));
            });
        });

        _routarrClient = _routarrFactory.CreateClient();

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _routarrClient.PostAsJsonAsync(
            "/api/internal/trip-completion-rollups/process-batch",
            new ProcessTripCompletionRollupsRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 25, 1));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_materializes_completed_trip_rollup()
    {
        var tripId = await SeedCompletedTripAsync();
        await UpsertRollupSettingsAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/trip-completion-rollups/process-batch",
            _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessTripCompletionRollupsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            1));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessTripCompletionRollupsResponse>())!;
        Assert.Equal(1, body.RefreshedCount);
        Assert.Single(body.Refreshed);
        Assert.True(body.Refreshed[0].IsMaterialized);

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var rollup = await db.TripCompletionRollups.SingleAsync(x => x.TripId == tripId);
        Assert.Equal(TripDispatchStatuses.Completed, rollup.DispatchStatus);
        Assert.True(await db.TripCompletionEvents.AnyAsync(x => x.TripId == tripId));

        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var detailResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/trip-completions/{tripId}", adminToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<TripCompletionDetailResponse>())!;
        Assert.Equal(tripId, detail.Summary.TripId);
        Assert.True(detail.Summary.IsMaterialized);
        Assert.NotEmpty(detail.Events);

        var routeListResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/route-completions", adminToken));
        routeListResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Pending_preview_lists_terminal_trips_without_rollups()
    {
        await SeedCompletedTripAsync();
        await UpsertRollupSettingsAsync();

        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var pendingResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/trip-completion-rollup-settings/pending", adminToken));
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingTripCompletionRollupsResponse>())!;
        Assert.NotEmpty(pending.Items);
    }

    [Fact]
    public async Task Rollup_settings_requires_admin()
    {
        var dispatcherToken = CreateRoutArrAccessToken(["routarr"], "routarr_dispatcher");
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/trip-completion-rollup-settings", dispatcherToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> SeedCompletedTripAsync()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid().ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Rollup test trip",
            "Completion rollup seed",
            "VEH-ROLLUP-1",
            DateTimeOffset.UtcNow.AddHours(-4),
            DateTimeOffset.UtcNow.AddHours(-1),
            [
                new CreateTripLoadRequest(
                    "load-rollup",
                    "Test load",
                    "delivery",
                    1,
                    "Origin yard",
                    "Destination yard"),
            ]));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId, false, false, false));
        await _routarrClient.SendAsync(assignRequest);

        foreach (var status in new[] { "dispatched", "in_progress", "completed" })
        {
            var statusRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/status", adminToken);
            statusRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest(status));
            var statusResponse = await _routarrClient.SendAsync(statusRequest);
            statusResponse.EnsureSuccessStatusCode();
        }

        var routeRequest = Authorized(HttpMethod.Post, "/api/routes", adminToken);
        routeRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "Rollup route",
            "Route for rollup test",
            created.TripId,
            [
                new CreateRouteStopRequest("stop-1", "Pickup", "123 Main St", "pickup", 1, null),
                new CreateRouteStopRequest("stop-2", "Delivery", "456 Oak Ave", "delivery", 2, null),
            ]));
        var routeResponse = await _routarrClient.SendAsync(routeRequest);
        routeResponse.EnsureSuccessStatusCode();
        var route = (await routeResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var routeEntity = await db.Routes.Include(x => x.Stops).SingleAsync(x => x.Id == route.RouteId);
        routeEntity.RouteStatus = RouteStatuses.Completed;
        routeEntity.CompletedAt = DateTimeOffset.UtcNow;
        foreach (var stop in routeEntity.Stops.OrderBy(x => x.SequenceNumber))
        {
            stop.StopStatus = RouteStopStatuses.Completed;
            stop.CompletedAt = DateTimeOffset.UtcNow;
        }

        var tripEntity = await db.Trips.Include(x => x.Loads).SingleAsync(x => x.Id == created.TripId);
        foreach (var load in tripEntity.Loads)
        {
            load.Status = TripLoadStatuses.Delivered;
        }

        await db.SaveChangesAsync();
        return created.TripId;
    }

    private async Task UpsertRollupSettingsAsync()
    {
        var token = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/trip-completion-rollup-settings", token);
        request.Content = JsonContent.Create(new UpsertTripCompletionRollupSettingsRequest(true, 1));
        var response = await _routarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private string CreateRoutArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _routarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RoutArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-rollup-{Guid.NewGuid():N}",
            $"{sourceProduct} rollup test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            allowedProducts,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<TContext>)
            || d.ServiceType == typeof(TContext)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
