using STLCompliance.Shared.Integration;
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

public sealed class RoutArrDispatchExceptionQueueTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _dispatcherToken = null!;
    private Guid _dispatcherUserId = PlatformSeeder.DemoAdminUserId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ExQueueNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"ExQueueRoutArr-{Guid.NewGuid():N}";

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
    public async Task Exception_queue_triage_lifecycle_with_trip_link()
    {
        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", _dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Exception trip",
            "Linked from queue test",
            null,
            null,
            null,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        var trip = (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var createExRequest = Authorized(HttpMethod.Post, "/api/dispatch/exceptions", _dispatcherToken);
        createExRequest.Content = JsonContent.Create(new CreateDispatchExceptionRequest(
            "Late departure",
            "Driver delayed at yard",
            "delay",
            null));
        var createExResponse = await _routarrClient.SendAsync(createExRequest);
        createExResponse.EnsureSuccessStatusCode();
        var created = (await createExResponse.Content.ReadFromJsonAsync<DispatchExceptionSummaryResponse>())!;
        Assert.Equal("open", created.Status);
        Assert.StartsWith("DEX-", created.ExceptionKey, StringComparison.Ordinal);

        var listResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/exceptions?status=open", _dispatcherToken));
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<DispatchExceptionListResponse>())!;
        Assert.True(list.OpenCount >= 1);
        Assert.Contains(list.Items, x => x.ExceptionId == created.ExceptionId);

        var assignRequest = Authorized(
            HttpMethod.Patch,
            $"/api/dispatch/exceptions/{created.ExceptionId}/assign",
            _dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignDispatchExceptionRequest(_dispatcherUserId));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var linkRequest = Authorized(
            HttpMethod.Patch,
            $"/api/dispatch/exceptions/{created.ExceptionId}/link-trip",
            _dispatcherToken);
        linkRequest.Content = JsonContent.Create(new LinkDispatchExceptionTripRequest(trip.TripId));
        var linkResponse = await _routarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();
        var linked = (await linkResponse.Content.ReadFromJsonAsync<DispatchExceptionSummaryResponse>())!;
        Assert.Equal(trip.TripId, linked.TripId);
        Assert.Equal(trip.TripNumber, linked.TripNumber);

        var resolveRequest = Authorized(
            HttpMethod.Patch,
            $"/api/dispatch/exceptions/{created.ExceptionId}/resolve",
            _dispatcherToken);
        resolveRequest.Content = JsonContent.Create(new ResolveDispatchExceptionRequest("Departed after delay"));
        var resolveResponse = await _routarrClient.SendAsync(resolveRequest);
        resolveResponse.EnsureSuccessStatusCode();
        var resolved = (await resolveResponse.Content.ReadFromJsonAsync<DispatchExceptionSummaryResponse>())!;
        Assert.Equal("resolved", resolved.Status);
        Assert.NotNull(resolved.ResolvedAt);

        var openListResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dispatch/exceptions?status=open", _dispatcherToken));
        openListResponse.EnsureSuccessStatusCode();
        var openList = (await openListResponse.Content.ReadFromJsonAsync<DispatchExceptionListResponse>())!;
        Assert.DoesNotContain(openList.Items, x => x.ExceptionId == created.ExceptionId);
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
            $"{productKey}-ex-queue",
            "exception queue test",
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
