using System.Text.Json;
using ComplianceCore.Api.Csv;
using ComplianceCore.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreBaselineRulePackCsvTests
{
    private static readonly IReadOnlyDictionary<string, string[]> ExpectedHeaders =
        StagedImportService.SupportedHeaders.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToArray(),
            StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> AllowedProducts =
        new(StringComparer.Ordinal)
        {
            "AssurArr",
            "ComplianceCore",
            "CustomArr",
            "FieldCompanion",
            "LedgArr",
            "LoadArr",
            "MaintainArr",
            "NexArr",
            "OrdArr",
            "RecordArr",
            "ReportArr",
            "RoutArr",
            "STLComplianceSite",
            "StaffArr",
            "SupplyArr",
            "TrainArr"
        };

    private static readonly HashSet<string> RequiredPacks =
        new(StringComparer.Ordinal)
        {
            "business.entity_authority_licensing",
            "commercial.ucc_orders_warranties",
            "communications.tsr_can_spam",
            "consumer.accessibility_disclosures",
            "electronic_records.esign_ueta",
            "employment.flsa_recordkeeping_notice",
            "employment.fmla_notice_leave",
            "epa.epcra_cercla_release_reporting",
            "epa.hazardous_waste_generator",
            "epa.spcc_oil_storage",
            "osha.hazard_communication",
            "osha.ppe_general_industry",
            "osha.recordkeeping_reporting",
            "privacy.ftc_glba_safeguards",
            "supplychain.trade_sanctions_import_product",
            "tax.statutory_financial_obligations"
        };

    [Fact]
    public void Baseline_rulepacks_use_current_csv_headers()
    {
        var packDirectories = GetPackDirectories();

        Assert.Equal(RequiredPacks.Count, packDirectories.Count);
        AssertSchemaManifest();
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

            Assert.Empty(ReadRows(Path.Combine(packDirectory, StagedImportService.EvidenceReferencesFile)));
        }
    }

    [Fact]
    public void Baseline_rulepack_keys_are_complete_and_references_are_not_orphaned()
    {
        var bundle = LoadBundle();
        var packKeys = new HashSet<string>(StringComparer.Ordinal);
        var citationKeys = new HashSet<string>(StringComparer.Ordinal);
        var complianceKeys = new HashSet<string>(StringComparer.Ordinal);
        var factKeys = new HashSet<string>(StringComparer.Ordinal);
        var requirementKeys = new HashSet<string>(StringComparer.Ordinal);
        var mappingKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (_, files) in bundle)
        {
            foreach (var row in files["rule_packs.csv"])
            {
                Assert.True(packKeys.Add(row["pack_key"]), $"Duplicate pack_key {row["pack_key"]}");
                AssertKey(row["pack_key"], "pack_key");
                Assert.Equal("published", row["status"]);
            }

            foreach (var row in files["rule_requirements.csv"])
            {
                Assert.True(citationKeys.Add(row["citation_key"]), $"Duplicate citation_key {row["citation_key"]}");
                AssertKey(row["citation_key"], "citation_key");
                Assert.False(string.IsNullOrWhiteSpace(row["source_reference"]), $"Missing source_reference for {row["citation_key"]}");
            }

            foreach (var row in files["compliance_keys.csv"])
            {
                Assert.True(complianceKeys.Add(row["key"]), $"Duplicate compliance key {row["key"]}");
                AssertKey(row["key"], "compliance key");
            }

            foreach (var row in files["rule_fact_requirements.csv"])
            {
                Assert.True(requirementKeys.Add(row["requirement_key"]), $"Duplicate requirement_key {row["requirement_key"]}");
                AssertKey(row["requirement_key"], "requirement_key");
                AssertKey(row["fact_key"], "fact_key");
                factKeys.Add(row["fact_key"]);
                Assert.Contains(row["pack_key"], packKeys);
                Assert.Contains(row["citation_key"], citationKeys);
                Assert.Contains(row["applicability_key"], complianceKeys);
                Assert.Equal("boolean", row["value_type"]);
                Assert.Equal("equals", row["operator"]);
                Assert.Equal("true", row["expected_value"]);
                Assert.False(string.IsNullOrWhiteSpace(row["source_product"]), $"Missing source_product for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["source_entity"]), $"Missing source_entity for {row["fact_key"]}");
                Assert.False(string.IsNullOrWhiteSpace(row["retention_period"]), $"Missing retention_period for {row["fact_key"]}");
                Assert.Contains("products=", row["description"], StringComparison.Ordinal);
                Assert.Contains("entities=", row["description"], StringComparison.Ordinal);

                foreach (var product in row["source_product"].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    Assert.Contains(product, AllowedProducts);
                }
            }

            foreach (var row in files["regulatory_mappings.csv"])
            {
                Assert.True(mappingKeys.Add(row["mapping_key"]), $"Duplicate mapping_key {row["mapping_key"]}");
                AssertKey(row["mapping_key"], "mapping_key");
                Assert.Contains(row["pack_key"], packKeys);
                if (!string.IsNullOrWhiteSpace(row["citation_key"]))
                {
                    Assert.Contains(row["citation_key"], citationKeys);
                }

                if (!string.IsNullOrWhiteSpace(row["compliance_key"]))
                {
                    Assert.Contains(row["compliance_key"], complianceKeys);
                }

                if (!string.IsNullOrWhiteSpace(row["fact_key"]))
                {
                    Assert.Contains(row["fact_key"], factKeys);
                }
            }
        }

        Assert.Equal(RequiredPacks, packKeys);
    }

    [Fact]
    public void Baseline_operational_packs_have_rule_content_and_docs_exist()
    {
        var bundle = LoadBundle();

        foreach (var (packDirectory, files) in bundle)
        {
            var packRow = Assert.Single(files["rule_packs.csv"]);
            var factRequirementKeys = files["rule_fact_requirements.csv"]
                .Select(row => row["fact_key"])
                .ToHashSet(StringComparer.Ordinal);

            Assert.Equal(Path.GetFileName(packDirectory), packRow["pack_key"]);
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
                Assert.Contains(rule.GetProperty("factKey").GetString()!, factRequirementKeys);
            }
        }

        var docPath = Path.Combine(RepoRoot(), "docs", "compliance-core", "baseline_rulepack_index.md");
        Assert.True(File.Exists(docPath), "Missing baseline rulepack index doc.");
        Assert.Contains("Baseline rulepack index", File.ReadAllText(docPath), StringComparison.Ordinal);
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
        var root = Path.Combine(RepoRoot(), "root", "rulepack", "baseline");
        Assert.True(Directory.Exists(root), $"Missing generated baseline rulepack root: {root}");
        return Directory.GetDirectories(root).OrderBy(value => value, StringComparer.Ordinal).ToList();
    }

    private static void AssertSchemaManifest()
    {
        var manifestPath = Path.Combine(RepoRoot(), "root", "rulepack", "baseline", "manifest.json");
        Assert.True(File.Exists(manifestPath), $"Missing generated schema manifest: {manifestPath}");

        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var directBundleFiles = manifest.RootElement.GetProperty("directBundleFiles")
            .EnumerateArray()
            .Select(file => file.GetProperty("fileName").GetString()!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var stagedOnlyFiles = manifest.RootElement.GetProperty("stagedOnlyFiles")
            .EnumerateArray()
            .Select(file => file.GetProperty("fileName").GetString()!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.True(directBundleFiles.IsSubsetOf(ExpectedHeaders.Keys), "Manifest direct bundle files must be generated CSV files.");
        Assert.Contains(StagedImportService.EvidenceReferencesFile, stagedOnlyFiles);
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
