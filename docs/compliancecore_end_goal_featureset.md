# Compliance Core — End Goal and Granular Feature Set

## End Goal

Compliance Core is the platform compliance intelligence layer for the STL Compliance / Arr ecosystem. Its job is to answer one central question across products:

> **Is this person, asset, action, route, vendor, customer, training state, inspection, repair, or workflow OK, not OK, risky, incomplete, expired, blocked, or needing review under the rules that apply right now?**

Compliance Core should not become another operational product. It should not own people, assets, routes, training records, work orders, vendors, customers, tenants, login, or product permissions. Instead, it should own the normalized rule system that interprets facts from the rest of the platform and returns explainable compliance results.

At completion, Compliance Core should allow platform admins to define, version, test, publish, evaluate, explain, and audit rule packs that connect legal citations, internal policy, applicability logic, evidence requirements, product facts, workflow gates, exceptions, remediation actions, and audit-ready proof.

### Core Logic Assumption

Compliance Core's executable logic is **not AI**, not legal interpretation at runtime, and not a trusted autonomous legal agent. Its runtime engine should be simple deterministic code that evaluates imported rule data:

```text
IF applicability conditions pass
AND required conditions pass
AND blocking conditions do not pass
THEN mapped outcome
ELSE mapped failure / unknown / stale / missing-evidence outcome
```

Plain-English text belongs to metadata, review screens, simulator output, audit exports, and explanation templates. The executable rule core is made from controlled keys, fact keys, operators, values, logic groups, outcomes, and tests.


## Product Positioning

Compliance Core is the connective intelligence layer between the operating products:

- **NexArr** validates identity, tenant membership, platform admin access, product access, entitlements, service tokens, and platform trust.
- **StaffArr** owns people, sites, departments, positions, teams, active/inactive status, workforce records, permission assignments, and incident routing.
- **TrainArr** owns training programs, training completion, evaluations, certifications, qualifications, expirations, renewal, remediation training, and training-derived authorization.
- **MaintainArr** owns assets, maintenance, inspections, defects, work orders, PM, evidence, repairs, readiness, downtime, and maintenance execution.
- **RoutArr** owns routes, dispatch, trip execution, driver assignment, vehicle assignment, stops, proof workflows, and transportation exceptions.
- **SupplyArr** owns vendors, customers, parts, inventory, purchasing, approvals, documents, external-party records, and supply execution.
- **Compliance Core** owns normalized compliance rules, citations, rule packs, applicability logic, evidence requirements, evaluation snapshots, explainable outcomes, waivers, remediation guidance, audit packaging, and platform-level compliance visibility.

## Completion Definition

Compliance Core is complete when a platform admin can:

1. Create and manage rule packs by domain, governing body, jurisdiction, effective date, citation, category, and product scope.
2. Normalize laws, regulations, company policies, customer requirements, insurance requirements, and internal controls into deterministic rules.
3. Define applicability logic using an intuitive any/all/none rule builder.
4. Define requirements, evidence expectations, outcomes, workflow gates, remediation paths, and escalation behavior.
5. Consume product-owned facts through APIs, events, local mirrors, or service-token calls without cross-database foreign keys.
6. Evaluate facts from StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, and NexArr-adjacent platform context.
7. Return clear outcomes such as `allow`, `warn`, `block`, `review`, `not_applicable`, `unknown`, `expired`, `missing_evidence`, or `needs_remediation`.
8. Explain every result using deterministic trace output and optional plain-English templates that show the facts used, operators evaluated, rules applied, citations referenced, evidence missing, and recommended next action.
9. Version every rule, citation, control, evidence requirement, exception, waiver, and evaluation snapshot.
10. Provide audit-ready exports that prove what the platform knew, when it knew it, which rules applied, and why the outcome was reached.
11. Keep Compliance Core platform-admin-only, with no product frontend or local role spoofing capable of bypassing rule authority.

## Core Rule Model: 14 Keys + 10 CSVs

Compliance Core should standardize most rule logic around **14 controlled vocabulary keys** and a **10-CSV rule-pack import/export model**.

The purpose is to make laws, regulations, customer requirements, insurance requirements, and internal policy reviewable as deterministic data instead of embedded code or AI interpretation.

### Fourteen Controlled Vocabulary Keys

The platform should maintain controlled vocabulary registries for these keys:

1. `governing_body_key`
2. `regulatory_program_key`
3. `regulated_context_key`
4. `subject_type_key`
5. `activity_context_key`
6. `material_key`
7. `hazard_class_key`
8. `equipment_class_key`
9. `training_requirement_key`
10. `inspection_type_key`
11. `permit_type_key`
12. `incident_report_type_key`
13. `evidence_type_key`
14. `record_retention_key`

These keys are not operational records. They are normalized compliance vocabulary used to make rule applicability and product fact contracts consistent across StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, and NexArr-adjacent platform context.

### Controlled Vocabulary Ownership

Compliance Core owns the normalized vocabulary values, aliases, versioning, deprecation status, regulatory mappings, and rule-pack applicability mappings.

Products expose the keys when their records involve compliance-sensitive concepts.

Examples:

```text
MaintainArr asset fuel type -> asset.fuel_material_key = LP_GAS
RoutArr shipment material -> shipment.material_key = LP_GAS
TrainArr authorization -> training_requirement_key = PIT_OPERATOR_TRAINING
SupplyArr SDS item -> material_key = LP_GAS
StaffArr person/site facts -> subject_type_key = person / site
```

A product does not need to own the vocabulary registry to expose a vocabulary value. It owns the operational record and returns the relevant compliance key as a fact.

### Material and Hazard Derivation

Compliance Core should support approved derivation mappings from exact regulatory classifications to broader facets.

Safe direction:

```text
DOT_DIVISION_2_1 -> fire_property = FLAMMABLE
DOT_DIVISION_2_1 -> physical_state = GAS
DOT_COMBUSTIBLE_LIQUID -> fire_property = COMBUSTIBLE
DOT_COMBUSTIBLE_LIQUID -> physical_state = LIQUID
```

Risky direction:

```text
FLAMMABLE + GAS -> DOT_DIVISION_2_1
COMBUSTIBLE + LIQUID -> DOT_COMBUSTIBLE_LIQUID
```

The engine may derive broad facets from exact approved classes, but it should not infer exact legal classifications from broad facets unless an approved, versioned material profile or regulatory mapping explicitly allows it.

### Ten Core CSVs

The canonical rule-pack import/export bundle should use these ten CSVs:

```text
1. controlled_vocabulary.csv
2. vocabulary_aliases.csv
3. compliance_keys.csv
4. material_keys.csv
5. rule_packs.csv
6. rule_requirements.csv
7. rule_fact_requirements.csv
8. regulatory_mappings.csv
9. sds_references.csv
10. exception_exemptions.csv
```

Optional supporting reference CSVs may seed vocabulary registries, but the audit contract remains anchored in `rule_fact_requirements.csv`. Legal relief is represented by `exception_exemptions.csv` and must not be modeled as an internal override list.

### CSV Responsibility Split

| CSV | Purpose |
| --- | --- |
| `controlled_vocabulary.csv` | Controlled compliance terms, enums, subject kinds, evidence states, and regulatory facets |
| `vocabulary_aliases.csv` | Synonyms, acronyms, and product-local terms that map into controlled vocabulary |
| `compliance_keys.csv` | Compliance domains and deterministic keys used across rules and evaluations |
| `material_keys.csv` | Material and hazard keys used by SupplyArr, RoutArr, and hazmat rule logic |
| `rule_packs.csv` | Versioned package, domain, scope, owner, lifecycle, and effective dates |
| `rule_requirements.csv` | Requirement-level labels, citations, applicability, and remediation metadata |
| `rule_fact_requirements.csv` | Source-of-truth audit contract for required facts, evidence kinds, operators, expected values, severity, and retention |
| `regulatory_mappings.csv` | Citation, requirement, and compliance-key mappings used for traceability and reporting |
| `sds_references.csv` | SDS and material reference records that support hazmat evidence paths |
| `exception_exemptions.csv` | First-class regulatory exceptions, exemptions, waivers, variances, special permits, approvals, alternate compliance paths, and conditional exclusions |

### Runtime Evaluation Pattern

The evaluator should only perform simple operations:

```text
- Resolve applicable rule packs
- Load rule version
- Resolve product-owned and derived facts
- Evaluate all / any / none groups
- Evaluate condition operators
- Apply missing/stale/unknown behavior
- Map result state to outcome
- Store immutable evaluation snapshot
- Generate trace/audit explanation
```

Compliance Core does not decide what the law means at runtime. A compliance author maps a cited requirement into deterministic rule data. Compliance Core evaluates the rule data against facts.


## Ownership Boundaries

### Compliance Core Owns

- Rule packs
- Rule versioning
- Legal citation references
- Internal policy references
- Governing body registry
- Jurisdiction registry
- Compliance domains
- Regulatory categories
- Applicability logic
- Requirement logic
- Evidence requirements
- Normalized controls
- Evaluation engine
- Evaluation snapshots
- Compliance outcomes
- Remediation guidance
- Exception and waiver framework
- Rule testing and simulation
- Rule publishing workflow
- Rule deprecation workflow
- Cross-product fact catalog
- Cross-product compliance visibility
- Cross-product compliance audit package generation
- Explainability layer
- Compliance dashboards
- Product integration contracts for compliance evaluation
- Controlled vocabulary registries for compliance keys
- 10-CSV rule-pack import/export schema
- Regulatory-class-to-facet derivation mappings
- Material, hazard, activity context, subject type, evidence type, and record-retention vocabulary used by rules

### Ownership Clarifications Applied

- Compliance Core owns the canonical compliance rule, citation, waiver, evidence-requirement, and evaluation framework; it does not own the operational records being evaluated.
- Product-owned evidence remains in the source product. Compliance Core defines what evidence is required, stores evaluation snapshots, and references or snapshots the product evidence needed to explain the decision.
- Compliance Core waivers are compliance waivers against rules, controls, or evidence requirements. They are distinct from StaffArr manual authorization overrides and product-local operational overrides.
- Compliance Core may recommend remediation and identify the responsible product, but the receiving product owns the operational workflow that resolves the issue.
- Compliance Core may display product permission facts as inputs, but StaffArr owns person-to-permission assignment records and each product owns permission definitions and enforcement.
- Compliance Core owns controlled vocabulary keys such as `material_key`, `activity_context_key`, and `hazard_class_key`; products own the operational records that expose those keys as facts.
- Compliance Core may derive broad facets from approved regulatory classifications, such as `DOT_DIVISION_2_1 -> FLAMMABLE + GAS`, but it should not infer exact legal classifications from broad facets without an approved material/regulatory mapping.

### Compliance Core Does Not Own

- User login
- Tenant identity
- Platform entitlements
- Product subscriptions
- Product launch authorization
- Canonical person records
- Canonical site records
- Canonical department records
- Canonical position records
- Canonical team records
- Product-specific permissions
- Training programs as operational workflows
- Certification issuance records
- Asset records
- Vehicle records
- Work orders
- Inspections as operational records
- Preventive maintenance schedules
- Routes
- Dispatch records
- Driver assignments
- Vendor records
- Customer records
- Inventory records
- Purchase orders
- Product-specific UI authorization
- Direct writes into another product database
- Cross-product database foreign keys
- Product-specific inventory material records
- Product-specific SDS/document records
- Product-specific asset fuel configuration records
- Product-specific shipment/load material records
- Product-specific work-order material usage records

## Core Operating Model

Compliance Core should operate as a separate product/layer with its own API, frontend, and PostgreSQL database.

Its runtime should be a deterministic rules service. It should not require AI to evaluate a rule. It may use generated explanations, templates, and admin-facing summaries, but the executable decision must be traceable to fact/operator/value comparisons and published rule versions.

Each product keeps its own PostgreSQL database. Compliance Core may maintain local reference tables, mirror tables, denormalized read models, cached fact snapshots, and evaluation snapshots, but it must not use direct database foreign keys into another product database.

Cross-product communication should use:

- Service-token authenticated API calls
- Product events
- Webhooks
- Local mirror/reference tables
- Immutable external IDs
- Contracted DTOs
- Background synchronization jobs
- Evaluation request/response APIs
- Audit logs for every cross-product interaction

## Platform Access Model

Compliance Core should be accessible only to platform admins through NexArr-authorized platform admin access.

Granular access levels inside Compliance Core may exist, but they should all be subordinate to platform-admin eligibility:

- **Platform Admin** — full access to Compliance Core.
- **Compliance Author** — create/edit draft rules, citations, controls, and evidence requirements.
- **Compliance Reviewer** — review, approve, reject, or request changes to rule packs.
- **Compliance Publisher** — publish approved rule pack versions.
- **Compliance Auditor** — view evaluation history and export audit packages.
- **Integration Admin** — manage product fact sources, API contracts, event subscriptions, and sync health.

Even with internal roles, no one should access Compliance Core unless NexArr confirms platform admin eligibility first.

## Core Concepts

### Controlled Vocabulary Key

A controlled vocabulary key is a canonical compliance value used across products and rule packs.

Examples:

- `material_key = LP_GAS`
- `activity_context_key = transport_hazmat`
- `equipment_class_key = POWERED_INDUSTRIAL_TRUCK`
- `training_requirement_key = PIT_OPERATOR_TRAINING`
- `evidence_type_key = SHIPPING_PAPER`

Granular features:

- Key registry
- Display name
- Description
- Owning vocabulary family
- Applicable governing body/program
- Alias support
- Deprecation/supersession support
- Versioned mappings
- Product API exposure contract
- Import/export support
- Audit trail for key changes

### Rule-Pack CSV Bundle

A rule-pack CSV bundle is the canonical machine-readable format for importing, exporting, reviewing, testing, and versioning executable rule logic.

Required files:

- `rule_packs.csv`
- `citations.csv`
- `facts.csv`
- `rules.csv`
- `rule_citations.csv`
- `logic_groups.csv`
- `conditions.csv`
- `outcomes.csv`
- `tests.csv`

Granular features:

- Import preview
- Dry-run validation
- Key uniqueness validation
- Fact existence validation
- Citation link validation
- Logic group validation
- Condition operator validation
- Outcome coverage validation
- Golden test execution
- Conflict detection
- Version diff
- Import rollback
- Immutable import audit event

### Derived Fact

A derived fact is a Compliance Core-calculated fact produced from an approved mapping or source fact, not from AI inference.

Examples:

- `shipment.dot_hazard_class = DOT_DIVISION_2_1` derives `material.fire_property = FLAMMABLE`.
- `shipment.dot_hazard_class = DOT_DIVISION_2_1` derives `material.physical_state = GAS`.
- `material_key = LP_GAS` may derive common aliases such as propane or LPG for search, but not exact legal classification unless the material profile is approved.

Granular features:

- Source fact dependency
- Mapping version
- Citation or reference basis
- Confidence level
- Directionality controls
- Derived fact snapshot
- Derived fact audit trace
- Deactivation/supersession handling

### Source

A source is the origin of a rule, requirement, or control. It may be a regulation, company policy, customer requirement, insurance requirement, contract requirement, manufacturer requirement, internal standard, or operational safety rule.

Granular features:

- Source type registry
- Source title
- Source owner
- Source URL/reference field
- Source version/date
- Source effective date
- Source expiration/supersession date
- Source jurisdiction
- Source governing body
- Source reliability status
- Source review cadence
- Source attachment support
- Source notes
- Source change history

### Citation

A citation is the specific reference used to justify a rule or control.

Granular features:

- Citation code/reference
- Citation title
- Citation body excerpt/summary field
- Citation source link
- Governing body
- Jurisdiction
- Domain/category
- Effective date
- Expiration/supersession date
- Plain-English summary
- Internal interpretation notes
- Related citations
- Supersedes/superseded-by relationship
- Citation attachment support
- Citation review workflow
- Citation confidence/status
- Citation-to-rule mapping
- Citation-to-evidence mapping
- Citation-to-remediation mapping

### Rule Pack

A rule pack is a versioned collection of rules, citations, applicability logic, controls, evidence requirements, outcomes, and remediation instructions. Its executable core should be importable/exportable through the nine core CSVs.

Granular features:

- Rule pack name
- Rule pack code/key
- Domain
- Product scope
- Jurisdiction scope
- Governing body scope
- Tenant scope
- System/default flag
- Draft/published/retired status
- Semantic version
- Effective date
- Expiration date
- Review date
- Owner
- Approval workflow
- Publish workflow
- Changelog
- Rollback support
- Import/export support
- Rule pack cloning
- Tenant override policy
- Dependency on other rule packs
- Conflict detection
- Deprecation/supersession handling

### Rule

A rule is a deterministic compliance statement made from applicability logic, conditions, outcomes, and evidence expectations. A rule may have human-readable labels and explanation templates, but its executable behavior is stored as logic groups and fact/operator/value conditions.

Granular features:

- Rule key
- Rule title
- Rule description
- Plain-English rule statement as review metadata
- Workflow key
- Subject type key
- Activity context key
- Applicability logic group
- Requirement logic group
- Evidence expectation metadata
- Outcome mapping
- Remediation mapping
- Severity
- Priority
- Blocking behavior
- Manual review behavior
- Effective date
- Expiration date
- Citation links
- Related control links
- Product fact dependencies
- Test cases
- Version history
- Change notes
- Approval status
- Published status

### Control

A control is the normalized platform action or check that satisfies, proves, monitors, or enforces a rule.

Granular features:

- Control key
- Control name
- Control description
- Control type
- Preventive/detective/corrective classification
- Product owner
- Evidence expectations
- Automation status
- Manual review requirement
- Evaluation frequency
- Related rules
- Related citations
- Related workflows
- Related remediation actions
- Control effectiveness status
- Control test history

### Fact

A fact is a product-owned data point used during compliance evaluation.

Granular features:

- Fact key
- Fact display name
- Fact description
- Fact owner product
- Fact entity type
- Fact data type
- Fact source endpoint/event
- Fact freshness requirement
- Fact staleness tolerance
- Fact confidence level
- Fact required/optional flag
- Fact null behavior
- Fact unknown behavior
- Fact mapping definition
- Controlled vocabulary key mapping
- Derived fact mapping
- Fact transformation rule
- Fact sample value
- Fact validation rules
- Fact provenance
- Fact last synced timestamp

### Evaluation

An evaluation is the result of applying rule logic to known facts at a point in time.

Granular features:

- Evaluation ID
- Tenant ID reference
- Subject type
- Subject external ID
- Triggering product
- Triggering workflow
- Evaluation reason
- Rule pack versions used
- Rules evaluated
- Facts used
- Derived facts used
- Facts missing
- Facts stale
- Outcome
- Severity
- Explanation
- Recommended next action
- Blocking decision
- Warning decision
- Review decision
- Remediation requirement
- Evidence snapshot
- Request metadata
- Response metadata
- Evaluation duration
- Evaluation timestamp
- Evaluated-by service/user
- Immutable snapshot storage

## Rule Builder

The rule builder should be intuitive enough for a platform admin to create rules without writing code, while still storing rules in a deterministic machine-readable format that can round-trip through the nine core CSVs. Plain-English previews are generated from rule metadata and condition traces; they are not executable logic.

### Logic Group Features

- `all` group: every condition must be true
- `any` group: at least one condition must be true
- `none` group: no listed condition may be true
- Nested groups
- Parenthetical grouping
- Drag-and-drop condition ordering
- Plain-English preview
- JSON/YAML advanced view
- Rule linting
- Missing fact detection
- Conflicting condition detection
- Unsupported fact detection
- Test mode
- Version comparison
- CSV round-trip validation against `logic_groups.csv` and `conditions.csv`

### Condition Operators

The builder should support at least:

- equals
- does not equal
- exists
- does not exist
- is true
- is false
- greater than
- greater than or equal
- less than
- less than or equal
- between
- not between
- contains
- does not contain
- includes any
- includes all
- excludes all
- in list
- not in list
- starts with
- ends with
- matches pattern
- before date
- after date
- on or before date
- on or after date
- expired
- not expired
- expires within
- age greater than
- age less than
- stale
- fresh
- changed since
- count greater than
- count less than
- matrix match
- lookup match
- relationship exists
- relationship missing

### Matrix Logic

Matrix rules should allow regulatory or policy decisions that depend on multiple attributes. For the core model, matrix behavior should be represented through controlled vocabulary keys, lookup/matrix conditions, and test cases rather than requiring extra mandatory CSVs.

Examples of matrix dimensions:

- Person role
- Person position
- Site
- Department
- Asset class
- Vehicle type
- Equipment category
- Route type
- Jurisdiction
- Customer requirement
- Load type
- Work type
- Material key
- Hazard class key
- Activity context key
- Regulatory program key
- Regulated context key
- Training category
- Certification category
- Training requirement key
- Activity context key
- Material/equipment authorization key when applicable
- Inspection type
- Defect severity
- Vendor type
- Part category
- Time since last event
- Mileage/hours since last event

Matrix features:

- Matrix editor
- CSV import/export
- Versioned matrix rows
- Effective dates per row
- Conflict detection
- Missing combination detection
- Default fallback row
- Test evaluator
- Plain-English matrix explanation

### Outcome Builder

Rule outcomes should be configurable.

Supported outcomes:

- Allow
- Warn
- Block
- Require review
- Require evidence
- Require remediation
- Require retraining
- Require supervisor approval
- Require platform admin approval
- Mark not applicable
- Mark unknown
- Mark stale
- Mark expired
- Escalate
- Notify product owner
- Create incident recommendation
- Create corrective workflow recommendation

Outcome configuration:

- Severity
- User-facing message
- Admin-facing explanation
- Product-facing machine code
- Blocking flag
- Override eligibility
- Required override reason
- Evidence needed to clear
- Remediation guidance
- Expiration of decision
- Re-evaluation trigger

## Evaluation Engine

The engine should be deterministic, explainable, versioned, and safe to use for workflow gating.

Granular features:

- Single-subject evaluation
- Batch evaluation
- Scheduled evaluation
- Event-triggered evaluation
- On-demand product API evaluation
- Pre-dispatch evaluation
- Pre-assignment evaluation
- Pre-work evaluation
- Pre-inspection evaluation
- Pre-purchase approval evaluation
- Person readiness evaluation
- Asset readiness evaluation
- Route readiness evaluation
- Vendor/customer compliance evaluation
- Evidence completeness evaluation
- Training/certification applicability evaluation
- Rule dependency resolution
- Fact dependency resolution
- Controlled vocabulary key resolution
- Derived fact resolution from approved mappings
- Local fact cache use
- Live product fact fetch fallback
- Unknown fact handling
- Stale fact handling
- Partial evaluation support
- Evaluation timeout behavior
- Retry behavior
- Idempotent request handling
- Deduplication
- Snapshot storage
- Trace-based explanation generation
- Audit event generation

## Evaluation Outcomes

Compliance Core should distinguish between failure, missing data, stale data, and true non-applicability.

Recommended outcome model:

- `allow` — rules passed and action may proceed.
- `warn` — action may proceed with warning or notice.
- `block` — action should not proceed.
- `review` — human review is required before proceeding.
- `not_applicable` — rule does not apply to this subject/action.
- `unknown` — Compliance Core lacks enough reliable facts to decide.
- `missing_evidence` — required evidence is absent.
- `stale_evidence` — required evidence exists but is too old.
- `expired` — required credential, inspection, document, or control has expired.
- `needs_remediation` — a corrective workflow is needed.
- `waived` — a valid waiver/exception allows temporary continuation.
- `superseded` — rule or requirement has been replaced by a newer version.

## Fact Catalog

The fact catalog is the contract between Compliance Core and the rest of the platform.

Granular features:

- Product fact registry
- Entity type registry
- Fact key registry
- Controlled vocabulary key registry
- Regulatory classification mapping registry
- Derived fact registry
- Fact schema definition
- Fact sample payloads
- Required/optional field classification
- Source product ownership
- API endpoint mapping
- Event mapping
- Local mirror table mapping
- Freshness rules
- Staleness warnings
- Data quality rules
- Fact confidence rules
- Fact transformation rules
- Fact lineage/provenance
- Material/hazard/activity/equipment key mappings
- Contract versioning
- Breaking change detection
- Product integration health
- Last successful sync
- Last failed sync
- Fact availability dashboard

### Core Controlled Vocabulary Families

Compliance Core should seed and manage at least these vocabulary families:

- Governing bodies
- Regulatory programs
- Regulated contexts
- Subject types
- Activity contexts
- Materials
- Hazard classes
- Equipment classes
- Training requirement types
- Inspection types
- Permit types
- Incident report types
- Evidence types
- Record retention types

These are reference vocabularies, not operational records. Product APIs should expose these keys when applicable so Compliance Core can evaluate rules without text guessing.

### Example Cross-Product Key Usage

```text
material_key = LP_GAS
activity_context_key = forklift_fuel
workflow_key = can_operate_pit
equipment_class_key = POWERED_INDUSTRIAL_TRUCK
training_requirement_key = PIT_OPERATOR_TRAINING
```

The same `material_key = LP_GAS` can also appear with:

```text
activity_context_key = transport_hazmat
workflow_key = can_dispatch_hazmat_load
```

or:

```text
activity_context_key = hot_work
workflow_key = can_start_hot_work
```

The material is shared; the workflow, activity context, product-owned facts, citations, and outcomes are different.

## Product Fact Examples

### NexArr-Adjacent Platform Facts

Compliance Core may reference platform context validated by NexArr, but NexArr remains the owner.

Examples:

- Tenant exists
- Tenant active status
- Product entitlement exists
- Product entitlement active status
- User has platform admin access
- Service token audience/scope validity
- Tenant product access map

### StaffArr Facts

Examples:

- Person exists
- Person active status
- Person site assignment
- Person department assignment
- Person position
- Person team
- Person manager/supervisor relationship
- Person permission assignment
- Person incident history reference
- Person employment/onboarding status
- Site active status
- Department active status
- Position active status

### TrainArr Facts

Examples:

- Certification exists
- Certification status
- Certification expiration date
- Qualification status
- Training program completion
- Training evaluation completion
- Practical evaluation signoff
- Refresher required status
- Remediation training required status
- Training evidence exists
- Trainer/evaluator signoff exists
- Certification governing body
- Certification category
- Training requirement key
- Activity context key
- Material/equipment authorization key when applicable

### MaintainArr Facts

Examples:

- Asset exists
- Asset active status
- Asset class/type
- Asset fuel material key
- Asset material/hazard profile keys
- Asset readiness status
- Inspection status
- Inspection expiration
- Defect status
- Defect severity
- Work order status
- Work order material keys
- Work order activity context key
- PM due status
- Repair evidence exists
- Inspection evidence exists
- Maintenance document exists
- Asset out-of-service status
- Downtime status

### RoutArr Facts

Examples:

- Route exists
- Route status
- Driver assignment
- Vehicle assignment
- Trailer assignment
- Trip status
- Stop status
- Proof-of-delivery status
- Dispatch exception status
- Route jurisdiction
- Route distance
- Load/customer requirement
- Shipment material key
- Hazmat class key
- Placarding required flag
- Driver duty context reference

### SupplyArr Facts

Examples:

- Vendor exists
- Vendor active status
- Vendor approval status
- Customer exists
- Customer approval status
- Part exists
- Part category
- Inventory item material key
- SDS/document material key
- Inventory availability
- Purchase order approval status
- Document evidence exists
- Vendor document expiration
- Customer requirement reference
- Supplier compliance status

## Rule Pack Domains

Compliance Core should support rule packs across multiple compliance and policy domains.

Potential domains:

- Driver qualification
- Vehicle qualification
- Asset readiness
- Inspection compliance
- Maintenance compliance
- Preventive maintenance compliance
- Defect response
- Out-of-service handling
- Route/dispatch gating
- Training and qualification applicability
- Powered industrial truck / equipment authorization
- Hazardous work authorization
- Hazardous material transportation
- LP-Gas storage, handling, dispensing, and motor fuel
- Hot work and oxygen/fuel-gas operations
- Chemical hazard communication
- Environmental/waste handling
- Site-specific work authorization
- Customer-specific requirements
- Vendor approval
- Customer approval
- Purchasing approval
- Document retention
- Evidence completeness
- Incident response
- Corrective action
- Safety policy
- Internal operating policy
- Insurance requirement
- Contract requirement
- Audit readiness

## Rule Pack Lifecycle

### Drafting

- Create new rule pack
- Clone existing rule pack
- Import rule pack through the 10-CSV bundle
- Validate controlled vocabulary keys
- Run golden tests from `tests.csv`
- Assign owner
- Assign domain
- Attach citations
- Define rules
- Define applicability
- Define evidence
- Define outcomes
- Define test cases
- Run validation
- Request review

### Review

- Reviewer assignment
- Commenting
- Change requests
- Side-by-side version diff
- Test result review
- Citation review
- Evidence review
- Product impact review
- Approval checklist

### Publishing

- Publish version
- Effective date selection
- Tenant/product scope selection
- Migration notes
- Product notification
- Evaluation cache invalidation
- Backfill evaluation option
- Changelog generation

### Retirement

- Mark rule pack deprecated
- Supersede with replacement
- Preserve historical evaluations
- Prevent new evaluations after expiration unless explicitly requested
- Keep audit history intact
- Notify affected products/admins

## Exceptions and Waivers

Compliance Core should support controlled exceptions without making the platform unsafe.

Granular features:

- Exception request
- Waiver request
- Exception type
- Subject type
- Subject external ID
- Rule/rule pack reference
- Reason code
- Free-text explanation
- Required evidence
- Approver requirement
- Approval chain
- Effective date
- Expiration date
- Renewal requirement
- Revocation
- Escalation
- Audit log
- Product visibility
- Evaluation behavior while waived
- Hard non-waivable rule support
- Waiver abuse reporting

## Remediation Guidance

When something is not OK, Compliance Core should explain what needs to happen next.

Granular features:

- Remediation code
- Remediation description
- Responsible product
- Responsible role
- Suggested workflow
- Required evidence to clear
- Expected resolution state
- Escalation threshold
- Due date rule
- Priority/severity
- Link to citation/control
- Link to affected facts
- Link to product action
- Product task recommendation
- Human-readable explanation

Examples:

- Send person to TrainArr for retraining/requalification.
- Send asset to MaintainArr for inspection or repair.
- Send route back to RoutArr dispatch review.
- Send missing person/site data to StaffArr.
- Send vendor document issue to SupplyArr.
- Send entitlement or access issue to NexArr/platform admin.

## Audit and Evidence

Compliance Core should be able to generate audit-ready evidence packages without owning the operational records themselves. Product systems remain the source of truth for raw evidence such as work order photos, training signoffs, vendor documents, route proof, and personnel documents. Compliance Core owns the evidence requirement, the evaluation snapshot, the rule explanation, and the cross-product audit package that references those product-owned records.

Granular features:

- Evaluation snapshot export
- Rule pack version export
- Citation export
- Fact provenance export
- Evidence checklist export
- Missing evidence report
- Waiver report
- Exception report
- Product source reference list
- Timestamped decision log
- Who/what triggered evaluation
- Product response log
- Approval history
- Remediation history
- PDF/HTML/CSV/JSON export options
- Tenant-specific audit package
- Person-specific audit package
- Asset-specific audit package
- Route-specific audit package
- Vendor/customer-specific audit package
- Rule-specific audit package
- Time-window audit package

## Admin Console

Compliance Core frontend should be purpose-built for platform admins.

### Main Navigation

Recommended sections:

- Dashboard
- Rule Packs
- Rules
- Citations
- Sources
- Governing Bodies
- Jurisdictions
- Fact Catalog
- Controlled Vocabulary
- CSV Imports
- Product Integrations
- Evaluations
- Simulator
- Exceptions & Waivers
- Evidence
- Remediation
- Audit Packages
- Change Impact
- Publishing Queue
- System Health
- Settings

### Dashboard

Granular features:

- Compliance status overview
- Active rule packs
- Rules by domain
- Evaluations by outcome
- Blocked actions
- Warning trends
- Unknown/stale fact count
- Missing evidence count
- Expiring waivers
- Expiring citations/reviews
- Product integration health
- Recent rule changes
- Recent evaluation failures
- Audit readiness score

### Rule Pack UI

Granular features:

- Rule pack list
- Search/filter/sort
- Domain filter
- Jurisdiction filter
- Product scope filter
- Status filter
- Version history
- Draft editor
- Published version viewer
- Rule pack compare
- Rule pack clone
- Rule pack import/export
- 10-CSV import validation and round-trip export
- 10-CSV bundle validation
- Import preview and dry-run
- CSV round-trip export
- Approval workflow
- Publish workflow
- Retire/supersede action

### Rule Editor UI

Granular features:

- Plain-English summary
- Citation selector
- Applicability builder
- Requirement builder
- Evidence builder
- Outcome builder
- Remediation builder
- Severity selector
- Product fact selector
- Controlled vocabulary key selector
- Derived fact preview
- Test cases panel
- Live validation panel
- JSON/YAML advanced view
- Explanation preview
- Version diff
- Save draft
- Submit for review

### Citation UI

Granular features:

- Citation registry
- Citation search
- Citation source type
- Governing body selector
- Jurisdiction selector
- Effective dates
- Supersession mapping
- Plain-English interpretation
- Related rules
- Related evidence
- Attachments
- Review status
- Change notes

### Fact Catalog UI

Granular features:

- Product fact list
- Owner product filter
- Entity type filter
- Data type filter
- Required/optional filter
- Freshness requirement
- API/event mapping
- Sample payload viewer
- Contract version viewer
- Sync status
- Stale fact warnings
- Missing fact warnings
- Fact usage map
- Rules using this fact

### Simulator UI

Granular features:

- Select tenant
- Select product context
- Select subject type
- Select subject external ID
- Select action/workflow
- Select rule pack version
- Mock facts
- Pull live facts
- Run evaluation
- View outcome
- View fact trace
- View rule trace
- View citation trace
- View missing evidence
- View remediation guidance
- Save as test case
- Export simulation report

### Evaluations UI

Granular features:

- Evaluation search
- Outcome filter
- Product filter
- Subject type filter
- Rule pack filter
- Date range filter
- Blocked/warned/review filter
- Evaluation detail view
- Fact snapshot view
- Rule explanation view
- Evidence status
- Remediation status
- Waiver status
- Re-evaluate action
- Export evaluation

### Exceptions & Waivers UI

Granular features:

- Waiver list
- Exception list
- Request form
- Approval form
- Evidence attachment
- Expiration date
- Rule scope
- Subject scope
- Non-waivable flag display
- Revocation action
- Renewal action
- Evaluation impact preview
- Audit history

## API Surface

Compliance Core should expose a clean API that products can call without sharing databases.

Recommended API groups:

### Controlled Vocabulary APIs

- `GET /api/v1/vocabulary`
- `GET /api/v1/vocabulary/{family}`
- `POST /api/v1/vocabulary/{family}`
- `PATCH /api/v1/vocabulary/{family}/{key}`
- `GET /api/v1/vocabulary/{family}/{key}/usage`
- `GET /api/v1/vocabulary/{family}/{key}/history`
- `POST /api/v1/vocabulary/validate-keys`

### Rule Pack Import APIs

- `POST /api/v1/rule-pack-imports/preview`
- `POST /api/v1/rule-pack-imports/validate`
- `POST /api/v1/rule-pack-imports/publish-draft`
- `GET /api/v1/rule-pack-imports/{id}`
- `GET /api/v1/rule-pack-imports/{id}/diff`
- `GET /api/v1/rule-pack-imports/{id}/test-results`
- `POST /api/v1/rule-pack-imports/{id}/rollback`

### Rule Pack APIs

- `GET /api/v1/rule-packs`
- `POST /api/v1/rule-packs`
- `GET /api/v1/rule-packs/{id}`
- `PATCH /api/v1/rule-packs/{id}`
- `POST /api/v1/rule-packs/{id}/clone`
- `POST /api/v1/rule-packs/{id}/submit-review`
- `POST /api/v1/rule-packs/{id}/approve`
- `POST /api/v1/rule-packs/{id}/publish`
- `POST /api/v1/rule-packs/{id}/retire`
- `GET /api/v1/rule-packs/{id}/versions`
- `GET /api/v1/rule-packs/{id}/diff`

### Rule APIs

- `GET /api/v1/rules`
- `POST /api/v1/rules`
- `GET /api/v1/rules/{id}`
- `PATCH /api/v1/rules/{id}`
- `POST /api/v1/rules/{id}/validate`
- `POST /api/v1/rules/{id}/test`
- `GET /api/v1/rules/{id}/usage`
- `GET /api/v1/rules/{id}/history`

### Citation APIs

- `GET /api/v1/citations`
- `POST /api/v1/citations`
- `GET /api/v1/citations/{id}`
- `PATCH /api/v1/citations/{id}`
- `GET /api/v1/citations/{id}/rules`
- `GET /api/v1/citations/{id}/history`

### Fact Catalog APIs

- `GET /api/v1/facts`
- `POST /api/v1/facts`
- `GET /api/v1/facts/{key}`
- `PATCH /api/v1/facts/{key}`
- `GET /api/v1/facts/{key}/usage`
- `POST /api/v1/facts/validate-payload`
- `GET /api/v1/fact-sources`
- `GET /api/v1/derived-facts`
- `POST /api/v1/derived-facts/preview`
- `POST /api/v1/fact-sources`
- `PATCH /api/v1/fact-sources/{id}`

### Evaluation APIs

- `POST /api/v1/evaluations/run`
- `POST /api/v1/evaluations/batch`
- `POST /api/v1/evaluations/simulate`
- `GET /api/v1/evaluations`
- `GET /api/v1/evaluations/{id}`
- `POST /api/v1/evaluations/{id}/re-evaluate`
- `GET /api/v1/evaluations/{id}/explanation`
- `GET /api/v1/evaluations/{id}/audit-export`

### Product Gate APIs

- `POST /api/v1/gates/evaluate`
- `POST /api/v1/gates/can-assign-person`
- `POST /api/v1/gates/can-dispatch-route`
- `POST /api/v1/gates/can-operate-asset`
- `POST /api/v1/gates/can-start-work`
- `POST /api/v1/gates/can-close-work-order`
- `POST /api/v1/gates/can-approve-purchase`
- `POST /api/v1/gates/can-use-vendor`
- `POST /api/v1/gates/can-serve-customer`

### Exception/Waiver APIs

- `GET /api/v1/waivers`
- `POST /api/v1/waivers`
- `GET /api/v1/waivers/{id}`
- `PATCH /api/v1/waivers/{id}`
- `POST /api/v1/waivers/{id}/approve`
- `POST /api/v1/waivers/{id}/reject`
- `POST /api/v1/waivers/{id}/revoke`
- `POST /api/v1/waivers/{id}/renew`

### Audit APIs

- `GET /api/v1/audit/events`
- `POST /api/v1/audit/packages`
- `GET /api/v1/audit/packages`
- `GET /api/v1/audit/packages/{id}`
- `GET /api/v1/audit/packages/{id}/download`

## Event Model

Compliance Core should publish and consume events to keep products synchronized without direct database coupling.

### Events Consumed

- `staffarr.person.created`
- `staffarr.person.updated`
- `staffarr.person.deactivated`
- `staffarr.assignment.changed`
- `staffarr.permission.changed`
- `staffarr.incident.created`
- `trainarr.certification.issued`
- `trainarr.certification.expired`
- `trainarr.certification.revoked`
- `trainarr.training.completed`
- `trainarr.remediation.required`
- `maintainarr.asset.updated`
- `maintainarr.inspection.completed`
- `maintainarr.inspection.expired`
- `maintainarr.defect.created`
- `maintainarr.defect.resolved`
- `maintainarr.workorder.closed`
- `maintainarr.pm.due`
- `routarr.route.created`
- `routarr.dispatch.requested`
- `routarr.trip.started`
- `routarr.trip.completed`
- `routarr.exception.created`
- `supplyarr.vendor.updated`
- `supplyarr.customer.updated`
- `supplyarr.purchase.approval.requested`
- `supplyarr.document.expired`

### Events Published

- `compliancecore.rulepack.published`
- `compliancecore.rulepack.retired`
- `compliancecore.rule.changed`
- `compliancecore.citation.changed`
- `compliancecore.evaluation.completed`
- `compliancecore.evaluation.blocked`
- `compliancecore.evaluation.warned`
- `compliancecore.evaluation.review_required`
- `compliancecore.evidence.missing`
- `compliancecore.evidence.stale`
- `compliancecore.waiver.approved`
- `compliancecore.waiver.expiring`
- `compliancecore.waiver.revoked`
- `compliancecore.remediation.required`
- `compliancecore.fact_contract.changed`
- `compliancecore.vocabulary.changed`
- `compliancecore.material_mapping.changed`
- `compliancecore.rulepack.imported`

## Suggested Database Areas

Compliance Core should have its own PostgreSQL schema built around compliance concepts.

Potential table groups:

### Reference Tables

- `governing_bodies`
- `regulatory_programs`
- `regulated_contexts`
- `jurisdictions`
- `compliance_domains`
- `subject_types`
- `activity_contexts`
- `materials`
- `material_aliases`
- `hazard_classes`
- `hazard_class_facet_mappings`
- `equipment_classes`
- `training_requirement_types`
- `inspection_types`
- `permit_types`
- `incident_report_types`
- `record_retention_types`
- `source_types`
- `rule_categories`
- `control_types`
- `severity_levels`
- `outcome_types`
- `evidence_types`
- `waiver_types`
- `remediation_types`

### Source and Citation Tables

- `sources`
- `citations`
- `citation_versions`
- `citation_relationships`
- `citation_attachments`
- `citation_review_events`

### Rule Pack Tables

- `rule_packs`
- `rule_pack_versions`
- `rule_pack_scopes`
- `rule_pack_dependencies`
- `rule_pack_changelogs`
- `rule_pack_publish_events`
- `rule_pack_imports`
- `rule_pack_import_files`
- `rule_pack_import_validation_results`
- `rule_pack_import_test_results`

### Rule Tables

- `rules`
- `rule_versions`
- `rule_citations`
- `logic_groups`
- `conditions`
- `rule_applicability_blocks`
- `rule_requirement_blocks`
- `rule_evidence_requirements`
- `rule_outcomes`
- `rule_remediation_links`
- `rule_test_cases`

### Control Tables

- `controls`
- `control_versions`
- `control_rules`
- `control_evidence_requirements`
- `control_test_results`

### Fact Tables

- `fact_catalog`
- `fact_sources`
- `fact_contract_versions`
- `fact_mappings`
- `derived_fact_mappings`
- `controlled_vocabulary_mappings`
- `fact_sync_status`
- `fact_snapshots`
- `product_reference_mirrors`

### Evaluation Tables

- `evaluation_requests`
- `evaluation_results`
- `evaluation_rule_results`
- `evaluation_fact_snapshots`
- `evaluation_evidence_results`
- `evaluation_explanations`
- `evaluation_product_responses`

### Exception and Waiver Tables

- `waivers`
- `waiver_rules`
- `waiver_subjects`
- `waiver_approvals`
- `waiver_evidence`
- `waiver_events`

### Audit Tables

- `audit_events`
- `audit_packages`
- `audit_package_items`
- `export_jobs`
- `admin_activity_log`

## Security Requirements

Compliance Core should be secure by default because it influences whether work can proceed.

Granular requirements:

- NexArr-backed authentication
- Platform-admin eligibility check before route access
- Server-side authorization on every API call
- No frontend-only role checks
- No localStorage/mock-user bypass
- Service-token authentication for product calls
- Audience validation
- Scope validation
- Tenant validation
- Token expiration
- Token rotation support
- Token revocation support
- Request signing option
- Idempotency keys for evaluation requests
- Audit logging for admin changes
- Audit logging for product calls
- Immutable evaluation snapshots
- Row-level tenant isolation where applicable
- No raw secrets in logs
- No direct database access from products
- Safe failure behavior
- No AI or text-inference fallback for workflow-gating decisions
- Unknown or unmapped controlled vocabulary values must produce `review` or `unknown`, not silent allow
- Rate limiting for evaluation APIs
- Permissioned export access
- Sensitive evidence redaction controls

## Product Integration Behavior

### MaintainArr Integration

MaintainArr should ask Compliance Core whether an asset, inspection, defect, PM, work order, material-handling activity, hazardous work activity, or maintenance action is compliant or blocked. MaintainArr should expose controlled vocabulary facts such as asset class, equipment class, material keys, activity contexts, inspection types, permit types, and evidence references.

Examples:

- Can this asset be marked ready?
- Can this inspection be accepted?
- Can this defect be deferred?
- Can this work order be closed?
- Is required inspection evidence missing?
- Is this asset out of compliance for dispatch?

### RoutArr Integration

RoutArr should ask Compliance Core whether dispatch, driver assignment, vehicle assignment, route start, route continuation, hazmat movement, or exception handling is allowed. RoutArr should expose controlled vocabulary facts such as shipment material keys, hazard class keys, route context, placarding requirement, and activity context.

Examples:

- Can this driver be assigned to this route?
- Can this vehicle be dispatched?
- Can this trip start?
- Does this route require special qualification?
- Does this customer/load/site require additional evidence?
- Should this dispatch be blocked, warned, or reviewed?

### StaffArr Integration

StaffArr should provide person/org facts and receive compliance visibility where person status, incidents, assignments, sites, roles, reporting triggers, or permissions affect compliance.

Examples:

- Is this person active?
- Is this person assigned to the correct site/department/position?
- Does this person have the required product-specific permission assignment?
- Has an incident created a compliance concern?
- Is this person eligible for a role or workflow assignment?

### TrainArr Integration

TrainArr should provide qualification/training facts keyed to training requirement types, activity contexts, material/equipment authorization, and refresher/remediation requirements.

Examples:

- Does this person hold the required qualification?
- Is the qualification expired?
- Is refresher training required?
- Is practical evaluation missing?
- Should retraining/remediation be triggered?

### SupplyArr Integration

SupplyArr should ask Compliance Core whether vendor/customer/part/material/SDS/purchasing/document workflows satisfy applicable requirements. SupplyArr should expose material keys, SDS links, vendor documents, hazardous material classifications when known, and inventory item compliance facts.

Examples:

- Can this vendor be used?
- Is this customer approved for this workflow?
- Is required vendor documentation expired?
- Can this purchase be approved?
- Does this part/category require special approval?

## Reporting

Compliance Core should provide platform-level compliance reporting without taking over the product data.

Granular reports:

- Compliance status by tenant
- Compliance status by product
- Compliance status by domain
- Rule pack adoption report
- Evaluation outcome report
- Blocked action report
- Warning trend report
- Unknown fact report
- Stale fact report
- Missing evidence report
- Expiring certification dependency report
- Expiring inspection dependency report
- Expiring document dependency report
- Waiver/exception report
- Remediation queue report
- Citation review report
- Rule change impact report
- Audit readiness report
- Product integration health report

## Change Impact Analysis

When a rule changes, Compliance Core should show what may be affected.

Granular features:

- Affected rule packs
- Affected products
- Affected tenants
- Affected subjects
- Affected workflows
- Potential new blocks
- Potential new warnings
- Potential new reviews
- Required product facts
- Affected controlled vocabulary keys
- Affected derived fact mappings
- Missing fact dependencies
- Backfill evaluation estimate
- Before/after comparison
- Publish risk warning
- Admin approval requirement

## Testing Requirements

Compliance Core should have strong automated and admin-facing tests because rule mistakes can block real work.

Granular features:

- Unit tests for evaluator operators
- Unit tests for nested logic groups
- Unit tests for matrix logic
- Unit tests for date/expiration logic
- Unit tests for unknown/stale fact behavior
- Rule validation tests
- 10-CSV import validation tests
- 10-CSV round-trip export/import tests
- Controlled vocabulary key validation tests
- Derived fact mapping tests
- Rule pack publishing tests
- Evaluation snapshot tests
- API contract tests
- Product integration tests
- Service-token tests
- Authorization tests
- Tenant isolation tests
- Regression tests for published rule packs
- Golden test cases for important rule packs
- Simulator-saved test cases
- Performance/load tests for batch evaluation

## Performance Expectations

Compliance Core should support both real-time gating and background analysis.

Granular features:

- Fast single-action evaluation for workflow gates
- Batch evaluation for dashboards and audits
- Cached product fact snapshots
- Cached controlled vocabulary lookups
- Cached derived fact mappings
- Staleness-aware cache use
- Background sync jobs
- Evaluation queue
- Retry queue
- Dead-letter queue
- Idempotent evaluation requests
- Database indexes for tenant/product/domain/outcome/time
- Archived historical snapshots
- Pagination for evaluation history
- Streaming/export jobs for large audit packages
- Rate limiting and throttling

## Implementation Phases

### Phase 1 — Foundation

- Separate Compliance Core app/API/database
- NexArr platform-admin-only access
- Basic admin shell
- Rule pack CRUD
- Citation CRUD
- Governing body/jurisdiction/domain references
- Initial 14-key controlled vocabulary registry
- Basic fact catalog
- 10-CSV import/export skeleton
- Any/all/none rule builder
- Basic operators
- Evaluation API
- Evaluation result snapshot
- Plain-English explanation
- Audit log for admin changes

### Phase 2 — Product Contracts

- Fact source registry
- StaffArr fact contract
- TrainArr fact contract
- MaintainArr fact contract
- RoutArr fact contract
- SupplyArr fact contract
- Service-token API calls
- Event consumption
- Local mirror/reference tables
- Integration health dashboard
- Missing/stale fact handling
- Product support for relevant controlled vocabulary keys
- Derived fact mapping support for approved regulatory classifications

### Phase 3 — Rule Publishing

- Draft/review/publish lifecycle
- Rule pack versioning
- Citation versioning
- Rule diff viewer
- Rule test cases
- Simulator
- Change impact analysis
- Backfill evaluation jobs
- Rule pack import/export
- 10-CSV import validation and round-trip export
- 10-CSV bundle validation
- Import preview and dry-run
- CSV round-trip export

### Phase 4 — Workflow Gating

- Product gate APIs
- Generic `gates/evaluate` API using `workflow.key`, `activity_context_key`, and subject references
- Allow/warn/block/review outcomes
- Remediation guidance
- Evidence requirements
- Product response tracking
- Evaluation webhooks/events
- Hard-block support
- Non-waivable rules
- Waiver-aware evaluations

### Phase 5 — Audit and Assurance

- Cross-product compliance audit package generation
- Evaluation history explorer
- Evidence completeness reports
- Exception/waiver reports
- Citation review reports
- Tenant/domain/product compliance dashboards
- Export jobs
- Immutable historical snapshots

### Phase 6 — Advanced Compliance Intelligence

- Source ingestion workflow
- Rule change monitoring
- Citation supersession tracking
- Rule recommendation drafts for human review only
- Control effectiveness tracking
- Cross-product trend analytics
- Risk scoring
- Predictive missing-evidence warnings
- Compliance readiness forecasting


---

## Audit-Informed Feature Additions: Platform Access, Ownership, and Verification

These additions are part of the product feature set. They are not optional implementation notes.

### NexArr Launch and Product Session Contract

Protected product experiences must use the platform launch pattern:

1. User starts in NexArr.
2. NexArr validates login, tenant status, product status, entitlement, callback allowlist, and launch state.
3. NexArr redirects to the product callback path: `/auth/nexarr/callback`.
4. The product backend redeems the handoff code server-side.
5. The product creates a local product session containing at minimum `personId`, `tenantId`, `productCode`, entitlement snapshot, and session expiry.
6. The product then applies its own server-side domain authorization rules.

### Required Access Features

- `/auth/nexarr/callback` route in the product frontend and backend.
- Server-side handoff redemption.
- Expired, reused, missing, wrong-product, and invalid-callback handoff rejection.
- Friendly launch failure, entitlement denied, invalid callback, product unavailable, and tenant selection states.
- Product session hydration endpoint.
- Product logout or session clear behavior that does not create a competing login system.
- Quick-switch menu that reads NexArr catalog data and sends users back through NexArr `/launch/{productCode}`.
- Tenant context display sourced from the validated product session.
- Current user display sourced from `personId` and product/session data.
- No product-generated trusted launch URLs.
- No product-side entitlement guessing.
- No product-owned platform login.

### Authority and Safety Rules

- Frontend hiding is not authorization.
- No production feature may rely on localStorage admin switches, mock users, hardcoded role strings, fake permission strings, or frontend-only entitlement checks.
- Development-only identity or permission shortcuts must be guarded by `VITE_APP_ENV=development` and must not ship as production fallbacks.
- Product APIs must validate tenant, session, entitlement, product permission, and record ownership server-side.
- Cross-product records must use APIs, events, service tokens, local mirrors, snapshots, or external references. No direct cross-product database foreign keys.
- Product switchers and shared shells are visual/structural only; they do not centralize product-specific authorization.

### Feature Verification Standard

A feature is complete only when there is concrete implementation evidence:

- Backend route/service/model/schema where applicable.
- Frontend route/page/component/API client where applicable.
- Persistence where the feature implies stored data.
- Authorization where the feature implies protected access.
- Cross-product contract where the feature depends on another product.
- Tests or smoke checks where practical.

TODO text, mock-only state, placeholder UI, documentation-only claims, sample data, or frontend-only screens do not count as completed features.

---

## Audit-Informed Feature Additions: Platform-Admin Safety and Product Gate APIs

### Platform-Admin Authorization Features

Compliance Core is high-risk because it models platform-wide rules and cross-product facts. Its admin UI and admin APIs must be protected by backend-validated platform authority.

Features:

- Current platform session hydrate endpoint.
- Route/API loader or middleware that rejects non-platform-admin users before admin data is returned.
- Platform-admin check delegated to NexArr/session validation, not local frontend state.
- No tenant/customer-facing Compliance Core admin UI.
- No localStorage admin switches.
- No mock users.
- No hardcoded role strings as authority.
- Clear unauthorized, forbidden, expired-session, and missing-entitlement states.
- Audit events for denied Compliance Core admin access.

Completion criteria:

- An ordinary tenant user cannot directly access Compliance Core admin screens or APIs.
- A product can consume Compliance Core outcomes through service-token-protected product/backend integration without granting the user direct admin access.

### Product Gate Evaluation API

Compliance Core should support deterministic workflow gate checks for products without becoming the product workflow owner.

Gate examples:

- `can_dispatch_route`
- `can_release_trip`
- `can_start_work_order`
- `can_close_work_order`
- `can_assign_person_to_task`
- `can_issue_training_qualification`
- `can_approve_purchase`
- `can_use_vendor`
- `can_use_asset`
- `can_accept_evidence`

Features:

- Product submits subject references, action key, rule context, and fact snapshot references.
- Compliance Core returns allow/warn/block/review/unknown/missing-evidence outcome.
- Response includes applied rule versions, citation references, missing facts, stale facts, evidence requirements, remediation hints, and trace ID.
- Product owns whether and how to enforce the returned gate inside its workflow.
- Evaluation snapshot is immutable and audit-exportable.

Completion criteria:

- Products can prove why a workflow was blocked, warned, allowed, or sent to review without Compliance Core owning the operational record.

### Waiver and Override Separation

Compliance Core waivers must stay distinct from product operational overrides and StaffArr manual authorization overrides.

Features:

- Compliance waiver scope: rule/control/evidence requirement/outcome.
- Required reason, approver, effective date, expiration date, citation/control reference, and affected subject.
- Waiver impact is visible in evaluation traces.
- Product override references may be displayed but are owned by the product or StaffArr.
- Expired waivers no longer affect evaluation results.

Completion criteria:

- A user can distinguish “Compliance Core waived this rule requirement” from “StaffArr manually authorized this person” or “MaintainArr manager overrode this asset hold.”


## Production-Complete Acceptance Criteria

Compliance Core can be considered production-complete when:

- Platform admins can manage all rules, citations, packs, facts, evaluations, waivers, and audits from the UI.
- Products can call Compliance Core to gate important workflows.
- Every evaluation is explainable and traceable.
- Every rule is versioned.
- Every citation is versioned or reviewable.
- Every published rule pack can be tested before release.
- Every cross-product fact has an owner and contract.
- Rule packs can round-trip through the nine core CSVs without losing executable logic.
- Controlled vocabulary keys are versioned, reviewable, and validated during import.
- Product APIs expose applicable keys such as material, activity context, equipment class, training requirement, inspection type, permit type, and evidence type.
- Derived facts are produced only from approved mappings and are traceable in evaluation snapshots.
- Missing/stale facts do not get confused with compliance failure.
- Product data ownership boundaries are preserved.
- No direct cross-database foreign keys exist.
- No product can bypass Compliance Core by trusting frontend state.
- No Compliance Core screen is accessible without NexArr-confirmed platform admin authority.
- Audit exports can prove what rule version, fact snapshot, evidence state, and outcome existed at a specific time.
- Exceptions and waivers are controlled, time-bound, auditable, and rule-aware.
- The platform can answer: **what applies, which keys selected it, which facts/operators/values were evaluated, what passed, what failed, what is missing, who approved it, what changed, and what must happen next.**
