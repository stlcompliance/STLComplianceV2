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

public sealed class StaffArrPersonAccountAccessTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrPersonAccountAccess-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
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

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Account_access_summary_returns_no_platform_login_state_for_unlinked_person()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Alex", "Rivera", "alex.rivera@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");

        var request = Authorized(HttpMethod.Get, $"/api/people/{personId}/account-access", token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<PersonAccountAccessSummaryResponse>())!;
        Assert.Equal(personId, summary.PersonId);
        Assert.Equal("no_platform_login", summary.AccountState);
        Assert.False(summary.HasPlatformIdentity);
        Assert.False(summary.HasPlatformLogin);
        Assert.Equal("alex.rivera@example.com", summary.WorkEmail);
    }

    [Fact]
    public async Task Account_access_summary_hides_nexarr_details_when_integration_is_unconfigured()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(
            personId,
            "Taylor",
            "Jordan",
            "taylor.jordan@example.com",
            externalUserId: Guid.NewGuid(),
            hasUserAccountSnapshot: true,
            canLoginSnapshot: true);
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "hr_admin");

        var request = Authorized(HttpMethod.Get, $"/api/people/{personId}/account-access", token);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<PersonAccountAccessSummaryResponse>())!;
        Assert.Equal("account_unavailable", summary.AccountState);
        Assert.False(summary.IntegrationAvailable);
        Assert.NotNull(summary.Notice);
    }

    [Fact]
    public async Task Account_access_provision_requires_writer_role()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Casey", "Ng", "casey.ng@example.com");
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");

        var request = Authorized(HttpMethod.Post, $"/api/people/{personId}/account-access/provision", token);
        request.Content = JsonContent.Create(new ProvisionPersonAccountRequest(
            "casey.ng@example.com",
            "TemporaryPass123!",
            false));

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task SeedPersonAsync(
        Guid personId,
        string givenName,
        string familyName,
        string email,
        Guid? externalUserId = null,
        bool hasUserAccountSnapshot = false,
        bool canLoginSnapshot = false)
    {
        using var scope = _factory.Services.CreateScope();
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
            ExternalUserId = externalUserId,
            HasUserAccountSnapshot = hasUserAccountSnapshot,
            CanLoginSnapshot = canLoginSnapshot,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> launchableProductKeys,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            launchableProductKeys,
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
