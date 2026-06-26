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
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrTrainingNotificationTests : IAsyncLifetime
{
    private readonly List<HttpRequestMessage> _webhookRequests = [];
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _trainarrToStaffarrToken = null!;
    private string _sharedWorkerToTrainarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TrainArrNotificationNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"TrainArrNotificationStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"TrainArrNotificationTrainArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            TrainingNotificationDispatchService.ProcessNotificationsActionScope);

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
            var staffDb = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            await staffDb.Database.EnsureCreatedAsync();
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
                services.AddHttpClient(TrainingNotificationDispatchService.WebhookHttpClientName)
                    .ConfigurePrimaryHttpMessageHandler(() => new WebhookCaptureHandler(_webhookRequests));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        using var trainScope = _trainarrFactory.Services.CreateScope();
        var trainDb = trainScope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        await trainDb.Database.EnsureCreatedAsync();
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
    public async Task Assignment_create_enqueues_dispatch_and_worker_posts_webhook()
    {
        const string webhookUrl = "https://hooks.example.test/trainarr-assignments";
        await UpsertNotificationSettingsAsync(webhookUrl);

        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Notify", "Assign", "notify.assign@example.com");

        var assignment = await TrainArrQualificationCheckTestHelper.CreateManualAssignmentAsync(
            _trainarrClient,
            adminToken,
            personId,
            definitionId,
            "notification_test",
            DateTimeOffset.UtcNow.AddDays(7));

        using (var scope = _trainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
            var pending = await db.TrainingNotificationDispatches.SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.EventKind == TrainingNotificationEventKinds.AssignmentCreated
                && x.RelatedEntityId == assignment.AssignmentId);
            Assert.Equal(TrainingNotificationDispatchStatuses.Pending, pending.DispatchStatus);
        }

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/training-notifications/process-batch",
            _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessTrainingNotificationsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));
        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        Assert.Single(_webhookRequests);
        Assert.Equal(webhookUrl, _webhookRequests[0].RequestUri?.ToString());

        using var verifyScope = _trainarrFactory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var dispatched = await verifyDb.TrainingNotificationDispatches.SingleAsync(x =>
            x.RelatedEntityId == assignment.AssignmentId);
        Assert.Equal(TrainingNotificationDispatchStatuses.Sent, dispatched.DispatchStatus);
        Assert.Equal(200, dispatched.HttpStatusCode);
        Assert.Equal("hooks.example.test", dispatched.WebhookHost);
    }

    [Fact]
    public async Task Notification_settings_put_rejects_invalid_webhook()
    {
        var token = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertTrainingNotificationSettingsRequest(
            true,
            "not-a-url",
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

        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Notification_settings_list_dispatches_requires_admin()
    {
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/notification-settings", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Notification_settings_reject_platform_admin_without_trainarr_role()
    {
        var platformAdminToken = CreateTrainArrAccessToken(
            ["trainarr"],
            tenantRoleKey: "routarr_driver",
            isPlatformAdmin: true);

        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/notification-settings", platformAdminToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Settings_manifest_v1_requires_settings_reader_and_lists_canonical_group()
    {
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");
        var forbiddenResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var manifestResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", adminToken));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<TrainArrSettingsManifestResponse>())!;
        var item = Assert.Single(manifest.Items);
        Assert.Equal("trainarr_tenant_settings", item.SettingKey);
        Assert.Equal("/api/v1/tenant-settings/trainarr", item.EndpointPath);
    }

    [Fact]
    public async Task Config_manifest_v1_requires_admin_and_matches_settings_manifest()
    {
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");
        var forbiddenResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/config", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var configResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/config", adminToken));
        configResponse.EnsureSuccessStatusCode();
        var configManifest = (await configResponse.Content.ReadFromJsonAsync<TrainArrSettingsManifestResponse>())!;

        var settingsResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", adminToken));
        settingsResponse.EnsureSuccessStatusCode();
        var settingsManifest = (await settingsResponse.Content.ReadFromJsonAsync<TrainArrSettingsManifestResponse>())!;

        Assert.Equal(settingsManifest.Items.Count, configManifest.Items.Count);
        foreach (var item in settingsManifest.Items)
        {
            Assert.Contains(configManifest.Items, x => x.SettingKey == item.SettingKey);
        }
    }

    private async Task UpsertNotificationSettingsAsync(
        string webhookUrl,
        int maxAttempts = 10,
        int retryIntervalMinutes = 5)
    {
        var token = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertTrainingNotificationSettingsRequest(
            true,
            webhookUrl,
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
            maxAttempts,
            retryIntervalMinutes));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Failed_webhook_retries_then_succeeds_on_second_attempt()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var trainArrDbName = $"TrainArrNotificationRetry-{Guid.NewGuid():N}";
        var retryWebhookRequests = new List<HttpRequestMessage>();

        await using var retryFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
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
                services.AddHttpClient(TrainingNotificationDispatchService.WebhookHttpClientName)
                    .ConfigurePrimaryHttpMessageHandler(() => new FlakyWebhookCaptureHandler(retryWebhookRequests));
            });
        });

        using var retryClient = retryFactory.CreateClient();
        using (var scope = retryFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        const string webhookUrl = "https://hooks.example.test/trainarr-retry";
        var adminToken = CreateTrainArrAccessTokenForFactory(retryFactory, ["trainarr"], tenantRoleKey: "trainarr_admin");
        var settingsRequest = Authorized(HttpMethod.Put, "/api/notification-settings", adminToken);
        settingsRequest.Content = JsonContent.Create(new UpsertTrainingNotificationSettingsRequest(
            true,
            webhookUrl,
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
            3,
            0));
        (await retryClient.SendAsync(settingsRequest)).EnsureSuccessStatusCode();

        var definitionId = await CreateTrainingDefinitionOnClientAsync(retryClient, adminToken);
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Retry", "Notify", "retry.notify@example.com");

        var check = await TrainArrQualificationCheckTestHelper.RunQualificationCheckAsync(
            retryClient,
            adminToken,
            personId,
            "notification_test",
            definitionId);

        var createRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            DateTimeOffset.UtcNow.AddDays(7),
            check.CheckId));
        var createResponse = await retryClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var assignment = (await createResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;

        var failProcess = Authorized(
            HttpMethod.Post,
            "/api/internal/training-notifications/process-batch",
            _sharedWorkerToTrainarrToken);
        failProcess.Content = JsonContent.Create(new ProcessTrainingNotificationsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            10));
        (await retryClient.SendAsync(failProcess)).EnsureSuccessStatusCode();

        using (var scope = retryFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
            var row = await db.TrainingNotificationDispatches.SingleAsync(x =>
                x.RelatedEntityId == assignment.AssignmentId);
            Assert.Equal(TrainingNotificationDispatchStatuses.Pending, row.DispatchStatus);
            Assert.Equal(1, row.AttemptCount);
            Assert.NotNull(row.NextRetryAt);
        }

        var retryProcess = Authorized(
            HttpMethod.Post,
            "/api/internal/training-notifications/process-batch",
            _sharedWorkerToTrainarrToken);
        retryProcess.Content = JsonContent.Create(new ProcessTrainingNotificationsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow.AddMinutes(1),
            10));
        (await retryClient.SendAsync(retryProcess)).EnsureSuccessStatusCode();

        Assert.Equal(2, retryWebhookRequests.Count);

        using var verifyScope = retryFactory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var dispatched = await verifyDb.TrainingNotificationDispatches.SingleAsync(x =>
            x.RelatedEntityId == assignment.AssignmentId);
        Assert.Equal(TrainingNotificationDispatchStatuses.Sent, dispatched.DispatchStatus);
        Assert.Equal(2, dispatched.AttemptCount);
    }

    private async Task SeedStaffPersonAsync(Guid personId, string givenName, string familyName, string email)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
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
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private string CreateTrainArrAccessTokenForFactory(
        WebApplicationFactory<global::TrainArr.Api.Program> factory,
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = factory.Services.CreateScope();
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

    private static async Task<Guid> CreateTrainingDefinitionOnClientAsync(HttpClient client, string trainarrAdminToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"notify_{Guid.NewGuid():N}"[..20],
            "Notification Test Training",
            "Training definition for notification dispatch tests.",
            "notification_test",
            "Notification Test"));
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var definition = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return definition.TrainingDefinitionId;
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string trainarrAdminToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"notify_{Guid.NewGuid():N}"[..20],
            "Notification Test Training",
            "Training definition for notification dispatch tests.",
            "notification_test",
            "Notification Test"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var definition = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return definition.TrainingDefinitionId;
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null,
        bool isPlatformAdmin = false)
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
            isPlatformAdmin);

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
            $"{sourceProduct}-training-notification-{Guid.NewGuid():N}",
            $"{sourceProduct} training notification test",
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

    private sealed class WebhookCaptureHandler(List<HttpRequestMessage> captured) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            captured.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class FlakyWebhookCaptureHandler(List<HttpRequestMessage> captured) : HttpMessageHandler
    {
        private int _callCount;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            captured.Add(request);
            _callCount++;
            if (_callCount == 1)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
