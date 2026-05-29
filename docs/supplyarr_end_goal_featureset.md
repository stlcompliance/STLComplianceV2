# SupplyArr End Goal and Granular Feature Set

## 1. Product Summary

**SupplyArr** is the supply-chain, procurement, external-party, parts, and purchasing product in the STL Compliance / Arr ecosystem.

Its job is to answer one operational question:

> **Can the business reliably source, approve, purchase, receive, document, and trace the goods or external services required to keep operations compliant, supplied, and ready?**

SupplyArr owns the business world around **vendors, suppliers, dealers, customers, external parties, parts, materials, purchasing, approvals, supplier documents, pricing, lead times, inventory availability, receiving, returns, and supply readiness**.

It should feel like the missing bridge between maintenance, operations, purchasing, compliance, and vendor management.

---

## 2. End Goal

The end goal of SupplyArr is to become the system of record for the platform's supply-side relationships and purchasing workflows.

A completed SupplyArr should allow a company to:

1. Maintain a clean master record of vendors, suppliers, dealers, customers, manufacturers, parts sources, and external business contacts.
2. Know which vendors are approved, restricted, expired, blocked, preferred, or pending review.
3. Know which parts, supplies, materials, documents, or services can be purchased, from whom, at what cost, with what lead time, and under what approval rules.
4. Convert demand from MaintainArr, RoutArr, StaffArr, TrainArr, or manual users into structured purchase requests.
5. Route purchase requests through approval workflows based on cost, category, vendor risk, site, department, requester authority, and compliance requirements.
6. Create RFQs, compare quotes, issue purchase orders, receive goods, track exceptions, and close the procurement loop.
7. Track pricing history, vendor reliability, lead-time trends, fill rates, substitutions, warranty claims, returns, and supplier performance.
8. Provide Compliance Core with facts about vendor status, required documents, approved sources, purchase evidence, document expiration, and procurement compliance.
9. Feed MaintainArr with accurate part availability, part reservations, cost records, receiving status, and warranty/source history.
10. Feed RoutArr, StaffArr, TrainArr, and other products with external-party and procurement facts without letting those products own SupplyArr records.

In plain English:

> **SupplyArr makes sure the right thing can be bought from the right source, with the right approval, at the right price, with the right documentation, before the lack of it becomes an operational or compliance failure.**

---

## 3. Product Boundary

### 3.1 SupplyArr Owns

SupplyArr owns:

- Vendors
- Suppliers
- Dealers
- Customers and external business parties
- External party contacts
- Manufacturer records and aliases
- Supplier onboarding
- Vendor approval status
- Supplier compliance documents
- Parts catalogs
- Materials and consumables catalogs
- Service-purchasing catalogs
- Part aliases, equivalents, supersessions, and substitutions
- Vendor part numbers
- Internal part numbers
- Pricing snapshots
- Lead-time snapshots
- Availability snapshots
- Purchase requests
- Purchase approvals
- RFQs
- Quotes
- Purchase orders
- Receiving records
- Backorders
- Returns
- Warranty claims tied to sourced parts or vendors
- Supplier scorecards
- Procurement documents
- Supply readiness reporting
- Supply-side audit history

### 3.2 SupplyArr Does Not Own

SupplyArr must not own:

- Platform login, identity, tenant existence, product entitlement, or launch behavior. That belongs to **NexArr**.
- People, employment status, org structure, sites, departments, positions, teams, or permission assignments. That belongs to **StaffArr**.
- Training programs, training completions, evaluations, retraining, signoffs, or certification issuance. That belongs to **TrainArr**.
- Assets, PMs, inspections, defects, work orders, repairs, or maintenance execution. That belongs to **MaintainArr**.
- Routes, dispatch, trips, stops, driver/vehicle assignments, or transportation exceptions. That belongs to **RoutArr**.
- Normalized legal rule packs, citation intelligence, applicability logic, or final compliance evaluation. That belongs to **Compliance Core**.
- General ledger, payroll, accounting close, taxes, or full ERP financial ownership. SupplyArr can export or integrate with accounting systems, but it should not become the accounting source of truth unless explicitly expanded later.

---

## 4. Platform Role

SupplyArr is a domain product inside the Arr ecosystem.

It should integrate with the rest of the platform through APIs, service tokens, events, and local reference/mirror tables. It should not rely on direct cross-database foreign keys.

Each product keeps its own PostgreSQL database. SupplyArr can locally mirror references such as `personRef`, `siteRef`, `departmentRef`, `assetRef`, `workOrderRef`, `routeRef`, or `certificationStatusSnapshot`, but the source product remains authoritative.

### 4.1 Dependency Direction

SupplyArr depends on:

- **NexArr** for tenant validation, product entitlement, service identity, and platform launch.
- **StaffArr** for people, approval authority, org/site/department references, active status, and permission context.
- **MaintainArr** for maintenance demand, asset references, part demand, work-order consumption, and warranty context.
- **RoutArr** for route/trip/customer demand where transportation operations require purchased goods or services.
- **TrainArr** only when supplier, purchasing, hazardous material, forklift, procurement, or receiving workflows require training/certification facts.
- **Compliance Core** for evaluating whether supply-side workflows satisfy applicable rules and document requirements.

SupplyArr provides facts to:

- MaintainArr: part availability, reservations, purchase status, received parts, part cost, warranty eligibility.
- RoutArr: external-party/customer/vendor references, route supply exceptions, vendor service availability.
- StaffArr: procurement actions taken by people, approval history, incidents or policy violations requiring personnel review.
- TrainArr: training-triggering procurement incidents, supplier handling incidents, receiving mistakes, hazardous material training needs.
- Compliance Core: vendor documents, approval evidence, purchase records, received evidence, supplier status, exception history.

---

## 5. Core Product Pillars

SupplyArr should be organized around nine product pillars:

1. **External Party Master Data** — vendors, suppliers, dealers, customers, contacts, manufacturer records, and external organizations.
2. **Vendor Approval and Compliance** — onboarding, required documents, approval status, restrictions, insurance, W-9s, certificates, and supplier risk.
3. **Parts and Materials Catalog** — internal part numbers, vendor part numbers, equivalents, substitutions, supersessions, categories, units of measure, and compatibility.
4. **Pricing, Lead Time, and Source Intelligence** — cost history, quote history, lead-time snapshots, preferred vendors, and source recommendations.
5. **Inventory and Availability** — stock locations, bins, counts, reorder points, reserved stock, receiving, backorders, transfers, and cycle counts.
6. **Purchasing Workflow** — purchase requests, approvals, RFQs, quotes, purchase orders, receiving, exceptions, returns, and warranty claims.
7. **Documents and Evidence** — purchase documents, supplier documents, compliance records, attachments, signatures, approval evidence, and audit packages.
8. **Reporting and Readiness** — stockout risk, vendor risk, procurement lag, spend visibility, demand planning, and supply readiness scoring.
9. **Platform Integration** — clean APIs, events, local references, service tokens, and compliance fact publishing.

---

## 6. User Types and Roles

SupplyArr should support role-based access using StaffArr permission assignments and NexArr product access.

### 6.1 Common User Personas

- **Requester** — creates purchase requests for parts, materials, supplies, or services.
- **Technician** — requests parts from MaintainArr work orders and sees part availability.
- **Supervisor** — approves requests within limits and monitors demand.
- **Purchasing Agent** — manages RFQs, quotes, POs, vendor communication, and order status.
- **Receiving Clerk** — receives goods, records discrepancies, scans packing slips, and closes receipts.
- **Inventory Clerk** — manages stock, bins, counts, transfers, reservations, and cycle counts.
- **Parts Manager** — owns part records, reorder points, substitutions, preferred vendors, and stocking strategy.
- **Vendor Manager** — manages supplier onboarding, approval, documents, restrictions, and scorecards.
- **Compliance Manager** — reviews document coverage, expired supplier evidence, exception workflows, and compliance packets.
- **Finance / Accounting Viewer** — reviews purchasing records, invoices, exports, spend, and supporting evidence.
- **Platform Admin** — configures high-level product settings through the platform boundary, without bypassing product ownership rules.

### 6.2 Permission Examples

SupplyArr permissions should be granular. StaffArr owns assignment of these permission keys to people and scopes; SupplyArr owns the permission catalog, approval/action rules, and backend enforcement for supply actions. Examples:

- `supplyarr.vendor.view`
- `supplyarr.vendor.create`
- `supplyarr.vendor.edit`
- `supplyarr.vendor.approve`
- `supplyarr.vendor.block`
- `supplyarr.vendor.documents.manage`
- `supplyarr.parts.view`
- `supplyarr.parts.create`
- `supplyarr.parts.edit`
- `supplyarr.parts.merge`
- `supplyarr.parts.substitutions.manage`
- `supplyarr.inventory.view`
- `supplyarr.inventory.adjust`
- `supplyarr.inventory.count`
- `supplyarr.inventory.transfer`
- `supplyarr.purchase_request.create`
- `supplyarr.purchase_request.approve`
- `supplyarr.purchase_request.reject`
- `supplyarr.rfq.manage`
- `supplyarr.quote.compare`
- `supplyarr.purchase_order.create`
- `supplyarr.purchase_order.issue`
- `supplyarr.purchase_order.cancel`
- `supplyarr.receiving.receive`
- `supplyarr.receiving.exception`
- `supplyarr.warranty.manage`
- `supplyarr.reports.view`
- `supplyarr.admin.configure`

---

## 7. Granular Feature Set

## 7.1 External Party Management

### 7.1.1 External Party Records

SupplyArr should support a normalized external-party directory.

External party types:

- Vendor
- Supplier
- Dealer
- Manufacturer
- Customer
- Carrier
- Broker
- Service provider
- Contractor
- Government agency
- Insurance provider
- Warranty provider
- Parts distributor
- Repair vendor
- Fuel vendor
- Tire vendor
- Safety equipment vendor
- Training supplier
- Other external organization

Core fields:

- External party ID
- Tenant ID
- Legal name
- Display name
- DBA name
- Party type
- Status
- Approval status
- Risk status
- Tax identifier reference or masked value
- Website
- Main phone
- Main email
- Primary address
- Billing address
- Shipping address
- Service areas
- Supported sites
- Supported categories
- Tags
- Notes
- Created date
- Created by person reference
- Last updated date
- Last updated by person reference

Statuses:

- Draft
- Pending onboarding
- Pending review
- Active
- Preferred
- Approved
- Restricted
- Probation
- Blocked
- Inactive
- Archived

### 7.1.2 External Party Contacts

Contact records should support:

- Name
- Title
- Role
- Email
- Phone
- Mobile
- Department
- Location
- Preferred contact method
- After-hours contact flag
- Emergency contact flag
- Purchasing contact flag
- Compliance contact flag
- Accounts receivable contact flag
- Accounts payable contact flag
- Warranty contact flag
- Notes

### 7.1.3 External Party Relationships

SupplyArr should model relationships such as:

- Manufacturer to distributor
- Dealer to manufacturer
- Vendor to customer
- Vendor to parent company
- Vendor to branch location
- Supplier to service region
- Customer to billing account
- Supplier to contract
- Vendor to approved sites
- Vendor to approved categories
- Vendor to preferred purchase channels

### 7.1.4 Customer Records

SupplyArr should own external customer records where customers are business parties rather than people or platform tenants.

Customer features:

SupplyArr owns customer master data for external business parties. RoutArr may reference these customers for trips/stops and may preserve stop-level execution snapshots, but RoutArr should not become the customer master data owner.

- Customer profile
- Customer contacts
- Billing details
- Shipping locations
- Customer-specific documents
- Service level notes
- Approved customer status
- Customer-vendor relationship mapping
- Customer-specific supply requirements
- Customer-specific proof/document requirements

RoutArr may reference customer records for trips and stops, but RoutArr should not own the customer master data.

---

## 7.2 Vendor Onboarding and Approval

### 7.2.1 Vendor Onboarding Workflow

SupplyArr should provide a guided onboarding workflow:

1. Create external party.
2. Select party type and categories.
3. Capture legal/business identity.
4. Add contacts.
5. Add addresses and service regions.
6. Attach required documents.
7. Define approved categories.
8. Define supported sites.
9. Configure payment/procurement settings.
10. Run compliance checks.
11. Submit for review.
12. Approve, restrict, reject, or request more information.

### 7.2.2 Vendor Approval Rules

Vendor approval should support:

- Approval by category
- Approval by site
- Approval by department
- Approval by spend limit
- Approval by service type
- Approval by document status
- Approval by insurance status
- Approval by contract status
- Approval by compliance rule status
- Approval by risk rating
- Approval expiration date
- Temporary approval
- Emergency approval
- Conditional approval

### 7.2.3 Vendor Risk Status

Risk states:

- Unknown
- Low
- Medium
- High
- Critical
- Blocked
- Under review
- Expired documentation
- Insurance issue
- Contract issue
- Performance issue
- Compliance issue

### 7.2.4 Vendor Restrictions

Restriction examples:

- Cannot issue new purchase orders
- Cannot receive new RFQs
- Cannot purchase certain categories
- Cannot serve certain sites
- Requires manager approval
- Requires compliance approval
- Requires prepaid terms
- Requires alternate vendor comparison
- Emergency use only

### 7.2.5 Vendor Scorecards

Vendor scorecards should measure:

- On-time delivery
- Average lead time
- Fill rate
- Quote response time
- Price competitiveness
- Backorder frequency
- Defect rate
- Return rate
- Warranty claim rate
- Document compliance
- Approval health
- Emergency order dependency
- Site satisfaction
- Category performance

---

## 7.3 Supplier Compliance Documents

### 7.3.1 Document Types

SupplyArr should track supplier documents such as:

- W-9
- Certificate of insurance
- General liability insurance
- Auto liability insurance
- Workers compensation insurance
- Cargo insurance
- Hazmat documentation
- Safety data sheets
- Supplier agreement
- Master service agreement
- Purchase terms
- Credit application
- Tax exemption certificate
- Business license
- DOT authority document
- FMCSA operating authority document
- Vendor safety policy
- Environmental compliance document
- Warranty terms
- Return policy
- Quality certification
- ISO certification
- Customer-specific supplier packet
- Other custom document type

### 7.3.2 Document Metadata

Each document should support:

- Document type
- External party reference
- File attachment
- Version
- Effective date
- Expiration date
- Issuer
- Policy number or document number
- Coverage amount
- Status
- Required flag
- Verified flag
- Verified by person reference
- Verified date
- Rejection reason
- Notes
- Related compliance rule references

### 7.3.3 Document Statuses

- Missing
- Uploaded
- Pending review
- Accepted
- Rejected
- Expiring soon
- Expired
- Superseded
- Waived
- Not applicable

### 7.3.4 Document Automation

Automation should support:

- Expiration reminders
- Vendor document request emails
- Internal review tasks
- Approval blocking when documents expire
- Compliance Core fact publishing
- Vendor status downgrade on expiration
- Waiver workflow for emergency purchasing

---

## 7.4 Parts, Materials, and Service Catalog

### 7.4.1 Item Master

SupplyArr should own the item master for parts, materials, consumables, supplies, and purchasable services.

Item types:

- Part
- Consumable
- Material
- Tool
- Safety supply
- Office supply
- Shop supply
- Tire
- Fluid
- Lubricant
- Chemical
- DEF
- Fuel-related item
- PPE
- Service
- Rental
- Contracted repair
- Subscription
- Other purchasable item

Core fields:

- Item ID
- Internal item number
- Display name
- Description
- Item type
- Category
- Subcategory
- Unit of measure
- Stocked flag
- Critical flag
- Serialized flag
- Lot-tracked flag
- Expiration-tracked flag
- Hazmat flag
- Temperature-sensitive flag
- Warranty-tracked flag
- Preferred vendor
- Manufacturer
- Manufacturer part number
- Default reorder point
- Default reorder quantity
- Minimum stock
- Maximum stock
- Average cost
- Last cost
- Standard cost
- Tags
- Notes

### 7.4.2 Parts Taxonomy

SupplyArr should support a flexible category tree:

- Vehicle parts
- Trailer parts
- MHE / forklift parts
- Construction equipment parts
- Shop supplies
- Safety supplies
- Fluids and lubricants
- Tires and wheels
- Electrical
- Brakes
- Suspension
- Steering
- Drivetrain
- Engine
- Cooling
- Exhaust / aftertreatment
- Body
- Lighting
- Hydraulics
- Pneumatics
- Fasteners
- Tools
- PPE
- Chemicals
- Office / admin supplies
- Facility supplies
- Services

### 7.4.3 Manufacturer and Brand Normalization

SupplyArr should support manufacturer normalization:

- Canonical manufacturer record
- Alias records
- Brand records
- DBA or acquisition history
- Vendor-specific naming variants
- Normalized search
- Merge workflow
- Duplicate detection
- Confidence scoring

Example normalization needs:

- Different spellings of the same manufacturer
- Vendor-specific abbreviations
- Legacy part catalogs
- Imported part lists with inconsistent names
- Dealer-specific part descriptions

### 7.4.4 Vendor Part Numbers

Each item should support many vendor part numbers.

Vendor part fields:

- Vendor reference
- Vendor item number
- Vendor description
- Manufacturer part number
- Vendor category
- Unit of measure
- Purchase multiple
- Minimum order quantity
- Preferred flag
- Approved flag
- Last quoted cost
- Last purchase cost
- Last lead time
- Last availability status
- Vendor URL
- Notes

### 7.4.5 Part Equivalents and Substitutions

SupplyArr should support:

- Equivalent parts
- Superseded parts
- Replacement parts
- Temporary substitutions
- Emergency substitutions
- Brand preference hierarchy
- Compatibility notes
- Approval requirements for substitutions
- Blocked substitutions
- Substitution reason history

Substitution statuses:

- Proposed
- Approved
- Approved for emergency use
- Approved for specific asset class
- Approved for specific site
- Rejected
- Blocked
- Retired

### 7.4.6 Asset Compatibility References

MaintainArr owns assets. SupplyArr can store local references to asset classes, asset types, or specific asset references for compatibility purposes.

SupplyArr should support:

- Item compatible with asset class
- Item compatible with asset type
- Item compatible with asset make/model
- Item compatible with specific asset reference
- Item incompatible with asset class/type
- Fitment notes
- Install notes
- Maintenance usage notes

SupplyArr must not become the asset system of record.

---

## 7.5 Inventory and Availability

### 7.5.1 Inventory Locations

SupplyArr should support inventory by:

- Tenant
- Site
- Warehouse
- Storeroom
- Parts room
- Bin
- Shelf
- Truck stock
- Mobile stock
- Consignment stock
- Vendor-managed inventory
- Quarantine location
- Return location
- Scrap location

Sites and org structure come from StaffArr as references.

### 7.5.2 Stock Records

Stock records should track:

- Item reference
- Site reference
- Location
- Bin
- Quantity on hand
- Quantity reserved
- Quantity available
- Quantity on order
- Quantity backordered
- Quantity allocated
- Minimum quantity
- Maximum quantity
- Reorder point
- Reorder quantity
- Average cost
- Last received cost
- Lot number
- Serial number
- Expiration date
- Last counted date
- Last adjusted date

### 7.5.3 Inventory Actions

Supported actions:

- Receive stock
- Issue stock
- Reserve stock
- Release reservation
- Transfer stock
- Adjust stock
- Count stock
- Reconcile count
- Quarantine stock
- Scrap stock
- Return stock
- Convert unit of measure
- Split lot
- Merge lot
- Assign serial number
- Move bin

### 7.5.4 Reorder Logic

SupplyArr should support reorder rules:

- Min/max reorder
- Reorder point
- Reorder quantity
- Demand-based reorder
- Lead-time-aware reorder
- Critical item reorder
- Seasonal reorder
- Site-specific reorder
- Vendor minimum order quantity
- Purchase multiple rounding
- Emergency stocking rule
- Auto-create purchase request
- Auto-suggest purchase request

### 7.5.5 Cycle Counts

Cycle count features:

- Count schedules
- ABC counting
- Critical-item counting
- Location-based counting
- Blind counts
- Variance thresholds
- Approval for large adjustments
- Count evidence attachments
- Count history
- Adjustment audit trail

---

## 7.6 Demand Intake

SupplyArr should accept demand from multiple sources.

### 7.6.1 Manual Demand

Users should be able to create requests for:

- Parts
- Shop supplies
- Safety supplies
- Tools
- Services
- Rentals
- Emergency purchases
- Customer-specific materials
- Site supplies
- Warranty replacement
- Stock replenishment

### 7.6.2 MaintainArr Demand

MaintainArr may generate demand from:

- Work order required parts
- Inspection defects
- PM forecasted parts
- Asset repair plans
- Warranty repair events
- Stock issue requests
- Emergency breakdowns

SupplyArr should receive the demand and respond with:

- Available in stock
- Reserved for work order
- Needs purchase
- Substitute available
- Backordered
- Waiting approval
- Ordered
- Received
- Cancelled

### 7.6.3 RoutArr Demand

RoutArr may generate demand from:

- Route supply needs
- Driver equipment needs
- Customer delivery requirements
- Trip exceptions
- Fuel card/vendor issues
- External service purchases
- Emergency roadside vendor use

### 7.6.4 StaffArr Demand

StaffArr may generate demand from:

- Employee equipment needs
- PPE needs
- Onboarding supplies
- Badge or uniform requests
- Department supply requests
- Personnel-related external service requests

### 7.6.5 TrainArr Demand

TrainArr may generate demand from:

- Training materials
- PPE for training
- External training vendor services
- Certification exam purchases
- Course supplies
- Equipment rental for training sessions

---

## 7.7 Purchase Requests

### 7.7.1 Purchase Request Creation

Purchase requests should support:

- Request title
- Request description
- Requester person reference
- Site reference
- Department reference
- Needed-by date
- Priority
- Business reason
- Source product reference
- Related work order reference
- Related asset reference
- Related route reference
- Related training reference
- Related incident reference
- Line items
- Suggested vendor
- Attachments
- Notes

Priorities:

- Low
- Normal
- High
- Urgent
- Emergency
- Compliance critical
- Safety critical
- Asset down
- Route blocking

### 7.7.2 Purchase Request Line Items

Line items should support:

- Item reference
- Free-text item
- Quantity
- Unit of measure
- Estimated unit cost
- Estimated extended cost
- Suggested vendor
- Required brand/manufacturer
- Accept substitutes flag
- Need-by date
- Category
- Notes
- Attachment
- Work order line reference
- Asset reference
- Route reference

### 7.7.3 Purchase Request Statuses

- Draft
- Submitted
- Needs clarification
- Pending approval
- Approved
- Rejected
- Cancelled
- Sourcing
- RFQ issued
- Quote received
- Converted to PO
- Partially ordered
- Ordered
- Partially received
- Received
- Closed

### 7.7.4 Purchase Request Actions

- Save draft
- Submit
- Add line item
- Edit line item
- Attach document
- Route for approval
- Approve
- Reject
- Request clarification
- Split request
- Merge requests
- Convert to RFQ
- Convert to PO
- Cancel
- Close

---

## 7.8 Approval Workflows

### 7.8.1 Approval Rule Inputs

Approval workflows should consider:

- Requester
- Requester's role
- Requester's department
- Requester's site
- Requested category
- Requested vendor
- Vendor approval status
- Vendor risk rating
- Total amount
- Line amount
- Priority
- Emergency flag
- Compliance-critical flag
- Safety-critical flag
- Budget code
- Asset class
- Work order priority
- Customer requirement
- Required documents
- Training/certification requirements

### 7.8.2 Approval Levels

SupplyArr should support:

- Auto-approval under threshold
- Supervisor approval
- Department manager approval
- Site manager approval
- Purchasing approval
- Compliance approval
- Finance approval
- Executive approval
- Emergency approval
- Dual approval
- Sequential approval
- Parallel approval

Approval authority should come from StaffArr permission and org data, not from local duplicated people ownership.

### 7.8.3 Approval Actions

- Approve
- Reject
- Request changes
- Delegate
- Escalate
- Hold
- Waive requirement
- Attach justification
- Add condition
- Approve emergency purchase

### 7.8.4 Approval Audit

Approval history should capture:

- Person reference
- Action
- Timestamp
- Decision
- Reason
- Old status
- New status
- Amount at approval
- Rule that required approval
- Attachments
- Comments

---

## 7.9 RFQs and Quotes

### 7.9.1 RFQ Creation

RFQs should support:

- RFQ number
- Title
- Description
- Request reference
- Line items
- Vendors invited
- Due date
- Delivery requirements
- Terms
- Attachments
- Notes

### 7.9.2 Vendor Quote Capture

Quotes should support:

- Vendor reference
- Quote number
- Quote date
- Expiration date
- Line item pricing
- Freight
- Tax estimate
- Discounts
- Lead time
- Availability
- Minimum order quantity
- Purchase multiple
- Alternate parts
- Terms
- Attachments
- Notes

### 7.9.3 Quote Comparison

SupplyArr should compare quotes by:

- Unit cost
- Extended cost
- Freight
- Taxes
- Total cost
- Lead time
- Vendor approval status
- Vendor risk status
- Document compliance
- Historical performance
- Substitution risk
- Warranty terms
- Return policy
- Quote expiration
- Preferred vendor status

### 7.9.4 Quote Outcomes

Quote line outcomes:

- Selected
- Not selected
- Rejected
- Expired
- Needs clarification
- Alternate accepted
- Alternate rejected
- Partial award

---

## 7.10 Purchase Orders

### 7.10.1 PO Creation

Purchase orders should support:

- PO number
- Vendor
- Buyer person reference
- Site
- Department
- Ship-to location
- Bill-to reference
- Terms
- Status
- Source request
- Source quote
- Line items
- Taxes
- Freight
- Attachments
- Notes

### 7.10.2 PO Line Items

PO lines should support:

- Item reference
- Vendor item number
- Description
- Quantity ordered
- Quantity received
- Quantity cancelled
- Unit of measure
- Unit price
- Extended price
- Taxable flag
- Expected date
- Need-by date
- Source demand reference
- Work order reference
- Asset reference
- Category
- Notes

### 7.10.3 PO Statuses

- Draft
- Pending approval
- Approved
- Issued
- Acknowledged
- Partially received
- Fully received
- Backordered
- Closed
- Cancelled
- Void
- Disputed

### 7.10.4 PO Actions

- Create
- Edit draft
- Submit for approval
- Approve
- Issue
- Send to vendor
- Record vendor acknowledgment
- Revise
- Cancel line
- Cancel PO
- Receive against PO
- Close
- Reopen
- Attach invoice
- Attach packing slip
- Export to accounting

---

## 7.11 Receiving

### 7.11.1 Receiving Workflow

Receiving should support:

1. Search PO or scan receiving reference.
2. Select line items.
3. Enter received quantity.
4. Record condition.
5. Capture packing slip.
6. Capture serial/lot numbers if required.
7. Record discrepancies.
8. Place into inventory location.
9. Release reserved demand.
10. Notify source product.
11. Close or partially close receipt.

### 7.11.2 Receipt Statuses

- Draft
- Received
- Partially received
- Overreceived
- Underreceived
- Damaged
- Wrong item
- Pending inspection
- Quarantined
- Returned
- Closed

### 7.11.3 Receiving Exceptions

Exception types:

- Quantity mismatch
- Damaged goods
- Wrong item
- Missing item
- Duplicate shipment
- Late delivery
- No PO
- Missing packing slip
- Price mismatch
- Quality issue
- Expired item
- Hazmat document missing
- Requires inspection

### 7.11.4 Receiving Outputs

Receiving should update:

- Inventory quantity
- On-order quantity
- Backorder status
- PO status
- Demand status
- Work order part availability
- Vendor performance metrics
- Pricing history
- Lead-time history
- Compliance evidence

---

## 7.12 Returns, Credits, and Warranty

### 7.12.1 Return Workflow

Returns should support:

- Return authorization number
- Vendor reference
- PO reference
- Receipt reference
- Item reference
- Quantity
- Reason
- Condition
- Photos/documents
- Return shipping tracking
- Credit expected
- Credit received
- Replacement expected
- Status

Return statuses:

- Draft
- Requested
- Authorized
- Shipped
- Received by vendor
- Credit pending
- Credit received
- Replacement pending
- Replacement received
- Rejected
- Closed

### 7.12.2 Warranty Claims

Warranty claim features:

- Claim number
- Vendor reference
- Manufacturer reference
- Item reference
- Asset reference from MaintainArr
- Work order reference from MaintainArr
- Failure description
- Install date
- Failure date
- Mileage/hours reference if provided
- Photos
- Supporting documents
- Claim status
- Credit/replacement status
- Root cause notes

Warranty statuses:

- Draft
- Submitted
- More information requested
- Approved
- Denied
- Credit issued
- Replacement issued
- Closed

---

## 7.13 Pricing and Lead-Time Intelligence

### 7.13.1 Pricing Snapshots

SupplyArr should store pricing snapshots from:

- Quotes
- Purchase orders
- Receipts
- Manual updates
- Imports
- Vendor catalogs
- API integrations

Pricing snapshot fields:

- Item reference
- Vendor reference
- Unit price
- Unit of measure
- Quantity break
- Effective date
- Expiration date
- Currency
- Freight estimate
- Tax estimate
- Source
- Confidence
- Notes

### 7.13.2 Lead-Time Snapshots

Lead-time fields:

- Item reference
- Vendor reference
- Quoted lead time
- Actual lead time
- Order date
- Received date
- Site
- Category
- Emergency flag
- Backorder flag
- Source PO

### 7.13.3 Source Recommendation

SupplyArr should recommend sources using:

- Vendor approval status
- Vendor compliance status
- Vendor preference
- Latest price
- Average price
- Quote expiration
- Lead time
- On-time delivery
- Fill rate
- Backorder risk
- Return rate
- Warranty performance
- Site availability
- Category approval
- Substitution approval

Recommendation output:

- Best overall
- Lowest cost
- Fastest delivery
- Preferred vendor
- Compliance safest
- Emergency option
- Needs approval
- Not recommended reason

---

## 7.14 Contracts and Purchasing Terms

SupplyArr should support vendor contracts and purchasing terms.

Features:

- Contract record
- Contract type
- Effective date
- Expiration date
- Renewal date
- Covered vendors
- Covered sites
- Covered categories
- Covered items
- Price agreement
- Discount agreement
- Minimum spend
- Service level agreement
- Payment terms
- Freight terms
- Warranty terms
- Attachments
- Alerts
- Approval status

Contract statuses:

- Draft
- Pending review
- Active
- Expiring soon
- Expired
- Superseded
- Cancelled
- Archived

---

## 7.15 Documents and Attachments

SupplyArr should include a document layer for procurement and vendor records.

Document attachment targets:

- External party
- Contact
- Vendor approval
- Supplier document
- Item
- Vendor item
- Purchase request
- Approval
- RFQ
- Quote
- Purchase order
- Receipt
- Return
- Warranty claim
- Contract
- Inventory adjustment
- Cycle count

Document capabilities:

- Upload file
- Add metadata
- Version document
- Mark confidential
- Mark compliance evidence
- Set expiration
- Request review
- Approve/reject document
- Link to Compliance Core rule
- Include in audit package

---

## 7.16 Incidents and Exceptions

SupplyArr should support supply-side incidents and exceptions.

Incident examples:

- Unauthorized vendor used
- Purchase made without approval
- Required document missing
- Expired vendor insurance used
- Wrong part ordered
- Wrong part received
- Damaged item received
- Critical stockout
- Emergency purchase required
- Supplier failed delivery
- Fraud or suspicious purchase
- Price mismatch
- Repeated receiving discrepancy
- Hazardous material handled incorrectly
- Warranty denied due to process failure

SupplyArr should report relevant incidents to StaffArr for personnel history and to TrainArr when retraining or certification review may be required. SupplyArr keeps ownership of the supply-side incident or receiving/purchasing exception record. StaffArr owns personnel cases created from the incident, TrainArr owns remediation or qualification impacts, and Compliance Core owns formal compliance evaluations or waivers.

---

## 7.17 Compliance Core Integration

SupplyArr should publish facts to Compliance Core rather than trying to own compliance law/rule evaluation.

Example facts:

- Vendor is approved for category
- Vendor is blocked
- Vendor document is expired
- Vendor insurance is accepted
- Purchase request was approved by authorized person
- Purchase order was issued before receipt
- Emergency purchase was justified
- Part was sourced from approved vendor
- Required document was attached
- Receiving discrepancy was recorded
- Warranty claim was filed
- Supplier incident occurred
- Critical inventory item is below minimum

Compliance Core should evaluate these facts against normalized rule packs and citations.

SupplyArr should display Compliance Core results where helpful, but Compliance Core remains the evaluator.

---

## 7.18 Reporting and Analytics

### 7.18.1 Supply Readiness Dashboard

Dashboard cards:

- Open purchase requests
- Requests pending approval
- Emergency requests
- Critical stockouts
- Items below reorder point
- Late purchase orders
- Backordered lines
- Vendor documents expiring
- Blocked vendors used attempt count
- Open receiving exceptions
- Open warranty claims
- Spend this month
- Average lead time
- Vendor on-time delivery

### 7.18.2 Vendor Reports

- Vendor approval status
- Vendor document expiration
- Vendor scorecard
- Vendor spend
- Vendor lead time
- Vendor fill rate
- Vendor backorder rate
- Vendor return rate
- Vendor warranty claim rate
- Vendor incidents
- Vendor category coverage

### 7.18.3 Parts and Inventory Reports

- Stock below minimum
- Stockout risk
- Dead stock
- Slow-moving stock
- Critical items
- Inventory valuation
- Cycle count variance
- Usage by item
- Usage by site
- Usage by asset class reference
- Part substitution history
- Price change history
- Lead-time trend

### 7.18.4 Purchasing Reports

- Requests by status
- Approval bottlenecks
- Spend by category
- Spend by site
- Spend by department
- Spend by vendor
- Emergency spend
- Off-contract spend
- Unapproved vendor attempts
- PO aging
- Receipt aging
- Open commitments
- Quote comparison history

### 7.18.5 Compliance Reports

- Vendor document coverage
- Missing required documents
- Expiring compliance documents
- Purchase approvals missing evidence
- Emergency purchase exceptions
- Restricted vendor usage attempts
- Supplier incidents
- Audit package by vendor
- Audit package by purchase order
- Audit package by item category

---

## 7.19 Search and Intelligence

SupplyArr should have fast, forgiving search.

Search targets:

- Vendors
- Customers
- Contacts
- Items
- Manufacturer part numbers
- Vendor part numbers
- Internal part numbers
- Purchase requests
- RFQs
- Quotes
- Purchase orders
- Receipts
- Returns
- Warranty claims
- Documents

Search capabilities:

- Fuzzy search
- Alias search
- Vendor part number search
- Barcode/QR search
- Recently used items
- Site-scoped search
- Category filters
- Approval-status filters
- Stock-status filters
- Vendor-status filters
- Saved views

---

## 7.20 Imports, Exports, and Integrations

### 7.20.1 Imports

SupplyArr should support imports for:

- Vendors
- Customers
- Contacts
- Parts catalog
- Vendor catalogs
- Inventory counts
- Price lists
- Lead-time lists
- Open POs
- Purchase history
- Supplier documents
- Contracts

Import features:

- CSV upload
- Excel upload
- Field mapping
- Validation preview
- Duplicate detection
- Alias matching
- Dry run
- Import history
- Rollback when safe
- Error export

### 7.20.2 Exports

Exports:

- Vendor list
- Approved vendor list
- Customer list
- Parts catalog
- Inventory valuation
- Purchase order export
- Receipt export
- Invoice-support export
- Supplier document report
- Compliance evidence packet
- Spend report

### 7.20.3 External Integrations

Potential integrations:

- Accounting / ERP systems
- Vendor catalog APIs
- Email inbox for quotes and order confirmations
- Barcode scanners
- Document storage
- E-signature provider
- Fleet/maintenance systems through MaintainArr
- Transportation systems through RoutArr
- Compliance systems through Compliance Core

---

## 8. UI / Application Structure

SupplyArr should use a unified Arr product shell while owning its product-specific navigation.

### 8.1 Primary Navigation

Recommended main nav:

1. Dashboard
2. Requests
3. Approvals
4. Vendors
5. Customers
6. Parts & Items
7. Inventory
8. RFQs & Quotes
9. Purchase Orders
10. Receiving
11. Returns & Warranty
12. Documents
13. Reports
14. Admin

### 8.2 Dashboard Pages

- Supply readiness overview
- Purchasing workload
- Approval queue
- Inventory risk
- Vendor compliance risk
- Receiving exceptions
- Spend snapshot
- Late orders
- Critical items

### 8.3 Vendor Pages

- Vendor list
- Vendor profile
- Approval status
- Contacts
- Documents
- Categories
- Sites served
- Scorecard
- Purchase history
- Incidents
- Contracts

### 8.4 Customer Pages

- Customer list
- Customer profile
- Contacts
- Locations
- Requirements
- Documents
- Linked RoutArr references
- Activity history

### 8.5 Parts and Items Pages

- Item list
- Item profile
- Vendor part numbers
- Substitutions
- Compatibility references
- Inventory by site
- Price history
- Lead-time history
- Usage history
- Documents

### 8.6 Purchasing Pages

- Purchase request board
- Request detail
- Approval detail
- RFQ detail
- Quote comparison
- PO list
- PO detail
- Receiving workspace
- Return detail
- Warranty claim detail

### 8.7 Inventory Pages

- Inventory overview
- Location/bin view
- Critical stock
- Reorder suggestions
- Cycle counts
- Adjustments
- Transfers
- Reservations
- Stock history

### 8.8 Reports Pages

- Supply readiness
- Vendor compliance
- Spend analysis
- Inventory risk
- PO aging
- Approval bottlenecks
- Receiving exceptions
- Warranty recovery
- Audit packets

---

## 9. API Surface

SupplyArr APIs should be versioned under `/api/v1`.

Recommended API groups:

- `/api/v1/health`
- `/api/v1/bootstrap`
- `/api/v1/external-parties`
- `/api/v1/vendors`
- `/api/v1/customers`
- `/api/v1/contacts`
- `/api/v1/vendor-documents`
- `/api/v1/items`
- `/api/v1/item-categories`
- `/api/v1/manufacturers`
- `/api/v1/vendor-items`
- `/api/v1/substitutions`
- `/api/v1/inventory`
- `/api/v1/inventory-locations`
- `/api/v1/stock-transactions`
- `/api/v1/cycle-counts`
- `/api/v1/purchase-requests`
- `/api/v1/approvals`
- `/api/v1/rfqs`
- `/api/v1/quotes`
- `/api/v1/purchase-orders`
- `/api/v1/receipts`
- `/api/v1/returns`
- `/api/v1/warranty-claims`
- `/api/v1/contracts`
- `/api/v1/documents`
- `/api/v1/reports`
- `/api/v1/search`
- `/api/v1/imports`
- `/api/v1/exports`
- `/api/v1/events`
- `/api/v1/admin`

---

## 10. Events

SupplyArr should use outbox/inbox patterns for cross-product communication.

### 10.1 Inbound Events

SupplyArr should consume events such as:

- `staffarr.person.created`
- `staffarr.person.updated`
- `staffarr.person.deactivated`
- `staffarr.site.created`
- `staffarr.site.updated`
- `staffarr.department.updated`
- `staffarr.permission.changed`
- `maintainarr.work_order.parts_required`
- `maintainarr.work_order.cancelled`
- `maintainarr.asset.updated`
- `maintainarr.defect.created`
- `maintainarr.pm.forecast_generated`
- `routarr.route.supply_needed`
- `routarr.trip.exception_created`
- `trainarr.certification_status.changed`
- `compliancecore.rulepack.updated`

### 10.2 Outbound Events

SupplyArr should publish events such as:

- `supplyarr.vendor.created`
- `supplyarr.vendor.updated`
- `supplyarr.vendor.approved`
- `supplyarr.vendor.blocked`
- `supplyarr.vendor.document.expiring`
- `supplyarr.vendor.document.expired`
- `supplyarr.customer.created`
- `supplyarr.item.created`
- `supplyarr.item.updated`
- `supplyarr.item.substitution_approved`
- `supplyarr.inventory.stock_low`
- `supplyarr.inventory.stockout`
- `supplyarr.inventory.reserved`
- `supplyarr.inventory.received`
- `supplyarr.purchase_request.created`
- `supplyarr.purchase_request.approved`
- `supplyarr.purchase_request.rejected`
- `supplyarr.purchase_order.issued`
- `supplyarr.purchase_order.cancelled`
- `supplyarr.purchase_order.partially_received`
- `supplyarr.purchase_order.received`
- `supplyarr.receiving.exception_created`
- `supplyarr.return.created`
- `supplyarr.warranty_claim.created`
- `supplyarr.incident.created`
- `supplyarr.compliance.fact_changed`

---

## 11. Data Model Overview

Recommended major entities:

| Entity | Purpose |
|---|---|
| `ExternalParty` | Master record for vendors, suppliers, customers, dealers, manufacturers, and other external organizations. |
| `ExternalPartyContact` | Contacts tied to external parties. |
| `ExternalPartyAddress` | Address records for billing, shipping, service, and branch locations. |
| `VendorApproval` | Approval status, scope, restrictions, and review history. |
| `VendorDocument` | Supplier compliance documents, expiration, verification, and status. |
| `Manufacturer` | Canonical manufacturer/brand record. |
| `ManufacturerAlias` | Alternate names and normalization rules. |
| `Item` | Part, material, consumable, supply, or service master. |
| `ItemCategory` | Category tree for item classification. |
| `VendorItem` | Vendor-specific part numbers, pricing, purchasing units, and availability. |
| `ItemSubstitution` | Equivalent, superseded, substitute, or blocked part relationships. |
| `InventoryLocation` | Warehouse, storeroom, bin, truck stock, or other stock location. |
| `StockBalance` | Current inventory position by item/location/site/lot/serial. |
| `StockTransaction` | Immutable stock movement ledger. |
| `CycleCount` | Inventory count event. |
| `CycleCountLine` | Item/location count result and variance. |
| `PurchaseRequest` | Demand request for purchase or replenishment. |
| `PurchaseRequestLine` | Requested item or free-text line. |
| `ApprovalWorkflow` | Approval routing definition or instance. |
| `ApprovalDecision` | Approval action history. |
| `RFQ` | Request for quote header. |
| `RFQLine` | RFQ line items. |
| `Quote` | Vendor quote header. |
| `QuoteLine` | Vendor quote line. |
| `PurchaseOrder` | PO header. |
| `PurchaseOrderLine` | PO line. |
| `Receipt` | Receiving header. |
| `ReceiptLine` | Receiving line. |
| `ReceivingException` | Damage, mismatch, shortage, overage, or other receiving problem. |
| `ReturnAuthorization` | Vendor return workflow. |
| `WarrantyClaim` | Claim against vendor/manufacturer for part or service failure. |
| `Contract` | Vendor agreement, price agreement, or purchasing terms. |
| `Document` | File metadata and links to records. |
| `PricingSnapshot` | Historical price by item/vendor/source/date. |
| `LeadTimeSnapshot` | Historical quoted and actual lead time. |
| `VendorScorecardSnapshot` | Periodic supplier performance summary. |
| `SupplyIncident` | Supply-side incident or exception. |
| `EventOutbox` | Events emitted by SupplyArr. |
| `EventInbox` | Events consumed from other products. |
| `PersonRef` | Local mirror/reference to StaffArr person. |
| `SiteRef` | Local mirror/reference to StaffArr site. |
| `DepartmentRef` | Local mirror/reference to StaffArr department. |
| `AssetRef` | Local mirror/reference to MaintainArr asset. |
| `WorkOrderRef` | Local mirror/reference to MaintainArr work order. |
| `RouteRef` | Local mirror/reference to RoutArr route/trip. |
| `ComplianceEvaluationSnapshot` | Local cached result from Compliance Core. |

---

## 12. Automations

SupplyArr automation should include:

- Auto-create purchase request when stock falls below reorder point.
- Auto-reserve inventory for approved MaintainArr work order demand.
- Auto-suggest substitutions when requested item is unavailable.
- Auto-flag unapproved vendor usage.
- Auto-block PO issuance when required vendor documents are expired.
- Auto-alert when supplier documents are expiring.
- Auto-route approvals based on amount, category, site, and vendor risk.
- Auto-create receiving exception for quantity mismatch.
- Auto-update vendor scorecards after receipts and returns.
- Auto-publish compliance facts after approval, PO issue, receipt, exception, or document change.
- Auto-create warranty claim suggestion when failed part is within warranty window.
- Auto-detect price increases above threshold.
- Auto-detect lead-time drift above threshold.
- Auto-detect duplicate vendors or duplicate part records.

---

## 13. Audit and Compliance History

Every important SupplyArr action should be auditable.

Audit records should capture:

- Tenant
- Product
- Entity type
- Entity ID
- Action
- Actor person reference
- Timestamp
- Previous value
- New value
- Source IP/session reference if available
- Reason/comment
- Related approval
- Related compliance rule
- Related document
- Source system

Audit-critical actions:

- Vendor approval changes
- Vendor restrictions
- Vendor document verification
- Item merge
- Substitution approval
- Inventory adjustment
- Purchase request approval
- PO issue
- PO cancellation
- Receiving exception
- Return creation
- Warranty claim submission
- Emergency purchase approval
- Compliance Core waiver reference / procurement exception
- Admin setting changes

---

## 14. Security and Ownership Rules

SupplyArr must protect ownership boundaries.

Rules:

1. SupplyArr must only use NexArr for login, tenant validation, entitlement, launch, and service authentication.
2. SupplyArr must not create an independent platform identity model.
3. SupplyArr must not own person records. It should reference StaffArr `personId` through local refs.
4. SupplyArr must not own sites, departments, positions, or teams. It should reference StaffArr.
5. SupplyArr must not own assets or work orders. It should reference MaintainArr.
6. SupplyArr must not own routes or trips. It should reference RoutArr.
7. SupplyArr must not own training completions or certifications. It should reference TrainArr/StaffArr-published facts.
8. SupplyArr must not own final compliance rule evaluation, legal/policy waivers, or normalized citation interpretation. It should publish facts to Compliance Core, display results from Compliance Core, and reference Compliance Core waivers where procurement exceptions require formal compliance approval.
9. Cross-product references must not use direct database foreign keys.
10. Customer-hosted or external product data must be treated as untrusted input.
11. Backend authorization must enforce product ownership. Frontend hiding alone is not security.
12. Service-token endpoints must be scoped, audited, and least-privilege.

---

## 15. Completion Definition

SupplyArr can be considered complete when it can reliably support the full supply lifecycle.

### 15.1 Minimum Complete Product

SupplyArr reaches minimum complete status when it supports:

- Vendor/customer/external party master data
- Vendor approval status
- Vendor documents with expirations
- Item/parts catalog
- Vendor part numbers
- Basic inventory by site/location
- Purchase requests
- Approval workflows
- Purchase orders
- Receiving
- Basic reporting
- StaffArr references for people/sites/departments
- MaintainArr demand intake for parts
- Compliance Core fact publishing
- Audit history

### 15.2 Operationally Complete Product

SupplyArr is operationally complete when it supports:

- Full supplier onboarding
- Category/site-scoped vendor approvals
- Supplier compliance document enforcement
- Parts substitutions and supersessions
- Pricing and lead-time history
- RFQs and quote comparison
- Inventory reservations
- Cycle counts
- Returns
- Warranty claims
- Vendor scorecards
- Reorder automation
- Approval authority from StaffArr
- Demand intake from MaintainArr, RoutArr, StaffArr, and TrainArr
- Event outbox/inbox integration
- Rich reporting

### 15.3 Enterprise Complete Product

SupplyArr is enterprise complete when it supports:

- Multi-site supply readiness scoring
- Advanced vendor risk management
- Contract and price agreement tracking
- Vendor portal or controlled vendor communication flow
- Catalog imports and vendor integrations
- Accounting/ERP export support
- Compliance audit packet generation
- Predictive stockout risk
- Vendor performance benchmarking
- Cross-product incident routing
- Emergency procurement controls
- Customer-specific supply requirements
- Advanced source recommendation
- Full approval explainability
- High-volume search and reporting
- Customer-hosted data-plane readiness

---

## 16. Example End-to-End Flows

### 16.1 MaintainArr Part Demand Flow

1. Technician creates or updates a MaintainArr work order.
2. MaintainArr determines a part is required.
3. MaintainArr emits `work_order.parts_required`.
4. SupplyArr receives demand.
5. SupplyArr checks inventory.
6. If stock exists, SupplyArr reserves the part.
7. If stock does not exist, SupplyArr checks substitutes and vendor sources.
8. SupplyArr creates a purchase request.
9. Approval rules route the request.
10. Approved request becomes RFQ or PO.
11. Vendor ships part.
12. Receiving records the item.
13. SupplyArr updates inventory and notifies MaintainArr.
14. MaintainArr consumes the part on the work order.
15. SupplyArr records cost and source history.
16. Compliance Core receives procurement facts.

### 16.2 Vendor Onboarding Flow

1. Purchasing creates new vendor.
2. SupplyArr captures legal name, contacts, categories, sites, and documents.
3. Required supplier documents are uploaded.
4. Compliance review verifies documents.
5. Vendor manager approves category/site scope.
6. SupplyArr marks vendor approved.
7. Compliance Core receives facts about approval and document status.
8. Vendor becomes available for purchase requests and POs.

### 16.3 Emergency Purchase Flow

1. Critical asset is down or route is blocked.
2. Requester creates emergency purchase request.
3. SupplyArr checks approved vendors first.
4. If only restricted/unapproved vendor can satisfy demand, SupplyArr requires emergency justification.
5. Supervisor/compliance approval is required.
6. PO is issued with emergency flag.
7. Receiving and documents are attached after purchase.
8. Compliance Core evaluates whether the emergency exception was properly evidenced.
9. StaffArr receives incident if policy was violated.
10. TrainArr receives retraining trigger if the event indicates training failure.

### 16.4 Vendor Document Expiration Flow

1. Vendor insurance is approaching expiration.
2. SupplyArr alerts vendor manager and compliance contact.
3. Vendor receives document request.
4. If document expires, vendor status changes to restricted or blocked based on rule.
5. New POs are blocked or require waiver.
6. Compliance Core receives updated facts.
7. Dashboards show vendor risk until resolved.

---

## 17. Implementation Priorities

Recommended implementation order:

1. Product shell, NexArr launch/auth, tenant context, and service-token validation.
2. StaffArr local refs for people, sites, departments, and permissions.
3. External party master data.
4. Vendor approval and document tracking.
5. Item/parts catalog.
6. Vendor item numbers and manufacturer aliases.
7. Inventory locations and stock balances.
8. Purchase requests.
9. Approval workflows.
10. Purchase orders.
11. Receiving.
12. MaintainArr part-demand integration.
13. Reporting dashboards.
14. RFQs and quotes.
15. Returns and warranty claims.
16. Compliance Core fact publishing.
17. Reorder automation.
18. Vendor scorecards.
19. Imports/exports.
20. Advanced source recommendation and predictive readiness.

---

## 18. North Star

SupplyArr should not just be a purchasing screen.

It should be the supply readiness engine for STL Compliance.

When complete, it should tell the business:

- Who can we buy from?
- Are they approved?
- Are their documents current?
- What can they supply?
- What does it cost?
- How long will it take?
- Is there a better source?
- Is the item in stock?
- Is this purchase allowed?
- Who approved it?
- Was it received correctly?
- Did it satisfy the operational need?
- Can we prove it later?

The product is complete when those answers are immediate, explainable, auditable, and connected to the rest of the Arr ecosystem.


---

## Boundary-Resolved Completion Notes

- SupplyArr audit packages are procurement/supply-scope exports. Compliance Core owns cross-product compliance audit packages that evaluate SupplyArr facts against rule packs.
- SupplyArr owns supplier/customer/external-party master records. RoutArr owns trip/stop execution snapshots and references these records rather than owning the master data.
- SupplyArr owns procurement exceptions. Compliance Core owns compliance waivers. StaffArr owns personnel cases, and TrainArr owns retraining or qualification impacts.

---

## Audit-Informed Feature Additions: Platform Access, Ownership, and Verification

These additions are part of the product feature set. They are not optional implementation notes.

### NexArr Launch and Product Session Contract

Protected product experiences must use the platform launch pattern:

1. User starts in NexArr.
2. NexArr validates login, tenant status, product status, entitlement, callback allowlist, and launch state.
3. NexArr redirects to the product callback path: `/auth/nexarr/callback`.
4. The product backend redeems the handoff code server-side.
5. The product creates a local product session containing at minimum `personId`, `tenantId`, `productCode`, entitlement snapshot, and session expiry.
6. The product then applies its own server-side domain authorization rules.

### Required Access Features

- `/auth/nexarr/callback` route in the product frontend and backend.
- Server-side handoff redemption.
- Expired, reused, missing, wrong-product, and invalid-callback handoff rejection.
- Friendly launch failure, entitlement denied, invalid callback, product unavailable, and tenant selection states.
- Product session hydration endpoint.
- Product logout or session clear behavior that does not create a competing login system.
- Quick-switch menu that reads NexArr catalog data and sends users back through NexArr `/launch/{productCode}`.
- Tenant context display sourced from the validated product session.
- Current user display sourced from `personId` and product/session data.
- No product-generated trusted launch URLs.
- No product-side entitlement guessing.
- No product-owned platform login.

### Authority and Safety Rules

- Frontend hiding is not authorization.
- No production feature may rely on localStorage admin switches, mock users, hardcoded role strings, fake permission strings, or frontend-only entitlement checks.
- Development-only identity or permission shortcuts must be guarded by `VITE_APP_ENV=development` and must not ship as production fallbacks.
- Product APIs must validate tenant, session, entitlement, product permission, and record ownership server-side.
- Cross-product records must use APIs, events, service tokens, local mirrors, snapshots, or external references. No direct cross-product database foreign keys.
- Product switchers and shared shells are visual/structural only; they do not centralize product-specific authorization.

### Feature Verification Standard

A feature is complete only when there is concrete implementation evidence:

- Backend route/service/model/schema where applicable.
- Frontend route/page/component/API client where applicable.
- Persistence where the feature implies stored data.
- Authorization where the feature implies protected access.
- Cross-product contract where the feature depends on another product.
- Tests or smoke checks where practical.

TODO text, mock-only state, placeholder UI, documentation-only claims, sample data, or frontend-only screens do not count as completed features.

---

## Audit-Informed Feature Additions: Owner-Controlled Embedded Procurement and Supply Readiness

### Owner-Controlled Embedded Procurement

Other products should be able to initiate routine supply workflows without becoming procurement systems.

Features:

- Product-originated purchase request API.
- Embedded SupplyArr-owned request surface that can appear inside MaintainArr or another product.
- Source product reference fields such as work order, asset, trip, site, requester, demand reason, and needed-by date.
- SupplyArr-owned approval rules, vendor selection, RFQ, PO, receiving, returns, warranty, and supplier document checks.
- Source product status callbacks/events.
- Deep-link to the SupplyArr source record.
- Audit trail showing which product originated the demand and who approved each supply action.

Completion criteria:

- A shop manager can request parts from a MaintainArr work order without switching apps for routine entry, but all procurement authority and records remain SupplyArr-owned.

### Supply Readiness API

SupplyArr should expose whether an item, vendor, supplier, or purchase path is ready for use.

Features:

- Vendor/supplier approval status check.
- Required supplier document status.
- Part/material availability check.
- Reservation status.
- Pricing/lead-time snapshot.
- Substitute/equivalent recommendation.
- Procurement blocker reason codes.
- Compliance Core evaluation references where applicable.
- Response staleness and source timestamp.

Completion criteria:

- MaintainArr and RoutArr can display supply readiness without duplicating supplier, part, inventory, or procurement rules.

### External Identifier Contract

SupplyArr should expose stable external references for cross-product use.

Features:

- Stable external party reference ID.
- Stable part/material reference ID.
- Stable purchase request, PO, receiving, and warranty reference IDs.
- Human-readable display codes that are not treated as database primary keys by other products.
- Reference resolution endpoints for product mirrors/snapshots.

Completion criteria:

- Other products can safely store SupplyArr references without relying on brittle internal IDs or direct database foreign keys.
