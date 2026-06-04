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

Compliance Core should usually be limited to platform/admin/compliance users.

```text
Typical access
- Platform admin
- Compliance admin
- Rulepack author
- Rulepack reviewer
- Compliance evaluator
- Evidence mapping reviewer
- Auditor/read-only

Products may consume Compliance Core through service tokens and scoped integration APIs.
```


---


# Compliance Core — Catalog, Citation, and Rulepack Model

## Governing body

A GoverningBody is an authority, standard owner, customer authority, or internal policy authority that can issue requirements.

```text
GoverningBody
- governingBodyId
- governingBodyKey
- displayName
- abbreviation
- description
- jurisdictionType
  - federal
  - state
  - local
  - international
  - industry
  - customer
  - internal
  - insurer
  - other
- country
- state
- locality
- websiteUrl
- status
  - draft
  - active
  - inactive
  - deprecated
  - archived
- replacedByGoverningBodyRef
- createdAt
- updatedAt
```

## Jurisdiction

```text
Jurisdiction
- jurisdictionId
- jurisdictionKey
- displayName
- jurisdictionType
  - country
  - state
  - province
  - county
  - city
  - region
  - site
  - customer
  - internal
- parentJurisdictionId
- country
- state
- locality
- status
  - active
  - inactive
  - deprecated
```

## Regulation source

A RegulationSource is the source container for citations and requirements. Examples: CFR title, OSHA standard, customer standard, internal policy manual, industry standard.

```text
RegulationSource
- regulationSourceId
- sourceKey
- title
- description
- sourceType
  - cfr
  - statute
  - regulation
  - standard
  - guidance
  - interpretation
  - customer_requirement
  - internal_policy
  - insurer_requirement
  - contract
  - other
- governingBodyId
- jurisdictionRefs
- publicationRef
- sourceUrl
- effectiveAt
- supersededAt
- status
  - draft
  - active
  - superseded
  - deprecated
  - archived
- versionLabel
- createdAt
- updatedAt
```

## Citation

A Citation is a stable reference to a section, paragraph, clause, appendix, table, or interpretive item from a RegulationSource.

```text
Citation
- citationId
- citationKey
- regulationSourceId
- parentCitationId
- displayCitation
- title
- citationText
- titleNumber
- subtitle
- chapter
- subchapter
- part
- subpart
- section
- paragraph
- appendix
- table
- clause
- citationPath
- effectiveAt
- supersededAt
- status
  - active
  - superseded
  - reserved
  - removed
  - deprecated
- replacedByCitationRef
- sourceUrl
- notes
```

## Citation relationship

```text
CitationRelationship
- relationshipId
- sourceCitationId
- targetCitationId
- relationshipType
  - parent_child
  - references
  - modifies
  - supersedes
  - interpreted_by
  - exception_to
  - definition_for
  - related_to
- status
```

## Controlled catalog

Compliance Core owns controlled compliance catalogs that other products can consume.

```text
ControlledCatalog
- catalogId
- catalogKey
- title
- description
- status
  - draft
  - active
  - deprecated
  - archived
- ownerProduct: compliancecore
- entryRefs
- version
- createdAt
- updatedAt
```

## Controlled catalog entry

```text
ControlledCatalogEntry
- entryId
- catalogId
- entryKey
- displayName
- description
- status
  - active
  - inactive
  - deprecated
- sortOrder
- parentEntryId
- aliases
- metadata
```

## Core catalogs

```text
- governing_bodies
- jurisdictions
- regulation_sources
- citation_types
- requirement_types
- evidence_types
- applicability_subject_types
- asset_compliance_categories
- training_compliance_categories
- document_compliance_categories
- incident_compliance_categories
- maintenance_compliance_categories
- transportation_compliance_categories
- inventory_compliance_categories
- supplier_compliance_categories
- customer_requirement_categories
- exception_types
- exemption_types
- severity_levels
- confidence_levels
- retention_triggers
```

## Alias

Aliases help normalize user language and imported text.

```text
Alias
- aliasId
- phrase
- normalizedKey
- aliasType
  - acronym
  - synonym
  - abbreviation
  - common_name
  - misspelling
  - legacy_term
  - product_term
- targetObjectType
  - governing_body
  - citation
  - rulepack
  - requirement
  - evidence_type
  - catalog_entry
- targetObjectRef
- status
  - active
  - inactive
  - deprecated
```

## Rulepack

A Rulepack is a packaged set of related requirements, applicability logic, evidence requirements, and citations.

```text
Rulepack
- rulepackId
- rulepackKey
- title
- description
- domain
  - fleet
  - workplace_safety
  - hazmat
  - training
  - maintenance
  - warehouse
  - transportation
  - environmental
  - quality
  - customer
  - supplier
  - document_control
  - internal
  - other
- status
  - draft
  - review
  - active
  - superseded
  - deprecated
  - archived
- version
- versionLabel
- governingBodyRefs
- jurisdictionRefs
- regulationSourceRefs
- citationRefs
- requirementRefs
- applicabilityRuleRefs
- exceptionRefs
- exemptionRefs
- evidenceTypeRefs
- retentionRuleRefs
- effectiveAt
- expiresAt
- supersededByRulepackRef
- ownerPersonId
- reviewerPersonId
- approvedByPersonId
- approvedAt
- createdAt
- updatedAt
- auditTrail
```

## Rulepack version

```text
RulepackVersion
- rulepackVersionId
- rulepackId
- version
- versionLabel
- status
  - draft
  - review
  - active
  - superseded
  - archived
- changeSummary
- effectiveAt
- supersededAt
- requirementSnapshot
- citationSnapshot
- approvedByPersonId
- approvedAt
```

## Rulepack family examples

```text
FMCSA
- Driver qualification file requirements
- Hours of service records
- Vehicle inspection, repair, and maintenance
- Annual inspections
- Roadside inspection handling
- ELD documentation and supporting records
- Accident register
- Drug and alcohol program references if in scope

OSHA
- Hazard communication
- PPE
- Lockout/tagout
- Powered industrial trucks
- Walking-working surfaces
- Recordkeeping
- Emergency action/fire prevention
- Machine guarding
- Respiratory protection if applicable

MSHA
- Part 46 training
- Workplace examinations
- Hazard reporting
- Contractor/customer mining-site applicability

EPA
- Hazardous waste handling
- Spill response evidence
- Environmental recordkeeping

Internal/customer
- Customer required PPE
- Customer delivery documentation
- Site-specific training
- Insurance/certificate requirements
```

## Rulepack lifecycle

```text
1. Rulepack is drafted.
2. Citations are mapped.
3. Requirements are created.
4. Applicability logic is defined.
5. Evidence requirements are defined.
6. Exceptions/exemptions are mapped.
7. Rulepack is validated.
8. Reviewer approves.
9. Rulepack becomes active.
10. Superseded versions remain available for historical evaluation.
```

## Events

```text
compliancecore.governing_body.created
compliancecore.governing_body.updated
compliancecore.catalog.created
compliancecore.catalog.updated
compliancecore.citation.created
compliancecore.citation.updated
compliancecore.rulepack.created
compliancecore.rulepack.submitted_for_review
compliancecore.rulepack.approved
compliancecore.rulepack.activated
compliancecore.rulepack.superseded
compliancecore.rulepack.deprecated
compliancecore.alias.created
compliancecore.alias.updated
```


---


# Compliance Core — Requirement, Applicability, and Logic Model

## Requirement

A Requirement is a single compliance expectation that can be evaluated against one or more operational objects.

```text
Requirement
- requirementId
- requirementKey
- rulepackId
- title
- plainLanguageSummary
- detailedDescription
- requirementType
  - document_required
  - training_required
  - inspection_required
  - maintenance_required
  - equipment_required
  - process_required
  - record_retention
  - posting_required
  - reporting_required
  - qualification_required
  - operational_limit
  - notification_required
  - approval_required
  - evidence_review_required
- requirementCategory
  - person
  - asset
  - location
  - training
  - maintenance
  - transportation
  - inventory
  - supplier
  - customer
  - document
  - incident
  - quality
  - environmental
  - safety
  - other
- severity
  - informational
  - low
  - moderate
  - high
  - critical
- status
  - draft
  - active
  - superseded
  - deprecated
  - archived
- citationRefs
- applicabilityRuleRefs
- complianceLogicRef
- evidenceRequirementRefs
- acceptableAlternativeRefs
- exceptionRefs
- exemptionRefs
- retentionRuleRefs
- relatedRequirementRefs
- effectiveAt
- supersededAt
- notes
- auditTrail
```

## Requirement relationship

```text
RequirementRelationship
- relationshipId
- sourceRequirementId
- targetRequirementId
- relationshipType
  - prerequisite
  - alternative_to
  - supports
  - conflicts_with
  - supersedes
  - duplicates
  - child_requirement
  - parent_requirement
  - related_to
- notes
```

## Applicability subject

Applicability subjects define the kind of object a requirement can apply to.

```text
ApplicabilitySubjectType
- subjectTypeKey
- displayName
- productKey
- objectType
- exampleObjects
```

Examples:

```text
- staffarr.person
- staffarr.location
- staffarr.site
- trainarr.training_assignment
- trainarr.qualification
- maintainarr.asset
- maintainarr.work_order
- maintainarr.inspection
- maintainarr.defect
- loadarr.inventory_item
- loadarr.receipt
- loadarr.stock_movement
- supplyarr.supplier
- supplyarr.purchase_order
- routarr.trip
- routarr.route
- routarr.stop
- customarr.customer
- customarr.customer_location
- ordarr.order
- recordarr.record
- assurarr.nonconformance
- fieldcompanion.mobile_upload
```

## Applicability rule

```text
ApplicabilityRule
- applicabilityRuleId
- requirementId
- title
- description
- appliesToSubjectType
- status
  - draft
  - active
  - deprecated
- conditionLogic
- includedWhen
- excludedWhen
- requiredFacts
- optionalFacts
- confidenceRules
- defaultApplicability
  - applicable
  - not_applicable
  - unknown
- explanation
```

## Applicability condition

```text
ApplicabilityCondition
- conditionId
- fieldPath
- operator
  - equals
  - not_equals
  - in
  - not_in
  - exists
  - missing
  - greater_than
  - greater_than_or_equal
  - less_than
  - less_than_or_equal
  - between
  - contains
  - starts_with
  - ends_with
  - date_before
  - date_after
  - within_days
  - older_than_days
  - any
  - all
  - none
  - not
- value
- valueType
  - string
  - number
  - boolean
  - date
  - duration
  - enum
  - object_ref
  - list
- sourceProduct
- required
- missingBehavior
  - unknown
  - not_applicable
  - fail
  - warning
```

## Compliance logic

ComplianceLogic defines pass/fail/warning evaluation after applicability has been determined.

```text
ComplianceLogic
- complianceLogicId
- requirementId
- title
- description
- logicType
  - evidence_exists
  - evidence_valid
  - evidence_current
  - field_value
  - date_window
  - frequency
  - count
  - threshold
  - status
  - composite
  - manual_review
- operator
  - all
  - any
  - none
  - not
  - exists
  - missing
  - equals
  - not_equals
  - greater_than
  - less_than
  - within_days
  - no_older_than
  - at_least
  - at_most
- operands
- passMessage
- failMessage
- warningMessage
- unknownMessage
- manualReviewRequired
```

## Compliance expression

A compliance expression is a composable piece of logic.

```text
ComplianceExpression
- expressionId
- parentExpressionId
- expressionType
  - condition
  - group
  - evidence_check
  - exception_check
  - exemption_check
  - calculation
- operator
  - all
  - any
  - none
  - not
  - compare
  - exists
  - date_window
- fieldPath
- expectedValue
- children
- explanation
```

## Acceptable alternative

Some requirements can be satisfied by one of several evidence or process alternatives.

```text
AcceptableAlternative
- alternativeId
- requirementId
- alternativeGroupKey
- title
- description
- status
  - active
  - inactive
  - deprecated
- evidenceRequirementRefs
- conditionLogic
- explanation
```

Example:

```text
Requirement
- Driver qualification evidence required

Acceptable alternatives
- Current medical examiner certificate
- Valid medical certification status from authorized system
- Approved exemption documentation
```

## Exception

An exception is a condition that changes or removes normal requirement applicability.

```text
Exception
- exceptionId
- exceptionKey
- requirementId
- title
- description
- exceptionType
  - applicability_exception
  - frequency_exception
  - documentation_exception
  - operational_exception
  - temporary_exception
- status
  - draft
  - active
  - deprecated
- conditionLogic
- requiredEvidenceRefs
- effect
  - requirement_not_applicable
  - requirement_modified
  - warning_only
  - reduced_frequency
  - alternate_evidence_allowed
- citationRefs
- explanation
```

## Exemption

An exemption is a formal relief or alternate compliance path, usually requiring eligibility and evidence.

```text
Exemption
- exemptionId
- exemptionKey
- requirementId
- title
- description
- exemptionType
  - regulatory
  - customer
  - internal
  - temporary
  - emergency
- status
  - draft
  - active
  - expired
  - deprecated
- eligibilityLogic
- approvalRequired
- evidenceRequirementRefs
- expirationRules
- citationRefs
- explanation
```

## Retention rule

```text
RetentionRule
- retentionRuleId
- retentionRuleKey
- requirementId
- title
- description
- appliesToRecordTypes
- retentionDuration
- retentionUnit
  - days
  - months
  - years
  - indefinite
- retentionStartTrigger
  - created_at
  - effective_at
  - expiration_at
  - closure_at
  - termination_at
  - superseded_at
  - incident_date
- legalHoldOverrides
- disposalAction
  - review
  - archive
  - purge
- complianceRefs
```

## Requirement evaluation order

```text
1. Resolve evaluated object facts.
2. Determine applicable rulepacks.
3. Determine applicable requirements.
4. Check exceptions.
5. Check exemptions.
6. Evaluate evidence and facts.
7. Evaluate acceptable alternatives.
8. Assign requirement result.
9. Produce explanation.
10. Produce missing/invalid evidence list.
```

## Requirement result statuses

```text
pass
- Requirement is satisfied.

fail
- Requirement is applicable and not satisfied.

warning
- Requirement may be satisfied but has a warning, expiration, weak evidence, or edge condition.

not_applicable
- Requirement does not apply.

unknown
- Facts are insufficient to determine applicability or compliance.

manual_review
- System cannot safely decide without human review.

exempt
- Requirement is satisfied through exemption.

exception_applied
- Requirement changed or removed by exception.
```

## Events

```text
compliancecore.requirement.created
compliancecore.requirement.updated
compliancecore.requirement.activated
compliancecore.requirement.superseded
compliancecore.applicability_rule.created
compliancecore.applicability_rule.updated
compliancecore.compliance_logic.created
compliancecore.compliance_logic.updated
compliancecore.exception.created
compliancecore.exemption.created
compliancecore.retention_rule.created
```


---


# Compliance Core — Evidence and Evaluation Model

## Evidence type

Compliance Core defines what kind of evidence can satisfy a requirement. RecordArr stores the actual document/file/record.

```text
EvidenceType
- evidenceTypeId
- evidenceTypeKey
- displayName
- description
- evidenceCategory
  - document
  - photo
  - signature
  - inspection_record
  - training_record
  - certificate
  - maintenance_record
  - route_record
  - inventory_record
  - supplier_record
  - customer_record
  - quality_record
  - system_record
  - manual_attestation
- allowedRecordTypes
- acceptableSourceProducts
- requiresExpirationDate
- requiresIssuer
- requiresSignature
- requiresHumanConfirmation
- requiresOriginal
- allowsCopy
- defaultRetentionRuleRef
- status
  - active
  - deprecated
  - archived
```

## Evidence requirement

```text
EvidenceRequirement
- evidenceRequirementId
- requirementId
- evidenceTypeId
- title
- description
- required
- alternativesGroupKey
- minimumConfidence
  - low
  - medium
  - high
  - verified
- validityRules
- freshnessRules
- retentionRuleRefs
- allowedSourceProducts
- allowedDocumentTypes
- requiresHumanConfirmation
- missingMessage
- invalidMessage
- warningMessage
```

## Evidence validity rule

```text
EvidenceValidityRule
- validityRuleId
- evidenceRequirementId
- ruleType
  - document_type
  - expiration_date
  - issue_date
  - signature_present
  - issuer_present
  - field_value
  - object_match
  - source_product
  - human_confirmed
  - not_superseded
  - not_rejected
  - retention_active
- fieldPath
- operator
- expectedValue
- failureMessage
```

## Evidence reference

An EvidenceRef points to a RecordArr record or product source fact.

```text
EvidenceRef
- evidenceRefId
- sourceProduct
- sourceObjectRef
- recordarrRecordId
- evidenceTypeKey
- documentTypeSnapshot
- statusSnapshot
  - active
  - expired
  - rejected
  - superseded
  - missing
  - unknown
- issuedAtSnapshot
- expiresAtSnapshot
- confidenceScore
- humanConfirmed
- confirmedByPersonId
- confirmedAt
- extractedFields
```

## Evidence mapping suggestion

```text
EvidenceMappingSuggestion
- suggestionId
- tenantId
- sourceProduct
- sourceObjectRef
- recordarrRecordId
- suggestedRequirementRef
- suggestedEvidenceTypeRef
- confidenceScore
- reason
- extractedFieldMatches
- aliasMatches
- status
  - suggested
  - accepted
  - rejected
  - superseded
- createdAt
```

## Evidence mapping confirmation

```text
EvidenceMappingConfirmation
- confirmationId
- suggestionId
- tenantId
- recordarrRecordId
- requirementId
- evidenceTypeId
- confirmedByPersonId
- confirmedAt
- decision
  - accepted
  - rejected
  - changed
- correctedRequirementRef
- correctedEvidenceTypeRef
- notes
```

## Compliance evaluation

A ComplianceEvaluation is the result of evaluating an object or situation against rulepacks/requirements.

```text
ComplianceEvaluation
- evaluationId
- tenantId
- evaluationNumber
- evaluationType
  - product_object
  - evidence_package
  - theoretical_situation
  - audit_scope
  - import_batch
  - manual_review
- evaluatedObjectRef
- evaluatedFacts
- rulepackRefs
- requirementResultRefs
- status
  - compliant
  - warning
  - noncompliant
  - not_applicable
  - unknown
  - insufficient_information
  - manual_review_required
- confidenceScore
- evaluatedAt
- evaluatedBy
  - system
  - person
- evaluatedByPersonId
- sourceProduct
- sourceObjectRef
- missingEvidence
- invalidEvidence
- expiringEvidence
- applicableExceptions
- applicableExemptions
- acceptableAlternatives
- shortExplanation
- detailedExplanation
- edgeCases
- recordRefs
- auditTrail
```

## Requirement result

```text
RequirementResult
- requirementResultId
- evaluationId
- requirementId
- requirementKey
- resultStatus
  - pass
  - fail
  - warning
  - not_applicable
  - unknown
  - manual_review
  - exempt
  - exception_applied
- applicabilityStatus
  - applicable
  - not_applicable
  - unknown
- evidenceRefs
- satisfiedEvidenceRequirementRefs
- missingEvidenceRequirementRefs
- invalidEvidenceRefs
- expiringEvidenceRefs
- exceptionRefs
- exemptionRefs
- acceptableAlternativeRef
- confidenceScore
- explanation
- requiredAction
```

## Missing evidence

```text
MissingEvidence
- missingEvidenceId
- evaluationId
- requirementId
- evidenceRequirementId
- evidenceTypeKey
- required
- alternativesGroupKey
- message
- acceptableAlternatives
- recommendedSourceProduct
```

## Invalid evidence

```text
InvalidEvidence
- invalidEvidenceId
- evaluationId
- requirementId
- evidenceRef
- invalidReason
  - expired
  - wrong_type
  - wrong_person
  - wrong_asset
  - wrong_location
  - missing_signature
  - missing_issuer
  - superseded
  - rejected
  - low_confidence
  - not_confirmed
  - outside_date_window
- message
- requiredCorrection
```

## Compliance status snapshot

Products can store snapshots from Compliance Core.

```text
ComplianceStatusSnapshot
- snapshotId
- tenantId
- targetProduct
- targetObjectRef
- overallStatus
  - compliant
  - warning
  - noncompliant
  - not_applicable
  - unknown
- evaluationRef
- evaluatedAt
- missingEvidenceCount
- invalidEvidenceCount
- warningCount
- criticalFailureCount
- nextReviewAt
- expiresAt
```

## Evaluation explanation

```text
EvaluationExplanation
- explanationId
- evaluationId
- audience
  - worker
  - supervisor
  - compliance_admin
  - auditor
  - developer
- summary
- details
- citations
- edgeCases
- recommendedActions
```

## Confidence scoring

```text
Confidence factors
- Rulepack applicability certainty
- Object fact completeness
- Evidence mapping confidence
- OCR/extraction confidence
- Human confirmation status
- Evidence freshness
- Source product trust
- Exception/exemption clarity
- Conflict between records
```

## Evaluation workflows

## Product object evaluation

```text
1. Product submits object facts and evidence refs.
2. Compliance Core determines applicable rulepacks.
3. Compliance Core evaluates applicability.
4. Compliance Core evaluates requirements.
5. Compliance Core checks evidence validity.
6. Compliance Core applies exceptions/exemptions.
7. Compliance Core returns evaluation result.
8. Product stores compliance status snapshot.
```

## Evidence package evaluation

```text
1. RecordArr or product submits evidence package.
2. Compliance Core maps records to requirements.
3. Compliance Core identifies missing/invalid evidence.
4. User confirms mapping where needed.
5. Compliance Core returns package readiness status.
```

## Manual review workflow

```text
1. Evaluation returns manual_review_required.
2. Compliance reviewer opens evaluation.
3. Reviewer inspects facts, evidence, exceptions, and citations.
4. Reviewer accepts, rejects, or corrects result.
5. Compliance Core records reviewer decision.
```

## Events

```text
compliancecore.evidence_type.created
compliancecore.evidence_requirement.created
compliancecore.evidence_mapping.suggested
compliancecore.evidence_mapping.confirmed
compliancecore.evidence_mapping.rejected
compliancecore.evaluation.requested
compliancecore.evaluation.completed
compliancecore.evaluation.manual_review_required
compliancecore.evaluation.reviewed
compliancecore.status_snapshot.published
```


---


# Compliance Core — Theoretical Situation, Import, and Mapping Model

## Theoretical Situation Evaluation

Theoretical Situation Evaluation allows a user to test a controlled scenario without freetyping a legal question. It evaluates selected facts, evidence states, classifications, and context against applicable rulepacks.

The user should not need to select rulepacks manually. Compliance Core should infer likely applicable rulepacks and avoid overwhelming edge-case guidance unless the edge case materially affects the answer.

## TheoreticalSituationEvaluation shape

```text
TheoreticalSituationEvaluation
- tseId
- tenantId
- tseNumber
- title
- description
- situationType
  - driver_documentation
  - vehicle_inspection
  - vehicle_maintenance
  - workplace_safety
  - warehouse_storage
  - training_qualification
  - inventory_handling
  - supplier_documentation
  - customer_requirement
  - route_transportation
  - incident_response
  - hazmat_handling
  - environmental_record
  - quality_nonconformance
  - document_retention
  - other
- selectedFacts
- selectedEvidenceStates
- selectedClasses
- selectedJurisdictions
- selectedObjectTypes
- inferredRulepackRefs
- excludedRulepackRefs
- evaluationRef
- resultStatus
  - likely_compliant
  - likely_noncompliant
  - warning
  - insufficient_information
  - not_applicable
  - manual_review_recommended
- missingEvidence
- invalidEvidence
- acceptableAlternatives
- applicableExceptions
- applicableExemptions
- shortExplanation
- detailedExplanation
- edgeCases
- createdAt
- createdByPersonId
- recordRefs
```

## TSE selected fact

```text
TseSelectedFact
- factId
- factKey
- displayName
- value
- valueType
  - boolean
  - enum
  - number
  - date
  - duration
  - object_class
  - evidence_state
- source
  - user_selected
  - inferred
  - defaulted
- confidence
```

## TSE evidence state

```text
TseEvidenceState
- evidenceStateId
- evidenceTypeKey
- state
  - exists_valid
  - exists_invalid
  - exists_expired
  - does_not_exist
  - unknown
  - alternative_exists
- confirmed
- notes
```

## TSE workflow

```text
1. User chooses situation type.
2. Compliance Core presents controlled fields.
3. User selects facts, classes, and evidence states.
4. Compliance Core infers applicable rulepacks.
5. Compliance Core suppresses irrelevant rulepacks.
6. Compliance Core evaluates requirements.
7. Result shows likely compliant/noncompliant/missing/invalid evidence.
8. Exceptions/exemptions are shown only when relevant.
9. Edge cases are summarized without overwhelming the user.
10. User can export a scenario report through ReportArr or store package in RecordArr.
```

## Import batch

Compliance Core should support importing structured catalogs, citations, requirements, aliases, evidence mappings, and rulepack rows from CSV/JSON/Markdown sources.

```text
ImportBatch
- importBatchId
- tenantId
- importNumber
- importType
  - governing_bodies
  - citations
  - rulepacks
  - requirements
  - evidence_types
  - evidence_requirements
  - exceptions
  - exemptions
  - aliases
  - full_rulepack
- sourceFormat
  - csv
  - json
  - markdown
  - xlsx
  - api
- sourceRecordRef
- status
  - uploaded
  - parsed
  - validating
  - validation_failed
  - ready_for_review
  - imported
  - partially_imported
  - canceled
- uploadedByPersonId
- uploadedAt
- parsedAt
- importedAt
- importedByPersonId
- rowCount
- validRowCount
- invalidRowCount
- validationIssueRefs
- importResultRefs
```

## Import row

```text
ImportRow
- importRowId
- importBatchId
- rowNumber
- rawData
- parsedObjectType
- parsedObjectKey
- validationStatus
  - valid
  - warning
  - error
  - skipped
- validationIssueRefs
- targetObjectRef
- action
  - create
  - update
  - skip
  - duplicate
  - conflict
```

## Validation issue

```text
ValidationIssue
- validationIssueId
- importBatchId
- importRowId
- severity
  - info
  - warning
  - error
  - blocking
- issueType
  - missing_required_field
  - invalid_key
  - duplicate_key
  - unknown_citation
  - unknown_requirement
  - invalid_logic
  - invalid_evidence_type
  - circular_reference
  - version_conflict
  - unsafe_overwrite
  - ambiguous_mapping
- message
- suggestedFix
- fieldName
```

## Import review

```text
ImportReview
- importReviewId
- importBatchId
- reviewerPersonId
- status
  - pending
  - in_review
  - approved
  - rejected
  - changes_requested
- reviewedAt
- decisionNotes
```

## Evidence mapping wizard

The evidence mapping wizard guides the user through mapping imported or uploaded evidence to compliance requirements.

```text
EvidenceMappingWizardSession
- sessionId
- tenantId
- sourceProduct
- sourceObjectRef
- recordRefs
- status
  - open
  - in_progress
  - completed
  - canceled
- currentStep
- suggestionRefs
- confirmedMappingRefs
- rejectedSuggestionRefs
- startedByPersonId
- startedAt
- completedAt
```

## Evidence mapping wizard item

```text
EvidenceMappingWizardItem
- itemId
- sessionId
- recordRef
- suggestedRequirementRef
- suggestedEvidenceTypeRef
- suggestedContext
- confidenceScore
- decision
  - pending
  - accept
  - change
  - reject
  - skip
- correctedRequirementRef
- correctedEvidenceTypeRef
- decidedByPersonId
- decidedAt
- notes
```

## Rulepack diff

Compliance Core should be able to compare rulepack versions or imported citation/requirement sets.

```text
RulepackDiff
- diffId
- tenantId
- baseRulepackVersionRef
- compareRulepackVersionRef
- status
  - pending
  - completed
  - failed
- addedRequirements
- removedRequirements
- changedRequirements
- addedCitations
- removedCitations
- changedCitations
- changedEvidenceRequirements
- changedApplicabilityRules
- riskSummary
- generatedAt
```

## Change impact analysis

```text
ChangeImpactAnalysis
- impactAnalysisId
- tenantId
- sourceChangeRef
- affectedRulepackRefs
- affectedRequirementRefs
- affectedProductObjectTypes
- affectedEvidenceTypes
- estimatedAffectedObjectCount
- severity
  - low
  - moderate
  - high
  - critical
- recommendedActions
- generatedAt
```

## Import workflow

```text
1. User uploads import file to RecordArr.
2. Compliance Core creates ImportBatch.
3. Compliance Core parses rows.
4. Compliance Core validates keys, references, logic, citations, and evidence types.
5. Blocking issues must be fixed.
6. Reviewer approves import.
7. Compliance Core creates/updates target objects.
8. Compliance Core publishes catalog/rulepack events.
9. ReportArr can display import status.
```

## Evidence mapping wizard workflow

```text
1. Product or RecordArr submits evidence records.
2. Compliance Core suggests mappings.
3. User reviews one item at a time.
4. User accepts, changes, rejects, or skips.
5. Compliance Core records mapping confidence and human decision.
6. Evaluations can use confirmed mappings.
```

## Rulepack diff workflow

```text
1. User selects two rulepack versions.
2. Compliance Core compares citations, requirements, evidence rules, and applicability logic.
3. Compliance Core flags added/removed/changed requirements.
4. Compliance Core produces operational impact estimate.
5. User decides whether to activate new version or review affected objects.
```

## Events

```text
compliancecore.tse.created
compliancecore.tse.completed
compliancecore.import_batch.created
compliancecore.import_batch.parsed
compliancecore.import_batch.validation_failed
compliancecore.import_batch.ready_for_review
compliancecore.import_batch.imported
compliancecore.import_batch.partially_imported
compliancecore.import_review.approved
compliancecore.import_review.rejected
compliancecore.mapping_wizard.created
compliancecore.mapping_wizard.completed
compliancecore.rulepack_diff.completed
compliancecore.change_impact.completed
```


---


# Compliance Core — Workflows, Status Logic, Events, and APIs

## Major workflow: product compliance evaluation

```text
1. Product gathers object facts and evidence refs.
2. Product calls Compliance Core evaluation endpoint.
3. Compliance Core resolves applicable rulepacks.
4. Compliance Core evaluates applicability.
5. Compliance Core applies exceptions and exemptions.
6. Compliance Core evaluates evidence requirements.
7. Compliance Core checks acceptable alternatives.
8. Compliance Core returns evaluation, requirement results, missing evidence, invalid evidence, warnings, and explanation.
9. Product stores ComplianceStatusSnapshot.
10. ReportArr consumes evaluation facts if needed.
```

## Major workflow: rulepack authoring

```text
1. Compliance admin creates rulepack.
2. Admin selects governing body, jurisdiction, source, and domain.
3. Admin creates citation references.
4. Admin creates requirements.
5. Admin defines applicability logic.
6. Admin defines evidence requirements.
7. Admin defines exceptions/exemptions.
8. Admin validates rulepack.
9. Reviewer approves.
10. Rulepack becomes active.
```

## Major workflow: evidence mapping

```text
1. RecordArr or product submits evidence record refs.
2. Compliance Core analyzes record metadata, OCR fields, aliases, and context.
3. Compliance Core creates mapping suggestions.
4. User confirms, changes, rejects, or skips suggestions.
5. Confirmed mappings become usable evidence.
6. Compliance evaluations use confirmed mappings.
```

## Major workflow: theoretical situation evaluation

```text
1. User opens TSE.
2. User selects situation type.
3. User answers controlled fields.
4. Compliance Core infers applicable rulepacks.
5. Compliance Core evaluates selected facts and evidence states.
6. Compliance Core returns likely compliant/noncompliant/warning/unknown.
7. User can export scenario report or save package.
```

## Major workflow: rulepack import

```text
1. User uploads import source.
2. Compliance Core parses import rows.
3. Compliance Core validates keys, citations, logic, evidence types, exceptions, and references.
4. Validation issues are shown.
5. Reviewer approves import.
6. Compliance Core creates/updates objects.
7. New rulepack/catalog version is activated when approved.
```

## Major workflow: audit package evaluation

```text
1. User selects audit scope in ReportArr or Compliance Core.
2. Compliance Core resolves applicable rulepacks and requirements.
3. RecordArr supplies evidence package.
4. Products supply operational object facts.
5. Compliance Core evaluates requirement coverage.
6. Missing/invalid evidence is returned.
7. ReportArr renders audit report.
8. RecordArr stores final audit package.
```

## Compliance Core emitted events

```text
compliancecore.governing_body.created
compliancecore.governing_body.updated

compliancecore.catalog.created
compliancecore.catalog.updated
compliancecore.catalog.entry_created
compliancecore.catalog.entry_updated

compliancecore.citation.created
compliancecore.citation.updated
compliancecore.citation.superseded

compliancecore.rulepack.created
compliancecore.rulepack.updated
compliancecore.rulepack.submitted_for_review
compliancecore.rulepack.approved
compliancecore.rulepack.activated
compliancecore.rulepack.superseded
compliancecore.rulepack.deprecated

compliancecore.requirement.created
compliancecore.requirement.updated
compliancecore.requirement.activated
compliancecore.requirement.superseded

compliancecore.applicability_rule.created
compliancecore.compliance_logic.created
compliancecore.exception.created
compliancecore.exemption.created
compliancecore.retention_rule.created

compliancecore.evidence_type.created
compliancecore.evidence_requirement.created
compliancecore.evidence_mapping.suggested
compliancecore.evidence_mapping.confirmed
compliancecore.evidence_mapping.rejected

compliancecore.evaluation.requested
compliancecore.evaluation.completed
compliancecore.evaluation.manual_review_required
compliancecore.evaluation.reviewed
compliancecore.status_snapshot.published

compliancecore.tse.created
compliancecore.tse.completed

compliancecore.import_batch.created
compliancecore.import_batch.parsed
compliancecore.import_batch.validation_failed
compliancecore.import_batch.ready_for_review
compliancecore.import_batch.imported

compliancecore.rulepack_diff.completed
compliancecore.change_impact.completed
```

## Integration APIs Compliance Core should expose

```text
GET /api/v1/catalogs
GET /api/v1/catalogs/{catalogKey}
GET /api/v1/catalogs/{catalogKey}/entries

GET /api/v1/governing-bodies
GET /api/v1/governing-bodies/{governingBodyId}
GET /api/v1/jurisdictions
GET /api/v1/regulation-sources
GET /api/v1/citations
GET /api/v1/citations/{citationId}

GET /api/v1/rulepacks
GET /api/v1/rulepacks/{rulepackId}
GET /api/v1/rulepacks/{rulepackId}/versions
POST /api/v1/rulepacks
POST /api/v1/rulepacks/{rulepackId}/submit-review
POST /api/v1/rulepacks/{rulepackId}/approve
POST /api/v1/rulepacks/{rulepackId}/activate

GET /api/v1/requirements
GET /api/v1/requirements/{requirementId}
POST /api/v1/requirements

GET /api/v1/evidence-types
GET /api/v1/evidence-requirements
POST /api/v1/evidence-mapping/suggest
POST /api/v1/evidence-mapping/confirm
POST /api/v1/evidence-mapping/reject

POST /api/v1/evaluations
GET /api/v1/evaluations/{evaluationId}
POST /api/v1/evaluations/{evaluationId}/review

POST /api/v1/tse/evaluate
GET /api/v1/tse/{tseId}

POST /api/v1/import-batches
GET /api/v1/import-batches/{importBatchId}
POST /api/v1/import-batches/{importBatchId}/validate
POST /api/v1/import-batches/{importBatchId}/approve
POST /api/v1/import-batches/{importBatchId}/import

POST /api/v1/rulepack-diffs
GET /api/v1/rulepack-diffs/{diffId}
POST /api/v1/change-impact-analyses
```

## APIs Compliance Core should consume

```text
NexArr
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}

StaffArr
- GET /persons/{personId}
- GET /locations/{locationId}
- GET /sites/{siteOrgUnitId}
- POST /permission-checks

TrainArr
- GET /qualification-definitions
- GET /persons/{personId}/qualifications

MaintainArr
- GET /assets/{assetId}
- GET /work-orders/{workOrderId}
- GET /inspections/{inspectionId}

LoadArr
- GET /inventory-items/{itemId}
- GET /receipts/{receiptId}
- GET /stock-movements/{movementId}

SupplyArr
- GET /suppliers/{supplierId}
- GET /purchase-orders/{purchaseOrderId}

RoutArr
- GET /trips/{tripId}
- GET /routes/{routeId}

CustomArr
- GET /customers/{customerId}
- GET /customer-locations/{locationId}

OrdArr
- GET /orders/{orderId}

RecordArr
- GET /records/{recordId}
- GET /record-packages/{packageId}

AssurArr
- GET /nonconformances/{nonconformanceId}
- GET /capas/{capaId}
- GET /holds/{holdId}

ReportArr
- POST /events
```

## Permission examples

```text
compliancecore.catalogs.read
compliancecore.catalogs.manage

compliancecore.governing_bodies.read
compliancecore.governing_bodies.manage

compliancecore.citations.read
compliancecore.citations.manage

compliancecore.rulepacks.read
compliancecore.rulepacks.create
compliancecore.rulepacks.update
compliancecore.rulepacks.review
compliancecore.rulepacks.approve
compliancecore.rulepacks.activate
compliancecore.rulepacks.deprecate

compliancecore.requirements.read
compliancecore.requirements.create
compliancecore.requirements.update
compliancecore.requirements.review

compliancecore.evidence_types.read
compliancecore.evidence_types.manage
compliancecore.evidence_mapping.suggest
compliancecore.evidence_mapping.confirm

compliancecore.evaluations.run
compliancecore.evaluations.read
compliancecore.evaluations.review

compliancecore.tse.run
compliancecore.tse.read

compliancecore.imports.create
compliancecore.imports.validate
compliancecore.imports.approve
compliancecore.imports.execute

compliancecore.admin
```

## Default role examples

```text
Compliance Viewer
- Read catalogs, rulepacks, requirements, citations, and evaluations.

Compliance Evaluator
- Run evaluations.
- Run TSE.
- Read evidence mapping suggestions.

Evidence Mapping Reviewer
- Confirm/reject evidence mappings.
- Review low-confidence evidence.

Rulepack Author
- Create and edit draft rulepacks, requirements, logic, and evidence rules.

Rulepack Reviewer
- Review rulepacks and request changes.

Compliance Approver
- Approve and activate rulepacks.

Compliance Admin
- Manage catalogs, governing bodies, citations, imports, rulepacks, and settings.

Auditor
- Read evaluations, evidence coverage, rulepack versions, and audit scope results.
```

## Compliance Core UI surfaces

```text
/app/compliancecore
- dashboard
- governing bodies
- jurisdictions
- regulation sources
- citations
- rulepacks
- rulepack detail
- requirements
- requirement detail
- applicability logic builder
- evidence types
- evidence requirements
- exceptions
- exemptions
- retention rules
- evidence mapping wizard
- evaluations
- theoretical situation evaluation
- imports
- rulepack diff
- change impact
- settings
```

## Rulepack detail UI

```text
RulepackDetailPage
- Header
  - rulepack key
  - title
  - version
  - status
  - governing bodies
  - effective date
- Citations
- Requirements
- Applicability rules
- Evidence requirements
- Exceptions/exemptions
- Retention rules
- Validation issues
- Version history
- Activation controls
```

## Requirement detail UI

```text
RequirementDetailPage
- Header
  - requirement key
  - title
  - type
  - severity
  - status
- Plain-language summary
- Citations
- Applicability logic
- Compliance logic
- Evidence requirements
- Acceptable alternatives
- Exceptions
- Exemptions
- Retention
- Related requirements
- Test evaluation panel
```

## TSE UI

```text
TheoreticalSituationEvaluationPage
- Situation type selector
- Controlled facts
- Evidence exists/does-not-exist/valid/invalid fields
- Optional class/category selectors
- Evaluation result
- Missing evidence
- Acceptable alternatives
- Exceptions/exemptions
- Edge cases
- Export/save action
```
