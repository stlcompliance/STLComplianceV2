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

public class StaffArrPersonUpdateWorkflowTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrPersonUpdate-{Guid.NewGuid():N}";

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
    public async Task Person_update_happy_path_updates_profile_fields()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Original", "User", "original.user@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var request = Authorized(HttpMethod.Put, $"/api/people/{personId}", token);
        request.Content = JsonContent.Create(new UpdateStaffPersonRequest(
            "Updated",
            "User",
            "updated.user@example.com",
            null,
            null,
            "Supervisor"));
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var updated = (await response.Content.ReadFromJsonAsync<StaffPersonDetailResponse>())!;
        Assert.Equal("Updated User", updated.DisplayName);
        Assert.Equal("updated.user@example.com", updated.PrimaryEmail);
        Assert.Equal("Supervisor", updated.JobTitle);

        var auditCount = await CountAuditEventsAsync("person.update", personId.ToString());
        Assert.Equal(1, auditCount);
    }

    [Fact]
    public async Task Person_update_denied_for_non_writer_role()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Read", "Only", "read.only@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");

        var request = Authorized(HttpMethod.Put, $"/api/people/{personId}", token);
        request.Content = JsonContent.Create(new UpdateStaffPersonRequest(
            "Denied",
            "Write",
            "denied.write@example.com",
            null,
            null,
            null));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Person_update_rejects_email_conflict()
    {
        var personId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        await SeedPersonAsync(personId, "First", "Person", "first.person@example.com");
        await SeedPersonAsync(otherId, "Second", "Person", "second.person@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");

        var request = Authorized(HttpMethod.Put, $"/api/people/{personId}", token);
        request.Content = JsonContent.Create(new UpdateStaffPersonRequest(
            "First",
            "Person",
            "second.person@example.com",
            null,
            null,
            null));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Person_employment_status_deactivate_and_reactivate()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Status", "User", "status.user@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "staffarr_admin");

        var deactivateRequest = Authorized(HttpMethod.Patch, $"/api/people/{personId}/employment-status", token);
        deactivateRequest.Content = JsonContent.Create(new UpdatePersonEmploymentStatusRequest("inactive", "Leave of absence"));
        var deactivateResponse = await _staffarrClient.SendAsync(deactivateRequest);
        deactivateResponse.EnsureSuccessStatusCode();
        var deactivated = (await deactivateResponse.Content.ReadFromJsonAsync<StaffPersonDetailResponse>())!;
        Assert.Equal("inactive", deactivated.EmploymentStatus);

        var reactivateRequest = Authorized(HttpMethod.Patch, $"/api/people/{personId}/employment-status", token);
        reactivateRequest.Content = JsonContent.Create(new UpdatePersonEmploymentStatusRequest("active", null));
        var reactivateResponse = await _staffarrClient.SendAsync(reactivateRequest);
        reactivateResponse.EnsureSuccessStatusCode();
        var reactivated = (await reactivateResponse.Content.ReadFromJsonAsync<StaffPersonDetailResponse>())!;
        Assert.Equal("active", reactivated.EmploymentStatus);
    }

    [Fact]
    public async Task Person_deactivate_rejects_active_subordinates()
    {
        var managerId = Guid.NewGuid();
        var subordinateId = Guid.NewGuid();
        await SeedPersonAsync(managerId, "Team", "Lead", "team.lead@example.com");
        await SeedPersonAsync(subordinateId, "Team", "Member", "team.member@example.com", managerId);
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var request = Authorized(HttpMethod.Patch, $"/api/people/{managerId}/employment-status", token);
        request.Content = JsonContent.Create(new UpdatePersonEmploymentStatusRequest("terminated", "Offboarding"));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Person_employment_status_update_writes_audit_event()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Audit", "Target", "audit.target@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var request = Authorized(HttpMethod.Patch, $"/api/people/{personId}/employment-status", token);
        request.Content = JsonContent.Create(new UpdatePersonEmploymentStatusRequest("inactive", null));
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var auditCount = await CountAuditEventsAsync("person.employment_status_update", personId.ToString());
        Assert.Equal(1, auditCount);
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

    private async Task<int> CountAuditEventsAsync(string action, string targetId)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        return await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == action && x.TargetId == targetId);
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
