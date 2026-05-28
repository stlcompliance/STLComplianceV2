# W151 — Companion field task validation and plain denied reasons

**Milestone:** M11 (Companion)  
**Products:** NexArr (aggregate validation), companion-frontend  
**Status:** Complete

## Summary

Adds server-side validation for companion field submissions (acknowledge + evidence) against the aggregated field inbox, with plain-English denied/blocked reason messages. Offline sync and evidence upload enforce validation; the companion UI pre-validates before queueing or uploading.

## Backend (NexArr)

- `CompanionFieldTaskValidationService` — entitlement, task-key format, inbox membership, evidence support, blocked-task rules
- `CompanionDeniedReasonCatalog` — stable plain messages for validation and related API errors
- `POST /api/companion/field-tasks/validate` — preflight validation for the companion UI
- `CompanionOfflineSyncService` / `CompanionFieldEvidenceService` — call `EnsureAllowedAsync` before persisting or proxying

## Frontend (companion)

- `validateCompanionFieldTask` API client
- `companionPlainReason` — surfaces API `message` from error JSON in toasts and submission state
- Acknowledge queue and evidence upload call validate before submit

## Tests

- `NexArrCompanionFieldValidationTests` — validate endpoint, sync rejection, sync success with stub inbox
- `CompanionDeniedReasonCatalogTests`
- Updated companion offline/evidence/submission integration tests for inbox stub
- `companionPlainReason.test.ts` (vitest)

## Permissions

- Same as companion field inbox: NexArr JWT + companion or field-product entitlement (`RequireCompanionAccess`)

## DB

- None (validation reads aggregated inbox via product APIs; submission tables unchanged)

## Next gaps

- Push notification delivery (beyond browser permission readiness)
- Per-product field inbox rows still thin for some products (MaintainArr/RoutArr/SupplyArr deep execution)
- Extend evidence validation/proxy beyond TrainArr assignments
