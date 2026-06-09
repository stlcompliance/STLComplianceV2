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
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
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

public sealed class SupplyArrStaffarrProcurementApprovalAuthorityTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _userToken = null!;
    private Guid _staffarrPersonId;
    private volatile bool _staffarrAuthorityLookupFails;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"SupplyProcAuthNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"SupplyProcAuthStaffArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplyProcAuthSupplyArr-{Guid.NewGuid():N}";

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
        var staffarrAuthorityToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["staffarr"],
            StaffArrIntegration.SupplyarrProcurementApprovalAuthorityReadActionScope);
        var staffarrHandoffToken = await IssueServiceTokenAsync(adminToken, "staffarr", ["staffarr"], "launch.redeem");
        var supplyarrHandoffToken = await IssueServiceTokenAsync(adminToken, "supplyarr", ["supplyarr"], "launch.redeem");

        WebApplicationFactory<global::SupplyArr.Api.Program>? supplyarrFactoryRef = null;

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", staffarrHandoffToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", supplyarrHandoffToken);
            builder.UseSetting("StaffArr:BaseUrl", _staffarrFactory.Server.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", staffarrAuthorityToken);
            builder.UseSetting("StaffArr:EnforceProcurementApprovalAuthority", "true");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrProcurementApprovalAuthorityClient>()
                    .ConfigurePrimaryHttpMessageHandler(() =>
                        new SwitchableStaffArrAuthorityLookupHandler(
                            _staffarrFactory.Server.CreateHandler(),
                            () => _staffarrAuthorityLookupFails));
            });
        });

        supplyarrFactoryRef = _supplyarrFactory;
        _staffarrClient = _staffarrFactory.CreateClient();
        _supplyarrClient = _supplyarrFactory.CreateClient();

        using var staffScope = _staffarrFactory.Services.CreateScope();
        var staffDb = staffScope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await staffDb.Database.EnsureCreatedAsync();
        _staffarrPersonId = await SeedStaffarrProcurementPersonAsync(staffDb);

        var handoffCode = await CreateHandoffAsync(adminToken);
        _userToken = await RedeemHandoffAsync(handoffCode);
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Purchase_request_submit_and_approve_enforce_staffarr_authority()
    {
        var vendor = await CreateVendorAsync();
        var part = await CreatePartAsync();

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"pa-pr-{Guid.NewGuid():N}"[..20],
            "Authority test PR",
            string.Empty,
            vendor.PartyId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var prResponse = await _supplyarrClient.SendAsync(createPrRequest);
        prResponse.EnsureSuccessStatusCode();
        var pr = (await prResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/purchase-requests/{pr.PurchaseRequestId}/submit", _userToken));
        submitResponse.EnsureSuccessStatusCode();

        var approveResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/purchase-requests/{pr.PurchaseRequestId}/approve", _userToken));
        approveResponse.EnsureSuccessStatusCode();

        var authorityResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/me/procurement-approval-authority", _userToken));
        authorityResponse.EnsureSuccessStatusCode();
        var authority = (await authorityResponse.Content.ReadFromJsonAsync<ProcurementApprovalAuthorityMirrorResponse>())!;
        Assert.True(authority.CanApprovePurchaseRequests);
        Assert.Equal("staffarr_mirror", authority.AuthoritySource);
    }

    [Fact]
    public async Task V1_procurement_approval_authority_alias_returns_staffarr_mirror()
    {
        var authorityResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/me/procurement-approval-authority", _userToken));
        authorityResponse.EnsureSuccessStatusCode();
        var authority = (await authorityResponse.Content.ReadFromJsonAsync<ProcurementApprovalAuthorityMirrorResponse>())!;
        Assert.True(authority.CanSubmitPurchaseRequests);
        Assert.StartsWith("staffarr_", authority.AuthoritySource, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Procurement_authority_mirror_uses_stale_cache_when_staffarr_is_unavailable()
    {
        var firstResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/me/procurement-approval-authority", _userToken));
        firstResponse.EnsureSuccessStatusCode();

        using (var scope = _supplyarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
            var mirror = await db.StaffarrProcurementApprovalAuthorityMirrors
                .FirstAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.StaffarrPersonId == _staffarrPersonId);
            mirror.RefreshedAt = DateTimeOffset.UtcNow - TimeSpan.FromHours(2);
            mirror.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }

        _staffarrAuthorityLookupFails = true;

        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/me/procurement-approval-authority", _userToken));
        response.EnsureSuccessStatusCode();

        var authority = (await response.Content.ReadFromJsonAsync<ProcurementApprovalAuthorityMirrorResponse>())!;
        Assert.Equal("staffarr_mirror", authority.AuthoritySource);
        Assert.True(authority.RefreshedAt < DateTimeOffset.UtcNow.AddMinutes(-90));
    }

    [Fact]
    public async Task Purchase_request_submit_denied_without_staffarr_submit_permission()
    {
        using (var staffScope = _staffarrFactory.Services.CreateScope())
        {
            var staffDb = staffScope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            var assignments = await staffDb.PersonRoleAssignments
                .Where(x => x.PersonId == _staffarrPersonId)
                .ToListAsync();
            staffDb.PersonRoleAssignments.RemoveRange(assignments);
            await staffDb.SaveChangesAsync();
        }

        var vendor = await CreateVendorAsync();
        var part = await CreatePartAsync();
        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", _userToken);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"pa-deny-{Guid.NewGuid():N}"[..20],
            "Denied submit PR",
            string.Empty,
            vendor.PartyId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 1m, string.Empty)]));
        var pr = (await (await _supplyarrClient.SendAsync(createPrRequest)).Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/purchase-requests/{pr.PurchaseRequestId}/submit", _userToken));
        Assert.Equal(HttpStatusCode.Forbidden, submitResponse.StatusCode);
    }

    private async Task<Guid> SeedStaffarrProcurementPersonAsync(StaffArrDbContext db)
    {
        var personId = Guid.NewGuid();
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            ExternalUserId = PlatformSeeder.DemoAdminUserId,
            GivenName = "Supply",
            FamilyName = "Approver",
            DisplayName = "Supply Approver",
            PrimaryEmail = "supply.approver@demo.stl",
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        var submitPermissionId = Guid.NewGuid();
        var approvePermissionId = Guid.NewGuid();
        var issuePermissionId = Guid.NewGuid();
        db.PermissionTemplates.AddRange(
            new PermissionTemplate
            {
                Id = submitPermissionId,
                TenantId = PlatformSeeder.DemoTenantId,
                PermissionKey = StaffArrProcurementPermissionKeys.PurchaseRequestsSubmit,
                Name = "Submit PR",
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            new PermissionTemplate
            {
                Id = approvePermissionId,
                TenantId = PlatformSeeder.DemoTenantId,
                PermissionKey = StaffArrProcurementPermissionKeys.PurchaseRequestsApprove,
                Name = "Approve PR",
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            new PermissionTemplate
            {
                Id = issuePermissionId,
                TenantId = PlatformSeeder.DemoTenantId,
                PermissionKey = StaffArrProcurementPermissionKeys.PurchaseOrdersIssue,
                Name = "Issue PO",
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });

        var roleId = Guid.NewGuid();
        db.RoleTemplates.Add(new RoleTemplate
        {
            Id = roleId,
            TenantId = PlatformSeeder.DemoTenantId,
            RoleKey = "supplyarr-buyer",
            Name = "SupplyArr buyer",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        db.RoleTemplatePermissions.AddRange(
            new RoleTemplatePermission
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                RoleTemplateId = roleId,
                PermissionTemplateId = submitPermissionId,
                ScopeType = "tenant",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            new RoleTemplatePermission
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                RoleTemplateId = roleId,
                PermissionTemplateId = approvePermissionId,
                ScopeType = "tenant",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            new RoleTemplatePermission
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                RoleTemplateId = roleId,
                PermissionTemplateId = issuePermissionId,
                ScopeType = "tenant",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });

        db.PersonRoleAssignments.Add(new PersonRoleAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = personId,
            RoleTemplateId = roleId,
            ScopeType = "tenant",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync();
        return personId;
    }

    private async Task<ExternalPartyResponse> CreateVendorAsync()
    {
        var createVendor = Authorized(HttpMethod.Post, "/api/vendors", _userToken);
        createVendor.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"v-pa-{Guid.NewGuid():N}"[..12],
            "Authority Vendor",
            string.Empty,
            null,
            string.Empty));
        var response = await _supplyarrClient.SendAsync(createVendor);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
    }

    private async Task<PartResponse> CreatePartAsync()
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", _userToken);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"pa-part-{Guid.NewGuid():N}"[..20],
            null,
            "Authority Part",
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-pa-{Guid.NewGuid():N}",
            "procurement approval cross-product test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            actionScope,
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

    private sealed class SwitchableStaffArrAuthorityLookupHandler(
        HttpMessageHandler innerHandler,
        Func<bool> shouldFail)
        : DelegatingHandler(innerHandler)
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (shouldFail())
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway)
                {
                    Content = new StringContent("upstream unavailable"),
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
