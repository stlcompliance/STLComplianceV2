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

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrTechnicianRefTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TechnicianRefNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"TechnicianRefMaintainArr-{Guid.NewGuid():N}";

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
    public async Task Technician_refs_list_and_upsert()
    {
        var token = await RedeemMaintainArrTokenAsync();

        var upsertRequest = Authorized(HttpMethod.Put, "/api/technician-refs", token);
        upsertRequest.Content = JsonContent.Create(new UpsertTechnicianRefRequest(
            "person-tech-001",
            "Alex Technician",
            "active",
            "yard-a",
            null,
            "staffarr-sync-1"));
        var upsertResponse = await _maintainarrClient.SendAsync(upsertRequest);
        upsertResponse.EnsureSuccessStatusCode();
        var upserted = (await upsertResponse.Content.ReadFromJsonAsync<TechnicianRefResponse>())!;
        Assert.Equal("person-tech-001", upserted.PersonId);
        Assert.Equal("Alex Technician", upserted.DisplayName);

        var listRequest = Authorized(HttpMethod.Get, "/api/technician-refs", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<TechnicianRefListResponse>())!;
        Assert.Contains(list.Items, x => x.PersonId == "person-tech-001" && x.DisplayName == "Alex Technician");
    }

    [Fact]
    public async Task Technician_refs_v1_alias_list_and_upsert()
    {
        var token = await RedeemMaintainArrTokenAsync();

        var upsertRequest = Authorized(HttpMethod.Put, "/api/v1/technician-refs", token);
        upsertRequest.Content = JsonContent.Create(new UpsertTechnicianRefRequest(
            "person-tech-v1-001",
            "V1 Alex Technician",
            "active",
            "yard-a",
            null,
            "staffarr-sync-v1"));
        var upsertResponse = await _maintainarrClient.SendAsync(upsertRequest);
        upsertResponse.EnsureSuccessStatusCode();
        var upserted = (await upsertResponse.Content.ReadFromJsonAsync<TechnicianRefResponse>())!;
        Assert.Equal("person-tech-v1-001", upserted.PersonId);
        Assert.Equal("V1 Alex Technician", upserted.DisplayName);

        var listRequest = Authorized(HttpMethod.Get, "/api/v1/technician-refs", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<TechnicianRefListResponse>())!;
        Assert.Contains(list.Items, x => x.PersonId == "person-tech-v1-001" && x.DisplayName == "V1 Alex Technician");
    }

    [Fact]
    public async Task Work_order_assignment_mirrors_technician_ref()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(token);

        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", token);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Replace filter",
            string.Empty,
            "medium",
            "person-tech-002",
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var listRequest = Authorized(HttpMethod.Get, "/api/technician-refs", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<TechnicianRefListResponse>())!;
        Assert.Contains(list.Items, x => x.PersonId == "person-tech-002");
    }

    [Fact]
    public async Task Inspection_voice_guidance_for_in_progress_run()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetId, templateId, _) = await SeedActiveTemplateWithAssetAsync(token);

        var startRequest = Authorized(HttpMethod.Post, "/api/inspections", token);
        startRequest.Content = JsonContent.Create(new StartInspectionRunRequest(assetId, templateId));
        var startResponse = await _maintainarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<InspectionRunDetailResponse>())!;

        var guidanceRequest = Authorized(
            HttpMethod.Get,
            $"/api/inspections/{started.InspectionRunId}/voice-guidance",
            token);
        var guidanceResponse = await _maintainarrClient.SendAsync(guidanceRequest);
        guidanceResponse.EnsureSuccessStatusCode();
        var guidance = (await guidanceResponse.Content.ReadFromJsonAsync<InspectionVoiceGuidanceResponse>())!;
        Assert.Equal(started.InspectionRunId, guidance.InspectionRunId);
        Assert.NotEmpty(guidance.Prompts);
        Assert.Equal(0, guidance.NextUnansweredIndex);
        Assert.Contains("pass", guidance.Prompts[0].VoiceAnswerHint, StringComparison.OrdinalIgnoreCase);

        var normalizeRequest = Authorized(HttpMethod.Post, "/api/inspections/voice/normalize-numeric", token);
        normalizeRequest.Content = JsonContent.Create(new NormalizeVoiceNumericRequest("twelve point five"));
        var normalizeResponse = await _maintainarrClient.SendAsync(normalizeRequest);
        normalizeResponse.EnsureSuccessStatusCode();
        var normalized = (await normalizeResponse.Content.ReadFromJsonAsync<NormalizeVoiceNumericResponse>())!;
        Assert.True(normalized.Understood);
        Assert.Equal(12.5m, normalized.Value);
    }

    private async Task<(Guid AssetId, Guid TemplateId, Guid ChecklistItemId)> SeedActiveTemplateWithAssetAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "voice-check",
            "Voice Check",
            string.Empty));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "brakes-ok",
            "Are the brakes operational?",
            "pass_fail",
            true,
            1,
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
            "TECH-ASSET-1",
            "Technician Ref Asset",
            string.Empty,
            "yard-a"));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        return (asset.AssetId, template.InspectionTemplateId, item.ChecklistItemId);
    }

    private async Task<Guid> SeedAssetOnlyAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);
        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            "WO-ASSET-1",
            "Work Order Asset",
            string.Empty,
            "yard-a"));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
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

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{productKey}-technician-ref-test",
            $"{productKey} Technician Ref Test",
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
            new NexArr.Api.Contracts.LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.AuthTokenResponse>())!;
        return login.AccessToken;
    }
}
