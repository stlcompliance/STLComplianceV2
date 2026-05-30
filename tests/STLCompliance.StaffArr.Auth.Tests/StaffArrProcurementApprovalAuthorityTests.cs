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
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrProcurementApprovalAuthorityTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _supplyarrAuthorityToken = null!;
    private Guid _personId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ProcAuthNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"ProcAuthStaffArr-{Guid.NewGuid():N}";

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
        _supplyarrAuthorityToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["staffarr"],
            StaffArrIntegration.SupplyarrProcurementApprovalAuthorityReadActionScope);
        var staffarrHandoffToken = await IssueServiceTokenAsync(adminToken, "staffarr", ["staffarr"], "launch.redeem");

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

        _staffarrClient = _staffarrFactory.CreateClient();
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();

        _personId = await SeedPersonWithProcurementAuthorityAsync(db);
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Integration_endpoint_returns_procurement_authority()
    {
        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/integrations/procurement-approval-authority?tenantId={PlatformSeeder.DemoTenantId}&personId={_personId}",
            _supplyarrAuthorityToken));
        response.EnsureSuccessStatusCode();
        var authority = (await response.Content.ReadFromJsonAsync<ProcurementApprovalAuthorityResponse>())!;
        Assert.True(authority.CanSubmitPurchaseRequests);
        Assert.True(authority.CanApprovePurchaseRequests);
        Assert.True(authority.CanIssuePurchaseOrders);
        Assert.Equal(25000m, authority.MaxApproveAmount);

        var v1Response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/integrations/procurement-approval-authority?tenantId={PlatformSeeder.DemoTenantId}&personId={_personId}",
            _supplyarrAuthorityToken));
        v1Response.EnsureSuccessStatusCode();
        var v1Authority = (await v1Response.Content.ReadFromJsonAsync<ProcurementApprovalAuthorityResponse>())!;
        Assert.Equal(authority.PersonId, v1Authority.PersonId);
        Assert.Equal(authority.CanApprovePurchaseRequests, v1Authority.CanApprovePurchaseRequests);
        Assert.Equal(authority.MaxApproveAmount, v1Authority.MaxApproveAmount);
    }

    [Fact]
    public async Task Integration_endpoint_rejects_trainarr_source_token()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var trainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            StaffArrIntegration.SupplyarrProcurementApprovalAuthorityReadActionScope);

        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/integrations/procurement-approval-authority?tenantId={PlatformSeeder.DemoTenantId}&personId={_personId}",
            trainarrToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> SeedPersonWithProcurementAuthorityAsync(StaffArrDbContext db)
    {
        var personId = Guid.NewGuid();
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            ExternalUserId = PlatformSeeder.DemoAdminUserId,
            GivenName = "Proc",
            FamilyName = "Approver",
            DisplayName = "Proc Approver",
            PrimaryEmail = "proc.approver@demo.stl",
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
            RoleKey = "supplyarr-procurement-approver",
            Name = "SupplyArr procurement approver",
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
                ScopeType = StaffArrProcurementScopeTypes.MonetaryLimit,
                ScopeValue = "25000",
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
            $"{sourceProduct}-proc-auth-{Guid.NewGuid():N}",
            "procurement approval authority test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
