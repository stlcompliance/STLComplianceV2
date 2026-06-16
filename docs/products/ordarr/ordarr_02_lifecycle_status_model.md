# OrdArr - Lifecycle and Status Model

OrdArr status values must map to the platform record lifecycle constitution.

## OrderRequest status definitions

draft:

- Request has been started but not submitted.
- No target product handoff is created by ordinary draft save.

submitted:

- Request was intentionally submitted for triage, review, approval, or conversion to work.

triage:

- OrdArr is determining ownership, required products, blockers, and next actions.

pending_review:

- A person, team, or source product must review the request before it can proceed.

pending_approval:

- Authority approval is required before work is released.

approved:

- The request/order is approved for orchestration, but execution may not have started.

active:

- The order/request is valid for current operational work.

handoff_requested:

- At least one target product handoff has been requested.

in_progress:

- One or more accepted target handoffs or execution records are in progress.

blocked:

- The order/request cannot proceed until a blocker is cleared.

on_hold:

- Work is intentionally paused. The hold must identify source, reason, and clearing action.

partially_fulfilled:

- Some requested lines or work packages are complete, but the full request is not complete.

fulfilled:

- Requested execution work has been completed by source products, but closeout may still be pending.

completed:

- Operational work is complete and required completion evidence has been assembled.

closed:

- Closeout, review, required documents, and financial handoff packet decisions are complete.

cancelled:

- Request was intentionally stopped before completion. History and reason remain.

archived:

- Retained for history and hidden from ordinary active workflows.

## Lifecycle category mapping

- `draft` maps to `draft`
- `submitted` maps to `submitted`
- `triage` maps to `pending_review`
- `pending_review` maps to `pending_review`
- `pending_approval` maps to `pending_approval`
- `approved` maps to `approved`
- `active` maps to `active`
- `handoff_requested` maps to `in_progress`
- `in_progress` maps to `in_progress`
- `blocked` maps to `blocked`
- `on_hold` maps to `blocked`
- `partially_fulfilled` maps to `in_progress`
- `fulfilled` maps to `completed`
- `completed` maps to `completed`
- `closed` maps to `closed`
- `cancelled` maps to `cancelled`
- `archived` maps to `archived`

## Transition rules

draft -> submitted:

- User intentionally submits request.
- Required intake fields must be present.
- Emits `ordarr.order_request.submitted`.

submitted -> triage:

- OrdArr begins routing and ownership evaluation.
- May be automatic or assigned to a queue.

triage -> pending_review:

- More information or source product review is required.

triage -> pending_approval:

- Approval policy requires authority decision.

pending_approval -> approved:

- Approver grants release.
- Approval decision is audited.

approved -> handoff_requested:

- OrdArr creates one or more product handoffs.
- Handoff creation must be idempotent.

handoff_requested -> in_progress:

- At least one target product accepts and begins work.

any_active_state -> blocked:

- A source product, rule, eligibility check, or requirement prevents progress.
- Blocker must identify source product and required clearing action.

in_progress -> partially_fulfilled:

- Some lines or work packages are completed.

in_progress or partially_fulfilled -> fulfilled:

- Target products report required execution complete.

fulfilled -> completed:

- Required completion evidence and closeout facts are assembled.

completed -> closed:

- Completion packet and finance handoff packet decisions are finished.

any_non_closed_state -> cancelled:

- Request is stopped before completion with reason and permission.

closed -> archived:

- Record is retained for history outside ordinary active workflows.

## Blocker model

Order blockers should include:

- blockerId
- orderRequestId
- sourceProductKey
- sourceRecordRef
- blockerType
- severity
- message
- requiredAction
- ownerProductKey
- ownerQueueRef
- openedAt
- clearedAt
- clearingEventRef

OrdArr may display blockers from other products but must not clear source-owned blockers without an approved API or handoff.

## Status events

- `ordarr.order_request.created`
- `ordarr.order_request.submitted`
- `ordarr.order_request.triage_started`
- `ordarr.order_request.approved`
- `ordarr.order_request.blocked`
- `ordarr.order_request.unblocked`
- `ordarr.order_request.handoff_requested`
- `ordarr.order_request.in_progress`
- `ordarr.order_request.partially_fulfilled`
- `ordarr.order_request.fulfilled`
- `ordarr.order_request.completed`
- `ordarr.order_request.closed`
- `ordarr.order_request.cancelled`
- `ordarr.order_request.archived`
