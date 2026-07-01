using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
using SupplyArr.Api.Entities;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrSupplierCatalogApiTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;
    private Guid _supplierId;
    private Guid _partOneId;
    private Guid _partTwoId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"SupplierCatalogApiNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplierCatalogApiSupplyArr-{Guid.NewGuid():N}";

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
        var serviceToken = await IssueServiceTokenAsync(adminToken);
        var handoffCode = await CreateHandoffAsync(adminToken);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", serviceToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();
        _userToken = await RedeemHandoffAsync(handoffCode);
        (_supplierId, _partOneId, _partTwoId) = await SeedSupplierCatalogAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Supplier_catalog_api_sync_applies_rows_and_updates_preferred_supplier()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                "/api/v1/supplier-catalogs/sync",
                _userToken,
                new SupplierCatalogApiSyncRequest(
                    "api-supplier",
                    false,
                    [
                        new SupplierCatalogApiSyncItem(
                            "filter-001",
                            "NEW-001",
                            true,
                            12.5m,
                            "usd",
                            5m,
                            7,
                            20m,
                            "in_stock"),
                        new SupplierCatalogApiSyncItem(
                            "filter-002",
                            "NEW-002",
                            false,
                            9.25m,
                            "USD",
                            2m,
                            14,
                            null,
                            "limited"),
                    ])));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var sync = await response.Content.ReadFromJsonAsync<SupplierCatalogApiSyncResponse>();
        Assert.NotNull(sync);
        Assert.True(sync!.Success);
        Assert.False(sync.DryRun);
        Assert.Equal(2, sync.ItemsRead);
        Assert.Equal(2, sync.ItemsAccepted);
        Assert.Equal(2, sync.ItemsApplied);
        Assert.Empty(sync.Issues);

        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();

        var preferredLink = await db.PartSupplierLinks.Include(x => x.Part).FirstAsync(x => x.PartId == _partOneId);
        Assert.Equal("NEW-001", preferredLink.SupplierPartNumber);
        Assert.True(preferredLink.IsPreferred);
        Assert.Equal(12.5m, preferredLink.CatalogUnitPrice);
        Assert.Equal("USD", preferredLink.CatalogCurrencyCode);
        Assert.Equal(5m, preferredLink.CatalogMinimumOrderQuantity);
        Assert.Equal(7, preferredLink.CatalogLeadTimeDays);
        Assert.Equal(20m, preferredLink.CatalogQuantityAvailable);
        Assert.Equal("in_stock", preferredLink.CatalogAvailabilityStatus);

        var createdLink = await db.PartSupplierLinks.Include(x => x.Part).FirstAsync(x => x.PartId == _partTwoId);
        Assert.Equal("NEW-002", createdLink.SupplierPartNumber);
        Assert.False(createdLink.IsPreferred);
        Assert.Equal(9.25m, createdLink.CatalogUnitPrice);
        Assert.Equal("USD", createdLink.CatalogCurrencyCode);
        Assert.Equal(2m, createdLink.CatalogMinimumOrderQuantity);
        Assert.Equal(14, createdLink.CatalogLeadTimeDays);
        Assert.Null(createdLink.CatalogQuantityAvailable);
        Assert.Equal("limited", createdLink.CatalogAvailabilityStatus);
    }

    [Fact]
    public async Task Supplier_catalog_api_sync_dry_run_does_not_persist_changes()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                "/api/v1/supplier-catalogs/sync",
                _userToken,
                new SupplierCatalogApiSyncRequest(
                    "api-supplier",
                    true,
                    [
                        new SupplierCatalogApiSyncItem(
                            "filter-001",
                            "DRY-001",
                            true,
                            22m,
                            "USD",
                            1m,
                            3,
                            11m,
                            "in_stock"),
                    ])));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var sync = await response.Content.ReadFromJsonAsync<SupplierCatalogApiSyncResponse>();
        Assert.NotNull(sync);
        Assert.True(sync!.Success);
        Assert.True(sync.DryRun);
        Assert.Equal(1, sync.ItemsRead);
        Assert.Equal(1, sync.ItemsAccepted);
        Assert.Equal(0, sync.ItemsApplied);

        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();

        var link = await db.PartSupplierLinks.Include(x => x.Part).FirstAsync(x => x.PartId == _partOneId);
        Assert.Equal("OLD-001", link.SupplierPartNumber);
        Assert.False(link.IsPreferred);
        Assert.Equal(10m, link.CatalogUnitPrice);
        Assert.Equal("USD", link.CatalogCurrencyCode);
        Assert.Equal(2m, link.CatalogMinimumOrderQuantity);
        Assert.Equal(8, link.CatalogLeadTimeDays);
        Assert.Equal(25m, link.CatalogQuantityAvailable);
        Assert.Equal("in_stock", link.CatalogAvailabilityStatus);
    }

    private async Task<(Guid supplierId, Guid partOneId, Guid partTwoId)> SeedSupplierCatalogAsync()
    {
        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierKey = "api-supplier",
            
            DisplayName = "API Supplier",
            LegalName = "API Supplier LLC",
            ApprovalStatus = "approved",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var partOne = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = "filter-001",
            DisplayName = "Primary Oil Filter",
            Description = string.Empty,
            CategoryKey = "filters",
            UnitOfMeasure = "each",
            ManufacturerName = "Fleet OEM",
            ManufacturerPartNumber = "FLT-001",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var partTwo = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = "filter-002",
            DisplayName = "Secondary Oil Filter",
            Description = string.Empty,
            CategoryKey = "filters",
            UnitOfMeasure = "each",
            ManufacturerName = "Fleet OEM",
            ManufacturerPartNumber = "FLT-002",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var link = new PartSupplierLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = partOne.Id,
            SupplierId = supplier.Id,
            SupplierPartNumber = "OLD-001",
            IsPreferred = false,
            CatalogUnitPrice = 10m,
            CatalogCurrencyCode = "USD",
            CatalogMinimumOrderQuantity = 2m,
            CatalogLeadTimeDays = 8,
            CatalogQuantityAvailable = 25m,
            CatalogAvailabilityStatus = "in_stock",
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Suppliers.Add(supplier);
        db.Parts.AddRange(partOne, partTwo);
        db.PartSupplierLinks.Add(link);
        await db.SaveChangesAsync();

        return (supplier.Id, partOne.Id, partTwo.Id);
    }

    private async Task SeedNexArrAsync()
    {
        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(payload);
        return payload!.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(string accessToken)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", accessToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "supplyarr-supplier-catalog-test",
            "SupplyArr Supplier Catalog Test",
            "supplyarr",
            ["supplyarr"]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", accessToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var payload = await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>();
        Assert.NotNull(payload);
        return payload!.AccessToken;
    }

    private async Task<string> CreateHandoffAsync(string accessToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", accessToken);
        request.Content = JsonContent.Create(new CreateHandoffRequest("supplyarr", "http://localhost:5179/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>();
        Assert.NotNull(payload);
        return payload!.HandoffCode;
    }

    private async Task<string> RedeemHandoffAsync(string handoffCode)
    {
        var response = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemHandoffRequest(handoffCode));
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>();
        Assert.NotNull(payload);
        return payload!.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private static void RemoveDbContext<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TDbContext>)
                || d.ServiceType == typeof(TDbContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}



