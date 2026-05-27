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
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreVocabularySpineTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreVocab-{Guid.NewGuid():N}";

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _complianceCoreClient = _complianceCoreFactory.CreateClient();

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
    }

    [Fact]
    public async Task Vocabulary_types_returns_fourteen_controlled_keys()
    {
        var token = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/vocabulary/types", token));

        response.EnsureSuccessStatusCode();
        var types = (await response.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTypeResponse>>())!;
        Assert.Equal(14, types.Count);
        Assert.Contains(types, t => t.TypeKey == "material_hazard");
        Assert.Contains(types, t => t.TypeKey == "incident_reason");
        Assert.Contains(types, t => t.TypeKey == "evidence_type");
    }

    [Fact]
    public async Task Vocabulary_term_create_and_list_with_alias()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var createTermRequest = Authorized(HttpMethod.Post, "/api/vocabulary", adminToken);
        createTermRequest.Content = JsonContent.Create(new CreateVocabularyTermRequest(
            "flammable",
            "Flammable",
            "material_hazard",
            "Can ignite under defined conditions."));
        var createTermResponse = await _complianceCoreClient.SendAsync(createTermRequest);
        createTermResponse.EnsureSuccessStatusCode();
        var term = (await createTermResponse.Content.ReadFromJsonAsync<VocabularyTermResponse>())!;

        var createAliasRequest = Authorized(HttpMethod.Post, "/api/vocabulary/aliases", adminToken);
        createAliasRequest.Content = JsonContent.Create(new CreateVocabularyAliasRequest(
            term.TermId,
            "Fire hazard"));
        var createAliasResponse = await _complianceCoreClient.SendAsync(createAliasRequest);
        createAliasResponse.EnsureSuccessStatusCode();

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/vocabulary?vocabularyTypeKey=material_hazard", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var terms = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTermResponse>>())!;
        Assert.Single(terms);
        Assert.Equal("flammable", terms[0].TermKey);
        Assert.Contains("Fire hazard", terms[0].Aliases);
    }

    [Fact]
    public async Task Compliance_key_create_denies_member_role()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var request = Authorized(HttpMethod.Post, "/api/compliance-keys", memberToken);
        request.Content = JsonContent.Create(new CreateComplianceKeyRequest(
            "driver_qualification",
            "Driver Qualification",
            "compliance_domain",
            "Driver qualification requirement domain."));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Material_key_create_and_list_for_compliance_admin()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/material-keys", adminToken);
        createRequest.Content = JsonContent.Create(new CreateMaterialKeyRequest(
            "gas",
            "Gas",
            "physical_state",
            "Material exists as gas under defined conditions."));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/material-keys", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var keys = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<MaterialKeyResponse>>())!;
        Assert.Single(keys);
        Assert.Equal("gas", keys[0].Key);
    }

    [Fact]
    public async Task Vocabulary_read_requires_compliancecore_entitlement()
    {
        var token = CreateComplianceCoreAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/vocabulary/types", token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
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
