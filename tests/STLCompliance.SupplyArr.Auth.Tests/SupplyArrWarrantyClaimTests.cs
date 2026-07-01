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
using STLCompliance.Shared.Integration;
using CreateSupplierRequest = SupplyArr.Api.Contracts.CreateSupplierRequest;
using SupplierResponse = SupplyArr.Api.Contracts.SupplierResponse;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrWarrantyClaimTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"WarrantyClaimNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"WarrantyClaimSupplyArr-{Guid.NewGuid():N}";

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
    public async Task Warranty_claim_submit_supplier_response_close_workflow()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/warranty-claims", _userToken);
        createRequest.Content = JsonContent.Create(new CreateSupplierWarrantyClaimRequest(
            "WC-FULL-001",
            WarrantyClaimTypes.Defective,
            supplier.SupplierId,
            supplier.SupplierId,
            part.PartId,
            2m,
            "Motor failed within 30 days of install.",
            null,
            null,
            null,
            null,
            null));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var claim = (await createResponse.Content.ReadFromJsonAsync<WarrantyClaimResponse>())!;
        Assert.Equal(WarrantyClaimStatuses.Draft, claim.Status);

        var submitResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/warranty-claims/{claim.WarrantyClaimId}/submit",
                _userToken,
                new SubmitWarrantyClaimRequest(null)));
        submitResponse.EnsureSuccessStatusCode();
        var submitted = (await submitResponse.Content.ReadFromJsonAsync<WarrantyClaimResponse>())!;
        Assert.Equal(WarrantyClaimStatuses.Submitted, submitted.Status);

        var supplierResponseRequest = Authorized(
            HttpMethod.Post,
            $"/api/warranty-claims/{claim.WarrantyClaimId}/record-supplier-response",
            _userToken);
        supplierResponseRequest.Content = JsonContent.Create(new RecordWarrantyClaimSupplierResponseRequest(
            WarrantyClaimSupplierDispositions.Approved,
            "Supplier approved replacement shipment.",
            "RMA-9001"));
        var supplierResponse = await _supplyarrClient.SendAsync(supplierResponseRequest);
        supplierResponse.EnsureSuccessStatusCode();
        var responded = (await supplierResponse.Content.ReadFromJsonAsync<WarrantyClaimResponse>())!;
        Assert.Equal(WarrantyClaimStatuses.SupplierResponded, responded.Status);
        Assert.Equal(WarrantyClaimSupplierDispositions.Approved, responded.SupplierDisposition);

        var closeRequest = Authorized(
            HttpMethod.Post,
            $"/api/warranty-claims/{claim.WarrantyClaimId}/close",
            _userToken);
        closeRequest.Content = JsonContent.Create(new CloseWarrantyClaimRequest("Replacement received and installed."));
        var closeResponse = await _supplyarrClient.SendAsync(closeRequest);
        closeResponse.EnsureSuccessStatusCode();
        var closed = (await closeResponse.Content.ReadFromJsonAsync<WarrantyClaimResponse>())!;
        Assert.Equal(WarrantyClaimStatuses.Closed, closed.Status);
    }

    [Fact]
    public async Task Warranty_claim_can_be_denied_from_submitted()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/warranty-claims", _userToken);
        createRequest.Content = JsonContent.Create(new CreateSupplierWarrantyClaimRequest(
            "WC-DENY-001",
            WarrantyClaimTypes.Doa,
            supplier.SupplierId,
            supplier.SupplierId,
            part.PartId,
            1m,
            "Dead on arrival.",
            null,
            null,
            null,
            null,
            null));
        var createResponse = await _supplyarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var claim = (await createResponse.Content.ReadFromJsonAsync<WarrantyClaimResponse>())!;

        await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/warranty-claims/{claim.WarrantyClaimId}/submit",
                _userToken,
                new SubmitWarrantyClaimRequest(null)));

        var denyRequest = Authorized(
            HttpMethod.Post,
            $"/api/warranty-claims/{claim.WarrantyClaimId}/deny",
            _userToken);
        denyRequest.Content = JsonContent.Create(new DenyWarrantyClaimRequest("Outside warranty period per supplier policy."));
        var denyResponse = await _supplyarrClient.SendAsync(denyRequest);
        denyResponse.EnsureSuccessStatusCode();
        var denied = (await denyResponse.Content.ReadFromJsonAsync<WarrantyClaimResponse>())!;
        Assert.Equal(WarrantyClaimStatuses.Denied, denied.Status);
    }

    [Fact]
    public async Task Create_warranty_claim_enqueues_outbox_event()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/warranty-claims", _userToken);
        createRequest.Content = JsonContent.Create(new CreateSupplierWarrantyClaimRequest(
            "WC-OUT-001",
            WarrantyClaimTypes.Other,
            supplier.SupplierId,
            supplier.SupplierId,
            part.PartId,
            1m,
            "Cosmetic defect on housing.",
            null,
            null,
            null,
            null,
            null));
        (await _supplyarrClient.SendAsync(createRequest)).EnsureSuccessStatusCode();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outbox = await db.IntegrationOutboxEvents
            .Where(x => x.EventKind == IntegrationOutboxEventKinds.WarrantyClaimCreated)
            .ToListAsync();
        Assert.NotEmpty(outbox);
    }

    private async Task<SupplierResponse> CreateSupplierAsync()
    {
        var createSupplier = Authorized(HttpMethod.Post, "/api/suppliers", _userToken);
        createSupplier.Content = JsonContent.Create(new CreateSupplierRequest(
            $"v-wc-{Guid.NewGuid():N}"[..12],
            null,
            null,
            "Warranty Supplier",
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
            $"wc-part-{Guid.NewGuid():N}"[..20],
            null,
            "Warranty Part",
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
            $"supplyarr-wc-handoff-{Guid.NewGuid():N}",
            "supplyarr warranty claim handoff test",
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

    private static HttpRequestMessage Authorized(
        HttpMethod method,
        string path,
        string token,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }
}
