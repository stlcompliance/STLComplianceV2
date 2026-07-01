using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrPriceSnapshotWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _sharedWorkerToSupplyArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PriceSnapshotNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"PriceSnapshotSupplyArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToSupplyArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["supplyarr"],
            PriceSnapshotWorkerService.ProcessPriceSnapshotCapturesActionScope);

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
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.PostAsJsonAsync(
            "/api/internal/price-snapshots/process-batch",
            new ProcessPriceSnapshotCapturesRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 25, 24));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_creates_pricing_snapshot_from_catalog_price()
    {
        var linkId = await SeedSupplierLinkWithCatalogPriceAsync(catalogUnitPrice: 12.5m);
        await UpsertSettingsAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/price-snapshots/process-batch",
            _sharedWorkerToSupplyArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPriceSnapshotCapturesRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            24));

        var processResponse = await _supplyarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessPriceSnapshotCapturesResponse>())!;
        Assert.Equal(1, body.CandidatesFound);
        Assert.Equal(1, body.CapturedCount);
        Assert.Single(body.Captured);
        Assert.Equal(12.5m, body.Captured[0].UnitPrice);
        Assert.Equal(SnapshotSources.SupplierFeed, body.Captured[0].Source);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var captureState = await db.PartSupplierPriceCaptureStates
            .SingleAsync(x => x.PartSupplierLinkId == linkId);
        Assert.Equal(12.5m, captureState.LastCapturedUnitPrice);
        Assert.NotNull(captureState.LastPricingSnapshotId);
    }

    [Fact]
    public async Task List_pending_price_snapshot_returns_catalog_drift_candidates()
    {
        await SeedSupplierLinkWithCatalogPriceAsync(catalogUnitPrice: 9.99m, currentSnapshotPrice: 8.5m);
        await UpsertSettingsAsync();

        var pendingRequest = Authorized(
            HttpMethod.Get,
            $"/api/internal/price-snapshots/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=25",
            _sharedWorkerToSupplyArrToken);

        var pendingResponse = await _supplyarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingPriceSnapshotCapturesResponse>())!;
        Assert.NotEmpty(pending.Items);
        Assert.Equal(9.99m, pending.Items[0].CatalogUnitPrice);
        Assert.NotEqual(Guid.Empty, pending.Items[0].SupplierId);
        Assert.False(string.IsNullOrWhiteSpace(pending.Items[0].SupplierKey));
    }

    private async Task UpsertSettingsAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantPriceSnapshotSettings.Add(new TenantPriceSnapshotSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            StalenessHours = 24,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedSupplierLinkWithCatalogPriceAsync(
        decimal catalogUnitPrice,
        decimal? currentSnapshotPrice = null)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierKey = $"supplier-{Guid.NewGuid():N}"[..16],
            DisplayName = "Price supplier",
            LegalName = "Price supplier LLC",
            TaxIdentifier = string.Empty,
            ApprovalStatus = "approved",
            Status = "active",
            Notes = string.Empty,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = $"part-{Guid.NewGuid():N}"[..16],
            DisplayName = "Price snapshot part",
            Description = string.Empty,
            CategoryKey = "general",
            UnitOfMeasure = "each",
            ManufacturerName = string.Empty,
            ManufacturerPartNumber = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var link = new PartSupplierLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            SupplierId = supplier.Id,
            SupplierPartNumber = "VPN-100",
            IsPreferred = true,
            CatalogUnitPrice = catalogUnitPrice,
            CatalogCurrencyCode = "USD",
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Suppliers.Add(supplier);
        db.Parts.Add(part);
        db.PartSupplierLinks.Add(link);

        if (currentSnapshotPrice is not null)
        {
            db.PartSupplierPricingSnapshots.Add(new PartSupplierPricingSnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PartSupplierLinkId = link.Id,
                SnapshotKey = $"manual-{Guid.NewGuid():N}"[..24],
                UnitPrice = currentSnapshotPrice.Value,
                CurrencyCode = "USD",
                EffectiveFrom = now.AddDays(-1),
                EffectiveTo = null,
                Source = SnapshotSources.Manual,
                Notes = string.Empty,
                CreatedByUserId = Guid.NewGuid(),
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await db.SaveChangesAsync();
        return link.Id;
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

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-price-snapshot-test",
            $"{sourceProduct} price snapshot test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            allowedProducts,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}


