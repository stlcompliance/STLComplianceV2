# LoadArr Rule

LoadArr is the warehouse execution system for the STL Compliance / Arr ecosystem.

LoadArr owns warehouse operations, inventory execution, stock state, warehouse tasks, physical movement, receiving, putaway, picking, packing, staging, shipping, counts, holds, quarantine, kitting, truck stock, manufacturing stock creation, and inventory origin events.

LoadArr must not become the source of truth for platform identity, people, tenants, product entitlement, canonical sites, procurement, vendor management, customer management, external-party records, training definitions, certification issuance, maintenance execution, dispatch execution, or regulatory rule authoring.

## Product Ownership Boundaries

NexArr owns:
- Platform login
- Tenant validation
- Product entitlement
- Service tokens
- Product launch/handoff
- Platform identity gate

StaffArr owns:
- People
- Person records
- PersonId
- Teams
- Departments
- Positions
- Org units
- Canonical internal sites
- StaffArr OrgUnit rows where UnitType = "site"
- User/person active status
- Platform-visible person history
- Product permission assignment surfaces where applicable

TrainArr owns:
- Training workflows
- Training definitions
- Training steps
- Evaluations
- Signoffs
- Remediation
- Training-derived qualifications
- Qualification/certification publishing to StaffArr

SupplyArr owns:
- Item master
- Parts/material master records
- Vendor records
- Customer records
- External-party records
- Approved substitutes
- Vendor part numbers
- Manufacturer part numbers
- Units of measure
- Purchasing
- Purchase orders
- Purchase requests
- Pricing snapshots
- Lead-time snapshots
- Vendor/customer documents
- Commercial sourcing context

MaintainArr owns:
- Assets
- Components
- Preventive maintenance
- Work orders
- Inspections
- Repairs
- Defects
- Maintenance readiness
- Maintenance labor/context
- Work-order parts demand

RoutArr owns:
- Dispatch
- Routes
- Trips
- Stops
- Driver/vehicle assignment
- Delivery execution
- Pickup/delivery exceptions
- Transportation handoff after warehouse loading

Compliance Core owns:
- Rule catalogs
- Governing bodies
- Regulatory vocabulary
- Applicability logic
- Evidence requirements
- Compliance evaluations
- Exceptions/exemptions
- Citation-backed compliance guidance
- Storage/handling/shipping rule interpretation where applicable

LoadArr owns:
- Warehouse utilization of StaffArr-owned locations
- Inventory state at StaffArr-owned locations
- Warehouse execution workflows that reference StaffArr-owned locations
- Inventory balances
- Inventory movements
- Receiving execution
- Putaway execution
- Pick/pack/stage/ship execution
- Transfer execution
- Cycle counts
- Physical inventory
- Inventory holds
- Quarantine
- Warehouse task queues
- Kitting execution
- Truck/mobile stock
- Dock/yard warehouse-side staging
- Manufacturing stock creation records
- Builds and conversions
- Recovered inventory
- Unexplained inventory queue
- Inventory origin events
- Lot/serial/container/license-plate warehouse state
- Audit trails for warehouse activity

## Canonical Site Rule

StaffArr OrgUnit rows with UnitType = "site" are the only canonical internal site identity across the suite.

LoadArr must not create canonical sites.

Whenever LoadArr references an internal site, it must store:

- staffarrSiteOrgUnitId
- staffarrSiteNameSnapshot where useful for display/history

StaffArr owns all internal locations, including warehouse-operational locations such as:

- Warehouse
- Parts room
- Dock
- Yard
- Aisle
- Bay
- Shelf
- Bin
- Cage
- Freezer
- Cooler
- Hazmat cabinet
- Staging lane
- Service truck
- Tool crib
- Quarantine area
- Scrap area

These are StaffArr-owned locations utilized by LoadArr and other apps according to product workflows.

## Item Master Rule

SupplyArr is the source of truth for item identity.

LoadArr must reference SupplyArr items rather than duplicating item master ownership.

LoadArr may store item snapshots for historical display and audit purposes, but the authoritative item, SKU, substitute, unit-of-measure, vendor, manufacturer, cost, and sourcing records belong to SupplyArr.

LoadArr owns the warehouse state of those items:

- Quantity on hand
- Quantity available
- Quantity reserved
- Quantity allocated
- Quantity picked
- Quantity staged
- Quantity in transit
- Quantity quarantined
- Quantity damaged
- Quantity expired
- Quantity pending inspection
- Quantity blocked by compliance
- Location-level balances
- Lot/serial/container/license-plate state

## Inventory Origin Rule

Inventory must never silently appear out of nowhere.

Every stock increase must have an origin event.

Valid inventory origin event types include:

- purchase_receipt
- transfer_receipt
- production_receipt
- build_output
- assembly_output
- disassembly_recovery
- conversion_output
- repack_output
- relabel_output
- repair_output
- refurb_output
- rework_output
- customer_supplied_material
- vendor_consignment_receipt
- return_to_stock
- route_return
- work_order_return
- cycle_count_gain
- manual_adjustment
- migration_opening_balance
- scrap_reversal

A stock increase without a trusted origin must enter an Unexplained Inventory workflow and must not become trusted available inventory until resolved according to permission, evidence, approval, and Compliance Core rules.

## Manufacturing and Conversion Rule

LoadArr may support light manufacturing inventory execution without becoming a full ERP or MES.

LoadArr may own:

- Builds
- Simple assemblies
- Kitting
- Repackaging
- Relabeling
- Cut/split/merge conversions
- Bulk-to-each conversions
- Disassembly recovery
- Repair/refurbishment stock output
- Rework output
- Production receipts
- Scrap and yield recording
- Lot genealogy
- Material consumption

LoadArr must not own advanced manufacturing planning unless a future dedicated manufacturing product is created.

A future manufacturing product may own:

- Manufacturing orders
- BOM engineering
- Routings
- Work centers
- Production scheduling
- Machine steps
- Labor routing
- Batch records
- Advanced yield/costing

If such a product exists, LoadArr remains the inventory execution layer:
- Consume material
- Receive output
- Track stock
- Track lots/serials
- Move finished goods
- Maintain inventory audit trail

## Warehouse Task Rule

LoadArr should be task-driven.

Warehouse work should be represented as WarehouseTask records whenever practical.

Task examples:

- Receive
- Inspect
- Put away
- Pick
- Pack
- Stage
- Load
- Transfer
- Count
- Recount
- Replenish
- Build kit
- Break kit
- Convert stock
- Quarantine
- Release hold
- Scrap
- Return
- Investigate variance
- Resolve shortage
- Resolve unexplained inventory

Every task should know:

- Tenant
- StaffArr site
- Location
- Assigned person or team
- Required permission
- Required qualification
- Required equipment if applicable
- Source object
- Destination object
- Related product/object
- Status
- Priority
- Due date/time where applicable
- Compliance blocker
- Evidence requirements
- Audit history

## Compliance Rule

Compliance Core validates rules; LoadArr enforces operational outcomes.

LoadArr should call Compliance Core when warehouse activity may have compliance impact, including:

- Receiving regulated material
- Storing regulated material
- Moving incompatible material
- Releasing held/quarantined material
- Picking hazardous material
- Shipping regulated material
- Scrapping controlled material
- Handling expired material
- Creating or approving unexplained inventory
- Assigning tasks requiring qualification
- Producing or converting regulated inventory
- Creating audit packages

LoadArr should store compliance evaluation snapshots where needed for audit history.

LoadArr must not hard-code governing body catalogs or regulatory rule ownership that belongs in Compliance Core.

## Training and Qualification Rule

LoadArr must check StaffArr/TrainArr qualification state before allowing restricted warehouse work.

Examples of restricted work:

- Forklift operation
- Hazmat handling
- DOT hazmat shipping
- Chemical handling
- Spill response
- Cold storage handling
- High-value inventory adjustment
- Controlled material release
- Quarantine release
- Cycle count variance approval
- Inventory adjustment approval
- Manufacturing/build approval where required

If a person is not qualified, LoadArr should block the task, warn the supervisor, or route the issue according to rule configuration.

Training-related warehouse incidents should be reported to StaffArr, which may route retraining evaluation to TrainArr.

## MaintainArr Integration Rule

MaintainArr must not directly move inventory.

MaintainArr may create demand for parts/materials.

LoadArr owns the warehouse execution of that demand.

MaintainArr can request:

- Work order parts reservation
- PM kit reservation
- Pick request
- Technician pickup
- Truck stock issue
- Return unused parts
- Core return
- Scrap/repair/recovery flow

LoadArr responds with:

- Available
- Reserved
- Allocated
- Picked
- Staged
- Picked up
- Short
- Backordered
- Substituted
- Blocked by compliance
- Blocked by qualification
- Returned
- Scrapped

## RoutArr Integration Rule

RoutArr owns transportation execution.

LoadArr owns warehouse-side staging/loading until handoff.

LoadArr may stage goods for a route, verify load, capture dock evidence, and mark warehouse handoff complete.

After driver/route handoff, RoutArr owns trip execution and delivery exceptions.

Returns or delivery exceptions that affect inventory must create LoadArr inventory events.

## SupplyArr Integration Rule

SupplyArr owns commercial/procurement context.

LoadArr owns physical inventory execution.

SupplyArr creates or owns purchase orders, item data, sourcing, substitutes, vendor docs, and procurement workflows.

LoadArr receives against SupplyArr purchase orders and raises shortages, demand, receiving discrepancies, damaged receipt evidence, and reorder signals back to SupplyArr.

## No Raw JSON / User Experience Rule

LoadArr UI must not expose raw JSON to end users.

Users should not need to free-type operational values when a controlled list, scanner, lookup, reference provider, or guided workflow can be used.

Prefer:
- Selectors
- Searchable dropdowns
- Scan actions
- Guided steps
- Reason-code buttons
- Confirmation screens
- Evidence upload prompts
- Permission-aware action menus

## Audit Rule

Every meaningful warehouse action must be auditable.

Audit records should capture:

- Who performed the action
- PersonId
- TenantId
- StaffArr site
- Source location
- Destination location
- Item
- Quantity
- Unit of measure
- Lot
- Serial
- License plate/container
- Condition
- Status before
- Status after
- Related product/object
- Reason code
- Evidence
- Compliance evaluation snapshot
- Approval if required
- Timestamp

## Preproduction Implementation Rule

The platform is preproduction.

Backwards compatibility is not required unless explicitly requested.

Prefer clean hard cutovers, destructive schema corrections, migration flattening, and canonical models over compatibility shims, duplicate ownership, or shadow models.

If SupplyArr currently contains WMS-like inventory behavior that belongs in LoadArr, move ownership cleanly to LoadArr and leave SupplyArr responsible for item/procurement/vendor/customer ownership.

Do not preserve confusing overlap for the sake of compatibility.
