using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
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

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrPersonExportDeliveryWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _sharedWorkerToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PersonExportDeliveryNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"PersonExportDeliveryStaffArr-{Guid.NewGuid():N}";

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
            PersonExportDeliveryService.ProcessDeliveriesActionScope);

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
            "/api/internal/person-export-deliveries/process-batch",
            new ProcessPersonExportDeliveriesRequest(PlatformSeeder.DemoTenantId, null, 10));
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
            PersonExportDeliveryService.ProcessDeliveriesActionScope);

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/person-export-deliveries/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPersonExportDeliveriesRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));

        var response = await _staffarrClient.SendAsync(processRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_returns_enabled_schedule_before_processing()
    {
        await SeedEnabledScheduleAsync();

        var pendingRequest = Authorized(
            HttpMethod.Get,
            $"/api/internal/person-export-deliveries/pending?tenantId={PlatformSeeder.DemoTenantId}",
            _sharedWorkerToStaffarrToken);
        var pendingResponse = await _staffarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingPersonExportDeliveriesResponse>())!;
        Assert.Single(pending.Items);
        Assert.Equal(PlatformSeeder.DemoTenantId, pending.Items[0].TenantId);
    }

    [Fact]
    public async Task Process_batch_delivers_export_and_records_run()
    {
        await SeedPersonAsync(Guid.NewGuid(), "Scheduled", "Export", "scheduled.export@example.com");
        await SeedEnabledScheduleAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/person-export-deliveries/process-batch",
            _sharedWorkerToStaffarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPersonExportDeliveriesRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));

        var processResponse = await _staffarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var result = (await processResponse.Content.ReadFromJsonAsync<ProcessPersonExportDeliveriesResponse>())!;
        Assert.Equal(1, result.DeliveredCount);
        Assert.Equal(1, result.Deliveries[0].PersonCount);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        Assert.Equal(1, await db.PersonExportDeliveryRuns.CountAsync(x => x.TenantId == PlatformSeeder.DemoTenantId));
        var schedule = await db.TenantPersonExportSchedules.SingleAsync(x => x.TenantId == PlatformSeeder.DemoTenantId);
        Assert.NotNull(schedule.LastDeliveredAt);
        Assert.Equal(1, await db.AuditEvents.CountAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "person.export.scheduled_delivery"));
    }

    [Fact]
    public async Task Process_batch_skips_recently_delivered_schedule_until_interval_elapses()
    {
        await SeedEnabledScheduleAsync(lastDeliveredAt: DateTimeOffset.UtcNow.AddHours(-1));

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/person-export-deliveries/process-batch",
            _sharedWorkerToStaffarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPersonExportDeliveriesRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));

        var processResponse = await _staffarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var result = (await processResponse.Content.ReadFromJsonAsync<ProcessPersonExportDeliveriesResponse>())!;
        Assert.Equal(0, result.CandidatesFound);
        Assert.Equal(0, result.DeliveredCount);
    }

    [Fact]
    public async Task Export_schedule_put_and_get_round_trip()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var upsertRequest = Authorized(HttpMethod.Put, "/api/people/export/schedule", token);
        upsertRequest.Content = JsonContent.Create(new UpsertPersonExportScheduleRequest(true, 12));

        var putResponse = await _staffarrClient.SendAsync(upsertRequest);
        putResponse.EnsureSuccessStatusCode();
        var saved = (await putResponse.Content.ReadFromJsonAsync<PersonExportScheduleResponse>())!;
        Assert.True(saved.IsEnabled);
        Assert.Equal(12, saved.IntervalHours);

        var getResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export/schedule", token));
        getResponse.EnsureSuccessStatusCode();
        var loaded = (await getResponse.Content.ReadFromJsonAsync<PersonExportScheduleResponse>())!;
        Assert.True(loaded.IsEnabled);
        Assert.Equal(12, loaded.IntervalHours);
    }

    private async Task SeedEnabledScheduleAsync(DateTimeOffset? lastDeliveredAt = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantPersonExportSchedules.Add(new TenantPersonExportSchedule
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            IntervalHours = 24,
            LastDeliveredAt = lastDeliveredAt,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedPersonAsync(Guid personId, string givenName, string familyName, string email)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = givenName,
            FamilyName = familyName,
            DisplayName = $"{givenName} {familyName}",
            PrimaryEmail = email,
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-export-delivery-{Guid.NewGuid():N}",
            $"{sourceProduct} export delivery test",
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
