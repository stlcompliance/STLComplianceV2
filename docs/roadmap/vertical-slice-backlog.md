# Vertical Slice Backlog

The product suite should advance by proving vertical slices, not by making every product's CRUD shell look equally complete.

| Slice | Release | Name | Primary owners | Proof |
| --- | --- | --- | --- | --- |
| V0 | R0 | Trust gate remediation | All products | No production path uses false success, missing tenancy, missing auth, process-global truth, or unreadable UI states. |
| V1 | R1 | Launch, person, location, evidence foundation | NexArr, StaffArr, RecordArr, Platform Reference Data | A product can launch, resolve actor/tenant/person/location/evidence/reference context, and fail truthfully. |
| V2 | R2 | Requirement to evidence guidance | Compliance Core, RecordArr, StaffArr | A product receives missing-fact/evidence-required guidance and attaches evidence without hardcoded regulatory conclusions. |
| V3 | R3 | Defect to work order to return-to-service | MaintainArr with StaffArr, RecordArr, Compliance Core | A defect can create/drive work, collect evidence, block readiness, close work, and explicitly return asset to service. |
| V4 | R4 | Incident to retraining to qualification restored | StaffArr, TrainArr, Compliance Core, RecordArr | An incident triggers retraining, completion/signoff updates qualification truth, and downstream products can check it. |
| V5 | R5 | Part request to PO to receive/putaway/issue | SupplyArr, LoadArr, MaintainArr, RecordArr | Demand becomes procurement expectation, inbound receiving, putaway, reservation, issue, and consumption with ledger trace. |
| V6 | R6 | Quality hold to release and CAPA | AssurArr with affected products | A quality decision blocks affected records, records evidence/reason, drives CAPA, and releases only by authorized action. |
| V7 | R7A/R7B | Customer requirement to order/request orchestration | CustomArr, OrdArr, affected execution products | Customer truth and requirements shape an order/request that hands off work without owning execution truth. |
| V8 | R8 | Order/demand to dispatch/trip/proof | RoutArr, OrdArr, CustomArr, MaintainArr, TrainArr, LoadArr | Dispatch uses readiness snapshots, executes trip/stops, captures exceptions/proof, and ties completion to source demand. |
| V9 | R9 | Mobile/offline work execution | Field Companion plus owning products | A field user can capture evidence and complete assigned work through owning APIs with offline retry truth. |
| V10 | R10 | Audit-ready reporting and drillback | ReportArr, RecordArr, source products | Reports show source provenance, filter/export/print cleanly, and store audit outputs without mutating source truth. |
| V11 | R11 | Finance packet to external ERP bridge | LedgArr plus source products | Financial packets are reviewable, dimensioned, controlled, and exportable/bridgeable while source products remain owners. |

## Slice completion rule

A vertical slice is complete only when the happy path, common blocked path, permission-denied path, stale/reference-missing path, and retry/recovery path are all designed and tested. Each slice must leave behind source refs, events, evidence refs, status history, and reportability hooks.
