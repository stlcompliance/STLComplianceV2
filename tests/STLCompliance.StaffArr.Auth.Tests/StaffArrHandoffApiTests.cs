using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
using CertificationDefinitionResponse = StaffArr.Api.Contracts.CertificationDefinitionResponse;
using PersonCertificationResponse = StaffArr.Api.Contracts.PersonCertificationResponse;
using GrantPersonCertificationRequest = StaffArr.Api.Contracts.GrantPersonCertificationRequest;
using UpdatePersonCertificationRequest = StaffArr.Api.Contracts.UpdatePersonCertificationRequest;
using PersonReadinessResponse = StaffArr.Api.Contracts.PersonReadinessResponse;
using GrantReadinessOverrideRequest = StaffArr.Api.Contracts.GrantReadinessOverrideRequest;
using CreatePersonnelIncidentRequest = StaffArr.Api.Contracts.CreatePersonnelIncidentRequest;
using PersonnelIncidentSummaryResponse = StaffArr.Api.Contracts.PersonnelIncidentSummaryResponse;
using PersonnelIncidentDetailResponse = StaffArr.Api.Contracts.PersonnelIncidentDetailResponse;
using CreatePersonnelNoteRequest = StaffArr.Api.Contracts.CreatePersonnelNoteRequest;
using PersonnelNoteSummaryResponse = StaffArr.Api.Contracts.PersonnelNoteSummaryResponse;
using PersonnelNoteDetailResponse = StaffArr.Api.Contracts.PersonnelNoteDetailResponse;
using CreatePersonnelDocumentRequest = StaffArr.Api.Contracts.CreatePersonnelDocumentRequest;
using PersonnelDocumentSummaryResponse = StaffArr.Api.Contracts.PersonnelDocumentSummaryResponse;
using PersonnelDocumentDetailResponse = StaffArr.Api.Contracts.PersonnelDocumentDetailResponse;
using PersonTimelineEntryResponse = StaffArr.Api.Contracts.PersonTimelineEntryResponse;
using StaffArrTokenService = StaffArr.Api.Services.StaffArrTokenService;
using StaffArrDbContext = StaffArr.Api.Data.StaffArrDbContext;
using StaffPerson = StaffArr.Api.Entities.StaffPerson;
using OrgUnit = StaffArr.Api.Entities.OrgUnit;
using PersonRoleAssignment = StaffArr.Api.Entities.PersonRoleAssignment;
using PermissionTemplate = StaffArr.Api.Entities.PermissionTemplate;
using RoleTemplate = StaffArr.Api.Entities.RoleTemplate;
using RoleTemplatePermission = StaffArr.Api.Entities.RoleTemplatePermission;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrHandoffApiTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _serviceToken = null!;
    private RecordingComplianceCorePersonReadinessGateHandler _complianceCoreReadinessHandler = null!;

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
        _complianceCoreReadinessHandler = new RecordingComplianceCorePersonReadinessGateHandler();

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _serviceToken);
            builder.UseSetting("ComplianceCore:BaseUrl", "http://compliancecore.test");
            builder.UseSetting("ComplianceCore:ServiceToken", "staffarr-to-compliancecore-token");
            builder.UseSetting("ComplianceCore:PersonReadinessActionKey", "can-use-person");
            builder.UseSetting("ComplianceCore:PersonReadinessWorkflowKey", "can_use_person");
            builder.UseSetting("ComplianceCore:PersonReadinessActivityContextKey", "person_readiness");
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

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StlNexArrLaunchClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<global::StaffArr.Api.Services.ComplianceCorePersonReadinessGateClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreReadinessHandler);
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
    public async Task Handoff_redeem_nexarr_alias_happy_path_returns_session()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _staffarrClient.PostAsJsonAsync(
            "/api/auth/nexarr/redeem",
            new StaffArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<StaffArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Contains("staffarr", session.Entitlements);
    }

    [Fact]
    public async Task V1_handoff_session_and_me_aliases_work()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _staffarrClient.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new StaffArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<StaffArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Contains("staffarr", session.Entitlements);

        var meResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/me", session.AccessToken));
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<StaffArrMeResponse>();
        Assert.NotNull(me);
        Assert.True(me.HasStaffArrEntitlement);

        var sessionResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/session", session.AccessToken));
        sessionResponse.EnsureSuccessStatusCode();
        var bootstrap = await sessionResponse.Content.ReadFromJsonAsync<StaffArrSessionBootstrapResponse>();
        Assert.NotNull(bootstrap);
        Assert.True(bootstrap.HasStaffArrEntitlement);
    }

    [Fact]
    public async Task V1_launch_handoff_proxy_returns_handoff_code()
    {
        var nexarrToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", nexarrToken);
        request.Content = JsonContent.Create(new CreateHandoffRequest("staffarr", "http://localhost:5175/launch"));
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(handoff.HandoffCode));
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
    public async Task People_directory_and_profile_v1_aliases_work()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "V1 Member User", "v1.member.user@example.com", externalUserId: PlatformSeeder.DemoAdminUserId);

        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var listRequest = Authorized(HttpMethod.Get, "/api/v1/people", adminToken);
        var listResponse = await _staffarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var listPayload = await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<StaffPersonSummaryResponse>>();
        Assert.NotNull(listPayload);
        Assert.NotEmpty(listPayload);

        var memberToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: personId);
        var getRequest = Authorized(HttpMethod.Get, $"/api/v1/people/{personId}", memberToken);
        var getResponse = await _staffarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var detail = await getResponse.Content.ReadFromJsonAsync<StaffPersonDetailResponse>();
        Assert.NotNull(detail);
        Assert.Equal(personId, detail.PersonId);
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
        Assert.NotEqual(created.AssignmentId, updated.AssignmentId);
        Assert.Equal(position2Id, updated.PositionOrgUnitId);

        var statusRequest = Authorized(
            HttpMethod.Patch,
            $"/api/people/{personId}/org-assignments/{updated.AssignmentId}/status",
            token);
        statusRequest.Content = JsonContent.Create(new UpdateOrgUnitAssignmentStatusRequest("ended"));
        var statusResponse = await _staffarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var statusPayload = (await statusResponse.Content.ReadFromJsonAsync<OrgUnitAssignmentResponse>())!;
        Assert.Equal("ended", statusPayload.Status);

        var transferredListResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/org-assignments", token));
        transferredListResponse.EnsureSuccessStatusCode();
        var transferredAssignments = (await transferredListResponse.Content.ReadFromJsonAsync<IReadOnlyList<OrgUnitAssignmentResponse>>())!;
        Assert.Equal(2, transferredAssignments.Count);
        Assert.Contains(transferredAssignments, x => x.AssignmentId == created.AssignmentId && x.Status == "ended");
        Assert.Contains(transferredAssignments, x => x.AssignmentId == updated.AssignmentId && x.PositionOrgUnitId == position2Id);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditEvents = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action.StartsWith("org_assignment."));
        Assert.True(auditEvents >= 4);
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
    public async Task Org_units_and_assignments_v1_aliases_happy_path()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "V1 Assignment User", "v1.assignment.user@example.com");

        var createSiteRequest = Authorized(HttpMethod.Post, "/api/v1/org-units", token);
        createSiteRequest.Content = JsonContent.Create(new CreateOrgUnitRequest("site", "V1 Site", null, Status: "active"));
        var createSiteResponse = await _staffarrClient.SendAsync(createSiteRequest);
        createSiteResponse.EnsureSuccessStatusCode();
        var site = (await createSiteResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;

        var createDeptRequest = Authorized(HttpMethod.Post, "/api/v1/org-units", token);
        createDeptRequest.Content = JsonContent.Create(new CreateOrgUnitRequest("department", "V1 Department", site.OrgUnitId, Status: "active"));
        var createDeptResponse = await _staffarrClient.SendAsync(createDeptRequest);
        createDeptResponse.EnsureSuccessStatusCode();
        var dept = (await createDeptResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;

        var createTeamRequest = Authorized(HttpMethod.Post, "/api/v1/org-units", token);
        createTeamRequest.Content = JsonContent.Create(new CreateOrgUnitRequest("team", "V1 Team", dept.OrgUnitId, Status: "active"));
        var createTeamResponse = await _staffarrClient.SendAsync(createTeamRequest);
        createTeamResponse.EnsureSuccessStatusCode();
        var team = (await createTeamResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;

        var createPositionRequest = Authorized(HttpMethod.Post, "/api/v1/org-units", token);
        createPositionRequest.Content = JsonContent.Create(new CreateOrgUnitRequest("position", "V1 Position", team.OrgUnitId, Status: "active"));
        var createPositionResponse = await _staffarrClient.SendAsync(createPositionRequest);
        createPositionResponse.EnsureSuccessStatusCode();
        var position = (await createPositionResponse.Content.ReadFromJsonAsync<OrgUnitResponse>())!;

        var listOrgUnitsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/org-units", token));
        listOrgUnitsResponse.EnsureSuccessStatusCode();
        var units = (await listOrgUnitsResponse.Content.ReadFromJsonAsync<IReadOnlyList<OrgUnitResponse>>())!;
        Assert.Contains(units, x => x.OrgUnitId == site.OrgUnitId);
        Assert.Contains(units, x => x.OrgUnitId == position.OrgUnitId);

        var createAssignmentRequest = Authorized(HttpMethod.Post, $"/api/v1/people/{personId}/org-assignments", token);
        createAssignmentRequest.Content = JsonContent.Create(
            new CreateOrgUnitAssignmentRequest(site.OrgUnitId, dept.OrgUnitId, team.OrgUnitId, position.OrgUnitId));
        var createAssignmentResponse = await _staffarrClient.SendAsync(createAssignmentRequest);
        createAssignmentResponse.EnsureSuccessStatusCode();
        var assignment = (await createAssignmentResponse.Content.ReadFromJsonAsync<OrgUnitAssignmentResponse>())!;

        var listAssignmentsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/people/{personId}/org-assignments", token));
        listAssignmentsResponse.EnsureSuccessStatusCode();
        var assignments = (await listAssignmentsResponse.Content.ReadFromJsonAsync<IReadOnlyList<OrgUnitAssignmentResponse>>())!;
        Assert.Contains(assignments, x => x.AssignmentId == assignment.AssignmentId);

        var deactivateAssignmentRequest = Authorized(
            HttpMethod.Patch,
            $"/api/v1/people/{personId}/org-assignments/{assignment.AssignmentId}/status",
            token);
        deactivateAssignmentRequest.Content = JsonContent.Create(new UpdateOrgUnitAssignmentStatusRequest("ended"));
        var deactivateAssignmentResponse = await _staffarrClient.SendAsync(deactivateAssignmentRequest);
        deactivateAssignmentResponse.EnsureSuccessStatusCode();
        var deactivatedAssignment = (await deactivateAssignmentResponse.Content.ReadFromJsonAsync<OrgUnitAssignmentResponse>())!;
        Assert.Equal("ended", deactivatedAssignment.Status);
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
    public async Task Manager_hierarchy_v1_aliases_happy_path()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var managerId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        await SeedStaffPersonAsync(managerId, "V1 Manager One", "v1.manager.one@example.com");
        await SeedStaffPersonAsync(leadId, "V1 Lead One", "v1.lead.one@example.com", managerPersonId: managerId);
        await SeedStaffPersonAsync(workerId, "V1 Worker One", "v1.worker.one@example.com");

        var siteId = await SeedOrgUnitAsync("site", "V1 HQ", null, "active");
        var deptId = await SeedOrgUnitAsync("department", "V1 Operations", siteId, "active");
        var teamId = await SeedOrgUnitAsync("team", "V1 Alpha Team", deptId, "active");
        var positionId = await SeedOrgUnitAsync("position", "V1 Operator", teamId, "active");

        var assignmentRequest = Authorized(HttpMethod.Post, $"/api/v1/people/{workerId}/org-assignments", token);
        assignmentRequest.Content = JsonContent.Create(new CreateOrgUnitAssignmentRequest(siteId, deptId, teamId, positionId));
        var assignmentResponse = await _staffarrClient.SendAsync(assignmentRequest);
        assignmentResponse.EnsureSuccessStatusCode();

        var updateManagerRequest = Authorized(HttpMethod.Put, $"/api/v1/people/{workerId}/manager", token);
        updateManagerRequest.Content = JsonContent.Create(new UpdatePersonManagerRequest(leadId));
        var updateManagerResponse = await _staffarrClient.SendAsync(updateManagerRequest);
        updateManagerResponse.EnsureSuccessStatusCode();
        var managerPayload = (await updateManagerResponse.Content.ReadFromJsonAsync<PersonManagerResponse>())!;
        Assert.Equal(leadId, managerPayload.ManagerPersonId);

        var chainResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/people/{workerId}/manager-chain", token));
        chainResponse.EnsureSuccessStatusCode();
        var chain = (await chainResponse.Content.ReadFromJsonAsync<IReadOnlyList<ManagerChainEntryResponse>>())!;
        Assert.Equal(2, chain.Count);
        Assert.Equal(leadId, chain[0].PersonId);
        Assert.Equal(managerId, chain[1].PersonId);

        var subordinatesResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/people/{managerId}/subordinates?includeIndirect=true", token));
        subordinatesResponse.EnsureSuccessStatusCode();
        var subordinateList = (await subordinatesResponse.Content.ReadFromJsonAsync<IReadOnlyList<SubordinateSummaryResponse>>())!;
        Assert.Equal(2, subordinateList.Count);
        Assert.Contains(subordinateList, x => x.PersonId == workerId && x.Depth == 2);

        var subordinateDetailResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/people/{managerId}/subordinates/{workerId}", token));
        subordinateDetailResponse.EnsureSuccessStatusCode();
        var subordinateDetail = (await subordinateDetailResponse.Content.ReadFromJsonAsync<SubordinateSummaryResponse>())!;
        Assert.Equal(2, subordinateDetail.Depth);
        Assert.Contains("V1 HQ / V1 Operations / V1 Alpha Team / V1 Operator", subordinateDetail.ActiveAssignmentPath);
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
    public async Task Sensitive_role_assignments_start_pending_review_and_require_approval()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Sensitive Role User", "sensitive.role@example.com");

        var permissionTemplateId = Guid.NewGuid();
        var roleTemplateId = Guid.NewGuid();
        await using (var scope = _staffarrFactory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            db.PermissionTemplates.Add(new PermissionTemplate
            {
                Id = permissionTemplateId,
                TenantId = PlatformSeeder.DemoTenantId,
                PermissionKey = "staffarr.people.manage_sensitive",
                Name = "Sensitive People Manage",
                Description = "Sensitive permission requiring review.",
                Status = "active",
                ProductKey = "staffarr",
                PermissionScope = "tenant",
                Sensitivity = "sensitive",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastSyncedAt = DateTimeOffset.UtcNow,
            });
            db.RoleTemplates.Add(new RoleTemplate
            {
                Id = roleTemplateId,
                TenantId = PlatformSeeder.DemoTenantId,
                RoleKey = "staffarr.high.risk",
                Name = "High Risk StaffArr Role",
                Description = "Role with sensitive permission.",
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
            db.RoleTemplatePermissions.Add(new RoleTemplatePermission
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                RoleTemplateId = roleTemplateId,
                PermissionTemplateId = permissionTemplateId,
                ScopeType = "tenant",
                ScopeValue = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        var assignmentRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/role-assignments", token);
        assignmentRequest.Content = JsonContent.Create(new CreatePersonRoleAssignmentRequest(
            roleTemplateId,
            "tenant",
            null));
        var assignmentResponse = await _staffarrClient.SendAsync(assignmentRequest);
        assignmentResponse.EnsureSuccessStatusCode();
        var assignment = (await assignmentResponse.Content.ReadFromJsonAsync<PersonRoleAssignmentResponse>())!;
        Assert.Equal("pending_review", assignment.Status);
        Assert.Equal("pending_review", assignment.EffectiveStatus);

        var projectionBeforeApprovalResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/permissions/effective", token));
        projectionBeforeApprovalResponse.EnsureSuccessStatusCode();
        var projectionBeforeApproval =
            (await projectionBeforeApprovalResponse.Content.ReadFromJsonAsync<EffectivePermissionProjectionResponse>())!;
        Assert.DoesNotContain(
            projectionBeforeApproval.Permissions,
            p => p.PermissionKey == "staffarr.people.manage_sensitive");

        var approveRequest = Authorized(
            HttpMethod.Patch,
            $"/api/people/{personId}/role-assignments/{assignment.AssignmentId}/status",
            token);
        approveRequest.Content = JsonContent.Create(new UpdatePersonRoleAssignmentStatusRequest("active"));
        var approveResponse = await _staffarrClient.SendAsync(approveRequest);
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<PersonRoleAssignmentResponse>())!;
        Assert.Equal("active", approved.Status);
        Assert.Equal("active", approved.EffectiveStatus);

        var projectionAfterApprovalResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/permissions/effective", token));
        projectionAfterApprovalResponse.EnsureSuccessStatusCode();
        var projectionAfterApproval =
            (await projectionAfterApprovalResponse.Content.ReadFromJsonAsync<EffectivePermissionProjectionResponse>())!;
        Assert.Contains(
            projectionAfterApproval.Permissions,
            p => p.PermissionKey == "staffarr.people.manage_sensitive");
    }

    [Fact]
    public async Task Expired_role_assignments_are_hidden_from_projection_but_reported_as_expired()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Expired Assignment User", "expired.assignment@example.com");

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

        var expiredAt = DateTimeOffset.UtcNow.AddHours(-2);
        await using (var scope = _staffarrFactory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            db.PersonRoleAssignments.Add(new PersonRoleAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                PersonId = personId,
                RoleTemplateId = roleTemplate.RoleTemplateId,
                ScopeType = "tenant",
                ScopeValue = null,
                Status = "active",
                ExpiresAt = expiredAt,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            });
            await db.SaveChangesAsync();
        }

        var projectionResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/permissions/effective", token));
        projectionResponse.EnsureSuccessStatusCode();
        var projection = (await projectionResponse.Content.ReadFromJsonAsync<EffectivePermissionProjectionResponse>())!;
        Assert.DoesNotContain(projection.Permissions, p => p.PermissionKey == "staffarr.people.read");

        var assignmentsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/role-assignments", token));
        assignmentsResponse.EnsureSuccessStatusCode();
        var assignments = (await assignmentsResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonRoleAssignmentResponse>>())!;
        var expiredAssignment = Assert.Single(assignments);
        Assert.Equal("expired", expiredAssignment.EffectiveStatus);
        Assert.True(
            Math.Abs((expiredAssignment.ExpiresAt!.Value - expiredAt).TotalSeconds) < 1,
            $"Expected expiry close to {expiredAt:O} but found {expiredAssignment.ExpiresAt:O}.");
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

    [Fact]
    public async Task Certification_definitions_and_manual_grant_happy_path_seeds_readiness_baseline_and_grants_record()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Certification User", "certification.user@example.com");

        var definitionsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/certifications", token));
        definitionsResponse.EnsureSuccessStatusCode();
        var definitions = (await definitionsResponse.Content.ReadFromJsonAsync<IReadOnlyList<CertificationDefinitionResponse>>())!;
        Assert.Contains(definitions, x => x.CertificationKey == "readiness.safety_orientation");
        Assert.Contains(definitions, x => x.CertificationKey == "readiness.hazmat_awareness");

        var safetyDefinition = definitions.First(x => x.CertificationKey == "readiness.safety_orientation");
        var grantRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/certifications", token);
        grantRequest.Content = JsonContent.Create(new GrantPersonCertificationRequest(
            safetyDefinition.CertificationDefinitionId,
            null,
            null,
            "Manual onboarding grant."));
        var grantResponse = await _staffarrClient.SendAsync(grantRequest);
        grantResponse.EnsureSuccessStatusCode();
        var granted = (await grantResponse.Content.ReadFromJsonAsync<PersonCertificationResponse>())!;
        Assert.Equal("manual", granted.SourceType);
        Assert.Equal("active", granted.EffectiveStatus);
        Assert.NotNull(granted.ExpiresAt);

        var listResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/certifications", token));
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonCertificationResponse>>())!;
        Assert.Single(list);
        Assert.Equal("readiness.safety_orientation", list[0].CertificationKey);

        var revokeRequest = Authorized(
            HttpMethod.Patch,
            $"/api/people/{personId}/certifications/{granted.PersonCertificationId}",
            token);
        revokeRequest.Content = JsonContent.Create(new UpdatePersonCertificationRequest(
            "revoked",
            granted.ExpiresAt,
            granted.Notes));
        var revokeResponse = await _staffarrClient.SendAsync(revokeRequest);
        revokeResponse.EnsureSuccessStatusCode();
        var revoked = (await revokeResponse.Content.ReadFromJsonAsync<PersonCertificationResponse>())!;
        Assert.Equal("revoked", revoked.EffectiveStatus);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditEvents = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && (x.Action == "person_certification.grant" || x.Action == "person_certification.update"));
        Assert.True(auditEvents >= 2);
    }

    [Fact]
    public async Task Certification_grant_denies_non_writer_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Denied Certification User", "denied.cert.user@example.com");

        var definitionsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/certifications", token));
        definitionsResponse.EnsureSuccessStatusCode();
        var definitions = (await definitionsResponse.Content.ReadFromJsonAsync<IReadOnlyList<CertificationDefinitionResponse>>())!;
        var definition = definitions.First();

        var grantRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/certifications", token);
        grantRequest.Content = JsonContent.Create(new GrantPersonCertificationRequest(
            definition.CertificationDefinitionId,
            null,
            null,
            null));
        var grantResponse = await _staffarrClient.SendAsync(grantRequest);
        Assert.Equal(HttpStatusCode.Forbidden, grantResponse.StatusCode);
    }

    [Fact]
    public async Task Certification_read_allows_tenant_member_self_and_denies_other_people()
    {
        var selfPersonId = Guid.NewGuid();
        var otherPersonId = Guid.NewGuid();
        await SeedStaffPersonAsync(selfPersonId, "Self Member", "self.member@example.com");
        await SeedStaffPersonAsync(otherPersonId, "Other Member", "other.member@example.com");

        var selfToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: selfPersonId);

        var selfResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{selfPersonId}/certifications", selfToken));
        selfResponse.EnsureSuccessStatusCode();

        var otherResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{otherPersonId}/certifications", selfToken));
        Assert.Equal(HttpStatusCode.Forbidden, otherResponse.StatusCode);
    }

    [Fact]
    public async Task Person_readiness_not_ready_without_certifications_lists_baseline_blockers()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Readiness Blocked User", "readiness.blocked@example.com");

        await _staffarrClient.SendAsync(Authorized(HttpMethod.Get, "/api/certifications", token));

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Equal(3, readiness.Blockers.Count);
        Assert.Contains(
            readiness.Blockers,
            x => x.BlockerSource == "certification" && x.CertificationKey == "readiness.safety_orientation");
        Assert.Contains(
            readiness.Blockers,
            x => x.BlockerSource == "certification" && x.CertificationKey == "readiness.hazmat_awareness");
        Assert.Contains(
            readiness.Blockers,
            x => x.BlockerSource == "certification" && x.CertificationKey == "readiness.equipment_operator");
        Assert.All(readiness.Blockers, x => Assert.Equal("missing", x.BlockerType));
        Assert.NotNull(readiness.AuditSnapshot);
        Assert.Equal("person_readiness", readiness.AuditSnapshot.SnapshotKind);

        await using var scope = _staffarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditEvent = await db.AuditEvents.SingleAsync(x => x.Id == readiness.AuditSnapshot.AuditEventId);
        Assert.Equal("staffarr.readiness.read", auditEvent.Action);
        Assert.Equal("person_readiness", auditEvent.TargetType);
        Assert.Equal(personId.ToString(), auditEvent.TargetId);
        Assert.Equal("not_ready", auditEvent.Result);
        Assert.Equal("certifications", auditEvent.ReasonCode);
    }

    [Fact]
    public async Task Person_readiness_ready_when_all_baseline_certifications_active()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Readiness Ready User", "readiness.ready@example.com");

        var definitionsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/certifications", token));
        definitionsResponse.EnsureSuccessStatusCode();
        var definitions = (await definitionsResponse.Content.ReadFromJsonAsync<IReadOnlyList<CertificationDefinitionResponse>>())!;

        foreach (var definition in definitions.Where(x => x.Category == "readiness"))
        {
            var grantRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/certifications", token);
            grantRequest.Content = JsonContent.Create(new GrantPersonCertificationRequest(
                definition.CertificationDefinitionId,
                null,
                null,
                "Readiness test grant."));
            var grantResponse = await _staffarrClient.SendAsync(grantRequest);
            grantResponse.EnsureSuccessStatusCode();
        }

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("ready", readiness.ReadinessStatus);
        Assert.Empty(readiness.Blockers);
        Assert.All(readiness.Requirements, x => Assert.Equal("satisfied", x.RequirementStatus));
    }

    [Fact]
    public async Task Person_readiness_expired_certification_produces_plain_english_blocker()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Readiness Expired User", "readiness.expired@example.com");

        var definitionsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/certifications", token));
        definitionsResponse.EnsureSuccessStatusCode();
        var definitions = (await definitionsResponse.Content.ReadFromJsonAsync<IReadOnlyList<CertificationDefinitionResponse>>())!;
        var safetyDefinition = definitions.First(x => x.CertificationKey == "readiness.safety_orientation");

        var grantRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/certifications", token);
        grantRequest.Content = JsonContent.Create(new GrantPersonCertificationRequest(
            safetyDefinition.CertificationDefinitionId,
            DateTimeOffset.UtcNow.AddYears(-2),
            DateTimeOffset.UtcNow.AddDays(-30),
            "Expired readiness grant."));
        var grantResponse = await _staffarrClient.SendAsync(grantRequest);
        grantResponse.EnsureSuccessStatusCode();

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        var safetyBlocker = Assert.Single(
            readiness.Blockers.Where(x =>
                x.BlockerSource == "certification" && x.CertificationKey == "readiness.safety_orientation"));
        Assert.Equal("expired", safetyBlocker.BlockerType);
        Assert.Contains("expired", safetyBlocker.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Person_readiness_query_surface_matches_nested_route()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Readiness Query User", "readiness.query@example.com");

        await _staffarrClient.SendAsync(Authorized(HttpMethod.Get, "/api/certifications", token));

        var nestedResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", token));
        nestedResponse.EnsureSuccessStatusCode();
        var nested = (await nestedResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        var queryResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/readiness?personId={personId}", token));
        queryResponse.EnsureSuccessStatusCode();
        var query = (await queryResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal(nested.ReadinessStatus, query.ReadinessStatus);
        Assert.Equal(nested.Blockers.Count, query.Blockers.Count);
    }

    [Fact]
    public async Task Person_readiness_denies_unrelated_tenant_member_reads()
    {
        var targetPersonId = Guid.NewGuid();
        await SeedStaffPersonAsync(targetPersonId, "Target Readiness Person", "target.readiness@example.com");

        var memberToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: Guid.NewGuid());
        var request = Authorized(HttpMethod.Get, $"/api/people/{targetPersonId}/readiness", memberToken);
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Person_readiness_override_grants_ready_status_with_manual_basis_while_blockers_remain()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Override Ready User", "readiness.override@example.com");

        await _staffarrClient.SendAsync(Authorized(HttpMethod.Get, "/api/certifications", token));

        var grantRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/readiness/override", token);
        grantRequest.Content = JsonContent.Create(new GrantReadinessOverrideRequest(
            "Operations manager approved temporary assignment pending scheduled training.",
            null));
        var grantResponse = await _staffarrClient.SendAsync(grantRequest);
        grantResponse.EnsureSuccessStatusCode();
        var readiness = (await grantResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("ready", readiness.ReadinessStatus);
        Assert.Equal("manual_override", readiness.ReadinessBasis);
        Assert.NotNull(readiness.ActiveOverride);
        Assert.NotEmpty(readiness.Blockers);
    }

    [Fact]
    public async Task Person_readiness_compliancecore_blocker_keeps_person_not_ready_even_with_manual_override()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Compliance Core Ready User", "readiness.compliancecore@example.com");

        var definitionsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/certifications", token));
        definitionsResponse.EnsureSuccessStatusCode();
        var definitions = (await definitionsResponse.Content.ReadFromJsonAsync<IReadOnlyList<CertificationDefinitionResponse>>())!;

        foreach (var definition in definitions.Where(x => x.Category == "readiness"))
        {
            var grantCertificationRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/certifications", token);
            grantCertificationRequest.Content = JsonContent.Create(new GrantPersonCertificationRequest(
                definition.CertificationDefinitionId,
                null,
                null,
                "Compliance Core readiness test grant."));
            var grantCertificationResponse = await _staffarrClient.SendAsync(grantCertificationRequest);
            grantCertificationResponse.EnsureSuccessStatusCode();
        }

        var grantOverrideRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/readiness/override", token);
        grantOverrideRequest.Content = JsonContent.Create(new GrantReadinessOverrideRequest(
            "Temporary workforce override cannot waive Compliance Core product rules.",
            null));
        var grantOverrideResponse = await _staffarrClient.SendAsync(grantOverrideRequest);
        grantOverrideResponse.EnsureSuccessStatusCode();

        _complianceCoreReadinessHandler.Requests.Clear();
        _complianceCoreReadinessHandler.NextOutcome = "block";
        _complianceCoreReadinessHandler.NextReasonCode = "regulated_driver_credentials_missing";
        _complianceCoreReadinessHandler.NextMessage = "Compliance Core requires regulated driver credential evidence before assignment.";

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", token));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Equal("compliancecore", readiness.ReadinessBasis);
        Assert.NotNull(readiness.ActiveOverride);
        var complianceBlocker = Assert.Single(readiness.Blockers, x => x.BlockerSource == "compliancecore");
        Assert.Equal("regulated_driver_credentials_missing", complianceBlocker.BlockerType);
        Assert.Contains("regulated driver credential", complianceBlocker.Message, StringComparison.OrdinalIgnoreCase);

        var gateRequest = Assert.Single(_complianceCoreReadinessHandler.Requests);
        Assert.Equal("/api/v1/gates/can-use-person", gateRequest.Path);
        Assert.Equal("Bearer", gateRequest.AuthorizationScheme);
        Assert.Equal("staffarr-to-compliancecore-token", gateRequest.AuthorizationParameter);
        Assert.Equal(PlatformSeeder.DemoTenantId, gateRequest.TenantId);
        Assert.Equal("person_readiness", gateRequest.ActivityContextKey);
        Assert.Equal("can_use_person", gateRequest.WorkflowKey);
        Assert.Contains(gateRequest.Subjects, subject =>
            subject.SubjectType == "person"
            && subject.SubjectReference == personId.ToString("D")
            && subject.SourceProduct == "staffarr"
            && subject.DisplayLabel == "Compliance Core Ready User");
        Assert.Equal(personId.ToString("D"), gateRequest.RuleContext["person_id"]);
        Assert.Equal("active", gateRequest.RuleContext["person_status"]);
        Assert.Equal("true", gateRequest.RuleContext["has_manual_override"]);
        Assert.Equal("ready", gateRequest.RuleContext["local_readiness_status"]);
    }

    [Fact]
    public async Task Person_readiness_override_clear_restores_not_ready_without_certifications()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Override Clear User", "readiness.override.clear@example.com");

        await _staffarrClient.SendAsync(Authorized(HttpMethod.Get, "/api/certifications", token));

        var grantRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/readiness/override", token);
        grantRequest.Content = JsonContent.Create(new GrantReadinessOverrideRequest(
            "Temporary coverage while training class is scheduled next week.",
            null));
        await _staffarrClient.SendAsync(grantRequest);

        var clearRequest = Authorized(HttpMethod.Delete, $"/api/people/{personId}/readiness/override", token);
        var clearResponse = await _staffarrClient.SendAsync(clearRequest);
        clearResponse.EnsureSuccessStatusCode();
        var readiness = (await clearResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Equal("certifications", readiness.ReadinessBasis);
        Assert.Null(readiness.ActiveOverride);
    }

    [Fact]
    public async Task Person_readiness_override_denies_supervisor_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Override Denied User", "readiness.override.denied@example.com");

        var grantRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/readiness/override", token);
        grantRequest.Content = JsonContent.Create(new GrantReadinessOverrideRequest(
            "Supervisor should not be able to grant readiness overrides.",
            null));
        var response = await _staffarrClient.SendAsync(grantRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Person_readiness_override_rejects_past_expiration()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Override Validation User", "readiness.override.validation@example.com");

        var grantRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/readiness/override", token);
        grantRequest.Content = JsonContent.Create(new GrantReadinessOverrideRequest(
            "Validation should reject overrides that already expired.",
            DateTimeOffset.UtcNow.AddMinutes(-5)));
        var response = await _staffarrClient.SendAsync(grantRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Person_readiness_v1_alias_query_and_override_flow()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Readiness V1 User", "readiness.v1@example.com");

        await _staffarrClient.SendAsync(Authorized(HttpMethod.Get, "/api/v1/certifications", token));

        var nestedResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/people/{personId}/readiness", token));
        nestedResponse.EnsureSuccessStatusCode();
        var nested = (await nestedResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        var queryResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/readiness?personId={personId}", token));
        queryResponse.EnsureSuccessStatusCode();
        var query = (await queryResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.Equal(nested.ReadinessStatus, query.ReadinessStatus);

        var grantRequest = Authorized(HttpMethod.Post, $"/api/v1/people/{personId}/readiness/override", token);
        grantRequest.Content = JsonContent.Create(new GrantReadinessOverrideRequest(
            "V1 temporary readiness authorization pending training completion.",
            null));
        var grantResponse = await _staffarrClient.SendAsync(grantRequest);
        grantResponse.EnsureSuccessStatusCode();
        var granted = (await grantResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.Equal("ready", granted.ReadinessStatus);
        Assert.Equal("manual_override", granted.ReadinessBasis);

        var clearResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/people/{personId}/readiness/override", token));
        clearResponse.EnsureSuccessStatusCode();
        var cleared = (await clearResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.Equal("not_ready", cleared.ReadinessStatus);
        Assert.Equal("certifications", cleared.ReadinessBasis);
    }

    [Fact]
    public async Task Personnel_incident_intake_creates_list_and_detail_records()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Incident Subject", "incident.subject@example.com");

        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "safety",
            "high",
            "Forklift near-miss in warehouse aisle",
            "Operator reported a near collision while reversing; no injuries but process review required.",
            DateTimeOffset.UtcNow.AddHours(-2)));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var listResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/incidents?personId={personId}", token));
        listResponse.EnsureSuccessStatusCode();
        var incidents = (await listResponse.Content.ReadFromJsonAsync<List<PersonnelIncidentSummaryResponse>>())!;
        Assert.Contains(incidents, x => x.IncidentId == created.IncidentId);

        var detailResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/incidents/{created.IncidentId}", token));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Equal(created.IncidentId, detail.IncidentId);
        Assert.Equal("open", detail.Status);
        Assert.Contains("near collision", detail.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Personnel_incident_intake_denies_supervisor_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Incident Denied User", "incident.denied@example.com");

        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "conduct",
            "medium",
            "Supervisor should not create incidents",
            "Supervisor role lacks staffarr.incidents.manage scope for intake creation.",
            DateTimeOffset.UtcNow.AddHours(-1)));
        var response = await _staffarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Personnel_incident_intake_v1_alias_create_list_and_detail()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Incident V1 Subject", "incident.v1.subject@example.com");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/incidents", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "safety",
            "high",
            "Forklift near-miss in v1 warehouse aisle",
            "Operator reported a near collision in v1 flow; no injuries but process review required.",
            DateTimeOffset.UtcNow.AddHours(-2)));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var listResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/incidents?personId={personId}", token));
        listResponse.EnsureSuccessStatusCode();
        var incidents = (await listResponse.Content.ReadFromJsonAsync<List<PersonnelIncidentSummaryResponse>>())!;
        Assert.Contains(incidents, x => x.IncidentId == created.IncidentId);

        var detailResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/incidents/{created.IncidentId}", token));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Equal(created.IncidentId, detail.IncidentId);
        Assert.Equal("open", detail.Status);
    }

    [Fact]
    public async Task Events_and_audit_v1_aliases_match_existing_endpoints()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Event Alias User", "event.alias.user@example.com");

        var legacyEventsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/person-history?personId={personId}&page=1&pageSize=10", token));
        legacyEventsResponse.EnsureSuccessStatusCode();
        var legacyEventsJson = JsonDocument.Parse(await legacyEventsResponse.Content.ReadAsStringAsync());
        var legacyEventCount = legacyEventsJson.RootElement.GetProperty("items").GetArrayLength();

        var v1EventsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/events?personId={personId}&page=1&pageSize=10", token));
        v1EventsResponse.EnsureSuccessStatusCode();
        var v1EventsJson = JsonDocument.Parse(await v1EventsResponse.Content.ReadAsStringAsync());
        var v1EventCount = v1EventsJson.RootElement.GetProperty("items").GetArrayLength();
        Assert.Equal(legacyEventCount, v1EventCount);

        var legacyAuditResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/timeline?page=1&pageSize=10", token));
        legacyAuditResponse.EnsureSuccessStatusCode();
        var legacyAuditJson = JsonDocument.Parse(await legacyAuditResponse.Content.ReadAsStringAsync());
        var legacyAuditCount = legacyAuditJson.RootElement.GetProperty("items").GetArrayLength();

        var v1AuditResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit?page=1&pageSize=10", token));
        v1AuditResponse.EnsureSuccessStatusCode();
        var v1AuditJson = JsonDocument.Parse(await v1AuditResponse.Content.ReadAsStringAsync());
        var v1AuditCount = v1AuditJson.RootElement.GetProperty("items").GetArrayLength();
        Assert.Equal(legacyAuditCount, v1AuditCount);
    }

    [Fact]
    public async Task Personnel_incident_list_allows_tenant_member_for_self_only()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Self Incident User", "incident.self@example.com");

        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "policy",
            "low",
            "Policy acknowledgment gap",
            "Employee missed annual policy acknowledgment deadline; HR documented for follow-up coaching.",
            DateTimeOffset.UtcNow.AddDays(-1)));
        await _staffarrClient.SendAsync(createRequest);

        var memberToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: personId);
        var selfListResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/incidents?personId={personId}", memberToken));
        selfListResponse.EnsureSuccessStatusCode();
        var selfIncidents = (await selfListResponse.Content.ReadFromJsonAsync<List<PersonnelIncidentSummaryResponse>>())!;
        Assert.NotEmpty(selfIncidents);

        var allListResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/incidents", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, allListResponse.StatusCode);
    }

    [Fact]
    public async Task Personnel_incident_intake_rejects_future_occurrence()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Incident Validation User", "incident.validation@example.com");

        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "equipment",
            "medium",
            "Future-dated incident should fail",
            "Validation rejects incidents with occurrence timestamps in the future.",
            DateTimeOffset.UtcNow.AddDays(2)));
        var response = await _staffarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Person_timeline_aggregates_incidents_readiness_certifications_and_permissions()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Timeline User", "timeline.user@example.com");

        var incidentRequest = Authorized(HttpMethod.Post, "/api/incidents", token);
        incidentRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "safety",
            "medium",
            "Slip hazard in receiving dock",
            "Wet floor signage missing during inbound shift.",
            DateTimeOffset.UtcNow.AddHours(-4)));
        var incidentResponse = await _staffarrClient.SendAsync(incidentRequest);
        incidentResponse.EnsureSuccessStatusCode();
        var incident = (await incidentResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var overrideRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/readiness/override", token);
        overrideRequest.Content = JsonContent.Create(new GrantReadinessOverrideRequest(
            "Temporary site access for audit support",
            DateTimeOffset.UtcNow.AddDays(7)));
        var overrideResponse = await _staffarrClient.SendAsync(overrideRequest);
        overrideResponse.EnsureSuccessStatusCode();

        var definitionsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/certifications", token));
        definitionsResponse.EnsureSuccessStatusCode();
        var definitions = (await definitionsResponse.Content.ReadFromJsonAsync<IReadOnlyList<CertificationDefinitionResponse>>())!;
        var safetyDefinition = definitions.First(x => x.CertificationKey == "readiness.safety_orientation");

        var grantRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/certifications", token);
        grantRequest.Content = JsonContent.Create(new GrantPersonCertificationRequest(
            safetyDefinition.CertificationDefinitionId,
            null,
            null,
            "Timeline certification grant."));
        var grantResponse = await _staffarrClient.SendAsync(grantRequest);
        grantResponse.EnsureSuccessStatusCode();

        var permissionRequest = Authorized(HttpMethod.Post, "/api/permissions", token);
        permissionRequest.Content = JsonContent.Create(new UpsertPermissionTemplateRequest(
            "staffarr.timeline.read",
            "Timeline Read",
            "Timeline test permission."));
        var permissionResponse = await _staffarrClient.SendAsync(permissionRequest);
        permissionResponse.EnsureSuccessStatusCode();
        var permissionTemplate = (await permissionResponse.Content.ReadFromJsonAsync<PermissionTemplateSummaryResponse>())!;

        var roleRequest = Authorized(HttpMethod.Post, "/api/roles", token);
        roleRequest.Content = JsonContent.Create(new CreateRoleTemplateRequest(
            "staffarr.timeline",
            "Timeline Role",
            "Timeline role template.",
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

        var timelineResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/timeline?page=1&pageSize=50", token));
        timelineResponse.EnsureSuccessStatusCode();
        var timelinePage = (await timelineResponse.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;

        Assert.True(timelinePage.TotalCount >= 4);
        Assert.Contains(
            timelinePage.Items,
            x => x.Category == "incident"
                && x.EventType == "incident_reported"
                && x.SourceEntityId == incident.IncidentId.ToString());
        Assert.Contains(
            timelinePage.Items,
            x => x.Category == "readiness" && x.EventType == "readiness_override_granted");
        Assert.Contains(
            timelinePage.Items,
            x => x.Category == "certification" && x.EventType == "certification_granted");
        Assert.Contains(
            timelinePage.Items,
            x => x.Category == "permission" && x.EventType == "assignment_created");
    }

    [Fact]
    public async Task Person_timeline_pagination_returns_has_next_page_when_events_exceed_page_size()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Timeline Pagination User", "timeline.pagination@example.com");

        for (var i = 0; i < 3; i++)
        {
            var createRequest = Authorized(HttpMethod.Post, "/api/incidents", token);
            createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
                personId,
                "policy",
                "low",
                $"Policy follow-up #{i + 1}",
                "Repeated policy coaching events for pagination coverage.",
                DateTimeOffset.UtcNow.AddHours(-i - 1)));
            var createResponse = await _staffarrClient.SendAsync(createRequest);
            createResponse.EnsureSuccessStatusCode();
        }

        var pageOneResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/timeline?page=1&pageSize=2", token));
        pageOneResponse.EnsureSuccessStatusCode();
        var pageOne = (await pageOneResponse.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;

        Assert.Equal(2, pageOne.Items.Count);
        Assert.Equal(3, pageOne.TotalCount);
        Assert.True(pageOne.HasNextPage);

        var pageTwoResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/timeline?page=2&pageSize=2", token));
        pageTwoResponse.EnsureSuccessStatusCode();
        var pageTwo = (await pageTwoResponse.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;

        Assert.Single(pageTwo.Items);
        Assert.False(pageTwo.HasNextPage);
    }

    [Fact]
    public async Task Person_timeline_category_filter_returns_only_matching_events()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Timeline Category User", "timeline.category@example.com");

        var incidentRequest = Authorized(HttpMethod.Post, "/api/incidents", token);
        incidentRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "policy",
            "low",
            "Policy coaching",
            "Category filter coverage incident.",
            DateTimeOffset.UtcNow.AddHours(-2)));
        (await _staffarrClient.SendAsync(incidentRequest)).EnsureSuccessStatusCode();

        var readinessRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/readiness/override", token);
        readinessRequest.Content = JsonContent.Create(new GrantReadinessOverrideRequest(
            "Temporary access for category filter coverage.",
            DateTimeOffset.UtcNow.AddDays(1)));
        (await _staffarrClient.SendAsync(readinessRequest)).EnsureSuccessStatusCode();

        var incidentOnlyResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/timeline?category=incident&page=1&pageSize=50", token));
        incidentOnlyResponse.EnsureSuccessStatusCode();
        var incidentOnly = (await incidentOnlyResponse.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;

        Assert.True(incidentOnly.TotalCount >= 1);
        Assert.All(incidentOnly.Items, x => Assert.Equal("incident", x.Category));

        var readinessOnlyResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/timeline?category=readiness&page=1&pageSize=50", token));
        readinessOnlyResponse.EnsureSuccessStatusCode();
        var readinessOnly = (await readinessOnlyResponse.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;

        Assert.True(readinessOnly.TotalCount >= 1);
        Assert.All(readinessOnly.Items, x => Assert.Equal("readiness", x.Category));
    }

    [Fact]
    public async Task Person_timeline_allows_tenant_member_self_and_denies_other_people()
    {
        var selfPersonId = Guid.NewGuid();
        var otherPersonId = Guid.NewGuid();
        await SeedStaffPersonAsync(selfPersonId, "Timeline Self", "timeline.self@example.com");
        await SeedStaffPersonAsync(otherPersonId, "Timeline Other", "timeline.other@example.com");

        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            selfPersonId,
            "conduct",
            "low",
            "Self coaching note",
            "Member-visible incident for timeline self-read test.",
            DateTimeOffset.UtcNow.AddHours(-1)));
        await _staffarrClient.SendAsync(createRequest);

        var memberToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: selfPersonId);

        var selfResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{selfPersonId}/timeline", memberToken));
        selfResponse.EnsureSuccessStatusCode();

        var otherResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{otherPersonId}/timeline", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, otherResponse.StatusCode);
    }

    [Fact]
    public async Task Personnel_note_create_list_and_detail_with_visibility_filtering()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Notes Subject", "notes.subject@example.com");

        var createRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/notes", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelNoteRequest(
            "coaching",
            "personnel_visible",
            "Quarterly coaching follow-up",
            "Documented coaching conversation and agreed follow-up actions for next review cycle."));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelNoteDetailResponse>())!;

        var listResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/notes", token));
        listResponse.EnsureSuccessStatusCode();
        var notes = (await listResponse.Content.ReadFromJsonAsync<List<PersonnelNoteSummaryResponse>>())!;
        Assert.Contains(notes, x => x.NoteId == created.NoteId);

        var detailResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/notes/{created.NoteId}", token));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<PersonnelNoteDetailResponse>())!;
        Assert.Contains("coaching conversation", detail.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Personnel_note_hr_only_hidden_from_tenant_member_self()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Notes Visibility User", "notes.visibility@example.com");

        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var createRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/notes", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePersonnelNoteRequest(
            "disciplinary",
            "hr_only",
            "Confidential HR note",
            "This note should remain hidden from tenant members even when viewing self."));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var memberToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: personId);
        var listResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/notes", memberToken));
        listResponse.EnsureSuccessStatusCode();
        var notes = (await listResponse.Content.ReadFromJsonAsync<List<PersonnelNoteSummaryResponse>>())!;
        Assert.Empty(notes);
    }

    [Fact]
    public async Task Personnel_note_create_denies_supervisor_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Notes Denied User", "notes.denied@example.com");

        var createRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/notes", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelNoteRequest(
            "general",
            "management",
            "Supervisor should not create notes",
            "Supervisor role lacks staffarr.notes.manage scope for note creation."));
        var response = await _staffarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Personnel_document_upload_list_download_and_timeline()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Documents Subject", "documents.subject@example.com");
        var fileBytes = "Signed offer letter content"u8.ToArray();

        var createRequest = Authorized(HttpMethod.Post, $"/api/v1/documents?personId={personId}", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelDocumentRequest(
            "employment_contract",
            "Signed offer letter",
            "offer-letter.txt",
            "text/plain",
            Convert.ToBase64String(fileBytes),
            "Initial employment contract upload",
            null));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.StartsWith($"/api/v1/documents/", createResponse.Headers.Location?.OriginalString);
        Assert.Contains($"personId={personId}", createResponse.Headers.Location?.OriginalString);
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelDocumentDetailResponse>())!;

        var listResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/documents?personId={personId}", token));
        listResponse.EnsureSuccessStatusCode();
        var documents = (await listResponse.Content.ReadFromJsonAsync<List<PersonnelDocumentSummaryResponse>>())!;
        Assert.Contains(documents, x => x.DocumentId == created.DocumentId);

        var detailResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/documents/{created.DocumentId}?personId={personId}", token));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<PersonnelDocumentDetailResponse>())!;
        Assert.Equal(created.DocumentId, detail.DocumentId);

        var downloadResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/documents/{created.DocumentId}/content?personId={personId}", token));
        downloadResponse.EnsureSuccessStatusCode();
        var downloaded = await downloadResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(fileBytes, downloaded);

        var timelineResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/timeline", token));
        timelineResponse.EnsureSuccessStatusCode();
        var timeline = (await timelineResponse.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;
        Assert.Contains(timeline.Items, x => x.EventType == "personnel_document_uploaded");
    }

    [Fact]
    public async Task Personnel_document_upload_denies_supervisor_role()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Documents Denied User", "documents.denied@example.com");

        var createRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/documents", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelDocumentRequest(
            "other",
            "Supervisor upload attempt",
            "attempt.txt",
            "text/plain",
            Convert.ToBase64String("denied"u8.ToArray()),
            null,
            null));
        var response = await _staffarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
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

    private sealed record RecordedComplianceCorePersonReadinessSubject(
        string SubjectType,
        string SubjectReference,
        string? SourceProduct,
        string? DisplayLabel);

    private sealed record RecordedComplianceCorePersonReadinessGateRequest(
        string Path,
        string? AuthorizationScheme,
        string? AuthorizationParameter,
        Guid TenantId,
        string ActivityContextKey,
        string? WorkflowKey,
        List<RecordedComplianceCorePersonReadinessSubject> Subjects,
        Dictionary<string, string> RuleContext);

    private sealed class RecordingComplianceCorePersonReadinessGateHandler : HttpMessageHandler
    {
        public List<RecordedComplianceCorePersonReadinessGateRequest> Requests { get; } = [];

        public string NextOutcome { get; set; } = "allow";

        public string NextReasonCode { get; set; } = "person_readiness_clear";

        public string NextMessage { get; set; } = "Person satisfies Compliance Core readiness requirements.";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? "{}"
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            var subjects = new List<RecordedComplianceCorePersonReadinessSubject>();
            if (root.TryGetProperty("subjectReferences", out var subjectReferencesElement)
                && subjectReferencesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var subjectElement in subjectReferencesElement.EnumerateArray())
                {
                    subjects.Add(new RecordedComplianceCorePersonReadinessSubject(
                        subjectElement.GetProperty("subjectType").GetString() ?? string.Empty,
                        subjectElement.GetProperty("subjectReference").GetString() ?? string.Empty,
                        subjectElement.TryGetProperty("sourceProduct", out var sourceProductElement)
                            && sourceProductElement.ValueKind != JsonValueKind.Null
                                ? sourceProductElement.GetString()
                                : null,
                        subjectElement.TryGetProperty("displayLabel", out var displayLabelElement)
                            && displayLabelElement.ValueKind != JsonValueKind.Null
                                ? displayLabelElement.GetString()
                                : null));
                }
            }

            var ruleContext = new Dictionary<string, string>();
            if (root.TryGetProperty("ruleContext", out var ruleContextElement)
                && ruleContextElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in ruleContextElement.EnumerateObject())
                {
                    ruleContext[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            Requests.Add(new RecordedComplianceCorePersonReadinessGateRequest(
                request.RequestUri?.AbsolutePath ?? string.Empty,
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter,
                root.GetProperty("tenantId").GetGuid(),
                root.GetProperty("activityContextKey").GetString() ?? string.Empty,
                root.TryGetProperty("workflowKey", out var workflowKeyElement)
                    && workflowKeyElement.ValueKind != JsonValueKind.Null
                        ? workflowKeyElement.GetString()
                        : null,
                subjects,
                ruleContext));

            var traceId = Guid.NewGuid();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    traceId,
                    tenantId = root.GetProperty("tenantId").GetGuid(),
                    workflowKey = root.TryGetProperty("workflowKey", out var responseWorkflowKey)
                        && responseWorkflowKey.ValueKind != JsonValueKind.Null
                            ? responseWorkflowKey.GetString()
                            : "can_use_person",
                    actionKey = "can_use_person",
                    activityContextKey = root.GetProperty("activityContextKey").GetString(),
                    subjectReferences = Array.Empty<object>(),
                    checkResultId = traceId,
                    ruleEvaluationRunId = (Guid?)null,
                    outcome = NextOutcome,
                    reasonCode = NextReasonCode,
                    message = NextMessage,
                    appliedRuleVersions = Array.Empty<object>(),
                    citationReferences = Array.Empty<object>(),
                    missingFacts = Array.Empty<string>(),
                    staleFacts = Array.Empty<object>(),
                    evidenceRequirements = Array.Empty<object>(),
                    remediationHints = Array.Empty<object>(),
                    appliedWaiverId = (Guid?)null,
                    appliedWaiverKey = (string?)null,
                    auditExportPath = (string?)null,
                    evaluatedAt = DateTimeOffset.UtcNow
                })
            };
        }
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
