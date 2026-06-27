# Product Rollout Lanes

This table tells implementers when each product becomes a release owner. Product feature catalogs still define the full end-state scope.

| Product | Category | Entry release | Completion release | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- | --- | --- | --- |
| NexArr | Platform/IAM | R1 | R1 | 69 | 14 | Foundation spine |
| RecordArr | DMS / evidence vault | R1 | R3 | 69 | 15 | Foundation evidence layer |
| StaffArr | HRM / people, roles, locations | R1 | R4 | 72 | 15 | Foundation spine and qualification gate |
| Compliance Core | GRC / rule engine | R2 | R2 | 79 | 17 | Compliance guidance baseline |
| MaintainArr | CMMS / EAM | R3 | R3 | 73 | 14 | First flagship operational slice |
| TrainArr | LMS / qualifications | R4 | R4 | 73 | 15 | Qualification and retraining gate |
| LoadArr | WMS / inventory | R5 | R5 | 71 | 15 | Parts/procurement/inventory loop |
| SupplyArr | SRM / procurement | R5 | R5 | 72 | 16 | Parts/procurement/inventory loop |
| AssurArr | QMS | R6 | R6 | 68 | 14 | Quality hold and corrective action loop |
| CustomArr | CRM | R7A | R7A | 70 | 16 | Customer master before order orchestration |
| OrdArr | OMS | R7B | R7B | 66 | 15 | Order/request orchestration after customer master baseline |
| RoutArr | TMS | R8 | R8 | 73 | 15 | Dispatch and transportation execution |
| Field Companion | MAM / mobile companion | R9 | R9 | 71 | 17 | Mobile execution after owning APIs are durable |
| ReportArr | BI / reporting | R10 | R10 | 68 | 15 | Operational reporting after source events exist |
| LedgArr | ERP / finance bridge | R11 | R11 | 75 | 20 | Bridge-first finance after operating loops produce trustworthy packets |
| STLComplianceSite | Public site / lead intake | R12 | R12 | 0 | 0 | Public presence and lead handoff once product promises match rollout reality |

## Product lane rules

- Entry release means the first release where that product becomes a central owner in the rollout, not the only release where it receives work.
- Completion release means the release where common category baseline should be made credible for the first usable slice.
- Expansion release retains advanced, broadly requested, or democratized capabilities that should not block the first vertical proof.
- Products can receive supporting work earlier if the current vertical slice requires it, but not by violating source-of-truth ownership.
