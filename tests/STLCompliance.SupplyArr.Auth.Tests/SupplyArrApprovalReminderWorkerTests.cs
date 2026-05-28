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
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrApprovalReminderWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _sharedWorkerToSupplyArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ApprovalReminderNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"ApprovalReminderSupplyArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToSupplyArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["supplyarr"],
            ApprovalReminderWorkerService.ProcessApprovalRemindersActionScope);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.PostAsJsonAsync(
            "/api/internal/approval-reminders/process-batch",
            new ProcessApprovalRemindersRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 25));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_sends_reminder_for_overdue_submitted_purchase_request()
    {
        var purchaseRequestId = await SeedSubmittedPurchaseRequestAsync();
        await BackdateSubmittedPurchaseRequestAsync(purchaseRequestId);
        await UpsertSettingsAsync();
        await UpsertNotificationSettingsAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/approval-reminders/process-batch",
            _sharedWorkerToSupplyArrToken);
        processRequest.Content = JsonContent.Create(new ProcessApprovalRemindersRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25));

        var processResponse = await _supplyarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessApprovalRemindersResponse>())!;
        Assert.Equal(1, body.CandidatesFound);
        Assert.Equal(1, body.RemindersSentCount);
        Assert.Single(body.RemindersSent);
        Assert.Equal(1, body.RemindersSent[0].ReminderCount);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var state = await db.ApprovalReminderStates.SingleAsync(
            x => x.SubjectType == ApprovalReminderSubjectTypes.PurchaseRequest
                && x.SubjectId == purchaseRequestId);
        Assert.Equal(1, state.ReminderCount);
        Assert.NotNull(state.LastReminderSentAt);

        var dispatch = await db.ProcurementNotificationDispatches.SingleAsync(
            x => x.EventKind == ProcurementNotificationEventKinds.PurchaseRequestApprovalReminder
                && x.RelatedEntityId == purchaseRequestId);
        Assert.Equal(ProcurementNotificationDispatchStatuses.Pending, dispatch.DispatchStatus);

        var adminToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_admin");
        var dashboardResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/approval-reminders", adminToken));
        dashboardResponse.EnsureSuccessStatusCode();
        var dashboard = (await dashboardResponse.Content.ReadFromJsonAsync<ApprovalRemindersDashboardResponse>())!;
        Assert.True(dashboard.OverdueCount >= 1);
        Assert.Contains(dashboard.Items, x => x.SubjectId == purchaseRequestId);
    }

    [Fact]
    public async Task Pending_preview_lists_due_approval_reminders()
    {
        var purchaseRequestId = await SeedSubmittedPurchaseRequestAsync();
        await BackdateSubmittedPurchaseRequestAsync(purchaseRequestId);
        await UpsertSettingsAsync();

        var pendingRequest = Authorized(
            HttpMethod.Get,
            $"/api/internal/approval-reminders/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=25",
            _sharedWorkerToSupplyArrToken);

        var pendingResponse = await _supplyarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingApprovalRemindersResponse>())!;
        Assert.Contains(pending.Items, x => x.SubjectId == purchaseRequestId);
    }

    [Fact]
    public async Task Approval_reminder_settings_requires_admin()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/approval-reminder-settings", buyerToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> SeedSubmittedPurchaseRequestAsync()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_admin");
        var vendorId = await SeedVendorAsync(token);
        var partId = await SeedPartAsync(token);

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"pr-{Guid.NewGuid():N}"[..16],
            "Approval reminder test PR",
            "Submitted for approval reminder worker",
            vendorId,
            [new CreatePurchaseRequestLineRequest(partId, 4m, "Test line")]));
        var createPrResponse = await _supplyarrClient.SendAsync(createPrRequest);
        createPrResponse.EnsureSuccessStatusCode();
        var purchaseRequest = (await createPrResponse.Content.ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token);
        var submitResponse = await _supplyarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        return purchaseRequest.PurchaseRequestId;
    }

    private async Task BackdateSubmittedPurchaseRequestAsync(Guid purchaseRequestId)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var purchaseRequest = await db.PurchaseRequests.SingleAsync(x => x.Id == purchaseRequestId);
        var backdated = DateTimeOffset.UtcNow.AddHours(-48);
        purchaseRequest.SubmittedAt = backdated;
        purchaseRequest.UpdatedAt = backdated;
        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedVendorAsync(string token)
    {
        var createVendorRequest = Authorized(HttpMethod.Post, "/api/vendors", token);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"vendor-{Guid.NewGuid():N}"[..16],
            "Reminder Vendor",
            "Reminder Vendor LLC",
            null,
            string.Empty));
        var createVendorResponse = await _supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var vendor = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;
        return vendor.PartyId;
    }

    private async Task<Guid> SeedPartAsync(string token)
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"part-{Guid.NewGuid():N}"[..16],
            null,
            "Reminder part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;
        return part.PartId;
    }

    private async Task UpsertSettingsAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantApprovalReminderSettings.Add(new TenantApprovalReminderSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            PrReminderAfterHours = 24,
            PoReminderAfterHours = 24,
            ReminderCooldownHours = 24,
            MaxRemindersPerSubject = 10,
            NotifyOnPrApprovalReminder = true,
            NotifyOnPoApprovalReminder = true,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task UpsertNotificationSettingsAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantProcurementNotificationSettings.Add(new TenantProcurementNotificationSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            NotificationWebhookUrl = "https://example.test/supplyarr-webhook",
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private string CreateSupplyArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<SupplyArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
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
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-approval-reminder-test",
            $"{sourceProduct} approval reminder test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
