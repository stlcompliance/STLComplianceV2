STL COMPLIANCE / ADAPTIVE RISK REDUCTION
OWNERSHIP CONSTITUTION

Purpose:
STL Compliance is a business orchestration and compliance execution suite for regulated operations.

STL coordinates people, work, assets, inventory, dispatch, customers, vendors, records, assurance, reporting, compliance evidence, finance, and external system handoffs.

STL owns financial truth through LedgArr and integrates with external accounting, payroll, banking, certified hardware, and other specialized external systems where those systems remain outside STL.

Core Rule:
Every record must have one clear owner.
Other products may reference, mirror, snapshot, request, consume, or report on that record.
No product may silently become the source of truth for another product's domain.

============================================================
GLOBAL OWNERSHIP RULES
============================================================

1. One owner per business truth.

A product may display data from another product, but it does not own that data.

Example:
- RoutArr may display driver qualification status.
- TrainArr owns the qualification.
- StaffArr owns the person.
- RoutArr owns the trip assignment decision.

2. No cross-database foreign keys.

Each product has its own database.
Cross-product relationships use stable IDs, APIs, events, service tokens, mirrors, and snapshots.
Products must not directly join another product's database.

3. NexArr is the only login/auth gate.

All products rely on NexArr for platform identity, tenant validation, entitlement, product launch, and service identity.

Products may own domain permissions after NexArr validates:
- user is valid
- tenant is valid
- product entitlement is valid

4. StaffArr owns internal people and internal places.

StaffArr owns:
- people
- workers
- org units
- internal sites
- buildings
- rooms
- docks
- yards
- operational locations
- departments
- positions
- teams
- manager relationships
- permission assignments

Other products reference StaffArr people and locations.

5. Products own execution in their lane.

Each product owns its own operational workflow, status, history, and evidence in its domain.

6. Compliance Core interprets rules.

Compliance Core owns regulatory meaning, rulepacks, governing bodies, applicability, evidence requirements, exemptions, exceptions, and audit logic.

Compliance Core stays focused on rule interpretation rather than operational execution in maintenance, dispatch, warehouse, training, customer, vendor, or financial domains.

7. RecordArr stores records.

Products may create records, attach documents, request retention, and classify evidence.
RecordArr owns the stored file, document metadata, versioning, retention, access history, and controlled document lifecycle.

8. ReportArr reports; it does not correct.

ReportArr owns dashboards, reports, analytics views, scheduled reports, exports, and cross-product insight.
It does not own or modify the source operational truth.

9. External systems remain external unless STL has a named owner.

QuickBooks, external ERP/accounting systems, payroll providers, ELD systems, telematics platforms, banks, tax systems, CRMs, and specialized hardware systems remain external unless STL explicitly builds a replacement product.

LedgArr is that named replacement for STL financial ledger, AP, AR, posting, close, tax accounting, and financial controls. External finance systems may remain bridge/export targets or external GL masters by tenant mode, but they are not the default STL source of financial truth.

STL may integrate with external systems, consume data from them, export LedgArr-approved batches to them, and store external status snapshots.

10. Financial ownership boundaries.

Operational products may prepare:
- invoice-ready packets
- bill-ready packets
- purchase intent
- operational cost snapshots
- customer signoff packets
- fulfillment summaries

LedgArr owns the STL financial records created from those packets:
- invoices
- bills
- payments
- tax
- general ledger
- accounts payable
- accounts receivable
- bank reconciliation
- accounting close

External ERP/accounting systems own their external records, IDs, sync state, and external GL truth only when a tenant deliberately configures LedgArr for External GL Master or Export Only mode.

============================================================
PRODUCT OWNERSHIP
============================================================

============================================================
ReferenceDataCore
============================================================

Identity:
The shared public reference-data and normalization service for identifiers that outlive a single tenant workflow.

Owns:
- public identifiers and aliases
- public taxonomies
- canonical units of measure and package normalization crosswalks
- manufacturer and brand identity
- public product, chemical, SDS, vehicle, equipment, and similar reference identities
- external-system crosswalks for shared reference entities
- reference ingestion, candidate review, merge, split, and provenance history

Does not own:
- tenant commercial item, part, material, or SKU context
- supplier pricing, lead time, approval, or procurement context
- inventory balances or execution profiles
- compliance rule meaning or evidence satisfaction
- retained files
- product execution records

Boundary:
ReferenceDataCore owns shared identity and normalization. Products may reference it or snapshot selected labels, but product-owned workflow, execution, commercial, evidence, and tenant overlay data stay in the owning product.

============================================================
NexArr
============================================================

Identity:
The secure front door and control plane of the STL suite.

Owns:
- platform login
- authentication
- tenant identity
- tenant membership
- product entitlement
- subscription/package state
- product launch
- platform admin
- platform service clients
- service tokens
- handoff sessions
- break-glass platform access
- platform access audit events
- product dependency rules
- external system authorization handoff where platform-level identity is required

Does not own:
- product-specific permissions
- personnel records
- customer records
- vendor records
- orders
- documents
- inventory
- dispatch
- maintenance
- reports
- financial execution

Rules:
- No product may implement a separate platform login.
- No product may own platform admin.
- No product may bypass NexArr entitlement validation.
- Products may own domain authorization after NexArr validates access.

============================================================
StaffArr
============================================================

Identity:
The people, organization, location, and authority center.

Owns:
- person master records
- worker records
- employees
- contractors
- non-employee workers
- internal org units
- departments
- positions
- teams
- reporting structure
- manager/subordinate relationships
- internal sites
- buildings
- rooms
- docks
- yards
- operational locations
- bins if treated as addressable physical locations
- permission assignments
- role assignments
- work authority context
- person status
- delegation
- temporary authority
- personnel history
- incident/personnel history
- person audit package

Does not own:
- training programs
- certification definitions
- maintenance work
- dispatch work
- inventory work
- customer contacts
- vendor contacts
- accounting
- payroll execution

Rules:
- StaffArr personId is the platform-wide human reference.
- Products reference StaffArr/NexArr people; they do not create local human identity as source of truth.
- StaffArr locations are canonical for internal physical places.
- LoadArr, MaintainArr, RoutArr, TrainArr, Compliance Core, and other products consume StaffArr location identity.

============================================================
TrainArr
============================================================

Identity:
The LMS + qualification management engine.

Owns:
- course catalog and learning paths
- course and program definitions
- training programs
- training modules
- training steps
- learner enrollments and assignments
- progress tracking
- assessments and evaluations
- instructor-led sessions and signoffs
- certificates and credentials
- transcripts and completion history
- remediation
- retraining
- qualification rules
- trainee signoffs
- trainer signoffs
- evaluator signoffs
- practical evaluations
- training assignments
- training content references
- training requirement profiles
- qualification definitions
- person qualifications
- qualification expiration
- qualification renewal
- certification issuance
- certification renewal
- certification expiration
- certification revocation events
- training evidence
- training completion records
- qualification publication to StaffArr

Does not own:
- person master records
- job/position master records
- incident master records
- maintenance work
- dispatch work
- inventory work
- payroll
- accounting

Rules:
- StaffArr owns the person.
- TrainArr owns the learning workflow and the qualification.
- TrainArr publishes qualification/readiness results to StaffArr.
- StaffArr may show qualification status but does not define the training program.
- Incidents may trigger TrainArr retraining, but TrainArr does not own the original incident unless the incident is training-specific.

============================================================
MaintainArr
============================================================

Identity:
The maintenance execution system.

Owns:
- asset master records
- asset hierarchy
- asset components
- asset condition
- asset readiness
- preventive maintenance
- inspections
- defects
- work orders
- repairs
- maintenance labor capture
- downtime
- failure history
- maintenance evidence
- warranty tracking
- recall tracking
- parts demand requests
- maintenance compliance records

Does not own:
- inventory stock ledger
- warehouse bins as inventory truth
- parts room ownership
- vendor master
- purchase approvals
- staff location identity
- dispatch execution
- customer relationship
- financial asset ledger

Rules:
- MaintainArr may request parts.
- LoadArr owns inventory availability, reservations, issues, and stock movement.
- SupplyArr owns item/vendor/procurement context.
- StaffArr owns locations and people.
- MaintainArr may expose guided parts actions, but it must not own inventory truth.

============================================================
RoutArr
============================================================

Identity:
The dispatch and transportation execution system.

Owns:
- routes
- trips
- transportation demand before trip creation
- carrier tender execution for a specific movement
- routing-guide selection snapshots for a specific movement
- freight rating snapshots and transportation cost facts
- accessorial events and freight audit deltas
- normalized transportation visibility events
- dispatch feasibility and HOS/capacity snapshots
- transportation yard, trailer, gate, drop/hook, detention, and dwell events
- freight claim movement context
- transportation document packet requests
- transportation finance packet contributions
- dispatch plans
- driver assignments
- vehicle assignments
- stop sequence
- pickup execution
- delivery execution
- trip status
- ETA/status updates
- transportation exceptions
- proof of pickup
- proof of delivery
- dock appointment notifications to LoadArr
- transportation evidence
- dispatch audit trail

Does not own:
- driver person master records
- driver certifications
- vehicle maintenance truth
- inventory truth
- warehouse receiving truth
- customer master records
- financial freight billing
- ELD hardware
- telematics hardware
- carrier/vendor master records
- carrier commercial rate agreements
- customer freight terms as customer truth
- accounting invoices, bills, payments, tax, ledger, or reconciliation

Rules:
- StaffArr owns drivers as people.
- TrainArr owns driver qualifications.
- MaintainArr owns vehicle maintenance readiness.
- LoadArr owns warehouse/load readiness.
- CustomArr owns customer context.
- OrdArr owns the parent request/order when transportation is part of a larger business request.
- ELD/telematics systems remain external sources of hardware-captured truth.
- TransportationDemand is RoutArr-owned schedulable transportation demand; it must not be confused with LoadArr inventory loads or SupplyArr procurement demand refs.
- RoutArr may prepare transportation finance contribution packets, but OrdArr/SupplyArr own invoice-ready and bill-ready packet assembly and LedgArr owns financial execution.

============================================================
SupplyArr
============================================================

Identity:
The supplier, vendor, item, and procurement context system.

Owns:
- vendor master records
- supplier master records
- supplier contacts
- supplier documents
- supplier requirements
- item master
- part master
- material master
- vendor-item relationships
- preferred suppliers
- price snapshots
- lead-time snapshots
- purchase requests
- RFQs
- operational purchasing approvals
- purchase intent
- PO metadata when used operationally
- procurement status
- external vendor IDs
- LedgArr vendor financial profile and external ERP vendor mapping

Does not own:
- customer master if CustomArr exists
- inventory balances
- stock ledger
- warehouse movement
- payment execution
- accounts payable
- tax
- banking
- general ledger

Rules:
- SupplyArr owns who/what the company buys from and what purchasable items exist.
- LoadArr owns physical inventory and stock movement.
- LedgArr owns financial posting, AP, bill payment records, payment exports, and accounting execution.
- CustomArr owns customers.
- OrdArr may request procurement through SupplyArr when an order/request requires purchased goods or services.

============================================================
LoadArr
============================================================

Identity:
The warehouse and inventory execution system.

Owns:
- expected receipts
- receiving workflow
- dock receiving queue
- putaway
- inventory balances
- stock ledger
- warehouse tasks
- reservations
- picks
- issues
- returns
- cycle counts
- inventory adjustments
- inventory holds
- quarantine status
- lot tracking
- serial tracking
- bin stock
- fulfillment status
- inventory availability API
- work-order parts fulfillment to MaintainArr
- order fulfillment handoff to OrdArr

Does not own:
- StaffArr location identity
- vendor master
- item commercial ownership
- purchase approvals
- customer master
- dispatch execution
- maintenance work orders
- financial inventory valuation ledger
- dedicated scanner/hardware ownership

Rules:
- LoadArr must use StaffArr locations.
- LoadArr owns stock movement truth.
- SupplyArr owns item/vendor/procurement context.
- MaintainArr consumes parts availability and fulfillment status.
- RoutArr consumes load readiness and dock appointment status.
- OrdArr consumes fulfillment status for order/request orchestration.

============================================================
Compliance Core
============================================================

Identity:
The rules, evidence, and regulatory intelligence system.

Owns:
- governing body catalogs
- rulepacks
- regulations
- law citations
- controlled compliance vocabulary
- applicability logic
- evidence requirements
- exemptions
- exceptions
- compliance interpretations
- audit package logic
- evidence classification
- compliance gap analysis
- theoretical situation evaluation
- rule-to-product mapping
- internal policy rules
- contract obligation rule support
- compliance event intake

Does not own:
- operational execution
- stored document files
- training execution
- inventory execution
- dispatch execution
- maintenance execution
- customer relationship
- vendor relationship
- accounting
- legal advice as a substitute for counsel

Rules:
- Compliance Core tells products what rules apply and what evidence is required.
- Compliance Core does not perform the product workflow.
- Products provide events/evidence to Compliance Core.
- RecordArr stores the underlying documents/records.
- ReportArr reports compliance posture.

============================================================
Field Companion
============================================================

Identity:
The mobile human execution layer.

Owns:
- mobile task inbox
- product switcher
- guided execution screens
- photo capture
- document capture
- signature capture
- secure no-login upload flows
- offline-capable field actions
- inspection execution UI
- delivery confirmation UI
- incident self-reporting UI
- human evidence capture UI
- push/in-app task surfaces

Does not own:
- final operational records
- ELD replacement
- scanner hardware replacement
- accounting
- source-of-truth product data
- product-specific business rules

Rules:
- Field Companion is a surface, not a source-of-truth product.
- Field actions write back to the owning product.
- Captured records/documents may be stored through RecordArr.
- Rule/evidence meaning comes from Compliance Core.

============================================================
CustomArr
============================================================

Identity:
The tenant customer relationship, customer master, and commercial intent system.

Owns:
- customer accounts
- customer hierarchy
- customer contacts
- customer locations/sites
- customer leads
- customer opportunities
- customer proposals as CRM snapshots
- customer agreements as CRM metadata and references
- customer relationship cases
- customer activities
- customer tasks
- customer communication history
- customer notes
- customer preferences
- customer requirements
- customer requirement evaluations as relationship-readiness facts
- customer onboarding
- account ownership
- customer status
- customer health/success snapshots
- customer risk/hold status
- customer portal relationship
- customer portal access records and NexArr identity references
- customer-specific operational restrictions
- customer service expectations
- customer service eligibility checks
- customer import batches
- customer duplicate candidates
- customer merge review records
- customer external IDs
- CRM external mapping
- LedgArr customer financial profile and external ERP customer mapping

Does not own:
- sales accounting
- invoices
- payments
- tax
- inventory
- dispatch execution
- maintenance execution
- dispatch/warehouse/maintenance execution records
- contract lifecycle legal execution if ContractArr exists later
- signed document/file lifecycle
- regulatory interpretation
- order/request lifecycle
- financial quote, invoice, payment, tax, ledger, or accounting close truth

Rules:
- CustomArr owns who the customer is, how the tenant relates to that customer, and what commercial intent has been recorded before downstream execution.
- OrdArr owns what the customer requested.
- Opportunities and proposals accepted in CustomArr create explicit handoffs or references; they do not directly create execution records in OrdArr, RoutArr, LoadArr, MaintainArr, SupplyArr, or finance systems.
- Proposal pricing and agreement terms in CustomArr are CRM snapshots, not financial ledger truth.
- LedgArr owns customer invoice, AR, payment application, tax accounting, and ledger execution.
- Compliance Core interprets customer requirements when they become evidence/rule obligations.
- RecordArr stores customer documents and controlled records.

============================================================
OrdArr
============================================================

Identity:
The operational order and request orchestration system.

Owns:
- customer orders
- internal requests
- service requests when no ServiceArr exists
- work requests
- request intake
- request triage
- order/request lifecycle
- order/request status
- parent business object tying work together
- request-to-work conversion
- order-to-LoadArr handoff
- order-to-RoutArr handoff
- order-to-MaintainArr handoff
- order-to-SupplyArr handoff
- completion packet
- invoice-ready packet
- bill-ready packet
- operational closeout
- customer-facing request status

Does not own:
- customer master
- inventory execution
- dispatch execution
- maintenance execution
- training execution
- accounting execution
- payment truth
- product-domain work records after handoff

Rules:
- OrdArr explains why work is happening.
- Execution products own how work is performed.
- OrdArr coordinates status across products without taking ownership of their records.
- OrdArr may prepare financial handoff packets but does not create invoices, bills, payments, or ledger entries.
- CustomArr owns customer context.
- ReportArr reports order performance.
- RecordArr stores order documents and completion packets.

============================================================
LedgArr
============================================================

Identity:
The financial ledger, accounting, and financial control system.

Owns:
- financial legal entities
- chart of accounts
- journals
- posting rules
- subledgers
- accounts payable
- accounts receivable
- payment application
- tax accounting
- budget and forecast ledgers
- financial reporting
- reconciliation workflows
- close controls
- audit controls
- external finance integrations and handoff status

Does not own:
- platform identity
- tenant membership
- people or worker records
- customer master records
- vendor master records
- operational execution in other products
- stored documents
- dispatch execution
- maintenance execution
- warehouse execution
- compliance rule interpretation
- Compliance Core governing bodies, regulators, agencies, citations, rulepacks, or regulatory vocabulary

Rules:
- LedgArr owns financial truth and accounting execution.
- LedgArr FinancialLegalEntity records describe tenant-owned accounting/reporting entities only; they must not model Compliance Core GoverningBody records.
- Other products may prepare operational handoff packets, snapshots, or references, but they do not create ledger truth.
- RecordArr stores supporting documents and evidence for accounting workflows.
- ReportArr may render financial analytics, but it does not post or correct accounting records.

============================================================
RecordArr
============================================================

Identity:
The documents, records, controlled files, and retention system.

Owns:
- document storage
- record metadata
- file versions
- controlled documents
- policies
- SOPs
- templates
- effective dates
- expiration dates
- retention schedules
- legal holds
- document approvals
- read-and-acknowledge records
- evidence file storage
- document access history
- record packages
- attachment service for products
- OCR/document processing metadata when used

Does not own:
- compliance rule interpretation
- operational product records
- customer master
- vendor master
- training programs
- inventory
- dispatch
- maintenance
- accounting

Rules:
- Products attach documents through RecordArr.
- Compliance Core classifies what evidence is required and what evidence means.
- RecordArr preserves the document and retention/audit trail.
- ReportArr may render/export report artifacts, but RecordArr owns stored records.

============================================================
AssurArr
============================================================

Identity:
The assurance, quality, nonconformance, CAPA, verification, and release system.

Owns:
- nonconformance reports
- assurance cases
- quality cases
- corrective actions
- preventive actions
- root cause analysis
- containment actions
- deviation records
- quality holds as business decisions
- release approvals
- effectiveness checks
- supplier quality issues
- customer complaints related to quality
- internal audit findings
- assurance inspections
- recurrence tracking
- CAPA evidence package
- escalation to TrainArr for retraining
- escalation to StaffArr for personnel action
- escalation to LoadArr for inventory hold/release
- escalation to MaintainArr for repair/correction
- escalation to SupplyArr for supplier issue tracking
- escalation to CustomArr for customer communication context

Does not own:
- inventory ledger
- warehouse physical movement
- employee discipline
- training execution
- maintenance repair execution
- regulatory interpretation
- customer master
- vendor master
- accounting

Rules:
- AssurArr owns the assurance case.
- The affected product owns the corrective execution.
- Compliance Core determines rule/evidence implications.
- RecordArr stores supporting evidence.
- StaffArr owns personnel history impact.
- TrainArr owns retraining.
- LoadArr owns inventory movement/holds at the execution level.

============================================================
ReportArr
============================================================

Identity:
The reporting, analytics, dashboards, exports, and cross-suite intelligence system.

Owns:
- cross-product dashboards
- report definitions
- scheduled reports
- KPI views
- executive summaries
- audit readiness dashboards
- compliance posture dashboards
- asset readiness reporting
- training readiness reporting
- inventory health reporting
- dispatch performance reporting
- vendor performance reporting
- customer performance reporting
- assurance/CAPA reporting
- order/request reporting
- export packages
- report subscriptions
- snapshot/report history

Does not own:
- operational source-of-truth records
- source data correction
- compliance interpretation
- product execution
- financial ledger
- document storage except rendered report artifacts

Rules:
- ReportArr consumes events, snapshots, and read models.
- ReportArr does not mutate source records.
- Corrections happen in the owning product.
- ReportArr can show confidence, freshness, source, and sync status.

============================================================
EXTERNAL SYSTEM OWNERSHIP
============================================================

QuickBooks / ERP owns only external finance system truth:
- external invoice, bill, payment, and posting IDs after export
- external sync status
- external error/status snapshots
- external GL truth when a tenant deliberately configures LedgArr for External GL Master mode
- manual import/export artifacts outside STL

LedgArr owns STL financial truth:
- financial legal entities
- invoices
- bills
- payments and payment application
- accounts payable
- accounts receivable
- tax accounting
- general ledger
- bank reconciliation references
- accounting close
- financial customer/vendor profile mappings as accounting objects
- external finance mappings and posting batch history

Other STL products own:
- operational customer/vendor records
- operational completion packets
- invoice-ready packets
- bill-ready packets
- source references and snapshots that LedgArr consumes

Payroll / HR payroll system owns:
- payroll execution
- tax withholding
- direct deposit
- wage statements
- payroll filings

STL owns:
- person records
- work authority
- training readiness
- labor/time operational capture where applicable
- payroll handoff snapshots if implemented

ELD / telematics / hardware vendors own:
- certified ELD capture
- hardware-generated records
- device-specific telemetry
- hardware compliance certifications
- device firmware/hardware lifecycle

STL owns:
- operational consumption of external hardware data
- evidence classification
- related work/status decisions
- external event snapshots where applicable

CRM systems, if used externally, own:
- external sales pipeline
- external marketing automation
- external CRM-specific workflows

CustomArr owns:
- STL customer master
- STL customer relationship CRM modules, including leads, opportunities, proposals, agreements, cases, activities, tasks, eligibility, onboarding, health, imports, merge review, and integration references
- customer requirements
- customer relationship context used by STL execution

============================================================
SOURCE OF TRUTH MAP
============================================================

Platform login:
- NexArr

Tenant:
- NexArr

Entitlement:
- NexArr

Internal people:
- StaffArr

Internal org structure:
- StaffArr

Internal locations:
- StaffArr

Product permissions/person authority:
- StaffArr, scoped by product

Training:
- TrainArr

Qualifications:
- TrainArr

Certifications:
- TrainArr, published to StaffArr

Assets:
- MaintainArr

Maintenance work:
- MaintainArr

Inspections:
- MaintainArr unless the inspection is specifically an assurance inspection owned by AssurArr

Transportation:
- RoutArr

Trips:
- RoutArr

Dispatch:
- RoutArr

Vendors:
- SupplyArr

Suppliers:
- SupplyArr

Tenant commercial items/parts/materials/SKUs:
- SupplyArr

Shared public identifiers, taxonomies, UOM, manufacturer identity, and crosswalks:
- ReferenceDataCore

Procurement context:
- SupplyArr

Inventory:
- LoadArr

Warehouse movement:
- LoadArr

Receiving:
- LoadArr

Customers:
- CustomArr

Customer contacts:
- CustomArr

Customer locations:
- CustomArr

Customer requirements:
- CustomArr, interpreted by Compliance Core when they become evidence/rule obligations

Customer leads:
- CustomArr

Customer opportunities:
- CustomArr

Customer proposals:
- CustomArr as CRM/commercial-intent snapshots; LedgArr owns invoice, payment, tax, and ledger truth once the commercial intent becomes financial execution

Customer agreements:
- CustomArr as customer relationship metadata and RecordArr/ContractArr references; RecordArr owns files and any future ContractArr owns full contract lifecycle

Customer cases:
- CustomArr for relationship/support cases; AssurArr owns quality cases, nonconformance, CAPA, and release decisions

Customer activities and tasks:
- CustomArr when they are relationship timeline/follow-up records; execution products own their own operational tasks

Customer eligibility, onboarding, health, imports, merge review, and integration references:
- CustomArr

Orders:
- OrdArr

Requests:
- OrdArr

Service requests:
- OrdArr unless a future ServiceArr exists

Documents:
- RecordArr

Records:
- RecordArr

Retention:
- RecordArr

Regulatory rules:
- Compliance Core

Evidence requirements:
- Compliance Core

Compliance interpretation:
- Compliance Core

Assurance cases:
- AssurArr

Nonconformance:
- AssurArr

CAPA:
- AssurArr

Quality holds as business decisions:
- AssurArr

Inventory holds as stock execution:
- LoadArr

Reports:
- ReportArr

Dashboards:
- ReportArr

Mobile execution UI:
- Field Companion

Financial execution:
- LedgArr

Payroll execution:
- External payroll system

Hardware capture:
- External hardware/vendor system

============================================================
COMMON CROSS-PRODUCT FLOWS
============================================================

Customer-requested work:
- CustomArr owns customer context.
- OrdArr owns the request/order.
- StaffArr validates people/authority.
- TrainArr validates qualification.
- MaintainArr executes maintenance work if needed.
- LoadArr executes inventory/warehouse work if needed.
- RoutArr executes transportation work if needed.
- SupplyArr supports vendor/item/procurement needs.
- Compliance Core determines rule/evidence requirements.
- RecordArr stores documents/evidence.
- AssurArr handles nonconformance/CAPA if something fails.
- ReportArr reports performance/status.
- LedgArr handles invoice, bill, payment, tax, and ledger execution.

Procurement:
- MaintainArr/LoadArr/OrdArr identifies need.
- SupplyArr owns vendor/item/procurement context.
- StaffArr validates approval authority.
- LoadArr receives and stocks physical goods.
- RecordArr stores procurement documents.
- Compliance Core checks evidence/rule requirements where applicable.
- LedgArr handles bill, payment, AP, and accounting execution.

Maintenance parts:
- MaintainArr creates parts demand.
- SupplyArr identifies item/vendor context.
- LoadArr reserves/issues/fulfills stock.
- MaintainArr records usage/installation.
- RecordArr stores supporting documents.
- LedgArr receives financial packets when cost, capitalization, or inventory valuation is needed.

Dispatch readiness:
- OrdArr states what needs to move.
- CustomArr provides customer requirements.
- RoutArr plans route/trip.
- StaffArr validates driver/person authority.
- TrainArr validates driver qualification.
- MaintainArr validates vehicle readiness.
- LoadArr validates load/inventory readiness.
- Compliance Core identifies blockers.
- RecordArr stores delivery/transport evidence.
- ReportArr reports performance.

Incident-to-retraining:
- Incident may originate in any product.
- StaffArr owns personnel history impact.
- Compliance Core identifies rule/evidence implications.
- TrainArr owns retraining assignment/completion.
- AssurArr owns CAPA/nonconformance if applicable.
- RecordArr stores evidence.
- ReportArr reports trends.

Nonconformance/CAPA:
- AssurArr owns the case.
- Originating product owns the failed operational record.
- Corrective work is executed by the appropriate product.
- StaffArr handles personnel implications.
- TrainArr handles retraining.
- Compliance Core handles rule/evidence interpretation.
- RecordArr stores evidence.
- ReportArr reports status/trends.

Document/evidence:
- Product creates or requests a document/record.
- RecordArr stores and controls it.
- Compliance Core classifies evidence meaning.
- Product references it.
- ReportArr includes it in reports.

Finance handoff:
- OrdArr creates invoice-ready packet.
- SupplyArr creates bill-ready/procurement packet.
- MaintainArr/LoadArr/RoutArr contribute operational completion details.
- RecordArr stores supporting documents.
- LedgArr ingests packets, resolves Financial Legal Entity/dimensions, previews postings, approves, posts, and exports when configured.
- External finance systems receive only LedgArr-approved exports and return external status snapshots.

============================================================
DEDICATED PRODUCT AVOIDANCE RULES
============================================================

No WorkflowArr initially:
- OrdArr owns order/request orchestration.
- Products own domain workflows.
- StaffArr owns authority.
- Compliance Core owns rule checks.
- Escalation can be coordinated through product events.

No NotificationArr initially:
- Products emit notification needs.
- NexArr/StaffArr provide identity/contact context.
- Shared infrastructure sends notifications.
- A dedicated product is only needed if notification preferences, templates, and delivery governance become large enough.

No FinanceArr:
- LedgArr is the dedicated finance product.
- Do not create a parallel FinanceArr or duplicate LedgArr financial ownership in operational products.
- External ERP/accounting systems remain bridge/export targets or tenant-selected external GL masters, not unmediated owners of STL financial packet/subledger/posting control.

No HardwareArr:
- Specialized hardware and vendors remain external.
- STL integrates, consumes, classifies, and orchestrates around hardware data.

No PayrollArr:
- Payroll systems own payroll execution.
- STL may provide labor/time/readiness context and handoff snapshots.

============================================================
NAMING SUMMARY
============================================================

NexArr:
- access to the suite

StaffArr:
- who and where internally

TrainArr:
- who is qualified

CustomArr:
- who the customer is

OrdArr:
- what was requested

SupplyArr:
- what items exist and who supplies them

LoadArr:
- where inventory is and how it moves

MaintainArr:
- what assets exist and how they are maintained

RoutArr:
- how transportation work is executed

Compliance Core:
- what rules apply and what evidence is required

RecordArr:
- where records and documents live

AssurArr:
- what failed, what was corrected, and whether it is acceptable

ReportArr:
- what the business knows across the suite

Field Companion:
- how humans execute work in the field

External systems:
- money, payroll, banking, tax, certified hardware, and specialized outside execution
