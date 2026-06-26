using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrTenantSettingsTests : IAsyncLifetime
{
    private WebApplicationFactory<global::TrainArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"TrainArrTenantSettings-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Tenant_settings_get_creates_default_profile()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/tenant-settings/trainarr", adminToken));

        response.EnsureSuccessStatusCode();
        var settings = (await response.Content.ReadFromJsonAsync<TrainArrTenantSettingsResponse>())!;
        Assert.Equal("trainarr", settings.ProductKey);
        Assert.Equal("tenant", settings.Scope);
        Assert.Equal(14, settings.Settings.Assignment.DefaultAssignmentDueDays);
        Assert.Equal("expired_or_incomplete", settings.Settings.ProgramVersioning.ProgramVersionChangePolicy);
        Assert.Equal([90, 60, 30, 14, 7, 1], settings.Settings.Certifications.DefaultExpirationWarningDays);
        Assert.Equal(1, settings.RowVersion);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        Assert.True(await db.TrainArrTenantSettings.AnyAsync(x => x.TenantId == PlatformSeeder.DemoTenantId));
    }

    [Fact]
    public async Task Tenant_settings_allows_manager_read_but_denies_manager_write()
    {
        var managerToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_manager");

        var readResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/tenant-settings/trainarr", managerToken));
        readResponse.EnsureSuccessStatusCode();

        var current = (await readResponse.Content.ReadFromJsonAsync<TrainArrTenantSettingsResponse>())!;
        var writeResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Put,
                "/api/v1/tenant-settings/trainarr",
                managerToken,
                new UpdateTrainArrTenantSettingsRequest(current.Settings, current.RowVersion)));

        Assert.Equal(HttpStatusCode.Forbidden, writeResponse.StatusCode);
    }

    [Fact]
    public async Task Tenant_settings_denies_trainee_read_and_write()
    {
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");
        var defaults = TrainArrTenantSettingsService.CreateDefaultPayload();

        var readResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/tenant-settings/trainarr", memberToken));
        var writeResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Put,
                "/api/v1/tenant-settings/trainarr",
                memberToken,
                new UpdateTrainArrTenantSettingsRequest(defaults, null)));

        Assert.Equal(HttpStatusCode.Forbidden, readResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, writeResponse.StatusCode);
    }

    [Fact]
    public async Task Tenant_settings_rejects_platform_admin_without_trainarr_role()
    {
        var platformAdminToken = CreateTrainArrAccessToken(
            ["trainarr"],
            tenantRoleKey: "routarr_driver",
            isPlatformAdmin: true);
        var defaults = TrainArrTenantSettingsService.CreateDefaultPayload();

        var readResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/tenant-settings/trainarr", platformAdminToken));
        var writeResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Put,
                "/api/v1/tenant-settings/trainarr",
                platformAdminToken,
                new UpdateTrainArrTenantSettingsRequest(defaults, null)));

        Assert.Equal(HttpStatusCode.Forbidden, readResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, writeResponse.StatusCode);
    }

    [Fact]
    public async Task Tenant_settings_put_persists_audits_and_enqueues_update_event()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var current = await GetCurrentAsync(adminToken);
        var updated = current.Settings with
        {
            Assignment = current.Settings.Assignment with { DefaultAssignmentDueDays = 21 },
            Notifications = current.Settings.Notifications with { DueSoonReminderDays = [21, 7, 7, 1] }
        };

        var putResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Put,
                "/api/v1/tenant-settings/trainarr",
                adminToken,
                new UpdateTrainArrTenantSettingsRequest(updated, current.RowVersion)));

        putResponse.EnsureSuccessStatusCode();
        var saved = (await putResponse.Content.ReadFromJsonAsync<TrainArrTenantSettingsResponse>())!;
        Assert.Equal(21, saved.Settings.Assignment.DefaultAssignmentDueDays);
        Assert.Equal([21, 7, 1], saved.Settings.Notifications.DueSoonReminderDays);
        Assert.True(saved.RowVersion > current.RowVersion);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        Assert.True(await db.AuditEvents.AnyAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.Action == "trainarr.tenant_settings.update"
                && x.TargetType == "trainarr_tenant_settings"));
        Assert.True(await db.TrainingDomainEvents.AnyAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.EventKind == TrainingDomainEventKinds.TenantSettingsUpdated
                && x.RelatedEntityType == "trainarr_tenant_settings"));
    }

    [Fact]
    public async Task Tenant_settings_put_rejects_stale_row_version()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var current = await GetCurrentAsync(adminToken);

        var first = current.Settings with
        {
            Assignment = current.Settings.Assignment with { DefaultAssignmentDueDays = 17 }
        };
        var firstResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Put,
                "/api/v1/tenant-settings/trainarr",
                adminToken,
                new UpdateTrainArrTenantSettingsRequest(first, current.RowVersion)));
        firstResponse.EnsureSuccessStatusCode();

        var stale = current.Settings with
        {
            Assignment = current.Settings.Assignment with { DefaultAssignmentDueDays = 18 }
        };
        var staleResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Put,
                "/api/v1/tenant-settings/trainarr",
                adminToken,
                new UpdateTrainArrTenantSettingsRequest(stale, current.RowVersion)));

        Assert.Equal(HttpStatusCode.Conflict, staleResponse.StatusCode);
    }

    [Fact]
    public async Task Tenant_settings_patch_updates_one_group()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var current = await GetCurrentAsync(adminToken);
        using var patchDocument = System.Text.Json.JsonDocument.Parse(
            """{"assignment":{"defaultAssignmentDueDays":18}}""");

        var patchResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Patch,
                "/api/v1/tenant-settings/trainarr",
                adminToken,
                new PatchTrainArrTenantSettingsRequest(
                    current.RowVersion,
                    patchDocument.RootElement.Clone())));

        patchResponse.EnsureSuccessStatusCode();
        var saved = (await patchResponse.Content.ReadFromJsonAsync<TrainArrTenantSettingsResponse>())!;
        Assert.Equal(18, saved.Settings.Assignment.DefaultAssignmentDueDays);
        Assert.Equal(current.Settings.Assignment.AssignmentPriorityDefault, saved.Settings.Assignment.AssignmentPriorityDefault);
    }

    [Fact]
    public async Task Tenant_settings_rejects_invalid_payload()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var current = await GetCurrentAsync(adminToken);
        var invalid = current.Settings with
        {
            EvidenceRecords = current.Settings.EvidenceRecords with { MaxEvidenceFileSizeMb = 0 }
        };

        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Put,
                "/api/v1/tenant-settings/trainarr",
                adminToken,
                new UpdateTrainArrTenantSettingsRequest(invalid, current.RowVersion)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void Tenant_settings_validation_normalizes_arrays_and_rejects_impossible_compliance_core_posture()
    {
        var defaults = TrainArrTenantSettingsService.CreateDefaultPayload();
        var normalized = TrainArrTenantSettingsService.NormalizeAndValidate(defaults with
        {
            Notifications = defaults.Notifications with { DueSoonReminderDays = [1, 14, 7, 14] },
            EvidenceRecords = defaults.EvidenceRecords with { AllowedEvidenceTypes = ["image", "pdf", "image"] }
        });

        Assert.Equal([14, 7, 1], normalized.Notifications.DueSoonReminderDays);
        Assert.Equal(["image", "pdf"], normalized.EvidenceRecords.AllowedEvidenceTypes);

        var invalid = defaults with
        {
            ComplianceCore = defaults.ComplianceCore with
            {
                RequireComplianceCoreProgramMapping = true,
                AllowUnmappedInternalPrograms = false
            }
        };
        Assert.Throws<StlApiException>(() => TrainArrTenantSettingsService.NormalizeAndValidate(invalid));
    }

    private async Task<TrainArrTenantSettingsResponse> GetCurrentAsync(string accessToken)
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/tenant-settings/trainarr", accessToken));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TrainArrTenantSettingsResponse>())!;
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey,
        bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin);

        return accessToken;
    }

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
}
