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

public class ComplianceCoreRegulatoryRegistriesTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreRegulatory-{Guid.NewGuid():N}";

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
    public async Task Regulatory_registry_chain_create_and_list()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "U.S. Department of Transportation",
            "Federal transportation safety and compliance authority."));
        var bodyResponse = await _complianceCoreClient.SendAsync(bodyRequest);
        bodyResponse.EnsureSuccessStatusCode();
        var body = (await bodyResponse.Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "us_federal",
            "United States Federal",
            "Federal jurisdiction for interstate transportation rules."));
        var jurisdictionResponse = await _complianceCoreClient.SendAsync(jurisdictionRequest);
        jurisdictionResponse.EnsureSuccessStatusCode();
        var jurisdiction = (await jurisdictionResponse.Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "fmcsa_safety",
            "FMCSA Safety Compliance",
            "Federal motor carrier safety compliance program."));
        var programResponse = await _complianceCoreClient.SendAsync(programRequest);
        programResponse.EnsureSuccessStatusCode();
        var program = (await programResponse.Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;

        var listProgramsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/regulatory-programs?jurisdictionId={jurisdiction.JurisdictionId}", adminToken));
        listProgramsResponse.EnsureSuccessStatusCode();
        var programs = (await listProgramsResponse.Content.ReadFromJsonAsync<IReadOnlyList<RegulatoryProgramResponse>>())!;
        Assert.Single(programs);
        Assert.Equal("fmcsa_safety", programs[0].ProgramKey);
        Assert.Equal(program.RegulatoryProgramId, programs[0].RegulatoryProgramId);
    }

    [Fact]
    public async Task Rule_pack_create_list_and_publish_lifecycle()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await CreateSampleProgramAsync(adminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification Rules",
            "Baseline driver qualification rule pack."));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;
        Assert.Equal("draft", created.Status);
        Assert.Equal(1, created.VersionNumber);
        Assert.Equal("fmcsa_safety", created.RegulatoryProgramKey);

        var reviewRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{created.RulePackId}/status", adminToken);
        reviewRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest("review"));
        var reviewResponse = await _complianceCoreClient.SendAsync(reviewRequest);
        reviewResponse.EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{created.RulePackId}/status", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest("published"));
        var publishResponse = await _complianceCoreClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;
        Assert.Equal("published", published.Status);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/rule-packs?regulatoryProgramId={programId}", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var packs = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<RulePackResponse>>())!;
        Assert.Single(packs);
        Assert.Equal("driver_qualification", packs[0].PackKey);
    }

    [Fact]
    public async Task Rule_pack_create_denies_member_role()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await CreateSampleProgramAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/rule-packs", memberToken);
        request.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "vehicle_inspection",
            "Vehicle Inspection Rules",
            "Baseline vehicle inspection rule pack."));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Governing_body_create_denies_member_role()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var request = Authorized(HttpMethod.Post, "/api/governing-bodies", memberToken);
        request.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "osha",
            "OSHA",
            "Occupational Safety and Health Administration."));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Regulatory_read_allows_tenant_admin_after_non_compliancecore_launch_context()
    {
        var token = CreateComplianceCoreAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/governing-bodies", token));
        response.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
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
        return program.RegulatoryProgramId;
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
