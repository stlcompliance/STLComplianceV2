using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreRuleChangeMonitoringTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _workerMonitorToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreRuleChanges-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrRuleChanges-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexarrDbName));
            });
        });

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(complianceDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        _complianceCoreClient = _complianceCoreFactory.CreateClient();

        await SeedNexArrAsync();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();

        var adminToken = await LoginNexArrAdminAsync();
        _workerMonitorToken = await IssueServiceTokenAsync(
            adminToken,
            RuleChangeMonitoringService.MonitorActionScope);
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Rule_pack_create_logs_version_created_event()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await EnsureRegulatoryProgramAsync(adminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "monitor_test_pack",
            "Monitor test pack",
            "Rule change monitoring test pack."));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var eventsResponse = await _complianceCoreClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/rule-changes/events?packKey=monitor_test_pack&changeType=version_created",
                adminToken));
        eventsResponse.EnsureSuccessStatusCode();
        var events = (await eventsResponse.Content.ReadFromJsonAsync<List<RuleChangeEventResponse>>())!;
        Assert.Single(events);
        Assert.Equal(RuleChangeTypes.VersionCreated, events[0].ChangeType);
        Assert.Equal("api", events[0].Source);
    }

    [Fact]
    public async Task Rule_pack_status_update_logs_status_changed_event()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await EnsureRegulatoryProgramAsync(adminToken);
        var packId = await CreateDraftRulePackAsync(adminToken, programId, "status_monitor_pack");

        var reviewRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{packId}/status", adminToken);
        reviewRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Review));
        (await _complianceCoreClient.SendAsync(reviewRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{packId}/status", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Published));
        (await _complianceCoreClient.SendAsync(publishRequest)).EnsureSuccessStatusCode();

        var eventsResponse = await _complianceCoreClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/rule-changes/events?packKey=status_monitor_pack&changeType=status_changed",
                adminToken));
        eventsResponse.EnsureSuccessStatusCode();
        var events = (await eventsResponse.Content.ReadFromJsonAsync<List<RuleChangeEventResponse>>())!;
        Assert.Contains(events, e => e.ToStatus == RulePackStatuses.Published);
    }

    [Fact]
    public async Task Rule_change_summary_returns_counts()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await EnsureRegulatoryProgramAsync(adminToken);
        await CreateDraftRulePackAsync(adminToken, programId, "summary_monitor_pack");

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/rule-changes/summary", adminToken));
        response.EnsureSuccessStatusCode();
        var summary = (await response.Content.ReadFromJsonAsync<RuleChangeMonitoringSummaryResponse>())!;
        Assert.True(summary.TotalEvents >= 1);
        Assert.True(summary.VersionCreatedCount >= 1);
    }

    [Fact]
    public async Task Internal_process_scan_detects_snapshot_drift()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await EnsureRegulatoryProgramAsync(adminToken);
        var packId = await CreateDraftRulePackAsync(adminToken, programId, "scan_monitor_pack");

        using (var scope = _complianceCoreFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
            var snapshot = await db.RulePackMonitorSnapshots.FirstAsync(x => x.RulePackId == packId);
            snapshot.ContentHash = "stale_hash_value";
            snapshot.CapturedAt = DateTimeOffset.UtcNow.AddHours(-1);
            await db.SaveChangesAsync();
        }

        var scanRequest = ServiceAuthorized(HttpMethod.Post, "/api/internal/rule-changes/process-scan", _workerMonitorToken);
        scanRequest.Content = JsonContent.Create(new ProcessRuleChangeScanRequest(
            PlatformSeeder.DemoTenantId,
            null,
            50));
        var scanResponse = await _complianceCoreClient.SendAsync(scanRequest);
        scanResponse.EnsureSuccessStatusCode();
        var result = (await scanResponse.Content.ReadFromJsonAsync<ProcessRuleChangeScanResponse>())!;
        Assert.True(result.ChangesDetectedCount >= 1);
        Assert.Contains(result.DetectedEvents, e => e.ChangeType == RuleChangeTypes.ScanDetected);
    }

    [Fact]
    public async Task Internal_process_scan_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/rule-changes/process-scan");
        request.Content = JsonContent.Create(new ProcessRuleChangeScanRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<Guid> EnsureRegulatoryProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "monitor_body",
            "Monitor Body",
            "Test governing body."));
        var bodyResponse = await _complianceCoreClient.SendAsync(bodyRequest);
        bodyResponse.EnsureSuccessStatusCode();
        var body = (await bodyResponse.Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "monitor_jurisdiction",
            "Monitor Jurisdiction",
            "Test jurisdiction."));
        var jurisdictionResponse = await _complianceCoreClient.SendAsync(jurisdictionRequest);
        jurisdictionResponse.EnsureSuccessStatusCode();
        var jurisdiction = (await jurisdictionResponse.Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "monitor_program",
            "Monitor Program",
            "Test program."));
        var programResponse = await _complianceCoreClient.SendAsync(programRequest);
        programResponse.EnsureSuccessStatusCode();
        var program = (await programResponse.Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
    }

    private async Task<Guid> CreateDraftRulePackAsync(string adminToken, Guid programId, string packKey)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            packKey,
            $"Label {packKey}",
            "Monitoring test pack."));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;
        return created.RulePackId;
    }

    private string CreateComplianceCoreAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
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

    private async Task<string> LoginNexArrAdminAsync()
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string actionScope)
    {
        var registerRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"shared-worker-rule-changes-{Guid.NewGuid():N}",
            "Rule change monitor test",
            "shared-worker",
            ["compliancecore"]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            ["compliancecore"],
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private HttpRequestMessage NexArrAuthorized(HttpMethod method, string url, string accessToken) =>
        Authorized(method, url, accessToken);

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
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
}
