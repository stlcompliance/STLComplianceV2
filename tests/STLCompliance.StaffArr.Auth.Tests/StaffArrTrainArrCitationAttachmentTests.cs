using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrCitationAttachmentTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _trainarrToComplianceCoreToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"CitationNexArr-{Guid.NewGuid():N}";
        var complianceDbName = $"CitationCompliance-{Guid.NewGuid():N}";
        var trainArrDbName = $"CitationTrainArr-{Guid.NewGuid():N}";

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
        _complianceCoreClient = _complianceCoreFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _trainarrToComplianceCoreToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["compliancecore"],
            $"{InternalRuleEvaluationService.EvaluateActionScope},{InternalCitationLookupService.ReadActionScope}");

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("ComplianceCore:BaseUrl", _complianceCoreClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("ComplianceCore:ServiceToken", _trainarrToComplianceCoreToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));

                services.AddHttpClient<TrainArr.Api.Services.ComplianceCoreRuleEvaluationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
                services.AddHttpClient<TrainArr.Api.Services.ComplianceCoreCitationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Training_definition_citation_attach_list_remove_with_metadata()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var citation = await CreateComplianceCitationAsync();

        var attachRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-definitions/{definitionId}/citations?validateWithComplianceCore=true",
            adminToken);
        attachRequest.Content = JsonContent.Create(new AttachTrainingCitationRequest(
            citation.CitationId,
            citation.CitationKey));
        var attachResponse = await _trainarrClient.SendAsync(attachRequest);
        attachResponse.EnsureSuccessStatusCode();
        var attached = (await attachResponse.Content.ReadFromJsonAsync<TrainingCitationAttachmentResponse>())!;
        Assert.Equal(citation.CitationKey, attached.CitationKey);
        Assert.NotNull(attached.Metadata);
        Assert.Equal(citation.Label, attached.Metadata!.Label);

        var listRequest = Authorized(
            HttpMethod.Get,
            $"/api/training-definitions/{definitionId}/citations?includeMetadata=true",
            adminToken);
        var listResponse = await _trainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingCitationAttachmentResponse>>())!;
        Assert.Single(list);

        var removeRequest = Authorized(
            HttpMethod.Delete,
            $"/api/training-definitions/{definitionId}/citations/{attached.AttachmentId}",
            adminToken);
        var removeResponse = await _trainarrClient.SendAsync(removeRequest);
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        var listAfterRemove = await _trainarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/training-definitions/{definitionId}/citations?includeMetadata=true",
                adminToken));
        listAfterRemove.EnsureSuccessStatusCode();
        var empty = (await listAfterRemove.Content.ReadFromJsonAsync<IReadOnlyList<TrainingCitationAttachmentResponse>>())!;
        Assert.Empty(empty);
    }

    [Fact]
    public async Task Training_program_citation_attachment_persists_reference_only()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var programId = await CreateTrainingProgramAsync(adminToken, definitionId);
        var citationId = Guid.NewGuid();

        var attachRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-programs/{programId}/citations",
            adminToken);
        attachRequest.Content = JsonContent.Create(new AttachTrainingCitationRequest(
            citationId,
            "cfr_391_11"));
        var attachResponse = await _trainarrClient.SendAsync(attachRequest);
        attachResponse.EnsureSuccessStatusCode();

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var stored = await db.TrainingCitationAttachments.SingleAsync();
        Assert.Equal(citationId, stored.ComplianceCoreCitationId);
        Assert.Equal(TrainingCitationEntityTypes.TrainingProgram, stored.EntityType);
        Assert.Equal(programId, stored.EntityId);
    }

    [Fact]
    public async Task Training_definition_citation_attach_denies_member_role()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");

        var attachRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-definitions/{definitionId}/citations",
            memberToken);
        attachRequest.Content = JsonContent.Create(new AttachTrainingCitationRequest(
            Guid.NewGuid(),
            "cfr_391_11"));
        var attachResponse = await _trainarrClient.SendAsync(attachRequest);
        Assert.Equal(HttpStatusCode.Forbidden, attachResponse.StatusCode);
    }

    [Fact]
    public async Task Training_definition_citation_attach_rejects_duplicate()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var citationId = Guid.NewGuid();

        var attachRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-definitions/{definitionId}/citations",
            adminToken);
        attachRequest.Content = JsonContent.Create(new AttachTrainingCitationRequest(citationId, "cfr_391_11"));
        (await _trainarrClient.SendAsync(attachRequest)).EnsureSuccessStatusCode();

        var duplicateRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-definitions/{definitionId}/citations",
            adminToken);
        duplicateRequest.Content = JsonContent.Create(new AttachTrainingCitationRequest(citationId, "cfr_391_11"));
        var duplicateResponse = await _trainarrClient.SendAsync(duplicateRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Citation_attach_writes_audit_event()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var attachRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-definitions/{definitionId}/citations",
            adminToken);
        attachRequest.Content = JsonContent.Create(new AttachTrainingCitationRequest(Guid.NewGuid(), "cfr_391_11"));
        (await _trainarrClient.SendAsync(attachRequest)).EnsureSuccessStatusCode();

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var audit = await db.AuditEvents.SingleAsync(x => x.Action == "citation.attach");
        Assert.Equal(TrainingCitationEntityTypes.TrainingDefinition, audit.TargetType);
        Assert.Equal(definitionId.ToString(), audit.TargetId);
    }

    private async Task<RegulatoryCitationResponse> CreateComplianceCitationAsync()
    {
        var complianceAdminToken = CreateComplianceCoreAccessToken(
            ["compliancecore"],
            tenantRoleKey: "compliance_admin");
        var programId = await CreateSampleProgramAsync(complianceAdminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/citations", complianceAdminToken);
        createRequest.Content = JsonContent.Create(new CreateRegulatoryCitationRequest(
            programId,
            null,
            $"cfr_{Guid.NewGuid():N}".Substring(0, 12),
            "391.11 General qualifications",
            "49 CFR 391.11",
            "General qualifications of drivers.",
            null));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        return (await createResponse.Content.ReadFromJsonAsync<RegulatoryCitationResponse>())!;
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "Department of Transportation",
            "US DOT"));
        var bodyResponse = await _complianceCoreClient.SendAsync(bodyRequest);
        if (bodyResponse.IsSuccessStatusCode)
        {
            var body = (await bodyResponse.Content.ReadFromJsonAsync<GoverningBodyResponse>())!;
            var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
            jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
                body.GoverningBodyId,
                "us_federal",
                "US Federal",
                "United States federal jurisdiction."));
            var jurisdictionResponse = await _complianceCoreClient.SendAsync(jurisdictionRequest);
            jurisdictionResponse.EnsureSuccessStatusCode();
            var jurisdiction = (await jurisdictionResponse.Content.ReadFromJsonAsync<JurisdictionResponse>())!;

            var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
            programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
                jurisdiction.JurisdictionId,
                "driver_compliance",
                "Driver Compliance",
                "Driver qualification program."));
            var programResponse = await _complianceCoreClient.SendAsync(programRequest);
            programResponse.EnsureSuccessStatusCode();
            var program = (await programResponse.Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
            return program.RegulatoryProgramId;
        }

        var listPrograms = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/regulatory-programs", adminToken));
        listPrograms.EnsureSuccessStatusCode();
        var programs = (await listPrograms.Content.ReadFromJsonAsync<IReadOnlyList<RegulatoryProgramResponse>>())!;
        return programs[0].RegulatoryProgramId;
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string adminToken)
    {
        var definitionKey = $"citation_def_{Guid.NewGuid():N}"[..20];
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            definitionKey,
            "Citation definition",
            "Training definition for citation attachment tests.",
            "hazmat_endorsement",
            "Hazmat Endorsement"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return created.TrainingDefinitionId;
    }

    private async Task<Guid> CreateTrainingProgramAsync(string adminToken, Guid definitionId)
    {
        var programKey = $"citation_prog_{Guid.NewGuid():N}"[..20];
        var request = Authorized(HttpMethod.Post, "/api/training-programs", adminToken);
        request.Content = JsonContent.Create(new CreateTrainingProgramRequest(
            programKey,
            "Citation program",
            "Program for citation attachment tests.",
            [definitionId]));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingProgramDetailResponse>())!;
        return created.ProgramId;
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArr.Api.Services.TrainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private string CreateComplianceCoreAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-citation-{Guid.NewGuid():N}",
            $"{sourceProduct} citation test",
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
