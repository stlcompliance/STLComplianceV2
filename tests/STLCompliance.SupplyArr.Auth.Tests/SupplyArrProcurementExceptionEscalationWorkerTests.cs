using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
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

public sealed class SupplyArrProcurementExceptionEscalationWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _sharedWorkerToSupplyArrToken = null!;
    private string _sharedWorkerToSupplyArrAutoCloseToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ProcExceptionEscalationNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"ProcExceptionEscalationSupplyArr-{Guid.NewGuid():N}";

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
            "proc-exception-escalation-test",
            ["supplyarr"],
            ProcurementExceptionEscalationWorkerService.ProcessProcurementExceptionEscalationsActionScope);
        _sharedWorkerToSupplyArrAutoCloseToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            "proc-exception-auto-close-test",
            ["supplyarr"],
            ProcurementExceptionAutomationWorkerService.ProcessProcurementExceptionAutoClosesActionScope);

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
            "/api/internal/procurement-exception-escalations/process-batch",
            new ProcessProcurementExceptionEscalationsRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 25));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_escalates_overdue_procurement_exception()
    {
        var exceptionId = await SeedOverdueProcurementExceptionAsync();
        await UpsertEscalationSettingsAsync();
        await UpsertNotificationSettingsAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/procurement-exception-escalations/process-batch",
            _sharedWorkerToSupplyArrToken);
        processRequest.Content = JsonContent.Create(new ProcessProcurementExceptionEscalationsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25));

        var processResponse = await _supplyarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessProcurementExceptionEscalationsResponse>())!;
        Assert.Equal(1, body.CandidatesFound);
        Assert.Equal(1, body.EscalatedCount);
        Assert.Single(body.Escalated);
        Assert.Equal(1, body.Escalated[0].EscalationCount);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var exception = await db.ProcurementExceptions.SingleAsync(x => x.Id == exceptionId);
        Assert.Equal(1, exception.EscalationCount);
        Assert.NotNull(exception.LastEscalatedAt);

        var escalationEvent = await db.ProcurementExceptionEscalationEvents.SingleAsync(
            x => x.ProcurementExceptionId == exceptionId);
        Assert.Equal(1, escalationEvent.EscalationLevel);

        var dispatch = await db.ProcurementNotificationDispatches.SingleAsync(
            x => x.EventKind == ProcurementNotificationEventKinds.ProcurementExceptionSlaEscalation
                && x.RelatedEntityId == exceptionId);
        Assert.Equal(ProcurementNotificationDispatchStatuses.Pending, dispatch.DispatchStatus);
    }

    [Fact]
    public async Task Pending_preview_lists_due_escalations()
    {
        var exceptionId = await SeedOverdueProcurementExceptionAsync();
        await UpsertEscalationSettingsAsync();

        var pendingRequest = Authorized(
            HttpMethod.Get,
            $"/api/internal/procurement-exception-escalations/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=25",
            _sharedWorkerToSupplyArrToken);

        var pendingResponse = await _supplyarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingProcurementExceptionEscalationsResponse>())!;
        Assert.Contains(pending.Items, x => x.ProcurementExceptionId == exceptionId);
    }

    [Fact]
    public async Task Pending_auto_close_preview_lists_due_completed_exceptions()
    {
        var exceptionId = await SeedCompletedProcurementExceptionAsync(
            ProcurementExceptionStatuses.Resolved,
            DateTimeOffset.UtcNow.AddHours(-72));
        await UpsertEscalationSettingsAsync(autoCloseEnabled: true, autoCloseAfterHours: 48);

        var adminToken = CreateSupplyArrAccessToken(["supplyarr"], "tenant_admin");
        var pendingResponse = await _supplyarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/v1/procurement-exception-escalation-settings/auto-close/pending",
                adminToken));

        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingProcurementExceptionAutoClosesResponse>())!;
        Assert.Contains(pending.Items, x => x.ProcurementExceptionId == exceptionId);
        var item = pending.Items.Single(x => x.ProcurementExceptionId == exceptionId);
        Assert.Equal("resolved", item.Status);
        Assert.Equal(72d, Math.Round(item.HoursCompleted));
    }

    [Fact]
    public async Task Process_auto_close_batch_closes_completed_exception()
    {
        var exceptionId = await SeedCompletedProcurementExceptionAsync(
            ProcurementExceptionStatuses.Resolved,
            DateTimeOffset.UtcNow.AddHours(-72));
        await UpsertEscalationSettingsAsync(autoCloseEnabled: true, autoCloseAfterHours: 48);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/procurement-exception-automation/process-batch",
            _sharedWorkerToSupplyArrAutoCloseToken);
        processRequest.Content = JsonContent.Create(new ProcessProcurementExceptionAutoClosesRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25));

        var processResponse = await _supplyarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessProcurementExceptionAutoClosesResponse>())!;
        Assert.Equal(1, body.CandidatesFound);
        Assert.Equal(1, body.ClosedCount);
        Assert.Empty(body.Skipped);
        Assert.Single(body.Closed);
        Assert.Equal("closed", body.Closed[0].Status);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var exception = await db.ProcurementExceptions.SingleAsync(x => x.Id == exceptionId);
        Assert.Equal(ProcurementExceptionStatuses.Closed, exception.Status);
        Assert.NotNull(exception.ClosedAt);
        Assert.Equal(ProcurementExceptionAutomationWorkerService.WorkerActorUserId, exception.ClosedByUserId);

        var auditEvent = await db.AuditEvents.SingleAsync(
            x => x.Action == "supplyarr.procurement_exception_auto_close.batch"
                && x.TenantId == PlatformSeeder.DemoTenantId);
        Assert.Equal(ProcurementExceptionAutomationWorkerService.WorkerActorUserId, auditEvent.ActorUserId);
    }

    [Fact]
    public async Task Process_auto_close_batch_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.PostAsJsonAsync(
            "/api/internal/procurement-exception-automation/process-batch",
            new ProcessProcurementExceptionAutoClosesRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 25));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Escalation_settings_requires_admin()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/procurement-exception-escalation-settings", buyerToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> SeedOverdueProcurementExceptionAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var vendorId = Guid.NewGuid();
        var prId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();

        db.ExternalParties.Add(new ExternalParty
        {
            Id = vendorId,
            TenantId = PlatformSeeder.DemoTenantId,
            PartyKey = $"vendor-{Guid.NewGuid():N}"[..16],
            PartyType = "vendor",
            DisplayName = "Escalation Vendor",
            LegalName = "Escalation Vendor LLC",
            ApprovalStatus = "approved",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PurchaseRequests.Add(new PurchaseRequest
        {
            Id = prId,
            TenantId = PlatformSeeder.DemoTenantId,
            RequestKey = $"pr-{Guid.NewGuid():N}"[..16],
            Title = "Escalation PR",
            Notes = string.Empty,
            Status = PurchaseRequestStatuses.Draft,
            VendorPartyId = vendorId,
            RequestedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.ProcurementExceptions.Add(new ProcurementException
        {
            Id = exceptionId,
            TenantId = PlatformSeeder.DemoTenantId,
            ExceptionKey = $"PEX-{Guid.NewGuid():N}"[..16],
            SubjectType = ProcurementExceptionSubjectTypes.PurchaseRequest,
            SubjectId = prId,
            SubjectKey = "pr-escalation",
            VendorPartyId = vendorId,
            ExceptionCategory = ProcurementExceptionCategories.ApprovalDelay,
            Title = "Overdue approval exception",
            Description = "SLA breached for escalation worker test.",
            Status = ProcurementExceptionStatuses.Open,
            SlaDueAt = now.AddHours(-12),
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
        return exceptionId;
    }

    private async Task<Guid> SeedCompletedProcurementExceptionAsync(string status, DateTimeOffset completedAt)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var vendorId = Guid.NewGuid();
        var prId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();

        db.ExternalParties.Add(new ExternalParty
        {
            Id = vendorId,
            TenantId = PlatformSeeder.DemoTenantId,
            PartyKey = $"vendor-{Guid.NewGuid():N}"[..16],
            PartyType = "vendor",
            DisplayName = "Completed Vendor",
            LegalName = "Completed Vendor LLC",
            ApprovalStatus = "approved",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.PurchaseRequests.Add(new PurchaseRequest
        {
            Id = prId,
            TenantId = PlatformSeeder.DemoTenantId,
            RequestKey = $"pr-{Guid.NewGuid():N}"[..16],
            Title = "Completed PR",
            Notes = string.Empty,
            Status = PurchaseRequestStatuses.Draft,
            VendorPartyId = vendorId,
            RequestedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.ProcurementExceptions.Add(new ProcurementException
        {
            Id = exceptionId,
            TenantId = PlatformSeeder.DemoTenantId,
            ExceptionKey = $"PEX-{Guid.NewGuid():N}"[..16],
            SubjectType = ProcurementExceptionSubjectTypes.PurchaseRequest,
            SubjectId = prId,
            SubjectKey = "pr-completed",
            VendorPartyId = vendorId,
            ExceptionCategory = ProcurementExceptionCategories.PolicyViolation,
            Title = "Completed exception",
            Description = "Completed exception for auto-close worker test.",
            Status = status,
            ResolvedAt = string.Equals(status, ProcurementExceptionStatuses.Resolved, StringComparison.OrdinalIgnoreCase)
                ? completedAt
                : null,
            WaivedAt = string.Equals(status, ProcurementExceptionStatuses.Waived, StringComparison.OrdinalIgnoreCase)
                ? completedAt
                : null,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = completedAt.AddHours(-24),
            UpdatedAt = completedAt,
        });

        await db.SaveChangesAsync();
        return exceptionId;
    }

    private async Task UpsertEscalationSettingsAsync(
        bool autoCloseEnabled = false,
        int autoCloseAfterHours = ProcurementExceptionEscalationDefaults.AutoCloseCompletedExceptionsAfterHours)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantProcurementExceptionEscalationSettings.Add(new TenantProcurementExceptionEscalationSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            EscalationCooldownHours = 24,
            MaxEscalationsPerException = 5,
            NotifyOnProcurementExceptionSlaEscalation = true,
            AutoCloseCompletedExceptionsEnabled = autoCloseEnabled,
            AutoCloseCompletedExceptionsAfterHours = autoCloseAfterHours,
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
        string clientSuffix,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-{clientSuffix}",
            $"{sourceProduct} procurement exception escalation test",
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
