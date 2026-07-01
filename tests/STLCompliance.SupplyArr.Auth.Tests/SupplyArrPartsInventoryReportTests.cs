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
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrPartsInventoryReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _serviceToken = null!;
    private string _userToken = null!;
    private Guid _partId;
    private Guid _locationId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PartsInventoryReportNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"PartsInventoryReportSupplyArr-{Guid.NewGuid():N}";

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
        _serviceToken = await IssueServiceTokenAsync(adminToken, "supplyarr");
        var handoffCode = await CreateHandoffAsync(adminToken);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _serviceToken);
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
        (_partId, _locationId) = await SeedPartsInventoryAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Parts_inventory_summary_returns_aggregates()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/parts-inventory/summary", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<PartsInventoryReportSummaryResponse>();
        Assert.NotNull(summary);
        Assert.NotEmpty(summary!.Parts);

        var part = summary.Parts.Single(x => x.PartId == _partId);
        Assert.True(part.BelowReorderPoint);
        Assert.Equal(8m, part.QuantityOnHand);
        Assert.Equal(1, part.SupplierLinkCount);
        Assert.True(summary.Totals.BelowReorderPointCount >= 1);
    }

    [Fact]
    public async Task Parts_inventory_part_detail_returns_stock_rows()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/reports/parts-inventory/parts/{_partId}", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<PartsInventoryPartDetailResponse>();
        Assert.NotNull(detail);
        Assert.Equal(_partId, detail!.Summary.PartId);
        Assert.NotEmpty(detail.StockByBin);
        Assert.NotEmpty(detail.SupplierLinks);
        var supplierLink = Assert.Single(detail.SupplierLinks);
        Assert.Equal("Inventory Supplier", supplierLink.SupplierDisplayName);
        Assert.Equal("identity", supplierLink.SupplierUnitKind);
        Assert.Equal("VPN-INV", supplierLink.SupplierPartNumber);
        Assert.True(supplierLink.IsPreferred);
    }

    [Fact]
    public async Task Parts_inventory_location_detail_returns_bins_and_parts()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/reports/parts-inventory/locations/{_locationId}",
                _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<PartsInventoryLocationDetailResponse>();
        Assert.NotNull(detail);
        Assert.Equal(_locationId, detail!.Summary.InventoryLocationId);
        Assert.NotEmpty(detail.Bins);
        Assert.NotEmpty(detail.Parts);
    }

    [Fact]
    public async Task Parts_inventory_export_returns_csv()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/parts-inventory/summary/export", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("partKey,displayName", csv, StringComparison.Ordinal);
        Assert.Contains("PART-INV", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Parts_inventory_summary_denied_without_auth()
    {
        var response = await _supplyarrClient.GetAsync("/api/reports/parts-inventory/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<(Guid PartId, Guid LocationId)> SeedPartsInventoryAsync()
    {
        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var location = new InventoryLocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LocationKey = "WH-INV",
            Name = "Inventory Report Warehouse",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var bin = new InventoryBin
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InventoryLocationId = location.Id,
            BinKey = "BIN-A",
            Name = "Bin A",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierKey = "V-INV",
            
            DisplayName = "Inventory Supplier",
            LegalName = "Inventory Supplier",
            ApprovalStatus = "approved",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = "PART-INV",
            DisplayName = "Inventory Report Part",
            Description = string.Empty,
            CategoryKey = "general",
            UnitOfMeasure = "each",
            Status = "active",
            ReorderPoint = 10m,
            ReorderQuantity = 5m,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var link = new PartSupplierLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            SupplierId = supplier.Id,
            SupplierPartNumber = "VPN-INV",
            IsPreferred = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var stock = new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            InventoryBinId = bin.Id,
            QuantityOnHand = 8m,
            QuantityReserved = 2m,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.InventoryLocations.Add(location);
        db.InventoryBins.Add(bin);
        db.Suppliers.Add(supplier);
        db.Parts.Add(part);
        db.PartSupplierLinks.Add(link);
        db.PartStockLevels.Add(stock);
        await db.SaveChangesAsync();
        return (part.Id, location.Id);
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

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-parts-inventory-report-test",
            $"{productKey} parts inventory report test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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


