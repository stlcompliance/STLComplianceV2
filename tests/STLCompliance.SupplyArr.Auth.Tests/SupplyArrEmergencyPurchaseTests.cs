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
using CreateTypedExternalPartyRequest = SupplyArr.Api.Contracts.CreateTypedExternalPartyRequest;
using ExternalPartyResponse = SupplyArr.Api.Contracts.ExternalPartyResponse;
using CreatePartCatalogRequest = SupplyArr.Api.Contracts.CreatePartCatalogRequest;
using PartCatalogResponse = SupplyArr.Api.Contracts.PartCatalogResponse;
using CreatePartRequest = SupplyArr.Api.Contracts.CreatePartRequest;
using PartResponse = SupplyArr.Api.Contracts.PartResponse;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrEmergencyPurchaseTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"EmergencyNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"EmergencySupplyArr-{Guid.NewGuid():N}";

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
    public async Task Emergency_purchase_end_to_end_override_approve_and_issue_po()
    {
        var (vendor, part) = await SeedVendorAndPartAsync();
        var requestKey = $"emg-{Guid.NewGuid():N}"[..12];

        var createRequest = Authorized(HttpMethod.Post, "/api/emergency-purchases", _userToken);
        createRequest.Content = JsonContent.Create(new CreateEmergencyPurchaseRequest(
            requestKey,
            "Emergency brake pads",
            "Fleet down — safety critical",
            vendor.PartyId,
            "Urgent replenishment",
            [new CreatePurchaseRequestLineRequest(part.PartId, 5m, "line")]));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<EmergencyPurchaseResponse>())!;
        Assert.Equal("draft", created.Status);
        Assert.Equal("Fleet down — safety critical", created.EmergencyReason);

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/emergency-purchases/{created.PurchaseRequestId}/expedited-submit",
            _userToken);
        submitRequest.Content = JsonContent.Create(new ExpeditedSubmitEmergencyPurchaseRequest("Expedite for approval"));
        var submitResponse = await _supplyarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();
        var submitted = (await submitResponse.Content.ReadFromJsonAsync<EmergencyPurchaseResponse>())!;
        Assert.Equal("submitted", submitted.Status);
        Assert.NotNull(submitted.EmergencyExpeditedAt);

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/emergency-purchases/pending", _userToken));
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<List<EmergencyPurchaseResponse>>())!;
        Assert.Contains(pending, x => x.PurchaseRequestId == created.PurchaseRequestId);

        var approveRequest = Authorized(
            HttpMethod.Post,
            $"/api/emergency-purchases/{created.PurchaseRequestId}/manager-override-approve",
            _userToken);
        approveRequest.Content = JsonContent.Create(
            new ManagerOverrideApproveEmergencyPurchaseRequest("Safety downtime — authorize immediate procurement"));
        var approveResponse = await _supplyarrClient.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<EmergencyPurchaseResponse>())!;
        Assert.Equal("approved", approved.Status);
        Assert.True(approved.ManagerOverrideApproved);

        var orderKey = $"PO-{Guid.NewGuid():N}"[..12];
        var issueRequest = Authorized(
            HttpMethod.Post,
            $"/api/emergency-purchases/{created.PurchaseRequestId}/issue-purchase-order",
            _userToken);
        issueRequest.Content = JsonContent.Create(new IssueEmergencyPurchaseOrderRequest(orderKey, null, null));
        var issueResponse = await _supplyarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<IssueEmergencyPurchaseOrderResponse>())!;
        Assert.Equal(orderKey.ToLowerInvariant(), issued.PurchaseOrder.OrderKey);
        Assert.Equal(created.PurchaseRequestId, issued.PurchaseOrder.PurchaseRequestId);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outbox = await db.IntegrationOutboxEvents
            .Where(x => x.EventKind == IntegrationOutboxEventKinds.EmergencyPurchaseManagerOverrideApproved)
            .ToListAsync();
        Assert.NotEmpty(outbox);
    }

    [Fact]
    public async Task Issue_purchase_order_fails_without_manager_override()
    {
        var (vendor, part) = await SeedVendorAndPartAsync();
        var requestKey = $"emg-{Guid.NewGuid():N}"[..12];

        var createRequest = Authorized(HttpMethod.Post, "/api/emergency-purchases", _userToken);
        createRequest.Content = JsonContent.Create(new CreateEmergencyPurchaseRequest(
            requestKey,
            "Emergency part",
            "Test",
            vendor.PartyId,
            string.Empty,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        (await _supplyarrClient.SendAsync(createRequest)).EnsureSuccessStatusCode();

        var pr = await dbGetEmergencyByKey(requestKey);

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/emergency-purchases/{pr.PurchaseRequestId}/expedited-submit",
            _userToken);
        submitRequest.Content = JsonContent.Create(new ExpeditedSubmitEmergencyPurchaseRequest(null));
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        var issueRequest = Authorized(
            HttpMethod.Post,
            $"/api/emergency-purchases/{pr.PurchaseRequestId}/issue-purchase-order",
            _userToken);
        issueRequest.Content = JsonContent.Create(new IssueEmergencyPurchaseOrderRequest($"PO-{Guid.NewGuid():N}"[..10], null, null));
        var issueResponse = await _supplyarrClient.SendAsync(issueRequest);
        Assert.Equal(HttpStatusCode.Conflict, issueResponse.StatusCode);
    }

    private async Task<EmergencyPurchaseResponse> dbGetEmergencyByKey(string requestKey)
    {
        var listResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/emergency-purchases", _userToken));
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<List<EmergencyPurchaseResponse>>())!;
        return list.First(x => x.RequestKey == requestKey);
    }

    private async Task<(ExternalPartyResponse Vendor, PartResponse Part)> SeedVendorAndPartAsync()
    {
        var createVendor = Authorized(HttpMethod.Post, "/api/vendors", _userToken);
        createVendor.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"v-emg-{Guid.NewGuid():N}"[..10],
            "Emergency Vendor",
            string.Empty,
            null,
            string.Empty));
        var vendorResponse = await _supplyarrClient.SendAsync(createVendor);
        vendorResponse.EnsureSuccessStatusCode();
        var vendor = (await vendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var createCatalog = Authorized(HttpMethod.Post, "/api/catalogs", _userToken);
        createCatalog.Content = JsonContent.Create(new CreatePartCatalogRequest(
            $"cat-{Guid.NewGuid():N}"[..10],
            "Emergency Catalog",
            string.Empty));
        var catalogResponse = await _supplyarrClient.SendAsync(createCatalog);
        catalogResponse.EnsureSuccessStatusCode();
        var catalog = (await catalogResponse.Content.ReadFromJsonAsync<PartCatalogResponse>())!;

        var createPart = Authorized(HttpMethod.Post, "/api/parts", _userToken);
        createPart.Content = JsonContent.Create(new CreatePartRequest(
            $"part-{Guid.NewGuid():N}"[..10],
            catalog.CatalogId,
            "Emergency Part",
            string.Empty,
            "general",
            "each",
            "OEM",
            "EMG-001"));
        var partResponse = await _supplyarrClient.SendAsync(createPart);
        partResponse.EnsureSuccessStatusCode();
        var part = (await partResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        return (vendor, part);
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
            $"supplyarr-emg-handoff-{Guid.NewGuid():N}",
            "supplyarr emergency handoff test",
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
        var request = Authorized(HttpMethod.Post, "/api/launch/handoff", adminToken);
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
