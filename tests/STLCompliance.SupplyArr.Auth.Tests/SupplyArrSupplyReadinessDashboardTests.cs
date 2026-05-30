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
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrSupplyReadinessDashboardTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"SupplyReadinessNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplyReadinessSupplyArr-{Guid.NewGuid():N}";

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

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Supply_readiness_dashboard_aggregates_tenant_scoped_totals()
    {
        await SeedReadinessScenarioAsync();

        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/supply-readiness/dashboard", buyerToken));
        response.EnsureSuccessStatusCode();

        var dashboard = (await response.Content.ReadFromJsonAsync<SupplyReadinessDashboardResponse>())!;
        Assert.True(dashboard.GeneratedAt > DateTimeOffset.MinValue);
        Assert.Equal(1, dashboard.Totals.ActivePartsCount);
        Assert.Equal(1, dashboard.Totals.PartsBelowReorderCount);
        Assert.Equal(1, dashboard.Totals.OpenBackorderCount);
        Assert.Equal(1, dashboard.Totals.OpenPurchaseRequestCount);
        Assert.Equal(1, dashboard.Totals.OpenDemandRefCount);
        Assert.Contains(
            dashboard.DemandRefsBySource,
            x => x.Source == DemandRefSources.MaintainArr && x.OpenCount == 1);
        Assert.Contains(dashboard.AttentionItems, x => x.Category == "stock");
        Assert.Contains(dashboard.AttentionItems, x => x.Category == "backorder");
    }

    [Fact]
    public async Task Supply_readiness_dashboard_requires_read_role()
    {
        var receiverToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_receiver");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/supply-readiness/dashboard", receiverToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Supply_readiness_dashboard_v1_aggregates_tenant_scoped_totals()
    {
        await SeedReadinessScenarioAsync();

        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/supply-readiness/dashboard", buyerToken));
        response.EnsureSuccessStatusCode();

        var dashboard = (await response.Content.ReadFromJsonAsync<SupplyReadinessDashboardResponse>())!;
        Assert.Equal(1, dashboard.Totals.ActivePartsCount);
        Assert.Equal(1, dashboard.Totals.PartsBelowReorderCount);
    }

    private async Task SeedReadinessScenarioAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var partId = Guid.NewGuid();
        db.Parts.Add(new Part
        {
            Id = partId,
            TenantId = tenantId,
            PartKey = "READINESS-PART",
            DisplayName = "Readiness test part",
            Status = "active",
            ReorderPoint = 10m,
            ReorderQuantity = 5m,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PartStockLevels.Add(new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = partId,
            InventoryBinId = Guid.NewGuid(),
            QuantityOnHand = 2m,
            QuantityReserved = 0m,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PurchaseRequests.Add(new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestKey = "PR-READY",
            Title = "Open PR",
            Status = PurchaseRequestStatuses.Draft,
            RequestedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.Backorders.Add(new Backorder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BackorderKey = "BO-READY",
            Status = BackorderStatuses.Open,
            SourceType = "purchase_order",
            PurchaseOrderId = Guid.NewGuid(),
            PurchaseOrderLineId = Guid.NewGuid(),
            PartId = partId,
            QuantityBackordered = 3m,
            Notes = "Test backorder",
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.MaintainArrDemandRefs.Add(new MaintainArrDemandRef
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MaintainarrPublicationId = Guid.NewGuid(),
            MaintainarrWorkOrderId = Guid.NewGuid(),
            MaintainarrWorkOrderNumber = "WO-READY",
            MaintainarrAssetId = Guid.NewGuid(),
            Title = "MaintainArr demand",
            Status = MaintainArrDemandRefStatuses.Received,
            ProcurementStatus = MaintainArrDemandRefProcurementStatuses.Received,
            ReceivedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private string CreateSupplyArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<SupplyArrTokenService>();
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
