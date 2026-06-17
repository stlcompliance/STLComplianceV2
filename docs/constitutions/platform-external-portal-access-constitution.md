# STL Compliance External Portal Access Constitution

## 1. Purpose

This constitution defines how customers, vendors, suppliers, carriers, auditors, and other external parties interact with STL Compliance without becoming internal StaffArr people or bypassing NexArr, StaffArr, product authority, audit, or tenant isolation.

## 2. Scope

This constitution applies to:

- Customer portal access
- Vendor/supplier portal access
- Carrier update links
- External proof/evidence upload links
- External approval or acknowledgement links
- Auditor package access
- Public-site inquiry handoffs
- Magic-link style scoped sessions
- Limited external accounts where implemented

## 3. Prime directive

External access is scoped access to a specific purpose.

External actors must not receive broad tenant access, internal product navigation, internal permission surfaces, or StaffArr identity unless they are truly internal people.

## 4. External actor

ExternalActorRef identifies a non-internal actor.

```text
ExternalActorRef
- tenantId
- externalActorType
  - customer_contact
  - supplier_contact
  - vendor_contact
  - carrier_contact
  - auditor
  - public_inquirer
  - other
- owningProduct
- owningExternalRecordRef
- displayName
- email
- phone
- organizationName
- authorityContext
- status
  - invited
  - active
  - suspended
  - expired
  - revoked
```

Customer contacts are owned by CustomArr.

Supplier/vendor contacts are owned by SupplyArr unless a product-specific workflow defines otherwise.

Internal employees remain StaffArr/NexArr people.

## 5. Portal invitation

```text
PortalInvitation
- invitationId
- tenantId
- sourceProduct
- sourceRecordRef
- externalActorRef
- purpose
  - customer_order_status
  - supplier_order_confirmation
  - vendor_completion_update
  - carrier_status_update
  - document_upload
  - evidence_review
  - approval_request
  - audit_package_access
  - onboarding_form
- allowedActions
- expiresAt
- maxUses
- status
  - created
  - sent
  - opened
  - used
  - expired
  - revoked
- createdByPersonId
- correlationId
- auditTrailRef
```

## 6. Portal session

```text
PortalSession
- portalSessionId
- tenantId
- invitationRef
- externalActorRef
- sourceProduct
- sourceRecordRef
- allowedActions
- startedAt
- expiresAt
- ipSummary
- deviceSummary
- status
  - active
  - complete
  - expired
  - revoked
```

Portal sessions should be short-lived and scoped.

## 7. Allowed external actions

Recommended external action types:

```text
- view_limited_status
- submit_status_update
- upload_evidence
- acknowledge_requirement
- approve_request
- reject_request
- answer_questionnaire
- sign_document
- confirm_completion
- report_issue
- accept_tender
- reject_tender
- submit_counter
- confirm_appointment
```

Each action must route to the owning product's API or review queue.

## 8. Forbidden external access

External actors must not be allowed to:

```text
- browse internal tenant records broadly
- access StaffArr people lists
- view unrelated customers/vendors/suppliers
- view unrestricted asset, inventory, route, or training data
- manage product permissions
- access platform admin screens
- access raw rule logic or hidden prompts
- bypass product validation
- override blockers unless explicitly designed and permissioned
- see other tenants
```

## 9. External submission

```text
ExternalPortalSubmission
- submissionId
- tenantId
- portalSessionRef
- externalActorRef
- sourceProduct
- sourceRecordRef
- actionType
- submittedDataSummary
- uploadedRecordRefs
- signatureRef
- status
  - received
  - accepted
  - rejected
  - review_required
  - superseded
- reviewedByPersonId
- reviewedAt
- auditTrailRef
```

External submissions should normally create reviewable product-owned updates.

A vendor marking an order complete should not silently dispatch a truck unless the owning product workflow explicitly allows automatic progression and all blockers are clear.

A carrier accepting, rejecting, or countering a RoutArr tender should create a RoutArr-owned reviewable tender response unless the tender workflow explicitly allows automatic acceptance and all dispatch blockers are clear.

## 10. External proof and evidence uploads

Files uploaded by external actors should be stored in RecordArr.

The owning product should receive a reference and decide whether the evidence satisfies its workflow.

Compliance Core may evaluate evidence meaning.

RecordArr owns the stored file and retention.

## 11. Magic-link rules

Magic links must be:

```text
- tenant-scoped
- purpose-scoped
- action-scoped
- time-limited
- revocable
- auditable
- unguessable
```

Magic links must not expose platform service tokens, internal IDs without context, or raw database references.

## 12. Limited external account rules

If external accounts are implemented:

- NexArr still owns authentication/session mechanics.
- The external account must be clearly marked external.
- The account must not imply StaffArr employment/person authority.
- Product-specific external permissions must be narrower than internal role permissions.
- Revocation must be immediate and audited.

## 13. Vendor order completion example

1. SupplyArr or OrdArr creates a vendor completion request.
2. External vendor contact receives scoped portal invitation.
3. Vendor opens portal session and confirms completion or reports exception.
4. RecordArr stores any attached proof.
5. OrdArr updates handoff status or creates review task.
6. RoutArr dispatch is allowed only if order readiness, customer requirements, quality holds, and inventory/transport prerequisites are clear.
7. ReportArr can show vendor completion timeliness.

## 14. Auditor package example

1. Internal user creates RecordArr audit package.
2. External auditor receives scoped package access.
3. Auditor may view/download only package records.
4. Access expires or is revoked.
5. Every access/download is audited.
6. Legal hold and retention rules remain RecordArr-owned.

## 15. Events

```text
{productKey}.portal_invitation.created
{productKey}.portal_invitation.sent
{productKey}.portal_invitation.revoked
{productKey}.external_submission.received
{productKey}.external_submission.review_required
{productKey}.external_submission.accepted
{productKey}.external_submission.rejected
recordarr.external_file.uploaded
```

## 16. Non-goals

This constitution does not create a separate PortalArr product.

External portal access is a shared platform pattern implemented through owning products, NexArr authentication/session mechanics where needed, and RecordArr evidence storage.
