using STLCompliance.Shared.Integration;
using System.Net.Http.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.E2E.Support;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// Failed inspection → auto defect → work order from defect → asset readiness blocked (docs/23 workflow 3).
/// </summary>
[Trait("Category", "Integration")]
public sealed class MaintainArrInspectionToWorkOrderFlowTests : IAsyncLifetime
{
    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();

        var adminToken = await _nexarr.LoginAsync();
        var handoffToken = await _nexarr.IssueServiceTokenAsync(adminToken, "maintainarr", "launch.redeem");
        var maintainArrDbName = $"E2E-MaintainArr-InspWO-{Guid.NewGuid():N}";

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarr.Client.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", handoffToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarr.Factory.Server.CreateHandler());
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarr.DisposeAsync();
    }

    [Fact]
    public async Task Failed_inspection_creates_defect_work_order_and_blocks_asset_readiness()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(token);

        var startRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var run = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var submitRequest = HttpTestClient.Authorized(HttpMethod.Put, $"/api/inspections/{run.InspectionRunId}/answers", token);
        submitRequest.Content = JsonContent.Create(new SubmitInspectionRunAnswersRequest([
            new InspectionRunAnswerInput(checklistItemId, "fail", null, null),
        ]));
        (await _maintainarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var completeRequest = HttpTestClient.Authorized(HttpMethod.Post, $"/api/inspections/{run.InspectionRunId}/complete", token);
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;
        Assert.Equal("failed", completed.Result);

        var defectsRequest = HttpTestClient.Authorized(
            HttpMethod.Get,
            $"/api/defects?inspectionRunId={run.InspectionRunId}",
            token);
        var defectsResponse = await _maintainarrClient.SendAsync(defectsRequest);
        defectsResponse.EnsureSuccessStatusCode();
        var defects = (await defectsResponse.Content.ReadFromJsonAsync<List<DefectSummaryResponse>>())!;
        var defect = Assert.Single(defects);
        Assert.Equal("open", defect.Status);

        var readinessBeforeWoRequest = HttpTestClient.Authorized(
            HttpMethod.Get,
            $"/api/asset-readiness?assetId={assetId}",
            token);
        var readinessBeforeWoResponse = await _maintainarrClient.SendAsync(readinessBeforeWoRequest);
        readinessBeforeWoResponse.EnsureSuccessStatusCode();
        var readinessBeforeWo = (await readinessBeforeWoResponse.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;
        Assert.Equal("not_ready", readinessBeforeWo.ReadinessStatus);

        var createWoRequest = HttpTestClient.Authorized(HttpMethod.Post, $"/api/defects/{defect.DefectId}/work-orders", token);
        createWoRequest.Content = JsonContent.Create(new CreateWorkOrderFromDefectRequest(
            "Repair failed inspection item",
            "Cross-product failed inspection → work order",
            "high",
            null));
        var createWoResponse = await _maintainarrClient.SendAsync(createWoRequest);
        createWoResponse.EnsureSuccessStatusCode();
        var workOrder = (await createWoResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        Assert.Equal(defect.DefectId, workOrder.DefectId);
        Assert.Equal("open", workOrder.Status);
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await _nexarr.CreateHandoffAsync("maintainarr", "http://localhost:5178/launch");
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<(Guid AssetId, Guid TemplateId, Guid ChecklistItemId)> SeedActiveTemplateWithAssetAsync(string token)
    {
        var createClassRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"e2e-class-{Guid.NewGuid():N}".Substring(0, 12),
            "E2E Vehicles",
            string.Empty));
        var assetClass = (await (await _maintainarrClient.SendAsync(createClassRequest)).Content
            .ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"e2e-type-{Guid.NewGuid():N}".Substring(0, 12),
            "E2E Forklift",
            string.Empty));
        var assetType = (await (await _maintainarrClient.SendAsync(createTypeRequest)).Content
            .ReadFromJsonAsync<AssetTypeResponse>())!;

        var createTemplateRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "pre-trip",
            "Pre-Trip",
            "Daily pre-trip inspection"));
        var template = (await (await _maintainarrClient.SendAsync(createTemplateRequest)).Content
            .ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createItemRequest = HttpTestClient.Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "brakes-ok",
            "Brakes operate correctly",
            null,
            "pass_fail",
            true,
            10,
            null));
        var item = (await (await _maintainarrClient.SendAsync(createItemRequest)).Content
            .ReadFromJsonAsync<InspectionChecklistItemResponse>())!;

        var replaceAssetTypesRequest = HttpTestClient.Authorized(
            HttpMethod.Put,
            $"/api/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([assetType.AssetTypeId]));
        await _maintainarrClient.SendAsync(replaceAssetTypesRequest);

        var activateRequest = HttpTestClient.Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        await _maintainarrClient.SendAsync(activateRequest);

        var createAssetRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetType.AssetTypeId,
            $"E2E-{Guid.NewGuid():N}".Substring(0, 12),
            "Inspection Test Asset",
            string.Empty,
            null));
        var asset = (await (await _maintainarrClient.SendAsync(createAssetRequest)).Content
            .ReadFromJsonAsync<AssetResponse>())!;

        return (asset.AssetId, template.InspectionTemplateId, item.ChecklistItemId);
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
}
