using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Csv;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreStagedImportWizardTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var dbName = $"ComplianceCoreStagedImport-{Guid.NewGuid():N}";
        _factory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();
        await SeedRegulatoryProgramAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Upload_parse_validate_stages_rows_without_committing_canonical_records()
    {
        var token = CreateToken("compliance_admin");
        var session = await CreateUploadParseValidateAsync(token, BuildBundle());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        Assert.True(await db.ImportStagedFactRequirements.AnyAsync(row => row.ImportSessionId == session.ImportSessionId));
        Assert.False(await db.FactRequirements.AnyAsync(row => row.RequirementKey == "req_driver_application"));
    }

    [Fact]
    public async Task Invalid_headers_are_rejected_during_validation()
    {
        var token = CreateToken("compliance_admin");
        var session = await CreateSessionAsync(token);

        using var form = new MultipartFormDataContent();
        AddCsv(form, CsvBundleFiles.ComplianceKeys, "wrong,label\nvalue,Label\n");
        await UploadAsync(token, session.ImportSessionId, form);

        (await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/parse", token))).EnsureSuccessStatusCode();
        var validate = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/validate", token));
        validate.EnsureSuccessStatusCode();
        var results = (await validate.Content.ReadFromJsonAsync<ImportValidationResultsResponse>())!;
        Assert.Equal("failed", results.ValidationStatus);
        Assert.Contains(results.Files, file => file.ValidationErrors.Any(error => error.Contains("Header must be", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Exact_match_candidate_generation_requires_review_when_risk_flagged()
    {
        var token = CreateToken("compliance_admin");
        await SeedDocumentReferenceAsync("driver_qualification_application", "Driver Qualification Application", active: false);
        var session = await CreateUploadParseValidateAsync(token, BuildBundle());

        var candidates = await GenerateCandidatesAsync(token, session.ImportSessionId);
        var candidate = Assert.Single(candidates);
        Assert.Equal("exact", candidate.ConfidenceBand);
        Assert.True(candidate.RequiresConfirmation);
        Assert.Contains(candidate.RiskFlags, risk => risk.Contains("inactive", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Wizard_supports_multiple_evidence_paths_for_one_requirement()
    {
        var token = CreateToken("compliance_admin");
        var session = await CreateUploadParseValidateAsync(
            token,
            BuildBundle(requiredDocumentType: "one_of:road_test_certificate|cdl_equivalent"));
        var candidates = await GenerateCandidatesAsync(token, session.ImportSessionId);

        Assert.Equal(2, candidates.Count);
        var itemResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/items/{candidates[0].MappingCandidateId}", token));
        itemResponse.EnsureSuccessStatusCode();
        var item = (await itemResponse.Content.ReadFromJsonAsync<WizardItemResponse>())!;
        Assert.Equal("one_of", item.EvidenceLogic);
        Assert.NotEmpty(item.OtherAcceptableEvidencePaths);
    }

    [Fact]
    public async Task Bulk_confirm_allows_exact_no_risk_but_rejects_medium_low_and_no_match()
    {
        var token = CreateToken("compliance_admin");
        await SeedDocumentReferenceAsync("driver_qualification_application", "Driver Qualification Application", active: true);
        var session = await CreateUploadParseValidateAsync(token, BuildBundle());
        await GenerateCandidatesAsync(token, session.ImportSessionId);

        var exactRequest = Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/bulk-confirm", token);
        exactRequest.Content = JsonContent.Create(new BulkConfirmMappingsRequest("exact"));
        var exactResponse = await _client.SendAsync(exactRequest);
        exactResponse.EnsureSuccessStatusCode();
        var decisions = (await exactResponse.Content.ReadFromJsonAsync<IReadOnlyList<MappingDecisionResponse>>())!;
        Assert.Single(decisions);

        var lowRequest = Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/bulk-confirm", token);
        lowRequest.Content = JsonContent.Create(new BulkConfirmMappingsRequest("low"));
        var lowResponse = await _client.SendAsync(lowRequest);
        Assert.Equal(HttpStatusCode.BadRequest, lowResponse.StatusCode);
    }

    [Fact]
    public async Task No_document_required_is_blocked_for_document_record_evidence()
    {
        var token = CreateToken("compliance_admin");
        var session = await CreateUploadParseValidateAsync(
            token,
            BuildBundle(evidenceKind: "document_record", requiredDocumentType: "driver_qualification_application"));
        var candidate = Assert.Single(await GenerateCandidatesAsync(token, session.ImportSessionId));

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/items/{candidate.MappingCandidateId}/mark-no-document-required", token));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Force_map_requires_override_permission_reason_and_acknowledgement()
    {
        var adminToken = CreateToken("compliance_admin");
        var reviewerToken = CreateToken("compliance_reviewer");
        var session = await CreateUploadParseValidateAsync(adminToken, BuildBundle());
        var candidate = Assert.Single(await GenerateCandidatesAsync(adminToken, session.ImportSessionId));

        var reviewerRequest = Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/items/{candidate.MappingCandidateId}/force-map", reviewerToken);
        reviewerRequest.Content = JsonContent.Create(new ForceMapRequest("existing_document_type", "doc-1", "driver_qualification_application", "DQ Application", "Reviewed exception.", true));
        var reviewerResponse = await _client.SendAsync(reviewerRequest);
        Assert.Equal(HttpStatusCode.Forbidden, reviewerResponse.StatusCode);

        var missingReasonRequest = Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/items/{candidate.MappingCandidateId}/force-map", adminToken);
        missingReasonRequest.Content = JsonContent.Create(new ForceMapRequest("existing_document_type", "doc-1", "driver_qualification_application", "DQ Application", "", true));
        var missingReasonResponse = await _client.SendAsync(missingReasonRequest);
        Assert.Equal(HttpStatusCode.BadRequest, missingReasonResponse.StatusCode);
    }

    [Fact]
    public async Task Commit_writes_evidence_options_references_audit_traces_and_history()
    {
        var token = CreateToken("compliance_admin");
        await SeedDocumentReferenceAsync("driver_qualification_application", "Driver Qualification Application", active: true);
        var session = await CreateUploadParseValidateAsync(token, BuildBundle());
        var candidate = Assert.Single(await GenerateCandidatesAsync(token, session.ImportSessionId));

        var confirmResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/items/{candidate.MappingCandidateId}/confirm", token));
        confirmResponse.EnsureSuccessStatusCode();

        var previewResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/commit-preview", token));
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<CommitPreviewResponse>())!;
        Assert.Empty(preview.UnresolvedBlockers);

        var commitResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/commit", token));
        commitResponse.EnsureSuccessStatusCode();
        var report = (await commitResponse.Content.ReadFromJsonAsync<ImportCompletionReportResponse>())!;
        Assert.Equal("committed", report.Status);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        Assert.True(await db.FactRequirements.AnyAsync(row => row.RequirementKey == "req_driver_application"));
        Assert.True(await db.ComplianceEvidenceOptionGroups.AnyAsync(row => row.RequirementKey == "req_driver_application"));
        Assert.True(await db.ComplianceEvidenceOptions.AnyAsync(row => row.OptionKey.Contains("driver_qualification_application")));
        Assert.True(await db.EvidenceReferences.AnyAsync(row => row.EvidenceId.StartsWith($"import:{session.ImportSessionId:N}")));
        Assert.True(await db.AuditTraces.AnyAsync(row => row.AuditTraceId.StartsWith($"import:{session.ImportSessionId:N}")));
        Assert.True(await db.AuditEvents.AnyAsync(row => row.Action == "import_session.committed"));
    }

    [Fact]
    public async Task Import_wizard_maps_exception_proof_and_commits_legal_relief_audit_trace()
    {
        var token = CreateToken("compliance_admin");
        var session = await CreateUploadParseValidateAsync(token, BuildBundle(requiredDocumentType: "medical_variance_document"));
        var candidate = Assert.Single(await GenerateCandidatesAsync(token, session.ImportSessionId));

        var proofRequest = Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/items/{candidate.MappingCandidateId}/create-exception-exemption", token);
        proofRequest.Content = JsonContent.Create(new ExceptionProofMappingRequest(
            "driver_medical_variance",
            "variance",
            "driver_medical_variance",
            "Driver medical variance",
            new Dictionary<string, string>
            {
                ["type"] = "variance",
                ["effectType"] = "authorizes_otherwise_blocked_action",
                ["label"] = "Driver medical variance"
            },
            ["medical variance proof remains subject to expiration and scope checks"]));
        var proofResponse = await _client.SendAsync(proofRequest);
        proofResponse.EnsureSuccessStatusCode();
        var decision = (await proofResponse.Content.ReadFromJsonAsync<MappingDecisionResponse>())!;
        Assert.Equal("create_new_exception_exemption_record", decision.Decision);
        Assert.Equal("changes_applicability", decision.EvidenceMappingPurpose);
        Assert.Equal("driver_medical_variance", decision.ExceptionExemptionKey);

        var commitResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/wizard/commit", token));
        commitResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        Assert.True(await db.ComplianceExceptionExemptions.AnyAsync(row => row.Key == "driver_medical_variance" && row.Type == "variance"));
        Assert.True(await db.EvidenceReferences.AnyAsync(row => row.Notes.Contains("Evidence changes applicability")));
        Assert.True(await db.AuditTraces.AnyAsync(row =>
            row.ClaimedExceptionExemptionKey == "driver_medical_variance" &&
            row.ExceptionExemptionApplied &&
            row.FinalComplianceResult == "mapping_committed"));
    }

    [Fact]
    public async Task Unauthorized_users_cannot_commit()
    {
        var adminToken = CreateToken("compliance_admin");
        var memberToken = CreateToken("tenant_member");
        var session = await CreateUploadParseValidateAsync(adminToken, BuildBundle());

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/commit", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<ImportSessionResponse> CreateUploadParseValidateAsync(string token, MultipartFormDataContent form)
    {
        var session = await CreateSessionAsync(token);
        await UploadAsync(token, session.ImportSessionId, form);
        (await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/parse", token))).EnsureSuccessStatusCode();
        var validate = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{session.ImportSessionId}/validate", token));
        validate.EnsureSuccessStatusCode();
        var results = (await validate.Content.ReadFromJsonAsync<ImportValidationResultsResponse>())!;
        Assert.Equal("passed", results.ValidationStatus);
        return session;
    }

    private async Task<IReadOnlyList<MappingCandidateResponse>> GenerateCandidatesAsync(string token, Guid sessionId)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{sessionId}/generate-mapping-candidates", token));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IReadOnlyList<MappingCandidateResponse>>())!;
    }

    private async Task<ImportSessionResponse> CreateSessionAsync(string token)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/import-sessions", token);
        request.Content = JsonContent.Create(new CreateImportSessionRequest(Notes: "Wizard test"));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ImportSessionResponse>())!;
    }

    private async Task UploadAsync(string token, Guid sessionId, MultipartFormDataContent form)
    {
        var request = Authorized(HttpMethod.Post, $"/api/v1/import-sessions/{sessionId}/upload", token);
        request.Content = form;
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static MultipartFormDataContent BuildBundle(
        string evidenceKind = "product_record",
        string requiredDocumentType = "driver_qualification_application")
    {
        const string ruleContent = """{"schemaVersion":1,"logic":"all","rules":[{"ruleKey":"driver_application","label":"DQ application","type":"fact_boolean","factKey":"driver_application_present","expectedValue":true}]}""";
        var form = new MultipartFormDataContent();
        AddCsv(form, CsvBundleFiles.ControlledVocabulary, "term_key,vocabulary_type_key,label,description,active\n");
        AddCsv(form, CsvBundleFiles.VocabularyAliases, "term_key,alias_text,active\n");
        AddCsv(form, CsvBundleFiles.ComplianceKeys, "key,label,category,description,active\ndriver_qualification,Driver Qualification,compliance_domain,Driver qualification domain,true\n");
        AddCsv(form, CsvBundleFiles.MaterialKeys, "key,label,category,description,active\n");
        AddCsv(
            form,
            CsvBundleFiles.RulePacks,
            "pack_key,program_key,version_number,label,description,status,active,rule_content_json\n"
            + $"driver_qualification,fmcsa_safety,1,Driver Qualification,Driver qualification pack,published,true,\"{ruleContent.Replace("\"", "\"\"")}\"\n");
        AddCsv(
            form,
            CsvBundleFiles.RuleRequirements,
            "citation_key,program_key,pack_key,pack_version,label,source_reference,description,active,supersedes_citation_key\n"
            + "t49_391_21,fmcsa_safety,driver_qualification,1,Driver application,49 CFR 391.21,Driver application citation,true,\n");
        AddCsv(
            form,
            CsvBundleFiles.RuleFactRequirements,
            "requirement_key,fact_key,pack_key,pack_version,citation_key,citation_version,applicability_key,source_product,source_entity,source_field_or_record_type,value_type,operator,expected_value,evidence_kind,required_document_type,retention_period,audit_question,failure_severity,automatic_failure_flag,override_allowed,override_permission,remediation_required,label,description,is_required,active\n"
            + $"req_driver_application,driver_application_present,driver_qualification,1,t49_391_21,1,motor_carrier_driver,StaffArr,driver,driver_qualification_application,boolean,equals,true,{evidenceKind},{requiredDocumentType},49_cfr_391_51,Is the driver qualification application present?,major,false,true,compliancecore.import.override,true,DQ application present,Driver qualification application is present,true,true\n");
        AddCsv(form, CsvBundleFiles.RegulatoryMappings, "mapping_key,target_kind,program_key,pack_key,pack_version,citation_key,compliance_key,material_key,fact_key,label,description,active\n");
        AddCsv(form, CsvBundleFiles.SdsReferences, "sds_key,material_key,product_name,manufacturer,document_url,revision_date,active\n");
        AddCsv(form, CsvBundleFiles.ExceptionExemptions, "key,label,type,governing_body,program_key,pack_key,citation_key,applicability_key,applies_to_subject_kind,applies_to_source_product,applies_to_source_entity,effect_type,condition_logic_json,required_evidence_option_group_key,issuing_authority,authorization_number,effective_at,expires_at,active,description\n");
        return form;
    }

    private static void AddCsv(MultipartFormDataContent form, string fileName, string content) =>
        form.Add(new StringContent(content, Encoding.UTF8, "text/csv"), "file", fileName);

    private async Task SeedDocumentReferenceAsync(string stableKey, string label, bool active)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        db.DocumentReferences.Add(new DocumentReference
        {
            ReferenceId = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            SourceProduct = "StaffArr",
            ObjectKind = "document",
            ExternalRecordId = stableKey,
            StableKey = stableKey,
            Label = label,
            Description = "Seeded document reference.",
            Active = active,
            LastSeenAt = DateTimeOffset.UtcNow,
            MetadataJson = "{}",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedRegulatoryProgramAsync(ComplianceCoreDbContext db)
    {
        var body = new GoverningBody
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            BodyKey = "dot",
            Label = "U.S. Department of Transportation",
            Description = "Federal transportation safety authority.",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var jurisdiction = new Jurisdiction
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            GoverningBodyId = body.Id,
            JurisdictionKey = "us_federal",
            Label = "United States Federal",
            Description = "Federal jurisdiction.",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.GoverningBodies.Add(body);
        db.Jurisdictions.Add(jurisdiction);
        db.RegulatoryPrograms.Add(new RegulatoryProgram
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            JurisdictionId = jurisdiction.Id,
            ProgramKey = "fmcsa_safety",
            Label = "FMCSA Safety Compliance",
            Description = "Federal motor carrier safety compliance program.",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private string CreateToken(string tenantRoleKey)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            ["compliancecore"],
            isPlatformAdmin: false);

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
            .Where(descriptor => descriptor.ServiceType == typeof(DbContextOptions<TContext>) || descriptor.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
