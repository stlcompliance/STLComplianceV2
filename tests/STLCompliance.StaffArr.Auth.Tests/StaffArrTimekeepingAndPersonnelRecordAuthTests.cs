using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrTimekeepingAndPersonnelRecordAuthTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrTimekeepingRecordsAuth-{Guid.NewGuid():N}";

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
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
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Platform_admin_without_staffarr_role_cannot_access_timekeeping_or_personnel_records()
    {
        var targetPersonId = Guid.NewGuid();
        await SeedPersonAsync(targetPersonId, "Protected", "Worker", "protected.worker@example.com");

        var platformAdminToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: Guid.NewGuid(),
            isPlatformAdmin: true);

        var workSessionRequest = Authorized(HttpMethod.Post, "/api/v1/timekeeping/work-sessions", platformAdminToken);
        workSessionRequest.Content = JsonContent.Create(new UpsertWorkSessionRequest(
            targetPersonId,
            new DateOnly(2026, 6, 1),
            new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 1, 16, 0, 0, TimeSpan.Zero),
            "UTC",
            "draft",
            "manual",
            "staffarr",
            null,
            null,
            null,
            null,
            30,
            false));
        var timekeepingManage = await _staffarrClient.SendAsync(workSessionRequest);
        Assert.Equal(HttpStatusCode.Forbidden, timekeepingManage.StatusCode);

        var payPolicyRequest = Authorized(HttpMethod.Post, "/api/v1/timekeeping/pay-policies", platformAdminToken);
        payPolicyRequest.Content = JsonContent.Create(new UpsertPayPolicyRequest(
            "Default hourly",
            "Default policy",
            "US-MO",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new DateOnly(2026, 1, 1),
            null,
            "active"));
        var timekeepingAdmin = await _staffarrClient.SendAsync(payPolicyRequest);
        Assert.Equal(HttpStatusCode.Forbidden, timekeepingAdmin.StatusCode);

        var noteList = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{targetPersonId:D}/notes", platformAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, noteList.StatusCode);

        var noteCreate = Authorized(HttpMethod.Post, $"/api/people/{targetPersonId:D}/notes", platformAdminToken);
        noteCreate.Content = JsonContent.Create(new CreatePersonnelNoteRequest(
            "performance",
            "management",
            "Protected note",
            "Platform admin should not bypass note write auth."));
        var noteCreateResponse = await _staffarrClient.SendAsync(noteCreate);
        Assert.Equal(HttpStatusCode.Forbidden, noteCreateResponse.StatusCode);

        var documentList = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{targetPersonId:D}/documents", platformAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, documentList.StatusCode);

        var documentCreate = Authorized(HttpMethod.Post, $"/api/people/{targetPersonId:D}/documents", platformAdminToken);
        documentCreate.Content = JsonContent.Create(new CreatePersonnelDocumentRequest(
            "employment_record",
            "hr_only",
            "personnel_file",
            true,
            "Protected document",
            "protected.txt",
            "text/plain",
            Convert.ToBase64String("protected"u8.ToArray()),
            "Platform admin should not bypass document write auth.",
            null));
        var documentCreateResponse = await _staffarrClient.SendAsync(documentCreate);
        Assert.Equal(HttpStatusCode.Forbidden, documentCreateResponse.StatusCode);
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
