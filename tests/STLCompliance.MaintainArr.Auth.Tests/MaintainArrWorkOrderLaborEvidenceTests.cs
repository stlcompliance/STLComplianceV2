using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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

public sealed class MaintainArrWorkOrderLaborEvidenceTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private readonly Guid _staffarrSiteOrgUnitId = MaintainArrTestSites.DefaultStaffArrSiteOrgUnitId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"LaborEvidenceNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"LaborEvidenceMaintainArr-{Guid.NewGuid():N}";

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
        await MaintainArrTestSites.SeedCachedStaffArrSiteAsync(_maintainarrFactory, _staffarrSiteOrgUnitId);
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Work_order_tasks_labor_and_evidence_lifecycle()
    {
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var workOrderId = await CreateOpenWorkOrderAsync(managerToken, assetId);

        var taskRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/tasks", managerToken);
        taskRequest.Content = JsonContent.Create(new CreateWorkOrderTaskLineRequest("Remove guard", "Safety first", 0));
        var taskResponse = await _maintainarrClient.SendAsync(taskRequest);
        taskResponse.EnsureSuccessStatusCode();
        var task = (await taskResponse.Content.ReadFromJsonAsync<WorkOrderTaskLineResponse>())!;
        Assert.Equal("pending", task.Status);

        var listTasksRequest = Authorized(HttpMethod.Get, $"/api/work-orders/{workOrderId}/tasks", managerToken);
        var listTasksResponse = await _maintainarrClient.SendAsync(listTasksRequest);
        listTasksResponse.EnsureSuccessStatusCode();
        var tasks = (await listTasksResponse.Content.ReadFromJsonAsync<List<WorkOrderTaskLineResponse>>())!;
        Assert.Single(tasks);

        var personId = PlatformSeeder.DemoAdminUserId.ToString();
        var laborRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/labor", managerToken);
        laborRequest.Content = JsonContent.Create(new CreateWorkOrderLaborEntryRequest(
            personId,
            2.5m,
            "regular",
            task.TaskLineId,
            "Morning shift"));
        var laborResponse = await _maintainarrClient.SendAsync(laborRequest);
        laborResponse.EnsureSuccessStatusCode();
        var labor = (await laborResponse.Content.ReadFromJsonAsync<WorkOrderLaborEntryResponse>())!;
        Assert.Equal(2.5m, labor.HoursWorked);
        Assert.Equal(task.TaskLineId, labor.WorkOrderTaskLineId);

        var evidenceBytes = Encoding.UTF8.GetBytes("before-photo");
        var evidenceRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/evidence", managerToken);
        evidenceRequest.Content = JsonContent.Create(new CreateWorkOrderEvidenceRequest(
            "before_photo",
            "before.jpg",
            "image/jpeg",
            Convert.ToBase64String(evidenceBytes),
            "Pre-repair"));
        var evidenceResponse = await _maintainarrClient.SendAsync(evidenceRequest);
        evidenceResponse.EnsureSuccessStatusCode();
        var evidence = (await evidenceResponse.Content.ReadFromJsonAsync<WorkOrderEvidenceResponse>())!;
        Assert.Equal(evidenceBytes.Length, evidence.SizeBytes);

        var detailRequest = Authorized(HttpMethod.Get, $"/api/work-orders/{workOrderId}", managerToken);
        var detailResponse = await _maintainarrClient.SendAsync(detailRequest);
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal("in_progress", detail.Status);
        Assert.NotNull(detail.StartedAt);
    }

    [Fact]
    public async Task Work_order_tasks_labor_and_evidence_v1_alias_lifecycle()
    {
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var workOrderId = await CreateOpenWorkOrderAsync(managerToken, assetId);

        var taskRequest = Authorized(HttpMethod.Post, $"/api/v1/work-orders/{workOrderId}/tasks", managerToken);
        taskRequest.Content = JsonContent.Create(new CreateWorkOrderTaskLineRequest("V1 task", "Alias coverage", 0));
        var taskResponse = await _maintainarrClient.SendAsync(taskRequest);
        taskResponse.EnsureSuccessStatusCode();
        var task = (await taskResponse.Content.ReadFromJsonAsync<WorkOrderTaskLineResponse>())!;

        var laborRequest = Authorized(HttpMethod.Post, $"/api/v1/work-orders/{workOrderId}/labor", managerToken);
        laborRequest.Content = JsonContent.Create(new CreateWorkOrderLaborEntryRequest(
            PlatformSeeder.DemoAdminUserId.ToString(),
            1.5m,
            "regular",
            task.TaskLineId,
            "v1 labor"));
        var laborResponse = await _maintainarrClient.SendAsync(laborRequest);
        laborResponse.EnsureSuccessStatusCode();

        var evidenceRequest = Authorized(HttpMethod.Post, $"/api/v1/work-orders/{workOrderId}/evidence", managerToken);
        evidenceRequest.Content = JsonContent.Create(new CreateWorkOrderEvidenceRequest(
            "before_photo",
            "v1-before.jpg",
            "image/jpeg",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("v1-before-photo")),
            "v1 evidence"));
        var evidenceResponse = await _maintainarrClient.SendAsync(evidenceRequest);
        evidenceResponse.EnsureSuccessStatusCode();

        var detailRequest = Authorized(HttpMethod.Get, $"/api/v1/work-orders/{workOrderId}", managerToken);
        var detailResponse = await _maintainarrClient.SendAsync(detailRequest);
        detailResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Top_level_v1_work_order_tasks_and_labor_aliases_round_trip()
    {
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var workOrderId = await CreateOpenWorkOrderAsync(managerToken, assetId);

        var taskCreateRequest = Authorized(HttpMethod.Post, "/api/v1/work-order-tasks", managerToken);
        taskCreateRequest.Content = JsonContent.Create(new CreateWorkOrderTaskLineAliasRequest(
            workOrderId,
            "Top-level v1 task",
            "Alias route",
            0));
        var taskCreateResponse = await _maintainarrClient.SendAsync(taskCreateRequest);
        taskCreateResponse.EnsureSuccessStatusCode();
        var task = (await taskCreateResponse.Content.ReadFromJsonAsync<WorkOrderTaskLineResponse>())!;

        var taskListRequest = Authorized(HttpMethod.Get, $"/api/v1/work-order-tasks?workOrderId={workOrderId}", managerToken);
        var taskListResponse = await _maintainarrClient.SendAsync(taskListRequest);
        taskListResponse.EnsureSuccessStatusCode();
        var tasks = (await taskListResponse.Content.ReadFromJsonAsync<List<WorkOrderTaskLineResponse>>())!;
        Assert.Contains(tasks, x => x.TaskLineId == task.TaskLineId);

        var laborCreateRequest = Authorized(HttpMethod.Post, "/api/v1/labor", managerToken);
        laborCreateRequest.Content = JsonContent.Create(new CreateWorkOrderLaborEntryAliasRequest(
            workOrderId,
            PlatformSeeder.DemoAdminUserId.ToString(),
            1.25m,
            "regular",
            task.TaskLineId,
            "top-level labor"));
        var laborCreateResponse = await _maintainarrClient.SendAsync(laborCreateRequest);
        laborCreateResponse.EnsureSuccessStatusCode();

        var laborListRequest = Authorized(HttpMethod.Get, $"/api/v1/labor?workOrderId={workOrderId}", managerToken);
        var laborListResponse = await _maintainarrClient.SendAsync(laborListRequest);
        laborListResponse.EnsureSuccessStatusCode();
        var labor = (await laborListResponse.Content.ReadFromJsonAsync<List<WorkOrderLaborEntryResponse>>())!;
        Assert.NotEmpty(labor);
    }

    [Fact]
    public async Task Documents_v1_alias_create_and_list_for_work_order()
    {
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var workOrderId = await CreateOpenWorkOrderAsync(managerToken, assetId);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/documents", managerToken);
        createRequest.Content = JsonContent.Create(new CreateMaintainArrDocumentRequest(
            "work_order",
            workOrderId,
            "before_photo",
            "doc-before.jpg",
            "image/jpeg",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("doc-before")),
            "top-level document alias"));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<MaintainArrDocumentResponse>())!;
        Assert.Equal("work_order", created.TargetType);
        Assert.Equal(workOrderId, created.TargetId);

        var listRequest = Authorized(HttpMethod.Get, $"/api/v1/documents?workOrderId={workOrderId}", managerToken);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var docs = (await listResponse.Content.ReadFromJsonAsync<List<MaintainArrDocumentResponse>>())!;
        Assert.Contains(docs, x => x.DocumentId == created.DocumentId);
    }

    [Fact]
    public async Task Documents_alerts_v1_include_open_defects_missing_evidence()
    {
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        var assetId = await SeedAssetOnlyAsync(managerToken);

        var defectRequest = Authorized(HttpMethod.Post, "/api/v1/defects", managerToken);
        defectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Hydraulic leak",
            "Visible leak near actuator.",
            DefectSeverities.High));
        var defectResponse = await _maintainarrClient.SendAsync(defectRequest);
        defectResponse.EnsureSuccessStatusCode();
        var defect = (await defectResponse.Content.ReadFromJsonAsync<DefectSummaryResponse>())!;

        var alertRequest = Authorized(HttpMethod.Get, "/api/v1/documents/alerts?targetType=defect", managerToken);
        var alertResponse = await _maintainarrClient.SendAsync(alertRequest);
        alertResponse.EnsureSuccessStatusCode();
        var alerts = (await alertResponse.Content.ReadFromJsonAsync<List<MaintainArrDocumentAlertResponse>>())!;

        Assert.Contains(alerts, alert => alert.TargetType == "defect" && alert.TargetId == defect.DefectId);
    }

    [Fact]
    public async Task Cannot_add_labor_to_completed_work_order()
    {
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var workOrderId = await CreateOpenWorkOrderAsync(managerToken, assetId);

        var completeRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{workOrderId}/status", managerToken);
        completeRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("in_progress"));
        await _maintainarrClient.SendAsync(completeRequest);

        var finishRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{workOrderId}/status", managerToken);
        finishRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("completed"));
        await _maintainarrClient.SendAsync(finishRequest);

        var laborRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/labor", managerToken);
        laborRequest.Content = JsonContent.Create(new CreateWorkOrderLaborEntryRequest(
            PlatformSeeder.DemoAdminUserId.ToString(),
            1m,
            "regular",
            null,
            null));
        var laborResponse = await _maintainarrClient.SendAsync(laborRequest);
        Assert.Equal(HttpStatusCode.Conflict, laborResponse.StatusCode);
    }

    [Fact]
    public async Task Technician_can_log_labor_on_assigned_work_order()
    {
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var technicianPersonId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb").ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", managerToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Assigned labor WO",
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

        var laborRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{created.WorkOrderId}/labor", technicianToken);
        laborRequest.Content = JsonContent.Create(new CreateWorkOrderLaborEntryRequest(
            technicianPersonId,
            1.25m,
            "overtime",
            null,
            null));
        var laborResponse = await _maintainarrClient.SendAsync(laborRequest);
        laborResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Technician_cannot_add_labor_to_unassigned_work_order()
    {
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        var assetId = await SeedAssetOnlyAsync(managerToken);
        var workOrderId = await CreateOpenWorkOrderAsync(managerToken, assetId);

        var technicianToken = CreateMaintainArrAccessToken(
            ["maintainarr"],
            "maintainarr_technician",
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

        var laborRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/labor", technicianToken);
        laborRequest.Content = JsonContent.Create(new CreateWorkOrderLaborEntryRequest(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb").ToString(),
            1m,
            "regular",
            null,
            null));
        var laborResponse = await _maintainarrClient.SendAsync(laborRequest);
        Assert.Equal(HttpStatusCode.Forbidden, laborResponse.StatusCode);
    }

    private async Task<Guid> CreateOpenWorkOrderAsync(string token, Guid assetId)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", token);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Labor test WO",
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
            $"LABOR-ASSET-{Guid.NewGuid():N}".Substring(0, 12),
            "Labor Evidence Test Asset",
            string.Empty,
            _staffarrSiteOrgUnitId.ToString("D")));
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
            $"{productKey}-labor-test",
            $"{productKey} Labor Test",
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
}
