using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrFieldCompanionFieldInboxTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _nexarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"NexArrfieldcompanion-{Guid.NewGuid():N}";

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
        await EnsureFieldCompanionEntitlementAsync();
    }

    public async Task DisposeAsync()
    {
        _nexarrClient.Dispose();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task fieldcompanion_handoff_redeem_issues_session_for_entitled_user()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var handoffResponse = await _nexarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/v1/launch/handoff",
            adminToken,
            new CreateHandoffRequest("fieldcompanion", null)));
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var redeemResponse = await _nexarrClient.PostAsJsonAsync(
            "/api/fieldcompanion/auth/handoff/redeem",
            new FieldCompanionRedeemHandoffRequest(handoff.HandoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<FieldCompanionSessionResponse>())!;

        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Contains("fieldcompanion", session.Entitlements, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task fieldcompanion_field_inbox_returns_product_slices_for_entitled_user()
    {
        var session = await RedeemFieldCompanionSessionAsync();

        var inboxResponse = await _nexarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/fieldcompanion/field-inbox", session.AccessToken));
        inboxResponse.EnsureSuccessStatusCode();
        var inbox = (await inboxResponse.Content.ReadFromJsonAsync<AggregatedFieldInboxResponse>())!;

        Assert.True(inbox.Sources.Count >= 6);
        Assert.Contains(inbox.Sources, source => source.ProductKey == "maintainarr" && source.Entitled);
        Assert.Contains(inbox.Sources, source => source.ProductKey == "loadarr");
        Assert.Contains(
            inbox.Sources.Where(source => source.Entitled),
            source => !source.Fetched && source.ErrorCode is "upstream_unreachable" or "upstream_401");
    }

    [Fact]
    public async Task fieldcompanion_field_inbox_requires_authentication()
    {
        var response = await _nexarrClient.GetAsync("/api/fieldcompanion/field-inbox");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<FieldCompanionSessionResponse> RedeemFieldCompanionSessionAsync()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var handoffResponse = await _nexarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/v1/launch/handoff",
            adminToken,
            new CreateHandoffRequest("fieldcompanion", null)));
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var redeemResponse = await _nexarrClient.PostAsJsonAsync(
            "/api/fieldcompanion/auth/handoff/redeem",
            new FieldCompanionRedeemHandoffRequest(handoff.HandoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        return (await redeemResponse.Content.ReadFromJsonAsync<FieldCompanionSessionResponse>())!;
    }

    private async Task EnsureFieldCompanionEntitlementAsync()
    {
        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var exists = await db.Entitlements.AnyAsync(
            e => e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "fieldcompanion");
        if (exists)
        {
            return;
        }

        db.ProductCatalog.Add(new ProductCatalogItem
        {
            ProductKey = "fieldcompanion",
            DisplayName = "fieldcompanion App",
            SortOrder = 80,
            IsActive = true
        });
        db.Entitlements.Add(new TenantProductEntitlement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ProductKey = "fieldcompanion",
            Status = EntitlementStatuses.Active,
            GrantedAt = DateTimeOffset.UtcNow
        });
        db.LaunchProfiles.Add(new ProductLaunchProfile
        {
            ProductKey = "fieldcompanion",
            BaseUrl = "http://localhost:5181",
            LaunchPath = "/launch",
            IsActive = true,
            ModifiedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private async Task SeedNexArrAsync()
    {
        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return auth.AccessToken;
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
