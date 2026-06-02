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
using NexArr.Api.Services;
using STLCompliance.Shared.Integration;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreSdsHazComRuleVersionTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private readonly Guid _staffarrSiteOrgUnitId = Guid.Parse("e15113ec-5e80-41e2-8d51-66dfd214dd8d");
    private RecordingStaffArrSiteLookupHandler _staffarrSiteLookupHandler = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreSdsHazCom-{Guid.NewGuid():N}";
        _staffarrSiteLookupHandler = new RecordingStaffArrSiteLookupHandler(_staffarrSiteOrgUnitId);

        _factory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("StaffArr:BaseUrl", "http://staffarr.test");
            builder.UseSetting("StaffArr:ServiceToken", "compliancecore-to-staffarr-token");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddHttpClient<StaffArrSiteLookupClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrSiteLookupHandler);
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Sds_create_and_list_round_trip()
    {
        var adminToken = CreateAccessToken(["compliancecore"], "compliance_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/sds", adminToken);
        createRequest.Content = JsonContent.Create(new CreateSdsReferenceRequest(
            "acetone-sds",
            null,
            "Acetone",
            "Example Co",
            "https://example.com/sds",
            new DateOnly(2026, 1, 1)));

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/sds", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var items = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<SdsReferenceResponse>>())!;
        Assert.Contains(items, x => x.SdsKey == "acetone-sds" && x.ProductName == "Acetone");
    }

    [Fact]
    public async Task HazCom_create_and_list_round_trip()
    {
        var adminToken = CreateAccessToken(["compliancecore"], "compliance_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/hazcom", adminToken);
        createRequest.Content = JsonContent.Create(new CreateHazComReferenceRequest(
            "shop-a-hazcom",
            "Shop A HazCom binder",
            "Central binder location",
            null,
            "shop-a",
            "https://example.com/hazcom",
            true,
            _staffarrSiteOrgUnitId));

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/hazcom", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var items = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<HazComReferenceResponse>>())!;
        Assert.Contains(items, x =>
            x.HazComKey == "shop-a-hazcom"
            && x.StaffarrSiteOrgUnitId == _staffarrSiteOrgUnitId
            && x.StaffarrSiteNameSnapshot == "Central Compliance Site");
    }

    [Fact]
    public async Task Rule_versions_list_returns_empty_when_no_packs()
    {
        var adminToken = CreateAccessToken(["compliancecore"], "compliance_admin");
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/rule-versions", adminToken));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<RuleVersionListResponse>())!;
        Assert.Empty(payload.Items);
    }

    [Fact]
    public async Task Rule_version_publish_archives_prior_published_and_rollback_restores()
    {
        var adminToken = CreateAccessToken(["compliancecore"], "compliance_admin");
        var programId = await CreateSampleProgramAsync(adminToken);

        var v1 = await CreateRulePackAsync(adminToken, programId, "driver_qualification", "Driver Qualification v1");
        await AdvanceStatusAsync(adminToken, v1.RulePackId, RulePackStatuses.Review);
        await AdvanceStatusAsync(adminToken, v1.RulePackId, RulePackStatuses.Published);

        var v2 = await CreateRulePackAsync(adminToken, programId, "driver_qualification", "Driver Qualification v2");
        Assert.Equal(2, v2.VersionNumber);
        await AdvanceStatusAsync(adminToken, v2.RulePackId, RulePackStatuses.Review);

        var publishRequest = Authorized(HttpMethod.Post, $"/api/rule-versions/{v2.RulePackId}/publish", adminToken);
        var publishResponse = await _client.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<RuleVersionResponse>())!;
        Assert.Equal(RulePackStatuses.Published, published.Status);
        Assert.Equal(2, published.VersionNumber);

        var listResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/rule-versions?packKey=driver_qualification", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var versions = (await listResponse.Content.ReadFromJsonAsync<RuleVersionListResponse>())!.Items;
        Assert.Contains(versions, x => x.RulePackId == v1.RulePackId && x.Status == RulePackStatuses.Archived);
        Assert.Contains(versions, x => x.RulePackId == v2.RulePackId && x.Status == RulePackStatuses.Published);

        var rollbackRequest = Authorized(HttpMethod.Post, $"/api/rule-versions/{v2.RulePackId}/rollback", adminToken);
        var rollbackResponse = await _client.SendAsync(rollbackRequest);
        rollbackResponse.EnsureSuccessStatusCode();
        var rollback = (await rollbackResponse.Content.ReadFromJsonAsync<RuleVersionRollbackResponse>())!;
        Assert.Equal(RulePackStatuses.Archived, rollback.ArchivedVersion.Status);
        Assert.Equal(2, rollback.ArchivedVersion.VersionNumber);
        Assert.Equal(RulePackStatuses.Published, rollback.RestoredVersion.Status);
        Assert.Equal(1, rollback.RestoredVersion.VersionNumber);
    }

    [Fact]
    public async Task Rule_version_publish_denies_member_role()
    {
        var adminToken = CreateAccessToken(["compliancecore"], "compliance_admin");
        var memberToken = CreateAccessToken(["compliancecore"], "tenant_member");
        var programId = await CreateSampleProgramAsync(adminToken);
        var pack = await CreateRulePackAsync(adminToken, programId, "vehicle_inspection", "Vehicle Inspection");
        await AdvanceStatusAsync(adminToken, pack.RulePackId, RulePackStatuses.Review);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/rule-versions/{pack.RulePackId}/publish", memberToken));
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "U.S. Department of Transportation",
            "Federal transportation safety and compliance authority."));
        var body = (await (await _client.SendAsync(bodyRequest)).Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "us_federal",
            "United States Federal",
            "Federal jurisdiction for interstate transportation rules."));
        var jurisdiction = (await (await _client.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "fmcsa_safety",
            "FMCSA Safety Compliance",
            "Federal motor carrier safety compliance program."));
        var program = (await (await _client.SendAsync(programRequest)).Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
    }

    private async Task<RulePackResponse> CreateRulePackAsync(
        string adminToken,
        Guid programId,
        string packKey,
        string label)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            packKey,
            label,
            $"{label} description."));
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        return (await createResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;
    }

    private async Task AdvanceStatusAsync(string adminToken, Guid rulePackId, string status)
    {
        var request = Authorized(HttpMethod.Patch, $"/api/rule-packs/{rulePackId}/status", adminToken);
        request.Content = JsonContent.Create(new UpdateRulePackStatusRequest(status));
        (await _client.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private string CreateAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey)
    {
        using var scope = _factory.Services.CreateScope();
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

    private sealed class RecordingStaffArrSiteLookupHandler(Guid siteOrgUnitId) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (!path.EndsWith($"/api/v1/integrations/sites/{siteOrgUnitId:D}", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            }

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new StaffArrSiteLookupResponse(
                    siteOrgUnitId,
                    "Central Compliance Site",
                    null,
                    "active"))
            });
        }
    }
}
