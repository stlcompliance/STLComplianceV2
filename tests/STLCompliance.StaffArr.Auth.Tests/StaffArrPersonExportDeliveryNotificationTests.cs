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

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrPersonExportDeliveryNotificationTests : IAsyncLifetime
{
    private readonly List<HttpRequestMessage> _webhookRequests = [];
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _sharedWorkerToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PersonExportNotificationNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"PersonExportNotificationStaffArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["staffarr"],
            PersonExportDeliveryService.ProcessDeliveriesActionScope);

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

                services.AddHttpClient(PersonExportDeliveryNotificationService.WebhookHttpClientName)
                    .ConfigurePrimaryHttpMessageHandler(() => new WebhookCaptureHandler(_webhookRequests));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Scheduled_delivery_posts_success_webhook_and_records_notification()
    {
        await SeedPersonAsync(Guid.NewGuid(), "Notify", "Success", "notify.success@example.com");
        const string webhookUrl = "https://hooks.example.test/staffarr-export";
        await SeedScheduleWithWebhookAsync(webhookUrl);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/person-export-deliveries/process-batch",
            _sharedWorkerToStaffarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPersonExportDeliveriesRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));

        var processResponse = await _staffarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        Assert.Single(_webhookRequests);
        Assert.Equal(HttpMethod.Post, _webhookRequests[0].Method);
        Assert.Equal(webhookUrl, _webhookRequests[0].RequestUri?.ToString());

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var notification = await db.PersonExportDeliveryNotifications.SingleAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId
            && x.EventKind == PersonExportDeliveryNotificationEventKinds.Success);
        Assert.Equal(PersonExportDeliveryNotificationStatuses.Sent, notification.DeliveryStatus);
        Assert.Equal(200, notification.HttpStatusCode);
        Assert.Equal("hooks.example.test", notification.WebhookHost);
    }

    [Fact]
    public async Task Export_schedule_put_rejects_non_https_webhook_in_testing_when_invalid()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var upsertRequest = Authorized(HttpMethod.Put, "/api/people/export/schedule", token);
        upsertRequest.Content = JsonContent.Create(new UpsertPersonExportScheduleRequest(
            true,
            24,
            NotificationWebhookUrl: "not-a-url",
            NotifyOnSuccess: true,
            NotifyOnFailure: true));

        var response = await _staffarrClient.SendAsync(upsertRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delivery_notifications_list_returns_recent_rows()
    {
        await SeedPersonAsync(Guid.NewGuid(), "Notify", "List", "notify.list@example.com");
        await SeedScheduleWithWebhookAsync("https://hooks.example.test/list");
        await ProcessBatchAsync();

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var listResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/people/export/delivery-notifications?limit=5", token));
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<PersonExportDeliveryNotificationsResponse>())!;
        Assert.NotEmpty(listed.Items);
        Assert.Contains(listed.Items, item => item.EventKind == PersonExportDeliveryNotificationEventKinds.Success);
    }

    private async Task ProcessBatchAsync()
    {
        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/person-export-deliveries/process-batch",
            _sharedWorkerToStaffarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPersonExportDeliveriesRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));
        var processResponse = await _staffarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
    }

    private async Task SeedScheduleWithWebhookAsync(string webhookUrl)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantPersonExportSchedules.Add(new TenantPersonExportSchedule
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            IntervalHours = 24,
            NotificationWebhookUrl = webhookUrl,
            NotifyOnSuccess = true,
            NotifyOnFailure = true,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedPersonAsync(Guid personId, string givenName, string familyName, string email)
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
            $"{sourceProduct}-export-notification-{Guid.NewGuid():N}",
            $"{sourceProduct} export notification test",
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

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
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

    private sealed class WebhookCaptureHandler(List<HttpRequestMessage> captured) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            captured.Add(request);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"accepted\":true}", System.Text.Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }
}
