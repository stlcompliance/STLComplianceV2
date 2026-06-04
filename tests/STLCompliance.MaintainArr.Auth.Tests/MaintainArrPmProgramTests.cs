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
using static MaintainArr.Api.Entities.MaintenancePlatformEventRelatedEntityTypes;
using static MaintainArr.Api.Entities.MaintenancePlatformOutboxEventKinds;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrPmProgramTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PmProgramNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"PmProgramMaintainArr-{Guid.NewGuid():N}";

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
    public async Task Pm_program_builder_crud_happy_path()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetTypeId, assetId, scheduleId) = await SeedAssetWithPmScheduleAsync(token);

        using (var scope = _maintainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
            var outbox = await db.MaintenancePlatformOutboxEvents
                .AsNoTracking()
                .Where(x => x.TenantId == PlatformSeeder.DemoTenantId
                    && x.RelatedEntityType == PmSchedule
                    && x.RelatedEntityId == scheduleId)
                .ToListAsync();

            Assert.Contains(outbox, x => x.EventKind == PmPlanCreated);
            Assert.Contains(outbox, x => x.EventKind == PmPlanActivated);
        }

        var createProgramRequest = Authorized(HttpMethod.Post, "/api/preventive-maintenance/programs", token);
        createProgramRequest.Content = JsonContent.Create(new CreatePmProgramRequest(
            "forklift-pm",
            "Forklift PM Program",
            "Standard forklift preventive maintenance",
            "asset_type",
            assetTypeId,
            null,
            null));
        var createProgramResponse = await _maintainarrClient.SendAsync(createProgramRequest);
        createProgramResponse.EnsureSuccessStatusCode();
        var program = (await createProgramResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;
        Assert.Equal("draft", program.Status);
        Assert.Equal("asset_type", program.ScopeType);
        Assert.Equal(assetTypeId, program.AssetTypeId);

        var replaceSchedulesRequest = Authorized(
            HttpMethod.Put,
            $"/api/preventive-maintenance/programs/{program.PmProgramId}/schedules",
            token);
        replaceSchedulesRequest.Content = JsonContent.Create(new ReplacePmProgramSchedulesRequest([scheduleId]));
        var replaceSchedulesResponse = await _maintainarrClient.SendAsync(replaceSchedulesRequest);
        replaceSchedulesResponse.EnsureSuccessStatusCode();
        var withSchedules = (await replaceSchedulesResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;
        Assert.Single(withSchedules.Schedules);
        Assert.Equal(scheduleId, withSchedules.Schedules[0].PmScheduleId);

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/preventive-maintenance/programs/{program.PmProgramId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdatePmProgramStatusRequest("active"));
        var activateResponse = await _maintainarrClient.SendAsync(activateRequest);
        activateResponse.EnsureSuccessStatusCode();
        var activated = (await activateResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;
        Assert.Equal("active", activated.Status);

        var getRequest = Authorized(
            HttpMethod.Get,
            $"/api/preventive-maintenance/programs/{program.PmProgramId}",
            token);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var detail = (await getResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;
        Assert.Equal("oil-change", detail.Schedules[0].ScheduleKey);

        var listRequest = Authorized(HttpMethod.Get, "/api/preventive-maintenance/programs", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var summaries = (await listResponse.Content.ReadFromJsonAsync<List<PmProgramSummaryResponse>>())!;
        Assert.Contains(summaries, x => x.PmProgramId == program.PmProgramId && x.ScheduleCount == 1);

        var assetScopedRequest = Authorized(HttpMethod.Post, "/api/preventive-maintenance/programs", token);
        assetScopedRequest.Content = JsonContent.Create(new CreatePmProgramRequest(
            "asset-pm",
            "Single Asset PM",
            "Asset-scoped PM program",
            "asset",
            null,
            assetId,
            [scheduleId]));
        var assetScopedResponse = await _maintainarrClient.SendAsync(assetScopedRequest);
        Assert.Equal(HttpStatusCode.Conflict, assetScopedResponse.StatusCode);
    }

    [Fact]
    public async Task Activate_program_without_schedules_returns_bad_request()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createProgramRequest = Authorized(HttpMethod.Post, "/api/preventive-maintenance/programs", token);
        createProgramRequest.Content = JsonContent.Create(new CreatePmProgramRequest(
            "empty-program",
            "Empty Program",
            "No schedules yet",
            "asset_type",
            assetTypeId,
            null,
            null));
        var createProgramResponse = await _maintainarrClient.SendAsync(createProgramRequest);
        createProgramResponse.EnsureSuccessStatusCode();
        var program = (await createProgramResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/preventive-maintenance/programs/{program.PmProgramId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdatePmProgramStatusRequest("active"));
        var activateResponse = await _maintainarrClient.SendAsync(activateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, activateResponse.StatusCode);
    }

    [Fact]
    public async Task Pm_program_manage_denied_for_technician()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_technician");
        var request = Authorized(HttpMethod.Post, "/api/preventive-maintenance/programs", token);
        request.Content = JsonContent.Create(new CreatePmProgramRequest(
            "denied-program",
            "Denied",
            "Should fail",
            "asset_type",
            Guid.NewGuid(),
            null,
            null));

        var response = await _maintainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Assign_schedule_outside_scope_returns_bad_request()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetTypeId, _, scheduleId) = await SeedAssetWithPmScheduleAsync(token);
        var otherAssetTypeId = await SeedAssetTypeAsync(token, "crane", "Crane");

        var createProgramRequest = Authorized(HttpMethod.Post, "/api/preventive-maintenance/programs", token);
        createProgramRequest.Content = JsonContent.Create(new CreatePmProgramRequest(
            "scope-test",
            "Scope Test",
            "Mismatch scope test",
            "asset_type",
            otherAssetTypeId,
            null,
            null));
        var createProgramResponse = await _maintainarrClient.SendAsync(createProgramRequest);
        createProgramResponse.EnsureSuccessStatusCode();
        var program = (await createProgramResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;

        var replaceSchedulesRequest = Authorized(
            HttpMethod.Put,
            $"/api/preventive-maintenance/programs/{program.PmProgramId}/schedules",
            token);
        replaceSchedulesRequest.Content = JsonContent.Create(new ReplacePmProgramSchedulesRequest([scheduleId]));
        var replaceSchedulesResponse = await _maintainarrClient.SendAsync(replaceSchedulesRequest);
        Assert.Equal(HttpStatusCode.BadRequest, replaceSchedulesResponse.StatusCode);
    }

    [Fact]
    public async Task Pm_programs_v1_alias_crud_happy_path()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var (assetTypeId, _, scheduleId) = await SeedAssetWithPmScheduleAsync(token);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/pm-programs", token);
        createRequest.Content = JsonContent.Create(new CreatePmProgramRequest(
            "v1-forklift-pm",
            "V1 Forklift PM Program",
            "v1 alias path",
            "asset_type",
            assetTypeId,
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var program = (await createResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;

        var replaceSchedulesRequest = Authorized(
            HttpMethod.Put,
            $"/api/v1/pm-programs/{program.PmProgramId}/schedules",
            token);
        replaceSchedulesRequest.Content = JsonContent.Create(new ReplacePmProgramSchedulesRequest([scheduleId]));
        (await _maintainarrClient.SendAsync(replaceSchedulesRequest)).EnsureSuccessStatusCode();

        var activateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/v1/pm-programs/{program.PmProgramId}/status",
            token);
        activateRequest.Content = JsonContent.Create(new UpdatePmProgramStatusRequest("active"));
        var activateResponse = await _maintainarrClient.SendAsync(activateRequest);
        activateResponse.EnsureSuccessStatusCode();
        var activated = (await activateResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;
        Assert.Equal("active", activated.Status);

        var getRequest = Authorized(HttpMethod.Get, $"/api/v1/pm-programs/{program.PmProgramId}", token);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var detail = (await getResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;
        Assert.Equal(program.PmProgramId, detail.PmProgramId);

        var listRequest = Authorized(HttpMethod.Get, "/api/v1/pm-programs", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var summaries = (await listResponse.Content.ReadFromJsonAsync<List<PmProgramSummaryResponse>>())!;
        Assert.Contains(summaries, x => x.PmProgramId == program.PmProgramId);
    }

    private async Task<(Guid AssetTypeId, Guid AssetId, Guid ScheduleId)> SeedAssetWithPmScheduleAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            "FL-001",
            "Forklift 1",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        var createScheduleRequest = Authorized(HttpMethod.Post, "/api/preventive-maintenance/schedules", token);
        createScheduleRequest.Content = JsonContent.Create(new CreatePmScheduleRequest(
            asset.AssetId,
            "oil-change",
            "Oil Change",
            "Quarterly oil change",
            90,
            DateTimeOffset.UtcNow.AddDays(30)));
        var createScheduleResponse = await _maintainarrClient.SendAsync(createScheduleRequest);
        createScheduleResponse.EnsureSuccessStatusCode();
        var schedule = (await createScheduleResponse.Content.ReadFromJsonAsync<PmScheduleResponse>())!;

        return (assetTypeId, asset.AssetId, schedule.PmScheduleId);
    }

    private async Task<Guid> SeedAssetTypeAsync(
        string token,
        string typeKey = "forklift",
        string typeName = "Forklift")
    {
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"{typeKey}-class",
            $"{typeName} Class",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            typeKey,
            typeName,
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
            new RedeemHandoffRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<HandoffSessionResponse>())!;
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
            $"{productKey}-pm-program-test",
            $"{productKey} PM Program Test",
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
