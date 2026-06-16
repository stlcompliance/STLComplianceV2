# STL Compliance V2 Audit Packet Standards

## Purpose

This document defines standard evidence/audit package types across STL Compliance.

RecordArr owns stored records, packages, manifests, retention, legal holds, and controlled document lifecycle.

Compliance Core owns evidence requirements and regulatory meaning.

Products own operational records and decisions.

## Package standard

Every audit/evidence package should include:

```text
- package type
- package owner
- source product refs
- source object refs
- record refs
- requirement/evidence refs where applicable
- status snapshot
- missing evidence list
- override list
- reviewer/signoff history
- generated manifest
- generated export artifact where needed
- retention policy
- access history
```

## Standard package lifecycle

```text
draft
assembling
review_required
complete
locked
exported
archived
expired
failed
canceled
```

## Manifest entry standard

```text
PackageManifestEntry
- entryType
  - source_record
  - evidence_file
  - compliance_requirement
  - evaluation
  - blocker
  - override
  - signature
  - external_submission
  - generated_report
  - note
- sourceProduct
- sourceObjectRef
- recordRef
- displayName
- statusSnapshot
- timestamp
- checksum
```

## Standard package types

| Package | RecordArr owns | Source products |
|---|---|---|
| DQF/person qualification | package/files/manifest | StaffArr, TrainArr, Compliance Core |
| Asset maintenance | package/files/manifest | MaintainArr, LoadArr, SupplyArr, RecordArr, Compliance Core |
| Training completion | package/files/manifest | TrainArr, StaffArr |
| Supplier compliance | package/files/manifest | SupplyArr, AssurArr, Compliance Core |
| Customer requirements | package/files/manifest | CustomArr, OrdArr, Compliance Core |
| Order completion | package/files/manifest | OrdArr plus execution products |
| Receiving | package/files/manifest | LoadArr, SupplyArr, AssurArr |
| Quality release | package/files/manifest | AssurArr plus affected products |
| Integration/export | retained package | owning product plus external mapping |
| Auditor access | package and access log | RecordArr plus external portal |

## DQF/person qualification package

Includes:

```text
- person snapshot
- role/position snapshot
- training assignments
- certificates/qualifications
- remediation records
- related incidents
- external refs where modeled and allowed
- evidence files
- missing requirement list
- override records
```

## Asset maintenance package

Includes:

```text
- asset snapshot
- component snapshot
- work orders
- inspection results
- defects
- PM occurrences
- parts usage
- downtime
- return-to-service decision
- photos/documents
- vendor repair documents
- missing evidence
- readiness blockers and overrides
```

## Order completion package

Includes:

```text
- order snapshot
- request/intake data
- handoff statuses
- fulfillment/pick/issue records from LoadArr
- dispatch/trip/proof records from RoutArr
- maintenance/service work from MaintainArr where applicable
- procurement context from SupplyArr where applicable
- quality holds/releases from AssurArr
- customer requirement checks from CustomArr
- completion evidence
- invoice-ready/bill-ready packet refs
```

## Receiving package

Includes:

```text
- purchase order snapshot
- receipt
- dock appointment where available
- staged/putaway movements
- discrepancies
- quarantine/hold records
- photos/documents
- external packing slips
- accepted/rejected quantities
- stock ledger refs
```

## Quality release package

Includes:

```text
- nonconformance
- hold record
- affected objects
- investigation notes
- CAPA actions
- verification evidence
- release approval
- override records if any
- downstream unblock events
```

## Missing evidence behavior

Missing evidence should be explicit.

```text
EvidenceStatus
- satisfied
- missing
- expired
- invalid
- not_applicable
- unknown
- review_required
```

A package may be complete with warnings only if the package type and owning product allow it.

## Locking behavior

Locked packages should preserve the snapshot used for audit/export.

Source records may continue changing after lock, but the package should show what was included at lock time.

## ReportArr usage

ReportArr may show package completeness, missing evidence, age, risk, and trends.

ReportArr must not correct package contents directly.

Corrections happen in the owning product or RecordArr package workflow.
