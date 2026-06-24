# RecordArr — Scope, Ownership, and Boundaries

## Product purpose

RecordArr is the document, evidence, controlled record, retention, OCR, scan-processing, and audit package system for the STL Compliance / ARR suite.

RecordArr is not just file upload. It is the record/evidence authority that gives every product a stable place to store, classify, version, retain, and package evidence.

RecordArr answers:

- What record exists?
- What file versions are attached?
- What source product/object created or uses it?
- What document type is it?
- What classification applies?
- What OCR or extracted metadata exists?
- Is this record active, rejected, superseded, archived, expired, purged, or on hold?
- What retention policy applies?
- What legal hold applies?
- What evidence mappings exist?
- What package contains this record?
- Can this record be shared, exported, or used as evidence?

## RecordArr owns

```text
- Record identity
- File metadata
- File storage reference
- Document versioning
- Document classification
- Document scan processing state
- Image edge/crop metadata
- OCR results
- Extracted fields
- Controlled document lifecycle
- Document approval workflow references
- Evidence mapping records
- Evidence package assembly
- Record package manifest
- Retention policy execution state
- Legal hold
- Record access policy
- Secure upload sessions where file persistence is involved
- Record audit trail
- Record export/package generation
```

## RecordArr does not own

```text
- Platform login
- Platform identity, active tenant membership, and session lifecycle
- Person master
- Product permissions
- Training assignment completion truth
- Certificate issuance truth
- Regulatory/rulepack meaning
- Asset truth
- Work order truth
- Inventory balance
- Stock ledger
- Receiving truth
- Supplier/vendor master
- Purchase order truth
- Route/trip execution
- Customer master
- Order lifecycle
- Quality hold/release decision
- Reporting read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product registry, launch context, and operational availability
- Login/handoff
- Service tokens
- Platform audit context

StaffArr
- Person references
- Owner/reviewer/approver references
- Site/location context
- Permission checks
- Personnel records/person audit packages

TrainArr
- Training evidence
- Certificate records
- Signoff records
- Qualification evidence packages

Compliance Core
- Evidence type definitions
- Evidence requirements
- Retention rule definitions
- Evidence mapping suggestions/confirmations
- Compliance evaluations

MaintainArr
- Asset documents
- Manuals
- Inspection records
- Work order photos
- Repair evidence
- Return-to-service evidence
- Defect evidence

LoadArr
- BOLs
- Packing slips
- Receiving photos
- Count evidence
- Adjustment evidence
- Inventory discrepancy evidence

SupplyArr
- Supplier documents
- Vendor contracts
- Insurance certificates
- PO documents
- Supplier corrective action responses

RoutArr
- Proof of delivery
- Proof of pickup
- BOL/POD photos
- Delivery signatures
- Route exception evidence

CustomArr
- Customer documents
- Customer signatures
- Customer complaint documents
- Customer requirement evidence

OrdArr
- Order documents
- Fulfillment proof
- Customer acceptance packages
- Closure packages

AssurArr
- Nonconformance evidence
- CAPA evidence
- Quality audit records
- Hold/release evidence
- Supplier/customer quality case records

ReportArr
- Generated reports
- Audit exports
- Scheduled report outputs

Field Companion
- Mobile uploads
- Document scans
- Photo capture
- Signature capture
- No-login secure upload flows
```

## Core source-of-truth rules

```text
1. RecordArr owns file/document/record truth.
2. Products own the operational event that caused the record.
3. Compliance Core owns whether a record satisfies a requirement.
4. RecordArr stores evidence mappings but Compliance Core owns requirement meaning.
5. RecordArr owns document versions and controlled document lifecycle.
6. RecordArr owns retention/legal hold execution state.
7. RecordArr must not decide asset readiness, inventory availability, route completion, training completion, quality release, or order closure.
8. RecordArr can package evidence from many products without becoming the source of those products' facts.
9. RecordArr should expose stable RecordRefs to products.
10. Products should not store raw files independently when the record should be controlled/evidence-bearing.
```

## Standard RecordArr object envelope

```text
RecordArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- sourceProduct
- sourceObjectRef
- classification
- ownerPersonId
- recordRefs
- fileRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- archivedAt
- purgedAt
- auditTrail
- eventLog
```

## RecordArr object prefixes

```text
REC    Record
FILE   File object
VER    Document version
DOC    Controlled document
SCAN   Document scan processing
OCR    OCR result
EXT    Extraction result
MAP    Evidence mapping
PKG    Record package
RET    Retention policy
DISP   Disposal review
HOLD   Legal hold
ACC    Access policy
UPL    Upload session
EXP    Export job
MAN    Package manifest
```

## Standard RecordRef

Other products should reference RecordArr records using a structured reference.

```text
RecordRef
- recordarrRecordId
- recordNumberSnapshot
- titleSnapshot
- recordTypeSnapshot
- documentTypeSnapshot
- statusSnapshot
- classificationSnapshot
- versionSnapshot
- expiresAtSnapshot
- retentionStatusSnapshot
- lastResolvedAt
```

## Standard source object reference

```text
SourceObjectRef
- productKey
- objectType
- objectId
- humanReadableNumber
- displayNameSnapshot
- statusSnapshot
- lastResolvedAt
```

## Standard record classification

```text
Classification
- public
- internal
- confidential
- restricted
- legal_hold
```

## Standard record lifecycle groups

```text
- draft
- processing
- active
- rejected
- review
- approved
- effective
- superseded
- expired
- archived
- purged
```
