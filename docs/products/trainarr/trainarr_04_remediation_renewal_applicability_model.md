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
