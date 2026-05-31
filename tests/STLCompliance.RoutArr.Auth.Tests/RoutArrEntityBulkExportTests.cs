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

public sealed class RoutArrEntityBulkExportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _managerToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"EntityExportNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"EntityExportRoutArr-{Guid.NewGuid():N}";

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
        _managerToken = CreateRoutArrAccessToken(["routarr"], "routarr_manager");
        await SeedExportDataAsync();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Entity_export_manifest_lists_entities_and_reports()
    {
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", _managerToken));
        response.EnsureSuccessStatusCode();

        var manifest = (await response.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Contains(manifest.Entities, x => x.EntityKey == "trips");
        Assert.Contains(manifest.Entities, x => x.EntityKey == "routes");
        Assert.Contains(manifest.Entities, x => x.EntityKey == "dispatch_exceptions");
        Assert.Contains(manifest.ReportExports, x => x.ReportKey == "dispatch");
        Assert.Contains(manifest.ReportExports, x => x.ReportKey == "time_summary");
        Assert.Contains(manifest.ReportExports, x => x.ReportKey == "routes_report");
    }

    [Fact]
    public async Task Entity_export_v1_manifest_and_csv_aliases_work()
    {
        var manifestResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/manifest", _managerToken));
        manifestResponse.EnsureSuccessStatusCode();

        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Contains(manifest.Entities, x => x.EntityKey == "trips" && x.Route == "/api/v1/exports/trips");
        Assert.Contains(manifest.ReportExports, x => x.ReportKey == "dispatch" && x.Route == "/api/v1/reports/dispatch/summary/export");
        Assert.Contains(manifest.ReportExports, x => x.ReportKey == "time_summary" && x.Route == "/api/v1/reports/dispatch/time-summary/export");

        var tripsResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/trips", _managerToken));
        tripsResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", tripsResponse.Content.Headers.ContentType?.MediaType);
        var tripsCsv = await tripsResponse.Content.ReadAsStringAsync();
        Assert.Contains(RoutArrEntityBulkExportService.TripsCsvHeader, tripsCsv, StringComparison.Ordinal);
        Assert.Contains("Export bulk trip", tripsCsv, StringComparison.Ordinal);

        var routesResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/routes", _managerToken));
        routesResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", routesResponse.Content.Headers.ContentType?.MediaType);
        var routesCsv = await routesResponse.Content.ReadAsStringAsync();
        Assert.Contains(RoutArrEntityBulkExportService.RoutesCsvHeader, routesCsv, StringComparison.Ordinal);
        Assert.Contains("Export bulk route", routesCsv, StringComparison.Ordinal);

        var exceptionsResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/dispatch-exceptions?status=open", _managerToken));
        exceptionsResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exceptionsResponse.Content.Headers.ContentType?.MediaType);
        var exceptionsCsv = await exceptionsResponse.Content.ReadAsStringAsync();
        Assert.Contains(RoutArrEntityBulkExportService.DispatchExceptionsCsvHeader, exceptionsCsv, StringComparison.Ordinal);
        Assert.Contains("Export delay", exceptionsCsv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Entity_export_trips_csv_includes_seeded_trip()
    {
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/trips", _managerToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("tripNumber,title", csv, StringComparison.Ordinal);
        Assert.Contains("Export bulk trip", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Entity_export_routes_csv_includes_seeded_route()
    {
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/routes", _managerToken));
        response.EnsureSuccessStatusCode();

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("routeNumber,title", csv, StringComparison.Ordinal);
        Assert.Contains("Export bulk route", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Entity_export_dispatch_exceptions_csv_includes_delay_row()
    {
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/dispatch-exceptions", _managerToken));
        response.EnsureSuccessStatusCode();

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("exceptionKey,title", csv, StringComparison.Ordinal);
        Assert.Contains("Export delay", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Entity_export_denies_driver_role()
    {
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver");
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/trips", driverToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task SeedExportDataAsync()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Export bulk trip",
            "Entity export seed",
            null,
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(4),
            null));
        var tripResponse = await _routarrClient.SendAsync(createTripRequest);
        tripResponse.EnsureSuccessStatusCode();
        var trip = (await tripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var createRouteRequest = Authorized(HttpMethod.Post, "/api/routes", dispatcherToken);
        createRouteRequest.Content = JsonContent.Create(new CreateRouteRequest(
            "Export bulk route",
            "Route export seed",
            trip.TripId,
            [new CreateRouteStopRequest("stop-x", "Stop", "Addr", "pickup", 1, null)]));
        (await _routarrClient.SendAsync(createRouteRequest)).EnsureSuccessStatusCode();

        var createExceptionRequest = Authorized(HttpMethod.Post, "/api/dispatch/exceptions", dispatcherToken);
        createExceptionRequest.Content = JsonContent.Create(new CreateDispatchExceptionRequest(
            "Export delay",
            "Bulk export exception",
            DispatchExceptionCategories.Delay,
            trip.TripId,
            null,
            null));
        (await _routarrClient.SendAsync(createExceptionRequest)).EnsureSuccessStatusCode();
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
            $"{productKey}-entity-export",
            "entity export test",
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
