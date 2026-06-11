# STL Compliance Record Lifecycle and Status Constitution

## 1. Purpose

This constitution standardizes lifecycle language across STL Compliance products so users, APIs, workflows, dashboards, reports, and audit records use consistent meaning.

Products may have domain-specific statuses, but they must map to platform lifecycle concepts.

## 2. Scope

This constitution applies to:

- Draft records
- Submitted records
- Approvals
- Active/inactive state
- Blocked/watch states
- Completion/closeout
- Cancellation
- Archive and supersession
- Deletion rules
- State transitions
- Lifecycle events
- Status badges and UI labels

## 3. Prime directive

A status must mean something.

Do not use vague labels that hide the business effect of a record.

A lifecycle transition must be explicit, permission-gated, auditable, and owned by the product that owns the record.

## 4. Platform lifecycle categories

Products should map local statuses to these platform lifecycle categories where applicable:

- `draft`
- `submitted`
- `pending_review`
- `pending_approval`
- `approved`
- `rejected`
- `active`
- `scheduled`
- `in_progress`
- `blocked`
- `watch`
- `completed`
- `closed`
- `cancelled`
- `inactive`
- `archived`
- `superseded`
- `deleted`

Not every product needs every state.

## 5. Draft

A draft is an incomplete or not-final record.

Rules:

- Drafts may have stable IDs.
- Drafts must be clearly labeled.
- Drafts may preserve completed sections and validation state.
- Draft saves must not trigger final workflows.
- Drafts must not appear as active, approved, published, dispatched, issued, posted, or completed.

Examples of effects that must not happen on ordinary draft save:

- Training assignment notification
- Inventory movement
- Route dispatch
- Work order release
- Evidence package finalization
- Rule publication
- External writeback

## 6. Submitted

Submitted means the user has intentionally moved the record from draft/intake into a reviewable or actionable state.

A submitted record may still require approval, acceptance, assignment, or activation.

Submit actions must clearly explain business effects before execution.

## 7. Pending review

Pending review means a human or system must evaluate the record before it can proceed.

Examples:

- Compliance Core import mapping review
- AssurArr finding review
- SupplyArr supplier approval review
- RecordArr document review
- TrainArr evaluation signoff

Pending review must identify the reviewing product, role, team, person, or queue where possible.

## 8. Pending approval

Pending approval means the record is waiting for an authority decision.

Approvals must record:

- Approver role/person/team
- Approval reason
- Requested time
- Due time when applicable
- Decision
- Decision time
- Decision notes when required

Approval and review are related but not identical.

## 9. Approved and rejected

Approved means the approving authority accepted the record or transition.

Rejected means the approving authority refused it.

Rejected records must provide a reason and next path when possible:

- Revise draft
- Resubmit
- Cancel
- Archive
- Escalate

## 10. Active

Active means the record is valid for current operational use.

Examples:

- Active person
- Active asset
- Active supplier
- Active qualification
- Active rulepack
- Active document version

Active must not be used for records that are only scheduled, draft, pending approval, or archived.

## 11. Scheduled

Scheduled means planned for future execution.

Examples:

- Scheduled trip
- Scheduled inspection
- Scheduled training session
- Scheduled report
- Scheduled PM

Scheduled records may still be blocked before start.

## 12. In progress

In progress means execution has started and is not complete.

Products should define what starts progress.

Examples:

- Work order started
- Trip departed
- Training assignment begun
- Receiving started
- CAPA action underway

## 13. Blocked

Blocked means the record cannot proceed until a required issue is resolved.

A blocker must include:

- Source product or rule
- Reason
- Severity
- Required clearing action
- Owner or queue when available

Blocked is not just a color. It is a business state.

## 14. Watch

Watch means the record may proceed but should be monitored.

Examples:

- Certification expiring soon
- Low inventory warning
- ETA risk
- Evidence due soon
- PM due soon

Watch state must not be confused with blocked state.

## 15. Completed and closed

Completed means the operational work is finished.

Closed means all required review, documentation, evidence, financial handoff, or administrative closeout is done.

Some products may use only one of these if the distinction is not useful.

Examples:

- A work order may be completed by a technician but not closed until reviewed.
- A route may be completed when deliveries are done but not closed until POD/evidence is verified.
- A CAPA may have actions completed but remain open until effectiveness check passes.

## 16. Cancelled

Cancelled means the record was intentionally stopped before completion.

Cancelled records must preserve history and reason.

Cancellation must not be used as deletion.

## 17. Inactive

Inactive means not usable for current operations but still present.

Examples:

- Inactive person
- Inactive supplier
- Inactive asset
- Inactive rulepack

Inactive records may remain visible in history.

## 18. Archived

Archived means retained for history but removed from normal active workflows.

Archived records must not appear in default active selects unless explicitly allowed.

Archived is not deleted.

## 19. Superseded

Superseded means replaced by a newer record or version.

Superseded records should point to the replacement where applicable.

Examples:

- Document version superseded by a new effective version
- Rule mapping superseded by new citation interpretation
- Training program superseded by revised program
- Report snapshot superseded by regenerated package

## 20. Deleted

Deletion should be rare.

Production deletion of business records must be restricted, audited, and usually soft-delete or tombstone-based.

Preproduction may allow destructive deletion or rebase by explicit project policy.

Deleted records referenced by history should resolve to a safe tombstone state rather than breaking pages.

## 21. Lifecycle transition rules

Every state transition must define:

- From state
- To state
- Actor/permission required
- Validation required
- Business effect
- Events emitted
- Notifications/handoffs triggered
- Audit entry

State transitions must not happen as hidden side effects of ordinary field edits unless explicitly documented.

## 22. Status display

UI statuses must use readable labels.

Avoid showing only raw enum values.

Use badges, text, and explanations where decisions are affected.

Color may reinforce state but must not be the only signal.

## 23. Product-specific mapping examples

### MaintainArr work order

- `draft` → planned but not released
- `submitted` → requested
- `approved` → authorized
- `scheduled` → planned for a date/person/team
- `in_progress` → work started
- `blocked` → cannot continue
- `completed` → technician finished
- `closed` → reviewed and finalized
- `cancelled` → stopped

### TrainArr assignment

- `assigned`
- `in_progress`
- `pending_evaluation`
- `completed`
- `expired`
- `revoked`
- `remediation_required`

These must map to platform lifecycle categories when reported cross-suite.

### RecordArr document

- `draft`
- `pending_review`
- `approved`
- `effective`
- `expired`
- `superseded`
- `archived`
- `legal_hold`

## 24. Anti-patterns

The following are not allowed:

- Using `complete` when evidence/approval is still missing
- Using `active` for draft records
- Hiding state changes in generic save actions
- Treating cancelled as deleted
- Treating archived as unavailable history
- Allowing blocked records to proceed without override/audit
- Product-specific status labels that cannot map to platform lifecycle for reports
- Frontend-only lifecycle rules

## 25. Minimum acceptable implementation

A lifecycle model is minimally acceptable when it has:

1. Clear owned record type
2. Defined local statuses
3. Mapping to platform lifecycle categories where applicable
4. Explicit transition rules
5. Permission checks for state changes
6. Audit/activity events for material transitions
7. Plain-language status labels
8. Safe archive/supersede/delete behavior
