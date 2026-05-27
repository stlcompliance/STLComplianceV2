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
using StaffArr.Api.Entities;
using TrainArrIncidentIntegration = TrainArr.Api.Endpoints.IntegrationEndpoints;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrIncidentRoutingTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _staffarrToTrainarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"StaffArrIncidentRouteNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"StaffArrIncidentRouteStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"StaffArrIncidentRouteTrainArr-{Guid.NewGuid():N}";

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
        _staffarrToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            ["trainarr"],
            TrainArrIncidentIntegration.IncidentRemediationIngestActionScope);

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArr.Api.Data.TrainArrDbContext>(services);
                services.AddDbContext<TrainArr.Api.Data.TrainArrDbContext>(options =>
                    options.UseInMemoryDatabase(trainArrDbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("TrainArr:BaseUrl", _trainarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("TrainArr:ServiceToken", _staffarrToTrainarrToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));

                services.AddHttpClient<global::StaffArr.Api.Services.TrainArrIncidentRemediationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _trainarrFactory.Server.CreateHandler());
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Training_compliance_incident_routes_to_trainarr_with_mirror_and_audit()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Routing Subject", "routing.subject@example.com");
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "training_compliance",
            "high",
            "Missed annual compliance training",
            "Employee missed required annual compliance training and must complete remediation before assignment.",
            DateTimeOffset.UtcNow.AddHours(-2)));

        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var routeRequest = Authorized(
            HttpMethod.Post,
            $"/api/incidents/{created.IncidentId}/route-to-trainarr",
            adminToken);
        var routeResponse = await _staffarrClient.SendAsync(routeRequest);
        routeResponse.EnsureSuccessStatusCode();
        var routed = (await routeResponse.Content.ReadFromJsonAsync<RouteIncidentToTrainarrResponse>())!;

        Assert.Equal("routed", routed.TrainarrRouting.RoutingStatus);
        Assert.NotEqual(Guid.Empty, routed.TrainarrRouting.TrainarrRemediationId);

        var detailResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/incidents/{created.IncidentId}", adminToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.NotNull(detail.TrainarrRouting);
        Assert.Equal(routed.TrainarrRouting.TrainarrRemediationId, detail.TrainarrRouting!.TrainarrRemediationId);

        using (var staffScope = _staffarrFactory.Services.CreateScope())
        {
            var staffDb = staffScope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            var audit = await staffDb.AuditEvents.CountAsync(
                x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "incident.route_trainarr");
            Assert.True(audit >= 1);
        }

        using (var trainScope = _trainarrFactory.Services.CreateScope())
        {
            var trainDb = trainScope.ServiceProvider.GetRequiredService<TrainArr.Api.Data.TrainArrDbContext>();
            var remediation = await trainDb.StaffarrIncidentRemediations.FirstOrDefaultAsync(
                x => x.TenantId == PlatformSeeder.DemoTenantId && x.StaffarrIncidentId == created.IncidentId);
            Assert.NotNull(remediation);
            Assert.Equal("intake_received", remediation!.Status);
            var audit = await trainDb.AuditEvents.CountAsync(
                x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "incident_remediation.intake");
            Assert.True(audit >= 1);
        }
    }

    [Fact]
    public async Task Safety_incident_route_to_trainarr_rejected()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Safety Subject", "safety.subject@example.com");
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "safety",
            "medium",
            "Near miss in warehouse aisle",
            "Forklift near-miss documented for safety review; no injury reported during inbound shift operations.",
            DateTimeOffset.UtcNow.AddHours(-1)));

        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var routeRequest = Authorized(
            HttpMethod.Post,
            $"/api/incidents/{created.IncidentId}/route-to-trainarr",
            adminToken);
        var routeResponse = await _staffarrClient.SendAsync(routeRequest);

        Assert.Equal(HttpStatusCode.BadRequest, routeResponse.StatusCode);
    }

    [Fact]
    public async Task Incident_remediation_ingest_rejects_missing_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/incident-remediations");
        request.Content = JsonContent.Create(new TrainArr.Api.Contracts.IngestStaffarrIncidentRemediationRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "training_compliance",
            "high",
            "Missed annual compliance training",
            "Employee missed required annual compliance training and must complete remediation before assignment.",
            DateTimeOffset.UtcNow.AddHours(-2),
            DateTimeOffset.UtcNow.AddHours(-1)));

        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Incident_route_to_trainarr_is_idempotent()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Idempotent Subject", "idempotent.subject@example.com");
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "training_compliance",
            "medium",
            "Expired operator certification",
            "Operator certification expired and requires retraining before equipment assignment can resume.",
            DateTimeOffset.UtcNow.AddDays(-1)));

        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var firstRoute = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/incidents/{created.IncidentId}/route-to-trainarr", adminToken));
        firstRoute.EnsureSuccessStatusCode();
        var first = (await firstRoute.Content.ReadFromJsonAsync<RouteIncidentToTrainarrResponse>())!;

        var secondRoute = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/incidents/{created.IncidentId}/route-to-trainarr", adminToken));
        secondRoute.EnsureSuccessStatusCode();
        var second = (await secondRoute.Content.ReadFromJsonAsync<RouteIncidentToTrainarrResponse>())!;

        Assert.Equal(first.TrainarrRouting.TrainarrRemediationId, second.TrainarrRouting.TrainarrRemediationId);
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
            $"{sourceProduct}-incident-route-{Guid.NewGuid():N}",
            $"{sourceProduct} incident route test",
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

    private async Task SeedStaffPersonAsync(Guid personId, string displayName, string email)
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
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
