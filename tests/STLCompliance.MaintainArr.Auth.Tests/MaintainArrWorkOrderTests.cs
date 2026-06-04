using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
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

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrWorkOrderTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _maintainarrIntegrationToken = null!;
    private RecordingTrainArrQualificationCheckHandler _trainarrQualificationHandler = null!;
    private RecordingComplianceCoreWorkOrderGateHandler _complianceCoreGateHandler = null!;
    private RecordingComplianceCoreAssetReadinessGateHandler _complianceCoreReadinessGateHandler = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"WorkOrderNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"WorkOrderMaintainArr-{Guid.NewGuid():N}";

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
        _maintainarrIntegrationToken = await IssueServiceTokenAsync(adminToken, "maintainarr");
        _trainarrQualificationHandler = new RecordingTrainArrQualificationCheckHandler();
        _complianceCoreGateHandler = new RecordingComplianceCoreWorkOrderGateHandler();
        _complianceCoreReadinessGateHandler = new RecordingComplianceCoreAssetReadinessGateHandler();

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _maintainarrIntegrationToken);
            builder.UseSetting("TrainArr:BaseUrl", "http://trainarr.test");
            builder.UseSetting("TrainArr:ServiceToken", "maintainarr-to-trainarr-token");
            builder.UseSetting("TrainArr:TechnicianQualificationKey", "maintenance_technician");
            builder.UseSetting("TrainArr:TechnicianRulePackKey", "maintenance_qualification");
            builder.UseSetting("ComplianceCore:BaseUrl", "http://compliancecore.test");
            builder.UseSetting("ComplianceCore:ServiceToken", "maintainarr-to-compliancecore-token");
            builder.UseSetting("ComplianceCore:WorkOrderActionKey", "can-perform-maintenance");
            builder.UseSetting("ComplianceCore:WorkOrderWorkflowKey", "can_perform_maintenance");
            builder.UseSetting("ComplianceCore:WorkOrderActivityContextKey", "maintenance_work_order");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<TrainArrQualificationCheckClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _trainarrQualificationHandler);
                services.AddHttpClient<ComplianceCoreWorkOrderGateClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreGateHandler);
                services.AddHttpClient<ComplianceCoreAssetReadinessGateClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreReadinessGateHandler);
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
    public async Task Manual_work_order_create_and_status_lifecycle()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Replace hydraulic hose",
            "Left cylinder hose leaking",
            "high",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("open", created.Status);
        Assert.Equal("manual", created.Source);
        Assert.StartsWith("WO-", created.WorkOrderNumber);

        var startRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{created.WorkOrderId}/status", managerToken);
        startRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var inProgress = (await startResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("in_progress", inProgress.Status);
        Assert.NotNull(inProgress.StartedAt);
        Assert.NotNull(inProgress.DowntimeFollowUp);
        Assert.Equal("work_order_started", inProgress.DowntimeFollowUp!.Trigger);
        Assert.Contains($"/downtime?assetId={assetId:D}", inProgress.DowntimeFollowUp.DeepLinkPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"workOrderId={created.WorkOrderId:D}", inProgress.DowntimeFollowUp.DeepLinkPath, StringComparison.OrdinalIgnoreCase);

        using (var scope = _maintainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
            var downtimeEvent = await db.AssetDowntimeEvents.SingleAsync(
                x => x.WorkOrderId == created.WorkOrderId && x.EndedAt == null);
            Assert.Equal(AssetDowntimeReasons.InRepair, downtimeEvent.Reason);
        }

        var completeRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{created.WorkOrderId}/status", managerToken);
        completeRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("completed"));
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("completed", completed.Status);
        Assert.NotNull(completed.CompletedAt);
    }

    [Fact]
    public async Task Manual_work_order_v1_alias_create_and_get()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "V1 work order",
            "Alias verification",
            "medium",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.StartsWith($"/api/v1/work-orders/{created.WorkOrderId}", createResponse.Headers.Location?.OriginalString);

        var getRequest = Authorized(HttpMethod.Get, $"/api/v1/work-orders/{created.WorkOrderId}", managerToken);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var fetched = (await getResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        Assert.Equal(created.WorkOrderId, fetched.WorkOrderId);
    }

    [Fact]
    public async Task Create_work_order_from_defect_is_idempotent()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var defectRequest = Authorized(HttpMethod.Post, "/api/defects", managerToken);
        defectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Bearing noise",
            "Audible grinding on rotation",
            "high"));
        var defectResponse = await _maintainarrClient.SendAsync(defectRequest);
        defectResponse.EnsureSuccessStatusCode();
        var defect = (await defectResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var firstRequest = Authorized(HttpMethod.Post, $"/api/defects/{defect.DefectId}/work-orders", managerToken);
        firstRequest.Content = JsonContent.Create(new CreateWorkOrderFromDefectRequest(null, null, null, null));
        var firstResponse = await _maintainarrClient.SendAsync(firstRequest);
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("defect", first.Source);
        Assert.Equal(defect.DefectId, first.DefectId);

        var secondRequest = Authorized(HttpMethod.Post, $"/api/defects/{defect.DefectId}/work-orders", managerToken);
        secondRequest.Content = JsonContent.Create(new CreateWorkOrderFromDefectRequest(null, null, null, null));
        var secondResponse = await _maintainarrClient.SendAsync(secondRequest);
        secondResponse.EnsureSuccessStatusCode();
        var second = (await secondResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal(first.WorkOrderId, second.WorkOrderId);
    }

    [Fact]
    public async Task Create_work_order_from_defect_v1_alias_is_idempotent()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var defectRequest = Authorized(HttpMethod.Post, "/api/v1/defects", managerToken);
        defectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "V1 bearing noise",
            "v1 defect route",
            "high"));
        var defectResponse = await _maintainarrClient.SendAsync(defectRequest);
        defectResponse.EnsureSuccessStatusCode();
        var defect = (await defectResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var firstRequest = Authorized(HttpMethod.Post, $"/api/v1/defects/{defect.DefectId}/work-orders", managerToken);
        firstRequest.Content = JsonContent.Create(new CreateWorkOrderFromDefectRequest(null, null, null, null));
        var firstResponse = await _maintainarrClient.SendAsync(firstRequest);
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.StartsWith($"/api/v1/work-orders/{first.WorkOrderId}", firstResponse.Headers.Location?.OriginalString);

        var secondRequest = Authorized(HttpMethod.Post, $"/api/v1/defects/{defect.DefectId}/work-orders", managerToken);
        secondRequest.Content = JsonContent.Create(new CreateWorkOrderFromDefectRequest(null, null, null, null));
        var secondResponse = await _maintainarrClient.SendAsync(secondRequest);
        secondResponse.EnsureSuccessStatusCode();
        var second = (await secondResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.StartsWith($"/api/v1/work-orders/{second.WorkOrderId}", secondResponse.Headers.Location?.OriginalString);

        Assert.Equal(first.WorkOrderId, second.WorkOrderId);
    }

    [Fact]
    public async Task Work_order_from_defect_moves_defect_through_repair_and_closeout()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var defectRequest = Authorized(HttpMethod.Post, "/api/defects", managerToken);
        defectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Hydraulic leak",
            "Fluid seepage around the rear seal",
            "high"));
        var defectResponse = await _maintainarrClient.SendAsync(defectRequest);
        defectResponse.EnsureSuccessStatusCode();
        var defect = (await defectResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var createRequest = Authorized(HttpMethod.Post, $"/api/defects/{defect.DefectId}/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderFromDefectRequest(
            "Hydraulic seal repair",
            "Replace the failed rear seal and verify lift operation.",
            "high",
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var workOrder = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        var startRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{workOrder.WorkOrderId}/status", managerToken);
        startRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();

        var inRepairDefect = await FetchDefectAsync(managerToken, defect.DefectId);
        Assert.Equal("in_repair", inRepairDefect.Status);

        var completeRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{workOrder.WorkOrderId}/status", managerToken);
        completeRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("completed"));
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();

        var resolvedDefect = await FetchDefectAsync(managerToken, defect.DefectId);
        Assert.Equal("resolved", resolvedDefect.Status);
        Assert.NotNull(resolvedDefect.ResolvedAt);

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
            "closed"));
        var closeoutResponse = await _maintainarrClient.SendAsync(closeoutRequest);
        closeoutResponse.EnsureSuccessStatusCode();

        var closedDefect = await FetchDefectAsync(managerToken, defect.DefectId);
        Assert.Equal("closed", closedDefect.Status);
        Assert.NotNull(closedDefect.ResolvedAt);
    }

    [Fact]
    public async Task Technician_cannot_view_other_users_unassigned_work_order()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Manager-only WO",
            string.Empty,
            "medium",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        var technicianToken = CreateMaintainArrAccessToken(
            ["maintainarr"],
            "maintainarr_technician",
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

        var peekRequest = Authorized(HttpMethod.Get, $"/api/work-orders/{created.WorkOrderId}", technicianToken);
        var peekResponse = await _maintainarrClient.SendAsync(peekRequest);
        Assert.Equal(HttpStatusCode.Forbidden, peekResponse.StatusCode);
    }

    [Fact]
    public async Task Technician_can_complete_assigned_work_order()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var technicianPersonId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb").ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Assigned repair",
            string.Empty,
            "medium",
            technicianPersonId,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        var technicianToken = CreateMaintainArrAccessToken(
            ["maintainarr"],
            "maintainarr_technician",
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

        var startRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{created.WorkOrderId}/status", technicianToken);
        startRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();

        var completeRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{created.WorkOrderId}/status", technicianToken);
        completeRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("completed"));
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("completed", completed.Status);
    }

    [Fact]
    public async Task Work_order_start_checks_compliancecore_maintenance_gate()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Compliance-gated repair",
            "Verify Compliance Core before starting maintenance work.",
            "high",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        _complianceCoreGateHandler.NextOutcome = "block";
        _complianceCoreGateHandler.NextReasonCode = "missing_permit";
        _complianceCoreGateHandler.NextMessage = "A maintenance permit is required before work can start.";

        var blockedStartRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{created.WorkOrderId}/status", managerToken);
        blockedStartRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        var blockedStartResponse = await _maintainarrClient.SendAsync(blockedStartRequest);
        Assert.Equal(HttpStatusCode.Conflict, blockedStartResponse.StatusCode);

        var unchangedRequest = Authorized(HttpMethod.Get, $"/api/work-orders/{created.WorkOrderId}", managerToken);
        var unchangedResponse = await _maintainarrClient.SendAsync(unchangedRequest);
        unchangedResponse.EnsureSuccessStatusCode();
        var unchanged = (await unchangedResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("open", unchanged.Status);
        Assert.Null(unchanged.StartedAt);

        var blockedGate = Assert.Single(_complianceCoreGateHandler.Requests);
        Assert.Equal("/api/v1/gates/can-perform-maintenance", blockedGate.Path);
        Assert.Equal("Bearer", blockedGate.AuthorizationScheme);
        Assert.Equal("maintainarr-to-compliancecore-token", blockedGate.AuthorizationParameter);
        Assert.Equal(PlatformSeeder.DemoTenantId, blockedGate.TenantId);
        Assert.Equal("maintenance_work_order", blockedGate.ActivityContextKey);
        Assert.Equal("can_perform_maintenance", blockedGate.WorkflowKey);
        Assert.Contains(blockedGate.Subjects, subject =>
            subject.SubjectType == "work_order"
            && subject.SubjectReference == created.WorkOrderId.ToString("D")
            && subject.SourceProduct == "maintainarr");
        Assert.Contains(blockedGate.Subjects, subject =>
            subject.SubjectType == "asset"
            && subject.SubjectReference == assetId.ToString("D")
            && subject.SourceProduct == "maintainarr");
        Assert.Equal(created.WorkOrderId.ToString("D"), blockedGate.RuleContext["work_order_id"]);
        Assert.Equal(assetId.ToString("D"), blockedGate.RuleContext["asset_id"]);
        Assert.Equal("open", blockedGate.RuleContext["from_status"]);
        Assert.Equal("in_progress", blockedGate.RuleContext["to_status"]);

        _complianceCoreGateHandler.NextOutcome = "allow";
        _complianceCoreGateHandler.NextReasonCode = "maintenance_clear";
        _complianceCoreGateHandler.NextMessage = "Maintenance work may proceed.";

        var allowedStartRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{created.WorkOrderId}/status", managerToken);
        allowedStartRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        var allowedStartResponse = await _maintainarrClient.SendAsync(allowedStartRequest);
        allowedStartResponse.EnsureSuccessStatusCode();
        var started = (await allowedStartResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("in_progress", started.Status);
        Assert.NotNull(started.StartedAt);
        Assert.Equal(2, _complianceCoreGateHandler.Requests.Count);
    }

    [Fact]
    public async Task Integration_work_order_create_get_and_status_update_use_real_service_data()
    {
        var assetId = await SeedIntegrationAssetAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/work-orders", _maintainarrIntegrationToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Integration hose replacement",
            "Created through integration API",
            "high",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal(assetId, created.AssetId);
        Assert.Equal("manual", created.Source);
        Assert.Equal("open", created.Status);

        var getRequest = Authorized(HttpMethod.Get, $"/api/v1/integrations/work-orders/{created.WorkOrderId}", _maintainarrIntegrationToken);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var fetched = (await getResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal(created.WorkOrderId, fetched.WorkOrderId);
        Assert.Equal(created.WorkOrderNumber, fetched.WorkOrderNumber);

        var statusRequest = Authorized(HttpMethod.Post, $"/api/v1/integrations/work-orders/{created.WorkOrderId}/status-updates", _maintainarrIntegrationToken);
        statusRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        var statusResponse = await _maintainarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var started = (await statusResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("in_progress", started.Status);
        Assert.NotNull(started.StartedAt);
    }

    [Fact]
    public async Task Integration_work_order_blocker_and_closeout_use_real_service_data()
    {
        var assetId = await SeedIntegrationAssetAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/work-orders", _maintainarrIntegrationToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Integration blocker repair",
            "Created to verify blocker and closeout persistence",
            "high",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        var blockerRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/integrations/work-orders/{created.WorkOrderId}/blockers",
            _maintainarrIntegrationToken);
        blockerRequest.Content = JsonContent.Create(new CreateWorkOrderBlockerRequest(
            "quality_hold",
            "assurarr",
            "quality-hold-123",
            "Quality hold active",
            "Asset cannot return to service until hold is cleared.",
            "high",
            "Release the external quality hold.",
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc").ToString(),
            null));
        var blockerResponse = await _maintainarrClient.SendAsync(blockerRequest);
        blockerResponse.EnsureSuccessStatusCode();
        var blocker = (await blockerResponse.Content.ReadFromJsonAsync<WorkOrderBlockerResponse>())!;
        Assert.Equal(created.WorkOrderId, blocker.WorkOrderId);
        Assert.Equal("active", blocker.Status);

        var startRequest = Authorized(HttpMethod.Post, $"/api/v1/integrations/work-orders/{created.WorkOrderId}/status-updates", _maintainarrIntegrationToken);
        startRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();

        var completeRequest = Authorized(HttpMethod.Post, $"/api/v1/integrations/work-orders/{created.WorkOrderId}/status-updates", _maintainarrIntegrationToken);
        completeRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("completed"));
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();

        var readinessRequest = Authorized(HttpMethod.Get, $"/api/v1/integrations/assets/{assetId}/readiness", _maintainarrIntegrationToken);
        var readinessResponse = await _maintainarrClient.SendAsync(readinessRequest);
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;
        Assert.Contains(readiness.Blockers, item =>
            item.BlockerType == "quality_hold"
            && item.SourceEntityType == "work_order_blocker"
            && item.SourceEntityId == blocker.BlockerId.ToString());

        var closeoutRequest = Authorized(HttpMethod.Post, $"/api/v1/integrations/work-orders/{created.WorkOrderId}/closeout", _maintainarrIntegrationToken);
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
            "closed"));
        var closeoutResponse = await _maintainarrClient.SendAsync(closeoutRequest);
        closeoutResponse.EnsureSuccessStatusCode();
        var closeout = (await closeoutResponse.Content.ReadFromJsonAsync<WorkOrderCloseoutResponse>())!;
        Assert.Equal(created.WorkOrderId, closeout.WorkOrderId);
        Assert.Equal("closed", closeout.FinalStatus);

        var getRequest = Authorized(HttpMethod.Get, $"/api/v1/integrations/work-orders/{created.WorkOrderId}", _maintainarrIntegrationToken);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var fetched = (await getResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Single(fetched.Blockers);
        Assert.NotNull(fetched.Closeout);
        Assert.Equal("closed", fetched.Closeout!.FinalStatus);
    }

    [Fact]
    public async Task Integration_work_order_comments_and_timeline_use_real_service_data()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedIntegrationAssetAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/work-orders", _maintainarrIntegrationToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Integration comment test",
            "Created to verify comments and timeline persistence",
            "medium",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        var commentRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/work-orders/{created.WorkOrderId}/comments",
            managerToken);
        commentRequest.Content = JsonContent.Create(new CreateWorkOrderCommentRequest(
            "Technician handoff is waiting on final QA confirmation.",
            "supervisor_only",
            true));
        var commentResponse = await _maintainarrClient.SendAsync(commentRequest);
        commentResponse.EnsureSuccessStatusCode();
        var comment = (await commentResponse.Content.ReadFromJsonAsync<WorkOrderCommentResponse>())!;
        Assert.Equal("supervisor_only", comment.Visibility);
        Assert.True(comment.Pinned);

        var commentsRequest = Authorized(
            HttpMethod.Get,
            $"/api/v1/work-orders/{created.WorkOrderId}/comments",
            managerToken);
        var commentsResponse = await _maintainarrClient.SendAsync(commentsRequest);
        commentsResponse.EnsureSuccessStatusCode();
        var comments = (await commentsResponse.Content.ReadFromJsonAsync<WorkOrderCommentResponse[]>())!;
        Assert.Single(comments);
        Assert.Equal(comment.CommentId, comments[0].CommentId);

        var timelineRequest = Authorized(
            HttpMethod.Get,
            $"/api/v1/work-orders/{created.WorkOrderId}/timeline",
            managerToken);
        var timelineResponse = await _maintainarrClient.SendAsync(timelineRequest);
        timelineResponse.EnsureSuccessStatusCode();
        var timeline = (await timelineResponse.Content.ReadFromJsonAsync<WorkOrderTimelineEventResponse[]>())!;
        Assert.Contains(timeline, item => item.EventType == "maintainarr.work_order.created");
        Assert.Contains(timeline, item => item.EventType == "maintainarr.work_order.comment_added");
    }

    [Fact]
    public async Task Integration_quality_hold_blocks_and_release_clears_asset_readiness()
    {
        var assetId = await SeedIntegrationAssetAsync();

        var holdRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/quality-holds", _maintainarrIntegrationToken);
        holdRequest.Content = JsonContent.Create(new CreateAssetQualityHoldRequest(
            assetId,
            "quality_hold",
            "assurarr",
            "hold-001",
            "Quality hold",
            "Asset cannot return to service.",
            "high",
            null));
        var holdResponse = await _maintainarrClient.SendAsync(holdRequest);
        holdResponse.EnsureSuccessStatusCode();
        var hold = (await holdResponse.Content.ReadFromJsonAsync<AssetQualityHoldResponse>())!;

        var readinessRequest = Authorized(HttpMethod.Get, $"/api/v1/integrations/assets/{assetId}/readiness", _maintainarrIntegrationToken);
        var readinessResponse = await _maintainarrClient.SendAsync(readinessRequest);
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;
        Assert.Contains(readiness.Blockers, blocker =>
            blocker.BlockerType == "quality_hold"
            && blocker.SourceEntityType == "asset_quality_hold"
            && blocker.SourceEntityId == hold.HoldId.ToString());

        var releaseRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/quality-hold-releases", _maintainarrIntegrationToken);
        releaseRequest.Content = JsonContent.Create(new ReleaseAssetQualityHoldRequest(
            hold.HoldId,
            null,
            "Released after review."));
        var releaseResponse = await _maintainarrClient.SendAsync(releaseRequest);
        releaseResponse.EnsureSuccessStatusCode();
        var released = (await releaseResponse.Content.ReadFromJsonAsync<AssetQualityHoldResponse>())!;
        Assert.Equal("resolved", released.Status);

        var readinessAfterReleaseRequest = Authorized(HttpMethod.Get, $"/api/v1/integrations/assets/{assetId}/readiness", _maintainarrIntegrationToken);
        var readinessAfterReleaseResponse = await _maintainarrClient.SendAsync(readinessAfterReleaseRequest);
        readinessAfterReleaseResponse.EnsureSuccessStatusCode();
        var readinessAfterRelease = (await readinessAfterReleaseResponse.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;
        Assert.DoesNotContain(readinessAfterRelease.Blockers, blocker => blocker.SourceEntityId == hold.HoldId.ToString());
    }

    [Fact]
    public async Task Integration_part_issue_event_updates_demand_status()
    {
        var assetId = await SeedIntegrationAssetAsync();
        var tenantId = GetIntegrationTenantId();

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/work-orders", _maintainarrIntegrationToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Integration issue test",
            "Created to verify part issue ingestion",
            "medium",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        using (var scope = _maintainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
            db.WorkOrderPartsDemandLines.Add(new WorkOrderPartsDemandLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkOrderId = created.WorkOrderId,
                LineNumber = 1,
                SupplyarrPartId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                PartNumber = "FILTER-123",
                Description = "Air filter",
                QuantityRequested = 3m,
                UnitOfMeasure = "each",
                Notes = string.Empty,
                Status = WorkOrderPartsDemandStatuses.Published,
                MaintainarrPublicationId = Guid.NewGuid(),
                SupplyarrDemandRefId = Guid.NewGuid(),
                PublishedAt = DateTimeOffset.UtcNow,
                ProcurementStatus = WorkOrderPartsDemandProcurementStatuses.AwaitingProcurement,
                QuantityReceived = 0m,
                ProcurementStatusMessage = string.Empty,
                CreatedByUserId = PlatformSeeder.DemoAdminUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        var issueRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/part-issue-events", _maintainarrIntegrationToken);
        issueRequest.Content = JsonContent.Create(new IngestPartIssueEventRequest(
            tenantId,
            created.WorkOrderId,
            Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            null,
            3m,
            "each",
            "loadarr-issue-001",
            "Issued to maintenance",
            DateTimeOffset.UtcNow));
        var issueResponse = await _maintainarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issue = (await issueResponse.Content.ReadFromJsonAsync<IngestPartIssueEventResponse>())!;
        Assert.Equal(created.WorkOrderId, issue.WorkOrderId);
        Assert.Equal(3m, issue.QuantityIssued);
        Assert.Equal("received_complete", issue.Status);

        using var verifyScope = _maintainarrFactory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var line = await verifyDb.WorkOrderPartsDemandLines.SingleAsync(x => x.WorkOrderId == created.WorkOrderId);
        Assert.Equal(3m, line.QuantityReceived);
        Assert.Equal(WorkOrderPartsDemandProcurementStatuses.ReceivedComplete, line.ProcurementStatus);
    }

    [Fact]
    public async Task Integration_supplier_work_status_upserts_vendor_work()
    {
        var assetId = await SeedIntegrationAssetAsync();
        var tenantId = GetIntegrationTenantId();

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/work-orders", _maintainarrIntegrationToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Integration vendor work test",
            "Created to verify vendor work ingestion",
            "medium",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        var vendorRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/supplier-work-status", _maintainarrIntegrationToken);
        vendorRequest.Content = JsonContent.Create(new IngestSupplierWorkStatusRequest(
            tenantId,
            created.WorkOrderId,
            "supplier-123",
            "Vendor contact snapshot",
            "scheduled",
            "Replace alternator cover",
            "quote-001",
            "approval-001",
            DateTimeOffset.UtcNow.AddDays(2),
            null,
            "1200.00",
            null,
            true,
            "Vendor scheduled on site",
            DateTimeOffset.UtcNow));
        var vendorResponse = await _maintainarrClient.SendAsync(vendorRequest);
        vendorResponse.EnsureSuccessStatusCode();
        var vendor = (await vendorResponse.Content.ReadFromJsonAsync<MaintenanceVendorWorkResponse>())!;
        Assert.False(vendor.Duplicate);
        Assert.Equal("scheduled", vendor.Status);

        using var verifyScope = _maintainarrFactory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var stored = await verifyDb.MaintenanceVendorWorks.SingleAsync(x => x.WorkOrderId == created.WorkOrderId);
        Assert.Equal("supplier-123", stored.SupplierRef);
        Assert.Equal("scheduled", stored.Status);
    }

    [Fact]
    public async Task Integration_asset_readiness_check_records_evaluation()
    {
        var assetId = await SeedIntegrationAssetAsync();

        var request = Authorized(HttpMethod.Post, "/api/v1/integrations/asset-readiness-checks", _maintainarrIntegrationToken);
        request.Content = JsonContent.Create(new CreateAssetReadinessCheckRequest(
            assetId,
            null,
            null,
            "routarr",
            "dispatch-system",
            "requested"));
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var check = (await response.Content.ReadFromJsonAsync<AssetReadinessCheckResponse>())!;
        Assert.Equal(assetId, check.AssetId);
        Assert.Equal("routarr", check.SourceProduct);
        Assert.Equal("requested", check.Status);
        Assert.False(string.IsNullOrWhiteSpace(check.ReadinessStatus));

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var stored = await db.AssetReadinessChecks.SingleAsync(x => x.Id == check.AssetReadinessCheckId);
        Assert.Equal(check.ReadinessBasis, stored.ReadinessBasis);
    }

    [Fact]
    public async Task Integration_defect_create_get_and_status_update_use_real_service_data()
    {
        var assetId = await SeedIntegrationAssetAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/defects", _maintainarrIntegrationToken);
        createRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Integration defect",
            "Created through integration API",
            "high"));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;
        Assert.Equal(assetId, created.AssetId);
        Assert.Equal("open", created.Status);

        var getRequest = Authorized(HttpMethod.Get, $"/api/v1/integrations/defects/{created.DefectId}", _maintainarrIntegrationToken);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var fetched = (await getResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;
        Assert.Equal(created.DefectId, fetched.DefectId);
        Assert.Equal(created.Title, fetched.Title);

        var statusRequest = Authorized(HttpMethod.Post, $"/api/v1/integrations/defects/{created.DefectId}/status-updates", _maintainarrIntegrationToken);
        statusRequest.Content = JsonContent.Create(new UpdateDefectStatusRequest("resolved"));
        var statusResponse = await _maintainarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var resolved = (await statusResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;
        Assert.Equal("resolved", resolved.Status);
        Assert.NotNull(resolved.ResolvedAt);
    }

    [Fact]
    public async Task Integration_asset_list_get_and_readiness_use_real_service_data()
    {
        var assetId = await SeedIntegrationAssetAsync();

        var listRequest = Authorized(HttpMethod.Get, "/api/v1/integrations/assets", _maintainarrIntegrationToken);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var listPayload = (await listResponse.Content.ReadFromJsonAsync<global::MaintainArr.Api.Endpoints.MaintainArrIntegrationListResponse<AssetResponse>>())!;
        Assert.Contains(listPayload.Items, asset => asset.AssetId == assetId);

        var getRequest = Authorized(HttpMethod.Get, $"/api/v1/integrations/assets/{assetId}", _maintainarrIntegrationToken);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var asset = (await getResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        Assert.Equal(assetId, asset.AssetId);

        var readinessRequest = Authorized(HttpMethod.Get, $"/api/v1/integrations/assets/{assetId}/readiness", _maintainarrIntegrationToken);
        var readinessResponse = await _maintainarrClient.SendAsync(readinessRequest);
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;
        Assert.Equal(assetId, readiness.AssetId);
    }

    [Fact]
    public async Task Assigned_work_order_requests_trainarr_technician_qualification_check()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var technicianPersonId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Qualified technician repair",
            "Verify TrainArr qualification before assignment",
            "medium",
            technicianPersonId.ToString(),
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        Assert.Equal("/api/v1/integrations/qualification-check", _trainarrQualificationHandler.RequestPath);
        Assert.Equal("Bearer", _trainarrQualificationHandler.AuthorizationScheme);
        Assert.Equal("maintainarr-to-trainarr-token", _trainarrQualificationHandler.AuthorizationParameter);
        Assert.NotNull(_trainarrQualificationHandler.Payload);
        var payload = _trainarrQualificationHandler.Payload.Value;
        Assert.Equal(PlatformSeeder.DemoTenantId, payload.GetProperty("tenantId").GetGuid());
        Assert.Equal(technicianPersonId, payload.GetProperty("staffarrPersonId").GetGuid());
        Assert.Equal("maintenance_technician", payload.GetProperty("qualificationKey").GetString());
        Assert.Equal("maintenance_qualification", payload.GetProperty("rulePackKey").GetString());
    }

    [Fact]
    public async Task Assigned_work_order_blocks_when_trainarr_technician_qualification_fails()
    {
        _trainarrQualificationHandler.Outcome = "block";
        _trainarrQualificationHandler.Message = "Technician qualification is expired.";
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Blocked technician repair",
            "This assignment should be blocked by TrainArr.",
            "medium",
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb").ToString(),
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);

        Assert.Equal(HttpStatusCode.Conflict, createResponse.StatusCode);
    }

    [Fact]
    public async Task Technician_cannot_cancel_work_order()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var technicianPersonId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb").ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Cancel gate test",
            string.Empty,
            "low",
            technicianPersonId,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        var technicianToken = CreateMaintainArrAccessToken(
            ["maintainarr"],
            "maintainarr_technician",
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

        var cancelRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{created.WorkOrderId}/status", technicianToken);
        cancelRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("cancelled"));
        var cancelResponse = await _maintainarrClient.SendAsync(cancelRequest);
        Assert.Equal(HttpStatusCode.Forbidden, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task Asset_components_endpoint_returns_installed_components()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        using (var scope = _maintainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
            var asset = await db.Assets.FirstAsync(x => x.Id == assetId);
            var now = DateTimeOffset.UtcNow;

            db.AssetInstalledComponents.Add(new AssetInstalledComponent
            {
                Id = Guid.NewGuid(),
                TenantId = asset.TenantId,
                ComponentNumber = "ENG-01",
                ParentAssetId = assetId,
                ParentComponentId = null,
                Name = "Primary engine",
                Description = "Main power unit",
                ComponentType = "engine",
                Status = "installed",
                Make = "Cummins",
                Model = "X15",
                SerialNumber = "SER-123",
                PartNumberSnapshot = "PN-999",
                InstalledPartUsageRef = "usage-1",
                InstallDate = now.AddDays(-1),
                InstalledByPersonId = "person-2",
                InstalledMeterReading = 1234,
                RemovedDate = null,
                RemovedByPersonId = null,
                RemovedMeterReading = null,
                RemovalReason = null,
                WarrantyStartDate = now.AddDays(-1),
                WarrantyEndDate = now.AddYears(1),
                ExpectedLifeHours = 5000,
                ExpectedLifeMiles = 250000,
                ExpectedLifeCycles = 1000,
                Condition = "good",
                ReplacementPartRefsJson = JsonSerializer.Serialize(new[] { "part-1" }),
                DocumentRefsJson = JsonSerializer.Serialize(new[] { "doc-1" }),
                DefectRefsJson = JsonSerializer.Serialize(Array.Empty<string>()),
                WorkOrderRefsJson = JsonSerializer.Serialize(new[] { "wo-1" }),
                CreatedAt = now,
                UpdatedAt = now,
            });

            await db.SaveChangesAsync();
        }

        var response = await _maintainarrClient.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/assets/{assetId}/components", managerToken));
        response.EnsureSuccessStatusCode();

        var components = (await response.Content.ReadFromJsonAsync<List<AssetInstalledComponentResponse>>())!;
        Assert.Single(components);

        var component = components[0];
        Assert.Equal("ENG-01", component.ComponentNumber);
        Assert.Equal("Primary engine", component.Name);
        Assert.Equal("engine", component.ComponentType);
        Assert.Equal("installed", component.Status);
        Assert.Equal("Cummins", component.Make);
        Assert.Equal("X15", component.Model);
        Assert.Equal(new[] { "part-1" }, component.ReplacementPartRefs);
        Assert.Equal(new[] { "doc-1" }, component.DocumentRefs);
        Assert.Equal(new[] { "wo-1" }, component.WorkOrderRefs);
    }

    private async Task<Guid> SeedAssetOnlyAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            $"WO-ASSET-{Guid.NewGuid():N}".Substring(0, 12),
            "Work Order Test Asset",
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
            $"vehicles-{Guid.NewGuid():N}".Substring(0, 12),
            "Vehicles",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"forklift-{Guid.NewGuid():N}".Substring(0, 12),
            "Forklift",
            string.Empty));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;
        return assetType.AssetTypeId;
    }

    private async Task<DefectDetailResponse> FetchDefectAsync(string token, Guid defectId)
    {
        var request = Authorized(HttpMethod.Get, $"/api/defects/{defectId}", token);
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DefectDetailResponse>())!;
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

    private Guid GetIntegrationTenantId()
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<StlServiceTokenValidator>();
        var preview = validator.TryValidate(ServiceTokenBearerParser.ParseAuthorizationHeader($"Bearer {_maintainarrIntegrationToken}"))
            ?? throw new InvalidOperationException("MaintainArr integration token could not be decoded.");
        return preview.TenantScope ?? Guid.Empty;
    }

    private async Task<Guid> SeedIntegrationAssetAsync()
    {
        var tenantId = GetIntegrationTenantId();
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();

        var assetClassId = Guid.NewGuid();
        var assetTypeId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.AssetClasses.Add(new AssetClass
        {
            Id = assetClassId,
            TenantId = tenantId,
            ClassKey = $"integration-class-{assetClassId:N}"[..20],
            Name = "Integration Class",
            Description = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.AssetTypes.Add(new AssetType
        {
            Id = assetTypeId,
            TenantId = tenantId,
            AssetClassId = assetClassId,
            TypeKey = $"integration-type-{assetTypeId:N}"[..20],
            Name = "Integration Type",
            Description = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.Assets.Add(new Asset
        {
            Id = assetId,
            TenantId = tenantId,
            AssetTypeId = assetTypeId,
            AssetTag = $"INTEGRATION-{assetId:N}"[..20],
            Name = "Integration Asset",
            Description = "Seeded for integration route coverage",
            LifecycleStatus = "active",
            SiteRef = null,
            StaffarrSiteOrgUnitId = null,
            StaffarrSiteNameSnapshot = string.Empty,
            CreatedAt = now,
            UpdatedAt = now
        });

        await db.SaveChangesAsync();
        return assetId;
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

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{productKey}-wo-test",
            $"{productKey} WO Test",
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

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        Guid? userIdOverride = null)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var userId = userIdOverride
            ?? (tenantRoleKey == "maintainarr_technician"
                ? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
                : PlatformSeeder.DemoAdminUserId);
        var personId = userIdOverride
            ?? (tenantRoleKey == "maintainarr_technician"
                ? Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
                : PlatformSeeder.DemoAdminUserId);
        var (token, _) = tokenService.CreateAccessToken(
            userId,
            personId,
            tenantRoleKey == "maintainarr_technician" ? "tech@example.com" : PlatformSeeder.DemoAdminEmail,
            tenantRoleKey == "maintainarr_technician" ? "Demo Technician" : "Demo Admin",
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

    private sealed class RecordingTrainArrQualificationCheckHandler : HttpMessageHandler
    {
        public string Outcome { get; set; } = "allow";

        public string Message { get; set; } = "Technician qualification is active.";

        public string? RequestPath { get; private set; }

        public string? AuthorizationScheme { get; private set; }

        public string? AuthorizationParameter { get; private set; }

        public JsonElement? Payload { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestPath = request.RequestUri?.AbsolutePath;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            Payload = document.RootElement.Clone();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    checkId = Guid.NewGuid(),
                    staffarrPersonId = Payload.Value.GetProperty("staffarrPersonId").GetGuid(),
                    qualificationKey = Payload.Value.GetProperty("qualificationKey").GetString(),
                    outcome = Outcome,
                    reasonCode = Outcome == "allow" ? "local_issued" : "local_expired",
                    message = Message,
                })
            };
        }
    }

    private sealed record RecordedComplianceCoreSubject(
        string SubjectType,
        string SubjectReference,
        string? SourceProduct,
        string? DisplayLabel);

    private sealed record RecordedComplianceCoreWorkOrderGateRequest(
        string Path,
        string? AuthorizationScheme,
        string? AuthorizationParameter,
        Guid TenantId,
        string ActivityContextKey,
        string? WorkflowKey,
        List<RecordedComplianceCoreSubject> Subjects,
        Dictionary<string, string> RuleContext);

    private sealed class RecordingComplianceCoreWorkOrderGateHandler : HttpMessageHandler
    {
        public List<RecordedComplianceCoreWorkOrderGateRequest> Requests { get; } = [];

        public string NextOutcome { get; set; } = "allow";

        public string NextReasonCode { get; set; } = "maintenance_clear";

        public string NextMessage { get; set; } = "Maintenance work may proceed.";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            var subjects = new List<RecordedComplianceCoreSubject>();
            if (root.TryGetProperty("subjectReferences", out var subjectReferencesElement)
                && subjectReferencesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var subjectElement in subjectReferencesElement.EnumerateArray())
                {
                    subjects.Add(new RecordedComplianceCoreSubject(
                        subjectElement.GetProperty("subjectType").GetString() ?? string.Empty,
                        subjectElement.GetProperty("subjectReference").GetString() ?? string.Empty,
                        subjectElement.TryGetProperty("sourceProduct", out var sourceProductElement)
                            && sourceProductElement.ValueKind != JsonValueKind.Null
                                ? sourceProductElement.GetString()
                                : null,
                        subjectElement.TryGetProperty("displayLabel", out var displayLabelElement)
                            && displayLabelElement.ValueKind != JsonValueKind.Null
                                ? displayLabelElement.GetString()
                                : null));
                }
            }

            var ruleContext = new Dictionary<string, string>();
            if (root.TryGetProperty("ruleContext", out var ruleContextElement)
                && ruleContextElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in ruleContextElement.EnumerateObject())
                {
                    ruleContext[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            Requests.Add(new RecordedComplianceCoreWorkOrderGateRequest(
                request.RequestUri?.AbsolutePath ?? string.Empty,
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter,
                root.GetProperty("tenantId").GetGuid(),
                root.GetProperty("activityContextKey").GetString() ?? string.Empty,
                root.TryGetProperty("workflowKey", out var workflowKeyElement)
                    && workflowKeyElement.ValueKind != JsonValueKind.Null
                        ? workflowKeyElement.GetString()
                        : null,
                subjects,
                ruleContext));

            var traceId = Guid.NewGuid();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    traceId,
                    tenantId = root.GetProperty("tenantId").GetGuid(),
                    workflowKey = root.TryGetProperty("workflowKey", out var responseWorkflowKey)
                        && responseWorkflowKey.ValueKind != JsonValueKind.Null
                            ? responseWorkflowKey.GetString()
                            : "can_perform_maintenance",
                    actionKey = "can_perform_maintenance",
                    activityContextKey = root.GetProperty("activityContextKey").GetString(),
                    subjectReferences = Array.Empty<object>(),
                    checkResultId = traceId,
                    ruleEvaluationRunId = (Guid?)null,
                    outcome = NextOutcome,
                    reasonCode = NextReasonCode,
                    message = NextMessage,
                    appliedRuleVersions = Array.Empty<object>(),
                    citationReferences = Array.Empty<object>(),
                    missingFacts = Array.Empty<string>(),
                    staleFacts = Array.Empty<object>(),
                    evidenceRequirements = Array.Empty<object>(),
                    remediationHints = Array.Empty<object>(),
                    appliedWaiverId = (Guid?)null,
                    appliedWaiverKey = (string?)null,
                    auditExportPath = (string?)null,
                    evaluatedAt = DateTimeOffset.UtcNow
                })
            };
        }
    }

    private sealed record RecordedComplianceCoreAssetReadinessSubject(
        string SubjectType,
        string SubjectReference,
        string? SourceProduct,
        string? DisplayLabel);

    private sealed record RecordedComplianceCoreAssetReadinessGateRequest(
        string Path,
        string? AuthorizationScheme,
        string? AuthorizationParameter,
        Guid TenantId,
        string ActivityContextKey,
        string? WorkflowKey,
        List<RecordedComplianceCoreAssetReadinessSubject> Subjects,
        Dictionary<string, string> RuleContext);

    private sealed class RecordingComplianceCoreAssetReadinessGateHandler : HttpMessageHandler
    {
        public List<RecordedComplianceCoreAssetReadinessGateRequest> Requests { get; } = [];

        public string NextOutcome { get; set; } = "allow";

        public string NextReasonCode { get; set; } = "asset_readiness_clear";

        public string NextMessage { get; set; } = "Asset satisfies Compliance Core readiness requirements.";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? "{}"
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            var subjects = new List<RecordedComplianceCoreAssetReadinessSubject>();
            if (root.TryGetProperty("subjectReferences", out var subjectReferencesElement)
                && subjectReferencesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var subjectElement in subjectReferencesElement.EnumerateArray())
                {
                    subjects.Add(new RecordedComplianceCoreAssetReadinessSubject(
                        subjectElement.GetProperty("subjectType").GetString() ?? string.Empty,
                        subjectElement.GetProperty("subjectReference").GetString() ?? string.Empty,
                        subjectElement.TryGetProperty("sourceProduct", out var sourceProductElement)
                            && sourceProductElement.ValueKind != JsonValueKind.Null
                                ? sourceProductElement.GetString()
                                : null,
                        subjectElement.TryGetProperty("displayLabel", out var displayLabelElement)
                            && displayLabelElement.ValueKind != JsonValueKind.Null
                                ? displayLabelElement.GetString()
                                : null));
                }
            }

            var ruleContext = new Dictionary<string, string>();
            if (root.TryGetProperty("ruleContext", out var ruleContextElement)
                && ruleContextElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in ruleContextElement.EnumerateObject())
                {
                    ruleContext[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            Requests.Add(new RecordedComplianceCoreAssetReadinessGateRequest(
                request.RequestUri?.AbsolutePath ?? string.Empty,
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter,
                root.GetProperty("tenantId").GetGuid(),
                root.GetProperty("activityContextKey").GetString() ?? string.Empty,
                root.TryGetProperty("workflowKey", out var workflowKeyElement)
                    && workflowKeyElement.ValueKind != JsonValueKind.Null
                        ? workflowKeyElement.GetString()
                        : null,
                subjects,
                ruleContext));

            var traceId = Guid.NewGuid();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    traceId,
                    tenantId = root.GetProperty("tenantId").GetGuid(),
                    workflowKey = root.TryGetProperty("workflowKey", out var responseWorkflowKey)
                        && responseWorkflowKey.ValueKind != JsonValueKind.Null
                            ? responseWorkflowKey.GetString()
                            : "can_dispatch_asset",
                    outcome = NextOutcome,
                    reasonCode = NextReasonCode,
                    message = NextMessage,
                    appliedWaiverId = (Guid?)null,
                    appliedWaiverKey = (string?)null,
                    ruleSet = "maintenance_readiness_gate",
                    evaluatedAtUtc = DateTimeOffset.UtcNow
                })
            };
        }
    }
}
