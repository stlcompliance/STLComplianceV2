using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using MaintainArr.Api.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using STLCompliance.E2E.Support;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using TrainArr.Api.Contracts;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using SupplyArrIntegration = SupplyArr.Api.Endpoints.IntegrationEndpoints;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// Multi-tenant isolation battery: tenant-scoped JWT and service tokens must not read or mutate
/// another tenant's records across product APIs.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Area", "TenantIsolation")]
public sealed class TenantIsolationFlowTests : IAsyncLifetime
{
    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private HttpClient _routarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _trainarrToStaffarrTokenTenantB = null!;
    private string _maintainarrToSupplyarrTokenTenantB = null!;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();
        await _nexarr.EnsureTenantAsync(
            E2ETenants.TenantBId,
            "e2e-tenant-b",
            "E2E Tenant B",
            ["staffarr", "trainarr", "maintainarr", "routarr", "compliancecore", "supplyarr"]);

        var adminToken = await _nexarr.LoginAsync();
        _trainarrToStaffarrTokenTenantB = await _nexarr.IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            StaffArrIntegration.TrainingBlockerIngestActionScope,
            ["staffarr"],
            E2ETenants.TenantBId);
        _maintainarrToSupplyarrTokenTenantB = await _nexarr.IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            SupplyArrIntegration.MaintainarrDemandIngestActionScope,
            ["supplyarr"],
            E2ETenants.TenantBId);

        var signingKey = E2ENexArrHost.SigningKey;
        var nexarrBaseUrl = _nexarr.Client.BaseAddress!.ToString().TrimEnd('/');

        _staffarrFactory = CreateProductFactory<global::StaffArr.Api.Program, StaffArrDbContext>(
            $"E2E-TenantIso-StaffArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _staffarrClient = _staffarrFactory.CreateClient();
        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<StaffArrDbContext>().Database.EnsureCreatedAsync();
        }

        _maintainarrFactory = CreateProductFactory<global::MaintainArr.Api.Program, MaintainArr.Api.Data.MaintainArrDbContext>(
            $"E2E-TenantIso-MaintainArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _maintainarrClient = _maintainarrFactory.CreateClient();

        _routarrFactory = CreateProductFactory<global::RoutArr.Api.Program, RoutArr.Api.Data.RoutArrDbContext>(
            $"E2E-TenantIso-RoutArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _routarrClient = _routarrFactory.CreateClient();

        _trainarrFactory = CreateProductFactory<global::TrainArr.Api.Program, TrainArr.Api.Data.TrainArrDbContext>(
            $"E2E-TenantIso-TrainArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _trainarrClient = _trainarrFactory.CreateClient();

        _complianceCoreFactory = CreateProductFactory<global::ComplianceCore.Api.Program, ComplianceCoreDbContext>(
            $"E2E-TenantIso-ComplianceCore-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _complianceCoreClient = _complianceCoreFactory.CreateClient();

        _supplyarrFactory = CreateProductFactory<global::SupplyArr.Api.Program, SupplyArrDbContext>(
            $"E2E-TenantIso-SupplyArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _supplyarrClient = _supplyarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _maintainarrClient.Dispose();
        _routarrClient.Dispose();
        _trainarrClient.Dispose();
        _complianceCoreClient.Dispose();
        _supplyarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _maintainarrFactory.DisposeAsync();
        await _routarrFactory.DisposeAsync();
        await _trainarrFactory.DisposeAsync();
        await _complianceCoreFactory.DisposeAsync();
        await _supplyarrFactory.DisposeAsync();
        await _nexarr.DisposeAsync();
    }

    [Fact]
    public async Task NexArr_tenant_A_admin_cannot_get_tenant_B_detail()
    {
        var tenantAToken = await _nexarr.LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var response = await _nexarr.Client.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/tenants/{E2ETenants.TenantBId}", tenantAToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StaffArr_tenant_B_cannot_read_tenant_A_person()
    {
        var tenantAPersonId = await SeedStaffPersonAsync(E2ETenants.TenantAId, "Tenant A Person", "tenant-a-person@e2e.stl");
        var tenantBToken = E2EAccessTokenHelper.StaffArr(
            _staffarrFactory.Services,
            E2ETenants.TenantBId,
            E2ETenants.TenantBUserId,
            E2ETenants.TenantBPersonId,
            ["staffarr"]);

        var response = await _staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/people/{tenantAPersonId}", tenantBToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StaffArr_tenant_A_list_excludes_tenant_B_people()
    {
        await SeedStaffPersonAsync(E2ETenants.TenantAId, "Tenant A Listed", "tenant-a-listed@e2e.stl");
        await SeedStaffPersonAsync(E2ETenants.TenantBId, "Tenant B Hidden", "tenant-b-hidden@e2e.stl");
        var tenantAToken = E2EAccessTokenHelper.StaffArr(
            _staffarrFactory.Services,
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            ["staffarr"]);

        var response = await _staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, "/api/people?limit=200", tenantAToken));
        response.EnsureSuccessStatusCode();
        var people = (await response.Content.ReadFromJsonAsync<IReadOnlyList<StaffPersonSummaryResponse>>())!;

        Assert.Contains(people, p => p.PrimaryEmail == "tenant-a-listed@e2e.stl");
        Assert.DoesNotContain(people, p => p.PrimaryEmail == "tenant-b-hidden@e2e.stl");
    }

    [Fact]
    public async Task StaffArr_service_token_scoped_to_tenant_B_rejects_tenant_A_ingest()
    {
        var tenantAPersonId = await SeedStaffPersonAsync(E2ETenants.TenantAId, "Blocker Target", "blocker-target@e2e.stl");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/training-blockers");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _trainarrToStaffarrTokenTenantB);
        request.Content = JsonContent.Create(new IngestTrainingBlockerRequest(
            E2ETenants.TenantAId,
            tenantAPersonId,
            Guid.NewGuid(),
            "e2e.qualification",
            "E2E Qualification",
            "missing_training",
            "Cross-tenant ingest must be denied.",
            null));

        var response = await _staffarrClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MaintainArr_tenant_B_cannot_read_tenant_A_asset()
    {
        var tenantAToken = E2EAccessTokenHelper.MaintainArr(
            _maintainarrFactory.Services,
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            ["maintainarr"]);
        var assetId = await CreateMaintainArrAssetAsync(tenantAToken);

        var tenantBToken = E2EAccessTokenHelper.MaintainArr(
            _maintainarrFactory.Services,
            E2ETenants.TenantBId,
            E2ETenants.TenantBUserId,
            ["maintainarr"]);
        var response = await _maintainarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/assets/{assetId}", tenantBToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RoutArr_tenant_B_cannot_read_tenant_A_trip()
    {
        var tenantAToken = E2EAccessTokenHelper.RoutArr(
            _routarrFactory.Services,
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            ["routarr"]);
        var tripId = await CreateRoutArrTripAsync(tenantAToken);

        var tenantBToken = E2EAccessTokenHelper.RoutArr(
            _routarrFactory.Services,
            E2ETenants.TenantBId,
            E2ETenants.TenantBUserId,
            ["routarr"]);
        var response = await _routarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/trips/{tripId}", tenantBToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TrainArr_tenant_B_cannot_read_tenant_A_assignment()
    {
        var tenantAPersonId = await SeedStaffPersonAsync(E2ETenants.TenantAId, "Train Target", "train-target@e2e.stl");
        var tenantAToken = E2EAccessTokenHelper.TrainArr(
            _trainarrFactory.Services,
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            tenantAPersonId,
            ["trainarr"]);
        var assignmentId = await CreateTrainArrAssignmentAsync(tenantAToken, tenantAPersonId);

        var tenantBToken = E2EAccessTokenHelper.TrainArr(
            _trainarrFactory.Services,
            E2ETenants.TenantBId,
            E2ETenants.TenantBUserId,
            E2ETenants.TenantBPersonId,
            ["trainarr"]);
        var response = await _trainarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/training-assignments/{assignmentId}", tenantBToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ComplianceCore_tenant_B_list_excludes_tenant_A_vocabulary_terms()
    {
        var tenantAToken = E2EAccessTokenHelper.ComplianceCore(
            _complianceCoreFactory.Services,
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            ["compliancecore"]);
        var createRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/vocabulary", tenantAToken);
        createRequest.Content = JsonContent.Create(new CreateVocabularyTermRequest(
            $"tenant_a_term_{Guid.NewGuid():N}".Substring(0, 20),
            "Tenant A Term",
            "material_hazard",
            "Tenant A isolation term."));
        (await _complianceCoreClient.SendAsync(createRequest)).EnsureSuccessStatusCode();

        var tenantBToken = E2EAccessTokenHelper.ComplianceCore(
            _complianceCoreFactory.Services,
            E2ETenants.TenantBId,
            E2ETenants.TenantBUserId,
            ["compliancecore"]);
        var listResponse = await _complianceCoreClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, "/api/vocabulary?vocabularyTypeKey=material_hazard", tenantBToken));
        listResponse.EnsureSuccessStatusCode();
        var terms = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTermResponse>>())!;

        Assert.DoesNotContain(terms, t => t.Label == "Tenant A Term");
    }

    [Fact]
    public async Task SupplyArr_tenant_B_cannot_read_tenant_A_vendor()
    {
        var tenantAToken = E2EAccessTokenHelper.SupplyArr(
            _supplyarrFactory.Services,
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            ["supplyarr"]);
        var vendorId = await CreateSupplyArrVendorAsync(tenantAToken, "Tenant A Vendor");

        var tenantBToken = E2EAccessTokenHelper.SupplyArr(
            _supplyarrFactory.Services,
            E2ETenants.TenantBId,
            E2ETenants.TenantBUserId,
            ["supplyarr"]);
        var response = await _supplyarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/vendors/{vendorId}", tenantBToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SupplyArr_tenant_A_list_excludes_tenant_B_vendors()
    {
        var tenantAToken = E2EAccessTokenHelper.SupplyArr(
            _supplyarrFactory.Services,
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            ["supplyarr"]);
        await CreateSupplyArrVendorAsync(tenantAToken, "Tenant A Listed Vendor");

        var tenantBToken = E2EAccessTokenHelper.SupplyArr(
            _supplyarrFactory.Services,
            E2ETenants.TenantBId,
            E2ETenants.TenantBUserId,
            ["supplyarr"]);
        await CreateSupplyArrVendorAsync(tenantBToken, "Tenant B Hidden Vendor");

        var response = await _supplyarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, "/api/vendors", tenantAToken));
        response.EnsureSuccessStatusCode();
        var vendors = (await response.Content.ReadFromJsonAsync<IReadOnlyList<ExternalPartyResponse>>())!;

        Assert.Contains(vendors, v => v.DisplayName == "Tenant A Listed Vendor");
        Assert.DoesNotContain(vendors, v => v.DisplayName == "Tenant B Hidden Vendor");
    }

    [Fact]
    public async Task SupplyArr_service_token_scoped_to_tenant_B_rejects_tenant_A_demand_ingest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/maintainarr-demand");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _maintainarrToSupplyarrTokenTenantB);
        request.Content = JsonContent.Create(new IngestMaintainarrDemandRequest(
            E2ETenants.TenantAId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WO-TENANT-A",
            Guid.NewGuid(),
            "Cross-tenant demand",
            "Cross-tenant ingest must be denied.",
            false,
            [
                new IngestMaintainarrDemandLineRequest(
                    Guid.NewGuid(),
                    null,
                    "ISO-PART",
                    "Isolation part",
                    1m,
                    "ea",
                    null)
            ]));

        var response = await _supplyarrClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> CreateSupplyArrVendorAsync(string token, string displayName)
    {
        var createRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/vendors", token);
        createRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"iso-{Guid.NewGuid():N}".Substring(0, 12),
            displayName,
            $"{displayName} LLC",
            null,
            "Tenant isolation vendor"));
        var vendor = (await (await _supplyarrClient.SendAsync(createRequest)).Content
            .ReadFromJsonAsync<ExternalPartyResponse>())!;
        return vendor.PartyId;
    }

    private async Task<Guid> SeedStaffPersonAsync(Guid tenantId, string displayName, string email)
    {
        var personId = Guid.NewGuid();
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = tenantId,
            DisplayName = displayName,
            PrimaryEmail = email,
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
        return personId;
    }

    private async Task<Guid> CreateMaintainArrAssetAsync(string token)
    {
        var createClassRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"iso-{Guid.NewGuid():N}".Substring(0, 12),
            "Isolation Class",
            string.Empty));
        var assetClass = (await (await _maintainarrClient.SendAsync(createClassRequest)).Content
            .ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"iso-type-{Guid.NewGuid():N}".Substring(0, 12),
            "Isolation Type",
            string.Empty));
        var assetType = (await (await _maintainarrClient.SendAsync(createTypeRequest)).Content
            .ReadFromJsonAsync<AssetTypeResponse>())!;

        var createAssetRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetType.AssetTypeId,
            $"ISO-{Guid.NewGuid():N}".Substring(0, 12),
            "Tenant A Asset",
            string.Empty,
            null));
        var asset = (await (await _maintainarrClient.SendAsync(createAssetRequest)).Content
            .ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
    }

    private async Task<Guid> CreateRoutArrTripAsync(string token)
    {
        var createRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/trips", token);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Tenant A Trip",
            "Isolation battery trip",
            null,
            DateTimeOffset.UtcNow.AddHours(2),
            DateTimeOffset.UtcNow.AddHours(4),
            null));
        var created = (await (await _routarrClient.SendAsync(createRequest)).Content
            .ReadFromJsonAsync<TripDetailResponse>())!;
        return created.TripId;
    }

    private async Task<Guid> CreateTrainArrAssignmentAsync(string token, Guid staffarrPersonId)
    {
        var definitionRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/training-definitions", token);
        definitionRequest.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"iso_def_{Guid.NewGuid():N}".Substring(0, 16),
            "Isolation Definition",
            "Tenant isolation assignment definition",
            "iso.qualification",
            "Isolation Qualification"));
        var definition = (await (await _trainarrClient.SendAsync(definitionRequest)).Content
            .ReadFromJsonAsync<TrainingDefinitionResponse>())!;

        var assignmentRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/training-assignments", token);
        assignmentRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            staffarrPersonId,
            definition.TrainingDefinitionId,
            null,
            "tenant_isolation_e2e",
            DateTimeOffset.UtcNow.AddDays(7)));
        var assignment = (await (await _trainarrClient.SendAsync(assignmentRequest)).Content
            .ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        return assignment.AssignmentId;
    }

    private static WebApplicationFactory<TProgram> CreateProductFactory<TProgram, TContext>(
        string databaseName,
        string signingKey,
        string nexarrBaseUrl,
        Action<IServiceCollection>? configure = null,
        IReadOnlyDictionary<string, string?>? extraSettings = null)
        where TProgram : class
        where TContext : DbContext
    {
        return new WebApplicationFactory<TProgram>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", nexarrBaseUrl);
            if (extraSettings is not null)
            {
                foreach (var (key, value) in extraSettings)
                {
                    builder.UseSetting(key, value ?? string.Empty);
                }
            }

            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TContext>(services);
                services.AddDbContext<TContext>(options => options.UseInMemoryDatabase(databaseName));
                configure?.Invoke(services);
            });
        });
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
