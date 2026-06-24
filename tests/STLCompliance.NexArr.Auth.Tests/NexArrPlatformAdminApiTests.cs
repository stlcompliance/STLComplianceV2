using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrPlatformAdminApiTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrPlatformAdminApiTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<NexArrDbContext>)
                        || d.ServiceType == typeof(NexArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<NexArrDbContext>(options =>
                    options.UseInMemoryDatabase("NexArrPlatformAdminTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Platform_admin_dashboard_requires_authentication()
    {
        var response = await _client.GetAsync("/api/platform-admin/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_read_dashboard()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/dashboard", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboard = await response.Content.ReadFromJsonAsync<PlatformAdminDashboardResponse>();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.TenantCount >= 1);
        Assert.True(dashboard.ProductCount >= 7);
        Assert.True(dashboard.LaunchProfileCount >= 1);
    }

    [Fact]
    public async Task Platform_admin_dashboard_requires_recent_admin_session()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var session = await db.UserSessions
                .OrderByDescending(x => x.CreatedAt)
                .FirstAsync(x => x.UserId == PlatformSeeder.DemoAdminUserId);
            session.CreatedAt = DateTimeOffset.UtcNow.AddHours(-2);
            await db.SaveChangesAsync();
        }

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/dashboard", token));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_platform_admin_dashboard()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/dashboard", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_read_launch_diagnostics()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/launch-diagnostics", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var diagnostics = await response.Content.ReadFromJsonAsync<LaunchDiagnosticsResponse>();
        Assert.NotNull(diagnostics);
        Assert.NotEmpty(diagnostics.Rows);

        var v1Response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/platform-admin/launch-diagnostics", token));
        Assert.Equal(HttpStatusCode.OK, v1Response.StatusCode);
        var v1Diagnostics = await v1Response.Content.ReadFromJsonAsync<LaunchDiagnosticsResponse>();
        Assert.NotNull(v1Diagnostics);
        Assert.NotEmpty(v1Diagnostics.Rows);
    }

    [Fact]
    public async Task Platform_admin_can_upload_master_csv_and_assign_rows_before_upsert()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var csv = string.Join('\n', new[]
        {
            "product,dataset,entity_type,canonical_key,display_name",
            "MaintainArr,Asset Class,asset_class,asset-class,Asset Class",
            "Party,,,,Vendor A",
        });

        var createRequest = Authorized(HttpMethod.Post, "/api/platform-admin/reference-data/imports/master-csv", token);
        createRequest.Content = JsonContent.Create(new CreateReferenceMasterCsvImportRequest(
            csv,
            "master-reference.csv",
            "seed/reference/master-reference.csv"));

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ReferenceImportResponse>())!;
        Assert.Equal(ReferenceImportStatuses.ReviewRequired, created.Status);
        Assert.Equal(2, created.StagingRecordCount);
        Assert.Equal(2, created.PendingReviewCount);

        var stagingResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/platform-admin/reference-data/imports/{created.Id}/staging-records", token));
        stagingResponse.EnsureSuccessStatusCode();
        var staged = (await stagingResponse.Content.ReadFromJsonAsync<IReadOnlyList<ReferenceStagingRecordResponse>>())!;
        Assert.Equal(2, staged.Count);

        var resolvedRow = Assert.Single(staged, row => row.TargetDatasetKey == "maintainarr-asset-class");
        Assert.Equal("MaintainArr", resolvedRow.TargetOwnerService);

        var unresolvedRow = Assert.Single(staged, row => row.TargetDatasetId is null);

        var resolvedApprove = Authorized(
            HttpMethod.Post,
            $"/api/platform-admin/reference-data/staging-records/{resolvedRow.Id}/approve",
            token);
        resolvedApprove.Content = JsonContent.Create(new ReviewDecisionRequest(
            "Auto-assigned from CSV",
            null,
            null,
            null,
            null,
            null,
            resolvedRow.TargetDatasetId));
        var resolvedApproveResponse = await _client.SendAsync(resolvedApprove);
        resolvedApproveResponse.EnsureSuccessStatusCode();

        var unresolvedApprove = Authorized(
            HttpMethod.Post,
            $"/api/platform-admin/reference-data/staging-records/{unresolvedRow.Id}/approve",
            token);
        unresolvedApprove.Content = JsonContent.Create(new ReviewDecisionRequest(
            "Assign the row manually",
            null,
            null,
            null,
            null,
            null,
            null));
        var unresolvedApproveResponse = await _client.SendAsync(unresolvedApprove);
        Assert.Equal(HttpStatusCode.BadRequest, unresolvedApproveResponse.StatusCode);

        Guid partyDatasetId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            partyDatasetId = await db.ReferenceDatasets
                .Where(x => x.Key == "supplyarr-party")
                .Select(x => x.Id)
                .SingleAsync();
        }

        var resolveAndApprove = Authorized(
            HttpMethod.Post,
            $"/api/platform-admin/reference-data/staging-records/{unresolvedRow.Id}/approve",
            token);
        resolveAndApprove.Content = JsonContent.Create(new ReviewDecisionRequest(
            "Assign the row manually",
            "Vendor A",
            "vendor-a",
            null,
            null,
            null,
            partyDatasetId));
        var resolveAndApproveResponse = await _client.SendAsync(resolveAndApprove);
        resolveAndApproveResponse.EnsureSuccessStatusCode();
        var approved = (await resolveAndApproveResponse.Content.ReadFromJsonAsync<ReferenceStagingRecordResponse>())!;
        Assert.Equal(partyDatasetId, approved.TargetDatasetId);
        Assert.Equal("supplyarr-party", approved.TargetDatasetKey);

        var importAfterReview = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/platform-admin/reference-data/imports/{created.Id}", token));
        importAfterReview.EnsureSuccessStatusCode();
        var reviewed = (await importAfterReview.Content.ReadFromJsonAsync<ReferenceImportResponse>())!;
        Assert.Equal(ReferenceImportStatuses.Completed, reviewed.Status);
        Assert.Equal(0, reviewed.PendingReviewCount);
        Assert.Equal(2, reviewed.ApprovedCount);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_launch_diagnostics()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/launch-diagnostics", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var v1Response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/platform-admin/launch-diagnostics", token));
        Assert.Equal(HttpStatusCode.Forbidden, v1Response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_lookup_launch_attempts_by_product_and_result()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "https://evil.example/callback"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        Assert.Equal(HttpStatusCode.Forbidden, handoffResponse.StatusCode);

        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/platform-admin/launch-attempts?productKey=staffarr&result=Denied",
                token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var attempts = await response.Content.ReadFromJsonAsync<PagedResult<LaunchAttemptTimelineItemResponse>>();
        Assert.NotNull(attempts);
        var attempt = Assert.Single(attempts.Items);
        Assert.Equal("launch.handoff.create", attempt.Action);
        Assert.Equal("Denied", attempt.Result);
        Assert.Equal("callback_not_allowed", attempt.ReasonCode);
        Assert.Equal("staffarr", attempt.ProductKey);
        Assert.Equal("StaffArr", attempt.ProductDisplayName);
        Assert.Equal(PlatformSeeder.DemoTenantId, attempt.TenantId);
        Assert.Equal(PlatformSeeder.DemoAdminEmail, attempt.ActorEmail);
        Assert.Contains("callback allowlist", attempt.RemediationHint);

        var v1Response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/v1/platform-admin/launch-attempts?productKey=staffarr&result=Denied",
                token));
        Assert.Equal(HttpStatusCode.OK, v1Response.StatusCode);
        var v1Attempts = await v1Response.Content.ReadFromJsonAsync<PagedResult<LaunchAttemptTimelineItemResponse>>();
        Assert.NotNull(v1Attempts);
        Assert.NotEmpty(v1Attempts.Items);
    }

    [Fact]
    public async Task Platform_admin_can_diagnose_handoff_redeem_after_entitlement_revoked()
    {
        await SeedDatabaseAsync();
        var platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", tenantAdminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        var serviceToken = await IssueServiceTokenAsync(platformAdminToken, "staffarr");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var entitlement = await db.Entitlements.SingleAsync(
                x => x.TenantId == PlatformSeeder.DemoTenantId && x.ProductKey == "staffarr");
            entitlement.Status = EntitlementStatuses.Revoked;
            await db.SaveChangesAsync();
        }

        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", platformAdminToken);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, serviceToken));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, redeemResponse.StatusCode);

        var attemptsResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/platform-admin/launch-attempts?productKey=staffarr&result=Denied",
                platformAdminToken));
        attemptsResponse.EnsureSuccessStatusCode();
        var attempts = (await attemptsResponse.Content.ReadFromJsonAsync<PagedResult<LaunchAttemptTimelineItemResponse>>())!;
        var redeemAttempt = Assert.Single(attempts.Items, x => x.Action == "launch.handoff.redeem");

        Assert.Equal("entitlement_revoked", redeemAttempt.ReasonCode);
        Assert.Equal("staffarr", redeemAttempt.ProductKey);
        Assert.Equal(PlatformSeeder.DemoTenantId, redeemAttempt.TenantId);
        Assert.Equal(PlatformSeeder.DemoTenantAdminEmail, redeemAttempt.ActorEmail);
        Assert.Contains("entitlement", redeemAttempt.RemediationHint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Platform_admin_can_diagnose_handoff_redeem_without_service_token()
    {
        await SeedDatabaseAsync();
        var platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", tenantAdminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", tenantAdminToken);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, null));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        Assert.Equal(HttpStatusCode.Forbidden, redeemResponse.StatusCode);

        var attemptsResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/platform-admin/launch-attempts?productKey=staffarr&result=Denied",
                platformAdminToken));
        attemptsResponse.EnsureSuccessStatusCode();
        var attempts = (await attemptsResponse.Content.ReadFromJsonAsync<PagedResult<LaunchAttemptTimelineItemResponse>>())!;
        var redeemAttempt = Assert.Single(attempts.Items, x => x.Action == "launch.handoff.redeem");

        Assert.Equal("auth.forbidden", redeemAttempt.ReasonCode);
        Assert.Equal("staffarr", redeemAttempt.ProductKey);
        Assert.Equal(PlatformSeeder.DemoTenantAdminEmail, redeemAttempt.ActorEmail);
        Assert.Contains("service token", redeemAttempt.RemediationHint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_launch_attempts()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/launch-attempts", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_read_user_login_and_launch_history_separately()
    {
        await SeedDatabaseAsync();
        var platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var loginResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/login-history?page=1&pageSize=20",
                platformAdminToken));
        loginResponse.EnsureSuccessStatusCode();
        var loginHistory = await loginResponse.Content.ReadFromJsonAsync<PagedResult<PlatformUserAccessHistoryItemResponse>>();
        Assert.NotNull(loginHistory);
        Assert.NotEmpty(loginHistory!.Items);
        Assert.All(loginHistory.Items, item => Assert.StartsWith("auth.", item.Action, StringComparison.Ordinal));

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", tenantAdminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();

        var launchResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/launch-history?page=1&pageSize=20",
                platformAdminToken));
        launchResponse.EnsureSuccessStatusCode();
        var launchHistory = await launchResponse.Content.ReadFromJsonAsync<PagedResult<PlatformUserAccessHistoryItemResponse>>();
        Assert.NotNull(launchHistory);
        Assert.NotEmpty(launchHistory!.Items);
        Assert.All(launchHistory.Items, item => Assert.StartsWith("launch.", item.Action, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Platform_support_role_can_read_platform_admin_dashboard()
    {
        await SeedDatabaseAsync();
        await GrantPlatformRoleAsync(PlatformSeeder.DemoTenantAdminUserId, "platform_support");
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/dashboard", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Read_only_auditor_role_can_read_launch_attempts()
    {
        await SeedDatabaseAsync();
        await GrantPlatformRoleAsync(PlatformSeeder.DemoTenantAdminUserId, "read_only_auditor");
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/launch-attempts", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_scoped_platform_support_requires_tenant_scope_for_launch_diagnostics()
    {
        await SeedDatabaseAsync();
        await GrantPlatformRoleAsync(
            PlatformSeeder.DemoTenantAdminUserId,
            "platform_support",
            PlatformSeeder.DemoTenantId);
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var unscopedResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/launch-diagnostics", token));
        Assert.Equal(HttpStatusCode.Forbidden, unscopedResponse.StatusCode);

        var scopedResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/launch-diagnostics?tenantId={PlatformSeeder.DemoTenantId}",
                token));
        Assert.Equal(HttpStatusCode.OK, scopedResponse.StatusCode);
    }

    [Fact]
    public async Task Tenant_scoped_auditor_cannot_read_other_tenant_launch_attempts()
    {
        await SeedDatabaseAsync();
        await GrantPlatformRoleAsync(
            PlatformSeeder.DemoTenantAdminUserId,
            "read_only_auditor",
            PlatformSeeder.DemoTenantId);
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var otherTenantId = Guid.Parse("99999999-9999-9999-9999-999999999901");

        var forbiddenResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/launch-attempts?tenantId={otherTenantId}",
                token));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_create_and_update_user_with_outbox_events()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", token);
        createRequest.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "Ops-Lead@Example.test",
            "Ops Lead",
            "StrongPass1234",
            IsPlatformAdmin: false));

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;

        Assert.Equal("ops-lead@example.test", created.Email);
        Assert.Equal("Ops Lead", created.DisplayName);
        Assert.True(created.IsActive);
        Assert.False(created.IsPlatformAdmin);

        var updateRequest = Authorized(HttpMethod.Patch, $"/api/v1/platform-admin/users/{created.UserId}", token);
        updateRequest.Content = JsonContent.Create(new UpdatePlatformUserRequest(
            "ops-admin@example.test",
            "Ops Admin",
            IsPlatformAdmin: true));

        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;

        Assert.Equal(created.UserId, updated.UserId);
        Assert.Equal("ops-admin@example.test", updated.Email);
        Assert.Equal("Ops Admin", updated.DisplayName);
        Assert.True(updated.IsPlatformAdmin);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var user = await db.Users.AsNoTracking().SingleAsync(x => x.Id == created.UserId);
        Assert.Equal("ops-admin@example.test", user.Email);
        Assert.True(user.IsPlatformAdmin);

        var outboxEvents = await db.PlatformOutboxEvents
            .AsNoTracking()
            .Where(x => x.PayloadJson.Contains(created.UserId.ToString()))
            .ToListAsync();

        var createdEvent = Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.UserCreated);
        Assert.Contains("ops-lead@example.test", createdEvent.PayloadJson);

        var updatedEvent = Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.UserUpdated);
        Assert.Contains("ops-admin@example.test", updatedEvent.PayloadJson);
        Assert.Contains("Ops Lead", updatedEvent.PayloadJson);
    }

    [Fact]
    public async Task Tenant_admin_cannot_create_platform_user()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", token);
        request.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "tenant-user@example.test",
            "Tenant User",
            "StrongPass1234"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_invite_user_without_login_credentials()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var inviteRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users/invite", token);
        inviteRequest.Content = JsonContent.Create(new InvitePlatformUserRequest(
            "invited-user@example.test",
            "Invited User"));

        var inviteResponse = await _client.SendAsync(inviteRequest);
        Assert.Equal(HttpStatusCode.Created, inviteResponse.StatusCode);
        var invited = (await inviteResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;
        Assert.NotEqual(Guid.Empty, invited.UserId);
        Assert.Equal("invited-user@example.test", invited.Email);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                "invited-user@example.test",
                "AnyPassword123!",
                PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_create_user_pending_verification_and_login_is_blocked_until_verified()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", token);
        createRequest.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "pending-verify@example.test",
            "Pending Verify User",
            "StrongPass1234",
            IsPlatformAdmin: false,
            IsActive: true,
            RequireEmailVerification: true));

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;
        Assert.True(created.CanLogin);
        Assert.Equal("pending_verification", created.Status);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                "pending-verify@example.test",
                "StrongPass1234",
                PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_cannot_invite_platform_user()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var inviteRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users/invite", token);
        inviteRequest.Content = JsonContent.Create(new InvitePlatformUserRequest(
            "blocked-invite@example.test",
            "Blocked Invite"));

        var inviteResponse = await _client.SendAsync(inviteRequest);
        Assert.Equal(HttpStatusCode.Forbidden, inviteResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_lock_and_unlock_user_with_outbox_events()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var lockResponse = await _client.SendAsync(
            AuthorizedWithConfirmation(HttpMethod.Post, $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/lock", token));

        Assert.Equal(HttpStatusCode.OK, lockResponse.StatusCode);
        var locked = (await lockResponse.Content.ReadFromJsonAsync<PlatformUserLockResponse>())!;
        Assert.False(locked.WasAlreadyLocked);
        Assert.True(locked.LockedUntil > DateTimeOffset.UtcNow);

        var blockedLoginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.Locked, blockedLoginResponse.StatusCode);

        var unlockResponse = await _client.SendAsync(
            AuthorizedWithConfirmation(HttpMethod.Post, $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/unlock", token));

        Assert.Equal(HttpStatusCode.OK, unlockResponse.StatusCode);
        var unlocked = (await unlockResponse.Content.ReadFromJsonAsync<PlatformUserUnlockResponse>())!;
        Assert.False(unlocked.WasAlreadyUnlocked);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var user = await db.Users
            .Include(x => x.Credential)
            .SingleAsync(x => x.Id == PlatformSeeder.DemoTenantAdminUserId);

        Assert.Null(user.Credential!.LockedUntil);
        Assert.Equal(0, user.Credential.FailedLoginCount);

        var outboxEvents = await db.PlatformOutboxEvents
            .AsNoTracking()
            .Where(x => x.PayloadJson.Contains(PlatformSeeder.DemoTenantAdminUserId.ToString()))
            .ToListAsync();

        Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.UserLocked);
        Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.UserUnlocked);
    }

    [Fact]
    public async Task Platform_admin_can_read_tenant_overview()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/overview/tenants", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var overview = await response.Content.ReadFromJsonAsync<PagedResult<TenantOverviewRowResponse>>();
        Assert.NotNull(overview);
        Assert.NotEmpty(overview.Items);
    }

    [Fact]
    public async Task Platform_admin_can_read_product_overview()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/overview/products", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductOverviewRowResponse>>();
        Assert.NotNull(products);
        Assert.Contains(products, p => p.ProductKey == "staffarr");
    }

    [Fact]
    public async Task Database_nuke_preview_requires_platform_owner()
    {
        await SeedDatabaseAsync();
        await GrantPlatformRoleAsync(PlatformSeeder.DemoTenantAdminUserId, "platform_support");
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/database-nuke/preview", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_owner_can_preview_database_nuke_plan()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/database-nuke/preview", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var preview = await response.Content.ReadFromJsonAsync<DatabaseNukePreviewResponse>();
        Assert.NotNull(preview);
        Assert.True(preview!.IsEnabled);
        Assert.Equal("NUKE PRODUCT DATA", preview.ConfirmationPhrase);
        Assert.Contains(preview.Targets, target => target.ProductDatabase == "nexarr");
        Assert.Contains(preview.Targets, target => target.ProductDatabase == "customarr");
        Assert.All(preview.Targets, target => Assert.Equal("missing_connection", target.Status));
    }

    [Fact]
    public async Task Database_nuke_execute_requires_confirmation_header()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/platform-admin/database-nuke", token);
        request.Content = JsonContent.Create(new ExecuteDatabaseNukeRequest(
            "NUKE PRODUCT DATA",
            "Reset seeded demo product data"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Database_nuke_execute_requires_confirmation_phrase()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var request = AuthorizedWithDatabaseNukeConfirmation(
            HttpMethod.Post,
            "/api/platform-admin/database-nuke",
            token);
        request.Content = JsonContent.Create(new ExecuteDatabaseNukeRequest(
            "CONFIRM",
            "Reset seeded demo product data"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_user_list_includes_last_login_timestamp()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", tenantAdminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/users?page=1&pageSize=20", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<PlatformUsersListResponse>();
        Assert.NotNull(users);
        var admin = Assert.Single(users.Items, x => x.UserId == PlatformSeeder.DemoAdminUserId);
        Assert.NotNull(admin.LastLoginAt);
        var tenantAdmin = Assert.Single(users.Items, x => x.UserId == PlatformSeeder.DemoTenantAdminUserId);
        Assert.NotNull(tenantAdmin.LastProductLaunchAt);
    }

    [Fact]
    public async Task Platform_admin_can_get_user_detail_with_last_activity_timestamps()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", tenantAdminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<PlatformUserDetailResponse>();
        Assert.NotNull(user);
        Assert.Equal(PlatformSeeder.DemoTenantAdminUserId, user.UserId);
        Assert.NotNull(user.LastLoginAt);
        Assert.NotNull(user.LastProductLaunchAt);
    }

    [Fact]
    public async Task Platform_admin_can_list_and_revoke_user_sessions()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var tenantLoginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        tenantLoginResponse.EnsureSuccessStatusCode();
        var tenantTokens = (await tenantLoginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>())!;

        var sessionsResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/sessions",
                adminToken));
        sessionsResponse.EnsureSuccessStatusCode();
        var sessions = (await sessionsResponse.Content.ReadFromJsonAsync<PlatformUserSessionsResponse>())!;

        Assert.Equal(PlatformSeeder.DemoTenantAdminUserId, sessions.UserId);
        Assert.Contains(sessions.Sessions, x => x.SessionId == tenantTokens.SessionId && x.IsActive);

        var revokeResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/sessions/{tenantTokens.SessionId}/revoke",
                adminToken));
        revokeResponse.EnsureSuccessStatusCode();
        var revoked = (await revokeResponse.Content.ReadFromJsonAsync<PlatformUserSessionRevokeResponse>())!;
        Assert.False(revoked.WasAlreadyRevoked);

        var renewResponse = await _client.PostAsJsonAsync(
            "/api/auth/renew",
            new RenewSessionRequest(tenantTokens.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, renewResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_toggle_user_mfa_with_confirmation()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "mfa-toggle@example.test",
            "Mfa Toggle",
            "StrongPass1234"));
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;
        Assert.False(created.IsMfaEnabled);

        var enableRequest = AuthorizedWithConfirmation(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{created.UserId}/mfa",
            adminToken);
        enableRequest.Content = JsonContent.Create(new SetPlatformUserMfaRequest(true));
        var enableResponse = await _client.SendAsync(enableRequest);
        enableResponse.EnsureSuccessStatusCode();
        var enabled = (await enableResponse.Content.ReadFromJsonAsync<PlatformUserMfaResponse>())!;
        Assert.True(enabled.IsMfaEnabled);
        Assert.False(enabled.WasAlreadySet);
        Assert.False(string.IsNullOrWhiteSpace(enabled.MfaSecret));
        Assert.False(string.IsNullOrWhiteSpace(enabled.ProvisioningUri));
        Assert.NotNull(enabled.RecoveryCodes);
        Assert.NotEmpty(enabled.RecoveryCodes!);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var storedCredential = await db.UserCredentials.SingleAsync(x => x.UserId == created.UserId);
            Assert.False(string.IsNullOrWhiteSpace(storedCredential.MfaSecret));
            Assert.StartsWith("v1.", storedCredential.MfaSecret);
            Assert.NotEqual(enabled.MfaSecret, storedCredential.MfaSecret);
        }

        var disableRequest = AuthorizedWithConfirmation(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{created.UserId}/mfa",
            adminToken);
        disableRequest.Content = JsonContent.Create(new SetPlatformUserMfaRequest(false));
        var disableResponse = await _client.SendAsync(disableRequest);
        disableResponse.EnsureSuccessStatusCode();
        var disabled = (await disableResponse.Content.ReadFromJsonAsync<PlatformUserMfaResponse>())!;
        Assert.False(disabled.IsMfaEnabled);
        Assert.Null(disabled.MfaSecret);
        Assert.Null(disabled.ProvisioningUri);
        Assert.Null(disabled.RecoveryCodes);

        var detailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/platform-admin/users/{created.UserId}", adminToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;
        Assert.False(detail.IsMfaEnabled);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_tenant_overview()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/overview/tenants", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_can_rename_own_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var currentResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/tenants/{PlatformSeeder.DemoTenantId}", token));
        currentResponse.EnsureSuccessStatusCode();
        var current = (await currentResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;

        var updateRequest = Authorized(HttpMethod.Put, $"/api/tenants/{PlatformSeeder.DemoTenantId}", token);
        updateRequest.Content = JsonContent.Create(new UpdateTenantRequest(
            "Renamed Demo Tenant",
            current.SubscriptionTier,
            current.BillingCustomerId,
            current.BillingSubscriptionId,
            current.BillingGraceDays,
            current.IsTrial,
            current.IsInternalTenant));

        var updateResponse = await _client.SendAsync(updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;
        Assert.Equal(PlatformSeeder.DemoTenantId, updated.TenantId);
        Assert.Equal("Renamed Demo Tenant", updated.DisplayName);
        Assert.Equal(current.SubscriptionTier, updated.SubscriptionTier);
        Assert.Equal(current.BillingCustomerId, updated.BillingCustomerId);
        Assert.Equal(current.BillingSubscriptionId, updated.BillingSubscriptionId);
        Assert.Equal(current.BillingGraceDays, updated.BillingGraceDays);
        Assert.Equal(current.IsTrial, updated.IsTrial);
        Assert.Equal(current.IsInternalTenant, updated.IsInternalTenant);
    }

    [Fact]
    public async Task Tenant_admin_cannot_change_tenant_settings_while_renaming()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var currentResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/tenants/{PlatformSeeder.DemoTenantId}", token));
        currentResponse.EnsureSuccessStatusCode();
        var current = (await currentResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;

        var updateRequest = Authorized(HttpMethod.Put, $"/api/tenants/{PlatformSeeder.DemoTenantId}", token);
        updateRequest.Content = JsonContent.Create(new UpdateTenantRequest(
            "Renamed Demo Tenant",
            "enterprise",
            current.BillingCustomerId,
            current.BillingSubscriptionId,
            current.BillingGraceDays,
            current.IsTrial,
            current.IsInternalTenant));

        var updateResponse = await _client.SendAsync(updateRequest);

        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_read_user_identity_audit_history()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "identity-audit@example.test",
            "Identity Audit",
            "StrongPass1234"));
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdUser = (await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;

        var disableResponse = await _client.SendAsync(
            AuthorizedWithConfirmation(HttpMethod.Post, $"/api/v1/platform-admin/users/{createdUser.UserId}/disable", adminToken));
        disableResponse.EnsureSuccessStatusCode();

        var roleRequest = Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{createdUser.UserId}/roles", adminToken);
        roleRequest.Content = JsonContent.Create(new AssignPlatformUserRoleRequest("read_only_auditor"));
        var roleResponse = await _client.SendAsync(roleRequest);
        roleResponse.EnsureSuccessStatusCode();

        var historyResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/users/{createdUser.UserId}/identity-audit-history",
                adminToken));

        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        var history = await historyResponse.Content.ReadFromJsonAsync<PagedResult<PlatformUserIdentityAuditHistoryItemResponse>>();
        Assert.NotNull(history);
        Assert.True(history.TotalCount >= 3);
        Assert.Contains(history.Items, x => x.Action == "user.created");
        Assert.Contains(history.Items, x => x.Action == "user.disabled");
        Assert.Contains(history.Items, x => x.Action == "platform.role.assigned");
        Assert.All(history.Items, item =>
        {
            Assert.Equal(createdUser.UserId, item.UserId);
            Assert.Equal("identity-audit@example.test", item.UserEmail);
            Assert.Equal(PlatformSeeder.DemoAdminUserId, item.ActorUserId);
            Assert.Equal(PlatformSeeder.DemoAdminEmail, item.ActorEmail);
        });
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_user_identity_audit_history()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/identity-audit-history",
                token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_sensitive_user_actions_require_confirmation_header()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", token);
        createRequest.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "confirm-required@example.test",
            "Confirm Required",
            "StrongPass1234"));
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;

        var lockResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{created.UserId}/lock", token));

        Assert.Equal(HttpStatusCode.Conflict, lockResponse.StatusCode);
    }

    [Fact]
    public async Task Owner_only_role_assignment_requires_recent_admin_session()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var session = await db.UserSessions
                .OrderByDescending(x => x.CreatedAt)
                .FirstAsync(x => x.UserId == PlatformSeeder.DemoAdminUserId);
            session.CreatedAt = DateTimeOffset.UtcNow.AddHours(-2);
            await db.SaveChangesAsync();
        }

        var roleRequest = AuthorizedWithConfirmation(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/roles",
            token);
        roleRequest.Content = JsonContent.Create(new AssignPlatformUserRoleRequest("platform_admin"));
        var roleResponse = await _client.SendAsync(roleRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, roleResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_password_reset_revokes_existing_sessions_for_user()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var tenantLoginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        tenantLoginResponse.EnsureSuccessStatusCode();
        var tenantTokens = (await tenantLoginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>())!;

        var resetRequest = AuthorizedWithConfirmation(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/reset-password",
            adminToken);
        const string newPassword = "ResetPass4567!";
        resetRequest.Content = JsonContent.Create(new AdminResetUserPasswordRequest(newPassword));
        var resetResponse = await _client.SendAsync(resetRequest);
        resetResponse.EnsureSuccessStatusCode();

        var renewResponse = await _client.PostAsJsonAsync(
            "/api/auth/renew",
            new RenewSessionRequest(tenantTokens.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, renewResponse.StatusCode);

        var oldPasswordLoginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLoginResponse.StatusCode);

        var newPasswordLoginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                newPassword,
                PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.OK, newPasswordLoginResponse.StatusCode);
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static HttpRequestMessage AuthorizedWithConfirmation(HttpMethod method, string url, string accessToken)
    {
        var request = Authorized(method, url, accessToken);
        request.Headers.Add("X-Admin-Confirm", "CONFIRM");
        return request;
    }

    private static HttpRequestMessage AuthorizedWithDatabaseNukeConfirmation(HttpMethod method, string url, string accessToken)
    {
        var request = Authorized(method, url, accessToken);
        request.Headers.Add("X-Admin-Confirm", ProductDatabaseNukeService.ConfirmationHeaderValue);
        return request;
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-platform-admin-test",
            $"{productKey} Platform Admin Test",
            productKey,
            [productKey]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            [productKey],
            "launch.redeem",
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!.AccessToken;
    }

    private async Task SeedDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private async Task GrantPlatformRoleAsync(Guid userId, string roleKey, Guid? tenantId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        db.PlatformRoleAssignments.Add(new PlatformRoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleKey = roleKey,
            TenantId = tenantId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
        });
        await db.SaveChangesAsync();
    }
}
