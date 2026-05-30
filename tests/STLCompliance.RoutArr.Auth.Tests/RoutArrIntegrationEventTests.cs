using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
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

public sealed class RoutArrIntegrationEventTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _sharedWorkerToRoutarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrIntegrationEventNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrIntegrationEventRoutArr-{Guid.NewGuid():N}";

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
            IntegrationEventProcessingService.ProcessEventsActionScope);

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
    public async Task Trip_assign_dispatch_complete_enqueues_integration_outbox_events()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid().ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Integration event trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId,
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var dispatchRequest = Authorized(
            HttpMethod.Patch,
            $"/api/trips/{created.TripId}/status",
            adminToken);
        dispatchRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        (await _routarrClient.SendAsync(dispatchRequest)).EnsureSuccessStatusCode();

        var startRequest = Authorized(
            HttpMethod.Patch,
            $"/api/trips/{created.TripId}/status",
            adminToken);
        startRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("in_progress"));
        (await _routarrClient.SendAsync(startRequest)).EnsureSuccessStatusCode();

        var completeRequest = Authorized(
            HttpMethod.Patch,
            $"/api/trips/{created.TripId}/status",
            adminToken);
        completeRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("completed"));
        (await _routarrClient.SendAsync(completeRequest)).EnsureSuccessStatusCode();

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var events = await db.IntegrationOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == created.TripId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        Assert.Equal(3, events.Count);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.DriverAssignmentChanged);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.TripDispatched);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.TripCompleted);
        Assert.All(events, x => Assert.Equal(IntegrationEventStatuses.Pending, x.ProcessingStatus));
    }

    [Fact]
    public async Task Worker_process_batch_marks_outbox_events_processed()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid().ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Worker process trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId,
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/integration-events/process-batch",
            _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessIntegrationOutboxEventsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var result = (await processResponse.Content.ReadFromJsonAsync<ProcessIntegrationOutboxEventsResponse>())!;

        Assert.True(result.ProcessedCount >= 1);

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var processed = await db.IntegrationOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == created.TripId)
            .ToListAsync();
        Assert.All(processed, x => Assert.Equal(IntegrationEventStatuses.Processed, x.ProcessingStatus));
    }

    [Fact]
    public async Task Integration_event_settings_disable_skips_enqueue()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var settingsRequest = Authorized(HttpMethod.Put, "/api/integration-event-settings", adminToken);
        settingsRequest.Content = JsonContent.Create(new UpsertIntegrationEventSettingsRequest(
            false,
            null,
            null));
        (await _routarrClient.SendAsync(settingsRequest)).EnsureSuccessStatusCode();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Disabled integration events",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            Guid.NewGuid().ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var count = await db.IntegrationOutboxEvents.CountAsync(x => x.RelatedEntityId == created.TripId);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _routarrClient.PostAsJsonAsync(
            "/api/internal/integration-events/process-batch",
            new ProcessIntegrationOutboxEventsRequest(PlatformSeeder.DemoTenantId, null, 10));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Events_v1_alias_matches_integration_event_outbox()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Events alias trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            Guid.NewGuid().ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var outboxResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/integration-event-settings/outbox?limit=10", adminToken));
        outboxResponse.EnsureSuccessStatusCode();
        var outbox = (await outboxResponse.Content.ReadFromJsonAsync<IntegrationOutboxEventListResponse>())!;

        var eventsV1Response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/events?limit=10", adminToken));
        eventsV1Response.EnsureSuccessStatusCode();
        var eventsV1 = (await eventsV1Response.Content.ReadFromJsonAsync<IntegrationOutboxEventListResponse>())!;

        Assert.Equal(outbox.Items.Count, eventsV1.Items.Count);
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
            $"{sourceProduct}-integration-{Guid.NewGuid():N}",
            $"{sourceProduct} integration test",
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
