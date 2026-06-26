using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrAuditPackageTests : IAsyncLifetime
{
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _trainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"TrainArrAuditPackage-{Guid.NewGuid():N}";

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Audit_package_manifest_lists_sections()
    {
        var trainerToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_trainer");
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/manifest", trainerToken));
        response.EnsureSuccessStatusCode();
        var manifest = (await response.Content.ReadFromJsonAsync<AuditPackageManifestResponse>())!;
        Assert.Equal("1", manifest.PackageVersion);
        Assert.Equal(12, manifest.Sections.Count);
        Assert.Contains(manifest.Sections, section => section.Key == "training_assignments");
        Assert.Contains(manifest.Sections, section => section.FileName == "person_training_history.json");
    }

    [Fact]
    public async Task Audit_package_export_zip_contains_json_files()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        await SeedTrainingDataAsync();

        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export", adminToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", response.Content.Headers.ContentType?.MediaType);

        var zipBytes = await response.Content.ReadAsByteArrayAsync();
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        Assert.Equal(13, archive.Entries.Count);
        Assert.Contains(archive.Entries, entry => entry.Name == "manifest.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "training_definitions.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "training_assignments.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "qualification_issues.json");
    }

    [Fact]
    public async Task Audit_package_export_json_returns_structured_package()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        await SeedTrainingDataAsync();

        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=json", adminToken));
        response.EnsureSuccessStatusCode();
        var package = (await response.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.NotEqual(Guid.Empty, package.PackageId);
        Assert.Equal(PlatformSeeder.DemoTenantId, package.TenantId);
        Assert.Equal(1, package.Counts.TrainingDefinitions);
        Assert.Equal(1, package.Counts.TrainingAssignments);
        Assert.Equal("hazmat_awareness", package.TrainingDefinitions[0].DefinitionKey);
    }

    [Fact]
    public async Task Audit_package_export_writes_audit_event()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var beforeCount = await CountAuditPackageExportEventsAsync();

        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=json", adminToken));
        response.EnsureSuccessStatusCode();

        var afterCount = await CountAuditPackageExportEventsAsync();
        Assert.Equal(beforeCount + 1, afterCount);
    }

    [Fact]
    public async Task Audit_package_export_denies_trainer()
    {
        var trainerToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_trainer");
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export", trainerToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Audit_package_export_denies_platform_admin_without_trainarr_role()
    {
        var platformAdminToken = CreateTrainArrAccessToken(
            ["trainarr"],
            tenantRoleKey: "tenant_member",
            isPlatformAdmin: true);
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export", platformAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Audit_package_export_rejects_invalid_date_range()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var from = DateTimeOffset.UtcNow;
        var to = from.AddDays(-1);
        var response = await _trainarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit-packages/export?format=json&from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}",
                adminToken));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Audit_package_export_date_filter_limits_assignments()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        await SeedTrainingDataWithDatesAsync();

        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 23, 59, 59, TimeSpan.Zero);
        var response = await _trainarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit-packages/export?format=json&from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}",
                adminToken));
        response.EnsureSuccessStatusCode();
        var package = (await response.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.Equal(1, package.Counts.TrainingAssignments);
        Assert.Equal("assigned", package.TrainingAssignments[0].Status);
    }

    [Fact]
    public async Task Audit_package_v1_aliases_manifest_and_export_work()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        await SeedTrainingDataAsync();

        var manifestResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages/manifest", adminToken));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<AuditPackageManifestResponse>())!;
        Assert.Equal("1", manifest.PackageVersion);

        var exportResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages/export?format=json", adminToken));
        exportResponse.EnsureSuccessStatusCode();
        var package = (await exportResponse.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.Equal(PlatformSeeder.DemoTenantId, package.TenantId);
        Assert.True(package.Counts.TrainingDefinitions >= 1);
    }

    [Fact]
    public async Task Audit_package_v1_index_lists_manifest_export_and_jobs_paths()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages", adminToken));
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = json.RootElement
            .GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("path").GetString())
            .ToList();

        Assert.Contains("/api/v1/audit-packages/manifest", paths);
        Assert.Contains("/api/v1/audit-packages/export", paths);
        Assert.Contains("/api/v1/audit-packages/jobs", paths);
    }

    [Fact]
    public async Task Audit_v1_alias_matches_primary_audit_timeline()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");

        var exportResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=json", adminToken));
        exportResponse.EnsureSuccessStatusCode();

        var primaryResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit?page=1&pageSize=10", adminToken));
        primaryResponse.EnsureSuccessStatusCode();
        var primaryJson = await primaryResponse.Content.ReadFromJsonAsync<JsonElement>();

        var v1Response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit?page=1&pageSize=10", adminToken));
        v1Response.EnsureSuccessStatusCode();
        var v1Json = await v1Response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            primaryJson.GetProperty("totalCount").GetInt32(),
            v1Json.GetProperty("totalCount").GetInt32());

        var primaryItems = primaryJson.GetProperty("items");
        var v1Items = v1Json.GetProperty("items");
        Assert.Equal(primaryItems.GetArrayLength(), v1Items.GetArrayLength());
        Assert.True(primaryItems.GetArrayLength() >= 1);
        var primaryFirst = primaryItems.EnumerateArray().First();
        var v1First = v1Items.EnumerateArray().First();
        Assert.Equal(
            primaryFirst.GetProperty("action").GetString(),
            v1First.GetProperty("action").GetString());
    }

    private async Task SeedTrainingDataAsync()
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var personId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        db.TrainingDefinitions.Add(new TrainingDefinition
        {
            Id = definitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = "hazmat_awareness",
            Name = "Hazmat Awareness",
            Description = "Seeded for audit package export test.",
            QualificationKey = "hazmat_endorsement",
            QualificationName = "Hazmat Endorsement",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = assignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            TrainingDefinitionId = definitionId,
            AssignmentReason = "manual",
            Status = "assigned",
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private async Task SeedTrainingDataWithDatesAsync()
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var personId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();

        db.TrainingDefinitions.Add(new TrainingDefinition
        {
            Id = definitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = "dated_definition",
            Name = "Dated Definition",
            Description = "Date filter test.",
            QualificationKey = "dated_qual",
            QualificationName = "Dated Qualification",
            Status = "active",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        });

        db.TrainingAssignments.AddRange(
            new TrainingAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                StaffarrPersonId = personId,
                TrainingDefinitionId = definitionId,
                AssignmentReason = "manual",
                Status = "assigned",
                CreatedAt = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero),
            },
            new TrainingAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                StaffarrPersonId = personId,
                TrainingDefinitionId = definitionId,
                AssignmentReason = "manual",
                Status = "completed",
                CreatedAt = new DateTimeOffset(2026, 2, 1, 12, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2026, 2, 1, 12, 0, 0, TimeSpan.Zero),
            });

        await db.SaveChangesAsync();
    }

    private async Task<int> CountAuditPackageExportEventsAsync()
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        return await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "audit_package.export");
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
