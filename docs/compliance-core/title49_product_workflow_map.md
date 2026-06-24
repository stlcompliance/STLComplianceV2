# Title 49 product workflow map

| Product | Owned records | Publishes facts for |
| --- | --- | --- |
| StaffArr | people/incidents/history/overrides | Driver qualification, medical status, drug/alcohol events, accident actions, hazmat worker records. |
| TrainArr | ELDT, hazmat, recurrent, retraining, certs | ELDT, hazmat employee training, recurrent training and retraining facts. |
| MaintainArr | assets, DVIR, inspections, PM, defects, repairs, readiness | Vehicle parts/accessories, DVIR, annual inspection, roadside correction, out-of-service readiness facts. |
| RoutArr | assignments, dispatch, HOS/ELD, hazmat load gates | Dispatch gates for driver eligibility, HOS/ELD, cargo securement, hazmat loading/placarding/routing facts. |
| SupplyArr | materials, SDS, classification, packaging, shipment docs | Hazmat applicability, classification, HMT lookup, packaging, shipping papers, markings, labels, placards facts. |

Compliance Core owns rule packs, citations, fact requirements, audit contracts, rule evaluation, evidence references, audit traces, and report surfaces. Product apps own operational records and publish facts and evidence references. No cross-product DB FKs are introduced.

NexArr owns platform authentication, tenant membership, platform administration, and launch/service context. The Compliance Core administrative studio is platform-admin-only; Compliance Core runtime results are available through authorized tenant product workflows.
