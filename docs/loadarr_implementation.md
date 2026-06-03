# LoadArr First Implementation Plan

This plan implements LoadArr as the dedicated WMS product for the STL Compliance / Arr ecosystem.

The goal is not to build every WMS feature immediately.

The goal is to establish the correct ownership, data model, API boundaries, integration contracts, and first useful workflows without creating product overlap that will become painful later.

---

# Guiding Decisions

LoadArr is the WMS.

SupplyArr remains the item, procurement, vendor, customer, sourcing, and purchasing system.

StaffArr remains the source of canonical sites, people, teams, org structure, and permission assignment surfaces where applicable.

Compliance Core remains the compliance, rule, evidence, exception, exemption, and regulatory interpretation authority.

MaintainArr and RoutArr request warehouse fulfillment. They do not directly mutate LoadArr inventory.

Every inventory increase must have an InventoryOriginEvent.

Every meaningful inventory action must have an auditable InventoryMovement.

Inventory that appears without a trusted source must enter Unexplained Inventory.

The platform is preproduction, so prefer clean schema ownership and hard cutovers over backward-compatible duplication.

---

# Target Technical Shape

Product key:

```
loadarr
```

API project:

```
loadarr-api
```

Frontend route:

```
/app/loadarr
```

Suggested API port:

```
5108
```

Suggested frontend port:

```
5182
```

Database key:

```
loadarr
```

Primary database:

```
loadarr-db
```

Expected stack:

```
.NET 10 API
PostgreSQL
Single suite frontend integration
NexArr launch/handoff
Service-token integrations with StaffArr, SupplyArr, MaintainArr, RoutArr, TrainArr, and Compliance Core
```

---

# Milestone 1: Product Shell and Integration Baseline

## Goal

Create LoadArr as a real first-class product in the suite.

## Backend Work

Create the LoadArr API project.

Add standard product concerns:

* Health endpoint
* Tenant context
* Auth middleware
* NexArr token validation
* Service-token client support
* Standard error response shape
* Standard pagination/filtering conventions
* OpenAPI/Swagger setup
* Database context
* Migration baseline
* Seed baseline reference data

Add product identity:

* Product key: loadarr
* Display name: LoadArr
* Description: Warehouse execution system
* Default route: /app/loadarr
* API base path: /api/v1

Create integration client scaffolds:

* StaffArr client
* SupplyArr client
* MaintainArr client
* RoutArr client
* TrainArr client
* Compliance Core client

## Frontend Work

Create the LoadArr app surface under the suite frontend.

Add navigation:

* Dashboard
* Tasks
* Receiving
* Inventory
* Locations
* Transfers
* Counts
* Holds & Quarantine
* Builds & Conversions
* Unexplained Inventory
* Reports
* Settings

Add empty, loading, and error states.

Add product launch tile if the suite dashboard supports product tiles.

## Acceptance Criteria

* LoadArr API runs independently.
* LoadArr database exists.
* LoadArr appears as a product in the suite.
* Authenticated entitled users can open /app/loadarr.
* Unauthorized users are blocked.
* StaffArr, SupplyArr, MaintainArr, RoutArr, TrainArr, and Compliance Core clients exist even if some methods are initially stubbed.
* No WMS ownership is added to NexArr, StaffArr, MaintainArr, RoutArr, or Compliance Core.

---

# Milestone 2: StaffArr Site Integration

## Goal

Make StaffArr site OrgUnits the only internal site source.

## Backend Work

Add LoadArr StaffArr site reference fields wherever LoadArr references an internal site:

* staffarrSiteOrgUnitId
* staffarrSiteNameSnapshot

Add API endpoints:

```
GET /api/v1/sites
GET /api/v1/sites/{staffarrSiteOrgUnitId}
```

These should proxy or mirror StaffArr integration sites.

LoadArr should call StaffArr:

```
GET /api/v1/integrations/sites
GET /api/v1/integrations/sites/{orgUnitId}
```

Required service-token scope:

```
staffarr.sites.read
```

Add validation helper:

```
ValidateStaffArrSiteAsync(staffarrSiteOrgUnitId)
```

## Frontend Work

Add StaffArr site selector component.

Use it in:

* Dashboard filters
* StaffArr location selector and utilization detail
* Inventory filters
* Receiving workflow
* Transfer workflow
* Count workflow
* Builds & Conversions
* Production Receipts
* Unexplained Inventory

## Acceptance Criteria

* LoadArr does not create canonical sites.
* All warehouse locations require a valid StaffArr site.
* Site names are displayed from StaffArr snapshot or live lookup.
* No free-text site fields exist for internal sites.

---

# Milestone 3: StaffArr Location Utilization Model

## Goal

Consume StaffArr-owned operational locations for warehouse execution.

## Backend Models

Create a LoadArr utilization snapshot model for StaffArr-owned locations used in inventory execution.

Fields:

* Id
* TenantId
* StaffarrSiteOrgUnitId
* StaffarrSiteNameSnapshot
* ParentLocationId
* Code
* Name
* LocationType
* Description
* BarcodeValue
* IsActive
* IsPickable
* IsReceivable
* IsStaging
* IsQuarantine
* IsScrap
* IsTruckStock
* IsProductionOutput
* CapacityQuantity
* CapacityUnitOfMeasure
* AllowedItemClassKeys
* BlockedItemClassKeys
* StorageRequirementKeys
* RequiresQualificationKey
* CreatedAt
* UpdatedAt

Location types:

* warehouse
* zone
* aisle
* bay
* shelf
* bin
* dock
* staging_lane
* yard
* parts_room
* service_truck
* tool_crib
* hazmat_cage
* flammable_cabinet
* quarantine_area
* inspection_area
* scrap_area
* return_area
* production_output
* finished_goods
* raw_material
* cold_storage
* secure_cage

## Backend API

* GET /api/v1/locations
* GET /api/v1/locations/{id}
* GET /api/v1/locations/tree
* GET /api/v1/locations/{id}/utilization

Do not expose LoadArr location create/edit/deactivate endpoints. Canonical location lifecycle belongs to StaffArr.

## Frontend Work

Build pages:

* Locations list
* Location detail
* StaffArr location selector
* StaffArr location utilization detail
* Location tree view

## Acceptance Criteria

* Location references are tenant-scoped.
* Location references point to StaffArr-owned locations tied to StaffArr sites.
* Location hierarchy is consumed from StaffArr.
* LoadArr does not create canonical or operational locations.
* Location detail shows StaffArr location configuration plus LoadArr inventory utilization.

---

# Milestone 4: SupplyArr Item Lookup Integration

## Goal

LoadArr references SupplyArr items instead of owning item master.

## Backend Work

Create SupplyArr item reference client.

Expected methods:

* SearchItemsAsync(query, tenantId)
* GetItemAsync(supplyarrItemId, tenantId)
* GetItemSnapshotAsync(supplyarrItemId, tenantId)

Create local snapshot type SupplyArrItemSnapshot.

Fields:

* SupplyarrItemId
* ItemNumber
* Name
* Description
* PrimaryUnitOfMeasure
* ItemType
* IsLotControlled
* IsSerialControlled
* IsHazardous
* RequiresSds
* DefaultStorageRequirementKeys
* UpdatedAt

Do not create LoadArr-owned item master.

## Frontend Work

Create SupplyArr item selector component.

Use it in:

* Receiving
* Inventory search
* Transfers
* Counts
* Builds & Conversions
* Production Receipts
* Unexplained Inventory resolution

## Acceptance Criteria

* LoadArr item fields are lookup/reference fields.
* LoadArr does not create item master records.
* Historical transactions store item snapshots.
* If SupplyArr is unavailable, LoadArr fails gracefully.

---

# Milestone 5: Inventory Core

## Goal

Implement inventory balances, movements, and origin events.

## Backend Models

Create InventoryBalance.

Fields:

* Id
* TenantId
* StaffarrSiteOrgUnitId
* WarehouseLocationId
* SupplyarrItemId
* ItemSnapshotJson
* LotCode
* SerialCode
* LicensePlateId
* Condition
* Status
* QuantityOnHand
* QuantityAvailable
* QuantityReserved
* QuantityAllocated
* QuantityPicked
* QuantityStaged
* QuantityQuarantined
* QuantityDamaged
* QuantityExpired
* QuantityPendingInspection
* UnitOfMeasure
* CreatedAt
* UpdatedAt

Create InventoryMovement.

Fields:

* Id
* TenantId
* MovementType
* StaffarrSiteOrgUnitId
* FromLocationId
* ToLocationId
* SupplyarrItemId
* ItemSnapshotJson
* Quantity
* UnitOfMeasure
* LotCode
* SerialCode
* ConditionBefore
* ConditionAfter
* StatusBefore
* StatusAfter
* RelatedProductKey
* RelatedObjectType
* RelatedObjectId
* ReasonCode
* PersonId
* ComplianceEvaluationId
* EvidenceJson
* CreatedAt

Create InventoryOriginEvent.

Fields:

* Id
* TenantId
* OriginType
* OriginProductKey
* OriginObjectType
* OriginObjectId
* StaffarrSiteOrgUnitId
* WarehouseLocationId
* SupplyarrItemId
* ItemSnapshotJson
* Quantity
* UnitOfMeasure
* LotCode
* SerialCode
* Condition
* Status
* ReasonCode
* PersonId
* ApprovedByPersonId
* ComplianceEvaluationId
* EvidenceJson
* CreatedAt

Origin types:

* purchase_receipt
* transfer_receipt
* production_receipt
* build_output
* assembly_output
* disassembly_recovery
* conversion_output
* repack_output
* relabel_output
* repair_output
* refurb_output
* rework_output
* customer_supplied_material
* vendor_consignment_receipt
* return_to_stock
* route_return
* work_order_return
* cycle_count_gain
* manual_adjustment
* migration_opening_balance
* scrap_reversal

Movement types:

* receive
* putaway
* transfer
* reserve
* allocate
* pick
* pack
* stage
* load
* consume
* return
* adjust
* hold
* release_hold
* quarantine
* release_quarantine
* scrap
* build_consume
* build_output
* conversion_consume
* conversion_output
* production_receipt
* count_gain
* count_loss

## Backend Services

Create InventoryLedgerService.

Responsibilities:

* Validate tenant
* Validate site
* Validate location
* Validate item
* Create origin events for stock increases
* Create movements for stock changes
* Update balances transactionally
* Prevent negative stock unless explicitly allowed by admin setting
* Preserve audit history
* Reject stock increases without origin event

## API

* GET /api/v1/inventory
* GET /api/v1/inventory/balances
* GET /api/v1/inventory/movements
* GET /api/v1/inventory/origins
* GET /api/v1/inventory/items/{supplyarrItemId}
* GET /api/v1/inventory/locations/{locationId}

## Frontend Work

Build:

* Inventory list
* Inventory detail
* Movement history tab
* Origin history tab
* Location inventory tab

## Acceptance Criteria

* Inventory can be viewed by site, location, item, and status.
* Stock increases require origin event.
* Movements are append-only audit records.
* Balances update transactionally.
* Inventory detail shows sources/origins.

---

# Milestone 6: Manual Receiving

## Goal

Implement first stock creation workflow through receiving.

## Backend Models

Create ReceivingSession.

Fields:

* Id
* TenantId
* ReceivingNumber
* ReceivingType
* Status
* StaffarrSiteOrgUnitId
* SourceProductKey
* SourceObjectType
* SourceObjectId
* SupplierNameSnapshot
* StartedByPersonId
* CompletedByPersonId
* StartedAt
* CompletedAt
* CreatedAt
* UpdatedAt

Create ReceivingLine.

Fields:

* Id
* TenantId
* ReceivingSessionId
* SupplyarrItemId
* ItemSnapshotJson
* ExpectedQuantity
* ReceivedQuantity
* UnitOfMeasure
* WarehouseLocationId
* LotCode
* SerialCode
* Condition
* Status
* DiscrepancyReasonCode
* EvidenceJson
* CreatedAt
* UpdatedAt

Receiving types:

* manual
* purchase_order
* transfer
* return
* customer_supplied
* vendor_consignment
* production_output

## API

* GET /api/v1/receiving
* GET /api/v1/receiving/{id}
* POST /api/v1/receiving
* POST /api/v1/receiving/{id}/lines
* POST /api/v1/receiving/{id}/complete
* POST /api/v1/receiving/{id}/cancel

On complete:

* Validate site.
* Validate location.
* Validate item.
* Run Compliance Core hook if configured.
* Create InventoryOriginEvent.
* Create InventoryMovement.
* Update InventoryBalance.
* Create putaway task if destination is temporary receiving location.

## Frontend Work

Create guided receiving workflow:

1. Select site.
2. Select receiving type.
3. Select source/reference if available.
4. Select item.
5. Enter or scan quantity.
6. Enter lot/serial if required.
7. Select receiving location.
8. Select condition.
9. Add evidence if needed.
10. Complete receiving.

## Acceptance Criteria

* User can receive inventory manually.
* Received inventory creates origin event.
* Received inventory creates movement.
* Received inventory updates balance.
* Receiving detail shows audit trail.
* Item selector comes from SupplyArr.
* Site selector comes from StaffArr.

---

# Milestone 7: Basic Transfers

## Goal

Allow controlled movement of stock between StaffArr-owned locations used by LoadArr.

## Backend Models

Create TransferOrder.

Fields:

* Id
* TenantId
* TransferNumber
* Status
* TransferType
* StaffarrSiteOrgUnitId
* FromLocationId
* ToLocationId
* RequestedByPersonId
* CompletedByPersonId
* ReasonCode
* CreatedAt
* CompletedAt
* UpdatedAt

Create TransferLine.

Fields:

* Id
* TenantId
* TransferOrderId
* SupplyarrItemId
* ItemSnapshotJson
* Quantity
* UnitOfMeasure
* LotCode
* SerialCode
* Status
* CreatedAt
* UpdatedAt

Transfer types:

* bin_to_bin
* zone_to_zone
* site_to_site
* warehouse_to_truck
* truck_to_warehouse
* quarantine_transfer
* return_to_stock
* scrap_transfer

## API

* GET /api/v1/transfers
* GET /api/v1/transfers/{id}
* POST /api/v1/transfers
* POST /api/v1/transfers/{id}/complete
* POST /api/v1/transfers/{id}/cancel

## Frontend Work

Create transfer form:

* Site
* From location
* To location
* Item
* Quantity
* Reason code
* Confirm action

## Acceptance Criteria

* Stock can move between locations.
* Movement record is created.
* Balance is updated.
* Invalid/negative moves are blocked.
* Transfers require reason codes.

---

# Milestone 8: Holds and Quarantine

## Goal

Support blocking inventory from normal use.

## Backend Models

Create InventoryHold.

Fields:

* Id
* TenantId
* HoldNumber
* Status
* HoldType
* StaffarrSiteOrgUnitId
* WarehouseLocationId
* SupplyarrItemId
* InventoryBalanceId
* Quantity
* UnitOfMeasure
* ReasonCode
* Description
* CreatedByPersonId
* ReleasedByPersonId
* ComplianceEvaluationId
* EvidenceJson
* CreatedAt
* ReleasedAt
* UpdatedAt

Hold types:

* compliance
* quality
* damage
* recall
* expired
* missing_sds
* missing_document
* wrong_item
* receiving_discrepancy
* training_qualification
* investigation
* customer
* vendor
* unknown_origin

## API

* GET /api/v1/holds
* GET /api/v1/holds/{id}
* POST /api/v1/holds
* POST /api/v1/holds/{id}/release

## Frontend Work

Create:

* Holds list
* Hold detail
* Create hold workflow
* Release hold workflow

## Acceptance Criteria

* Held quantity is not available.
* Hold/release creates movement records.
* Release requires permission and reason.
* Compliance hold can store Compliance Core evaluation id.

---

# Milestone 9: Unexplained Inventory

## Goal

Create safe workflow for inventory that appears unexpectedly.

## Backend Models

Create UnexplainedInventoryRecord.

Fields:

* Id
* TenantId
* RecordNumber
* Status
* StaffarrSiteOrgUnitId
* WarehouseLocationId
* SupplyarrItemId
* ItemSnapshotJson
* ObservedQuantity
* UnitOfMeasure
* LotCode
* SerialCode
* Condition
* DiscoveredByPersonId
* DiscoveryMethod
* ReasonCode
* ResolutionType
* ResolvedByPersonId
* ApprovedByPersonId
* InventoryOriginEventId
* EvidenceJson
* CreatedAt
* ResolvedAt
* UpdatedAt

Statuses:

* needs_investigation
* needs_identification
* needs_document
* needs_approval
* needs_quarantine
* resolved_valid_stock
* resolved_scrap
* resolved_duplicate
* resolved_receiving_error
* resolved_production_error
* resolved_migration_correction
* canceled

## API

* GET /api/v1/unexplained-inventory
* GET /api/v1/unexplained-inventory/{id}
* POST /api/v1/unexplained-inventory
* POST /api/v1/unexplained-inventory/{id}/resolve
* POST /api/v1/unexplained-inventory/{id}/quarantine
* POST /api/v1/unexplained-inventory/{id}/scrap

## Frontend Work

Create:

* Unexplained Inventory queue
* Create found stock record
* Resolution workflow
* Approval workflow placeholder

## Acceptance Criteria

* Found stock does not become available automatically.
* Resolution as valid stock creates InventoryOriginEvent.
* Resolution creates movement and balance update.
* Location references remain StaffArr-owned and item references remain SupplyArr-owned throughout the workflow.
* Unknown origin can require quarantine/approval.

---

# Milestone 10: Basic Counts and Adjustments

## Goal

Support inventory accuracy workflows.

## Backend Models

Create CycleCountTask.

Fields:

* Id
* TenantId
* CountNumber
* Status
* CountType
* StaffarrSiteOrgUnitId
* WarehouseLocationId
* SupplyarrItemId
* ExpectedQuantity
* CountedQuantity
* VarianceQuantity
* UnitOfMeasure
* CountedByPersonId
* ApprovedByPersonId
* ReasonCode
* InventoryAdjustmentId
* EvidenceJson
* CreatedAt
* CompletedAt
* ApprovedAt
* UpdatedAt

Create InventoryAdjustment.

Fields:

* Id
* TenantId
* AdjustmentNumber
* AdjustmentType
* Status
* StaffarrSiteOrgUnitId
* WarehouseLocationId
* SupplyarrItemId
* QuantityDelta
* UnitOfMeasure
* ReasonCode
* CreatedByPersonId
* ApprovedByPersonId
* InventoryOriginEventId
* EvidenceJson
* CreatedAt
* ApprovedAt
* UpdatedAt

Adjustment types:

* gain
* loss
* status_correction
* condition_correction
* unit_of_measure_correction
* migration_correction

## API

* GET /api/v1/counts
* POST /api/v1/counts
* POST /api/v1/counts/{id}/complete
* POST /api/v1/counts/{id}/approve-variance
* GET /api/v1/adjustments
* POST /api/v1/adjustments
* POST /api/v1/adjustments/{id}/approve

## Frontend Work

Create:

* Count list
* Count detail
* Perform count screen
* Variance approval screen
* Adjustment list/detail

## Acceptance Criteria

* Count variance can create adjustment.
* Positive adjustment creates origin event.
* Negative adjustment creates movement.
* Reason code is required.
* Approval threshold hook exists.

---

# Milestone 11: Builds & Conversions Foundation

## Goal

Support manufacturing-like inventory changes without building full MES.

## Backend Models

Create BuildExecution.

Fields:

* Id
* TenantId
* BuildNumber
* BuildType
* Status
* StaffarrSiteOrgUnitId
* OutputLocationId
* OutputSupplyarrItemId
* OutputItemSnapshotJson
* OutputQuantity
* OutputUnitOfMeasure
* LotCode
* SerialCode
* StartedByPersonId
* CompletedByPersonId
* ApprovedByPersonId
* ComplianceEvaluationId
* EvidenceJson
* CreatedAt
* CompletedAt
* UpdatedAt

Create BuildInputLine.

Fields:

* Id
* TenantId
* BuildExecutionId
* SupplyarrItemId
* ItemSnapshotJson
* SourceLocationId
* Quantity
* UnitOfMeasure
* LotCode
* SerialCode
* CreatedAt
* UpdatedAt

Build types:

* build
* assemble
* disassemble
* convert
* repack
* relabel
* repair
* refurbish
* recover
* blend_mix
* cut_split
* combine
* rework

## API

* GET /api/v1/builds
* GET /api/v1/builds/{id}
* POST /api/v1/builds
* POST /api/v1/builds/{id}/complete
* POST /api/v1/builds/{id}/cancel

On completion:

* Consume input inventory.
* Create input movement records.
* Create output origin event.
* Create output movement record.
* Update output balance.
* Preserve lot genealogy where provided.
* Run Compliance Core hook if configured.

## Frontend Work

Create guided Builds & Conversions workflow:

1. Select site.
2. Select build type.
3. Select output item.
4. Select output quantity.
5. Select output location.
6. Add input lines.
7. Confirm material consumption.
8. Add lot/serial/evidence.
9. Complete build.

## Acceptance Criteria

* User can consume inventory and produce different inventory.
* Output inventory has origin event.
* Inputs and outputs are auditable.
* No stock appears without build/conversion origin.

---

# Milestone 12: Production Receipts Foundation

## Goal

Allow manufacturers to record finished goods output.

## Backend Models

Create ProductionReceipt.

Fields:

* Id
* TenantId
* ProductionReceiptNumber
* Status
* StaffarrSiteOrgUnitId
* ProductionReference
* OutputLocationId
* SupplyarrItemId
* ItemSnapshotJson
* ProducedQuantity
* RejectedQuantity
* ScrapQuantity
* UnitOfMeasure
* LotCode
* SerialCode
* ProducedByPersonId
* ApprovedByPersonId
* ComplianceEvaluationId
* EvidenceJson
* CreatedAt
* ApprovedAt
* UpdatedAt

Statuses:

* draft
* pending_inspection
* available
* quarantined
* rejected
* scrapped
* canceled

## API

* GET /api/v1/production-receipts
* GET /api/v1/production-receipts/{id}
* POST /api/v1/production-receipts
* POST /api/v1/production-receipts/{id}/approve
* POST /api/v1/production-receipts/{id}/quarantine
* POST /api/v1/production-receipts/{id}/cancel

## Frontend Work

Create Production Receipt screen:

* Site
* Production reference
* Output item
* Quantity
* Lot/serial
* Output location
* Condition/status
* Evidence
* Approval

## Acceptance Criteria

* Production output creates inventory origin event.
* Output can enter pending inspection/quarantine.
* Available stock only occurs after allowed status transition.
* Production reference is preserved.

---

# Milestone 13: Warehouse Task Engine Foundation

## Goal

Create the task system that future receiving, putaway, picking, counts, and holds will use.

## Backend Models

Create WarehouseTask.

Fields:

* Id
* TenantId
* TaskNumber
* TaskType
* Status
* Priority
* StaffarrSiteOrgUnitId
* AssignedPersonId
* AssignedTeamId
* RequiredPermissionKey
* RequiredQualificationKey
* SourceLocationId
* DestinationLocationId
* SupplyarrItemId
* ItemSnapshotJson
* Quantity
* UnitOfMeasure
* RelatedProductKey
* RelatedObjectType
* RelatedObjectId
* ComplianceEvaluationId
* EvidenceJson
* CreatedAt
* StartedAt
* CompletedAt
* UpdatedAt

Task types:

* receive
* inspect
* putaway
* pick
* pack
* stage
* load
* count
* transfer
* replenish
* quarantine
* release_hold
* scrap
* return
* build
* convert
* investigate_variance
* resolve_shortage
* resolve_unexplained_inventory

## API

* GET /api/v1/tasks
* GET /api/v1/tasks/{id}
* POST /api/v1/tasks
* POST /api/v1/tasks/{id}/assign
* POST /api/v1/tasks/{id}/start
* POST /api/v1/tasks/{id}/complete
* POST /api/v1/tasks/{id}/cancel

## Frontend Work

Create:

* Task dashboard
* My tasks
* Team tasks
* Task detail
* Basic task action buttons

## Acceptance Criteria

* Tasks can be created and assigned.
* Tasks can reference locations, items, and related product objects.
* Task state transitions are audited.
* Permissions/qualification fields exist even if enforcement is initially partial.

---

# Milestone 14: MaintainArr Demand Scaffold

## Goal

Allow MaintainArr to request parts from LoadArr without mutating inventory directly.

## Backend Models

Create PickRequest.

Fields:

* Id
* TenantId
* PickRequestNumber
* Status
* RequestSourceProductKey
* RequestSourceObjectType
* RequestSourceObjectId
* StaffarrSiteOrgUnitId
* RequestedByPersonId
* DestinationLocationId
* NeededBy
* CreatedAt
* UpdatedAt

Create PickRequestLine.

Fields:

* Id
* TenantId
* PickRequestId
* SupplyarrItemId
* ItemSnapshotJson
* RequestedQuantity
* ReservedQuantity
* PickedQuantity
* UnitOfMeasure
* Status
* CreatedAt
* UpdatedAt

Statuses:

* requested
* reserved
* allocated
* pick_released
* picked
* staged
* fulfilled
* shorted
* backordered
* substituted
* canceled
* blocked

## API

* POST /api/v1/integrations/maintainarr/pick-requests
* GET /api/v1/integrations/maintainarr/pick-requests/{id}
* POST /api/v1/pick-requests/{id}/release
* POST /api/v1/pick-requests/{id}/stage
* POST /api/v1/pick-requests/{id}/complete

## Frontend Work

Create Pick Requests page.

Show:

* Source product
* Source object
* Item
* Quantity
* Status
* Shortage state
* Staging location
* Related work order link placeholder

## Acceptance Criteria

* MaintainArr can create demand by API.
* LoadArr tracks pick request.
* LoadArr does not require MaintainArr to know bins.
* MaintainArr receives status updates or can poll status.

---

# Milestone 15: Compliance Core Hook Layer

## Goal

Prepare LoadArr for compliance-aware behavior without blocking development on full rulepack depth.

## Backend Model

Create ComplianceEvaluationSnapshot.

Fields:

* Id
* TenantId
* EvaluationSource
* RelatedEntityType
* RelatedEntityId
* ComplianceCoreEvaluationId
* Decision
* Summary
* RequiredEvidenceJson
* WarningsJson
* BlockingReasonsJson
* EvaluatedAt

Decision values:

* allowed
* allowed_with_warning
* requires_evidence
* requires_approval
* blocked
* not_applicable
* unknown

## Backend Service

Create LoadArrComplianceService.

Methods:

* EvaluateReceivingAsync(...)
* EvaluatePutawayAsync(...)
* EvaluateTransferAsync(...)
* EvaluateHoldReleaseAsync(...)
* EvaluateBuildConversionAsync(...)
* EvaluateProductionReceiptAsync(...)
* EvaluateInventoryAdjustmentAsync(...)
* EvaluateTaskAssignmentAsync(...)

Initial implementation may return not_applicable when Compliance Core is unavailable, but must preserve the interface.

## Acceptance Criteria

* Compliance hook methods exist.
* Critical inventory actions can store evaluation snapshots.
* Future rule integration does not require refactoring core workflows.

---

# Milestone 16: Permissions and Roles

## Goal

Add LoadArr permission definitions and default roles.

## Backend Work

Seed permission keys:

* loadarr.dashboard.view
* loadarr.locations.view
* loadarr.locations.manage
* loadarr.inventory.view
* loadarr.inventory.transfer
* loadarr.inventory.adjust
* loadarr.inventory.hold
* loadarr.inventory.release_hold
* loadarr.receiving.view
* loadarr.receiving.perform
* loadarr.receiving.override
* loadarr.putaway.view
* loadarr.putaway.perform
* loadarr.picking.view
* loadarr.picking.perform
* loadarr.counts.view
* loadarr.counts.perform
* loadarr.counts.approve_variance
* loadarr.holds.view
* loadarr.holds.create
* loadarr.holds.release
* loadarr.builds.view
* loadarr.builds.create
* loadarr.builds.execute
* loadarr.production.view
* loadarr.production.receive
* loadarr.unexplained.view
* loadarr.unexplained.resolve
* loadarr.unexplained.approve
* loadarr.reports.view
* loadarr.admin.manage

Suggested default roles:

* Warehouse Associate
* Receiver
* Inventory Control
* Warehouse Lead
* Warehouse Manager
* Maintenance Parts Clerk
* Production Receiver
* Build/Conversion Operator
* Compliance Reviewer
* LoadArr Admin

## Acceptance Criteria

* API endpoints require permissions.
* UI hides actions user cannot perform.
* Admin role can access LoadArr settings.
* Dangerous actions require elevated permissions.

---

# Milestone 17: Dashboard and Reporting Foundation

## Goal

Give users useful operational visibility immediately.

## Dashboard Cards

Implement counts for:

* Open receiving
* Awaiting putaway
* Inventory locations
* Inventory items
* Open transfers
* Open counts
* Active holds
* Quarantined inventory
* Unexplained inventory
* Open builds
* Production receipts pending inspection
* Pick requests

## Reports

Initial reports:

* Inventory by location
* Inventory by item
* Inventory by status
* Movement history
* Origin history
* Holds report
* Unexplained inventory report
* Count variance report

## Acceptance Criteria

* Dashboard loads meaningful data.
* Reports use filters by site, item, location, status, and date.
* No raw JSON is shown in reports.

---

# Milestone 18: Frontend UX Polish

## Goal

Make LoadArr usable by normal warehouse users.

## UX Requirements

* Minimal free typing
* Searchable selectors
* Site selector
* Item selector
* Location selector
* Reason-code selectors
* Clear status badges
* Action buttons instead of raw data
* Scanner-friendly layouts
* Mobile-friendly task pages
* Empty states with next actions
* Error messages that explain what to fix
* No raw JSON shown to users

## Key Pages To Polish

* Dashboard
* Inventory list/detail
* Receiving workflow
* StaffArr location utilization detail
* Transfer workflow
* Holds
* Unexplained Inventory
* Builds & Conversions
* Production Receipts
* Tasks

## Acceptance Criteria

* A user can receive, move, hold, count, and resolve inventory without developer knowledge.
* All forms use controlled/selectable fields where possible.
* Workflows are guided and not giant raw admin forms.

---

# First Useful Demo Flow

The first demo should prove the whole concept:

1. User opens LoadArr from NexArr.
2. User selects a StaffArr site.
3. User selects StaffArr-owned warehouse locations for LoadArr utilization:

   * Receiving Dock
   * Parts Room
   * Bin A-01
   * Quarantine Area
   * Production Output
4. User receives a SupplyArr item manually.
5. LoadArr creates:

   * Receiving session
   * Inventory origin event
   * Inventory movement
   * Inventory balance
6. User transfers stock from Receiving Dock to Bin A-01.
7. User creates a hold for damaged quantity.
8. User performs a count and records a variance.
9. Positive variance creates Unexplained Inventory.
10. User resolves unexplained inventory as valid stock with approval.
11. User performs a Build/Conversion:

    * Consumes input item
    * Produces output item
    * Creates origin event
12. Dashboard shows:

    * Inventory on hand
    * Held inventory
    * Unexplained inventory resolved
    * Build output
    * Movement history

This proves LoadArr can handle normal WMS inventory and manufacturing-style inventory that appears through controlled origin events.

---

# Implementation Order Summary

Build in this order:

1. Product shell
2. Auth/handoff
3. StaffArr site integration
4. Warehouse locations
5. SupplyArr item lookup
6. Inventory balances/movements/origins
7. Manual receiving
8. Transfers
9. Holds/quarantine
10. Unexplained inventory
11. Counts/adjustments
12. Builds & conversions
13. Production receipts
14. Warehouse tasks
15. MaintainArr pick request scaffold
16. Compliance Core hook layer
17. Permissions/roles
18. Dashboard/reports
19. UX polish

---

# What Not To Build First

Do not start with:

* Full wave picking
* Full route shipping
* Advanced dock appointments
* RFID
* Voice-directed picking
* Advanced manufacturing scheduling
* Full ERP/MES BOM routing
* Automated replenishment AI
* Complex costing
* External carrier integrations
* Multi-warehouse optimization

Those are later features.

The first implementation should establish trusted inventory movement, origin tracking, StaffArr site alignment, SupplyArr item references, Compliance Core hooks, and real warehouse usability.
