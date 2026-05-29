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
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrQualificationCheckConsumptionTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"QualCheckConsumeNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"QualCheckConsumeTrainArr-{Guid.NewGuid():N}";

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

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task List_qualification_checks_returns_persisted_history()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var personId = Guid.NewGuid();
        var definitionId = await CreateTrainingDefinitionAsync(adminToken, "history_qualification");

        await TrainArrQualificationCheckTestHelper.RunQualificationCheckAsync(
            _trainarrClient,
            adminToken,
            personId,
            "history_qualification",
            definitionId);

        var listRequest = Authorized(HttpMethod.Get, $"/api/qualification-checks?staffarrPersonId={personId}", adminToken);
        var listResponse = await _trainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var history = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<QualificationCheckHistoryItemResponse>>())!;

        Assert.NotEmpty(history);
        Assert.Equal(personId, history[0].StaffarrPersonId);
        Assert.Equal("history_qualification", history[0].QualificationKey);
    }

    [Fact]
    public async Task Manual_assignment_requires_authorization_qualification_check()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var personId = Guid.NewGuid();
        var definitionId = await CreateTrainingDefinitionAsync(adminToken, "manual_gate_qualification");

        var createRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            null,
            null));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
    }

    [Fact]
    public async Task Manual_assignment_rejects_stale_qualification_check()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var personId = Guid.NewGuid();
        var definitionId = await CreateTrainingDefinitionAsync(adminToken, "stale_check_qualification");
        var check = await TrainArrQualificationCheckTestHelper.RunQualificationCheckAsync(
            _trainarrClient,
            adminToken,
            personId,
            "stale_check_qualification",
            definitionId);

        using (var scope = _trainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
            var record = await db.QualificationCheckRecords.SingleAsync(x => x.Id == check.CheckId);
            record.CheckedAt = DateTimeOffset.UtcNow.AddHours(-2);
            await db.SaveChangesAsync();
        }

        var createRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            null,
            check.CheckId));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Conflict, createResponse.StatusCode);
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string trainarrAdminToken, string qualificationKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"def_{Guid.NewGuid():N}"[..20],
            "Qualification check consumption test definition",
            "Used by qualification check consumption tests.",
            qualificationKey,
            "Qualification Check Consumption"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var definition = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return definition.TrainingDefinitionId;
    }

    private string CreateTrainArrAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey = "tenant_member")
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArr.Api.Services.TrainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
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
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }
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
}
