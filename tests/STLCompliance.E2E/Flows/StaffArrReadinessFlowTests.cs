using STLCompliance.Shared.Integration;
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
using STLCompliance.E2E.Support;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// StaffArr baseline certification grants → person readiness ready/not_ready journey.
/// </summary>
[Trait("Category", "Integration")]
public sealed class StaffArrReadinessFlowTests : IAsyncLifetime
{
    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();

        var adminToken = await _nexarr.LoginAsync();
        var handoffToken = await _nexarr.IssueServiceTokenAsync(adminToken, "staffarr", "launch.redeem");
        var staffArrDbName = $"E2E-StaffArr-Readiness-{Guid.NewGuid():N}";

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarr.Client.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", handoffToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarr.Factory.Server.CreateHandler());
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
        await _nexarr.DisposeAsync();
    }

    [Fact]
    public async Task Person_without_certifications_is_not_ready_with_baseline_blockers()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], "tenant_admin");
        var personId = await SeedPersonAsync("E2E Blocked Driver", "e2e.blocked@example.com");

        var response = await _staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", adminToken));
        response.EnsureSuccessStatusCode();
        var readiness = (await response.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.NotEmpty(readiness.Blockers);
        Assert.Contains(readiness.Blockers, b => b.BlockerSource == "certification");
    }

    [Fact]
    public async Task Granting_baseline_certifications_makes_person_ready()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], "tenant_admin");
        var personId = await SeedPersonAsync("E2E Ready Driver", "e2e.ready@example.com");

        var definitionsResponse = await _staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, "/api/certifications", adminToken));
        definitionsResponse.EnsureSuccessStatusCode();
        var definitions = (await definitionsResponse.Content.ReadFromJsonAsync<IReadOnlyList<CertificationDefinitionResponse>>())!;

        foreach (var definition in definitions.Where(x => x.Category == "readiness"))
        {
            var grantRequest = HttpTestClient.Authorized(
                HttpMethod.Post,
                $"/api/people/{personId}/certifications",
                adminToken);
            grantRequest.Content = JsonContent.Create(new GrantPersonCertificationRequest(
                definition.CertificationDefinitionId,
                null,
                null,
                "E2E readiness grant."));
            (await _staffarrClient.SendAsync(grantRequest)).EnsureSuccessStatusCode();
        }

        var readinessResponse = await _staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", adminToken));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("ready", readiness.ReadinessStatus);
        Assert.Empty(readiness.Blockers);
    }

    private async Task<Guid> SeedPersonAsync(string displayName, string email)
    {
        var personId = Guid.NewGuid();
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            DisplayName = displayName,
            PrimaryEmail = email,
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
        return personId;
    }

    private string CreateStaffArrAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "E2E Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
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
