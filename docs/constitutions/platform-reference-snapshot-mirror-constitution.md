# STL Compliance Reference, Snapshot, and Mirror Constitution

## 1. Purpose

This constitution defines how STL Compliance products reference records owned by other products, when labels may be cached, when snapshots are acceptable, and how mirrors remain read-only projections instead of duplicate masters.

## 2. Scope

This constitution applies to:

- Cross-product reference fields
- Controlled selects
- Search/select providers
- Cached display labels
- Snapshot fields
- Mirror tables
- Reference validation
- Archived, deleted, merged, and superseded references
- External ID mappings

## 3. Prime directive

A reference is not ownership.

A snapshot is not live truth.

A mirror is not a duplicate master.

Products must make the distinction visible in data models, APIs, and UI where it affects decisions.

## 4. Reference definition

A reference points to a canonical record owned by another product.

Required reference fields:

- Source product
- Entity type
- Stable source ID
- Display label or label snapshot
- Reference status when known
- Fetched or validated timestamp when used for decisions

Recommended shape:

```json
{
  "sourceProduct": "StaffArr",
  "entityType": "person",
  "id": "person_123",
  "displayNameSnapshot": "Marcus Hill",
  "statusSnapshot": "active",
  "snapshotAt": "2026-06-10T00:00:00Z"
}
```

## 5. Canonical reference owners

Use canonical source IDs from the owning product.

- Human references: StaffArr/NexArr `personId`
- Internal people/authority/location references: StaffArr
- Tenant and entitlement references: NexArr
- Training, certifications, qualifications: TrainArr
- Assets, components, defects, work orders, PMs, inspections: MaintainArr
- Routes, trips, stops, dispatch exceptions: RoutArr
- Vendors, suppliers, tenant commercial parts, items, materials, SKUs, supplier locations, procurement context: SupplyArr
- Shared public identifiers, taxonomies, UOM, manufacturer identity, and crosswalks: ReferenceDataCore
- Inventory, stock ledger, holds, reservations, picks, issues, receiving: LoadArr
- Customers, customer contacts, customer requirements: CustomArr
- Orders and requests: OrdArr
- Documents, files, record packages, versions, retention: RecordArr
- Rules, rulepacks, governing bodies, evidence requirements, applicability: Compliance Core
- Nonconformance, CAPA, assurance cases, release decisions: AssurArr
- Reports, report definitions, report snapshots, scheduled exports: ReportArr

## 6. Selected, not typed

Canonical references must be selected, searched, scanned, or resolved through approved APIs/reference providers.

Free-text names are allowed only for narratives, notes, descriptions, or non-canonical descriptive fields.

Examples:

- A work order references a StaffArr person, not typed mechanic name.
- A trip references a MaintainArr asset, not typed truck number alone.
- A purchase request references a SupplyArr item, not a typed part description alone.
- A compliance mapping references a Compliance Core governing body/citation, not free-text law names alone.

## 7. Display labels

Products may cache display labels for usability and history.

Cached display labels must not be treated as identifiers.

When a label may have changed, show one of:

- Current label fetched from source
- Snapshot label with snapshot time
- Archived/superseded label with warning
- Source unavailable state

## 8. Snapshot rule

A snapshot captures what was known at a point in time.

Snapshots are valid when:

- A historical decision must be preserved.
- A report must preserve generated criteria/results.
- External system status was known at a time.
- A record package needs audit history.
- A source product may later change a label/status but the original business context matters.

Snapshots must include:

- Source product
- Source entity type
- Source entity ID
- Snapshot values
- Snapshot time
- Snapshot reason when material

## 9. Mirror rule

A mirror is a local projection of selected source fields.

Mirrors are allowed for:

- Performance
- Search/filtering
- Dashboard summaries
- Offline support
- Reporting
- Integration staging
- Readiness panels
- Resilience when source is temporarily unavailable

Mirrors must be read-only from the consuming product's perspective unless the mirror owner is explicitly responsible for a local projection field.

A mirror must expose freshness.

## 10. Mirror fields

Recommended mirror metadata:

- Tenant ID
- Source product
- Source entity type
- Source entity ID
- Mirrored fields
- Source version or cursor when available
- Last source event ID
- Last refreshed time
- Freshness state
- Reconciliation state

## 11. Archived, deleted, superseded, and merged references

References must handle source lifecycle changes.

Possible reference states:

- `active`
- `inactive`
- `archived`
- `deleted`
- `superseded`
- `merged`
- `unknown`
- `source_unavailable`

Historical records should preserve old references when needed for audit, but current workflows must prevent unsafe use of invalid references.

## 12. External IDs

External IDs are mappings, not STL canonical IDs.

A SupplyArr vendor may map to a QuickBooks vendor ID.

A RoutArr trip may map to a carrier or telematics record.

A StaffArr person may map to payroll or HRIS.

External ID mappings must include:

- External system name
- External entity type
- External ID
- Direction of sync
- Last verified time
- Status

Do not use external IDs as primary STL references unless the external system is explicitly the system of record for that domain.

## 13. Reference validation

Before final submission, workflows with cross-product references must validate that required references:

- Exist
- Belong to the tenant
- Are usable for the intended action
- Are not archived/deleted unless historical use is allowed
- Meet permission rules
- Meet readiness/authority rules when relevant

Validation failures must be business-readable.

## 14. UI labeling

UI should label cross-product fields when ownership matters.

Examples:

- `Driver — StaffArr person`
- `Vehicle — MaintainArr asset`
- `Qualification — TrainArr certification`
- `Evidence requirement — Compliance Core`
- `Attachment — RecordArr document`
- `Inventory availability — LoadArr snapshot`

## 15. Read-only cross-product sections

A product may display another product's data, but if the current product does not own the action, it must provide a link or handoff to the owning product instead of pretending to edit locally.

## 16. Anti-patterns

The following are not allowed:

- Free-text canonical references
- Product-local duplicate person records
- Product-local duplicate location masters
- Copying another product's full record as editable local data
- Treating cached labels as source truth
- Showing snapshots as live current data
- Letting archived references silently pass current workflow validation
- Reusing external IDs as STL canonical IDs without explicit ownership decision
- Mirrors that mutate source-owned fields

## 17. Minimum acceptable implementation

A cross-product reference is minimally acceptable when it has:

1. Source product
2. Source entity type
3. Stable source ID
4. Display label or snapshot label
5. Source/freshness state when used for decisions
6. Validation through approved provider/API
7. Clear UI ownership labeling when displayed
8. Safe behavior for archived/superseded/source-unavailable states
