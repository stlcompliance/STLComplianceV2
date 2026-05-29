using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Services;
using MaintainArrIntegration = MaintainArr.Api.Endpoints.IntegrationEndpoints;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Services;
using SupplyArrIntegration = SupplyArr.Api.Endpoints.IntegrationEndpoints;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using SupplyArrRedeemRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using AssetClassResponse = MaintainArr.Api.Contracts.AssetClassResponse;
using CreateAssetClassRequest = MaintainArr.Api.Contracts.CreateAssetClassRequest;
using AssetTypeResponse = MaintainArr.Api.Contracts.AssetTypeResponse;
using CreateAssetTypeRequest = MaintainArr.Api.Contracts.CreateAssetTypeRequest;
using AssetResponse = MaintainArr.Api.Contracts.AssetResponse;
using CreateAssetRequest = MaintainArr.Api.Contracts.CreateAssetRequest;
using STLCompliance.Shared.Auth;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrSupplyArrPartsDemandTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _supplyarrIntegrationToken = null!;
    private string _maintainarrStatusCallbackToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"MaintainSupplyDemandNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"MaintainSupplyDemandMaintainArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"MaintainSupplyDemandSupplyArr-{Guid.NewGuid():N}";

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
            "maintainarr",
            ["supplyarr"],
            SupplyArrIntegration.MaintainarrDemandIngestActionScope);
        _maintainarrStatusCallbackToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["maintainarr"],
            MaintainArrIntegration.SupplyarrDemandStatusIngestActionScope);
        var supplyarrHandoffToken = await IssueServiceTokenAsync(adminToken, "supplyarr", ["supplyarr"], "launch.redeem");

        var maintainarrHandoffToken = await IssueServiceTokenAsync(adminToken, "maintainarr", ["maintainarr"], "launch.redeem");

        WebApplicationFactory<global::SupplyArr.Api.Program>? supplyarrFactoryRef = null;

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", maintainarrHandoffToken);
            builder.UseSetting("SupplyArr:BaseUrl", "http://localhost");
            builder.UseSetting("SupplyArr:ServiceToken", _supplyarrIntegrationToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

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
            builder.UseSetting("MaintainArr:BaseUrl", _maintainarrFactory.Server.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("MaintainArr:ServiceToken", _maintainarrStatusCallbackToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<MaintainArrDemandStatusClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _maintainarrFactory.Server.CreateHandler());
            });
        });

        supplyarrFactoryRef = _supplyarrFactory;
        _maintainarrClient = _maintainarrFactory.CreateClient();
        _supplyarrClient = _supplyarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Work_order_parts_demand_publish_creates_supplyarr_mirror()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var assetId = await SeedAssetOnlyAsync(maintainarrToken);
        var workOrderId = await CreateOpenWorkOrderAsync(maintainarrToken, assetId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand", maintainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateWorkOrderPartsDemandLineRequest(
            partId,
            "BRK-001",
            "Front brake pads",
            2m,
            "each",
            "Urgent"));
        var createLineResponse = await _maintainarrClient.SendAsync(createLineRequest);
        createLineResponse.EnsureSuccessStatusCode();
        var line = (await createLineResponse.Content.ReadFromJsonAsync<WorkOrderPartsDemandLineResponse>())!;
        Assert.Equal("pending", line.Status);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand/publish", maintainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishWorkOrderPartsDemandRequest(false));
        var publishResponse = await _maintainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishWorkOrderPartsDemandResponse>())!;
        Assert.NotEqual(Guid.Empty, published.SupplyarrDemandRefId);

        var listDemandRefsRequest = Authorized(HttpMethod.Get, "/api/demand-refs", supplyarrToken);
        var listDemandRefsResponse = await _supplyarrClient.SendAsync(listDemandRefsRequest);
        listDemandRefsResponse.EnsureSuccessStatusCode();
        var demandRefs = (await listDemandRefsResponse.Content.ReadFromJsonAsync<List<MaintainArrDemandRefResponse>>())!;
        var demandRef = Assert.Single(demandRefs);
        Assert.Equal(workOrderId, demandRef.MaintainarrWorkOrderId);
        Assert.Equal("received", demandRef.Status);
        Assert.Single(demandRef.Lines);
        Assert.Equal(partId, demandRef.Lines[0].PartId);
    }

    [Fact]
    public async Task Maintainarr_demand_ingest_is_idempotent()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var assetId = await SeedAssetOnlyAsync(maintainarrToken);
        var workOrderId = await CreateOpenWorkOrderAsync(maintainarrToken, assetId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand", maintainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateWorkOrderPartsDemandLineRequest(
            partId,
            "FIL-001",
            "Oil filter",
            1m,
            "each",
            null));
        await _maintainarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand/publish", maintainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishWorkOrderPartsDemandRequest(false));
        var firstPublish = await _maintainarrClient.SendAsync(publishRequest);
        firstPublish.EnsureSuccessStatusCode();
        var first = (await firstPublish.Content.ReadFromJsonAsync<PublishWorkOrderPartsDemandResponse>())!;

        var ingestRequest = ServiceAuthorized(HttpMethod.Post, "/api/integrations/maintainarr-demand", _supplyarrIntegrationToken);
        ingestRequest.Content = JsonContent.Create(new IngestMaintainarrDemandRequest(
            PlatformSeeder.DemoTenantId,
            first.PublicationId,
            workOrderId,
            "WO-REPLAY",
            assetId,
            "Replay WO",
            null,
            false,
            [
                new IngestMaintainarrDemandLineRequest(
                    Guid.NewGuid(),
                    partId,
                    "FIL-001",
                    "Oil filter",
                    1m,
                    "each",
                    null)
            ]));
        var replayResponse = await _supplyarrClient.SendAsync(ingestRequest);
        replayResponse.EnsureSuccessStatusCode();
        var replay = (await replayResponse.Content.ReadFromJsonAsync<MaintainarrDemandIntakeResponse>())!;
        Assert.True(replay.IdempotentReplay);
        Assert.Equal(first.SupplyarrDemandRefId, replay.DemandRefId);

        var listDemandRefsRequest = Authorized(HttpMethod.Get, "/api/demand-refs", supplyarrToken);
        var listDemandRefsResponse = await _supplyarrClient.SendAsync(listDemandRefsRequest);
        listDemandRefsResponse.EnsureSuccessStatusCode();
        var demandRefs = (await listDemandRefsResponse.Content.ReadFromJsonAsync<List<MaintainArrDemandRefResponse>>())!;
        Assert.Single(demandRefs.Where(x => x.MaintainarrPublicationId == first.PublicationId));
    }

    [Fact]
    public async Task Publish_with_pr_draft_creates_purchase_request()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var assetId = await SeedAssetOnlyAsync(maintainarrToken);
        var workOrderId = await CreateOpenWorkOrderAsync(maintainarrToken, assetId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand", maintainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateWorkOrderPartsDemandLineRequest(
            partId,
            "ALT-001",
            "Alternator",
            1m,
            "each",
            null));
        await _maintainarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand/publish", maintainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishWorkOrderPartsDemandRequest(true));
        var publishResponse = await _maintainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishWorkOrderPartsDemandResponse>())!;
        Assert.True(published.CreatedPurchaseRequestDraft);
        Assert.NotNull(published.SupplyarrPurchaseRequestId);

        var listPrRequest = Authorized(HttpMethod.Get, "/api/purchase-requests", supplyarrToken);
        var listPrResponse = await _supplyarrClient.SendAsync(listPrRequest);
        listPrResponse.EnsureSuccessStatusCode();
        var purchaseRequests = (await listPrResponse.Content.ReadFromJsonAsync<List<PurchaseRequestResponse>>())!;
        Assert.Contains(purchaseRequests, x => x.PurchaseRequestId == published.SupplyarrPurchaseRequestId);
    }

    [Fact]
    public async Task Maintainarr_demand_ingest_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/maintainarr-demand");
        request.Content = JsonContent.Create(new IngestMaintainarrDemandRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WO-401",
            Guid.NewGuid(),
            "Unauthorized",
            null,
            false,
            [
                new IngestMaintainarrDemandLineRequest(
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

    [Fact]
    public async Task Pr_submit_updates_maintainarr_procurement_status()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var assetId = await SeedAssetOnlyAsync(maintainarrToken);
        var workOrderId = await CreateOpenWorkOrderAsync(maintainarrToken, assetId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand", maintainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateWorkOrderPartsDemandLineRequest(
            partId,
            "STAT-001",
            "Status callback part",
            1m,
            "each",
            null));
        await _maintainarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand/publish", maintainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishWorkOrderPartsDemandRequest(true));
        var publishResponse = await _maintainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishWorkOrderPartsDemandResponse>())!;
        Assert.NotNull(published.SupplyarrPurchaseRequestId);

        var afterDraftLines = await ListDemandLinesAsync(maintainarrToken, workOrderId);
        Assert.All(afterDraftLines, line => Assert.Equal("pr_drafted", line.ProcurementStatus));

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{published.SupplyarrPurchaseRequestId}/submit",
            supplyarrToken);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var afterSubmitLines = await ListDemandLinesAsync(maintainarrToken, workOrderId);
        Assert.All(afterSubmitLines, line => Assert.Equal("pr_submitted", line.ProcurementStatus));
    }

    [Fact]
    public async Task Parts_demand_status_events_list_after_callback()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var assetId = await SeedAssetOnlyAsync(maintainarrToken);
        var workOrderId = await CreateOpenWorkOrderAsync(maintainarrToken, assetId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand", maintainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateWorkOrderPartsDemandLineRequest(
            partId,
            "EVT-001",
            "Status events list part",
            1m,
            "each",
            null));
        await _maintainarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand/publish", maintainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishWorkOrderPartsDemandRequest(false));
        var publishResponse = await _maintainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishWorkOrderPartsDemandResponse>())!;

        var callbackRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/supplyarr-demand-status",
            _maintainarrStatusCallbackToken);
        callbackRequest.Content = JsonContent.Create(new IngestSupplyarrDemandStatusRequest(
            PlatformSeeder.DemoTenantId,
            published.PublicationId,
            published.SupplyarrDemandRefId,
            Guid.NewGuid(),
            "pr_submitted",
            "pr_submitted",
            null,
            null,
            null,
            null,
            "PR submitted for approval",
            DateTimeOffset.UtcNow));
        (await _maintainarrClient.SendAsync(callbackRequest)).EnsureSuccessStatusCode();

        var statusEvents = await ListStatusEventsAsync(maintainarrToken, workOrderId);
        Assert.Single(statusEvents);
        Assert.Equal("pr_submitted", statusEvents[0].ProcurementStatus);
        Assert.Equal("PR submitted for approval", statusEvents[0].Message);
    }

    [Fact]
    public async Task Parts_demand_status_events_list_is_empty_before_publish()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(maintainarrToken);
        var workOrderId = await CreateOpenWorkOrderAsync(maintainarrToken, assetId);

        var events = await ListStatusEventsAsync(maintainarrToken, workOrderId);
        Assert.Empty(events);
    }

    [Fact]
    public async Task Supplyarr_demand_status_callback_is_idempotent()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var assetId = await SeedAssetOnlyAsync(maintainarrToken);
        var workOrderId = await CreateOpenWorkOrderAsync(maintainarrToken, assetId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand", maintainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateWorkOrderPartsDemandLineRequest(
            partId,
            "IDEM-001",
            "Idempotent callback part",
            1m,
            "each",
            null));
        await _maintainarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand/publish", maintainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishWorkOrderPartsDemandRequest(false));
        var publishResponse = await _maintainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishWorkOrderPartsDemandResponse>())!;

        var callbackPublicationId = Guid.NewGuid();
        var callbackRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/supplyarr-demand-status",
            _maintainarrStatusCallbackToken);
        callbackRequest.Content = JsonContent.Create(new IngestSupplyarrDemandStatusRequest(
            PlatformSeeder.DemoTenantId,
            published.PublicationId,
            published.SupplyarrDemandRefId,
            callbackPublicationId,
            "pr_submitted",
            "pr_submitted",
            null,
            null,
            null,
            null,
            "Replay test",
            DateTimeOffset.UtcNow));

        var firstResponse = await _maintainarrClient.SendAsync(callbackRequest);
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<IngestSupplyarrDemandStatusResponse>())!;
        Assert.False(first.IdempotentReplay);

        var replayRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/supplyarr-demand-status",
            _maintainarrStatusCallbackToken);
        replayRequest.Content = JsonContent.Create(new IngestSupplyarrDemandStatusRequest(
            PlatformSeeder.DemoTenantId,
            published.PublicationId,
            published.SupplyarrDemandRefId,
            callbackPublicationId,
            "pr_submitted",
            "pr_submitted",
            null,
            null,
            null,
            null,
            "Replay test",
            DateTimeOffset.UtcNow));
        var replayResponse = await _maintainarrClient.SendAsync(replayRequest);
        replayResponse.EnsureSuccessStatusCode();
        var replay = (await replayResponse.Content.ReadFromJsonAsync<IngestSupplyarrDemandStatusResponse>())!;
        Assert.True(replay.IdempotentReplay);
        Assert.Equal(first.StatusEventId, replay.StatusEventId);
    }

    [Fact]
    public async Task Supplyarr_demand_status_callback_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/supplyarr-demand-status");
        request.Content = JsonContent.Create(new IngestSupplyarrDemandStatusRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "pr_submitted",
            "pr_submitted",
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow));
        var response = await _maintainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<IReadOnlyList<WorkOrderPartsDemandLineResponse>> ListDemandLinesAsync(
        string maintainarrToken,
        Guid workOrderId)
    {
        var listRequest = Authorized(HttpMethod.Get, $"/api/work-orders/{workOrderId}/parts-demand", maintainarrToken);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        return (await listResponse.Content.ReadFromJsonAsync<List<WorkOrderPartsDemandLineResponse>>())!;
    }

    private async Task<IReadOnlyList<WorkOrderPartsDemandStatusEventResponse>> ListStatusEventsAsync(
        string maintainarrToken,
        Guid workOrderId)
    {
        var listRequest = Authorized(
            HttpMethod.Get,
            $"/api/work-orders/{workOrderId}/parts-demand/status-events",
            maintainarrToken);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        return (await listResponse.Content.ReadFromJsonAsync<List<WorkOrderPartsDemandStatusEventResponse>>())!;
    }

    private async Task<Guid> SeedSupplyArrPartAsync(string token)
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"part-{Guid.NewGuid():N}".Substring(0, 12),
            null,
            "Demand Test Part",
            string.Empty,
            "general",
            "each",
            "Acme",
            "AC-100"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;
        return part.PartId;
    }

    private async Task<Guid> CreateOpenWorkOrderAsync(string token, Guid assetId)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", token);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Parts demand WO",
            string.Empty,
            "medium",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        return created.WorkOrderId;
    }

    private async Task<Guid> SeedAssetOnlyAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            $"DEMAND-ASSET-{Guid.NewGuid():N}".Substring(0, 12),
            "Demand Test Asset",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
    }

    private async Task<Guid> SeedAssetTypeAsync(string token)
    {
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"class-{Guid.NewGuid():N}".Substring(0, 12),
            "Equipment",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"type-{Guid.NewGuid():N}".Substring(0, 12),
            "Machine",
            string.Empty));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;
        return assetType.AssetTypeId;
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync("maintainarr", "http://localhost:5178/launch");
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
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
