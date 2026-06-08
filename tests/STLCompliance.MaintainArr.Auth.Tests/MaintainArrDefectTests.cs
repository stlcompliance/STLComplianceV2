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

public sealed class MaintainArrDefectTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"DefectNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"DefectMaintainArr-{Guid.NewGuid():N}";

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

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", serviceToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
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
    public async Task Failed_inspection_completion_auto_creates_defect()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(token);
        var runId = await CompleteFailedRunAsync(token, assetId, templateId, checklistItemId);

        var listRequest = Authorized(HttpMethod.Get, $"/api/defects?inspectionRunId={runId}", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var defects = (await listResponse.Content.ReadFromJsonAsync<List<DefectSummaryResponse>>())!;

        Assert.Single(defects);
        Assert.Equal(runId, defects[0].InspectionRunId);
        Assert.Equal(checklistItemId, defects[0].ChecklistItemId);
        Assert.Equal("inspection_auto", defects[0].Source);
        Assert.Equal("open", defects[0].Status);
    }

    [Fact]
    public async Task Manual_defect_create_and_status_update()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/defects", managerToken);
        createRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Hydraulic leak",
            "Visible fluid under left cylinder",
            "high"));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;
        Assert.Equal("manual", created.Source);
        Assert.Equal("open", created.Status);

        var statusRequest = Authorized(HttpMethod.Patch, $"/api/defects/{created.DefectId}/status", managerToken);
        statusRequest.Content = JsonContent.Create(new UpdateDefectStatusRequest("acknowledged"));
        var statusResponse = await _maintainarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var updated = (await statusResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;
        Assert.Equal("acknowledged", updated.Status);
    }

    [Fact]
    public async Task Manual_defect_v1_alias_create_and_fetch()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/defects", managerToken);
        createRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "V1 alias defect",
            "Verifies /api/v1/defects alias",
            "medium"));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var getRequest = Authorized(HttpMethod.Get, $"/api/v1/defects/{created.DefectId}", managerToken);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var fetched = (await getResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        Assert.Equal(created.DefectId, fetched.DefectId);
        Assert.Equal("open", fetched.Status);
    }

    [Fact]
    public async Task Critical_manual_defect_marks_asset_oos_and_returns_downtime_follow_up()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/defects", managerToken);
        createRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Brake failure",
            "Complete loss of service brakes",
            "critical"));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;
        Assert.NotNull(created.DowntimeFollowUp);
        Assert.Equal("critical_defect_oos", created.DowntimeFollowUp!.Trigger);
        Assert.Contains($"/downtime?assetId={assetId:D}", created.DowntimeFollowUp.DeepLinkPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"defectId={created.DefectId:D}", created.DowntimeFollowUp.DeepLinkPath, StringComparison.OrdinalIgnoreCase);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var asset = await db.Assets.SingleAsync(x => x.Id == assetId);
        Assert.Equal("out_of_service", asset.LifecycleStatus);
        var downtimeEvent = await db.AssetDowntimeEvents.SingleAsync(
            x => x.DefectId == created.DefectId && x.EndedAt == null);
        Assert.Equal(AssetDowntimeReasons.OutOfService, downtimeEvent.Reason);
    }

    [Fact]
    public async Task Manual_create_from_inspection_is_idempotent()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(token);
        var runId = await CompleteFailedRunAsync(token, assetId, templateId, checklistItemId);

        var firstRequest = Authorized(HttpMethod.Post, $"/api/inspections/{runId}/defects", token);
        firstRequest.Content = JsonContent.Create(new CreateDefectsFromInspectionRunRequest(null));
        var firstResponse = await _maintainarrClient.SendAsync(firstRequest);
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<CreateDefectsFromInspectionRunResponse>())!;
        Assert.Empty(first.Created);
        Assert.Single(first.Existing);

        var secondRequest = Authorized(HttpMethod.Post, $"/api/inspections/{runId}/defects", token);
        secondRequest.Content = JsonContent.Create(new CreateDefectsFromInspectionRunRequest(null));
        var secondResponse = await _maintainarrClient.SendAsync(secondRequest);
        secondResponse.EnsureSuccessStatusCode();
        var second = (await secondResponse.Content.ReadFromJsonAsync<CreateDefectsFromInspectionRunResponse>())!;
        Assert.Empty(second.Created);
        Assert.Single(second.Existing);
    }

    [Fact]
    public async Task Technician_cannot_view_other_users_defect()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/defects", managerToken);
        createRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Manager defect",
            string.Empty,
            "low"));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var technicianToken = CreateMaintainArrAccessToken(
            ["maintainarr"],
            "maintainarr_technician",
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

        var peekRequest = Authorized(HttpMethod.Get, $"/api/defects/{created.DefectId}", technicianToken);
        var peekResponse = await _maintainarrClient.SendAsync(peekRequest);
        Assert.Equal(HttpStatusCode.Forbidden, peekResponse.StatusCode);

        var managerPeekRequest = Authorized(HttpMethod.Get, $"/api/defects/{created.DefectId}", managerToken);
        var managerPeekResponse = await _maintainarrClient.SendAsync(managerPeekRequest);
        managerPeekResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Technician_cannot_update_defect_status()
    {
        var managerToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/defects", managerToken);
        createRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Status gate test",
            string.Empty,
            "medium"));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var technicianToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_technician");
        var statusRequest = Authorized(HttpMethod.Patch, $"/api/defects/{created.DefectId}/status", technicianToken);
        statusRequest.Content = JsonContent.Create(new UpdateDefectStatusRequest("resolved"));
        var statusResponse = await _maintainarrClient.SendAsync(statusRequest);
        Assert.Equal(HttpStatusCode.Forbidden, statusResponse.StatusCode);
    }

    private async Task<Guid> CompleteFailedRunAsync(
        string token,
        Guid assetId,
        Guid templateId,
        Guid checklistItemId)
    {
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
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();

        return started.InspectionRunId;
    }

    private async Task<Guid> SeedAssetOnlyAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            $"DEF-ASSET-{Guid.NewGuid():N}".Substring(0, 12),
            "Defect Test Asset",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
    }

    private async Task<(Guid AssetId, Guid TemplateId, Guid ChecklistItemId)> SeedActiveTemplateWithAssetAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            $"pre-trip-{Guid.NewGuid():N}".Substring(0, 12),
            "Pre-Trip",
            "Daily pre-trip inspection"));
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
            $"RUN-ASSET-{Guid.NewGuid():N}".Substring(0, 12),
            "Inspection Test Asset",
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
            $"{productKey}-defect-test",
            $"{productKey} Defect Test",
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
        var (token, _) = tokenService.CreateAccessToken(
            userId,
            userId,
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
}
