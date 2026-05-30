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
        Assert.Equal(9, manifest.Files.Count);
        Assert.Contains(manifest.Files, file => file.FileName == CsvBundleFiles.ControlledVocabulary);
        Assert.Contains(manifest.Files, file => file.FileName == CsvBundleFiles.SdsReferences);
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
        Assert.Equal(9, archive.Entries.Count);
        Assert.Contains(archive.Entries, entry => entry.Name == CsvBundleFiles.ComplianceKeys);
    }

    [Fact]
    public async Task Csv_bundle_import_round_trip_upserts_keys()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
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
    public async Task Csv_bundle_import_dry_run_reports_validation_without_apply()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
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
    public async Task V1_rule_pack_import_routes_preview_validate_publish_and_followups()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
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
