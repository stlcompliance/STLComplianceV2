# OrdArr - Scope, Ownership, and Boundaries

OrdArr is the operational order and request orchestration system.

OrdArr explains why work is happening. Execution products own how the work is performed.

## OrdArr answers

- What did the customer or internal requester ask for?
- Which customer, location, contact, or internal requester is tied to the work?
- Which products need to act on the request?
- What is the current order/request lifecycle state?
- Which handoffs are accepted, blocked, completed, or waiting?
- Is the order ready for operational closeout?
- Is an invoice-ready or bill-ready packet ready for the external finance system?

## OrdArr owns

- Customer orders
- Internal requests
- Service requests when no dedicated ServiceArr exists
- Work requests
- Request intake
- Request triage
- Order/request lifecycle
- Order/request status
- Parent business object tying work together
- Request-to-work conversion
- Order-to-LoadArr handoff
- Order-to-RoutArr handoff
- Order-to-MaintainArr handoff
- Order-to-SupplyArr handoff
- Completion packet coordination
- Invoice-ready packets
- Bill-ready packets
- Operational closeout
- Customer-facing request status
- Order exception coordination
- Order audit trail

## OrdArr does not own

- Customer master records
- Customer contacts or customer locations
- Inventory execution
- Warehouse movement
- Dispatch execution
- Maintenance execution
- Training execution
- Supplier/vendor master records
- Item commercial ownership
- Stored files or document retention
- Compliance rule interpretation
- Assurance/CAPA decisions
- Accounting execution
- Invoices, bills, payments, tax, general ledger, or bank reconciliation
- Report definitions or analytics read models

## Boundary rules

1. CustomArr owns who the customer is.
2. OrdArr owns what the customer or internal requester asked for.
3. Execution products own domain work after handoff.
4. OrdArr may coordinate status across products, but it must not take ownership of execution records.
5. OrdArr may prepare financial handoff packets, but external finance systems own financial execution.
6. RecordArr stores completion packets and supporting files.
7. Compliance Core owns evidence requirements and regulatory meaning.
8. ReportArr reports order performance; corrections happen in the owning product.
9. Field Companion may surface OrdArr tasks or status, but it is not the order source of truth.

## Standard OrdArr object envelope

Every major OrdArr object should include:

- tenantId
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- status
- lifecycleCategory
- sourceProduct
- correlationId
- auditTrailRef

## OrdArr object prefixes

Object prefixes are product-scoped. A globally meaningful reference must include `productKey`, `objectType`, and stable ID.

Suggested OrdArr prefixes:

- ORD - Order
- REQ - Request
- OIT - Order item
- OEX - Order exception
- OHO - Order handoff
- OCP - Completion packet
- IFP - Invoice-ready packet
- BFP - Bill-ready packet
- OTL - Order timeline entry

## Cross-product dependencies

CustomArr:

- customer account refs
- customer contact refs
- customer location refs
- customer requirements and eligibility checks

StaffArr:

- internal requester person refs
- owner, reviewer, approver, and team refs
- internal location refs
- authority context

SupplyArr:

- item, supplier, vendor, procurement, purchase request, and purchase order context

LoadArr:

- fulfillment status
- inventory availability
- reservation, pick, issue, receiving, and warehouse status

RoutArr:

- route, trip, stop, proof, transportation exception, and ETA status

MaintainArr:

- work order, asset, inspection, defect, and maintenance status

Compliance Core:

- rule/evidence requirements
- compliance blockers and requirement evaluations

RecordArr:

- stored files
- completion packet records
- evidence and retention

AssurArr:

- nonconformance, quality hold/release, CAPA, and customer complaint quality context

ReportArr:

- reporting read models and KPI views

NexArr:

- identity, active tenant membership, and session context, product launch, handoff trust, service clients, and service tokens

## Common relationships

Customer-requested work:

- CustomArr owns customer context.
- OrdArr owns the request/order.
- Execution products own accepted work.
- RecordArr stores supporting records.
- Compliance Core identifies evidence requirements.
- External finance system executes billing or payment.

Internal work request:

- StaffArr owns requester and internal location context.
- OrdArr owns request intake and triage.
- Target product owns accepted work.

Financial handoff:

- OrdArr assembles operational completion details.
- RecordArr stores supporting documents.
- External finance system creates invoices, bills, payments, tax, and ledger entries.

## MVP scope in this repo

The current implementation covers:

- Order creation from the workspace
- Order lines
- Status timeline
- Holds and approvals
- Handoffs to downstream products
- Completion / finance packet coordination
- Basic return / RMA records
- Dashboard and report summary projections

Deferred until a later phase:

- Quote versioning
- Customer portal order submission workflows
- AI-assisted intake
- Advanced pricing and discount governance
- Full return/exchange automation
- Persistent database state
