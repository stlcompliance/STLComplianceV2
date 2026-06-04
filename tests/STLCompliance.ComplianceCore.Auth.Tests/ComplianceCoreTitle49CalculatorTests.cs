using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreTitle49CalculatorTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private string _adminToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreTitle49Calculators-{Guid.NewGuid():N}";

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
        _adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();
        await SeedCalculatorDataAsync(db);
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
    }

    [Fact]
    public async Task Title49_calculator_summary_parses_thresholds_and_retention_durations()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/calculators/title49/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<Title49CalculatorSummaryResponse>())!;
        Assert.Equal(3, summary.TotalRequirements);
        Assert.Equal(1, summary.NumericThresholdCount);
        Assert.Equal(2, summary.RetentionDurationCount);
        Assert.Equal(1, summary.MixedCalculatorCount);
        Assert.Equal(2, summary.ReadyCount);
        Assert.Equal(1, summary.ReviewCount);
        Assert.Contains(summary.Requirements, item => item.RequirementKey == "t49_calc_threshold" && item.CalculatorKind == "mixed");
        Assert.Contains(summary.Requirements, item => item.RequirementKey == "t49_calc_retention" && item.CalculatorKind == "retention_duration");
        Assert.Contains(summary.Requirements, item => item.RequirementKey == "t49_calc_review" && item.CalculatorKind == "review");
        Assert.Contains(summary.Requirements, item => item.ParsedNumericThreshold == 1000m);
        Assert.Contains(summary.Requirements, item => item.ParsedRetentionDays == 365);

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/calculators/title49/summary/export", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("t49_calc_threshold", csv, StringComparison.Ordinal);
        Assert.Contains("mixed", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task V1_title49_calculator_alias_matches_primary_route()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/calculators/title49/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<Title49CalculatorSummaryResponse>())!;
        Assert.Equal(3, summary.TotalRequirements);
        Assert.Contains(summary.Requirements, item => item.RequirementKey == "t49_calc_threshold");
    }

    private async Task SeedCalculatorDataAsync(ComplianceCoreDbContext db)
    {
        var now = DateTimeOffset.UtcNow;
        var governingBodyId = Guid.NewGuid();
        var jurisdictionId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var rulePackId = Guid.NewGuid();
        var citationId = Guid.NewGuid();

        db.GoverningBodies.Add(new GoverningBody
        {
            Id = governingBodyId,
            TenantId = PlatformSeeder.DemoTenantId,
            BodyKey = "dot",
            Label = "U.S. Department of Transportation",
            Description = "Federal transportation safety and compliance authority.",
            IsActive = true,
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-3)
        });

        db.Jurisdictions.Add(new Jurisdiction
        {
            Id = jurisdictionId,
            TenantId = PlatformSeeder.DemoTenantId,
            GoverningBodyId = governingBodyId,
            JurisdictionKey = "us_federal",
            Label = "United States Federal",
            Description = "Federal jurisdiction.",
            IsActive = true,
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-3)
        });

        db.RegulatoryPrograms.Add(new RegulatoryProgram
        {
            Id = programId,
            TenantId = PlatformSeeder.DemoTenantId,
            JurisdictionId = jurisdictionId,
            ProgramKey = "title49_calculator_program",
            Label = "Title 49 Calculator Program",
            Description = "Program used for calculator verification.",
            IsActive = true,
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-2)
        });

        db.RulePacks.Add(new RulePack
        {
            Id = rulePackId,
            TenantId = PlatformSeeder.DemoTenantId,
            RegulatoryProgramId = programId,
            PackKey = "title49_calculator_pack",
            Label = "Title 49 Calculator Pack",
            Description = "Calculator verification rule pack.",
            VersionNumber = 1,
            Status = RulePackStatuses.Published,
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-1),
            RuleContentJson = """{"schemaVersion":1,"logic":"all","rules":[{"ruleKey":"t49_calc_threshold","label":"Threshold","type":"fact_boolean","factKey":"calc_threshold","expectedValue":true}]}"""
        });

        db.RegulatoryCitations.Add(new RegulatoryCitation
        {
            Id = citationId,
            TenantId = PlatformSeeder.DemoTenantId,
            RegulatoryProgramId = programId,
            RulePackId = rulePackId,
            CitationKey = "cfr_172_101",
            Label = "Hazardous materials table",
            SourceReference = "49 CFR 172.101",
            Description = "Calculator verification citation.",
            VersionNumber = 1,
            IsActive = true,
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now.AddDays(-1)
        });

        var thresholdFactId = Guid.NewGuid();
        var retentionFactId = Guid.NewGuid();
        var reviewFactId = Guid.NewGuid();

        db.FactDefinitions.AddRange(
            new FactDefinition
            {
                Id = thresholdFactId,
                TenantId = PlatformSeeder.DemoTenantId,
                FactKey = "calc_threshold",
                Label = "Calculation threshold",
                Description = "Numeric threshold used by the calculator report.",
                ValueType = FactValueTypes.Number,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new FactDefinition
            {
                Id = retentionFactId,
                TenantId = PlatformSeeder.DemoTenantId,
                FactKey = "calc_retention",
                Label = "Retention duration",
                Description = "Retention duration used by the calculator report.",
                ValueType = FactValueTypes.String,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new FactDefinition
            {
                Id = reviewFactId,
                TenantId = PlatformSeeder.DemoTenantId,
                FactKey = "calc_review",
                Label = "Needs review",
                Description = "Unparsed requirement that should be classified as review.",
                ValueType = FactValueTypes.String,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

        db.FactRequirements.AddRange(
            new FactRequirement
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                FactDefinitionId = thresholdFactId,
                RulePackId = rulePackId,
                CitationId = citationId,
                RequirementKey = "t49_calc_threshold",
                Label = "Threshold calculator",
                Description = "Threshold calculator requirement.",
                ApplicabilityKey = "hazmat",
                SourceProduct = "SupplyArr",
                SourceEntity = "shipments",
                SourceFieldOrRecordType = "net_weight",
                ValueType = FactValueTypes.Number,
                Operator = FactRequirementOperators.Equal,
                ExpectedValue = "1000",
                EvidenceKind = FactRequirementEvidenceKinds.SystemFact,
                RequiredDocumentType = string.Empty,
                RetentionPeriod = "365 days",
                AuditQuestion = "Is the shipment threshold met?",
                FailureSeverity = FactRequirementFailureSeverities.Major,
                AutomaticFailureFlag = false,
                OverrideAllowed = true,
                OverridePermission = "compliance.override.calculator",
                RemediationRequired = true,
                ExternallyAssertable = false,
                IsRequired = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new FactRequirement
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                FactDefinitionId = retentionFactId,
                RulePackId = rulePackId,
                CitationId = citationId,
                RequirementKey = "t49_calc_retention",
                Label = "Retention calculator",
                Description = "Retention calculator requirement.",
                ApplicabilityKey = "hazmat",
                SourceProduct = "SupplyArr",
                SourceEntity = "documents",
                SourceFieldOrRecordType = "shipping_paper",
                ValueType = FactValueTypes.String,
                Operator = FactRequirementOperators.Equal,
                ExpectedValue = "true",
                EvidenceKind = FactRequirementEvidenceKinds.DocumentRecord,
                RequiredDocumentType = "shipping_paper",
                RetentionPeriod = "7 days",
                AuditQuestion = "Is the record retained for the right period?",
                FailureSeverity = FactRequirementFailureSeverities.Major,
                AutomaticFailureFlag = false,
                OverrideAllowed = true,
                OverridePermission = "compliance.override.calculator",
                RemediationRequired = true,
                ExternallyAssertable = false,
                IsRequired = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new FactRequirement
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                FactDefinitionId = reviewFactId,
                RulePackId = rulePackId,
                CitationId = citationId,
                RequirementKey = "t49_calc_review",
                Label = "Review calculator",
                Description = "Requirement that should remain review-only.",
                ApplicabilityKey = "hazmat",
                SourceProduct = "SupplyArr",
                SourceEntity = "documents",
                SourceFieldOrRecordType = "notes",
                ValueType = FactValueTypes.String,
                Operator = FactRequirementOperators.Equal,
                ExpectedValue = "manual_review",
                EvidenceKind = FactRequirementEvidenceKinds.DocumentRecord,
                RequiredDocumentType = string.Empty,
                RetentionPeriod = string.Empty,
                AuditQuestion = "Does this need manual review?",
                FailureSeverity = FactRequirementFailureSeverities.Minor,
                AutomaticFailureFlag = false,
                OverrideAllowed = true,
                OverridePermission = "compliance.override.calculator",
                RemediationRequired = false,
                ExternallyAssertable = false,
                IsRequired = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

        await db.SaveChangesAsync();
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

