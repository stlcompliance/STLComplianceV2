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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreAuditDeliveryOrchestrationTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreOrchestration-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrOrchestration-{Guid.NewGuid():N}";

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

        _complianceCoreClient = _complianceCoreFactory.CreateClient();

        await SeedNexArrAsync();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Status_read_allowed_for_compliance_reviewer()
    {
        var reviewerToken = CreateComplianceCoreAccessToken(
            ["compliancecore"],
            tenantRoleKey: "compliance_reviewer");

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-delivery-orchestration", reviewerToken));
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<AuditDeliveryOrchestrationStatusResponse>())!;
        Assert.NotNull(body.WorkerSettings);
        Assert.NotNull(body.ScheduledEvaluation);
        Assert.NotNull(body.M12Batch);
        Assert.NotNull(body.AuditPackages);
    }

    [Fact]
    public async Task Trigger_endpoints_reject_compliance_reviewer()
    {
        var reviewerToken = CreateComplianceCoreAccessToken(
            ["compliancecore"],
            tenantRoleKey: "compliance_reviewer");

        var scheduledResponse = await _complianceCoreClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                "/api/audit-delivery-orchestration/trigger-scheduled-evaluation",
                reviewerToken));
        Assert.Equal(HttpStatusCode.Forbidden, scheduledResponse.StatusCode);

        var m12Response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Post, "/api/audit-delivery-orchestration/trigger-m12-batch", reviewerToken));
        Assert.Equal(HttpStatusCode.Forbidden, m12Response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_trigger_scheduled_evaluation_and_m12_batch()
    {
        var adminToken = CreateComplianceCoreAccessToken(
            ["compliancecore"],
            tenantRoleKey: "compliance_admin");
        await SeedPublishedScheduledRulePackAsync(adminToken);

        var putRequest = Authorized(HttpMethod.Put, "/api/m12-analytics-worker-settings", adminToken);
        putRequest.Content = JsonContent.Create(new UpsertM12AnalyticsWorkerSettingsRequest(
            true,
            "tenant",
            24,
            true,
            true,
            true,
            true,
            false));
        (await _complianceCoreClient.SendAsync(putRequest)).EnsureSuccessStatusCode();

        var statusBefore = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-delivery-orchestration", adminToken));
        statusBefore.EnsureSuccessStatusCode();

        var scheduledRequest = Authorized(
            HttpMethod.Post,
            "/api/audit-delivery-orchestration/trigger-scheduled-evaluation",
            adminToken);
        var scheduledResponse = await _complianceCoreClient.SendAsync(scheduledRequest);
        scheduledResponse.EnsureSuccessStatusCode();
        var scheduledBody =
            (await scheduledResponse.Content.ReadFromJsonAsync<TriggerScheduledRuleEvaluationResponse>())!;
        Assert.True(scheduledBody.EvaluatedCount >= 0);

        var m12Request = Authorized(
            HttpMethod.Post,
            "/api/audit-delivery-orchestration/trigger-m12-batch",
            adminToken);
        var m12Response = await _complianceCoreClient.SendAsync(m12Request);
        m12Response.EnsureSuccessStatusCode();
        var m12Body = (await m12Response.Content.ReadFromJsonAsync<TriggerM12AnalyticsBatchResponse>())!;
        Assert.Equal(M12AnalyticsBatchRunStatuses.Completed, m12Body.Status);
        Assert.NotNull(m12Body.BatchRunId);

        var statusAfter = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-delivery-orchestration", adminToken));
        statusAfter.EnsureSuccessStatusCode();
        var statusBody =
            (await statusAfter.Content.ReadFromJsonAsync<AuditDeliveryOrchestrationStatusResponse>())!;
        Assert.NotNull(statusBody.ScheduledEvaluation.LastRun);
        Assert.NotNull(statusBody.M12Batch.LastRun);
    }

    [Fact]
    public async Task Trigger_m12_batch_rejects_when_worker_disabled()
    {
        var adminToken = CreateComplianceCoreAccessToken(
            ["compliancecore"],
            tenantRoleKey: "compliance_admin");

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Post, "/api/audit-delivery-orchestration/trigger-m12-batch", adminToken));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task SeedPublishedScheduledRulePackAsync(string adminToken)
    {
        var programId = await CreateSampleProgramAsync(adminToken);
        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "orch_scheduled_pack",
            "Orchestration Scheduled Pack",
            "Audit delivery orchestration test pack."));
        var createPackResponse = await _complianceCoreClient.SendAsync(createPackRequest);
        createPackResponse.EnsureSuccessStatusCode();
        var pack = (await createPackResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;

        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "orch_license_valid");
        var createSourceRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createSourceRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            "orch_license_flag",
            FactSourceTypes.StaticConfig,
            "Orchestration license flag",
            "Static default for orchestration tests.",
            null,
            null,
            """{"booleanValue":true}""",
            0));
        (await _complianceCoreClient.SendAsync(createSourceRequest)).EnsureSuccessStatusCode();

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid license",
                    "fact_boolean",
                    "orch_license_valid",
                    true),
            ]);

        var updateContentRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", adminToken);
        updateContentRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateContentRequest)).EnsureSuccessStatusCode();

        var reviewRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{pack.RulePackId}/status", adminToken);
        reviewRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Review));
        (await _complianceCoreClient.SendAsync(reviewRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{pack.RulePackId}/status", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Published));
        (await _complianceCoreClient.SendAsync(publishRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "orch_dot",
            "Orchestration DOT",
            "Test governing body."));
        var body = (await (await _complianceCoreClient.SendAsync(bodyRequest)).Content
            .ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "orch_us",
            "Orchestration US",
            "Test jurisdiction."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content
            .ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "orch_program",
            "Orchestration Program",
            "Test program."));
        var program = (await (await _complianceCoreClient.SendAsync(programRequest)).Content
            .ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
    }

    private async Task<Guid> CreateBooleanFactDefinitionAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            factKey.Replace('_', ' '),
            "Orchestration test fact.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!.FactDefinitionId;
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string accessToken)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private string CreateComplianceCoreAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "compliance_admin")
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
}
