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

public sealed class RoutArrRouteTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrRouteNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrRoute-{Guid.NewGuid():N}";

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
    public async Task Route_create_link_trip_ordered_stops_and_status_lifecycle()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Route test trip",
            "Linked route trip",
            null,
            null,
            null,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        var trip = (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var createRouteRequest = Authorized(HttpMethod.Post, "/api/routes", dispatcherToken);
        createRouteRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "North quarry loop",
            "Pickup then delivery",
            null,
            [
                new CreateRouteStopRequest("stop-1", "Quarry pickup", "North quarry gate", "pickup", 1, null),
                new CreateRouteStopRequest("stop-2", "Yard delivery", "South yard dock", "delivery", 2, null),
            ]));
        var createRouteResponse = await _routarrClient.SendAsync(createRouteRequest);
        createRouteResponse.EnsureSuccessStatusCode();
        var created = (await createRouteResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;
        Assert.Equal("draft", created.RouteStatus);
        Assert.Equal(2, created.Stops.Count);
        Assert.StartsWith("RT-", created.RouteNumber);
        Assert.Equal("stop-1", created.Stops[0].StopKey);
        Assert.Equal("stop-2", created.Stops[1].StopKey);

        var linkRequest = Authorized(HttpMethod.Patch, $"/api/routes/{created.RouteId}/link-trip", dispatcherToken);
        linkRequest.Content = JsonContent.Create(new LinkRouteTripRequest(trip.TripId));
        var linkResponse = await _routarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();
        var linked = (await linkResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;
        Assert.Equal(trip.TripId, linked.TripId);
        Assert.Equal("planned", linked.RouteStatus);

        var reorderRequest = Authorized(HttpMethod.Put, $"/api/routes/{created.RouteId}/stops/reorder", dispatcherToken);
        reorderRequest.Content = JsonContent.Create(new ReorderRouteStopsRequest(
            [linked.Stops[1].StopId, linked.Stops[0].StopId]));
        var reorderResponse = await _routarrClient.SendAsync(reorderRequest);
        reorderResponse.EnsureSuccessStatusCode();
        var reordered = (await reorderResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;
        Assert.Equal("stop-2", reordered.Stops[0].StopKey);
        Assert.Equal("stop-1", reordered.Stops[1].StopKey);

        var firstStopId = reordered.Stops[0].StopId;
        var arriveRequest = Authorized(HttpMethod.Patch, $"/api/stops/{firstStopId}/status", dispatcherToken);
        arriveRequest.Content = JsonContent.Create(new UpdateRouteStopStatusRequest("arrived"));
        var arriveResponse = await _routarrClient.SendAsync(arriveRequest);
        arriveResponse.EnsureSuccessStatusCode();
        var arrived = (await arriveResponse.Content.ReadFromJsonAsync<RouteStopSummaryResponse>())!;
        Assert.Equal("arrived", arrived.StopStatus);
        Assert.NotNull(arrived.ArrivedAt);

        var completeRequest = Authorized(HttpMethod.Patch, $"/api/stops/{firstStopId}/status", dispatcherToken);
        completeRequest.Content = JsonContent.Create(new UpdateRouteStopStatusRequest("completed"));
        var completeResponse = await _routarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<RouteStopSummaryResponse>())!;
        Assert.Equal("completed", completed.StopStatus);

        var listStopsRequest = Authorized(HttpMethod.Get, $"/api/stops?routeId={created.RouteId}", dispatcherToken);
        var listStopsResponse = await _routarrClient.SendAsync(listStopsRequest);
        listStopsResponse.EnsureSuccessStatusCode();
        var stops = (await listStopsResponse.Content.ReadFromJsonAsync<List<RouteStopSummaryResponse>>())!;
        Assert.Equal(2, stops.Count);

        using (var scope = _routarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
            var routeEvents = await db.IntegrationOutboxEvents
                .Where(x => x.TenantId == PlatformSeeder.DemoTenantId
                    && x.RelatedEntityId == created.RouteId)
                .ToListAsync();
            Assert.Contains(routeEvents, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.RouteCreated);
            Assert.True(
                routeEvents.Count(x => x.EventKind == RoutArrIntegrationOutboxEventKinds.RouteUpdated) >= 2);

            var stopEvents = await db.IntegrationOutboxEvents
                .Where(x => x.TenantId == PlatformSeeder.DemoTenantId
                    && x.RelatedEntityId == firstStopId)
                .ToListAsync();
            Assert.Contains(stopEvents, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.StopArrived);
            Assert.Contains(stopEvents, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.StopCompleted);
        }

        var listRoutesRequest = Authorized(HttpMethod.Get, $"/api/routes?tripId={trip.TripId}", dispatcherToken);
        var listRoutesResponse = await _routarrClient.SendAsync(listRoutesRequest);
        listRoutesResponse.EnsureSuccessStatusCode();
        var routes = (await listRoutesResponse.Content.ReadFromJsonAsync<List<RouteSummaryResponse>>())!;
        Assert.Single(routes);
        Assert.Equal(created.RouteId, routes[0].RouteId);
    }

    [Fact]
    public async Task Route_v1_alias_create_and_get_return_v1_location()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var createRouteRequest = Authorized(HttpMethod.Post, "/api/v1/routes", dispatcherToken);
        createRouteRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "V1 customer delivery loop",
            "Created through the documented v1 route surface.",
            null,
            [
                new CreateRouteStopRequest("pickup", "Pickup", "Warehouse", "pickup", 1, null),
                new CreateRouteStopRequest("delivery", "Delivery", "Customer", "delivery", 2, null),
            ]));
        var createRouteResponse = await _routarrClient.SendAsync(createRouteRequest);
        createRouteResponse.EnsureSuccessStatusCode();
        var created = (await createRouteResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;
        Assert.StartsWith($"/api/v1/routes/{created.RouteId}", createRouteResponse.Headers.Location?.OriginalString);

        var getRouteResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/routes/{created.RouteId}", dispatcherToken));
        getRouteResponse.EnsureSuccessStatusCode();
        var fetched = (await getRouteResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;
        Assert.Equal(created.RouteId, fetched.RouteId);
        Assert.Equal(2, fetched.Stops.Count);
    }

    [Fact]
    public async Task Route_template_v1_alias_create_list_and_get_use_unlinked_routes()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/v1/route-templates", dispatcherToken);
        createTemplateRequest.Content = JsonContent.Create(new CreateRouteTemplateRequest(
            "Reusable quarry loop",
            "Template for recurring aggregate pickup and yard delivery.",
            [
                new CreateRouteStopRequest("quarry", "Quarry pickup", "North quarry", "pickup", 1, null),
                new CreateRouteStopRequest("yard", "Yard delivery", "South yard", "delivery", 2, null),
            ]));
        var createTemplateResponse = await _routarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var created = (await createTemplateResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;
        Assert.Null(created.TripId);
        Assert.Equal("draft", created.RouteStatus);
        Assert.StartsWith($"/api/v1/route-templates/{created.RouteId}", createTemplateResponse.Headers.Location?.OriginalString);

        var listTemplatesResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/route-templates", dispatcherToken));
        listTemplatesResponse.EnsureSuccessStatusCode();
        var templates = (await listTemplatesResponse.Content.ReadFromJsonAsync<List<RouteSummaryResponse>>())!;
        Assert.Contains(templates, x => x.RouteId == created.RouteId && x.TripId is null && x.StopCount == 2);

        var getTemplateResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/route-templates/{created.RouteId}", dispatcherToken));
        getTemplateResponse.EnsureSuccessStatusCode();
        var fetched = (await getTemplateResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;
        Assert.Equal(created.RouteId, fetched.RouteId);
        Assert.Null(fetched.TripId);
        Assert.Equal(["quarry", "yard"], fetched.Stops.Select(x => x.StopKey).ToArray());

        var createTripRequest = Authorized(HttpMethod.Post, "/api/v1/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Template exclusion trip",
            "Trip used to ensure linked routes do not appear as templates.",
            null,
            null,
            null,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        var trip = (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var createLinkedRouteRequest = Authorized(HttpMethod.Post, "/api/v1/routes", dispatcherToken);
        createLinkedRouteRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "Linked operational route",
            "Should not be listed as a route template.",
            trip.TripId,
            [
                new CreateRouteStopRequest("linked", "Linked stop", "Depot", "depot", 1, null),
            ]));
        var createLinkedRouteResponse = await _routarrClient.SendAsync(createLinkedRouteRequest);
        createLinkedRouteResponse.EnsureSuccessStatusCode();
        var linkedRoute = (await createLinkedRouteResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;

        var refreshedTemplatesResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/route-templates", dispatcherToken));
        refreshedTemplatesResponse.EnsureSuccessStatusCode();
        var refreshedTemplates = (await refreshedTemplatesResponse.Content.ReadFromJsonAsync<List<RouteSummaryResponse>>())!;
        Assert.DoesNotContain(refreshedTemplates, x => x.RouteId == linkedRoute.RouteId);
    }

    [Fact]
    public async Task Route_create_denied_for_driver_role()
    {
        var token = CreateRoutArrAccessToken(["routarr"], "routarr_driver");
        var request = Authorized(HttpMethod.Post, "/api/routes", token);
        request.Content = JsonContent.Create(new CreateRouteRequest(
            "Denied route",
            string.Empty,
            null,
            null));

        var response = await _routarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Stop_cannot_complete_before_arrival()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var createRouteRequest = Authorized(HttpMethod.Post, "/api/routes", dispatcherToken);
        createRouteRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "Arrival guard route",
            string.Empty,
            null,
            [
                new CreateRouteStopRequest("stop-1", "Only stop", "Site A", "waypoint", 1, null),
            ]));
        var createRouteResponse = await _routarrClient.SendAsync(createRouteRequest);
        createRouteResponse.EnsureSuccessStatusCode();
        var created = (await createRouteResponse.Content.ReadFromJsonAsync<RouteDetailResponse>())!;

        var completeRequest = Authorized(
            HttpMethod.Patch,
            $"/api/stops/{created.Stops[0].StopId}/status",
            dispatcherToken);
        completeRequest.Content = JsonContent.Create(new UpdateRouteStopStatusRequest("completed"));
        var completeResponse = await _routarrClient.SendAsync(completeRequest);
        Assert.Equal(HttpStatusCode.BadRequest, completeResponse.StatusCode);
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
            $"{productKey}-route-test",
            $"{productKey} Route Test",
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
