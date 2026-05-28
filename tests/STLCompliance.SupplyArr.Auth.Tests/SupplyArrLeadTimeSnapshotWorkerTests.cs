using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrLeadTimeSnapshotWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _sharedWorkerToSupplyArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"LeadTimeSnapshotNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"LeadTimeSnapshotSupplyArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToSupplyArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["supplyarr"],
            LeadTimeSnapshotWorkerService.ProcessLeadTimeSnapshotCapturesActionScope);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.PostAsJsonAsync(
            "/api/internal/lead-time-snapshots/process-batch",
            new ProcessLeadTimeSnapshotCapturesRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 25, 24));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_creates_lead_time_snapshot_from_catalog_lead_time()
    {
        var linkId = await SeedVendorLinkWithCatalogLeadTimeAsync(catalogLeadTimeDays: 14);
        await UpsertSettingsAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/lead-time-snapshots/process-batch",
            _sharedWorkerToSupplyArrToken);
        processRequest.Content = JsonContent.Create(new ProcessLeadTimeSnapshotCapturesRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            24));

        var processResponse = await _supplyarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessLeadTimeSnapshotCapturesResponse>())!;
        Assert.Equal(1, body.CandidatesFound);
        Assert.Equal(1, body.CapturedCount);
        Assert.Single(body.Captured);
        Assert.Equal(14, body.Captured[0].LeadTimeDays);
        Assert.Equal(SnapshotSources.VendorFeed, body.Captured[0].Source);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var captureState = await db.PartVendorLeadTimeCaptureStates
            .SingleAsync(x => x.PartVendorLinkId == linkId);
        Assert.Equal(14, captureState.LastCapturedLeadTimeDays);
        Assert.NotNull(captureState.LastLeadTimeSnapshotId);
    }

    [Fact]
    public async Task List_pending_lead_time_snapshot_returns_catalog_drift_candidates()
    {
        await SeedVendorLinkWithCatalogLeadTimeAsync(catalogLeadTimeDays: 21, currentLeadTimeDays: 14);
        await UpsertSettingsAsync();

        var pendingRequest = Authorized(
            HttpMethod.Get,
            $"/api/internal/lead-time-snapshots/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=25",
            _sharedWorkerToSupplyArrToken);

        var pendingResponse = await _supplyarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingLeadTimeSnapshotCapturesResponse>())!;
        Assert.NotEmpty(pending.Items);
        Assert.Equal(21, pending.Items[0].CatalogLeadTimeDays);
    }

    private async Task UpsertSettingsAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantLeadTimeSnapshotSettings.Add(new TenantLeadTimeSnapshotSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            StalenessHours = 24,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedVendorLinkWithCatalogLeadTimeAsync(
        int catalogLeadTimeDays,
        int? currentLeadTimeDays = null)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var vendor = new ExternalParty
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartyKey = $"vendor-{Guid.NewGuid():N}"[..16],
            PartyType = "vendor",
            DisplayName = "Lead-time vendor",
            LegalName = "Lead-time vendor LLC",
            TaxIdentifier = string.Empty,
            ApprovalStatus = "approved",
            Status = "active",
            Notes = string.Empty,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = $"part-{Guid.NewGuid():N}"[..16],
            DisplayName = "Lead-time snapshot part",
            Description = string.Empty,
            CategoryKey = "general",
            UnitOfMeasure = "each",
            ManufacturerName = string.Empty,
            ManufacturerPartNumber = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var link = new PartVendorLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            ExternalPartyId = vendor.Id,
            VendorPartNumber = "VPN-200",
            IsPreferred = true,
            CatalogLeadTimeDays = catalogLeadTimeDays,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ExternalParties.Add(vendor);
        db.Parts.Add(part);
        db.PartVendorLinks.Add(link);

        if (currentLeadTimeDays is not null)
        {
            db.PartVendorLeadTimeSnapshots.Add(new PartVendorLeadTimeSnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PartVendorLinkId = link.Id,
                SnapshotKey = $"manual-{Guid.NewGuid():N}"[..24],
                LeadTimeDays = currentLeadTimeDays.Value,
                EffectiveFrom = now.AddDays(-1),
                EffectiveTo = null,
                Source = SnapshotSources.Manual,
                Notes = string.Empty,
                CreatedByUserId = Guid.NewGuid(),
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await db.SaveChangesAsync();
        return link.Id;
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
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-lead-time-snapshot-test",
            $"{sourceProduct} lead-time snapshot test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
