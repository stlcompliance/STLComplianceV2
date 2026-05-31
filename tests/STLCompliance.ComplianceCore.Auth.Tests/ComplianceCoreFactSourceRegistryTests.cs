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

public class ComplianceCoreFactSourceRegistryTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _staffarrResolveToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreFactSources-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrFactSources-{Guid.NewGuid():N}";

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
        _staffarrResolveToken = await IssueServiceTokenAsync(
            adminToken,
            sourceProduct: "staffarr",
            allowedProducts: ["compliancecore"],
            $"{FactResolveService.ResolveActionScope},{FactResolveService.ValidateActionScope}");
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Fact_source_create_list_and_internal_resolve_static_config()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "driver_license_valid");

        var createRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            "default_license_flag",
            "static_config",
            "Default license valid",
            "Static default for driver license validity checks.",
            null,
            null,
            """{"booleanValue":true}""",
            0));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<FactSourceResponse>())!;
        Assert.Equal("driver_license_valid", created.FactKey);
        Assert.Equal("static_config", created.SourceType);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/fact-sources", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<FactSourceResponse>>())!;
        Assert.Contains(listed, item => item.SourceKey == "default_license_flag");

        var resolveRequest = ServiceAuthorized(HttpMethod.Post, "/api/internal/resolve", _staffarrResolveToken);
        resolveRequest.Content = JsonContent.Create(new InternalResolveFactsRequest(
            PlatformSeeder.DemoTenantId,
            ["driver_license_valid"],
            null));
        var resolveResponse = await _complianceCoreClient.SendAsync(resolveRequest);
        resolveResponse.EnsureSuccessStatusCode();
        var resolved = (await resolveResponse.Content.ReadFromJsonAsync<InternalResolveFactsResponse>())!;
        Assert.Empty(resolved.UnresolvedFactKeys);
        Assert.Single(resolved.Resolved);
        Assert.Equal("driver_license_valid", resolved.Resolved[0].FactKey);
        Assert.True(resolved.Resolved[0].Value!.Value.GetBoolean());
    }

    [Fact]
    public async Task Fact_source_v1_aliases_and_update_route_work()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "v1_fact_source_enabled");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/fact-sources", adminToken);
        createRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            "v1_fact_source",
            "static_config",
            "V1 source",
            "Created via v1 route.",
            null,
            null,
            """{"booleanValue":true}""",
            5));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        Assert.StartsWith("/api/v1/fact-sources/", createResponse.Headers.Location?.ToString());
        var created = (await createResponse.Content.ReadFromJsonAsync<FactSourceResponse>())!;

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/fact-sources", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<FactSourceResponse>>())!;
        Assert.Contains(listed, item => item.FactSourceId == created.FactSourceId);

        var updateRequest = Authorized(HttpMethod.Patch, $"/api/v1/fact-sources/{created.FactSourceId}", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateFactSourceRequest(
            "V1 source updated",
            "Updated via v1 patch route.",
            null,
            null,
            """{"booleanValue":false}""",
            9,
            true));
        var updateResponse = await _complianceCoreClient.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<FactSourceResponse>())!;
        Assert.Equal("V1 source updated", updated.Label);
        Assert.Equal(9, updated.Priority);
        Assert.Contains("\"booleanValue\":false", updated.ConfigJson);
    }

    [Fact]
    public async Task Internal_resolve_uses_context_for_product_api_source()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "medical_cert_on_file");

        var createRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            "staffarr_med_cert",
            "product_api",
            "StaffArr medical certificate",
            "Resolved from StaffArr caller context until product fetch is implemented.",
            "staffarr",
            "/api/people/{personId}/certifications",
            """{"scopeKey":"tenant","fetchRelativePath":"/api/internal/compliance-facts/{factKey}"}""",
            0));
        (await _complianceCoreClient.SendAsync(createRequest)).EnsureSuccessStatusCode();

        var withoutContext = ServiceAuthorized(HttpMethod.Post, "/api/internal/resolve", _staffarrResolveToken);
        withoutContext.Content = JsonContent.Create(new InternalResolveFactsRequest(
            PlatformSeeder.DemoTenantId,
            ["medical_cert_on_file"],
            null));
        var missingResponse = await _complianceCoreClient.SendAsync(withoutContext);
        missingResponse.EnsureSuccessStatusCode();
        var missing = (await missingResponse.Content.ReadFromJsonAsync<InternalResolveFactsResponse>())!;
        Assert.Contains("medical_cert_on_file", missing.UnresolvedFactKeys);

        var withContext = ServiceAuthorized(HttpMethod.Post, "/api/internal/resolve", _staffarrResolveToken);
        withContext.Content = JsonContent.Create(new InternalResolveFactsRequest(
            PlatformSeeder.DemoTenantId,
            ["medical_cert_on_file"],
            new Dictionary<string, string> { ["medical_cert_on_file"] = "true" }));
        var contextResponse = await _complianceCoreClient.SendAsync(withContext);
        contextResponse.EnsureSuccessStatusCode();
        var resolved = (await contextResponse.Content.ReadFromJsonAsync<InternalResolveFactsResponse>())!;
        Assert.Empty(resolved.UnresolvedFactKeys);
        Assert.True(resolved.Resolved[0].FromContext);
    }

    [Fact]
    public async Task Internal_validate_reports_missing_catalog_and_sources()
    {
        var validateRequest = ServiceAuthorized(HttpMethod.Post, "/api/internal/validate", _staffarrResolveToken);
        validateRequest.Content = JsonContent.Create(new InternalValidateFactsRequest(
            PlatformSeeder.DemoTenantId,
            ["unknown_fact_key"]));
        var unknownResponse = await _complianceCoreClient.SendAsync(validateRequest);
        unknownResponse.EnsureSuccessStatusCode();
        var unknown = (await unknownResponse.Content.ReadFromJsonAsync<InternalValidateFactsResponse>())!;
        Assert.False(unknown.IsValid);
        Assert.Contains(unknown.Results, item => item.FactKey == "unknown_fact_key" && !item.CanResolve);

        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await CreateBooleanFactDefinitionAsync(adminToken, "training_current");

        validateRequest = ServiceAuthorized(HttpMethod.Post, "/api/internal/validate", _staffarrResolveToken);
        validateRequest.Content = JsonContent.Create(new InternalValidateFactsRequest(
            PlatformSeeder.DemoTenantId,
            ["training_current"]));
        var noSourceResponse = await _complianceCoreClient.SendAsync(validateRequest);
        noSourceResponse.EnsureSuccessStatusCode();
        var noSource = (await noSourceResponse.Content.ReadFromJsonAsync<InternalValidateFactsResponse>())!;
        Assert.False(noSource.IsValid);
        Assert.Contains(noSource.Results, item => item.FactKey == "training_current" && !item.CanResolve);
    }

    [Fact]
    public async Task Internal_resolve_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/resolve");
        request.Content = JsonContent.Create(new InternalResolveFactsRequest(
            PlatformSeeder.DemoTenantId,
            ["driver_license_valid"],
            null));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Fact_source_create_denies_member_role()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "hazmat_endorsement");

        var createRequest = Authorized(HttpMethod.Post, "/api/fact-sources", memberToken);
        createRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            "hazmat_default",
            "static_config",
            "Hazmat default",
            "Sample static hazmat endorsement flag.",
            null,
            null,
            """{"booleanValue":false}""",
            0));
        var response = await _complianceCoreClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
            $"{sourceProduct}-fact-resolve-{Guid.NewGuid():N}",
            $"{sourceProduct} fact resolve test",
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
