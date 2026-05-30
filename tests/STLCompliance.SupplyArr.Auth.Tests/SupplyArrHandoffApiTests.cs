using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupplyArrRedeemRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using SupplyArrMeResponse = SupplyArr.Api.Contracts.SupplyArrMeResponse;
using ExternalPartyResponse = SupplyArr.Api.Contracts.ExternalPartyResponse;
using CreateExternalPartyRequest = SupplyArr.Api.Contracts.CreateExternalPartyRequest;
using CreateTypedExternalPartyRequest = SupplyArr.Api.Contracts.CreateTypedExternalPartyRequest;
using CreatePartyContactRequest = SupplyArr.Api.Contracts.CreatePartyContactRequest;
using UpdateExternalPartyApprovalStatusRequest = SupplyArr.Api.Contracts.UpdateExternalPartyApprovalStatusRequest;
using PartCatalogResponse = SupplyArr.Api.Contracts.PartCatalogResponse;
using CreatePartCatalogRequest = SupplyArr.Api.Contracts.CreatePartCatalogRequest;
using PartResponse = SupplyArr.Api.Contracts.PartResponse;
using CreatePartRequest = SupplyArr.Api.Contracts.CreatePartRequest;
using CreatePartVendorLinkRequest = SupplyArr.Api.Contracts.CreatePartVendorLinkRequest;
using PartVendorLinkResponse = SupplyArr.Api.Contracts.PartVendorLinkResponse;
using InventoryLocationResponse = SupplyArr.Api.Contracts.InventoryLocationResponse;
using CreateInventoryLocationRequest = SupplyArr.Api.Contracts.CreateInventoryLocationRequest;
using InventoryBinResponse = SupplyArr.Api.Contracts.InventoryBinResponse;
using CreateInventoryBinRequest = SupplyArr.Api.Contracts.CreateInventoryBinRequest;
using PartStockLevelResponse = SupplyArr.Api.Contracts.PartStockLevelResponse;
using UpsertPartStockLevelRequest = SupplyArr.Api.Contracts.UpsertPartStockLevelRequest;
using StockReservationResponse = SupplyArr.Api.Contracts.StockReservationResponse;
using CreateStockReservationRequest = SupplyArr.Api.Contracts.CreateStockReservationRequest;
using ReleaseStockReservationRequest = SupplyArr.Api.Contracts.ReleaseStockReservationRequest;
using RfqResponse = SupplyArr.Api.Contracts.RfqResponse;
using WarrantyClaimResponse = SupplyArr.Api.Contracts.WarrantyClaimResponse;
using PurchaseRequestResponse = SupplyArr.Api.Contracts.PurchaseRequestResponse;
using CreatePurchaseRequestRequest = SupplyArr.Api.Contracts.CreatePurchaseRequestRequest;
using CreatePurchaseRequestLineRequest = SupplyArr.Api.Contracts.CreatePurchaseRequestLineRequest;
using RejectPurchaseRequestRequest = SupplyArr.Api.Contracts.RejectPurchaseRequestRequest;
using PurchaseOrderResponse = SupplyArr.Api.Contracts.PurchaseOrderResponse;
using CreatePurchaseOrderFromPurchaseRequestRequest = SupplyArr.Api.Contracts.CreatePurchaseOrderFromPurchaseRequestRequest;
using ReceivingReceiptResponse = SupplyArr.Api.Contracts.ReceivingReceiptResponse;
using CreateReceivingReceiptFromPurchaseOrderRequest = SupplyArr.Api.Contracts.CreateReceivingReceiptFromPurchaseOrderRequest;
using CreateReceivingExceptionRequest = SupplyArr.Api.Contracts.CreateReceivingExceptionRequest;
using UpdateReceivingReceiptLineRequest = SupplyArr.Api.Contracts.UpdateReceivingReceiptLineRequest;
using BackorderResponse = SupplyArr.Api.Contracts.BackorderResponse;
using CreateBackorderFromPurchaseOrderLineRequest = SupplyArr.Api.Contracts.CreateBackorderFromPurchaseOrderLineRequest;
using CancelBackorderRequest = SupplyArr.Api.Contracts.CancelBackorderRequest;
using VendorReturnResponse = SupplyArr.Api.Contracts.VendorReturnResponse;
using CreateVendorReturnFromStockRequest = SupplyArr.Api.Contracts.CreateVendorReturnFromStockRequest;
using CreateVendorReturnFromStockLineRequest = SupplyArr.Api.Contracts.CreateVendorReturnFromStockLineRequest;
using CreateVendorReturnFromPurchaseOrderLineRequest = SupplyArr.Api.Contracts.CreateVendorReturnFromPurchaseOrderLineRequest;
using CancelVendorReturnRequest = SupplyArr.Api.Contracts.CancelVendorReturnRequest;
using PricingSnapshotResponse = SupplyArr.Api.Contracts.PricingSnapshotResponse;
using CreatePricingSnapshotRequest = SupplyArr.Api.Contracts.CreatePricingSnapshotRequest;
using LeadTimeSnapshotResponse = SupplyArr.Api.Contracts.LeadTimeSnapshotResponse;
using CreateLeadTimeSnapshotRequest = SupplyArr.Api.Contracts.CreateLeadTimeSnapshotRequest;
using AvailabilitySnapshotResponse = SupplyArr.Api.Contracts.AvailabilitySnapshotResponse;
using CreateAvailabilitySnapshotRequest = SupplyArr.Api.Contracts.CreateAvailabilitySnapshotRequest;
using VendorRestrictionResponse = SupplyArr.Api.Contracts.VendorRestrictionResponse;
using CreateVendorRestrictionRequest = SupplyArr.Api.Contracts.CreateVendorRestrictionRequest;
using SupplierIncidentResponse = SupplyArr.Api.Contracts.SupplierIncidentResponse;
using CreateSupplierIncidentRequest = SupplyArr.Api.Contracts.CreateSupplierIncidentRequest;
using ReorderEvaluationResponse = SupplyArr.Api.Contracts.ReorderEvaluationResponse;
using UpsertPartReorderPolicyRequest = SupplyArr.Api.Contracts.UpsertPartReorderPolicyRequest;
using CreatePurchaseRequestFromReorderRequest = SupplyArr.Api.Contracts.CreatePurchaseRequestFromReorderRequest;
using SupplyArr.Api.Data;
using SupplyArr.Api.Services;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrHandoffApiTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _serviceToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"SupplyArrHandoffNexArrTests-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplyArrHandoffTests-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<NexArrDbContext>)
                        || d.ServiceType == typeof(NexArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<NexArrDbContext>(options =>
                    options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _serviceToken = await IssueServiceTokenAsync(adminToken, "supplyarr");

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
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<SupplyArrDbContext>)
                        || d.ServiceType == typeof(SupplyArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<SupplyArrDbContext>(options =>
                    options.UseInMemoryDatabase(supplyArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StlNexArrLaunchClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
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
    public async Task Handoff_redeem_happy_path_returns_session_and_me_works()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Equal(PlatformSeeder.DemoAdminUserId, session.UserId);
        Assert.Contains("supplyarr", session.Entitlements);

        var meRequest = Authorized(HttpMethod.Get, "/api/me", session.AccessToken);
        var meResponse = await _supplyarrClient.SendAsync(meRequest);
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<SupplyArrMeResponse>();
        Assert.NotNull(me);
        Assert.True(me.HasSupplyArrEntitlement);
    }

    [Fact]
    public async Task Handoff_redeem_nexarr_alias_happy_path_returns_session()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/nexarr/redeem",
            new SupplyArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Contains("supplyarr", session.Entitlements);
    }

    [Fact]
    public async Task V1_handoff_session_and_me_aliases_work()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new SupplyArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));

        var meResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/me", session.AccessToken));
        meResponse.EnsureSuccessStatusCode();
        var me = (await meResponse.Content.ReadFromJsonAsync<SupplyArrMeResponse>())!;
        Assert.True(me.HasSupplyArrEntitlement);

        var sessionResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/session", session.AccessToken));
        sessionResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task V1_launch_handoff_proxy_returns_handoff_code()
    {
        var nexarrToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", nexarrToken);
        request.Content = JsonContent.Create(new CreateHandoffRequest("supplyarr", "http://localhost:5179/launch"));
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(handoff.HandoffCode));
    }

    [Fact]
    public async Task Party_registry_crud_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "acme-parts",
            "Acme Parts Co.",
            "Acme Parts Company LLC",
            "12-3456789",
            "Preferred OEM vendor"));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
        Assert.Equal("vendor", vendor.PartyType);
        Assert.Equal("pending", vendor.ApprovalStatus);

        var createPartyRequest = Authorized(HttpMethod.Post, "/api/parties", token);
        createPartyRequest.Content = JsonContent.Create(new CreateExternalPartyRequest(
            "regional-supply",
            "supplier",
            "Regional Supply Hub",
            "Regional Supply Hub Inc.",
            null,
            string.Empty));
        var createPartyResponse = await _supplyarrClient.SendAsync(createPartyRequest);
        createPartyResponse.EnsureSuccessStatusCode();

        var listVendorsRequest = Authorized(HttpMethod.Get, "/api/vendors", token);
        var listVendorsResponse = await _supplyarrClient.SendAsync(listVendorsRequest);
        listVendorsResponse.EnsureSuccessStatusCode();
        var vendors = (await listVendorsResponse.Content.ReadFromJsonAsync<List<ExternalPartyResponse>>())!;
        Assert.Contains(vendors, x => x.PartyId == vendor.PartyId);

        var contactRequest = Authorized(HttpMethod.Post, $"/api/vendors/{vendor.PartyId}/contacts", token);
        contactRequest.Content = JsonContent.Create(new CreatePartyContactRequest(
            "Jordan Lee",
            "jordan@acmeparts.example",
            "555-0100",
            "Account manager",
            true));
        var contactResponse = await _supplyarrClient.SendAsync(contactRequest);
        contactResponse.EnsureSuccessStatusCode();

        var approveRequest = Authorized(HttpMethod.Patch, $"/api/vendors/{vendor.PartyId}/approval-status", token);
        approveRequest.Content = JsonContent.Create(new UpdateExternalPartyApprovalStatusRequest("approved"));
        var approveResponse = await _supplyarrClient.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
        Assert.Equal("approved", approved.ApprovalStatus);
        Assert.Single(approved.Contacts);
    }

    [Fact]
    public async Task Party_registry_v1_vendor_alias_crud_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/v1/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "acme-parts-v1",
            "Acme Parts Co V1",
            "Acme Parts Company V1 LLC",
            "98-7654321",
            "Preferred OEM vendor v1"));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
        Assert.Equal("vendor", vendor.PartyType);

        var listVendorsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/vendors", token));
        listVendorsResponse.EnsureSuccessStatusCode();
        var vendors = (await listVendorsResponse.Content.ReadFromJsonAsync<List<ExternalPartyResponse>>())!;
        Assert.Contains(vendors, x => x.PartyId == vendor.PartyId);

        var contactRequest = Authorized(HttpMethod.Post, $"/api/v1/vendors/{vendor.PartyId}/contacts", token);
        contactRequest.Content = JsonContent.Create(new CreatePartyContactRequest(
            "Taylor Reed",
            "taylor@acmepartsv1.example",
            "555-0199",
            "V1 account manager",
            true));
        var contactResponse = await _supplyarrClient.SendAsync(contactRequest);
        contactResponse.EnsureSuccessStatusCode();

        var approveRequest = Authorized(HttpMethod.Patch, $"/api/v1/vendors/{vendor.PartyId}/approval-status", token);
        approveRequest.Content = JsonContent.Create(new UpdateExternalPartyApprovalStatusRequest("approved"));
        var approveResponse = await _supplyarrClient.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
        Assert.Equal("approved", approved.ApprovalStatus);
        Assert.Single(approved.Contacts);
    }

    [Fact]
    public async Task Party_create_denied_without_manage_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_clerk");
        var request = Authorized(HttpMethod.Post, "/api/vendors", token);
        request.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "denied-vendor",
            "Denied Vendor",
            string.Empty,
            null,
            string.Empty));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Part_catalog_crud_with_vendor_link_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "parts-vendor",
            "Parts Vendor Inc.",
            string.Empty,
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createCatalogRequest = Authorized(HttpMethod.Post, "/api/catalogs", token);
        createCatalogRequest.Content = JsonContent.Create(new CreatePartCatalogRequest(
            "oem-filters",
            "OEM Filters",
            "Standard filter catalog"));
        var createCatalogResponse = await _supplyarrClient.SendAsync(createCatalogRequest);
        createCatalogResponse.EnsureSuccessStatusCode();
        var catalog = (await createCatalogResponse.Content.ReadFromJsonAsync<PartCatalogResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "filter-001",
            catalog.CatalogId,
            "Primary Oil Filter",
            "OEM oil filter for fleet vehicles",
            "filters",
            "each",
            "Fleet OEM",
            "FLT-001"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;
        Assert.Equal("active", part.Status);
        Assert.Equal(catalog.CatalogKey, part.CatalogKey);

        var linkRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/vendor-links", token);
        linkRequest.Content = JsonContent.Create(new CreatePartVendorLinkRequest(
            vendor.PartyId,
            "V-FLT-001",
            true));
        var linkResponse = await _supplyarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();
        var link = (await linkResponse.Content.ReadFromJsonAsync<PartVendorLinkResponse>())!;
        Assert.Equal("V-FLT-001", link.VendorPartNumber);

        var listPartsRequest = Authorized(HttpMethod.Get, "/api/parts", token);
        var listPartsResponse = await _supplyarrClient.SendAsync(listPartsRequest);
        listPartsResponse.EnsureSuccessStatusCode();
        var parts = (await listPartsResponse.Content.ReadFromJsonAsync<List<PartResponse>>())!;
        Assert.Contains(parts, x => x.PartId == part.PartId);

        var getPartRequest = Authorized(HttpMethod.Get, $"/api/parts/{part.PartId}", token);
        var getPartResponse = await _supplyarrClient.SendAsync(getPartRequest);
        getPartResponse.EnsureSuccessStatusCode();
        var loaded = (await getPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;
        Assert.Single(loaded.VendorLinks);
        Assert.Equal(vendor.PartyKey, loaded.VendorLinks[0].PartyKey);
        Assert.True(loaded.VendorLinks[0].IsPreferred);
    }

    [Fact]
    public async Task Part_catalog_crud_with_vendor_link_v1_alias_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "parts-vendor-v1",
            "Parts Vendor V1 Inc.",
            string.Empty,
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createCatalogRequest = Authorized(HttpMethod.Post, "/api/v1/catalogs", token);
        createCatalogRequest.Content = JsonContent.Create(new CreatePartCatalogRequest(
            "oem-filters-v1",
            "OEM Filters V1",
            "Standard filter catalog v1"));
        var createCatalogResponse = await _supplyarrClient.SendAsync(createCatalogRequest);
        createCatalogResponse.EnsureSuccessStatusCode();
        var catalog = (await createCatalogResponse.Content.ReadFromJsonAsync<PartCatalogResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/v1/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "filter-v1-001",
            catalog.CatalogId,
            "Primary Oil Filter V1",
            "OEM oil filter for fleet vehicles v1",
            "filters",
            "each",
            "Fleet OEM",
            "FLT-V1-001"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var linkRequest = Authorized(HttpMethod.Post, $"/api/v1/parts/{part.PartId}/vendor-links", token);
        linkRequest.Content = JsonContent.Create(new CreatePartVendorLinkRequest(
            vendor.PartyId,
            "V-FLT-V1-001",
            true));
        var linkResponse = await _supplyarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();

        var listPartsRequest = Authorized(HttpMethod.Get, "/api/v1/parts", token);
        var listPartsResponse = await _supplyarrClient.SendAsync(listPartsRequest);
        listPartsResponse.EnsureSuccessStatusCode();
        var parts = (await listPartsResponse.Content.ReadFromJsonAsync<List<PartResponse>>())!;
        Assert.Contains(parts, x => x.PartId == part.PartId);

        var getPartRequest = Authorized(HttpMethod.Get, $"/api/v1/parts/{part.PartId}", token);
        var getPartResponse = await _supplyarrClient.SendAsync(getPartRequest);
        getPartResponse.EnsureSuccessStatusCode();
        var loaded = (await getPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;
        Assert.Single(loaded.VendorLinks);
    }

    [Fact]
    public async Task Part_create_denied_without_manage_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_clerk");
        var request = Authorized(HttpMethod.Post, "/api/parts", token);
        request.Content = JsonContent.Create(new CreatePartRequest(
            "denied-part",
            null,
            "Denied Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Inventory_locations_bins_and_stock_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "stock-part-001",
            null,
            "Stock Test Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "main-wh",
            "Main Warehouse",
            "warehouse",
            "100 Dock St"));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;
        Assert.Equal("active", location.Status);
        Assert.Equal(0, location.BinCount);

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/inventory/locations/{location.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("a-01", "Aisle 01"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;
        Assert.Equal("main-wh", bin.LocationKey);

        var upsertStockRequest = Authorized(HttpMethod.Post, "/api/inventory/stock", token);
        upsertStockRequest.Content = JsonContent.Create(new UpsertPartStockLevelRequest(
            part.PartId,
            bin.BinId,
            24m));
        var upsertStockResponse = await _supplyarrClient.SendAsync(upsertStockRequest);
        upsertStockResponse.EnsureSuccessStatusCode();
        var stock = (await upsertStockResponse.Content.ReadFromJsonAsync<PartStockLevelResponse>())!;
        Assert.Equal(24m, stock.QuantityOnHand);
        Assert.Equal(24m, stock.QuantityAvailable);
        Assert.Equal(part.PartKey, stock.PartKey);

        var listStockRequest = Authorized(
            HttpMethod.Get,
            $"/api/inventory/stock?locationId={location.LocationId}",
            token);
        var listStockResponse = await _supplyarrClient.SendAsync(listStockRequest);
        listStockResponse.EnsureSuccessStatusCode();
        var stockLevels = (await listStockResponse.Content.ReadFromJsonAsync<List<PartStockLevelResponse>>())!;
        Assert.Single(stockLevels);
        Assert.Equal(bin.BinId, stockLevels[0].BinId);

        var listLocationsRequest = Authorized(HttpMethod.Get, "/api/inventory/locations", token);
        var listLocationsResponse = await _supplyarrClient.SendAsync(listLocationsRequest);
        listLocationsResponse.EnsureSuccessStatusCode();
        var locations = (await listLocationsResponse.Content.ReadFromJsonAsync<List<InventoryLocationResponse>>())!;
        Assert.Contains(locations, x => x.LocationId == location.LocationId && x.BinCount == 1);
    }

    [Fact]
    public async Task Inventory_v1_locations_bins_and_stock_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "stock-v1-part-001",
            null,
            "Stock v1 Test Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "v1-main-wh",
            "v1 Main Warehouse",
            "warehouse",
            "100 Dock St"));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/inventory/locations/{location.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("v1-a-01", "v1 Aisle 01"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var upsertStockRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/stock", token);
        upsertStockRequest.Content = JsonContent.Create(new UpsertPartStockLevelRequest(
            part.PartId,
            bin.BinId,
            12m));
        var upsertStockResponse = await _supplyarrClient.SendAsync(upsertStockRequest);
        upsertStockResponse.EnsureSuccessStatusCode();
        var stock = (await upsertStockResponse.Content.ReadFromJsonAsync<PartStockLevelResponse>())!;
        Assert.Equal(12m, stock.QuantityOnHand);

        var listStockRequest = Authorized(
            HttpMethod.Get,
            $"/api/v1/inventory/stock?locationId={location.LocationId}",
            token);
        var listStockResponse = await _supplyarrClient.SendAsync(listStockRequest);
        listStockResponse.EnsureSuccessStatusCode();
        var stockLevels = (await listStockResponse.Content.ReadFromJsonAsync<List<PartStockLevelResponse>>())!;
        Assert.Single(stockLevels);
    }

    [Fact]
    public async Task Inventory_v1_stock_reservations_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "reservation-part-001",
            null,
            "Reservation Test Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "reservation-wh",
            "Reservation warehouse",
            "warehouse",
            "300 Dock St"));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/inventory/locations/{location.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("reservation-bin", "B-01"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var stockRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/stock", token);
        stockRequest.Content = JsonContent.Create(new UpsertPartStockLevelRequest(part.PartId, bin.BinId, 20m));
        (await _supplyarrClient.SendAsync(stockRequest)).EnsureSuccessStatusCode();

        var createReservationRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/reservations", token);
        createReservationRequest.Content = JsonContent.Create(new CreateStockReservationRequest(
            "reservation-001",
            part.PartId,
            bin.BinId,
            5m,
            "work_order",
            null,
            "reserve stock for job"));
        var createReservationResponse = await _supplyarrClient.SendAsync(createReservationRequest);
        createReservationResponse.EnsureSuccessStatusCode();
        var reservation = (await createReservationResponse.Content.ReadFromJsonAsync<StockReservationResponse>())!;
        Assert.Equal("active", reservation.Status);
        Assert.Equal(5m, reservation.QuantityReserved);

        var listReservationsRequest = Authorized(HttpMethod.Get, "/api/v1/inventory/reservations", token);
        var listReservationsResponse = await _supplyarrClient.SendAsync(listReservationsRequest);
        listReservationsResponse.EnsureSuccessStatusCode();
        var reservations = (await listReservationsResponse.Content.ReadFromJsonAsync<List<StockReservationResponse>>())!;
        Assert.Contains(reservations, x => x.ReservationId == reservation.ReservationId);

        var releaseReservationRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/inventory/reservations/{reservation.ReservationId}/release",
            token);
        releaseReservationRequest.Content = JsonContent.Create(new ReleaseStockReservationRequest("cancelled work"));
        var releaseReservationResponse = await _supplyarrClient.SendAsync(releaseReservationRequest);
        releaseReservationResponse.EnsureSuccessStatusCode();
        var released = (await releaseReservationResponse.Content.ReadFromJsonAsync<StockReservationResponse>())!;
        Assert.Equal("released", released.Status);
        Assert.Equal("cancelled work", released.ReleaseReason);
    }

    [Fact]
    public async Task Backorders_and_returns_v1_list_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var listBackordersRequest = Authorized(HttpMethod.Get, "/api/v1/backorders", token);
        var listBackordersResponse = await _supplyarrClient.SendAsync(listBackordersRequest);
        listBackordersResponse.EnsureSuccessStatusCode();
        var backorders = await listBackordersResponse.Content.ReadFromJsonAsync<List<BackorderResponse>>();
        Assert.NotNull(backorders);

        var listReturnsRequest = Authorized(HttpMethod.Get, "/api/v1/returns", token);
        var listReturnsResponse = await _supplyarrClient.SendAsync(listReturnsRequest);
        listReturnsResponse.EnsureSuccessStatusCode();
        var returns = await listReturnsResponse.Content.ReadFromJsonAsync<List<VendorReturnResponse>>();
        Assert.NotNull(returns);
    }

    [Fact]
    public async Task Rfqs_v1_list_alias_works()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var listRfqsRequest = Authorized(HttpMethod.Get, "/api/v1/rfqs", token);
        var listRfqsResponse = await _supplyarrClient.SendAsync(listRfqsRequest);
        listRfqsResponse.EnsureSuccessStatusCode();
        var rfqs = await listRfqsResponse.Content.ReadFromJsonAsync<List<RfqResponse>>();
        Assert.NotNull(rfqs);
    }

    [Fact]
    public async Task Warranty_claims_v1_list_alias_works()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var listClaimsRequest = Authorized(HttpMethod.Get, "/api/v1/warranty-claims", token);
        var listClaimsResponse = await _supplyarrClient.SendAsync(listClaimsRequest);
        listClaimsResponse.EnsureSuccessStatusCode();
        var claims = await listClaimsResponse.Content.ReadFromJsonAsync<List<WarrantyClaimResponse>>();
        Assert.NotNull(claims);
    }

    [Fact]
    public async Task Snapshot_v1_list_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var pricingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/pricing-snapshots", token));
        pricingResponse.EnsureSuccessStatusCode();
        Assert.NotNull(await pricingResponse.Content.ReadFromJsonAsync<List<PricingSnapshotResponse>>());

        var leadTimeResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/lead-time-snapshots", token));
        leadTimeResponse.EnsureSuccessStatusCode();
        Assert.NotNull(await leadTimeResponse.Content.ReadFromJsonAsync<List<LeadTimeSnapshotResponse>>());

        var availabilityResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/availability-snapshots", token));
        availabilityResponse.EnsureSuccessStatusCode();
        Assert.NotNull(await availabilityResponse.Content.ReadFromJsonAsync<List<AvailabilitySnapshotResponse>>());
    }

    [Fact]
    public async Task Demand_and_reorder_v1_list_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var demandRefsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/demand-refs", token));
        demandRefsResponse.EnsureSuccessStatusCode();

        var demandProcessingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/demand-processing", token));
        demandProcessingResponse.EnsureSuccessStatusCode();

        var reorderResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reorder-evaluation", token));
        reorderResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Demand_processing_settings_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var settingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/demand-processing-settings", token));
        settingsResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/demand-processing-settings/pending", token));
        pendingResponse.EnsureSuccessStatusCode();

        var runsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/demand-processing-settings/runs?limit=5", token));
        runsResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Integration_event_settings_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var settingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integration-event-settings", token));
        settingsResponse.EnsureSuccessStatusCode();

        var outboxResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integration-event-settings/outbox?limit=5", token));
        outboxResponse.EnsureSuccessStatusCode();

        var inboxResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integration-event-settings/inbox?limit=5", token));
        inboxResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Notification_settings_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var settingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/notification-settings", token));
        settingsResponse.EnsureSuccessStatusCode();

        var dispatchesResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/notification-settings/dispatches?limit=5", token));
        dispatchesResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Price_snapshot_settings_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var settingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/price-snapshot-settings", token));
        settingsResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/price-snapshot-settings/pending", token));
        pendingResponse.EnsureSuccessStatusCode();

        var runsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/price-snapshot-settings/runs?limit=5", token));
        runsResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Lead_time_and_availability_snapshot_settings_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var leadTimeSettingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/lead-time-snapshot-settings", token));
        leadTimeSettingsResponse.EnsureSuccessStatusCode();

        var leadTimePendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/lead-time-snapshot-settings/pending", token));
        leadTimePendingResponse.EnsureSuccessStatusCode();

        var leadTimeRunsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/lead-time-snapshot-settings/runs?limit=5", token));
        leadTimeRunsResponse.EnsureSuccessStatusCode();

        var availabilitySettingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/availability-snapshot-settings", token));
        availabilitySettingsResponse.EnsureSuccessStatusCode();

        var availabilityPendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/availability-snapshot-settings/pending", token));
        availabilityPendingResponse.EnsureSuccessStatusCode();

        var availabilityRunsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/availability-snapshot-settings/runs?limit=5", token));
        availabilityRunsResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Procurement_coordination_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var dashboardResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-coordination?activeOnly=true", token));
        dashboardResponse.EnsureSuccessStatusCode();

        var settingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-coordination-settings", token));
        settingsResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-coordination-settings/pending", token));
        pendingResponse.EnsureSuccessStatusCode();

        var runsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-coordination-settings/runs?limit=5", token));
        runsResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Approval_reminder_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var dashboardResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/approval-reminders?includeUpcoming=false", token));
        dashboardResponse.EnsureSuccessStatusCode();

        var settingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/approval-reminder-settings", token));
        settingsResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/approval-reminder-settings/pending", token));
        pendingResponse.EnsureSuccessStatusCode();

        var runsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/approval-reminder-settings/runs?limit=5", token));
        runsResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Procurement_exception_escalation_settings_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var settingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-exception-escalation-settings", token));
        settingsResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-exception-escalation-settings/pending", token));
        pendingResponse.EnsureSuccessStatusCode();

        var runsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-exception-escalation-settings/runs?limit=5", token));
        runsResponse.EnsureSuccessStatusCode();

        var eventsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-exception-escalation-settings/events?limit=5", token));
        eventsResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Procurement_exceptions_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var templatesResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-exceptions/resolution-templates", token));
        templatesResponse.EnsureSuccessStatusCode();

        var listResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/procurement-exceptions", token));
        listResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Supplier_incidents_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "incident-vendor",
            "Incident Vendor",
            string.Empty,
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createIncidentRequest = Authorized(HttpMethod.Post, "/api/v1/supplier-incidents", token);
        createIncidentRequest.Content = JsonContent.Create(new CreateSupplierIncidentRequest(
            vendor.PartyId,
            "incident-001",
            "Shipment quality issue",
            "Observed defects in received batch",
            "quality",
            "high",
            null,
            null,
            null,
            null,
            null));
        var createIncidentResponse = await _supplyarrClient.SendAsync(createIncidentRequest);
        createIncidentResponse.EnsureSuccessStatusCode();
        var incident = (await createIncidentResponse.Content.ReadFromJsonAsync<SupplierIncidentResponse>())!;
        Assert.Equal(vendor.PartyId, incident.ExternalPartyId);

        var listResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/supplier-incidents", token));
        listResponse.EnsureSuccessStatusCode();

        var byPartyResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/parties/{vendor.PartyId}/supplier-incidents", token));
        byPartyResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Vendor_restrictions_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "restricted-vendor",
            "Restricted Vendor",
            string.Empty,
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createRestrictionRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/parties/{vendor.PartyId}/vendor-restrictions",
            token);
        createRestrictionRequest.Content = JsonContent.Create(new CreateVendorRestrictionRequest(
            "hold-vendor",
            ["purchase_orders"],
            "Open compliance hold",
            null,
            null));
        var createRestrictionResponse = await _supplyarrClient.SendAsync(createRestrictionRequest);
        createRestrictionResponse.EnsureSuccessStatusCode();
        var restriction = (await createRestrictionResponse.Content.ReadFromJsonAsync<VendorRestrictionResponse>())!;
        Assert.Equal(vendor.PartyId, restriction.ExternalPartyId);

        var listResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/vendor-restrictions", token));
        listResponse.EnsureSuccessStatusCode();

        var byPartyResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/parties/{vendor.PartyId}/vendor-restrictions", token));
        byPartyResponse.EnsureSuccessStatusCode();

        var enforcementResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/parties/{vendor.PartyId}/vendor-restrictions/enforcement", token));
        enforcementResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Supplier_onboarding_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "onboarding-vendor",
            "Onboarding Vendor",
            string.Empty,
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var requirementsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/supplier-onboarding/document-requirements", token));
        requirementsResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/supplier-onboarding/pending", token));
        pendingResponse.EnsureSuccessStatusCode();

        var partyDocsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/parties/{vendor.PartyId}/compliance-documents", token));
        partyDocsResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Emergency_purchases_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var listResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/emergency-purchases", token));
        listResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/emergency-purchases/pending", token));
        pendingResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Reports_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var vendorResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/vendors/summary", token));
        vendorResponse.EnsureSuccessStatusCode();

        var partsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/parts-inventory/summary", token));
        partsResponse.EnsureSuccessStatusCode();

        var purchasingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/purchasing/summary", token));
        purchasingResponse.EnsureSuccessStatusCode();

        var complianceResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/compliance/summary", token));
        complianceResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Forgiving_search_v1_alias_works()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/search/forgiving?q=oil&limit=5", token));
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Audit_history_v1_alias_works()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-history?limit=5", token));
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Events_and_audit_v1_aliases_match_existing_endpoints()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var outboxResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integration-event-settings/outbox?limit=5", token));
        outboxResponse.EnsureSuccessStatusCode();
        var outbox = (await outboxResponse.Content.ReadFromJsonAsync<global::SupplyArr.Api.Contracts.IntegrationEventsListResponse>())!;

        var eventsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/events?limit=5", token));
        eventsResponse.EnsureSuccessStatusCode();
        var eventsAlias = (await eventsResponse.Content.ReadFromJsonAsync<global::SupplyArr.Api.Contracts.IntegrationEventsListResponse>())!;
        Assert.Equal(outbox.Items.Count, eventsAlias.Items.Count);

        const string actionFilter = "alias-test-no-match";
        var auditHistoryResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/audit-history?limit=5&action={actionFilter}", token));
        auditHistoryResponse.EnsureSuccessStatusCode();
        var auditHistory = (await auditHistoryResponse.Content.ReadFromJsonAsync<global::SupplyArr.Api.Contracts.AuditHistoryListResponse>())!;

        var auditResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/audit?limit=5&action={actionFilter}", token));
        auditResponse.EnsureSuccessStatusCode();
        var auditAlias = (await auditResponse.Content.ReadFromJsonAsync<global::SupplyArr.Api.Contracts.AuditHistoryListResponse>())!;
        Assert.Equal(auditHistory.Items.Count, auditAlias.Items.Count);
    }

    [Fact]
    public async Task Field_inbox_v1_alias_works()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/field-inbox", token));
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Demand_ref_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var routarrResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/routarr-demand-refs", token));
        routarrResponse.EnsureSuccessStatusCode();

        var staffarrResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/staffarr-demand-refs", token));
        staffarrResponse.EnsureSuccessStatusCode();

        var trainarrResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/trainarr-demand-refs", token));
        trainarrResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Inventory_location_create_denied_without_manage_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_clerk");
        var request = Authorized(HttpMethod.Post, "/api/inventory/locations", token);
        request.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "denied-wh",
            "Denied Warehouse",
            "warehouse",
            string.Empty));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Purchase_request_submit_approve_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "pr-vendor-001",
            "PR Vendor",
            "PR Vendor LLC",
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "pr-part-001",
            null,
            "PR Test Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            "pr-2026-001",
            "Shop restock",
            "Filters for PM week",
            vendor.PartyId,
            [
                new CreatePurchaseRequestLineRequest(part.PartId, 6m, "Oil filters")
            ]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
        Assert.Equal("draft", purchaseRequest.Status);
        Assert.Single(purchaseRequest.Lines);
        Assert.Equal(vendor.PartyKey, purchaseRequest.VendorPartyKey);

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token);
        var submitResponse = await _supplyarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();
        var submitted = (await submitResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
        Assert.Equal("submitted", submitted.Status);
        Assert.NotNull(submitted.SubmittedAt);

        var approveRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            token);
        var approveResponse = await _supplyarrClient.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
        Assert.Equal("approved", approved.Status);
        Assert.NotNull(approved.ApprovedAt);

        var listRequest = Authorized(HttpMethod.Get, "/api/purchase-requests?status=approved", token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<PurchaseRequestResponse>>())!;
        Assert.Contains(listed, x => x.PurchaseRequestId == purchaseRequest.PurchaseRequestId);
    }

    [Fact]
    public async Task Purchase_request_v1_submit_approve_and_list_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "pr-v1-part-001",
            null,
            "PR v1 Test Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/v1/purchase-requests", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            "pr-v1-2026-001",
            "v1 restock",
            string.Empty,
            null,
            [new CreatePurchaseRequestLineRequest(part.PartId, 3m, "v1 line")]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
        Assert.Equal("draft", purchaseRequest.Status);

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token);
        var submitResponse = await _supplyarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();
        var submitted = (await submitResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
        Assert.Equal("submitted", submitted.Status);

        var approveRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            token);
        var approveResponse = await _supplyarrClient.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
        Assert.Equal("approved", approved.Status);

        var listRequest = Authorized(HttpMethod.Get, "/api/v1/purchase-requests?status=approved", token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<PurchaseRequestResponse>>())!;
        Assert.Contains(listed, x => x.PurchaseRequestId == purchaseRequest.PurchaseRequestId);
    }

    [Fact]
    public async Task Purchase_request_submit_reject_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "pr-part-reject",
            null,
            "Reject Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            "pr-reject-001",
            "Rejected request",
            string.Empty,
            null,
            [new CreatePurchaseRequestLineRequest(part.PartId, 2m, string.Empty)]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var rejectRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/reject",
            token);
        rejectRequest.Content = JsonContent.Create(new RejectPurchaseRequestRequest("Budget hold"));
        var rejectResponse = await _supplyarrClient.SendAsync(rejectRequest);
        rejectResponse.EnsureSuccessStatusCode();
        var rejected = (await rejectResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
        Assert.Equal("rejected", rejected.Status);
        Assert.Equal("Budget hold", rejected.RejectionReason);
    }

    [Fact]
    public async Task Purchase_request_create_denied_for_clerk_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_clerk");
        var request = Authorized(HttpMethod.Post, "/api/purchase-requests", token);
        request.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            "denied-pr",
            "Denied",
            string.Empty,
            null,
            null));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Purchase_request_approve_denied_for_buyer_role()
    {
        var managerToken = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", managerToken);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "pr-part-buyer",
            null,
            "Buyer Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", managerToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            "pr-buyer-approve",
            "Buyer approval test",
            string.Empty,
            null,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            managerToken);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var approveRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            buyerToken);
        var approveResponse = await _supplyarrClient.SendAsync(approveRequest);
        Assert.Equal(HttpStatusCode.Forbidden, approveResponse.StatusCode);
    }

    [Fact]
    public async Task Purchase_order_from_approved_pr_approve_issue_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "po-vendor-001",
            "PO Vendor",
            "PO Vendor LLC",
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "po-part-001",
            null,
            "PO Test Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            "pr-po-2026-001",
            "PO source request",
            "For PO workflow test",
            vendor.PartyId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 4m, "Brake pads")]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var approvePrRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            token);
        (await _supplyarrClient.SendAsync(approvePrRequest)).EnsureSuccessStatusCode();

        var createPoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/from-purchase-request/{purchaseRequest.PurchaseRequestId}",
            token);
        createPoRequest.Content = JsonContent.Create(
            new CreatePurchaseOrderFromPurchaseRequestRequest("po-2026-001", null, null));
        var createPoResponse = await _supplyarrClient.SendAsync(createPoRequest);
        createPoResponse.EnsureSuccessStatusCode();
        var purchaseOrder = (await createPoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        Assert.Equal("draft", purchaseOrder.Status);
        Assert.Equal(purchaseRequest.PurchaseRequestId, purchaseOrder.PurchaseRequestId);
        Assert.Equal(vendor.PartyId, purchaseOrder.VendorPartyId);
        Assert.Single(purchaseOrder.Lines);
        Assert.Equal(4m, purchaseOrder.Lines[0].QuantityOrdered);

        var approvePoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/approve",
            token);
        var approvePoResponse = await _supplyarrClient.SendAsync(approvePoRequest);
        approvePoResponse.EnsureSuccessStatusCode();
        var approvedPo = (await approvePoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        Assert.Equal("approved", approvedPo.Status);
        Assert.NotNull(approvedPo.ApprovedAt);

        var issueRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/issue",
            token);
        var issueResponse = await _supplyarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issuedPo = (await issueResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        Assert.Equal("issued", issuedPo.Status);
        Assert.NotNull(issuedPo.IssuedAt);

        var listRequest = Authorized(HttpMethod.Get, "/api/purchase-orders?status=issued", token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<PurchaseOrderResponse>>())!;
        Assert.Contains(listed, x => x.PurchaseOrderId == purchaseOrder.PurchaseOrderId);
    }

    [Fact]
    public async Task Purchase_order_v1_from_approved_pr_approve_issue_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "po-v1-vendor-001",
            "PO v1 Vendor",
            "PO v1 Vendor LLC",
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "po-v1-part-001",
            null,
            "PO v1 Test Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/v1/purchase-requests", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            "pr-po-v1-2026-001",
            "PO v1 source request",
            string.Empty,
            vendor.PartyId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 2m, "v1 po line")]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var approvePrRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            token);
        (await _supplyarrClient.SendAsync(approvePrRequest)).EnsureSuccessStatusCode();

        var createPoRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/purchase-orders/from-purchase-request/{purchaseRequest.PurchaseRequestId}",
            token);
        createPoRequest.Content = JsonContent.Create(
            new CreatePurchaseOrderFromPurchaseRequestRequest("po-v1-2026-001", null, null));
        var createPoResponse = await _supplyarrClient.SendAsync(createPoRequest);
        createPoResponse.EnsureSuccessStatusCode();
        var purchaseOrder = (await createPoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        Assert.Equal("draft", purchaseOrder.Status);

        var approvePoRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/purchase-orders/{purchaseOrder.PurchaseOrderId}/approve",
            token);
        var approvePoResponse = await _supplyarrClient.SendAsync(approvePoRequest);
        approvePoResponse.EnsureSuccessStatusCode();
        var approvedPo = (await approvePoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        Assert.Equal("approved", approvedPo.Status);

        var issueRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/purchase-orders/{purchaseOrder.PurchaseOrderId}/issue",
            token);
        var issueResponse = await _supplyarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issuedPo = (await issueResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        Assert.Equal("issued", issuedPo.Status);

        var listRequest = Authorized(HttpMethod.Get, "/api/v1/purchase-orders?status=issued", token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<PurchaseOrderResponse>>())!;
        Assert.Contains(listed, x => x.PurchaseOrderId == purchaseOrder.PurchaseOrderId);
    }

    [Fact]
    public async Task Purchase_order_create_denied_for_clerk_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_clerk");
        var request = Authorized(
            HttpMethod.Post,
            "/api/purchase-orders/from-purchase-request/00000000-0000-0000-0000-000000000001",
            token);
        request.Content = JsonContent.Create(
            new CreatePurchaseOrderFromPurchaseRequestRequest("denied-po", null, null));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Purchase_order_approve_denied_for_buyer_role()
    {
        var managerToken = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", managerToken);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "po-vendor-buyer",
            "Buyer PO Vendor",
            "Buyer PO Vendor LLC",
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", managerToken);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "po-part-buyer",
            null,
            "Buyer PO Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", managerToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            "pr-po-buyer",
            "Buyer PO test",
            string.Empty,
            vendor.PartyId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            managerToken);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var approvePrRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            managerToken);
        (await _supplyarrClient.SendAsync(approvePrRequest)).EnsureSuccessStatusCode();

        var createPoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/from-purchase-request/{purchaseRequest.PurchaseRequestId}",
            managerToken);
        createPoRequest.Content = JsonContent.Create(
            new CreatePurchaseOrderFromPurchaseRequestRequest("po-buyer-approve", null, null));
        var createPoResponse = await _supplyarrClient.SendAsync(createPoRequest);
        createPoResponse.EnsureSuccessStatusCode();
        var purchaseOrder = (await createPoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;

        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var approvePoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/approve",
            buyerToken);
        var approvePoResponse = await _supplyarrClient.SendAsync(approvePoRequest);
        Assert.Equal(HttpStatusCode.Forbidden, approvePoResponse.StatusCode);
    }

    [Fact]
    public async Task Receiving_against_issued_po_posts_stock_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            "rcv-vendor-001",
            "Receiving Vendor",
            "Receiving Vendor LLC",
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "rcv-part-001",
            null,
            "Receiving Test Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "rcv-wh",
            "Receiving Warehouse",
            "warehouse",
            "200 Dock St"));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/inventory/locations/{location.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("rcv-01", "Receiving Bin 01"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            "pr-rcv-2026-001",
            "Receiving source request",
            string.Empty,
            vendor.PartyId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 5m, string.Empty)]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var approvePrRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            token);
        (await _supplyarrClient.SendAsync(approvePrRequest)).EnsureSuccessStatusCode();

        var createPoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/from-purchase-request/{purchaseRequest.PurchaseRequestId}",
            token);
        createPoRequest.Content = JsonContent.Create(
            new CreatePurchaseOrderFromPurchaseRequestRequest("po-rcv-2026-001", null, null));
        var createPoResponse = await _supplyarrClient.SendAsync(createPoRequest);
        createPoResponse.EnsureSuccessStatusCode();
        var purchaseOrder = (await createPoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;

        var approvePoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/approve",
            token);
        (await _supplyarrClient.SendAsync(approvePoRequest)).EnsureSuccessStatusCode();

        var issueRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/issue",
            token);
        (await _supplyarrClient.SendAsync(issueRequest)).EnsureSuccessStatusCode();

        var createReceiptRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/from-purchase-order/{purchaseOrder.PurchaseOrderId}",
            token);
        createReceiptRequest.Content = JsonContent.Create(
            new CreateReceivingReceiptFromPurchaseOrderRequest("rcpt-2026-001", bin.BinId, "Dock delivery"));
        var createReceiptResponse = await _supplyarrClient.SendAsync(createReceiptRequest);
        createReceiptResponse.EnsureSuccessStatusCode();
        var receipt = (await createReceiptResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("draft", receipt.Status);
        Assert.Single(receipt.Lines);
        Assert.Equal(5m, receipt.Lines[0].QuantityReceived);

        var postReceiptRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var postReceiptResponse = await _supplyarrClient.SendAsync(postReceiptRequest);
        postReceiptResponse.EnsureSuccessStatusCode();
        var posted = (await postReceiptResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("posted", posted.Status);
        Assert.NotNull(posted.PostedAt);

        var getPoRequest = Authorized(
            HttpMethod.Get,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}",
            token);
        var getPoResponse = await _supplyarrClient.SendAsync(getPoRequest);
        getPoResponse.EnsureSuccessStatusCode();
        var issuedPo = (await getPoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        Assert.Equal(5m, issuedPo.Lines[0].QuantityReceived);
        Assert.Equal(0m, issuedPo.Lines[0].QuantityRemaining);

        var listStockRequest = Authorized(
            HttpMethod.Get,
            $"/api/inventory/stock?partId={part.PartId}&binId={bin.BinId}",
            token);
        var listStockResponse = await _supplyarrClient.SendAsync(listStockRequest);
        listStockResponse.EnsureSuccessStatusCode();
        var stockLevels = (await listStockResponse.Content.ReadFromJsonAsync<List<PartStockLevelResponse>>())!;
        Assert.Single(stockLevels);
        Assert.Equal(5m, stockLevels[0].QuantityOnHand);
    }

    [Fact]
    public async Task Receiving_v1_against_issued_po_posts_stock_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-v1-001",
            "po-rcv-v1-001",
            "rcv-v1",
            4m);

        var createReceiptRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/receiving/from-purchase-order/{purchaseOrder.PurchaseOrderId}",
            token);
        createReceiptRequest.Content = JsonContent.Create(
            new CreateReceivingReceiptFromPurchaseOrderRequest("rcpt-v1-001", bin.BinId, "Dock delivery"));
        var createReceiptResponse = await _supplyarrClient.SendAsync(createReceiptRequest);
        createReceiptResponse.EnsureSuccessStatusCode();
        var receipt = (await createReceiptResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("draft", receipt.Status);

        var postReceiptRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var postReceiptResponse = await _supplyarrClient.SendAsync(postReceiptRequest);
        postReceiptResponse.EnsureSuccessStatusCode();
        var posted = (await postReceiptResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("posted", posted.Status);

        var listRequest = Authorized(HttpMethod.Get, "/api/v1/receiving?status=posted", token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var receipts = (await listResponse.Content.ReadFromJsonAsync<List<ReceivingReceiptResponse>>())!;
        Assert.Contains(receipts, x => x.ReceivingReceiptId == receipt.ReceivingReceiptId);

        var listStockRequest = Authorized(
            HttpMethod.Get,
            $"/api/inventory/stock?partId={part.PartId}&binId={bin.BinId}",
            token);
        var listStockResponse = await _supplyarrClient.SendAsync(listStockRequest);
        listStockResponse.EnsureSuccessStatusCode();
        var stockLevels = (await listStockResponse.Content.ReadFromJsonAsync<List<PartStockLevelResponse>>())!;
        Assert.Single(stockLevels);
        Assert.Equal(4m, stockLevels[0].QuantityOnHand);
    }

    [Fact]
    public async Task Receiving_create_denied_for_buyer_role()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var request = Authorized(
            HttpMethod.Post,
            "/api/receiving/from-purchase-order/00000000-0000-0000-0000-000000000001",
            buyerToken);
        request.Content = JsonContent.Create(
            new CreateReceivingReceiptFromPurchaseOrderRequest(
                "denied-rcpt",
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                null));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Receiving_short_shipment_with_exception_posts_partial_stock()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-short-001",
            "po-rcv-short-001",
            "rcv-short",
            5m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-short-001");

        var line = receipt.Lines.Single();
        Assert.Equal(5m, line.QuantityExpected);

        var updateLineRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}",
            token);
        updateLineRequest.Content = JsonContent.Create(new UpdateReceivingReceiptLineRequest(3m));
        (await _supplyarrClient.SendAsync(updateLineRequest)).EnsureSuccessStatusCode();

        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/exceptions",
            token);
        createExceptionRequest.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("short", 2m, "Two units missing from carton"));
        var createExceptionResponse = await _supplyarrClient.SendAsync(createExceptionRequest);
        createExceptionResponse.EnsureSuccessStatusCode();

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postRequest)).EnsureSuccessStatusCode();

        var stockLevels = await ListStockAsync(token, part.PartId, bin.BinId);
        Assert.Single(stockLevels);
        Assert.Equal(3m, stockLevels[0].QuantityOnHand);

        var issuedPo = await GetPurchaseOrderAsync(token, purchaseOrder.PurchaseOrderId);
        Assert.Equal(3m, issuedPo.Lines[0].QuantityReceived);
    }

    [Fact]
    public async Task Receiving_over_receive_requires_exception_before_post()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-over-001",
            "po-rcv-over-001",
            "rcv-over",
            5m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-over-001");
        var line = receipt.Lines.Single();

        var updateLineRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}",
            token);
        updateLineRequest.Content = JsonContent.Create(new UpdateReceivingReceiptLineRequest(6m));
        (await _supplyarrClient.SendAsync(updateLineRequest)).EnsureSuccessStatusCode();

        var postWithoutException = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var blockedPost = await _supplyarrClient.SendAsync(postWithoutException);
        Assert.Equal(HttpStatusCode.BadRequest, blockedPost.StatusCode);

        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/exceptions",
            token);
        createExceptionRequest.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("over", 1m, "Vendor shipped one extra unit"));
        (await _supplyarrClient.SendAsync(createExceptionRequest)).EnsureSuccessStatusCode();

        var postWithException = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postWithException)).EnsureSuccessStatusCode();

        var stockLevels = await ListStockAsync(token, part.PartId, bin.BinId);
        Assert.Equal(6m, stockLevels[0].QuantityOnHand);
    }

    [Fact]
    public async Task Receiving_damage_exception_posts_good_quantity_only()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-dmg-001",
            "po-rcv-dmg-001",
            "rcv-dmg",
            5m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-dmg-001");
        var line = receipt.Lines.Single();

        var updateLineRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}",
            token);
        updateLineRequest.Content = JsonContent.Create(new UpdateReceivingReceiptLineRequest(4m));
        (await _supplyarrClient.SendAsync(updateLineRequest)).EnsureSuccessStatusCode();

        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/exceptions",
            token);
        createExceptionRequest.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("damage", 1m, "Carton crushed in transit"));
        (await _supplyarrClient.SendAsync(createExceptionRequest)).EnsureSuccessStatusCode();

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postRequest)).EnsureSuccessStatusCode();

        var stockLevels = await ListStockAsync(token, part.PartId, bin.BinId);
        Assert.Equal(4m, stockLevels[0].QuantityOnHand);
    }

    [Fact]
    public async Task Receiving_short_shipment_creates_open_backorder()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-bo-short-001",
            "po-bo-short-001",
            "bo-short",
            5m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-bo-short-001");
        var line = receipt.Lines.Single();

        var updateLineRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}",
            token);
        updateLineRequest.Content = JsonContent.Create(new UpdateReceivingReceiptLineRequest(3m));
        (await _supplyarrClient.SendAsync(updateLineRequest)).EnsureSuccessStatusCode();

        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/exceptions",
            token);
        createExceptionRequest.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("short", 2m, "Vendor shorted shipment"));
        (await _supplyarrClient.SendAsync(createExceptionRequest)).EnsureSuccessStatusCode();

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postRequest)).EnsureSuccessStatusCode();

        var backorders = await ListBackordersAsync(token, purchaseOrder.PurchaseOrderId);
        Assert.Single(backorders);
        Assert.Equal("open", backorders[0].Status);
        Assert.Equal("receipt_post", backorders[0].SourceType);
        Assert.Equal(2m, backorders[0].QuantityBackordered);
        Assert.Equal(purchaseOrder.PurchaseRequestId, backorders[0].PurchaseRequestId);
        Assert.Equal(line.PurchaseOrderLineId, backorders[0].PurchaseOrderLineId);
    }

    [Fact]
    public async Task Backorder_fulfilled_when_po_line_fully_received()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-bo-full-001",
            "po-bo-full-001",
            "bo-full",
            4m);

        var firstReceipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-bo-full-001");
        var firstLine = firstReceipt.Lines.Single();

        var updateFirst = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{firstReceipt.ReceivingReceiptId}/lines/{firstLine.LineId}",
            token);
        updateFirst.Content = JsonContent.Create(new UpdateReceivingReceiptLineRequest(1m));
        (await _supplyarrClient.SendAsync(updateFirst)).EnsureSuccessStatusCode();

        var shortException = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{firstReceipt.ReceivingReceiptId}/lines/{firstLine.LineId}/exceptions",
            token);
        shortException.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("short", 3m, "Initial partial delivery"));
        (await _supplyarrClient.SendAsync(shortException)).EnsureSuccessStatusCode();

        var postFirst = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{firstReceipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postFirst)).EnsureSuccessStatusCode();

        var openBackorders = await ListBackordersAsync(token, purchaseOrder.PurchaseOrderId, "open");
        Assert.Single(openBackorders);
        Assert.Equal(3m, openBackorders[0].QuantityBackordered);

        var secondReceipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-bo-full-002");
        var secondLine = secondReceipt.Lines.Single();
        Assert.Equal(3m, secondLine.QuantityExpected);

        var postSecond = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{secondReceipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postSecond)).EnsureSuccessStatusCode();

        var fulfilled = await ListBackordersAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            "fulfilled");
        Assert.Single(fulfilled);
        Assert.Equal(openBackorders[0].BackorderId, fulfilled[0].BackorderId);
        Assert.Equal(3m, fulfilled[0].QuantityFulfilled);
    }

    [Fact]
    public async Task Backorder_manual_create_and_cancel()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, _, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-bo-manual-001",
            "po-bo-manual-001",
            "bo-manual",
            2m);

        var poLineId = purchaseOrder.Lines[0].LineId;
        var createRequest = Authorized(
            HttpMethod.Post,
            $"/api/backorders/from-purchase-order-line/{poLineId}",
            token);
        createRequest.Content = JsonContent.Create(
            new CreateBackorderFromPurchaseOrderLineRequest(
                "bo-manual-001",
                2m,
                null,
                "Vendor confirmed delay"));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<BackorderResponse>())!;

        Assert.Equal("open", created.Status);
        Assert.Equal("purchase_order_line", created.SourceType);
        Assert.Equal(2m, created.QuantityBackordered);
        Assert.Equal(purchaseOrder.PurchaseRequestId, created.PurchaseRequestId);

        var cancelRequest = Authorized(
            HttpMethod.Post,
            $"/api/backorders/{created.BackorderId}/cancel",
            token);
        cancelRequest.Content = JsonContent.Create(new CancelBackorderRequest("Sourced elsewhere"));
        var cancelResponse = await _supplyarrClient.SendAsync(cancelRequest);
        cancelResponse.EnsureSuccessStatusCode();
        var cancelled = (await cancelResponse.Content.ReadFromJsonAsync<BackorderResponse>())!;
        Assert.Equal("cancelled", cancelled.Status);
        Assert.Equal("Sourced elsewhere", cancelled.CancellationReason);
    }

    [Fact]
    public async Task Return_from_po_line_post_decrements_stock()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-ret-po-001",
            "po-ret-po-001",
            "ret-po",
            5m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-ret-po-001");
        var line = receipt.Lines.Single();

        var updateLineRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}",
            token);
        updateLineRequest.Content = JsonContent.Create(new UpdateReceivingReceiptLineRequest(4m));
        (await _supplyarrClient.SendAsync(updateLineRequest)).EnsureSuccessStatusCode();

        var shortException = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/exceptions",
            token);
        shortException.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("short", 1m, "Keeping one unit for return test"));
        (await _supplyarrClient.SendAsync(shortException)).EnsureSuccessStatusCode();

        var postReceiptRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postReceiptRequest)).EnsureSuccessStatusCode();

        var stockBefore = await ListStockAsync(token, part.PartId, bin.BinId);
        Assert.Equal(4m, stockBefore[0].QuantityOnHand);

        var poLineId = purchaseOrder.Lines[0].LineId;
        var createReturnRequest = Authorized(
            HttpMethod.Post,
            $"/api/returns/from-purchase-order-line/{poLineId}",
            token);
        createReturnRequest.Content = JsonContent.Create(
            new CreateVendorReturnFromPurchaseOrderLineRequest(
                "ret-po-001",
                bin.BinId,
                2m,
                "RMA-PO-1001",
                "Defective batch"));
        var createReturnResponse = await _supplyarrClient.SendAsync(createReturnRequest);
        createReturnResponse.EnsureSuccessStatusCode();
        var created = (await createReturnResponse.Content.ReadFromJsonAsync<VendorReturnResponse>())!;

        Assert.Equal("draft", created.Status);
        Assert.Equal("purchase_order_line", created.SourceType);
        Assert.Equal("RMA-PO-1001", created.RmaNumber);
        Assert.Equal(purchaseOrder.PurchaseOrderId, created.PurchaseOrderId);
        Assert.Equal(purchaseOrder.PurchaseRequestId, created.PurchaseRequestId);

        var postReturnRequest = Authorized(
            HttpMethod.Post,
            $"/api/returns/{created.ReturnId}/post",
            token);
        (await _supplyarrClient.SendAsync(postReturnRequest)).EnsureSuccessStatusCode();

        var posted = await GetVendorReturnAsync(token, created.ReturnId);
        Assert.Equal("posted", posted.Status);

        var stockAfter = await ListStockAsync(token, part.PartId, bin.BinId);
        Assert.Equal(2m, stockAfter[0].QuantityOnHand);
    }

    [Fact]
    public async Task Return_from_stock_post_decrements_stock()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-ret-stk-001",
            "po-ret-stk-001",
            "ret-stk",
            1m);

        var upsertStockRequest = Authorized(HttpMethod.Post, "/api/inventory/stock", token);
        upsertStockRequest.Content = JsonContent.Create(
            new UpsertPartStockLevelRequest(part.PartId, bin.BinId, 6m));
        (await _supplyarrClient.SendAsync(upsertStockRequest)).EnsureSuccessStatusCode();

        var purchaseOrderDetail = await GetPurchaseOrderAsync(token, purchaseOrder.PurchaseOrderId);

        var createReturnRequest = Authorized(HttpMethod.Post, "/api/returns/from-stock", token);
        createReturnRequest.Content = JsonContent.Create(
            new CreateVendorReturnFromStockRequest(
                "ret-stk-001",
                purchaseOrderDetail.VendorPartyId,
                bin.BinId,
                "RMA-STK-2001",
                "Overstock return",
                [new CreateVendorReturnFromStockLineRequest(part.PartId, 3m, null)]));
        var createReturnResponse = await _supplyarrClient.SendAsync(createReturnRequest);
        createReturnResponse.EnsureSuccessStatusCode();
        var created = (await createReturnResponse.Content.ReadFromJsonAsync<VendorReturnResponse>())!;

        Assert.Equal("stock", created.SourceType);
        Assert.Equal("RMA-STK-2001", created.RmaNumber);
        Assert.Single(created.Lines);
        Assert.Equal(3m, created.Lines[0].Quantity);

        var postReturnRequest = Authorized(
            HttpMethod.Post,
            $"/api/returns/{created.ReturnId}/post",
            token);
        (await _supplyarrClient.SendAsync(postReturnRequest)).EnsureSuccessStatusCode();

        var stockAfter = await ListStockAsync(token, part.PartId, bin.BinId);
        Assert.Equal(3m, stockAfter[0].QuantityOnHand);
    }

    [Fact]
    public async Task Return_draft_cancel()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-ret-can-001",
            "po-ret-can-001",
            "ret-can",
            1m);

        var upsertStockRequest = Authorized(HttpMethod.Post, "/api/inventory/stock", token);
        upsertStockRequest.Content = JsonContent.Create(
            new UpsertPartStockLevelRequest(part.PartId, bin.BinId, 2m));
        (await _supplyarrClient.SendAsync(upsertStockRequest)).EnsureSuccessStatusCode();

        var purchaseOrderDetail = await GetPurchaseOrderAsync(token, purchaseOrder.PurchaseOrderId);

        var createReturnRequest = Authorized(HttpMethod.Post, "/api/returns/from-stock", token);
        createReturnRequest.Content = JsonContent.Create(
            new CreateVendorReturnFromStockRequest(
                "ret-can-001",
                purchaseOrderDetail.VendorPartyId,
                bin.BinId,
                null,
                null,
                [new CreateVendorReturnFromStockLineRequest(part.PartId, 1m, null)]));
        var createReturnResponse = await _supplyarrClient.SendAsync(createReturnRequest);
        createReturnResponse.EnsureSuccessStatusCode();
        var created = (await createReturnResponse.Content.ReadFromJsonAsync<VendorReturnResponse>())!;

        var cancelRequest = Authorized(
            HttpMethod.Post,
            $"/api/returns/{created.ReturnId}/cancel",
            token);
        cancelRequest.Content = JsonContent.Create(new CancelVendorReturnRequest("Vendor declined RMA"));
        var cancelResponse = await _supplyarrClient.SendAsync(cancelRequest);
        cancelResponse.EnsureSuccessStatusCode();
        var cancelled = (await cancelResponse.Content.ReadFromJsonAsync<VendorReturnResponse>())!;

        Assert.Equal("cancelled", cancelled.Status);
        Assert.Equal("Vendor declined RMA", cancelled.CancellationReason);

        var stockAfter = await ListStockAsync(token, part.PartId, bin.BinId);
        Assert.Equal(2m, stockAfter[0].QuantityOnHand);
    }

    [Fact]
    public async Task Pricing_and_lead_time_snapshots_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var link = await CreatePartWithVendorLinkAsync(token, "snap-vendor", "snap-part", "SNAP-V-001");

        var createPricingRequest = Authorized(HttpMethod.Post, "/api/pricing-snapshots", token);
        createPricingRequest.Content = JsonContent.Create(new CreatePricingSnapshotRequest(
            "price-snap-001",
            link.LinkId,
            24.99m,
            "USD",
            5m,
            null,
            "manual",
            "Initial catalog price"));
        var createPricingResponse = await _supplyarrClient.SendAsync(createPricingRequest);
        createPricingResponse.EnsureSuccessStatusCode();
        var pricing = (await createPricingResponse.Content.ReadFromJsonAsync<PricingSnapshotResponse>())!;
        Assert.Equal("price-snap-001", pricing.SnapshotKey);
        Assert.True(pricing.IsCurrent);
        Assert.Equal(24.99m, pricing.UnitPrice);

        var createLeadTimeRequest = Authorized(HttpMethod.Post, "/api/lead-time-snapshots", token);
        createLeadTimeRequest.Content = JsonContent.Create(new CreateLeadTimeSnapshotRequest(
            "lt-snap-001",
            link.LinkId,
            10,
            null,
            "quote",
            "Vendor quote May 2026"));
        var createLeadTimeResponse = await _supplyarrClient.SendAsync(createLeadTimeRequest);
        createLeadTimeResponse.EnsureSuccessStatusCode();
        var leadTime = (await createLeadTimeResponse.Content.ReadFromJsonAsync<LeadTimeSnapshotResponse>())!;
        Assert.Equal(10, leadTime.LeadTimeDays);
        Assert.True(leadTime.IsCurrent);

        var listPricingRequest = Authorized(
            HttpMethod.Get,
            $"/api/pricing-snapshots?partVendorLinkId={link.LinkId}&asOf={Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"))}",
            token);
        var listPricingResponse = await _supplyarrClient.SendAsync(listPricingRequest);
        listPricingResponse.EnsureSuccessStatusCode();
        var pricingList = (await listPricingResponse.Content.ReadFromJsonAsync<List<PricingSnapshotResponse>>())!;
        Assert.Single(pricingList);

        var supersedePricingRequest = Authorized(HttpMethod.Post, "/api/pricing-snapshots", token);
        supersedePricingRequest.Content = JsonContent.Create(new CreatePricingSnapshotRequest(
            "price-snap-002",
            link.LinkId,
            22.50m,
            "USD",
            null,
            DateTimeOffset.UtcNow,
            "contract",
            "Contract renewal"));
        var supersedePricingResponse = await _supplyarrClient.SendAsync(supersedePricingRequest);
        supersedePricingResponse.EnsureSuccessStatusCode();

        var allPricingRequest = Authorized(
            HttpMethod.Get,
            $"/api/pricing-snapshots?partVendorLinkId={link.LinkId}",
            token);
        var allPricingResponse = await _supplyarrClient.SendAsync(allPricingRequest);
        allPricingResponse.EnsureSuccessStatusCode();
        var allPricing = (await allPricingResponse.Content.ReadFromJsonAsync<List<PricingSnapshotResponse>>())!;
        Assert.Equal(2, allPricing.Count);
        Assert.Single(allPricing, x => x.IsCurrent);
        Assert.NotNull(allPricing.Single(x => x.SnapshotKey == "price-snap-001").EffectiveTo);
    }

    [Fact]
    public async Task Pricing_snapshot_create_denied_without_manage_role()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var adminToken = token;
        var link = await CreatePartWithVendorLinkAsync(adminToken, "snap-deny-v", "snap-deny-p", "SNAP-D-001");

        var clerkToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_clerk");
        var request = Authorized(HttpMethod.Post, "/api/pricing-snapshots", clerkToken);
        request.Content = JsonContent.Create(new CreatePricingSnapshotRequest(
            "price-deny-001",
            link.LinkId,
            10m,
            "USD",
            null,
            null,
            null,
            null));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Availability_snapshots_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var link = await CreatePartWithVendorLinkAsync(token, "avail-vendor", "avail-part", "AVAIL-V-001");

        var createRequest = Authorized(HttpMethod.Post, "/api/availability-snapshots", token);
        createRequest.Content = JsonContent.Create(new CreateAvailabilitySnapshotRequest(
            "avail-snap-001",
            link.LinkId,
            120m,
            "in_stock",
            null,
            "vendor_feed",
            "Vendor portal May 2026"));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var snapshot = (await createResponse.Content.ReadFromJsonAsync<AvailabilitySnapshotResponse>())!;
        Assert.Equal("avail-snap-001", snapshot.SnapshotKey);
        Assert.True(snapshot.IsCurrent);
        Assert.Equal(120m, snapshot.QuantityAvailable);
        Assert.Equal("in_stock", snapshot.AvailabilityStatus);

        var listRequest = Authorized(
            HttpMethod.Get,
            $"/api/availability-snapshots?partVendorLinkId={link.LinkId}&asOf={Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"))}",
            token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<List<AvailabilitySnapshotResponse>>())!;
        Assert.Single(list);

        var supersedeRequest = Authorized(HttpMethod.Post, "/api/availability-snapshots", token);
        supersedeRequest.Content = JsonContent.Create(new CreateAvailabilitySnapshotRequest(
            "avail-snap-002",
            link.LinkId,
            null,
            "backorder",
            DateTimeOffset.UtcNow,
            "manual",
            "Vendor reported backorder"));
        var supersedeResponse = await _supplyarrClient.SendAsync(supersedeRequest);
        supersedeResponse.EnsureSuccessStatusCode();

        var allRequest = Authorized(
            HttpMethod.Get,
            $"/api/availability-snapshots?partVendorLinkId={link.LinkId}",
            token);
        var allResponse = await _supplyarrClient.SendAsync(allRequest);
        allResponse.EnsureSuccessStatusCode();
        var all = (await allResponse.Content.ReadFromJsonAsync<List<AvailabilitySnapshotResponse>>())!;
        Assert.Equal(2, all.Count);
        Assert.Single(all, x => x.IsCurrent);
        Assert.NotNull(all.Single(x => x.SnapshotKey == "avail-snap-001").EffectiveTo);
    }

    [Fact]
    public async Task Availability_snapshot_create_denied_without_manage_role()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var adminToken = token;
        var link = await CreatePartWithVendorLinkAsync(adminToken, "avail-deny-v", "avail-deny-p", "AVAIL-D-001");

        var clerkToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_clerk");
        var request = Authorized(HttpMethod.Post, "/api/availability-snapshots", clerkToken);
        request.Content = JsonContent.Create(new CreateAvailabilitySnapshotRequest(
            "avail-deny-001",
            link.LinkId,
            5m,
            "limited",
            null,
            null,
            null));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reorder_evaluation_suggests_low_stock_and_creates_draft_purchase_request()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "reorder-part-001",
            null,
            "Reorder Test Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "reorder-wh",
            "Reorder warehouse",
            "warehouse",
            string.Empty));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/inventory/locations/{location.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("reorder-bin", "A1"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var policyRequest = Authorized(
            HttpMethod.Put,
            $"/api/reorder-evaluation/parts/{part.PartId}/policy",
            token);
        policyRequest.Content = JsonContent.Create(new UpsertPartReorderPolicyRequest(10m, 24m));
        var policyResponse = await _supplyarrClient.SendAsync(policyRequest);
        policyResponse.EnsureSuccessStatusCode();

        var stockRequest = Authorized(HttpMethod.Post, "/api/inventory/stock", token);
        stockRequest.Content = JsonContent.Create(new UpsertPartStockLevelRequest(part.PartId, bin.BinId, 3m));
        var stockResponse = await _supplyarrClient.SendAsync(stockRequest);
        stockResponse.EnsureSuccessStatusCode();

        var evaluateRequest = Authorized(HttpMethod.Get, "/api/reorder-evaluation", token);
        var evaluateResponse = await _supplyarrClient.SendAsync(evaluateRequest);
        evaluateResponse.EnsureSuccessStatusCode();
        var evaluation = (await evaluateResponse.Content.ReadFromJsonAsync<ReorderEvaluationResponse>())!;
        Assert.Contains(evaluation.Suggestions, x => x.PartId == part.PartId && x.SuggestedOrderQuantity == 24m);

        var createPrRequest = Authorized(HttpMethod.Post, "/api/reorder-evaluation/create-purchase-request", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestFromReorderRequest(
            "reorder-pr-001",
            "Reorder restock",
            "Created from reorder evaluation",
            [part.PartId]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
        Assert.Equal("draft", purchaseRequest.Status);
        Assert.Single(purchaseRequest.Lines);
        Assert.Equal(24m, purchaseRequest.Lines[0].QuantityRequested);
    }

    [Fact]
    public async Task Reorder_policy_upsert_denied_for_clerk_role()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "reorder-clerk-part",
            null,
            "Clerk policy part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var clerkToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_clerk");

        var request = Authorized(
            HttpMethod.Put,
            $"/api/reorder-evaluation/parts/{part.PartId}/policy",
            clerkToken);
        request.Content = JsonContent.Create(new UpsertPartReorderPolicyRequest(5m, null));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Me_forbids_users_without_supplyarr_entitlement_claim()
    {
        var token = CreateSupplyArrAccessToken(["nexarr"]);
        var request = Authorized(HttpMethod.Get, "/api/me", token);
        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<PartVendorLinkResponse> CreatePartWithVendorLinkAsync(
        string token,
        string vendorKey,
        string partKey,
        string vendorPartNumber)
    {
        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            vendorKey,
            $"{vendorKey} Vendor",
            $"{vendorKey} Vendor LLC",
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            partKey,
            null,
            $"{partKey} display",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var linkRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/vendor-links", token);
        linkRequest.Content = JsonContent.Create(new CreatePartVendorLinkRequest(
            vendor.PartyId,
            vendorPartNumber,
            true));
        var linkResponse = await _supplyarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();
        return (await linkResponse.Content.ReadFromJsonAsync<PartVendorLinkResponse>())!;
    }

    private async Task<(PartResponse Part, InventoryBinResponse Bin, PurchaseOrderResponse PurchaseOrder)>
        CreateIssuedPurchaseOrderAsync(
            string token,
            string purchaseRequestKey,
            string purchaseOrderKey,
            string locationKeyPrefix,
            decimal orderQuantity)
    {
        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"{locationKeyPrefix}-vendor",
            $"{locationKeyPrefix} Vendor",
            $"{locationKeyPrefix} Vendor LLC",
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"{locationKeyPrefix}-part",
            null,
            $"{locationKeyPrefix} Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            $"{locationKeyPrefix}-wh",
            $"{locationKeyPrefix} Warehouse",
            "warehouse",
            "200 Dock St"));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/inventory/locations/{location.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(
            new CreateInventoryBinRequest($"{locationKeyPrefix}-bin", $"{locationKeyPrefix} Bin"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            purchaseRequestKey,
            $"{locationKeyPrefix} request",
            string.Empty,
            vendor.PartyId,
            [new CreatePurchaseRequestLineRequest(part.PartId, orderQuantity, string.Empty)]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var approvePrRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            token);
        (await _supplyarrClient.SendAsync(approvePrRequest)).EnsureSuccessStatusCode();

        var createPoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/from-purchase-request/{purchaseRequest.PurchaseRequestId}",
            token);
        createPoRequest.Content = JsonContent.Create(
            new CreatePurchaseOrderFromPurchaseRequestRequest(purchaseOrderKey, null, null));
        var createPoResponse = await _supplyarrClient.SendAsync(createPoRequest);
        createPoResponse.EnsureSuccessStatusCode();
        var purchaseOrder = (await createPoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;

        var approvePoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/approve",
            token);
        (await _supplyarrClient.SendAsync(approvePoRequest)).EnsureSuccessStatusCode();

        var issueRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/issue",
            token);
        (await _supplyarrClient.SendAsync(issueRequest)).EnsureSuccessStatusCode();

        return (part, bin, purchaseOrder);
    }

    private async Task<ReceivingReceiptResponse> CreateDraftReceivingReceiptAsync(
        string token,
        Guid purchaseOrderId,
        Guid binId,
        string receiptKey)
    {
        var createReceiptRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/from-purchase-order/{purchaseOrderId}",
            token);
        createReceiptRequest.Content = JsonContent.Create(
            new CreateReceivingReceiptFromPurchaseOrderRequest(receiptKey, binId, null));
        var createReceiptResponse = await _supplyarrClient.SendAsync(createReceiptRequest);
        createReceiptResponse.EnsureSuccessStatusCode();
        return (await createReceiptResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
    }

    private async Task<List<PartStockLevelResponse>> ListStockAsync(
        string token,
        Guid partId,
        Guid binId)
    {
        var listStockRequest = Authorized(
            HttpMethod.Get,
            $"/api/inventory/stock?partId={partId}&binId={binId}",
            token);
        var listStockResponse = await _supplyarrClient.SendAsync(listStockRequest);
        listStockResponse.EnsureSuccessStatusCode();
        return (await listStockResponse.Content.ReadFromJsonAsync<List<PartStockLevelResponse>>())!;
    }

    private async Task<PurchaseOrderResponse> GetPurchaseOrderAsync(string token, Guid purchaseOrderId)
    {
        var getPoRequest = Authorized(
            HttpMethod.Get,
            $"/api/purchase-orders/{purchaseOrderId}",
            token);
        var getPoResponse = await _supplyarrClient.SendAsync(getPoRequest);
        getPoResponse.EnsureSuccessStatusCode();
        return (await getPoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
    }

    private async Task<List<BackorderResponse>> ListBackordersAsync(
        string token,
        Guid purchaseOrderId,
        string? status = null)
    {
        var query = $"/api/backorders?purchaseOrderId={purchaseOrderId}";
        if (!string.IsNullOrWhiteSpace(status))
        {
            query += $"&status={status}";
        }

        var listRequest = Authorized(HttpMethod.Get, query, token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        return (await listResponse.Content.ReadFromJsonAsync<List<BackorderResponse>>())!;
    }

    private async Task<VendorReturnResponse> GetVendorReturnAsync(string token, Guid returnId)
    {
        var getRequest = Authorized(HttpMethod.Get, $"/api/returns/{returnId}", token);
        var getResponse = await _supplyarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        return (await getResponse.Content.ReadFromJsonAsync<VendorReturnResponse>())!;
    }

    private async Task<string> RedeemSupplyArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("supplyarr", "http://localhost:5179/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-supplyarr-handoff-test",
            $"{productKey} SupplyArr Handoff Test",
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

    private string CreateSupplyArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<SupplyArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return token;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return login.AccessToken;
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
