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

namespace STLCompliance.E2E.Support;

/// <summary>
/// In-memory NexArr host seeded with demo tenant credentials for cross-product E2E flows.
/// </summary>
internal sealed class E2ENexArrHost : IAsyncDisposable
{
    public const string SigningKey = "e2e-signing-key-at-least-32-characters-long";

    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly string _databaseName;

    public E2ENexArrHost()
    {
        _databaseName = $"E2E-NexArr-{Guid.NewGuid():N}";
        _factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        });

        Client = _factory.CreateClient();
    }

    public HttpClient Client { get; }

    public WebApplicationFactory<global::NexArr.Api.Program> Factory => _factory;

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
    }

    public async Task EnsureTenantAsync(
        Guid tenantId,
        string slug,
        string displayName,
        IReadOnlyList<string> productKeys,
        CancellationToken cancellationToken = default)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        if (await db.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Slug = slug,
            DisplayName = displayName,
            Status = TenantStatuses.Active,
            CreatedAt = now,
            ModifiedAt = now,
        });

        foreach (var productKey in productKeys)
        {
            db.Entitlements.Add(new TenantProductEntitlement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductKey = productKey,
                Status = EntitlementStatuses.Active,
                GrantedAt = now,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> LoginAsync(string email = PlatformSeeder.DemoAdminEmail)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    public async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string productKey,
        string actionScope,
        IReadOnlyList<string>? targetProducts = null,
        Guid? tenantId = null)
    {
        var effectiveTenantId = tenantId ?? PlatformSeeder.DemoTenantId;
        var registerRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-e2e-{Guid.NewGuid():N}",
            $"{productKey} E2E client",
            productKey,
            targetProducts ?? [productKey]));
        var registerResponse = await Client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            effectiveTenantId,
            targetProducts,
            actionScope,
            30));
        var issueResponse = await Client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    public async Task<string> CreateHandoffAsync(string productKey, string callbackUrl)
    {
        var token = await LoginAsync();
        var request = HttpTestClient.Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest(productKey, callbackUrl));
        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
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
