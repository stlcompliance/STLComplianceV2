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
