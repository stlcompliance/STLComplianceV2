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
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

using AuthTokenResponse = NexArr.Api.Contracts.AuthTokenResponse;
using CreateHandoffRequest = NexArr.Api.Contracts.CreateHandoffRequest;
using CreatePartCatalogRequest = SupplyArr.Api.Contracts.CreatePartCatalogRequest;
using CreatePartRequest = SupplyArr.Api.Contracts.CreatePartRequest;
using CreatePartyContactRequest = SupplyArr.Api.Contracts.CreatePartyContactRequest;
using CreateRfqLineRequest = SupplyArr.Api.Contracts.CreateRfqLineRequest;
using CreateRfqRequest = SupplyArr.Api.Contracts.CreateRfqRequest;
using InviteRfqVendorsRequest = SupplyArr.Api.Contracts.InviteRfqVendorsRequest;
using CreatePurchaseOrderFromPurchaseRequestRequest = SupplyArr.Api.Contracts.CreatePurchaseOrderFromPurchaseRequestRequest;
using CreatePurchaseRequestFromRfqRequest = SupplyArr.Api.Contracts.CreatePurchaseRequestFromRfqRequest;
using CreateTypedExternalPartyRequest = SupplyArr.Api.Contracts.CreateTypedExternalPartyRequest;
using CreateVendorQuoteRequest = SupplyArr.Api.Contracts.CreateVendorQuoteRequest;
using IngestVendorEmailInboxRequest = SupplyArr.Api.Contracts.IngestVendorEmailInboxRequest;
using IngestVendorEmailInboxResponse = SupplyArr.Api.Contracts.IngestVendorEmailInboxResponse;
using PurchaseOrderResponse = SupplyArr.Api.Contracts.PurchaseOrderResponse;
using PurchaseRequestResponse = SupplyArr.Api.Contracts.PurchaseRequestResponse;
using RfqResponse = SupplyArr.Api.Contracts.RfqResponse;
using VendorQuoteResponse = SupplyArr.Api.Contracts.VendorQuoteResponse;
using ServiceClientResponse = NexArr.Api.Contracts.ServiceClientResponse;
using ServiceTokenIssueResponse = NexArr.Api.Contracts.ServiceTokenIssueResponse;
using HandoffCreatedResponse = NexArr.Api.Contracts.HandoffCreatedResponse;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using UpsertVendorQuoteLineRequest = SupplyArr.Api.Contracts.UpsertVendorQuoteLineRequest;
using VendorEmailInboxListResponse = SupplyArr.Api.Contracts.VendorEmailInboxListResponse;
using ExternalPartyResponse = SupplyArr.Api.Contracts.ExternalPartyResponse;
using PartCatalogResponse = SupplyArr.Api.Contracts.PartCatalogResponse;
using PartResponse = SupplyArr.Api.Contracts.PartResponse;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrVendorEmailInboxTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"VendorEmailNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"VendorEmailSupplyArr-{Guid.NewGuid():N}";

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
        var handoffToken = await IssueHandoffServiceTokenAsync(adminToken);
        var handoffCode = await CreateHandoffAsync(adminToken);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", handoffToken);
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
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Vendor_email_inbox_auto_links_quotes_and_order_confirmations()
    {
        var (vendor, part) = await SeedVendorAndPartAsync();

        var rfq = await CreateRfqAsync(part.PartId, vendor.PartyId, "RFQ-EMAIL-001");
        var vendorQuote = await CreateAndSubmitVendorQuoteAsync(rfq.RfqId, vendor.PartyId, rfq.Lines[0].LineId);
        await SelectVendorQuoteAsync(rfq.RfqId, vendorQuote.VendorQuoteId);
        var awardedRfq = await GetRfqAsync(rfq.RfqId);
        Assert.Equal("awarded", awardedRfq.Status);
        Assert.Equal(vendor.PartyId, awardedRfq.AwardedVendorPartyId);

        var quoteEmail = await IngestVendorEmailAsync(new IngestVendorEmailInboxRequest(
            "mail-001",
            "quote_received",
            "vendor@example.com",
            "Vendor Supply",
            $"RFQ {rfq.RfqKey} quote attached",
            "Please see the attached quote for your request.",
            null));

        Assert.False(quoteEmail.WasDuplicate);
        Assert.Equal("matched", quoteEmail.Message.MatchStatus);
        Assert.Equal("rfq", quoteEmail.Message.LinkedReferenceType);
        Assert.Equal(rfq.RfqKey, quoteEmail.Message.LinkedReferenceKey);
        Assert.Equal(vendor.PartyKey, quoteEmail.Message.VendorPartyKey);

        var purchaseRequest = await CreatePurchaseRequestFromRfqAsync(rfq.RfqId, "PR-EMAIL-001");
        await SubmitPurchaseRequestAsync(purchaseRequest.PurchaseRequestId);
        await ApprovePurchaseRequestAsync(purchaseRequest.PurchaseRequestId);
        var purchaseOrder = await CreatePurchaseOrderAsync(purchaseRequest.PurchaseRequestId, "PO-EMAIL-001");
        var orderEmail = await IngestVendorEmailAsync(new IngestVendorEmailInboxRequest(
            "mail-002",
            "order_confirmation_received",
            "vendor@example.com",
            "Vendor Supply",
            $"Order confirmation for {purchaseOrder.OrderKey}",
            "Confirmed and scheduled.",
            null));

        Assert.False(orderEmail.WasDuplicate);
        Assert.Equal("matched", orderEmail.Message.MatchStatus);
        Assert.Equal("purchase_order", orderEmail.Message.LinkedReferenceType);
        Assert.Equal(purchaseOrder.OrderKey, orderEmail.Message.LinkedReferenceKey);

        var listResponse = await _supplyarrClient.SendAsync(Authorized(HttpMethod.Get, "/api/v1/vendor-email-inbox?limit=10", _userToken));
        listResponse.EnsureSuccessStatusCode();
        var inbox = (await listResponse.Content.ReadFromJsonAsync<VendorEmailInboxListResponse>())!;
        Assert.Equal(2, inbox.Items.Count);
        Assert.Equal("mail-002", inbox.Items[0].MessageKey);
        Assert.Equal("mail-001", inbox.Items[1].MessageKey);
    }

    private async Task<(ExternalPartyResponse Vendor, PartResponse Part)> SeedVendorAndPartAsync()
    {
        var vendorRequest = Authorized(HttpMethod.Post, "/api/vendors", _userToken);
        vendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"vendor-email-{Guid.NewGuid():N}"[..20],
            "Vendor Email Supply",
            "Vendor Email Supply LLC",
            null,
            "Vendor for email inbox integration."));
        var vendorResponse = await _supplyarrClient.SendAsync(vendorRequest);
        vendorResponse.EnsureSuccessStatusCode();
        var vendor = (await vendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var contactRequest = Authorized(HttpMethod.Post, $"/api/vendors/{vendor.PartyId}/contacts", _userToken);
        contactRequest.Content = JsonContent.Create(new CreatePartyContactRequest(
            "Vendor Supply",
            "vendor@example.com",
            "555-0100",
            "Sales",
            true));
        var contactResponse = await _supplyarrClient.SendAsync(contactRequest);
        contactResponse.EnsureSuccessStatusCode();

        var catalogRequest = Authorized(HttpMethod.Post, "/api/catalogs", _userToken);
        catalogRequest.Content = JsonContent.Create(new CreatePartCatalogRequest(
            $"catalog-{Guid.NewGuid():N}"[..20],
            "Email Inbox Catalog",
            "Test catalog for vendor email inbox"));
        var catalogResponse = await _supplyarrClient.SendAsync(catalogRequest);
        catalogResponse.EnsureSuccessStatusCode();
        var catalog = (await catalogResponse.Content.ReadFromJsonAsync<PartCatalogResponse>())!;

        var partRequest = Authorized(HttpMethod.Post, "/api/parts", _userToken);
        partRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"part-{Guid.NewGuid():N}"[..20],
            catalog.CatalogId,
            "Email Inbox Part",
            "Test part for vendor email inbox",
            "general",
            "each",
            "Vendor",
            "V-100"));
        var partResponse = await _supplyarrClient.SendAsync(partRequest);
        partResponse.EnsureSuccessStatusCode();
        var part = (await partResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        return (vendor, part);
    }

    private async Task<RfqResponse> CreateRfqAsync(Guid partId, Guid vendorPartyId, string rfqKey)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/rfqs", _userToken);
        createRequest.Content = JsonContent.Create(new CreateRfqRequest(
            rfqKey,
            "Vendor email inbox RFQ",
            "RFQ used for vendor email inbox tests",
            [new CreateRfqLineRequest(partId, 10m, "Inbox line")]));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var rfq = (await createResponse.Content.ReadFromJsonAsync<RfqResponse>())!;

        var submitResponse = await _supplyarrClient.SendAsync(Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/submit", _userToken));
        submitResponse.EnsureSuccessStatusCode();

        var inviteRequest = Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/invite-vendors", _userToken);
        inviteRequest.Content = JsonContent.Create(new InviteRfqVendorsRequest([vendorPartyId]));
        var inviteResponse = await _supplyarrClient.SendAsync(inviteRequest);
        inviteResponse.EnsureSuccessStatusCode();

        return (await inviteResponse.Content.ReadFromJsonAsync<RfqResponse>())!;
    }

    private async Task<VendorQuoteResponse> CreateAndSubmitVendorQuoteAsync(Guid rfqId, Guid vendorPartyId, Guid rfqLineId)
    {
        var createQuote = Authorized(HttpMethod.Post, $"/api/rfqs/{rfqId}/quotes", _userToken);
        createQuote.Content = JsonContent.Create(new CreateVendorQuoteRequest(
            vendorPartyId,
            $"QUOTE-{Guid.NewGuid():N}"[..12],
            "USD",
            "Vendor inbox quote"));
        var createResponse = await _supplyarrClient.SendAsync(createQuote);
        createResponse.EnsureSuccessStatusCode();
        var quote = (await createResponse.Content.ReadFromJsonAsync<VendorQuoteResponse>())!;

        var lineRequest = Authorized(HttpMethod.Put, $"/api/rfqs/{rfqId}/quotes/{quote.VendorQuoteId}/lines", _userToken);
        lineRequest.Content = JsonContent.Create(new UpsertVendorQuoteLineRequest(
            rfqLineId,
            7.50m,
            10m,
            4,
            "Vendor inbox quote line"));
        var lineResponse = await _supplyarrClient.SendAsync(lineRequest);
        lineResponse.EnsureSuccessStatusCode();

        var submitResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/rfqs/{rfqId}/quotes/{quote.VendorQuoteId}/submit", _userToken));
        submitResponse.EnsureSuccessStatusCode();
        return (await submitResponse.Content.ReadFromJsonAsync<VendorQuoteResponse>())!;
    }

    private async Task SelectVendorQuoteAsync(Guid rfqId, Guid vendorQuoteId)
    {
        var request = Authorized(HttpMethod.Post, $"/api/rfqs/{rfqId}/select-quote", _userToken);
        request.Content = JsonContent.Create(new SelectVendorQuoteRequest(vendorQuoteId));
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<RfqResponse> GetRfqAsync(Guid rfqId)
    {
        var response = await _supplyarrClient.SendAsync(Authorized(HttpMethod.Get, $"/api/rfqs/{rfqId}", _userToken));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RfqResponse>())!;
    }

    private async Task<PurchaseRequestResponse> CreatePurchaseRequestFromRfqAsync(Guid rfqId, string requestKey)
    {
        var request = Authorized(HttpMethod.Post, $"/api/rfqs/{rfqId}/create-purchase-request", _userToken);
        request.Content = JsonContent.Create(new CreatePurchaseRequestFromRfqRequest(requestKey, "Vendor inbox PR", "Created from inbox RFQ"));
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;
    }

    private async Task<PurchaseOrderResponse> CreatePurchaseOrderAsync(Guid purchaseRequestId, string orderKey)
    {
        var request = Authorized(HttpMethod.Post, $"/api/purchase-orders/from-purchase-request/{purchaseRequestId}", _userToken);
        request.Content = JsonContent.Create(new CreatePurchaseOrderFromPurchaseRequestRequest(orderKey, "Vendor inbox PO", "Created from inbox PR"));
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PurchaseOrderResponse>())!;
    }

    private async Task ApprovePurchaseRequestAsync(Guid purchaseRequestId)
    {
        var request = Authorized(HttpMethod.Post, $"/api/purchase-requests/{purchaseRequestId}/approve", _userToken);
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task SubmitPurchaseRequestAsync(Guid purchaseRequestId)
    {
        var request = Authorized(HttpMethod.Post, $"/api/purchase-requests/{purchaseRequestId}/submit", _userToken);
        var response = await _supplyarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<IngestVendorEmailInboxResponse> IngestVendorEmailAsync(IngestVendorEmailInboxRequest request)
    {
        var messageRequest = Authorized(HttpMethod.Post, "/api/v1/vendor-email-inbox", _userToken);
        messageRequest.Content = JsonContent.Create(request);
        var response = await _supplyarrClient.SendAsync(messageRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IngestVendorEmailInboxResponse>())!;
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
        return (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!.AccessToken;
    }

    private async Task<string> IssueHandoffServiceTokenAsync(string adminToken)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"supplyarr-vendor-email-{Guid.NewGuid():N}",
            "supplyarr vendor email inbox test",
            "supplyarr",
            ["supplyarr"]));
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
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!.AccessToken;
    }

    private async Task<string> CreateHandoffAsync(string adminToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", adminToken);
        request.Content = JsonContent.Create(new CreateHandoffRequest("supplyarr", "http://localhost:5179/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!.HandoffCode;
    }

    private async Task<string> RedeemHandoffAsync(string handoffCode)
    {
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemHandoffRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        return (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!.AccessToken;
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
