using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
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
using STLCompliance.Shared.Integration;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrDefectEvidenceTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"DefectEvidenceNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"DefectEvidenceMaintainArr-{Guid.NewGuid():N}";

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
    public async Task Defect_evidence_upload_and_list()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var createDefectRequest = Authorized(HttpMethod.Post, "/api/defects", token);
        createDefectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Hydraulic leak",
            "Visible fluid under mast",
            "high"));
        var createDefectResponse = await _maintainarrClient.SendAsync(createDefectRequest);
        createDefectResponse.EnsureSuccessStatusCode();
        var defect = (await createDefectResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;
        Assert.Equal(0, defect.EvidenceCount);

        var evidenceBytes = Encoding.UTF8.GetBytes("leak-photo");
        var uploadRequest = Authorized(HttpMethod.Post, $"/api/defects/{defect.DefectId}/evidence", token);
        uploadRequest.Content = JsonContent.Create(new CreateMaintainArrEvidenceRequest(
            "defect_photo",
            "leak.jpg",
            "image/jpeg",
            Convert.ToBase64String(evidenceBytes),
            "Under carriage"));
        var uploadResponse = await _maintainarrClient.SendAsync(uploadRequest);
        uploadResponse.EnsureSuccessStatusCode();
        var uploaded = (await uploadResponse.Content.ReadFromJsonAsync<DefectEvidenceResponse>())!;
        Assert.Equal(evidenceBytes.Length, uploaded.SizeBytes);

        var listRequest = Authorized(HttpMethod.Get, $"/api/defects/{defect.DefectId}/evidence", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var evidence = (await listResponse.Content.ReadFromJsonAsync<List<DefectEvidenceResponse>>())!;
        Assert.Single(evidence);

        var detailRequest = Authorized(HttpMethod.Get, $"/api/defects/{defect.DefectId}", token);
        var detailResponse = await _maintainarrClient.SendAsync(detailRequest);
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;
        Assert.Equal(1, detail.EvidenceCount);
    }

    [Fact]
    public async Task Defect_evidence_v1_alias_upload_and_list()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var createDefectRequest = Authorized(HttpMethod.Post, "/api/v1/defects", token);
        createDefectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "V1 evidence defect",
            "Uses v1 defect evidence routes",
            "medium"));
        var createDefectResponse = await _maintainarrClient.SendAsync(createDefectRequest);
        createDefectResponse.EnsureSuccessStatusCode();
        var defect = (await createDefectResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var uploadRequest = Authorized(HttpMethod.Post, $"/api/v1/defects/{defect.DefectId}/evidence", token);
        uploadRequest.Content = JsonContent.Create(new CreateMaintainArrEvidenceRequest(
            "defect_photo",
            "v1.jpg",
            "image/jpeg",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("v1-photo")),
            "v1 upload"));
        var uploadResponse = await _maintainarrClient.SendAsync(uploadRequest);
        uploadResponse.EnsureSuccessStatusCode();

        var listRequest = Authorized(HttpMethod.Get, $"/api/v1/defects/{defect.DefectId}/evidence", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var evidence = (await listResponse.Content.ReadFromJsonAsync<List<DefectEvidenceResponse>>())!;

        Assert.Single(evidence);
    }

    [Fact]
    public async Task Inspection_run_evidence_upload_while_in_progress()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, checklistItemId) = await SeedActiveTemplateWithAssetAsync(token);

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var evidenceBytes = Encoding.UTF8.GetBytes("failed-brake-photo");
        var uploadRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspections/{started.InspectionRunId}/evidence",
            token);
        uploadRequest.Content = JsonContent.Create(new CreateMaintainArrEvidenceRequest(
            "failed_item_photo",
            "brake-fail.jpg",
            "image/jpeg",
            Convert.ToBase64String(evidenceBytes),
            "Brake pad worn",
            checklistItemId));
        var uploadResponse = await _maintainarrClient.SendAsync(uploadRequest);
        uploadResponse.EnsureSuccessStatusCode();
        var uploaded = (await uploadResponse.Content.ReadFromJsonAsync<InspectionRunEvidenceResponse>())!;
        Assert.Equal(checklistItemId, uploaded.ChecklistItemId);

        var listRequest = Authorized(
            HttpMethod.Get,
            $"/api/inspections/{started.InspectionRunId}/evidence",
            token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var evidence = (await listResponse.Content.ReadFromJsonAsync<List<InspectionRunEvidenceResponse>>())!;
        Assert.Single(evidence);
    }

    [Fact]
    public async Task Cannot_upload_defect_evidence_when_resolved()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var createDefectRequest = Authorized(HttpMethod.Post, "/api/defects", token);
        createDefectRequest.Content = JsonContent.Create(new CreateDefectRequest(
            assetId,
            "Resolved defect",
            string.Empty,
            "low"));
        var createDefectResponse = await _maintainarrClient.SendAsync(createDefectRequest);
        createDefectResponse.EnsureSuccessStatusCode();
        var defect = (await createDefectResponse.Content.ReadFromJsonAsync<DefectDetailResponse>())!;

        var resolveRequest = Authorized(HttpMethod.Patch, $"/api/defects/{defect.DefectId}/status", token);
        resolveRequest.Content = JsonContent.Create(new UpdateDefectStatusRequest("resolved"));
        await _maintainarrClient.SendAsync(resolveRequest);

        var uploadRequest = Authorized(HttpMethod.Post, $"/api/defects/{defect.DefectId}/evidence", token);
        uploadRequest.Content = JsonContent.Create(new CreateMaintainArrEvidenceRequest(
            "defect_photo",
            "late.jpg",
            "image/jpeg",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("late")),
            null));
        var uploadResponse = await _maintainarrClient.SendAsync(uploadRequest);
        Assert.Equal(HttpStatusCode.Conflict, uploadResponse.StatusCode);
    }

    private async Task<Guid> SeedAssetOnlyAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            $"EVD-ASSET-{Guid.NewGuid():N}".Substring(0, 12),
            "Evidence Test Asset",
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
            null,
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
            "Inspection Evidence Asset",
            string.Empty,
            "yard-a"));
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
            $"{productKey}-defect-evidence-test",
            $"{productKey} Defect Evidence Test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
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
}
