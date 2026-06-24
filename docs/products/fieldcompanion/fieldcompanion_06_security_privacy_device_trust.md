# Field Companion — Security, Privacy, and Device Trust

## Audit mandate

Correct the NexArr `userId` versus StaffArr `personId` boundary. Protect browser credentials and minimize offline geolocation, clock, note, and task data. Shared Permissions Policy must allow product-required camera/geolocation capabilities without granting them broadly.

## Device/session model

Register device/session context, bind queued work to tenant/person/device/sequence, support revocation, and clear sensitive state on logout, tenant switch, account disable, or device removal.

## Offline storage

Store only minimum data. Use protected IndexedDB/encrypted envelopes where the browser threat model permits; never treat local timestamps, location, signatures, or photos as trusted final evidence. Show pending, synced, conflicted, rejected, and expired states.

## Identity

Resolve StaffArr person identity through an authoritative link. Never issue account ID as person ID. Audit records preserve account, person, device, asserted time/location, server receipt, and validation outcome separately.

## Product action surfaces

Field Companion remains an execution surface. Source products own task/work status. Every offline mutation is idempotent, conflict-aware, and confirmed by the owner before final success.
