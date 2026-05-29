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
using SupplyArr.Api.Endpoints;
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrSupplyReadinessCheckTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _maintainarrServiceToken = null!;
    private string _referenceServiceToken = null!;

    private Guid _partId;
    private Guid _vendorId;
    private Guid _purchaseOrderId;
    private Guid _receivingReceiptId;
    private Guid _warrantyClaimId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"SupplyReadinessCheckNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplyReadinessCheckSupplyArr-{Guid.NewGuid():N}";

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

        var adminToken = await LoginNexArrAsync();
        _maintainarrServiceToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            ["supplyarr"],
            IntegrationEndpoints.SupplyReadinessReadActionScope);
        _referenceServiceToken = await IssueServiceTokenAsync(
            adminToken,
            "routarr",
            ["supplyarr"],
            IntegrationEndpoints.SupplyReferenceReadActionScope);

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
        (_partId, _vendorId, _purchaseOrderId, _receivingReceiptId, _warrantyClaimId) =
            await SeedReadinessCheckScenarioAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Part_supply_readiness_returns_stockout_blocker()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/supply-readiness/parts/{_partId}", buyerToken));
        response.EnsureSuccessStatusCode();

        var readiness = (await response.Content.ReadFromJsonAsync<PartSupplyReadinessResponse>())!;
        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Contains(readiness.Blockers, x => x.ReasonCode == SupplyReadinessReasonCodes.PartStockout);
    }

    [Fact]
    public async Task Vendor_supply_readiness_returns_approval_blocker()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/supply-readiness/vendors/{_vendorId}", buyerToken));
        response.EnsureSuccessStatusCode();

        var readiness = (await response.Content.ReadFromJsonAsync<VendorSupplyReadinessResponse>())!;
        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Contains(readiness.Blockers, x => x.ReasonCode == SupplyReadinessReasonCodes.VendorApprovalRestricted);
    }

    [Fact]
    public async Task Procurement_path_readiness_combines_part_vendor_and_link_blockers()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/supply-readiness/procurement-path?partId={_partId}&externalPartyId={_vendorId}&quantity=5",
                buyerToken));
        response.EnsureSuccessStatusCode();

        var readiness = (await response.Content.ReadFromJsonAsync<ProcurementPathReadinessResponse>())!;
        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Contains(readiness.Blockers, x => x.ReasonCode == SupplyReadinessReasonCodes.NoVendorPartLink);
        Assert.Contains(readiness.Blockers, x => x.ReasonCode == SupplyReadinessReasonCodes.InsufficientAvailableQuantity);
    }

    [Fact]
    public async Task Integration_part_supply_readiness_allows_maintainarr_service_token()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/integrations/part-supply-readiness?tenantId={PlatformSeeder.DemoTenantId}&partId={_partId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _maintainarrServiceToken);

        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var readiness = (await response.Content.ReadFromJsonAsync<PartSupplyReadinessResponse>())!;
        Assert.Equal("not_ready", readiness.ReadinessStatus);
    }

    [Fact]
    public async Task Integration_part_supply_readiness_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.GetAsync(
            $"/api/integrations/part-supply-readiness?tenantId={PlatformSeeder.DemoTenantId}&partId={_partId}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Integration_reference_resolution_resolves_stable_part_and_vendor_references()
    {
        var part = await ResolveReferenceAsync(
            $"/api/integrations/references/by-key?tenantId={PlatformSeeder.DemoTenantId}&referenceType=item&referenceKey=CHECK-PART");

        Assert.Equal(SupplyReferenceTypes.Part, part.ReferenceType);
        Assert.Equal(_partId, part.SupplyArrReferenceId);
        Assert.Equal("check-part", part.DisplayCode);
        Assert.Equal("/api/parts/" + _partId, part.ApiPath);

        var vendor = await ResolveReferenceAsync(
            $"/api/integrations/references/vendor/{_vendorId}?tenantId={PlatformSeeder.DemoTenantId}");

        Assert.Equal(SupplyReferenceTypes.ExternalParty, vendor.ReferenceType);
        Assert.Equal(_vendorId, vendor.SupplyArrReferenceId);
        Assert.Equal("vendor-check", vendor.DisplayCode);
        Assert.Equal("vendor", vendor.Metadata["partyType"]);
    }

    [Fact]
    public async Task Integration_reference_resolution_resolves_procurement_receiving_and_warranty_references()
    {
        var purchaseOrder = await ResolveReferenceAsync(
            $"/api/integrations/references/purchase-order/{_purchaseOrderId}?tenantId={PlatformSeeder.DemoTenantId}");

        Assert.Equal(SupplyReferenceTypes.PurchaseOrder, purchaseOrder.ReferenceType);
        Assert.Equal("po-check", purchaseOrder.DisplayCode);
        Assert.Equal(_vendorId.ToString(), purchaseOrder.Metadata["vendorPartyId"]);

        var receipt = await ResolveReferenceAsync(
            $"/api/integrations/references/by-key?tenantId={PlatformSeeder.DemoTenantId}&referenceType=receipt&referenceKey=RCV-CHECK");

        Assert.Equal(SupplyReferenceTypes.ReceivingReceipt, receipt.ReferenceType);
        Assert.Equal(_receivingReceiptId, receipt.SupplyArrReferenceId);
        Assert.Equal(_purchaseOrderId.ToString(), receipt.Metadata["purchaseOrderId"]);

        var warranty = await ResolveReferenceAsync(
            $"/api/integrations/references/by-key?tenantId={PlatformSeeder.DemoTenantId}&referenceType=warranty&referenceKey=WCL-CHECK");

        Assert.Equal(SupplyReferenceTypes.WarrantyClaim, warranty.ReferenceType);
        Assert.Equal(_warrantyClaimId, warranty.SupplyArrReferenceId);
        Assert.Equal(_partId.ToString(), warranty.Metadata["partId"]);
    }

    [Fact]
    public async Task Integration_reference_resolution_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.GetAsync(
            $"/api/integrations/references/part/{_partId}?tenantId={PlatformSeeder.DemoTenantId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<SupplyReferenceResolutionResponse> ResolveReferenceAsync(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _referenceServiceToken);

        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplyReferenceResolutionResponse>())!;
    }

    private async Task<(
        Guid PartId,
        Guid VendorId,
        Guid PurchaseOrderId,
        Guid ReceivingReceiptId,
        Guid WarrantyClaimId)> SeedReadinessCheckScenarioAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var partId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();
        var purchaseRequestId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var receivingReceiptId = Guid.NewGuid();
        var warrantyClaimId = Guid.NewGuid();

        db.Parts.Add(new Part
        {
            Id = partId,
            TenantId = tenantId,
            PartKey = "check-part",
            DisplayName = "Readiness check part",
            Status = "active",
            ReorderPoint = 10m,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PartStockLevels.Add(new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = partId,
            InventoryBinId = Guid.NewGuid(),
            QuantityOnHand = 0m,
            QuantityReserved = 0m,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.ExternalParties.Add(new ExternalParty
        {
            Id = vendorId,
            TenantId = tenantId,
            PartyKey = "vendor-check",
            PartyType = "vendor",
            DisplayName = "Restricted vendor",
            LegalName = "Restricted vendor LLC",
            ApprovalStatus = "restricted",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PurchaseRequests.Add(new PurchaseRequest
        {
            Id = purchaseRequestId,
            TenantId = tenantId,
            RequestKey = "pr-check",
            Title = "Readiness check purchase request",
            Status = PurchaseRequestStatuses.Submitted,
            VendorPartyId = vendorId,
            RequestedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PurchaseOrders.Add(new PurchaseOrder
        {
            Id = purchaseOrderId,
            TenantId = tenantId,
            OrderKey = "po-check",
            Title = "Readiness check purchase order",
            Status = PurchaseOrderStatuses.Issued,
            PurchaseRequestId = purchaseRequestId,
            VendorPartyId = vendorId,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            IssuedAt = now,
            IssuedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.ReceivingReceipts.Add(new ReceivingReceipt
        {
            Id = receivingReceiptId,
            TenantId = tenantId,
            ReceiptKey = "rcv-check",
            PurchaseOrderId = purchaseOrderId,
            InventoryBinId = Guid.NewGuid(),
            Status = ReceivingReceiptStatuses.Posted,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            PostedAt = now,
            PostedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.WarrantyClaims.Add(new WarrantyClaim
        {
            Id = warrantyClaimId,
            TenantId = tenantId,
            ClaimKey = "WCL-CHECK",
            Status = WarrantyClaimStatuses.Submitted,
            ClaimType = WarrantyClaimTypes.Defective,
            VendorPartyId = vendorId,
            PartId = partId,
            PurchaseOrderId = purchaseOrderId,
            ReceivingReceiptId = receivingReceiptId,
            QuantityClaimed = 1m,
            ProblemDescription = "Reference resolution warranty claim",
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            SubmittedAt = now,
            SubmittedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
        return (partId, vendorId, purchaseOrderId, receivingReceiptId, warrantyClaimId);
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

    private async Task<string> LoginNexArrAsync()
    {
        var loginResponse = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        loginResponse.EnsureSuccessStatusCode();
        var login = (await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return login.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-readiness-check-test",
            $"{sourceProduct} readiness check test",
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
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!.AccessToken;
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
