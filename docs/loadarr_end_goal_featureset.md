# LoadArr End Goal Featureset

LoadArr is the warehouse execution system for STL Compliance.

It is the system that answers:

- Where is the inventory?
- What state is it in?
- Who can touch it?
- Can it legally/safely move?
- What created it?
- What demand needs it?
- What task should happen next?
- What evidence proves what happened?

LoadArr should feel like a practical WMS, but with STL Compliance’s larger Adaptive Risk Reduction purpose built into every movement, hold, release, count, and warehouse decision.

---

# 1. Core Product Purpose

LoadArr manages physical inventory execution across warehouses, parts rooms, docks, yards, staging areas, service trucks, mobile stock, quarantine areas, and manufacturing output locations.

LoadArr is not just an inventory list.

It is the operational layer for:

- Receiving
- Putaway
- Bin/location inventory
- Picking
- Packing
- Staging
- Shipping handoff
- Transfers
- Counts
- Adjustments
- Holds
- Quarantine
- Kitting
- Truck stock
- Dock work
- Warehouse tasks
- Inventory origin tracking
- Builds and conversions
- Recovered inventory
- Manufacturing output receipt
- Compliance-aware inventory control

---

# 2. Product Boundaries

## LoadArr Owns

- Warehouse utilization of StaffArr-owned locations
- Inventory balances
- Inventory movements
- Warehouse task queues
- Receiving execution
- Putaway execution
- Picking execution
- Packing execution
- Shipping/staging execution
- Warehouse-side route handoff
- Transfer execution
- Cycle counts
- Physical inventory
- Inventory adjustments
- Holds and quarantine
- Kit build/break execution
- Truck stock
- Dock and staging lanes
- Lot/serial/license-plate warehouse state
- Inventory origin events
- Manufacturing/build/conversion stock events
- Recovered stock
- Unexplained inventory resolution
- Warehouse evidence and audit trail

## LoadArr Does Not Own

- Platform login: NexArr
- Tenant/product entitlement: NexArr
- Canonical people/personId: StaffArr/NexArr platform identity model
- Canonical internal sites: StaffArr
- Internal locations: StaffArr
- Training definitions and signoffs: TrainArr
- Item master: SupplyArr
- Vendors/customers: SupplyArr
- Purchase orders/procurement: SupplyArr
- Assets/work orders/PMs: MaintainArr
- Routes/trips/dispatch: RoutArr
- Regulatory catalogs/rules: Compliance Core

---

# 3. StaffArr Site Integration

LoadArr must use StaffArr OrgUnit rows with UnitType = "site" as canonical internal site identity.

Every location used by LoadArr must be a StaffArr-owned location tied to a StaffArr site.

Required concepts:

- staffarrSiteOrgUnitId
- staffarrSiteNameSnapshot
- StaffArr location hierarchy consumed by LoadArr
- StaffArr location type consumed by LoadArr
- Location active/inactive status
- Location capacity rules
- Location compliance restrictions

Example StaffArr-owned location hierarchy used by LoadArr:

- Site
  - Warehouse
    - Zone
      - Aisle
        - Bay
          - Shelf
            - Bin

Location types:

- warehouse
- parts_room
- dock
- staging_lane
- yard
- service_truck
- tool_crib
- hazmat_cage
- flammable_cabinet
- quarantine_area
- inspection_area
- scrap_area
- return_area
- production_output
- finished_goods
- raw_material
- cold_storage
- secure_cage

---

# 4. SupplyArr Item Master Integration

SupplyArr owns item identity.

LoadArr consumes SupplyArr item records.

LoadArr should reference:

- supplyarrItemId
- item number
- item name snapshot
- unit of measure snapshot
- item type snapshot
- hazardous/regulated hints
- lot/serial control flags
- approved substitutes
- vendor/manufacturer identifiers
- item dimensions/weight where available

LoadArr owns warehouse state:

- Quantity on hand
- Quantity available
- Quantity reserved
- Quantity allocated
- Quantity picked
- Quantity packed
- Quantity staged
- Quantity in transit
- Quantity quarantined
- Quantity damaged
- Quantity expired
- Quantity pending inspection
- Quantity blocked by compliance
- Quantity blocked by investigation

---

# 5. Inventory State Management

Inventory must support explicit states.

Core states:

- Available
- Reserved
- Allocated
- Picked
- Packed
- Staged
- Loaded
- In transit
- Pending inspection
- Quarantined
- On hold
- Damaged
- Expired
- Scrapped
- Consumed
- Returned
- Blocked
- Unknown/unexplained

Inventory should be trackable by:

- Site
- Location
- Item
- Lot
- Serial
- License plate
- Container
- Pallet
- Condition
- Status
- Origin event
- Related product object

---

# 6. Inventory Origin Events

Every stock increase must have an origin.

Origin event types:

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

Each origin event should capture:

- Tenant
- Site
- Location
- Item
- Quantity
- Unit of measure
- Person
- Timestamp
- Origin type
- Origin product key
- Origin object type
- Origin object id
- Reason code
- Evidence
- Approval if required
- Compliance evaluation snapshot
- Lot/serial/container data
- Condition/status

---

# 7. Receiving

LoadArr should support:

- PO receiving
- Blind receiving
- ASN receiving
- Transfer receiving
- Return receiving
- Work-order return receiving
- Route return receiving
- Customer-supplied material receiving
- Vendor consignment receiving
- Damaged freight receiving
- Hazmat receiving
- Serialized receiving
- Lot-controlled receiving
- Partial receiving
- Overage receiving
- Shortage receiving

Receiving workflow:

1. Select/scan source
2. Confirm supplier/source
3. Select/scan item
4. Confirm quantity
5. Capture lot/serial if required
6. Capture condition
7. Capture photos/evidence if needed
8. Run Compliance Core evaluation if applicable
9. Create inventory origin event
10. Create putaway, inspection, quarantine, or cross-dock task

Receiving statuses:

- Expected
- In progress
- Partially received
- Received
- Discrepant
- Damaged
- Pending inspection
- Quarantined
- Closed

---

# 8. Putaway

Putaway should be guided and rule-aware.

Supported putaway behavior:

- Directed putaway
- Manual putaway
- Compliance-aware putaway
- Hazmat-compatible putaway
- Temperature-controlled putaway
- High-value secure putaway
- Fast-moving slotting
- Overflow putaway
- Quarantine putaway
- Cross-dock bypass

Putaway should consider:

- Site
- Item class
- Storage requirements
- Hazard class
- SDS/document requirements
- Incompatible materials
- Dimensions
- Weight
- Velocity
- Existing stock
- Open demand
- Location capacity
- User qualification
- Equipment requirement

---

# 9. Inventory Locations

LoadArr should support detailed physical location control.

Location features:

- Location hierarchy
- Location type
- Location capacity
- Active/inactive
- Allowed item classes
- Blocked item classes
- Temperature requirements
- Hazmat/storage restrictions
- Secure access flag
- Count frequency
- Assigned team/zone
- Location barcode/QR code
- Location status
- Current inventory
- Movement history

---

# 10. Reservations and Allocations

LoadArr should manage demand from other products.

Demand sources:

- MaintainArr work orders
- MaintainArr PM kits
- RoutArr route loads
- SupplyArr purchase/fulfillment flows
- Manual warehouse requests
- Emergency requests
- Replenishment requests
- Inter-site transfers
- Manufacturing/build requests

Demand states:

- Requested
- Soft reserved
- Hard allocated
- Pick released
- Picked
- Staged
- Fulfilled
- Shorted
- Backordered
- Substituted
- Canceled
- Blocked

---

# 11. Picking

Picking should be scanner-friendly and task-driven.

Pick types:

- Single request picking
- Batch picking
- Wave picking
- Zone picking
- Cluster picking
- Maintenance work-order picking
- Route load picking
- Transfer picking
- Emergency picking
- Hazmat picking
- Serialized picking
- Lot/FEFO picking

Pick workflow:

1. Assign pick task
2. Validate person permission/qualification
3. Navigate to source location
4. Scan location
5. Scan item
6. Confirm quantity
7. Confirm lot/serial if required
8. Handle shortage or substitution
9. Move to destination
10. Capture evidence if required

---

# 12. Packing, Staging, and Shipping Handoff

LoadArr owns warehouse-side packing/staging/shipping handoff.

Features:

- Pack station workflow
- Cartonization
- Palletization
- License plate creation
- Packing slip generation
- Label printing
- Dock staging
- Route staging
- Load verification
- Driver pickup scan
- Carrier handoff
- Proof of pickup
- Proof of shipment
- Short-load handling
- Returned-to-warehouse handling

RoutArr owns the trip after transportation handoff.

---

# 13. Transfers

Transfer types:

- Bin-to-bin transfer
- Zone-to-zone transfer
- Site-to-site transfer
- Warehouse-to-maintenance transfer
- Warehouse-to-route transfer
- Warehouse-to-truck-stock transfer
- Quarantine transfer
- Return-to-stock transfer
- Scrap transfer
- Vendor return transfer

Transfer states:

- Requested
- Approved
- Pick released
- Picked
- In transit
- Received
- Put away
- Completed
- Canceled
- Discrepant

---

# 14. Cycle Counts and Physical Inventory

LoadArr should include strong inventory accuracy workflows.

Count types:

- Cycle count
- Blind count
- Recount
- Location count
- Item count
- Lot count
- Serial count
- Full physical inventory
- Compliance-triggered count
- Variance-triggered count

Variance handling:

- Reason code required
- Approval threshold
- Photo/evidence support
- Compliance Core evaluation for controlled items
- Audit trail
- Recount workflow
- Adjustment workflow

Reason codes:

- Found stock
- Missing stock
- Wrong bin
- Receiving error
- Picking error
- Return not recorded
- Production output not recorded
- Unit-of-measure correction
- Damaged
- Expired
- Scrap not recorded
- Migration correction
- Unknown origin

---

# 15. Holds and Quarantine

Hold types:

- Compliance hold
- Quality hold
- Damage hold
- Recall hold
- Expired material hold
- Missing SDS hold
- Missing document hold
- Wrong item hold
- Receiving discrepancy hold
- Training/qualification hold
- Investigation hold
- Customer hold
- Vendor hold
- Unknown origin hold

Release requirements:

- Permission
- Reason
- Evidence
- Approval if required
- Compliance Core validation if applicable
- Audit record

---

# 16. Builds and Conversions

LoadArr should support light manufacturing inventory execution.

Transaction types:

- Build
- Assemble
- Disassemble
- Convert
- Repack
- Relabel
- Repair
- Refurbish
- Recover
- Blend/mix
- Cut/split
- Combine
- Scrap
- Rework

Supported flows:

- Consume raw materials
- Produce finished goods
- Produce byproducts
- Record scrap
- Record yield variance
- Generate lots/serials
- Preserve lot genealogy
- Send output to inspection/quarantine/putaway
- Create inventory origin events

Example:

- Consume 1 case of 100 items
- Produce 100 eaches
- Preserve original lot
- Print labels
- Move eaches to available stock after validation

---

# 17. Production Receipts

For manufacturing users, LoadArr should support production receipts.

Production receipt fields:

- Production reference
- External manufacturing order reference if applicable
- Item produced
- Quantity produced
- Unit of measure
- Site
- Output location
- Person/team
- Lot/serial
- Raw material references if available
- Scrap quantity
- Rejected quantity
- Pending inspection quantity
- Evidence
- Approval
- Compliance result

Output statuses:

- Pending inspection
- Available
- Quarantined
- Rejected
- Scrapped
- Blocked

---

# 18. Recovered Inventory

LoadArr should support inventory recovered from assets, work orders, routes, teardown, repair, or returns.

Recovery examples:

- Recovered parts from asset teardown
- Core returns
- Repairable parts
- Refurbished stock
- Returned route inventory
- Customer rejected goods
- Found tools
- Reclaimed material
- Reusable offcuts

Recovered stock should often enter:

- Pending inspection
- Quarantine
- Return to stock
- Repair/refurb queue
- Scrap

---

# 19. Unexplained Inventory

LoadArr should have an explicit Unexplained Inventory queue.

This queue captures:

- Found stock
- Unknown lot items
- Unknown serial items
- Items without labels
- Count gains
- Production output not tied to run
- Returned goods without source
- Customer-owned material with unclear owner
- Vendor material not tied to PO
- Inventory without required documents

Resolution statuses:

- Needs investigation
- Needs identification
- Needs document
- Needs approval
- Needs quarantine
- Resolved as valid stock
- Resolved as scrap
- Resolved as duplicate
- Resolved as receiving error
- Resolved as production error
- Resolved as migration correction

---

# 20. Kitting

LoadArr should support:

- Maintenance kits
- PM kits
- Inspection kits
- Emergency kits
- PPE kits
- Route kits
- Training kits
- Hazmat spill kits
- First aid kits
- Tool kits

Kit operations:

- Build kit
- Reserve kit
- Pick kit
- Break kit
- Replenish kit
- Inspect kit
- Assign kit
- Return kit
- Expire kit components
- Track kit location

---

# 21. Truck Stock and Mobile Inventory

LoadArr should support mobile inventory.

Examples:

- Service truck stock
- Technician stock
- Driver stock
- Route vehicle stock
- Mobile bins
- Tool carts
- Emergency kits

Features:

- Truck stock locations
- Person/vehicle association
- Min/max
- Restock request
- Transfer to truck
- Issue from truck
- Return from truck
- Truck stock count
- Truck stock audit

---

# 22. Warehouse Task Engine

LoadArr should include a general warehouse task system.

Task types:

- Receive
- Inspect
- Put away
- Pick
- Pack
- Stage
- Load
- Count
- Transfer
- Replenish
- Quarantine
- Release hold
- Scrap
- Return
- Build
- Convert
- Investigate variance
- Resolve shortage
- Resolve unexplained stock

Task fields:

- Tenant
- StaffArr site
- Task type
- Status
- Priority
- Assigned person
- Assigned team
- Required permission
- Required qualification
- Source location
- Destination location
- Item
- Quantity
- Related product
- Related object
- Compliance blocker
- Evidence requirement
- Due date/time
- Created by
- Completed by
- Completion timestamp

---

# 23. Compliance Core Integration

LoadArr should use Compliance Core to evaluate:

- Storage rules
- Handling rules
- Hazmat rules
- SDS requirements
- Shipping requirements
- Evidence requirements
- Training/qualification requirements
- Hold/release requirements
- Expiration rules
- Lot/serial tracking requirements
- Controlled material rules
- Audit retention requirements
- Exception/exemption logic

LoadArr should store ComplianceEvaluationSnapshot records for audit-critical decisions.

---

# 24. StaffArr and TrainArr Integration

LoadArr should use StaffArr/TrainArr to determine:

- Who the person is
- Whether the person is active
- Which site/team they belong to
- Which permissions they have
- Which qualifications they hold
- Whether they can perform restricted tasks

Blocked examples:

- Unqualified person tries to handle hazmat
- Unauthorized user tries to adjust inventory
- Inactive person attempts task
- User lacks approval permission
- User lacks forklift qualification

---

# 25. MaintainArr Integration

MaintainArr demand should create LoadArr fulfillment workflows.

Supported flows:

- Work-order part demand
- PM kit demand
- Parts reservation
- Pick request
- Technician pickup
- Truck stock issue
- Return unused parts
- Core return
- Scrap part
- Recovered part
- Asset teardown recovery
- Work-order blocked by shortage
- Work-order ready when parts staged

---

# 26. RoutArr Integration

RoutArr should consume LoadArr staging and handoff.

Supported flows:

- Route load demand
- Pick for route
- Stage by route/vehicle
- Driver scan pickup
- Load verification
- Short-load reporting
- Return from route
- Delivery exception inventory impact
- Site-to-site transfer transportation

---

# 27. SupplyArr Integration

SupplyArr and LoadArr should work together tightly.

Supported flows:

- PO created in SupplyArr
- PO received in LoadArr
- Receiving discrepancy reported to SupplyArr
- Damaged receipt evidence reported to SupplyArr
- Shortage detected by LoadArr
- Reorder signal sent to SupplyArr
- Approved substitutes consumed from SupplyArr
- Vendor/customer docs consumed from SupplyArr
- SDS/vendor documents used for Compliance Core validation

---

# 28. Mobile and Scanner Support

LoadArr should be mobile-first.

Mobile workflows:

- Scan receive
- Scan putaway
- Scan pick
- Scan pack
- Scan stage
- Scan load
- Scan transfer
- Scan count
- Scan return
- Scan truck stock
- Scan quarantine
- Scan build/convert
- Scan unexplained inventory

Mobile features:

- Camera barcode scanning
- QR scanning
- Location label scanning
- Item label scanning
- License plate scanning
- Offline-tolerant task queue
- Photo capture
- Signature capture
- Voice notes
- Large buttons
- Glove-friendly UI
- Minimal typing
- Guided reason codes

---

# 29. Reporting and Dashboards

Dashboard cards:

- Open receiving
- Awaiting putaway
- Pick backlog
- Staged shipments
- Short picks
- Compliance holds
- Quarantined inventory
- Unexplained inventory
- Cycle counts due
- Inventory value
- Aging tasks
- Dock workload
- Warehouse incidents
- Production receipts pending inspection
- Builds/conversions in progress

Reports:

- Inventory valuation
- Inventory by site/location
- Inventory by status
- Aging inventory
- Quarantined inventory
- Expired/near-expired inventory
- Movement history
- Lot genealogy
- Serial trace
- Adjustment history
- Cycle count accuracy
- Pick accuracy
- Receiving discrepancies
- Shortage report
- Work-order parts readiness
- Route load readiness
- Compliance-blocked inventory
- Unexplained inventory report
- Production yield/scrap report

---

# 30. Core Screens

Suggested LoadArr navigation:

- Dashboard
- Tasks
- Receiving
- Putaway
- Inventory
- Locations
- Picking
- Packing
- Staging
- Shipping
- Transfers
- Counts
- Builds & Conversions
- Production Receipts
- Kits
- Truck Stock
- Holds & Quarantine
- Unexplained Inventory
- Dock/Yard
- Compliance
- Reports
- Settings

---

# 31. Suggested Data Objects

Core objects:

- StaffArrLocationReference
- InventoryBalance
- InventoryLot
- InventorySerial
- LicensePlate
- InventoryMovement
- InventoryOriginEvent
- WarehouseTask
- ReceivingSession
- ReceivingLine
- PutawayTask
- PickRequest
- PickTask
- PackSession
- ShipmentStage
- TransferOrder
- TransferLine
- CycleCountPlan
- CycleCountTask
- InventoryAdjustment
- InventoryHold
- QuarantineRecord
- KitDefinition
- KitInstance
- BuildDefinition
- BuildExecution
- ConversionExecution
- ProductionReceipt
- RecoveredInventoryRecord
- UnexplainedInventoryRecord
- TruckStockLocationReference
- DockAppointment
- WarehouseIncident
- ComplianceEvaluationSnapshot
- EvidenceRecord

Common reference fields:

- tenantId
- staffarrSiteOrgUnitId
- staffarrSiteNameSnapshot
- supplyarrItemId
- supplyarrItemSnapshot
- personId
- relatedProductKey
- relatedObjectType
- relatedObjectId
- complianceCoreEvaluationId
- createdAt
- updatedAt

---

# 32. Permission Keys

Suggested permissions:

- loadarr.dashboard.view

- loadarr.locations.view
- loadarr.locations.manage

- loadarr.inventory.view
- loadarr.inventory.transfer
- loadarr.inventory.adjust
- loadarr.inventory.hold
- loadarr.inventory.release_hold
- loadarr.inventory.view_cost

- loadarr.receiving.view
- loadarr.receiving.perform
- loadarr.receiving.override

- loadarr.putaway.view
- loadarr.putaway.perform
- loadarr.putaway.override

- loadarr.picking.view
- loadarr.picking.perform
- loadarr.picking.override

- loadarr.packing.view
- loadarr.packing.perform

- loadarr.shipping.view
- loadarr.shipping.perform
- loadarr.shipping.handoff

- loadarr.transfers.view
- loadarr.transfers.create
- loadarr.transfers.perform
- loadarr.transfers.approve

- loadarr.counts.view
- loadarr.counts.perform
- loadarr.counts.approve_variance

- loadarr.holds.view
- loadarr.holds.create
- loadarr.holds.release

- loadarr.kits.view
- loadarr.kits.build
- loadarr.kits.break
- loadarr.kits.manage

- loadarr.truckstock.view
- loadarr.truckstock.manage
- loadarr.truckstock.issue
- loadarr.truckstock.count

- loadarr.production.view
- loadarr.production.receive
- loadarr.production.approve

- loadarr.builds.view
- loadarr.builds.create
- loadarr.builds.execute
- loadarr.builds.approve

- loadarr.unexplained.view
- loadarr.unexplained.resolve
- loadarr.unexplained.approve

- loadarr.compliance.view
- loadarr.compliance.override

- loadarr.reports.view
- loadarr.admin.manage

---

# 33. Suggested Roles

Suggested default roles:

- Warehouse Associate
- Receiver
- Picker
- Packer/Shipper
- Inventory Control
- Warehouse Lead
- Warehouse Manager
- Maintenance Parts Clerk
- Dock Coordinator
- Truck Stock User
- Production Receiver
- Build/Conversion Operator
- Compliance Reviewer
- LoadArr Admin

---

# 34. V1 Must-Have Features

V1 should include:

- LoadArr product shell
- NexArr authentication/handoff integration
- StaffArr site selector integration
- SupplyArr item lookup integration
- Warehouse location management
- Inventory balances
- Inventory movements
- Inventory origin events
- Manual receiving
- PO-linked receiving scaffold
- Putaway tasks
- Basic transfers
- Work-order parts demand scaffold from MaintainArr
- Pick requests
- Basic picking
- Basic staging
- Cycle counts
- Inventory adjustments with reason codes
- Holds and quarantine
- Unexplained inventory queue
- Builds & conversions foundation
- Production receipt foundation
- Audit trail
- Permission gates
- Compliance Core validation hooks
- Mobile/scanner-friendly UI patterns

---

# 35. V1.5 Features

V1.5 should include:

- Packing
- Shipping handoff
- Dock staging
- Route load staging
- Truck stock
- Kitting
- Returns
- Shortage workflow
- Substitutions
- SDS/hazmat enforcement
- Photo evidence
- Signature evidence
- Mobile-first scanner UI
- Compliance hold/release workflows
- Recovered inventory
- Work-order return flow
- Route return flow

---

# 36. V2 Features

V2 should include:

- Wave picking
- Batch picking
- Zone picking
- Cross-docking
- Advanced slotting
- Yard/dock appointments
- Labor productivity
- RFID support
- Voice-directed picking
- Automated replenishment
- Advanced lot genealogy
- Advanced compliance audit packages
- Forecasting/reorder intelligence
- Production yield analytics
- Manufacturing system integration if a dedicated manufacturing product exists
