# STL Compliance V2 Event Catalog and Consumer Matrix

## Purpose

This document provides a starter event catalog for V2 implementation.

The existing events/handoffs/read-models constitution governs envelope, naming, outbox, replay, idempotency, and source-of-truth behavior.

## Event naming rule

Use canonical machine product keys:

```text
{productKey}.{resource}.{past_tense_fact}
```

## Starter consumer matrix

| Event | Source owner | Likely consumers | Typical effect |
|---|---|---|---|
| `nexarr.entitlement.granted` | NexArr | Product APIs, ReportArr | enable launch/visibility |
| `nexarr.entitlement.revoked` | NexArr | Product APIs, Field Companion | disable launch/actions |
| `staffarr.person.deactivated` | StaffArr | TrainArr, RoutArr, MaintainArr, Field Companion | block assignments/dispatch/work |
| `staffarr.location.created` | StaffArr | MaintainArr, LoadArr, SupplyArr, RoutArr, ReportArr | refresh location refs |
| `staffarr.permission.assigned` | StaffArr | product APIs, Field Companion | refresh authority context |
| `staffarr.incident.forwarded_to_trainarr` | StaffArr | TrainArr | create remediation evaluation |
| `trainarr.qualification.issued` | TrainArr | StaffArr, RoutArr, MaintainArr | allow qualified actions |
| `trainarr.qualification.expired` | TrainArr | StaffArr, RoutArr, MaintainArr | create readiness blockers |
| `maintainarr.asset.readiness_changed` | MaintainArr | RoutArr, OrdArr, ReportArr, Field Companion | block/allow dispatch or use |
| `maintainarr.defect.created` | MaintainArr | AssurArr, StaffArr, ReportArr | quality/personnel review if applicable |
| `maintainarr.work_order.created` | MaintainArr | LoadArr, SupplyArr, Field Companion, ReportArr | assign work/request parts |
| `maintainarr.work_order.closed` | MaintainArr | OrdArr, RecordArr, ReportArr | close handoff/package evidence |
| `maintainarr.parts_demand.created` | MaintainArr | LoadArr, SupplyArr | reserve/issue/procure parts |
| `supplyarr.supplier.status_changed` | SupplyArr | OrdArr, LoadArr, ReportArr | eligibility/blockers |
| `supplyarr.purchase_order.issued` | SupplyArr | LoadArr, OrdArr, LedgArr, ReportArr | expected receipt and purchase commitment packet |
| `loadarr.receipt.completed` | LoadArr | SupplyArr, OrdArr, LedgArr, ReportArr | procurement/fulfillment update and receiving accrual packet |
| `loadarr.inventory_balance.changed` | LoadArr | OrdArr, MaintainArr, LedgArr, ReportArr | availability/readiness update and valuation trigger |
| `loadarr.pick.completed` | LoadArr | OrdArr, RoutArr, ReportArr | ready for staging/dispatch |
| `loadarr.inventory_hold.created` | LoadArr | AssurArr, OrdArr, ReportArr | block inventory use |
| `assurarr.hold.created` | AssurArr | LoadArr, OrdArr, SupplyArr, MaintainArr | block use/release |
| `assurarr.hold.released` | AssurArr | LoadArr, OrdArr, SupplyArr, MaintainArr | clear blocker |
| `assurarr.capa.opened` | AssurArr | StaffArr, TrainArr, MaintainArr, ReportArr | corrective work/remediation |
| `ordarr.order.created` | OrdArr | CustomArr, LoadArr, RoutArr, SupplyArr, ReportArr | start orchestration |
| `ordarr.order.triaged` | OrdArr | execution products | create handoffs |
| `ordarr.handoff.requested` | OrdArr | target product | review/accept/reject work |
| `ordarr.order.completed` | OrdArr | RecordArr, LedgArr, ReportArr | package and invoice-ready financial packet |
| `customarr.customer_requirement.created` | CustomArr | OrdArr, RoutArr, LoadArr, ReportArr | apply requirements |
| `routarr.trip.dispatched` | RoutArr | OrdArr, LoadArr, CustomArr, ReportArr | status update |
| `routarr.proof.captured` | RoutArr | RecordArr, OrdArr, ReportArr | delivery evidence |
| `routarr.trip.exception_created` | RoutArr | OrdArr, StaffArr, MaintainArr, ReportArr | exception/remediation |
| `routarr.transportation_demand.created` | RoutArr | OrdArr, LoadArr, SupplyArr, CustomArr, ReportArr | transportation demand visibility |
| `routarr.tender.accepted` | RoutArr | SupplyArr, OrdArr, ReportArr | carrier tender status snapshot |
| `routarr.freight_rate.estimated` | RoutArr | OrdArr, SupplyArr, ReportArr | freight packet preparation |
| `routarr.visibility_event.received` | RoutArr | OrdArr, CustomArr, LoadArr, ReportArr | tracking/status visibility |
| `routarr.gate.in` | RoutArr | LoadArr, MaintainArr, ReportArr | yard and appointment context |
| `routarr.freight_claim.requested` | RoutArr | AssurArr, SupplyArr, OrdArr, RecordArr | claim/evidence workflow |
| `routarr.finance_packet.contribution_ready` | RoutArr | OrdArr, SupplyArr, LedgArr, RecordArr | financial handoff packet contribution |
| `recordarr.record.uploaded` | RecordArr | Compliance Core, owning product, ReportArr | classification/evidence |
| `recordarr.package.completed` | RecordArr | requesting product, external portal, ReportArr | package ready |
| `compliancecore.rulepack.activated` | Compliance Core | product APIs, ReportArr | refresh compliance context |
| `compliancecore.requirement.evaluated` | Compliance Core | owning product, ReportArr | missing/satisfied/unknown |
| `compliancecore.evidence_gap.detected` | Compliance Core | RecordArr, owning product | create evidence blocker/review |
| `compliancecore.questionnaire_session.completed` | Compliance Core | requesting product | facts/follow-ups |
| `referencedatacore.dataset_version.published` | ReferenceDataCore | SupplyArr, LoadArr, MaintainArr, ReportArr | refresh reference cache |
| `referencedatacore.reference_entity.merged` | ReferenceDataCore | consuming products | stale reference review |
| `referencedatacore.crosswalk.review_required` | ReferenceDataCore | source product | mapping review |
| `fieldcompanion.capture.uploaded` | Field Companion | RecordArr, owning product | store/attach evidence |
| `fieldcompanion.offline_sync.failed` | Field Companion | owning product, support queue | review failure |
| `reportarr.report_run.completed` | ReportArr | RecordArr, subscribers | store artifact/notify |

## LedgArr financial events

LedgArr events publish financial state after LedgArr has validated tenant entitlement, source references, Financial Legal Entity assignment, fiscal period rules, mapping, and posting controls.

| Event | Source owner | Likely consumers | Typical effect |
|---|---|---|---|
| `ledgarr.financial_packet.received` | LedgArr | source product, ReportArr | packet intake acknowledgement |
| `ledgarr.financial_packet.validation_failed` | LedgArr | source product, ReportArr | review source/mapping issue |
| `ledgarr.financial_packet.needs_mapping` | LedgArr | finance users, source product | resolve account/dimension/entity mapping |
| `ledgarr.financial_packet.mapped` | LedgArr | finance users, source product | preview can be generated |
| `ledgarr.financial_packet.preview_ready` | LedgArr | finance approvers, source product | posting preview awaits approval |
| `ledgarr.financial_packet.approved` | LedgArr | source product, ReportArr | packet cleared for posting |
| `ledgarr.financial_packet.posted` | LedgArr | source product, ReportArr | ledger/subledger updated |
| `ledgarr.financial_packet.rejected` | LedgArr | source product, ReportArr | packet will not post |
| `ledgarr.posting_preview.created` | LedgArr | finance approvers | review balanced preview |
| `ledgarr.journal.submitted` | LedgArr | finance approvers | manual journal awaits approval |
| `ledgarr.journal.approved` | LedgArr | finance users | manual journal cleared for posting |
| `ledgarr.journal.posted` | LedgArr | ReportArr | GL changed |
| `ledgarr.journal.reversed` | LedgArr | ReportArr | correcting entry created |
| `ledgarr.period.closed` | LedgArr | product APIs, ReportArr | normal postings blocked for period |
| `ledgarr.period.reopened` | LedgArr | product APIs, ReportArr | controlled posting resumed |
| `ledgarr.period.locked` | LedgArr | product APIs, ReportArr | all postings blocked except reopening workflow |
| `ledgarr.vendor_bill.created` | LedgArr | SupplyArr, RecordArr, ReportArr | AP bill created |
| `ledgarr.vendor_bill.matched` | LedgArr | SupplyArr, LoadArr | AP matching status updated |
| `ledgarr.vendor_bill.approved` | LedgArr | finance users, ReportArr | AP bill cleared |
| `ledgarr.vendor_bill.posted` | LedgArr | SupplyArr, ReportArr | AP subledger and GL updated |
| `ledgarr.payment_run.created` | LedgArr | finance users | payment batch assembled |
| `ledgarr.payment_run.exported` | LedgArr | external ERP bridge, ReportArr | payment export sent |
| `ledgarr.customer_invoice.created` | LedgArr | OrdArr, CustomArr, ReportArr | AR invoice created |
| `ledgarr.customer_invoice.issued` | LedgArr | OrdArr, CustomArr | customer-facing invoice issued |
| `ledgarr.customer_invoice.posted` | LedgArr | OrdArr, ReportArr | AR subledger and GL updated |
| `ledgarr.customer_payment.recorded` | LedgArr | OrdArr, CustomArr, ReportArr | payment applied |
| `ledgarr.inventory_valuation.updated` | LedgArr | LoadArr, ReportArr | valuation/subledger refreshed |
| `ledgarr.inventory_reconciliation.issue_detected` | LedgArr | LoadArr, AssurArr, ReportArr | inventory financial discrepancy review |
| `ledgarr.fixed_asset.capitalized` | LedgArr | MaintainArr, ReportArr | financial asset record created |
| `ledgarr.fixed_asset.depreciation_posted` | LedgArr | MaintainArr, ReportArr | depreciation posted |
| `ledgarr.budget.approved` | LedgArr | spend-requesting products | budget available for checks |
| `ledgarr.budget.threshold_exceeded` | LedgArr | SupplyArr, MaintainArr, RoutArr, LoadArr | budget warning/blocker |
| `ledgarr.external_export.created` | LedgArr | finance users, ReportArr | export batch staged |
| `ledgarr.external_export.sent` | LedgArr | finance users, ReportArr | external ERP/accounting export sent |
| `ledgarr.external_export.failed` | LedgArr | finance users, ReportArr | export retry/review required |
| `ledgarr.financial_legal_entity.created` | LedgArr | product APIs, ReportArr | accounting entity available |
| `ledgarr.financial_legal_entity.updated` | LedgArr | product APIs, ReportArr | accounting entity mapping should refresh |
| `ledgarr.financial_legal_entity.deactivated` | LedgArr | product APIs, ReportArr | accounting entity no longer selectable |

## Consumer implementation rules

Consumers must:

```text
- verify tenant context
- process idempotently
- store source event reference
- avoid mutating source truth directly
- fetch details from owning product when needed
- create local read models or product-owned tasks only when appropriate
- preserve correlationId
```

## Dead-letter handling

Failed event handling should create:

```text
- failure record
- owning consumer
- retry count
- last error summary
- next retry time
- manual review option
- correlationId
```

Dead-letter queues should not silently discard material business events.
