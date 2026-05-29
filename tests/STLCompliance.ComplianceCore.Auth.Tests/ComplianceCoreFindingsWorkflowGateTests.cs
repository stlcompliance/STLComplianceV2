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

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreFindingsWorkflowGateTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _trainarrGateToken = null!;
    private string _trainarrProductGateToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreFindings-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrFindings-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexarrDbName));
            });
        });

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(complianceDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        _complianceCoreClient = _complianceCoreFactory.CreateClient();

        await SeedNexArrAsync();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();

        var adminToken = await LoginNexArrAdminAsync();
        _trainarrGateToken = await IssueServiceTokenAsync(
            adminToken,
            sourceProduct: "trainarr",
            allowedProducts: ["compliancecore"],
            WorkflowGateService.CheckActionScope);
        _trainarrProductGateToken = await IssueServiceTokenAsync(
            adminToken,
            sourceProduct: "trainarr",
            allowedProducts: ["compliancecore"],
            ProductGateEvaluationService.EvaluateActionScope);

        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedDriverQualificationRulePackAsync(complianceAdminToken);
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Evaluate_with_emit_findings_creates_findings_for_failed_rules()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);

        var evaluateRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(
            new Dictionary<string, bool> { ["driver_license_valid"] = false, ["medical_cert_on_file"] = true },
            EmitFindings: true));
        var evaluateResponse = await _complianceCoreClient.SendAsync(evaluateRequest);
        evaluateResponse.EnsureSuccessStatusCode();
        var evaluation = (await evaluateResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("fail", evaluation.OverallResult);
        Assert.NotEmpty(evaluation.FindingsEmitted);
        Assert.All(evaluation.FindingsEmitted, finding => Assert.Equal("block", finding.Severity));

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/findings?evaluationRunId={evaluation.EvaluationRunId}",
                adminToken));
        listResponse.EnsureSuccessStatusCode();
        var findings = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<ComplianceFindingResponse>>())!;
        Assert.NotEmpty(findings);
    }

    [Fact]
    public async Task Workflow_gate_check_blocks_when_rules_fail()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await CreateWorkflowGateAsync(adminToken, rulePackId, "driver_assignment");

        var checkRequest = Authorized(HttpMethod.Post, "/api/workflow-gates/check", adminToken);
        checkRequest.Content = JsonContent.Create(new WorkflowGateCheckRequest(
            "driver_assignment",
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = false,
                ["medical_cert_on_file"] = true,
            },
            null,
            EmitFindings: true));
        var checkResponse = await _complianceCoreClient.SendAsync(checkRequest);
        checkResponse.EnsureSuccessStatusCode();
        var result = (await checkResponse.Content.ReadFromJsonAsync<WorkflowGateCheckResponse>())!;
        Assert.Equal(ComplianceEvaluationOutcomes.Block, result.Outcome);
        Assert.Equal("rule_evaluation_failed", result.ReasonCode);
        Assert.NotEmpty(result.Reasons);
        Assert.NotEmpty(result.FindingsEmitted);
    }

    [Fact]
    public async Task Workflow_gate_check_allows_when_all_rules_pass()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await CreateWorkflowGateAsync(adminToken, rulePackId, "driver_clearance");

        var checkRequest = Authorized(HttpMethod.Post, "/api/workflow-gates/check", adminToken);
        checkRequest.Content = JsonContent.Create(new WorkflowGateCheckRequest(
            "driver_clearance",
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = true,
                ["medical_cert_on_file"] = true,
            },
            null));
        var checkResponse = await _complianceCoreClient.SendAsync(checkRequest);
        checkResponse.EnsureSuccessStatusCode();
        var result = (await checkResponse.Content.ReadFromJsonAsync<WorkflowGateCheckResponse>())!;
        Assert.Equal(ComplianceEvaluationOutcomes.Allow, result.Outcome);
        Assert.Empty(result.FindingsEmitted);
    }

    [Fact]
    public async Task Internal_workflow_gate_check_requires_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/workflow-gate-check");
        request.Content = JsonContent.Create(new InternalWorkflowGateCheckRequest(
            PlatformSeeder.DemoTenantId,
            "driver_assignment",
            null));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Internal_workflow_gate_check_warns_on_unresolved_facts()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await CreateWorkflowGateAsync(adminToken, rulePackId, "driver_route_gate");

        var request = ServiceAuthorized(HttpMethod.Post, "/api/internal/workflow-gate-check", _trainarrGateToken);
        request.Content = JsonContent.Create(new InternalWorkflowGateCheckRequest(
            PlatformSeeder.DemoTenantId,
            "driver_route_gate",
            new Dictionary<string, string> { ["personId"] = Guid.NewGuid().ToString() }));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<WorkflowGateCheckResponse>())!;
        Assert.Equal(ComplianceEvaluationOutcomes.Warn, result.Outcome);
        Assert.Equal("facts_unresolved", result.ReasonCode);
    }

    [Fact]
    public async Task Workflow_gate_batch_check_returns_per_gate_results_and_summary()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await CreateWorkflowGateAsync(adminToken, rulePackId, "batch_gate_allow");
        await CreateWorkflowGateAsync(adminToken, rulePackId, "batch_gate_block");

        var request = Authorized(HttpMethod.Post, "/api/workflow-gates/check/batch", adminToken);
        request.Content = JsonContent.Create(new WorkflowGateBatchCheckRequest(
            [
                new WorkflowGateBatchCheckItem(
                    "batch_gate_allow",
                    new Dictionary<string, bool>
                    {
                        ["driver_license_valid"] = true,
                        ["medical_cert_on_file"] = true,
                    }),
                new WorkflowGateBatchCheckItem(
                    "batch_gate_block",
                    new Dictionary<string, bool>
                    {
                        ["driver_license_valid"] = false,
                        ["medical_cert_on_file"] = true,
                    }),
            ]));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var batch = (await response.Content.ReadFromJsonAsync<WorkflowGateBatchCheckResponse>())!;
        Assert.Equal(2, batch.Summary.Total);
        Assert.Equal(1, batch.Summary.AllowCount);
        Assert.Equal(1, batch.Summary.BlockCount);
        Assert.Equal(2, batch.Results.Count);
        Assert.Contains(batch.Results, result => result.GateKey == "batch_gate_allow" && result.Outcome == ComplianceEvaluationOutcomes.Allow);
        Assert.Contains(batch.Results, result => result.GateKey == "batch_gate_block" && result.Outcome == ComplianceEvaluationOutcomes.Block);
    }

    [Fact]
    public async Task Workflow_gate_batch_check_allows_when_shared_facts_pass()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await CreateWorkflowGateAsync(adminToken, rulePackId, "batch_all_clear_a");
        await CreateWorkflowGateAsync(adminToken, rulePackId, "batch_all_clear_b");

        var request = Authorized(HttpMethod.Post, "/api/workflow-gates/check/batch", adminToken);
        request.Content = JsonContent.Create(new WorkflowGateBatchCheckRequest(
            [
                new WorkflowGateBatchCheckItem("batch_all_clear_a"),
                new WorkflowGateBatchCheckItem("batch_all_clear_b"),
            ],
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = true,
                ["medical_cert_on_file"] = true,
            }));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var batch = (await response.Content.ReadFromJsonAsync<WorkflowGateBatchCheckResponse>())!;
        Assert.Equal(2, batch.Summary.AllowCount);
        Assert.Equal(0, batch.Summary.BlockCount);
        Assert.All(batch.Results, result => Assert.Equal(ComplianceEvaluationOutcomes.Allow, result.Outcome));
    }

    [Fact]
    public async Task Workflow_gate_batch_check_rejects_empty_items()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var request = Authorized(HttpMethod.Post, "/api/workflow-gates/check/batch", adminToken);
        request.Content = JsonContent.Create(new WorkflowGateBatchCheckRequest([]));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Internal_workflow_gate_batch_check_requires_service_token()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/workflow-gate-check/batch");
        request.Content = JsonContent.Create(new InternalWorkflowGateBatchCheckRequest(
            PlatformSeeder.DemoTenantId,
            [new InternalWorkflowGateBatchCheckItem("driver_assignment")]));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Internal_workflow_gate_batch_check_warns_on_unresolved_facts()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await CreateWorkflowGateAsync(adminToken, rulePackId, "batch_internal_warn_a");
        await CreateWorkflowGateAsync(adminToken, rulePackId, "batch_internal_warn_b");

        var request = ServiceAuthorized(HttpMethod.Post, "/api/internal/workflow-gate-check/batch", _trainarrGateToken);
        request.Content = JsonContent.Create(new InternalWorkflowGateBatchCheckRequest(
            PlatformSeeder.DemoTenantId,
            [
                new InternalWorkflowGateBatchCheckItem("batch_internal_warn_a"),
                new InternalWorkflowGateBatchCheckItem("batch_internal_warn_b"),
            ],
            new Dictionary<string, string> { ["personId"] = Guid.NewGuid().ToString() }));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var batch = (await response.Content.ReadFromJsonAsync<WorkflowGateBatchCheckResponse>())!;
        Assert.Equal(2, batch.Summary.WarnCount);
        Assert.All(batch.Results, result => Assert.Equal(ComplianceEvaluationOutcomes.Warn, result.Outcome));
    }

    [Fact]
    public async Task Product_gate_evaluate_returns_audit_ready_trace_metadata()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePack = await GetDriverQualificationRulePackAsync(adminToken);
        await CreateWorkflowGateAsync(adminToken, rulePack.RulePackId, "can_assign_person_to_task");
        var citation = await CreateCitationAsync(adminToken, rulePack, "driver_gate_citation");
        var medicalFactId = await GetFactDefinitionIdAsync(adminToken, "medical_cert_on_file");
        await CreateFactRequirementAsync(
            adminToken,
            medicalFactId,
            rulePack.RulePackId,
            citation.CitationId,
            "medical_cert_gate_evidence");

        var now = DateTimeOffset.UtcNow;
        var personId = Guid.NewGuid();
        var request = ServiceAuthorized(HttpMethod.Post, "/api/v1/gates/evaluate", _trainarrProductGateToken);
        request.Content = JsonContent.Create(new ProductGateEvaluationRequest(
            PlatformSeeder.DemoTenantId,
            "CAN_ASSIGN_PERSON_TO_TASK",
            "can_assign_person_to_task",
            "driver_assignment",
            [new ProductGateSubjectReference("person", personId.ToString(), "staffarr", "Driver")],
            new Dictionary<string, string>
            {
                ["medical_cert_on_file"] = "false",
            },
            [new ProductGateFactSnapshotReference(
                "medical_cert_on_file",
                "staffarr:cert:expired",
                now.AddHours(-3),
                now.AddHours(-1))],
            EmitFindings: true));

        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<ProductGateEvaluationResponse>())!;

        Assert.Equal(ComplianceEvaluationOutcomes.Block, result.Outcome);
        Assert.Equal("can_assign_person_to_task", result.WorkflowKey);
        Assert.Equal("can_assign_person_to_task", result.ActionKey);
        Assert.Equal("driver_assignment", result.ActivityContextKey);
        Assert.Equal(result.CheckResultId, result.TraceId);
        Assert.NotEqual(Guid.Empty, result.TraceId);
        Assert.NotNull(result.RuleEvaluationRunId);
        Assert.NotNull(result.AuditExportPath);
        Assert.Contains(result.SubjectReferences, subject =>
            subject.SubjectType == "person" && subject.SubjectReference == personId.ToString());
        Assert.Contains(result.AppliedRuleVersions, rule =>
            rule.RuleKey == "med_cert"
            && rule.Result == "fail"
            && rule.RulePackVersion == rulePack.VersionNumber);
        Assert.Contains(result.CitationReferences, citationReference =>
            citationReference.CitationKey == "driver_gate_citation");
        Assert.Contains(result.EvidenceRequirements, requirement =>
            requirement.RequirementKey == "medical_cert_gate_evidence"
            && requirement.FactKey == "medical_cert_on_file"
            && requirement.CitationKey == "driver_gate_citation");
        Assert.Contains(result.StaleFacts, stale =>
            stale.FactKey == "medical_cert_on_file"
            && stale.SnapshotReference == "staffarr:cert:expired");
        Assert.Empty(result.MissingFacts);
        Assert.Contains(result.RemediationHints, hint =>
            hint.RuleKey == "med_cert" && hint.Code == "rule_failed");

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, result.AuditExportPath!, adminToken));
        exportResponse.EnsureSuccessStatusCode();
        var export = (await exportResponse.Content.ReadFromJsonAsync<RuleEvaluationAuditExportResponse>())!;
        Assert.Equal(result.RuleEvaluationRunId, export.EvaluationRun.EvaluationRunId);
        Assert.Contains(export.WorkflowGateChecks, check => check.CheckResultId == result.CheckResultId);
        Assert.NotEmpty(export.Findings);
    }

    [Fact]
    public async Task Product_gate_evaluate_requires_product_gate_scope()
    {
        var request = ServiceAuthorized(HttpMethod.Post, "/api/v1/gates/evaluate", _trainarrGateToken);
        request.Content = JsonContent.Create(new ProductGateEvaluationRequest(
            PlatformSeeder.DemoTenantId,
            "driver_assignment",
            "driver_assignment",
            "driver_assignment",
            [new ProductGateSubjectReference("person", Guid.NewGuid().ToString(), "staffarr")]));

        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Findings_manage_denies_tenant_member_create()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/findings", memberToken);
        request.Content = JsonContent.Create(new CreateComplianceFindingRequest(
            rulePackId,
            null,
            "manual_finding",
            "warn",
            null,
            null,
            "Manual",
            "Member cannot create",
            "manual"));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task CreateWorkflowGateAsync(string adminToken, Guid rulePackId, string gateKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/workflow-gates", adminToken);
        request.Content = JsonContent.Create(new CreateWorkflowGateDefinitionRequest(
            gateKey,
            $"Gate {gateKey}",
            "Workflow gate for integration tests.",
            rulePackId));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<Guid> GetDriverQualificationRulePackIdAsync(string adminToken)
    {
        var pack = await GetDriverQualificationRulePackAsync(adminToken);
        return pack.RulePackId;
    }

    private async Task<RulePackResponse> GetDriverQualificationRulePackAsync(string adminToken)
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/rule-packs", adminToken));
        response.EnsureSuccessStatusCode();
        var packs = (await response.Content.ReadFromJsonAsync<IReadOnlyList<RulePackResponse>>())!;
        return packs.First(p => p.PackKey == "driver_qualification");
    }

    private async Task<RegulatoryCitationResponse> CreateCitationAsync(
        string adminToken,
        RulePackResponse rulePack,
        string citationKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/citations", adminToken);
        request.Content = JsonContent.Create(new CreateRegulatoryCitationRequest(
            rulePack.RegulatoryProgramId,
            rulePack.RulePackId,
            citationKey,
            "Driver gate citation",
            "49 CFR 391",
            "Driver qualification citation for product gate tests.",
            null));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RegulatoryCitationResponse>())!;
    }

    private async Task<Guid> GetFactDefinitionIdAsync(string adminToken, string factKey)
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/fact-definitions", adminToken));
        response.EnsureSuccessStatusCode();
        var facts = (await response.Content.ReadFromJsonAsync<IReadOnlyList<FactDefinitionResponse>>())!;
        return facts.First(fact => fact.FactKey == factKey).FactDefinitionId;
    }

    private async Task CreateFactRequirementAsync(
        string adminToken,
        Guid factDefinitionId,
        Guid rulePackId,
        Guid citationId,
        string requirementKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-requirements", adminToken);
        request.Content = JsonContent.Create(new CreateFactRequirementRequest(
            factDefinitionId,
            rulePackId,
            citationId,
            requirementKey,
            "Medical certificate evidence",
            "Product gate must surface the medical certificate evidence requirement.",
            true));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task SeedDriverQualificationRulePackAsync(string adminToken)
    {
        var programId = await CreateSampleProgramAsync(adminToken);

        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification Rules",
            "Baseline driver qualification rule pack."));
        var createPackResponse = await _complianceCoreClient.SendAsync(createPackRequest);
        createPackResponse.EnsureSuccessStatusCode();
        var pack = (await createPackResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;

        var licenseFactId = await CreateBooleanFactDefinitionAsync(adminToken, "driver_license_valid");
        var medicalFactId = await CreateBooleanFactDefinitionAsync(adminToken, "medical_cert_on_file");

        var licenseSourceRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        licenseSourceRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            licenseFactId,
            "default_license_flag",
            "static_config",
            "Default license valid",
            "Static default for driver license validity checks.",
            null,
            null,
            """{"booleanValue":true}""",
            0));
        (await _complianceCoreClient.SendAsync(licenseSourceRequest)).EnsureSuccessStatusCode();

        var medicalSourceRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        medicalSourceRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            medicalFactId,
            "staffarr_med_cert",
            "product_api",
            "StaffArr medical certificate",
            "Resolved from StaffArr caller context until product fetch is implemented.",
            "staffarr",
            "/api/people/{personId}/certifications",
            """{"scopeKey":"tenant","fetchRelativePath":"/api/internal/compliance-facts/{factKey}"}""",
            0));
        (await _complianceCoreClient.SendAsync(medicalSourceRequest)).EnsureSuccessStatusCode();

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto("license_valid", "Valid driver license", "fact_boolean", "driver_license_valid", true),
                new RuleDefinitionDto("med_cert", "Medical certificate on file", "fact_boolean", "medical_cert_on_file", true),
            ]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateBooleanFactDefinitionAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            factKey.Replace('_', ' '),
            "Test fact for findings and workflow gates.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        return created.FactDefinitionId;
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
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
            "Federal jurisdiction for interstate transportation rules."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "fmcsa_safety",
            "FMCSA Safety Compliance",
            "Federal motor carrier safety compliance program."));
        var program = (await (await _complianceCoreClient.SendAsync(programRequest)).Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
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

    private async Task<string> LoginNexArrAdminAsync()
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
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
        var registerRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-findings-gate-{Guid.NewGuid():N}",
            $"{sourceProduct} findings gate test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens", adminToken);
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

    private static HttpRequestMessage NexArrAuthorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
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

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
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
