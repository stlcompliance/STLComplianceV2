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
