using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using STLCompliance.E2E.Support;
using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// M13 ship-gate: authenticated JWT without the target product entitlement must be denied on
/// <c>/api/me</c> (product APIs) and launch context (NexArr).
/// </summary>
[Trait("Category", "Integration")]
[Trait("Area", "EntitlementDenial")]
public sealed class EntitlementDenialFlowTests : IAsyncLifetime
{
    private static readonly string[] WrongEntitlements = ["nexarr"];

    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private WebApplicationFactory<global::ReportArr.Api.Program> _reportarrFactory = null!;
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private HttpClient _routarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private HttpClient _reportarrClient = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();

        var signingKey = E2ENexArrHost.SigningKey;
        var nexarrBaseUrl = _nexarr.Client.BaseAddress!.ToString().TrimEnd('/');

        _staffarrFactory = CreateProductFactory<global::StaffArr.Api.Program, StaffArr.Api.Data.StaffArrDbContext>(
            $"E2E-Entitlement-StaffArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _staffarrClient = _staffarrFactory.CreateClient();

        _trainarrFactory = CreateProductFactory<global::TrainArr.Api.Program, TrainArr.Api.Data.TrainArrDbContext>(
            $"E2E-Entitlement-TrainArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _trainarrClient = _trainarrFactory.CreateClient();

        _maintainarrFactory = CreateProductFactory<global::MaintainArr.Api.Program, MaintainArr.Api.Data.MaintainArrDbContext>(
            $"E2E-Entitlement-MaintainArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _maintainarrClient = _maintainarrFactory.CreateClient();

        _routarrFactory = CreateProductFactory<global::RoutArr.Api.Program, RoutArr.Api.Data.RoutArrDbContext>(
            $"E2E-Entitlement-RoutArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _routarrClient = _routarrFactory.CreateClient();

        _supplyarrFactory = CreateProductFactory<global::SupplyArr.Api.Program, SupplyArr.Api.Data.SupplyArrDbContext>(
            $"E2E-Entitlement-SupplyArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _supplyarrClient = _supplyarrFactory.CreateClient();

        _reportarrFactory = CreateProductFactory<global::ReportArr.Api.Program, ReportArr.Api.Data.ReportArrDbContext>(
            $"E2E-Entitlement-ReportArr-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _reportarrClient = _reportarrFactory.CreateClient();

        _complianceCoreFactory = CreateProductFactory<global::ComplianceCore.Api.Program, ComplianceCore.Api.Data.ComplianceCoreDbContext>(
            $"E2E-Entitlement-ComplianceCore-{Guid.NewGuid():N}",
            signingKey,
            nexarrBaseUrl);
        _complianceCoreClient = _complianceCoreFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _trainarrClient.Dispose();
        _maintainarrClient.Dispose();
        _routarrClient.Dispose();
        _supplyarrClient.Dispose();
        _reportarrClient.Dispose();
        _complianceCoreClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _trainarrFactory.DisposeAsync();
        await _maintainarrFactory.DisposeAsync();
        await _routarrFactory.DisposeAsync();
        await _supplyarrFactory.DisposeAsync();
        await _reportarrFactory.DisposeAsync();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarr.DisposeAsync();
    }

    public static TheoryData<M13EntitlementDenialProbe> ProductEntitlementDenialProbes =>
        new(StlM13ShipGateCatalog.ProductApiEntitlementDenialProbes);

    [Theory]
    [MemberData(nameof(ProductEntitlementDenialProbes))]
    public async Task Product_api_me_forbidden_without_product_entitlement(M13EntitlementDenialProbe probe)
    {
        var token = MintTokenWithoutProductEntitlement(probe.ProductKey);
        var client = ClientFor(probe.ProductKey);

        var response = await client.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, probe.DenialPath, token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task NexArr_launch_context_forbidden_for_unknown_product_key()
    {
        var loginToken = await _nexarr.LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var response = await _nexarr.Client.SendAsync(
            HttpTestClient.Authorized(
                HttpMethod.Get,
                $"{StlM13ShipGateCatalog.NexArrLaunchContextPath}?productKey={StlM13ShipGateCatalog.NexArrDeniedLaunchProductKey}",
                loginToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string MintTokenWithoutProductEntitlement(string productKey)
    {
        var tenantId = E2ETenants.TenantAId;
        var userId = PlatformSeeder.DemoAdminUserId;
        var personId = PlatformSeeder.DemoAdminUserId;

        return productKey switch
        {
            "staffarr" => E2EAccessTokenHelper.StaffArr(
                _staffarrFactory.Services, tenantId, userId, personId, WrongEntitlements),
            "trainarr" => E2EAccessTokenHelper.TrainArr(
                _trainarrFactory.Services, tenantId, userId, personId, WrongEntitlements),
            "maintainarr" => E2EAccessTokenHelper.MaintainArr(
                _maintainarrFactory.Services, tenantId, userId, WrongEntitlements),
            "routarr" => E2EAccessTokenHelper.RoutArr(
                _routarrFactory.Services, tenantId, userId, WrongEntitlements),
            "supplyarr" => E2EAccessTokenHelper.SupplyArr(
                _supplyarrFactory.Services, tenantId, userId, WrongEntitlements),
            "reportarr" => E2EAccessTokenHelper.ReportArr(
                _reportarrFactory.Services, tenantId, userId, WrongEntitlements),
            "compliancecore" => E2EAccessTokenHelper.ComplianceCore(
                _complianceCoreFactory.Services, tenantId, userId, WrongEntitlements),
            _ => throw new ArgumentOutOfRangeException(nameof(productKey), productKey, "Unknown product key."),
        };
    }

    private HttpClient ClientFor(string productKey) =>
        productKey switch
        {
            "staffarr" => _staffarrClient,
            "trainarr" => _trainarrClient,
            "maintainarr" => _maintainarrClient,
            "routarr" => _routarrClient,
            "supplyarr" => _supplyarrClient,
            "reportarr" => _reportarrClient,
            "compliancecore" => _complianceCoreClient,
            _ => throw new ArgumentOutOfRangeException(nameof(productKey), productKey, "Unknown product key."),
        };

    private static WebApplicationFactory<TProgram> CreateProductFactory<TProgram, TContext>(
        string databaseName,
        string signingKey,
        string nexarrBaseUrl)
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
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TContext>(services);
                services.AddDbContext<TContext>(options => options.UseInMemoryDatabase(databaseName));
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
