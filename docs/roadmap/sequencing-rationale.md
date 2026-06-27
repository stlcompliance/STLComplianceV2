# Sequencing Rationale

## Why the roadmap changes

The existing docs describe a complete product universe. That is valuable, but it can accidentally create an implementation pattern where every product receives a list page, dashboard, drawer, detail page, and settings area before any one workflow is undeniably real.

The reconfigured rollout changes the question from “What features exist?” to “Which vertical proof do we ship next?”

## Key sequencing decisions

| Decision | Rationale |
| --- | --- |
| R0 before feature expansion | Several products can look production-like while still depending on partial/scaffolded behavior. Trust must be repaired before scope increases. |
| StaffArr, NexArr, and RecordArr first | Products cannot safely reference people, authority, locations, sessions, or evidence without them. |
| Compliance Core early but narrow | Regulatory meaning must be centralized, but first release should prove one operational spine deeply rather than shallowly cover every law domain. |
| MaintainArr as first flagship slice | It already has strong durable surface area and naturally proves assets, work, evidence, readiness, training, inventory demand, and compliance guidance. |
| TrainArr immediately after MaintainArr | Qualification truth makes operational gating meaningful and turns incidents into retraining loops. |
| SupplyArr and LoadArr together | Procurement expectation and physical inventory ledger need each other for the parts loop. |
| AssurArr after inventory/maintenance loops | Quality holds matter once there are real assets, inventory, suppliers, and orders to block or release. |
| CustomArr before OrdArr | Orders require customer truth, contacts, locations, requirements, eligibility, and preferences. |
| OrdArr before RoutArr | Transportation should execute explicit demand, not become a generic dispatcher detached from order/customer truth. |
| Field Companion after owning APIs | Mobile/offline work must replay through products that already enforce workflow, permission, and idempotency. |
| ReportArr later, reportability immediately | Formal BI should wait for source events/read models, but every release must emit source refs and evidence hooks from day one. |
| LedgArr bridge-first | Finance should govern packets, legal entities, dimensions, and ERP bridges after operational loops produce trustworthy source data. |
| R12 retains category depth | Advanced features are preserved, but they should not delay the first shippable operating spine. |

## Correct commercial wedge

The earliest credible wedge is not “fifteen apps.” It is a maintenance-heavy compliance operating loop:

MaintainArr + StaffArr + TrainArr + RecordArr + Compliance Core, followed by SupplyArr/LoadArr for parts and inventory, then AssurArr quality holds.

That path gives the suite a real center of gravity while preserving the full long-term product map.
