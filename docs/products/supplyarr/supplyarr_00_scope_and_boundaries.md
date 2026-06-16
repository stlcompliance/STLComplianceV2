# SupplyArr — Scope, Ownership, and Boundaries

## Product purpose

SupplyArr is the supplier, vendor, dealer, sourcing, purchase request, purchase order, and procurement workflow product for the STL Compliance / ARR suite.

SupplyArr is not the WMS. LoadArr receives, stores, moves, counts, reserves, picks, and issues inventory. SupplyArr decides and tracks how items/services are sourced and purchased.

SupplyArr answers:

- Who is this supplier/vendor/dealer?
- Is this supplier approved?
- What compliance documents are required?
- What items/services can this supplier provide?
- What tenant commercial item, part, material, or SKU context exists?
- What vendor part number, manufacturer part number, price snapshot, lead time, and MOQ apply?
- What purchase request exists?
- Who approved or rejected the request?
- What purchase order exists?
- What is expected to be received?
- What supplier quality/performance status should procurement consider?
- What external financial/accounting reference exists?

## SupplyArr owns

```text
- Supplier master
- Vendor master
- Dealer master
- Supplier contacts
- Supplier addresses
- Supplier locations for operational pickup, delivery, return, and service use
- Supplier status
- Supplier eligibility/approval status
- Supplier compliance requirement tracking
- Supplier document requirement references
- Supplier onboarding workflow
- Supplier approval/restriction/suspension workflow
- Procurement item sourcing records
- Tenant commercial item, part, material, and SKU master context
- Supplier item catalog
- Vendor part numbers
- Manufacturer part numbers
- Price snapshots
- Lead time snapshots
- MOQ/package quantity snapshots
- Approved substitutes
- Purchase requests
- Purchase request approval workflow
- Purchase orders
- Purchase order lifecycle
- Purchase order line state
- PO expected receipt publication to LoadArr
- Supplier performance snapshots
- Procurement-origin events
```

## SupplyArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Permission assignment truth
- Canonical internal location identity
- Training/certification truth
- Regulatory/rulepack meaning
- Document/file storage truth
- Asset truth
- Work order truth
- Inventory balance
- Stock ledger
- Warehouse receiving execution
- Putaway
- Pick/issue
- Route/trip execution
- Customer master
- Customer order lifecycle
- Shared public identifiers, public taxonomies, UOM catalogs, UPC/GTIN normalization, manufacturer identity, and public crosswalks owned by ReferenceDataCore
- Quality hold/release decision
- Analytics read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens

StaffArr
- Buyer/approver/requester person references
- Site/location references
- Permission checks
- Personnel incidents when procurement issue involves people/process behavior

TrainArr
- Buyer/approver qualification if special procurement workflow requires trained/qualified personnel

Compliance Core
- Supplier compliance requirements
- Procurement document requirements
- Regulated item sourcing rules
- Evidence requirements
- Retention rules

RecordArr
- Supplier documents
- Contracts
- Insurance certificates
- PO PDFs
- Quotes
- Supplier corrective action responses
- Procurement evidence packages

LoadArr
- Inventory shortages
- Replenishment signals
- Expected receipts
- Receipt status
- Receiving discrepancies
- Supplier receipt performance facts

MaintainArr
- Work order parts demand
- Maintenance part/service purchase requests
- Vendor maintenance support references

RoutArr
- Supplier pickup/delivery transportation context
- Inbound ETA/appointment context if RoutArr controls the move

CustomArr
- Customer-specific procurement requirements where applicable
- Customer-owned inventory/customer requirement context if needed

OrdArr
- Order-driven procurement demand
- Fulfillment blockers related to procurement

AssurArr
- Supplier quality issues
- Supplier holds/restrictions
- SCARs
- Nonconformance
- Supplier quality status

ReportArr
- Procurement dashboards
- Supplier performance KPIs
- PO cycle time
- Emergency purchase metrics

Field Companion
- Supplier document upload
- Mobile receiving evidence where delegated
- Photo/document capture for procurement evidence

ReferenceDataCore
- Shared public product identifiers and crosswalks
- Public item/product taxonomies
- Units of measure and package normalization
- Manufacturer and brand identity
```

## Core source-of-truth rules

```text
1. SupplyArr owns supplier/vendor/dealer master.
2. SupplyArr owns purchase request and purchase order lifecycle.
3. SupplyArr owns tenant commercial item/part/material/SKU context, sourcing records, and supplier-item relationships.
4. LoadArr owns receiving execution and inventory truth.
5. StaffArr owns internal location identity.
6. RecordArr owns supplier/procurement documents and files.
7. AssurArr owns supplier quality nonconformance, holds, SCARs, and release decisions.
8. Compliance Core owns compliance meaning and evidence requirements.
9. MaintainArr owns maintenance demand; SupplyArr owns procurement response.
10. OrdArr owns order demand; SupplyArr owns procurement response.
11. External accounting owns bills, invoices, payments, tax, general ledger, and reconciliation.
12. SupplyArr may store external accounting IDs/status snapshots only.
13. ReferenceDataCore owns shared public identifiers, public taxonomies, UOM normalization, manufacturer identity, and crosswalks; SupplyArr links to those records but does not replace them.
```

## Standard SupplyArr object envelope

```text
SupplyArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- sourceProduct
- sourceObjectRef
- supplierRef
- requesterPersonId
- ownerPersonId
- staffarrSiteId
- staffarrLocationId
- recordRefs
- complianceRefs
- externalFinancialRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- closedAt
- auditTrail
- eventLog
```

## SupplyArr object prefixes

```text
SUP    Supplier
VEN    Vendor
DLR    Dealer
SCON   Supplier contact
SADR   Supplier address
SLOC   Supplier location
SREQ   Supplier compliance requirement
ITEM   Tenant commercial item/part/material/SKU
SRC    Sourcing record
SITEM  Supplier item
SUB    Substitute item relationship
PR     Purchase request
PRL    Purchase request line
PO     Purchase order
POL    Purchase order line
QTE    Quote
APP    Procurement approval
PERF   Supplier performance record
EXT    External financial reference
```

## Standard supplier reference

```text
SupplierRef
- supplierId
- supplierNumberSnapshot
- supplierNameSnapshot
- supplierTypeSnapshot
- statusSnapshot
- complianceStatusSnapshot
- qualityStatusSnapshot
- lastResolvedAt
```

## Standard sourcing reference

```text
SourcingRef
- sourcingRecordId
- supplierId
- itemRef
- supplierItemNumberSnapshot
- manufacturerPartNumberSnapshot
- vendorPartNumberSnapshot
- preferredSnapshot
- leadTimeDaysSnapshot
- priceSnapshot
- lastResolvedAt
```
