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

public class StaffArrPersonOffboardingTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrOffboarding-{Guid.NewGuid():N}";

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
    public async Task Offboarding_happy_path_starts_executes_and_marks_person_inactive()
    {
        var personId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Departing", "Worker", "departing.worker@example.com");
        await SeedPersonAsync(managerId, "Replacement", "Manager", "replacement.manager@example.com");

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var separationDate = DateTimeOffset.UtcNow.AddDays(7);

        var startRequest = Authorized(HttpMethod.Post, "/api/offboarding", token);
        startRequest.Content = JsonContent.Create(new StartPersonOffboardingRequest(
            personId,
            separationDate,
            "Role elimination",
            "inactive",
            DisableLoginRequested: false,
            NewManagerPersonIdForReports: null));
        var startResponse = await _staffarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<PersonOffboardingResponse>())!;
        Assert.Equal("in_progress", started.Status);
        Assert.Equal(7, started.Steps.Count);

        var executeRequest = Authorized(HttpMethod.Post, $"/api/offboarding/{started.OffboardingId}/execute", token);
        executeRequest.Content = JsonContent.Create(new ExecutePersonOffboardingRequest(null));
        var executeResponse = await _staffarrClient.SendAsync(executeRequest);
        executeResponse.EnsureSuccessStatusCode();
        var completed = (await executeResponse.Content.ReadFromJsonAsync<PersonOffboardingResponse>())!;
        Assert.Equal("completed", completed.Status);
        Assert.All(completed.Steps.Where(x => x.StepKey is not "disable_login"), step =>
            Assert.True(step.Status is "complete" or "skipped"));

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var person = await db.People.SingleAsync(x => x.Id == personId);
        Assert.Equal("inactive", person.EmploymentStatus);

        var auditCount = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.TargetId == started.OffboardingId.ToString()
                && x.Action.StartsWith("offboarding."));
        Assert.Equal(2, auditCount);
    }

    [Fact]
    public async Task Offboarding_execute_requires_replacement_manager_when_direct_reports_exist()
    {
        var personId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Supervisor", "Leaving", "supervisor.leaving@example.com");
        await SeedPersonAsync(reportId, "Direct", "Report", "direct.report@example.com", managerPersonId: personId);

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var startRequest = Authorized(HttpMethod.Post, "/api/offboarding", token);
        startRequest.Content = JsonContent.Create(new StartPersonOffboardingRequest(
            personId,
            DateTimeOffset.UtcNow.AddDays(3),
            null,
            "terminated",
            DisableLoginRequested: false,
            NewManagerPersonIdForReports: null));
        var startResponse = await _staffarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        var started = (await startResponse.Content.ReadFromJsonAsync<PersonOffboardingResponse>())!;
        Assert.Equal(1, started.ActiveDirectReportCount);

        var executeRequest = Authorized(HttpMethod.Post, $"/api/offboarding/{started.OffboardingId}/execute", token);
        executeRequest.Content = JsonContent.Create(new ExecutePersonOffboardingRequest(null));
        var executeResponse = await _staffarrClient.SendAsync(executeRequest);
        Assert.Equal(HttpStatusCode.Conflict, executeResponse.StatusCode);
    }

    [Fact]
    public async Task V1_offboarding_query_returns_active_workflow_for_person()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "V1", "Offboarding", "v1.offboarding@example.com");

        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var startRequest = Authorized(HttpMethod.Post, "/api/v1/offboarding", adminToken);
        startRequest.Content = JsonContent.Create(new StartPersonOffboardingRequest(
            personId,
            DateTimeOffset.UtcNow.AddDays(5),
            "Seasonal separation",
            "inactive",
            DisableLoginRequested: false,
            NewManagerPersonIdForReports: null));
        var startResponse = await _staffarrClient.SendAsync(startRequest);
        startResponse.EnsureSuccessStatusCode();
        Assert.StartsWith("/api/v1/offboarding/", startResponse.Headers.Location?.OriginalString);
        var started = (await startResponse.Content.ReadFromJsonAsync<PersonOffboardingResponse>())!;

        var memberToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: personId);
        var queryResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/offboarding?personId={personId:D}", memberToken));
        queryResponse.EnsureSuccessStatusCode();
        var active = (await queryResponse.Content.ReadFromJsonAsync<PersonOffboardingResponse>())!;
        Assert.Equal(started.OffboardingId, active.OffboardingId);
        Assert.Equal(personId, active.PersonId);
        Assert.Equal("in_progress", active.Status);

        var missingPersonId = Guid.NewGuid();
        var missingResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/offboarding?personId={missingPersonId:D}", adminToken));
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Offboarding_read_denied_for_non_reader_role()
    {
        var personId = Guid.NewGuid();
        var otherPersonId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Read", "Only", "read.only@example.com");
        await SeedPersonAsync(otherPersonId, "Other", "Person", "other.person@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: personId);

        var request = Authorized(HttpMethod.Get, $"/api/people/{otherPersonId}/offboarding", token);
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
