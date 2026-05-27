using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrPmDueScanWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _sharedWorkerToMaintainArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PmDueScanNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"PmDueScanMaintainArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToMaintainArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["maintainarr"],
            PmDueScanService.ProcessDueScanActionScope);

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_due_scan_rejects_missing_service_token()
    {
        var response = await _maintainarrClient.PostAsJsonAsync(
            "/api/internal/pm/process-due-scan",
            new ProcessPmDueScanRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 50, 1));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_due_scan_marks_past_due_schedule_as_due()
    {
        var schedule = await SeedPastDuePmScheduleAsync(daysPastDue: 0);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var processResponse = await _maintainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessPmDueScanResponse>())!;
        Assert.Equal(1, body.MarkedDueCount);
        Assert.Contains(schedule.Id, body.UpdatedPmScheduleIds);
        Assert.Equal(1, body.WorkOrdersCreatedCount);
        Assert.Single(body.CreatedWorkOrderIds);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var stored = await db.PmSchedules.SingleAsync(x => x.Id == schedule.Id);
        Assert.Equal(PmDueStatuses.Due, stored.DueStatus);
        Assert.NotNull(stored.LastDueScanAt);

        var workOrder = await db.WorkOrders.SingleAsync(x => x.PmScheduleId == schedule.Id);
        Assert.Equal(WorkOrderSources.PmSchedule, workOrder.Source);
        Assert.Equal(WorkOrderStatuses.Open, workOrder.Status);
        Assert.StartsWith("PM:", workOrder.Title);
    }

    [Fact]
    public async Task Process_due_scan_work_order_generation_is_idempotent()
    {
        var schedule = await SeedPastDuePmScheduleAsync(daysPastDue: 0);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var firstResponse = await _maintainarrClient.SendAsync(processRequest);
        firstResponse.EnsureSuccessStatusCode();
        var firstBody = (await firstResponse.Content.ReadFromJsonAsync<ProcessPmDueScanResponse>())!;
        Assert.Equal(1, firstBody.WorkOrdersCreatedCount);

        var secondRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        secondRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        secondRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var secondResponse = await _maintainarrClient.SendAsync(secondRequest);
        secondResponse.EnsureSuccessStatusCode();
        var secondBody = (await secondResponse.Content.ReadFromJsonAsync<ProcessPmDueScanResponse>())!;
        Assert.Equal(0, secondBody.WorkOrdersCreatedCount);
        Assert.Equal(1, secondBody.WorkOrdersLinkedCount);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        Assert.Equal(1, await db.WorkOrders.CountAsync(x => x.PmScheduleId == schedule.Id));
    }

    [Fact]
    public async Task Due_list_includes_linked_work_order()
    {
        var schedule = await SeedPastDuePmScheduleAsync();

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));
        await _maintainarrClient.SendAsync(processRequest);

        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var dueRequest = Authorized(HttpMethod.Get, "/api/preventive-maintenance/due", token);
        var dueResponse = await _maintainarrClient.SendAsync(dueRequest);
        dueResponse.EnsureSuccessStatusCode();
        var dueItems = (await dueResponse.Content.ReadFromJsonAsync<List<PmScheduleResponse>>())!;

        var dueItem = dueItems.Single(x => x.PmScheduleId == schedule.Id);
        Assert.NotNull(dueItem.LinkedWorkOrderId);
        Assert.False(string.IsNullOrWhiteSpace(dueItem.LinkedWorkOrderNumber));
        Assert.Equal(WorkOrderStatuses.Open, dueItem.LinkedWorkOrderStatus);
    }

    [Fact]
    public async Task List_pending_due_returns_candidates_before_processing()
    {
        var schedule = await SeedPastDuePmScheduleAsync();

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/pm/pending-due?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);

        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingPmDueResponse>())!;
        Assert.Contains(pending.Items, x => x.PmScheduleId == schedule.Id);
    }

    [Fact]
    public async Task Due_list_returns_marked_due_schedules_for_managers()
    {
        var schedule = await SeedPastDuePmScheduleAsync();

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));
        await _maintainarrClient.SendAsync(processRequest);

        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var dueRequest = Authorized(HttpMethod.Get, "/api/preventive-maintenance/due", token);
        var dueResponse = await _maintainarrClient.SendAsync(dueRequest);
        dueResponse.EnsureSuccessStatusCode();
        var dueItems = (await dueResponse.Content.ReadFromJsonAsync<List<PmScheduleResponse>>())!;
        Assert.Contains(
            dueItems,
            x => x.PmScheduleId == schedule.Id
                && (x.DueStatus == PmDueStatuses.Due || x.DueStatus == PmDueStatuses.Overdue));
    }

    private async Task<PmSchedule> SeedPastDuePmScheduleAsync(int daysPastDue = 2)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        var assetClass = new AssetClass
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ClassKey = "pm-scan-class",
            Name = "PM Scan Class",
            Description = "Test class",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };
        var assetType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetClassId = assetClass.Id,
            TypeKey = "pm-scan-type",
            Name = "PM Scan Type",
            Description = "Test type",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetTypeId = assetType.Id,
            AssetTag = "PM-1001",
            Name = "PM Scan Asset",
            Description = "Seeded for PM due scan worker test",
            LifecycleStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        };
        var schedule = new PmSchedule
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetId = asset.Id,
            ScheduleKey = "oil-change",
            Name = "Oil Change",
            Description = "Quarterly oil change",
            IntervalDays = 90,
            NextDueAt = now.AddDays(-daysPastDue),
            DueStatus = PmDueStatuses.Scheduled,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.AssetClasses.Add(assetClass);
        db.AssetTypes.Add(assetType);
        db.Assets.Add(asset);
        db.PmSchedules.Add(schedule);
        await db.SaveChangesAsync();
        return schedule;
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return token;
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
            $"{sourceProduct}-pm-due-{Guid.NewGuid():N}",
            $"{sourceProduct} PM due scan test",
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
