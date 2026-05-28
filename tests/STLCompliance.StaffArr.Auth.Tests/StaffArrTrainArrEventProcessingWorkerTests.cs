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
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrEventProcessingWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _sharedWorkerToTrainarrToken = null!;
    private string _trainarrAdminToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"EventProcNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"EventProcTrainArr-{Guid.NewGuid():N}";

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
            TrainingEventProcessingService.ProcessEventsActionScope);

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();
        _trainarrAdminToken = CreateTrainArrAccessToken(["trainarr"], "trainarr_admin");
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_events_batch_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/training-events/process-batch",
            new ProcessTrainingDomainEventsRequest(null, DateTimeOffset.UtcNow, 25));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_events_batch_materializes_person_training_history()
    {
        var personId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        await SeedPendingDomainEventAsync(personId, assignmentId);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/training-events/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessTrainingDomainEventsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessTrainingDomainEventsResponse>())!;
        Assert.Equal(1, body.PendingFound);
        Assert.Equal(1, body.ProcessedCount);

        using var trainarrScope = _trainarrFactory.Services.CreateScope();
        var trainarrDb = trainarrScope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var domainEvent = await trainarrDb.TrainingDomainEvents.SingleAsync(x => x.RelatedEntityId == assignmentId);
        Assert.Equal(TrainingDomainEventStatuses.Processed, domainEvent.ProcessingStatus);

        var history = await trainarrDb.PersonTrainingHistoryEntries.SingleAsync(
            x => x.StaffarrPersonId == personId && x.SourceDomainEventId == domainEvent.Id);
        Assert.Equal(TrainingDomainEventKinds.AssignmentCreated, history.EventKind);
        Assert.Contains("Training assignment created", history.Summary);

        var historyRequest = Authorized(
            HttpMethod.Get,
            $"/api/person-training-history?staffarrPersonId={personId}",
            _trainarrAdminToken);
        var historyResponse = await _trainarrClient.SendAsync(historyRequest);
        historyResponse.EnsureSuccessStatusCode();
        var historyBody = (await historyResponse.Content.ReadFromJsonAsync<PersonTrainingHistoryResponse>())!;
        Assert.Equal(1, historyBody.TotalCount);
        Assert.Single(historyBody.Items);
    }

    [Fact]
    public async Task Process_events_batch_enqueues_notification_dispatch_from_domain_event()
    {
        var personId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        await SeedPendingDomainEventAsync(personId, assignmentId);

        using (var scope = _trainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
            var now = DateTimeOffset.UtcNow;
            db.TenantTrainingNotificationSettings.Add(new TenantTrainingNotificationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                IsEnabled = true,
                NotificationWebhookUrl = "https://hooks.example.test/trainarr-event-fanout",
                NotifyOnAssignmentCreated = true,
                CreatedAt = now,
                UpdatedAt = now,
            });
            await db.SaveChangesAsync();
        }

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/training-events/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessTrainingDomainEventsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        using var verifyScope = _trainarrFactory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var notification = await verifyDb.TrainingNotificationDispatches.SingleOrDefaultAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId
            && x.EventKind == TrainingNotificationEventKinds.AssignmentCreated
            && x.RelatedEntityId == assignmentId);
        Assert.NotNull(notification);
        Assert.Equal(TrainingNotificationDispatchStatuses.Pending, notification!.DispatchStatus);
    }

    private async Task SeedPendingDomainEventAsync(Guid personId, Guid assignmentId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        db.TenantEventProcessingSettings.Add(new TenantEventProcessingSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            MaxAttempts = 10,
            RetryIntervalMinutes = 5,
            CreatedAt = now,
            UpdatedAt = now,
        });

        var payload = new TrainingDomainEventPayload(
            personId,
            "training_assignment",
            assignmentId,
            "Training assignment created: Safety Basics (due test).",
            now,
            "qual.event_worker",
            "Event Worker Qualification",
            "Safety Basics",
            assignmentId);

        db.TrainingDomainEvents.Add(new TrainingDomainEvent
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            EventKind = TrainingDomainEventKinds.AssignmentCreated,
            IdempotencyKey = EventProcessingRules.BuildIdempotencyKey(
                TrainingDomainEventKinds.AssignmentCreated,
                "training_assignment",
                assignmentId),
            StaffarrPersonId = personId,
            RelatedEntityType = "training_assignment",
            RelatedEntityId = assignmentId,
            PayloadJson = System.Text.Json.JsonSerializer.Serialize(payload),
            ProcessingStatus = TrainingDomainEventStatuses.Pending,
            AttemptCount = 0,
            NextRetryAt = now.AddMinutes(-1),
            CreatedAt = now.AddMinutes(-5),
            UpdatedAt = now.AddMinutes(-5),
        });

        await db.SaveChangesAsync();
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArrTokenService>();
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
            $"{sourceProduct}-events-{Guid.NewGuid():N}",
            $"{sourceProduct} event processing test",
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
