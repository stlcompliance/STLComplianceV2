using System.IO.Compression;
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
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrAuditPackageTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _managerToken = null!;
    private string _workerToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrAuditPackageNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrAuditPackageRoutArr-{Guid.NewGuid():N}";

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
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["routarr"],
            AuditPackageGenerationService.ProcessJobsActionScope);

        var handoffToken = await IssueServiceTokenAsync(adminToken, "routarr", ["routarr"]);

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", handoffToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _routarrClient = _routarrFactory.CreateClient();
        using (var scope = _routarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        _managerToken = CreateRoutArrAccessToken(["routarr"], "routarr_manager");
        await SeedAuditEventsAsync();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Manager_can_export_zip_with_csv_entry()
    {
        var exportResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=zip", _managerToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", exportResponse.Content.Headers.ContentType?.MediaType);

        await using var zipStream = await exportResponse.Content.ReadAsStreamAsync();
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        Assert.NotNull(archive.GetEntry("audit_events.json"));
        Assert.NotNull(archive.GetEntry("audit_events.csv"));
        Assert.NotNull(archive.GetEntry("proof_records.json"));
        Assert.NotNull(archive.GetEntry("proof_records.csv"));
        Assert.NotNull(archive.GetEntry("dvir_inspections.json"));
        Assert.NotNull(archive.GetEntry("dvir_inspections.csv"));
        Assert.NotNull(archive.GetEntry("capture_attachments.json"));
        Assert.NotNull(archive.GetEntry("capture_attachments.csv"));
    }

    [Fact]
    public async Task Manager_can_use_v1_audit_package_manifest_and_json_export()
    {
        var manifestResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages/manifest", _managerToken));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<AuditPackageManifestResponse>())!;
        Assert.Contains(manifest.Sections, section => section.Key == "audit_events");
        Assert.Contains(manifest.Sections, section => section.Key == "proof_records");

        var exportResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit-packages/export?format=json", _managerToken));
        exportResponse.EnsureSuccessStatusCode();
        var package = (await exportResponse.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.Equal(PlatformSeeder.DemoTenantId, package.TenantId);
        Assert.True(package.Counts.AuditEvents >= 1);
        Assert.True(package.Counts.ProofRecords >= 1);
    }

    [Fact]
    public async Task Filter_options_summary_csv_and_timeline()
    {
        var filterResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/filter-options", _managerToken));
        filterResponse.EnsureSuccessStatusCode();
        var filterOptions =
            (await filterResponse.Content.ReadFromJsonAsync<AuditPackageFilterOptionsResponse>())!;
        Assert.Contains("w227.test.success", filterOptions.Actions);

        var summaryResponse = await _routarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/audit-packages/summary?action=w227.test.success&result=success",
                _managerToken));
        summaryResponse.EnsureSuccessStatusCode();
        var summary =
            (await summaryResponse.Content.ReadFromJsonAsync<AuditPackageExportSummaryResponse>())!;
        Assert.True(summary.Counts.AuditEvents >= 1);

        var jsonResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=json", _managerToken));
        jsonResponse.EnsureSuccessStatusCode();
        var package = (await jsonResponse.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.True(package.Counts.ProofRecords >= 1);
        Assert.True(package.Counts.DvirInspections >= 1);
        Assert.True(package.Counts.CaptureAttachments >= 1);
        Assert.Contains(package.ProofRecords, x =>
            x.ProofType == TripProofTypes.Delivery && !string.IsNullOrWhiteSpace(x.EvidenceHash));
        Assert.Contains(package.DvirInspections, x =>
            x.Phase == DvirInspectionPhases.PreTrip && !string.IsNullOrWhiteSpace(x.EvidenceHash));
        Assert.Contains(package.CaptureAttachments, x =>
            x.AttachmentKind == TripCaptureAttachmentKinds.Photo && !string.IsNullOrWhiteSpace(x.EvidenceHash));

        var csvResponse = await _routarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/audit-packages/export?format=csv&action=w227.test.success",
                _managerToken));
        csvResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", csvResponse.Content.Headers.ContentType?.MediaType);
        var csv = await csvResponse.Content.ReadAsStringAsync();
        Assert.Contains("w227.test.success", csv, StringComparison.Ordinal);

        var timelineResponse = await _routarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/audit-packages/timeline?action=w227.test.failure&pageSize=20",
                _managerToken));
        timelineResponse.EnsureSuccessStatusCode();
        var timeline =
            (await timelineResponse.Content.ReadFromJsonAsync<PagedResult<AuditEventExportItem>>())!;
        Assert.All(timeline.Items, item => Assert.Equal("w227.test.failure", item.Action));
    }

    [Fact]
    public async Task Dispatcher_can_read_but_not_export()
    {
        var dispatcherToken = CreateRoutArrAccessToken(["routarr"], "routarr_dispatcher");

        var summaryResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/summary", dispatcherToken));
        summaryResponse.EnsureSuccessStatusCode();

        var exportResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export", dispatcherToken));
        Assert.Equal(HttpStatusCode.Forbidden, exportResponse.StatusCode);
    }

    [Fact]
    public async Task Background_job_completes_via_internal_process_batch()
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/audit-packages/jobs", _managerToken);
        createRequest.Content = JsonContent.Create(
            new CreateAuditPackageGenerationJobRequest("zip", null, null, "w227.test.success", "success"));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<AuditPackageGenerationJobResponse>())!;
        Assert.Equal(AuditPackageGenerationJobStatuses.Pending, created.Status);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/audit-package-jobs/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(
            new ProcessAuditPackageGenerationJobsRequest(PlatformSeeder.DemoTenantId, null, 5));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch =
            (await processResponse.Content.ReadFromJsonAsync<ProcessAuditPackageGenerationJobsResponse>())!;
        Assert.Equal(AuditPackageGenerationJobStatuses.Completed, batch.Results[0].Status);

        var downloadResponse = await _routarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit-packages/jobs/{created.JobId}/download",
                _managerToken));
        downloadResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", downloadResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Background_job_v1_create_returns_versioned_status_location()
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/v1/audit-packages/jobs", _managerToken);
        createRequest.Content = JsonContent.Create(
            new CreateAuditPackageGenerationJobRequest("json", null, null, "w227.test.success", "success"));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);
        Assert.StartsWith("/api/v1/audit-packages/jobs/", createResponse.Headers.Location?.OriginalString);
        var created = (await createResponse.Content.ReadFromJsonAsync<AuditPackageGenerationJobResponse>())!;
        Assert.Equal(AuditPackageGenerationJobStatuses.Pending, created.Status);

        var statusResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/audit-packages/jobs/{created.JobId}", _managerToken));
        statusResponse.EnsureSuccessStatusCode();
        var status = (await statusResponse.Content.ReadFromJsonAsync<AuditPackageGenerationJobResponse>())!;
        Assert.Equal(created.JobId, status.JobId);
    }

    [Fact]
    public async Task Audit_v1_alias_matches_audit_package_timeline()
    {
        var legacyResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/timeline?action=w227.test.failure&pageSize=20", _managerToken));
        legacyResponse.EnsureSuccessStatusCode();
        var legacy = (await legacyResponse.Content.ReadFromJsonAsync<PagedResult<AuditEventExportItem>>())!;

        var v1Response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit?action=w227.test.failure&pageSize=20", _managerToken));
        v1Response.EnsureSuccessStatusCode();
        var v1 = (await v1Response.Content.ReadFromJsonAsync<PagedResult<AuditEventExportItem>>())!;

        Assert.Equal(legacy.TotalCount, v1.TotalCount);
        Assert.Equal(legacy.Items.Count, v1.Items.Count);
        Assert.All(v1.Items, item => Assert.Equal("w227.test.failure", item.Action));
    }

    private async Task SeedAuditEventsAsync()
    {
        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var tripId = Guid.NewGuid();
        var proofId = Guid.NewGuid();
        var dvirId = Guid.NewGuid();
        db.Trips.Add(new Trip
        {
            Id = tripId,
            TenantId = PlatformSeeder.DemoTenantId,
            TripNumber = "AUD-PROOF-001",
            Title = "Audit proof packet seed",
            Description = "Trip seeded for audit package proof packet export.",
            DispatchStatus = TripDispatchStatuses.Completed,
            AssignedDriverPersonId = PlatformSeeder.DemoAdminUserId.ToString(),
            VehicleRefKey = "VEH-AUD",
            ScheduledStartAt = now.AddHours(-4),
            ScheduledEndAt = now.AddHours(-2),
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now.AddHours(-5),
            UpdatedAt = now,
            CompletedAt = now.AddHours(-1),
        });
        db.TripProofRecords.Add(new TripProofRecord
        {
            Id = proofId,
            TenantId = PlatformSeeder.DemoTenantId,
            TripId = tripId,
            ProofType = TripProofTypes.Delivery,
            CapturedByPersonId = PlatformSeeder.DemoAdminUserId.ToString(),
            VehicleRefKey = "VEH-AUD",
            ReferenceKey = "POD-AUD-001",
            Notes = "Delivered with signature on file.",
            CapturedAt = now.AddMinutes(-45),
            CreatedAt = now.AddMinutes(-44),
            UpdatedAt = now.AddMinutes(-44),
        });
        db.TripDvirInspections.Add(new TripDvirInspection
        {
            Id = dvirId,
            TenantId = PlatformSeeder.DemoTenantId,
            TripId = tripId,
            Phase = DvirInspectionPhases.PreTrip,
            VehicleRefKey = "VEH-AUD",
            Result = DvirInspectionResults.Pass,
            OdometerReading = 12345,
            DefectNotes = string.Empty,
            SubmittedByPersonId = PlatformSeeder.DemoAdminUserId.ToString(),
            SubmittedAt = now.AddHours(-4),
            CreatedAt = now.AddHours(-4),
            UpdatedAt = now.AddHours(-4),
        });
        db.TripCaptureAttachments.Add(new TripCaptureAttachment
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TripId = tripId,
            SubjectType = TripCaptureAttachmentSubjects.Proof,
            SubjectId = proofId,
            AttachmentKind = TripCaptureAttachmentKinds.Photo,
            FileName = "pod-audit.jpg",
            ContentType = "image/jpeg",
            SizeBytes = 2048,
            StorageKey = "audit/pod-audit.jpg",
            Notes = "Photo metadata only for package export.",
            CapturedByPersonId = PlatformSeeder.DemoAdminUserId.ToString(),
            CreatedAt = now.AddMinutes(-43),
        });
        db.AuditEvents.AddRange(
            new RoutArrAuditEvent
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                Action = "w227.test.success",
                TargetType = "trip",
                Result = "success",
                CorrelationId = Guid.NewGuid(),
                OccurredAt = now,
            },
            new RoutArrAuditEvent
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                Action = "w227.test.failure",
                TargetType = "trip",
                Result = "failure",
                CorrelationId = Guid.NewGuid(),
                OccurredAt = now,
            });
        await db.SaveChangesAsync();
    }

    private string CreateRoutArrAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey)
    {
        using var scope = _routarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RoutArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return token;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string? actionScope = null)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-audit-{Guid.NewGuid():N}",
            $"{sourceProduct} audit package test",
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

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
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
