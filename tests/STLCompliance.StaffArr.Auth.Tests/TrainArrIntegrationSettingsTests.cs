using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Endpoints;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrIntegrationSettingsTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _staffarrToTrainarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TrainArrIntegrationSettingsNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"TrainArrIntegrationSettingsTrainArr-{Guid.NewGuid():N}";

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
        _staffarrToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            ["trainarr"],
            IntegrationEndpoints.IncidentRemediationIngestActionScope);

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Integration_settings_defaults_when_missing()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/integration-settings", adminToken));
        response.EnsureSuccessStatusCode();

        var settings = (await response.Content.ReadFromJsonAsync<IntegrationSettingsResponse>())!;
        Assert.True(settings.StaffArrIntegrationEnabled);
        Assert.True(settings.StaffArrIncidentIntakeEnabled);
        Assert.True(settings.ComplianceCoreQualificationChecksEnabled);
        Assert.True(settings.RoutarrQualificationDispatchEnabled);
    }

    [Fact]
    public async Task Integration_settings_upsert_persists_and_writes_audit()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var putResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Put, "/api/integration-settings", adminToken,
                new UpsertIntegrationSettingsRequest(
                    StaffArrIntegrationEnabled: false,
                    StaffArrIncidentIntakeEnabled: false,
                    StaffArrPublicationDeliveryEnabled: false,
                    ComplianceCoreIntegrationEnabled: true,
                    ComplianceCoreQualificationChecksEnabled: false,
                    RoutarrIntegrationEnabled: true,
                    RoutarrQualificationDispatchEnabled: false)));
        putResponse.EnsureSuccessStatusCode();

        var getResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/integration-settings", adminToken));
        var settings = (await getResponse.Content.ReadFromJsonAsync<IntegrationSettingsResponse>())!;
        Assert.False(settings.StaffArrIntegrationEnabled);
        Assert.False(settings.ComplianceCoreQualificationChecksEnabled);

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        Assert.True(await db.AuditEvents.AnyAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "integration_settings.upsert"));
    }

    [Fact]
    public async Task Incident_intake_rejects_when_disabled()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Put, "/api/integration-settings", adminToken,
                new UpsertIntegrationSettingsRequest(
                    StaffArrIntegrationEnabled: true,
                    StaffArrIncidentIntakeEnabled: false,
                    StaffArrPublicationDeliveryEnabled: true,
                    ComplianceCoreIntegrationEnabled: true,
                    ComplianceCoreQualificationChecksEnabled: true,
                    RoutarrIntegrationEnabled: true,
                    RoutarrQualificationDispatchEnabled: true)));

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/incident-remediations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _staffarrToTrainarrToken);
        request.Content = JsonContent.Create(new IngestStaffarrIncidentRemediationRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "training_compliance",
            "high",
            "Disabled intake test",
            "Should be rejected.",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow));

        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Integration_settings_denies_trainer()
    {
        var trainerToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_trainer");
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/integration-settings", trainerToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Integration_probes_returns_items()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/integration-settings/probes", adminToken));
        response.EnsureSuccessStatusCode();

        var probes = (await response.Content.ReadFromJsonAsync<IntegrationProbesResponse>())!;
        Assert.Equal(2, probes.Items.Count);
        Assert.Contains(probes.Items, item => item.IntegrationKey == "staffarr");
        Assert.Contains(probes.Items, item => item.IntegrationKey == "compliancecore");
    }

    [Fact]
    public async Task Events_v1_alias_matches_event_processing_settings_events()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        await SeedTrainingDomainEventAsync();

        var legacyResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/event-processing-settings/events?limit=10", adminToken));
        legacyResponse.EnsureSuccessStatusCode();
        var legacy = (await legacyResponse.Content.ReadFromJsonAsync<TrainingDomainEventsResponse>())!;

        var v1Response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/events?limit=10", adminToken));
        v1Response.EnsureSuccessStatusCode();
        var v1 = (await v1Response.Content.ReadFromJsonAsync<TrainingDomainEventsResponse>())!;

        Assert.Equal(legacy.Items.Count, v1.Items.Count);
    }

    [Fact]
    public async Task Remediation_v1_alias_matches_incident_remediations_list()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var incidentId = Guid.NewGuid();
        var remediationId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var ingestRequest = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/incident-remediations");
        ingestRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _staffarrToTrainarrToken);
        ingestRequest.Content = JsonContent.Create(new IngestStaffarrIncidentRemediationRequest(
            PlatformSeeder.DemoTenantId,
            incidentId,
            remediationId,
            "training_compliance",
            "high",
            "Alias check remediation",
            "Validate remediation alias parity.",
            occurredAt,
            occurredAt));
        var ingestResponse = await _trainarrClient.SendAsync(ingestRequest);
        ingestResponse.EnsureSuccessStatusCode();

        var legacyResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/incident-remediations", adminToken));
        legacyResponse.EnsureSuccessStatusCode();
        var legacy = (await legacyResponse.Content.ReadFromJsonAsync<IReadOnlyList<StaffarrIncidentRemediationResponse>>())!;

        var v1Response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/remediation", adminToken));
        v1Response.EnsureSuccessStatusCode();
        var v1 = (await v1Response.Content.ReadFromJsonAsync<IReadOnlyList<StaffarrIncidentRemediationResponse>>())!;

        Assert.Equal(legacy.Count, v1.Count);
        Assert.Contains(v1, x => x.StaffarrIncidentId == incidentId && x.ReasonCategoryKey == "training_compliance");
    }

    [Fact]
    public async Task Recertification_v1_alias_matches_recertification_settings()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");

        var legacyResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/recertification-settings", adminToken));
        legacyResponse.EnsureSuccessStatusCode();
        var legacy = (await legacyResponse.Content.ReadFromJsonAsync<RecertificationSettingsResponse>())!;

        var v1Response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/recertification", adminToken));
        v1Response.EnsureSuccessStatusCode();
        var v1 = (await v1Response.Content.ReadFromJsonAsync<RecertificationSettingsResponse>())!;

        Assert.Equal(legacy.IsEnabled, v1.IsEnabled);
        Assert.Equal(legacy.LeadDays, v1.LeadDays);
    }

    [Fact]
    public async Task Integrations_v1_alias_accepts_incident_remediation_ingest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/integrations/incident-remediations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _staffarrToTrainarrToken);
        request.Content = JsonContent.Create(new IngestStaffarrIncidentRemediationRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "training_compliance",
            "medium",
            "V1 integration alias",
            "Validate /api/v1/integrations alias.",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow));

        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Integrations_v1_index_lists_expected_integration_paths()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/integrations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _staffarrToTrainarrToken);

        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = json.RootElement
            .GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("path").GetString())
            .ToList();

        Assert.Contains("/api/v1/integrations/incident-remediations", paths);
        Assert.Contains("/api/v1/integrations/qualification-check", paths);
        Assert.Contains("/api/v1/integrations/person-training-history", paths);
    }

    private async Task SeedTrainingDomainEventAsync()
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TrainingDomainEvents.Add(new TrainingDomainEvent
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            EventKind = TrainingDomainEventKinds.AssignmentCreated,
            IdempotencyKey = $"seed-{Guid.NewGuid():N}",
            StaffarrPersonId = Guid.NewGuid(),
            RelatedEntityType = "training_assignment",
            RelatedEntityId = Guid.NewGuid(),
            PayloadJson = "{}",
            ProcessingStatus = TrainingDomainEventStatuses.Pending,
            AttemptCount = 0,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
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
            $"{sourceProduct}-integration-settings-{Guid.NewGuid():N}",
            $"{sourceProduct} integration settings test",
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
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private static HttpRequestMessage Authorized(
        HttpMethod method,
        string url,
        string accessToken,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
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

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }
    }
}
