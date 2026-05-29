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
using PersonnelUpdateRequestTypes = StaffArr.Api.Entities.PersonnelUpdateRequestTypes;
using PersonnelUpdateRequestStatuses = StaffArr.Api.Entities.PersonnelUpdateRequestStatuses;
using StaffArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrMePortalTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrMePortal-{Guid.NewGuid():N}";

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(dbName));
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
        await _staffarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Me_portal_summary_and_update_request_intake_for_self()
    {
        var personId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Self", "Worker", "self.worker@example.com", managerId);
        await SeedPersonAsync(managerId, "Team", "Manager", "team.manager@example.com");
        await SeedPersonAsync(reportId, "Direct", "Report", "direct.report@example.com", managerPersonId: personId);

        var token = CreateStaffArrAccessToken(["staffarr", "trainarr"], tenantRoleKey: "tenant_member", personId: personId);

        var portalRequest = Authorized(HttpMethod.Get, "/api/me/portal", token);
        var portalResponse = await _staffarrClient.SendAsync(portalRequest);
        portalResponse.EnsureSuccessStatusCode();
        var portal = (await portalResponse.Content.ReadFromJsonAsync<MePortalSummaryResponse>())!;
        Assert.Equal(personId, portal.Session.PersonId);
        Assert.Equal("self.worker@example.com", portal.Profile.PrimaryEmail);
        Assert.Equal(managerId, portal.Profile.Placement.ManagerPersonId);
        Assert.Equal(1, portal.DirectReportCount);
        Assert.Contains("trainarr", portal.ProductAccess);

        var submitRequest = Authorized(HttpMethod.Post, "/api/me/update-requests", token);
        submitRequest.Content = JsonContent.Create(new SubmitPersonnelUpdateRequest(
            PersonnelUpdateRequestTypes.PhoneUpdate,
            "work_phone",
            "+1 555 0000",
            "+1 555 0100",
            "Updated mobile number"));
        var submitResponse = await _staffarrClient.SendAsync(submitRequest);
        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);
        var created = (await submitResponse.Content.ReadFromJsonAsync<PersonnelUpdateRequestResponse>())!;
        Assert.Equal(personId, created.PersonId);
        Assert.Equal("submitted", created.Status);

        var listRequest = Authorized(HttpMethod.Get, "/api/me/update-requests", token);
        var listResponse = await _staffarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonnelUpdateRequestResponse>>())!;
        Assert.Single(listed);
        Assert.Equal(created.RequestId, listed[0].RequestId);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditCount = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.TargetId == created.RequestId.ToString()
                && x.Action == "personnel_update.submitted");
        Assert.Equal(1, auditCount);
    }

    [Fact]
    public async Task Me_incident_self_report_intake_and_history()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Self", "Worker", "self.worker@example.com");

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: personId);
        var occurredAt = DateTimeOffset.UtcNow.AddHours(-2);

        var submitRequest = Authorized(HttpMethod.Post, "/api/me/incidents", token);
        submitRequest.Content = JsonContent.Create(new SubmitSelfReportedPersonnelIncidentRequest(
            "safety",
            "medium",
            "Slip on loading dock",
            "I slipped on a wet loading dock while moving pallets.",
            occurredAt));
        var submitResponse = await _staffarrClient.SendAsync(submitRequest);
        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);
        var created = (await submitResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Equal(personId, created.PersonId);
        Assert.Equal("submitted", created.Status);
        Assert.Equal("safety", created.ReasonCategoryKey);

        var listRequest = Authorized(HttpMethod.Get, "/api/me/incidents", token);
        var listResponse = await _staffarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonnelIncidentSummaryResponse>>())!;
        Assert.Single(listed);
        Assert.Equal(created.IncidentId, listed[0].IncidentId);

        var getRequest = Authorized(HttpMethod.Get, $"/api/me/incidents/{created.IncidentId}", token);
        var getResponse = await _staffarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var detail = (await getResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Equal("Slip on loading dock", detail.Title);

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditCount = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.TargetId == created.IncidentId.ToString()
                && x.Action == "incident.self_report.submitted");
        Assert.Equal(1, auditCount);
    }

    [Fact]
    public async Task Me_incident_detail_denied_for_other_person_without_incidents_read()
    {
        var personId = Guid.NewGuid();
        var otherPersonId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Self", "Only", "self.only@example.com");
        await SeedPersonAsync(otherPersonId, "Other", "Person", "other.person@example.com");

        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin", personId: personId);
        var submitRequest = Authorized(HttpMethod.Post, "/api/incidents", adminToken);
        submitRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            otherPersonId,
            "conduct",
            "low",
            "Other person incident",
            "This incident belongs to another person for access testing.",
            DateTimeOffset.UtcNow.AddHours(-1)));
        var submitResponse = await _staffarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();
        var created = (await submitResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var memberToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: personId);
        var getRequest = Authorized(HttpMethod.Get, $"/api/me/incidents/{created.IncidentId}", memberToken);
        var getResponse = await _staffarrClient.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.Forbidden, getResponse.StatusCode);
    }

    [Fact]
    public async Task Me_team_dashboard_lists_direct_report_readiness_and_pending_requests()
    {
        var managerId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        await SeedPersonAsync(managerId, "Team", "Manager", "team.manager@example.com");
        await SeedPersonAsync(reportId, "Direct", "Report", "direct.report@example.com", managerPersonId: managerId);

        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            var now = DateTimeOffset.UtcNow;
            db.PersonnelUpdateRequests.Add(new PersonnelUpdateRequest
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                PersonId = reportId,
                RequestType = PersonnelUpdateRequestTypes.PhoneUpdate,
                Status = PersonnelUpdateRequestStatuses.Submitted,
                FieldKey = "work_phone",
                RequestedValue = "+1 555 0100",
                SubmittedByUserId = PlatformSeeder.DemoAdminUserId,
                SubmittedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
            });
            await db.SaveChangesAsync();
        }

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: managerId);
        var request = Authorized(HttpMethod.Get, "/api/me/team", token);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var dashboard = (await response.Content.ReadFromJsonAsync<MyTeamDashboardResponse>())!;
        Assert.Equal(1, dashboard.DirectReportCount);
        Assert.Equal(1, dashboard.PendingUpdateRequestCount);
        Assert.Single(dashboard.Members);
        Assert.Equal(reportId, dashboard.Members[0].Summary.PersonId);
        Assert.Single(dashboard.PendingUpdateRequests);
    }

    [Fact]
    public async Task Me_subordinates_lists_direct_reports_without_people_read()
    {
        var managerId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        await SeedPersonAsync(managerId, "Supervisor", "Self", "supervisor.self@example.com");
        await SeedPersonAsync(reportId, "Direct", "Report", "direct.report@example.com", managerPersonId: managerId);

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: managerId);
        var request = Authorized(HttpMethod.Get, "/api/me/subordinates", token);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var subordinates = (await response.Content.ReadFromJsonAsync<IReadOnlyList<SubordinateSummaryResponse>>())!;
        Assert.Single(subordinates);
        Assert.Equal(reportId, subordinates[0].PersonId);
    }

    [Fact]
    public async Task Me_update_request_detail_denied_for_other_person_without_people_read()
    {
        var personId = Guid.NewGuid();
        var otherPersonId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Self", "Only", "self.only@example.com");
        await SeedPersonAsync(otherPersonId, "Other", "Person", "other.person@example.com");

        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin", personId: personId);
        var submitRequest = Authorized(HttpMethod.Post, "/api/me/update-requests", adminToken);
        submitRequest.Content = JsonContent.Create(new SubmitPersonnelUpdateRequest(
            PersonnelUpdateRequestTypes.Other,
            "job_title",
            null,
            "Lead Technician",
            null));
        var submitResponse = await _staffarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();
        var created = (await submitResponse.Content.ReadFromJsonAsync<PersonnelUpdateRequestResponse>())!;

        var memberToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: otherPersonId);
        var getRequest = Authorized(HttpMethod.Get, $"/api/me/update-requests/{created.RequestId}", memberToken);
        var getResponse = await _staffarrClient.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.Forbidden, getResponse.StatusCode);
    }

    private async Task SeedPersonAsync(
        Guid personId,
        string givenName,
        string familyName,
        string email,
        Guid? managerPersonId = null)
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
            ManagerPersonId = managerPersonId,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
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
