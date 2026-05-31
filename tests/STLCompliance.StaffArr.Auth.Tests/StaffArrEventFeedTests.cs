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
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Endpoints;
using StaffArr.Api.Entities;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrEventFeedTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _routarrEventFeedToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"StaffArrEventFeedNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"StaffArrEventFeedStaffArr-{Guid.NewGuid():N}";

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
        _routarrEventFeedToken = await IssueServiceTokenAsync(
            adminToken,
            "routarr",
            ["staffarr"],
            IntegrationEndpoints.EventFeedReadActionScope);

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedEventFeedAsync(db);
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Integration_event_feed_projects_staffarr_workforce_events()
    {
        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/v1/integrations/event-feed?tenantId={PlatformSeeder.DemoTenantId}&pageSize=25",
            _routarrEventFeedToken));
        response.EnsureSuccessStatusCode();

        var feed = (await response.Content.ReadFromJsonAsync<StaffArrEventFeedResponse>())!;
        Assert.True(feed.TotalCount >= 5);
        Assert.Contains(feed.Items, x => x.EventKind == "person.created");
        Assert.Contains(feed.Items, x => x.EventKind == "person.activated");
        Assert.Contains(feed.Items, x => x.EventKind == "site.created");
        Assert.Contains(feed.Items, x => x.EventKind == "permission.revoked");
        Assert.Contains(feed.Items, x => x.EventKind == "override.revoked");
        Assert.Contains(feed.Items, x => x.EventKind == "incident.created");
    }

    [Fact]
    public async Task Integration_event_feed_rejects_unauthorized_source_product()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var staffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            ["staffarr"],
            IntegrationEndpoints.EventFeedReadActionScope);

        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/integrations/event-feed?tenantId={PlatformSeeder.DemoTenantId}",
            staffarrToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task SeedEventFeedAsync(StaffArrDbContext db)
    {
        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        var personId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var overrideId = Guid.NewGuid();
        var incidentId = Guid.NewGuid();

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Event",
            FamilyName = "Worker",
            DisplayName = "Event Worker",
            PrimaryEmail = "event.worker@demo.stl",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.OrgUnits.Add(new OrgUnit
        {
            Id = siteId,
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = "site",
            Name = "North Yard",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.PersonRoleAssignments.Add(new PersonRoleAssignment
        {
            Id = assignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = personId,
            RoleTemplateId = Guid.NewGuid(),
            ScopeType = "tenant",
            Status = "inactive",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.PersonReadinessOverrides.Add(new PersonReadinessOverride
        {
            Id = overrideId,
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = personId,
            Status = "cleared",
            Reason = "Temporary authorization ended.",
            GrantedAt = now,
            GrantedByUserId = PlatformSeeder.DemoAdminUserId,
            ClearedAt = now.AddMinutes(2),
            ClearedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now.AddMinutes(2)
        });

        db.AuditEvents.AddRange(
            BuildAudit("person.create", "person", personId, now),
            BuildAudit("person.employment_status_update", "person", personId, now.AddMinutes(1)),
            BuildAudit("org_unit.create", "org_unit", siteId, now.AddMinutes(2)),
            BuildAudit("person_role_assignment.status_update", "person_role_assignment", assignmentId, now.AddMinutes(3)),
            BuildAudit("readiness_override.clear", "person_readiness_override", overrideId, now.AddMinutes(4)),
            BuildAudit("incident.product_intake", "personnel_incident", incidentId, now.AddMinutes(5)));

        await db.SaveChangesAsync();
    }

    private static StaffArrAuditEvent BuildAudit(
        string action,
        string targetType,
        Guid targetId,
        DateTimeOffset occurredAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ActorUserId = PlatformSeeder.DemoAdminUserId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId.ToString(),
            Result = "Succeeded",
            CorrelationId = Guid.NewGuid(),
            OccurredAt = occurredAt
        };

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
        return (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-staffarr-event-feed-{Guid.NewGuid():N}",
            "staffarr event feed test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
