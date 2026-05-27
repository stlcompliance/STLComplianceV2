using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using StaffArrRedeemRequest = StaffArr.Api.Contracts.RedeemHandoffRequest;
using StaffArrSessionBootstrapResponse = StaffArr.Api.Contracts.StaffArrSessionBootstrapResponse;
using StaffArrMeResponse = StaffArr.Api.Contracts.StaffArrMeResponse;
using StaffArrHandoffSessionResponse = StaffArr.Api.Contracts.HandoffSessionResponse;
using StaffPersonSummaryResponse = StaffArr.Api.Contracts.StaffPersonSummaryResponse;
using StaffPersonDetailResponse = StaffArr.Api.Contracts.StaffPersonDetailResponse;
using CreateStaffPersonRequest = StaffArr.Api.Contracts.CreateStaffPersonRequest;
using OrgUnitResponse = StaffArr.Api.Contracts.OrgUnitResponse;
using CreateOrgUnitRequest = StaffArr.Api.Contracts.CreateOrgUnitRequest;
using UpdateOrgUnitRequest = StaffArr.Api.Contracts.UpdateOrgUnitRequest;
using UpdateOrgUnitStatusRequest = StaffArr.Api.Contracts.UpdateOrgUnitStatusRequest;
using StaffArrTokenService = StaffArr.Api.Services.StaffArrTokenService;
using StaffArrDbContext = StaffArr.Api.Data.StaffArrDbContext;
using StaffPerson = StaffArr.Api.Entities.StaffPerson;
using OrgUnit = StaffArr.Api.Entities.OrgUnit;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrHandoffApiTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _serviceToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"StaffArrHandoffNexArrTests-{Guid.NewGuid():N}";
        var staffArrDbName = $"StaffArrHandoffTests-{Guid.NewGuid():N}";

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
        _serviceToken = await IssueServiceTokenAsync(adminToken, "staffarr");

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _serviceToken);
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<global::StaffArr.Api.Data.StaffArrDbContext>)
                        || d.ServiceType == typeof(global::StaffArr.Api.Data.StaffArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<global::StaffArr.Api.Data.StaffArrDbContext>(options =>
                    options.UseInMemoryDatabase(staffArrDbName));

                services.AddHttpClient<global::StaffArr.Api.Services.NexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Handoff_redeem_happy_path_returns_session_and_me_works()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _staffarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new StaffArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<StaffArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Equal(PlatformSeeder.DemoAdminUserId, session.UserId);
        Assert.NotEqual(Guid.Empty, session.PersonId);
        Assert.Contains(session.TenantRoleKey, new[] { "tenant_admin", "platform_admin" });
        Assert.Contains("staffarr", session.Entitlements);

        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        var meResponse = await _staffarrClient.SendAsync(meRequest);
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<StaffArrMeResponse>();
        Assert.NotNull(me);
        Assert.True(me.HasStaffArrEntitlement);
        Assert.Contains(me.TenantRoleKey, new[] { "tenant_admin", "platform_admin" });
        Assert.Equal(session.PersonId, me.PersonId);
    }

    [Fact]
    public async Task Handoff_redeem_invalid_code_returns_unauthorized()
    {
        var response = await _staffarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new StaffArrRedeemRequest("not-a-valid-handoff-code"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handoff_redeem_revoked_entitlement_is_rejected()
    {
        await RevokeStaffArrEntitlementAsync();
        var handoffCode = await CreateHandoffAsync();

        var response = await _staffarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new StaffArrRedeemRequest(handoffCode));

        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden });
    }

    [Fact]
    public async Task Me_requires_authenticated_user()
    {
        var response = await _staffarrClient.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_forbids_users_without_staffarr_entitlement_claim()
    {
        var token = CreateStaffArrAccessToken(["nexarr"]);
        var request = Authorized(HttpMethod.Get, "/api/me", token);
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Session_bootstrap_returns_claim_backed_identity()
    {
        var token = CreateStaffArrAccessToken(["staffarr", "nexarr"], tenantRoleKey: "tenant_admin");
        var request = Authorized(HttpMethod.Get, "/api/session", token);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<StaffArrSessionBootstrapResponse>();
        Assert.NotNull(payload);
        Assert.Equal(PlatformSeeder.DemoAdminUserId, payload.UserId);
        Assert.NotEqual(Guid.Empty, payload.PersonId);
        Assert.Equal("tenant_admin", payload.TenantRoleKey);
        Assert.True(payload.HasStaffArrEntitlement);
    }

    [Fact]
    public async Task People_directory_denies_member_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member");
        var request = Authorized(HttpMethod.Get, "/api/people", token);
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task People_directory_returns_records_for_tenant_admin()
    {
        await SeedStaffPersonAsync(Guid.NewGuid(), "Directory User", "directory.user@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var request = Authorized(HttpMethod.Get, "/api/people", token);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<StaffPersonSummaryResponse>>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload);
    }

    [Fact]
    public async Task People_profile_allows_self_access_for_member_role()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Member User", "member.user@example.com", externalUserId: PlatformSeeder.DemoAdminUserId);
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: personId);
        var request = Authorized(HttpMethod.Get, $"/api/people/{personId}", token);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<StaffPersonDetailResponse>();
        Assert.NotNull(payload);
        Assert.Equal(personId, payload.PersonId);
    }

    [Fact]
    public async Task People_create_validates_input_and_permissions()
    {
        var memberToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var deniedRequest = Authorized(HttpMethod.Post, "/api/people", memberToken);
        deniedRequest.Content = JsonContent.Create(new CreateStaffPersonRequest(
            "No",
            "Write",
            "no.write@example.com",
            "active",
            null,
            null,
            "Inspector"));
        var deniedResponse = await _staffarrClient.SendAsync(deniedRequest);
        Assert.Equal(HttpStatusCode.Forbidden, deniedResponse.StatusCode);

        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var badRequest = Authorized(HttpMethod.Post, "/api/people", adminToken);
        badRequest.Content = JsonContent.Create(new CreateStaffPersonRequest(
            "Bad",
            "Email",
            "not-an-email",
            "active",
            null,
            null,
            null));
        var badResponse = await _staffarrClient.SendAsync(badRequest);
        Assert.Equal(HttpStatusCode.BadRequest, badResponse.StatusCode);
    }

    [Fact]
    public async Task Org_unit_write_happy_path_update_and_status_flow()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/org-units", token);
        createRequest.Content = JsonContent.Create(new CreateOrgUnitRequest("department", "Operations", null));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;

        var updateRequest = Authorized(HttpMethod.Put, $"/api/org-units/{created.OrgUnitId}", token);
        updateRequest.Content = JsonContent.Create(new UpdateOrgUnitRequest("department", "Operations HQ", null));
        var updateResponse = await _staffarrClient.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;
        Assert.Equal("Operations HQ", updated.Name);

        var statusRequest = Authorized(HttpMethod.Patch, $"/api/org-units/{created.OrgUnitId}/status", token);
        statusRequest.Content = JsonContent.Create(new UpdateOrgUnitStatusRequest("inactive"));
        var statusResponse = await _staffarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var statusPayload = (await statusResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;
        Assert.Equal("inactive", statusPayload.Status);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditEvents = await db.AuditEvents.CountAsync(x => x.TenantId == PlatformSeeder.DemoTenantId);
        Assert.True(auditEvents >= 3);
    }

    [Fact]
    public async Task Org_unit_write_denies_non_writer_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var request = Authorized(HttpMethod.Post, "/api/org-units", token);
        request.Content = JsonContent.Create(new CreateOrgUnitRequest("department", "Denied Unit", null));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Org_unit_write_rejects_invalid_hierarchy_and_conflicts()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var rootId = await SeedOrgUnitAsync("department", "Root Unit", null, "active");
        var childId = await SeedOrgUnitAsync("team", "Child Unit", rootId, "active");
        await SeedOrgUnitAsync("department", "Root Unit", null, "active", tenantId: Guid.NewGuid());

        var duplicateRequest = Authorized(HttpMethod.Post, "/api/org-units", token);
        duplicateRequest.Content = JsonContent.Create(new CreateOrgUnitRequest("department", "Root Unit", null));
        var duplicateResponse = await _staffarrClient.SendAsync(duplicateRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var cycleRequest = Authorized(HttpMethod.Put, $"/api/org-units/{rootId}", token);
        cycleRequest.Content = JsonContent.Create(new UpdateOrgUnitRequest("department", "Root Unit", childId));
        var cycleResponse = await _staffarrClient.SendAsync(cycleRequest);
        Assert.Equal(HttpStatusCode.Conflict, cycleResponse.StatusCode);

        var deactivateParentRequest = Authorized(HttpMethod.Patch, $"/api/org-units/{rootId}/status", token);
        deactivateParentRequest.Content = JsonContent.Create(new UpdateOrgUnitStatusRequest("inactive"));
        var deactivateParentResponse = await _staffarrClient.SendAsync(deactivateParentRequest);
        Assert.Equal(HttpStatusCode.Conflict, deactivateParentResponse.StatusCode);
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", "http://localhost:5175/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task RevokeStaffArrEntitlementAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var entitlement = await db.Entitlements.FirstAsync(
            e => e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "staffarr");
        entitlement.Status = EntitlementStatuses.Revoked;
        await db.SaveChangesAsync();
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-staffarr-handoff-test",
            $"{productKey} StaffArr Handoff Test",
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

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
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

    private async Task SeedStaffPersonAsync(
        Guid personId,
        string displayName,
        string email,
        Guid? externalUserId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var split = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var givenName = split.FirstOrDefault() ?? "User";
        var familyName = split.Length > 1 ? string.Join(' ', split.Skip(1)) : "Test";

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            ExternalUserId = externalUserId,
            GivenName = givenName,
            FamilyName = familyName,
            DisplayName = displayName,
            PrimaryEmail = email,
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedOrgUnitAsync(
        string unitType,
        string name,
        Guid? parentOrgUnitId,
        string status,
        Guid? tenantId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var orgUnit = new OrgUnit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? PlatformSeeder.DemoTenantId,
            UnitType = unitType,
            Name = name,
            ParentOrgUnitId = parentOrgUnitId,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.OrgUnits.Add(orgUnit);
        await db.SaveChangesAsync();
        return orgUnit.Id;
    }
}
