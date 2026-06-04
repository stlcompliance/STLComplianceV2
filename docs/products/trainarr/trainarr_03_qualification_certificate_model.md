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
