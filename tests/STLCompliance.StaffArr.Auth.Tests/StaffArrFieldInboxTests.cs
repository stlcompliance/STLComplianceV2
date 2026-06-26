using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrFieldInboxTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrFieldInbox-{Guid.NewGuid():N}";

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
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();
        await SeedInboxDataAsync(db);
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Field_inbox_allows_member_self_scope_without_explicit_person_filter()
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: InboxPersonId);

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));
        response.EnsureSuccessStatusCode();
        var inbox = (await response.Content.ReadFromJsonAsync<FieldInboxResponse>())!;

        Assert.Single(inbox.Items);
        Assert.All(inbox.Items, item => Assert.Equal($"staffarr:incident:{InboxIncidentId:D}", item.TaskKey));
    }

    [Fact]
    public async Task Field_inbox_platform_admin_member_is_still_self_scoped()
    {
        var token = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: InboxPersonId,
            isPlatformAdmin: true);

        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));
        response.EnsureSuccessStatusCode();
        var inbox = (await response.Content.ReadFromJsonAsync<FieldInboxResponse>())!;

        Assert.Single(inbox.Items);
        Assert.All(inbox.Items, item => Assert.Equal($"staffarr:incident:{InboxIncidentId:D}", item.TaskKey));
    }

    [Fact]
    public async Task Field_inbox_requires_authentication()
    {
        var response = await _staffarrClient.GetAsync("/api/field-inbox");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static readonly Guid InboxPersonId = Guid.Parse("00000000-0000-0000-0000-0000000000aa");
    private static readonly Guid OtherInboxPersonId = Guid.Parse("00000000-0000-0000-0000-0000000000ab");
    private static readonly Guid InboxIncidentId = Guid.Parse("00000000-0000-0000-0000-000000000201");
    private static readonly Guid OtherInboxIncidentId = Guid.Parse("00000000-0000-0000-0000-000000000202");

    private static async Task SeedInboxDataAsync(StaffArrDbContext db)
    {
        var now = DateTimeOffset.UtcNow;
        db.PersonnelIncidents.Add(new PersonnelIncident
        {
            Id = InboxIncidentId,
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = InboxPersonId,
            ReasonCategoryKey = "safety",
            Severity = "medium",
            Status = "open",
            Title = "Inbox self incident",
            Description = "Self-scoped incident for StaffArr field inbox tests.",
            OccurredAt = now.AddHours(-3),
            ReportedAt = now.AddHours(-2),
            ReportedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now.AddHours(-2),
            UpdatedAt = now.AddHours(-2),
        });

        db.PersonnelIncidents.Add(new PersonnelIncident
        {
            Id = OtherInboxIncidentId,
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = OtherInboxPersonId,
            ReasonCategoryKey = "conduct",
            Severity = "high",
            Status = "open",
            Title = "Inbox other incident",
            Description = "Other person's incident that must stay hidden from self-scoped members.",
            OccurredAt = now.AddHours(-5),
            ReportedAt = now.AddHours(-4),
            ReportedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now.AddHours(-4),
            UpdatedAt = now.AddHours(-4),
        });

        await db.SaveChangesAsync();
    }

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null,
        bool isPlatformAdmin = false)
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
            isPlatformAdmin);

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
}
