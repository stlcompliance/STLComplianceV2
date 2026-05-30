# Title 49 9-CSV alignment

The repo already defines the Compliance Core CSV bundle as these nine files. Title 49 uses that existing import shape instead of the fallback names.

| CSV | Title 49 use |
| --- | --- |
| `controlled_vocabulary.csv` | Adds a compliance_domain term for each pack. Headers: `term_key,vocabulary_type_key,label,description,active` |
| `vocabulary_aliases.csv` | Reserved for later aliases; generated with headers only. Headers: `term_key,alias_text,active` |
| `compliance_keys.csv` | Creates one deterministic compliance key per rule pack. Headers: `key,label,category,description,active` |
| `material_keys.csv` | Reserved; HMR material classification stays in SupplyArr material/SDS facts. Headers: `key,label,category,description,active` |
| `rule_packs.csv` | Creates one pack row and embeds operational rule content JSON where applicable. Headers: `pack_key,program_key,version_number,label,description,status,active,rule_content_json` |
| `rule_requirements.csv` | Creates citation rows for Title 49 hierarchy nodes. Headers: `citation_key,program_key,pack_key,pack_version,label,source_reference,description,active,supersedes_citation_key` |
| `rule_fact_requirements.csv` | Defines the audit-fact contract for each pack/citation: fact key, applicability, source product/entity/record, value semantics, evidence kind, document type, retention, audit question, severity, override, and remediation metadata. Headers: `requirement_key,fact_key,pack_key,pack_version,citation_key,citation_version,applicability_key,source_product,source_entity,source_field_or_record_type,value_type,operator,expected_value,evidence_kind,required_document_type,retention_period,audit_question,failure_severity,automatic_failure_flag,override_allowed,override_permission,remediation_required,label,description,is_required,active` |
| `regulatory_mappings.csv` | Maps packs, citations, compliance keys, and fact keys. Headers: `mapping_key,target_kind,program_key,pack_key,pack_version,citation_key,compliance_key,material_key,fact_key,label,description,active` |
| `sds_references.csv` | Reserved; products own SDS documents and publish facts. Headers: `sds_key,material_key,product_name,manufacturer,document_url,revision_date,active` |

Fact definitions are not represented by a separate CSV. The Compliance Core importer upserts fact definitions directly from `rule_fact_requirements.csv`, including `value_type`, before it persists pack-specific fact requirement metadata. `tools/compliancecore/import-title49-rulepacks.ps1` still posts exactly the 9 CSV files per bundle.

Compliance Core owns rule packs, citations, fact requirements, audit contracts, rule evaluation, evidence references, audit traces, and report surfaces. Product apps own operational records and publish facts and evidence references. The CSVs contain deterministic keys only; no cross-product database foreign keys are introduced.
