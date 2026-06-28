using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NexArr.Api.Contracts;
using SupplyArr.Api.Options;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using NexArr.Api.Data;
using NexArr.Api.Services;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrIntegrationEventTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _workerToken = null!;
    private string _maintainarrToken = null!;
    private string _handoffServiceToken = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"IntegrationEventsNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"IntegrationEventsSupplyArr-{Guid.NewGuid():N}";

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
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["supplyarr"],
            IntegrationEventProcessingService.ProcessEventsActionScope);
        _maintainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            ["supplyarr"],
            "supplyarr.demand_intake.write");
        _handoffServiceToken = await IssueServiceTokenAsync(adminToken, "supplyarr");
        var handoffCode = await CreateHandoffAsync(adminToken);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _handoffServiceToken);
            builder.UseSetting("MaintainArr:BaseUrl", "http://localhost:5999");
            builder.UseSetting("MaintainArr:ServiceToken", "test-maintainarr-token");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.RemoveAll<MaintainArrDemandStatusClient>();
                services.AddSingleton(_ =>
                    new MaintainArrDemandStatusClient(
                        new HttpClient(new SuccessHttpMessageHandler())
                        {
                            BaseAddress = new Uri("http://localhost:5999/"),
                        },
                        Microsoft.Extensions.Options.Options.Create(new MaintainArrClientOptions
                        {
                            BaseUrl = "http://localhost:5999",
                            ServiceToken = "test-maintainarr-token",
                        })));
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();
        _supplyarrClient.BaseAddress = new Uri("http://localhost");

        _userToken = await RedeemHandoffAsync(handoffCode);
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.PostAsJsonAsync(
            "/api/internal/integration-events/process-batch",
            new ProcessIntegrationEventsRequest(PlatformSeeder.DemoTenantId, 25));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Outbox_enqueue_is_idempotent()
    {
        await EnableIntegrationSettingsAsync();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var enqueue = scope.ServiceProvider.GetRequiredService<IntegrationOutboxEnqueueService>();
        var entityId = Guid.NewGuid();
        var first = await enqueue.TryEnqueueAsync(
            PlatformSeeder.DemoTenantId,
            IntegrationOutboxEventKinds.PartyCreated,
            "external_party",
            entityId,
            new IntegrationOutboxPayload(PlatformSeeder.DemoTenantId, "Test party"));
        var second = await enqueue.TryEnqueueAsync(
            PlatformSeeder.DemoTenantId,
            IntegrationOutboxEventKinds.PartyCreated,
            "external_party",
            entityId,
            new IntegrationOutboxPayload(PlatformSeeder.DemoTenantId, "Test party"));

        Assert.NotNull(first);
        Assert.Null(second);

        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var count = await db.IntegrationOutboxEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == entityId);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Process_batch_processes_outbox_and_inbox()
    {
        await EnableIntegrationSettingsAsync();

        var publicationId = Guid.NewGuid();
        var ingest = new IngestMaintainarrDemandRequest(
            PlatformSeeder.DemoTenantId,
            publicationId,
            Guid.NewGuid(),
            "WO-INT-001",
            Guid.NewGuid(),
            "Integration test demand",
            null,
            false,
            [
                new IngestMaintainarrDemandLineRequest(
                    Guid.NewGuid(),
                    null,
                    "TEST-PART-001",
                    "Test line",
                    1m,
                    "ea",
                    null),
            ]);

        var enqueueRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/integration-events/inbox/enqueue",
            _maintainarrToken);
        enqueueRequest.Content = JsonContent.Create(new EnqueueIntegrationInboxRequest(
            PlatformSeeder.DemoTenantId,
            "maintainarr",
            IntegrationInboxEventKinds.MaintainarrDemandIngest,
            publicationId.ToString(),
            "maintainarr_publication",
            publicationId.ToString(),
            JsonSerializer.Serialize(ingest),
            null));

        var enqueueResponse = await _supplyarrClient.SendAsync(enqueueRequest);
        enqueueResponse.EnsureSuccessStatusCode();

        using (var scope = _supplyarrFactory.Services.CreateScope())
        {
            var outboxEnqueue = scope.ServiceProvider.GetRequiredService<IntegrationOutboxEnqueueService>();
            await outboxEnqueue.TryEnqueueAsync(
                PlatformSeeder.DemoTenantId,
                IntegrationOutboxEventKinds.PartCreated,
                "part",
                Guid.NewGuid(),
                new IntegrationOutboxPayload(PlatformSeeder.DemoTenantId, "Queued part"));
        }

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/integration-events/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessIntegrationEventsRequest(
            PlatformSeeder.DemoTenantId,
            50));

        var processResponse = await _supplyarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessIntegrationEventsResponse>())!;
        Assert.True(body.OutboxProcessedCount + body.InboxProcessedCount >= 1);

        using var verifyScope = _supplyarrFactory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var demandRef = await db.MaintainArrDemandRefs.SingleOrDefaultAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.MaintainarrPublicationId == publicationId);
        Assert.NotNull(demandRef);

        var inbox = await db.IntegrationInboxEvents.SingleAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.EventKind == IntegrationInboxEventKinds.MaintainarrDemandIngest);
        Assert.Equal(IntegrationEventStatuses.Processed, inbox.ProcessingStatus);
    }

    [Fact]
    public async Task Product_qualified_outbound_events_are_enqueued_for_core_supply_workflows()
    {
        await EnableIntegrationSettingsAsync();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var parties = scope.ServiceProvider.GetRequiredService<ExternalPartyService>();
        var parts = scope.ServiceProvider.GetRequiredService<PartRegistryService>();
        var reservations = scope.ServiceProvider.GetRequiredService<StockReservationService>();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var actorUserId = PlatformSeeder.DemoAdminUserId;

        var vendor = await parties.CreateTypedAsync(
            PlatformSeeder.DemoTenantId,
            actorUserId,
            "vendor",
            new CreateTypedExternalPartyRequest(
                $"vendor-{Guid.NewGuid():N}"[..16],
                "Qualified Event Vendor",
                "Qualified Event Vendor LLC",
                null,
                string.Empty));
        await parties.UpdateAsync(
            PlatformSeeder.DemoTenantId,
            actorUserId,
            vendor.PartyId,
            new UpdateExternalPartyRequest(
                "Qualified Event Vendor Updated",
                "Qualified Event Vendor LLC",
                null,
                "updated"));
        await parties.UpdateApprovalStatusAsync(
            PlatformSeeder.DemoTenantId,
            actorUserId,
            vendor.PartyId,
            new UpdateExternalPartyApprovalStatusRequest("approved"));
        await parties.UpdateApprovalStatusAsync(
            PlatformSeeder.DemoTenantId,
            actorUserId,
            vendor.PartyId,
            new UpdateExternalPartyApprovalStatusRequest("restricted"));

        var customer = await parties.CreateTypedAsync(
            PlatformSeeder.DemoTenantId,
            actorUserId,
            "customer",
            new CreateTypedExternalPartyRequest(
                $"customer-{Guid.NewGuid():N}"[..16],
                "Qualified Event Customer",
                "Qualified Event Customer LLC",
                null,
                string.Empty));

        var part = await parts.CreateAsync(
            PlatformSeeder.DemoTenantId,
            actorUserId,
            new CreatePartRequest(
                $"part-{Guid.NewGuid():N}"[..16],
                null,
                "Qualified Event Part",
                "Evented inventory part",
                "maintenance",
                "each",
                "Acme",
                "ACME-100"));
        await parts.UpdateAsync(
            PlatformSeeder.DemoTenantId,
            actorUserId,
            part.PartId,
            new UpdatePartRequest(
                null,
                "Qualified Event Part Updated",
                "Evented inventory part",
                "maintenance",
                "each",
                "Acme",
                "ACME-101"));

        var (binId, stockLevelId) = await SeedStockAsync(part.PartId);
        var reservation = await reservations.CreateAsync(
            PlatformSeeder.DemoTenantId,
            actorUserId,
            new CreateStockReservationRequest(
                $"reservation-{Guid.NewGuid():N}"[..24],
                part.PartId,
                binId,
                2m,
                "manual",
                null,
                "needed for work order"));

        var outbox = await db.IntegrationOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId)
            .ToListAsync();

        Assert.Contains(outbox, x => x.EventKind == IntegrationOutboxEventKinds.SupplyArrVendorCreated
            && x.RelatedEntityId == vendor.PartyId);
        Assert.Contains(outbox, x => x.EventKind == IntegrationOutboxEventKinds.SupplyArrVendorUpdated
            && x.RelatedEntityId == vendor.PartyId);
        Assert.Contains(outbox, x => x.EventKind == IntegrationOutboxEventKinds.SupplyArrVendorApproved
            && x.RelatedEntityId == vendor.PartyId);
        Assert.Contains(outbox, x => x.EventKind == IntegrationOutboxEventKinds.SupplyArrVendorBlocked
            && x.RelatedEntityId == vendor.PartyId);
        Assert.Contains(outbox, x => x.EventKind == IntegrationOutboxEventKinds.SupplyArrCustomerCreated
            && x.RelatedEntityId == customer.PartyId);
        Assert.Contains(outbox, x => x.EventKind == IntegrationOutboxEventKinds.SupplyArrItemCreated
            && x.RelatedEntityId == part.PartId);
        Assert.Contains(outbox, x => x.EventKind == IntegrationOutboxEventKinds.SupplyArrItemUpdated
            && x.RelatedEntityId == part.PartId);
        Assert.Contains(outbox, x => x.EventKind == IntegrationOutboxEventKinds.SupplyArrInventoryReserved
            && x.RelatedEntityId == reservation.ReservationId);

        var stock = await db.PartStockLevels.SingleAsync(x => x.Id == stockLevelId);
        Assert.Equal(2m, stock.QuantityReserved);
    }

    [Fact]
    public async Task Settings_endpoints_require_authenticated_user()
    {
        var response = await _supplyarrClient.GetAsync("/api/integration-event-settings");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var authed = Authorized(HttpMethod.Get, "/api/integration-event-settings", _userToken);
        var ok = await _supplyarrClient.SendAsync(authed);
        ok.EnsureSuccessStatusCode();
    }

    private async Task EnableIntegrationSettingsAsync()
    {
        var request = Authorized(HttpMethod.Put, "/api/integration-event-settings", _userToken);
        request.Content = JsonContent.Create(new UpsertIntegrationEventSettingsRequest(true, 5, 15));
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<(Guid BinId, Guid StockLevelId)> SeedStockAsync(Guid partId)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var location = new InventoryLocation
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            LocationKey = $"loc-{Guid.NewGuid():N}"[..16],
            Name = "Qualified Event Location",
            LocationType = "warehouse",
            AddressLine = "100 Event Way",
            Status = "active",
            StaffarrSiteOrgUnitId = Guid.Parse("7d96aa4b-1116-4a27-9660-b1f64dd03261"),
            StaffarrSiteNameSnapshot = "Qualified Event Site",
            StaffarrSiteResolutionStatus = InventoryLocationSiteResolutionStatuses.Active,
            CreatedAt = now,
            UpdatedAt = now,
        };
        var bin = new InventoryBin
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            InventoryLocationId = location.Id,
            BinKey = $"bin-{Guid.NewGuid():N}"[..16],
            Name = "Qualified Event Bin",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var stock = new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PartId = partId,
            InventoryBinId = bin.Id,
            QuantityOnHand = 10m,
            QuantityReserved = 0m,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.InventoryLocations.Add(location);
        db.InventoryBins.Add(bin);
        db.PartStockLevels.Add(stock);
        await db.SaveChangesAsync();
        return (bin.Id, stock.Id);
    }

    private async Task<string> CreateHandoffAsync(string token)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("supplyarr", "http://localhost:5179/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> RedeemHandoffAsync(string handoffCode)
    {
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemHandoffRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
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

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-integration-events-handoff-test",
            $"{productKey} integration events handoff test",
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-integration-events-test",
            $"{sourceProduct} integration events test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private sealed class SuccessHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
