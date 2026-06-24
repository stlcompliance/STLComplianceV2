# Workflow Pack — Vendor Order Completion and Dispatch

## Purpose

This workflow defines how an external vendor can confirm completion status before STL dispatches transportation or continues order fulfillment.

This supports operations where a broker or coordinator arranges transportation only after the vendor confirms the order is ready or complete.

## Trigger

```text
Vendor completion request created
```

Possible owners:

```text
OrdArr owns order/request lifecycle.
SupplyArr owns supplier/vendor context.
RoutArr owns dispatch/trip execution.
CustomArr owns customer context.
```

## Participating products

```text
OrdArr
SupplyArr
RoutArr
CustomArr
RecordArr
AssurArr
Compliance Core
Field Companion
ReportArr
NexArr
StaffArr
```

## Source-of-truth table

| Business truth | Owner |
|---|---|
| customer identity and requirements | CustomArr |
| order/request lifecycle | OrdArr |
| vendor/supplier identity and contact | SupplyArr |
| external portal scoped invitation/session | owning product with NexArr/session support |
| dispatch/trip execution | RoutArr |
| evidence files/proof | RecordArr |
| quality hold/release | AssurArr |
| compliance/evidence requirement meaning | Compliance Core |
| user/permission context | StaffArr |
| reporting/read model | ReportArr |

## Main flow

1. OrdArr order requires vendor completion before dispatch.
2. OrdArr links vendor/supplier context from SupplyArr.
3. OrdArr creates vendor completion handoff or request.
4. External portal invitation is generated for authorized vendor contact.
5. Vendor opens scoped portal session.
6. Vendor submits one of:
   - ready_for_pickup
   - complete
   - partially_complete
   - delayed
   - rejected/cannot_complete
   - exception
7. Vendor may upload evidence/photos/documents.
8. RecordArr stores uploads.
9. OrdArr reviews or accepts completion update based on policy.
10. AssurArr may be invoked if quality issue/exception is reported.
11. Compliance Core may evaluate missing evidence or questionnaire facts if needed.
12. RoutArr dispatch is allowed only after required readiness checks pass.
13. RoutArr creates dispatch/route/trip.
14. ReportArr tracks vendor responsiveness and dispatch delay.

## Vendor completion request

```text
VendorCompletionRequest
- requestId
- tenantId
- sourceProduct
- orderRef
- supplierOrVendorRef
- vendorContactRef
- requestedStatus
- allowedResponses
- requiredEvidenceTypes
- dueAt
- status
  - draft
  - sent
  - opened
  - submitted
  - accepted
  - rejected
  - expired
  - canceled
- portalInvitationRef
- submittedAt
- reviewedByPersonId
- auditTrailRef
```

## Vendor response

```text
VendorCompletionResponse
- responseId
- requestId
- externalActorRef
- responseStatus
  - ready_for_pickup
  - complete
  - partially_complete
  - delayed
  - cannot_complete
  - exception_reported
- quantitySummary
- readyAt
- notes
- uploadedRecordRefs
- submittedAt
- reviewStatus
  - accepted
  - review_required
  - rejected
```

## Required events

```text
ordarr.vendor_completion.requested
ordarr.vendor_completion.submitted
ordarr.vendor_completion.accepted
ordarr.vendor_completion.rejected
ordarr.order.ready_for_dispatch
routarr.dispatch.created
routarr.trip.dispatched
recordarr.record.uploaded
assurarr.nonconformance.created
```

## Required handoffs

```text
ordarr -> supplyarr: vendor/contact context check
ordarr -> external portal: completion request
external portal -> ordarr: completion response
ordarr -> recordarr: store completion evidence
ordarr -> assurarr: quality/exception review where needed
ordarr -> routarr: dispatch request when ready
routarr -> ordarr: dispatch/trip status
```

## Dispatch readiness check

Before RoutArr dispatch:

```text
- order is active and not canceled
- customer requirements are satisfied or accepted with warning
- vendor completion is accepted
- required evidence is present or waived
- quality holds are clear
- driver/equipment are available and qualified
- asset readiness is acceptable
- route/trip requirements are satisfied
```

## Blockers

Common blockers:

```text
- vendor has not responded
- vendor response says delayed
- vendor response requires review
- evidence missing
- quantity mismatch
- quality issue reported
- customer requirement blocks dispatch
- driver/equipment not ready
```

## External portal access rules

Vendor portal access must be:

```text
- scoped to one request/order
- time-limited
- action-limited
- revocable
- audited
```

Vendor should not see unrelated customer, vendor, order, route, inventory, or internal staff data.

## Field Companion behavior

Field Companion may show:

```text
- dispatch blocked waiting on vendor
- vendor ready status
- pickup proof task
- exception capture task
```

Field Companion does not own vendor completion truth.

## Evidence package

RecordArr package should include:

```text
- vendor completion request
- vendor response
- portal access log
- uploaded evidence
- review decision
- dispatch readiness result
- trip/proof refs after dispatch
```

## Non-goals

This workflow does not make SupplyArr own dispatch.

RoutArr still owns dispatch/trip execution.

OrdArr owns the order/request lifecycle and readiness for dispatch handoff.
