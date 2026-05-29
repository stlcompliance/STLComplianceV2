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
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Services;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Services;
using TrainArrIntegration = TrainArr.Api.Endpoints.IntegrationEndpoints;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using SupplyArrIntegration = SupplyArr.Api.Endpoints.IntegrationEndpoints;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrSupplyArrMaterialDemandTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _supplyarrIntegrationToken = null!;
    private string _trainarrToStaffarrToken = null!;
    private string _trainarrStatusCallbackToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TrainSupplyDemandNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"TrainSupplyDemandTrainArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"TrainSupplyDemandSupplyArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"TrainSupplyDemandStaffArr-{Guid.NewGuid():N}";

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
            "trainarr",
            ["supplyarr"],
            SupplyArrIntegration.TrainarrDemandIngestActionScope);
        _trainarrToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            $"{StaffArrIntegration.TrainingBlockerIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementReadActionScope}");
        _trainarrStatusCallbackToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["trainarr"],
            TrainArrIntegration.SupplyarrDemandStatusIngestActionScope);
        var trainarrHandoffToken = await IssueServiceTokenAsync(adminToken, "trainarr", ["trainarr"], "launch.redeem");
        var supplyarrHandoffToken = await IssueServiceTokenAsync(adminToken, "supplyarr", ["supplyarr"], "launch.redeem");

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
            });
        });
        _staffarrClient = _staffarrFactory.CreateClient();
        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        WebApplicationFactory<global::SupplyArr.Api.Program>? supplyarrFactoryRef = null;
        WebApplicationFactory<global::TrainArr.Api.Program>? trainarrFactoryRef = null;

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", trainarrHandoffToken);
            builder.UseSetting("SupplyArr:BaseUrl", "http://localhost");
            builder.UseSetting("SupplyArr:ServiceToken", _supplyarrIntegrationToken);
            builder.UseSetting("StaffArr:BaseUrl", _staffarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", _trainarrToStaffarrToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<SupplyArrDemandClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => supplyarrFactoryRef!.Server.CreateHandler());
                services.AddHttpClient<StaffArrTrainingBlockerClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrTrainingAcknowledgementClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
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
            builder.UseSetting("TrainArr:BaseUrl", trainarrFactoryRef!.Server.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("TrainArr:ServiceToken", _trainarrStatusCallbackToken);
            builder.UseSetting("StaffArr:EnforceProcurementApprovalAuthority", "false");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<TrainArrDemandStatusClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => trainarrFactoryRef!.Server.CreateHandler());
            });
        });

        trainarrFactoryRef = _trainarrFactory;
        supplyarrFactoryRef = _supplyarrFactory;
        _trainarrClient = _trainarrFactory.CreateClient();
        _supplyarrClient = _supplyarrFactory.CreateClient();

        using var trainScope = _trainarrFactory.Services.CreateScope();
        await trainScope.ServiceProvider.GetRequiredService<TrainArrDbContext>().Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _supplyarrClient.Dispose();
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _supplyarrFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Material_demand_publish_creates_supplyarr_mirror()
    {
        var trainarrToken = CreateTrainArrAccessToken(["trainarr"], "trainarr_admin");
        var supplyarrToken = CreateSupplyArrAccessToken(["supplyarr"]);
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);
        var assignmentId = await CreateAssignmentAsync(trainarrToken, personId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/material-demand", trainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateTrainingAssignmentMaterialDemandLineRequest(
            partId, "MAT-001", "Training kit", 1m, "each", null));
        (await _trainarrClient.SendAsync(createLineRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/material-demand/publish", trainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishTrainingAssignmentMaterialDemandRequest(false));
        var publishResponse = await _trainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishTrainingAssignmentMaterialDemandResponse>())!;
        Assert.NotEqual(Guid.Empty, published.DemandRefId);

        var listRefsRequest = Authorized(HttpMethod.Get, "/api/trainarr-demand-refs", supplyarrToken);
        var listRefsResponse = await _supplyarrClient.SendAsync(listRefsRequest);
        listRefsResponse.EnsureSuccessStatusCode();
        var refs = (await listRefsResponse.Content.ReadFromJsonAsync<List<TrainArrDemandRefResponse>>())!;
        var demandRef = Assert.Single(refs);
        Assert.Equal(assignmentId, demandRef.TrainarrAssignmentId);
        Assert.Equal("received", demandRef.Status);
    }

    [Fact]
    public async Task Trainarr_demand_ingest_is_idempotent()
    {
        var trainarrToken = CreateTrainArrAccessToken(["trainarr"], "trainarr_admin");
        var supplyarrToken = CreateSupplyArrAccessToken(["supplyarr"]);
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);
        var assignmentId = await CreateAssignmentAsync(trainarrToken, personId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/material-demand", trainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateTrainingAssignmentMaterialDemandLineRequest(
            partId, "MAT-002", "Replay kit", 1m, "each", null));
        await _trainarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/material-demand/publish", trainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishTrainingAssignmentMaterialDemandRequest(false));
        var firstPublish = await _trainarrClient.SendAsync(publishRequest);
        firstPublish.EnsureSuccessStatusCode();
        var first = (await firstPublish.Content.ReadFromJsonAsync<PublishTrainingAssignmentMaterialDemandResponse>())!;

        var ingestRequest = ServiceAuthorized(HttpMethod.Post, "/api/integrations/trainarr-demand", _supplyarrIntegrationToken);
        ingestRequest.Content = JsonContent.Create(new IngestTrainarrDemandRequest(
            PlatformSeeder.DemoTenantId,
            first.PublicationId,
            assignmentId,
            "replay-key",
            personId,
            "Replay",
            null,
            false,
            [new IngestTrainarrDemandLineRequest(Guid.NewGuid(), partId, "MAT-002", "Replay kit", 1m, "each", null)]));
        var replayResponse = await _supplyarrClient.SendAsync(ingestRequest);
        replayResponse.EnsureSuccessStatusCode();
        var replay = (await replayResponse.Content.ReadFromJsonAsync<TrainarrDemandIntakeResponse>())!;
        Assert.True(replay.IdempotentReplay);
        Assert.Equal(first.DemandRefId, replay.DemandRefId);
    }

    [Fact]
    public async Task Pr_submit_updates_trainarr_procurement_status()
    {
        var trainarrToken = CreateTrainArrAccessToken(["trainarr"], "trainarr_admin");
        var supplyarrToken = CreateSupplyArrAccessToken(["supplyarr"]);
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);
        var assignmentId = await CreateAssignmentAsync(trainarrToken, personId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/material-demand", trainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateTrainingAssignmentMaterialDemandLineRequest(
            partId, "STAT-T-001", "TrainArr status callback kit", 1m, "each", null));
        await _trainarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/material-demand/publish", trainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishTrainingAssignmentMaterialDemandRequest(true));
        var publishResponse = await _trainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishTrainingAssignmentMaterialDemandResponse>())!;
        Assert.NotNull(published.PurchaseRequestId);

        var afterDraftLines = await ListDemandLinesAsync(trainarrToken, assignmentId);
        Assert.All(afterDraftLines, line => Assert.Equal("pr_drafted", line.ProcurementStatus));

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{published.PurchaseRequestId}/submit",
            supplyarrToken);
        var submitResponse = await _supplyarrClient.SendAsync(submitRequest);
        var submitBody = await submitResponse.Content.ReadAsStringAsync();
        Assert.True(submitResponse.IsSuccessStatusCode, submitBody);

        var afterSubmitLines = await ListDemandLinesAsync(trainarrToken, assignmentId);
        Assert.All(afterSubmitLines, line => Assert.Equal("pr_submitted", line.ProcurementStatus));

        var statusEvents = await ListStatusEventsAsync(trainarrToken, assignmentId);
        Assert.Contains(statusEvents, e => e.ProcurementStatus == "pr_submitted");
    }

    [Fact]
    public async Task Supplyarr_demand_status_callback_is_idempotent_for_trainarr()
    {
        var trainarrToken = CreateTrainArrAccessToken(["trainarr"], "trainarr_admin");
        var supplyarrToken = CreateSupplyArrAccessToken(["supplyarr"]);
        var partId = await SeedSupplyArrPartAsync(supplyarrToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);
        var assignmentId = await CreateAssignmentAsync(trainarrToken, personId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/material-demand", trainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateTrainingAssignmentMaterialDemandLineRequest(
            partId, "IDEM-T-001", "TrainArr idempotent callback kit", 1m, "each", null));
        await _trainarrClient.SendAsync(createLineRequest);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/material-demand/publish", trainarrToken);
        publishRequest.Content = JsonContent.Create(new PublishTrainingAssignmentMaterialDemandRequest(false));
        var publishResponse = await _trainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<PublishTrainingAssignmentMaterialDemandResponse>())!;

        var callbackPublicationId = Guid.NewGuid();
        var callbackRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/supplyarr-demand-status",
            _trainarrStatusCallbackToken);
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

        var firstResponse = await _trainarrClient.SendAsync(callbackRequest);
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<IngestSupplyarrDemandStatusResponse>())!;
        Assert.False(first.IdempotentReplay);

        var replayRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/supplyarr-demand-status",
            _trainarrStatusCallbackToken);
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
        var replayResponse = await _trainarrClient.SendAsync(replayRequest);
        replayResponse.EnsureSuccessStatusCode();
        var replay = (await replayResponse.Content.ReadFromJsonAsync<IngestSupplyarrDemandStatusResponse>())!;
        Assert.True(replay.IdempotentReplay);
        Assert.Equal(first.StatusEventId, replay.StatusEventId);
    }

    [Fact]
    public async Task Trainarr_demand_ingest_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/trainarr-demand");
        request.Content = JsonContent.Create(new IngestTrainarrDemandRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "x",
            Guid.NewGuid(),
            "Unauthorized",
            null,
            false,
            [new IngestTrainarrDemandLineRequest(Guid.NewGuid(), null, "T", "T", 1m, "each", null)]));
        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<IReadOnlyList<TrainingAssignmentMaterialDemandLineResponse>> ListDemandLinesAsync(
        string trainarrToken,
        Guid assignmentId)
    {
        var request = Authorized(
            HttpMethod.Get,
            $"/api/training-assignments/{assignmentId}/material-demand",
            trainarrToken);
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<TrainingAssignmentMaterialDemandLineResponse>>())!;
    }

    private async Task<IReadOnlyList<TrainingAssignmentMaterialDemandStatusEventResponse>> ListStatusEventsAsync(
        string trainarrToken,
        Guid assignmentId)
    {
        var request = Authorized(
            HttpMethod.Get,
            $"/api/training-assignments/{assignmentId}/material-demand/status-events",
            trainarrToken);
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<TrainingAssignmentMaterialDemandStatusEventResponse>>())!;
    }

    private async Task<Guid> CreateAssignmentAsync(string trainarrToken, Guid personId)
    {
        var definitionId = await CreateTrainingDefinitionAsync(trainarrToken);
        var assignment = await TrainArrQualificationCheckTestHelper.CreateManualAssignmentAsync(
            _trainarrClient,
            trainarrToken,
            personId,
            definitionId,
            "demand_test",
            DateTimeOffset.UtcNow.AddDays(7));
        return assignment.AssignmentId;
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string trainarrToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"demand_{Guid.NewGuid():N}"[..16],
            "Demand Test Training",
            "Material demand test",
            "demand_test",
            "Demand Test"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var definition = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return definition.TrainingDefinitionId;
    }

    private async Task<Guid> SeedSupplyArrPartAsync(string token)
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"part-{Guid.NewGuid():N}".Substring(0, 12),
            null,
            "Train Demand Part",
            string.Empty,
            "general",
            "each",
            "Acme",
            "TR-100"));
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
                GivenName = "Demand",
                FamilyName = "Trainee",
                DisplayName = "Demand Trainee",
                PrimaryEmail = $"demand.trainee.{personId:N}@example.com",
                EmploymentStatus = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }
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

    private string CreateTrainArrAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
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
