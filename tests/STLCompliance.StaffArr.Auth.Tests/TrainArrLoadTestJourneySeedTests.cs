using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using STLCompliance.Shared.Operations.LoadTesting;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrLoadTestJourneySeedTests : IAsyncLifetime
{
    private WebApplicationFactory<global::TrainArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"TrainArrJourneySeed-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Load_test_journey_seed_is_idempotent_and_creates_issued_qualification_mirror()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var firstResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlTrainArrLoadTestJourneySeedCatalog.SeedEndpointPath, adminToken));
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<LoadTestJourneySeedResponse>())!;
        Assert.Equal(StlTrainArrLoadTestJourneySeedCatalog.SubjectPersonId, first.StaffarrPersonId);
        Assert.Equal(StlTrainArrLoadTestJourneySeedCatalog.QualificationKey, first.QualificationKey);
        Assert.True(first.TrainingDefinitionCreated);
        Assert.True(first.TrainingAssignmentCreated);
        Assert.True(first.QualificationIssueCreated);
        Assert.True(first.QualificationGrantPublicationCreated);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
            var issue = await db.QualificationIssues.SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.StaffarrPersonId == StlTrainArrLoadTestJourneySeedCatalog.SubjectPersonId
                && x.QualificationKey == StlTrainArrLoadTestJourneySeedCatalog.QualificationKey);
            Assert.Equal("issued", issue.Status);
            Assert.Equal(1, await db.AuditEvents.CountAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "load_test_journey.seed"));
        }

        var secondResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlTrainArrLoadTestJourneySeedCatalog.SeedEndpointPath, adminToken));
        secondResponse.EnsureSuccessStatusCode();
        var second = (await secondResponse.Content.ReadFromJsonAsync<LoadTestJourneySeedResponse>())!;
        Assert.False(second.TrainingDefinitionCreated);
        Assert.False(second.TrainingAssignmentCreated);
        Assert.False(second.QualificationIssueCreated);
        Assert.False(second.QualificationGrantPublicationCreated);
        Assert.Equal(first.QualificationIssueId, second.QualificationIssueId);
    }

    [Fact]
    public async Task Load_test_journey_seed_denied_for_read_only_role()
    {
        var token = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlTrainArrLoadTestJourneySeedCatalog.SeedEndpointPath, token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArr.Api.Services.TrainArrTokenService>();
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
