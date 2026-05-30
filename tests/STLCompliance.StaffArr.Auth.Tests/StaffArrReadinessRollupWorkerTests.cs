using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrReadinessRollupWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _supervisorToken = null!;
    private string _sharedWorkerToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ReadinessRollupNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"ReadinessRollupStaffArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["staffarr"],
            ReadinessRollupService.ProcessRollupsActionScope);

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
        _supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _staffarrClient.PostAsJsonAsync(
            "/api/internal/readiness-rollups/process-batch",
            new ProcessReadinessRollupsRequest(PlatformSeeder.DemoTenantId, null, 50, 1));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_rejects_trainarr_source_token()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var trainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            ReadinessRollupService.ProcessRollupsActionScope);

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/readiness-rollups/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessReadinessRollupsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            50,
            1));

        var response = await _staffarrClient.SendAsync(processRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_returns_team_and_site_org_units_before_processing()
    {
        var (teamId, _) = await SeedOrgHierarchyWithAssignmentAsync();

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/readiness-rollups/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=20&stalenessHours=1");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);

        var listResponse = await _staffarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingReadinessRollupsResponse>())!;
        Assert.Contains(pending.Items, x => x.ScopeType == ReadinessRollupRules.TeamScope && x.OrgUnitId == teamId);
        Assert.Contains(pending.Items, x => x.ScopeType == ReadinessRollupRules.SiteScope);
    }

    [Fact]
    public async Task Process_batch_refreshes_team_readiness_rollup_and_supervisor_can_read_it()
    {
        var (teamId, readyPersonId) = await SeedOrgHierarchyWithAssignmentAsync();
        await GrantBaselineCertificationsAsync(readyPersonId);

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/readiness-rollups/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);
        processRequest.Content = JsonContent.Create(new ProcessReadinessRollupsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var processResponse = await _staffarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessReadinessRollupsResponse>())!;
        Assert.True(body.RefreshedCount >= 1);
        Assert.Contains(
            body.RefreshedRollups,
            x => x.ScopeType == ReadinessRollupRules.TeamScope && x.OrgUnitId == teamId && x.ReadyCount >= 1);

        var teamRollupRequest = Authorized(
            HttpMethod.Get,
            $"/api/readiness-rollups/teams/{teamId}",
            _supervisorToken);
        var teamRollupResponse = await _staffarrClient.SendAsync(teamRollupRequest);
        teamRollupResponse.EnsureSuccessStatusCode();
        var teamRollup = (await teamRollupResponse.Content.ReadFromJsonAsync<ReadinessRollupSummaryResponse>())!;
        Assert.Equal(1, teamRollup.TotalMembers);
        Assert.Equal(1, teamRollup.ReadyCount);
        Assert.Equal(0, teamRollup.NotReadyCount);

        var membersRequest = Authorized(
            HttpMethod.Get,
            $"/api/readiness-rollups/teams/{teamId}/members",
            _supervisorToken);
        var membersResponse = await _staffarrClient.SendAsync(membersRequest);
        membersResponse.EnsureSuccessStatusCode();
        var membersBody = (await membersResponse.Content.ReadFromJsonAsync<ReadinessRollupMembersResponse>())!;
        Assert.Equal(teamId, membersBody.Rollup.OrgUnitId);
        Assert.Single(membersBody.Members);
        Assert.Equal(readyPersonId, membersBody.Members[0].PersonId);
        Assert.Equal("ready", membersBody.Members[0].ReadinessStatus);
        Assert.Equal("Rollup Ready", membersBody.Members[0].DisplayName);

        var notReadyFilterRequest = Authorized(
            HttpMethod.Get,
            $"/api/readiness-rollups/teams/{teamId}/members?readinessStatus=not_ready",
            _supervisorToken);
        var notReadyFilterResponse = await _staffarrClient.SendAsync(notReadyFilterRequest);
        notReadyFilterResponse.EnsureSuccessStatusCode();
        var notReadyFilterBody =
            (await notReadyFilterResponse.Content.ReadFromJsonAsync<ReadinessRollupMembersResponse>())!;
        Assert.Empty(notReadyFilterBody.Members);

        var v1TeamRollupResponse = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/readiness-rollups/teams/{teamId}",
            _supervisorToken));
        v1TeamRollupResponse.EnsureSuccessStatusCode();
        var v1TeamRollup = (await v1TeamRollupResponse.Content.ReadFromJsonAsync<ReadinessRollupSummaryResponse>())!;
        Assert.Equal(teamRollup.TotalMembers, v1TeamRollup.TotalMembers);
        Assert.Equal(teamRollup.ReadyCount, v1TeamRollup.ReadyCount);

        var v1MembersResponse = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/readiness-rollups/teams/{teamId}/members",
            _supervisorToken));
        v1MembersResponse.EnsureSuccessStatusCode();
        var v1MembersBody = (await v1MembersResponse.Content.ReadFromJsonAsync<ReadinessRollupMembersResponse>())!;
        Assert.Equal(membersBody.Members.Count, v1MembersBody.Members.Count);
        Assert.Equal(membersBody.Members[0].PersonId, v1MembersBody.Members[0].PersonId);
    }

    [Fact]
    public async Task List_team_rollups_denies_tenant_member_without_supervisor_scope()
    {
        var memberToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member");
        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            "/api/readiness-rollups/teams",
            memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var v1Response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            "/api/v1/readiness-rollups/teams",
            memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, v1Response.StatusCode);
    }

    private async Task<(Guid TeamId, Guid ReadyPersonId)> SeedOrgHierarchyWithAssignmentAsync()
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        var siteId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var readyPersonId = Guid.NewGuid();

        db.OrgUnits.AddRange(
            new OrgUnit
            {
                Id = siteId,
                TenantId = PlatformSeeder.DemoTenantId,
                UnitType = "site",
                Name = "Rollup Site",
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            },
            new OrgUnit
            {
                Id = deptId,
                TenantId = PlatformSeeder.DemoTenantId,
                UnitType = "department",
                Name = "Rollup Department",
                ParentOrgUnitId = siteId,
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            },
            new OrgUnit
            {
                Id = teamId,
                TenantId = PlatformSeeder.DemoTenantId,
                UnitType = "team",
                Name = "Rollup Team",
                ParentOrgUnitId = deptId,
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            },
            new OrgUnit
            {
                Id = positionId,
                TenantId = PlatformSeeder.DemoTenantId,
                UnitType = "position",
                Name = "Rollup Position",
                ParentOrgUnitId = teamId,
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            });

        db.People.Add(new StaffPerson
        {
            Id = readyPersonId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Rollup",
            FamilyName = "Ready",
            DisplayName = "Rollup Ready",
            PrimaryEmail = "rollup.ready@example.com",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.OrgUnitAssignments.Add(new OrgUnitAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = readyPersonId,
            SiteOrgUnitId = siteId,
            DepartmentOrgUnitId = deptId,
            TeamOrgUnitId = teamId,
            PositionOrgUnitId = positionId,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        await db.SaveChangesAsync();
        return (teamId, readyPersonId);
    }

    private async Task GrantBaselineCertificationsAsync(Guid personId)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        await StaffArrReadinessCertificationSeed.EnsureBaselineDefinitionsAsync(
            db,
            PlatformSeeder.DemoTenantId,
            CancellationToken.None);

        var definitions = await db.CertificationDefinitions
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.Category == "readiness")
            .ToListAsync();

        foreach (var definition in definitions)
        {
            db.PersonCertifications.Add(new PersonCertification
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                PersonId = personId,
                CertificationDefinitionId = definition.Id,
                SourceType = "manual",
                Status = "active",
                GrantedAt = now.AddMonths(-1),
                ExpiresAt = now.AddYears(1),
                GrantedByUserId = PlatformSeeder.DemoAdminUserId,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await db.SaveChangesAsync();
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-readiness-rollup-{Guid.NewGuid():N}",
            $"{sourceProduct} readiness rollup test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            allowedProducts,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<TContext>)
            || d.ServiceType == typeof(TContext)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
