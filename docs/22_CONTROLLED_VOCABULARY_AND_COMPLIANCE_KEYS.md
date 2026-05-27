# Controlled Vocabulary and Compliance Keys

## Purpose

Compliance Core provides stable vocabulary and key structures that products can use for compliance-aware decisions without hardcoding regulatory language independently inside each product.

## Principles

- Stable keys drive logic.
- Labels can change without breaking logic.
- Aliases map real-world language to canonical terms.
- Products use keys; users see readable labels.
- Atomic keys and derived categories can coexist.
- Rule packs can combine keys instead of requiring bloated one-off fields.

## Key Families

### Compliance Keys

Examples:

- driver_qualification
- vehicle_inspection
- preventive_maintenance
- defect_repair
- hazardous_material
- ppe_required
- training_required
- incident_reportable
- audit_evidence_required

### Material Keys

Examples:

- flammable
- combustible
- gas
- liquid
- corrosive
- oxidizer
- toxic
- compressed_gas
- hazardous_waste
- battery
- aerosol
- fuel
- oil
- solvent

A rule can evaluate `flammable` + `gas`. A derived category such as `flammable_gas` can exist where the rule or workflow needs it.

## CSV Import Families

1. `controlled_vocabulary.csv`
2. `vocabulary_aliases.csv`
3. `compliance_keys.csv`
4. `material_keys.csv`
5. `rule_packs.csv`
6. `rule_requirements.csv`
7. `rule_fact_requirements.csv`
8. `regulatory_mappings.csv`
9. `sds_references.csv`

## Example Rows

```csv
key,label,category,description,active
flammable,Flammable,material_hazard,Can ignite under defined conditions,true
gas,Gas,physical_state,Material exists as gas under defined conditions,true
vehicle_inspection,Vehicle Inspection,compliance_domain,Inspection requirement domain,true
```

## Product Use

| Product | Uses |
|---|---|
| StaffArr | certification categories, readiness blockers, incident reason categories |
| TrainArr | training requirement categories, citation context, material/task training requirements |
| MaintainArr | inspection categories, defect severity, maintenance evidence requirements |
| RoutArr | dispatch categories, DVIR reason codes, route exception categories |
| SupplyArr | material hazards, SDS/HazCom references, part/vendor compliance categories |

## Evaluation Results

Compliance Core can return allow, warn, block, needs_review, or not_applicable with reason codes and reference context. Product APIs decide workflow behavior using those results plus product permissions and product-specific rules.
