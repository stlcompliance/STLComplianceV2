# StaffArr — Incident, Readiness, Restriction, and History Model

## Personnel incident

A PersonnelIncident is StaffArr’s central people-impact record. It may originate from MaintainArr, RoutArr, LoadArr, AssurArr, StaffArr itself, Field Companion, or another product.

The origin product owns the operational source event. StaffArr owns the personnel-facing incident record.

```text
PersonnelIncident
- incidentId
- tenantId
- incidentNumber
- title
- description
- incidentType
  - safety
  - compliance
  - quality
  - training
  - attendance
  - behavior
  - equipment_misuse
  - route_exception
  - inventory_variance
  - customer_complaint
  - policy_violation
  - injury
  - near_miss
  - property_damage
  - security
  - other
- severity
  - low
  - moderate
  - high
  - critical
- status
  - reported
  - triage
  - investigating
  - action_required
  - waiting_training
  - waiting_external_review
  - resolved
  - closed
  - canceled
- occurredAt
- reportedAt
- reportedByPersonId
- involvedPersonIds
- witnessPersonIds
- supervisorPersonId
- ownerPersonId
- sourceProduct
- sourceObjectRef
- staffarrSiteId
- staffarrLocationId
- immediateActionTaken
- injuryOrDamageFlag
- complianceImpactFlag
- trainingImpactFlag
- qualityImpactFlag
- customerImpactFlag
- requiresTrainArrReview
- trainarrAssignmentRefs
- restrictionRefs
- correctiveActionRefs
- recordRefs
- closedAt
- closedByPersonId
- closureSummary
- auditTrail
```

## Incident status definitions

```text
reported
- Incident has been reported but not reviewed.

triage
- Supervisor/safety/HR is determining classification, severity, and next steps.

investigating
- Facts are being collected.

action_required
- Corrective action, restriction, retraining, review, or documentation is needed.

waiting_training
- TrainArr remediation or qualification update is pending.

waiting_external_review
- External review, legal, customer, vendor, or regulatory response is pending.

resolved
- Required actions are complete.

closed
- Administrative review is complete and incident is closed.

canceled
- Incident was invalid, duplicate, or created in error.
```

## Personnel corrective action

```text
PersonnelCorrectiveAction
- correctiveActionId
- incidentId
- personId
- actionType
  - coaching
  - retraining
  - warning
  - restriction
  - suspension
  - reassignment
  - observation_period
  - policy_acknowledgement
  - other
- status
  - open
  - in_progress
  - completed
  - canceled
- assignedByPersonId
- assignedToPersonId
- dueAt
- completedAt
- completionSummary
- trainarrAssignmentRef
- recordRefs
```

## Person restriction

A restriction limits what a person can do until lifted, expired, or overridden.

```text
PersonRestriction
- restrictionId
- tenantId
- personId
- restrictionType
  - cannot_operate_asset
  - cannot_drive
  - cannot_dispatch
  - cannot_train
  - cannot_evaluate
  - cannot_approve
  - cannot_work_unsupervised
  - site_restricted
  - location_restricted
  - task_restricted
  - product_restricted
  - customer_restricted
- sourceProduct
- sourceObjectRef
- incidentRef
- reason
- status
  - active
  - lifted
  - expired
  - overridden
  - canceled
- effectiveAt
- expiresAt
- liftedAt
- liftedByPersonId
- liftReason
- affectedProductKeys
- affectedObjectRefs
- recordRefs
```

## Readiness snapshot

StaffArr owns the readiness view, not all readiness facts. It composes data from StaffArr, TrainArr, product restrictions, incidents, permissions, and assignment requirements.

```text
PersonReadinessSnapshot
- readinessSnapshotId
- tenantId
- personId
- overallStatus
  - ready
  - limited
  - blocked
  - onboarding
  - unknown
- evaluatedAt
- evaluatedBy
  - system
  - person
- positionReadiness
- siteReadiness
- productReadiness
- qualificationSummary
- permissionSummary
- activeRestrictionRefs
- activeIncidentRefs
- missingQualificationRefs
- expiredQualificationRefs
- pendingTrainingAssignmentRefs
- missingDocumentRefs
- blockerRefs
- warningRefs
- sourceSnapshots
```

## Product readiness

```text
ProductReadiness
- productKey
- readinessStatus
  - ready
  - limited
  - blocked
  - permission_missing
  - unknown
- permissionStatus
  - sufficient
  - insufficient
  - unknown
- qualificationStatus
  - sufficient
  - missing
  - expired
  - not_required
  - unknown
- restrictionStatus
  - none
  - active
- blockers
- warnings
```

## Readiness blocker

```text
ReadinessBlocker
- blockerId
- personId
- blockerType
  - missing_permission
  - missing_qualification
  - expired_qualification
  - pending_training
  - active_incident
  - active_restriction
  - missing_document
  - suspended_status
  - leave_status
  - missing_workflow_authority
  - compliance_issue
- sourceProduct
- sourceObjectRef
- severity
  - warning
  - blocking
  - critical
- title
- description
- requiredAction
- status
  - active
  - resolved
  - overridden
```

## Person history event

A PersonHistoryEvent is the audit-friendly timeline record.

```text
PersonHistoryEvent
- historyEventId
- tenantId
- personId
- eventType
  - person_created
  - status_changed
  - manager_changed
  - position_changed
  - department_changed
  - site_changed
  - location_changed
  - team_changed
  - permission_granted
  - permission_revoked
  - qualification_issued
  - qualification_expired
  - training_assigned
  - training_completed
  - incident_created
  - incident_closed
  - restriction_created
  - restriction_lifted
  - document_added
  - override_used
  - audit_package_generated
- occurredAt
- actorPersonId
- actorServiceClientId
- sourceProduct
- sourceObjectRef
- title
- summary
- beforeSnapshot
- afterSnapshot
- recordRefs
```

## Person audit package

A PersonAuditPackage is assembled for internal review, compliance audit, incident investigation, or legal/personnel review.

```text
PersonAuditPackage
- packageId
- tenantId
- packageNumber
- personId
- packageType
  - compliance
  - training
  - incident
  - employment_history
  - permission_review
  - full_person_history
- status
  - draft
  - assembling
  - complete
  - locked
  - archived
- requestedByPersonId
- requestedAt
- dateRangeStart
- dateRangeEnd
- includedSections
  - profile
  - org_history
  - permission_history
  - qualification_history
  - training_history
  - incident_history
  - restriction_history
  - document_refs
  - product_activity_refs
- sourceRefs
- recordarrPackageRef
- generatedAt
- lockedAt
```

## Incident routing rules

```text
MaintainArr incident examples
- Asset misuse
- Repeated repair error
- Unsafe repair behavior
- Missed inspection
- Safety-critical defect ignored

RoutArr incident examples
- Driver accident
- Refused delivery behavior issue
- HOS/compliance concern
- Vehicle misuse
- Missed route due to person issue

LoadArr incident examples
- Inventory variance with personnel concern
- Unsafe forklift operation
- Receiving error with training concern
- Pick/issue misconduct

AssurArr incident examples
- Repeat quality failure
- CAPA action not completed
- Process nonconformance tied to person/team

Field Companion incident examples
- Self-reported safety issue
- Near miss
- Injury
- Policy violation
```

## Incident-to-training workflow

```text
1. Product reports incident to StaffArr.
2. StaffArr creates PersonnelIncident.
3. StaffArr triages severity and impact flags.
4. StaffArr determines whether training review is required.
5. StaffArr sends remediation request to TrainArr.
6. TrainArr creates assignment.
7. StaffArr sets incident status to waiting_training.
8. TrainArr reports completion/failure.
9. StaffArr updates readiness and restrictions.
10. StaffArr closes incident after required actions.
```

## Restriction workflow

```text
1. Incident, qualification issue, supervisor action, or compliance finding creates restriction.
2. StaffArr publishes restriction event.
3. Products consume restriction and block affected actions.
4. Restriction expires or is lifted by authorized person.
5. StaffArr publishes restriction lifted event.
6. Person readiness recalculates.
```

## Readiness evaluation workflow

```text
1. StaffArr receives person/org/permission/incident/restriction change.
2. StaffArr requests or consumes TrainArr qualification status.
3. StaffArr checks active restrictions and incidents.
4. StaffArr checks product permission state.
5. StaffArr checks required documents where applicable.
6. StaffArr produces readiness snapshot.
7. Products may query readiness before assignment.
```

## Incident/readiness/history events

```text
staffarr.incident.created
staffarr.incident.triaged
staffarr.incident.status_changed
staffarr.incident.forwarded_to_trainarr
staffarr.incident.closed

staffarr.corrective_action.created
staffarr.corrective_action.completed

staffarr.restriction.created
staffarr.restriction.lifted
staffarr.restriction.expired

staffarr.readiness.evaluated
staffarr.readiness.changed
staffarr.readiness.blocker_created
staffarr.readiness.blocker_resolved

staffarr.person_history.event_created
staffarr.audit_package.created
staffarr.audit_package.locked
```
