using System.Net;
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

public sealed class StaffArrIntegrationSurfaceTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "staffarr-integration-surface-test-signing-key";
        var databaseName = $"StaffArrIntegrationSurface-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(databaseName));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Integration_aliases_are_served_for_staffarr_surface()
    {
        var token = CreateStaffArrToken();
        var personId = await SeedPersonOnlyAsync();

        var routes = new[]
        {
            "/api/v1/integrations/persons",
            "/api/v1/integrations/org-units",
            "/api/v1/integrations/locations",
            $"/api/v1/integrations/persons/{personId}",
            $"/api/v1/integrations/persons/{personId}/readiness",
            $"/api/v1/integrations/persons/{personId}/permissions",
            $"/api/v1/integrations/persons/{personId}/history",
            $"/api/v1/integrations/persons/{personId}/restrictions",
            $"/api/v1/integrations/incidents",
            "/api/v1/integrations/audit-packages"
        };

        foreach (var route in routes)
        {
            using var request = string.Equals(route, "/api/v1/integrations/audit-packages", StringComparison.Ordinal)
                ? Authorized(HttpMethod.Post, route, token)
                : Authorized(HttpMethod.Get, route, token);

            if (string.Equals(route, "/api/v1/integrations/audit-packages", StringComparison.Ordinal))
            {
                request.Content = JsonContent.Create(new CreateAuditPackageGenerationJobRequest(
                    "json",
                    null,
                    null));
            }

            var response = await _client.SendAsync(request);
            Assert.True(
                response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Accepted or HttpStatusCode.Created,
                $"{route} returned {(int)response.StatusCode} {response.StatusCode}");
        }
    }

    [Fact]
    public async Task Integration_restrictions_lift_round_trips()
    {
        var token = CreateStaffArrToken();
        var personId = await SeedPersonOnlyAsync();

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/integrations/restrictions", token);
        createRequest.Content = JsonContent.Create(new CreateRestrictionRequest(
            personId,
            "Temporary restriction for integration testing.",
            DateTimeOffset.UtcNow.AddHours(1)));

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ReadinessOverrideResponse>())!;

        var liftResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/integrations/restrictions/{created.OverrideId}/lift", token));
        liftResponse.EnsureSuccessStatusCode();
    }

    private async Task SeedAsync(StaffArrDbContext db)
    {
        var now = DateTimeOffset.UtcNow;
        db.People.Add(new StaffPerson
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Integration",
            FamilyName = "Tester",
            DisplayName = "Integration Tester",
            PrimaryEmail = "integration.tester@example.com",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.OrgUnits.Add(new OrgUnit
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = "site",
            Name = "Integration Site",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.OrgUnits.Add(new OrgUnit
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            TenantId = PlatformSeeder.DemoTenantId,
            UnitType = "department",
            Name = "Integration Department",
            ParentOrgUnitId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedPersonOnlyAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var personId = Guid.NewGuid();
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Route",
            FamilyName = "Person",
            DisplayName = "Route Person",
            PrimaryEmail = $"route.person.{Guid.NewGuid():N}@example.com",
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
        return personId;
    }

    private string CreateStaffArrToken()
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Integration Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            "tenant_admin",
            ["staffarr"],
            isPlatformAdmin: false);
        return token;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
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
