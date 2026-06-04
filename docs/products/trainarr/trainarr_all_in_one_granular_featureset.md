# TrainArr — Scope, Ownership, and Boundaries

## Product purpose

TrainArr is the training, assignment, evaluation, signoff, remediation, qualification, certificate, expiration, and renewal product for the STL Compliance / ARR suite.

TrainArr is the qualification engine. It owns whether a person completed required training and whether that completion produces an active qualification/certificate.

TrainArr answers:

- What training program is required?
- What module or step must be completed?
- Who is assigned?
- What is due or overdue?
- What evidence is required?
- Who trained, evaluated, or signed off?
- Did the person pass?
- Is remediation required?
- What qualification is issued?
- When does it expire?
- Should StaffArr readiness be updated?

## TrainArr owns

```text
- Training program
- Training program version
- Training module
- Training step
- Training content references
- Training requirement profile
- Training assignment
- Module progress
- Step progress
- Trainee acknowledgement
- Trainer signoff
- Evaluator signoff
- Written evaluation result
- Practical evaluation result
- Observation result
- Remediation assignment
- Qualification definition
- Person qualification
- Certificate issuance
- Qualification expiration
- Qualification renewal
- Qualification suspension/revocation
- Training evidence references
- Training-origin events
```

## TrainArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Person employment status
- Org structure
- Site/location identity
- Product permission assignment truth
- Regulatory/rulepack meaning
- Governing body catalog truth
- Document/file storage truth
- Asset truth
- Work order truth
- Inventory truth
- Procurement truth
- Route/trip truth
- Customer/order truth
- Quality hold/release truth
- Reporting read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens

StaffArr
- Person references
- Position, department, team, site, location context
- Permission checks
- Person readiness display
- Incident/remediation requests
- Person history updates

Compliance Core
- Rulepacks
- Training requirement regulatory meaning
- Citation references
- Evidence requirements
- Applicability logic
- Retention requirements

RecordArr
- Training evidence files
- Signed acknowledgements
- Certificates as files
- Evaluation evidence
- Training completion packages

MaintainArr
- Asset/equipment training applicability
- Maintenance skill requirements
- Incident-driven retraining requests

LoadArr
- Warehouse equipment/process training requirements
- Forklift/PIT/receiving/count/pick/issue task qualification checks

SupplyArr
- Procurement/supplier compliance training requirements where applicable

RoutArr
- Driver/route/equipment/customer/site training requirements
- Driver qualification checks before dispatch

CustomArr
- Customer-specific training requirements
- Customer site access/training constraints

OrdArr
- Order/customer/service requirements that depend on qualified workers

AssurArr
- Remediation after quality issue, audit finding, nonconformance, or CAPA
- CAPA actions requiring training completion

ReportArr
- Training dashboards
- Qualification expiration reports
- Completion metrics
- Remediation KPIs

Field Companion
- Mobile training steps
- Trainer/evaluator signoff
- Practical evaluation execution
- Training evidence capture
```

## Core source-of-truth rules

```text
1. TrainArr owns training program definitions.
2. TrainArr owns training assignment execution state.
3. TrainArr owns signoff/evaluation state.
4. TrainArr owns qualification/certificate issuance truth.
5. StaffArr owns the person and readiness/person history view.
6. TrainArr publishes qualification status to StaffArr.
7. Compliance Core owns regulatory meaning and citations.
8. RecordArr owns actual files and evidence records.
9. Origin products own incidents or operational events that trigger retraining.
10. Products may block work based on TrainArr qualification checks.
11. TrainArr should not create product-local permissions.
12. TrainArr should not own StaffArr org/position/site identity.
```

## Standard TrainArr object envelope

```text
TrainArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- version
- sourceProduct
- sourceObjectRef
- personId
- staffarrSiteId
- staffarrLocationId
- complianceRefs
- recordRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- completedAt
- closedAt
- auditTrail
- eventLog
```

## TrainArr object prefixes

```text
TPROG  Training program
TPV    Training program version
TMOD   Training module
TSTEP  Training step
TREQ   Training requirement profile
TASN   Training assignment
MPROG  Module progress
SPROG  Step progress
SIGN   Signoff
EVAL   Evaluation
OBS    Observation
REM    Remediation assignment
QDEF   Qualification definition
QUAL   Person qualification
CERT   Certificate
REN    Renewal event
SUSP   Qualification suspension
REV    Qualification revocation
```

## Standard person training reference

```text
TrainingPersonRef
- personId
- personNumberSnapshot
- displayNameSnapshot
- primaryPositionSnapshot
- primaryDepartmentSnapshot
- primarySiteSnapshot
- statusSnapshot
- lastResolvedAt
```

## Standard qualification status

```text
QualificationStatus
- pending
- active
- expiring_soon
- expired
- suspended
- revoked
- superseded
- not_required
```


---


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


---


# TrainArr — Assignment, Execution, Evaluation, and Signoff Model

## Training assignment

A TrainingAssignment is a person-specific instance of a program/version.

```text
TrainingAssignment
- assignmentId
- tenantId
- assignmentNumber
- programId
- programVersionId
- personId
- assignedByPersonId
- assignmentSource
  - manual
  - position_requirement
  - site_requirement
  - department_requirement
  - team_requirement
  - incident_remediation
  - qualification_renewal
  - compliance_rule
  - customer_requirement
  - asset_requirement
  - route_requirement
  - quality_capa
  - onboarding
  - system
- sourceProduct
- sourceObjectRef
- status
  - assigned
  - not_started
  - in_progress
  - waiting_trainee
  - waiting_trainer
  - waiting_evaluator
  - waiting_supervisor
  - failed
  - remediation_required
  - completed_pending_review
  - completed
  - expired
  - canceled
- priority
  - low
  - normal
  - high
  - urgent
- assignedAt
- dueAt
- startedAt
- completedAt
- expiresAt
- canceledAt
- cancelReason
- trainerPersonId
- evaluatorPersonId
- supervisorPersonId
- moduleProgressRefs
- stepProgressRefs
- evaluationRefs
- signoffRefs
- evidenceRecordRefs
- failureReason
- remediationAssignmentRefs
- resultingQualificationRefs
- auditTrail
```

## Assignment status definitions

```text
assigned
- Assignment has been created.

not_started
- Assignment is available but trainee has not started.

in_progress
- At least one required step/module is in progress.

waiting_trainee
- Trainee action is needed.

waiting_trainer
- Trainer signoff/action is needed.

waiting_evaluator
- Evaluator signoff/action is needed.

waiting_supervisor
- Supervisor review/action is needed.

failed
- Assignment failed.

remediation_required
- Remediation must be completed.

completed_pending_review
- Training steps are complete, but final review remains.

completed
- Assignment is complete and outcomes may be issued.

expired
- Assignment expired before completion.

canceled
- Assignment was canceled.
```

## Module progress

```text
ModuleProgress
- moduleProgressId
- tenantId
- assignmentId
- moduleId
- status
  - not_started
  - in_progress
  - waiting_signoff
  - passed
  - failed
  - skipped
  - waived
- startedAt
- completedAt
- completedByPersonId
- score
- stepProgressRefs
- notes
```

## Step progress

```text
StepProgress
- stepProgressId
- tenantId
- assignmentId
- moduleId
- stepId
- status
  - not_started
  - in_progress
  - completed
  - passed
  - failed
  - waived
  - skipped
- responseValue
- score
- result
  - pass
  - fail
  - warning
  - not_applicable
- traineeSignedAt
- trainerSignedAt
- evaluatorSignedAt
- supervisorSignedAt
- trainerPersonId
- evaluatorPersonId
- supervisorPersonId
- evidenceRecordRefs
- startedAt
- completedAt
- completedByPersonId
- failureReason
- waiverReason
- notes
```

## Signoff

A Signoff is a formal attestation from a trainee, trainer, evaluator, supervisor, or other authorized person.

```text
TrainingSignoff
- signoffId
- tenantId
- assignmentId
- moduleId
- stepId
- signoffType
  - trainee_acknowledgement
  - trainer_signoff
  - evaluator_signoff
  - supervisor_signoff
  - compliance_review
  - manual_override
- status
  - pending
  - signed
  - rejected
  - revoked
  - expired
- signerPersonId
- signedAt
- attestationText
- signatureRecordRef
- rejectionReason
- revokedAt
- revokedByPersonId
- recordRefs
```

## Evaluation

An Evaluation is a formal assessment of whether the trainee passed a written, practical, oral, observation, or field evaluation.

```text
Evaluation
- evaluationId
- tenantId
- evaluationNumber
- assignmentId
- programId
- moduleId
- stepId
- personId
- evaluationType
  - written
  - practical
  - oral
  - observation
  - simulator
  - field
  - document_review
  - supervisor_review
- status
  - scheduled
  - in_progress
  - passed
  - failed
  - inconclusive
  - canceled
- evaluatorPersonId
- scheduledAt
- startedAt
- completedAt
- score
- passingScore
- resultSummary
- criteriaResults
- failureReasons
- remediationRequired
- evidenceRecordRefs
- staffarrLocationId
- sourceProduct
- sourceObjectRef
```

## Evaluation criterion result

```text
EvaluationCriterionResult
- criterionResultId
- evaluationId
- criterionRef
- result
  - pass
  - fail
  - warning
  - not_applicable
- score
- notes
- evidenceRecordRefs
```

## Observation

An Observation is a field observation of a person performing a task.

```text
Observation
- observationId
- tenantId
- assignmentId
- personId
- observerPersonId
- observationType
  - field_task
  - equipment_operation
  - process_execution
  - route_execution
  - maintenance_task
  - warehouse_task
  - safety_behavior
  - quality_process
- status
  - scheduled
  - in_progress
  - satisfactory
  - unsatisfactory
  - incomplete
  - canceled
- observedAt
- staffarrLocationId
- sourceProduct
- sourceObjectRef
- notes
- evidenceRecordRefs
- resultingEvaluationRef
```

## Training evidence requirement

```text
TrainingEvidenceRequirement
- trainingEvidenceRequirementId
- tenantId
- programId
- moduleId
- stepId
- assignmentId
- evidenceType
  - photo
  - video
  - document
  - signature
  - observation_note
  - test_result
  - checklist
  - external_certificate
  - other
- required
- status
  - missing
  - submitted
  - accepted
  - rejected
  - waived
- complianceCoreEvidenceRequirementRef
- recordRefs
- reviewedByPersonId
- reviewedAt
- rejectionReason
```

## Assignment execution workflow

```text
1. TrainingAssignment is created.
2. Trainee starts assignment.
3. Required modules/steps become available.
4. Trainee completes reading/video/question/evidence steps.
5. Trainer signs required trainer steps.
6. Evaluator performs required evaluations.
7. Failed steps create remediation or assignment failure.
8. Required evidence is stored in RecordArr.
9. Assignment enters completed_pending_review if final review is needed.
10. Assignment completes.
11. Qualification/certificate outcomes are issued.
```

## Signoff workflow

```text
1. Step requires signoff.
2. Field Companion or TrainArr UI presents attestation.
3. Signer validates trainee/context.
4. Signature is captured through RecordArr if required.
5. Signoff is recorded.
6. Step progress updates.
```

## Evaluation workflow

```text
1. Evaluation is scheduled or started.
2. Evaluator reviews criteria.
3. Trainee performs/answers required items.
4. Evaluator records pass/fail/score.
5. Evidence is attached.
6. Evaluation passes, fails, or is inconclusive.
7. Assignment progress updates.
8. Remediation is created if required.
```

## Events

```text
trainarr.assignment.created
trainarr.assignment.started
trainarr.assignment.status_changed
trainarr.assignment.failed
trainarr.assignment.remediation_required
trainarr.assignment.completed_pending_review
trainarr.assignment.completed
trainarr.assignment.canceled
trainarr.assignment.expired

trainarr.module_progress.started
trainarr.module_progress.completed
trainarr.module_progress.failed

trainarr.step_progress.started
trainarr.step_progress.completed
trainarr.step_progress.failed
trainarr.step_progress.waived

trainarr.signoff.requested
trainarr.signoff.signed
trainarr.signoff.rejected
trainarr.signoff.revoked

trainarr.evaluation.scheduled
trainarr.evaluation.started
trainarr.evaluation.passed
trainarr.evaluation.failed
trainarr.evaluation.canceled

trainarr.observation.created
trainarr.observation.completed
trainarr.evidence.submitted
trainarr.evidence.accepted
trainarr.evidence.rejected
```


---


# TrainArr — Qualification and Certificate Model

## Qualification definition

A QualificationDefinition defines a capability, authorization, certification, or qualification that a person may earn.

```text
QualificationDefinition
- qualificationDefinitionId
- tenantId
- qualificationKey
- qualificationNumber
- title
- description
- qualificationType
  - equipment_authorization
  - process_authorization
  - compliance_certification
  - customer_required_qualification
  - site_access
  - internal_skill
  - trainer_authorization
  - evaluator_authorization
  - driver_authorization
  - maintenance_authorization
  - warehouse_authorization
  - quality_authorization
  - other
- category
  - safety
  - compliance
  - maintenance
  - warehouse
  - transportation
  - quality
  - customer
  - equipment
  - process
  - leadership
  - other
- status
  - draft
  - active
  - retired
  - archived
- requiredProgramRefs
- prerequisiteQualificationRefs
- renewalInterval
- renewalUnit
  - days
  - months
  - years
  - none
- expirationPolicyRef
- suspensionPolicyRef
- revocationPolicyRef
- complianceRefs
- certificateTemplateRef
- publishToStaffArr
- createdAt
- updatedAt
```

## Qualification definition status

```text
draft
- Qualification is being configured.

active
- Qualification can be issued.

retired
- Qualification should not be newly issued.

archived
- Qualification retained for history.
```

## Person qualification

A PersonQualification is TrainArr’s truth that a person currently has, had, or lost a qualification.

```text
PersonQualification
- personQualificationId
- tenantId
- qualificationNumber
- qualificationDefinitionId
- personId
- status
  - pending
  - active
  - expiring_soon
  - expired
  - suspended
  - revoked
  - superseded
- issuedAt
- issuedByPersonId
- effectiveAt
- expiresAt
- sourceAssignmentRefs
- sourceEvaluationRefs
- certificateRef
- certificateRecordRef
- renewalAssignmentRef
- suspensionRefs
- revocationRefs
- publishedToStaffArrAt
- lastPublishedStatus
- notes
- auditTrail
```

## Qualification status definitions

```text
pending
- Qualification is expected but not active.

active
- Person currently holds qualification.

expiring_soon
- Qualification is active but nearing expiration.

expired
- Qualification is no longer valid by date.

suspended
- Qualification is temporarily inactive.

revoked
- Qualification was removed before normal expiration.

superseded
- Qualification was replaced by another qualification/version.
```

## Certificate

A Certificate is the formal certificate issuance object. RecordArr stores the certificate file.

```text
Certificate
- certificateId
- tenantId
- certificateNumber
- personId
- qualificationDefinitionId
- personQualificationId
- status
  - draft
  - issued
  - active
  - expired
  - suspended
  - revoked
  - replaced
  - archived
- issuedAt
- issuedByPersonId
- effectiveAt
- expiresAt
- certificateTemplateRef
- certificateRecordRef
- verificationCode
- verificationUrl
- complianceRefs
- replacedByCertificateRef
- notes
```

## Certificate status definitions

```text
draft
- Certificate is being generated.

issued
- Certificate was issued.

active
- Certificate is currently valid.

expired
- Certificate expired.

suspended
- Certificate temporarily inactive.

revoked
- Certificate invalidated.

replaced
- Certificate replaced by another certificate.

archived
- Certificate retained for history.
```

## Qualification expiration policy

```text
QualificationExpirationPolicy
- expirationPolicyId
- tenantId
- qualificationDefinitionId
- expirationType
  - never
  - fixed_interval
  - fixed_date
  - source_program_rule
  - external_expiration
- intervalValue
- intervalUnit
  - days
  - months
  - years
- warningDaysBeforeExpiration
- gracePeriodDays
- expireOnFailedRenewal
```

## Qualification suspension

```text
QualificationSuspension
- suspensionId
- tenantId
- personQualificationId
- personId
- qualificationDefinitionId
- status
  - active
  - lifted
  - expired
  - canceled
- reason
  - incident
  - failed_evaluation
  - expired_document
  - quality_issue
  - compliance_issue
  - supervisor_action
  - other
- sourceProduct
- sourceObjectRef
- suspendedByPersonId
- suspendedAt
- expiresAt
- liftedByPersonId
- liftedAt
- liftReason
- recordRefs
```

## Qualification revocation

```text
QualificationRevocation
- revocationId
- tenantId
- personQualificationId
- personId
- qualificationDefinitionId
- reason
  - serious_incident
  - falsified_record
  - failed_remediation
  - compliance_issue
  - supervisor_action
  - other
- sourceProduct
- sourceObjectRef
- revokedByPersonId
- revokedAt
- recordRefs
- notes
```

## Qualification check

Other products ask TrainArr whether a person has a required qualification.

```text
QualificationCheck
- qualificationCheckId
- tenantId
- sourceProduct
- sourceObjectRef
- personId
- requiredQualificationRefs
- context
  - work_order
  - route
  - equipment_operation
  - inspection
  - warehouse_task
  - training_signoff
  - quality_review
  - customer_requirement
  - site_access
  - other
- result
  - pass
  - warning
  - fail
  - unknown
  - not_required
- missingQualificationRefs
- expiredQualificationRefs
- suspendedQualificationRefs
- expiringSoonQualificationRefs
- evaluatedAt
```

## Certificate generation workflow

```text
1. Assignment completes.
2. Program outcome determines qualification/certificate.
3. TrainArr creates PersonQualification.
4. TrainArr generates Certificate.
5. RecordArr stores certificate file if required.
6. Qualification status is published to StaffArr.
7. Products can pass/fail qualification checks.
```

## Qualification suspension workflow

```text
1. Incident, failed evaluation, expired evidence, or supervisor action triggers suspension.
2. TrainArr creates QualificationSuspension.
3. PersonQualification becomes suspended.
4. TrainArr publishes status to StaffArr.
5. Products relying on qualification now fail/warn checks.
6. Suspension is lifted after remediation or review.
```

## Qualification renewal workflow

```text
1. Qualification approaches expiration.
2. TrainArr creates renewal warning.
3. Renewal assignment is created.
4. Person completes refresher/renewal.
5. Qualification is extended or reissued.
6. StaffArr receives updated readiness snapshot.
```

## Events

```text
trainarr.qualification_definition.created
trainarr.qualification_definition.updated
trainarr.qualification_definition.activated
trainarr.qualification_definition.retired

trainarr.person_qualification.pending
trainarr.person_qualification.issued
trainarr.person_qualification.active
trainarr.person_qualification.expiring_soon
trainarr.person_qualification.expired
trainarr.person_qualification.suspended
trainarr.person_qualification.revoked
trainarr.person_qualification.superseded
trainarr.person_qualification.published_to_staffarr

trainarr.certificate.created
trainarr.certificate.issued
trainarr.certificate.expired
trainarr.certificate.suspended
trainarr.certificate.revoked
trainarr.certificate.replaced

trainarr.qualification_check.completed
trainarr.qualification_suspension.created
trainarr.qualification_suspension.lifted
trainarr.qualification_revocation.created
```


---


# TrainArr — Remediation, Renewal, Applicability, and Requirement Model

## Training requirement profile

A TrainingRequirementProfile defines who or what context requires a training program/qualification.

StaffArr owns people/org/locations. Compliance Core owns regulatory meaning. TrainArr owns the training assignment rule/profile.

```text
TrainingRequirementProfile
- requirementProfileId
- tenantId
- profileNumber
- title
- description
- status
  - draft
  - active
  - paused
  - retired
  - archived
- profileType
  - position
  - department
  - site
  - team
  - location
  - asset_type
  - equipment
  - task
  - customer
  - route
  - quality
  - compliance
  - onboarding
  - incident_remediation
  - custom
- applicabilityRules
- requiredProgramRefs
- requiredQualificationRefs
- dueDateRules
- renewalRules
- complianceRefs
- createdAt
- updatedAt
```

## Applicability rule

```text
TrainingApplicabilityRule
- applicabilityRuleId
- requirementProfileId
- ruleType
  - person_position
  - person_department
  - person_site
  - person_team
  - staffarr_location
  - asset_type
  - task_type
  - customer_requirement
  - route_type
  - incident_type
  - quality_finding
  - compliance_rule
  - manual
- sourceProduct
- sourceObjectRef
- conditionLogic
- required
- explanation
```

## Assignment generation rule

```text
AssignmentGenerationRule
- generationRuleId
- requirementProfileId
- triggerType
  - person_created
  - position_changed
  - site_changed
  - department_changed
  - team_changed
  - incident_received
  - qualification_expiring
  - qualification_expired
  - manual
  - scheduled
  - compliance_rule_changed
- dueDateOffsetDays
- priority
- duplicateHandling
  - skip_if_active
  - create_new
  - update_existing
  - require_review
- assignmentSource
```

## Remediation assignment

A RemediationAssignment is a training assignment caused by failure, incident, quality issue, audit finding, expired/invalid qualification, or supervisor action.

```text
RemediationAssignment
- remediationAssignmentId
- tenantId
- remediationNumber
- personId
- sourceProduct
  - staffarr
  - assurarr
  - maintainarr
  - routarr
  - loadarr
  - compliancecore
  - trainarr
  - manual
- sourceObjectRef
- sourceIncidentRef
- sourceNonconformanceRef
- sourceCapaRef
- sourceFindingRef
- reason
  - incident
  - failed_training
  - failed_evaluation
  - expired_qualification
  - quality_issue
  - safety_issue
  - compliance_issue
  - supervisor_required
  - repeated_error
  - other
- severity
  - low
  - moderate
  - high
  - critical
- status
  - created
  - assigned
  - in_progress
  - completed
  - failed
  - escalated
  - canceled
- trainingAssignmentRef
- requiredProgramRefs
- affectedQualificationRefs
- dueAt
- completedAt
- outcome
  - qualification_restored
  - qualification_suspended
  - qualification_revoked
  - no_status_change
  - escalated_to_staffarr
- recordRefs
```

## Renewal event

```text
QualificationRenewalEvent
- renewalEventId
- tenantId
- personQualificationId
- personId
- qualificationDefinitionId
- status
  - upcoming
  - renewal_assignment_created
  - in_progress
  - renewed
  - expired
  - failed
  - canceled
- renewalAssignmentRef
- currentExpiresAt
- newExpiresAt
- warningSentAt
- dueAt
- completedAt
- notes
```

## Expiration monitor

```text
QualificationExpirationMonitor
- monitorId
- tenantId
- qualificationDefinitionId
- status
  - active
  - paused
- warningDays
- autoCreateRenewalAssignment
- escalationRules
- lastRunAt
```

## Training waiver

A waiver should be controlled and auditable.

```text
TrainingWaiver
- waiverId
- tenantId
- personId
- assignmentId
- programId
- qualificationDefinitionId
- waiverType
  - step
  - module
  - assignment
  - qualification_requirement
  - evidence_requirement
- status
  - requested
  - approved
  - rejected
  - expired
  - revoked
- requestedByPersonId
- approvedByPersonId
- requestedAt
- approvedAt
- expiresAt
- reason
- complianceImpact
- recordRefs
```

## Manual qualification override

```text
QualificationOverride
- overrideId
- tenantId
- personId
- qualificationDefinitionId
- personQualificationId
- overrideType
  - grant
  - extend
  - suspend
  - lift_suspension
  - revoke
  - restore
- status
  - pending
  - approved
  - rejected
  - applied
  - expired
- requestedByPersonId
- approvedByPersonId
- appliedByPersonId
- reason
- effectiveAt
- expiresAt
- recordRefs
- complianceImpact
```

## Requirement evaluation result

```text
TrainingRequirementEvaluation
- evaluationId
- tenantId
- personId
- contextProduct
- contextObjectRef
- status
  - requirements_met
  - missing_training
  - missing_qualification
  - expired_qualification
  - suspended_qualification
  - warning
  - unknown
- requiredProgramRefs
- requiredQualificationRefs
- activeAssignmentRefs
- missingRequirementRefs
- evaluatedAt
```

## Requirement profile workflow

```text
1. Admin creates TrainingRequirementProfile.
2. Admin selects scope such as position, site, task, asset, route, customer, or compliance rule.
3. Admin selects required programs/qualifications.
4. Admin defines due date and assignment generation behavior.
5. Profile becomes active.
6. Person/context changes trigger assignment evaluation.
```

## Incident remediation workflow

```text
1. StaffArr receives personnel incident.
2. StaffArr forwards remediation request to TrainArr.
3. TrainArr evaluates incident type/severity.
4. TrainArr creates RemediationAssignment and TrainingAssignment.
5. Person completes remedial program.
6. TrainArr determines outcome.
7. Qualification may be restored, suspended, revoked, or unchanged.
8. StaffArr receives readiness/person history update.
```

## CAPA remediation workflow

```text
1. AssurArr CAPA requires retraining.
2. AssurArr sends action/request to TrainArr.
3. TrainArr creates assignment for affected people/group.
4. Completion evidence is stored in RecordArr.
5. TrainArr sends completion status to AssurArr.
6. CAPA action can be verified/closed by AssurArr.
```

## Renewal workflow

```text
1. Qualification monitor detects upcoming expiration.
2. TrainArr creates warning and optional renewal assignment.
3. Person completes renewal training/evaluation.
4. TrainArr extends/reissues qualification.
5. Certificate is updated if required.
6. StaffArr readiness updates.
```

## Waiver workflow

```text
1. Waiver is requested.
2. Compliance impact is evaluated.
3. Approver approves/rejects.
4. Approved waiver changes assignment/requirement status.
5. Audit trail and evidence are retained.
```

## Events

```text
trainarr.requirement_profile.created
trainarr.requirement_profile.updated
trainarr.requirement_profile.activated
trainarr.requirement_profile.retired

trainarr.requirement_evaluation.completed
trainarr.assignment_generation.completed

trainarr.remediation.created
trainarr.remediation.assigned
trainarr.remediation.completed
trainarr.remediation.failed
trainarr.remediation.escalated
trainarr.remediation.canceled

trainarr.renewal.upcoming
trainarr.renewal.assignment_created
trainarr.renewal.completed
trainarr.renewal.failed

trainarr.waiver.requested
trainarr.waiver.approved
trainarr.waiver.rejected
trainarr.waiver.revoked

trainarr.qualification_override.requested
trainarr.qualification_override.approved
trainarr.qualification_override.applied
trainarr.qualification_override.rejected
```


---


# TrainArr — Workflows, Status Logic, Events, and APIs

## Major workflow: new hire onboarding

```text
1. StaffArr creates Person.
2. StaffArr assigns position, department, site, manager, and location.
3. StaffArr publishes person/org event.
4. TrainArr evaluates active TrainingRequirementProfiles.
5. TrainArr creates TrainingAssignments.
6. Person completes onboarding/compliance/safety/task training.
7. Trainer/evaluator signs required steps.
8. TrainArr issues qualifications/certificates.
9. TrainArr publishes qualification status to StaffArr.
10. StaffArr readiness updates.
```

## Major workflow: program completion to qualification

```text
1. Person starts assigned program.
2. Person completes required modules and steps.
3. Required evidence is uploaded to RecordArr.
4. Trainer/evaluator/supervisor signoffs are completed.
5. Evaluations pass.
6. Assignment completes.
7. ProgramOutcome issues PersonQualification.
8. Certificate is generated if required.
9. Qualification is published to StaffArr.
```

## Major workflow: qualification check before work

```text
1. Product wants to assign person to work.
2. Product calls TrainArr qualification check.
3. TrainArr evaluates required qualifications.
4. TrainArr returns pass/warning/fail/unknown.
5. Product applies product-local assignment rules.
6. Failed check creates blocker in source product.
```

## Major workflow: incident-driven remediation

```text
1. Operational product reports incident to StaffArr.
2. StaffArr determines training impact and sends remediation request to TrainArr.
3. TrainArr creates RemediationAssignment.
4. TrainArr creates assigned remedial program.
5. Person completes remediation.
6. TrainArr updates affected qualification if needed.
7. StaffArr receives readiness/history update.
```

## Major workflow: quality/CAPA retraining

```text
1. AssurArr CAPA action requires retraining.
2. TrainArr receives remediation/training request.
3. TrainArr creates assignments for affected people.
4. Completion status is sent to AssurArr.
5. AssurArr verifies CAPA action/effectiveness.
```

## Major workflow: qualification renewal

```text
1. Expiration monitor detects expiring qualification.
2. TrainArr emits expiring warning.
3. Renewal assignment is created.
4. Person completes refresher/evaluation.
5. Qualification is renewed or expires/fails.
6. StaffArr readiness updates.
```

## Major workflow: program version update

```text
1. Admin drafts new program version.
2. Modules/steps/evidence/outcomes are changed.
3. Version is reviewed and approved.
4. New version becomes active.
5. Existing assignments stay on original version unless migration is selected.
6. New assignments use active version.
```

## TrainArr emitted events

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

trainarr.assignment.created
trainarr.assignment.started
trainarr.assignment.status_changed
trainarr.assignment.failed
trainarr.assignment.remediation_required
trainarr.assignment.completed_pending_review
trainarr.assignment.completed
trainarr.assignment.canceled
trainarr.assignment.expired

trainarr.step_progress.started
trainarr.step_progress.completed
trainarr.step_progress.failed
trainarr.step_progress.waived

trainarr.signoff.requested
trainarr.signoff.signed
trainarr.signoff.rejected

trainarr.evaluation.scheduled
trainarr.evaluation.passed
trainarr.evaluation.failed

trainarr.qualification_definition.created
trainarr.qualification_definition.activated
trainarr.person_qualification.issued
trainarr.person_qualification.active
trainarr.person_qualification.expiring_soon
trainarr.person_qualification.expired
trainarr.person_qualification.suspended
trainarr.person_qualification.revoked
trainarr.person_qualification.published_to_staffarr

trainarr.certificate.issued
trainarr.remediation.created
trainarr.remediation.completed
trainarr.renewal.assignment_created
trainarr.renewal.completed
```

## Integration APIs TrainArr should expose

```text
GET /api/v1/integrations/programs
GET /api/v1/integrations/programs/{programId}
POST /api/v1/integrations/programs
POST /api/v1/integrations/programs/{programId}/versions
POST /api/v1/integrations/programs/{programId}/activate
POST /api/v1/integrations/programs/{programId}/retire

GET /api/v1/integrations/requirement-profiles
POST /api/v1/integrations/requirement-profiles
POST /api/v1/integrations/training-requirement-evaluations

GET /api/v1/integrations/assignments
GET /api/v1/integrations/assignments/{assignmentId}
POST /api/v1/integrations/assignments
POST /api/v1/integrations/assignments/{assignmentId}/start
POST /api/v1/integrations/assignments/{assignmentId}/complete
POST /api/v1/integrations/assignments/{assignmentId}/cancel

POST /api/v1/integrations/assignments/{assignmentId}/steps/{stepId}/complete
POST /api/v1/integrations/signoffs
POST /api/v1/integrations/evaluations
POST /api/v1/integrations/observations

GET /api/v1/integrations/qualification-definitions
POST /api/v1/integrations/qualification-definitions
GET /api/v1/integrations/persons/{personId}/qualifications
POST /api/v1/integrations/qualification-checks
POST /api/v1/integrations/qualifications/{personQualificationId}/suspend
POST /api/v1/integrations/qualifications/{personQualificationId}/revoke
POST /api/v1/integrations/qualifications/{personQualificationId}/restore

GET /api/v1/integrations/certificates/{certificateId}
POST /api/v1/integrations/certificates

POST /api/v1/integrations/remediation-requests
GET /api/v1/integrations/remediations/{remediationAssignmentId}
POST /api/v1/integrations/remediations/{remediationAssignmentId}/complete

POST /api/v1/integrations/renewals/run
POST /api/v1/integrations/waivers
POST /api/v1/integrations/qualification-overrides
```

## APIs TrainArr should consume

```text
NexArr
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/summary
- GET /persons/{personId}/permissions
- GET /org-units
- GET /positions
- GET /teams
- GET /locations
- POST /person-history-events
- POST /readiness-updates

Compliance Core
- GET /rulepacks
- GET /requirements
- GET /evidence-types
- POST /evaluations

RecordArr
- POST /records
- GET /records/{recordId}
- POST /upload-sessions
- POST /record-packages

MaintainArr
- GET /assets/{assetId}
- GET /work-orders/{workOrderId}

LoadArr
- GET /location-profiles
- GET /mobile/warehouse-task-context where needed

RoutArr
- GET /trips/{tripId}
- GET /routes/{routeId}

AssurArr
- GET /capas/{capaId}
- POST /capa-actions/{actionId}/status-updates

ReportArr
- POST /events
```

## Permission examples

```text
trainarr.programs.read
trainarr.programs.create
trainarr.programs.update
trainarr.programs.review
trainarr.programs.approve
trainarr.programs.activate
trainarr.programs.retire

trainarr.assignments.read
trainarr.assignments.create
trainarr.assignments.start
trainarr.assignments.complete
trainarr.assignments.cancel

trainarr.steps.complete
trainarr.evidence.submit
trainarr.evidence.review

trainarr.trainer.signoff
trainarr.evaluator.signoff
trainarr.supervisor.signoff

trainarr.evaluations.create
trainarr.evaluations.perform
trainarr.evaluations.review

trainarr.qualifications.read
trainarr.qualifications.issue
trainarr.qualifications.suspend
trainarr.qualifications.revoke
trainarr.qualifications.override

trainarr.certificates.read
trainarr.certificates.issue

trainarr.remediation.create
trainarr.remediation.manage
trainarr.renewals.manage
trainarr.waivers.request
trainarr.waivers.approve

trainarr.admin
```

## Default role examples

```text
Training Viewer
- Read programs, assignments, qualifications, and certificates where permitted.

Trainee
- Complete assigned training.
- Upload required evidence.
- Sign acknowledgements.

Trainer
- View assigned trainees.
- Sign trainer steps.
- Complete trainer observations.

Evaluator
- Perform evaluations.
- Pass/fail practical or written checks.
- Attach evidence.

Training Coordinator
- Assign programs.
- Monitor completion.
- Manage due dates and reminders.

Training Program Manager
- Create/update programs, modules, steps, requirement profiles, and outcomes.

Qualification Manager
- Issue/suspend/revoke/restore qualifications where allowed.
- Manage renewals and expirations.

Compliance Training Reviewer
- Review compliance-linked training/evidence.
- Approve waivers where allowed.

TrainArr Admin
- Manage settings, program templates, requirement profiles, and permissions.
```

## TrainArr UI surfaces

```text
/app/trainarr
- dashboard
- programs
- program builder
- program detail
- modules
- steps
- requirement profiles
- assignments
- assignment detail
- evaluations
- signoffs
- remediation
- renewals
- qualification definitions
- person qualifications
- certificates
- waivers
- overrides
- settings
```

## Program detail UI

```text
ProgramDetailPage
- Header
  - title
  - status
  - version
  - type/category
- Modules
- Steps
- Evidence requirements
- Prerequisites
- Outcomes
- Compliance references
- Version history
- Activation controls
```

## Assignment detail UI

```text
AssignmentDetailPage
- Header
  - assignment number
  - trainee
  - program/version
  - status
  - due date
- Progress
  - modules
  - steps
  - signoffs
  - evaluations
- Evidence
- Remediation
- Resulting qualifications
- Timeline
```

## Qualification detail UI

```text
QualificationDetailPage
- Person
- Qualification definition
- Status
- Issued/effective/expires dates
- Source assignments/evaluations
- Certificate
- Suspension/revocation history
- Renewal status
- StaffArr publication status
```
