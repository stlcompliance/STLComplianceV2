using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreInternalRuleEvaluationTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _trainarrEvaluateToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreInternalEval-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrInternalEval-{Guid.NewGuid():N}";

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
        _trainarrEvaluateToken = await IssueServiceTokenAsync(
            adminToken,
            sourceProduct: "trainarr",
            allowedProducts: ["compliancecore"],
            InternalRuleEvaluationService.EvaluateActionScope);

        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedDriverQualificationRulePackAsync(complianceAdminToken);
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Internal_evaluate_resolves_facts_and_returns_allow()
    {
        var request = ServiceAuthorized(HttpMethod.Post, "/api/internal/evaluate", _trainarrEvaluateToken);
        request.Content = JsonContent.Create(new InternalEvaluateRulePackRequest(
            PlatformSeeder.DemoTenantId,
            "driver_qualification",
            null));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<InternalEvaluateRulePackResponse>())!;
        Assert.Equal(ComplianceEvaluationOutcomes.Allow, result.Outcome);
        Assert.Equal("rule_evaluation_passed", result.ReasonCode);
        Assert.Equal("pass", result.EvaluationResult);
        Assert.Empty(result.UnresolvedFactKeys);
        Assert.True(result.ResolvedFacts["driver_license_valid"]);
    }

    [Fact]
    public async Task Internal_evaluate_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/evaluate");
        request.Content = JsonContent.Create(new InternalEvaluateRulePackRequest(
            PlatformSeeder.DemoTenantId,
            "driver_qualification",
            null));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Internal_evaluate_batch_returns_per_item_results_and_summary()
    {
        var request = ServiceAuthorized(HttpMethod.Post, "/api/internal/evaluate/batch", _trainarrEvaluateToken);
        request.Content = JsonContent.Create(new InternalEvaluateRulePackBatchRequest(
            PlatformSeeder.DemoTenantId,
            [
                new InternalEvaluateRulePackBatchItem("driver_qualification"),
                new InternalEvaluateRulePackBatchItem("driver_qualification"),
            ]));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var batch = (await response.Content.ReadFromJsonAsync<InternalEvaluateRulePackBatchResponse>())!;
        Assert.NotEqual(Guid.Empty, batch.BatchId);
        Assert.Equal(2, batch.Results.Count);
        Assert.Equal(2, batch.Summary.Total);
        Assert.Equal(2, batch.Summary.AllowCount);
        Assert.Equal(0, batch.Summary.WarnCount);
        Assert.Equal(0, batch.Summary.BlockCount);
        Assert.All(batch.Results, result => Assert.Equal(ComplianceEvaluationOutcomes.Allow, result.Outcome));
    }

    [Fact]
    public async Task Internal_evaluate_batch_rejects_empty_items()
    {
        var request = ServiceAuthorized(HttpMethod.Post, "/api/internal/evaluate/batch", _trainarrEvaluateToken);
        request.Content = JsonContent.Create(new InternalEvaluateRulePackBatchRequest(
            PlatformSeeder.DemoTenantId,
            []));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Internal_evaluate_batch_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/evaluate/batch");
        request.Content = JsonContent.Create(new InternalEvaluateRulePackBatchRequest(
            PlatformSeeder.DemoTenantId,
            [new InternalEvaluateRulePackBatchItem("driver_qualification")]));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task SeedDriverQualificationRulePackAsync(string adminToken)
    {
        var programId = await CreateSampleProgramAsync(adminToken);
        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification Rules",
            "TrainArr authorization check rule pack."));
        var createPackResponse = await _complianceCoreClient.SendAsync(createPackRequest);
        createPackResponse.EnsureSuccessStatusCode();
        var pack = (await createPackResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;

        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "driver_license_valid");
        var createSourceRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createSourceRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            "default_license_flag",
            "static_config",
            "Default license valid",
            "Static default for driver license validity checks.",
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
                    "Valid driver license",
                    "fact_boolean",
                    "driver_license_valid",
                    true),
            ]);

        var updateContentRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", adminToken);
        updateContentRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateContentRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "Department of Transportation",
            "US DOT"));
        var bodyResponse = await _complianceCoreClient.SendAsync(bodyRequest);
        bodyResponse.EnsureSuccessStatusCode();
        var body = (await bodyResponse.Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "us_federal",
            "US Federal",
            "United States federal jurisdiction."));
        var jurisdictionResponse = await _complianceCoreClient.SendAsync(jurisdictionRequest);
        jurisdictionResponse.EnsureSuccessStatusCode();
        var jurisdiction = (await jurisdictionResponse.Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "driver_compliance",
            "Driver Compliance",
            "Driver qualification program."));
        var programResponse = await _complianceCoreClient.SendAsync(programRequest);
        programResponse.EnsureSuccessStatusCode();
        var program = (await programResponse.Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
    }

    private async Task<Guid> CreateBooleanFactDefinitionAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            factKey.Replace('_', ' '),
            "Test fact for internal evaluation.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        return created.FactDefinitionId;
    }

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        return request;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-internal-eval-{Guid.NewGuid():N}",
            $"{sourceProduct} internal evaluation test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens", adminToken);
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private static HttpRequestMessage NexArrAuthorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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
