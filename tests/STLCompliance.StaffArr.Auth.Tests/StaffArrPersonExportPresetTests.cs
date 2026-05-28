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

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrPersonExportPresetTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrPersonExportPreset-{Guid.NewGuid():N}";

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
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
    public async Task People_export_preset_get_returns_not_found_when_unconfigured()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export/preset", token));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task People_export_preset_put_and_get_round_trip()
    {
        var northSiteId = Guid.NewGuid();
        await SeedOrgUnitAsync(northSiteId, "site", "North Site");

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var upsert = new UpsertPersonExportPresetRequest("active", northSiteId, "active-at-org-unit");

        var putResponse = await _staffarrClient.SendAsync(
            AuthorizedJson(HttpMethod.Put, "/api/people/export/preset", token, upsert));
        putResponse.EnsureSuccessStatusCode();
        var saved = (await putResponse.Content.ReadFromJsonAsync<PersonExportPresetResponse>())!;
        Assert.Equal("active", saved.EmploymentStatus);
        Assert.Equal(northSiteId, saved.OrgUnitId);
        Assert.Equal("active-at-org-unit", saved.PresetKey);

        var getResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export/preset", token));
        getResponse.EnsureSuccessStatusCode();
        var loaded = (await getResponse.Content.ReadFromJsonAsync<PersonExportPresetResponse>())!;
        Assert.Equal(saved.EmploymentStatus, loaded.EmploymentStatus);
        Assert.Equal(saved.OrgUnitId, loaded.OrgUnitId);
        Assert.Equal(saved.PresetKey, loaded.PresetKey);
    }

    [Fact]
    public async Task People_export_preset_put_rejects_invalid_employment_status()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var upsert = new UpsertPersonExportPresetRequest("on_leave", null, null);

        var response = await _staffarrClient.SendAsync(
            AuthorizedJson(HttpMethod.Put, "/api/people/export/preset", token, upsert));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task People_export_preset_put_rejects_unknown_org_unit()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var upsert = new UpsertPersonExportPresetRequest("active", Guid.NewGuid(), null);

        var response = await _staffarrClient.SendAsync(
            AuthorizedJson(HttpMethod.Put, "/api/people/export/preset", token, upsert));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task People_export_preset_put_requires_org_unit_for_active_at_org_unit_preset()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var upsert = new UpsertPersonExportPresetRequest("active", null, "active-at-org-unit");

        var response = await _staffarrClient.SendAsync(
            AuthorizedJson(HttpMethod.Put, "/api/people/export/preset", token, upsert));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task People_export_preset_put_writes_audit_event()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var upsert = new UpsertPersonExportPresetRequest("inactive", null, "inactive-records");

        var response = await _staffarrClient.SendAsync(
            AuthorizedJson(HttpMethod.Put, "/api/people/export/preset", token, upsert));
        response.EnsureSuccessStatusCode();

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditEvents = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "person.export_preset.update");
        Assert.Equal(1, auditEvents);
    }

    [Fact]
    public async Task People_export_preset_denied_for_non_writer_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var getResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export/preset", token));
        Assert.Equal(HttpStatusCode.Forbidden, getResponse.StatusCode);

        var putResponse = await _staffarrClient.SendAsync(
            AuthorizedJson(
                HttpMethod.Put,
                "/api/people/export/preset",
                token,
                new UpsertPersonExportPresetRequest("active", null, "active-workforce")));
        Assert.Equal(HttpStatusCode.Forbidden, putResponse.StatusCode);
    }

    private async Task SeedOrgUnitAsync(Guid orgUnitId, string unitType, string name)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.OrgUnits.Add(new OrgUnit
        {
            Id = orgUnitId,
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = unitType,
            Name = name,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
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

    private static HttpRequestMessage AuthorizedJson<T>(HttpMethod method, string url, string accessToken, T body)
    {
        var request = Authorized(method, url, accessToken);
        request.Content = JsonContent.Create(body);
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
}
