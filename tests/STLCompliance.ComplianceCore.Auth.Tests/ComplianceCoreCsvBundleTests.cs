using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Csv;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreCsvBundleTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreCsvBundle-{Guid.NewGuid():N}";

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _complianceCoreClient = _complianceCoreFactory.CreateClient();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
    }

    [Fact]
    public async Task Csv_bundle_manifest_lists_nine_files()
    {
        var token = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/csv-bundle/manifest", token));
        response.EnsureSuccessStatusCode();
        var manifest = (await response.Content.ReadFromJsonAsync<CsvBundleManifestResponse>())!;
        Assert.Equal(10, manifest.Files.Count);
        Assert.Contains(manifest.Files, file => file.FileName == CsvBundleFiles.ControlledVocabulary);
        Assert.Contains(manifest.Files, file => file.FileName == CsvBundleFiles.SdsReferences);
        Assert.Contains(manifest.Files, file => file.FileName == CsvBundleFiles.ExceptionExemptions);
    }

    [Fact]
    public async Task Csv_bundle_export_zip_contains_nine_csv_files()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedSampleTenantDataAsync(adminToken);

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/csv-bundle/export", adminToken));
        response.EnsureSuccessStatusCode();
        var zipBytes = await response.Content.ReadAsByteArrayAsync();
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        Assert.Equal(10, archive.Entries.Count);
        Assert.Contains(archive.Entries, entry => entry.Name == CsvBundleFiles.ComplianceKeys);
        Assert.Contains(archive.Entries, entry => entry.Name == CsvBundleFiles.ExceptionExemptions);
    }

    [Fact]
    public async Task Csv_bundle_import_round_trip_upserts_keys()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin", isPlatformAdmin: true);
        await SeedSampleTenantDataAsync(adminToken);

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/csv-bundle/files/{CsvBundleFiles.ComplianceKeys}", adminToken));
        exportResponse.EnsureSuccessStatusCode();
        var exportedCsv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("vehicle_inspection", exportedCsv, StringComparison.Ordinal);

        var updatedCsv = $"{exportedCsv.Trim()}\nextra_key,Extra Key,compliance_domain,Imported via CSV,true";

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(updatedCsv, Encoding.UTF8, "text/csv"), "file", CsvBundleFiles.ComplianceKeys);

        var importRequest = Authorized(HttpMethod.Post, "/api/csv-bundle/import?dryRun=false", adminToken);
        importRequest.Content = form;
        var importResponse = await _complianceCoreClient.SendAsync(importRequest);
        importResponse.EnsureSuccessStatusCode();
        var result = (await importResponse.Content.ReadFromJsonAsync<CsvImportResultResponse>())!;
        Assert.True(result.Applied);
        Assert.Empty(result.Issues);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/compliance-keys", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var keys = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<ComplianceKeyResponse>>())!;
        Assert.Contains(keys, key => key.Key == "extra_key");
    }

    [Fact]
    public async Task Csv_bundle_import_denies_tenant_member()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        using var form = new MultipartFormDataContent();
        form.Add(
            new StringContent(
                "term_key,vocabulary_type_key,label,description,active\nsample,material_hazard,Sample,Desc,true",
                Encoding.UTF8,
                "text/csv"),
            "file",
            CsvBundleFiles.ControlledVocabulary);

        var importRequest = Authorized(HttpMethod.Post, "/api/csv-bundle/import?dryRun=false", memberToken);
        importRequest.Content = form;
        var response = await _complianceCoreClient.SendAsync(importRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Csv_bundle_import_denies_non_platform_compliance_admin()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        using var form = new MultipartFormDataContent();
        form.Add(
            new StringContent(
                "term_key,vocabulary_type_key,label,description,active\nsample,material_hazard,Sample,Desc,true",
                Encoding.UTF8,
                "text/csv"),
            "file",
            CsvBundleFiles.ControlledVocabulary);

        var importRequest = Authorized(HttpMethod.Post, "/api/csv-bundle/import?dryRun=false", adminToken);
        importRequest.Content = form;
        var response = await _complianceCoreClient.SendAsync(importRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Csv_bundle_import_dry_run_reports_validation_without_apply()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin", isPlatformAdmin: true);
        using var form = new MultipartFormDataContent();
        form.Add(
            new StringContent(
                "term_key,vocabulary_type_key,label,description,active\nbad_term,unknown_type,Bad,Desc,true",
                Encoding.UTF8,
                "text/csv"),
            "file",
            CsvBundleFiles.ControlledVocabulary);

        var importRequest = Authorized(HttpMethod.Post, "/api/csv-bundle/import?dryRun=true", adminToken);
        importRequest.Content = form;
        var response = await _complianceCoreClient.SendAsync(importRequest);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<CsvImportResultResponse>())!;
        Assert.True(result.DryRun);
        Assert.False(result.Applied);
        Assert.Contains(result.Issues, issue => issue.Code == "vocabulary.type_unknown");
    }

    [Fact]
    public async Task Csv_bundle_import_upserts_expanded_fact_requirements_from_rule_fact_requirements()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin", isPlatformAdmin: true);
        await SeedSampleTenantDataAsync(adminToken);

        using var form = BuildExpandedRuleFactRequirementBundle(valueType: "boolean");
        var importRequest = Authorized(HttpMethod.Post, "/api/csv-bundle/import?dryRun=false", adminToken);
        importRequest.Content = form;
        var importResponse = await _complianceCoreClient.SendAsync(importRequest);
        importResponse.EnsureSuccessStatusCode();
        var result = (await importResponse.Content.ReadFromJsonAsync<CsvImportResultResponse>())!;
        Assert.True(result.Applied);
        Assert.Empty(result.Issues);

        var factResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/facts/t49_dq_application_present", adminToken));
        factResponse.EnsureSuccessStatusCode();
        var fact = (await factResponse.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        Assert.Equal("boolean", fact.ValueType);

        var requirementsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/fact-requirements?sourceProduct=StaffArr&sourceEntity=driver", adminToken));
        requirementsResponse.EnsureSuccessStatusCode();
        var requirements = (await requirementsResponse.Content.ReadFromJsonAsync<IReadOnlyList<FactRequirementResponse>>())!;
        var requirement = Assert.Single(requirements);
        Assert.Equal("StaffArr", requirement.SourceProduct);
        Assert.Equal("driver", requirement.SourceEntity);
        Assert.Equal("driver_qualification_application", requirement.SourceFieldOrRecordType);
        Assert.Equal("Is the driver qualification application present?", requirement.AuditQuestion);
        Assert.Equal("major", requirement.FailureSeverity);
        Assert.True(requirement.RemediationRequired);
    }

    [Fact]
    public async Task Csv_bundle_import_rejects_invalid_fact_requirement_enum_values()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin", isPlatformAdmin: true);
        await SeedSampleTenantDataAsync(adminToken);

        using var form = BuildExpandedRuleFactRequirementBundle(valueType: "blob");
        var importRequest = Authorized(HttpMethod.Post, "/api/csv-bundle/import?dryRun=false", adminToken);
        importRequest.Content = form;
        var importResponse = await _complianceCoreClient.SendAsync(importRequest);
        importResponse.EnsureSuccessStatusCode();
        var result = (await importResponse.Content.ReadFromJsonAsync<CsvImportResultResponse>())!;
        Assert.False(result.Applied);
        Assert.Contains(result.Issues, issue => issue.Code == "fact_requirements.validation");
    }

    [Fact]
    public async Task V1_rule_pack_import_routes_preview_validate_publish_and_followups()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin", isPlatformAdmin: true);
        await SeedSampleTenantDataAsync(adminToken);

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/csv-bundle/files/{CsvBundleFiles.ComplianceKeys}", adminToken));
        exportResponse.EnsureSuccessStatusCode();
        var exportedCsv = await exportResponse.Content.ReadAsStringAsync();
        var updatedCsv = $"{exportedCsv.Trim()}\nrule_import_key,Rule Import Key,compliance_domain,Imported via rule-pack-import routes,true";

        using var previewForm = new MultipartFormDataContent();
        previewForm.Add(new StringContent(updatedCsv, Encoding.UTF8, "text/csv"), "file", CsvBundleFiles.ComplianceKeys);
        var previewRequest = Authorized(HttpMethod.Post, "/api/v1/rule-pack-imports/preview", adminToken);
        previewRequest.Content = previewForm;
        var previewResponse = await _complianceCoreClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<RulePackImportRunResponse>())!;
        Assert.Equal("validated", preview.Status);
        Assert.True(preview.DryRun);

        var validateGetResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/rule-pack-imports/{preview.ImportId}", adminToken));
        validateGetResponse.EnsureSuccessStatusCode();

        var diffResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/rule-pack-imports/{preview.ImportId}/diff", adminToken));
        diffResponse.EnsureSuccessStatusCode();
        var diff = (await diffResponse.Content.ReadFromJsonAsync<RulePackImportDiffResponse>())!;
        Assert.Equal(preview.ImportId, diff.ImportId);

        var testsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/rule-pack-imports/{preview.ImportId}/test-results", adminToken));
        testsResponse.EnsureSuccessStatusCode();
        var testResults = (await testsResponse.Content.ReadFromJsonAsync<RulePackImportTestResultsResponse>())!;
        Assert.Equal(preview.ImportId, testResults.ImportId);

        using var publishForm = new MultipartFormDataContent();
        publishForm.Add(new StringContent(updatedCsv, Encoding.UTF8, "text/csv"), "file", CsvBundleFiles.ComplianceKeys);
        var publishRequest = Authorized(HttpMethod.Post, "/api/v1/rule-pack-imports/publish-draft", adminToken);
        publishRequest.Content = publishForm;
        var publishResponse = await _complianceCoreClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<RulePackImportRunResponse>())!;
        Assert.Equal("applied", published.Status);
        Assert.False(published.DryRun);

        var rollbackResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/rule-pack-imports/{published.ImportId}/rollback", adminToken));
        rollbackResponse.EnsureSuccessStatusCode();
        var rollback = (await rollbackResponse.Content.ReadFromJsonAsync<RulePackImportRollbackResponse>())!;
        Assert.Equal(published.ImportId, rollback.ImportId);
    }

    private async Task SeedSampleTenantDataAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "U.S. Department of Transportation",
            "Federal transportation safety and compliance authority."));
        var body = (await (await _complianceCoreClient.SendAsync(bodyRequest)).Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "us_federal",
            "United States Federal",
            "Federal jurisdiction."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "fmcsa_safety",
            "FMCSA Safety Compliance",
            "Federal motor carrier safety compliance program."));
        await _complianceCoreClient.SendAsync(programRequest);

        var keyRequest = Authorized(HttpMethod.Post, "/api/compliance-keys", adminToken);
        keyRequest.Content = JsonContent.Create(new CreateComplianceKeyRequest(
            "vehicle_inspection",
            "Vehicle Inspection",
            "compliance_domain",
            "Inspection requirement domain."));
        await _complianceCoreClient.SendAsync(keyRequest);
    }

    private static MultipartFormDataContent BuildExpandedRuleFactRequirementBundle(string valueType)
    {
        const string ruleContent = """{"schemaVersion":1,"logic":"all","rules":[{"ruleKey":"dq_application_present","label":"DQ application present","type":"fact_boolean","factKey":"t49_dq_application_present","expectedValue":true,"nonWaivable":false}]}""";
        var form = new MultipartFormDataContent();
        AddCsv(form, CsvBundleFiles.ControlledVocabulary, "term_key,vocabulary_type_key,label,description,active\n");
        AddCsv(form, CsvBundleFiles.VocabularyAliases, "term_key,alias_text,active\n");
        AddCsv(form, CsvBundleFiles.ComplianceKeys, "key,label,category,description,active\n");
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
            + $"req_t49_dq_application_present_t49_391_21,t49_dq_application_present,driver_qualification,1,t49_391_21,1,motor_carrier_driver,StaffArr,driver,driver_qualification_application,{valueType},equals,true,product_record,driver_qualification_application,49_cfr_391_51,Is the driver qualification application present?,major,false,true,compliance.override.title49,true,DQ application present,Driver qualification application is present,true,true\n");
        AddCsv(form, CsvBundleFiles.RegulatoryMappings, "mapping_key,target_kind,program_key,pack_key,pack_version,citation_key,compliance_key,material_key,fact_key,label,description,active\n");
        AddCsv(form, CsvBundleFiles.SdsReferences, "sds_key,material_key,product_name,manufacturer,document_url,revision_date,active\n");
        AddCsv(form, CsvBundleFiles.ExceptionExemptions, "key,label,type,governing_body,program_key,pack_key,citation_key,applicability_key,applies_to_subject_kind,applies_to_source_product,applies_to_source_entity,effect_type,condition_logic_json,required_evidence_option_group_key,issuing_authority,authorization_number,effective_at,expires_at,active,description\n");
        return form;
    }

    private static void AddCsv(MultipartFormDataContent form, string fileName, string content) =>
        form.Add(new StringContent(content, Encoding.UTF8, "text/csv"), "file", fileName);

    private string CreateComplianceCoreAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        bool isPlatformAdmin = false)
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
            isPlatformAdmin: isPlatformAdmin);

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
