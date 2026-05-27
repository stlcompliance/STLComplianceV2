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
using StaffArrRedeemRequest = StaffArr.Api.Contracts.RedeemHandoffRequest;
using StaffArrSessionBootstrapResponse = StaffArr.Api.Contracts.StaffArrSessionBootstrapResponse;
using StaffArrMeResponse = StaffArr.Api.Contracts.StaffArrMeResponse;
using StaffArrHandoffSessionResponse = StaffArr.Api.Contracts.HandoffSessionResponse;
using StaffPersonSummaryResponse = StaffArr.Api.Contracts.StaffPersonSummaryResponse;
using StaffPersonDetailResponse = StaffArr.Api.Contracts.StaffPersonDetailResponse;
using CreateStaffPersonRequest = StaffArr.Api.Contracts.CreateStaffPersonRequest;
using OrgUnitResponse = StaffArr.Api.Contracts.OrgUnitResponse;
using CreateOrgUnitRequest = StaffArr.Api.Contracts.CreateOrgUnitRequest;
using UpdateOrgUnitRequest = StaffArr.Api.Contracts.UpdateOrgUnitRequest;
using UpdateOrgUnitStatusRequest = StaffArr.Api.Contracts.UpdateOrgUnitStatusRequest;
using OrgUnitAssignmentResponse = StaffArr.Api.Contracts.OrgUnitAssignmentResponse;
using CreateOrgUnitAssignmentRequest = StaffArr.Api.Contracts.CreateOrgUnitAssignmentRequest;
using UpdateOrgUnitAssignmentRequest = StaffArr.Api.Contracts.UpdateOrgUnitAssignmentRequest;
using UpdateOrgUnitAssignmentStatusRequest = StaffArr.Api.Contracts.UpdateOrgUnitAssignmentStatusRequest;
using UpdatePersonManagerRequest = StaffArr.Api.Contracts.UpdatePersonManagerRequest;
using PersonManagerResponse = StaffArr.Api.Contracts.PersonManagerResponse;
using ManagerChainEntryResponse = StaffArr.Api.Contracts.ManagerChainEntryResponse;
using SubordinateSummaryResponse = StaffArr.Api.Contracts.SubordinateSummaryResponse;
using PermissionTemplateSummaryResponse = StaffArr.Api.Contracts.PermissionTemplateSummaryResponse;
using RoleTemplateResponse = StaffArr.Api.Contracts.RoleTemplateResponse;
using UpsertPermissionTemplateRequest = StaffArr.Api.Contracts.UpsertPermissionTemplateRequest;
using CreateRoleTemplateRequest = StaffArr.Api.Contracts.CreateRoleTemplateRequest;
using RoleTemplatePermissionInput = StaffArr.Api.Contracts.RoleTemplatePermissionInput;
using UpdateRoleTemplateRequest = StaffArr.Api.Contracts.UpdateRoleTemplateRequest;
using PersonRoleAssignmentResponse = StaffArr.Api.Contracts.PersonRoleAssignmentResponse;
using CreatePersonRoleAssignmentRequest = StaffArr.Api.Contracts.CreatePersonRoleAssignmentRequest;
using UpdatePersonRoleAssignmentStatusRequest = StaffArr.Api.Contracts.UpdatePersonRoleAssignmentStatusRequest;
using EffectivePermissionProjectionResponse = StaffArr.Api.Contracts.EffectivePermissionProjectionResponse;
using PermissionHistoryTimelineEntryResponse = StaffArr.Api.Contracts.PermissionHistoryTimelineEntryResponse;
using StaffArrTokenService = StaffArr.Api.Services.StaffArrTokenService;
using StaffArrDbContext = StaffArr.Api.Data.StaffArrDbContext;
using StaffPerson = StaffArr.Api.Entities.StaffPerson;
using OrgUnit = StaffArr.Api.Entities.OrgUnit;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrHandoffApiTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _serviceToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"StaffArrHandoffNexArrTests-{Guid.NewGuid():N}";
        var staffArrDbName = $"StaffArrHandoffTests-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
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
                    options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _serviceToken = await IssueServiceTokenAsync(adminToken, "staffarr");

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _serviceToken);
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<global::StaffArr.Api.Data.StaffArrDbContext>)
                        || d.ServiceType == typeof(global::StaffArr.Api.Data.StaffArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<global::StaffArr.Api.Data.StaffArrDbContext>(options =>
                    options.UseInMemoryDatabase(staffArrDbName));

                services.AddHttpClient<global::StaffArr.Api.Services.NexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Handoff_redeem_happy_path_returns_session_and_me_works()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _staffarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new StaffArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<StaffArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Equal(PlatformSeeder.DemoAdminUserId, session.UserId);
        Assert.NotEqual(Guid.Empty, session.PersonId);
        Assert.Contains(session.TenantRoleKey, new[] { "tenant_admin", "platform_admin" });
        Assert.Contains("staffarr", session.Entitlements);

        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        var meResponse = await _staffarrClient.SendAsync(meRequest);
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<StaffArrMeResponse>();
        Assert.NotNull(me);
        Assert.True(me.HasStaffArrEntitlement);
        Assert.Contains(me.TenantRoleKey, new[] { "tenant_admin", "platform_admin" });
        Assert.Equal(session.PersonId, me.PersonId);
    }

    [Fact]
    public async Task Handoff_redeem_invalid_code_returns_unauthorized()
    {
        var response = await _staffarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new StaffArrRedeemRequest("not-a-valid-handoff-code"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handoff_redeem_revoked_entitlement_is_rejected()
    {
        await RevokeStaffArrEntitlementAsync();
        var handoffCode = await CreateHandoffAsync();

        var response = await _staffarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new StaffArrRedeemRequest(handoffCode));

        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden });
    }

    [Fact]
    public async Task Me_requires_authenticated_user()
    {
        var response = await _staffarrClient.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_forbids_users_without_staffarr_entitlement_claim()
    {
        var token = CreateStaffArrAccessToken(["nexarr"]);
        var request = Authorized(HttpMethod.Get, "/api/me", token);
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Session_bootstrap_returns_claim_backed_identity()
    {
        var token = CreateStaffArrAccessToken(["staffarr", "nexarr"], tenantRoleKey: "tenant_admin");
        var request = Authorized(HttpMethod.Get, "/api/session", token);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<StaffArrSessionBootstrapResponse>();
        Assert.NotNull(payload);
        Assert.Equal(PlatformSeeder.DemoAdminUserId, payload.UserId);
        Assert.NotEqual(Guid.Empty, payload.PersonId);
        Assert.Equal("tenant_admin", payload.TenantRoleKey);
        Assert.True(payload.HasStaffArrEntitlement);
    }

    [Fact]
    public async Task People_directory_denies_member_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member");
        var request = Authorized(HttpMethod.Get, "/api/people", token);
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task People_directory_returns_records_for_tenant_admin()
    {
        await SeedStaffPersonAsync(Guid.NewGuid(), "Directory User", "directory.user@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var request = Authorized(HttpMethod.Get, "/api/people", token);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<StaffPersonSummaryResponse>>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload);
    }

    [Fact]
    public async Task People_profile_allows_self_access_for_member_role()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Member User", "member.user@example.com", externalUserId: PlatformSeeder.DemoAdminUserId);
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: personId);
        var request = Authorized(HttpMethod.Get, $"/api/people/{personId}", token);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<StaffPersonDetailResponse>();
        Assert.NotNull(payload);
        Assert.Equal(personId, payload.PersonId);
    }

    [Fact]
    public async Task People_create_validates_input_and_permissions()
    {
        var memberToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var deniedRequest = Authorized(HttpMethod.Post, "/api/people", memberToken);
        deniedRequest.Content = JsonContent.Create(new CreateStaffPersonRequest(
            "No",
            "Write",
            "no.write@example.com",
            "active",
            null,
            null,
            "Inspector"));
        var deniedResponse = await _staffarrClient.SendAsync(deniedRequest);
        Assert.Equal(HttpStatusCode.Forbidden, deniedResponse.StatusCode);

        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var badRequest = Authorized(HttpMethod.Post, "/api/people", adminToken);
        badRequest.Content = JsonContent.Create(new CreateStaffPersonRequest(
            "Bad",
            "Email",
            "not-an-email",
            "active",
            null,
            null,
            null));
        var badResponse = await _staffarrClient.SendAsync(badRequest);
        Assert.Equal(HttpStatusCode.BadRequest, badResponse.StatusCode);
    }

    [Fact]
    public async Task Org_unit_write_happy_path_update_and_status_flow()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/org-units", token);
        createRequest.Content = JsonContent.Create(new CreateOrgUnitRequest("department", "Operations", null));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;

        var updateRequest = Authorized(HttpMethod.Put, $"/api/org-units/{created.OrgUnitId}", token);
        updateRequest.Content = JsonContent.Create(new UpdateOrgUnitRequest("department", "Operations HQ", null));
        var updateResponse = await _staffarrClient.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;
        Assert.Equal("Operations HQ", updated.Name);

        var statusRequest = Authorized(HttpMethod.Patch, $"/api/org-units/{created.OrgUnitId}/status", token);
        statusRequest.Content = JsonContent.Create(new UpdateOrgUnitStatusRequest("inactive"));
        var statusResponse = await _staffarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var statusPayload = (await statusResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;
        Assert.Equal("inactive", statusPayload.Status);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditEvents = await db.AuditEvents.CountAsync(x => x.TenantId == PlatformSeeder.DemoTenantId);
        Assert.True(auditEvents >= 3);
    }

    [Fact]
    public async Task Org_unit_write_denies_non_writer_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var request = Authorized(HttpMethod.Post, "/api/org-units", token);
        request.Content = JsonContent.Create(new CreateOrgUnitRequest("department", "Denied Unit", null));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Org_unit_write_rejects_invalid_hierarchy_and_conflicts()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var rootId = await SeedOrgUnitAsync("department", "Root Unit", null, "active");
        var childId = await SeedOrgUnitAsync("team", "Child Unit", rootId, "active");
        await SeedOrgUnitAsync("department", "Root Unit", null, "active", tenantId: Guid.NewGuid());

        var duplicateRequest = Authorized(HttpMethod.Post, "/api/org-units", token);
        duplicateRequest.Content = JsonContent.Create(new CreateOrgUnitRequest("department", "Root Unit", null));
        var duplicateResponse = await _staffarrClient.SendAsync(duplicateRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var cycleRequest = Authorized(HttpMethod.Put, $"/api/org-units/{rootId}", token);
        cycleRequest.Content = JsonContent.Create(new UpdateOrgUnitRequest("department", "Root Unit", childId));
        var cycleResponse = await _staffarrClient.SendAsync(cycleRequest);
        Assert.Equal(HttpStatusCode.Conflict, cycleResponse.StatusCode);

        var deactivateParentRequest = Authorized(HttpMethod.Patch, $"/api/org-units/{rootId}/status", token);
        deactivateParentRequest.Content = JsonContent.Create(new UpdateOrgUnitStatusRequest("inactive"));
        var deactivateParentResponse = await _staffarrClient.SendAsync(deactivateParentRequest);
        Assert.Equal(HttpStatusCode.Conflict, deactivateParentResponse.StatusCode);
    }

    [Fact]
    public async Task Org_assignment_write_happy_path_update_and_status_flow()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Assignment User", "assignment.user@example.com");
        var siteId = await SeedOrgUnitAsync("site", "East Site", null, "active");
        var deptId = await SeedOrgUnitAsync("department", "Operations", siteId, "active");
        var teamId = await SeedOrgUnitAsync("team", "Field Team", deptId, "active");
        var positionId = await SeedOrgUnitAsync("position", "Operator", teamId, "active");
        var position2Id = await SeedOrgUnitAsync("position", "Lead Operator", teamId, "active");

        var createRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/org-assignments", token);
        createRequest.Content = JsonContent.Create(new CreateOrgUnitAssignmentRequest(siteId, deptId, teamId, positionId));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<OrgUnitAssignmentResponse>())!;

        var listResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/org-assignments", token));
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<OrgUnitAssignmentResponse>>())!;
        Assert.Single(listed);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/people/{personId}/org-assignments/{created.AssignmentId}", token);
        updateRequest.Content = JsonContent.Create(new UpdateOrgUnitAssignmentRequest(siteId, deptId, teamId, position2Id));
        var updateResponse = await _staffarrClient.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<OrgUnitAssignmentResponse>())!;
        Assert.Equal(position2Id, updated.PositionOrgUnitId);

        var statusRequest = Authorized(
            HttpMethod.Patch,
            $"/api/people/{personId}/org-assignments/{created.AssignmentId}/status",
            token);
        statusRequest.Content = JsonContent.Create(new UpdateOrgUnitAssignmentStatusRequest("inactive"));
        var statusResponse = await _staffarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var statusPayload = (await statusResponse.Content.ReadFromJsonAsync<OrgUnitAssignmentResponse>())!;
        Assert.Equal("inactive", statusPayload.Status);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditEvents = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action.StartsWith("org_assignment."));
        Assert.True(auditEvents >= 3);
    }

    [Fact]
    public async Task Org_assignment_write_denies_non_writer_role()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Denied Assignment", "assignment.denied@example.com");
        var siteId = await SeedOrgUnitAsync("site", "Denied Site", null, "active");
        var deptId = await SeedOrgUnitAsync("department", "Denied Department", siteId, "active");
        var teamId = await SeedOrgUnitAsync("team", "Denied Team", deptId, "active");
        var positionId = await SeedOrgUnitAsync("position", "Denied Position", teamId, "active");

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var request = Authorized(HttpMethod.Post, $"/api/people/{personId}/org-assignments", token);
        request.Content = JsonContent.Create(new CreateOrgUnitAssignmentRequest(siteId, deptId, teamId, positionId));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Org_assignment_write_rejects_invalid_linkage_duplicate_and_inactive_refs()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Invalid Assignment", "assignment.invalid@example.com");

        var siteAId = await SeedOrgUnitAsync("site", "Site A", null, "active");
        var deptAId = await SeedOrgUnitAsync("department", "Dept A", siteAId, "active");
        var teamAId = await SeedOrgUnitAsync("team", "Team A", deptAId, "active");
        var positionAId = await SeedOrgUnitAsync("position", "Position A", teamAId, "active");

        var siteBId = await SeedOrgUnitAsync("site", "Site B", null, "active");
        var deptBId = await SeedOrgUnitAsync("department", "Dept B", siteBId, "active");
        var inactiveTeamBId = await SeedOrgUnitAsync("team", "Team B", deptBId, "inactive");
        var positionBId = await SeedOrgUnitAsync("position", "Position B", inactiveTeamBId, "active");

        var invalidLinkRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/org-assignments", token);
        invalidLinkRequest.Content = JsonContent.Create(new CreateOrgUnitAssignmentRequest(siteAId, deptBId, inactiveTeamBId, positionBId));
        var invalidLinkResponse = await _staffarrClient.SendAsync(invalidLinkRequest);
        Assert.Equal(HttpStatusCode.Conflict, invalidLinkResponse.StatusCode);

        var inactiveRefRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/org-assignments", token);
        inactiveRefRequest.Content = JsonContent.Create(new CreateOrgUnitAssignmentRequest(siteBId, deptBId, inactiveTeamBId, positionBId));
        var inactiveRefResponse = await _staffarrClient.SendAsync(inactiveRefRequest);
        Assert.Equal(HttpStatusCode.Conflict, inactiveRefResponse.StatusCode);

        var createRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/org-assignments", token);
        createRequest.Content = JsonContent.Create(new CreateOrgUnitAssignmentRequest(siteAId, deptAId, teamAId, positionAId));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var duplicateRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/org-assignments", token);
        duplicateRequest.Content = JsonContent.Create(new CreateOrgUnitAssignmentRequest(siteAId, deptAId, teamAId, positionAId));
        var duplicateResponse = await _staffarrClient.SendAsync(duplicateRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Manager_hierarchy_happy_path_supports_update_chain_and_subordinate_views()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var managerId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        await SeedStaffPersonAsync(managerId, "Manager One", "manager.one@example.com");
        await SeedStaffPersonAsync(leadId, "Lead One", "lead.one@example.com", managerPersonId: managerId);
        await SeedStaffPersonAsync(workerId, "Worker One", "worker.one@example.com");

        var siteId = await SeedOrgUnitAsync("site", "HQ", null, "active");
        var deptId = await SeedOrgUnitAsync("department", "Operations", siteId, "active");
        var teamId = await SeedOrgUnitAsync("team", "Alpha Team", deptId, "active");
        var positionId = await SeedOrgUnitAsync("position", "Operator", teamId, "active");

        var assignmentRequest = Authorized(HttpMethod.Post, $"/api/people/{workerId}/org-assignments", token);
        assignmentRequest.Content = JsonContent.Create(new CreateOrgUnitAssignmentRequest(siteId, deptId, teamId, positionId));
        var assignmentResponse = await _staffarrClient.SendAsync(assignmentRequest);
        assignmentResponse.EnsureSuccessStatusCode();

        var updateManagerRequest = Authorized(HttpMethod.Put, $"/api/people/{workerId}/manager", token);
        updateManagerRequest.Content = JsonContent.Create(new UpdatePersonManagerRequest(leadId));
        var updateManagerResponse = await _staffarrClient.SendAsync(updateManagerRequest);
        updateManagerResponse.EnsureSuccessStatusCode();
        var managerPayload = (await updateManagerResponse.Content.ReadFromJsonAsync<PersonManagerResponse>())!;
        Assert.Equal(leadId, managerPayload.ManagerPersonId);

        var chainResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{workerId}/manager-chain", token));
        chainResponse.EnsureSuccessStatusCode();
        var chain = (await chainResponse.Content.ReadFromJsonAsync<IReadOnlyList<ManagerChainEntryResponse>>())!;
        Assert.Equal(2, chain.Count);
        Assert.Equal(leadId, chain[0].PersonId);
        Assert.Equal(managerId, chain[1].PersonId);

        var subordinatesResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{managerId}/subordinates?includeIndirect=true", token));
        subordinatesResponse.EnsureSuccessStatusCode();
        var subordinateList = (await subordinatesResponse.Content.ReadFromJsonAsync<IReadOnlyList<SubordinateSummaryResponse>>())!;
        Assert.Equal(2, subordinateList.Count);
        Assert.Contains(subordinateList, x => x.PersonId == workerId && x.Depth == 2);

        var subordinateDetailResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{managerId}/subordinates/{workerId}", token));
        subordinateDetailResponse.EnsureSuccessStatusCode();
        var subordinateDetail = (await subordinateDetailResponse.Content.ReadFromJsonAsync<SubordinateSummaryResponse>())!;
        Assert.Equal(2, subordinateDetail.Depth);
        Assert.Contains("HQ / Operations / Alpha Team / Operator", subordinateDetail.ActiveAssignmentPath);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditEvents = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "people.manager_update");
        Assert.True(auditEvents >= 1);
    }

    [Fact]
    public async Task Manager_update_denies_non_writer_role()
    {
        var managerId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(managerId, "Denied Manager", "denied.manager@example.com");
        await SeedStaffPersonAsync(personId, "Denied User", "denied.user@example.com");

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var request = Authorized(HttpMethod.Put, $"/api/people/{personId}/manager", token);
        request.Content = JsonContent.Create(new UpdatePersonManagerRequest(managerId));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Manager_update_rejects_self_cycle_and_unknown_manager()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personAId = Guid.NewGuid();
        var personBId = Guid.NewGuid();

        await SeedStaffPersonAsync(personAId, "Cycle A", "cycle.a@example.com", managerPersonId: personBId);
        await SeedStaffPersonAsync(personBId, "Cycle B", "cycle.b@example.com");

        var selfRequest = Authorized(HttpMethod.Put, $"/api/people/{personAId}/manager", token);
        selfRequest.Content = JsonContent.Create(new UpdatePersonManagerRequest(personAId));
        var selfResponse = await _staffarrClient.SendAsync(selfRequest);
        Assert.Equal(HttpStatusCode.BadRequest, selfResponse.StatusCode);

        var cycleRequest = Authorized(HttpMethod.Put, $"/api/people/{personBId}/manager", token);
        cycleRequest.Content = JsonContent.Create(new UpdatePersonManagerRequest(personAId));
        var cycleResponse = await _staffarrClient.SendAsync(cycleRequest);
        Assert.Equal(HttpStatusCode.Conflict, cycleResponse.StatusCode);

        var missingManagerRequest = Authorized(HttpMethod.Put, $"/api/people/{personBId}/manager", token);
        missingManagerRequest.Content = JsonContent.Create(new UpdatePersonManagerRequest(Guid.NewGuid()));
        var missingManagerResponse = await _staffarrClient.SendAsync(missingManagerRequest);
        Assert.Equal(HttpStatusCode.NotFound, missingManagerResponse.StatusCode);
    }

    [Fact]
    public async Task Role_template_and_permission_assignment_happy_path_supports_template_and_person_assignment_flows()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Role Assignment User", "role.assignment.user@example.com");

        var permissionRequest = Authorized(HttpMethod.Post, "/api/permissions", token);
        permissionRequest.Content = JsonContent.Create(new UpsertPermissionTemplateRequest(
            "staffarr.permissions.assign",
            "Permission Assignment",
            "Allows assignment of permission templates."));
        var permissionResponse = await _staffarrClient.SendAsync(permissionRequest);
        permissionResponse.EnsureSuccessStatusCode();
        var permissionTemplate = (await permissionResponse.Content.ReadFromJsonAsync<PermissionTemplateSummaryResponse>())!;

        var roleRequest = Authorized(HttpMethod.Post, "/api/roles", token);
        roleRequest.Content = JsonContent.Create(new CreateRoleTemplateRequest(
            "staffarr.supervisor",
            "StaffArr Supervisor",
            "Supervises teams.",
            [new RoleTemplatePermissionInput(permissionTemplate.PermissionTemplateId, "tenant", null)]));
        var roleResponse = await _staffarrClient.SendAsync(roleRequest);
        roleResponse.EnsureSuccessStatusCode();
        var roleTemplate = (await roleResponse.Content.ReadFromJsonAsync<RoleTemplateResponse>())!;
        Assert.Single(roleTemplate.Permissions);

        var rolesListResponse = await _staffarrClient.SendAsync(Authorized(HttpMethod.Get, "/api/roles", token));
        rolesListResponse.EnsureSuccessStatusCode();
        var rolesList = (await rolesListResponse.Content.ReadFromJsonAsync<IReadOnlyList<RoleTemplateResponse>>())!;
        Assert.Contains(rolesList, x => x.RoleTemplateId == roleTemplate.RoleTemplateId);

        var assignmentRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/role-assignments", token);
        assignmentRequest.Content = JsonContent.Create(new CreatePersonRoleAssignmentRequest(
            roleTemplate.RoleTemplateId,
            "tenant",
            null));
        var assignmentResponse = await _staffarrClient.SendAsync(assignmentRequest);
        assignmentResponse.EnsureSuccessStatusCode();
        var assignment = (await assignmentResponse.Content.ReadFromJsonAsync<PersonRoleAssignmentResponse>())!;
        Assert.Equal("staffarr.supervisor", assignment.RoleKey);

        var assignmentStatusRequest = Authorized(
            HttpMethod.Patch,
            $"/api/people/{personId}/role-assignments/{assignment.AssignmentId}/status",
            token);
        assignmentStatusRequest.Content = JsonContent.Create(new UpdatePersonRoleAssignmentStatusRequest("inactive"));
        var assignmentStatusResponse = await _staffarrClient.SendAsync(assignmentStatusRequest);
        assignmentStatusResponse.EnsureSuccessStatusCode();
        var assignmentStatus = (await assignmentStatusResponse.Content.ReadFromJsonAsync<PersonRoleAssignmentResponse>())!;
        Assert.Equal("inactive", assignmentStatus.Status);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditEvents = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && (x.Action.StartsWith("role_template.")
                    || x.Action.StartsWith("permission_template.")
                    || x.Action.StartsWith("person_role_assignment.")));
        Assert.True(auditEvents >= 3);
    }

    [Fact]
    public async Task Role_template_write_denies_non_writer_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var request = Authorized(HttpMethod.Post, "/api/permissions", token);
        request.Content = JsonContent.Create(new UpsertPermissionTemplateRequest(
            "staffarr.permissions.assign",
            "Permission Assignment",
            null));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Role_assignment_rejects_inactive_role_templates()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Inactive Role User", "inactive.role.user@example.com");

        var permissionRequest = Authorized(HttpMethod.Post, "/api/permissions", token);
        permissionRequest.Content = JsonContent.Create(new UpsertPermissionTemplateRequest(
            "staffarr.people.read",
            "People Read",
            null));
        var permissionResponse = await _staffarrClient.SendAsync(permissionRequest);
        permissionResponse.EnsureSuccessStatusCode();
        var permissionTemplate = (await permissionResponse.Content.ReadFromJsonAsync<PermissionTemplateSummaryResponse>())!;

        var roleRequest = Authorized(HttpMethod.Post, "/api/roles", token);
        roleRequest.Content = JsonContent.Create(new CreateRoleTemplateRequest(
            "staffarr.viewer",
            "StaffArr Viewer",
            null,
            [new RoleTemplatePermissionInput(permissionTemplate.PermissionTemplateId, "tenant", null)]));
        var roleResponse = await _staffarrClient.SendAsync(roleRequest);
        roleResponse.EnsureSuccessStatusCode();
        var roleTemplate = (await roleResponse.Content.ReadFromJsonAsync<RoleTemplateResponse>())!;

        var deactivateRoleRequest = Authorized(HttpMethod.Put, $"/api/roles/{roleTemplate.RoleTemplateId}", token);
        deactivateRoleRequest.Content = JsonContent.Create(new UpdateRoleTemplateRequest(
            roleTemplate.Name,
            roleTemplate.Description,
            "inactive",
            [new RoleTemplatePermissionInput(permissionTemplate.PermissionTemplateId, "tenant", null)]));
        var deactivateRoleResponse = await _staffarrClient.SendAsync(deactivateRoleRequest);
        deactivateRoleResponse.EnsureSuccessStatusCode();

        var assignmentRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/role-assignments", token);
        assignmentRequest.Content = JsonContent.Create(new CreatePersonRoleAssignmentRequest(
            roleTemplate.RoleTemplateId,
            "tenant",
            null));
        var assignmentResponse = await _staffarrClient.SendAsync(assignmentRequest);
        Assert.Equal(HttpStatusCode.Conflict, assignmentResponse.StatusCode);
    }

    [Fact]
    public async Task Permission_projection_and_history_timeline_reflect_assignment_and_status_changes()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Projection User", "projection.user@example.com");

        var permissionRequest = Authorized(HttpMethod.Post, "/api/permissions", token);
        permissionRequest.Content = JsonContent.Create(new UpsertPermissionTemplateRequest(
            "staffarr.people.read",
            "People Read",
            "Read access."));
        var permissionResponse = await _staffarrClient.SendAsync(permissionRequest);
        permissionResponse.EnsureSuccessStatusCode();
        var permissionTemplate = (await permissionResponse.Content.ReadFromJsonAsync<PermissionTemplateSummaryResponse>())!;

        var roleRequest = Authorized(HttpMethod.Post, "/api/roles", token);
        roleRequest.Content = JsonContent.Create(new CreateRoleTemplateRequest(
            "staffarr.viewer",
            "StaffArr Viewer",
            "Viewer role.",
            [new RoleTemplatePermissionInput(permissionTemplate.PermissionTemplateId, "tenant", null)]));
        var roleResponse = await _staffarrClient.SendAsync(roleRequest);
        roleResponse.EnsureSuccessStatusCode();
        var roleTemplate = (await roleResponse.Content.ReadFromJsonAsync<RoleTemplateResponse>())!;

        var assignmentRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/role-assignments", token);
        assignmentRequest.Content = JsonContent.Create(new CreatePersonRoleAssignmentRequest(
            roleTemplate.RoleTemplateId,
            "tenant",
            null));
        var assignmentResponse = await _staffarrClient.SendAsync(assignmentRequest);
        assignmentResponse.EnsureSuccessStatusCode();
        var assignment = (await assignmentResponse.Content.ReadFromJsonAsync<PersonRoleAssignmentResponse>())!;

        var projectionResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/permissions/effective", token));
        projectionResponse.EnsureSuccessStatusCode();
        var projection = (await projectionResponse.Content.ReadFromJsonAsync<EffectivePermissionProjectionResponse>())!;
        Assert.Contains(projection.Permissions, p => p.PermissionKey == "staffarr.people.read" && p.ScopeType == "tenant");

        var timelineResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/permissions/history?limit=20", token));
        timelineResponse.EnsureSuccessStatusCode();
        var timeline = (await timelineResponse.Content.ReadFromJsonAsync<IReadOnlyList<PermissionHistoryTimelineEntryResponse>>())!;
        Assert.Contains(timeline, x => x.EventType == "assignment_created" && x.AssignmentId == assignment.AssignmentId);

        var statusRequest = Authorized(
            HttpMethod.Patch,
            $"/api/people/{personId}/role-assignments/{assignment.AssignmentId}/status",
            token);
        statusRequest.Content = JsonContent.Create(new UpdatePersonRoleAssignmentStatusRequest("inactive"));
        var statusResponse = await _staffarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();

        var projectionAfterStatusResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/permissions/effective", token));
        projectionAfterStatusResponse.EnsureSuccessStatusCode();
        var projectionAfterStatus =
            (await projectionAfterStatusResponse.Content.ReadFromJsonAsync<EffectivePermissionProjectionResponse>())!;
        Assert.DoesNotContain(projectionAfterStatus.Permissions, p => p.PermissionKey == "staffarr.people.read");

        var timelineAfterStatusResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/permissions/history?limit=20", token));
        timelineAfterStatusResponse.EnsureSuccessStatusCode();
        var timelineAfterStatus =
            (await timelineAfterStatusResponse.Content.ReadFromJsonAsync<IReadOnlyList<PermissionHistoryTimelineEntryResponse>>())!;
        Assert.Contains(
            timelineAfterStatus,
            x => x.EventType == "assignment_status_updated"
                && x.AssignmentId == assignment.AssignmentId
                && x.AssignmentStatus == "inactive");
    }

    [Fact]
    public async Task Permission_projection_denies_unrelated_tenant_member_reads()
    {
        var targetPersonId = Guid.NewGuid();
        await SeedStaffPersonAsync(targetPersonId, "Target Person", "target.person@example.com");

        var memberToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: Guid.NewGuid());
        var request = Authorized(HttpMethod.Get, $"/api/people/{targetPersonId}/permissions/effective", memberToken);
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", "http://localhost:5175/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task RevokeStaffArrEntitlementAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var entitlement = await db.Entitlements.FirstAsync(
            e => e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "staffarr");
        entitlement.Status = EntitlementStatuses.Revoked;
        await db.SaveChangesAsync();
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-staffarr-handoff-test",
            $"{productKey} StaffArr Handoff Test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
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

    private async Task SeedStaffPersonAsync(
        Guid personId,
        string displayName,
        string email,
        Guid? externalUserId = null,
        Guid? managerPersonId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var split = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var givenName = split.FirstOrDefault() ?? "User";
        var familyName = split.Length > 1 ? string.Join(' ', split.Skip(1)) : "Test";

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            ExternalUserId = externalUserId,
            GivenName = givenName,
            FamilyName = familyName,
            DisplayName = displayName,
            PrimaryEmail = email,
            EmploymentStatus = "active",
            ManagerPersonId = managerPersonId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedOrgUnitAsync(
        string unitType,
        string name,
        Guid? parentOrgUnitId,
        string status,
        Guid? tenantId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var orgUnit = new OrgUnit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? PlatformSeeder.DemoTenantId,
            UnitType = unitType,
            Name = name,
            ParentOrgUnitId = parentOrgUnitId,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.OrgUnits.Add(orgUnit);
        await db.SaveChangesAsync();
        return orgUnit.Id;
    }
}
