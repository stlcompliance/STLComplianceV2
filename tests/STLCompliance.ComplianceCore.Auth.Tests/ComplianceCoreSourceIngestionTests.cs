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

public sealed class ComplianceCoreSourceIngestionTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _supplyarrSourcesToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreSourceIngestion-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrSourceIngestion-{Guid.NewGuid():N}";

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
        _supplyarrSourcesToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["compliancecore"],
            SourceIngestionService.IngestSourcesActionScope);
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Fact_source_ingestion_validate_persists_batch_without_creating_sources()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "ingestion_validate_flag");

        var beforeCount = await CountFactSourcesAsync();

        var request = Authorized(HttpMethod.Post, "/api/source-ingestion/fact-sources/validate", adminToken);
        request.Content = JsonContent.Create(new FactSourceBulkIngestionRequest(
        [
            new FactSourceIngestionRowRequest(
                factDefinitionId,
                "ingestion_validate_src",
                FactSourceTypes.StaticConfig,
                "Ingestion validate source",
                "Validated only.",
                null,
                null,
                """{"booleanValue":true}""",
                0),
        ]));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<SourceIngestionBatchResponse>())!;

        Assert.True(result.DryRun);
        Assert.Equal(SourceIngestionTypes.FactSources, result.IngestionType);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(await CountFactSourcesAsync(), beforeCount);
    }

    [Fact]
    public async Task Fact_source_ingestion_commit_creates_sources_and_lists_batch()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "ingestion_commit_flag");

        var commitRequest = Authorized(HttpMethod.Post, "/api/source-ingestion/fact-sources/commit", adminToken);
        commitRequest.Content = JsonContent.Create(new FactSourceBulkIngestionRequest(
        [
            new FactSourceIngestionRowRequest(
                factDefinitionId,
                "ingestion_commit_src",
                FactSourceTypes.StaticConfig,
                "Ingestion commit source",
                "Committed via batch.",
                null,
                null,
                """{"booleanValue":false}""",
                0),
        ]));
        var commitResponse = await _complianceCoreClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commitResult = (await commitResponse.Content.ReadFromJsonAsync<SourceIngestionBatchResponse>())!;
        Assert.False(commitResult.DryRun);
        Assert.Equal("created", commitResult.Jobs[0].Status);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/source-ingestion/batches?ingestionType=fact_sources", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<SourceIngestionBatchSummary>>())!;
        Assert.Contains(listed, batch => batch.BatchId == commitResult.BatchId);

        var detailResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/source-ingestion/batches/{commitResult.BatchId}", adminToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<SourceIngestionBatchDetailResponse>())!;
        Assert.Single(detail.Jobs);
    }

    [Fact]
    public async Task Fact_source_ingestion_denies_member_role()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "ingestion_denied_flag");

        var request = Authorized(HttpMethod.Post, "/api/source-ingestion/fact-sources/validate", memberToken);
        request.Content = JsonContent.Create(new FactSourceBulkIngestionRequest(
        [
            new FactSourceIngestionRowRequest(
                factDefinitionId,
                "ingestion_denied_src",
                FactSourceTypes.StaticConfig,
                "Denied",
                "Member cannot ingest.",
                null,
                null,
                """{"booleanValue":true}""",
                0),
        ]));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Product_fact_source_ingestion_validate_and_commit_via_service_token()
    {
        var publicationId = Guid.NewGuid();
        var scopeKey = $"purchase_request:{Guid.NewGuid():D}".ToLowerInvariant();
        const string factKey = "supplyarr.purchase_request.status";

        var validateRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/source-ingestion/product-facts/validate",
            _supplyarrSourcesToken);
        validateRequest.Content = JsonContent.Create(new ProductFactBulkIngestionRequest(
            PlatformSeeder.DemoTenantId,
            publicationId,
            "supplyarr",
            DateTimeOffset.UtcNow,
            [
                new ProductFactPublicationItemRequest(
                    factKey,
                    "string",
                    scopeKey,
                    "submitted",
                    null,
                    null,
                    null,
                    "purchase_request",
                    Guid.NewGuid(),
                    "purchase_request.submitted",
                    $"supplyarr:{publicationId:D}:{factKey}:{scopeKey}"),
            ]));
        var validateResponse = await _complianceCoreClient.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var validateResult = (await validateResponse.Content.ReadFromJsonAsync<SourceIngestionBatchResponse>())!;
        Assert.True(validateResult.DryRun);
        Assert.Equal(1, validateResult.SuccessCount);

        var commitRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/source-ingestion/product-facts/commit",
            _supplyarrSourcesToken);
        commitRequest.Content = validateRequest.Content;
        var commitResponse = await _complianceCoreClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commitResult = (await commitResponse.Content.ReadFromJsonAsync<SourceIngestionBatchResponse>())!;
        Assert.False(commitResult.DryRun);
        Assert.Equal(1, commitResult.SuccessCount);

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        Assert.True(await db.ProductFactMirrors.AnyAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.FactKey == factKey && x.ScopeKey == scopeKey));
    }

    private async Task<int> CountFactSourcesAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        return await db.FactSources.CountAsync();
    }

    private async Task<Guid> CreateBooleanFactDefinitionAsync(string adminToken, string factKey)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        createRequest.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            $"Label for {factKey}",
            "Test fact definition.",
            "boolean"));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        return created.FactDefinitionId;
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
            $"{sourceProduct}-source-ingestion-{Guid.NewGuid():N}",
            $"{sourceProduct} source ingestion test",
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

    private HttpRequestMessage NexArrAuthorized(HttpMethod method, string url, string accessToken) =>
        Authorized(method, url, accessToken);

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string accessToken)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string path, string serviceToken)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(DbContextOptions<TContext>)
                || descriptor.ServiceType == typeof(TContext))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
