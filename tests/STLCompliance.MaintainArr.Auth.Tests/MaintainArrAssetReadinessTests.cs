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

public sealed class MaintainArrAssetReadinessTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _handoffServiceToken = null!;
    private string _pmScanServiceToken = null!;
    private string _routarrAssetReadinessToken = null!;
    private string _routarrDispatchAssetReadinessToken = null!;
    private RecordingComplianceCoreAssetReadinessGateHandler _complianceCoreReadinessGateHandler = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ReadinessNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"ReadinessMaintainArr-{Guid.NewGuid():N}";

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
        _pmScanServiceToken = await IssuePmScanServiceTokenAsync(adminToken);
        _routarrAssetReadinessToken = await IssueAssetReadinessIntegrationTokenAsync(adminToken);
        _routarrDispatchAssetReadinessToken = await IssueAssetReadinessIntegrationTokenAsync(
            adminToken,
            global::MaintainArr.Api.Endpoints.IntegrationEndpoints.RoutarrAssetReadinessDispatchActionScope);
        _complianceCoreReadinessGateHandler = new RecordingComplianceCoreAssetReadinessGateHandler();

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _handoffServiceToken);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("ComplianceCore:BaseUrl", "http://compliancecore.test");
            builder.UseSetting("ComplianceCore:ServiceToken", "maintainarr-to-compliancecore-token");
            builder.UseSetting("ComplianceCore:AssetReadinessActionKey", "can-dispatch-asset");
            builder.UseSetting("ComplianceCore:AssetReadinessWorkflowKey", "can_dispatch_asset");
            builder.UseSetting("ComplianceCore:AssetReadinessActivityContextKey", "asset_readiness");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
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
    public async Task Asset_readiness_ready_when_no_blockers()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/asset-readiness?assetId={assetId}", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;

        Assert.Equal(assetId, readiness.AssetId);
        Assert.Equal("ready", readiness.ReadinessStatus);
        Assert.Equal("maintenance_clear", readiness.ReadinessBasis);
        Assert.Empty(readiness.Blockers);
        Assert.NotNull(readiness.Dispatchability);
        Assert.True(readiness.Dispatchability.IsDispatchable);
        Assert.Equal("allow", readiness.Dispatchability.Outcome);
        Assert.NotNull(readiness.Confidence);
        Assert.Equal("live_query", readiness.Confidence.DataSource);
        Assert.NotNull(readiness.AuditSnapshot);
        Assert.NotEqual(Guid.Empty, readiness.AuditSnapshot.AuditEventId);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        Assert.True(await db.AuditEvents.AnyAsync(x =>
            x.Id == readiness.AuditSnapshot.AuditEventId
            && x.Action == "asset_readiness.read"
            && x.TargetId == assetId.ToString()));
    }

    [Fact]
    public async Task Asset_readiness_not_ready_for_open_critical_defect()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var createDefectRequest = Authorized(HttpMethod.Post, "/api/defects", token);
        createDefectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Brake failure",
            "Primary brake inoperative",
            DefectSeverities.Critical));
        await _maintainarrClient.SendAsync(createDefectRequest);

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/asset-readiness?assetId={assetId}", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Contains(readiness.Blockers, blocker => blocker.BlockerType == "critical_defect");
        Assert.Equal(1, readiness.Signals.OpenCriticalDefectCount);
        Assert.NotNull(readiness.Dispatchability);
        Assert.False(readiness.Dispatchability.IsDispatchable);
        Assert.Equal("block", readiness.Dispatchability.Outcome);
        Assert.Equal("critical_defect", readiness.Dispatchability.PrimaryBlockerType);
    }

    [Fact]
    public async Task Asset_readiness_includes_compliancecore_blocker_and_reference()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        _complianceCoreReadinessGateHandler.NextOutcome = "block";
        _complianceCoreReadinessGateHandler.NextReasonCode = "annual_inspection_rule_failed";
        _complianceCoreReadinessGateHandler.NextMessage = "Compliance Core requires annual inspection evidence before dispatch.";

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/asset-readiness?assetId={assetId}", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Equal("compliancecore", readiness.ReadinessBasis);
        var complianceBlocker = Assert.Single(readiness.Blockers, blocker => blocker.BlockerType == "compliancecore_rule");
        Assert.Equal("compliancecore_check", complianceBlocker.SourceEntityType);
        Assert.Contains("annual inspection evidence", complianceBlocker.Message, StringComparison.OrdinalIgnoreCase);
        var reference = Assert.Single(readiness.ComplianceCoreReferences ?? []);
        Assert.Equal("check_result", reference.ReferenceType);
        Assert.Equal("block", reference.Outcome);
        Assert.Contains(reference.ReferenceId, reference.DeepLinkPath, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(readiness.Dispatchability);
        Assert.False(readiness.Dispatchability.IsDispatchable);
        Assert.Equal("block", readiness.Dispatchability.Outcome);
        Assert.Equal("compliancecore_rule", readiness.Dispatchability.PrimaryBlockerType);

        var gateRequest = Assert.Single(_complianceCoreReadinessGateHandler.Requests);
        Assert.Equal("/api/v1/gates/can-dispatch-asset", gateRequest.Path);
        Assert.Equal("Bearer", gateRequest.AuthorizationScheme);
        Assert.Equal("maintainarr-to-compliancecore-token", gateRequest.AuthorizationParameter);
        Assert.Equal(PlatformSeeder.DemoTenantId, gateRequest.TenantId);
        Assert.Equal("asset_readiness", gateRequest.ActivityContextKey);
        Assert.Equal("can_dispatch_asset", gateRequest.WorkflowKey);
        Assert.Contains(gateRequest.Subjects, subject =>
            subject.SubjectType == "asset"
            && subject.SubjectReference == assetId.ToString("D")
            && subject.SourceProduct == "maintainarr");
        Assert.Equal(assetId.ToString("D"), gateRequest.RuleContext["asset_id"]);
        Assert.Equal("ready", gateRequest.RuleContext["local_readiness_status"]);
        Assert.Equal("maintenance_clear", gateRequest.RuleContext["local_readiness_basis"]);
    }

    [Fact]
    public async Task Asset_readiness_not_ready_for_active_work_order_and_pm_overdue()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var createWorkOrderRequest = Authorized(HttpMethod.Post, "/api/work-orders", token);
        createWorkOrderRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Oil change",
            "Scheduled service",
            WorkOrderPriorities.Medium,
            null,
            null));
        await _maintainarrClient.SendAsync(createWorkOrderRequest);

        var createPmRequest = Authorized(HttpMethod.Post, "/api/preventive-maintenance/schedules", token);
        createPmRequest.Content = JsonContent.Create(new CreatePmScheduleRequest(
            assetId,
            $"pm-{Guid.NewGuid():N}".Substring(0, 10),
            "Quarterly service",
            string.Empty,
            30,
            DateTimeOffset.UtcNow.AddDays(-5)));
        var createPmResponse = await _maintainarrClient.SendAsync(createPmRequest);
        createPmResponse.EnsureSuccessStatusCode();

        var scanRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/pm/process-due-scan");
        scanRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _pmScanServiceToken);
        scanRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            0));
        await _maintainarrClient.SendAsync(scanRequest);

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/asset-readiness?assetId={assetId}", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Contains(readiness.Blockers, blocker => blocker.BlockerType == "active_work_order");
        Assert.True(
            readiness.Blockers.Any(blocker => blocker.BlockerType is "pm_due" or "pm_overdue"),
            "Expected PM due or overdue blocker.");
    }

    [Fact]
    public async Task Asset_readiness_fleet_list_returns_all_assets_with_status()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetTypeId = await SeedAssetTypeAsync(token);
        var readyAssetId = await SeedAssetWithTypeAsync(token, assetTypeId);
        var blockedAssetId = await SeedAssetWithTypeAsync(token, assetTypeId);

        var createDefectRequest = Authorized(HttpMethod.Post, "/api/defects", token);
        createDefectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            blockedAssetId,
            "Hydraulic leak",
            "Visible fluid",
            DefectSeverities.High));
        await _maintainarrClient.SendAsync(createDefectRequest);

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/asset-readiness", token));
        response.EnsureSuccessStatusCode();
        var fleet = (await response.Content.ReadFromJsonAsync<List<AssetReadinessSummaryResponse>>())!;

        Assert.True(fleet.Count >= 2);
        Assert.Contains(fleet, item => item.AssetId == readyAssetId && item.ReadinessStatus == "ready");
        Assert.Contains(
            fleet,
            item => item.AssetId == blockedAssetId
                && item.ReadinessStatus == "not_ready"
                && item.BlockerCount >= 1
                && item.PrimaryBlockerMessage is not null);
    }

    [Fact]
    public async Task Asset_readiness_v1_detail_returns_asset_status()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/readiness?assetId={assetId}", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;

        Assert.Equal(assetId, readiness.AssetId);
        Assert.Equal("ready", readiness.ReadinessStatus);
    }

    [Fact]
    public async Task Asset_readiness_v1_asset_path_returns_asset_status()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/readiness/assets/{assetId}", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;

        Assert.Equal(assetId, readiness.AssetId);
        Assert.Equal("ready", readiness.ReadinessStatus);
    }

    [Fact]
    public async Task Asset_readiness_v1_fleet_list_returns_assets()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/readiness", token));
        response.EnsureSuccessStatusCode();
        var fleet = (await response.Content.ReadFromJsonAsync<List<AssetReadinessSummaryResponse>>())!;

        Assert.Contains(fleet, item => item.AssetId == assetId);
    }

    [Fact]
    public async Task Asset_readiness_missing_asset_returns_not_found()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var missingAssetId = Guid.NewGuid();

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/asset-readiness?assetId={missingAssetId}", token));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Asset_readiness_requires_maintainarr_entitlement()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);
        var unauthorizedToken = CreateMaintainArrAccessToken([], "tenant_member");

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/asset-readiness?assetId={assetId}", unauthorizedToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Integration_asset_readiness_supports_v1_alias()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var asset = await db.Assets.SingleAsync(x => x.Id == assetId);

        var response = await _maintainarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/integrations/asset-readiness?tenantId={PlatformSeeder.DemoTenantId:D}&assetTag={Uri.EscapeDataString(asset.AssetTag)}",
            _routarrAssetReadinessToken));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;
        Assert.Equal(assetId, readiness.AssetId);

        var v1Response = await _maintainarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/integrations/asset-readiness?tenantId={PlatformSeeder.DemoTenantId:D}&assetTag={Uri.EscapeDataString(asset.AssetTag)}",
            _routarrAssetReadinessToken));
        v1Response.EnsureSuccessStatusCode();
        var v1Readiness = (await v1Response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;
        Assert.Equal(readiness.AssetId, v1Readiness.AssetId);
        Assert.Equal(readiness.ReadinessStatus, v1Readiness.ReadinessStatus);
    }

    [Fact]
    public async Task Integration_asset_readiness_supports_asset_id_lookup_for_product_contracts()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var response = await _maintainarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/integrations/asset-readiness?tenantId={PlatformSeeder.DemoTenantId:D}&assetId={assetId:D}",
            _routarrAssetReadinessToken));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;

        Assert.Equal(assetId, readiness.AssetId);
        Assert.Equal("ready", readiness.ReadinessStatus);
        Assert.NotNull(readiness.Dispatchability);
        Assert.True(readiness.Dispatchability.IsDispatchable);
        Assert.NotNull(readiness.AuditSnapshot);
        Assert.NotEqual(Guid.Empty, readiness.AuditSnapshot.AuditEventId);
    }

    [Fact]
    public async Task Routarr_asset_readiness_dispatch_gate_supports_asset_id_lookup()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var response = await _maintainarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/integrations/routarr-asset-readiness?tenantId={PlatformSeeder.DemoTenantId:D}&assetId={assetId:D}",
            _routarrDispatchAssetReadinessToken));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;

        Assert.Equal(assetId, readiness.AssetId);
        Assert.Equal("ready", readiness.ReadinessStatus);
        Assert.NotNull(readiness.Dispatchability);
        Assert.True(readiness.Dispatchability.IsDispatchable);
    }

    [Fact]
    public async Task Integrations_v1_index_lists_expected_integration_paths()
    {
        var response = await _maintainarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            "/api/v1/integrations",
            _routarrAssetReadinessToken));
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = json.RootElement.GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("path").GetString())
            .ToList();

        Assert.Contains("/api/v1/integrations/asset-readiness", paths);
        Assert.Contains("/api/v1/integrations/routarr-events", paths);
        Assert.Contains("/api/v1/integrations/supplyarr-demand-status", paths);
        Assert.Contains("/api/v1/integrations/staffarr-person-sync", paths);
    }

    private async Task<Guid> SeedAssetOnlyAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);
        return await SeedAssetWithTypeAsync(token, assetTypeId);
    }

    private async Task<Guid> SeedAssetWithTypeAsync(string token, Guid assetTypeId)
    {
        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        var assetTag = $"RDY-{Guid.NewGuid().ToString("N")[..8]}";
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            assetTag,
            "Readiness Test Asset",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
    }

    private async Task<Guid> SeedAssetTypeAsync(string token)
    {
        var classKey = $"rdyv-{Guid.NewGuid().ToString("N")[..8]}";
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            classKey,
            "Readiness Vehicles",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var typeKey = $"rdyf-{Guid.NewGuid().ToString("N")[..8]}";
        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            typeKey,
            "Readiness Forklift",
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
            $"{productKey}-readiness-handoff-test",
            $"{productKey} Readiness Handoff Test",
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
            $"shared-worker-readiness-{Guid.NewGuid():N}",
            "Readiness PM due scan test",
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

    private async Task<string> IssueAssetReadinessIntegrationTokenAsync(
        string adminToken,
        string actionScope = global::MaintainArr.Api.Endpoints.IntegrationEndpoints.AssetReadinessReadActionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"routarr-asset-readiness-{Guid.NewGuid():N}",
            "MaintainArr asset readiness integration test",
            "routarr",
            ["maintainarr"]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            ["maintainarr"],
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
                    actionKey = "can_dispatch_asset",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
