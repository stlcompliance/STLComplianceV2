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
using STLCompliance.Shared.Auth;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Endpoints;
using TrainArrIntegration = TrainArr.Api.Endpoints.IntegrationEndpoints;
using STLCompliance.E2E.Support;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// docs/23 workflow 1 — StaffArr workforce onboarding journey with TrainArr training history integration.
/// </summary>
[Trait("Category", "Integration")]
public sealed class StaffArrWorkforceOnboardingFlowTests : IAsyncLifetime
{
    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private Guid _personId;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();

        var adminToken = await _nexarr.LoginAsync();
        var staffarrToTrainarrToken = await _nexarr.IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            $"{TrainArrIntegration.IncidentRemediationIngestActionScope},{TrainArrIntegration.PersonTrainingHistoryReadActionScope}",
            ["trainarr"]);

        var staffArrDbName = $"E2E-StaffOnboard-StaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"E2E-StaffOnboard-TrainArr-{Guid.NewGuid():N}";

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", E2ENexArrHost.SigningKey);
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
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarr.Client.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("TrainArr:BaseUrl", _trainarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("TrainArr:ServiceToken", staffarrToTrainarrToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
                services.AddHttpClient<TrainArrPersonTrainingHistoryClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _trainarrFactory.Server.CreateHandler());
            });
        });
        _staffarrClient = _staffarrFactory.CreateClient();

        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        _personId = Guid.NewGuid();
        await SeedPersonAsync(_personId);
        await GrantBaselineCertificationsAsync(_personId);
        await SeedTrainarrQualificationHistoryAsync(_personId);
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _staffarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarr.DisposeAsync();
    }

    [Fact]
    public async Task Workforce_onboarding_journey_reports_qualified_after_trainarr_history_sync()
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], "tenant_admin");
        var response = await _staffarrClient.SendAsync(
            HttpTestClient.Authorized(
                HttpMethod.Get,
                $"/api/people/{_personId}/workforce-onboarding-journey",
                adminToken));
        response.EnsureSuccessStatusCode();

        var journey = (await response.Content.ReadFromJsonAsync<WorkforceOnboardingJourneyResponse>())!;
        Assert.Equal(_personId, journey.PersonId);
        Assert.Equal(WorkforceOnboardingJourneyService.JourneyKey, journey.JourneyKey);
        Assert.Contains(journey.Steps, x => x.StepKey == "workforce_profile" && x.Status == "complete");
        Assert.Contains(journey.Steps, x => x.StepKey == "trainarr_training_completed" && x.Status == "complete");
        Assert.Contains(journey.Steps, x => x.StepKey == "operational_clearance" && x.Status == "complete");
        Assert.Equal("qualified", journey.OverallStatus);
    }

    private async Task GrantBaselineCertificationsAsync(Guid personId)
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], "tenant_admin");
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
                "E2E onboarding readiness grant."));
            (await _staffarrClient.SendAsync(grantRequest)).EnsureSuccessStatusCode();
        }
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
            Summary = "Assignment created (E2E onboarding)",
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
            Summary = "Qualification issued (E2E onboarding)",
            RelatedEntityType = "qualification",
            RelatedEntityId = Guid.NewGuid(),
            SourceDomainEventId = Guid.NewGuid(),
            OccurredAt = now.AddDays(-1),
            CreatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedPersonAsync(Guid personId)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            DisplayName = "E2E Onboarding Worker",
            PrimaryEmail = "e2e.onboarding@example.com",
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
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
