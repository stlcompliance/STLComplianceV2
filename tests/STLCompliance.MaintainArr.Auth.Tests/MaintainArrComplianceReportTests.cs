using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using CreateAssetClassRequest = MaintainArr.Api.Contracts.CreateAssetClassRequest;
using CreateAssetRequest = MaintainArr.Api.Contracts.CreateAssetRequest;
using CreateAssetTypeRequest = MaintainArr.Api.Contracts.CreateAssetTypeRequest;
using CreateInspectionTemplateRequest = MaintainArr.Api.Contracts.CreateInspectionTemplateRequest;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrComplianceReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _managerToken = null!;
    private Guid _templateId;
    private readonly Guid _staffarrSiteOrgUnitId = MaintainArrTestSites.DefaultStaffArrSiteOrgUnitId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ComplianceReportNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"ComplianceReportMaintainArr-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var serviceToken = await IssueServiceTokenAsync(adminToken, "maintainarr");

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", serviceToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
        await MaintainArrTestSites.SeedCachedStaffArrSiteAsync(_maintainarrFactory, _staffarrSiteOrgUnitId);
        _managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        _templateId = await SeedComplianceFixtureAsync(_managerToken);
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Compliance_report_summary_returns_rollups()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/compliance/summary", _managerToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<ComplianceReportSummaryResponse>())!;
        Assert.True(summary.RegulatoryKeyMirrorCount >= 1);
        Assert.Contains(summary.RegulatoryKeyGroups, x => x.ComplianceKey == "dot.annual");
        Assert.Contains(summary.TemplateSummaries, x => x.InspectionTemplateId == _templateId);
    }

    [Fact]
    public async Task Compliance_report_template_detail_returns_inspection_stats()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/reports/compliance/inspection-templates/{_templateId:D}",
                _managerToken));
        response.EnsureSuccessStatusCode();

        var detail = (await response.Content.ReadFromJsonAsync<ComplianceReportTemplateDetailResponse>())!;
        Assert.Equal(_templateId, detail.InspectionTemplateId);
        Assert.NotEmpty(detail.RegulatoryKeys);
    }

    [Fact]
    public async Task Compliance_report_summary_export_returns_csv()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/compliance/summary/export", _managerToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("inspection,pass_rate_percent", csv, StringComparison.Ordinal);
        Assert.Contains("dot.annual", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Compliance_report_alerts_returns_compliance_alert_items()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/compliance/alerts", _managerToken));
        response.EnsureSuccessStatusCode();

        var alerts = (await response.Content.ReadFromJsonAsync<List<ComplianceAlertResponse>>())!;
        Assert.Contains(alerts, x => x.AlertType == "critical_defect");
    }

    [Fact]
    public async Task Compliance_report_v1_aliases_work()
    {
        var summaryResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/compliance/summary", _managerToken));
        summaryResponse.EnsureSuccessStatusCode();

        var detailResponse = await _maintainarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/reports/compliance/inspection-templates/{_templateId:D}",
                _managerToken));
        detailResponse.EnsureSuccessStatusCode();

        var exportResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/compliance/summary/export", _managerToken));
        exportResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Compliance_report_summary_denies_unauthenticated()
    {
        var response = await _maintainarrClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "/api/reports/compliance/summary"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<Guid> SeedComplianceFixtureAsync(string token)
    {
        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            $"cmp-{Guid.NewGuid():N}".Substring(0, 10),
            "DOT Annual",
            "Compliance inspection template"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        using (var scope = _maintainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
            var tenantId = PlatformSeeder.DemoTenantId;
            var now = DateTimeOffset.UtcNow;
            db.ComplianceRegulatoryKeyMirrors.Add(new ComplianceRegulatoryKeyMirror
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SubjectType = ComplianceRegulatoryKeyMirrorSubjectTypes.InspectionTemplate,
                SubjectId = template.InspectionTemplateId,
                ComplianceKey = "dot.annual",
                MaterialKey = "vehicle.fleet",
                RegulatoryCitationKey = "fmcsa.396.3",
                SourceProduct = ComplianceRegulatoryKeyMirrorSources.ComplianceCore,
                SourceRecordKey = "cc-key-1",
                SourceUpdatedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
            });
            await db.SaveChangesAsync();
        }

        var assetTypeId = await SeedAssetTypeAsync(token);
        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            $"CMP-{Guid.NewGuid():N}".Substring(0, 10),
            "Compliance Test Asset",
            string.Empty,
            _staffarrSiteOrgUnitId.ToString("D")));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        using (var scope = _maintainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
            var tenantId = PlatformSeeder.DemoTenantId;
            var now = DateTimeOffset.UtcNow;
            db.Defects.Add(new Defect
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = asset.AssetId,
                Title = "Brake pressure fault",
                Description = "Critical compliance defect.",
                Severity = DefectSeverities.Critical,
                Status = DefectStatuses.Open,
                Source = DefectSources.Manual,
                ReportedByUserId = PlatformSeeder.DemoAdminUserId,
                CreatedAt = now,
                UpdatedAt = now,
            });
            await db.SaveChangesAsync();
        }

        return template.InspectionTemplateId;
    }

    private async Task<Guid> SeedAssetTypeAsync(string token)
    {
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"class-{Guid.NewGuid():N}".Substring(0, 10),
            "Compliance Class",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"type-{Guid.NewGuid():N}".Substring(0, 10),
            "Compliance Type",
            string.Empty));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;
        return assetType.AssetTypeId;
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new NexArr.Api.Contracts.CreateHandoffRequest(
            "maintainarr",
            "http://localhost:5178/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{productKey}-compliance-report-test",
            $"{productKey} compliance report test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        Guid? userIdOverride = null)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var userId = userIdOverride ?? PlatformSeeder.DemoAdminUserId;
        var (accessToken, _) = tokenService.CreateAccessToken(
            userId,
            userId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new NexArr.Api.Contracts.LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.AuthTokenResponse>())!;
        return login.AccessToken;
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

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        foreach (var descriptor in services
                     .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
                     .ToList())
        {
            services.Remove(descriptor);
        }
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
