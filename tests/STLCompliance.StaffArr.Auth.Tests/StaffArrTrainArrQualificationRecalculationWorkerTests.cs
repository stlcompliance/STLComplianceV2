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
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrQualificationRecalculationWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _sharedWorkerToTrainarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RecalcNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"RecalcTrainArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            QualificationRecalculationService.ProcessRecalculationsActionScope);

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
    public async Task Process_recalculation_batch_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/qualification-recalculation/process-batch",
            new ProcessQualificationRecalculationsRequest(null, DateTimeOffset.UtcNow, 50, 24));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_recalculation_batch_persists_materialized_state()
    {
        var personId = Guid.NewGuid();
        var issue = await SeedIssuedQualificationIssueAsync(personId);
        await SeedRecalculationSettingsAsync(PlatformSeeder.DemoTenantId);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/qualification-recalculation/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessQualificationRecalculationsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            24));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessQualificationRecalculationsResponse>())!;
        Assert.Equal(1, body.RecalculatedCount);
        Assert.Contains(issue.Id, body.RecalculatedIssueIds);

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var state = await db.QualificationRecalculationStates.SingleAsync(
            x => x.QualificationIssueId == issue.Id);
        Assert.Equal("allow", state.Outcome);
        Assert.Equal("recalc_worker", state.QualificationKey);

        var run = await db.QualificationRecalculationRuns.SingleAsync(
            x => x.QualificationIssueId == issue.Id);
        Assert.Equal("recalculated", run.Outcome);
        Assert.Equal("allow", run.CheckOutcome);
    }

    [Fact]
    public async Task List_pending_recalculation_returns_candidates_before_processing()
    {
        var personId = Guid.NewGuid();
        var issue = await SeedIssuedQualificationIssueAsync(personId);
        await SeedRecalculationSettingsAsync(PlatformSeeder.DemoTenantId);

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/qualification-recalculation/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10&stalenessHours=24");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);

        var listResponse = await _trainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingQualificationRecalculationsResponse>())!;
        Assert.Contains(pending.Items, x => x.QualificationIssueId == issue.Id);
    }

    private async Task<QualificationIssue> SeedIssuedQualificationIssueAsync(Guid personId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var grantPublicationId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMonths(6);
        var now = DateTimeOffset.UtcNow;

        var definitionId = Guid.NewGuid();
        db.TrainingDefinitions.Add(new TrainingDefinition
        {
            Id = definitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = "recalc_worker_def",
            Name = "Recalculation Worker Training",
            Description = "Seeded definition for recalculation worker test.",
            QualificationKey = "recalc_worker",
            QualificationName = "Recalculation Worker Qualification",
            Status = "active",
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6),
        });

        db.CertificationPublications.Add(new CertificationPublication
        {
            Id = grantPublicationId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            QualificationKey = "recalc_worker",
            QualificationName = "Recalculation Worker Qualification",
            PublicationType = "qualification_grant",
            BlockerType = "issued",
            Message = "Seeded qualification grant for recalculation worker integration test.",
            Status = "published",
            ExpiresAt = expiresAt,
            PublishedAt = now.AddMonths(-6),
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6),
        });

        var assignmentId = Guid.NewGuid();
        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = assignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            TrainingDefinitionId = definitionId,
            AssignmentReason = "manual",
            Status = "completed",
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6),
        });

        var issue = new QualificationIssue
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingAssignmentId = assignmentId,
            StaffarrPersonId = personId,
            QualificationKey = "recalc_worker",
            QualificationName = "Recalculation Worker Qualification",
            GrantPublicationId = grantPublicationId,
            Status = "issued",
            IssuedAt = now.AddMonths(-6),
            ExpiresAt = expiresAt,
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6),
        };
        db.QualificationIssues.Add(issue);

        await db.SaveChangesAsync();
        return issue;
    }

    private async Task SeedRecalculationSettingsAsync(Guid tenantId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantQualificationRecalculationSettings.Add(new TenantQualificationRecalculationSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IsEnabled = true,
            StalenessHours = 24,
            AutoSuspendOnBlock = false,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-recalc-{Guid.NewGuid():N}",
            $"{sourceProduct} qualification recalculation test",
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
