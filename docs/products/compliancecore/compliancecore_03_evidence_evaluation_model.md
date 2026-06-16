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

Compliance Core owns evidence meaning, mapping suggestions, evidence requirement satisfaction logic, and evaluation results. RecordArr owns the persistent file-to-evidence linkage when a mapping is confirmed against stored records.

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

EvidenceMappingConfirmation records the compliance decision to accept, reject, or correct a suggested mapping. The confirmed RecordArr record/evidence linkage is stored in RecordArr.

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
