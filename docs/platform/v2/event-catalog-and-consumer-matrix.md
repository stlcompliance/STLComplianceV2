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
| `supplyarr.purchase_order.issued` | SupplyArr | LoadArr, OrdArr, ReportArr | expected receipt |
| `loadarr.receipt.completed` | LoadArr | SupplyArr, OrdArr, ReportArr | procurement/fulfillment update |
| `loadarr.inventory_balance.changed` | LoadArr | OrdArr, MaintainArr, ReportArr | availability/readiness update |
| `loadarr.pick.completed` | LoadArr | OrdArr, RoutArr, ReportArr | ready for staging/dispatch |
| `loadarr.inventory_hold.created` | LoadArr | AssurArr, OrdArr, ReportArr | block inventory use |
| `assurarr.hold.created` | AssurArr | LoadArr, OrdArr, SupplyArr, MaintainArr | block use/release |
| `assurarr.hold.released` | AssurArr | LoadArr, OrdArr, SupplyArr, MaintainArr | clear blocker |
| `assurarr.capa.opened` | AssurArr | StaffArr, TrainArr, MaintainArr, ReportArr | corrective work/remediation |
| `ordarr.order.created` | OrdArr | CustomArr, LoadArr, RoutArr, SupplyArr, ReportArr | start orchestration |
| `ordarr.order.triaged` | OrdArr | execution products | create handoffs |
| `ordarr.handoff.requested` | OrdArr | target product | review/accept/reject work |
| `ordarr.order.completed` | OrdArr | RecordArr, external finance integration, ReportArr | package/finance handoff |
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
| `routarr.finance_packet.contribution_ready` | RoutArr | OrdArr, SupplyArr, RecordArr | financial handoff packet contribution |
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
