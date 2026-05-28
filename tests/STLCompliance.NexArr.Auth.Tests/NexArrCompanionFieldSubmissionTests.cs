using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrCompanionFieldSubmissionTests : IAsyncLifetime
{
    private static readonly Guid AssignmentId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var dbName = $"NexArrCompanionSubmission-{Guid.NewGuid():N}";
        _factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("TrainArr__BaseUrl", "http://trainarr.test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddHttpClient(nameof(CompanionProductClient))
                    .ConfigurePrimaryHttpMessageHandler(() => new FieldInboxStubHandler());
            });
        });

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
        await EnsureCompanionEntitlementAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Offline_sync_records_acknowledge_submission_status()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var taskKey = $"trainarr:assignment:{AssignmentId:D}";

        var syncRequest = Authorized(HttpMethod.Post, "/api/companion/offline-actions/sync", token);
        syncRequest.Content = JsonContent.Create(new SyncCompanionOfflineActionsRequest(
        [
            new CompanionOfflineActionItem(
                Guid.NewGuid().ToString("N"),
                CompanionOfflineActionKinds.FieldInboxAcknowledge,
                taskKey,
                "trainarr",
                DateTimeOffset.UtcNow),
        ]));

        var syncResponse = await _client.SendAsync(syncRequest);
        syncResponse.EnsureSuccessStatusCode();

        var statusResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/companion/field-tasks/submission-status?taskKeys={Uri.EscapeDataString(taskKey)}",
                token));
        statusResponse.EnsureSuccessStatusCode();

        var status = (await statusResponse.Content.ReadFromJsonAsync<FieldTaskSubmissionStatusResponse>())!;
        var acknowledge = Assert.Single(status.Items);
        Assert.Equal(taskKey, acknowledge.TaskKey);
        Assert.Equal(CompanionFieldSubmissionKinds.Acknowledge, acknowledge.SubmissionKind);
        Assert.Equal(CompanionFieldSubmissionStatuses.Synced, acknowledge.Status);
    }

    [Fact]
    public async Task Submission_status_requires_task_keys()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/companion/field-tasks/submission-status", token));
        response.EnsureSuccessStatusCode();

        var status = (await response.Content.ReadFromJsonAsync<FieldTaskSubmissionStatusResponse>())!;
        Assert.Empty(status.Items);
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return body.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(descriptor => descriptor.ServiceType == typeof(DbContextOptions<TContext>)
                || descriptor.ServiceType == typeof(TContext))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static async Task EnsureCompanionEntitlementAsync(NexArrDbContext db)
    {
        if (await db.Entitlements.AnyAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId && x.ProductKey == "companion"))
        {
            return;
        }

        db.Entitlements.Add(new TenantProductEntitlement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ProductKey = "companion",
            Status = EntitlementStatuses.Active,
            GrantedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private sealed class FieldInboxStubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/api/field-inbox", StringComparison.OrdinalIgnoreCase) == true
                && request.RequestUri.Host.Contains("trainarr", StringComparison.OrdinalIgnoreCase))
            {
                var inbox = new FieldInboxResponse(
                    new FieldInboxSummary(1, 0, new Dictionary<string, int> { ["trainarr"] = 1 }),
                    [
                        new FieldInboxTaskItem(
                            $"trainarr:assignment:{AssignmentId:D}",
                            "trainarr",
                            "training_assignment",
                            "Submission status assignment",
                            null,
                            "assigned",
                            null,
                            null,
                            DateTimeOffset.UtcNow,
                            $"/assignments/{AssignmentId:D}",
                            null,
                            $"http://trainarr.test/assignments/{AssignmentId:D}"),
                    ]);

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(inbox),
                });
            }

            var empty = new FieldInboxResponse(
                FieldInboxRules.BuildProductResponse([]).Summary,
                []);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(empty),
            });
        }
    }
}
