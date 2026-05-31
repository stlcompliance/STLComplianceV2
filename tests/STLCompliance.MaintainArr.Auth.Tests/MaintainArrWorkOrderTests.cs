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
    private RecordingTrainArrQualificationCheckHandler _trainarrQualificationHandler = null!;
    private RecordingComplianceCoreWorkOrderGateHandler _complianceCoreGateHandler = null!;

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
        var serviceToken = await IssueServiceTokenAsync(adminToken, "maintainarr");
        _trainarrQualificationHandler = new RecordingTrainArrQualificationCheckHandler();
        _complianceCoreGateHandler = new RecordingComplianceCoreWorkOrderGateHandler();

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", serviceToken);
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
}
