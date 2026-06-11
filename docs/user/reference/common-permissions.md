# Common Permissions

Permissions and role keys vary by product. If an action is missing, check entitlement first, then product role or permission.

## Platform roles
- platform_admin: platform administration access in NexArr.
- tenant_admin: tenant-level administrative role used by multiple products.

## StaffArr examples
- staffarr.incidents.manage: shown in the StaffArr incident panel for users who can create or manage incidents.
- staffarr_admin and hr_admin: roles that can manage StaffArr incidents in the current implementation.
- supervisor: can receive some StaffArr field incident manager context.

## TrainArr examples
- trainee: allowed signoff role for trainee signoff.
- trainer: allowed signoff role for trainer signoff.

## MaintainArr examples
- maintainarr_admin, maintainarr_manager, maintainarr_technician: role keys used by MaintainArr authorization.
- maintainarr.assets.create: asset management access.
- maintainarr.pm.manage: preventive maintenance management access.
- maintainarr.inspections.manage: inspection template management access.
- maintainarr.pmPrograms.create and maintainarr.pmPrograms.activate: PM program actions.

## RoutArr examples
- routarr_admin, routarr_manager, routarr_dispatcher, routarr_driver: role keys used by RoutArr authorization.
- routarr.routes.create: trip or route creation access.
- routarr.dispatch.assign: driver or equipment assignment access.
- routarr.trips.perform: trip execution access.
- routarr.dispatch.manage: dispatch management access.

## SupplyArr examples
- supplyarr_admin, supplyarr_manager, supplyarr_buyer, supplyarr_clerk: role keys used by SupplyArr authorization.
- supplyarr.parties.manage: manage vendors and supplier parties.
- supplyarr.parts.manage: manage parts.
- supplyarr.purchaseRequests.create: create purchase requests.
- supplyarr.purchaseRequests.approve: approve purchase requests.

## LoadArr examples
- loadarr.receiving.create
- loadarr.receiving.confirm
- loadarr.putaway.execute
- loadarr.inventory.read
- loadarr.inventory.hold
- loadarr.inventory.release
- loadarr.transfers.create
- loadarr.transfers.execute
- loadarr.exceptions.resolve

## Compliance Core examples
- compliance_admin
- compliance_reviewer
- tenant_member
- compliancecore.import.create
- compliancecore.import.read
- compliancecore.import.validate
- compliancecore.import.map
- compliancecore.import.commit
- compliancecore.simulation.evaluate

## ReportArr examples
- report_builder
- reportarr_builder
- report_runner
- reportarr_runner
- report_scheduler
- reportarr_scheduler
- analytics_admin
- reportarr_admin
- compliance_reporter

## RecordArr examples
- recordarr.records.read
- recordarr.files.download
- read
- download
