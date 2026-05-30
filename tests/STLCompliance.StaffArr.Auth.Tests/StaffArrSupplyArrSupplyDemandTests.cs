using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using SupplyArrIntegration = SupplyArr.Api.Endpoints.IntegrationEndpoints;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrSupplyArrSupplyDemandTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _supplyarrIntegrationToken = null!;
    private string _staffarrStatusCallbackToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"StaffSupplyDemandNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"StaffSupplyDemandStaffArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"StaffSupplyDemandSupplyArr-{Guid.NewGuid():N}";

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
        _supplyarrIntegrationToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            ["supplyarr"],
            SupplyArrIntegration.StaffarrDemandIngestActionScope);
        _staffarrStatusCallbackToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["staffarr"],
            StaffArrIntegration.SupplyarrDemandStatusIngestActionScope);
        var staffarrHandoffToken = await IssueServiceTokenAsync(adminToken, "staffarr", ["staffarr"], "launch.redeem");
        var supplyarrHandoffToken = await IssueServiceTokenAsync(adminToken, "supplyarr", ["supplyarr"], "launch.redeem");

        WebApplicationFactory<global::SupplyArr.Api.Program>? supplyarrFactoryRef = null;

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", staffarrHandoffToken);
            builder.UseSetting("SupplyArr:BaseUrl", "http://localhost");
            builder.UseSetting("SupplyArr:ServiceToken", _supplyarrIntegrationToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<SupplyArrDemandClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => supplyarrFactoryRef!.Server.CreateHandler());
            });
        });

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", supplyarrHandoffToken);
            builder.UseSetting("StaffArr:BaseUrl", _staffarrFactory.Server.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", _staffarrStatusCallbackToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrDemandStatusClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
            });
        });

        supplyarrFactoryRef = _supplyarrFactory;
        _staffarrClient = _staffarrFactory.CreateClient();
        _supplyarrClient = _supplyarrFactory.CreateClient();

        using var staffScope = _staffarrFactory.Services.CreateScope();
        await staffScope.ServiceProvider.GetRequiredService<StaffArrDbContext>().Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Supply_demand_publish_creates_supplyarr_mirror()
    {
        var staffarrToken = CreateStaffArrAccessToken(["staffarr"]);
        var supplyarrToken = CreateSupplyArrAccessToken(["supplyarr"]);
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);
        var incidentId = await CreateIncidentAsync(staffarrToken, personId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/incidents/{incidentId}/supply-demand", staffarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateIncidentSupplyDemandLineRequest(
            partId, "SUP-001", "Incident supplies", 2m, "each", null));
        (await _staffarrClient.SendAsync(createLineRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Post, $"/api/incidents/{incidentId}/supply-demand/publish", staffarrToken);
        publishRequest.Content = JsonContent.Create(new PublishIncidentSupplyDemandRequest(false));
        var publishResponse = await _staffarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishIncidentSupplyDemandResponse>())!;
        Assert.NotEqual(Guid.Empty, published.DemandRefId);

        var listRefsRequest = Authorized(HttpMethod.Get, "/api/staffarr-demand-refs", supplyarrToken);
        var listRefsResponse = await _supplyarrClient.SendAsync(listRefsRequest);
        listRefsResponse.EnsureSuccessStatusCode();
        var refs = (await listRefsResponse.Content.ReadFromJsonAsync<List<StaffArrDemandRefResponse>>())!;
        var demandRef = Assert.Single(refs);
        Assert.Equal(incidentId, demandRef.StaffarrIncidentId);
        Assert.Equal("received", demandRef.Status);
    }

    [Fact]
    public async Task Supply_demand_publish_v1_alias_creates_supplyarr_mirror()
    {
        var staffarrToken = CreateStaffArrAccessToken(["staffarr"]);
        var supplyarrToken = CreateSupplyArrAccessToken(["supplyarr"]);
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);
        var incidentId = await CreateIncidentAsync(staffarrToken, personId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/v1/incidents/{incidentId}/supply-demand", staffarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateIncidentSupplyDemandLineRequest(
            partId, "SUP-V1-001", "Incident supplies v1", 2m, "each", null));
        (await _staffarrClient.SendAsync(createLineRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Post, $"/api/v1/incidents/{incidentId}/supply-demand/publish", staffarrToken);
        publishRequest.Content = JsonContent.Create(new PublishIncidentSupplyDemandRequest(false));
        var publishResponse = await _staffarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishIncidentSupplyDemandResponse>())!;
        Assert.NotEqual(Guid.Empty, published.DemandRefId);
    }

    [Fact]
    public async Task Staffarr_demand_ingest_is_idempotent()
    {
        var staffarrToken = CreateStaffArrAccessToken(["staffarr"]);
        var supplyarrToken = CreateSupplyArrAccessToken(["supplyarr"]);
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);
        var incidentId = await CreateIncidentAsync(staffarrToken, personId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/incidents/{incidentId}/supply-demand", staffarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateIncidentSupplyDemandLineRequest(
            partId, "SUP-002", "Replay supplies", 1m, "each", null));
        await _staffarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/incidents/{incidentId}/supply-demand/publish", staffarrToken);
        publishRequest.Content = JsonContent.Create(new PublishIncidentSupplyDemandRequest(false));
        var firstPublish = await _staffarrClient.SendAsync(publishRequest);
        firstPublish.EnsureSuccessStatusCode();
        var first = (await firstPublish.Content.ReadFromJsonAsync<PublishIncidentSupplyDemandResponse>())!;

        var ingestRequest = ServiceAuthorized(HttpMethod.Post, "/api/integrations/staffarr-demand", _supplyarrIntegrationToken);
        ingestRequest.Content = JsonContent.Create(new IngestStaffarrDemandRequest(
            PlatformSeeder.DemoTenantId,
            first.PublicationId,
            incidentId,
            personId,
            "Replay incident",
            "Replay",
            null,
            false,
            [new IngestStaffarrDemandLineRequest(Guid.NewGuid(), partId, "SUP-002", "Replay supplies", 1m, "each", null)]));
        var replayResponse = await _supplyarrClient.SendAsync(ingestRequest);
        replayResponse.EnsureSuccessStatusCode();
        var replay = (await replayResponse.Content.ReadFromJsonAsync<StaffarrDemandIntakeResponse>())!;
        Assert.True(replay.IdempotentReplay);
        Assert.Equal(first.DemandRefId, replay.DemandRefId);
    }

    [Fact]
    public async Task Pr_submit_updates_staffarr_procurement_status()
    {
        var staffarrToken = CreateStaffArrAccessToken(["staffarr"]);
        var supplyarrToken = CreateSupplyArrAccessToken(["supplyarr"]);
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);
        var incidentId = await CreateIncidentAsync(staffarrToken, personId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/incidents/{incidentId}/supply-demand", staffarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateIncidentSupplyDemandLineRequest(
            partId, "STAT-S-001", "StaffArr status callback supplies", 1m, "each", null));
        await _staffarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/incidents/{incidentId}/supply-demand/publish", staffarrToken);
        publishRequest.Content = JsonContent.Create(new PublishIncidentSupplyDemandRequest(true));
        var publishResponse = await _staffarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishIncidentSupplyDemandResponse>())!;
        Assert.NotNull(published.PurchaseRequestId);

        var afterDraftLines = await ListDemandLinesAsync(staffarrToken, incidentId);
        Assert.All(afterDraftLines, line => Assert.Equal("pr_drafted", line.ProcurementStatus));

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{published.PurchaseRequestId}/submit",
            supplyarrToken);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var afterSubmitLines = await ListDemandLinesAsync(staffarrToken, incidentId);
        Assert.All(afterSubmitLines, line => Assert.Equal("pr_submitted", line.ProcurementStatus));
    }

    [Fact]
    public async Task Supplyarr_demand_status_callback_is_idempotent_for_staffarr()
    {
        var staffarrToken = CreateStaffArrAccessToken(["staffarr"]);
        var supplyarrToken = CreateSupplyArrAccessToken(["supplyarr"]);
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);
        var incidentId = await CreateIncidentAsync(staffarrToken, personId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/incidents/{incidentId}/supply-demand", staffarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateIncidentSupplyDemandLineRequest(
            partId, "IDEM-S-001", "StaffArr idempotent callback supplies", 1m, "each", null));
        await _staffarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/incidents/{incidentId}/supply-demand/publish", staffarrToken);
        publishRequest.Content = JsonContent.Create(new PublishIncidentSupplyDemandRequest(false));
        var publishResponse = await _staffarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishIncidentSupplyDemandResponse>())!;

        var callbackPublicationId = Guid.NewGuid();
        var callbackRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/supplyarr-demand-status",
            _staffarrStatusCallbackToken);
        callbackRequest.Content = JsonContent.Create(new IngestSupplyarrDemandStatusRequest(
            PlatformSeeder.DemoTenantId,
            published.PublicationId,
            published.DemandRefId,
            callbackPublicationId,
            "pr_submitted",
            "pr_submitted",
            null,
            null,
            null,
            null,
            "Replay test",
            DateTimeOffset.UtcNow));

        var firstResponse = await _staffarrClient.SendAsync(callbackRequest);
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<IngestSupplyarrDemandStatusResponse>())!;
        Assert.False(first.IdempotentReplay);

        var replayRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/supplyarr-demand-status",
            _staffarrStatusCallbackToken);
        replayRequest.Content = JsonContent.Create(new IngestSupplyarrDemandStatusRequest(
            PlatformSeeder.DemoTenantId,
            published.PublicationId,
            published.DemandRefId,
            callbackPublicationId,
            "pr_submitted",
            "pr_submitted",
            null,
            null,
            null,
            null,
            "Replay test",
            DateTimeOffset.UtcNow));
        var replayResponse = await _staffarrClient.SendAsync(replayRequest);
        replayResponse.EnsureSuccessStatusCode();
        var replay = (await replayResponse.Content.ReadFromJsonAsync<IngestSupplyarrDemandStatusResponse>())!;
        Assert.True(replay.IdempotentReplay);
        Assert.Equal(first.StatusEventId, replay.StatusEventId);
    }

    [Fact]
    public async Task Staffarr_demand_ingest_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/staffarr-demand");
        request.Content = JsonContent.Create(new IngestStaffarrDemandRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Unauthorized",
            "Unauthorized",
            null,
            false,
            [new IngestStaffarrDemandLineRequest(Guid.NewGuid(), null, "T", "T", 1m, "each", null)]));
        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<IReadOnlyList<IncidentSupplyDemandLineResponse>> ListDemandLinesAsync(
        string staffarrToken,
        Guid incidentId)
    {
        var request = Authorized(HttpMethod.Get, $"/api/incidents/{incidentId}/supply-demand", staffarrToken);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<IncidentSupplyDemandLineResponse>>())!;
    }

    private async Task<Guid> CreateIncidentAsync(string staffarrToken, Guid personId)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", staffarrToken);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "safety",
            "medium",
            "Supply demand incident",
            "Incident for supply demand test",
            DateTimeOffset.UtcNow.AddHours(-1)));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        return created.IncidentId;
    }

    private async Task<Guid> SeedSupplyArrPartAsync(string token)
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"part-{Guid.NewGuid():N}".Substring(0, 12),
            null,
            "Staff Demand Part",
            string.Empty,
            "general",
            "each",
            "Acme",
            "ST-100"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;
        return part.PartId;
    }

    private async Task SeedStaffPersonAsync(Guid personId)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        if (!await db.People.AnyAsync(x => x.Id == personId))
        {
            db.People.Add(new StaffPerson
            {
                Id = personId,
                TenantId = PlatformSeeder.DemoTenantId,
                GivenName = "Supply",
                FamilyName = "Incident",
                DisplayName = "Supply Incident",
                PrimaryEmail = $"supply.incident.{personId:N}@example.com",
                EmploymentStatus = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }
    }

    private string CreateStaffArrAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey = "tenant_admin")
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
    }

    private string CreateSupplyArrAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey = "tenant_admin")
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<SupplyArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-demand-test-{Guid.NewGuid():N}",
            $"{sourceProduct} demand test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            allowedProducts,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!.AccessToken;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!.AccessToken;
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        foreach (var descriptor in services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList())
        {
            services.Remove(descriptor);
        }
    }
}
