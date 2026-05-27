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

public sealed class RoutArrTripTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrTripNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrTrip-{Guid.NewGuid():N}";

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
    public async Task Trip_create_assign_driver_and_status_lifecycle()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var driverPersonId = Guid.NewGuid().ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "North yard delivery",
            "Two-stop gravel haul",
            "VEH-FL-100",
            DateTimeOffset.UtcNow.AddHours(2),
            DateTimeOffset.UtcNow.AddHours(6),
            [
                new CreateTripLoadRequest(
                    "load-1",
                    "Gravel pickup",
                    "pickup",
                    1,
                    "North quarry",
                    "South yard"),
            ]));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal("planned", created.DispatchStatus);
        Assert.Single(created.Loads);
        Assert.StartsWith("TR-", created.TripNumber);

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId));
        var assignResponse = await _routarrClient.SendAsync(assignRequest);
        assignResponse.EnsureSuccessStatusCode();
        var assigned = (await assignResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal("assigned", assigned.DispatchStatus);
        Assert.Equal(driverPersonId, assigned.AssignedDriverPersonId);

        var dispatchRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/status", dispatcherToken);
        dispatchRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        var dispatchResponse = await _routarrClient.SendAsync(dispatchRequest);
        dispatchResponse.EnsureSuccessStatusCode();
        var dispatched = (await dispatchResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal("dispatched", dispatched.DispatchStatus);
        Assert.NotNull(dispatched.DispatchedAt);

        var startRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/status", dispatcherToken);
        startRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("in_progress"));
        var startResponse = await _routarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var inProgress = (await startResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal("in_progress", inProgress.DispatchStatus);
        Assert.NotNull(inProgress.StartedAt);

        var completeRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/status", dispatcherToken);
        completeRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("completed"));
        var completeResponse = await _routarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal("completed", completed.DispatchStatus);
        Assert.NotNull(completed.CompletedAt);

        var listRequest = Authorized(HttpMethod.Get, "/api/trips?dispatchStatus=completed", dispatcherToken);
        var listResponse = await _routarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var trips = (await listResponse.Content.ReadFromJsonAsync<List<TripSummaryResponse>>())!;
        Assert.Contains(trips, x => x.TripId == created.TripId);
    }

    [Fact]
    public async Task Trip_create_denied_for_driver_role()
    {
        var token = CreateRoutArrAccessToken(["routarr"], "routarr_driver");
        var request = Authorized(HttpMethod.Post, "/api/trips", token);
        request.Content = JsonContent.Create(new CreateTripRequest(
            "Denied trip",
            string.Empty,
            null,
            null,
            null,
            null));

        var response = await _routarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Driver_cannot_cancel_trip()
    {
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var driverPersonId = PlatformSeeder.DemoAdminUserId.ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Cancel guard trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(driverPersonId));
        await _routarrClient.SendAsync(assignRequest);

        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", PlatformSeeder.DemoAdminUserId);
        var cancelRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/status", driverToken);
        cancelRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("cancelled"));
        var cancelResponse = await _routarrClient.SendAsync(cancelRequest);
        Assert.Equal(HttpStatusCode.Forbidden, cancelResponse.StatusCode);
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
            $"{productKey}-trip-test",
            $"{productKey} Trip Test",
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
