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
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using CreateTypedExternalPartyRequest = SupplyArr.Api.Contracts.CreateTypedExternalPartyRequest;
using ExternalPartyResponse = SupplyArr.Api.Contracts.ExternalPartyResponse;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrComplianceCoreFactPublishingTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _workerToken = null!;
    private string _supplyarrComplianceCoreToken = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        var nexArrDbName = $"SupplyFactsNexArr-{Guid.NewGuid():N}";
        var complianceDbName = $"SupplyFactsCompliance-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplyFactsSupplyArr-{Guid.NewGuid():N}";

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
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["supplyarr"],
            IntegrationEventProcessingService.ProcessEventsActionScope);
        _supplyarrComplianceCoreToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["compliancecore"],
            $"{ProductFactIngestionService.IngestFactsActionScope},{FactResolveService.ResolveActionScope}");
        var supplyarrHandoffToken = await IssueServiceTokenAsync(adminToken, "supplyarr", ["supplyarr"], "launch.redeem");

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
        using (var scope = _complianceCoreFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
            await db.Database.EnsureCreatedAsync();
            var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
            await vocabularyService.EnsureVocabularyTypesSeededAsync();
        }

        await SeedComplianceCoreFactCatalogAsync();

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", supplyarrHandoffToken);
            builder.UseSetting("ComplianceCore:BaseUrl", _complianceCoreFactory.Server.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("ComplianceCore:ServiceToken", _supplyarrComplianceCoreToken);
            builder.UseSetting("StaffArr:EnforceProcurementApprovalAuthority", "false");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<ComplianceCoreFactPublicationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();

        var handoffCode = await CreateHandoffAsync(adminToken);
        _userToken = await RedeemHandoffAsync(handoffCode);
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Purchase_request_submit_publishes_fact_to_compliance_core_mirror()
    {
        var vendor = await CreateVendorAsync();
        var part = await CreatePartAsync();

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"fc-pr-{Guid.NewGuid():N}"[..20],
            "Fact publish PR",
            string.Empty,
            vendor.PartyId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var prResponse = await _supplyarrClient.SendAsync(createPrRequest);
        prResponse.EnsureSuccessStatusCode();
        var pr = (await prResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/purchase-requests/{pr.PurchaseRequestId}/submit", _userToken));
        submitResponse.EnsureSuccessStatusCode();

        var processRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/internal/integration-events/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessIntegrationEventsRequest(
            PlatformSeeder.DemoTenantId,
            50));
        var processResponse = await _supplyarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        using (var scope = _complianceCoreFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
            var mirror = await db.ProductFactMirrors.SingleOrDefaultAsync(
                x => x.TenantId == PlatformSeeder.DemoTenantId
                    && x.FactKey == SupplyArrComplianceCoreFactKeys.PurchaseRequestStatus
                    && x.ScopeKey == $"purchase_request:{pr.PurchaseRequestId:D}".ToLowerInvariant());
            Assert.NotNull(mirror);
            Assert.Equal("submitted", mirror!.StringValue);
        }

        var resolveRequest = ServiceAuthorized(HttpMethod.Post, "/api/internal/resolve", _supplyarrComplianceCoreToken);
        resolveRequest.Content = JsonContent.Create(new InternalResolveFactsRequest(
            PlatformSeeder.DemoTenantId,
            [SupplyArrComplianceCoreFactKeys.PurchaseRequestStatus],
            new Dictionary<string, string>
            {
                ["purchase_request_id"] = pr.PurchaseRequestId.ToString(),
            }));
        var resolveResponse = await _complianceCoreClient.SendAsync(resolveRequest);
        resolveResponse.EnsureSuccessStatusCode();
        var resolveResult = (await resolveResponse.Content.ReadFromJsonAsync<InternalResolveFactsResponse>())!;
        Assert.Empty(resolveResult.UnresolvedFactKeys);
        Assert.Equal("submitted", resolveResult.Resolved[0].Value!.Value.GetString());
    }

    private async Task SeedComplianceCoreFactCatalogAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
        var (adminToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            "compliance_admin",
            ["compliancecore"],
            isPlatformAdmin: false);

        var createDefinition = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        createDefinition.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            SupplyArrComplianceCoreFactKeys.PurchaseRequestStatus,
            "SupplyArr purchase request status",
            "Published from SupplyArr procurement workflow.",
            "string"));
        var definitionResponse = await _complianceCoreClient.SendAsync(createDefinition);
        definitionResponse.EnsureSuccessStatusCode();
        var definition = (await definitionResponse.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;

        var createSource = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createSource.Content = JsonContent.Create(new CreateFactSourceRequest(
            definition.FactDefinitionId,
            "supplyarr_pr_status",
            FactSourceTypes.ProductMirror,
            "SupplyArr PR status",
            "Mirror ingested from SupplyArr outbox processing.",
            "supplyarr",
            "purchase_request.status",
            "{}",
            0));
        var sourceResponse = await _complianceCoreClient.SendAsync(createSource);
        sourceResponse.EnsureSuccessStatusCode();
    }

    private async Task<ExternalPartyResponse> CreateVendorAsync()
    {
        var request = Authorized(HttpMethod.Post, "/api/vendors", _userToken);
        request.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"fc-v-{Guid.NewGuid():N}"[..12],
            "Fact Vendor",
            string.Empty,
            null,
            string.Empty));
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
    }

    private async Task<PartResponse> CreatePartAsync()
    {
        var request = Authorized(HttpMethod.Post, "/api/parts", _userToken);
        request.Content = JsonContent.Create(new CreatePartRequest(
            $"fc-part-{Guid.NewGuid():N}"[..20],
            null,
            "Fact Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PartResponse>())!;
    }

    private async Task<string> CreateHandoffAsync(string token)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("supplyarr", "http://localhost:5179/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> RedeemHandoffAsync(string handoffCode)
    {
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemHandoffRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!;
        return session.AccessToken;
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-facts-{Guid.NewGuid():N}",
            $"{sourceProduct} fact test",
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
