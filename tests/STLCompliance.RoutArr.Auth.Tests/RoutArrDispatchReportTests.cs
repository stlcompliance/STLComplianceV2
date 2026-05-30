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

public sealed class RoutArrDispatchReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _dispatcherToken = null!;
    private Guid _tripId;
    private Guid _exceptionId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"DispatchReportNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"DispatchReportRoutArr-{Guid.NewGuid():N}";

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
        (_tripId, _exceptionId) = await SeedDispatchReportDataAsync();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Dispatch_report_summary_returns_trip_and_delay_metrics()
    {
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/dispatch/summary?scope=daily", _dispatcherToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<DispatchReportSummaryResponse>())!;
        Assert.True(summary.TotalTripCount >= 1);
        Assert.Contains(summary.Trips, x => x.TripId == _tripId);
        Assert.True(summary.DelayExceptionCount >= 1);
        Assert.Contains(summary.ExceptionCategoryCounts, x =>
            string.Equals(x.Key, DispatchExceptionCategories.Delay, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Dispatch_report_trip_and_exception_detail()
    {
        var tripResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/reports/dispatch/trips/{_tripId:D}", _dispatcherToken));
        tripResponse.EnsureSuccessStatusCode();
        var trip = (await tripResponse.Content.ReadFromJsonAsync<DispatchReportTripDetailResponse>())!;
        Assert.Equal(_tripId, trip.TripId);
        Assert.True(trip.DelayExceptionCount >= 1);

        var exceptionResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/reports/dispatch/exceptions/{_exceptionId:D}", _dispatcherToken));
        exceptionResponse.EnsureSuccessStatusCode();
        var exception = (await exceptionResponse.Content.ReadFromJsonAsync<DispatchReportExceptionDetailResponse>())!;
        Assert.Equal(_exceptionId, exception.ExceptionId);
        Assert.Equal(DispatchExceptionCategories.Delay, exception.Category);
        Assert.Equal(_tripId, exception.TripId);
    }

    [Fact]
    public async Task Dispatch_report_summary_export_returns_csv()
    {
        var managerToken = CreateRoutArrAccessToken(["routarr"], "routarr_manager");
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/dispatch/summary/export?scope=daily", managerToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("tripNumber,title", csv, StringComparison.Ordinal);
        Assert.Contains("Report trip", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Dispatch_report_export_denied_for_driver_role()
    {
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver");
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/dispatch/summary/export", driverToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Dispatch_report_summary_denies_driver_role()
    {
        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver");
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/dispatch/summary", driverToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<(Guid TripId, Guid ExceptionId)> SeedDispatchReportDataAsync()
    {
        var now = DateTimeOffset.UtcNow;

        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Report trip",
            "Dispatch report seed",
            "VEH-RPT",
            now.AddHours(1),
            now.AddHours(4),
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        var trip = (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var createExceptionRequest = Authorized(HttpMethod.Post, "/api/dispatch/exceptions", _dispatcherToken);
        createExceptionRequest.Content = JsonContent.Create(new CreateDispatchExceptionRequest(
            "Highway delay",
            "Weather-related slowdown",
            DispatchExceptionCategories.Delay,
            trip.TripId,
            null,
            null));
        var createExceptionResponse = await _routarrClient.SendAsync(createExceptionRequest);
        createExceptionResponse.EnsureSuccessStatusCode();
        var exception = (await createExceptionResponse.Content.ReadFromJsonAsync<DispatchExceptionSummaryResponse>())!;

        return (trip.TripId, exception.ExceptionId);
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
            $"{productKey}-dispatch-report",
            "dispatch report test",
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
