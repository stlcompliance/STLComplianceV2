# Compliance Core — Questionnaire Engine Model

## Purpose

The Compliance Core questionnaire engine asks plain-language operational questions and converts answers into normalized compliance facts.

Products render short, context-aware questionnaires.

Compliance Core owns:

- question definitions
- answer options
- applicability of questions
- fact mappings
- follow-up generation
- compliance fact interpretation
- confidence and conflict states

Products own:

- where the question is shown
- the source workflow record
- whether the product workflow can proceed
- product-owned tasks, blockers, and status changes

## Prime directive

Users should answer operational facts, not legal conclusions.

Good questions:

```text
- Is this vehicle used in interstate commerce?
- Is the material flammable?
- Is the employee operating a powered industrial truck?
- Is this order going to a customer location with special delivery requirements?
- Was the defect found during pre-trip, post-trip, PM, or repair?
```

Bad questions:

```text
- Is 49 CFR 395 applicable?
- Is OSHA 1910.178 triggered?
- Is this a regulated hazmat shipping event?
```

Compliance Core interprets answers using rulepacks and applicability logic.

## Required applicability fact families

The rule engine cannot decide applicability from NAICS alone. Questionnaires, imports, tenant profiles, and product events should be able to gather facts in these families:

```text
- Employer and employee counts by location and period
- Legal-entity type and formation jurisdiction
- Worksite and employee work locations
- Regulated role: motor carrier, broker, shipper, loader, receiver, freight forwarder, importer, exporter, manufacturer, distributor, retailer, government contractor, and similar roles
- Interstate versus intrastate operations
- Vehicle weight, passenger capacity, use, and cargo
- Hazardous-material classes and quantities
- Facility chemical and fuel storage
- Waste streams and generator status
- Food, drug, device, chemical, consumer-product, or controlled-product handling
- Personal-data categories and data-subject jurisdictions
- Marketing channels and consent sources
- Consumer versus business customers
- Credit offered or reports obtained
- Contract clauses and award types
- Sites, permits, discharges, emissions, and tanks
- Shipment origin, destination, transit jurisdictions, and border crossings
- Public-company, healthcare, financial-institution, educational, or public-sector status
```

Unknown or conflicting answers should create reviewable follow-up needs instead of silently suppressing rulepacks.

## Questionnaire definition

```text
QuestionnaireDefinition
- questionnaireId
- questionnaireKey
- title
- description
- status
  - draft
  - active
  - deprecated
  - archived
- productApplicability
- workflowApplicability
- subjectTypeApplicability
- rulepackRefs
- triggerRefs
- questionRefs
- priority
- version
- effectiveAt
- supersededAt
- auditTrail
```

## Questionnaire context

Products request a questionnaire using context.

```text
QuestionnaireContext
- tenantId
- productKey
- workflowKey
- subjectType
- subjectRef
- sourceRecordRef
- knownFactRefs
- existingAnswerRefs
- sourceSnapshot
- requestedByPersonId
- locale
- urgency
```

Examples:

```text
- productKey: maintainarr
  workflowKey: asset_create
  subjectType: maintainarr.asset

- productKey: routarr
  workflowKey: trip_dispatch
  subjectType: routarr.trip

- productKey: supplyarr
  workflowKey: supplier_onboarding
  subjectType: supplyarr.supplier

- productKey: ordarr
  workflowKey: order_triage
  subjectType: ordarr.order
```

## Question definition

```text
QuestionDefinition
- questionId
- questionKey
- title
- plainLanguagePrompt
- helpText
- questionType
  - yes_no
  - single_select
  - multi_select
  - number
  - date
  - text_short
  - text_long
  - file_reference
  - person_reference
  - location_reference
  - asset_reference
  - item_reference
- answerOptionRefs
- requiredness
  - required
  - optional
  - conditional
- applicabilityLogicRef
- displayConditionRefs
- followUpRefs
- factMappingRefs
- validationRules
- confidenceRules
- status
```

## Answer option

```text
AnswerOption
- answerOptionId
- questionId
- optionKey
- displayText
- helpText
- sortOrder
- factMappingRefs
- followUpTriggerRefs
- conflictTriggerRefs
- status
```

## Fact mapping

A FactMapping converts an answer into a normalized fact.

```text
FactMapping
- factMappingId
- questionId
- answerCondition
- factTypeKey
- factValue
- factSubjectType
- factSubjectRefPath
- confidenceScore
- effectiveDateRule
- sourceContextRule
- expiresAfter
- rulepackRefs
- notes
```

## Compliance fact

```text
ComplianceFact
- complianceFactId
- tenantId
- factTypeKey
- subjectType
- subjectRef
- value
- valueType
  - boolean
  - enum
  - number
  - date
  - text
  - reference
- sourceProduct
- sourceWorkflowKey
- sourceRecordRef
- sourceQuestionnaireSessionRef
- sourceQuestionRef
- sourceAnswerRef
- confidenceScore
- reviewStatus
  - accepted
  - suggested
  - needs_review
  - conflicted
  - rejected
  - expired
- effectiveAt
- expiresAt
- createdAt
- auditTrail
```

Unknown and conflicted facts are valid outcomes.

They should create reviewable follow-up needs, not hard validation failures by default.

## Questionnaire session

```text
QuestionnaireSession
- questionnaireSessionId
- tenantId
- questionnaireDefinitionRef
- questionnaireVersion
- productKey
- workflowKey
- subjectType
- subjectRef
- sourceRecordRef
- status
  - created
  - in_progress
  - answered
  - partially_answered
  - needs_follow_up
  - conflict_detected
  - complete
  - canceled
  - expired
- requestedByPersonId
- answeredByPersonId
- startedAt
- completedAt
- answerRefs
- generatedFactRefs
- followUpNeedRefs
```

## Questionnaire answer

```text
QuestionnaireAnswer
- answerId
- questionnaireSessionId
- questionId
- answerValue
- selectedOptionRefs
- freeText
- fileRef
- answeredByPersonId
- answeredAt
- source
  - user
  - tenant_default
  - prior_answer
  - import
  - ai_suggested
  - system
- confidenceScore
- reviewStatus
```

AI-suggested answers may prefill suggestions, but a product or human review path must confirm them before they become accepted facts unless the workflow explicitly permits low-risk automation.

## Follow-up need

```text
FollowUpNeed
- followUpNeedId
- tenantId
- questionnaireSessionId
- reason
  - missing_required_fact
  - conflicting_answer
  - low_confidence
  - rulepack_requires_detail
  - evidence_needed
  - reviewer_needed
- prompt
- targetProductKey
- targetWorkflowKey
- assignedQueue
- blockingEffect
  - none
  - warning
  - blocks_current_step
  - blocks_closeout
- status
  - open
  - answered
  - dismissed
  - escalated
  - expired
```

## Tenant compliance profile

The questionnaire engine may maintain a tenant compliance profile made from onboarding answers and accepted facts.

```text
TenantComplianceProfile
- tenantId
- businessProfileFacts
- exposureFacts
- likelyRulepackRefs
- assumptionRefs
- missingFactRefs
- setupChecklistRefs
- lastReviewedAt
- reviewStatus
```

This profile supports reuse of tenant defaults so products ask fewer questions during routine workflows.

## Workflow mini-questionnaires

Examples:

```text
MaintainArr asset create
- vehicle type
- interstate use
- hazardous material support
- regulated inspection evidence

RoutArr trip dispatch
- interstate/intrastate route
- driver qualification implications
- customer delivery restrictions
- equipment readiness blockers

SupplyArr supplier onboarding
- supplied material categories
- SDS need
- insurance/certification need
- quality approval need

LoadArr receiving
- material identification
- package mismatch
- quarantine need
- SDS/evidence need

OrdArr order triage
- customer requirement match
- regulated service exposure
- execution products needed
- missing information

CustomArr customer onboarding
- required documents
- site restrictions
- special handling needs
- communication requirements
```

## Result payload

Products should receive a summarized result.

```text
QuestionnaireResult
- sessionRef
- status
- generatedFactRefs
- likelyApplicableRequirementRefs
- missingFactRefs
- conflictRefs
- followUpNeedRefs
- recommendedNextActions
- blockerRecommendations
- evidenceRecommendations
- explanation
```

Compliance Core may recommend blockers. The owning product creates and owns the actual product blocker.

## API routes

```text
POST /api/v1/compliance-core/questionnaires/resolve
POST /api/v1/compliance-core/questionnaire-sessions
GET /api/v1/compliance-core/questionnaire-sessions/{sessionId}
POST /api/v1/compliance-core/questionnaire-sessions/{sessionId}/answers
POST /api/v1/compliance-core/questionnaire-sessions/{sessionId}/complete
GET /api/v1/compliance-core/facts
POST /api/v1/compliance-core/facts/review
GET /api/v1/compliance-core/tenant-profile
POST /api/v1/compliance-core/tenant-profile/review
```

## Events

```text
compliancecore.questionnaire.resolved
compliancecore.questionnaire_session.created
compliancecore.questionnaire_session.completed
compliancecore.questionnaire_session.follow_up_needed
compliancecore.compliance_fact.created
compliancecore.compliance_fact.conflict_detected
compliancecore.compliance_fact.expired
compliancecore.tenant_profile.updated
```

## UI rules

Products should render questions as native UI controls.

Do not show raw rule JSON.

Do not ask the user to pick rulepacks during routine workflows unless the user is a platform admin doing setup.

Do not make users answer the same tenant-level question repeatedly when an accepted tenant default already exists.

Always show why a question is being asked when the answer may block progress.
