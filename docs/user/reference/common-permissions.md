# Common Permissions

Permissions and role keys vary by product. If an action is missing, check entitlement first, then product role or permission.

## Suite roles
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

## CustomArr examples
- customarr.accounts.read: view customer account context.
- customarr.accounts.manage: create or update customer accounts.
- customarr.contacts.manage: manage customer contacts and authorization records.
- customarr.locations.manage: manage customer locations and access requirements.
- customarr.leads.manage: create and update lead records.
- customarr.leads.convert: convert leads into customer accounts and opportunities.
- customarr.opportunities.manage: create and update opportunities.
- customarr.opportunities.handoff: mark opportunities won and request downstream handoffs.
- customarr.proposals.manage: create and update proposal snapshots.
- customarr.proposals.accept: record customer proposal acceptance and request downstream handoffs.
- customarr.cases.manage: manage customer relationship cases.
- customarr.eligibility.check: run customer eligibility checks before handoff.
- customarr.portal_access.manage: manage customer portal access records.
- customarr.imports.manage: manage imports, duplicate review, and merge proposals.
- customarr.integration_references.manage: manage external mappings and integration references.

## OrdArr examples
- ordarr.orders.read: view order and request context.
- ordarr.orders.create: create order requests.
- ordarr.orders.manage: manage order lifecycle and coordination.
- ordarr.handoffs.manage: launch or manage product handoffs.
- ordarr.exceptions.resolve: resolve order coordination exceptions.
- ordarr.packets.prepare: prepare completion or finance handoff packets.
- ordarr.packets.export: export approved handoff packets.

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
