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
using RoutArrIntegration = RoutArr.Api.Endpoints.IntegrationEndpoints;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;
using SupplyArrRedeemRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using SupplyArrIntegration = SupplyArr.Api.Endpoints.IntegrationEndpoints;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrSupplyArrPartsDemandTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _supplyarrIntegrationToken = null!;
    private string _routarrStatusCallbackToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutSupplyDemandNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutSupplyDemandRoutArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"RoutSupplyDemandSupplyArr-{Guid.NewGuid():N}";

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
        _supplyarrIntegrationToken = await IssueServiceTokenAsync(
            adminToken,
            "routarr",
            ["supplyarr"],
            SupplyArrIntegration.RoutarrDemandIngestActionScope);
        _routarrStatusCallbackToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["routarr"],
            RoutArrIntegration.SupplyarrDemandStatusIngestActionScope);
        var routarrHandoffToken = await IssueServiceTokenAsync(adminToken, "routarr", ["routarr"], "launch.redeem");
        var supplyarrHandoffToken = await IssueServiceTokenAsync(adminToken, "supplyarr", ["supplyarr"], "launch.redeem");

        WebApplicationFactory<global::SupplyArr.Api.Program>? supplyarrFactoryRef = null;

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", routarrHandoffToken);
            builder.UseSetting("SupplyArr:BaseUrl", "http://localhost");
            builder.UseSetting("SupplyArr:ServiceToken", _supplyarrIntegrationToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<SupplyArrDemandClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => supplyarrFactoryRef!.Server.CreateHandler());
            });
        });

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", supplyarrHandoffToken);
            builder.UseSetting("RoutArr:BaseUrl", _routarrFactory.Server.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("RoutArr:ServiceToken", _routarrStatusCallbackToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<RoutArrDemandStatusClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _routarrFactory.Server.CreateHandler());
            });
        });

        supplyarrFactoryRef = _supplyarrFactory;
        _routarrClient = _routarrFactory.CreateClient();
        _supplyarrClient = _supplyarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Trip_parts_demand_publish_creates_supplyarr_mirror()
    {
        var routarrToken = await RedeemRoutArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var tripId = await CreateTripAsync(routarrToken);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand", routarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateTripPartsDemandLineRequest(
            partId,
            "BRK-001",
            "Trip brake pads",
            2m,
            "each",
            "Urgent"));
        var createLineResponse = await _routarrClient.SendAsync(createLineRequest);
        createLineResponse.EnsureSuccessStatusCode();
        var line = (await createLineResponse.Content.ReadFromJsonAsync<TripPartsDemandLineResponse>())!;
        Assert.Equal("pending", line.Status);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand/publish", routarrToken);
        publishRequest.Content = JsonContent.Create(new PublishTripPartsDemandRequest(false));
        var publishResponse = await _routarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishTripPartsDemandResponse>())!;
        Assert.NotEqual(Guid.Empty, published.DemandRefId);

        var listDemandRefsRequest = Authorized(HttpMethod.Get, "/api/routarr-demand-refs", supplyarrToken);
        var listDemandRefsResponse = await _supplyarrClient.SendAsync(listDemandRefsRequest);
        listDemandRefsResponse.EnsureSuccessStatusCode();
        var demandRefs = (await listDemandRefsResponse.Content.ReadFromJsonAsync<List<RoutArrDemandRefResponse>>())!;
        var demandRef = Assert.Single(demandRefs);
        Assert.Equal(tripId, demandRef.RoutarrTripId);
        Assert.Equal("received", demandRef.Status);
        Assert.Single(demandRef.Lines);
        Assert.Equal(partId, demandRef.Lines[0].PartId);
    }

    [Fact]
    public async Task Routarr_demand_ingest_is_idempotent()
    {
        var routarrToken = await RedeemRoutArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var tripId = await CreateTripAsync(routarrToken);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand", routarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateTripPartsDemandLineRequest(
            partId,
            "FIL-001",
            "Trip oil filter",
            1m,
            "each",
            null));
        await _routarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand/publish", routarrToken);
        publishRequest.Content = JsonContent.Create(new PublishTripPartsDemandRequest(false));
        var firstPublish = await _routarrClient.SendAsync(publishRequest);
        firstPublish.EnsureSuccessStatusCode();
        var first = (await firstPublish.Content.ReadFromJsonAsync<PublishTripPartsDemandResponse>())!;

        var ingestRequest = ServiceAuthorized(HttpMethod.Post, "/api/integrations/routarr-demand", _supplyarrIntegrationToken);
        ingestRequest.Content = JsonContent.Create(new IngestRoutarrDemandRequest(
            PlatformSeeder.DemoTenantId,
            first.PublicationId,
            tripId,
            "TR-REPLAY",
            "VEH-REPLAY",
            "Replay trip",
            null,
            false,
            [
                new IngestRoutarrDemandLineRequest(
                    Guid.NewGuid(),
                    partId,
                    "FIL-001",
                    "Trip oil filter",
                    1m,
                    "each",
                    null)
            ]));
        var replayResponse = await _supplyarrClient.SendAsync(ingestRequest);
        replayResponse.EnsureSuccessStatusCode();
        var replay = (await replayResponse.Content.ReadFromJsonAsync<RoutarrDemandIntakeResponse>())!;
        Assert.True(replay.IdempotentReplay);
        Assert.Equal(first.DemandRefId, replay.DemandRefId);

        var listDemandRefsRequest = Authorized(HttpMethod.Get, "/api/routarr-demand-refs", supplyarrToken);
        var listDemandRefsResponse = await _supplyarrClient.SendAsync(listDemandRefsRequest);
        listDemandRefsResponse.EnsureSuccessStatusCode();
        var demandRefs = (await listDemandRefsResponse.Content.ReadFromJsonAsync<List<RoutArrDemandRefResponse>>())!;
        Assert.Single(demandRefs.Where(x => x.RoutarrPublicationId == first.PublicationId));
    }

    [Fact]
    public async Task Publish_with_pr_draft_creates_purchase_request()
    {
        var routarrToken = await RedeemRoutArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var tripId = await CreateTripAsync(routarrToken);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand", routarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateTripPartsDemandLineRequest(
            partId,
            "ALT-001",
            "Trip alternator",
            1m,
            "each",
            null));
        await _routarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand/publish", routarrToken);
        publishRequest.Content = JsonContent.Create(new PublishTripPartsDemandRequest(true));
        var publishResponse = await _routarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishTripPartsDemandResponse>())!;
        Assert.True(published.CreatedPurchaseRequestDraft);
        Assert.NotNull(published.PurchaseRequestId);

        var listPrRequest = Authorized(HttpMethod.Get, "/api/purchase-requests", supplyarrToken);
        var listPrResponse = await _supplyarrClient.SendAsync(listPrRequest);
        listPrResponse.EnsureSuccessStatusCode();
        var purchaseRequests = (await listPrResponse.Content.ReadFromJsonAsync<List<PurchaseRequestResponse>>())!;
        Assert.Contains(purchaseRequests, x => x.PurchaseRequestId == published.PurchaseRequestId);
    }

    [Fact]
    public async Task Pr_submit_updates_routarr_procurement_status()
    {
        var routarrToken = await RedeemRoutArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var tripId = await CreateTripAsync(routarrToken);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand", routarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateTripPartsDemandLineRequest(
            partId,
            "STAT-R-001",
            "RoutArr status callback part",
            1m,
            "each",
            null));
        await _routarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand/publish", routarrToken);
        publishRequest.Content = JsonContent.Create(new PublishTripPartsDemandRequest(true));
        var publishResponse = await _routarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishTripPartsDemandResponse>())!;
        Assert.NotNull(published.PurchaseRequestId);

        var afterDraftLines = await ListDemandLinesAsync(routarrToken, tripId);
        Assert.All(afterDraftLines, line => Assert.Equal("pr_drafted", line.ProcurementStatus));

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{published.PurchaseRequestId}/submit",
            supplyarrToken);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var afterSubmitLines = await ListDemandLinesAsync(routarrToken, tripId);
        Assert.All(afterSubmitLines, line => Assert.Equal("pr_submitted", line.ProcurementStatus));
    }

    [Fact]
    public async Task Supplyarr_demand_status_callback_is_idempotent_for_routarr()
    {
        var routarrToken = await RedeemRoutArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var tripId = await CreateTripAsync(routarrToken);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand", routarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateTripPartsDemandLineRequest(
            partId,
            "IDEM-R-001",
            "RoutArr idempotent callback part",
            1m,
            "each",
            null));
        await _routarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/trips/{tripId}/parts-demand/publish", routarrToken);
        publishRequest.Content = JsonContent.Create(new PublishTripPartsDemandRequest(false));
        var publishResponse = await _routarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishTripPartsDemandResponse>())!;

        var callbackPublicationId = Guid.NewGuid();
        var callbackRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/supplyarr-demand-status",
            _routarrStatusCallbackToken);
        callbackRequest.Content = JsonContent.Create(new IngestSupplyarrDemandStatusRequest(
            PlatformSeeder.DemoTenantId,
            published.PublicationId,
            published.DemandRefId,
            callbackPublicationId,
            "pr_submitted",
            "pr_submitted",
            null,
            null,
            null,
            null,
            "Replay test",
            DateTimeOffset.UtcNow));

        var firstResponse = await _routarrClient.SendAsync(callbackRequest);
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<IngestSupplyarrDemandStatusResponse>())!;
        Assert.False(first.IdempotentReplay);

        var replayRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/supplyarr-demand-status",
            _routarrStatusCallbackToken);
        replayRequest.Content = JsonContent.Create(new IngestSupplyarrDemandStatusRequest(
            PlatformSeeder.DemoTenantId,
            published.PublicationId,
            published.DemandRefId,
            callbackPublicationId,
            "pr_submitted",
            "pr_submitted",
            null,
            null,
            null,
            null,
            "Replay test",
            DateTimeOffset.UtcNow));
        var replayResponse = await _routarrClient.SendAsync(replayRequest);
        replayResponse.EnsureSuccessStatusCode();
        var replay = (await replayResponse.Content.ReadFromJsonAsync<IngestSupplyarrDemandStatusResponse>())!;
        Assert.True(replay.IdempotentReplay);
        Assert.Equal(first.StatusEventId, replay.StatusEventId);
    }

    [Fact]
    public async Task Routarr_demand_ingest_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/routarr-demand");
        request.Content = JsonContent.Create(new IngestRoutarrDemandRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TR-401",
            "VEH-401",
            "Unauthorized",
            null,
            false,
            [
                new IngestRoutarrDemandLineRequest(
                    Guid.NewGuid(),
                    null,
                    "TEST-001",
                    "Test part",
                    1m,
                    "each",
                    null)
            ]));
        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<IReadOnlyList<TripPartsDemandLineResponse>> ListDemandLinesAsync(string routarrToken, Guid tripId)
    {
        var request = Authorized(HttpMethod.Get, $"/api/trips/{tripId}/parts-demand", routarrToken);
        var response = await _routarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<TripPartsDemandLineResponse>>())!;
    }

    private async Task<Guid> CreateTripAsync(string routarrToken)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/trips", routarrToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Parts demand trip",
            "Trip for supply demand test",
            "VEH-FL-200",
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddHours(4),
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        return created.TripId;
    }

    private async Task<Guid> SeedSupplyArrPartAsync(string token)
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"part-{Guid.NewGuid():N}".Substring(0, 12),
            null,
            "RoutArr Demand Test Part",
            string.Empty,
            "general",
            "each",
            "Acme",
            "AC-200"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;
        return part.PartId;
    }

    private async Task<string> RedeemRoutArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync("routarr", "http://localhost:5180/launch");
        var redeemResponse = await _routarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new RoutArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<RoutArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> RedeemSupplyArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync("supplyarr", "http://localhost:5179/launch");
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync(string productKey, string callbackUrl)
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest(productKey, callbackUrl));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-demand-test-{Guid.NewGuid():N}",
            $"{sourceProduct} demand test",
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
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
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

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
