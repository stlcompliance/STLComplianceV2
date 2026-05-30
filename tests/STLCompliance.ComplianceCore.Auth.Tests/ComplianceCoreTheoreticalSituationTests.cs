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

public sealed class ComplianceCoreTheoreticalSituationTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var dbName = $"ComplianceCoreTse-{Guid.NewGuid():N}";
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
        await SeedProgramsAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Create_situation_does_not_require_freetext_or_rule_pack_selection()
    {
        var token = CreateToken("compliance_reviewer");
        var situation = await CreateSituationAsync(token, "driver_dispatch_readiness");

        Assert.Equal("driver_dispatch_readiness", situation.SituationKind);
        Assert.Equal("Driver dispatch readiness", situation.Title);

        var fieldsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/theoretical-situations/options/context-fields?situationKind=driver_dispatch_readiness", token));
        fieldsResponse.EnsureSuccessStatusCode();
        var fields = (await fieldsResponse.Content.ReadFromJsonAsync<IReadOnlyList<TheoreticalContextFieldResponse>>())!;
        Assert.DoesNotContain(fields, field => field.ContextKey.Contains("rule_pack", StringComparison.OrdinalIgnoreCase));

        var stateResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/theoretical-situations/options/evidence-states", token));
        stateResponse.EnsureSuccessStatusCode();
        var states = (await stateResponse.Content.ReadFromJsonAsync<IReadOnlyList<TheoreticalOptionResponse>>())!;
        Assert.Contains(states, state => state.Key == "special_permit_valid");
    }

    [Fact]
    public async Task Resolve_applicability_across_active_rules_and_collapses_unrelated_edge_cases()
    {
        await SeedDriverRuleAsync("driver_med_current", "driver medical certificate current", "driver", "StaffArr", automaticFailure: true);
        await SeedDriverRuleAsync("msha_mine_driver_training", "MSHA mining driver training", "mine_driver", "StaffArr", packKey: "msha_mining", programKey: "msha_safety");
        await SeedDriverRuleAsync("hazmat_employee_training", "hazmat employee training", "hazmat_employee", "TrainArr", packKey: "hazmat_training", programKey: "phmsa_hazmat");

        var token = CreateToken("compliance_reviewer");
        var situation = await CreateSituationAsync(token, "driver_dispatch_readiness");
        await SetDriverContextAsync(token, situation.SituationId, hazmat: "no", mining: "no");

        var results = await ResolveApplicabilityAsync(token, situation.SituationId);

        Assert.Contains(results, result => result.PackKey == "fmcsa_driver" && result.ApplicabilityBand == "primary");
        Assert.Contains(results, result => result.PackKey == "msha_mining" && result.EdgeCase);
        Assert.DoesNotContain(results, result => result.PackKey == "msha_mining" && result.ApplicabilityBand == "primary");
    }

    [Fact]
    public async Task Evaluates_valid_missing_expired_and_invalid_evidence_states()
    {
        await SeedDriverRuleAsync("medical_current", "medical certificate current", "driver", "StaffArr", operatorName: "current", automaticFailure: true, overrideAllowed: false);
        await SeedDriverRuleAsync("annual_mvr_review", "annual MVR review", "driver", "StaffArr");
        await SeedDriverRuleAsync("road_test_valid", "road test certificate valid", "driver", "StaffArr");

        var token = CreateToken("compliance_reviewer");
        var situation = await CreateSituationAsync(token, "driver_dispatch_readiness");
        await SetDriverContextAsync(token, situation.SituationId);
        await ResolveApplicabilityAsync(token, situation.SituationId);
        await SetFactsAsync(token, situation.SituationId,
            Fact("medical_current", "expired"),
            Fact("annual_mvr_review", "missing"),
            Fact("road_test_valid", "invalid"));

        var evaluation = await EvaluateAsync(token, situation.SituationId);

        Assert.Equal("blocked", evaluation.Result);
        Assert.Contains(evaluation.Details, detail => detail.FactKey == "medical_current" && detail.Result == "blocked");
        Assert.Contains(evaluation.Details, detail => detail.SimulatedState == "missing");
        Assert.Contains(evaluation.Details, detail => detail.SimulatedState == "invalid");
    }

    [Fact]
    public async Task Evaluates_any_of_all_of_and_one_of_evidence_paths()
    {
        await SeedDriverRuleAsync("road_test_satisfied", "road test requirement", "driver", "StaffArr");
        await SeedEvidenceGroupAsync("req_road_test_satisfied", "road_test_satisfied", "any_of", "road_test_certificate", "cdl_equivalent");
        await SeedDriverRuleAsync("prior_employer_bundle", "prior employer inquiry", "driver", "StaffArr");
        await SeedEvidenceGroupAsync("req_prior_employer_bundle", "prior_employer_bundle", "all_of", "request", "response", "review_signoff");
        await SeedDriverRuleAsync("classification_source", "classification source", "driver", "StaffArr");
        await SeedEvidenceGroupAsync("req_classification_source", "classification_source", "one_of", "product_master", "shipper_certified");

        var token = CreateToken("compliance_reviewer");
        var situation = await CreateSituationAsync(token, "driver_qualification");
        await SetDriverContextAsync(token, situation.SituationId);
        await ResolveApplicabilityAsync(token, situation.SituationId);
        await SetFactsAsync(token, situation.SituationId,
            Fact("road_test_satisfied", "alternate_evidence", requirementKey: "req_road_test_satisfied", optionKey: "cdl_equivalent"),
            Fact("prior_employer_bundle", "valid", requirementKey: "req_prior_employer_bundle", optionKey: "request"),
            Fact("prior_employer_bundle", "valid", requirementKey: "req_prior_employer_bundle", optionKey: "response"),
            Fact("prior_employer_bundle", "valid", requirementKey: "req_prior_employer_bundle", optionKey: "review_signoff"),
            Fact("classification_source", "valid", requirementKey: "req_classification_source", optionKey: "product_master"));

        var evaluation = await EvaluateAsync(token, situation.SituationId);

        Assert.Equal("compliant", evaluation.Result);
        Assert.Contains(evaluation.Details, detail => detail.RequirementKey == "req_road_test_satisfied" && detail.Result == "compliant");
        Assert.Contains(evaluation.Details, detail => detail.RequirementKey == "req_prior_employer_bundle" && detail.Result == "compliant");
        Assert.Contains(evaluation.Details, detail => detail.RequirementKey == "req_classification_source" && detail.Result == "compliant");
    }

    [Fact]
    public async Task Evaluates_derived_rollups_and_incident_triggered_requirements()
    {
        await SeedDriverRuleAsync("component_a", "component A", "driver", "StaffArr");
        await SeedDriverRuleAsync("component_b", "component B", "driver", "StaffArr");
        await SeedDriverRuleAsync("dq_rollup", "Driver qualification file rollup", "driver", "StaffArr", evidenceKind: "derived_fact", operatorName: "all_true", expectedValue: "component_a,component_b");
        await SeedDriverRuleAsync("post_accident_test_required", "post accident testing required after accident", "incident", "RoutArr", packKey: "fmcsa_incident", automaticFailure: true);

        var token = CreateToken("compliance_reviewer");
        var situation = await CreateSituationAsync(token, "accident_post_accident_testing");
        await SetDriverContextAsync(token, situation.SituationId);
        await SetIncidentAsync(token, situation.SituationId, "accident");
        await ResolveApplicabilityAsync(token, situation.SituationId);
        await SetFactsAsync(token, situation.SituationId,
            Fact("component_a", "valid"),
            Fact("component_b", "valid"),
            Fact("dq_rollup", "derived"),
            Fact("post_accident_test_required", "missing"));

        var evaluation = await EvaluateAsync(token, situation.SituationId);

        Assert.Contains(evaluation.Details, detail => detail.FactKey == "dq_rollup" && detail.Result == "compliant");
        Assert.Contains(evaluation.Details, detail => detail.FactKey == "post_accident_test_required" && detail.Result == "blocked");
    }

    [Fact]
    public async Task Override_requested_returns_allowed_or_blocked_by_requirement_metadata()
    {
        await SeedDriverRuleAsync("dispatch_override_allowed", "dispatch warning override", "driver", "StaffArr", overrideAllowed: true);
        await SeedDriverRuleAsync("medical_override_blocked", "medical non waivable", "driver", "StaffArr", overrideAllowed: false, automaticFailure: true);

        var token = CreateToken("compliance_reviewer");
        var situation = await CreateSituationAsync(token, "driver_dispatch_readiness");
        await SetDriverContextAsync(token, situation.SituationId);
        await ResolveApplicabilityAsync(token, situation.SituationId);
        await SetFactsAsync(token, situation.SituationId,
            Fact("dispatch_override_allowed", "override_requested"),
            Fact("medical_override_blocked", "override_requested"));

        var evaluation = await EvaluateAsync(token, situation.SituationId);

        Assert.Equal("blocked", evaluation.Result);
        Assert.Contains(evaluation.Details, detail => detail.FactKey == "dispatch_override_allowed" && detail.Result == "allowed_with_override");
        Assert.Contains(evaluation.Details, detail => detail.FactKey == "medical_override_blocked" && detail.Result == "override_not_allowed");
    }

    [Fact]
    public async Task Evaluates_exception_exemption_special_permit_and_alternate_path_logic()
    {
        await SeedDriverRuleAsync("road_test_required", "road test required", "driver", "StaffArr");
        await SeedExceptionExemptionAsync(
            "cdl_equivalent_path",
            "CDL equivalent accepted path",
            "alternate_compliance_path",
            "allows_alternate_evidence",
            "fmcsa_driver",
            "cit_road_test_required");
        await SeedDriverRuleAsync("medical_variance_current", "medical variance current", "driver", "StaffArr", automaticFailure: true, overrideAllowed: false);
        await SeedExceptionExemptionAsync(
            "medical_variance",
            "Medical variance",
            "variance",
            "authorizes_otherwise_blocked_action",
            "fmcsa_driver",
            "cit_medical_variance_current");
        await SeedDriverRuleAsync("hazmat_permit_scope", "hazmat permit scope", "driver", "StaffArr");

        var token = CreateToken("compliance_reviewer");
        var situation = await CreateSituationAsync(token, "driver_dispatch_readiness");
        await SetDriverContextAsync(token, situation.SituationId);
        await ResolveApplicabilityAsync(token, situation.SituationId);
        await SetFactsAsync(token, situation.SituationId,
            Fact("road_test_required", "alternate_compliance_path_selected", value: "cdl_equivalent_path"),
            Fact("medical_variance_current", "exemption_missing_proof", value: "medical_variance"),
            Fact("hazmat_permit_scope", "special_permit_outside_scope", value: "hazmat_route_permit"));

        var evaluation = await EvaluateAsync(token, situation.SituationId);

        Assert.Equal("blocked", evaluation.Result);
        var alternate = Assert.Single(evaluation.Details, detail => detail.FactKey == "road_test_required");
        Assert.Equal("compliant", alternate.Result);
        Assert.True(alternate.ExceptionExemptionConsidered);
        Assert.True(alternate.ExceptionExemptionApplies);
        Assert.Equal("not_compliant", alternate.NormalRuleResult);
        Assert.Contains("Normal rule result", alternate.Explanation);

        Assert.Contains(evaluation.Details, detail =>
            detail.FactKey == "medical_variance_current" &&
            detail.Result == "blocked" &&
            detail.ExceptionExemptionProofValid == false);
        Assert.Contains(evaluation.Details, detail =>
            detail.FactKey == "hazmat_permit_scope" &&
            detail.Result == "not_compliant" &&
            detail.ExceptionExemptionApplies == false);
    }

    [Fact]
    public async Task Unknown_required_facts_return_insufficient_information()
    {
        await SeedDriverRuleAsync("annual_mvr_review", "annual MVR review", "driver", "StaffArr");

        var token = CreateToken("compliance_reviewer");
        var situation = await CreateSituationAsync(token, "driver_qualification");
        await SetDriverContextAsync(token, situation.SituationId);
        await ResolveApplicabilityAsync(token, situation.SituationId);
        await SetFactsAsync(token, situation.SituationId, Fact("annual_mvr_review", "unknown"));

        var evaluation = await EvaluateAsync(token, situation.SituationId);

        Assert.Equal("insufficient_information", evaluation.Result);
    }

    [Fact]
    public async Task Evaluation_is_sandboxed_and_does_not_create_product_or_real_evidence_records()
    {
        await SeedDriverRuleAsync("driver_application_present", "driver application present", "driver", "StaffArr");
        var token = CreateToken("compliance_reviewer");
        var situation = await CreateSituationAsync(token, "driver_qualification");
        await SetDriverContextAsync(token, situation.SituationId);
        await ResolveApplicabilityAsync(token, situation.SituationId);
        await SetFactsAsync(token, situation.SituationId, Fact("driver_application_present", "valid"));
        await EvaluateAsync(token, situation.SituationId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        Assert.False(await db.FactAssertions.AnyAsync());
        Assert.False(await db.EvidenceReferences.AnyAsync());
        Assert.True(await db.TheoreticalSituationEvaluations.AnyAsync());
    }

    [Fact]
    public async Task Tenant_isolation_and_permission_enforcement_apply()
    {
        await SeedDriverRuleAsync("driver_application_present", "driver application present", "driver", "StaffArr");
        var reviewerToken = CreateToken("compliance_reviewer");
        var memberToken = CreateToken("tenant_member");
        var situation = await CreateSituationAsync(reviewerToken, "driver_qualification");

        var evaluateRequest = Authorized(HttpMethod.Post, $"/api/v1/theoretical-situations/{situation.SituationId}/evaluate", memberToken);
        evaluateRequest.Content = JsonContent.Create(new TheoreticalEvaluateRequest());
        var forbidden = await _client.SendAsync(evaluateRequest);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        db.TheoreticalSituations.Add(new TheoreticalSituation
        {
            SituationId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            CreatedByPersonId = Guid.NewGuid(),
            Title = "Other tenant",
            SituationKind = "driver_qualification",
            Status = "draft",
            EvaluationMode = "what_if",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var listResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/theoretical-situations", reviewerToken));
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TheoreticalSituationListItemResponse>>())!;
        Assert.Single(list);
    }

    private async Task<TheoreticalSituationResponse> CreateSituationAsync(string token, string situationKind)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/theoretical-situations", token);
        request.Content = JsonContent.Create(new CreateTheoreticalSituationRequest(situationKind));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TheoreticalSituationResponse>())!;
    }

    private async Task SetDriverContextAsync(string token, Guid situationId, string hazmat = "unknown", string mining = "unknown")
    {
        var request = Authorized(HttpMethod.Post, $"/api/v1/theoretical-situations/{situationId}/context", token);
        request.Content = JsonContent.Create(new TheoreticalSituationContextRequest([
            new("commercial_motor_vehicle_operation", "yes"),
            new("operation_scope", "interstate"),
            new("operation_mode", "highway_motor_carrier"),
            new("question_type", "dispatch"),
            new("hazmat_involved", hazmat),
            new("mining_site_work", mining)
        ]));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task SetIncidentAsync(string token, Guid situationId, string incidentType)
    {
        var request = Authorized(HttpMethod.Post, $"/api/v1/theoretical-situations/{situationId}/incidents", token);
        request.Content = JsonContent.Create(new TheoreticalSituationIncidentRequest([
            new(incidentType, "major", "driver", "active", "post_accident_testing", "yes", "unknown", "open")
        ]));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task SetFactsAsync(string token, Guid situationId, params TheoreticalSituationFactValueRequest[] facts)
    {
        var request = Authorized(HttpMethod.Post, $"/api/v1/theoretical-situations/{situationId}/facts", token);
        request.Content = JsonContent.Create(new TheoreticalSituationFactRequest(facts));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static TheoreticalSituationFactValueRequest Fact(
        string factKey,
        string state,
        string? requirementKey = null,
        string? optionKey = null,
        string? value = null) =>
        new(factKey, state, requirementKey, SimulatedValue: value, EvidenceOptionKey: optionKey);

    private async Task<IReadOnlyList<TheoreticalApplicabilityResultResponse>> ResolveApplicabilityAsync(string token, Guid situationId)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, $"/api/v1/theoretical-situations/{situationId}/resolve-applicability", token));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IReadOnlyList<TheoreticalApplicabilityResultResponse>>())!;
    }

    private async Task<TheoreticalSituationEvaluationResponse> EvaluateAsync(string token, Guid situationId)
    {
        var request = Authorized(HttpMethod.Post, $"/api/v1/theoretical-situations/{situationId}/evaluate", token);
        request.Content = JsonContent.Create(new TheoreticalEvaluateRequest());
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TheoreticalSituationEvaluationResponse>())!;
    }

    private async Task SeedEvidenceGroupAsync(string requirementKey, string factKey, string logicType, params string[] optionKeys)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var group = new ComplianceEvidenceOptionGroup
        {
            EvidenceOptionGroupId = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            RequirementKey = requirementKey,
            FactKey = factKey,
            PackKey = "fmcsa_driver",
            CitationKey = $"cit_{factKey}",
            LogicType = logicType,
            Label = $"{factKey} options",
            Description = "Seeded options.",
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.ComplianceEvidenceOptionGroups.Add(group);
        for (var index = 0; index < optionKeys.Length; index++)
        {
            db.ComplianceEvidenceOptions.Add(new ComplianceEvidenceOption
            {
                EvidenceOptionId = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                EvidenceOptionGroupId = group.EvidenceOptionGroupId,
                OptionKey = optionKeys[index],
                OptionLabel = optionKeys[index].Replace('_', ' '),
                EvidenceKind = "document_record",
                TargetKind = "document_type",
                SourceProduct = "StaffArr",
                SourceEntity = "driver",
                SourceFieldOrRecordType = optionKeys[index],
                DocumentTypeKey = optionKeys[index],
                Required = true,
                Priority = index + 1,
                Active = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    private async Task SeedDriverRuleAsync(
        string factKey,
        string label,
        string sourceEntity,
        string sourceProduct,
        string packKey = "fmcsa_driver",
        string programKey = "fmcsa_safety",
        string evidenceKind = "product_record",
        string operatorName = "equals",
        string expectedValue = "true",
        bool automaticFailure = false,
        bool overrideAllowed = true)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var program = await db.RegulatoryPrograms.SingleAsync(x => x.ProgramKey == programKey);
        var pack = await db.RulePacks.FirstOrDefaultAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.PackKey == packKey);
        if (pack is null)
        {
            pack = new RulePack
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = program.Id,
                PackKey = packKey,
                Label = packKey.Replace('_', ' '),
                Description = "Seeded TSE rule pack.",
                Status = RulePackStatuses.Published,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            db.RulePacks.Add(pack);
        }

        var citation = new RegulatoryCitation
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            RegulatoryProgramId = program.Id,
            RulePackId = pack.Id,
            CitationKey = $"cit_{factKey}",
            Label = label,
            SourceReference = "TSE test citation",
            Description = label,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var definition = new FactDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            FactKey = factKey,
            Label = label,
            Description = label,
            ValueType = FactValueTypes.Boolean,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.RegulatoryCitations.Add(citation);
        db.FactDefinitions.Add(definition);
        db.FactRequirements.Add(new FactRequirement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            FactDefinitionId = definition.Id,
            RulePackId = pack.Id,
            CitationId = citation.Id,
            RequirementKey = $"req_{factKey}",
            Label = label,
            Description = label,
            ApplicabilityKey = "motor_carrier_driver",
            SourceProduct = sourceProduct,
            SourceEntity = sourceEntity,
            SourceFieldOrRecordType = factKey,
            ValueType = FactValueTypes.Boolean,
            Operator = operatorName,
            ExpectedValue = expectedValue,
            EvidenceKind = evidenceKind,
            RequiredDocumentType = string.Empty,
            RetentionPeriod = "test",
            AuditQuestion = $"Is {label} satisfied?",
            FailureSeverity = automaticFailure ? "automatic_failure" : "major",
            AutomaticFailureFlag = automaticFailure,
            OverrideAllowed = overrideAllowed,
            OverridePermission = "compliancecore.simulation.evaluate",
            RemediationRequired = true,
            ExternallyAssertable = false,
            IsRequired = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedExceptionExemptionAsync(
        string key,
        string label,
        string type,
        string effectType,
        string packKey,
        string citationKey)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        db.ComplianceExceptionExemptions.Add(new ComplianceExceptionExemption
        {
            ExceptionExemptionId = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            Key = key,
            Label = label,
            Type = type,
            GoverningBody = "FMCSA",
            ProgramKey = "fmcsa_safety",
            PackKey = packKey,
            CitationKey = citationKey,
            ApplicabilityKey = "motor_carrier_driver",
            AppliesToSubjectKind = "driver",
            AppliesToSourceProduct = "StaffArr",
            AppliesToSourceEntity = "driver",
            EffectType = effectType,
            ConditionLogicJson = "{}",
            IssuingAuthority = "FMCSA",
            AuthorizationNumber = key,
            EffectiveAt = DateTimeOffset.UtcNow.AddDays(-1),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            Active = true,
            Description = "Seeded legal relief.",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedProgramsAsync(ComplianceCoreDbContext db)
    {
        var body = new GoverningBody
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            BodyKey = "dot",
            Label = "U.S. Department of Transportation",
            Description = "Federal transportation authority.",
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
        foreach (var programKey in new[] { "fmcsa_safety", "msha_safety", "phmsa_hazmat" })
        {
            db.RegulatoryPrograms.Add(new RegulatoryProgram
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                JurisdictionId = jurisdiction.Id,
                ProgramKey = programKey,
                Label = programKey.Replace('_', ' '),
                Description = "Seeded TSE program.",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

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
