using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupplyArrRedeemRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using SupplyArrMeResponse = SupplyArr.Api.Contracts.SupplyArrMeResponse;
using SupplierDirectoryMetadataResponse = SupplyArr.Api.Contracts.SupplierDirectoryMetadataResponse;
using SupplierResponse = SupplyArr.Api.Contracts.SupplierResponse;
using CreateSupplierRequest = SupplyArr.Api.Contracts.CreateSupplierRequest;
using CreateSupplierContactRequest = SupplyArr.Api.Contracts.CreateSupplierContactRequest;
using UpdateSupplierApprovalStatusRequest = SupplyArr.Api.Contracts.UpdateSupplierApprovalStatusRequest;
using PartCatalogResponse = SupplyArr.Api.Contracts.PartCatalogResponse;
using CreatePartCatalogRequest = SupplyArr.Api.Contracts.CreatePartCatalogRequest;
using PartResponse = SupplyArr.Api.Contracts.PartResponse;
using CreatePartRequest = SupplyArr.Api.Contracts.CreatePartRequest;
using CreatePartSourceRequest = SupplyArr.Api.Contracts.CreatePartSourceRequest;
using CreatePartSupplierLinkRequest = SupplyArr.Api.Contracts.CreatePartSupplierLinkRequest;
using PartSupplierLinkResponse = SupplyArr.Api.Contracts.PartSupplierLinkResponse;
using UpsertPartSupplierLinkCatalogPriceRequest = SupplyArr.Api.Contracts.UpsertPartSupplierLinkCatalogPriceRequest;
using InventoryLocationResponse = SupplyArr.Api.Contracts.InventoryLocationResponse;
using CreateInventoryLocationRequest = SupplyArr.Api.Contracts.CreateInventoryLocationRequest;
using SupplierComplianceDocumentResponse = SupplyArr.Api.Contracts.SupplierComplianceDocumentResponse;
using SupplierComplianceDocumentRegistrationRequest = SupplyArr.Api.Contracts.SupplierComplianceDocumentRegistrationRequest;
using ItemCategorySummaryResponse = SupplyArr.Api.Contracts.ItemCategorySummaryResponse;
using ManufacturerSummaryResponse = SupplyArr.Api.Contracts.ManufacturerSummaryResponse;
using SupplierItemResponse = SupplyArr.Api.Contracts.SupplierItemResponse;
using CreateSupplierItemRequest = SupplyArr.Api.Contracts.CreateSupplierItemRequest;
using CreateSupplierQuoteRequest = SupplyArr.Api.Contracts.CreateSupplierQuoteRequest;
using UpsertSupplierQuoteLineRequest = SupplyArr.Api.Contracts.UpsertSupplierQuoteLineRequest;
using CreateRfqRequest = SupplyArr.Api.Contracts.CreateRfqRequest;
using CreateRfqLineRequest = SupplyArr.Api.Contracts.CreateRfqLineRequest;
using InviteRfqSuppliersRequest = SupplyArr.Api.Contracts.InviteRfqSuppliersRequest;
using SupplierQuoteResponse = SupplyArr.Api.Contracts.SupplierQuoteResponse;
using CreateSupplierWarrantyClaimRequest = SupplyArr.Api.Contracts.CreateSupplierWarrantyClaimRequest;
using SubmitWarrantyClaimRequest = SupplyArr.Api.Contracts.SubmitWarrantyClaimRequest;
using SupplyArrSessionBootstrapResponse = SupplyArr.Api.Contracts.SupplyArrSessionBootstrapResponse;
using ApprovalQueueItemResponse = SupplyArr.Api.Contracts.ApprovalQueueItemResponse;
using StockTransactionItemResponse = SupplyArr.Api.Contracts.StockTransactionItemResponse;
using CreateStockTransactionRequest = SupplyArr.Api.Contracts.CreateStockTransactionRequest;
using CycleCountItemResponse = SupplyArr.Api.Contracts.CycleCountItemResponse;
using CreateCycleCountRequest = SupplyArr.Api.Contracts.CreateCycleCountRequest;
using SubstitutionItemResponse = SupplyArr.Api.Contracts.SubstitutionItemResponse;
using CreateSupplyDocumentRequest = SupplyArr.Api.Contracts.CreateSupplyDocumentRequest;
using SupplyDocumentItemResponse = SupplyArr.Api.Contracts.SupplyDocumentItemResponse;
using ContractSnapshotItemResponse = SupplyArr.Api.Contracts.ContractSnapshotItemResponse;
using CreateSupplyContractRequest = SupplyArr.Api.Contracts.CreateSupplyContractRequest;
using SupplyContractResponse = SupplyArr.Api.Contracts.SupplyContractResponse;
using ImportOptionResponse = SupplyArr.Api.Contracts.ImportOptionResponse;
using ImportHistoryListResponse = SupplyArr.Api.Contracts.ImportHistoryListResponse;
using ImportErrorExportIssueRequest = SupplyArr.Api.Contracts.ImportErrorExportIssueRequest;
using ImportErrorExportRequest = SupplyArr.Api.Contracts.ImportErrorExportRequest;
using ImportFieldMappingRequest = SupplyArr.Api.Contracts.ImportFieldMappingRequest;
using ImportFieldMappingResponse = SupplyArr.Api.Contracts.ImportFieldMappingResponse;
using PartCatalogCsvImportRequest = SupplyArr.Api.Contracts.PartCatalogCsvImportRequest;
using PartCatalogCsvImportResponse = SupplyArr.Api.Contracts.PartCatalogCsvImportResponse;
using SupplierCatalogCsvImportRequest = SupplyArr.Api.Contracts.SupplierCatalogCsvImportRequest;
using SupplierCatalogCsvImportResponse = SupplyArr.Api.Contracts.SupplierCatalogCsvImportResponse;
using SupplierDocumentsCsvImportRequest = SupplyArr.Api.Contracts.SupplierDocumentsCsvImportRequest;
using SupplierDocumentsCsvImportResponse = SupplyArr.Api.Contracts.SupplierDocumentsCsvImportResponse;
using InventoryCountsCsvImportRequest = SupplyArr.Api.Contracts.InventoryCountsCsvImportRequest;
using InventoryCountsCsvImportResponse = SupplyArr.Api.Contracts.InventoryCountsCsvImportResponse;
using PriceListCsvImportRequest = SupplyArr.Api.Contracts.PriceListCsvImportRequest;
using PriceListCsvImportResponse = SupplyArr.Api.Contracts.PriceListCsvImportResponse;
using LeadTimeListCsvImportRequest = SupplyArr.Api.Contracts.LeadTimeListCsvImportRequest;
using LeadTimeListCsvImportResponse = SupplyArr.Api.Contracts.LeadTimeListCsvImportResponse;
using AvailabilityListCsvImportRequest = SupplyArr.Api.Contracts.AvailabilityListCsvImportRequest;
using AvailabilityListCsvImportResponse = SupplyArr.Api.Contracts.AvailabilityListCsvImportResponse;
using ContractsCsvImportRequest = SupplyArr.Api.Contracts.ContractsCsvImportRequest;
using ContractsCsvImportResponse = SupplyArr.Api.Contracts.ContractsCsvImportResponse;
using SuppliersCsvImportRequest = SupplyArr.Api.Contracts.SuppliersCsvImportRequest;
using SuppliersCsvImportResponse = SupplyArr.Api.Contracts.SuppliersCsvImportResponse;
using ContactsCsvImportRequest = SupplyArr.Api.Contracts.ContactsCsvImportRequest;
using ContactsCsvImportResponse = SupplyArr.Api.Contracts.ContactsCsvImportResponse;
using OpenPurchaseOrdersCsvImportRequest = SupplyArr.Api.Contracts.OpenPurchaseOrdersCsvImportRequest;
using OpenPurchaseOrdersCsvImportResponse = SupplyArr.Api.Contracts.OpenPurchaseOrdersCsvImportResponse;
using PurchaseHistoryCsvImportRequest = SupplyArr.Api.Contracts.PurchaseHistoryCsvImportRequest;
using PurchaseHistoryCsvImportResponse = SupplyArr.Api.Contracts.PurchaseHistoryCsvImportResponse;
using ExportOptionResponse = SupplyArr.Api.Contracts.ExportOptionResponse;
using AdminOverviewResponse = SupplyArr.Api.Contracts.AdminOverviewResponse;
using CreatePartManufacturerAliasRequest = SupplyArr.Api.Contracts.CreatePartManufacturerAliasRequest;
using InventoryBinResponse = SupplyArr.Api.Contracts.InventoryBinResponse;
using CreateInventoryBinRequest = SupplyArr.Api.Contracts.CreateInventoryBinRequest;
using PartStockLevelResponse = SupplyArr.Api.Contracts.PartStockLevelResponse;
using UpsertPartStockLevelRequest = SupplyArr.Api.Contracts.UpsertPartStockLevelRequest;
using StockReservationResponse = SupplyArr.Api.Contracts.StockReservationResponse;
using CreateStockReservationRequest = SupplyArr.Api.Contracts.CreateStockReservationRequest;
using ReleaseStockReservationRequest = SupplyArr.Api.Contracts.ReleaseStockReservationRequest;
using WmsMovementResponse = SupplyArr.Api.Contracts.WmsMovementResponse;
using WmsStockLedgerEntryResponse = SupplyArr.Api.Contracts.WmsStockLedgerEntryResponse;
using ReserveStockRequest = SupplyArr.Api.Contracts.ReserveStockRequest;
using PickStockRequest = SupplyArr.Api.Contracts.PickStockRequest;
using ShipStockRequest = SupplyArr.Api.Contracts.ShipStockRequest;
using CancelStockMovementRequest = SupplyArr.Api.Contracts.CancelStockMovementRequest;
using RfqResponse = SupplyArr.Api.Contracts.RfqResponse;
using WarrantyClaimResponse = SupplyArr.Api.Contracts.WarrantyClaimResponse;
using PurchaseRequestResponse = SupplyArr.Api.Contracts.PurchaseRequestResponse;
using CreatePurchaseRequestRequest = SupplyArr.Api.Contracts.CreatePurchaseRequestRequest;
using CreatePurchaseRequestLineRequest = SupplyArr.Api.Contracts.CreatePurchaseRequestLineRequest;
using RejectPurchaseRequestRequest = SupplyArr.Api.Contracts.RejectPurchaseRequestRequest;
using PurchaseOrderResponse = SupplyArr.Api.Contracts.PurchaseOrderResponse;
using CreatePurchaseOrderFromPurchaseRequestRequest = SupplyArr.Api.Contracts.CreatePurchaseOrderFromPurchaseRequestRequest;
using ReceivingReceiptResponse = SupplyArr.Api.Contracts.ReceivingReceiptResponse;
using ReceivingExceptionResponse = SupplyArr.Api.Contracts.ReceivingExceptionResponse;
using CreateReceivingReceiptFromPurchaseOrderRequest = SupplyArr.Api.Contracts.CreateReceivingReceiptFromPurchaseOrderRequest;
using CreateReceivingExceptionRequest = SupplyArr.Api.Contracts.CreateReceivingExceptionRequest;
using CancelReceivingExceptionRequest = SupplyArr.Api.Contracts.CancelReceivingExceptionRequest;
using ReopenReceivingExceptionRequest = SupplyArr.Api.Contracts.ReopenReceivingExceptionRequest;
using UpdateReceivingReceiptLineRequest = SupplyArr.Api.Contracts.UpdateReceivingReceiptLineRequest;
using UpdateReceivingPackingSlipRequest = SupplyArr.Api.Contracts.UpdateReceivingPackingSlipRequest;
using UpdateReceivingInvoiceRequest = SupplyArr.Api.Contracts.UpdateReceivingInvoiceRequest;
using UpdateReceivingReceiptLineTrackingRequest = SupplyArr.Api.Contracts.UpdateReceivingReceiptLineTrackingRequest;
using UpdateReceivingInventoryBinRequest = SupplyArr.Api.Contracts.UpdateReceivingInventoryBinRequest;
using UpdateReceivingReceiptLineConditionRequest = SupplyArr.Api.Contracts.UpdateReceivingReceiptLineConditionRequest;
using EmergencyPurchaseResponse = SupplyArr.Api.Contracts.EmergencyPurchaseResponse;
using CreateEmergencyPurchaseRequest = SupplyArr.Api.Contracts.CreateEmergencyPurchaseRequest;
using ExpeditedSubmitEmergencyPurchaseRequest = SupplyArr.Api.Contracts.ExpeditedSubmitEmergencyPurchaseRequest;
using ManagerOverrideApproveEmergencyPurchaseRequest = SupplyArr.Api.Contracts.ManagerOverrideApproveEmergencyPurchaseRequest;
using BackorderResponse = SupplyArr.Api.Contracts.BackorderResponse;
using CreateBackorderFromPurchaseOrderLineRequest = SupplyArr.Api.Contracts.CreateBackorderFromPurchaseOrderLineRequest;
using CancelBackorderRequest = SupplyArr.Api.Contracts.CancelBackorderRequest;
using SupplierReturnResponse = SupplyArr.Api.Contracts.SupplierReturnResponse;
using CreateSupplierReturnFromStockLineRequest = SupplyArr.Api.Contracts.CreateSupplierReturnFromStockLineRequest;
using CreateSupplierReturnFromStockRequest = SupplyArr.Api.Contracts.CreateSupplierReturnFromStockRequest;
using CreateSupplierReturnFromPurchaseOrderLineRequest = SupplyArr.Api.Contracts.CreateSupplierReturnFromPurchaseOrderLineRequest;
using CancelSupplierReturnRequest = SupplyArr.Api.Contracts.CancelSupplierReturnRequest;
using PricingSnapshotResponse = SupplyArr.Api.Contracts.PricingSnapshotResponse;
using CreatePricingSnapshotRequest = SupplyArr.Api.Contracts.CreatePricingSnapshotRequest;
using LeadTimeSnapshotResponse = SupplyArr.Api.Contracts.LeadTimeSnapshotResponse;
using CreateLeadTimeSnapshotRequest = SupplyArr.Api.Contracts.CreateLeadTimeSnapshotRequest;
using AvailabilitySnapshotResponse = SupplyArr.Api.Contracts.AvailabilitySnapshotResponse;
using CreateAvailabilitySnapshotRequest = SupplyArr.Api.Contracts.CreateAvailabilitySnapshotRequest;
using SupplierRestrictionResponse = SupplyArr.Api.Contracts.SupplierRestrictionResponse;
using CreateSupplierRestrictionRequest = SupplyArr.Api.Contracts.CreateSupplierRestrictionRequest;
using SupplierOnboardingResponse = SupplyArr.Api.Contracts.SupplierOnboardingResponse;
using StartSupplierOnboardingRequest = SupplyArr.Api.Contracts.StartSupplierOnboardingRequest;
using SubmitSupplierOnboardingForReviewRequest = SupplyArr.Api.Contracts.SubmitSupplierOnboardingForReviewRequest;
using SupplierIncidentResponse = SupplyArr.Api.Contracts.SupplierIncidentResponse;
using CreateSupplierIncidentRequest = SupplyArr.Api.Contracts.CreateSupplierIncidentRequest;
using ReorderEvaluationResponse = SupplyArr.Api.Contracts.ReorderEvaluationResponse;
using UpsertPartReorderPolicyRequest = SupplyArr.Api.Contracts.UpsertPartReorderPolicyRequest;
using CreatePurchaseRequestFromReorderRequest = SupplyArr.Api.Contracts.CreatePurchaseRequestFromReorderRequest;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
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
    private RecordingTrainArrQualificationCheckHandler _trainarrQualificationHandler = null!;
    private RecordingComplianceCoreHandler _complianceCoreHandler = null!;
    private RecordingStaffArrSiteLookupHandler _staffarrSiteLookupHandler = null!;
    private readonly Guid _staffarrSiteOrgUnitId = Guid.Parse("7d96aa4b-1116-4a27-9660-b1f64dd03261");

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
        _trainarrQualificationHandler = new RecordingTrainArrQualificationCheckHandler();
        _complianceCoreHandler = new RecordingComplianceCoreHandler();
        _staffarrSiteLookupHandler = new RecordingStaffArrSiteLookupHandler(_staffarrSiteOrgUnitId);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _serviceToken);
            builder.UseSetting("TrainArr:BaseUrl", "http://trainarr.test");
            builder.UseSetting("TrainArr:ServiceToken", "supplyarr-to-trainarr-token");
            builder.UseSetting("TrainArr:ReceivingQualificationKey", "supplyarr_receiving");
            builder.UseSetting("TrainArr:ReceivingRulePackKey", "supplyarr_receiving_authorization");
            builder.UseSetting("ComplianceCore:BaseUrl", "http://compliancecore.test");
            builder.UseSetting("ComplianceCore:ServiceToken", "supplyarr-to-compliancecore-token");
            builder.UseSetting("ComplianceCore:SupplierUseActionKey", "can-use-supplier");
            builder.UseSetting("ComplianceCore:SupplierUseWorkflowKey", "can_use_supplier");
            builder.UseSetting("ComplianceCore:SupplierUseActivityContextKey", "purchase_order_issue");
            builder.UseSetting("StaffArr:BaseUrl", "http://staffarr.test");
            builder.UseSetting("StaffArr:ServiceToken", "supplyarr-to-staffarr-token");
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
                services.AddHttpClient<TrainArrQualificationCheckClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _trainarrQualificationHandler);
                services.AddHttpClient<ComplianceCoreFactPublicationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreHandler);
                services.AddHttpClient<ComplianceCoreSupplierUseGateClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreHandler);
                services.AddHttpClient<StaffArrSiteLookupClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrSiteLookupHandler);
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
        Assert.Contains("supplyarr", session.LaunchableProductKeys);
        Assert.Contains("ledgarr", session.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", session.LaunchableProductKeys);

        var meRequest = Authorized(HttpMethod.Get, "/api/me", session.AccessToken);
        var meResponse = await _supplyarrClient.SendAsync(meRequest);
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<SupplyArrMeResponse>();
        Assert.NotNull(me);
        Assert.Contains("supplyarr", me.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", me.LaunchableProductKeys);
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
        Assert.Contains("supplyarr", session.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", session.LaunchableProductKeys);
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
        Assert.Contains("supplyarr", me.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", me.LaunchableProductKeys);

        var sessionResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/session", session.AccessToken));
        sessionResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task V1_health_endpoint_returns_ok()
    {
        var response = await _supplyarrClient.GetAsync("/api/v1/health");
        response.EnsureSuccessStatusCode();
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
    public async Task Supplier_directory_crud_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "acme-parts",
            null,
            null,
            "Acme Parts Co.",
            "Acme Parts Company LLC",
            "12-3456789",
            "Preferred OEM supplier",
            ["parts", "products"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;
        Assert.Equal("pending", supplier.ApprovalStatus);

        var listSuppliersRequest = Authorized(HttpMethod.Get, "/api/suppliers", token);
        var listSuppliersResponse = await _supplyarrClient.SendAsync(listSuppliersRequest);
        listSuppliersResponse.EnsureSuccessStatusCode();
        var suppliers = (await listSuppliersResponse.Content.ReadFromJsonAsync<List<SupplierResponse>>())!;
        Assert.Contains(suppliers, x => x.SupplierId == supplier.SupplierId);

        var contactRequest = Authorized(HttpMethod.Post, $"/api/suppliers/{supplier.SupplierId}/contacts", token);
        contactRequest.Content = JsonContent.Create(new CreateSupplierContactRequest(
            "Jordan Lee",
            "jordan@acmeparts.example",
            "555-0100",
            "Account manager",
            true));
        var contactResponse = await _supplyarrClient.SendAsync(contactRequest);
        contactResponse.EnsureSuccessStatusCode();

        var approveRequest = Authorized(HttpMethod.Patch, $"/api/suppliers/{supplier.SupplierId}/approval-status", token);
        approveRequest.Content = JsonContent.Create(new UpdateSupplierApprovalStatusRequest("approved"));
        var approveResponse = await _supplyarrClient.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;
        Assert.Equal("approved", approved.ApprovalStatus);
        Assert.Single(approved.Contacts);
    }

    [Fact]
    public async Task Supplier_directory_v1_crud_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/v1/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "acme-parts-v1",
            null,
            null,
            "Acme Parts Co V1",
            "Acme Parts Company V1 LLC",
            "98-7654321",
            "Preferred OEM supplier v1",
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var listSuppliersResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/suppliers", token));
        listSuppliersResponse.EnsureSuccessStatusCode();
        var suppliers = (await listSuppliersResponse.Content.ReadFromJsonAsync<List<SupplierResponse>>())!;
        Assert.Contains(suppliers, x => x.SupplierId == supplier.SupplierId);

        var contactRequest = Authorized(HttpMethod.Post, $"/api/v1/suppliers/{supplier.SupplierId}/contacts", token);
        contactRequest.Content = JsonContent.Create(new CreateSupplierContactRequest(
            "Taylor Reed",
            "taylor@acmepartsv1.example",
            "555-0199",
            "V1 account manager",
            true));
        var contactResponse = await _supplyarrClient.SendAsync(contactRequest);
        contactResponse.EnsureSuccessStatusCode();

        var approveRequest = Authorized(HttpMethod.Patch, $"/api/v1/suppliers/{supplier.SupplierId}/approval-status", token);
        approveRequest.Content = JsonContent.Create(new UpdateSupplierApprovalStatusRequest("approved"));
        var approveResponse = await _supplyarrClient.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;
        Assert.Equal("approved", approved.ApprovalStatus);
        Assert.Single(approved.Contacts);
    }

    [Fact]
    public async Task Supplier_directory_v1_metadata_and_contacts_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/v1/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "midwest-supplier-v1",
            null,
            null,
            "Midwest Supplier V1",
            "Midwest Supplier V1 LLC",
            null,
            string.Empty,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createContactRequest = Authorized(HttpMethod.Post, $"/api/v1/suppliers/{supplier.SupplierId}/contacts", token);
        createContactRequest.Content = JsonContent.Create(new CreateSupplierContactRequest(
            "Morgan Contact",
            "morgan@midwestsupplier.example",
            "555-0101",
            "Coordinator",
            true));
        var createContactResponse = await _supplyarrClient.SendAsync(createContactRequest);
        createContactResponse.EnsureSuccessStatusCode();

        var metadataResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/suppliers/metadata", token));
        metadataResponse.EnsureSuccessStatusCode();
        var metadata = (await metadataResponse.Content.ReadFromJsonAsync<SupplierDirectoryMetadataResponse>())!;
        Assert.Contains(metadata.UnitKindOptions, x => x.Value == "sub_unit" && x.Label == "Supplier sub-unit");

        var listSuppliersResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/suppliers", token));
        listSuppliersResponse.EnsureSuccessStatusCode();
        var suppliers = (await listSuppliersResponse.Content.ReadFromJsonAsync<List<SupplierResponse>>())!;
        Assert.Contains(suppliers, x => x.SupplierId == supplier.SupplierId);

        var getSupplierResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/suppliers/{supplier.SupplierId}", token));
        getSupplierResponse.EnsureSuccessStatusCode();
        var loadedSupplier = (await getSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;
        Assert.Contains(loadedSupplier.Contacts, x => x.ContactName == "Morgan Contact");
    }

    [Fact]
    public async Task Supplier_directory_v1_rejects_legacy_location_unit_kind()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var request = Authorized(HttpMethod.Post, "/api/v1/suppliers", token);
        request.Content = JsonContent.Create(new CreateSupplierRequest(
            "legacy-location-kind-v1",
            null,
            "location",
            "Legacy Location Kind",
            "Legacy Location Kind LLC",
            null,
            string.Empty,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("Supplier unit kind must be identity or sub_unit.", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Supplier_create_denied_without_manage_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_clerk");
        var request = Authorized(HttpMethod.Post, "/api/suppliers", token);
        request.Content = JsonContent.Create(new CreateSupplierRequest(
            "denied-supplier",
            null,
            null,
            "Denied Supplier",
            string.Empty,
            null,
            string.Empty,
            null,
            null,
            null,
            null,
            null,
            null,
            null));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Part_catalog_crud_with_supplier_link_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "parts-supplier",
            null,
            null,
            "Parts Supplier Inc.",
            string.Empty,
            null,
            string.Empty,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

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

        var linkRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/supplier-links", token);
        linkRequest.Content = JsonContent.Create(new CreatePartSupplierLinkRequest(
            null,
            supplier.SupplierId,
            "V-FLT-001",
            true));
        var linkResponse = await _supplyarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();
        var link = (await linkResponse.Content.ReadFromJsonAsync<PartSupplierLinkResponse>())!;
        Assert.Equal("V-FLT-001", link.SupplierPartNumber);

        var listPartsRequest = Authorized(HttpMethod.Get, "/api/parts", token);
        var listPartsResponse = await _supplyarrClient.SendAsync(listPartsRequest);
        listPartsResponse.EnsureSuccessStatusCode();
        var parts = (await listPartsResponse.Content.ReadFromJsonAsync<List<PartResponse>>())!;
        Assert.Contains(parts, x => x.PartId == part.PartId);

        var getPartRequest = Authorized(HttpMethod.Get, $"/api/parts/{part.PartId}", token);
        var getPartResponse = await _supplyarrClient.SendAsync(getPartRequest);
        getPartResponse.EnsureSuccessStatusCode();
        var loaded = (await getPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;
        Assert.Single(loaded.SupplierLinks);
        Assert.Equal(supplier.SupplierKey, loaded.SupplierLinks[0].SupplierKey);
        Assert.True(loaded.SupplierLinks[0].IsPreferred);
    }

    [Fact]
    public async Task Part_catalog_parts_can_store_operational_sources_without_supplier_links()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "legacy-clamp-001",
            null,
            "Hydraulic Hose Clamp",
            "Legacy clamp carried without a purchasing record",
            "hardware",
            "each",
            string.Empty,
            string.Empty,
            true,
            false));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        Assert.True(part.IsTrackable);
        Assert.False(part.IsStocked);
        Assert.Empty(part.SupplierLinks);
        Assert.Empty(part.Sources);

        var sourceRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/sources", token);
        sourceRequest.Content = JsonContent.Create(new CreatePartSourceRequest(
            "salvage",
            "Retired Truck 104",
            "Recovered during decommissioning."));
        var sourceResponse = await _supplyarrClient.SendAsync(sourceRequest);
        sourceResponse.EnsureSuccessStatusCode();

        var getPartRequest = Authorized(HttpMethod.Get, $"/api/parts/{part.PartId}", token);
        var getPartResponse = await _supplyarrClient.SendAsync(getPartRequest);
        getPartResponse.EnsureSuccessStatusCode();
        var loaded = (await getPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        Assert.Single(loaded.Sources);
        Assert.Equal("salvage", loaded.Sources[0].SourceType);
        Assert.Equal("Retired Truck 104", loaded.Sources[0].Label);
        Assert.Empty(loaded.SupplierLinks);
    }

    [Fact]
    public async Task Part_catalog_crud_with_supplier_link_v1_alias_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/v1/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "parts-supplier-v1",
            null,
            null,
            "Parts Supplier V1 Inc.",
            string.Empty,
            null,
            string.Empty,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

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

        var linkRequest = Authorized(HttpMethod.Post, $"/api/v1/parts/{part.PartId}/supplier-links", token);
        linkRequest.Content = JsonContent.Create(new CreatePartSupplierLinkRequest(
            null,
            supplier.SupplierId,
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
        Assert.Single(loaded.SupplierLinks);
    }

    [Fact]
    public async Task Part_catalog_v1_items_alias_crud_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createCatalogRequest = Authorized(HttpMethod.Post, "/api/v1/catalogs", token);
        createCatalogRequest.Content = JsonContent.Create(new CreatePartCatalogRequest(
            "items-catalog-v1",
            "Items Catalog V1",
            "Catalog for items alias"));
        var createCatalogResponse = await _supplyarrClient.SendAsync(createCatalogRequest);
        createCatalogResponse.EnsureSuccessStatusCode();
        var catalog = (await createCatalogResponse.Content.ReadFromJsonAsync<PartCatalogResponse>())!;

        var createItemRequest = Authorized(HttpMethod.Post, "/api/v1/items", token);
        createItemRequest.Content = JsonContent.Create(new CreatePartRequest(
            "item-v1-001",
            catalog.CatalogId,
            "Item Alias Part",
            "Created through items alias",
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createItemResponse = await _supplyarrClient.SendAsync(createItemRequest);
        createItemResponse.EnsureSuccessStatusCode();
        var item = (await createItemResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var listItemsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/items", token));
        listItemsResponse.EnsureSuccessStatusCode();
        var items = (await listItemsResponse.Content.ReadFromJsonAsync<List<PartResponse>>())!;
        Assert.Contains(items, x => x.PartId == item.PartId);

        var getItemResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/items/{item.PartId}", token));
        getItemResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task V1_supplier_documents_alias_register_and_list_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "docs-supplier-v1",
            null,
            null,
            "Docs Supplier V1",
            string.Empty,
            string.Empty,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var registerRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/suppliers/{supplier.SupplierId}/compliance-documents",
            token);
        registerRequest.Content = JsonContent.Create(new SupplierComplianceDocumentRegistrationRequest(
            "DOC-V1-1",
            "w9",
            "W9",
            null,
            null,
            "w9.pdf",
            "application/pdf",
            256,
            string.Empty));
        var registerResponse = await _supplyarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var doc = (await registerResponse.Content.ReadFromJsonAsync<SupplierComplianceDocumentResponse>())!;

        var listResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/suppliers/{supplier.SupplierId}/compliance-documents", token));
        listResponse.EnsureSuccessStatusCode();
        var docs = (await listResponse.Content.ReadFromJsonAsync<List<SupplierComplianceDocumentResponse>>())!;
        Assert.Contains(docs, x => x.DocumentId == doc.DocumentId);
    }

    [Fact]
    public async Task V1_inventory_locations_receipts_and_reports_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/v1/inventory-locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "alias-main",
            "Alias Main Location",
            "warehouse",
            "100 Main St",
            _staffarrSiteOrgUnitId));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();

        var listLocationsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/inventory-locations", token));
        listLocationsResponse.EnsureSuccessStatusCode();

        var receiptsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/receipts", token));
        receiptsResponse.EnsureSuccessStatusCode();

        var reportsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports", token));
        reportsResponse.EnsureSuccessStatusCode();
        using var reportsDocument = JsonDocument.Parse(await reportsResponse.Content.ReadAsStringAsync());
        var groups = reportsDocument.RootElement
            .GetProperty("reports")
            .EnumerateArray()
            .Select(report => report.GetProperty("key").GetString())
            .OfType<string>()
            .ToList();
        Assert.Contains("suppliers", groups);
        Assert.Contains("purchasing", groups);
    }

    [Fact]
    public async Task V1_item_categories_manufacturers_and_supplier_items_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "vim-supplier-v1",
            null,
            null,
            "Supplier Items Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "vim-part-v1",
            null,
            "Supplier Item Part",
            "Part for supplier-items alias",
            "filters",
            "each",
            "Fleet OEM",
            "FLEET-001"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createSupplierItemRequest = Authorized(HttpMethod.Post, "/api/v1/supplier-items", token);
        createSupplierItemRequest.Content = JsonContent.Create(new CreateSupplierItemRequest(
            part.PartId,
            supplier.SupplierId,
            "VIM-001",
            true));
        var createSupplierItemResponse = await _supplyarrClient.SendAsync(createSupplierItemRequest);
        createSupplierItemResponse.EnsureSuccessStatusCode();

        var categoriesResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/item-categories", token));
        categoriesResponse.EnsureSuccessStatusCode();
        var categories = (await categoriesResponse.Content.ReadFromJsonAsync<List<ItemCategorySummaryResponse>>())!;
        Assert.Contains(categories, x => x.CategoryKey == "filters");

        var manufacturersResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/manufacturers", token));
        manufacturersResponse.EnsureSuccessStatusCode();
        var manufacturers = (await manufacturersResponse.Content.ReadFromJsonAsync<List<ManufacturerSummaryResponse>>())!;
        Assert.Contains(manufacturers, x => x.ManufacturerName == "Fleet OEM");

        var supplierItemsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/supplier-items?supplierId={supplier.SupplierId}", token));
        supplierItemsResponse.EnsureSuccessStatusCode();
        var supplierItems = (await supplierItemsResponse.Content.ReadFromJsonAsync<List<SupplierItemResponse>>())!;
        Assert.Contains(supplierItems, x => x.PartId == part.PartId && x.SupplierId == supplier.SupplierId);
    }

    [Fact]
    public async Task V1_supplier_quote_crud_and_submit_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "quote-supplier-v1",
            null,
            null,
            "Quote Supplier V1",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "quote-part-v1",
            null,
            "Quote Part",
            "Part for quote alias",
            "general",
            "each",
            "Quote Maker",
            "QM-100"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createRfqRequest = Authorized(HttpMethod.Post, "/api/v1/rfqs", token);
        createRfqRequest.Content = JsonContent.Create(new CreateRfqRequest(
            "rfq-qalias-v1",
            "RFQ Quote Workflow",
            "Supplier quote flow",
            [new CreateRfqLineRequest(part.PartId, 5, "line")]));
        var createRfqResponse = await _supplyarrClient.SendAsync(createRfqRequest);
        createRfqResponse.EnsureSuccessStatusCode();
        var rfq = (await createRfqResponse.Content.ReadFromJsonAsync<RfqResponse>())!;

        var submitRfqResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/rfqs/{rfq.RfqId}/submit", token));
        submitRfqResponse.EnsureSuccessStatusCode();

        var inviteRequest = Authorized(HttpMethod.Post, $"/api/v1/rfqs/{rfq.RfqId}/invite-suppliers", token);
        inviteRequest.Content = JsonContent.Create(new InviteRfqSuppliersRequest([supplier.SupplierId]));
        var inviteResponse = await _supplyarrClient.SendAsync(inviteRequest);
        inviteResponse.EnsureSuccessStatusCode();

        var createQuoteRequest = Authorized(HttpMethod.Post, $"/api/v1/rfqs/{rfq.RfqId}/quotes", token);
        createQuoteRequest.Content = JsonContent.Create(new CreateSupplierQuoteRequest(
            supplier.SupplierId,
            "Q-ALIAS-001",
            "USD",
            "Alias quote"));
        var createQuoteResponse = await _supplyarrClient.SendAsync(createQuoteRequest);
        createQuoteResponse.EnsureSuccessStatusCode();
        var quote = (await createQuoteResponse.Content.ReadFromJsonAsync<SupplierQuoteResponse>())!;

        var upsertLineRequest = Authorized(HttpMethod.Put, $"/api/v1/rfqs/{rfq.RfqId}/quotes/{quote.SupplierQuoteId}/lines", token);
        upsertLineRequest.Content = JsonContent.Create(new UpsertSupplierQuoteLineRequest(
            rfq.Lines[0].LineId,
            10m,
            5m,
            3,
            "quoted"));
        var upsertLineResponse = await _supplyarrClient.SendAsync(upsertLineRequest);
        upsertLineResponse.EnsureSuccessStatusCode();

        var submitQuoteResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/rfqs/{rfq.RfqId}/quotes/{quote.SupplierQuoteId}/submit", token));
        submitQuoteResponse.EnsureSuccessStatusCode();

        var listQuotesResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/rfqs/{rfq.RfqId}", token));
        listQuotesResponse.EnsureSuccessStatusCode();
        var updatedRfq = (await listQuotesResponse.Content.ReadFromJsonAsync<RfqResponse>())!;
        Assert.Contains(updatedRfq.Quotes, x => x.SupplierQuoteId == quote.SupplierQuoteId);
    }

    [Fact]
    public async Task V1_bootstrap_approvals_stock_transactions_and_cycle_counts_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var bootstrapResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/bootstrap", token));
        bootstrapResponse.EnsureSuccessStatusCode();
        var bootstrap = (await bootstrapResponse.Content.ReadFromJsonAsync<SupplyArrSessionBootstrapResponse>())!;
        Assert.Contains("supplyarr", bootstrap.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", bootstrap.LaunchableProductKeys);

        var approvalsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/approvals", token));
        approvalsResponse.EnsureSuccessStatusCode();
        var approvals = (await approvalsResponse.Content.ReadFromJsonAsync<List<ApprovalQueueItemResponse>>())!;
        Assert.NotNull(approvals);

        var stockTransactionsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/stock-transactions?limit=5", token));
        stockTransactionsResponse.EnsureSuccessStatusCode();
        var stockTransactions = (await stockTransactionsResponse.Content.ReadFromJsonAsync<List<StockTransactionItemResponse>>())!;
        Assert.NotNull(stockTransactions);

        var cycleCountsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/cycle-counts", token));
        cycleCountsResponse.EnsureSuccessStatusCode();
        var cycleCounts = (await cycleCountsResponse.Content.ReadFromJsonAsync<List<CycleCountItemResponse>>())!;
        Assert.NotNull(cycleCounts);

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "workflow-stock-part",
            null,
            "Workflow Stock Part",
            "stock transaction alias part",
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/v1/inventory-locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "workflow-stock-wh",
            "Workflow Stock Warehouse",
            "warehouse",
            "100 Alias Way",
            _staffarrSiteOrgUnitId));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        Assert.Equal(HttpStatusCode.Created, createLocationResponse.StatusCode);
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(HttpMethod.Post, $"/api/v1/inventory-locations/{location.LocationId}/bins", token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("workflow-stock-bin", "Workflow Stock Bin"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        Assert.Equal(HttpStatusCode.Created, createBinResponse.StatusCode);
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var createTransactionRequest = Authorized(HttpMethod.Post, "/api/v1/stock-transactions", token);
        createTransactionRequest.Content = JsonContent.Create(new CreateStockTransactionRequest(
            part.PartId,
            bin.BinId,
            5m,
            "in"));
        var createTransactionResponse = await _supplyarrClient.SendAsync(createTransactionRequest);
        Assert.Equal(HttpStatusCode.Created, createTransactionResponse.StatusCode);
        Assert.StartsWith("/api/v1/inventory/stock", createTransactionResponse.Headers.Location?.ToString());
        var stock = (await createTransactionResponse.Content.ReadFromJsonAsync<PartStockLevelResponse>())!;
        Assert.Equal(5m, stock.QuantityOnHand);

        var createCycleCountRequest = Authorized(HttpMethod.Post, "/api/v1/cycle-counts", token);
        createCycleCountRequest.Content = JsonContent.Create(new CreateCycleCountRequest(
            part.PartId,
            bin.BinId,
            3m));
        var createCycleCountResponse = await _supplyarrClient.SendAsync(createCycleCountRequest);
        Assert.Equal(HttpStatusCode.Created, createCycleCountResponse.StatusCode);
        Assert.Equal($"/api/v1/cycle-counts/{stock.StockLevelId}", createCycleCountResponse.Headers.Location?.ToString());
        var cycleCount = (await createCycleCountResponse.Content.ReadFromJsonAsync<CycleCountItemResponse>())!;
        Assert.Equal(3m, cycleCount.QuantityOnHand);

        var filteredCycleCountsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/cycle-counts?partId={part.PartId}", token));
        filteredCycleCountsResponse.EnsureSuccessStatusCode();
        var filteredCycleCounts = (await filteredCycleCountsResponse.Content.ReadFromJsonAsync<List<CycleCountItemResponse>>())!;
        Assert.Contains(filteredCycleCounts, x => x.StockLevelId == stock.StockLevelId && x.QuantityOnHand == 3m);

        var updatedStockTransactionsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/stock-transactions?limit=10", token));
        updatedStockTransactionsResponse.EnsureSuccessStatusCode();
        var updatedStockTransactions = (await updatedStockTransactionsResponse.Content.ReadFromJsonAsync<List<StockTransactionItemResponse>>())!;
        Assert.Contains(updatedStockTransactions, x => x.TargetId == stock.StockLevelId.ToString());
    }

    [Fact]
    public async Task V1_substitutions_documents_contracts_imports_exports_and_admin_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "cov-supplier-v1",
            null,
            null,
            "Coverage Supplier V1",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "cov-part-v1",
            null,
            "Coverage Part",
            "part for substitutions",
            "general",
            "each",
            "CoverageCo",
            "COV-1"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var aliasRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/manufacturer-aliases", token);
        aliasRequest.Content = JsonContent.Create(new CreatePartManufacturerAliasRequest(
            "cov-alias-1",
            "CoverageCo",
            "COV-1A"));
        var aliasResponse = await _supplyarrClient.SendAsync(aliasRequest);
        aliasResponse.EnsureSuccessStatusCode();

        var substitutionsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/substitutions?partId={part.PartId}", token));
        substitutionsResponse.EnsureSuccessStatusCode();
        var substitutions = (await substitutionsResponse.Content.ReadFromJsonAsync<List<SubstitutionItemResponse>>())!;
        Assert.Contains(substitutions, x => x.PartId == part.PartId);

        var createDocumentRequest = Authorized(HttpMethod.Post, "/api/v1/documents", token);
        createDocumentRequest.Content = JsonContent.Create(new CreateSupplyDocumentRequest(
            supplier.SupplierId,
            "COV-DOC-1",
            "w9",
            "Coverage Doc",
            null,
            null,
            "cov.pdf",
            "application/pdf",
            512,
            string.Empty));
        var createDocumentResponse = await _supplyarrClient.SendAsync(createDocumentRequest);
        createDocumentResponse.EnsureSuccessStatusCode();

        var listDocumentsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/documents?supplierId={supplier.SupplierId}", token));
        listDocumentsResponse.EnsureSuccessStatusCode();
        var documents = (await listDocumentsResponse.Content.ReadFromJsonAsync<List<SupplyDocumentItemResponse>>())!;
        Assert.Contains(documents, x => x.SupplierId == supplier.SupplierId);

        var contractsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/contracts", token));
        contractsResponse.EnsureSuccessStatusCode();
        var contracts = (await contractsResponse.Content.ReadFromJsonAsync<List<ContractSnapshotItemResponse>>())!;
        Assert.NotNull(contracts);

        var importsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/imports", token));
        importsResponse.EnsureSuccessStatusCode();
        var imports = (await importsResponse.Content.ReadFromJsonAsync<List<ImportOptionResponse>>())!;
        Assert.Contains(imports, x => x.ImportType == "part_catalog_csv");
        Assert.Contains(imports, x => x.ImportType == "supplier_catalog_csv");
        Assert.Contains(imports, x => x.ImportType == "inventory_counts_csv");
        Assert.Contains(imports, x => x.ImportType == "price_list_csv");
        Assert.Contains(imports, x => x.ImportType == "lead_time_list_csv");
        Assert.Contains(imports, x => x.ImportType == "availability_list_csv");
        Assert.Contains(imports, x => x.ImportType == "contracts_csv");
        Assert.Contains(imports, x => x.ImportType == "suppliers_csv");
        Assert.Contains(imports, x => x.ImportType == "contacts_csv");
        Assert.Contains(imports, x => x.ImportType == "open_purchase_orders_csv");
        Assert.Contains(imports, x => x.ImportType == "purchase_history_csv");

        var exportsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports", token));
        exportsResponse.EnsureSuccessStatusCode();
        var exports = (await exportsResponse.Content.ReadFromJsonAsync<List<ExportOptionResponse>>())!;
        Assert.Contains(exports, x => x.ExportType == "supplier_summary_csv");
        Assert.Contains(exports, x => x.ExportType == "supplier_list_csv");
        Assert.Contains(exports, x => x.ExportType == "approved_supplier_list_csv");
        Assert.Contains(exports, x => x.ExportType == "parts_catalog_csv");
        Assert.Contains(exports, x => x.ExportType == "inventory_valuation_csv");
        Assert.Contains(exports, x => x.ExportType == "purchase_orders_csv");
        Assert.Contains(exports, x => x.ExportType == "receipts_csv");
        Assert.Contains(exports, x => x.ExportType == "invoice_support_csv");
        Assert.Contains(exports, x => x.ExportType == "supplier_document_report_csv");
        Assert.Contains(exports, x => x.ExportType == "compliance_evidence_packet_csv");
        Assert.Contains(exports, x => x.ExportType == "spend_report_csv");

        var adminResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/admin", token));
        adminResponse.EnsureSuccessStatusCode();
        var admin = (await adminResponse.Content.ReadFromJsonAsync<AdminOverviewResponse>())!;
        Assert.Equal("supplyarr", admin.ProductKey);
        Assert.Contains(admin.LaunchableProductKeys, value => string.Equals(value, "nexarr", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task V1_part_catalog_csv_import_preview_and_commit_create_catalogs_and_parts()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var csv = string.Join('\n', new[]
        {
            "catalog_key,catalog_name,catalog_description,part_key,part_name,part_description,category_key,unit_of_measure,manufacturer_name,manufacturer_part_number",
            "csv-catalog-v1,CSV Import Catalog,Imported from CSV,csv-part-v1,CSV Import Part,Imported part,filters,each,CSVCo,CSV-1",
            "csv-catalog-v1,CSV Import Catalog,Imported from CSV,csv-part-two-v1,CSV Import Part Two,Second imported part,filters,box,CSVCo,CSV-2"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/part-catalog-csv", token);
        previewRequest.Content = JsonContent.Create(new PartCatalogCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<PartCatalogCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(2, preview.RowsRead);
        Assert.Equal(1, preview.CatalogsAccepted);
        Assert.Equal(2, preview.PartsAccepted);
        Assert.Equal(0, preview.CatalogsCreated);
        Assert.Equal(0, preview.PartsCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/part-catalog-csv", token);
        commitRequest.Content = JsonContent.Create(new PartCatalogCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<PartCatalogCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.CatalogsCreated);
        Assert.Equal(2, commit.PartsCreated);

        var catalogsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/catalogs", token));
        catalogsResponse.EnsureSuccessStatusCode();
        var catalogs = (await catalogsResponse.Content.ReadFromJsonAsync<List<PartCatalogResponse>>())!;
        var catalog = Assert.Single(catalogs, x => x.CatalogKey == "csv-catalog-v1");

        var partsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/parts?catalogId={catalog.CatalogId}", token));
        partsResponse.EnsureSuccessStatusCode();
        var parts = (await partsResponse.Content.ReadFromJsonAsync<List<PartResponse>>())!;
        Assert.Contains(parts, x => x.PartKey == "csv-part-v1" && x.CatalogId == catalog.CatalogId);
        Assert.Contains(parts, x => x.PartKey == "csv-part-two-v1" && x.UnitOfMeasure == "box");

        var historyResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/imports/history?importType=part_catalog_csv&limit=5", token));
        historyResponse.EnsureSuccessStatusCode();
        var history = (await historyResponse.Content.ReadFromJsonAsync<ImportHistoryListResponse>())!;
        Assert.Contains(history.Items, x => x.ImportType == "part_catalog_csv" && x.DryRun && x.Succeeded && x.RowsRead == 2);
        Assert.Contains(history.Items, x => x.ImportType == "part_catalog_csv" && !x.DryRun && x.Succeeded && x.RowsRead == 2);
    }

    [Fact]
    public async Task V1_import_error_export_returns_csv_from_preview_issues()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var csv = string.Join('\n', new[]
        {
            "catalog_key,catalog_name,catalog_description,part_key,part_name,part_description,category_key,unit_of_measure,manufacturer_name,manufacturer_part_number",
            "x,,description,,,"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/part-catalog-csv", token);
        previewRequest.Content = JsonContent.Create(new PartCatalogCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        Assert.Equal(HttpStatusCode.BadRequest, previewResponse.StatusCode);
        var preview = (await previewResponse.Content.ReadFromJsonAsync<PartCatalogCsvImportResponse>())!;
        Assert.False(preview.Succeeded);
        Assert.NotEmpty(preview.Issues);

        var exportRequest = Authorized(HttpMethod.Post, "/api/v1/imports/errors/export", token);
        exportRequest.Content = JsonContent.Create(new ImportErrorExportRequest(
            preview.ImportType,
            preview.Issues
                .Select(x => new ImportErrorExportIssueRequest(x.LineNumber, x.Code, x.Message))
                .ToList()));
        var exportResponse = await _supplyarrClient.SendAsync(exportRequest);
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var content = await exportResponse.Content.ReadAsStringAsync();
        Assert.StartsWith("importType,lineNumber,code,message", content, StringComparison.Ordinal);
        Assert.Contains("part_catalog_csv", content, StringComparison.Ordinal);
        Assert.Contains("csv.columns", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task V1_import_field_mapping_rewrites_source_columns_to_canonical_csv()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var sourceCsv = string.Join('\n', new[]
        {
            "Catalog Code,Catalog Name,Catalog Notes,Part Number,Part Label,Part Notes,Category,UOM,Mfg,Mfg Part",
            "mapped-catalog-v1,Mapped Catalog,Imported with field mapping,mapped-part-v1,Mapped Part,Part from mapped CSV,filters,each,MapCo,MAP-1"
        });

        var mapRequest = Authorized(HttpMethod.Post, "/api/v1/imports/map-fields", token);
        mapRequest.Content = JsonContent.Create(new ImportFieldMappingRequest(
            "part_catalog_csv",
            sourceCsv,
            new Dictionary<string, string>
            {
                ["Catalog Code"] = "catalog_key",
                ["Catalog Name"] = "catalog_name",
                ["Catalog Notes"] = "catalog_description",
                ["Part Number"] = "part_key",
                ["Part Label"] = "part_name",
                ["Part Notes"] = "part_description",
                ["Category"] = "category_key",
                ["UOM"] = "unit_of_measure",
                ["Mfg"] = "manufacturer_name",
                ["Mfg Part"] = "manufacturer_part_number"
            }));
        var mapResponse = await _supplyarrClient.SendAsync(mapRequest);
        mapResponse.EnsureSuccessStatusCode();
        var mapped = (await mapResponse.Content.ReadFromJsonAsync<ImportFieldMappingResponse>())!;
        Assert.True(mapped.Succeeded);
        Assert.Empty(mapped.MissingRequiredHeaders);
        Assert.Contains("catalog_key,catalog_name,catalog_description,part_key", mapped.Csv, StringComparison.Ordinal);

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/part-catalog-csv", token);
        previewRequest.Content = JsonContent.Create(new PartCatalogCsvImportRequest(mapped.Csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<PartCatalogCsvImportResponse>())!;
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.CatalogsAccepted);
        Assert.Equal(1, preview.PartsAccepted);
    }

    [Fact]
    public async Task V1_entity_exports_return_supplier_and_parts_catalog_csv()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "export-supplier-v1",
            null,
            null,
            "Export Supplier",
            "Export Supplier LLC",
            null,
            "supplier export",
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var approveSupplierRequest = Authorized(
            HttpMethod.Patch,
            $"/api/v1/suppliers/{supplier.SupplierId}/approval-status",
            token);
        approveSupplierRequest.Content = JsonContent.Create(new UpdateSupplierApprovalStatusRequest("approved"));
        var approveSupplierResponse = await _supplyarrClient.SendAsync(approveSupplierRequest);
        approveSupplierResponse.EnsureSuccessStatusCode();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "export-part-v1",
            null,
            "Export Part",
            "Exported part catalog row",
            "filters",
            "each",
            "ExportCo",
            "EXP-1"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();

        var supplierExportResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/suppliers.csv", token));
        supplierExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", supplierExportResponse.Content.Headers.ContentType?.MediaType);
        var supplierCsv = await supplierExportResponse.Content.ReadAsStringAsync();
        Assert.Contains("supplierKey,parentSupplierKey,displayName", supplierCsv, StringComparison.Ordinal);
        Assert.Contains("export-supplier-v1,,Export Supplier", supplierCsv, StringComparison.Ordinal);

        var approvedSupplierExportResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/approved-suppliers.csv", token));
        approvedSupplierExportResponse.EnsureSuccessStatusCode();
        var approvedSupplierCsv = await approvedSupplierExportResponse.Content.ReadAsStringAsync();
        Assert.Contains("export-supplier-v1,,Export Supplier", approvedSupplierCsv, StringComparison.Ordinal);

        var partsExportResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/parts-catalog.csv", token));
        partsExportResponse.EnsureSuccessStatusCode();
        var partsCsv = await partsExportResponse.Content.ReadAsStringAsync();
        Assert.Contains("partKey,catalogKey,displayName", partsCsv, StringComparison.Ordinal);
        Assert.Contains("export-part-v1,,Export Part", partsCsv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task V1_valuation_supplier_document_and_spend_exports_return_csv()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "export-valued-supplier-v1",
            null,
            null,
            "Export Valued Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var documentRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/suppliers/{supplier.SupplierId}/compliance-documents",
            token);
        documentRequest.Content = JsonContent.Create(new SupplierComplianceDocumentRegistrationRequest(
            "EXPORT-DOC-1",
            "insurance",
            "Export Insurance",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "insurance.pdf",
            "application/pdf",
            4096,
            "s3://supplyarr/export/insurance.pdf"));
        var documentResponse = await _supplyarrClient.SendAsync(documentRequest);
        documentResponse.EnsureSuccessStatusCode();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "export-valued-part-v1",
            null,
            "Export Valued Part",
            "Valuation export part",
            "filters",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var supplierLinkRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/supplier-links", token);
        supplierLinkRequest.Content = JsonContent.Create(new CreatePartSupplierLinkRequest(null, supplier.SupplierId, "VAL-1", true));
        var supplierLinkResponse = await _supplyarrClient.SendAsync(supplierLinkRequest);
        supplierLinkResponse.EnsureSuccessStatusCode();
        var link = (await supplierLinkResponse.Content.ReadFromJsonAsync<PartSupplierLinkResponse>())!;

        var priceRequest = Authorized(HttpMethod.Put, $"/api/parts/{part.PartId}/supplier-links/{link.LinkId}/catalog-price", token);
        priceRequest.Content = JsonContent.Create(new UpsertPartSupplierLinkCatalogPriceRequest(15.25m, "USD", 1m));
        var priceResponse = await _supplyarrClient.SendAsync(priceRequest);
        priceResponse.EnsureSuccessStatusCode();

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "export-value-wh-v1",
            "Export Value Warehouse",
            "warehouse",
            string.Empty,
            _staffarrSiteOrgUnitId));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(HttpMethod.Post, $"/api/inventory/locations/{location.LocationId}/bins", token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("export-value-bin-v1", "Value A1"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var stockRequest = Authorized(HttpMethod.Post, "/api/inventory/stock", token);
        stockRequest.Content = JsonContent.Create(new UpsertPartStockLevelRequest(part.PartId, bin.BinId, 6m));
        var stockResponse = await _supplyarrClient.SendAsync(stockRequest);
        stockResponse.EnsureSuccessStatusCode();

        var valuationResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/inventory-valuation.csv", token));
        valuationResponse.EnsureSuccessStatusCode();
        var valuationCsv = await valuationResponse.Content.ReadAsStringAsync();
        Assert.Contains("partKey,partDisplayName,locationKey,binKey", valuationCsv, StringComparison.Ordinal);
        Assert.Contains("export-valued-part-v1,Export Valued Part,export-value-wh-v1,export-value-bin-v1,6", valuationCsv, StringComparison.Ordinal);
        Assert.Contains(",15.25,91.50,", valuationCsv, StringComparison.Ordinal);

        var documentsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/supplier-documents.csv", token));
        documentsResponse.EnsureSuccessStatusCode();
        var documentsCsv = await documentsResponse.Content.ReadAsStringAsync();
        Assert.Contains("supplierKey,parentSupplierKey,supplierDisplayName,supplierUnitKind,documentKey", documentsCsv, StringComparison.Ordinal);
        Assert.Contains("export-valued-supplier-v1,,Export Valued Supplier,identity,EXPORT-DOC-1", documentsCsv, StringComparison.Ordinal);

        var spendResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/spend.csv", token));
        spendResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", spendResponse.Content.Headers.ContentType?.MediaType);
        var spendCsv = await spendResponse.Content.ReadAsStringAsync();
        Assert.Contains("orderKey,status,requestKey,supplierKey", spendCsv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task V1_supplier_documents_csv_import_preview_and_commit_register_documents()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "csv-doc-supplier-v1",
            null,
            null,
            "CSV Document Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var csv = string.Join('\n', new[]
        {
            "supplier_key,document_key,document_type_key,title,effective_at,expires_at,file_name,content_type,size_bytes,storage_uri",
            "csv-doc-supplier-v1,DOC-CSV-1,w9,Imported W9,2026-01-01,2027-01-01,w9.pdf,application/pdf,2048,s3://supplyarr/documents/w9.pdf"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/supplier-documents-csv", token);
        previewRequest.Content = JsonContent.Create(new SupplierDocumentsCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<SupplierDocumentsCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.DocumentsAccepted);
        Assert.Equal(0, preview.DocumentsCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/supplier-documents-csv", token);
        commitRequest.Content = JsonContent.Create(new SupplierDocumentsCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<SupplierDocumentsCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.DocumentsCreated);

        var documentsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/documents?supplierId={supplier.SupplierId}", token));
        documentsResponse.EnsureSuccessStatusCode();
        var documents = (await documentsResponse.Content.ReadFromJsonAsync<List<SupplyDocumentItemResponse>>())!;
        Assert.Contains(documents, x => x.DocumentKey == "DOC-CSV-1" && x.DocumentTypeKey == "w9");
    }

    [Fact]
    public async Task V1_supplier_catalog_csv_import_preview_and_commit_creates_supplier_link_with_catalog_facts()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "csv-supplier-catalog-supplier-v1",
            null,
            null,
            "CSV Supplier Catalog Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "csv-supplier-catalog-part-v1",
            null,
            "CSV Supplier Catalog Part",
            "Imported supplier catalog part",
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var csv = string.Join('\n', new[]
        {
            "supplier_key,part_key,supplier_part_number,is_preferred,catalog_unit_price,catalog_currency_code,catalog_minimum_order_quantity,catalog_lead_time_days,catalog_quantity_available,catalog_availability_status",
            "csv-supplier-catalog-supplier-v1,csv-supplier-catalog-part-v1,CSV-SUPPLIER-PART-1,true,12.50,USD,2,5,18,in_stock"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/supplier-catalog-csv", token);
        previewRequest.Content = JsonContent.Create(new SupplierCatalogCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<SupplierCatalogCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.LinksAccepted);
        Assert.Equal(0, preview.LinksCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/supplier-catalog-csv", token);
        commitRequest.Content = JsonContent.Create(new SupplierCatalogCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<SupplierCatalogCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.LinksCreated);

        var partResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/parts/{part.PartId}", token));
        partResponse.EnsureSuccessStatusCode();
        var importedPart = (await partResponse.Content.ReadFromJsonAsync<PartResponse>())!;
        Assert.Contains(
            importedPart.SupplierLinks,
            x => x.SupplierId == supplier.SupplierId
                && x.SupplierPartNumber == "CSV-SUPPLIER-PART-1"
                && x.IsPreferred
                && x.CatalogUnitPrice == 12.50m
                && x.CatalogMinimumOrderQuantity == 2m
                && x.CatalogLeadTimeDays == 5
                && x.CatalogQuantityAvailable == 18m
                && x.CatalogAvailabilityStatus == "in_stock");
    }

    [Fact]
    public async Task V1_inventory_counts_csv_import_preview_and_commit_updates_stock()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "csv-count-part-v1",
            null,
            "CSV Count Part",
            "Imported inventory count part",
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/v1/inventory-locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "csv-count-wh-v1",
            "CSV Count Warehouse",
            "warehouse",
            "100 Count Way",
            _staffarrSiteOrgUnitId));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(HttpMethod.Post, $"/api/v1/inventory-locations/{location.LocationId}/bins", token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("count-a1", "Count A1"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var csv = string.Join('\n', new[]
        {
            "location_key,bin_key,part_key,quantity_on_hand",
            "csv-count-wh-v1,count-a1,csv-count-part-v1,12.5"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/inventory-counts-csv", token);
        previewRequest.Content = JsonContent.Create(new InventoryCountsCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<InventoryCountsCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.CountsAccepted);
        Assert.Equal(0, preview.CountsApplied);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/inventory-counts-csv", token);
        commitRequest.Content = JsonContent.Create(new InventoryCountsCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<InventoryCountsCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.CountsApplied);

        var stockResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/inventory/stock?partId={part.PartId}&binId={bin.BinId}", token));
        stockResponse.EnsureSuccessStatusCode();
        var stock = (await stockResponse.Content.ReadFromJsonAsync<List<PartStockLevelResponse>>())!;
        var level = Assert.Single(stock);
        Assert.Equal(12.5m, level.QuantityOnHand);
        Assert.Equal(bin.BinId, level.BinId);
    }

    [Fact]
    public async Task V1_price_list_csv_import_preview_and_commit_creates_pricing_snapshot()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "csv-price-supplier-v1",
            null,
            null,
            "CSV Price Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "csv-price-part-v1",
            null,
            "CSV Price Part",
            "Imported price list part",
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var linkRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/supplier-links", token);
        linkRequest.Content = JsonContent.Create(new CreatePartSupplierLinkRequest(
            null,
            supplier.SupplierId,
            "CSV-VP-1",
            true));
        var linkResponse = await _supplyarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();

        var csv = string.Join('\n', new[]
        {
            "supplier_key,part_key,snapshot_key,unit_price,currency_code,minimum_order_quantity,effective_from,source,notes",
            "csv-price-supplier-v1,csv-price-part-v1,CSV-PRICE-1,42.75,USD,5,2026-01-01,supplier_feed,Imported supplier price list"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/price-list-csv", token);
        previewRequest.Content = JsonContent.Create(new PriceListCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<PriceListCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.PricesAccepted);
        Assert.Equal(0, preview.PricesCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/price-list-csv", token);
        commitRequest.Content = JsonContent.Create(new PriceListCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<PriceListCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.PricesCreated);

        var pricesResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/pricing-snapshots?partId={part.PartId}&supplierId={supplier.SupplierId}", token));
        pricesResponse.EnsureSuccessStatusCode();
        var prices = (await pricesResponse.Content.ReadFromJsonAsync<List<PricingSnapshotResponse>>())!;
        Assert.Contains(prices, x => x.SnapshotKey == "CSV-PRICE-1" && x.UnitPrice == 42.75m);
    }

    [Fact]
    public async Task V1_lead_time_list_csv_import_preview_and_commit_creates_lead_time_snapshot()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "csv-lead-supplier-v1",
            null,
            null,
            "CSV Lead Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "csv-lead-part-v1",
            null,
            "CSV Lead Part",
            "Imported lead time list part",
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var linkRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/supplier-links", token);
        linkRequest.Content = JsonContent.Create(new CreatePartSupplierLinkRequest(
            null,
            supplier.SupplierId,
            "CSV-LT-1",
            true));
        var linkResponse = await _supplyarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();

        var csv = string.Join('\n', new[]
        {
            "supplier_key,part_key,snapshot_key,lead_time_days,effective_from,source,notes",
            "csv-lead-supplier-v1,csv-lead-part-v1,CSV-LEAD-1,9,2026-01-01,manual,Imported lead-time list"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/lead-time-list-csv", token);
        previewRequest.Content = JsonContent.Create(new LeadTimeListCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<LeadTimeListCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.LeadTimesAccepted);
        Assert.Equal(0, preview.LeadTimesCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/lead-time-list-csv", token);
        commitRequest.Content = JsonContent.Create(new LeadTimeListCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<LeadTimeListCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.LeadTimesCreated);

        var leadTimesResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/lead-time-snapshots?partId={part.PartId}&supplierId={supplier.SupplierId}", token));
        leadTimesResponse.EnsureSuccessStatusCode();
        var leadTimes = (await leadTimesResponse.Content.ReadFromJsonAsync<List<LeadTimeSnapshotResponse>>())!;
        Assert.Contains(leadTimes, x => x.SnapshotKey == "CSV-LEAD-1" && x.LeadTimeDays == 9);
    }

    [Fact]
    public async Task V1_availability_list_csv_import_preview_and_commit_creates_availability_snapshot()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "csv-avail-supplier-v1",
            null,
            null,
            "CSV Availability Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "csv-avail-part-v1",
            null,
            "CSV Availability Part",
            "Imported availability list part",
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var linkRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/supplier-links", token);
        linkRequest.Content = JsonContent.Create(new CreatePartSupplierLinkRequest(
            null,
            supplier.SupplierId,
            "CSV-AVL-1",
            true));
        var linkResponse = await _supplyarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();

        var csv = string.Join('\n', new[]
        {
            "supplier_key,part_key,snapshot_key,quantity_available,availability_status,effective_from,source,notes",
            "csv-avail-supplier-v1,csv-avail-part-v1,CSV-AVAIL-1,14,limited,2026-01-01,supplier_feed,Imported availability list"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/availability-list-csv", token);
        previewRequest.Content = JsonContent.Create(new AvailabilityListCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<AvailabilityListCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.AvailabilityAccepted);
        Assert.Equal(0, preview.AvailabilityCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/availability-list-csv", token);
        commitRequest.Content = JsonContent.Create(new AvailabilityListCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<AvailabilityListCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.AvailabilityCreated);

        var availabilityResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/availability-snapshots?partId={part.PartId}&supplierId={supplier.SupplierId}", token));
        availabilityResponse.EnsureSuccessStatusCode();
        var availability = (await availabilityResponse.Content.ReadFromJsonAsync<List<AvailabilitySnapshotResponse>>())!;
        Assert.Contains(availability, x => x.SnapshotKey == "CSV-AVAIL-1" && x.QuantityAvailable == 14m && x.AvailabilityStatus == "limited");
    }

    [Fact]
    public async Task V1_contracts_csv_import_preview_and_commit_creates_contract_records()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "csv-contract-supplier-v1",
            null,
            null,
            "CSV Contract Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var csv = string.Join('\n', new[]
        {
            "supplier_key,contract_key,contract_type,title,effective_at,expires_at,renewal_at,payment_terms,freight_terms,warranty_terms,minimum_spend,service_level_agreement,approval_status,status,notes",
            "csv-contract-supplier-v1,CSV-CONTRACT-1,price_agreement,CSV Contract Import,2026-01-01,2027-01-01,2026-10-01,net_30,supplier_prepaid,one_year,2500,ships stocked parts within two business days,approved,active,Imported contract"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/contracts-csv", token);
        previewRequest.Content = JsonContent.Create(new ContractsCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<ContractsCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.ContractsAccepted);
        Assert.Equal(0, preview.ContractsCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/contracts-csv", token);
        commitRequest.Content = JsonContent.Create(new ContractsCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<ContractsCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.ContractsCreated);

        var contractsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/contracts/records?supplierId={supplier.SupplierId}&status=active", token));
        contractsResponse.EnsureSuccessStatusCode();
        var contracts = (await contractsResponse.Content.ReadFromJsonAsync<List<SupplyContractResponse>>())!;
        Assert.Contains(contracts, x => x.ContractKey == "CSV-CONTRACT-1" && x.MinimumSpend == 2500m);
    }

    [Fact]
    public async Task V1_suppliers_csv_import_preview_and_commit_creates_parent_and_sub_unit_suppliers()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var csv = string.Join('\n', new[]
        {
            "supplier_key,parent_supplier_key,unit_kind,display_name,legal_name,tax_identifier,approval_status,status,notes,service_types",
            "csv-supplier-import-parent-v1,,identity,CSV Supplier Parent,CSV Supplier Parent LLC,12-3456789,approved,active,Imported supplier identity,products|parts",
            "csv-supplier-import-unit-v1,csv-supplier-import-parent-v1,sub_unit,CSV Supplier Unit,CSV Supplier Unit LLC,,pending,active,Imported supplier sub unit,parts|maintenance"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/suppliers-csv", token);
        previewRequest.Content = JsonContent.Create(new SuppliersCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<SuppliersCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(2, preview.RowsRead);
        Assert.Equal(2, preview.SuppliersAccepted);
        Assert.Equal(0, preview.SuppliersCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/suppliers-csv", token);
        commitRequest.Content = JsonContent.Create(new SuppliersCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<SuppliersCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(2, commit.SuppliersCreated);

        var suppliersResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/suppliers", token));
        suppliersResponse.EnsureSuccessStatusCode();
        var suppliers = (await suppliersResponse.Content.ReadFromJsonAsync<List<SupplierResponse>>())!;
        Assert.Contains(suppliers, x => x.SupplierKey == "csv-supplier-import-parent-v1" && x.ApprovalStatus == "approved" && x.ServiceTypes.SequenceEqual(["products", "parts"]));
        Assert.Contains(suppliers, x => x.SupplierKey == "csv-supplier-import-unit-v1" && x.ParentSupplierDisplayName == "CSV Supplier Parent" && x.ServiceTypes.SequenceEqual(["parts", "maintenance"]));
    }

    [Fact]
    public async Task V1_contacts_csv_import_preview_and_commit_creates_supplier_contacts()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "csv-contact-supplier-v1",
            null,
            null,
            "CSV Contact Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplierRef = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var csv = string.Join('\n', new[]
        {
            "supplier_key,contact_name,email,phone,role_label,is_primary",
            "csv-contact-supplier-v1,CSV Contact,contact@csvsupplier.example,555-0101,Sales,true"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/contacts-csv", token);
        previewRequest.Content = JsonContent.Create(new ContactsCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<ContactsCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.ContactsAccepted);
        Assert.Equal(0, preview.ContactsCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/contacts-csv", token);
        commitRequest.Content = JsonContent.Create(new ContactsCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<ContactsCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.ContactsCreated);

        var contactsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/suppliers/{supplierRef.SupplierId}", token));
        contactsResponse.EnsureSuccessStatusCode();
        var supplier = (await contactsResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;
        Assert.Contains(supplier.Contacts, x => x.Email == "contact@csvsupplier.example" && x.IsPrimary);
    }

    [Fact]
    public async Task V1_open_purchase_orders_csv_import_preview_and_commit_creates_approved_open_po()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "csv-open-po-supplier-v1",
            null,
            null,
            "CSV Open PO Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "csv-open-po-part-v1",
            null,
            "CSV Open PO Part",
            "Imported open purchase order part",
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();

        var csv = string.Join('\n', new[]
        {
            "order_key,request_key,supplier_key,part_key,quantity_ordered,title,line_notes,order_notes",
            "csv-open-po-1,csv-open-pr-1,csv-open-po-supplier-v1,csv-open-po-part-v1,7,CSV Open PO,imported line,imported open purchase order"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/open-purchase-orders-csv", token);
        previewRequest.Content = JsonContent.Create(new OpenPurchaseOrdersCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<OpenPurchaseOrdersCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.OrdersAccepted);
        Assert.Equal(1, preview.LinesAccepted);
        Assert.Equal(0, preview.OrdersCreated);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/open-purchase-orders-csv", token);
        commitRequest.Content = JsonContent.Create(new OpenPurchaseOrdersCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<OpenPurchaseOrdersCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.OrdersCreated);

        var purchaseOrdersResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/purchase-orders?status=approved", token));
        purchaseOrdersResponse.EnsureSuccessStatusCode();
        var purchaseOrders = (await purchaseOrdersResponse.Content.ReadFromJsonAsync<List<PurchaseOrderResponse>>())!;
        Assert.Contains(
            purchaseOrders,
            x => x.OrderKey == "csv-open-po-1"
                && x.SupplierId == supplier.SupplierId
                && x.Lines.Count == 1
                && x.Lines[0].QuantityOrdered == 7m);
    }

    [Fact]
    public async Task V1_purchase_history_csv_import_preview_and_commit_posts_receipt()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "csv-history-supplier-v1",
            null,
            null,
            "CSV History Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "csv-history-part-v1",
            null,
            "CSV History Part",
            "Imported purchase history part",
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "csv-history-wh-v1",
            "CSV History Warehouse",
            "warehouse",
            string.Empty,
            _staffarrSiteOrgUnitId));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/inventory/locations/{location.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("csv-history-bin-v1", "History A1"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();

        var csv = string.Join('\n', new[]
        {
            "order_key,request_key,receipt_key,supplier_key,part_key,quantity_ordered,quantity_received,inventory_bin_key,title,line_notes,order_notes,receipt_notes",
            "csv-history-po-1,csv-history-pr-1,csv-history-rcpt-1,csv-history-supplier-v1,csv-history-part-v1,4,4,csv-history-bin-v1,CSV Purchase History,imported historical line,imported historical order,imported historical receipt"
        });

        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/imports/purchase-history-csv", token);
        previewRequest.Content = JsonContent.Create(new PurchaseHistoryCsvImportRequest(csv, DryRun: true));
        var previewResponse = await _supplyarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<PurchaseHistoryCsvImportResponse>())!;
        Assert.True(preview.DryRun);
        Assert.True(preview.Succeeded);
        Assert.Equal(1, preview.RowsRead);
        Assert.Equal(1, preview.OrdersAccepted);
        Assert.Equal(1, preview.LinesAccepted);
        Assert.Equal(0, preview.OrdersCreated);
        Assert.Equal(0, preview.ReceiptsPosted);

        var commitRequest = Authorized(HttpMethod.Post, "/api/v1/imports/purchase-history-csv", token);
        commitRequest.Content = JsonContent.Create(new PurchaseHistoryCsvImportRequest(csv, DryRun: false));
        var commitResponse = await _supplyarrClient.SendAsync(commitRequest);
        commitResponse.EnsureSuccessStatusCode();
        var commit = (await commitResponse.Content.ReadFromJsonAsync<PurchaseHistoryCsvImportResponse>())!;
        Assert.False(commit.DryRun);
        Assert.True(commit.Succeeded);
        Assert.Equal(1, commit.OrdersCreated);
        Assert.Equal(1, commit.ReceiptsPosted);

        var purchaseOrdersResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/purchase-orders?status=issued", token));
        purchaseOrdersResponse.EnsureSuccessStatusCode();
        var purchaseOrders = (await purchaseOrdersResponse.Content.ReadFromJsonAsync<List<PurchaseOrderResponse>>())!;
        Assert.Contains(
            purchaseOrders,
            x => x.OrderKey == "csv-history-po-1"
                && x.Lines.Count == 1
                && x.Lines[0].QuantityReceived == 4m
                && x.Lines[0].QuantityRemaining == 0m);

        var purchaseOrdersExportResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/purchase-orders.csv?status=issued", token));
        purchaseOrdersExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", purchaseOrdersExportResponse.Content.Headers.ContentType?.MediaType);
        var purchaseOrdersCsv = await purchaseOrdersExportResponse.Content.ReadAsStringAsync();
        Assert.Contains("orderKey,status,requestKey,supplierKey", purchaseOrdersCsv, StringComparison.Ordinal);
        Assert.Contains("csv-history-po-1,issued,csv-history-pr-1,csv-history-supplier-v1", purchaseOrdersCsv, StringComparison.Ordinal);

        var receiptsExportResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/receipts.csv?status=posted", token));
        receiptsExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", receiptsExportResponse.Content.Headers.ContentType?.MediaType);
        var receiptsCsv = await receiptsExportResponse.Content.ReadAsStringAsync();
        Assert.Contains("receiptKey,status,orderKey,supplierKey", receiptsCsv, StringComparison.Ordinal);
        Assert.Contains("csv-history-rcpt-1,posted,csv-history-po-1,csv-history-supplier-v1", receiptsCsv, StringComparison.Ordinal);

        var invoiceSupportExportResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/invoice-support.csv?status=posted", token));
        invoiceSupportExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", invoiceSupportExportResponse.Content.Headers.ContentType?.MediaType);
        var invoiceSupportCsv = await invoiceSupportExportResponse.Content.ReadAsStringAsync();
        Assert.Contains("receiptKey,receiptStatus,postedAt,orderKey,orderStatus,requestKey,supplierKey", invoiceSupportCsv, StringComparison.Ordinal);
        Assert.Contains("csv-history-rcpt-1,posted,", invoiceSupportCsv, StringComparison.Ordinal);
        Assert.Contains(",csv-history-po-1,issued,csv-history-pr-1,csv-history-supplier-v1", invoiceSupportCsv, StringComparison.Ordinal);

        var importedPurchaseOrder = purchaseOrders.Single(x => x.OrderKey == "csv-history-po-1");
        var evidencePacketResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/exports/compliance-evidence-packet.csv?purchaseOrderId={importedPurchaseOrder.PurchaseOrderId}", token));
        evidencePacketResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", evidencePacketResponse.Content.Headers.ContentType?.MediaType);
        var evidencePacketCsv = await evidencePacketResponse.Content.ReadAsStringAsync();
        Assert.Contains("recordType,entityType,entityKey,status,relatedKey,description,evidenceAt,sourcePath", evidencePacketCsv, StringComparison.Ordinal);
        Assert.Contains("purchase_order,purchase_order,csv-history-po-1,issued,csv-history-pr-1", evidencePacketCsv, StringComparison.Ordinal);
        Assert.Contains("receipt,receiving_receipt,csv-history-rcpt-1,posted,csv-history-po-1", evidencePacketCsv, StringComparison.Ordinal);
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
            "100 Dock St",
            _staffarrSiteOrgUnitId));
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
    public async Task Inventory_location_create_requires_staffarr_site_and_snapshots_active_site()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var missingSiteRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/locations", token);
        missingSiteRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "missing-site",
            "Missing StaffArr Site",
            "warehouse",
            "100 Dock St"));
        var missingSiteResponse = await _supplyarrClient.SendAsync(missingSiteRequest);
        Assert.Equal(HttpStatusCode.BadRequest, missingSiteResponse.StatusCode);

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            "legacy-type-site",
            "Legacy Type Site",
            "site",
            "100 Dock St",
            _staffarrSiteOrgUnitId));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        Assert.Equal(_staffarrSiteOrgUnitId, location.StaffarrSiteOrgUnitId);
        Assert.Equal("Central Parts Site", location.StaffarrSiteNameSnapshot);
        Assert.Equal("active", location.StaffarrSiteResolutionStatus);
        Assert.Equal("parts_room", location.LocationType);
    }

    [Fact]
    public async Task Wms_reserve_pick_ship_cancel_are_ledger_backed_and_idempotent()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/v1/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            "wms-ledger-part",
            null,
            "WMS Ledger Part",
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
            "wms-ledger-wh",
            "WMS Ledger Warehouse",
            "warehouse",
            "400 Dock St",
            _staffarrSiteOrgUnitId));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/inventory/locations/{location.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("wms-bin", "WMS Bin"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var bin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var stockRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/stock", token);
        stockRequest.Content = JsonContent.Create(new UpsertPartStockLevelRequest(part.PartId, bin.BinId, 20m));
        var stockResponse = await _supplyarrClient.SendAsync(stockRequest);
        stockResponse.EnsureSuccessStatusCode();

        var reserveRequest = Authorized(HttpMethod.Post, "/api/v1/wms/reserve", token);
        reserveRequest.Content = JsonContent.Create(new ReserveStockRequest(
            "wms-reserve-1",
            part.PartId,
            bin.BinId,
            5m,
            "work_order",
            Guid.NewGuid(),
            "reserve"));
        var reserveResponse = await _supplyarrClient.SendAsync(reserveRequest);
        reserveResponse.EnsureSuccessStatusCode();
        var reserved = (await reserveResponse.Content.ReadFromJsonAsync<WmsMovementResponse>())!;
        Assert.Single(reserved.Entries);
        Assert.Equal(5m, reserved.Entries[0].QuantityReservedDelta);
        Assert.Equal(_staffarrSiteOrgUnitId, reserved.Entries[0].StaffarrSiteOrgUnitId);

        var replayReserveRequest = Authorized(HttpMethod.Post, "/api/v1/wms/reserve", token);
        replayReserveRequest.Content = JsonContent.Create(new ReserveStockRequest(
            "wms-reserve-1",
            part.PartId,
            bin.BinId,
            5m,
            "work_order",
            null,
            "reserve replay"));
        var replayReserveResponse = await _supplyarrClient.SendAsync(replayReserveRequest);
        replayReserveResponse.EnsureSuccessStatusCode();
        var replayed = (await replayReserveResponse.Content.ReadFromJsonAsync<WmsMovementResponse>())!;
        Assert.Equal(reserved.MovementGroupId, replayed.MovementGroupId);
        Assert.Single(replayed.Entries);

        var pickRequest = Authorized(HttpMethod.Post, "/api/v1/wms/pick", token);
        pickRequest.Content = JsonContent.Create(new PickStockRequest("wms-pick-1", part.PartId, bin.BinId, 3m));
        (await _supplyarrClient.SendAsync(pickRequest)).EnsureSuccessStatusCode();

        var shipRequest = Authorized(HttpMethod.Post, "/api/v1/wms/ship", token);
        shipRequest.Content = JsonContent.Create(new ShipStockRequest("wms-ship-1", part.PartId, bin.BinId, 3m));
        (await _supplyarrClient.SendAsync(shipRequest)).EnsureSuccessStatusCode();

        var cancelRequest = Authorized(HttpMethod.Post, "/api/v1/wms/cancel", token);
        cancelRequest.Content = JsonContent.Create(new CancelStockMovementRequest(
            "wms-cancel-1",
            part.PartId,
            bin.BinId,
            2m,
            "cancel remaining"));
        (await _supplyarrClient.SendAsync(cancelRequest)).EnsureSuccessStatusCode();

        var ledgerRequest = Authorized(
            HttpMethod.Get,
            $"/api/v1/wms/stock-ledger?partId={part.PartId}&binId={bin.BinId}",
            token);
        var ledgerResponse = await _supplyarrClient.SendAsync(ledgerRequest);
        ledgerResponse.EnsureSuccessStatusCode();
        var ledger = (await ledgerResponse.Content.ReadFromJsonAsync<List<WmsStockLedgerEntryResponse>>())!;

        Assert.Equal(4, ledger.Count);
        Assert.Contains(ledger, x => x.MovementType == "reserve" && x.QuantityReservedDelta == 5m);
        Assert.Contains(ledger, x => x.MovementType == "pick" && x.QuantityReservedDelta == 0m);
        Assert.Contains(ledger, x => x.MovementType == "ship" && x.QuantityOnHandDelta == -3m && x.QuantityReservedDelta == -3m);
        Assert.Contains(ledger, x => x.MovementType == "cancel" && x.QuantityReservedDelta == -2m);
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
            "100 Dock St",
            _staffarrSiteOrgUnitId));
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
            "300 Dock St",
            _staffarrSiteOrgUnitId));
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
        var returns = await listReturnsResponse.Content.ReadFromJsonAsync<List<SupplierReturnResponse>>();
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
    public async Task Warranty_claim_submit_publishes_claim_facts_to_compliancecore()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var keyPrefix = $"wc-fact-{Guid.NewGuid():N}"[..18];

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            $"{keyPrefix}-supplier",
            null,
            null,
            "Warranty Fact Supplier",
            string.Empty,
            string.Empty,
            null,
            ["maintenance"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"{keyPrefix}-part",
            null,
            "Warranty Fact Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createClaimRequest = Authorized(HttpMethod.Post, "/api/warranty-claims", token);
        createClaimRequest.Content = JsonContent.Create(new CreateSupplierWarrantyClaimRequest(
            $"{keyPrefix}-claim",
            WarrantyClaimTypes.Defective,
            supplier.SupplierId,
            supplier.SupplierId,
            part.PartId,
            1m,
            "Failed acceptance inspection.",
            null,
            null,
            null,
            null,
            null));
        var createClaimResponse = await _supplyarrClient.SendAsync(createClaimRequest);
        createClaimResponse.EnsureSuccessStatusCode();
        var claim = (await createClaimResponse.Content.ReadFromJsonAsync<WarrantyClaimResponse>())!;

        var submitClaimRequest = Authorized(
            HttpMethod.Post,
            $"/api/warranty-claims/{claim.WarrantyClaimId}/submit",
            token);
        submitClaimRequest.Content = JsonContent.Create(new SubmitWarrantyClaimRequest("Filed with supplier."));
        var submitClaimResponse = await _supplyarrClient.SendAsync(submitClaimRequest);
        submitClaimResponse.EnsureSuccessStatusCode();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IntegrationEventProcessingService>();
        await processor.ProcessBatchAsync(
            new global::SupplyArr.Api.Contracts.ProcessIntegrationEventsRequest(PlatformSeeder.DemoTenantId, 20));

        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.WarrantyClaimStatus
                && x.StringValue == WarrantyClaimStatuses.Submitted
                && x.SourceEventKind == IntegrationOutboxEventKinds.WarrantyClaimSubmitted);
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.WarrantyClaimFiled
                && x.BooleanValue == true
                && x.ScopeKey == $"warranty_claim:{claim.WarrantyClaimId:D}".ToLowerInvariant());
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
    public async Task Supplier_incidents_v1_supplier_routes_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "incident-supplier",
            null,
            null,
            "Incident Supplier",
            string.Empty,
            null,
            string.Empty,
            null,
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createIncidentRequest = Authorized(HttpMethod.Post, "/api/v1/supplier-incidents", token);
        createIncidentRequest.Content = JsonContent.Create(new CreateSupplierIncidentRequest(
            supplier.SupplierId,
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
        Assert.Equal(supplier.SupplierId, incident.SupplierId);

        var listResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/supplier-incidents", token));
        listResponse.EnsureSuccessStatusCode();

        var bySupplierResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/suppliers/{supplier.SupplierId}/supplier-incidents", token));
        bySupplierResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Supplier_incident_create_publishes_incident_facts_to_compliancecore()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var keyPrefix = $"si-fact-{Guid.NewGuid():N}"[..18];

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            $"{keyPrefix}-supplier",
            null,
            null,
            "Supplier Incident Fact Supplier",
            string.Empty,
            string.Empty,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createIncidentRequest = Authorized(HttpMethod.Post, "/api/v1/supplier-incidents", token);
        createIncidentRequest.Content = JsonContent.Create(new CreateSupplierIncidentRequest(
            supplier.SupplierId,
            $"{keyPrefix}-incident",
            "Supplier incident fact",
            "Supplier shipment triggered a compliance incident.",
            "compliance",
            "high",
            null,
            null,
            null,
            null,
            null));
        var createIncidentResponse = await _supplyarrClient.SendAsync(createIncidentRequest);
        createIncidentResponse.EnsureSuccessStatusCode();
        var incident = (await createIncidentResponse.Content.ReadFromJsonAsync<SupplierIncidentResponse>())!;

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outboxEvent = await db.IntegrationOutboxEvents.SingleAsync(
            x => x.EventKind == IntegrationOutboxEventKinds.SupplierIncidentCreated
                && x.RelatedEntityId == incident.IncidentId);
        var publisher = scope.ServiceProvider.GetRequiredService<ComplianceCoreFactPublisherService>();
        await publisher.TryPublishFromOutboxAsync(outboxEvent);

        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.SupplierIncidentStatus
                && x.StringValue == SupplierIncidentStatuses.Open
                && x.SourceEventKind == IntegrationOutboxEventKinds.SupplierIncidentCreated);
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.SupplierIncidentIsActive
                && x.BooleanValue == true
                && x.ScopeKey == $"supplier_incident:{incident.IncidentId:D}".ToLowerInvariant());
    }

    [Fact]
    public async Task Supplier_restrictions_v1_routes_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "restricted-supplier",
            null,
            null,
            "Restricted Supplier",
            string.Empty,
            string.Empty,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createRestrictionRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/suppliers/{supplier.SupplierId}/restrictions",
            token);
        createRestrictionRequest.Content = JsonContent.Create(new CreateSupplierRestrictionRequest(
            "hold-supplier",
            ["purchase_orders"],
            "Open compliance hold",
            null,
            null));
        var createRestrictionResponse = await _supplyarrClient.SendAsync(createRestrictionRequest);
        createRestrictionResponse.EnsureSuccessStatusCode();
        var restriction = (await createRestrictionResponse.Content.ReadFromJsonAsync<SupplierRestrictionResponse>())!;
        Assert.Equal(supplier.SupplierId, restriction.SupplierId);

        var listResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/supplier-restrictions", token));
        listResponse.EnsureSuccessStatusCode();

        var bySupplierResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/suppliers/{supplier.SupplierId}/restrictions", token));
        bySupplierResponse.EnsureSuccessStatusCode();

        var enforcementResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/suppliers/{supplier.SupplierId}/restrictions/enforcement", token));
        enforcementResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Supplier_onboarding_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "onboarding-supplier",
            null,
            null,
            "Onboarding Supplier",
            "Onboarding Supplier LLC",
            null,
            string.Empty,
            ["parts", "products"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var requirementsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/supplier-onboarding/document-requirements", token));
        requirementsResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/supplier-onboarding/pending", token));
        pendingResponse.EnsureSuccessStatusCode();

        var supplierDocsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/suppliers/{supplier.SupplierId}/compliance-documents", token));
        supplierDocsResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Supplier_onboarding_approval_publishes_supplier_approval_facts_to_compliancecore()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            $"cc-onb-{Guid.NewGuid():N}"[..16],
            null,
            null,
            "Compliance Core Onboarding Supplier",
            "Compliance Core Onboarding Supplier LLC",
            null,
            string.Empty,
            ["parts", "products"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var startRequest = Authorized(HttpMethod.Post, "/api/supplier-onboarding/start", token);
        startRequest.Content = JsonContent.Create(new StartSupplierOnboardingRequest(supplier.SupplierId, "Initial onboarding"));
        var startResponse = await _supplyarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();

        foreach (var (documentKey, documentTypeKey, title) in new[]
        {
            ("W9", "w9", "W-9"),
            ("INS", "insurance_certificate", "Insurance"),
            ("AGR", "supplier_agreement", "Agreement")
        })
        {
            var registerRequest = Authorized(
                HttpMethod.Post,
                $"/api/suppliers/{supplier.SupplierId}/compliance-documents",
                token);
            registerRequest.Content = JsonContent.Create(new SupplierComplianceDocumentRegistrationRequest(
                $"{documentKey}-{Guid.NewGuid():N}"[..16],
                documentTypeKey,
                title,
                null,
                null,
                $"{documentKey}.pdf",
                "application/pdf",
                1024,
                string.Empty));
            var registerResponse = await _supplyarrClient.SendAsync(registerRequest);
            registerResponse.EnsureSuccessStatusCode();
            var document = (await registerResponse.Content.ReadFromJsonAsync<SupplierComplianceDocumentResponse>())!;

            var approveDocumentResponse = await _supplyarrClient.SendAsync(
                Authorized(
                    HttpMethod.Post,
                    $"/api/suppliers/{supplier.SupplierId}/compliance-documents/{document.DocumentId}/approve",
                    token));
            approveDocumentResponse.EnsureSuccessStatusCode();
        }

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/supplier-onboarding/suppliers/{supplier.SupplierId}/submit",
            token);
        submitRequest.Content = JsonContent.Create(new SubmitSupplierOnboardingForReviewRequest("Ready"));
        var submitResponse = await _supplyarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var approveResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/supplier-onboarding/suppliers/{supplier.SupplierId}/approve", token));
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<SupplierOnboardingResponse>())!;
        Assert.Equal("approved", approved.OnboardingStatus);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IntegrationEventProcessingService>();
        var processed = await processor.ProcessBatchAsync(
            new global::SupplyArr.Api.Contracts.ProcessIntegrationEventsRequest(PlatformSeeder.DemoTenantId, 20));

        Assert.True(processed.OutboxProcessedCount >= 2);
        Assert.True(_complianceCoreHandler.FactIngestCount >= 1);
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.SupplierApprovalStatus
                && x.StringValue == "approved"
                && x.SourceEventKind == IntegrationOutboxEventKinds.SupplierOnboardingApproved);
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.SupplierIsApproved
                && x.BooleanValue == true
                && x.ScopeKey == $"supplier:{supplier.SupplierId:D}".ToLowerInvariant());
    }

    [Fact]
    public async Task Supplier_document_approval_publishes_document_status_facts_to_compliancecore()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var keyPrefix = $"doc-fact-{Guid.NewGuid():N}"[..18];

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            $"{keyPrefix}-supplier",
            null,
            null,
            "Document Fact Supplier",
            "Document Fact Supplier LLC",
            null,
            string.Empty,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var registerRequest = Authorized(
            HttpMethod.Post,
            $"/api/suppliers/{supplier.SupplierId}/compliance-documents",
            token);
        registerRequest.Content = JsonContent.Create(new SupplierComplianceDocumentRegistrationRequest(
            $"{keyPrefix}-insurance",
            "insurance_certificate",
            "Expired insurance certificate",
            DateTimeOffset.UtcNow.AddDays(-1),
            null,
            "insurance.pdf",
            "application/pdf",
            2048,
            string.Empty));
        var registerResponse = await _supplyarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var document = (await registerResponse.Content.ReadFromJsonAsync<SupplierComplianceDocumentResponse>())!;

        var approveResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/suppliers/{supplier.SupplierId}/compliance-documents/{document.DocumentId}/approve",
                token));
        approveResponse.EnsureSuccessStatusCode();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outboxEvent = await db.IntegrationOutboxEvents.SingleAsync(
            x => x.EventKind == IntegrationOutboxEventKinds.SupplierComplianceDocumentApproved
                && x.RelatedEntityId == document.DocumentId);
        var publisher = scope.ServiceProvider.GetRequiredService<ComplianceCoreFactPublisherService>();
        await publisher.TryPublishFromOutboxAsync(outboxEvent);

        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.SupplierDocumentStatus
                && x.StringValue == SupplierComplianceDocumentReviewStatuses.Expired
                && x.SourceEventKind == IntegrationOutboxEventKinds.SupplierComplianceDocumentApproved);
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.SupplierDocumentAttached
                && x.BooleanValue == true
                && x.ScopeKey == $"supplier_document:{document.DocumentId:D}".ToLowerInvariant());
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.SupplierDocumentExpired
                && x.BooleanValue == true);
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
    public async Task Emergency_purchase_override_publishes_justification_facts_to_compliancecore()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var keyPrefix = $"ep-fact-{Guid.NewGuid():N}"[..18];

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            $"{keyPrefix}-supplier",
            null,
            null,
            "Emergency Fact Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"{keyPrefix}-part",
            null,
            "Emergency Fact Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createEmergencyRequest = Authorized(HttpMethod.Post, "/api/emergency-purchases", token);
        createEmergencyRequest.Content = JsonContent.Create(new
        {
            RequestKey = $"{keyPrefix}-req",
            Title = "Emergency procurement",
            EmergencyReason = "Vehicle down for safety-critical service.",
            SupplierId = supplier.SupplierId,
            Notes = string.Empty,
            Lines = new[]
            {
                new CreatePurchaseRequestLineRequest(part.PartId, 1m, "Emergency replacement")
            }
        });
        var createEmergencyResponse = await _supplyarrClient.SendAsync(createEmergencyRequest);
        createEmergencyResponse.EnsureSuccessStatusCode();
        var emergency = (await createEmergencyResponse.Content.ReadFromJsonAsync<EmergencyPurchaseResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/emergency-purchases/{emergency.PurchaseRequestId}/expedited-submit",
            token);
        submitRequest.Content = JsonContent.Create(new ExpeditedSubmitEmergencyPurchaseRequest("Immediate override review."));
        var submitResponse = await _supplyarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var approveRequest = Authorized(
            HttpMethod.Post,
            $"/api/emergency-purchases/{emergency.PurchaseRequestId}/manager-override-approve",
            token);
        approveRequest.Content = JsonContent.Create(
            new ManagerOverrideApproveEmergencyPurchaseRequest("Safety downtime requires immediate supplier use."));
        var approveResponse = await _supplyarrClient.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outboxEvent = await db.IntegrationOutboxEvents.SingleAsync(
            x => x.EventKind == IntegrationOutboxEventKinds.EmergencyPurchaseManagerOverrideApproved
                && x.RelatedEntityId == emergency.PurchaseRequestId);
        var publisher = scope.ServiceProvider.GetRequiredService<ComplianceCoreFactPublisherService>();
        await publisher.TryPublishFromOutboxAsync(outboxEvent);

        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.EmergencyPurchaseStatus
                && x.StringValue == PurchaseRequestStatuses.Approved
                && x.SourceEventKind == IntegrationOutboxEventKinds.EmergencyPurchaseManagerOverrideApproved);
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.EmergencyPurchaseJustified
                && x.BooleanValue == true
                && x.ScopeKey == $"purchase_request:{emergency.PurchaseRequestId:D}".ToLowerInvariant());
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.EmergencyPurchaseManagerOverrideApproved
                && x.BooleanValue == true);
    }

    [Fact]
    public async Task Reports_v1_aliases_work()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var supplierResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/suppliers/summary", token));
        supplierResponse.EnsureSuccessStatusCode();

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
            string.Empty,
            _staffarrSiteOrgUnitId));

        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Purchase_request_submit_approve_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "pr-supplier-001",
            null,
            null,
            "PR Supplier",
            "PR Supplier LLC",
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

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
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 6m, "Oil filters")]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
        Assert.Equal("draft", purchaseRequest.Status);
        Assert.Single(purchaseRequest.Lines);
        Assert.Equal(supplier.SupplierKey, purchaseRequest.SupplierKey);

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

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "po-supplier-001",
            null,
            null,
            "PO Supplier",
            "PO Supplier LLC",
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

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
            supplier.SupplierId,
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
        Assert.Equal(supplier.SupplierId, purchaseOrder.SupplierId);
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
    public async Task Purchase_order_issue_checks_compliancecore_supplier_use_gate()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var keyPrefix = $"po-gate-{Guid.NewGuid():N}"[..18];

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            $"{keyPrefix}-supplier",
            null,
            null,
            "PO Gate Supplier",
            "PO Gate Supplier LLC",
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"{keyPrefix}-part",
            null,
            "PO Gate Part",
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
            $"{keyPrefix}-pr",
            "PO gate source request",
            "For Compliance Core supplier gate test",
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 4m, "Gate-controlled part")]));
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
            new CreatePurchaseOrderFromPurchaseRequestRequest($"{keyPrefix}-po", null, null));
        var createPoResponse = await _supplyarrClient.SendAsync(createPoRequest);
        createPoResponse.EnsureSuccessStatusCode();
        var purchaseOrder = (await createPoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;

        var approvePoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/approve",
            token);
        (await _supplyarrClient.SendAsync(approvePoRequest)).EnsureSuccessStatusCode();

        _complianceCoreHandler.NextOutcome = "missing-evidence";
        _complianceCoreHandler.NextReasonCode = "supplier_documents_expired";
        _complianceCoreHandler.NextMessage = "Required supplier compliance documents are expired.";

        var blockedIssueRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/issue",
            token);
        var blockedIssueResponse = await _supplyarrClient.SendAsync(blockedIssueRequest);
        Assert.Equal(HttpStatusCode.Conflict, blockedIssueResponse.StatusCode);

        var blockedPo = await GetPurchaseOrderAsync(token, purchaseOrder.PurchaseOrderId);
        Assert.Equal("approved", blockedPo.Status);
        Assert.Null(blockedPo.IssuedAt);

        var blockedGateRequest = Assert.Single(_complianceCoreHandler.GateRequests);
        Assert.Equal("/api/v1/gates/can-use-supplier", blockedGateRequest.Path);
        Assert.Equal("Bearer", blockedGateRequest.AuthorizationScheme);
        Assert.Equal("supplyarr-to-compliancecore-token", blockedGateRequest.AuthorizationParameter);
        Assert.Equal(PlatformSeeder.DemoTenantId, blockedGateRequest.TenantId);
        Assert.Equal("purchase_order_issue", blockedGateRequest.ActivityContextKey);
        Assert.Equal("can_use_supplier", blockedGateRequest.WorkflowKey);
        Assert.Contains(blockedGateRequest.Subjects, subject =>
            subject.SubjectType == "supplier"
            && subject.SubjectReference == supplier.SupplierId.ToString("D")
            && subject.SourceProduct == "supplyarr");
        Assert.Equal(purchaseOrder.PurchaseOrderId.ToString("D"), blockedGateRequest.RuleContext["purchase_order_id"]);
        Assert.Equal(supplier.SupplierId.ToString("D"), blockedGateRequest.RuleContext["supplier_id"]);
        Assert.Equal(part.PartId.ToString("D"), blockedGateRequest.RuleContext["part_ids"]);

        _complianceCoreHandler.NextOutcome = "allow";
        _complianceCoreHandler.NextReasonCode = "supplier_compliance_clear";
        _complianceCoreHandler.NextMessage = "Supplier satisfies Compliance Core requirements.";

        var allowedIssueRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/issue",
            token);
        var allowedIssueResponse = await _supplyarrClient.SendAsync(allowedIssueRequest);
        allowedIssueResponse.EnsureSuccessStatusCode();
        var issuedPo = (await allowedIssueResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
        Assert.Equal("issued", issuedPo.Status);
        Assert.Equal(2, _complianceCoreHandler.GateRequests.Count);
    }

    [Fact]
    public async Task Purchase_order_issue_publishes_approved_supplier_source_facts_to_compliancecore()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var keyPrefix = $"po-src-{Guid.NewGuid():N}"[..18];

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            $"{keyPrefix}-supplier",
            null,
            null,
            "Approved Source Supplier",
            string.Empty,
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var approveSupplierRequest = Authorized(
            HttpMethod.Patch,
            $"/api/suppliers/{supplier.SupplierId}/approval-status",
            token);
        approveSupplierRequest.Content = JsonContent.Create(new UpdateSupplierApprovalStatusRequest("approved"));
        var approveSupplierResponse = await _supplyarrClient.SendAsync(approveSupplierRequest);
        approveSupplierResponse.EnsureSuccessStatusCode();

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"{keyPrefix}-part",
            null,
            "Approved Source Part",
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
            $"{keyPrefix}-pr",
            "Approved source PR",
            string.Empty,
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 2m, "Approved source part")]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        (await _supplyarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token))).EnsureSuccessStatusCode();
        (await _supplyarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            token))).EnsureSuccessStatusCode();

        var createPoRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/from-purchase-request/{purchaseRequest.PurchaseRequestId}",
            token);
        createPoRequest.Content = JsonContent.Create(
            new CreatePurchaseOrderFromPurchaseRequestRequest($"{keyPrefix}-po", null, null));
        var createPoResponse = await _supplyarrClient.SendAsync(createPoRequest);
        createPoResponse.EnsureSuccessStatusCode();
        var purchaseOrder = (await createPoResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;

        (await _supplyarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/approve",
            token))).EnsureSuccessStatusCode();
        var issueResponse = await _supplyarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/{purchaseOrder.PurchaseOrderId}/issue",
            token));
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outboxEvent = await db.IntegrationOutboxEvents.SingleAsync(
            x => x.EventKind == IntegrationOutboxEventKinds.PurchaseOrderIssued
                && x.RelatedEntityId == issued.PurchaseOrderId);
        var publisher = scope.ServiceProvider.GetRequiredService<ComplianceCoreFactPublisherService>();
        await publisher.TryPublishFromOutboxAsync(outboxEvent);

        var lineId = Assert.Single(issued.Lines).LineId;
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.PurchaseOrderLineSupplierApprovalStatus
                && x.StringValue == "approved"
                && x.ScopeKey == $"purchase_order_line:{lineId:D}".ToLowerInvariant());
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.PartSourcedFromApprovedSupplier
                && x.BooleanValue == true
                && x.SourceEventKind == IntegrationOutboxEventKinds.PurchaseOrderIssued);
    }

    [Fact]
    public async Task Purchase_order_v1_from_approved_pr_approve_issue_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "po-v1-supplier-001",
            null,
            null,
            "PO v1 Supplier",
            "PO v1 Supplier LLC",
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

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
            supplier.SupplierId,
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

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", managerToken);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            "po-supplier-buyer",
            null,
            null,
            "Buyer PO Supplier",
            "Buyer PO Supplier LLC",
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

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
            supplier.SupplierId,
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
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-2026-001",
            "po-rcv-2026-001",
            "rcv",
            5m);

        var createReceiptRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/receiving/from-purchase-order/{purchaseOrder.PurchaseOrderId}",
            token);
        createReceiptRequest.Content = JsonContent.Create(
            new CreateReceivingReceiptFromPurchaseOrderRequest(
                "rcpt-2026-001",
                bin.BinId,
                "Dock delivery",
                "ps-rcpt-2026-001",
                "packing-slip-2026-001.pdf"));
        var createReceiptResponse = await _supplyarrClient.SendAsync(createReceiptRequest);
        createReceiptResponse.EnsureSuccessStatusCode();
        var receipt = (await createReceiptResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("draft", receipt.Status);
        Assert.Single(receipt.Lines);
        Assert.Equal(5m, receipt.Lines[0].QuantityReceived);

        var postReceiptRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/receiving/{receipt.ReceivingReceiptId}/post",
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
            new CreateReceivingReceiptFromPurchaseOrderRequest(
                "rcpt-v1-001",
                bin.BinId,
                "Dock delivery",
                "ps-rcpt-v1-001",
                "packing-slip-v1-001.pdf"));
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
    public async Task Receiving_can_get_receipt_by_reference_key()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-key-001",
            "po-rcv-key-001",
            "rcv-key",
            3m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-key-001");

        var getByKeyRequest = Authorized(
            HttpMethod.Get,
            "/api/receiving/by-key/rcpt-key-001",
            token);
        var getByKeyResponse = await _supplyarrClient.SendAsync(getByKeyRequest);
        getByKeyResponse.EnsureSuccessStatusCode();
        var byKey = (await getByKeyResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;

        Assert.Equal(receipt.ReceivingReceiptId, byKey.ReceivingReceiptId);
        Assert.Equal("rcpt-key-001", byKey.ReceiptKey);
        Assert.Equal("draft", byKey.Status);
    }

    [Fact]
    public async Task Receiving_can_lookup_receipts_by_packing_slip_reference_route()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-ps-key-001",
            "po-rcv-ps-key-001",
            "rcv-ps-key",
            3m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-ps-key-001");

        var updatePackingSlipRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/packing-slip",
            token);
        updatePackingSlipRequest.Content = JsonContent.Create(
            new UpdateReceivingPackingSlipRequest("PS-SCAN-001", "ps-scan-001.pdf"));
        (await _supplyarrClient.SendAsync(updatePackingSlipRequest)).EnsureSuccessStatusCode();

        var byPackingSlipRequest = Authorized(
            HttpMethod.Get,
            "/api/receiving/by-packing-slip/PS-SCAN-001",
            token);
        var byPackingSlipResponse = await _supplyarrClient.SendAsync(byPackingSlipRequest);
        byPackingSlipResponse.EnsureSuccessStatusCode();
        var matches = (await byPackingSlipResponse.Content.ReadFromJsonAsync<List<ReceivingReceiptResponse>>())!;

        var matched = Assert.Single(matches, x => x.ReceivingReceiptId == receipt.ReceivingReceiptId);
        Assert.Equal("PS-SCAN-001", matched.PackingSlipReference);
    }

    [Fact]
    public async Task Receiving_can_create_receipt_from_purchase_order_key()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-po-create-key-001",
            "po-rcv-po-create-key-001",
            "rcv-po-create-key",
            4m);

        var createRequest = Authorized(
            HttpMethod.Post,
            "/api/receiving/from-purchase-order-key/po-rcv-po-create-key-001",
            token);
        createRequest.Content = JsonContent.Create(new CreateReceivingReceiptFromPurchaseOrderRequest(
            "rcpt-po-create-key-001",
            bin.BinId,
            "Created from order key",
            null,
            null));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;

        Assert.Equal("draft", created.Status);
        Assert.Equal(purchaseOrder.PurchaseOrderId, created.PurchaseOrderId);
        Assert.Equal("po-rcv-po-create-key-001", created.PurchaseOrderKey);
        Assert.Equal("rcpt-po-create-key-001", created.ReceiptKey);
    }

    [Fact]
    public async Task Receiving_create_from_purchase_order_can_select_lines_and_reject_invalid_selection()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, binOne, purchaseOrderOne) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-line-select-001",
            "po-rcv-line-select-001",
            "rcv-line-select-1",
            3m);
        var (_, _, purchaseOrderTwo) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-line-select-002",
            "po-rcv-line-select-002",
            "rcv-line-select-2",
            2m);

        var invalidRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/from-purchase-order/{purchaseOrderOne.PurchaseOrderId}",
            token);
        invalidRequest.Content = JsonContent.Create(new CreateReceivingReceiptFromPurchaseOrderRequest(
            "rcpt-line-select-002",
            binOne.BinId,
            null,
            null,
            null,
            [purchaseOrderTwo.Lines[0].LineId]));
        var invalidResponse = await _supplyarrClient.SendAsync(invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        var selectedLineId = purchaseOrderOne.Lines[0].LineId;
        var createRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/from-purchase-order/{purchaseOrderOne.PurchaseOrderId}",
            token);
        createRequest.Content = JsonContent.Create(new CreateReceivingReceiptFromPurchaseOrderRequest(
            "rcpt-line-select-001",
            binOne.BinId,
            null,
            null,
            null,
            [selectedLineId]));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Single(created.Lines);
        Assert.Equal(selectedLineId, created.Lines[0].PurchaseOrderLineId);
    }

    [Fact]
    public async Task Receiving_can_list_receipts_by_purchase_order_key()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, binOne, purchaseOrderOne) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-po-key-001",
            "po-rcv-po-key-001",
            "rcv-po-key-1",
            2m);
        var (_, binTwo, purchaseOrderTwo) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-po-key-002",
            "po-rcv-po-key-002",
            "rcv-po-key-2",
            1m);

        var receiptOne = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrderOne.PurchaseOrderId,
            binOne.BinId,
            "rcpt-po-key-001");
        _ = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrderTwo.PurchaseOrderId,
            binTwo.BinId,
            "rcpt-po-key-002");

        var listRequest = Authorized(
            HttpMethod.Get,
            "/api/receiving?purchaseOrderKey=po-rcv-po-key-001",
            token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var receipts = (await listResponse.Content.ReadFromJsonAsync<List<ReceivingReceiptResponse>>())!;

        Assert.Single(receipts);
        Assert.Equal(receiptOne.ReceivingReceiptId, receipts[0].ReceivingReceiptId);
        Assert.Equal(purchaseOrderOne.PurchaseOrderId, receipts[0].PurchaseOrderId);
    }

    [Fact]
    public async Task Receiving_can_list_receipts_by_packing_slip_reference()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, binOne, purchaseOrderOne) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-slip-001",
            "po-rcv-slip-001",
            "rcv-slip-1",
            2m);
        var (_, binTwo, purchaseOrderTwo) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-slip-002",
            "po-rcv-slip-002",
            "rcv-slip-2",
            1m);

        var receiptOne = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrderOne.PurchaseOrderId,
            binOne.BinId,
            "rcpt-slip-001");
        _ = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrderTwo.PurchaseOrderId,
            binTwo.BinId,
            "rcpt-slip-002");

        var listRequest = Authorized(
            HttpMethod.Get,
            "/api/receiving?packingSlipReference=ps-rcpt-slip-001",
            token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var receipts = (await listResponse.Content.ReadFromJsonAsync<List<ReceivingReceiptResponse>>())!;

        Assert.Single(receipts);
        Assert.Equal(receiptOne.ReceivingReceiptId, receipts[0].ReceivingReceiptId);
    }

    [Fact]
    public async Task Receiving_can_search_receipts_with_single_query()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, binOne, purchaseOrderOne) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-query-001",
            "po-rcv-query-001",
            "rcv-query-1",
            2m);
        var (_, binTwo, purchaseOrderTwo) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-query-002",
            "po-rcv-query-002",
            "rcv-query-2",
            1m);

        var receiptOne = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrderOne.PurchaseOrderId,
            binOne.BinId,
            "rcpt-query-001");
        _ = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrderTwo.PurchaseOrderId,
            binTwo.BinId,
            "rcpt-query-002");

        var listRequest = Authorized(
            HttpMethod.Get,
            "/api/receiving?query=rcpt-query-001",
            token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var receipts = (await listResponse.Content.ReadFromJsonAsync<List<ReceivingReceiptResponse>>())!;

        Assert.Single(receipts);
        Assert.Equal(receiptOne.ReceivingReceiptId, receipts[0].ReceivingReceiptId);
    }

    [Fact]
    public async Task Receiving_can_filter_and_search_receipts_by_invoice_reference()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, binOne, purchaseOrderOne) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-inv-filter-001",
            "po-rcv-inv-filter-001",
            "rcv-inv-filter-1",
            2m);
        var (_, binTwo, purchaseOrderTwo) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-inv-filter-002",
            "po-rcv-inv-filter-002",
            "rcv-inv-filter-2",
            1m);

        var receiptOne = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrderOne.PurchaseOrderId,
            binOne.BinId,
            "rcpt-inv-filter-001");
        _ = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrderTwo.PurchaseOrderId,
            binTwo.BinId,
            "rcpt-inv-filter-002");

        var updateInvoiceRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receiptOne.ReceivingReceiptId}/invoice",
            token);
        updateInvoiceRequest.Content = JsonContent.Create(
            new UpdateReceivingInvoiceRequest("INV-FILTER-001", "inv-filter-001.pdf"));
        (await _supplyarrClient.SendAsync(updateInvoiceRequest)).EnsureSuccessStatusCode();

        var filterRequest = Authorized(
            HttpMethod.Get,
            "/api/receiving?invoiceReference=INV-FILTER-001",
            token);
        var filterResponse = await _supplyarrClient.SendAsync(filterRequest);
        filterResponse.EnsureSuccessStatusCode();
        var filtered = (await filterResponse.Content.ReadFromJsonAsync<List<ReceivingReceiptResponse>>())!;
        Assert.Single(filtered);
        Assert.Equal(receiptOne.ReceivingReceiptId, filtered[0].ReceivingReceiptId);

        var queryRequest = Authorized(
            HttpMethod.Get,
            "/api/receiving?query=inv-filter-001",
            token);
        var queryResponse = await _supplyarrClient.SendAsync(queryRequest);
        queryResponse.EnsureSuccessStatusCode();
        var queryResults = (await queryResponse.Content.ReadFromJsonAsync<List<ReceivingReceiptResponse>>())!;
        Assert.Single(queryResults);
        Assert.Equal(receiptOne.ReceivingReceiptId, queryResults[0].ReceivingReceiptId);
    }

    [Fact]
    public async Task Receiving_can_update_inventory_bin_while_draft_only()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, originalBin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-bin-001",
            "po-rcv-bin-001",
            "rcv-bin",
            2m);
        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            originalBin.BinId,
            "rcpt-bin-001");

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/inventory/locations/{originalBin.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest("rcv-02", "Receiving Bin 02"));
        var createBinResponse = await _supplyarrClient.SendAsync(createBinRequest);
        createBinResponse.EnsureSuccessStatusCode();
        var replacementBin = (await createBinResponse.Content.ReadFromJsonAsync<InventoryBinResponse>())!;

        var updateDraftRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/inventory-bin",
            token);
        updateDraftRequest.Content = JsonContent.Create(new UpdateReceivingInventoryBinRequest(replacementBin.BinId));
        var updateDraftResponse = await _supplyarrClient.SendAsync(updateDraftRequest);
        updateDraftResponse.EnsureSuccessStatusCode();
        var updatedDraft = (await updateDraftResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;

        Assert.Equal(replacementBin.BinId, updatedDraft.InventoryBinId);
        Assert.Equal(replacementBin.LocationId, updatedDraft.InventoryLocationId);
        Assert.Equal("draft", updatedDraft.Status);

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var postResponse = await _supplyarrClient.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();

        var updatePostedRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/inventory-bin",
            token);
        updatePostedRequest.Content = JsonContent.Create(new UpdateReceivingInventoryBinRequest(originalBin.BinId));
        var updatePostedResponse = await _supplyarrClient.SendAsync(updatePostedRequest);
        Assert.Equal(HttpStatusCode.Conflict, updatePostedResponse.StatusCode);
    }

    [Fact]
    public async Task Receiving_can_update_line_condition_and_reject_invalid_value()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-cond-001",
            "po-rcv-cond-001",
            "rcv-cond",
            2m);
        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-cond-001");
        var line = receipt.Lines[0];

        var updateRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/condition",
            token);
        updateRequest.Content = JsonContent.Create(new UpdateReceivingReceiptLineConditionRequest("damaged"));
        var updateResponse = await _supplyarrClient.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;

        Assert.Equal("damaged", Assert.Single(updated.Lines).Condition);

        var invalidRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/condition",
            token);
        invalidRequest.Content = JsonContent.Create(new UpdateReceivingReceiptLineConditionRequest("unknown_condition"));
        var invalidResponse = await _supplyarrClient.SendAsync(invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
    }

    [Fact]
    public async Task Receiving_post_uses_line_condition_for_receipt_status()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-cond-status-001",
            "po-rcv-cond-status-001",
            "rcv-cond-status",
            2m);
        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-cond-status-001");
        var line = receipt.Lines[0];

        var updateConditionRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/condition",
            token);
        updateConditionRequest.Content = JsonContent.Create(new UpdateReceivingReceiptLineConditionRequest("damaged"));
        var updateConditionResponse = await _supplyarrClient.SendAsync(updateConditionRequest);
        updateConditionResponse.EnsureSuccessStatusCode();

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var postResponse = await _supplyarrClient.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();
        var posted = (await postResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;

        Assert.Equal("damaged", posted.Status);
    }

    [Fact]
    public async Task Receiving_post_releases_linked_purchase_order_line_reservations()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-resv-001",
            "po-rcv-resv-001",
            "rcv-resv",
            2m);

        var seedStockRequest = Authorized(HttpMethod.Post, "/api/inventory/stock", token);
        seedStockRequest.Content = JsonContent.Create(new UpsertPartStockLevelRequest(part.PartId, bin.BinId, 10m));
        var seedStockResponse = await _supplyarrClient.SendAsync(seedStockRequest);
        seedStockResponse.EnsureSuccessStatusCode();

        var poLineId = purchaseOrder.Lines[0].LineId;
        var createReservationRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/reservations", token);
        createReservationRequest.Content = JsonContent.Create(new CreateStockReservationRequest(
            "rcv-resv-001",
            part.PartId,
            bin.BinId,
            2m,
            "purchase_order_line",
            poLineId,
            "linked reservation"));
        var createReservationResponse = await _supplyarrClient.SendAsync(createReservationRequest);
        createReservationResponse.EnsureSuccessStatusCode();
        var reservation = (await createReservationResponse.Content.ReadFromJsonAsync<StockReservationResponse>())!;
        Assert.Equal("active", reservation.Status);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-resv-001");
        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var postResponse = await _supplyarrClient.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();

        var listReservationsRequest = Authorized(HttpMethod.Get, "/api/v1/inventory/reservations", token);
        var listReservationsResponse = await _supplyarrClient.SendAsync(listReservationsRequest);
        listReservationsResponse.EnsureSuccessStatusCode();
        var reservations = (await listReservationsResponse.Content.ReadFromJsonAsync<List<StockReservationResponse>>())!;
        var released = Assert.Single(reservations, x => x.ReservationId == reservation.ReservationId);
        Assert.Equal("released", released.Status);
        Assert.Equal("released after linked receipt posting", released.ReleaseReason);
    }

    [Fact]
    public async Task Receiving_post_partially_releases_linked_purchase_order_line_reservations()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (part, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-resv-partial-001",
            "po-rcv-resv-partial-001",
            "rcv-resv-partial",
            2m);

        var seedStockRequest = Authorized(HttpMethod.Post, "/api/inventory/stock", token);
        seedStockRequest.Content = JsonContent.Create(new UpsertPartStockLevelRequest(part.PartId, bin.BinId, 10m));
        var seedStockResponse = await _supplyarrClient.SendAsync(seedStockRequest);
        seedStockResponse.EnsureSuccessStatusCode();

        var poLineId = purchaseOrder.Lines[0].LineId;
        var createReservationRequest = Authorized(HttpMethod.Post, "/api/v1/inventory/reservations", token);
        createReservationRequest.Content = JsonContent.Create(new CreateStockReservationRequest(
            "rcv-resv-partial-001",
            part.PartId,
            bin.BinId,
            5m,
            "purchase_order_line",
            poLineId,
            "linked reservation partial"));
        var createReservationResponse = await _supplyarrClient.SendAsync(createReservationRequest);
        createReservationResponse.EnsureSuccessStatusCode();
        var reservation = (await createReservationResponse.Content.ReadFromJsonAsync<StockReservationResponse>())!;
        Assert.Equal("active", reservation.Status);
        Assert.Equal(5m, reservation.QuantityReserved);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-resv-partial-001");
        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var postResponse = await _supplyarrClient.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();

        var listReservationsRequest = Authorized(HttpMethod.Get, "/api/v1/inventory/reservations", token);
        var listReservationsResponse = await _supplyarrClient.SendAsync(listReservationsRequest);
        listReservationsResponse.EnsureSuccessStatusCode();
        var reservations = (await listReservationsResponse.Content.ReadFromJsonAsync<List<StockReservationResponse>>())!;
        var partial = Assert.Single(reservations, x => x.ReservationId == reservation.ReservationId);
        Assert.Equal("active", partial.Status);
        Assert.Equal(3m, partial.QuantityReserved);
        Assert.True(string.IsNullOrWhiteSpace(partial.ReleaseReason));
    }

    [Fact]
    public async Task Receiving_post_checks_trainarr_receiving_qualification()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-trainarr-001",
            "po-rcv-trainarr-001",
            "rcv-trainarr",
            2m);
        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-trainarr-001");

        _trainarrQualificationHandler.NextOutcome = "block";
        _trainarrQualificationHandler.NextReasonCode = "local_suspended";
        _trainarrQualificationHandler.NextMessage = "Receiving qualification is suspended.";

        var blockedPostRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var blockedPostResponse = await _supplyarrClient.SendAsync(blockedPostRequest);
        Assert.Equal(HttpStatusCode.Conflict, blockedPostResponse.StatusCode);

        var check = Assert.Single(_trainarrQualificationHandler.Requests);
        Assert.Equal("/api/v1/integrations/qualification-check", check.Path);
        Assert.Equal("Bearer", check.AuthorizationScheme);
        Assert.Equal("supplyarr-to-trainarr-token", check.AuthorizationParameter);
        Assert.Equal(PlatformSeeder.DemoTenantId, check.TenantId);
        Assert.Equal(PlatformSeeder.DemoAdminUserId, check.StaffarrPersonId);
        Assert.Equal("supplyarr_receiving", check.QualificationKey);
        Assert.Equal("supplyarr_receiving_authorization", check.RulePackKey);
        Assert.Equal("receiving_post", check.Context["action"]);
        Assert.Equal(receipt.ReceivingReceiptId.ToString("D"), check.Context["receivingReceiptId"]);
        Assert.Equal(purchaseOrder.PurchaseOrderId.ToString("D"), check.Context["purchaseOrderId"]);
        Assert.Equal(bin.BinId.ToString("D"), check.Context["inventoryBinId"]);

        var unchanged = await GetPurchaseOrderAsync(token, purchaseOrder.PurchaseOrderId);
        Assert.Equal(0m, unchanged.Lines[0].QuantityReceived);

        _trainarrQualificationHandler.NextOutcome = "allow";
        _trainarrQualificationHandler.NextReasonCode = "local_issued";
        _trainarrQualificationHandler.NextMessage = "Receiving qualification is current.";

        var allowedPostRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var allowedPostResponse = await _supplyarrClient.SendAsync(allowedPostRequest);
        allowedPostResponse.EnsureSuccessStatusCode();
        var posted = (await allowedPostResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("posted", posted.Status);
        Assert.Equal(2, _trainarrQualificationHandler.Requests.Count);
    }

    [Fact]
    public async Task Receiving_exception_create_publishes_discrepancy_facts_to_compliancecore()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-fact-001",
            "po-rcv-fact-001",
            "rcv-fact",
            3m);
        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-fact-001");

        var exceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{receipt.Lines[0].LineId}/exceptions",
            token);
        exceptionRequest.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("short", 1m, "One unit missing from shipment."));
        var exceptionResponse = await _supplyarrClient.SendAsync(exceptionRequest);
        exceptionResponse.EnsureSuccessStatusCode();
        var receivingException = (await exceptionResponse.Content.ReadFromJsonAsync<ReceivingExceptionResponse>())!;

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outboxEvent = await db.IntegrationOutboxEvents.SingleAsync(
            x => x.EventKind == IntegrationOutboxEventKinds.ReceivingExceptionCreated
                && x.RelatedEntityId == receivingException.ReceivingExceptionId);
        var publisher = scope.ServiceProvider.GetRequiredService<ComplianceCoreFactPublisherService>();
        await publisher.TryPublishFromOutboxAsync(outboxEvent);

        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.ReceivingExceptionStatus
                && x.StringValue == ReceivingExceptionStatuses.Open
                && x.SourceEventKind == IntegrationOutboxEventKinds.ReceivingExceptionCreated);
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.ReceivingDiscrepancyRecorded
                && x.BooleanValue == true
                && x.ScopeKey == $"receiving_exception:{receivingException.ReceivingExceptionId:D}".ToLowerInvariant());
    }

    [Fact]
    public async Task Receiving_exception_cancel_and_reopen_updates_status_and_reason_history()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-cancel-reopen-001",
            "po-rcv-cancel-reopen-001",
            "rcv-cancel-reopen",
            2m);
        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-cancel-reopen-001");

        var createRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{receipt.Lines[0].LineId}/exceptions",
            token);
        createRequest.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("short", 1m, "Missing unit from shipment."));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ReceivingExceptionResponse>())!;

        var cancelRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/exceptions/{created.ReceivingExceptionId}/cancel",
            token);
        cancelRequest.Content = JsonContent.Create(new CancelReceivingExceptionRequest("No longer needed."));
        var cancelResponse = await _supplyarrClient.SendAsync(cancelRequest);
        cancelResponse.EnsureSuccessStatusCode();
        var cancelled = (await cancelResponse.Content.ReadFromJsonAsync<ReceivingExceptionResponse>())!;
        Assert.Equal("cancelled", cancelled.Status);
        Assert.Equal("No longer needed.", cancelled.CancellationReason);
        Assert.NotNull(cancelled.CancelledAt);

        var reopenRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/exceptions/{created.ReceivingExceptionId}/reopen",
            token);
        reopenRequest.Content = JsonContent.Create(new ReopenReceivingExceptionRequest("Shipment review confirmed the issue remains."));
        var reopenResponse = await _supplyarrClient.SendAsync(reopenRequest);
        reopenResponse.EnsureSuccessStatusCode();
        var reopened = (await reopenResponse.Content.ReadFromJsonAsync<ReceivingExceptionResponse>())!;
        Assert.Equal("open", reopened.Status);
        Assert.Equal(1, reopened.ReopenCount);
        Assert.Equal("Shipment review confirmed the issue remains.", reopened.LastReopenReason);
        Assert.NotNull(reopened.ReopenedAt);

        var listRequest = Authorized(
            HttpMethod.Get,
            $"/api/receiving/{receipt.ReceivingReceiptId}/exceptions",
            token);
        var listResponse = await _supplyarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var exceptions = (await listResponse.Content.ReadFromJsonAsync<List<ReceivingExceptionResponse>>())!;
        var listed = Assert.Single(exceptions, x => x.ReceivingExceptionId == created.ReceivingExceptionId);
        Assert.Equal("open", listed.Status);
        Assert.Equal(1, listed.ReopenCount);
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
    public async Task Receiving_can_record_missing_packing_slip_exception_type()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-packingslip-001",
            "po-rcv-packingslip-001",
            "rcv-packingslip",
            2m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-packingslip-001");
        var line = receipt.Lines.Single();

        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/exceptions",
            token);
        createExceptionRequest.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("missing_packing_slip", 1m, "Packing slip was not included with shipment."));

        var createExceptionResponse = await _supplyarrClient.SendAsync(createExceptionRequest);
        createExceptionResponse.EnsureSuccessStatusCode();

        var exceptionsRequest = Authorized(
            HttpMethod.Get,
            $"/api/receiving/{receipt.ReceivingReceiptId}/exceptions",
            token);
        var exceptionsResponse = await _supplyarrClient.SendAsync(exceptionsRequest);
        exceptionsResponse.EnsureSuccessStatusCode();
        var exceptions = (await exceptionsResponse.Content.ReadFromJsonAsync<List<ReceivingExceptionResponse>>())!;

        Assert.Contains(
            exceptions,
            x => x.ExceptionType == "missing_packing_slip"
                && x.Quantity == 1m
                && x.Status == ReceivingExceptionStatuses.Open);
    }

    [Fact]
    public async Task Receiving_post_requires_packing_slip_or_missing_packing_slip_exception()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-ps-guard-001",
            "po-rcv-ps-guard-001",
            "rcv-ps-guard",
            2m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-ps-guard-001");

        var clearPackingSlipRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/packing-slip",
            token);
        clearPackingSlipRequest.Content = JsonContent.Create(
            new UpdateReceivingPackingSlipRequest(string.Empty, string.Empty));
        (await _supplyarrClient.SendAsync(clearPackingSlipRequest)).EnsureSuccessStatusCode();

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var postResponse = await _supplyarrClient.SendAsync(postRequest);
        Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
        var postBody = await postResponse.Content.ReadAsStringAsync();
        using var postProblemJson = JsonDocument.Parse(postBody);
        Assert.Equal(
            "receiving.packing_slip.required",
            postProblemJson.RootElement.GetProperty("code").GetString());

        var line = receipt.Lines.Single();
        var createExceptionRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/exceptions",
            token);
        createExceptionRequest.Content = JsonContent.Create(
            new CreateReceivingExceptionRequest("missing_packing_slip", 1m, "Packing slip not available at receiving."));
        (await _supplyarrClient.SendAsync(createExceptionRequest)).EnsureSuccessStatusCode();

        var allowedPostRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var allowedPostResponse = await _supplyarrClient.SendAsync(allowedPostRequest);
        allowedPostResponse.EnsureSuccessStatusCode();
        var posted = (await allowedPostResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("posted", posted.Status);
    }

    [Fact]
    public async Task Receiving_invoice_can_be_attached_after_posting()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-inv-001",
            "po-rcv-inv-001",
            "rcv-invoice",
            2m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-inv-001");

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postRequest)).EnsureSuccessStatusCode();

        var updateInvoiceRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/invoice",
            token);
        updateInvoiceRequest.Content = JsonContent.Create(
            new UpdateReceivingInvoiceRequest("INV-2026-0001", "invoice-2026-0001.pdf"));
        var updateInvoiceResponse = await _supplyarrClient.SendAsync(updateInvoiceRequest);
        updateInvoiceResponse.EnsureSuccessStatusCode();
        var updated = (await updateInvoiceResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;

        Assert.Equal("INV-2026-0001", updated.InvoiceReference);
        Assert.Equal("invoice-2026-0001.pdf", updated.InvoiceFileName);
        Assert.Equal("posted", updated.Status);
    }

    [Fact]
    public async Task Receiving_export_accounting_csv_returns_posted_receipt_rows()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-expacct-001",
            "po-rcv-expacct-001",
            "rcv-expacct",
            2m);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-expacct-001");

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postRequest)).EnsureSuccessStatusCode();

        var exportRequest = Authorized(
            HttpMethod.Get,
            $"/api/receiving/{receipt.ReceivingReceiptId}/export-accounting.csv",
            token);
        var exportResponse = await _supplyarrClient.SendAsync(exportRequest);
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);

        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("receiptKey,receiptStatus,postedAt", csv);
        Assert.Contains(receipt.ReceiptKey, csv);
        Assert.Contains("rcv-expacct", csv);
    }

    [Fact]
    public async Task Receiving_post_requires_serial_lot_tracking_when_part_is_configured()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, bin, purchaseOrder) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-seriallot-001",
            "po-rcv-seriallot-001",
            "rcv-seriallot",
            2m,
            requiresSerialLotTracking: true);

        var receipt = await CreateDraftReceivingReceiptAsync(
            token,
            purchaseOrder.PurchaseOrderId,
            bin.BinId,
            "rcpt-seriallot-001");

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var blockedPostResponse = await _supplyarrClient.SendAsync(postRequest);
        Assert.Equal(HttpStatusCode.BadRequest, blockedPostResponse.StatusCode);
        var blockedBody = await blockedPostResponse.Content.ReadAsStringAsync();
        using var blockedJson = JsonDocument.Parse(blockedBody);
        Assert.Equal(
            "receiving.line.serial_lot.required",
            blockedJson.RootElement.GetProperty("code").GetString());

        var line = receipt.Lines.Single();
        var updateTrackingRequest = Authorized(
            HttpMethod.Put,
            $"/api/receiving/{receipt.ReceivingReceiptId}/lines/{line.LineId}/tracking",
            token);
        updateTrackingRequest.Content = JsonContent.Create(
            new UpdateReceivingReceiptLineTrackingRequest(["SN-001", "SN-002"]));
        (await _supplyarrClient.SendAsync(updateTrackingRequest)).EnsureSuccessStatusCode();

        var allowedPostRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{receipt.ReceivingReceiptId}/post",
            token);
        var allowedPostResponse = await _supplyarrClient.SendAsync(allowedPostRequest);
        allowedPostResponse.EnsureSuccessStatusCode();
        var posted = (await allowedPostResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("posted", posted.Status);
    }

    [Fact]
    public async Task Receiving_close_requires_non_draft_receipt_and_closes_posted_receipt()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, draftBin, draftPo) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-close-001",
            "po-rcv-close-001",
            "rcv-close",
            2m);

        var draftReceipt = await CreateDraftReceivingReceiptAsync(
            token,
            draftPo.PurchaseOrderId,
            draftBin.BinId,
            "rcpt-close-draft-001");

        var closeDraftRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{draftReceipt.ReceivingReceiptId}/close",
            token);
        var closeDraftResponse = await _supplyarrClient.SendAsync(closeDraftRequest);
        Assert.Equal(HttpStatusCode.Conflict, closeDraftResponse.StatusCode);
        var closeDraftBody = await closeDraftResponse.Content.ReadAsStringAsync();
        using var closeDraftJson = JsonDocument.Parse(closeDraftBody);
        Assert.Equal("receiving.not_posted", closeDraftJson.RootElement.GetProperty("code").GetString());

        var (_, postedBin, postedPo) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-close-002",
            "po-rcv-close-002",
            "rcv-close-posted",
            2m);

        var postedReceipt = await CreateDraftReceivingReceiptAsync(
            token,
            postedPo.PurchaseOrderId,
            postedBin.BinId,
            "rcpt-close-posted-001");

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{postedReceipt.ReceivingReceiptId}/post",
            token);
        var postResponse = await _supplyarrClient.SendAsync(postRequest);
        postResponse.EnsureSuccessStatusCode();

        var closePostedRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{postedReceipt.ReceivingReceiptId}/close",
            token);
        var closePostedResponse = await _supplyarrClient.SendAsync(closePostedRequest);
        closePostedResponse.EnsureSuccessStatusCode();
        var closed = (await closePostedResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("closed", closed.Status);
    }

    [Fact]
    public async Task Receiving_reopen_requires_closed_receipt_and_reopens_closed_receipt()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var (_, draftBin, draftPo) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-reopen-001",
            "po-rcv-reopen-001",
            "rcv-reopen",
            2m);

        var draftReceipt = await CreateDraftReceivingReceiptAsync(
            token,
            draftPo.PurchaseOrderId,
            draftBin.BinId,
            "rcpt-reopen-draft-001");

        var reopenDraftRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{draftReceipt.ReceivingReceiptId}/reopen",
            token);
        var reopenDraftResponse = await _supplyarrClient.SendAsync(reopenDraftRequest);
        Assert.Equal(HttpStatusCode.Conflict, reopenDraftResponse.StatusCode);
        var reopenDraftBody = await reopenDraftResponse.Content.ReadAsStringAsync();
        using var reopenDraftJson = JsonDocument.Parse(reopenDraftBody);
        Assert.Equal("receiving.not_closed", reopenDraftJson.RootElement.GetProperty("code").GetString());

        var (_, postedBin, postedPo) = await CreateIssuedPurchaseOrderAsync(
            token,
            "pr-rcv-reopen-002",
            "po-rcv-reopen-002",
            "rcv-reopen-posted",
            2m);
        var postedReceipt = await CreateDraftReceivingReceiptAsync(
            token,
            postedPo.PurchaseOrderId,
            postedBin.BinId,
            "rcpt-reopen-posted-001");

        var postRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{postedReceipt.ReceivingReceiptId}/post",
            token);
        (await _supplyarrClient.SendAsync(postRequest)).EnsureSuccessStatusCode();

        var closeRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{postedReceipt.ReceivingReceiptId}/close",
            token);
        (await _supplyarrClient.SendAsync(closeRequest)).EnsureSuccessStatusCode();

        var reopenClosedRequest = Authorized(
            HttpMethod.Post,
            $"/api/receiving/{postedReceipt.ReceivingReceiptId}/reopen",
            token);
        var reopenClosedResponse = await _supplyarrClient.SendAsync(reopenClosedRequest);
        reopenClosedResponse.EnsureSuccessStatusCode();
        var reopened = (await reopenClosedResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("posted", reopened.Status);
    }

    [Fact]
    public async Task Receiving_over_receive_auto_creates_exception_on_post()
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
        var postResponse = await _supplyarrClient.SendAsync(postWithoutException);
        postResponse.EnsureSuccessStatusCode();
        var posted = (await postResponse.Content.ReadFromJsonAsync<ReceivingReceiptResponse>())!;
        Assert.Equal("overreceived", posted.Status);
        Assert.Contains(posted.Exceptions, ex => ex.ExceptionType == "over" && ex.Quantity == 1m);

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
            $"/api/v1/returns/from-purchase-order-line/{poLineId}",
            token);
        createReturnRequest.Content = JsonContent.Create(
            new CreateSupplierReturnFromPurchaseOrderLineRequest(
                "ret-po-001",
                bin.BinId,
                2m,
                "RMA-PO-1001",
                "Defective batch"));
        var createReturnResponse = await _supplyarrClient.SendAsync(createReturnRequest);
        createReturnResponse.EnsureSuccessStatusCode();
        var created = (await createReturnResponse.Content.ReadFromJsonAsync<SupplierReturnResponse>())!;

        Assert.Equal("draft", created.Status);
        Assert.Equal("purchase_order_line", created.SourceType);
        Assert.Equal("RMA-PO-1001", created.RmaNumber);
        Assert.Equal(purchaseOrder.PurchaseOrderId, created.PurchaseOrderId);
        Assert.Equal(purchaseOrder.PurchaseRequestId, created.PurchaseRequestId);

        var postReturnRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/returns/{created.ReturnId}/post",
            token);
        (await _supplyarrClient.SendAsync(postReturnRequest)).EnsureSuccessStatusCode();

        var posted = await GetSupplierReturnAsync(token, created.ReturnId);
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

        var createReturnRequest = Authorized(HttpMethod.Post, "/api/v1/returns/from-stock", token);
        createReturnRequest.Content = JsonContent.Create(
            new CreateSupplierReturnFromStockRequest(
                "ret-stk-001",
                null,
                purchaseOrderDetail.SupplierId,
                bin.BinId,
                "RMA-STK-2001",
                "Overstock return",
                [new CreateSupplierReturnFromStockLineRequest(part.PartId, 3m, null)]));
        var createReturnResponse = await _supplyarrClient.SendAsync(createReturnRequest);
        createReturnResponse.EnsureSuccessStatusCode();
        var created = (await createReturnResponse.Content.ReadFromJsonAsync<SupplierReturnResponse>())!;

        Assert.Equal("stock", created.SourceType);
        Assert.Equal("RMA-STK-2001", created.RmaNumber);
        Assert.Single(created.Lines);
        Assert.Equal(3m, created.Lines[0].Quantity);

        var postReturnRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/returns/{created.ReturnId}/post",
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

        var createReturnRequest = Authorized(HttpMethod.Post, "/api/v1/returns/from-stock", token);
        createReturnRequest.Content = JsonContent.Create(
            new CreateSupplierReturnFromStockRequest(
                "ret-can-001",
                null,
                purchaseOrderDetail.SupplierId,
                bin.BinId,
                null,
                null,
                [new CreateSupplierReturnFromStockLineRequest(part.PartId, 1m, null)]));
        var createReturnResponse = await _supplyarrClient.SendAsync(createReturnRequest);
        createReturnResponse.EnsureSuccessStatusCode();
        var created = (await createReturnResponse.Content.ReadFromJsonAsync<SupplierReturnResponse>())!;

        var cancelRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/returns/{created.ReturnId}/cancel",
            token);
        cancelRequest.Content = JsonContent.Create(new CancelSupplierReturnRequest("Supplier declined RMA"));
        var cancelResponse = await _supplyarrClient.SendAsync(cancelRequest);
        cancelResponse.EnsureSuccessStatusCode();
        var cancelled = (await cancelResponse.Content.ReadFromJsonAsync<SupplierReturnResponse>())!;

        Assert.Equal("cancelled", cancelled.Status);
        Assert.Equal("Supplier declined RMA", cancelled.CancellationReason);

        var stockAfter = await ListStockAsync(token, part.PartId, bin.BinId);
        Assert.Equal(2m, stockAfter[0].QuantityOnHand);
    }

    [Fact]
    public async Task Pricing_and_lead_time_snapshots_happy_path()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var link = await CreatePartWithSupplierLinkAsync(token, "snap-supplier", "snap-part", "SNAP-V-001");

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
            "Supplier quote May 2026"));
        var createLeadTimeResponse = await _supplyarrClient.SendAsync(createLeadTimeRequest);
        createLeadTimeResponse.EnsureSuccessStatusCode();
        var leadTime = (await createLeadTimeResponse.Content.ReadFromJsonAsync<LeadTimeSnapshotResponse>())!;
        Assert.Equal(10, leadTime.LeadTimeDays);
        Assert.True(leadTime.IsCurrent);

        var listPricingRequest = Authorized(
            HttpMethod.Get,
            $"/api/pricing-snapshots?partSupplierLinkId={link.LinkId}&asOf={Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"))}",
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
            $"/api/pricing-snapshots?partSupplierLinkId={link.LinkId}",
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
        var link = await CreatePartWithSupplierLinkAsync(adminToken, "snap-deny-s", "snap-deny-p", "SNAP-D-001");

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
        var link = await CreatePartWithSupplierLinkAsync(token, "avail-supplier", "avail-part", "AVAIL-V-001");

        var createRequest = Authorized(HttpMethod.Post, "/api/availability-snapshots", token);
        createRequest.Content = JsonContent.Create(new CreateAvailabilitySnapshotRequest(
            "avail-snap-001",
            link.LinkId,
            120m,
            "in_stock",
            null,
            "supplier_feed",
            "Supplier portal May 2026"));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var snapshot = (await createResponse.Content.ReadFromJsonAsync<AvailabilitySnapshotResponse>())!;
        Assert.Equal("avail-snap-001", snapshot.SnapshotKey);
        Assert.True(snapshot.IsCurrent);
        Assert.Equal(120m, snapshot.QuantityAvailable);
        Assert.Equal("in_stock", snapshot.AvailabilityStatus);

        var listRequest = Authorized(
            HttpMethod.Get,
            $"/api/availability-snapshots?partSupplierLinkId={link.LinkId}&asOf={Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"))}",
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
            $"/api/availability-snapshots?partSupplierLinkId={link.LinkId}",
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
        var link = await CreatePartWithSupplierLinkAsync(adminToken, "avail-deny-s", "avail-deny-p", "AVAIL-D-001");

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
            string.Empty,
            _staffarrSiteOrgUnitId));
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
    public async Task Reorder_evaluation_publishes_low_inventory_facts_to_compliancecore()
    {
        var token = await RedeemSupplyArrTokenAsync();
        var keyPrefix = $"reorder-fact-{Guid.NewGuid():N}"[..18];

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"{keyPrefix}-part",
            null,
            "Reorder Fact Part",
            string.Empty,
            "critical",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            $"{keyPrefix}-wh",
            "Reorder Fact Warehouse",
            "warehouse",
            string.Empty,
            _staffarrSiteOrgUnitId));
        var createLocationResponse = await _supplyarrClient.SendAsync(createLocationRequest);
        createLocationResponse.EnsureSuccessStatusCode();
        var location = (await createLocationResponse.Content.ReadFromJsonAsync<InventoryLocationResponse>())!;

        var createBinRequest = Authorized(
            HttpMethod.Post,
            $"/api/inventory/locations/{location.LocationId}/bins",
            token);
        createBinRequest.Content = JsonContent.Create(new CreateInventoryBinRequest($"{keyPrefix}-bin", "A1"));
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

        using var scope = _supplyarrFactory.Services.CreateScope();
        var reorder = scope.ServiceProvider.GetRequiredService<ReorderEvaluationService>();
        var processed = await reorder.ProcessBatchAsync(
            new global::SupplyArr.Api.Contracts.ProcessReorderEvaluationRequest(
                PlatformSeeder.DemoTenantId,
                50,
                false));

        Assert.Contains(processed.Suggestions, x => x.PartId == part.PartId && x.QuantityAvailable == 3m);
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.CriticalInventoryBelowMinimum
                && x.BooleanValue == true
                && x.ScopeKey == $"part:{part.PartId:D}".ToLowerInvariant());
        Assert.Contains(
            _complianceCoreHandler.Facts,
            x => x.FactKey == SupplyArrComplianceCoreFactKeys.InventoryQuantityAvailable
                && x.NumberValue == 3m
                && x.SourceEventKind == "reorder_evaluation.processed");
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
    public async Task Me_allows_users_after_non_supplyarr_launch_context()
    {
        var token = CreateSupplyArrAccessToken(["nexarr"]);
        var request = Authorized(HttpMethod.Get, "/api/me", token);
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var me = (await response.Content.ReadFromJsonAsync<SupplyArrMeResponse>())!;
        Assert.Contains("supplyarr", me.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", me.LaunchableProductKeys);
    }

    private async Task<PartSupplierLinkResponse> CreatePartWithSupplierLinkAsync(
        string token,
        string supplierKey,
        string partKey,
        string supplierPartNumber)
    {
        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            supplierKey,
            null,
            null,
            $"{supplierKey} Supplier",
            $"{supplierKey} Supplier LLC",
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

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

        var linkRequest = Authorized(HttpMethod.Post, $"/api/parts/{part.PartId}/supplier-links", token);
        linkRequest.Content = JsonContent.Create(new CreatePartSupplierLinkRequest(
            null,
            supplier.SupplierId,
            supplierPartNumber,
            true));
        var linkResponse = await _supplyarrClient.SendAsync(linkRequest);
        linkResponse.EnsureSuccessStatusCode();
        return (await linkResponse.Content.ReadFromJsonAsync<PartSupplierLinkResponse>())!;
    }

    private async Task<(PartResponse Part, InventoryBinResponse Bin, PurchaseOrderResponse PurchaseOrder)>
        CreateIssuedPurchaseOrderAsync(
            string token,
            string purchaseRequestKey,
            string purchaseOrderKey,
            string locationKeyPrefix,
            decimal orderQuantity,
            bool requiresSerialLotTracking = false)
    {
        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            $"{locationKeyPrefix}-supplier",
            null,
            null,
            $"{locationKeyPrefix} Supplier",
            $"{locationKeyPrefix} Supplier LLC",
            string.Empty,
            null,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var createSupplierResponse = await _supplyarrClient.SendAsync(createSupplierRequest);
        createSupplierResponse.EnsureSuccessStatusCode();
        var supplier = (await createSupplierResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"{locationKeyPrefix}-part",
            null,
            $"{locationKeyPrefix} Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty,
            requiresSerialLotTracking));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        var createLocationRequest = Authorized(HttpMethod.Post, "/api/inventory/locations", token);
        createLocationRequest.Content = JsonContent.Create(new CreateInventoryLocationRequest(
            $"{locationKeyPrefix}-wh",
            $"{locationKeyPrefix} Warehouse",
            "warehouse",
            "200 Dock St",
            _staffarrSiteOrgUnitId));
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
            supplier.SupplierId,
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
            new CreateReceivingReceiptFromPurchaseOrderRequest(
                receiptKey,
                binId,
                null,
                $"ps-{receiptKey}",
                $"{receiptKey}-packing-slip.pdf"));
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

    private async Task<SupplierReturnResponse> GetSupplierReturnAsync(string token, Guid returnId)
    {
        var getRequest = Authorized(HttpMethod.Get, $"/api/v1/returns/{returnId}", token);
        var getResponse = await _supplyarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        return (await getResponse.Content.ReadFromJsonAsync<SupplierReturnResponse>())!;
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

    private sealed record RecordedTrainArrQualificationCheck(
        string Path,
        string? AuthorizationScheme,
        string? AuthorizationParameter,
        Guid TenantId,
        Guid StaffarrPersonId,
        string QualificationKey,
        string? RulePackKey,
        Dictionary<string, string> Context);

    private sealed record RecordedComplianceCoreProductGateSubject(
        string SubjectType,
        string SubjectReference,
        string? SourceProduct,
        string? DisplayLabel);

    private sealed record RecordedComplianceCoreProductGateRequest(
        string Path,
        string? AuthorizationScheme,
        string? AuthorizationParameter,
        Guid TenantId,
        string ActivityContextKey,
        string? WorkflowKey,
        List<RecordedComplianceCoreProductGateSubject> Subjects,
        Dictionary<string, string> RuleContext);

    private sealed record RecordedComplianceCoreFact(
        string FactKey,
        string ScopeKey,
        string? StringValue,
        bool? BooleanValue,
        decimal? NumberValue,
        string SourceEntityType,
        Guid? SourceEntityId,
        string SourceEventKind);

    private sealed class RecordingTrainArrQualificationCheckHandler : HttpMessageHandler
    {
        public List<RecordedTrainArrQualificationCheck> Requests { get; } = [];

        public string NextOutcome { get; set; } = "allow";

        public string NextReasonCode { get; set; } = "local_issued";

        public string NextMessage { get; set; } = "Qualification is current.";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? "{}"
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var context = new Dictionary<string, string>();
            if (root.TryGetProperty("context", out var contextElement)
                && contextElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in contextElement.EnumerateObject())
                {
                    context[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            Requests.Add(new RecordedTrainArrQualificationCheck(
                request.RequestUri?.AbsolutePath ?? string.Empty,
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter,
                root.GetProperty("tenantId").GetGuid(),
                root.GetProperty("staffarrPersonId").GetGuid(),
                root.GetProperty("qualificationKey").GetString() ?? string.Empty,
                root.TryGetProperty("rulePackKey", out var rulePackKey) && rulePackKey.ValueKind != JsonValueKind.Null
                    ? rulePackKey.GetString()
                    : null,
                context));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    checkId = Guid.NewGuid(),
                    staffarrPersonId = root.GetProperty("staffarrPersonId").GetGuid(),
                    qualificationKey = root.GetProperty("qualificationKey").GetString(),
                    outcome = NextOutcome,
                    reasonCode = NextReasonCode,
                    message = NextMessage
                })
            };
        }
    }

    private sealed class RecordingStaffArrSiteLookupHandler(Guid siteOrgUnitId) : HttpMessageHandler
    {
        public List<string> Paths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            Paths.Add(path);

            if (!path.Contains("/api/v1/integrations/sites", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var response = new StaffArrSiteLookupResponse(
                siteOrgUnitId,
                "Central Parts Site",
                null,
                null,
                "active",
                DateTimeOffset.UnixEpoch);

            if (path.EndsWith("/sites", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[] { response })
                });
            }

            if (path.EndsWith($"/{siteOrgUnitId:D}", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(response)
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }

    private sealed class RecordingComplianceCoreHandler : HttpMessageHandler
    {
        public List<RecordedComplianceCoreProductGateRequest> GateRequests { get; } = [];

        public List<RecordedComplianceCoreFact> Facts { get; } = [];

        public int FactIngestCount { get; private set; }

        public string NextOutcome { get; set; } = "allow";

        public string NextReasonCode { get; set; } = "supplier_compliance_clear";

        public string NextMessage { get; set; } = "Supplier satisfies Compliance Core requirements.";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            var body = request.Content is null
                ? "{}"
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (path.EndsWith("/api/integrations/product-facts/ingest", StringComparison.OrdinalIgnoreCase))
            {
                FactIngestCount++;
                foreach (var factElement in root.GetProperty("facts").EnumerateArray())
                {
                    Facts.Add(new RecordedComplianceCoreFact(
                        factElement.GetProperty("factKey").GetString() ?? string.Empty,
                        factElement.GetProperty("scopeKey").GetString() ?? string.Empty,
                        factElement.TryGetProperty("stringValue", out var stringValueElement)
                            && stringValueElement.ValueKind != JsonValueKind.Null
                                ? stringValueElement.GetString()
                                : null,
                        factElement.TryGetProperty("booleanValue", out var booleanValueElement)
                            && booleanValueElement.ValueKind != JsonValueKind.Null
                                ? booleanValueElement.GetBoolean()
                                : null,
                        factElement.TryGetProperty("numberValue", out var numberValueElement)
                            && numberValueElement.ValueKind != JsonValueKind.Null
                                ? numberValueElement.GetDecimal()
                                : null,
                        factElement.GetProperty("sourceEntityType").GetString() ?? string.Empty,
                        factElement.TryGetProperty("sourceEntityId", out var sourceEntityIdElement)
                            && sourceEntityIdElement.ValueKind != JsonValueKind.Null
                                ? sourceEntityIdElement.GetGuid()
                                : null,
                        factElement.GetProperty("sourceEventKind").GetString() ?? string.Empty));
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new
                    {
                        tenantId = root.GetProperty("tenantId").GetGuid(),
                        publicationId = root.GetProperty("publicationId").GetGuid(),
                        acceptedCount = root.GetProperty("facts").GetArrayLength(),
                        skippedDuplicateCount = 0
                    })
                };
            }

            var subjects = new List<RecordedComplianceCoreProductGateSubject>();
            if (root.TryGetProperty("subjectReferences", out var subjectReferencesElement)
                && subjectReferencesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var subjectElement in subjectReferencesElement.EnumerateArray())
                {
                    subjects.Add(new RecordedComplianceCoreProductGateSubject(
                        subjectElement.GetProperty("subjectType").GetString() ?? string.Empty,
                        subjectElement.GetProperty("subjectReference").GetString() ?? string.Empty,
                        subjectElement.TryGetProperty("sourceProduct", out var sourceProductElement)
                            && sourceProductElement.ValueKind != JsonValueKind.Null
                                ? sourceProductElement.GetString()
                                : null,
                        subjectElement.TryGetProperty("displayLabel", out var displayLabelElement)
                            && displayLabelElement.ValueKind != JsonValueKind.Null
                                ? displayLabelElement.GetString()
                                : null));
                }
            }

            var ruleContext = new Dictionary<string, string>();
            if (root.TryGetProperty("ruleContext", out var ruleContextElement)
                && ruleContextElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in ruleContextElement.EnumerateObject())
                {
                    ruleContext[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            GateRequests.Add(new RecordedComplianceCoreProductGateRequest(
                path,
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter,
                root.GetProperty("tenantId").GetGuid(),
                root.GetProperty("activityContextKey").GetString() ?? string.Empty,
                root.TryGetProperty("workflowKey", out var workflowKeyElement)
                    && workflowKeyElement.ValueKind != JsonValueKind.Null
                        ? workflowKeyElement.GetString()
                        : null,
                subjects,
                ruleContext));

            var traceId = Guid.NewGuid();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    traceId,
                    tenantId = root.GetProperty("tenantId").GetGuid(),
                    workflowKey = root.TryGetProperty("workflowKey", out var responseWorkflowKey)
                        && responseWorkflowKey.ValueKind != JsonValueKind.Null
                            ? responseWorkflowKey.GetString()
                            : "can_use_supplier",
                    actionKey = "can_use_supplier",
                    activityContextKey = root.GetProperty("activityContextKey").GetString(),
                    subjectReferences = Array.Empty<object>(),
                    checkResultId = traceId,
                    ruleEvaluationRunId = (Guid?)null,
                    outcome = NextOutcome,
                    reasonCode = NextReasonCode,
                    message = NextMessage,
                    appliedRuleVersions = Array.Empty<object>(),
                    citationReferences = Array.Empty<object>(),
                    missingFacts = Array.Empty<string>(),
                    staleFacts = Array.Empty<object>(),
                    evidenceRequirements = Array.Empty<object>(),
                    remediationHints = Array.Empty<object>(),
                    appliedWaiverId = (Guid?)null,
                    appliedWaiverKey = (string?)null,
                    auditExportPath = (string?)null,
                    evaluatedAt = DateTimeOffset.UtcNow
                })
            };
        }
    }
}

