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
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrAssignmentReminderEscalationWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _trainarrToStaffarrToken = null!;
    private string _dueReminderToken = null!;
    private string _escalationToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TrainArrReminderEscNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"TrainArrReminderEscStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"TrainArrReminderEscTrainArr-{Guid.NewGuid():N}";

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
        _trainarrToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            $"{StaffArrIntegration.TrainingBlockerIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementReadActionScope}");
        _dueReminderToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            AssignmentDueReminderWorkerService.ProcessDueRemindersActionScope);
        _escalationToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            AssignmentEscalationWorkerService.ProcessEscalationsActionScope);

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<StaffArrDbContext>().Database.EnsureCreatedAsync();
        }

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("StaffArr:BaseUrl", _staffarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", _trainarrToStaffarrToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));
                services.AddHttpClient<StaffArrTrainingBlockerClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<TrainArr.Api.Services.StaffArrTrainingAcknowledgementClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();
        using (var scope = _trainarrFactory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<TrainArrDbContext>().Database.EnsureCreatedAsync();
        }
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
    public async Task Due_reminder_process_batch_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/assignment-due-reminders/process-batch",
            new ProcessAssignmentDueRemindersRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 25));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_due_reminder_batch_sends_reminder_for_assignment_due_soon()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        await UpsertNotificationSettingsAsync(adminToken);
        await UpsertDueReminderSettingsAsync(adminToken);

        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);

        var dueAt = DateTimeOffset.UtcNow.AddDays(3);
        var createRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            dueAt));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var assignment = (await createResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/assignment-due-reminders/process-batch",
            _dueReminderToken);
        processRequest.Content = JsonContent.Create(new ProcessAssignmentDueRemindersRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25));
        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessAssignmentDueRemindersResponse>())!;
        Assert.Equal(1, body.CandidatesFound);
        Assert.Equal(1, body.RemindersSentCount);

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var updated = await db.TrainingAssignments.SingleAsync(x => x.Id == assignment.AssignmentId);
        Assert.Equal(1, updated.DueReminderCount);
        Assert.NotNull(updated.LastDueReminderSentAt);

        var dispatch = await db.TrainingNotificationDispatches.SingleAsync(x =>
            x.EventKind == TrainingNotificationEventKinds.AssignmentDueReminder
            && x.RelatedEntityId == assignment.AssignmentId);
        Assert.Equal(TrainingNotificationDispatchStatuses.Pending, dispatch.DispatchStatus);
    }

    [Fact]
    public async Task Process_escalation_batch_escalates_overdue_assignment()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        await UpsertNotificationSettingsAsync(adminToken);
        await UpsertEscalationSettingsAsync(adminToken);

        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId);

        var createRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            DateTimeOffset.UtcNow.AddDays(7)));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var assignment = (await createResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;

        using (var scope = _trainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
            var entity = await db.TrainingAssignments.SingleAsync(x => x.Id == assignment.AssignmentId);
            entity.DueAt = DateTimeOffset.UtcNow.AddHours(-48);
            await db.SaveChangesAsync();
        }

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/assignment-escalations/process-batch",
            _escalationToken);
        processRequest.Content = JsonContent.Create(new ProcessAssignmentEscalationsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25));
        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessAssignmentEscalationsResponse>())!;
        Assert.Equal(1, body.CandidatesFound);
        Assert.Equal(1, body.EscalatedCount);

        using var verifyScope = _trainarrFactory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var updated = await verifyDb.TrainingAssignments.SingleAsync(x => x.Id == assignment.AssignmentId);
        Assert.Equal(1, updated.EscalationCount);
        Assert.Single(await verifyDb.AssignmentEscalationEvents
            .Where(x => x.TrainingAssignmentId == assignment.AssignmentId)
            .ToListAsync());
    }

    private async Task UpsertNotificationSettingsAsync(string adminToken)
    {
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", adminToken);
        request.Content = JsonContent.Create(new UpsertTrainingNotificationSettingsRequest(
            true,
            "https://hooks.example.test/trainarr-reminders",
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            30,
            10,
            5));
        (await _trainarrClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task UpsertDueReminderSettingsAsync(string adminToken)
    {
        var request = Authorized(HttpMethod.Put, "/api/assignment-due-reminder-settings", adminToken);
        request.Content = JsonContent.Create(new UpsertAssignmentDueReminderSettingsRequest(
            true,
            7,
            24,
            5));
        (await _trainarrClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task UpsertEscalationSettingsAsync(string adminToken)
    {
        var request = Authorized(HttpMethod.Put, "/api/assignment-escalation-settings", adminToken);
        request.Content = JsonContent.Create(new UpsertAssignmentEscalationSettingsRequest(
            true,
            24,
            48,
            10));
        (await _trainarrClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string trainarrAdminToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"remind_{Guid.NewGuid():N}"[..20],
            "Reminder Test Training",
            "Training definition for reminder tests.",
            "reminder_test",
            "Reminder Test"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var definition = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return definition.TrainingDefinitionId;
    }

    private async Task SeedStaffPersonAsync(Guid personId)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Due",
            FamilyName = "Reminder",
            DisplayName = "Due Reminder",
            PrimaryEmail = $"due.reminder.{Guid.NewGuid():N}@example.com",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArrTokenService>();
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
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
            $"{sourceProduct}-reminder-esc-{Guid.NewGuid():N}",
            $"{sourceProduct} reminder escalation test",
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
