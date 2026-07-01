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
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrSupplierReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _serviceToken = null!;
    private string _userToken = null!;
    private Guid _supplierUnitId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"SupplierReportNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplierReportSupplyArr-{Guid.NewGuid():N}";

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
        _supplierUnitId = await SeedSupplierWithProcurementActivityAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Supplier_report_summary_returns_aggregates()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/suppliers/summary", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var suppliers = GetPropertyIgnoreCase(payload.RootElement, "suppliers");
        Assert.True(suppliers.GetArrayLength() > 0);

        var supplier = suppliers.EnumerateArray()
            .Single(x => GetPropertyIgnoreCase(x, "supplierId").GetGuid() == _supplierUnitId);
        Assert.Equal("sub_unit", GetPropertyIgnoreCase(supplier, "supplierUnitKind").GetString());
        Assert.Equal("Report Supplier", GetPropertyIgnoreCase(supplier, "parentSupplierDisplayName").GetString());
        Assert.Equal(1, GetPropertyIgnoreCase(supplier, "partSupplierLinkCount").GetInt32());
        Assert.Equal(1, GetPropertyIgnoreCase(supplier, "openPurchaseRequestCount").GetInt32());
        Assert.Equal(1, GetPropertyIgnoreCase(supplier, "issuedPurchaseOrderCount").GetInt32());
        Assert.Equal(4, GetPropertyIgnoreCase(supplier, "averageLeadTimeDays").GetInt32());
        Assert.Equal(100, GetPropertyIgnoreCase(supplier, "onTimeDeliveryRate").GetInt32());
    }

    [Fact]
    public async Task Supplier_report_detail_returns_recent_documents()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/reports/suppliers/{_supplierUnitId}", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<SupplierReportDetailResponse>();
        Assert.NotNull(detail);
        Assert.Equal(_supplierUnitId, detail!.Summary.SupplierId);
        Assert.Equal("sub_unit", detail.Summary.SupplierUnitKind);
        Assert.Equal(4, detail.Summary.AverageLeadTimeDays);
        Assert.Equal(100, detail.Summary.OnTimeDeliveryRate);
        Assert.NotEmpty(detail.RecentPurchaseOrders);
        Assert.NotEmpty(detail.PartLinks);
        Assert.Equal("VN-001", detail.PartLinks[0].SupplierPartNumber);
    }

    [Fact]
    public async Task Supplier_report_export_returns_csv()
    {
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/suppliers/summary/export", _userToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("supplierKey,supplierDisplayName,parentSupplierDisplayName,supplierUnitKind,supplierServiceTypes", csv, StringComparison.Ordinal);
        Assert.Contains("averageLeadTimeDays", csv, StringComparison.Ordinal);
        Assert.Contains("SUPPLIER-REPORT-COUNTER", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Supplier_report_summary_denied_without_auth()
    {
        var response = await _supplyarrClient.GetAsync("/api/reports/suppliers/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<Guid> SeedSupplierWithProcurementActivityAsync()
    {
        await using var scope = _supplyarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var supplierIdentity = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierKey = "SUPPLIER-REPORT",
            
            UnitKind = "identity",
            DisplayName = "Report Supplier",
            LegalName = "Report Supplier LLC",
            ApprovalStatus = "approved",
            Status = "active",
            ServiceTypesJson = "[\"parts\",\"maintenance\"]",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var supplierUnit = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierKey = "SUPPLIER-REPORT-COUNTER",
            
            ParentSupplierId = supplierIdentity.Id,
            UnitKind = "sub_unit",
            DisplayName = "North Yard Counter",
            LegalName = "Report Supplier North Yard Counter",
            ApprovalStatus = "approved",
            Status = "active",
            ServiceTypesJson = "[\"parts\",\"maintenance\"]",
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

        var link = new PartSupplierLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            SupplierId = supplierUnit.Id,
            SupplierPartNumber = "VN-001",
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
            SupplierId = supplierUnit.Id,
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
            SupplierId = supplierUnit.Id,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            IssuedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var purchaseOrderLine = new PurchaseOrderLine
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
        };
        purchaseOrder.Lines.Add(purchaseOrderLine);

        var inventoryLocation = new InventoryLocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LocationKey = "LOC-REPORT",
            Name = "Report Location",
            LocationType = "warehouse",
            AddressLine = "1 Report Way",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var inventoryBin = new InventoryBin
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InventoryLocationId = inventoryLocation.Id,
            BinKey = "BIN-REPORT",
            Name = "Report Bin",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var leadTimeSnapshot = new PartSupplierLeadTimeSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartSupplierLinkId = link.Id,
            SnapshotKey = "LTS-REPORT",
            LeadTimeDays = 4,
            EffectiveFrom = now.AddDays(-1),
            EffectiveTo = null,
            Source = SnapshotSources.Manual,
            Notes = "Report lead time snapshot",
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Suppliers.AddRange(supplierIdentity, supplierUnit);
        db.Parts.Add(part);
        db.PartSupplierLinks.Add(link);
        db.PurchaseRequests.Add(purchaseRequest);
        db.PurchaseOrders.Add(purchaseOrder);
        db.InventoryLocations.Add(inventoryLocation);
        db.InventoryBins.Add(inventoryBin);
        db.PartSupplierLeadTimeSnapshots.Add(leadTimeSnapshot);

        var receipt = new ReceivingReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReceiptKey = "RCV-REPORT",
            PurchaseOrderId = purchaseOrder.Id,
            InventoryBinId = inventoryBin.Id,
            Status = ReceivingReceiptStatuses.Posted,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            PostedAt = now.AddDays(3),
            CreatedAt = now,
            UpdatedAt = now,
        };
        receipt.Lines.Add(new ReceivingReceiptLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReceivingReceiptId = receipt.Id,
            PurchaseOrderLineId = purchaseOrderLine.Id,
            PartId = part.Id,
            LineNumber = 1,
            QuantityExpected = 5m,
            QuantityReceived = 5m,
            Condition = "good",
            SerialLotNumbersJson = "[]",
            CreatedAt = now,
            UpdatedAt = now,
        });
        db.ReceivingReceipts.Add(receipt);
        await db.SaveChangesAsync();
        return supplierUnit.Id;
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
            $"{productKey}-supplier-report-test",
            $"{productKey} supplier report test",
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

    private static JsonElement GetPropertyIgnoreCase(JsonElement element, string propertyName)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        throw new KeyNotFoundException($"Property '{propertyName}' was not present in the JSON payload.");
    }

}



