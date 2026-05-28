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

public sealed class SupplyArrVendorReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _serviceToken = null!;
    private string _userToken = null!;
    private Guid _vendorPartyId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"VendorReportNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"VendorReportSupplyArr-{Guid.NewGuid():N}";

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
        _vendorPartyId = await SeedVendorWithProcurementActivityAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Vendor_report_summary_returns_aggregates()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/vendors/summary", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<VendorReportSummaryResponse>();
        Assert.NotNull(summary);
        Assert.NotEmpty(summary!.Vendors);

        var vendor = summary.Vendors.Single(x => x.VendorPartyId == _vendorPartyId);
        Assert.Equal(1, vendor.PartVendorLinkCount);
        Assert.Equal(1, vendor.OpenPurchaseRequestCount);
        Assert.Equal(1, vendor.IssuedPurchaseOrderCount);
    }

    [Fact]
    public async Task Vendor_report_detail_returns_recent_documents()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/reports/vendors/{_vendorPartyId}", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<VendorReportDetailResponse>();
        Assert.NotNull(detail);
        Assert.Equal(_vendorPartyId, detail!.Summary.VendorPartyId);
        Assert.NotEmpty(detail.RecentPurchaseOrders);
        Assert.NotEmpty(detail.PartLinks);
    }

    [Fact]
    public async Task Vendor_report_export_returns_csv()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/vendors/summary/export", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("partyKey,displayName", csv, StringComparison.Ordinal);
        Assert.Contains("VENDOR-REPORT", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Vendor_report_summary_denied_without_auth()
    {
        var response = await _supplyarrClient.GetAsync("/api/reports/vendors/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<Guid> SeedVendorWithProcurementActivityAsync()
    {
        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var vendor = new ExternalParty
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartyKey = "VENDOR-REPORT",
            PartyType = "vendor",
            DisplayName = "Report Vendor",
            LegalName = "Report Vendor LLC",
            ApprovalStatus = "approved",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = "PART-REPORT",
            DisplayName = "Report Part",
            Description = string.Empty,
            UnitOfMeasure = "each",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var link = new PartVendorLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            ExternalPartyId = vendor.Id,
            VendorPartNumber = "VN-001",
            IsPreferred = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var purchaseRequest = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestKey = "PR-REPORT",
            Title = "Report PR",
            Status = PurchaseRequestStatuses.Submitted,
            VendorPartyId = vendor.Id,
            RequestedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var purchaseOrder = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderKey = "PO-REPORT",
            Title = "Report PO",
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
            QuantityOrdered = 5m,
            QuantityReceived = 2m,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.ExternalParties.Add(vendor);
        db.Parts.Add(part);
        db.PartVendorLinks.Add(link);
        db.PurchaseRequests.Add(purchaseRequest);
        db.PurchaseOrders.Add(purchaseOrder);
        await db.SaveChangesAsync();
        return vendor.Id;
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
            $"{productKey}-vendor-report-test",
            $"{productKey} vendor report test",
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
        var request = Authorized(HttpMethod.Post, "/api/launch/handoff", token);
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
