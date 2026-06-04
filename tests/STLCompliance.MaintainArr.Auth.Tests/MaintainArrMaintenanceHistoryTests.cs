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
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using AssetClassResponse = MaintainArr.Api.Contracts.AssetClassResponse;
using CreateAssetClassRequest = MaintainArr.Api.Contracts.CreateAssetClassRequest;
using AssetTypeResponse = MaintainArr.Api.Contracts.AssetTypeResponse;
using CreateAssetTypeRequest = MaintainArr.Api.Contracts.CreateAssetTypeRequest;
using AssetResponse = MaintainArr.Api.Contracts.AssetResponse;
using CreateAssetRequest = MaintainArr.Api.Contracts.CreateAssetRequest;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrMaintenanceHistoryTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _handoffServiceToken = null!;
    private string _maintainarrIntegrationToken = null!;
    private string _staffarrSiteLookupToken = null!;
    private string _pmScanServiceToken = null!;
    private RecordingStaffArrSiteLookupHandler _staffarrSiteLookupHandler = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"HistoryNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"HistoryMaintainArr-{Guid.NewGuid():N}";

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
        _handoffServiceToken = await IssueHandoffServiceTokenAsync(adminToken, "maintainarr");
        _maintainarrIntegrationToken = await IssueServiceTokenAsync(adminToken, "maintainarr");
        _staffarrSiteLookupToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            ["staffarr"],
            StaffArrSiteIntegrationScopes.SitesRead,
            "staffarr-sites");
        _pmScanServiceToken = await IssuePmScanServiceTokenAsync(adminToken);
        _staffarrSiteLookupHandler = new RecordingStaffArrSiteLookupHandler(Guid.Parse("5f0b49a9-7c67-4ce1-a0e9-3e7e226d3992"));

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _handoffServiceToken);
            builder.UseSetting("StaffArr:BaseUrl", "http://staffarr.test");
            builder.UseSetting("StaffArr:ServiceToken", _staffarrSiteLookupToken);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrSiteLookupClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrSiteLookupHandler);
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Maintenance_history_aggregates_inspections_defects_work_orders_and_pm()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(token);

        var asset = await GetAssetAsync(token, assetId);
        var componentUpdateRequest = Authorized(HttpMethod.Patch, $"/api/v1/assets/{assetId}", token);
        componentUpdateRequest.Content = JsonContent.Create(new AssetUpsertV1Request(
            asset.AssetTag,
            asset.Name,
            asset.Description,
            new Dictionary<string, object?>
            {
                ["assetClass"] = "powered_industrial_truck",
                ["assetType"] = "forklift",
                ["engineMake"] = "Cummins",
                ["lifecycleStatus"] = "active",
            }));
        var componentUpdateResponse = await _maintainarrClient.SendAsync(componentUpdateRequest);
        componentUpdateResponse.EnsureSuccessStatusCode();
        var lifecycleRequest = Authorized(HttpMethod.Patch, $"/api/v1/assets/{assetId}/lifecycle-status", token);
        lifecycleRequest.Content = JsonContent.Create(new UpdateAssetLifecycleStatusRequest("active"));
        var lifecycleResponse = await _maintainarrClient.SendAsync(lifecycleRequest);
        lifecycleResponse.EnsureSuccessStatusCode();
        var updatedAsset = await GetAssetAsync(token, assetId);
        Assert.Equal("active", updatedAsset.LifecycleStatus);

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var submitRequest = Authorized(HttpMethod.Put, $"/api/inspections/{started.InspectionRunId}/answers", token);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(checklistItemId, "fail", null, null),
        ]));
        await _maintainarrClient.SendAsync(submitRequest);

        var completeRequest = Authorized(HttpMethod.Post, $"/api/inspections/{started.InspectionRunId}/complete", token);
        await _maintainarrClient.SendAsync(completeRequest);

        var defectRequest = Authorized(HttpMethod.Post, "/api/defects", token);
        defectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Manual leak check",
            "Visible seepage",
            "medium"));
        var defectResponse = await _maintainarrClient.SendAsync(defectRequest);
        defectResponse.EnsureSuccessStatusCode();
        var defect = (await defectResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var resolveRequest = Authorized(HttpMethod.Patch, $"/api/defects/{defect.DefectId}/status", token);
        resolveRequest.Content = JsonContent.Create(new UpdateDefectStatusRequest("resolved"));
        await _maintainarrClient.SendAsync(resolveRequest);

        var workOrderRequest = Authorized(HttpMethod.Post, "/api/work-orders", token);
        workOrderRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Seal replacement",
            "Replace worn seal",
            "high",
            null,
            null));
        var workOrderResponse = await _maintainarrClient.SendAsync(workOrderRequest);
        workOrderResponse.EnsureSuccessStatusCode();
        var workOrder = (await workOrderResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        var pmRequest = Authorized(HttpMethod.Post, "/api/preventive-maintenance/schedules", token);
        pmRequest.Content = JsonContent.Create(new CreatePmScheduleRequest(
            assetId,
            "oil-change",
            "Oil Change",
            "Replace engine oil",
            90,
            DateTimeOffset.UtcNow.AddDays(-5)));
        var pmResponse = await _maintainarrClient.SendAsync(pmRequest);
        pmResponse.EnsureSuccessStatusCode();

        var dueScanRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        dueScanRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _pmScanServiceToken);
        dueScanRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            0));
        var dueScanResponse = await _maintainarrClient.SendAsync(dueScanRequest);
        dueScanResponse.EnsureSuccessStatusCode();

        var historyResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/maintenance-history?assetId={assetId}&page=1&pageSize=50", token));
        historyResponse.EnsureSuccessStatusCode();
        var history = (await historyResponse.Content.ReadFromJsonAsync<PagedResult<MaintenanceHistoryEntryResponse>>())!;

        Assert.True(history.TotalCount >= 6);
        Assert.Contains(history.Items, x => x.Category == "inspection" && x.EventType == "inspection_started");
        Assert.Contains(history.Items, x => x.Category == "inspection" && x.EventType == "inspection_completed");
        Assert.Contains(history.Items, x => x.Category == "component" && x.EventType == "component_created");
        Assert.Contains(history.Items, x => x.Category == "defect" && x.EventType == "defect_reported");
        Assert.Contains(history.Items, x => x.Category == "defect" && x.EventType == "defect_resolved");
        Assert.Contains(history.Items, x => x.Category == "work_order" && x.EventType == "work_order_created");
        Assert.Contains(history.Items, x => x.Category == "pm" && x.EventType == "pm_schedule_created");
        Assert.Contains(
            history.Items,
            x => x.Category == "pm" && (x.EventType == "pm_marked_due" || x.EventType == "pm_marked_overdue"));
        Assert.Contains(
            history.Items,
            x => x.Category == "work_order"
                && x.EventType == "work_order_created"
                && x.SourceEntityId == workOrder.WorkOrderId.ToString());
    }

    [Fact]
    public async Task Maintenance_history_includes_repair_progression_events_for_defects()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var defectRequest = Authorized(HttpMethod.Post, "/api/defects", token);
        defectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Repair history defect",
            "Track the repair lifecycle in maintenance history",
            "high"));
        var defectResponse = await _maintainarrClient.SendAsync(defectRequest);
        defectResponse.EnsureSuccessStatusCode();
        var defect = (await defectResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var workOrderRequest = Authorized(HttpMethod.Post, $"/api/defects/{defect.DefectId}/work-orders", token);
        workOrderRequest.Content = JsonContent.Create(new CreateWorkOrderFromDefectRequest(
            "Repair history work order",
            "Exercise the repair lifecycle timeline",
            "high",
            null));
        var workOrderResponse = await _maintainarrClient.SendAsync(workOrderRequest);
        workOrderResponse.EnsureSuccessStatusCode();
        var workOrder = (await workOrderResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        var startRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{workOrder.WorkOrderId}/status", token);
        startRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        await _maintainarrClient.SendAsync(startRequest);

        var evidenceRequest = Authorized(HttpMethod.Post, $"/api/v1/work-orders/{workOrder.WorkOrderId}/evidence", token);
        evidenceRequest.Content = JsonContent.Create(new CreateWorkOrderEvidenceRequest(
            "after_photo",
            "history-after.jpg",
            "image/jpeg",
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("history-after-photo")),
            "History closeout evidence"));
        var evidenceResponse = await _maintainarrClient.SendAsync(evidenceRequest);
        evidenceResponse.EnsureSuccessStatusCode();
        var evidence = (await evidenceResponse.Content.ReadFromJsonAsync<WorkOrderEvidenceResponse>())!;

        var completeRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{workOrder.WorkOrderId}/status", token);
        completeRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("completed"));
        await _maintainarrClient.SendAsync(completeRequest);

        var closeoutRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/integrations/work-orders/{workOrder.WorkOrderId}/closeout",
            _maintainarrIntegrationToken);
        closeoutRequest.Content = JsonContent.Create(new CreateWorkOrderCloseoutRequest(
            "Repair completed and asset returned to service.",
            "wear",
            "Replaced failed seal and verified operation.",
            "Monitor seal wear at next PM.",
            true,
            DateTimeOffset.UtcNow,
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd").ToString(),
            true,
            Guid.NewGuid(),
            true,
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee").ToString(),
            DateTimeOffset.UtcNow,
            false,
            null,
            null,
            false,
            null,
            null,
            true,
            null,
            null,
            "Customer reported brief downtime.",
            "2.5 hours downtime.",
            "ready",
            "closed",
            new[] { evidence.EvidenceId }));
        await _maintainarrClient.SendAsync(closeoutRequest);

        var historyResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/maintenance-history?assetId={assetId}&page=1&pageSize=50", token));
        historyResponse.EnsureSuccessStatusCode();
        var history = (await historyResponse.Content.ReadFromJsonAsync<PagedResult<MaintenanceHistoryEntryResponse>>())!;

        Assert.Contains(history.Items, x => x.Category == "defect" && x.EventType == "defect_reported" && x.SourceEntityId == defect.DefectId.ToString());
        Assert.Contains(history.Items, x => x.Category == "defect" && x.EventType == "defect_in_repair" && x.EntryId.StartsWith($"defect:{defect.DefectId}:in_repair:"));
        Assert.Contains(history.Items, x => x.Category == "defect" && x.EventType == "defect_resolved" && x.SourceEntityId == defect.DefectId.ToString());
        Assert.Contains(history.Items, x => x.Category == "defect" && x.EventType == "defect_closed" && x.SourceEntityId == defect.DefectId.ToString());
        Assert.Contains(history.Items, x => x.Category == "work_order" && x.EventType == "work_order_started" && x.SourceEntityId == workOrder.WorkOrderId.ToString());
        Assert.Contains(history.Items, x => x.Category == "work_order" && x.EventType == "work_order_completed" && x.SourceEntityId == workOrder.WorkOrderId.ToString());
        Assert.Contains(history.Items, x => x.Category == "work_order" && x.EventType == "work_order_closed" && x.SourceEntityType == "work_order_closeout" && x.RelatedEntityId == defect.DefectId.ToString());

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var closeoutTimeline = await db.WorkOrderTimelineEvents
            .AsNoTracking()
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.WorkOrderId == workOrder.WorkOrderId
                && x.EventType == "maintainarr.work_order.closed")
            .OrderByDescending(x => x.OccurredAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(closeoutTimeline);
        Assert.Contains(evidence.EvidenceId.ToString("D"), closeoutTimeline!.AfterSnapshot);
    }

    [Fact]
    public async Task Maintenance_history_pagination_returns_has_next_page_when_events_exceed_page_size()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        for (var i = 0; i < 3; i++)
        {
            var defectRequest = Authorized(HttpMethod.Post, "/api/defects", token);
            defectRequest.Content = JsonContent.Create(new CreateDefectRequest(
                assetId,
                $"Defect {i + 1}",
                string.Empty,
                "low"));
            await _maintainarrClient.SendAsync(defectRequest);
        }

        var pageOneResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/maintenance-history?assetId={assetId}&page=1&pageSize=2", token));
        pageOneResponse.EnsureSuccessStatusCode();
        var pageOne = (await pageOneResponse.Content.ReadFromJsonAsync<PagedResult<MaintenanceHistoryEntryResponse>>())!;
        Assert.Equal(2, pageOne.Items.Count);
        Assert.True(pageOne.HasNextPage);
        Assert.True(pageOne.TotalCount >= 3);

        var pageTwoResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/maintenance-history?assetId={assetId}&page=2&pageSize=2", token));
        pageTwoResponse.EnsureSuccessStatusCode();
        var pageTwo = (await pageTwoResponse.Content.ReadFromJsonAsync<PagedResult<MaintenanceHistoryEntryResponse>>())!;
        Assert.True(pageTwo.Items.Count >= 1);
        Assert.False(pageTwo.HasNextPage);
    }

    [Fact]
    public async Task Maintenance_history_missing_asset_returns_not_found()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var missingAssetId = Guid.NewGuid();

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/maintenance-history?assetId={missingAssetId}", token));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Maintenance_history_v1_alias_missing_asset_returns_not_found()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var missingAssetId = Guid.NewGuid();

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/maintenance-history?assetId={missingAssetId}", token));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Maintenance_history_requires_maintainarr_entitlement()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);
        var unauthorizedToken = CreateMaintainArrAccessToken([], "tenant_member");

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/maintenance-history?assetId={assetId}", unauthorizedToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> SeedAssetOnlyAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            $"HIST-ASSET-{Guid.NewGuid():N}".Substring(0, 12),
            "History Test Asset",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
    }

    private async Task<AssetResponse> GetAssetAsync(string token, Guid assetId)
    {
        var request = Authorized(HttpMethod.Get, $"/api/assets/{assetId}", token);
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AssetResponse>())!;
    }

    private async Task<(Guid AssetId, Guid TemplateId, Guid ChecklistItemId)> SeedActiveTemplateWithAssetAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            $"hist-trip-{Guid.NewGuid():N}".Substring(0, 12),
            "History Pre-Trip",
            "History inspection template"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "brakes-ok",
            "Brakes operate correctly",
            "pass_fail",
            true,
            10,
            null));
        var createItemResponse = await _maintainarrClient.SendAsync(createItemRequest);
        createItemResponse.EnsureSuccessStatusCode();
        var item = (await createItemResponse.Content.ReadFromJsonAsync<InspectionChecklistItemResponse>())!;

        var replaceAssetTypesRequest = Authorized(
            HttpMethod.Put,
            $"/api/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([assetTypeId]));
        await _maintainarrClient.SendAsync(replaceAssetTypesRequest);

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        await _maintainarrClient.SendAsync(activateRequest);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            $"HIST-RUN-{Guid.NewGuid():N}".Substring(0, 12),
            "History Inspection Asset",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        return (asset.AssetId, template.InspectionTemplateId, item.ChecklistItemId);
    }

    private async Task<Guid> SeedAssetTypeAsync(string token)
    {
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            "vehicles",
            "Vehicles",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            "forklift",
            "Forklift",
            string.Empty));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;
        return assetType.AssetTypeId;
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new NexArr.Api.Contracts.CreateHandoffRequest(
            "maintainarr",
            "http://localhost:5178/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueHandoffServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{productKey}-history-handoff-test",
            $"{productKey} History Handoff Test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private async Task<string> IssuePmScanServiceTokenAsync(string adminToken)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"shared-worker-history-{Guid.NewGuid():N}",
            "History PM due scan test",
            "shared-worker",
            ["maintainarr"]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            ["maintainarr"],
            PmDueScanService.ProcessDueScanActionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string productKey,
        string[]? targetProducts = null,
        string actionScope = "launch.redeem",
        string clientSuffix = "history-test")
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{productKey}-{clientSuffix}",
            $"{productKey} History Test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            targetProducts,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        Guid? userIdOverride = null)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
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
            new NexArr.Api.Contracts.LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.AuthTokenResponse>())!;
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

    private sealed class RecordingStaffArrSiteLookupHandler(Guid siteOrgUnitId) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (!path.Contains("/api/v1/integrations/sites", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var response = new StaffArrSiteLookupResponse(
                siteOrgUnitId,
                "Central Maintenance Site",
                null,
                "active");

            if (path.EndsWith("/sites", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[] { response })
                });
            }

            if (path.EndsWith($"/{siteOrgUnitId:D}", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(response)
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
