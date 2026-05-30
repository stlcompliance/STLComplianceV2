using System.Text.Json;
using ComplianceCore.Api.Csv;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreTitle49RulePackCsvTests
{
    private static readonly IReadOnlyDictionary<string, string[]> ExpectedHeaders =
        new Dictionary<string, string[]>
        {
            ["controlled_vocabulary.csv"] = ["term_key", "vocabulary_type_key", "label", "description", "active"],
            ["vocabulary_aliases.csv"] = ["term_key", "alias_text", "active"],
            ["compliance_keys.csv"] = ["key", "label", "category", "description", "active"],
            ["material_keys.csv"] = ["key", "label", "category", "description", "active"],
            ["rule_packs.csv"] =
            [
                "pack_key",
                "program_key",
                "version_number",
                "label",
                "description",
                "status",
                "active",
                "rule_content_json"
            ],
            ["rule_requirements.csv"] =
            [
                "citation_key",
                "program_key",
                "pack_key",
                "pack_version",
                "label",
                "source_reference",
                "description",
                "active",
                "supersedes_citation_key"
            ],
            ["rule_fact_requirements.csv"] =
            [
                "requirement_key",
                "fact_key",
                "pack_key",
                "pack_version",
                "citation_key",
                "citation_version",
                "applicability_key",
                "source_product",
                "source_entity",
                "source_field_or_record_type",
                "value_type",
                "operator",
                "expected_value",
                "evidence_kind",
                "required_document_type",
                "retention_period",
                "audit_question",
                "failure_severity",
                "automatic_failure_flag",
                "override_allowed",
                "override_permission",
                "remediation_required",
                "label",
                "description",
                "is_required",
                "active"
            ],
            ["regulatory_mappings.csv"] =
            [
                "mapping_key",
                "target_kind",
                "program_key",
                "pack_key",
                "pack_version",
                "citation_key",
                "compliance_key",
                "material_key",
                "fact_key",
                "label",
                "description",
                "active"
            ],
            ["sds_references.csv"] = ["sds_key", "material_key", "product_name", "manufacturer", "document_url", "revision_date", "active"],
            ["exception_exemptions.csv"] =
            [
                "key",
                "label",
                "type",
                "governing_body",
                "program_key",
                "pack_key",
                "citation_key",
                "applicability_key",
                "applies_to_subject_kind",
                "applies_to_source_product",
                "applies_to_source_entity",
                "effect_type",
                "condition_logic_json",
                "required_evidence_option_group_key",
                "issuing_authority",
                "authorization_number",
                "effective_at",
                "expires_at",
                "active",
                "description"
            ]
        };

    private static readonly HashSet<string> AllowedProducts =
        new(StringComparer.Ordinal)
        {
            "StaffArr",
            "TrainArr",
            "MaintainArr",
            "RoutArr",
            "SupplyArr",
            "ComplianceCore"
        };

    private static readonly HashSet<string> RequiredOperationalPacks =
        new(StringComparer.Ordinal)
        {
            "title49.motorcarrier.applicability",
            "title49.motorcarrier.registration_authority",
            "title49.motorcarrier.insurance_financial_responsibility",
            "title49.driver.qualification_file",
            "title49.driver.cdl_clp_endorsements",
            "title49.driver.entry_level_driver_training",
            "title49.driver.medical_qualification",
            "title49.driver.drug_alcohol_program",
            "title49.driver.hours_of_service",
            "title49.driver.eld_records",
            "title49.driver.accident_post_accident_actions",
            "title49.vehicle.parts_accessories_condition",
            "title49.vehicle.inspection_repair_maintenance",
            "title49.vehicle.dvir",
            "title49.vehicle.annual_inspection",
            "title49.vehicle.roadside_inspection_correction",
            "title49.vehicle.out_of_service_readiness",
            "title49.vehicle.cargo_securement",
            "title49.hazmat.applicability",
            "title49.hazmat.classification",
            "title49.hazmat.hazardous_materials_table",
            "title49.hazmat.shipping_papers",
            "title49.hazmat.marking",
            "title49.hazmat.labeling",
            "title49.hazmat.placarding",
            "title49.hazmat.packaging",
            "title49.hazmat.loading_unloading_segregation",
            "title49.hazmat.training",
            "title49.hazmat.security_plan",
            "title49.hazmat.incident_reporting",
            "title49.hazmat.registration",
            "title49.hazmat.special_permits_exceptions"
        };

    private static readonly HashSet<string> AllowedFactOperators =
        new(StringComparer.Ordinal)
        {
            "equals",
            "all_true",
            "exists",
            "not_empty",
            "current"
        };

    private static readonly HashSet<string> RequiredDqAtomicFacts =
        new(StringComparer.Ordinal)
        {
            "t49_dq_application_present",
            "t49_dq_mvr_initial_present",
            "t49_dq_mvr_annual_current",
            "t49_dq_medical_certificate_current",
            "t49_dq_road_test_or_equivalent_present",
            "t49_dq_prior_employer_inquiry_complete",
            "t49_dq_annual_violation_review_complete"
        };

    [Fact]
    public void Title49_rulepacks_use_current_csv_headers()
    {
        var packDirectories = GetPackDirectories();

        Assert.Equal(44, packDirectories.Count);
        foreach (var packDirectory in packDirectories)
        {
            var csvFiles = Directory.GetFiles(packDirectory, "*.csv").Select(Path.GetFileName).ToHashSet(StringComparer.Ordinal);
            Assert.Equal(
                ExpectedHeaders.Keys.OrderBy(value => value, StringComparer.Ordinal),
                csvFiles.OrderBy(value => value, StringComparer.Ordinal));

            foreach (var (fileName, expectedHeader) in ExpectedHeaders)
            {
                var firstLine = File.ReadLines(Path.Combine(packDirectory, fileName)).First();
                Assert.Equal(expectedHeader, CsvText.ParseRow(firstLine));
            }
        }
    }

    [Fact]
    public void Title49_rulepack_keys_are_unique_and_references_are_not_orphaned()
    {
        var bundle = LoadBundle();
        var packKeys = new HashSet<string>(StringComparer.Ordinal);
        var citationKeys = new HashSet<string>(StringComparer.Ordinal);
        var complianceKeys = new HashSet<string>(StringComparer.Ordinal);
        var materialKeys = new HashSet<string>(StringComparer.Ordinal);
        var factKeys = new HashSet<string>(StringComparer.Ordinal);
        var requirementKeys = new HashSet<string>(StringComparer.Ordinal);
        var mappingKeys = new HashSet<string>(StringComparer.Ordinal);
        var exceptionKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (_, files) in bundle)
        {
            foreach (var row in files["rule_packs.csv"])
            {
                Assert.True(packKeys.Add(row["pack_key"]), $"Duplicate pack_key {row["pack_key"]}");
                AssertKey(row["pack_key"], "pack_key");
            }

            foreach (var row in files["rule_requirements.csv"])
            {
                Assert.True(citationKeys.Add(row["citation_key"]), $"Duplicate citation_key {row["citation_key"]}");
                AssertKey(row["citation_key"], "citation_key");
            }

            foreach (var row in files["compliance_keys.csv"])
            {
                Assert.True(complianceKeys.Add(row["key"]), $"Duplicate compliance key {row["key"]}");
                AssertKey(row["key"], "compliance key");
            }

            foreach (var row in files["material_keys.csv"])
            {
                Assert.True(materialKeys.Add(row["key"]), $"Duplicate material key {row["key"]}");
                AssertKey(row["key"], "material key");
            }

            foreach (var row in files["rule_fact_requirements.csv"])
            {
                Assert.True(requirementKeys.Add(row["requirement_key"]), $"Duplicate requirement_key {row["requirement_key"]}");
                AssertKey(row["requirement_key"], "requirement_key");
                AssertKey(row["fact_key"], "fact_key");
                factKeys.Add(row["fact_key"]);
                Assert.False(string.IsNullOrWhiteSpace(row["applicability_key"]), $"Missing applicability_key for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["source_product"]), $"Missing source_product for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["source_entity"]), $"Missing source_entity for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["source_field_or_record_type"]), $"Missing source_field_or_record_type for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["value_type"]), $"Missing value_type for {row["fact_key"]}");
                Assert.Contains(row["operator"], AllowedFactOperators);
                Assert.False(string.IsNullOrWhiteSpace(row["expected_value"]), $"Missing expected_value for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["evidence_kind"]), $"Missing evidence_kind for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["retention_period"]), $"Missing retention_period for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["audit_question"]), $"Missing audit_question for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["failure_severity"]), $"Missing failure_severity for {row["fact_key"]}");
                Assert.Contains(row["automatic_failure_flag"], new[] { "true", "false" });
                Assert.Contains(row["override_allowed"], new[] { "true", "false" });
                Assert.Contains(row["remediation_required"], new[] { "true", "false" });
                foreach (var product in row["source_product"].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    Assert.Contains(product, AllowedProducts);
                }
            }

            foreach (var row in files["regulatory_mappings.csv"])
            {
                Assert.True(mappingKeys.Add(row["mapping_key"]), $"Duplicate mapping_key {row["mapping_key"]}");
                AssertKey(row["mapping_key"], "mapping_key");
            }

            foreach (var row in files["exception_exemptions.csv"])
            {
                Assert.True(exceptionKeys.Add(row["key"]), $"Duplicate exception/exemption key {row["key"]}");
                AssertKey(row["key"], "exception/exemption key");
                Assert.Contains(row["type"], new[]
                {
                    "regulatory_exception",
                    "regulatory_exemption",
                    "waiver",
                    "variance",
                    "special_permit",
                    "approval",
                    "alternate_compliance_path",
                    "conditional_exclusion",
                    "grandfathering",
                    "temporary_relief"
                });
                Assert.Contains(row["effect_type"], new[]
                {
                    "makes_requirement_not_applicable",
                    "changes_expected_value",
                    "changes_required_evidence",
                    "allows_alternate_evidence",
                    "reduces_requirement",
                    "extends_deadline",
                    "authorizes_otherwise_blocked_action",
                    "changes_frequency",
                    "requires_additional_conditions",
                    "reference_only"
                });
            }
        }

        Assert.True(RequiredOperationalPacks.IsSubsetOf(packKeys), "Missing one or more required operational Title 49 packs.");

        foreach (var (packDirectory, files) in bundle)
        {
            var directoryPackKey = Path.GetFileName(packDirectory);
            foreach (var row in files["rule_requirements.csv"])
            {
                Assert.Contains(row["pack_key"], packKeys);
                Assert.Equal(directoryPackKey, row["pack_key"]);
            }

            foreach (var row in files["rule_fact_requirements.csv"])
            {
                Assert.Contains(row["pack_key"], packKeys);
                if (!string.IsNullOrWhiteSpace(row["citation_key"]))
                {
                    Assert.Contains(row["citation_key"], citationKeys);
                }

                Assert.Contains("products=", row["description"], StringComparison.Ordinal);
                Assert.Contains("entities=", row["description"], StringComparison.Ordinal);
                Assert.Contains("value_type=", row["description"], StringComparison.Ordinal);
                Assert.Contains("evidence_kind=", row["description"], StringComparison.Ordinal);
            }

            foreach (var row in files["regulatory_mappings.csv"])
            {
                Assert.Contains(row["pack_key"], packKeys);
                if (!string.IsNullOrWhiteSpace(row["citation_key"]))
                {
                    Assert.Contains(row["citation_key"], citationKeys);
                }

                if (!string.IsNullOrWhiteSpace(row["compliance_key"]))
                {
                    Assert.Contains(row["compliance_key"], complianceKeys);
                }

                if (!string.IsNullOrWhiteSpace(row["material_key"]))
                {
                    Assert.Contains(row["material_key"], materialKeys);
                }

                if (!string.IsNullOrWhiteSpace(row["fact_key"]))
                {
                    Assert.Contains(row["fact_key"], factKeys);
                }
            }
        }
    }

    [Fact]
    public void Title49_operational_packs_have_rules_conditions_outcomes_and_product_metadata()
    {
        var bundle = LoadBundle();

        foreach (var packKey in RequiredOperationalPacks)
        {
            var files = bundle.Single(item => Path.GetFileName(item.Key) == packKey).Value;
            var packRow = Assert.Single(files["rule_packs.csv"]);
            var factRequirementKeys = files["rule_fact_requirements.csv"]
                .Select(row => row["fact_key"])
                .ToHashSet(StringComparer.Ordinal);

            using var content = JsonDocument.Parse(packRow["rule_content_json"]);
            var root = content.RootElement;
            Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
            Assert.Equal("all", root.GetProperty("logic").GetString());

            var rules = root.GetProperty("rules").EnumerateArray().ToList();
            var conditions = root.GetProperty("conditions").EnumerateArray().ToList();
            var outcomes = root.GetProperty("outcomes").EnumerateArray().ToList();

            Assert.NotEmpty(rules);
            Assert.Equal(rules.Count, conditions.Count);
            Assert.Equal(2, outcomes.Count);

            foreach (var rule in rules)
            {
                Assert.Equal("fact_boolean", rule.GetProperty("type").GetString());
                Assert.True(rule.GetProperty("expectedValue").GetBoolean());
                Assert.Contains(rule.GetProperty("factKey").GetString()!, factRequirementKeys);
            }

            foreach (var condition in conditions)
            {
                Assert.Equal("equals", condition.GetProperty("operator").GetString());
                Assert.True(condition.GetProperty("expectedValue").GetBoolean());
                Assert.NotEmpty(condition.GetProperty("entities").EnumerateArray());
                foreach (var product in condition.GetProperty("sourceProducts").EnumerateArray())
                {
                    Assert.Contains(product.GetString()!, AllowedProducts);
                }
            }

            Assert.Contains(outcomes, item => item.GetProperty("result").GetString() == "allow");
            Assert.Contains(outcomes, item => item.GetProperty("result").GetString() == "block");
        }
    }

    [Fact]
    public void Title49_driver_qualification_uses_atomic_audit_facts_and_derived_rollup()
    {
        var files = LoadBundle().Single(item => Path.GetFileName(item.Key) == "title49.driver.qualification_file").Value;
        var factRows = files["rule_fact_requirements.csv"];
        var factKeys = factRows.Select(row => row["fact_key"]).ToHashSet(StringComparer.Ordinal);

        Assert.True(RequiredDqAtomicFacts.IsSubsetOf(factKeys), "Driver qualification file is missing one or more atomic audit facts.");

        var rollup = Assert.Single(factRows, row => row["fact_key"] == "t49_driver_dq_file_complete");
        Assert.Equal("ComplianceCore", rollup["source_product"]);
        Assert.Equal("all_true", rollup["operator"]);
        Assert.Equal("derived_fact", rollup["evidence_kind"]);
        foreach (var atomicFact in RequiredDqAtomicFacts)
        {
            Assert.Contains(atomicFact, rollup["expected_value"], StringComparison.Ordinal);
        }

        using var content = JsonDocument.Parse(Assert.Single(files["rule_packs.csv"])["rule_content_json"]);
        var ruleFactKeys = content.RootElement.GetProperty("rules")
            .EnumerateArray()
            .Select(rule => rule.GetProperty("factKey").GetString()!)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(RequiredDqAtomicFacts.IsSubsetOf(ruleFactKeys));
        Assert.DoesNotContain("t49_driver_dq_file_complete", ruleFactKeys);
    }

    [Fact]
    public void Title49_special_permits_pack_has_first_class_exception_exemption_rows()
    {
        var files = LoadBundle().Single(item => Path.GetFileName(item.Key) == "title49.hazmat.special_permits_exceptions").Value;
        var rows = files["exception_exemptions.csv"];

        Assert.Contains(rows, row => row["type"] == "regulatory_exception" && row["key"] == "t49_hazmat_material_of_trade_exception");
        Assert.Contains(rows, row => row["type"] == "special_permit" && row["effect_type"] == "authorizes_otherwise_blocked_action");
        Assert.Contains(rows, row => row["type"] == "alternate_compliance_path" && row["effect_type"] == "allows_alternate_evidence");
    }

    [Fact]
    public void Title49_reference_packs_have_citation_coverage_and_docs_exist()
    {
        var bundle = LoadBundle();

        foreach (var (_, files) in bundle)
        {
            var packRow = Assert.Single(files["rule_packs.csv"]);
            var packKey = packRow["pack_key"];
            if (RequiredOperationalPacks.Contains(packKey) ||
                string.Equals(packKey, "title49.transportation.citation_metadata", StringComparison.Ordinal))
            {
                continue;
            }

            Assert.NotEmpty(files["rule_requirements.csv"]);
            Assert.True(string.IsNullOrWhiteSpace(packRow["rule_content_json"]), $"{packKey} should remain reference-only.");
        }

        foreach (var doc in new[]
                 {
                     "title49_coverage_report.md",
                     "title49_rulepack_index.md",
                     "title49_10_csv_alignment.md",
                     "title49_product_workflow_map.md",
                     "title49_remaining_gaps.md"
                 })
        {
            var path = Path.Combine(RepoRoot(), "docs", "compliance-core", doc);
            Assert.True(File.Exists(path), $"Missing {doc}");
            Assert.Contains("Title 49", File.ReadAllText(path), StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertKey(string value, string fieldName)
    {
        Assert.False(string.IsNullOrWhiteSpace(value), $"{fieldName} is blank.");
        Assert.InRange(value.Length, 2, 64);
    }

    private static Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> LoadBundle()
    {
        return GetPackDirectories()
            .ToDictionary(
                directory => directory,
                directory => ExpectedHeaders.Keys.ToDictionary(
                    fileName => fileName,
                    fileName => ReadRows(Path.Combine(directory, fileName)),
                    StringComparer.Ordinal),
                StringComparer.Ordinal);
    }

    private static List<string> GetPackDirectories()
    {
        var root = Path.Combine(RepoRoot(), "root", "rulepack", "title49");
        Assert.True(Directory.Exists(root), $"Missing generated Title 49 rulepack root: {root}");
        return Directory.GetDirectories(root).OrderBy(value => value, StringComparer.Ordinal).ToList();
    }

    private static List<Dictionary<string, string>> ReadRows(string path)
    {
        var lines = File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
        Assert.NotEmpty(lines);

        var headers = CsvText.ParseRow(lines[0]).ToArray();
        var rows = new List<Dictionary<string, string>>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = CsvText.ParseRow(lines[index]);
            Assert.Equal(headers.Length, fields.Count);
            rows.Add(headers
                .Select((header, fieldIndex) => (header, value: fields[fieldIndex]))
                .ToDictionary(pair => pair.header, pair => pair.value, StringComparer.Ordinal));
        }

        return rows;
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "STLCompliance.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
