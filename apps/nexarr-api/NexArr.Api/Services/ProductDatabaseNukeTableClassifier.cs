using STLCompliance.Shared.Operations;

namespace NexArr.Api.Services;

public sealed record ProductDatabaseNukeTableClassification(
    string Disposition,
    string Reason)
{
    public bool Preserve => string.Equals(Disposition, ProductDatabaseNukeTableDispositions.Preserve, StringComparison.Ordinal);
}

public static class ProductDatabaseNukeTableDispositions
{
    public const string Truncate = "truncate";
    public const string Preserve = "preserve";
}

public static class ProductDatabaseNukeTableClassifier
{
    private static readonly HashSet<string> InfrastructureTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "__EFMigrationsHistory",
        "platform_metadata",
    };

    private static readonly HashSet<string> NexArrControlPlaneTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "platform_users",
        "user_credentials",
        "user_sessions",
        "tenants",
        "tenant_memberships",
        "platform_role_assignments",
        "external_identity_provider_mappings",
        "product_catalog",
        "tenant_product_entitlements",
        "platform_audit_events",
        "service_clients",
        "service_tokens",
        "nexarr_platform_service_token_cleanup_settings",
        "nexarr_platform_session_settings",
        "product_launch_profiles",
        "product_callback_allowlist",
        "nexarr_platform_tenant_lifecycle_settings",
        "nexarr_tenant_product_data_plane_profiles",
        "nexarr_platform_outbox_publisher_settings",
        "nexarr_import_mapping_templates",
    };

    private static readonly HashSet<string> NexArrReferenceTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "reference_datasets",
        "reference_sources",
        "ingestion_jobs",
        "staging_records",
        "reference_entities",
        "reference_entity_versions",
        "reference_crosswalks",
        "tenant_reference_overlays",
        "product_mappings",
        "reference_publish_events",
        "reference_audit_events",
    };

    private static readonly HashSet<string> ComplianceCoreReferenceTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "compliancecore_vocabulary_types",
        "compliancecore_vocabulary_terms",
        "compliancecore_vocabulary_aliases",
        "compliancecore_compliance_keys",
        "compliancecore_material_keys",
        "compliancecore_governing_bodies",
        "compliancecore_jurisdictions",
        "compliancecore_regulatory_programs",
        "compliancecore_rule_packs",
        "compliancecore_regulatory_citations",
        "compliancecore_fact_definitions",
        "compliancecore_fact_requirements",
        "compliancecore_regulatory_mappings",
        "compliancecore_fact_sources",
        "compliancecore_workflow_gate_definitions",
        "compliancecore_sds_references",
        "compliancecore_hazcom_references",
        "compliancecore_evidence_option_groups",
        "compliancecore_evidence_options",
        "compliancecore_rule_test_cases",
        "compliance_exception_exemption",
    };

    private static readonly HashSet<string> MaintainArrReferenceTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "maintainarr_asset_classes",
        "maintainarr_asset_types",
        "maintainarr_catalogs",
        "maintainarr_catalog_options",
        "maintainarr_catalog_option_dependencies",
        "maintainarr_compliance_regulatory_key_mirrors",
        "maintainarr_fieldset_definitions",
        "maintainarr_fieldset_fields",
        "maintainarr_inspection_templates",
        "maintainarr_inspection_template_categories",
        "maintainarr_inspection_checklist_items",
        "maintainarr_inspection_template_asset_types",
        "maintainarr_maintenance_permit_refs",
        "maintainarr_recall_make_model_aliases",
        "maintainarr_reference_cache_entries",
        "maintainarr_staff_person_refs",
    };

    private static readonly HashSet<string> StaffArrReferenceTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "staffarr_role_templates",
        "staffarr_permission_templates",
        "staffarr_role_template_permissions",
        "staffarr_permission_catalog_cache",
        "staffarr_certification_definitions",
    };

    private static readonly HashSet<string> TrainArrReferenceTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "trainarr_training_definitions",
        "trainarr_training_definition_steps",
        "trainarr_training_definition_completion_rules",
        "trainarr_training_definition_step_branches",
        "trainarr_training_program_definitions",
        "trainarr_training_program_content_references",
        "trainarr_training_program_version_definitions",
        "trainarr_training_citation_attachments",
        "trainarr_training_rule_pack_requirements",
    };

    private static readonly HashSet<string> RoutArrReferenceTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "routarr_staffarr_person_refs",
        "routarr_vehicle_refs",
    };

    private static readonly HashSet<string> SupplyArrReferenceTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "supplyarr_part_catalogs",
        "supplyarr_part_manufacturer_aliases",
    };

    public static ProductDatabaseNukeTableClassification Classify(
        string productDatabase,
        string schema,
        string table)
    {
        if (InfrastructureTables.Contains(table))
        {
            return Preserve("schema infrastructure");
        }

        if (IsAuditTable(table))
        {
            return Preserve("audit trail");
        }

        if (productDatabase.Equals(StlProductDatabaseCatalog.NexArr, StringComparison.OrdinalIgnoreCase))
        {
            if (NexArrControlPlaneTables.Contains(table))
            {
                return Preserve("NexArr platform control plane");
            }

            if (NexArrReferenceTables.Contains(table))
            {
                return Preserve("platform reference data");
            }
        }

        if (productDatabase.Equals(StlProductDatabaseCatalog.ComplianceCore, StringComparison.OrdinalIgnoreCase)
            && ComplianceCoreReferenceTables.Contains(table))
        {
            return Preserve("Compliance Core reference data");
        }

        if (productDatabase.Equals(StlProductDatabaseCatalog.MaintainArr, StringComparison.OrdinalIgnoreCase)
            && MaintainArrReferenceTables.Contains(table))
        {
            return Preserve("MaintainArr reference data");
        }

        if (productDatabase.Equals(StlProductDatabaseCatalog.StaffArr, StringComparison.OrdinalIgnoreCase)
            && StaffArrReferenceTables.Contains(table))
        {
            return Preserve("StaffArr authority reference data");
        }

        if (productDatabase.Equals(StlProductDatabaseCatalog.TrainArr, StringComparison.OrdinalIgnoreCase)
            && TrainArrReferenceTables.Contains(table))
        {
            return Preserve("TrainArr training reference data");
        }

        if (productDatabase.Equals(StlProductDatabaseCatalog.RoutArr, StringComparison.OrdinalIgnoreCase)
            && RoutArrReferenceTables.Contains(table))
        {
            return Preserve("RoutArr reference mirror data");
        }

        if (productDatabase.Equals(StlProductDatabaseCatalog.SupplyArr, StringComparison.OrdinalIgnoreCase)
            && SupplyArrReferenceTables.Contains(table))
        {
            return Preserve("SupplyArr reference data");
        }

        if (LooksLikeReferenceData(table))
        {
            return Preserve("reference data");
        }

        return new ProductDatabaseNukeTableClassification(
            ProductDatabaseNukeTableDispositions.Truncate,
            "product data");
    }

    private static ProductDatabaseNukeTableClassification Preserve(string reason) =>
        new(ProductDatabaseNukeTableDispositions.Preserve, reason);

    private static bool IsAuditTable(string table)
    {
        var normalized = table.ToLowerInvariant();
        return normalized.Contains("_audit_", StringComparison.Ordinal)
            || normalized.EndsWith("_audit_events", StringComparison.Ordinal)
            || normalized.EndsWith("_audit_event", StringComparison.Ordinal)
            || normalized.EndsWith("_audit_log", StringComparison.Ordinal)
            || normalized.EndsWith("_audit_logs", StringComparison.Ordinal);
    }

    private static bool LooksLikeReferenceData(string table)
    {
        var normalized = table.ToLowerInvariant();
        return normalized.Contains("_reference_", StringComparison.Ordinal)
            || normalized.Contains("_references", StringComparison.Ordinal)
            || normalized.Contains("reference_", StringComparison.Ordinal)
            || normalized.Contains("_catalog_", StringComparison.Ordinal)
            || normalized.EndsWith("_catalogs", StringComparison.Ordinal)
            || normalized.Contains("_definition_", StringComparison.Ordinal)
            || normalized.EndsWith("_definitions", StringComparison.Ordinal)
            || normalized.Contains("_template_", StringComparison.Ordinal)
            || normalized.EndsWith("_templates", StringComparison.Ordinal)
            || normalized.Contains("_vocabulary_", StringComparison.Ordinal)
            || normalized.Contains("_governing_", StringComparison.Ordinal)
            || normalized.Contains("_regulatory_", StringComparison.Ordinal)
            || normalized.Contains("_jurisdiction", StringComparison.Ordinal)
            || normalized.Contains("_citation", StringComparison.Ordinal)
            || normalized.Contains("_rule_pack", StringComparison.Ordinal)
            || normalized.EndsWith("_aliases", StringComparison.Ordinal)
            || normalized.EndsWith("_cache_entries", StringComparison.Ordinal)
            || normalized.Contains("_fieldset_", StringComparison.Ordinal);
    }
}
