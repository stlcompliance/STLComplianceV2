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
using StaffArr.Api.Endpoints;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using STLCompliance.Shared.Operations;
using STLCompliance.Shared.Integration;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrIntegrationPermissionCheckTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _maintainarrPermissionCheckToken = null!;
    private string _maintainarrPermissionCatalogSyncToken = null!;
    private string _maintainarrSitesReadToken = null!;
    private string _loadarrPermissionCatalogSyncToken = null!;
    private Guid _personId;
    private Guid _inactivePersonId;
    private Guid _activeSiteOrgUnitId;
    private Guid _inactiveSiteOrgUnitId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PermissionCheckNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"PermissionCheckStaffArr-{Guid.NewGuid():N}";

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
        _maintainarrPermissionCheckToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            ["staffarr"],
            IntegrationEndpoints.PermissionCheckReadActionScope);
        _maintainarrPermissionCatalogSyncToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            ["staffarr"],
            IntegrationEndpoints.ProductPermissionCatalogSyncActionScope);
        _maintainarrSitesReadToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            ["staffarr"],
            IntegrationEndpoints.SitesReadActionScope);
        _loadarrPermissionCatalogSyncToken = await IssueServiceTokenAsync(
            adminToken,
            "loadarr",
            ["staffarr"],
            IntegrationEndpoints.ProductPermissionCatalogSyncActionScope);

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
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();

        (_personId, _inactivePersonId, _activeSiteOrgUnitId, _inactiveSiteOrgUnitId) = await SeedPeopleAsync(db);
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Integration_permission_check_returns_grants_and_authorization_summary()
    {
        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/integrations/permission-check?tenantId={PlatformSeeder.DemoTenantId}&personId={_personId}&permissionKey=maintainarr.work_order.close&permissionKey=maintainarr.work_order.create",
            _maintainarrPermissionCheckToken));
        response.EnsureSuccessStatusCode();

        var check = (await response.Content.ReadFromJsonAsync<IntegrationPermissionCheckResponse>())!;
        Assert.Equal(_personId, check.PersonId);
        Assert.True(check.IsPersonActive);
        Assert.False(check.IsAuthorizedAll);
        Assert.True(check.IsAuthorizedAny);

        var closeCheck = Assert.Single(check.Checks, x => x.PermissionKey == "maintainarr.work_order.close");
        Assert.True(closeCheck.Granted);
        Assert.NotEmpty(closeCheck.Grants);

        var createCheck = Assert.Single(check.Checks, x => x.PermissionKey == "maintainarr.work_order.create");
        Assert.False(createCheck.Granted);
        Assert.Empty(createCheck.Grants);

        var v1Response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/integrations/permission-check?tenantId={PlatformSeeder.DemoTenantId}&personId={_personId}&permissionKey=maintainarr.work_order.close&permissionKey=maintainarr.work_order.create",
            _maintainarrPermissionCheckToken));
        v1Response.EnsureSuccessStatusCode();
        var v1Check = (await v1Response.Content.ReadFromJsonAsync<IntegrationPermissionCheckResponse>())!;
        Assert.Equal(check.PersonId, v1Check.PersonId);
        Assert.Equal(check.IsAuthorizedAll, v1Check.IsAuthorizedAll);
        Assert.Equal(check.IsAuthorizedAny, v1Check.IsAuthorizedAny);
        Assert.Equal(check.Checks.Count, v1Check.Checks.Count);
    }

    [Fact]
    public async Task Integration_permission_check_returns_denied_for_inactive_person()
    {
        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/integrations/permission-check?tenantId={PlatformSeeder.DemoTenantId}&personId={_inactivePersonId}&permissionKey=maintainarr.work_order.close",
            _maintainarrPermissionCheckToken));
        response.EnsureSuccessStatusCode();

        var check = (await response.Content.ReadFromJsonAsync<IntegrationPermissionCheckResponse>())!;
        Assert.False(check.IsPersonActive);
        Assert.False(check.IsAuthorizedAll);
        Assert.False(check.IsAuthorizedAny);
        Assert.False(Assert.Single(check.Checks).Granted);
    }

    [Fact]
    public async Task Integration_permission_check_grants_materialized_projection_entries_without_sources()
    {
        var materializedPersonId = Guid.NewGuid();
        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            var projectionId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            db.People.Add(new StaffPerson
            {
                Id = materializedPersonId,
                TenantId = PlatformSeeder.DemoTenantId,
                ExternalUserId = Guid.NewGuid(),
                GivenName = "Materialized",
                FamilyName = "Projection",
                DisplayName = "Materialized Projection",
                PrimaryEmail = $"materialized.projection.{Guid.NewGuid():N}@demo.stl",
                EmploymentStatus = "active",
                CreatedAt = now,
                UpdatedAt = now
            });
            db.PersonPermissionProjections.Add(new PersonPermissionProjection
            {
                Id = projectionId,
                TenantId = PlatformSeeder.DemoTenantId,
                PersonId = materializedPersonId,
                PermissionCount = 1,
                ComputedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
                Entries =
                [
                    new PersonPermissionProjectionEntry
                    {
                        Id = Guid.NewGuid(),
                        TenantId = PlatformSeeder.DemoTenantId,
                        PersonId = materializedPersonId,
                        ProjectionId = projectionId,
                        PermissionKey = "maintainarr.work_order.materialized",
                        PermissionName = "Materialized Permission",
                        ScopeType = "tenant"
                    }
                ]
            });
            await db.SaveChangesAsync();
        }

        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/integrations/permission-check?tenantId={PlatformSeeder.DemoTenantId}&personId={materializedPersonId}&permissionKey=maintainarr.work_order.materialized",
            _maintainarrPermissionCheckToken));
        response.EnsureSuccessStatusCode();

        var check = (await response.Content.ReadFromJsonAsync<IntegrationPermissionCheckResponse>())!;
        var permissionCheck = Assert.Single(check.Checks);
        Assert.True(permissionCheck.Granted);
        Assert.Empty(permissionCheck.Grants);
        Assert.True(check.IsAuthorizedAll);
        Assert.True(check.IsAuthorizedAny);
    }

    [Fact]
    public async Task Integration_permission_check_rejects_unauthorized_source_product()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var staffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            ["staffarr"],
            IntegrationEndpoints.PermissionCheckReadActionScope);

        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/integrations/permission-check?tenantId={PlatformSeeder.DemoTenantId}&personId={_personId}&permissionKey=maintainarr.work_order.close",
            staffarrToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task V1_permissions_check_supports_user_scoped_access()
    {
        var userToken = CreateStaffArrAccessToken(_personId, "tenant_member");
        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/permissions/check?personId={_personId}&permissionKey=maintainarr.work_order.close&permissionKey=maintainarr.work_order.create",
            userToken));
        response.EnsureSuccessStatusCode();

        var check = (await response.Content.ReadFromJsonAsync<IntegrationPermissionCheckResponse>())!;
        Assert.Equal(_personId, check.PersonId);
        Assert.True(check.IsAuthorizedAny);
        Assert.False(check.IsAuthorizedAll);
    }

    [Fact]
    public async Task V1_permissions_check_denies_tenant_member_for_other_person()
    {
        var userToken = CreateStaffArrAccessToken(_personId, "tenant_member");
        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/permissions/check?personId={_inactivePersonId}&permissionKey=maintainarr.work_order.close",
            userToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Product_permission_catalog_sync_upserts_product_owned_permissions()
    {
        var syncRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/product-permission-catalog", _maintainarrPermissionCatalogSyncToken);
        syncRequest.Content = JsonContent.Create(new SyncProductPermissionCatalogRequest(
            PlatformSeeder.DemoTenantId,
            "maintainarr",
            [
                new ProductPermissionCatalogItemRequest(
                    "maintainarr.work_order.release",
                    "Release Work Order",
                    "Allows a MaintainArr user to release a work order for execution.",
                    "site",
                    "sensitive"),
                new ProductPermissionCatalogItemRequest(
                    "maintainarr.asset.dispatchability.override",
                    "Override Asset Dispatchability",
                    "Allows a supervisor to override a blocked asset dispatchability result.",
                    "tenant",
                    "critical")
            ]));

        var syncResponse = await _staffarrClient.SendAsync(syncRequest);
        syncResponse.EnsureSuccessStatusCode();
        var synced = (await syncResponse.Content.ReadFromJsonAsync<SyncProductPermissionCatalogResponse>())!;
        Assert.Equal("maintainarr", synced.ProductKey);
        Assert.Equal(2, synced.UpsertedCount);
        Assert.All(synced.Permissions, item =>
        {
            Assert.Equal("maintainarr", item.ProductKey);
            Assert.StartsWith("maintainarr.", item.PermissionKey, StringComparison.Ordinal);
            Assert.NotEqual(Guid.Empty, item.PermissionTemplateId);
        });

        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            var catalog = await db.PermissionTemplates.AsNoTracking()
                .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.ProductKey == "maintainarr")
                .ToListAsync();
            Assert.Contains(catalog, x =>
                x.PermissionKey == "maintainarr.work_order.release"
                && x.PermissionScope == "site"
                && x.Sensitivity == "sensitive");
        }

        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            Assert.Contains(await db.PermissionTemplates.AsNoTracking().ToListAsync(), x =>
                x.PermissionKey == "maintainarr.asset.dispatchability.override"
                && x.ProductKey == "maintainarr"
                && x.Sensitivity == "critical"
                && x.LastSyncedAt is not null);
        }

        var permissionCatalogResponse = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            "/api/v1/permissions/catalog",
            CreateStaffArrAccessToken(_personId, "tenant_admin", ["nexarr"])));
        permissionCatalogResponse.EnsureSuccessStatusCode();
        var staffCatalog = (await permissionCatalogResponse.Content.ReadFromJsonAsync<IReadOnlyList<PermissionCatalogResponse>>())!;
        Assert.Contains(staffCatalog, catalog => catalog.ProductKey == "maintainarr");
        Assert.Contains(
            staffCatalog.SelectMany(catalog => catalog.Modules).SelectMany(module => module.PermissionGroups).SelectMany(group => group.Permissions),
            x => x.Key == "staffarr.permissions.assign"
                && x.Label == "Manage role permissions"
                && x.Description == "Assign permission templates to roles and role scopes; people inherit access through role assignments only.");
    }

    [Fact]
    public async Task Product_permission_catalog_sync_upserts_loadarr_permissions()
    {
        var syncRequest = Authorized(
            HttpMethod.Post,
            "/api/v1/integrations/product-permission-catalog",
            _loadarrPermissionCatalogSyncToken);
        syncRequest.Content = JsonContent.Create(new SyncProductPermissionCatalogRequest(
            PlatformSeeder.DemoTenantId,
            "loadarr",
            StlLoadArrPermissionCatalog.All.Select(item => new ProductPermissionCatalogItemRequest(
                item.PermissionKey,
                item.Label,
                item.Description,
                item.Scope,
                item.Sensitivity,
                item.Status)).ToList()));

        var syncResponse = await _staffarrClient.SendAsync(syncRequest);
        syncResponse.EnsureSuccessStatusCode();
        var synced = (await syncResponse.Content.ReadFromJsonAsync<SyncProductPermissionCatalogResponse>())!;
        Assert.Equal("loadarr", synced.ProductKey);
        Assert.Equal(StlLoadArrPermissionCatalog.All.Count, synced.UpsertedCount);
        Assert.Contains(synced.Permissions, item =>
            item.PermissionKey == "loadarr.dashboard.read"
            && item.ProductKey == "loadarr"
            && item.Scope == "product");

        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            var catalog = await db.PermissionTemplates.AsNoTracking()
                .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.ProductKey == "loadarr")
                .ToListAsync();
            Assert.Contains(catalog, item =>
                item.PermissionKey == "loadarr.permissions.manage"
                && item.PermissionScope == "tenant"
                && item.Sensitivity == "critical");
        }
    }

    [Fact]
    public async Task Integration_sites_read_returns_active_sites_and_rejects_inactive_site_detail()
    {
        var listResponse = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/integrations/sites?tenantId={PlatformSeeder.DemoTenantId}&includeArchived=false",
            _maintainarrSitesReadToken));
        listResponse.EnsureSuccessStatusCode();
        var sites = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<StaffArrSiteLookupResponse>>())!;
        Assert.Contains(sites, x =>
            x.OrgUnitId == _activeSiteOrgUnitId
            && x.Name == "Central Yard"
            && x.Status == "active");
        Assert.DoesNotContain(sites, x => x.OrgUnitId == _inactiveSiteOrgUnitId);

        var detailResponse = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/integrations/sites/{_activeSiteOrgUnitId}?tenantId={PlatformSeeder.DemoTenantId}&includeArchived=false",
            _maintainarrSitesReadToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<StaffArrSiteLookupResponse>())!;
        Assert.Equal(_activeSiteOrgUnitId, detail.OrgUnitId);
        Assert.Equal("Central Yard", detail.Name);

        var inactiveResponse = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/integrations/sites/{_inactiveSiteOrgUnitId}?tenantId={PlatformSeeder.DemoTenantId}&includeArchived=false",
            _maintainarrSitesReadToken));
        Assert.Equal(HttpStatusCode.NotFound, inactiveResponse.StatusCode);
    }

    private async Task<(Guid ActivePersonId, Guid InactivePersonId, Guid ActiveSiteOrgUnitId, Guid InactiveSiteOrgUnitId)> SeedPeopleAsync(StaffArrDbContext db)
    {
        var activePersonId = Guid.NewGuid();
        var inactivePersonId = Guid.NewGuid();
        var activeSiteOrgUnitId = Guid.NewGuid();
        var inactiveSiteOrgUnitId = Guid.NewGuid();
        db.People.AddRange(
            new StaffPerson
            {
                Id = activePersonId,
                TenantId = PlatformSeeder.DemoTenantId,
                ExternalUserId = PlatformSeeder.DemoAdminUserId,
                GivenName = "Integration",
                FamilyName = "Allowed",
                DisplayName = "Integration Allowed",
                PrimaryEmail = "integration.allowed@demo.stl",
                EmploymentStatus = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            new StaffPerson
            {
                Id = inactivePersonId,
                TenantId = PlatformSeeder.DemoTenantId,
                ExternalUserId = Guid.NewGuid(),
                GivenName = "Integration",
                FamilyName = "Inactive",
                DisplayName = "Integration Inactive",
                PrimaryEmail = "integration.inactive@demo.stl",
                EmploymentStatus = "inactive",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });

        db.OrgUnits.AddRange(
            new OrgUnit
            {
                Id = activeSiteOrgUnitId,
                TenantId = PlatformSeeder.DemoTenantId,
                UnitType = "site",
                Name = "Central Yard",
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            new OrgUnit
            {
                Id = inactiveSiteOrgUnitId,
                TenantId = PlatformSeeder.DemoTenantId,
                UnitType = "site",
                Name = "Retired Yard",
                Status = "archived",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });

        var roleId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        db.StaffRoles.Add(new StaffRole
        {
            Id = roleId,
            TenantId = PlatformSeeder.DemoTenantId,
            Name = "MaintainArr Supervisor",
            Description = "MaintainArr permission-check seed role.",
            RoleType = "tenant_role",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.StaffRolePermissions.Add(new StaffRolePermission
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            RoleId = roleId,
            ProductKey = "maintainarr",
            PermissionKey = "maintainarr.work_order.close",
            Effect = "allow",
            CreatedAt = now,
        });

        db.StaffPersonRoles.Add(new StaffPersonRole
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = activePersonId,
            RoleId = roleId,
            AssignmentScopeType = "tenant",
            AssignedByPersonId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
        });

        await db.SaveChangesAsync();
        return (activePersonId, inactivePersonId, activeSiteOrgUnitId, inactiveSiteOrgUnitId);
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
            $"{sourceProduct}-permission-check-{Guid.NewGuid():N}",
            "permission check test",
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

    private string CreateStaffArrAccessToken(
        Guid personId,
        string tenantRoleKey,
        IReadOnlyList<string>? launchableProductKeys = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId,
            PlatformSeeder.DemoAdminEmail,
            "StaffArr Test User",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            launchableProductKeys ?? ["staffarr"],
            isPlatformAdmin: false);
        return accessToken;
    }
}
