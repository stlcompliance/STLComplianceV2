using ComplianceCore.Api.Contracts;

namespace ComplianceCore.Api.Services;

public static class CoreVocabularyKeyCatalog
{
    public static readonly CoreVocabularyKeyResponse[] Keys =
    [
        new("governing_body_key", "Governing Body", "Regulatory or policy authority that owns the requirement.", 10, true),
        new("regulatory_program_key", "Regulatory Program", "Program or rule family under a governing body.", 20, true),
        new("regulated_context_key", "Regulated Context", "Operating context where the rule applies.", 30, true),
        new("subject_type_key", "Subject Type", "Person, asset, site, shipment, vendor, or other subject class.", 40, true),
        new("activity_context_key", "Activity Context", "Work activity or process being evaluated.", 50, true),
        new("material_key", "Material", "Canonical material, substance, commodity, or product material classification.", 60, true),
        new("hazard_class_key", "Hazard Class", "Hazard class used by HazCom, hazmat, safety, and routing rules.", 70, true),
        new("equipment_class_key", "Equipment Class", "Canonical equipment class used by maintenance and training rules.", 80, true),
        new("training_requirement_key", "Training Requirement", "Training or authorization requirement that may gate work.", 90, true),
        new("inspection_type_key", "Inspection Type", "Inspection, check, or examination type required by a rule.", 100, true),
        new("permit_type_key", "Permit Type", "Permit, authorization, or license type tied to an activity.", 110, true),
        new("incident_report_type_key", "Incident Report Type", "Incident or event report classification.", 120, true),
        new("evidence_type_key", "Evidence Type", "Evidence class expected to prove compliance.", 130, true),
        new("record_retention_key", "Record Retention", "Retention schedule or retention rule applied to evidence.", 140, true)
    ];

    public static readonly HashSet<string> KeyNames = Keys
        .Select(key => key.Key)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static CoreVocabularyKeyRegistryResponse GetRegistry() => new(Keys);

    public static ValidateCoreVocabularyKeysResponse Validate(IReadOnlyList<string> keys) =>
        new(keys
            .Select(key =>
            {
                var normalized = key.Trim();
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    return new ValidateCoreVocabularyKeyResult(key, false, "empty_key");
                }

                return KeyNames.Contains(normalized)
                    ? new ValidateCoreVocabularyKeyResult(normalized, true, null)
                    : new ValidateCoreVocabularyKeyResult(normalized, false, "unknown_core_key");
            })
            .ToArray());
}
