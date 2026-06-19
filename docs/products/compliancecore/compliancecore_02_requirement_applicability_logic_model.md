# Compliance Core — Requirement, Applicability, and Logic Model

## Atomic requirement rule

Compliance Core requirements are atomic, versioned obligations. A requirement should represent one operationally evaluable expectation, prohibition, qualification, deadline, filing, inspection, evidence requirement, retention period, consent requirement, or corrective action.

A requirement should not be an entire statute, regulation, part, subpart, or manual imported as a blob. Broad legal instruments should be modeled as sources, citations, rulepacks, and related requirement sets.

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
- legalInstrumentRefs
- jurisdictionScopeRefs
- territorialScope
- issuingAuthorityRef
- governingBodyRef
- provisionVersion
- publicationDate
- effectiveDate
- enforcementDate
- repealDate
- transitionDate
- lifecycleStatus
  - proposed
  - final_not_effective
  - effective
  - stayed
  - enjoined
  - vacated
  - superseded
  - repealed
- bindingClassificationRef
- regulatedActorOrRole
- regulatedObjectType
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
- applicabilityPredicates
- exemptionRefs
- exceptionRefs
- thresholdRules
- calculationRules
- complianceLogicRef
- evidenceRequirementRefs
- acceptableAlternativeRefs
- retentionRuleRefs
- trigger
- deadlineRule
- intervalRule
- recurrenceRule
- gracePeriodRule
- trainingOrQualificationRequirementRef
- inspectionOrMaintenanceRequirementRef
- filingOrNotificationRecipientRef
- formOrSubmissionSchemaRef
- signerOrAttestationAuthority
- penaltyAndConsequenceMetadata
- remediationExpectations
- federalPreemptionOrStateVarianceTreatment
- owningExecutionApp
- contributingApps
- evidenceProducingApps
- reportingApp
- humanOrCounselReviewStatus
- sourceProvenance
- lastLegalReviewAt
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
