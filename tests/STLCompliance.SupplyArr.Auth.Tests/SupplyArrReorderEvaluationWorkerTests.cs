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

public sealed class SupplyArrReorderEvaluationWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _sharedWorkerToSupplyArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ReorderEvalNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"ReorderEvalSupplyArr-{Guid.NewGuid():N}";

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
            ReorderEvaluationService.ProcessEvaluationActionScope);

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
    public async Task Process_reorder_evaluation_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.PostAsJsonAsync(
            "/api/internal/reorder/process-evaluation",
            new ProcessReorderEvaluationRequest(PlatformSeeder.DemoTenantId, 50, true));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_reorder_evaluation_creates_draft_purchase_request_for_low_stock_part()
    {
        var partId = await SeedLowStockPartAsync(reorderPoint: 10m, onHand: 2m);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/reorder/process-evaluation");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToSupplyArrToken);
        processRequest.Content = JsonContent.Create(new ProcessReorderEvaluationRequest(
            PlatformSeeder.DemoTenantId,
            50,
            CreateDraftPurchaseRequests: true));

        var processResponse = await _supplyarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessReorderEvaluationResponse>())!;
        Assert.True(body.SuggestionsCount >= 1);
        Assert.Equal(1, body.DraftPurchaseRequestsCreated);
        Assert.Single(body.CreatedPurchaseRequestIds);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var purchaseRequest = await db.PurchaseRequests
            .Include(x => x.Lines)
            .SingleAsync(x => x.Id == body.CreatedPurchaseRequestIds[0]);
        Assert.Equal(PurchaseRequestStatuses.Draft, purchaseRequest.Status);
        Assert.Contains(purchaseRequest.Lines, x => x.PartId == partId);
    }

    [Fact]
    public async Task List_pending_reorder_returns_candidates_before_processing()
    {
        await SeedLowStockPartAsync(reorderPoint: 8m, onHand: 1m);

        var pendingRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/reorder/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=50");
        pendingRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToSupplyArrToken);

        var pendingResponse = await _supplyarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingReorderEvaluationResponse>())!;
        Assert.NotEmpty(pending.Items);
    }

    private async Task<Guid> SeedLowStockPartAsync(decimal reorderPoint, decimal onHand)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var location = new InventoryLocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LocationKey = $"loc-{Guid.NewGuid():N}"[..16],
            Name = "Main warehouse",
            LocationType = "warehouse",
            AddressLine = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        var bin = new InventoryBin
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InventoryLocationId = location.Id,
            BinKey = $"bin-{Guid.NewGuid():N}"[..16],
            Name = "A1",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = $"part-{Guid.NewGuid():N}"[..16],
            DisplayName = "Reorder test part",
            Description = string.Empty,
            CategoryKey = "general",
            UnitOfMeasure = "each",
            ManufacturerName = string.Empty,
            ManufacturerPartNumber = string.Empty,
            Status = "active",
            ReorderPoint = reorderPoint,
            ReorderQuantity = 12m,
            CreatedAt = now,
            UpdatedAt = now
        };

        var stock = new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            InventoryBinId = bin.Id,
            QuantityOnHand = onHand,
            QuantityReserved = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.InventoryLocations.Add(location);
        db.InventoryBins.Add(bin);
        db.Parts.Add(part);
        db.PartStockLevels.Add(stock);
        await db.SaveChangesAsync();
        return part.Id;
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
            $"{sourceProduct}-reorder-eval-test",
            $"{sourceProduct} reorder evaluation test",
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
