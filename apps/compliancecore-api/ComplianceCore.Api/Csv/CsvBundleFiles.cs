namespace ComplianceCore.Api.Csv;

public static class CsvBundleFiles
{
    public const string ControlledVocabulary = "controlled_vocabulary.csv";
    public const string VocabularyAliases = "vocabulary_aliases.csv";
    public const string ComplianceKeys = "compliance_keys.csv";
    public const string MaterialKeys = "material_keys.csv";
    public const string RulePacks = "rule_packs.csv";
    public const string RuleRequirements = "rule_requirements.csv";
    public const string RuleFactRequirements = "rule_fact_requirements.csv";
    public const string RegulatoryMappings = "regulatory_mappings.csv";
    public const string SdsReferences = "sds_references.csv";

    public static readonly IReadOnlyList<string> All =
    [
        ControlledVocabulary,
        VocabularyAliases,
        ComplianceKeys,
        MaterialKeys,
        RulePacks,
        RuleRequirements,
        RuleFactRequirements,
        RegulatoryMappings,
        SdsReferences
    ];

    public static bool IsKnownFile(string fileName) =>
        All.Any(name => string.Equals(name, fileName, StringComparison.OrdinalIgnoreCase));
}
