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
