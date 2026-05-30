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
using SupplyArrRedeemHandoffRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrSupplierOnboardingTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"OnboardingNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"OnboardingSupplyArr-{Guid.NewGuid():N}";

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
    public async Task Supplier_onboarding_end_to_end_with_compliance_documents_and_outbox()
    {
        var vendor = await CreateVendorAsync();

        var startRequest = Authorized(HttpMethod.Post, "/api/supplier-onboarding/start", _userToken);
        startRequest.Content = JsonContent.Create(new StartSupplierOnboardingRequest(vendor.PartyId, "Initial onboarding"));
        var startResponse = await _supplyarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var onboarding = (await startResponse.Content.ReadFromJsonAsync<SupplierOnboardingResponse>())!;
        Assert.Equal("draft", onboarding.OnboardingStatus);

        await RegisterAndApproveDocumentAsync(vendor.PartyId, "W9", "w9", "W-9");
        await RegisterAndApproveDocumentAsync(vendor.PartyId, "INS", "insurance_certificate", "Insurance");
        await RegisterAndApproveDocumentAsync(vendor.PartyId, "AGR", "supplier_agreement", "Agreement");

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/supplier-onboarding/parties/{vendor.PartyId}/submit",
            _userToken);
        submitRequest.Content = JsonContent.Create(new SubmitSupplierOnboardingForReviewRequest("Ready for review"));
        var submitResponse = await _supplyarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();
        var submitted = (await submitResponse.Content.ReadFromJsonAsync<SupplierOnboardingResponse>())!;
        Assert.Equal("pending_review", submitted.OnboardingStatus);

        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/supplier-onboarding/pending", _userToken));
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<List<SupplierOnboardingResponse>>())!;
        Assert.Contains(pending, x => x.ExternalPartyId == vendor.PartyId);

        var approveResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/supplier-onboarding/parties/{vendor.PartyId}/approve", _userToken));
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<SupplierOnboardingResponse>())!;
        Assert.Equal("approved", approved.OnboardingStatus);

        var partyResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/vendors/{vendor.PartyId}", _userToken));
        partyResponse.EnsureSuccessStatusCode();
        var party = (await partyResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
        Assert.Equal("approved", party.ApprovalStatus);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var outbox = await db.IntegrationOutboxEvents
            .Where(x => x.EventKind == IntegrationOutboxEventKinds.SupplierOnboardingSubmitted)
            .ToListAsync();
        Assert.NotEmpty(outbox);
    }

    [Fact]
    public async Task Submit_for_review_fails_when_required_documents_missing()
    {
        var vendor = await CreateVendorAsync();

        var startRequest = Authorized(HttpMethod.Post, "/api/supplier-onboarding/start", _userToken);
        startRequest.Content = JsonContent.Create(new StartSupplierOnboardingRequest(vendor.PartyId, null));
        (await _supplyarrClient.SendAsync(startRequest)).EnsureSuccessStatusCode();

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/supplier-onboarding/parties/{vendor.PartyId}/submit",
            _userToken);
        submitRequest.Content = JsonContent.Create(new SubmitSupplierOnboardingForReviewRequest(null));
        var submitResponse = await _supplyarrClient.SendAsync(submitRequest);
        Assert.Equal(HttpStatusCode.BadRequest, submitResponse.StatusCode);
    }

    private async Task RegisterAndApproveDocumentAsync(
        Guid partyId,
        string documentKey,
        string documentTypeKey,
        string title)
    {
        var registerRequest = Authorized(
            HttpMethod.Post,
            $"/api/parties/{partyId}/compliance-documents",
            _userToken);
        registerRequest.Content = JsonContent.Create(new RegisterPartyComplianceDocumentRequest(
            documentKey,
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
        var doc = (await registerResponse.Content.ReadFromJsonAsync<PartyComplianceDocumentResponse>())!;

        var approveResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/parties/{partyId}/compliance-documents/{doc.DocumentId}/approve",
                _userToken));
        approveResponse.EnsureSuccessStatusCode();
    }

    private async Task<ExternalPartyResponse> CreateVendorAsync()
    {
        var createVendor = Authorized(HttpMethod.Post, "/api/vendors", _userToken);
        createVendor.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"v-onb-{Guid.NewGuid():N}"[..12],
            "Onboarding Vendor",
            string.Empty,
            null,
            string.Empty));
        var response = await _supplyarrClient.SendAsync(createVendor);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
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
            $"supplyarr-onb-handoff-{Guid.NewGuid():N}",
            "supplyarr onboarding handoff test",
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
