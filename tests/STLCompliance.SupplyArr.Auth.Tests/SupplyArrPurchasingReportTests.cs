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

public sealed class SupplyArrPurchasingReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _serviceToken = null!;
    private string _userToken = null!;
    private Guid _purchaseRequestId;
    private Guid _purchaseOrderId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PurchasingReportNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"PurchasingReportSupplyArr-{Guid.NewGuid():N}";

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
        (_purchaseRequestId, _purchaseOrderId) = await SeedPurchasingPipelineAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Purchasing_summary_returns_pipeline_documents()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/purchasing/summary", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<PurchasingReportSummaryResponse>();
        Assert.NotNull(summary);
        Assert.Contains(summary!.Documents, x =>
            x.DocumentType == "purchase_request" && x.DocumentId == _purchaseRequestId);
        Assert.Contains(summary.Documents, x =>
            x.DocumentType == "purchase_order" && x.DocumentId == _purchaseOrderId);
        Assert.Equal(1, summary.Totals.IssuedPurchaseOrderCount);
        Assert.Equal(1, summary.Totals.PostedReceivingReceiptCount);
    }

    [Fact]
    public async Task Purchasing_purchase_request_detail_returns_lines()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/reports/purchasing/purchase-requests/{_purchaseRequestId}",
                _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<PurchasingPurchaseRequestDetailResponse>();
        Assert.NotNull(detail);
        Assert.Equal(_purchaseOrderId, detail!.LinkedPurchaseOrderId);
        Assert.NotEmpty(detail.Lines);
    }

    [Fact]
    public async Task Purchasing_purchase_order_detail_returns_receiving()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/reports/purchasing/purchase-orders/{_purchaseOrderId}",
                _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<PurchasingPurchaseOrderDetailResponse>();
        Assert.NotNull(detail);
        Assert.NotEmpty(detail!.Lines);
        Assert.NotEmpty(detail.ReceivingReceipts);
    }

    [Fact]
    public async Task Purchasing_export_returns_csv()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/purchasing/summary/export", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("documentType,documentKey", csv, StringComparison.Ordinal);
        Assert.Contains("PR-PURCH", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Purchasing_summary_denied_without_auth()
    {
        var response = await _supplyarrClient.GetAsync("/api/reports/purchasing/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<(Guid PurchaseRequestId, Guid PurchaseOrderId)> SeedPurchasingPipelineAsync()
    {
        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var vendor = new ExternalParty
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartyKey = "V-PURCH",
            PartyType = "vendor",
            DisplayName = "Purchasing Vendor",
            LegalName = "Purchasing Vendor",
            ApprovalStatus = "approved",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = "PART-PURCH",
            DisplayName = "Purchasing Part",
            Description = string.Empty,
            CategoryKey = "general",
            UnitOfMeasure = "each",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var purchaseRequest = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestKey = "PR-PURCH",
            Title = "Purchasing report PR",
            Status = PurchaseRequestStatuses.Submitted,
            VendorPartyId = vendor.Id,
            RequestedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        purchaseRequest.Lines.Add(new PurchaseRequestLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PurchaseRequestId = purchaseRequest.Id,
            LineNumber = 1,
            PartId = part.Id,
            QuantityRequested = 10m,
            CreatedAt = now,
            UpdatedAt = now,
        });

        var purchaseOrder = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderKey = "PO-PURCH",
            Title = "Purchasing report PO",
            Status = PurchaseOrderStatuses.Issued,
            PurchaseRequestId = purchaseRequest.Id,
            VendorPartyId = vendor.Id,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            IssuedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        purchaseOrder.Lines.Add(new PurchaseOrderLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PurchaseOrderId = purchaseOrder.Id,
            LineNumber = 1,
            PartId = part.Id,
            QuantityOrdered = 10m,
            QuantityReceived = 4m,
            CreatedAt = now,
            UpdatedAt = now,
        });

        var receipt = new ReceivingReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReceiptKey = "RCV-PURCH",
            PurchaseOrderId = purchaseOrder.Id,
            InventoryBinId = Guid.NewGuid(),
            Status = ReceivingReceiptStatuses.Posted,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            PostedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ExternalParties.Add(vendor);
        db.Parts.Add(part);
        db.PurchaseRequests.Add(purchaseRequest);
        db.PurchaseOrders.Add(purchaseOrder);
        db.ReceivingReceipts.Add(receipt);
        await db.SaveChangesAsync();
        return (purchaseRequest.Id, purchaseOrder.Id);
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
            $"{productKey}-purchasing-report-test",
            $"{productKey} purchasing report test",
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
