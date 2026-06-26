# RecordArr — Controlled Document, Review, Access, and Sharing Model

## Controlled document

A ControlledDocument is a formal document with lifecycle control, approval, effective date, versioning, review intervals, and distribution/acknowledgement requirements.

```text
ControlledDocument
- controlledDocumentId
- tenantId
- documentNumber
- recordId
- title
- description
- controlledDocumentType
  - policy
  - procedure
  - work_instruction
  - form
  - safety_data_sheet
  - training_material
  - specification
  - contract
  - permit
  - certificate
  - manual
  - other
- status
  - draft
  - review
  - approved
  - effective
  - superseded
  - obsolete
  - archived
- ownerPersonId
- departmentOrgUnitId
- staffarrSiteId
- currentVersionId
- approvalWorkflowRef
- reviewIntervalDays
- nextReviewAt
- effectiveAt
- expiresAt
- supersedesDocumentRef
- supersededByDocumentRef
- distributionRefs
- acknowledgementRequired
- complianceRefs
- auditTrail
```

## Document review

```text
DocumentReview
- documentReviewId
- controlledDocumentId
- versionId
- reviewType
  - approval
  - periodic_review
  - change_review
  - compliance_review
  - quality_review
  - legal_review
- status
  - pending
  - in_review
  - approved
  - rejected
  - changes_requested
  - canceled
- requestedByPersonId
- reviewerPersonId
- requestedAt
- dueAt
- reviewedAt
- decisionReason
- comments
```

## Document distribution

```text
DocumentDistribution
- distributionId
- controlledDocumentId
- versionId
- distributionType
  - person
  - role
  - department
  - site
  - team
  - product
  - external_link
- targetRef
- status
  - pending
  - distributed
  - acknowledged
  - expired
  - revoked
- distributedAt
- acknowledgedAt
- acknowledgementRef
```

## Document acknowledgement

```text
DocumentAcknowledgement
- acknowledgementId
- controlledDocumentId
- versionId
- personId
- status
  - pending
  - acknowledged
  - overdue
  - waived
- acknowledgedAt
- signatureRecordRef
- attestationText
- dueAt
```

## Document permission policy

RecordArr document permission policy should control who can read, download, modify, approve, share, export, or purge a record.

```text
RecordAccessPolicy
- accessPolicyId
- tenantId
- recordId
- policyType
  - default
  - restricted
  - legal_hold
  - product_scoped
  - public_link
  - external_share
- readRules
- writeRules
- downloadRules
- shareRules
- exportRules
- purgeRules
- status
  - active
  - inactive
  - superseded
```

## Access grant

```text
RecordAccessGrant
- accessGrantId
- tenantId
- recordId
- granteeType
  - person
  - role
  - product
  - service_client
  - external_link
- granteeRef
- permission
  - read
  - download
  - upload_new_version
  - approve
  - classify
  - map_evidence
  - export
  - share
  - archive
  - purge
- status
  - active
  - expired
  - revoked
- grantedByPersonId
- grantedAt
- expiresAt
- revokedAt
- revokeReason
```

## External share

External sharing should be narrow, expiring, and auditable.

```text
ExternalShare
- externalShareId
- tenantId
- recordId
- shareNumber
- sharePurpose
  - customer_view
  - supplier_response
  - auditor_access
  - legal_review
  - public_download
  - temporary_upload
- status
  - created
  - active
  - expired
  - revoked
  - completed
- tokenHash
- recipientName
- recipientEmail
- allowedActions
  - view
  - download
  - upload
  - sign
- createdAt
- createdByPersonId
- expiresAt
- revokedAt
- revokedByPersonId
- lastAccessedAt
- accessCount
```

## Redaction

```text
Redaction
- redactionId
- tenantId
- sourceRecordId
- redactedRecordId
- redactionReason
  - privacy
  - legal
  - customer
  - supplier
  - internal
  - security
- redactedByPersonId
- redactedAt
- redactionRules
- status
  - draft
  - completed
  - rejected
```

## Record access log

```text
RecordAccessLog
- accessLogId
- tenantId
- recordId
- actorPersonId
- actorServiceClientId
- externalShareId
- action
  - view
  - download
  - upload
  - approve
  - reject
  - share
  - export
  - archive
  - purge
- result
  - allowed
  - denied
  - failed
- occurredAt
- sourceIp
- userAgent
- reasonCode
```

## Controlled document lifecycle workflow

```text
1. User creates controlled document.
2. Draft version is uploaded.
3. Review workflow is started.
4. Reviewers approve, reject, or request changes.
5. Approved version becomes effective.
6. Document is distributed if required.
7. Acknowledgements are collected if required.
8. Periodic review is scheduled.
9. New version supersedes prior version when updated.
10. Obsolete documents are archived according to retention policy.
```

## Access workflow

```text
1. Product/user requests record.
2. RecordArr checks tenant, product, person/service, classification, policy, and legal hold.
3. Access is allowed/denied.
4. Access log is written.
5. Download/view/share/export proceeds if allowed.
```

## External share workflow

```text
1. Authorized user creates ExternalShare.
2. Recipient gets narrow scoped link.
3. Recipient views/downloads/uploads/signs only permitted record/action.
4. Access is logged.
5. Share expires or is revoked.
```

## Redaction workflow

```text
1. User requests redacted copy.
2. Redaction rules are applied.
3. Redacted rendition/record is created.
4. Original remains preserved.
5. Redacted record can be shared/exported.
```

## Events

```text
recordarr.controlled_document.created
recordarr.controlled_document.submitted_for_review
recordarr.controlled_document.approved
recordarr.controlled_document.effective
recordarr.controlled_document.superseded
recordarr.controlled_document.obsolete
recordarr.controlled_document.archived

recordarr.document_review.requested
recordarr.document_review.approved
recordarr.document_review.rejected
recordarr.document_review.changes_requested

recordarr.document_distribution.created
recordarr.document_distribution.acknowledged
recordarr.document_acknowledgement.completed
recordarr.document_acknowledgement.overdue

recordarr.access_policy.created
recordarr.access_grant.created
recordarr.access_grant.revoked
recordarr.external_share.created
recordarr.external_share.accessed
recordarr.external_share.revoked
recordarr.external_share.expired
recordarr.redaction.created
recordarr.redaction.completed
recordarr.access.logged
```
