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
using CreateSupplierRequest = SupplyArr.Api.Contracts.CreateSupplierRequest;
using SupplierResponse = SupplyArr.Api.Contracts.SupplierResponse;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrSupplierRestrictionTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"SupplierRestrictionNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplierRestrictionSupplyArr-{Guid.NewGuid():N}";

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
    public async Task Supplier_restriction_blocks_purchase_request_then_lift_allows()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createRestrictionRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/suppliers/{supplier.SupplierId}/restrictions",
            _userToken);
        createRestrictionRequest.Content = JsonContent.Create(new CreateSupplierRestrictionRequest(
            "quality-hold",
            [SupplierRestrictionScopes.PurchaseRequests],
            "Failed quality audit",
            null,
            null));
        var createRestrictionResponse = await _supplyarrClient.SendAsync(createRestrictionRequest);
        createRestrictionResponse.EnsureSuccessStatusCode();
        var restriction = (await createRestrictionResponse.Content.ReadFromJsonAsync<SupplierRestrictionResponse>())!;
        Assert.Equal("active", restriction.Status);

        var enforcementResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/suppliers/{supplier.SupplierId}/restrictions/enforcement", _userToken));
        enforcementResponse.EnsureSuccessStatusCode();
        var enforcement = (await enforcementResponse.Content.ReadFromJsonAsync<SupplierRestrictionEnforcementResponse>())!;
        Assert.True(enforcement.IsBlocked);
        Assert.Contains(SupplierRestrictionScopes.PurchaseRequests, enforcement.ActiveScopes);

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"vr-pr-{Guid.NewGuid():N}"[..20],
            "Restricted supplier PR",
            string.Empty,
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var blockedPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        Assert.Equal(HttpStatusCode.Conflict, blockedPrResponse.StatusCode);

        var liftRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/supplier-restrictions/{restriction.RestrictionId}/lift",
            _userToken);
        liftRequest.Content = JsonContent.Create(new LiftSupplierRestrictionRequest("Audit cleared"));
        var liftResponse = await _supplyarrClient.SendAsync(liftRequest);
        liftResponse.EnsureSuccessStatusCode();

        var allowedPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        allowedPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"vr-pr-{Guid.NewGuid():N}"[..20],
            "Restricted supplier PR after lift",
            string.Empty,
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var allowedPrResponse = await _supplyarrClient.SendAsync(allowedPrRequest);
        allowedPrResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Create_supplier_restriction_enqueues_outbox_event()
    {
        var supplier = await CreateSupplierAsync();

        var updateApproval = Authorized(
            HttpMethod.Patch,
            $"/api/suppliers/{supplier.SupplierId}/approval-status",
            _userToken);
        updateApproval.Content = JsonContent.Create(new UpdateSupplierApprovalStatusRequest("approved"));
        (await _supplyarrClient.SendAsync(updateApproval)).EnsureSuccessStatusCode();

        var createRestrictionRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/suppliers/{supplier.SupplierId}/restrictions",
            _userToken);
        createRestrictionRequest.Content = JsonContent.Create(new CreateSupplierRestrictionRequest(
            "compliance-hold",
            [SupplierRestrictionScopes.AllProcurement],
            "Missing insurance renewal",
            null,
            null));
        (await _supplyarrClient.SendAsync(createRestrictionRequest)).EnsureSuccessStatusCode();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outbox = await db.IntegrationOutboxEvents
            .Where(x => x.EventKind == IntegrationOutboxEventKinds.SupplierRestrictionCreated)
            .ToListAsync();
        Assert.NotEmpty(outbox);

        var persistedSupplier = await db.Suppliers.SingleAsync(x => x.Id == supplier.SupplierId);
        Assert.Equal("restricted", persistedSupplier.ApprovalStatus);
    }

    [Fact]
    public async Task Issue_purchase_order_is_blocked_when_required_supplier_document_is_expired()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var registerDocument = Authorized(
            HttpMethod.Post,
            $"/api/suppliers/{supplier.SupplierId}/compliance-documents",
            _userToken);
        registerDocument.Content = JsonContent.Create(new SupplierComplianceDocumentRegistrationRequest(
            $"doc-{Guid.NewGuid():N}"[..20],
            "insurance_certificate",
            "Expired insurance certificate",
            DateTimeOffset.UtcNow.AddDays(-1),
            null,
            "insurance.pdf",
            "application/pdf",
            1024,
            "s3://tests/insurance.pdf"));
        var registerDocumentResponse = await _supplyarrClient.SendAsync(registerDocument);
        registerDocumentResponse.EnsureSuccessStatusCode();
        var document = (await registerDocumentResponse.Content.ReadFromJsonAsync<SupplierComplianceDocumentResponse>())!;

        var approveDocument = Authorized(
            HttpMethod.Post,
            $"/api/suppliers/{supplier.SupplierId}/compliance-documents/{document.DocumentId}/approve",
            _userToken);
        (await _supplyarrClient.SendAsync(approveDocument)).EnsureSuccessStatusCode();

        var createPr = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPr.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"po-doc-{Guid.NewGuid():N}"[..20],
            "PO blocked by expired document",
            string.Empty,
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPr);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        (await _supplyarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            _userToken))).EnsureSuccessStatusCode();
        (await _supplyarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/approve",
            _userToken))).EnsureSuccessStatusCode();

        var createPo = Authorized(
            HttpMethod.Post,
            $"/api/purchase-orders/from-purchase-request/{purchaseRequest.PurchaseRequestId}",
            _userToken);
        createPo.Content = JsonContent.Create(new CreatePurchaseOrderFromPurchaseRequestRequest(
            $"po-{Guid.NewGuid():N}"[..16],
            "Blocked PO",
            string.Empty));
        var createPoResponse = await _supplyarrClient.SendAsync(createPo);
        Assert.Equal(HttpStatusCode.Conflict, createPoResponse.StatusCode);
    }

    private async Task<SupplierResponse> CreateSupplierAsync()
    {
        var createSupplier = Authorized(HttpMethod.Post, "/api/suppliers", _userToken);
        createSupplier.Content = JsonContent.Create(new CreateSupplierRequest(
            $"v-vr-{Guid.NewGuid():N}"[..12],
            null,
            null,
            "Restriction Supplier",
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
        var response = await _supplyarrClient.SendAsync(createSupplier);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplierResponse>())!;
    }

    private async Task<PartResponse> CreatePartAsync()
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", _userToken);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"vr-part-{Guid.NewGuid():N}"[..20],
            null,
            "Restriction Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var response = await _supplyarrClient.SendAsync(createPartRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PartResponse>())!;
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
            $"supplyarr-sr-handoff-{Guid.NewGuid():N}",
            "supplyarr supplier restriction handoff test",
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

