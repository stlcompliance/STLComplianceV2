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
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrCompanionScanResolveTests : IAsyncLifetime
{
    private static readonly Guid AssignmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var dbName = $"NexArrCompanionScan-{Guid.NewGuid():N}";

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
    public async Task Resolve_scan_returns_matching_inbox_task()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/companion/scan/resolve",
            token,
            new CompanionScanResolveRequest($"trainarr:assignment:{AssignmentId:D}", "qr")));

        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<CompanionScanResolveResponse>())!;

        Assert.Equal(CompanionScanOutcomes.Resolved, payload.Outcome);
        Assert.Equal($"trainarr:assignment:{AssignmentId:D}", payload.TaskKey);
        Assert.Equal("trainarr", payload.ProductKey);
        Assert.Contains("/assignments/", payload.DeepLinkUrl, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Resolve_scan_denies_task_not_in_inbox()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var missingId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/companion/scan/resolve",
            token,
            new CompanionScanResolveRequest($"trainarr:assignment:{missingId:D}", null)));

        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<CompanionScanResolveResponse>())!;

        Assert.Equal(CompanionScanOutcomes.Denied, payload.Outcome);
        Assert.Equal(CompanionScanReasonCodes.NotInInbox, payload.ReasonCode);
    }

    [Fact]
    public async Task Resolve_scan_requires_authentication()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/companion/scan/resolve",
            new CompanionScanResolveRequest($"trainarr:assignment:{AssignmentId:D}", null));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Resolve_scan_rejects_invalid_payload()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/companion/scan/resolve",
            token,
            new CompanionScanResolveRequest("not-a-valid-scan", null)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private static async Task EnsureCompanionEntitlementAsync(NexArrDbContext db)
    {
        if (await db.Entitlements.AnyAsync(e => e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "companion"))
        {
            return;
        }

        db.ProductCatalog.Add(new ProductCatalogItem
        {
            ProductKey = "companion",
            DisplayName = "Companion App",
            SortOrder = 80,
            IsActive = true,
        });
        db.Entitlements.Add(new TenantProductEntitlement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ProductKey = "companion",
            Status = EntitlementStatuses.Active,
            GrantedAt = DateTimeOffset.UtcNow,
        });
        db.LaunchProfiles.Add(new ProductLaunchProfile
        {
            ProductKey = "companion",
            BaseUrl = "http://localhost:5181",
            LaunchPath = "/launch",
            IsActive = true,
            ModifiedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
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
                            "Scan target assignment",
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
