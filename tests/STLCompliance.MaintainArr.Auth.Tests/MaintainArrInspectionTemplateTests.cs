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
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrInspectionTemplateTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"InspectionTemplateNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"InspectionTemplateMaintainArr-{Guid.NewGuid():N}";

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
    public async Task Inspection_template_builder_crud_happy_path()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "daily-walkaround",
            "Daily Walkaround",
            "Pre-shift equipment walkaround"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;
        Assert.Equal("draft", template.Status);
        Assert.Equal(1, template.Version);

        var createCategoryRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/categories",
            token);
        createCategoryRequest.Content = JsonContent.Create(new CreateInspectionTemplateCategoryRequest(
            "safety",
            "Safety",
            10));
        var createCategoryResponse = await _maintainarrClient.SendAsync(createCategoryRequest);
        createCategoryResponse.EnsureSuccessStatusCode();
        var category = (await createCategoryResponse.Content.ReadFromJsonAsync<InspectionTemplateCategoryResponse>())!;

        var createItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "horn-works",
            "Horn operates correctly",
            "pass_fail",
            true,
            10,
            category.CategoryId));
        var createItemResponse = await _maintainarrClient.SendAsync(createItemRequest);
        createItemResponse.EnsureSuccessStatusCode();

        var replaceAssetTypesRequest = Authorized(
            HttpMethod.Put,
            $"/api/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([assetTypeId]));
        var replaceAssetTypesResponse = await _maintainarrClient.SendAsync(replaceAssetTypesRequest);
        replaceAssetTypesResponse.EnsureSuccessStatusCode();
        var linked = (await replaceAssetTypesResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;
        Assert.Single(linked.LinkedAssetTypes);
        Assert.Equal(assetTypeId, linked.LinkedAssetTypes[0].AssetTypeId);

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        var activateResponse = await _maintainarrClient.SendAsync(activateRequest);
        activateResponse.EnsureSuccessStatusCode();
        var activated = (await activateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;
        Assert.Equal("active", activated.Status);
        Assert.True(activated.Version > template.Version);

        var listRequest = Authorized(HttpMethod.Get, "/api/inspection-templates", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var summaries = (await listResponse.Content.ReadFromJsonAsync<List<InspectionTemplateSummaryResponse>>())!;
        Assert.Contains(summaries, x => x.InspectionTemplateId == template.InspectionTemplateId && x.ChecklistItemCount == 1);
    }

    [Fact]
    public async Task Inspection_template_builder_crud_v1_alias_happy_path()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/v1/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "daily-v1-walkaround",
            "Daily V1 Walkaround",
            "Pre-shift v1 walkaround"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createCategoryRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/inspection-templates/{template.InspectionTemplateId}/categories",
            token);
        createCategoryRequest.Content = JsonContent.Create(new CreateInspectionTemplateCategoryRequest(
            "safety-v1",
            "Safety V1",
            10));
        var createCategoryResponse = await _maintainarrClient.SendAsync(createCategoryRequest);
        createCategoryResponse.EnsureSuccessStatusCode();
        var category = (await createCategoryResponse.Content.ReadFromJsonAsync<InspectionTemplateCategoryResponse>())!;

        var createItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "lights-work-v1",
            "Lights operate correctly",
            "pass_fail",
            true,
            10,
            category.CategoryId));
        var createItemResponse = await _maintainarrClient.SendAsync(createItemRequest);
        createItemResponse.EnsureSuccessStatusCode();

        var replaceAssetTypesRequest = Authorized(
            HttpMethod.Put,
            $"/api/v1/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([assetTypeId]));
        var replaceAssetTypesResponse = await _maintainarrClient.SendAsync(replaceAssetTypesRequest);
        replaceAssetTypesResponse.EnsureSuccessStatusCode();
        var linked = (await replaceAssetTypesResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;
        Assert.Single(linked.LinkedAssetTypes);
    }

    [Fact]
    public async Task Activate_template_without_checklist_items_returns_bad_request()
    {
        var token = await RedeemMaintainArrTokenAsync();

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "empty-template",
            "Empty Template",
            string.Empty));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        var activateResponse = await _maintainarrClient.SendAsync(activateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, activateResponse.StatusCode);
    }

    [Fact]
    public async Task Inspection_template_manage_denied_for_technician()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_technician");
        var request = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        request.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "denied-template",
            "Denied",
            string.Empty));

        var response = await _maintainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
            $"{productKey}-inspection-template-test",
            $"{productKey} Inspection Template Test",
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
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
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
}
