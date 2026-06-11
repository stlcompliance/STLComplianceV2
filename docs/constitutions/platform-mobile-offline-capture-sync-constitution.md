# STL Compliance Mobile, Offline, Capture, and Sync Constitution

## 1. Purpose

This constitution defines how STL Compliance supports real mobile work, offline capture, evidence capture, signatures, QR/code workflows, and sync without making Field Companion or local device storage the source of truth.

## 2. Scope

This constitution applies to:

- Field Companion
- Product mobile surfaces
- Offline drafts
- Mobile task inbox
- Photo capture
- Document capture
- Signature capture
- QR/barcode scanning
- Poor-connection workflows
- Sync queues
- Conflict handling
- Device-local storage
- Mobile notifications

## 3. Prime directive

Mobile is a first-class work surface, not just a viewer.

Field Companion is an execution surface, not a source-of-truth product.

Mobile actions write back to the owning product.

Offline actions are pending until confirmed by the owning product.

## 4. Field Companion ownership

Field Companion owns:

- Mobile task inbox surface
- Product switcher or entitled task surface
- Guided execution screens
- Photo/document/signature capture UI
- Offline-capable field action UI
- Push/in-app task surface

Field Companion does not own:

- Final operational records
- Training truth
- Maintenance truth
- Inventory truth
- Dispatch truth
- Compliance interpretation
- Document storage
- Certified hardware truth

## 5. Mobile action ownership

A mobile action must route to the owning product API.

Examples:

- Work order update → MaintainArr
- Training signoff → TrainArr
- Incident self-report → StaffArr or the owning incident intake flow
- Receiving action → LoadArr
- Trip update → RoutArr
- Evidence upload → product intake + RecordArr storage
- CAPA verification → AssurArr
- Document acknowledgment → RecordArr where controlled

## 6. Offline state

Offline records must be visibly marked.

Recommended states:

- `online`
- `offline_available`
- `offline_draft`
- `pending_sync`
- `syncing`
- `synced`
- `sync_failed`
- `conflict`
- `server_rejected`

Pending local actions must not be displayed as final, active, approved, dispatched, posted, issued, or completed until the owning product confirms.

## 7. Offline drafts

Offline drafts may exist for long workflows.

Offline drafts must preserve:

- Local draft ID
- Intended owning product
- Tenant
- Actor/person
- Captured fields
- Captured files pending upload
- Validation known locally
- Missing server validation warnings
- Created/updated times

Offline drafts must show that server-side validation is pending.

## 8. Sync queue

The mobile sync queue should preserve:

- Operation ID
- Tenant
- Owning product
- Target record/reference
- Operation type
- Idempotency key
- Local timestamp
- Payload summary
- Attachments pending upload
- Retry count
- Current sync state
- Last error

Sync must be idempotent.

## 9. Conflict handling

Conflicts must not silently overwrite source truth.

Conflict UI should explain:

- What changed locally
- What changed on the server
- Which fields conflict
- Which source owns the current truth
- Available actions: discard local, apply allowed changes, create note, submit for review, retry

High-risk conflicts should require review.

## 10. Capture rules

Photo, document, audio note, signature, and scan capture must include enough context to attach correctly.

Capture metadata should include:

- Tenant
- Captured by `personId`
- Captured time
- Device time and server receive time
- Owning product
- Target record/context
- Capture type
- Location metadata only when allowed/needed
- Upload/sync state

Files that become evidence or records must be stored through RecordArr.

## 11. Signature and signoff

Signatures and signoffs must be tied to:

- Person
- Role/authority where relevant
- Record
- Action being signed
- Timestamp
- Attestation text
- Device/session context where appropriate

Do not treat a scribble alone as an auditable signoff without identity and intent.

## 12. QR/barcode/code scanning

Scans must resolve to canonical records through owning-product APIs or reference providers.

Examples:

- Asset QR → MaintainArr asset
- Location QR → StaffArr location
- Inventory/bin/item scan → LoadArr/SupplyArr depending on object
- Document QR → RecordArr document
- Trip/load code → RoutArr/LoadArr/OrdArr depending on owner

Scans must not create free-text references.

## 13. Poor connection behavior

Mobile must show connection state when it affects work.

Critical safety/compliance actions should clearly show whether they are:

- Captured locally
- Pending sync
- Confirmed by server
- Rejected
- Requiring review

## 14. Device-local storage

Device-local storage must be minimized and protected.

Sensitive data should not be stored offline unless necessary.

Offline data should support:

- Expiration/cleanup
- Encryption where available
- Tenant separation
- User logout cleanup where appropriate
- Lost-device risk mitigation

## 15. Notifications and tasks

Mobile task surfaces should prioritize action needed, not product hierarchy.

Each task must still show or resolve:

- Owning product
- Record
- Required action
- Due/severity
- Offline capability
- Sync state after action

## 16. Certified hardware boundary

Mobile workflows must not pretend to replace ELDs, certified telematics, dedicated scanners, or other regulated/specialized hardware where those remain external systems.

Mobile may supplement, display, capture supporting evidence, and orchestrate around hardware data.

## 17. Anti-patterns

The following are not allowed:

- Showing pending offline action as confirmed
- Field Companion owning final operational records
- Device-local data as permanent source truth
- Silent conflict overwrite
- QR scans creating free-text references
- Signature capture without person/action/time context
- Evidence files stored outside RecordArr when retained
- Mobile pretending to replace certified hardware
- Tiny desktop controls copied onto mobile

## 18. Minimum acceptable implementation

A mobile/offline feature is minimally acceptable when it has:

1. Owning product for every action
2. Clear offline/pending/synced state
3. Idempotent sync operation
4. Conflict handling
5. RecordArr storage for retained evidence/files
6. Canonical reference resolution for scans
7. Device-local storage controls
8. Plain-language failure recovery
9. No false confirmation of unsynced actions
