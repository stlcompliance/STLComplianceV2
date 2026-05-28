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
using STLCompliance.Shared.Contracts;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrFieldInboxTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TrainArrFieldInboxNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"TrainArrFieldInbox-{Guid.NewGuid():N}";

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
            builder.UseSetting("TrainArr:FrontendBaseUrl", "https://trainarr-frontend.test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedAssignmentAsync(db);
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Field_inbox_returns_assignment_with_evidence_deep_link()
    {
        var token = CreateTrainArrAccessToken(["trainarr"], "tenant_member", FieldInboxPersonId);
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));
        response.EnsureSuccessStatusCode();
        var inbox = (await response.Content.ReadFromJsonAsync<FieldInboxResponse>())!;

        Assert.Single(inbox.Items);
        var task = inbox.Items[0];
        Assert.Equal("trainarr", task.ProductKey);
        Assert.Equal("training_assignment", task.TaskType);
        Assert.Contains("/evidence", task.DeepLinkPath, StringComparison.Ordinal);
        Assert.Equal(
            $"https://trainarr-frontend.test{task.DeepLinkPath}",
            task.DeepLinkUrl);
        Assert.Equal("Evidence required", task.BlockedReason);
    }

    [Fact]
    public async Task Field_inbox_requires_authentication()
    {
        var response = await _trainarrClient.GetAsync("/api/field-inbox");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static readonly Guid FieldInboxPersonId = Guid.Parse("00000000-0000-0000-0000-0000000000ee");

    private static async Task SeedAssignmentAsync(TrainArrDbContext db)
    {
        var now = DateTimeOffset.UtcNow;
        var definition = new TrainingDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = "field_inbox_hazmat",
            Name = "Field inbox hazmat",
            QualificationKey = "hazmat",
            QualificationName = "Hazmat",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var assignment = new TrainingAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = FieldInboxPersonId,
            TrainingDefinitionId = definition.Id,
            AssignmentReason = "manual",
            Status = "in_progress",
            AssignedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingDefinitions.Add(definition);
        db.TrainingAssignments.Add(assignment);
        await db.SaveChangesAsync();
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

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey,
        Guid personId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId,
            PlatformSeeder.DemoAdminEmail,
            "Test Trainee",
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
