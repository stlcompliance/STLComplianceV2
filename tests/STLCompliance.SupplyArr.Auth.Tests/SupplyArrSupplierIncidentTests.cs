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
using CreateTypedExternalPartyRequest = SupplyArr.Api.Contracts.CreateTypedExternalPartyRequest;
using ExternalPartyResponse = SupplyArr.Api.Contracts.ExternalPartyResponse;
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrSupplierIncidentTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"SupplierIncidentNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplierIncidentSupplyArr-{Guid.NewGuid():N}";

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
    public async Task Supplier_incident_workflow_with_procurement_restriction()
    {
        var supplier = await CreateSupplierAsync();
        var part = await CreatePartAsync();

        var createIncidentRequest = Authorized(HttpMethod.Post, "/api/supplier-incidents", _userToken);
        createIncidentRequest.Content = JsonContent.Create(new CreateSupplierIncidentRequest(
            supplier.PartyId,
            "SI-QUAL-001",
            "Contaminated shipment",
            "Foreign material found in sealed containers.",
            SupplierIncidentTypes.Quality,
            SupplierIncidentSeverities.Critical,
            null,
            null,
            null,
            null,
            null));
        var createResponse = await _supplyarrClient.SendAsync(createIncidentRequest);
        createResponse.EnsureSuccessStatusCode();
        var incident = (await createResponse.Content.ReadFromJsonAsync<SupplierIncidentResponse>())!;
        Assert.Equal(SupplierIncidentStatuses.Open, incident.Status);

        var investigateResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/supplier-incidents/{incident.IncidentId}/start-investigation",
                _userToken));
        investigateResponse.EnsureSuccessStatusCode();
        var investigating = (await investigateResponse.Content.ReadFromJsonAsync<SupplierIncidentResponse>())!;
        Assert.Equal(SupplierIncidentStatuses.Investigating, investigating.Status);

        var restrictRequest = Authorized(
            HttpMethod.Post,
            $"/api/supplier-incidents/{incident.IncidentId}/apply-procurement-restriction",
            _userToken);
        restrictRequest.Content = JsonContent.Create(new ApplySupplierIncidentProcurementRestrictionRequest(
            "si-hold-001",
            [VendorRestrictionScopes.AllProcurement],
            "Critical quality incident SI-QUAL-001"));
        var restrictResponse = await _supplyarrClient.SendAsync(restrictRequest);
        restrictResponse.EnsureSuccessStatusCode();
        var restricted = (await restrictResponse.Content.ReadFromJsonAsync<SupplierIncidentResponse>())!;
        Assert.NotNull(restricted.VendorRestrictionId);

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"si-pr-{Guid.NewGuid():N}"[..20],
            "Blocked by incident",
            string.Empty,
            supplier.PartyId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var blockedPr = await _supplyarrClient.SendAsync(createPrRequest);
        Assert.Equal(HttpStatusCode.Conflict, blockedPr.StatusCode);

        var resolveRequest = Authorized(
            HttpMethod.Post,
            $"/api/supplier-incidents/{incident.IncidentId}/resolve",
            _userToken);
        resolveRequest.Content = JsonContent.Create(new ResolveSupplierIncidentRequest(
            "Supplier corrective action plan accepted."));
        (await _supplyarrClient.SendAsync(resolveRequest)).EnsureSuccessStatusCode();

        var closeRequest = Authorized(
            HttpMethod.Post,
            $"/api/supplier-incidents/{incident.IncidentId}/close",
            _userToken);
        closeRequest.Content = JsonContent.Create(new CloseSupplierIncidentRequest(null));
        var closeResponse = await _supplyarrClient.SendAsync(closeRequest);
        closeResponse.EnsureSuccessStatusCode();
        var closed = (await closeResponse.Content.ReadFromJsonAsync<SupplierIncidentResponse>())!;
        Assert.Equal(SupplierIncidentStatuses.Closed, closed.Status);
    }

    [Fact]
    public async Task Create_supplier_incident_enqueues_outbox_event()
    {
        var supplier = await CreateSupplierAsync();

        var createIncidentRequest = Authorized(HttpMethod.Post, "/api/supplier-incidents", _userToken);
        createIncidentRequest.Content = JsonContent.Create(new CreateSupplierIncidentRequest(
            supplier.PartyId,
            "SI-DEL-002",
            "Late delivery",
            "PO arrived 5 days late.",
            SupplierIncidentTypes.Delivery,
            SupplierIncidentSeverities.Medium,
            null,
            null,
            null,
            null,
            null));
        (await _supplyarrClient.SendAsync(createIncidentRequest)).EnsureSuccessStatusCode();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outbox = await db.IntegrationOutboxEvents
            .Where(x => x.EventKind == IntegrationOutboxEventKinds.SupplierIncidentCreated)
            .ToListAsync();
        Assert.NotEmpty(outbox);
    }

    private async Task<ExternalPartyResponse> CreateSupplierAsync()
    {
        var createSupplier = Authorized(HttpMethod.Post, "/api/suppliers", _userToken);
        createSupplier.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"s-si-{Guid.NewGuid():N}"[..12],
            "Incident Supplier",
            string.Empty,
            null,
            string.Empty));
        var response = await _supplyarrClient.SendAsync(createSupplier);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
    }

    private async Task<PartResponse> CreatePartAsync()
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", _userToken);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"si-part-{Guid.NewGuid():N}"[..20],
            null,
            "Incident Part",
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
            $"supplyarr-si-handoff-{Guid.NewGuid():N}",
            "supplyarr supplier incident handoff test",
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
