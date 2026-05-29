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

public sealed class StaffArrWorkerAdminTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;
    private string _adminToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrWorkerAdmin-{Guid.NewGuid():N}";

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
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Worker_admin_settings_default_then_upsert_readiness_rollup()
    {
        var getResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/worker-admin/readiness-rollup/settings", _adminToken));
        getResponse.EnsureSuccessStatusCode();
        var defaults = (await getResponse.Content.ReadFromJsonAsync<StaffArrWorkerSettingsResponse>())!;
        Assert.False(defaults.IsEnabled);
        Assert.Equal("readiness-rollup", defaults.WorkerKey);

        var putResponse = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Put,
                "/api/worker-admin/readiness-rollup/settings",
                _adminToken,
                new UpsertStaffArrWorkerSettingsRequest(true, 45, 25, 2)));
        putResponse.EnsureSuccessStatusCode();
        var saved = (await putResponse.Content.ReadFromJsonAsync<StaffArrWorkerSettingsResponse>())!;
        Assert.True(saved.IsEnabled);
        Assert.Equal(45, saved.ScanIntervalMinutes);
        Assert.Equal(25, saved.BatchSize);
        Assert.Equal(2, saved.StalenessHours);
    }

    [Fact]
    public async Task Worker_admin_pending_preview_returns_empty_or_list_for_certification_expiration()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/worker-admin/certification-expiration/pending", _adminToken));
        response.EnsureSuccessStatusCode();

        var pending = (await response.Content.ReadFromJsonAsync<StaffArrWorkerPendingPreviewResponse>())!;
        Assert.Equal("certification-expiration", pending.WorkerKey);
        Assert.True(pending.ItemCount >= 0);
    }

    [Fact]
    public async Task Export_delivery_runs_list_returns_empty_when_none_recorded()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export/delivery-runs", _adminToken));
        response.EnsureSuccessStatusCode();

        var runs = (await response.Content.ReadFromJsonAsync<PersonExportDeliveryRunsResponse>())!;
        Assert.Empty(runs.Items);
    }

    [Fact]
    public async Task Supervisor_cannot_manage_worker_admin_settings()
    {
        var supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/worker-admin/permission-projection/settings", supervisorToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
