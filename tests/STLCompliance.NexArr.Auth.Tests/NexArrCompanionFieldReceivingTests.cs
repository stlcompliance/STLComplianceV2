using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrCompanionFieldReceivingTests : IAsyncLifetime
{
    private readonly Guid _receivingReceiptId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private readonly Guid _lineId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpRequestMessage? _capturedLineRequest;
    private HttpRequestMessage? _capturedPostRequest;

    public async Task InitializeAsync()
    {
        _capturedLineRequest = null;
        _capturedPostRequest = null;
        var dbName = $"NexArrCompanionReceiving-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("SupplyArr__BaseUrl", "http://supplyarr.test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddHttpClient(nameof(CompanionProductClient))
                    .ConfigurePrimaryHttpMessageHandler(() => new SupplyArrReceivingCaptureHandler(
                        _receivingReceiptId,
                        _lineId,
                        request => _capturedLineRequest = request,
                        request => _capturedPostRequest = request));
            });
        });

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
        await EnsureCompanionEntitlementAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Get_companion_field_receiving_detail_proxies_to_supplyarr()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(
            HttpMethod.Get,
            $"/api/companion/field-tasks/receiving?taskKey=supplyarr:receiving:{_receivingReceiptId:D}",
            token);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<CompanionFieldReceivingDetailResponse>())!;
        Assert.Equal("supplyarr", payload.ProductKey);
        Assert.Equal(_receivingReceiptId, payload.ReceivingReceiptId);
        Assert.Equal("RCPT-1001", payload.ReceiptKey);
        Assert.Single(payload.Lines);
    }

    [Fact]
    public async Task Update_companion_field_receiving_line_proxies_to_supplyarr()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/companion/field-tasks/receiving/line", token);
        request.Content = JsonContent.Create(new UpdateCompanionFieldReceivingLineRequest(
            $"supplyarr:receiving:{_receivingReceiptId:D}",
            _lineId,
            4m));

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<CompanionFieldReceivingLineResponse>())!;
        Assert.Equal(4m, payload.QuantityReceived);

        Assert.NotNull(_capturedLineRequest);
        Assert.Equal(HttpMethod.Put, _capturedLineRequest!.Method);
        Assert.Contains(
            $"/api/receiving/{_receivingReceiptId:D}/lines/{_lineId:D}",
            _capturedLineRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task Post_companion_field_receiving_proxies_to_supplyarr()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/companion/field-tasks/receiving/post", token);
        request.Content = JsonContent.Create(new PostCompanionFieldReceivingRequest(
            $"supplyarr:receiving:{_receivingReceiptId:D}"));

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = (await response.Content.ReadFromJsonAsync<CompanionFieldReceivingPostResponse>())!;
        Assert.Equal("posted", payload.Status);

        Assert.NotNull(_capturedPostRequest);
        Assert.Equal(HttpMethod.Post, _capturedPostRequest!.Method);
        Assert.Contains($"/api/receiving/{_receivingReceiptId:D}/post", _capturedPostRequest.RequestUri!.AbsolutePath);
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task EnsureCompanionEntitlementAsync(NexArrDbContext db)
    {
        if (await db.Entitlements.AnyAsync(e =>
                e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "companion"))
        {
            return;
        }

        db.Entitlements.Add(new TenantProductEntitlement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ProductKey = "companion",
            Status = EntitlementStatuses.Active,
            GrantedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
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

    private sealed class SupplyArrReceivingCaptureHandler(
        Guid receivingReceiptId,
        Guid lineId,
        Action<HttpRequestMessage> onLineRequest,
        Action<HttpRequestMessage> onPostRequest) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/api/field-inbox", StringComparison.OrdinalIgnoreCase) == true)
            {
                var inbox = new FieldInboxResponse(
                    new FieldInboxSummary(1, 0, new Dictionary<string, int> { ["supplyarr"] = 1 }),
                    [
                        new FieldInboxTaskItem(
                            $"supplyarr:receiving:{receivingReceiptId:D}",
                            "supplyarr",
                            "receiving",
                            "RCPT-1001",
                            "PO-5001",
                            "draft",
                            null,
                            DateTimeOffset.UtcNow,
                            DateTimeOffset.UtcNow,
                            $"/receiving/{receivingReceiptId:D}",
                            null,
                            $"http://supplyarr.test/receiving/{receivingReceiptId:D}"),
                    ]);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(inbox),
                };
            }

            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.Equals($"/api/receiving/{receivingReceiptId:D}", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(BuildReceiptJson("draft", 0m), Encoding.UTF8, "application/json"),
                };
            }

            if (request.Method == HttpMethod.Put
                && request.RequestUri?.AbsolutePath.Equals(
                    $"/api/receiving/{receivingReceiptId:D}/lines/{lineId:D}",
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                onLineRequest(request);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(BuildReceiptJson("draft", 4m), Encoding.UTF8, "application/json"),
                };
            }

            if (request.Method == HttpMethod.Post
                && request.RequestUri?.AbsolutePath.Equals(
                    $"/api/receiving/{receivingReceiptId:D}/post",
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                onPostRequest(request);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(BuildReceiptJson("posted", 4m, posted: true), Encoding.UTF8, "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private string BuildReceiptJson(string status, decimal quantityReceived, bool posted = false)
        {
            var payload = new
            {
                receivingReceiptId,
                receiptKey = "RCPT-1001",
                status,
                purchaseOrderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                purchaseOrderKey = "PO-5001",
                inventoryBinId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                binKey = "BIN-A1",
                binName = "Main bin",
                inventoryLocationId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                locationKey = "WH1",
                locationName = "Warehouse 1",
                notes = "Dock delivery",
                createdByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                postedAt = posted ? DateTimeOffset.UtcNow : (DateTimeOffset?)null,
                postedByUserId = posted ? Guid.Parse("11111111-1111-1111-1111-111111111111") : (Guid?)null,
                lines = new[]
                {
                    new
                    {
                        lineId,
                        lineNumber = 1,
                        purchaseOrderLineId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                        partId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                        partKey = "FLT-001",
                        partDisplayName = "Oil filter",
                        quantityExpected = 4m,
                        quantityReceived,
                        quantityOrdered = 4m,
                        quantityPreviouslyReceived = 0m,
                        quantityRemainingOnOrder = 4m,
                        exceptions = Array.Empty<object>(),
                        createdAt = DateTimeOffset.UtcNow,
                        updatedAt = DateTimeOffset.UtcNow,
                    },
                },
                exceptions = Array.Empty<object>(),
                createdAt = DateTimeOffset.UtcNow,
                updatedAt = DateTimeOffset.UtcNow,
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}
