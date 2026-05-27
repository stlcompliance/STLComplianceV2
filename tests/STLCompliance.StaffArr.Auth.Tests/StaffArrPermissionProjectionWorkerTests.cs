using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrPermissionProjectionWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _adminToken = null!;
    private string _sharedWorkerToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PermissionProjectionNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"PermissionProjectionStaffArr-{Guid.NewGuid():N}";

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

        _adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _sharedWorkerToStaffarrToken = await IssueServiceTokenAsync(
            _adminToken,
            "shared-worker",
            ["staffarr"],
            PermissionProjectionService.ProjectPermissionsActionScope);

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
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _staffarrClient.PostAsJsonAsync(
            "/api/internal/permission-projections/process-batch",
            new ProcessPermissionProjectionsRequest(PlatformSeeder.DemoTenantId, null, 100, 1));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_rejects_trainarr_source_token()
    {
        var trainarrToken = await IssueServiceTokenAsync(
            _adminToken,
            "trainarr",
            ["staffarr"],
            PermissionProjectionService.ProjectPermissionsActionScope);

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/permission-projections/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPermissionProjectionsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            100,
            1));

        var response = await _staffarrClient.SendAsync(processRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_returns_active_people_before_processing()
    {
        var personId = await SeedPersonWithRoleAssignmentAsync();

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/permission-projections/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=20&stalenessHours=1");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);

        var listResponse = await _staffarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingPermissionProjectionsResponse>())!;
        Assert.Contains(pending.Items, x => x.PersonId == personId);
    }

    [Fact]
    public async Task Process_batch_refreshes_projection_and_effective_read_uses_materialized_rows()
    {
        var personId = await SeedPersonWithRoleAssignmentAsync();
        var asOf = DateTimeOffset.UtcNow;

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/permission-projections/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPermissionProjectionsRequest(
            PlatformSeeder.DemoTenantId,
            asOf,
            100,
            1));

        var processResponse = await _staffarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessPermissionProjectionsResponse>())!;
        Assert.True(body.RefreshedCount >= 1);
        Assert.Contains(
            body.RefreshedProjections,
            x => x.PersonId == personId && x.PermissionCount >= 1);

        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            var projection = await db.PersonPermissionProjections
                .Include(x => x.Entries)
                .FirstAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.PersonId == personId);
            Assert.True(projection.PermissionCount >= 1);
            Assert.Contains(projection.Entries, x => x.PermissionKey == "staffarr.people.read");
        }

        var effectiveRequest = Authorized(
            HttpMethod.Get,
            $"/api/people/{personId}/permissions/effective",
            CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin"));
        var effectiveResponse = await _staffarrClient.SendAsync(effectiveRequest);
        effectiveResponse.EnsureSuccessStatusCode();
        var effective = (await effectiveResponse.Content.ReadFromJsonAsync<EffectivePermissionProjectionResponse>())!;
        Assert.Contains(effective.Permissions, p => p.PermissionKey == "staffarr.people.read");
        Assert.True(effective.ComputedAt >= asOf.AddMinutes(-1));
    }

    private async Task<Guid> SeedPersonWithRoleAssignmentAsync()
    {
        var personId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            db.People.Add(new StaffPerson
            {
                Id = personId,
                TenantId = PlatformSeeder.DemoTenantId,
                GivenName = "Projection",
                FamilyName = "Worker",
                DisplayName = "Projection Worker",
                PrimaryEmail = $"projection.worker.{Guid.NewGuid():N}@example.com",
                EmploymentStatus = "active",
                CreatedAt = now,
                UpdatedAt = now
            });
            await db.SaveChangesAsync();
        }

        var permissionRequest = Authorized(HttpMethod.Post, "/api/permissions", CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin"));
        permissionRequest.Content = JsonContent.Create(new UpsertPermissionTemplateRequest(
            "staffarr.people.read",
            "People Read",
            "Read access."));
        var permissionResponse = await _staffarrClient.SendAsync(permissionRequest);
        permissionResponse.EnsureSuccessStatusCode();
        var permissionTemplate = (await permissionResponse.Content.ReadFromJsonAsync<PermissionTemplateSummaryResponse>())!;

        var roleRequest = Authorized(HttpMethod.Post, "/api/roles", CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin"));
        roleRequest.Content = JsonContent.Create(new CreateRoleTemplateRequest(
            "staffarr.viewer",
            "StaffArr Viewer",
            "Viewer role.",
            [new RoleTemplatePermissionInput(permissionTemplate.PermissionTemplateId, "tenant", null)]));
        var roleResponse = await _staffarrClient.SendAsync(roleRequest);
        roleResponse.EnsureSuccessStatusCode();
        var roleTemplate = (await roleResponse.Content.ReadFromJsonAsync<RoleTemplateResponse>())!;

        var assignmentRequest = Authorized(HttpMethod.Post, $"/api/people/{personId}/role-assignments", CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin"));
        assignmentRequest.Content = JsonContent.Create(new CreatePersonRoleAssignmentRequest(
            roleTemplate.RoleTemplateId,
            "tenant",
            null));
        var assignmentResponse = await _staffarrClient.SendAsync(assignmentRequest);
        assignmentResponse.EnsureSuccessStatusCode();

        return personId;
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-permission-projection-{Guid.NewGuid():N}",
            $"{sourceProduct} permission projection test",
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
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
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<TContext>)
            || d.ServiceType == typeof(TContext)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
