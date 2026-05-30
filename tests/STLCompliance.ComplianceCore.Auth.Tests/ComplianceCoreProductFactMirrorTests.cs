using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreProductFactMirrorTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _supplyarrIngestToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreProductFacts-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrProductFacts-{Guid.NewGuid():N}";

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
        _supplyarrIngestToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["compliancecore"],
            $"{ProductFactIngestionService.IngestFactsActionScope},{FactResolveService.ResolveActionScope}");
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Product_fact_ingest_and_resolve_via_product_mirror_source()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var factDefinitionId = await CreateStringFactDefinitionAsync(adminToken, SupplyArrFactKey);

        var createSourceRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createSourceRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            "supplyarr_pr_status_mirror",
            FactSourceTypes.ProductMirror,
            "SupplyArr PR status mirror",
            "Rebuildable mirror ingested from SupplyArr procurement events.",
            "supplyarr",
            "purchase_request.status",
            "{}",
            0));
        var createSourceResponse = await _complianceCoreClient.SendAsync(createSourceRequest);
        createSourceResponse.EnsureSuccessStatusCode();

        var purchaseRequestId = Guid.NewGuid();
        var publicationId = Guid.NewGuid();
        var scopeKey = $"purchase_request:{purchaseRequestId:D}".ToLowerInvariant();

        var ingestResult = await IngestFactAsync(
            "/api/integrations/product-facts/ingest",
            publicationId,
            purchaseRequestId,
            scopeKey,
            "submitted",
            "purchase_request.submitted");
        Assert.Equal(1, ingestResult.AcceptedCount);

        var v1IngestResult = await IngestFactAsync(
            "/api/v1/integrations/product-facts/ingest",
            Guid.NewGuid(),
            purchaseRequestId,
            scopeKey,
            "approved",
            "purchase_request.approved");
        Assert.Equal(1, v1IngestResult.AcceptedCount);

        var resolveRequest = ServiceAuthorized(HttpMethod.Post, "/api/internal/resolve", _supplyarrIngestToken);
        resolveRequest.Content = JsonContent.Create(new InternalResolveFactsRequest(
            PlatformSeeder.DemoTenantId,
            [SupplyArrFactKey],
            new Dictionary<string, string>
            {
                ["purchase_request_id"] = purchaseRequestId.ToString(),
            }));
        var resolveResponse = await _complianceCoreClient.SendAsync(resolveRequest);
        resolveResponse.EnsureSuccessStatusCode();
        var resolveResult = (await resolveResponse.Content.ReadFromJsonAsync<InternalResolveFactsResponse>())!;
        Assert.Empty(resolveResult.UnresolvedFactKeys);
        Assert.Single(resolveResult.Resolved);
        Assert.Equal("approved", resolveResult.Resolved[0].Value!.Value.GetString());
    }

    private const string SupplyArrFactKey = "supplyarr.purchase_request.status";

    private async Task<IngestProductFactsResponse> IngestFactAsync(
        string endpoint,
        Guid publicationId,
        Guid purchaseRequestId,
        string scopeKey,
        string status,
        string eventName)
    {
        var ingestRequest = ServiceAuthorized(HttpMethod.Post, endpoint, _supplyarrIngestToken);
        ingestRequest.Content = JsonContent.Create(new IngestProductFactsRequest(
            PlatformSeeder.DemoTenantId,
            publicationId,
            "supplyarr",
            DateTimeOffset.UtcNow,
            [
                new ProductFactPublicationItemRequest(
                    SupplyArrFactKey,
                    "string",
                    scopeKey,
                    status,
                    null,
                    null,
                    null,
                    "purchase_request",
                    purchaseRequestId,
                    eventName,
                    $"supplyarr:{publicationId:D}:{SupplyArrFactKey}:{scopeKey}")
            ]));
        var ingestResponse = await _complianceCoreClient.SendAsync(ingestRequest);
        ingestResponse.EnsureSuccessStatusCode();
        return (await ingestResponse.Content.ReadFromJsonAsync<IngestProductFactsResponse>())!;
    }

    private async Task<Guid> CreateStringFactDefinitionAsync(string adminToken, string factKey)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        createRequest.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            $"Label for {factKey}",
            "Test fact definition.",
            "string"));
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
            $"{sourceProduct}-facts-{Guid.NewGuid():N}",
            $"{sourceProduct} fact publishing test",
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
