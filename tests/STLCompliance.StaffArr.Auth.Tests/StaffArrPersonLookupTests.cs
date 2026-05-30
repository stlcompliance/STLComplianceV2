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
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Endpoints;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrPersonLookupTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _trainarrLookupToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"StaffArrPersonLookupNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"StaffArrPersonLookupStaffArr-{Guid.NewGuid():N}";

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
        _trainarrLookupToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            IntegrationEndpoints.TrainarrPersonLookupActionScope);

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
        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            await db.Database.EnsureCreatedAsync();
        }
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Person_lookup_returns_identity_placement_and_active_assignments()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var managerId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(managerId, "Lookup Manager", "lookup.manager@example.com");
        await SeedStaffPersonAsync(personId, "Lookup Worker", "lookup.worker@example.com", managerPersonId: managerId);

        var siteId = await SeedOrgUnitAsync("site", "North Plant", null, "active");
        var departmentId = await SeedOrgUnitAsync("department", "Operations", siteId, "active");
        var teamId = await SeedOrgUnitAsync("team", "Day Shift", departmentId, "active");
        var positionId = await SeedOrgUnitAsync("position", "Operator", teamId, "active");

        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            var person = await db.People.SingleAsync(x => x.Id == personId);
            person.PrimaryOrgUnitId = teamId;
            person.JobTitle = "Plant Operator";
            db.OrgUnitAssignments.Add(new OrgUnitAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                PersonId = personId,
                SiteOrgUnitId = siteId,
                DepartmentOrgUnitId = departmentId,
                TeamOrgUnitId = teamId,
                PositionOrgUnitId = positionId,
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/lookup", token));
        response.EnsureSuccessStatusCode();
        var lookup = (await response.Content.ReadFromJsonAsync<PersonLookupResponse>())!;

        Assert.Equal(personId, lookup.PersonId);
        Assert.Equal("Lookup Worker", lookup.DisplayName);
        Assert.Equal("lookup.worker@example.com", lookup.PrimaryEmail);
        Assert.Equal("Plant Operator", lookup.JobTitle);
        Assert.Equal(teamId, lookup.Placement.PrimaryOrgUnitId);
        Assert.Equal("Day Shift", lookup.Placement.PrimaryOrgUnitName);
        Assert.Equal("team", lookup.Placement.PrimaryOrgUnitType);
        Assert.Equal(managerId, lookup.Placement.ManagerPersonId);
        Assert.Equal("Lookup Manager", lookup.Placement.ManagerDisplayName);
        var assignment = Assert.Single(lookup.Placement.ActiveAssignments);
        Assert.Equal("North Plant / Operations / Day Shift / Operator", assignment.AssignmentPath);
    }

    [Fact]
    public async Task Person_lookup_query_surface_matches_nested_route_and_email_lookup()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Query Lookup User", "query.lookup@example.com");

        var nestedResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/lookup", token));
        nestedResponse.EnsureSuccessStatusCode();
        var nested = (await nestedResponse.Content.ReadFromJsonAsync<PersonLookupResponse>())!;

        var queryResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/person-lookup?personId={personId}", token));
        queryResponse.EnsureSuccessStatusCode();
        var query = (await queryResponse.Content.ReadFromJsonAsync<PersonLookupResponse>())!;

        Assert.Equal(nested.PersonId, query.PersonId);
        Assert.Equal(nested.DisplayName, query.DisplayName);
        Assert.Equal(nested.Placement.ManagerPersonId, query.Placement.ManagerPersonId);

        var emailResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/person-lookup?email=query.lookup@example.com", token));
        emailResponse.EnsureSuccessStatusCode();
        var emailLookup = (await emailResponse.Content.ReadFromJsonAsync<PersonLookupResponse>())!;
        Assert.Equal(personId, emailLookup.PersonId);
    }

    [Fact]
    public async Task Person_lookup_v1_aliases_match_nested_and_email_lookup()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "V1 Query Lookup User", "v1.query.lookup@example.com");

        var nestedResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/people/{personId}/lookup", token));
        nestedResponse.EnsureSuccessStatusCode();
        var nested = (await nestedResponse.Content.ReadFromJsonAsync<PersonLookupResponse>())!;

        var queryResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/person-lookup?personId={personId}", token));
        queryResponse.EnsureSuccessStatusCode();
        var query = (await queryResponse.Content.ReadFromJsonAsync<PersonLookupResponse>())!;

        Assert.Equal(nested.PersonId, query.PersonId);
        Assert.Equal(nested.DisplayName, query.DisplayName);
        Assert.Equal(nested.Placement.ManagerPersonId, query.Placement.ManagerPersonId);

        var emailResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/person-lookup?email=v1.query.lookup@example.com", token));
        emailResponse.EnsureSuccessStatusCode();
        var emailLookup = (await emailResponse.Content.ReadFromJsonAsync<PersonLookupResponse>())!;
        Assert.Equal(personId, emailLookup.PersonId);
    }

    [Fact]
    public async Task Person_lookup_denies_unrelated_tenant_member_reads()
    {
        var targetPersonId = Guid.NewGuid();
        await SeedStaffPersonAsync(targetPersonId, "Protected Lookup Person", "protected.lookup@example.com");

        var memberToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: Guid.NewGuid());
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{targetPersonId}/lookup", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Integration_person_lookup_allows_trainarr_service_token()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Integration Lookup User", "integration.lookup@example.com");

        var request = ServiceAuthorized(
            HttpMethod.Get,
            $"/api/integrations/person-lookup?tenantId={PlatformSeeder.DemoTenantId}&personId={personId}",
            _trainarrLookupToken);
        var response = await _staffarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var lookup = (await response.Content.ReadFromJsonAsync<PersonLookupResponse>())!;
        Assert.Equal("Integration Lookup User", lookup.DisplayName);
    }

    [Fact]
    public async Task Integration_person_lookup_rejects_routarr_source_token()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var routarrToken = await IssueServiceTokenAsync(
            adminToken,
            "routarr",
            ["staffarr"],
            IntegrationEndpoints.TrainarrPersonLookupActionScope);

        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Wrong Source Person", "wrong.source@example.com");

        var request = ServiceAuthorized(
            HttpMethod.Get,
            $"/api/integrations/person-lookup?tenantId={PlatformSeeder.DemoTenantId}&personId={personId}",
            routarrToken);
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-person-lookup-test-{Guid.NewGuid():N}",
            $"{sourceProduct} person lookup test",
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

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<global::StaffArr.Api.Services.StaffArrTokenService>();
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

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken) =>
        Authorized(method, url, serviceToken);

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
        Guid? managerPersonId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var split = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = split.FirstOrDefault() ?? "User",
            FamilyName = split.Length > 1 ? string.Join(' ', split.Skip(1)) : "Test",
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
        string status)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var orgUnit = new OrgUnit
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
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
