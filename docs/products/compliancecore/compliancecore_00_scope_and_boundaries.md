# Compliance Core — Scope, Ownership, and Boundaries

## Product purpose

Compliance Core is the regulatory meaning, rulepack, controlled catalog, requirement, applicability, evidence, exception, exemption, and evaluation engine for the STL Compliance / ARR suite.

Compliance Core answers:

- Which governing body or standard applies?
- Which citation or rule is relevant?
- What does the requirement mean operationally?
- Which objects does the requirement apply to?
- What evidence can satisfy the requirement?
- Are there acceptable alternatives?
- Are exceptions or exemptions available?
- Is a situation likely compliant?
- What is missing, invalid, expired, insufficient, or uncertain?
- Which products need to collect or preserve evidence?

Compliance Core does not execute operational work. It tells products what compliance means and how to evaluate a scenario.

## Compliance Core owns

```text
- Governing body catalog
- Jurisdiction catalog
- Regulation source catalog
- Citation model
- Rulepack definitions
- Rulepack versions
- Requirement definitions
- Requirement categories
- Applicability logic
- Compliance logic
- Evidence type catalog
- Evidence requirement definitions
- Acceptable alternatives
- Exception definitions
- Exemption definitions
- Retention rule definitions
- Controlled compliance vocabulary
- Alias catalog
- Regulatory object type mapping
- Compliance evaluation results
- Evidence mapping suggestions
- Evidence mapping confirmations
- Theoretical Situation Evaluation
- Rulepack import/validation workflow
- Rulepack lifecycle governance
```

## Compliance Core does not own

```text
- Platform login
- Tenant entitlement
- Product launch/handoff
- Person master
- Product permissions
- Training assignment execution
- Certificate issuance truth
- Asset truth
- Work order truth
- Inventory balance
- Stock ledger
- Receiving execution
- Procurement truth
- Supplier master
- Route/trip execution
- Customer master
- Order lifecycle
- Document/file storage truth
- Quality hold/release decisions
- Analytics read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product entitlement
- Platform admin access
- Service tokens
- Tenant identity

StaffArr
- Person, org, site, location facts
- Personnel incidents
- Person readiness context
- Permission checks for Compliance Core users/admins

TrainArr
- Training programs and qualification outcomes
- Training evidence references
- Training requirement implementation facts

MaintainArr
- Assets
- PMs
- Inspections
- Work orders
- Defects
- Maintenance evidence context

LoadArr
- Inventory
- Receiving
- Putaway
- Stock movement
- Storage/location behavior
- Inventory evidence context

SupplyArr
- Supplier/vendor
- Procurement
- Supplier compliance documents
- Purchase/order receipt context

RoutArr
- Routes
- Trips
- Stops
- Driver/vehicle assignment context
- Transportation evidence context

CustomArr
- Customer-specific requirements
- Customer sites
- Customer documents/requirements

OrdArr
- Orders
- Fulfillment dependencies
- Commitment evidence context

RecordArr
- Actual documents/files/evidence
- OCR metadata
- Evidence packages
- Retention execution

AssurArr
- Nonconformance
- CAPA
- Quality hold/release
- Quality evidence context

ReportArr
- Compliance dashboards and analytics
- Audit exports based on Compliance Core evaluations

Field Companion
- Mobile evidence collection
- Mobile situation/evidence input
- Field-facing compliance prompts where permitted
```

## Core source-of-truth rules

```text
1. Compliance Core owns regulatory meaning.
2. Compliance Core owns rulepacks and requirement logic.
3. Compliance Core owns controlled compliance catalogs.
4. Compliance Core owns evidence requirement definitions.
5. Compliance Core owns exception and exemption definitions.
6. Compliance Core owns evaluation results.
7. Compliance Core does not own the operational object being evaluated.
8. RecordArr owns the evidence document/file.
9. Product domains own the facts being evaluated.
10. ReportArr owns dashboards and exports, not compliance meaning.
11. Compliance Core may mirror product object references for evaluation traceability but must not become the source of operational truth.
12. Products should store stable Compliance Core keys/IDs for rulepack applicability and governing body references.
13. Products should not seed/own governing body catalogs that belong to Compliance Core.
```

## Standard Compliance Core object envelope

Every major Compliance Core object should include:

```text
ComplianceCoreObject
- id
- tenantId
- objectNumber
- objectKey
- objectType
- title
- description
- status
- version
- effectiveAt
- retiredAt
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- approvedAt
- approvedByPersonId
- sourceRefs
- citationRefs
- complianceRefs
- auditTrail
- eventLog
```

## Compliance Core object prefixes

```text
GB     Governing body
JUR    Jurisdiction
SRC    Regulation source
CIT    Citation
RPK    Rulepack
REQ    Requirement
APP    Applicability rule
LOG    Compliance logic
EVT    Evidence type
EVR    Evidence requirement
ALT    Acceptable alternative
EXC    Exception
EXM    Exemption
RET    Retention rule
CAT    Controlled catalog
ALIAS  Alias
EVAL   Compliance evaluation
RR     Requirement result
MAP    Evidence mapping
TSE    Theoretical Situation Evaluation
IMP    Import batch
VAL    Validation issue
```

## Standard evaluated object reference

```text
EvaluatedObjectRef
- productKey
- objectType
- objectId
- humanReadableNumber
- displayNameSnapshot
- statusSnapshot
- factsSnapshot
- recordRefs
- lastResolvedAt
```

## Standard Compliance Core key rules

Keys should be stable, readable, and domain-oriented.

```text
Examples
- governingBody.fmcsa
- governingBody.osha
- governingBody.msha
- governingBody.epa
- rulepack.fmcsa.driver_qualification_file
- rulepack.fmcsa.vehicle_inspection_repair_maintenance
- requirement.docs.req.driver_qualification_file
- requirement.inspection.req.annual_vehicle_inspection
- evidence.safety_data_sheet
- evidence.driver_medical_card
- exception.short_haul
- exemption.farm_vehicle_driver
```

## Access model

Compliance Core has separate access paths for platform-controlled authoring, service evaluation, and tenant-facing read/evaluation output.

```text
Authoring/admin routes
- Require NexArr platform-admin validation.
- Cover governing bodies, citations, rulepacks, controlled vocabulary, requirement logic, evidence definitions, imports, publication, and activation.

Service evaluation routes
- Require trusted service token, tenant scope, target product scope, and operation reason.
- Return evaluations, blockers, evidence requirements, and requirement results.

Tenant-facing read/evaluation routes
- May expose evaluated outputs, requirement status, and read-only catalog context.
- Must not expose rulepack authoring or catalog mutation unless the actor is platform-admin validated.

Products may consume Compliance Core through service tokens and scoped integration APIs.
```
