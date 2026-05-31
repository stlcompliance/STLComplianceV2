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

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class NexArrPlatformAdminUserTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private string _platformAdminToken = null!;
    private string _tenantAdminToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"NexArrPlatformAdminUsers-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("Auth:BreakGlassAdminEmails", PlatformSeeder.DemoAdminEmail);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();
        await SeedAsync();
        _platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Platform_admin_can_list_users_v1()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/platform-admin/users?page=1&pageSize=10", _platformAdminToken));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PlatformUsersListResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.TotalCount >= 2);
        Assert.Equal(1, payload.Page);
        Assert.Equal(10, payload.PageSize);
        Assert.Contains(payload.Items, item => item.Email == PlatformSeeder.DemoAdminEmail);
        Assert.Contains(payload.Items, item => item.Email == PlatformSeeder.DemoTenantAdminEmail);
    }

    [Fact]
    public async Task Platform_admin_can_search_users_by_email_fragment()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/platform-admin/users?search=tenant-admin", _platformAdminToken));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PlatformUsersListResponse>();
        Assert.NotNull(payload);
        Assert.Single(payload!.Items);
        Assert.Equal(PlatformSeeder.DemoTenantAdminEmail, payload.Items[0].Email);
    }

    [Fact]
    public async Task Tenant_admin_cannot_list_platform_users()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/platform-admin/users", _tenantAdminToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_disable_user_and_then_reenable_user()
    {
        var createResponse = await _client.SendAsync(CreateUserRequest(
            $"disable-test-{Guid.NewGuid():N}@demo.stl",
            "Disable Test User",
            _platformAdminToken));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>();
        Assert.NotNull(created);

        var disableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{created!.UserId}/disable", _platformAdminToken));
        disableResponse.EnsureSuccessStatusCode();
        var disabled = await disableResponse.Content.ReadFromJsonAsync<PlatformUserDisableResponse>();
        Assert.NotNull(disabled);
        Assert.False(disabled!.WasAlreadyDisabled);

        var secondDisableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{created.UserId}/disable", _platformAdminToken));
        secondDisableResponse.EnsureSuccessStatusCode();
        var secondDisabled = await secondDisableResponse.Content.ReadFromJsonAsync<PlatformUserDisableResponse>();
        Assert.NotNull(secondDisabled);
        Assert.True(secondDisabled!.WasAlreadyDisabled);

        var enableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{created.UserId}/enable", _platformAdminToken));
        enableResponse.EnsureSuccessStatusCode();
        var enabled = await enableResponse.Content.ReadFromJsonAsync<PlatformUserEnableResponse>();
        Assert.NotNull(enabled);
        Assert.False(enabled!.WasAlreadyEnabled);
    }

    [Fact]
    public async Task Platform_user_status_and_can_login_are_exposed_in_create_invite_and_list()
    {
        var activeEmail = $"status-active-{Guid.NewGuid():N}@demo.stl";
        var invitedEmail = $"status-invited-{Guid.NewGuid():N}@demo.stl";

        var createResponse = await _client.SendAsync(CreateUserRequest(
            activeEmail,
            "Status Active User",
            _platformAdminToken));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>();
        Assert.NotNull(created);
        Assert.True(created!.CanLogin);
        Assert.Equal("active", created.Status);

        var inviteRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users/invite", _platformAdminToken);
        inviteRequest.Content = JsonContent.Create(new InvitePlatformUserRequest(
            invitedEmail,
            "Status Invited User"));
        var inviteResponse = await _client.SendAsync(inviteRequest);
        inviteResponse.EnsureSuccessStatusCode();
        var invited = await inviteResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>();
        Assert.NotNull(invited);
        Assert.False(invited!.CanLogin);
        Assert.Equal("invited", invited.Status);

        var lockResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{created.UserId}/lock", _platformAdminToken));
        lockResponse.EnsureSuccessStatusCode();

        var disabledResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{created.UserId}/disable", _platformAdminToken));
        disabledResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/platform-admin/users?search=status-", _platformAdminToken));
        listResponse.EnsureSuccessStatusCode();
        var listed = await listResponse.Content.ReadFromJsonAsync<PlatformUsersListResponse>();
        Assert.NotNull(listed);

        var disabledItem = Assert.Single(listed!.Items, x => x.Email == activeEmail);
        Assert.True(disabledItem.CanLogin);
        Assert.Equal("disabled", disabledItem.Status);

        var invitedItem = Assert.Single(listed.Items, x => x.Email == invitedEmail);
        Assert.False(invitedItem.CanLogin);
        Assert.Equal("invited", invitedItem.Status);
    }

    [Fact]
    public async Task Tenant_admin_cannot_disable_platform_users()
    {
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{PlatformSeeder.DemoAdminUserId}/disable", _tenantAdminToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_reset_user_password_and_clear_lockout_state()
    {
        var before = await ReadCredentialStateAsync(PlatformSeeder.DemoTenantAdminUserId);
        Assert.NotNull(before);

        var response = await _client.SendAsync(ResetPasswordRequest(
            PlatformSeeder.DemoTenantAdminUserId,
            "ChangeMe!Reset2026",
            _platformAdminToken));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AdminResetUserPasswordResponse>();
        Assert.NotNull(payload);
        Assert.Equal(PlatformSeeder.DemoTenantAdminUserId, payload!.UserId);

        var after = await ReadCredentialStateAsync(PlatformSeeder.DemoTenantAdminUserId);
        Assert.NotNull(after);
        Assert.NotEqual(before!.PasswordHash, after!.PasswordHash);
        Assert.Null(after.LockedUntil);
        Assert.Equal(0, after.FailedLoginCount);
        Assert.True(after.PasswordChangedAt >= before.PasswordChangedAt);
    }

    [Fact]
    public async Task Tenant_admin_cannot_reset_user_password()
    {
        var response = await _client.SendAsync(ResetPasswordRequest(
            PlatformSeeder.DemoTenantAdminUserId,
            "ChangeMe!Reset2026",
            _tenantAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_view_user_access_history()
    {
        // Generate at least one fresh user access event.
        var _ = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/access-history?page=1&pageSize=20",
                _platformAdminToken));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<PlatformUserAccessHistoryItemResponse>>();
        Assert.NotNull(payload);
        Assert.True(payload!.TotalCount >= 1);
        Assert.All(payload.Items, item => Assert.Equal(PlatformSeeder.DemoTenantAdminUserId, item.UserId));
        Assert.Contains(payload.Items, item => item.Action == "auth.login");
    }

    [Fact]
    public async Task Tenant_admin_cannot_view_user_access_history()
    {
        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/access-history",
                _tenantAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_manage_user_tenant_memberships()
    {
        var createResponse = await _client.SendAsync(CreateUserRequest(
            $"membership-test-{Guid.NewGuid():N}@demo.stl",
            "Membership Test User",
            _platformAdminToken));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>();
        Assert.NotNull(created);

        var assignRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{created!.UserId}/tenant-memberships",
            _platformAdminToken);
        assignRequest.Content = JsonContent.Create(new AssignPlatformUserTenantMembershipRequest(
            PlatformSeeder.DemoTenantId,
            "tenant_admin"));
        var assignResponse = await _client.SendAsync(assignRequest);
        assignResponse.EnsureSuccessStatusCode();
        var assigned = await assignResponse.Content.ReadFromJsonAsync<AssignPlatformUserTenantMembershipResponse>();
        Assert.NotNull(assigned);
        Assert.Equal(created.UserId, assigned!.UserId);
        Assert.Equal(PlatformSeeder.DemoTenantId, assigned.TenantId);
        Assert.False(assigned.WasReactivated);

        var listResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/platform-admin/users/{created.UserId}/tenant-memberships",
                _platformAdminToken));
        listResponse.EnsureSuccessStatusCode();
        var memberships = await listResponse.Content.ReadFromJsonAsync<PlatformUserTenantMembershipsResponse>();
        Assert.NotNull(memberships);
        var membership = Assert.Single(memberships!.Items);
        Assert.Equal(PlatformSeeder.DemoTenantId, membership.TenantId);
        Assert.Equal("tenant_admin", membership.RoleKey);
        Assert.True(membership.IsActive);

        var removeResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Delete,
                $"/api/v1/platform-admin/users/{created.UserId}/tenant-memberships/{PlatformSeeder.DemoTenantId}",
                _platformAdminToken));
        removeResponse.EnsureSuccessStatusCode();
        var removed = await removeResponse.Content.ReadFromJsonAsync<RemovePlatformUserTenantMembershipResponse>();
        Assert.NotNull(removed);
        Assert.False(removed!.WasAlreadyRemoved);

        var listAfterRemoveResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/platform-admin/users/{created.UserId}/tenant-memberships",
                _platformAdminToken));
        listAfterRemoveResponse.EnsureSuccessStatusCode();
        var membershipsAfterRemove = await listAfterRemoveResponse.Content.ReadFromJsonAsync<PlatformUserTenantMembershipsResponse>();
        Assert.NotNull(membershipsAfterRemove);
        var after = Assert.Single(membershipsAfterRemove!.Items);
        Assert.False(after.IsActive);
    }

    [Fact]
    public async Task Tenant_admin_cannot_manage_user_tenant_memberships()
    {
        var listResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/tenant-memberships",
                _tenantAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);

        var assignRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/tenant-memberships",
            _tenantAdminToken);
        assignRequest.Content = JsonContent.Create(new AssignPlatformUserTenantMembershipRequest(
            PlatformSeeder.DemoTenantId,
            "tenant_user"));
        var assignResponse = await _client.SendAsync(assignRequest);
        Assert.Equal(HttpStatusCode.Forbidden, assignResponse.StatusCode);

        var removeResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Delete,
                $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/tenant-memberships/{PlatformSeeder.DemoTenantId}",
                _tenantAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, removeResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_assign_and_remove_platform_role()
    {
        var createResponse = await _client.SendAsync(CreateUserRequest(
            $"role-test-{Guid.NewGuid():N}@demo.stl",
            "Role Test User",
            _platformAdminToken));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>();
        Assert.NotNull(created);
        Assert.False(created!.IsPlatformAdmin);

        var assignRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{created.UserId}/roles",
            _platformAdminToken);
        assignRequest.Headers.Add("X-Admin-Confirm", "CONFIRM");
        assignRequest.Content = JsonContent.Create(new AssignPlatformUserRoleRequest("platform_admin"));
        var assignResponse = await _client.SendAsync(assignRequest);
        assignResponse.EnsureSuccessStatusCode();
        var assigned = await assignResponse.Content.ReadFromJsonAsync<AssignPlatformUserRoleResponse>();
        Assert.NotNull(assigned);
        Assert.False(assigned!.WasAlreadyAssigned);
        Assert.Null(assigned.TenantId);

        var assignTenantScopedRoleRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{created.UserId}/roles",
            _platformAdminToken);
        assignTenantScopedRoleRequest.Content = JsonContent.Create(new AssignPlatformUserRoleRequest(
            "platform_support",
            PlatformSeeder.DemoTenantId));
        var assignTenantScopedRoleResponse = await _client.SendAsync(assignTenantScopedRoleRequest);
        assignTenantScopedRoleResponse.EnsureSuccessStatusCode();
        var tenantScopedAssigned = await assignTenantScopedRoleResponse.Content.ReadFromJsonAsync<AssignPlatformUserRoleResponse>();
        Assert.NotNull(tenantScopedAssigned);
        Assert.False(tenantScopedAssigned!.WasAlreadyAssigned);
        Assert.Equal(PlatformSeeder.DemoTenantId, tenantScopedAssigned.TenantId);

        var listResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/platform-admin/users/{created.UserId}/roles", _platformAdminToken));
        listResponse.EnsureSuccessStatusCode();
        var roles = await listResponse.Content.ReadFromJsonAsync<PlatformUserRolesResponse>();
        Assert.NotNull(roles);
        Assert.Contains(roles!.Items, x => x.RoleKey == "platform_admin" && x.IsAssigned && x.TenantId == null);
        Assert.Contains(roles.Items, x => x.RoleKey == "platform_support" && x.IsAssigned && x.TenantId == PlatformSeeder.DemoTenantId);

        var removeRequest = Authorized(
            HttpMethod.Delete,
            $"/api/v1/platform-admin/users/{created.UserId}/roles/platform_admin",
            _platformAdminToken);
        removeRequest.Headers.Add("X-Admin-Confirm", "CONFIRM");
        var removeResponse = await _client.SendAsync(removeRequest);
        removeResponse.EnsureSuccessStatusCode();
        var removed = await removeResponse.Content.ReadFromJsonAsync<RemovePlatformUserRoleResponse>();
        Assert.NotNull(removed);
        Assert.False(removed!.WasAlreadyRemoved);
        Assert.Null(removed.TenantId);

        var removeTenantScopedResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Delete,
                $"/api/v1/platform-admin/users/{created.UserId}/roles/platform_support?tenantId={PlatformSeeder.DemoTenantId}",
                _platformAdminToken));
        removeTenantScopedResponse.EnsureSuccessStatusCode();
        var tenantScopedRemoved = await removeTenantScopedResponse.Content.ReadFromJsonAsync<RemovePlatformUserRoleResponse>();
        Assert.NotNull(tenantScopedRemoved);
        Assert.False(tenantScopedRemoved!.WasAlreadyRemoved);
        Assert.Equal(PlatformSeeder.DemoTenantId, tenantScopedRemoved.TenantId);

        var listAfterRemoveResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/platform-admin/users/{created.UserId}/roles", _platformAdminToken));
        listAfterRemoveResponse.EnsureSuccessStatusCode();
        var rolesAfterRemove = await listAfterRemoveResponse.Content.ReadFromJsonAsync<PlatformUserRolesResponse>();
        Assert.NotNull(rolesAfterRemove);
        Assert.DoesNotContain(rolesAfterRemove!.Items, x => x.RoleKey == "platform_admin" && x.IsAssigned);
        Assert.DoesNotContain(rolesAfterRemove.Items, x => x.RoleKey == "platform_support" && x.TenantId == PlatformSeeder.DemoTenantId);
    }

    [Fact]
    public async Task Tenant_admin_cannot_manage_platform_roles()
    {
        var listResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/roles",
                _tenantAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);

        var assignRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/roles",
            _tenantAdminToken);
        assignRequest.Headers.Add("X-Admin-Confirm", "CONFIRM");
        assignRequest.Content = JsonContent.Create(new AssignPlatformUserRoleRequest("platform_admin"));
        var assignResponse = await _client.SendAsync(assignRequest);
        Assert.Equal(HttpStatusCode.Forbidden, assignResponse.StatusCode);

        var removeRequest = Authorized(
            HttpMethod.Delete,
            $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/roles/platform_admin",
            _tenantAdminToken);
        removeRequest.Headers.Add("X-Admin-Confirm", "CONFIRM");
        var removeResponse = await _client.SendAsync(removeRequest);
        Assert.Equal(HttpStatusCode.Forbidden, removeResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_support_role_cannot_assign_or_remove_platform_admin_role()
    {
        await GrantGlobalRoleAsync(PlatformSeeder.DemoTenantAdminUserId, "platform_support");
        var supportToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var createResponse = await _client.SendAsync(CreateUserRequest(
            $"owner-guard-test-{Guid.NewGuid():N}@demo.stl",
            "Owner Guard Test User",
            _platformAdminToken));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>();
        Assert.NotNull(created);

        var assignAdminRoleRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{created!.UserId}/roles",
            supportToken);
        assignAdminRoleRequest.Headers.Add("X-Admin-Confirm", "CONFIRM");
        assignAdminRoleRequest.Content = JsonContent.Create(new AssignPlatformUserRoleRequest("platform_admin"));
        var assignAdminRoleResponse = await _client.SendAsync(assignAdminRoleRequest);
        Assert.Equal(HttpStatusCode.Forbidden, assignAdminRoleResponse.StatusCode);

        var removeAdminRoleRequest = Authorized(
            HttpMethod.Delete,
            $"/api/v1/platform-admin/users/{PlatformSeeder.DemoAdminUserId}/roles/platform_admin",
            supportToken);
        removeAdminRoleRequest.Headers.Add("X-Admin-Confirm", "CONFIRM");
        var removeAdminRoleResponse = await _client.SendAsync(removeAdminRoleRequest);
        Assert.Equal(HttpStatusCode.Forbidden, removeAdminRoleResponse.StatusCode);
    }

    [Fact]
    public async Task Break_glass_admin_cannot_be_disabled_locked_or_demoted()
    {
        var disableResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/v1/platform-admin/users/{PlatformSeeder.DemoAdminUserId}/disable",
                _platformAdminToken));
        Assert.Equal(HttpStatusCode.Conflict, disableResponse.StatusCode);

        var lockResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Post,
                $"/api/v1/platform-admin/users/{PlatformSeeder.DemoAdminUserId}/lock",
                _platformAdminToken));
        Assert.Equal(HttpStatusCode.Conflict, lockResponse.StatusCode);

        var demoteRequest = Authorized(
            HttpMethod.Patch,
            $"/api/v1/platform-admin/users/{PlatformSeeder.DemoAdminUserId}",
            _platformAdminToken);
        demoteRequest.Content = JsonContent.Create(new UpdatePlatformUserRequest(
            PlatformSeeder.DemoAdminEmail,
            "Platform Administrator",
            IsPlatformAdmin: false));
        var demoteResponse = await _client.SendAsync(demoteRequest);
        Assert.Equal(HttpStatusCode.Conflict, demoteResponse.StatusCode);

        var removeRoleRequest = Authorized(
            HttpMethod.Delete,
            $"/api/v1/platform-admin/users/{PlatformSeeder.DemoAdminUserId}/roles/platform_admin",
            _platformAdminToken);
        removeRoleRequest.Headers.Add("X-Admin-Confirm", "CONFIRM");
        var removeRoleResponse = await _client.SendAsync(removeRoleRequest);
        Assert.Equal(HttpStatusCode.Conflict, removeRoleResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_owner_confirmation_is_required_for_platform_admin_role_changes()
    {
        var createResponse = await _client.SendAsync(CreateUserRequest(
            $"confirm-role-test-{Guid.NewGuid():N}@demo.stl",
            "Confirm Role Test User",
            _platformAdminToken));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>();
        Assert.NotNull(created);

        var assignRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/platform-admin/users/{created!.UserId}/roles",
            _platformAdminToken);
        assignRequest.Content = JsonContent.Create(new AssignPlatformUserRoleRequest("platform_admin"));

        var assignResponse = await _client.SendAsync(assignRequest);
        Assert.Equal(HttpStatusCode.Conflict, assignResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_manage_external_identity_provider_mappings()
    {
        var createResponse = await _client.SendAsync(CreateUserRequest(
            $"idp-map-{Guid.NewGuid():N}@demo.stl",
            "External Identity User",
            _platformAdminToken));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>();
        Assert.NotNull(created);

        var upsertRequest = Authorized(
            HttpMethod.Put,
            $"/api/v1/platform-admin/users/{created!.UserId}/external-identity-mappings",
            _platformAdminToken);
        upsertRequest.Content = JsonContent.Create(new UpsertPlatformUserExternalIdentityProviderMappingRequest(
            "entra-id",
            "oidc-subject-123",
            "external-user@example.test"));
        var upsertResponse = await _client.SendAsync(upsertRequest);
        upsertResponse.EnsureSuccessStatusCode();
        var createdMapping = await upsertResponse.Content.ReadFromJsonAsync<UpsertPlatformUserExternalIdentityProviderMappingResponse>();
        Assert.NotNull(createdMapping);
        Assert.False(createdMapping!.WasUpdated);

        var updateRequest = Authorized(
            HttpMethod.Put,
            $"/api/v1/platform-admin/users/{created.UserId}/external-identity-mappings",
            _platformAdminToken);
        updateRequest.Content = JsonContent.Create(new UpsertPlatformUserExternalIdentityProviderMappingRequest(
            "entra-id",
            "oidc-subject-123-updated",
            "external-user-updated@example.test"));
        var updateResponse = await _client.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updatedMapping = await updateResponse.Content.ReadFromJsonAsync<UpsertPlatformUserExternalIdentityProviderMappingResponse>();
        Assert.NotNull(updatedMapping);
        Assert.True(updatedMapping!.WasUpdated);
        Assert.Equal(createdMapping.MappingId, updatedMapping.MappingId);
        Assert.Equal("oidc-subject-123-updated", updatedMapping.ExternalSubject);

        var listResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/platform-admin/users/{created.UserId}/external-identity-mappings",
                _platformAdminToken));
        listResponse.EnsureSuccessStatusCode();
        var list = await listResponse.Content.ReadFromJsonAsync<PlatformUserExternalIdentityProviderMappingsResponse>();
        Assert.NotNull(list);
        var mapping = Assert.Single(list!.Items);
        Assert.Equal(updatedMapping.MappingId, mapping.MappingId);
        Assert.Equal("entra-id", mapping.ProviderKey);
        Assert.Equal("oidc-subject-123-updated", mapping.ExternalSubject);
        Assert.Equal("external-user-updated@example.test", mapping.ExternalEmail);

        var removeResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Delete,
                $"/api/v1/platform-admin/users/{created.UserId}/external-identity-mappings/{mapping.MappingId}",
                _platformAdminToken));
        removeResponse.EnsureSuccessStatusCode();
        var removed = await removeResponse.Content.ReadFromJsonAsync<RemovePlatformUserExternalIdentityProviderMappingResponse>();
        Assert.NotNull(removed);
        Assert.False(removed!.WasAlreadyRemoved);

        var listAfterRemoveResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/platform-admin/users/{created.UserId}/external-identity-mappings",
                _platformAdminToken));
        listAfterRemoveResponse.EnsureSuccessStatusCode();
        var listAfterRemove = await listAfterRemoveResponse.Content.ReadFromJsonAsync<PlatformUserExternalIdentityProviderMappingsResponse>();
        Assert.NotNull(listAfterRemove);
        Assert.Empty(listAfterRemove!.Items);
    }

    [Fact]
    public async Task Tenant_admin_cannot_manage_external_identity_provider_mappings()
    {
        var listResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/external-identity-mappings",
                _tenantAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);

        var upsertRequest = Authorized(
            HttpMethod.Put,
            $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/external-identity-mappings",
            _tenantAdminToken);
        upsertRequest.Content = JsonContent.Create(new UpsertPlatformUserExternalIdentityProviderMappingRequest(
            "entra-id",
            "tenant-admin-subject"));
        var upsertResponse = await _client.SendAsync(upsertRequest);
        Assert.Equal(HttpStatusCode.Forbidden, upsertResponse.StatusCode);
    }

    private async Task SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(token);
        return token!.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static HttpRequestMessage CreateUserRequest(string email, string displayName, string token)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", token);
        request.Content = JsonContent.Create(new CreatePlatformUserRequest(
            email,
            displayName,
            "ChangeMe!Demo2026",
            IsPlatformAdmin: false,
            IsActive: true));
        return request;
    }

    private static HttpRequestMessage ResetPasswordRequest(Guid userId, string newPassword, string token)
    {
        var request = Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{userId}/reset-password", token);
        request.Content = JsonContent.Create(new AdminResetUserPasswordRequest(newPassword));
        return request;
    }

    private async Task<UserCredentialState?> ReadCredentialStateAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        return await db.UserCredentials
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new UserCredentialState(x.PasswordHash, x.PasswordChangedAt, x.FailedLoginCount, x.LockedUntil))
            .FirstOrDefaultAsync();
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

    private sealed record UserCredentialState(
        string PasswordHash,
        DateTimeOffset PasswordChangedAt,
        int FailedLoginCount,
        DateTimeOffset? LockedUntil);

    private async Task GrantGlobalRoleAsync(Guid userId, string roleKey)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var exists = await db.PlatformRoleAssignments.AnyAsync(
            x => x.UserId == userId && x.TenantId == null && x.RoleKey == roleKey);
        if (exists)
        {
            return;
        }

        db.PlatformRoleAssignments.Add(new PlatformRoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = null,
            RoleKey = roleKey,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
        });
        await db.SaveChangesAsync();
    }
}
