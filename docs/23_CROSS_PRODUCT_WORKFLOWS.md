# Cross-Product Workflows

## New Employee to Qualified Worker

1. NexArr validates tenant and product access.
2. StaffArr creates person and org assignment.
3. StaffArr assigns permissions.
4. TrainArr determines requirements from role, site, department, equipment, material, or task.
5. TrainArr assigns programs.
6. Trainee completes evidence and steps.
7. Trainer/evaluator signs off.
8. TrainArr issues qualification.
9. TrainArr publishes to StaffArr.
10. StaffArr recalculates readiness.
11. Operational products check readiness before assigning work.

## Asset to Dispatch-Ready

1. MaintainArr creates and classifies asset.
2. MaintainArr assigns inspections and PM.
3. Defects and work orders affect readiness.
4. Compliance Core evaluates maintenance facts where needed.
5. RoutArr checks asset readiness before dispatch.

## Failed Inspection to Work Order

1. Technician performs inspection.
2. Defect is created with evidence.
3. Asset readiness changes.
4. Work order is created.
5. StaffArr/TrainArr validate worker authorization where required.
6. Repair is completed and verified.
7. Asset readiness recalculates.

## Work Order Parts Demand to SupplyArr

1. MaintainArr creates parts demand.
2. SupplyArr checks availability or creates purchase request.
3. SupplyArr owns approval, PO, receiving, and inventory.
4. MaintainArr displays status and records cost snapshot when consumed.

## Route Assignment with Checks

1. RoutArr creates route/trip.
2. RoutArr checks StaffArr driver readiness.
3. RoutArr checks TrainArr qualification.
4. RoutArr checks MaintainArr asset readiness.
5. RoutArr asks Compliance Core for rule context where needed.
6. RoutArr stores decision snapshot.

## Incident to Retraining

1. Product reports incident.
2. StaffArr records involved people and readiness impact.
3. TrainArr assigns remediation where applicable.
4. Completion publishes back to StaffArr.
5. Products consume new readiness state.

## Audit Package

1. Owning product gathers source records.
2. Related product references are resolved through APIs.
3. Point-in-time snapshots are included where needed.
4. Export identifies source product for every record.
