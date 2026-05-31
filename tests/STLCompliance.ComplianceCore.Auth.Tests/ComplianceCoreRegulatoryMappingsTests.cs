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
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreRegulatoryMappingsTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreRegulatoryMappings-{Guid.NewGuid():N}";

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
    public async Task Regulatory_mapping_create_list_and_filter()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (programId, rulePackId) = await CreateSampleRulePackAsync(adminToken);
        var complianceKeyId = await CreateComplianceKeyAsync(adminToken, "vehicle_inspection");

        var createRequest = Authorized(HttpMethod.Post, "/api/regulatory-mappings", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRegulatoryMappingRequest(
            "dq_vehicle_inspection",
            "Vehicle inspection under driver qualification",
            "Maps vehicle inspection compliance key to FMCSA driver qualification.",
            "compliance_key",
            programId,
            rulePackId,
            null,
            null,
            complianceKeyId,
            null));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<RegulatoryMappingResponse>())!;
        Assert.Equal("compliance_key", created.TargetKind);
        Assert.Equal("vehicle_inspection", created.ComplianceKey);
        Assert.Equal("fmcsa_safety", created.RegulatoryProgramKey);
        Assert.Equal("driver_qualification", created.RulePackKey);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/regulatory-mappings?rulePackId={rulePackId}", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var mappings = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<RegulatoryMappingResponse>>())!;
        Assert.Single(mappings);
    }

    [Fact]
    public async Task Regulatory_mapping_material_key_target()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (programId, _) = await CreateSampleRulePackAsync(adminToken);
        var materialKeyId = await CreateMaterialKeyAsync(adminToken, "flammable");

        var createRequest = Authorized(HttpMethod.Post, "/api/regulatory-mappings", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRegulatoryMappingRequest(
            "hazmat_flammable",
            "Flammable material under FMCSA safety",
            "Maps flammable material key to federal motor carrier safety program.",
            "material_key",
            programId,
            null,
            null,
            null,
            null,
            materialKeyId));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<RegulatoryMappingResponse>())!;
        Assert.Equal("material_key", created.TargetKind);
        Assert.Equal("flammable", created.MaterialKey);
    }

    [Fact]
    public async Task Regulatory_mapping_create_denies_member_role()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (programId, rulePackId) = await CreateSampleRulePackAsync(adminToken);
        var complianceKeyId = await CreateComplianceKeyAsync(adminToken, "driver_qualification");

        var request = Authorized(HttpMethod.Post, "/api/regulatory-mappings", memberToken);
        request.Content = JsonContent.Create(new CreateRegulatoryMappingRequest(
            "dq_mapping",
            "Driver qualification mapping",
            "Sample mapping.",
            "compliance_key",
            programId,
            rulePackId,
            null,
            null,
            complianceKeyId,
            null));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Regulatory_mapping_requires_target_key()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (programId, _) = await CreateSampleRulePackAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/regulatory-mappings", adminToken);
        request.Content = JsonContent.Create(new CreateRegulatoryMappingRequest(
            "orphan_mapping",
            "Orphan mapping",
            "Mapping without a key target.",
            "compliance_key",
            programId,
            null,
            null,
            null,
            null,
            null));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Regulatory_mapping_read_requires_compliancecore_entitlement()
    {
        var token = CreateComplianceCoreAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/regulatory-mappings", token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task V1_regulatory_mapping_and_derived_facts_aliases_work()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (programId, rulePackId) = await CreateSampleRulePackAsync(adminToken);
        var complianceKeyId = await CreateComplianceKeyAsync(adminToken, "v1_derived_fact_key");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/regulatory-mappings", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRegulatoryMappingRequest(
            "v1_derived_fact_mapping",
            "V1 derived fact mapping",
            "Mapping created through v1 alias route.",
            "compliance_key",
            programId,
            rulePackId,
            null,
            null,
            complianceKeyId,
            null));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        Assert.StartsWith("/api/v1/regulatory-mappings/", createResponse.Headers.Location?.ToString());

        var listDerivedResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/derived-facts", adminToken));
        listDerivedResponse.EnsureSuccessStatusCode();
        var derivedFacts = (await listDerivedResponse.Content.ReadFromJsonAsync<IReadOnlyList<RegulatoryMappingResponse>>())!;
        Assert.Contains(derivedFacts, x => x.MappingKey == "v1_derived_fact_mapping");

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/derived-facts/preview", adminToken);
        previewRequest.Content = JsonContent.Create(new DerivedFactPreviewRequest(
            programId,
            rulePackId,
            null,
            complianceKeyId,
            null,
            10));
        var previewResponse = await _complianceCoreClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<DerivedFactPreviewResponse>())!;
        Assert.True(preview.ReturnedCount >= 1);
        Assert.Contains(preview.Items, x => x.MappingKey == "v1_derived_fact_mapping");
    }

    private async Task<Guid> CreateComplianceKeyAsync(string adminToken, string key)
    {
        var request = Authorized(HttpMethod.Post, "/api/compliance-keys", adminToken);
        request.Content = JsonContent.Create(new CreateComplianceKeyRequest(
            key,
            "Test compliance key",
            "compliance_domain",
            "Test compliance key for regulatory mapping."));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<ComplianceKeyResponse>())!;
        return created.ComplianceKeyId;
    }

    private async Task<Guid> CreateMaterialKeyAsync(string adminToken, string key)
    {
        var request = Authorized(HttpMethod.Post, "/api/material-keys", adminToken);
        request.Content = JsonContent.Create(new CreateMaterialKeyRequest(
            key,
            "Test material key",
            "material_hazard",
            "Test material key for regulatory mapping."));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<MaterialKeyResponse>())!;
        return created.MaterialKeyId;
    }

    private async Task<(Guid ProgramId, Guid RulePackId)> CreateSampleRulePackAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "U.S. Department of Transportation",
            "Federal transportation safety and compliance authority."));
        var body = (await (await _complianceCoreClient.SendAsync(bodyRequest)).Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "us_federal",
            "United States Federal",
            "Federal jurisdiction for interstate transportation rules."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "fmcsa_safety",
            "FMCSA Safety Compliance",
            "Federal motor carrier safety compliance program."));
        var program = (await (await _complianceCoreClient.SendAsync(programRequest)).Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;

        var packRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        packRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            program.RegulatoryProgramId,
            "driver_qualification",
            "Driver Qualification Rules",
            "Baseline driver qualification rule pack."));
        var pack = (await (await _complianceCoreClient.SendAsync(packRequest)).Content.ReadFromJsonAsync<RulePackResponse>())!;

        return (program.RegulatoryProgramId, pack.RulePackId);
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
