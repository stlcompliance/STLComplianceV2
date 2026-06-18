using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrTenantSettingsTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;
    private string _adminToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrTenantSettings-{Guid.NewGuid():N}";

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
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
        _adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "staffarr_admin");

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Tenant_settings_get_creates_defaults_and_put_audits_before_after()
    {
        var getResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/staffarr/tenant-settings", _adminToken));
        getResponse.EnsureSuccessStatusCode();
        var defaults = (await getResponse.Content.ReadFromJsonAsync<StaffArrTenantSettingsResponse>())!;
        Assert.Equal("preferred_first_last", defaults.PersonDirectory.DisplayNameFormat);
        Assert.True(defaults.OrgStructure.PreventCircularReporting);

        var request = ToUpsert(defaults) with
        {
            PersonDirectory = defaults.PersonDirectory with
            {
                DisplayNameFormat = "last_first",
                EmployeeNumberRequired = true
            },
            OrgStructure = defaults.OrgStructure with
            {
                PreventCircularReporting = false
            }
        };

        var putResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Put, "/api/v1/staffarr/tenant-settings", _adminToken, request));
        putResponse.EnsureSuccessStatusCode();
        var saved = (await putResponse.Content.ReadFromJsonAsync<StaffArrTenantSettingsResponse>())!;
        Assert.Equal("last_first", saved.PersonDirectory.DisplayNameFormat);
        Assert.True(saved.PersonDirectory.EmployeeNumberRequired);
        Assert.True(saved.OrgStructure.PreventCircularReporting);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        Assert.Equal(1, await db.TenantSettings.CountAsync(x => x.TenantId == PlatformSeeder.DemoTenantId));
        var auditEvent = await db.AuditEvents.SingleAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "staffarr.tenant_settings.update");
        Assert.Contains("before", auditEvent.MetadataJson);
        Assert.Contains("after", auditEvent.MetadataJson);
    }

    [Fact]
    public async Task Tenant_settings_are_isolated_by_tenant()
    {
        var firstDefaults = await GetTenantSettingsAsync(_adminToken);
        var firstRequest = ToUpsert(firstDefaults) with
        {
            PersonDirectory = firstDefaults.PersonDirectory with { DisplayNameFormat = "last_first" }
        };
        var putResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Put, "/api/v1/staffarr/tenant-settings", _adminToken, firstRequest));
        putResponse.EnsureSuccessStatusCode();

        var secondTenantId = Guid.NewGuid();
        var secondTenantToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "staffarr_admin",
            tenantId: secondTenantId);
        var secondDefaults = await GetTenantSettingsAsync(secondTenantToken);
        Assert.Equal("preferred_first_last", secondDefaults.PersonDirectory.DisplayNameFormat);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        Assert.Equal(2, await db.TenantSettings.CountAsync());
        Assert.True(await db.TenantSettings.AnyAsync(x => x.TenantId == PlatformSeeder.DemoTenantId));
        Assert.True(await db.TenantSettings.AnyAsync(x => x.TenantId == secondTenantId));
    }

    [Fact]
    public async Task Tenant_settings_reject_invalid_values()
    {
        var defaults = await GetTenantSettingsAsync(_adminToken);
        var request = ToUpsert(defaults) with
        {
            PersonDirectory = defaults.PersonDirectory with
            {
                EmployeeNumberUniquenessScope = "global"
            }
        };

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Put, "/api/v1/staffarr/tenant-settings", _adminToken, request));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Non_admin_users_cannot_edit_tenant_settings()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var defaults = await GetTenantSettingsAsync(_adminToken);

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Put, "/api/v1/staffarr/tenant-settings", supervisorToken, ToUpsert(defaults)));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Lifecycle_settings_affect_person_activation_validation()
    {
        var defaults = await GetTenantSettingsAsync(_adminToken);
        var request = ToUpsert(defaults) with
        {
            PersonLifecycle = defaults.PersonLifecycle with
            {
                RequireManagerBeforeActivation = true
            }
        };
        var putResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Put, "/api/v1/staffarr/tenant-settings", _adminToken, request));
        putResponse.EnsureSuccessStatusCode();

        var createPersonResponse = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Post,
                "/api/people",
                _adminToken,
                new CreateStaffPersonRequest(
                    "active.no.manager@example.com",
                    LegalFirstName: "Active",
                    LegalLastName: "No Manager",
                    EmploymentStatus: "active")));

        Assert.Equal(HttpStatusCode.Conflict, createPersonResponse.StatusCode);
    }

    [Fact]
    public async Task Role_settings_affect_role_assignment_validation()
    {
        var options = new DbContextOptionsBuilder<StaffArrDbContext>()
            .UseInMemoryDatabase($"staffarr-role-settings-{Guid.NewGuid():N}")
            .Options;
        await using var db = new StaffArrDbContext(options);
        var audit = new NoOpStaffArrAuditService();
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        db.TenantSettings.Add(new StaffArrTenantSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequireAssignmentReason = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = tenantId,
            GivenName = "Role",
            FamilyName = "Target",
            DisplayName = "Role Target",
            PrimaryEmail = "role.target@example.com",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        });
        db.StaffRoles.Add(new StaffRole
        {
            Id = roleId,
            TenantId = tenantId,
            Name = "Local manager",
            RoleType = "tenant_role",
            CreatedAt = now,
            UpdatedAt = now
        });
        await db.SaveChangesAsync();

        var service = new RoleManagementService(db, audit, new StaffArrTenantSettingsService(db, audit));
        var exception = await Assert.ThrowsAsync<StlApiException>(() =>
            service.SetPersonRolesAsync(
                tenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                personId,
                new SetStaffPersonRolesRequest(
                    [new SetStaffPersonRoleItemRequest(roleId, "tenant", null, null, null)])));

        Assert.Equal("staff_role.assignment_reason_required", exception.Code);
    }

    [Fact]
    public async Task Location_settings_affect_location_validation()
    {
        var options = new DbContextOptionsBuilder<StaffArrDbContext>()
            .UseInMemoryDatabase($"staffarr-location-settings-{Guid.NewGuid():N}")
            .Options;
        await using var db = new StaffArrDbContext(options);
        var audit = new NoOpStaffArrAuditService();
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        db.TenantSettings.Add(new StaffArrTenantSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequireLocationCode = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        await db.SaveChangesAsync();

        var service = new InternalLocationService(db, audit, new StaffArrTenantSettingsService(db, audit));
        var exception = await Assert.ThrowsAsync<StlApiException>(() =>
            service.CreateAsync(
                tenantId,
                Guid.NewGuid(),
                new CreateInternalLocationRequest(
                    "Main warehouse",
                    "warehouse",
                    ParentLocationId: null,
                    SiteOrgUnitId: null)));

        Assert.Equal("location.validation", exception.Code);
        Assert.Contains("Location code is required", exception.Message);
    }

    private async Task<StaffArrTenantSettingsResponse> GetTenantSettingsAsync(string token)
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/staffarr/tenant-settings", token));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StaffArrTenantSettingsResponse>())!;
    }

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null,
        Guid? tenantId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            tenantId ?? PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private static UpsertStaffArrTenantSettingsRequest ToUpsert(StaffArrTenantSettingsResponse settings) =>
        new(
            settings.PersonDirectory,
            settings.PersonLifecycle,
            settings.OrgStructure,
            settings.LocationHierarchy,
            settings.RolePermissions,
            settings.TeamsAssignments,
            settings.Incidents,
            settings.ProfileFieldGovernance,
            settings.NotificationsReviews,
            settings.DataGovernanceAudit,
            settings.CrossProductReferences);

    private static HttpRequestMessage Authorized(
        HttpMethod method,
        string url,
        string accessToken,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
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

    private sealed class NoOpStaffArrAuditService : IStaffArrAuditService
    {
        public Task<StaffArrAuditWriteResult> WriteAsync(
            string action,
            Guid tenantId,
            Guid? actorUserId,
            string targetType,
            string? targetId,
            string result,
            string? reasonCode = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new StaffArrAuditWriteResult(Guid.NewGuid(), DateTimeOffset.UtcNow));

        public Task<StaffArrAuditWriteResult> WriteWithMetadataAsync(
            string action,
            Guid tenantId,
            Guid? actorUserId,
            string targetType,
            string? targetId,
            string result,
            string? metadataJson,
            string? reasonCode = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new StaffArrAuditWriteResult(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }
}
