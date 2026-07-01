using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrForgivingSearchTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _serviceToken = null!;
    private string _userToken = null!;
    private Guid _partId;
    private Guid _supplierId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ForgivingSearchNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"ForgivingSearchSupplyArr-{Guid.NewGuid():N}";

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
        (_partId, _supplierId) = await SeedSearchCorpusAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public void Normalizer_matches_fuzzy_part_key_with_dashes_removed()
    {
        var score = ForgivingSearchNormalizer.ScoreMatch("brk-pad-001", "brk pad 001");
        Assert.True(score >= 65);
    }

    [Fact]
    public async Task Forgiving_search_returns_cross_entity_matches()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/search/forgiving?q=brk", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ForgivingSearchResponse>();
        Assert.NotNull(payload);
        Assert.Contains(payload!.Results, x => x.EntityType == "part" && x.EntityId == _partId);
        Assert.Contains(payload.Results, x => x.EntityType == "supplier" && x.EntityId == _supplierId);
        Assert.Contains(payload.Results, x => x.EntityType == "supplier_sku");
        Assert.Contains(payload.Results, x => x.EntityType == "purchase_request");
        Assert.Contains(payload.Results, x => x.EntityType == "purchase_order");
    }

    [Fact]
    public async Task Forgiving_search_rejects_short_query()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/search/forgiving?q=a", _userToken));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Forgiving_search_denied_without_auth()
    {
        var response = await _supplyarrClient.GetAsync("/api/search/forgiving?q=acme");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task V1_search_and_inventory_index_endpoints_are_available()
    {
        var searchIndexResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/search", _userToken));
        searchIndexResponse.EnsureSuccessStatusCode();
        var searchIndexJson = await searchIndexResponse.Content.ReadAsStringAsync();
        using var searchDoc = JsonDocument.Parse(searchIndexJson);
        Assert.Contains(
            searchDoc.RootElement.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("path").GetString()),
            path => string.Equals(path, "/api/v1/search/forgiving", StringComparison.Ordinal));

        var inventoryIndexResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/inventory", _userToken));
        inventoryIndexResponse.EnsureSuccessStatusCode();
        var inventoryIndexJson = await inventoryIndexResponse.Content.ReadAsStringAsync();
        using var inventoryDoc = JsonDocument.Parse(inventoryIndexJson);
        var inventoryPaths = inventoryDoc.RootElement
            .GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("path").GetString())
            .ToList();
        Assert.Contains("/api/v1/inventory/locations", inventoryPaths);
        Assert.Contains("/api/v1/inventory/stock", inventoryPaths);
    }

    [Fact]
    public async Task External_reference_resolve_returns_part_and_supplier_matches()
    {
        var resolvePartResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/references/resolve?entityType=part&key=BRK-PAD-001", _userToken));
        resolvePartResponse.EnsureSuccessStatusCode();
        var partResolution = await resolvePartResponse.Content.ReadFromJsonAsync<ExternalReferenceResolutionResponse>();
        Assert.NotNull(partResolution);
        Assert.Equal("part", partResolution!.EntityType);
        Assert.Equal(_partId, partResolution.EntityId);
        Assert.Equal("BRK-PAD-001", partResolution.DisplayCode);

        var resolveSupplierResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/references/resolve?entityType=supplier&key=ACME-BRK", _userToken));
        resolveSupplierResponse.EnsureSuccessStatusCode();
        var supplierResolution = await resolveSupplierResponse.Content.ReadFromJsonAsync<ExternalReferenceResolutionResponse>();
        Assert.NotNull(supplierResolution);
        Assert.Equal("supplier", supplierResolution!.EntityType);
        Assert.Equal(_supplierId, supplierResolution.EntityId);
        Assert.Equal("ACME-BRK", supplierResolution.DisplayCode);
    }

    [Fact]
    public async Task External_reference_resolve_returns_not_found_for_unknown_key()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/references/resolve?entityType=part&key=DOES-NOT-EXIST", _userToken));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task External_reference_contract_index_is_available_on_base_and_v1_routes()
    {
        var v1Response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/references", _userToken));
        v1Response.EnsureSuccessStatusCode();
        var v1Index = await v1Response.Content.ReadFromJsonAsync<ExternalReferenceContractIndexResponse>();
        Assert.NotNull(v1Index);
        Assert.Contains(v1Index!.Items, item => item.EntityType == "part");
        Assert.Contains(v1Index.Items, item => item.EntityType == "purchase_order");

        var baseResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/references", _userToken));
        baseResponse.EnsureSuccessStatusCode();
        var baseIndex = await baseResponse.Content.ReadFromJsonAsync<ExternalReferenceContractIndexResponse>();
        Assert.NotNull(baseIndex);
        Assert.Equal(v1Index.Items.Count, baseIndex!.Items.Count);
    }

    [Fact]
    public async Task External_reference_batch_resolve_returns_found_and_missing_items()
    {
        var request = new ExternalReferenceBatchResolveRequest(
        [
            new ExternalReferenceResolveRequest("part", "BRK-PAD-001"),
            new ExternalReferenceResolveRequest("supplier", "ACME-BRK"),
            new ExternalReferenceResolveRequest("purchase_order", "MISSING-PO")
        ]);

        var httpRequest = Authorized(HttpMethod.Post, "/api/v1/references/resolve-batch", _userToken);
        httpRequest.Content = JsonContent.Create(request);
        var response = await _supplyarrClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ExternalReferenceBatchResolveResponse>();
        Assert.NotNull(payload);
        Assert.Equal(3, payload!.Items.Count);
        Assert.True(payload.Items[0].Found);
        Assert.Equal("part", payload.Items[0].Resolution!.EntityType);
        Assert.True(payload.Items[1].Found);
        Assert.Equal("supplier", payload.Items[1].Resolution!.EntityType);
        Assert.False(payload.Items[2].Found);
        Assert.Null(payload.Items[2].Resolution);
    }

    private async Task<(Guid PartId, Guid SupplierId)> SeedSearchCorpusAsync()
    {
        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierKey = "ACME-BRK",
            
            DisplayName = "Acme Brake Supply",
            LegalName = "Acme Brake Supply LLC",
            ApprovalStatus = "approved",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = "BRK-PAD-001",
            DisplayName = "Brake Pad Assembly",
            Description = "Front brake pad kit",
            CategoryKey = "brakes",
            UnitOfMeasure = "each",
            ManufacturerName = "OEM Brake",
            ManufacturerPartNumber = "BRKPAD001",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        part.ManufacturerAliases.Add(new PartManufacturerAlias
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            AliasKey = "ALT-1",
            ManufacturerName = "OEM Brake",
            ManufacturerPartNumber = "BRK-PAD-ALT",
            CreatedAt = now,
            UpdatedAt = now,
        });

        var supplierLink = new PartSupplierLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            SupplierId = supplier.Id,
            SupplierPartNumber = "ACME-BRKPAD-SKU",
            IsPreferred = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var purchaseRequest = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestKey = "PR-BRKPAD",
            Title = "Brake pad replenishment",
            Status = PurchaseRequestStatuses.Submitted,
            SupplierId = supplier.Id,
            RequestedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var purchaseOrder = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderKey = "PO-BRKPAD",
            Title = "Brake pad purchase order",
            Status = PurchaseOrderStatuses.Issued,
            PurchaseRequestId = purchaseRequest.Id,
            SupplierId = supplier.Id,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            IssuedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Suppliers.Add(supplier);
        db.Parts.Add(part);
        db.PartSupplierLinks.Add(supplierLink);
        db.PurchaseRequests.Add(purchaseRequest);
        db.PurchaseOrders.Add(purchaseOrder);
        await db.SaveChangesAsync();
        return (part.Id, supplier.Id);
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
            $"{productKey}-forgiving-search-test",
            $"{productKey} forgiving search test",
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


