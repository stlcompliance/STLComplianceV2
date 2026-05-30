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
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrStockReservationTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _serviceToken = null!;
    private string _userToken = null!;
    private Guid _partId;
    private Guid _binId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"StockReservationNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"StockReservationSupplyArr-{Guid.NewGuid():N}";

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
        (_partId, _binId) = await SeedInventoryAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Create_reservation_increments_quantity_reserved_on_stock_level()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                "/api/inventory/reservations",
                _userToken,
                new CreateStockReservationRequest(
                    "RSV-001",
                    _partId,
                    _binId,
                    3m,
                    "manual",
                    null,
                    "Hold for work order")));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var reservation = await response.Content.ReadFromJsonAsync<StockReservationResponse>();
        Assert.NotNull(reservation);
        Assert.Equal("active", reservation!.Status);
        Assert.Equal(3m, reservation.QuantityReserved);

        var stockResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/inventory/stock?partId={_partId}", _userToken));
        var stockLevels = await stockResponse.Content.ReadFromJsonAsync<PartStockLevelResponse[]>();
        Assert.NotNull(stockLevels);
        var stock = stockLevels!.Single();
        Assert.Equal(5m, stock.QuantityReserved);
        Assert.Equal(5m, stock.QuantityAvailable);
    }

    [Fact]
    public async Task Release_reservation_restores_available_quantity()
    {
        var createResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                "/api/inventory/reservations",
                _userToken,
                new CreateStockReservationRequest(
                    "RSV-RELEASE",
                    _partId,
                    _binId,
                    2m,
                    "manual",
                    null,
                    null)));
        var created = await createResponse.Content.ReadFromJsonAsync<StockReservationResponse>();
        Assert.NotNull(created);

        var releaseResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/inventory/reservations/{created!.ReservationId}/release",
                _userToken,
                new ReleaseStockReservationRequest("No longer needed")));
        Assert.Equal(HttpStatusCode.OK, releaseResponse.StatusCode);

        var stockResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/inventory/stock?partId={_partId}", _userToken));
        var stockLevels = await stockResponse.Content.ReadFromJsonAsync<PartStockLevelResponse[]>();
        Assert.NotNull(stockLevels);
        var stock = stockLevels!.Single();
        Assert.Equal(2m, stock.QuantityReserved);
        Assert.Equal(8m, stock.QuantityAvailable);
    }

    [Fact]
    public async Task Fulfill_reservation_decrements_on_hand_and_reserved()
    {
        var createResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                "/api/inventory/reservations",
                _userToken,
                new CreateStockReservationRequest(
                    "RSV-FULFILL",
                    _partId,
                    _binId,
                    4m,
                    "manual",
                    null,
                    null)));
        var created = await createResponse.Content.ReadFromJsonAsync<StockReservationResponse>();
        Assert.NotNull(created);

        var fulfillResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/inventory/reservations/{created!.ReservationId}/fulfill",
                _userToken));
        Assert.Equal(HttpStatusCode.OK, fulfillResponse.StatusCode);

        var stockResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/inventory/stock?partId={_partId}", _userToken));
        var stockLevels = await stockResponse.Content.ReadFromJsonAsync<PartStockLevelResponse[]>();
        Assert.NotNull(stockLevels);
        var stock = stockLevels!.Single();
        Assert.Equal(6m, stock.QuantityOnHand);
        Assert.Equal(2m, stock.QuantityReserved);
        Assert.Equal(4m, stock.QuantityAvailable);
    }

    private async Task<(Guid PartId, Guid BinId)> SeedInventoryAsync()
    {
        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var location = new InventoryLocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LocationKey = "WH-RSV",
            Name = "Reservation Warehouse",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var bin = new InventoryBin
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InventoryLocationId = location.Id,
            BinKey = "BIN-RSV",
            Name = "Reservation Bin",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = "PART-RSV",
            DisplayName = "Reservation Test Part",
            Description = string.Empty,
            CategoryKey = "general",
            UnitOfMeasure = "each",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var stock = new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            InventoryBinId = bin.Id,
            QuantityOnHand = 10m,
            QuantityReserved = 2m,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.InventoryLocations.Add(location);
        db.InventoryBins.Add(bin);
        db.Parts.Add(part);
        db.PartStockLevels.Add(stock);
        await db.SaveChangesAsync();
        return (part.Id, bin.Id);
    }

    private static HttpRequestMessage Authorized(
        HttpMethod method,
        string path,
        string token,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token) =>
        Authorized(method, path, token, null);

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
            $"{productKey}-stock-reservation-test",
            $"{productKey} stock reservation test",
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
