# TrainArr — Program, Module, Step, and Content Model

## Training program

A TrainingProgram defines a training path that can be assigned, completed, evaluated, and used to issue qualifications.

```text
TrainingProgram
- programId
- tenantId
- programNumber
- programKey
- title
- description
- programType
  - onboarding
  - compliance
  - equipment
  - safety
  - process
  - customer_required
  - remedial
  - refresher
  - certification
  - qualification
  - site_access
  - task_authorization
  - other
- category
  - general
  - safety
  - maintenance
  - warehouse
  - transportation
  - quality
  - compliance
  - customer
  - supplier
  - environmental
  - equipment
  - leadership
  - other
- status
  - draft
  - review
  - active
  - paused
  - retired
  - archived
- currentVersionRef
- ownerPersonId
- approverPersonId
- complianceRefs
- qualificationOutcomeRefs
- prerequisiteRules
- renewalRules
- evidenceRequirements
- moduleRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- activatedAt
- retiredAt
- auditTrail
```

## Program status definitions

```text
draft
- Program is being built.

review
- Program is awaiting approval.

active
- Program can be assigned.

paused
- Program is temporarily unavailable for new assignments.

retired
- Program is no longer used for new assignments.

archived
- Program is retained for history.
```

## Training program version

```text
TrainingProgramVersion
- programVersionId
- tenantId
- programId
- version
- versionLabel
- status
  - draft
  - review
  - active
  - superseded
  - retired
  - archived
- changeSummary
- moduleSnapshot
- stepSnapshot
- evidenceRequirementSnapshot
- qualificationOutcomeSnapshot
- effectiveAt
- supersededAt
- approvedByPersonId
- approvedAt
- createdAt
```

## Training module

A module is a section of a training program.

```text
TrainingModule
- moduleId
- tenantId
- programId
- programVersionId
- moduleNumber
- title
- description
- sequence
- moduleType
  - reading
  - video
  - classroom
  - hands_on
  - observation
  - written_test
  - practical_evaluation
  - acknowledgement
  - document_review
  - field_demonstration
  - supervisor_review
- status
  - draft
  - active
  - retired
- required
- estimatedDurationMinutes
- passingCriteria
- contentRefs
- stepRefs
- evidenceRequirements
- createdAt
- updatedAt
```

## Training step

A step is the smallest assigned unit of completion/signoff.

```text
TrainingStep
- stepId
- tenantId
- moduleId
- programId
- programVersionId
- stepNumber
- title
- instructions
- sequence
- stepType
  - read
  - watch
  - listen
  - demonstrate
  - observe
  - answer_question
  - upload_evidence
  - trainer_signoff
  - evaluator_signoff
  - trainee_acknowledgement
  - supervisor_acknowledgement
  - practical_task
  - written_question
  - quiz
  - field_observation
- status
  - draft
  - active
  - retired
- required
- requiresTrainer
- requiresEvaluator
- requiresSupervisor
- requiresEvidence
- requiresSignature
- allowedEvidenceTypes
- responseSchema
- passFailEnabled
- scoringRules
- remediationOnFail
- complianceRefs
- recordTemplateRefs
```

## Training content reference

RecordArr owns files. TrainArr owns content usage within a program.

```text
TrainingContentRef
- contentRefId
- programId
- moduleId
- stepId
- recordarrRecordId
- contentType
  - document
  - video
  - audio
  - image
  - link
  - form
  - generated_page
  - external
- titleSnapshot
- versionSnapshot
- required
- displayOrder
- effectiveAt
```

## Question item

```text
QuestionItem
- questionId
- stepId
- questionType
  - multiple_choice
  - multi_select
  - true_false
  - short_answer
  - numeric
  - acknowledgement
- prompt
- helpText
- options
- correctAnswerRules
- points
- required
- explanation
```

## Practical criterion

```text
PracticalCriterion
- criterionId
- stepId
- title
- description
- required
- passFail
- scoringMethod
  - pass_fail
  - numeric_score
  - rubric
  - observation_only
- minimumScore
- evidenceRequired
```

## Rubric

```text
Rubric
- rubricId
- stepId
- title
- description
- maximumScore
- passingScore
- criterionRefs
```

## Program prerequisite rule

```text
ProgramPrerequisiteRule
- prerequisiteRuleId
- programId
- prerequisiteType
  - program_completion
  - qualification_active
  - position
  - site
  - department
  - permission
  - manual_approval
  - other
- prerequisiteRef
- required
- failureMessage
```

## Program outcome

A program may issue one or more qualifications/certificates.

```text
ProgramOutcome
- outcomeId
- programId
- qualificationDefinitionRef
- certificateTemplateRef
- issueWhen
  - all_required_steps_complete
  - all_modules_passed
  - evaluator_approved
  - manual_review
- expirationRuleRef
- publishToStaffArr
```

## Certificate template

```text
CertificateTemplate
- certificateTemplateId
- tenantId
- title
- description
- status
  - draft
  - active
  - retired
- templateRecordRef
- fields
- signatureRequirements
- layoutSettings
```

## Program builder workflow

```text
1. User creates TrainingProgram.
2. User defines program type/category/outcomes.
3. User adds modules.
4. User adds steps.
5. User attaches RecordArr content.
6. User defines evidence/signoff/evaluation requirements.
7. User defines prerequisites and renewal rules.
8. User maps Compliance Core requirements/citations where applicable.
9. Program is reviewed and approved.
10. Program becomes active and assignable.
```

## Program versioning workflow

```text
1. User creates draft version from active program.
2. User changes modules, steps, evidence, or outcomes.
3. Version is reviewed.
4. New version becomes active.
5. Existing assignments remain tied to their assigned version unless migrated by policy.
6. Old version becomes superseded/retired.
```

## Events

```text
trainarr.program.created
trainarr.program.updated
trainarr.program.submitted_for_review
trainarr.program.approved
trainarr.program.activated
trainarr.program.paused
trainarr.program.retired
trainarr.program.archived

trainarr.program_version.created
trainarr.program_version.activated
trainarr.program_version.superseded

trainarr.module.created
trainarr.module.updated
trainarr.step.created
trainarr.step.updated
trainarr.content_ref.added
trainarr.outcome.created
```
