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
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Endpoints;
using STLCompliance.Shared.Auth;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrWorkforceOnboardingJourneyTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _staffarrToTrainarrToken = null!;
    private Guid _personId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"WorkforceJourneyNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"WorkforceJourneyStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"WorkforceJourneyTrainArr-{Guid.NewGuid():N}";

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
            $"{IntegrationEndpoints.IncidentRemediationIngestActionScope},{IntegrationEndpoints.PersonTrainingHistoryReadActionScope}");

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
                services.AddHttpClient<TrainArrPersonTrainingHistoryClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _trainarrFactory.Server.CreateHandler());
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
        _personId = PlatformSeeder.DemoAdminUserId;
        await SeedStaffPersonAsync(_personId);
        await SeedTrainarrQualificationHistoryAsync(_personId);
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
    public async Task Workforce_onboarding_journey_returns_docs_23_steps_for_person()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], "tenant_admin");
        var response = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/people/{_personId:D}/workforce-onboarding-journey",
                adminToken));
        response.EnsureSuccessStatusCode();

        var journey = (await response.Content.ReadFromJsonAsync<WorkforceOnboardingJourneyResponse>())!;
        Assert.Equal(_personId, journey.PersonId);
        Assert.Equal(WorkforceOnboardingJourneyService.JourneyKey, journey.JourneyKey);
        Assert.True(journey.Steps.Count >= 7);
        Assert.Contains(journey.Steps, x => x.StepKey == "workforce_profile" && x.Status == "complete");
        Assert.Contains(
            journey.Steps,
            x => x.StepKey == "trainarr_training_completed" && x.Status == "complete");
    }

    [Fact]
    public async Task Workforce_onboarding_journey_requires_people_read_for_other_person()
    {
        var response = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/people/{_personId:D}/workforce-onboarding-journey",
                CreateStaffArrAccessToken(["staffarr"], "tenant_member", personId: Guid.NewGuid())));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task SeedTrainarrQualificationHistoryAsync(Guid personId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.PersonTrainingHistoryEntries.Add(new PersonTrainingHistoryEntry
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            EventKind = TrainingDomainEventKinds.AssignmentCreated,
            Summary = "Assignment created (journey seed)",
            RelatedEntityType = "training_assignment",
            RelatedEntityId = Guid.NewGuid(),
            SourceDomainEventId = Guid.NewGuid(),
            OccurredAt = now.AddDays(-2),
            CreatedAt = now,
        });
        db.PersonTrainingHistoryEntries.Add(new PersonTrainingHistoryEntry
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            EventKind = TrainingDomainEventKinds.QualificationIssued,
            Summary = "Qualification issued (journey seed)",
            RelatedEntityType = "qualification",
            RelatedEntityId = Guid.NewGuid(),
            SourceDomainEventId = Guid.NewGuid(),
            OccurredAt = now.AddDays(-1),
            CreatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedStaffPersonAsync(Guid personId)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        if (await db.People.AnyAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.Id == personId))
        {
            return;
        }

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Demo",
            FamilyName = "Admin",
            DisplayName = "Demo Admin",
            PrimaryEmail = PlatformSeeder.DemoAdminEmail,
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        Guid? personId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var userId = personId ?? PlatformSeeder.DemoAdminUserId;
        var (token, _) = tokenService.CreateAccessToken(
            userId,
            userId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return token;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-workforce-journey-{Guid.NewGuid():N}",
            "workforce journey test",
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
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return login.AccessToken;
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

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        foreach (var descriptor in services
                     .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
                     .ToList())
        {
            services.Remove(descriptor);
        }
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
