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
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using CreateSupplierRequest = SupplyArr.Api.Contracts.CreateSupplierRequest;
using SupplierResponse = SupplyArr.Api.Contracts.SupplierResponse;
using CreatePartCatalogRequest = SupplyArr.Api.Contracts.CreatePartCatalogRequest;
using PartCatalogResponse = SupplyArr.Api.Contracts.PartCatalogResponse;
using CreatePartRequest = SupplyArr.Api.Contracts.CreatePartRequest;
using PartResponse = SupplyArr.Api.Contracts.PartResponse;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrRfqTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RfqNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"RfqSupplyArr-{Guid.NewGuid():N}";

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
    public async Task Rfq_end_to_end_compare_award_and_create_pr()
    {
        var (supplierUnitA, supplierUnitB, part) = await SeedSupplierUnitsAndPartAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/rfqs", _userToken);
        createRequest.Content = JsonContent.Create(new CreateRfqRequest(
            $"RFQ-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
            "Brake pads RFQ",
            "Fleet replenishment",
            [new CreateRfqLineRequest(part.PartId, 10m, "line")]));

        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var rfq = (await createResponse.Content.ReadFromJsonAsync<RfqResponse>())!;

        var submitResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/submit", _userToken));
        submitResponse.EnsureSuccessStatusCode();

        var inviteRequest = Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/invite-suppliers", _userToken);
        inviteRequest.Content = JsonContent.Create(new InviteRfqSuppliersRequest([supplierUnitA.SupplierId, supplierUnitB.SupplierId]));
        var inviteResponse = await _supplyarrClient.SendAsync(inviteRequest);
        inviteResponse.EnsureSuccessStatusCode();

        var quoteA = await CreateAndSubmitQuoteAsync(rfq.RfqId, supplierUnitA.SupplierId, "QUOTE-A", rfq.Lines[0].LineId, 12.5m, 5);
        var quoteB = await CreateAndSubmitQuoteAsync(rfq.RfqId, supplierUnitB.SupplierId, "QUOTE-B", rfq.Lines[0].LineId, 11.0m, 10);

        var compareResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/rfqs/{rfq.RfqId}/quote-comparison", _userToken));
        compareResponse.EnsureSuccessStatusCode();
        var comparison = (await compareResponse.Content.ReadFromJsonAsync<RfqQuoteComparisonResponse>())!;
        Assert.Equal(2, comparison.QuoteSummaries.Count);
        var lineMetric = comparison.Lines.Single().Quotes;
        Assert.Contains(lineMetric, x => x.SupplierQuoteId == quoteB.SupplierQuoteId && x.IsLowestPrice);
        Assert.Contains(lineMetric, x => x.SupplierQuoteId == quoteA.SupplierQuoteId && x.IsFastestLeadTime);

        var selectRequest = Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/select-quote", _userToken);
        selectRequest.Content = JsonContent.Create(new SelectSupplierQuoteRequest(quoteB.SupplierQuoteId));
        var selectResponse = await _supplyarrClient.SendAsync(selectRequest);
        selectResponse.EnsureSuccessStatusCode();
        var awarded = (await selectResponse.Content.ReadFromJsonAsync<RfqResponse>())!;
        Assert.Equal(RfqStatuses.Awarded, awarded.Status);
        Assert.Equal(quoteB.SupplierQuoteId, awarded.SelectedSupplierQuoteId);

        var prRequest = Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/create-purchase-request", _userToken);
        prRequest.Content = JsonContent.Create(new CreatePurchaseRequestFromRfqRequest($"PR-{Guid.NewGuid():N}"[..12], null, null));
        var prResponse = await _supplyarrClient.SendAsync(prRequest);
        prResponse.EnsureSuccessStatusCode();
        var prBody = (await prResponse.Content.ReadFromJsonAsync<CreatePurchaseRequestFromRfqResponse>())!;
        Assert.Equal(supplierUnitB.SupplierId, prBody.PurchaseRequest.SupplierId);
        Assert.Single(prBody.PurchaseRequest.Lines);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outboxCount = await db.IntegrationOutboxEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityType == "rfq");
        Assert.True(outboxCount >= 3);
    }

    [Fact]
    public async Task Rfq_supplier_portal_can_create_update_and_submit_quote()
    {
        var (supplierUnitA, _, part) = await SeedSupplierUnitsAndPartAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/rfqs", _userToken);
        createRequest.Content = JsonContent.Create(new CreateRfqRequest(
            $"RFQ-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
            "Portal RFQ",
            "Supplier portal flow",
            [new CreateRfqLineRequest(part.PartId, 5m, "line")]));

        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var rfq = (await createResponse.Content.ReadFromJsonAsync<RfqResponse>())!;

        (await _supplyarrClient.SendAsync(Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/submit", _userToken)))
            .EnsureSuccessStatusCode();

        var inviteRequest = Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/invite-suppliers", _userToken);
        inviteRequest.Content = JsonContent.Create(new InviteRfqSuppliersRequest([supplierUnitA.SupplierId]));
        var inviteResponse = await _supplyarrClient.SendAsync(inviteRequest);
        inviteResponse.EnsureSuccessStatusCode();
        var invited = (await inviteResponse.Content.ReadFromJsonAsync<RfqResponse>())!;
        var invitation = invited.Invitations.Single();
        Assert.False(string.IsNullOrWhiteSpace(invitation.PortalAccessCode));
        Assert.Contains("/supplier-quote-portal", invitation.PortalUrl);

        var portalResponse = await _supplyarrClient.GetAsync(
            $"/api/v1/supplier-portal/rfqs/{rfq.RfqId}?accessCode={Uri.EscapeDataString(invitation.PortalAccessCode)}");
        portalResponse.EnsureSuccessStatusCode();
        var portal = (await portalResponse.Content.ReadFromJsonAsync<SupplierPortalRfqResponse>())!;
        Assert.Equal(supplierUnitA.SupplierId, portal.SupplierId);
        Assert.Null(portal.SupplierQuoteId);
        Assert.Single(portal.Lines);

        var createPortalQuote = await _supplyarrClient.PostAsJsonAsync(
            $"/api/v1/supplier-portal/rfqs/{rfq.RfqId}/quotes?accessCode={Uri.EscapeDataString(invitation.PortalAccessCode)}",
            new SupplierPortalCreateQuoteRequest("PORTAL-QUOTE-1", "USD", "Portal response"));
        createPortalQuote.EnsureSuccessStatusCode();
        var createdQuote = (await createPortalQuote.Content.ReadFromJsonAsync<SupplierQuoteResponse>())!;
        Assert.Equal("draft", createdQuote.Status);

        var upsertLine = await _supplyarrClient.PutAsJsonAsync(
            $"/api/v1/supplier-portal/rfqs/{rfq.RfqId}/quotes/{createdQuote.SupplierQuoteId}/lines?accessCode={Uri.EscapeDataString(invitation.PortalAccessCode)}",
            new UpsertSupplierQuoteLineRequest(
                portal.Lines[0].RfqLineId,
                7.5m,
                5m,
                4,
                "Supplier portal line note"));
        upsertLine.EnsureSuccessStatusCode();

        var submitQuote = await _supplyarrClient.PostAsync(
            $"/api/v1/supplier-portal/rfqs/{rfq.RfqId}/quotes/{createdQuote.SupplierQuoteId}/submit?accessCode={Uri.EscapeDataString(invitation.PortalAccessCode)}",
            null);
        submitQuote.EnsureSuccessStatusCode();
        var submitted = (await submitQuote.Content.ReadFromJsonAsync<SupplierQuoteResponse>())!;
        Assert.Equal("submitted", submitted.Status);
        Assert.Equal(37.5m, submitted.TotalAmount);
        Assert.Equal(4, submitted.LeadTimeDays);
    }

    [Fact]
    public async Task Rfq_create_rejects_duplicate_key()
    {
        var (_, _, part) = await SeedSupplierUnitsAndPartAsync();
        var key = $"RFQ-DUP-{Guid.NewGuid():N}"[..14];

        var first = Authorized(HttpMethod.Post, "/api/rfqs", _userToken);
        first.Content = JsonContent.Create(new CreateRfqRequest(key, "First", "", [new CreateRfqLineRequest(part.PartId, 1m, "")]));
        (await _supplyarrClient.SendAsync(first)).EnsureSuccessStatusCode();

        var second = Authorized(HttpMethod.Post, "/api/rfqs", _userToken);
        second.Content = JsonContent.Create(new CreateRfqRequest(key, "Second", "", [new CreateRfqLineRequest(part.PartId, 1m, "")]));
        var secondResponse = await _supplyarrClient.SendAsync(second);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Rfq_list_requires_authentication()
    {
        var response = await _supplyarrClient.GetAsync("/api/rfqs");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Rfq_invite_rejects_supplier_sub_unit_without_parts_coverage()
    {
        var (partsSupplier, maintenanceOnlySubUnit, part) = await SeedSupplierHierarchyAndPartAsync(["maintenance"]);

        var createRequest = Authorized(HttpMethod.Post, "/api/rfqs", _userToken);
        createRequest.Content = JsonContent.Create(new CreateRfqRequest(
            $"RFQ-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
            "Coverage-gated RFQ",
            "Sub-unit service coverage",
            [new CreateRfqLineRequest(part.PartId, 2m, "line")]));

        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var rfq = (await createResponse.Content.ReadFromJsonAsync<RfqResponse>())!;

        (await _supplyarrClient.SendAsync(Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/submit", _userToken)))
            .EnsureSuccessStatusCode();

        var inviteRequest = Authorized(HttpMethod.Post, $"/api/rfqs/{rfq.RfqId}/invite-suppliers", _userToken);
        inviteRequest.Content = JsonContent.Create(new InviteRfqSuppliersRequest([partsSupplier.SupplierId, maintenanceOnlySubUnit.SupplierId]));
        var inviteResponse = await _supplyarrClient.SendAsync(inviteRequest);

        Assert.Equal(HttpStatusCode.Conflict, inviteResponse.StatusCode);
        var payload = await inviteResponse.Content.ReadAsStringAsync();
        Assert.Contains("include products or parts", payload, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<SupplierQuoteResponse> CreateAndSubmitQuoteAsync(
        Guid rfqId,
        Guid supplierId,
        string quoteKey,
        Guid rfqLineId,
        decimal unitPrice,
        int leadDays)
    {
        var createQuote = Authorized(HttpMethod.Post, $"/api/rfqs/{rfqId}/quotes", _userToken);
        createQuote.Content = JsonContent.Create(new CreateSupplierQuoteRequest(
            supplierId,
            quoteKey,
            "USD",
            string.Empty));
        var createQuoteResponse = await _supplyarrClient.SendAsync(createQuote);
        createQuoteResponse.EnsureSuccessStatusCode();
        var quote = (await createQuoteResponse.Content.ReadFromJsonAsync<SupplierQuoteResponse>())!;

        var lineRequest = Authorized(HttpMethod.Put, $"/api/rfqs/{rfqId}/quotes/{quote.SupplierQuoteId}/lines", _userToken);
        lineRequest.Content = JsonContent.Create(new UpsertSupplierQuoteLineRequest(
            rfqLineId,
            unitPrice,
            10m,
            leadDays,
            string.Empty));
        (await _supplyarrClient.SendAsync(lineRequest)).EnsureSuccessStatusCode();

        var submitQuote = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/rfqs/{rfqId}/quotes/{quote.SupplierQuoteId}/submit", _userToken));
        submitQuote.EnsureSuccessStatusCode();
        return (await submitQuote.Content.ReadFromJsonAsync<SupplierQuoteResponse>())!;
    }

    private async Task<(SupplierResponse SupplierUnitA, SupplierResponse SupplierUnitB, PartResponse Part)> SeedSupplierUnitsAndPartAsync()
    {
        var (_, supplierUnitA, part) = await SeedSupplierHierarchyAndPartAsync(["parts", "products"]);
        var (_, supplierUnitB, _) = await SeedSupplierHierarchyAndPartAsync(["parts"]);

        return (supplierUnitA, supplierUnitB, part);
    }

    private async Task<(SupplierResponse ParentSupplier, SupplierResponse SupplierSubUnit, PartResponse Part)> SeedSupplierHierarchyAndPartAsync(IReadOnlyList<string> subUnitServiceTypes)
    {
        var createParent = Authorized(HttpMethod.Post, "/api/suppliers", _userToken);
        createParent.Content = JsonContent.Create(new CreateSupplierRequest(
            $"rfq-parent-{Guid.NewGuid():N}"[..16],
            null,
            "identity",
            "RFQ Supplier Parent",
            string.Empty,
            null,
            string.Empty,
            ["parts", "products"],
            null,
            null,
            null,
            null,
            null,
            null));
        var parentResponse = await _supplyarrClient.SendAsync(createParent);
        parentResponse.EnsureSuccessStatusCode();
        var parentSupplier = (await parentResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createSubUnit = Authorized(HttpMethod.Post, "/api/suppliers", _userToken);
        createSubUnit.Content = JsonContent.Create(new CreateSupplierRequest(
            $"rfq-sub-{Guid.NewGuid():N}"[..16],
            parentSupplier.SupplierId,
            "sub_unit",
            "RFQ Supplier Branch",
            string.Empty,
            null,
            string.Empty,
            subUnitServiceTypes,
            "500 Branch Rd",
            null,
            "Omaha",
            "NE",
            "68102",
            "US"));
        var subUnitResponse = await _supplyarrClient.SendAsync(createSubUnit);
        subUnitResponse.EnsureSuccessStatusCode();
        var supplierSubUnit = (await subUnitResponse.Content.ReadFromJsonAsync<SupplierResponse>())!;

        var createCatalog = Authorized(HttpMethod.Post, "/api/catalogs", _userToken);
        createCatalog.Content = JsonContent.Create(new CreatePartCatalogRequest(
            $"cat-{Guid.NewGuid():N}"[..10],
            "RFQ Catalog",
            string.Empty));
        var catalogResponse = await _supplyarrClient.SendAsync(createCatalog);
        catalogResponse.EnsureSuccessStatusCode();
        var catalog = (await catalogResponse.Content.ReadFromJsonAsync<PartCatalogResponse>())!;

        var createPart = Authorized(HttpMethod.Post, "/api/parts", _userToken);
        createPart.Content = JsonContent.Create(new CreatePartRequest(
            $"part-{Guid.NewGuid():N}"[..10],
            catalog.CatalogId,
            "RFQ Part",
            string.Empty,
            "general",
            "each",
            "OEM",
            "RFQ-001"));
        var partResponse = await _supplyarrClient.SendAsync(createPart);
        partResponse.EnsureSuccessStatusCode();
        var part = (await partResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        return (parentSupplier, supplierSubUnit, part);
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
            $"supplyarr-rfq-handoff-{Guid.NewGuid():N}",
            "supplyarr rfq handoff test",
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
}
