using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using StaffArr.Api.Data;
using StaffArr.Api.Endpoints;
using StaffArr.Api.Entities;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrOrphanReferenceWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _sharedWorkerToTrainarrToken = null!;
    private string _trainarrToStaffarrToken = null!;
    private string _trainarrToComplianceCoreToken = null!;
    private Guid _validPersonId = Guid.Parse("33333333-3333-3333-3333-333333333301");
    private Guid _orphanPersonId = Guid.Parse("44444444-4444-4444-4444-444444444401");
    private Guid _orphanCitationId = Guid.Parse("55555555-5555-5555-5555-555555555501");

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"OrphanRefNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"OrphanRefStaffArr-{Guid.NewGuid():N}";
        var complianceDbName = $"OrphanRefCompliance-{Guid.NewGuid():N}";
        var trainArrDbName = $"OrphanRefTrainArr-{Guid.NewGuid():N}";

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
            });
        });

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(complianceDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        _staffarrClient = _staffarrFactory.CreateClient();
        _complianceCoreClient = _complianceCoreFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _sharedWorkerToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            OrphanReferenceWorkerService.ProcessOrphanReferenceScansActionScope);
        _trainarrToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            IntegrationEndpoints.TrainarrPersonLookupActionScope);
        _trainarrToComplianceCoreToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["compliancecore"],
            $"{InternalRuleEvaluationService.EvaluateActionScope},{InternalCitationLookupService.ReadActionScope},{InternalRulePackLookupService.ReadActionScope}");

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("StaffArr:BaseUrl", _staffarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", _trainarrToStaffarrToken);
            builder.UseSetting("ComplianceCore:BaseUrl", _complianceCoreClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("ComplianceCore:ServiceToken", _trainarrToComplianceCoreToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));

                services.AddHttpClient<StaffArrPersonLookupClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<ComplianceCoreCitationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
                services.AddHttpClient<ComplianceCoreRulePackClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            await db.Database.EnsureCreatedAsync();
            await SeedStaffArrPersonAsync(db, _validPersonId);
        }

        using (var scope = _complianceCoreFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
            await db.Database.EnsureCreatedAsync();
            var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
            await vocabularyService.EnsureVocabularyTypesSeededAsync();
        }
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _complianceCoreClient.Dispose();
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _complianceCoreFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_orphan_reference_batch_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/orphan-references/process-batch",
            new ProcessOrphanReferenceScansRequest(null, DateTimeOffset.UtcNow, 10, 24));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_orphan_reference_scans_returns_enabled_tenant()
    {
        await SeedOrphanReferenceSettingsAsync(PlatformSeeder.DemoTenantId);

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/orphan-references/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10&stalenessHours=24");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);

        var listResponse = await _trainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingOrphanReferenceScansResponse>())!;
        Assert.Contains(pending.Items, x => x.TenantId == PlatformSeeder.DemoTenantId);
    }

    [Fact]
    public async Task Process_orphan_reference_batch_detects_missing_staffarr_person_and_citation()
    {
        await SeedOrphanReferenceSettingsAsync(PlatformSeeder.DemoTenantId);
        await SeedOrphanTrainingRecordsAsync();

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/orphan-references/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessOrphanReferenceScansRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            10,
            24));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessOrphanReferenceScansResponse>())!;
        Assert.Equal(1, body.TenantsScanned);
        Assert.True(body.FindingsDetected >= 2);

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var personFinding = await db.OrphanReferenceFindings.SingleAsync(x =>
            x.ReferenceKind == OrphanReferenceRules.ReferenceKindStaffarrPerson
            && x.ReferenceKey == OrphanReferenceRules.BuildStaffarrPersonReferenceKey(_orphanPersonId));
        Assert.True(personFinding.IsActive);

        var citationFinding = await db.OrphanReferenceFindings.SingleAsync(x =>
            x.ReferenceKind == OrphanReferenceRules.ReferenceKindComplianceCoreCitation
            && x.ReferenceKey == OrphanReferenceRules.BuildComplianceCoreCitationReferenceKey(_orphanCitationId));
        Assert.True(citationFinding.IsActive);

        var run = await db.OrphanReferenceRuns.SingleAsync();
        Assert.Equal("found", run.Outcome);
    }

    private async Task SeedOrphanReferenceSettingsAsync(Guid tenantId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantOrphanReferenceSettings.Add(new TenantOrphanReferenceSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IsEnabled = true,
            ScanStalenessHours = 24,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedOrphanTrainingRecordsAsync()
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var definitionId = Guid.NewGuid();

        db.TrainingDefinitions.Add(new TrainingDefinition
        {
            Id = definitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = "orphan_scan_def",
            Name = "Orphan scan definition",
            Description = "Definition for orphan reference worker tests.",
            QualificationKey = "hazmat_endorsement",
            QualificationName = "Hazmat Endorsement",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingDefinitionId = definitionId,
            StaffarrPersonId = _validPersonId,
            Status = "assigned",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingDefinitionId = definitionId,
            StaffarrPersonId = _orphanPersonId,
            Status = "assigned",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingCitationAttachments.Add(new TrainingCitationAttachment
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            EntityType = "training_definition",
            EntityId = definitionId,
            ComplianceCoreCitationId = _orphanCitationId,
            CitationKey = "missing.citation",
            CitationVersion = 1,
            CreatedAt = now,
        });

        db.TrainingRulePackRequirements.Add(new TrainingRulePackRequirement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            EntityType = "training_definition",
            EntityId = definitionId,
            RulePackKey = "missing_rule_pack",
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private static async Task SeedStaffArrPersonAsync(StaffArrDbContext db, Guid personId)
    {
        var now = DateTimeOffset.UtcNow;
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            ExternalUserId = PlatformSeeder.DemoAdminUserId,
            GivenName = "Valid",
            FamilyName = "Person",
            DisplayName = "Valid Person",
            PrimaryEmail = "valid.person@demo.stl",
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
            $"{sourceProduct}-orphan-ref-{Guid.NewGuid():N}",
            $"{sourceProduct} orphan reference test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<TContext>)
            || d.ServiceType == typeof(TContext)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
