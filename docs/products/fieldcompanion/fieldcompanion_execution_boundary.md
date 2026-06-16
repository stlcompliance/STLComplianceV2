# Field Companion Execution Boundary

## Purpose

This document defines how Field Companion participates in scheduled work without becoming the canonical owner of product work records.

## Ownership

Field Companion owns:

- mobile task inbox surfaces
- guided execution screens
- photo, note, signature, and document capture UI
- offline queue state
- mobile interaction events

Field Companion does not own:

- work orders
- trips
- dock appointments
- training assignments
- quality checks
- orders
- inventory movements
- rule interpretation
- retained documents

## Assigned Work Flow

1. Owning product schedules work.
2. Owning product emits a scheduled event.
3. Field Companion displays assigned executable work from owning product APIs or projections.
4. Mobile user acknowledges, updates progress, captures notes/photos/signatures, or completes allowed actions.
5. Field Companion sends commands to the owning product API.
6. Owning product validates and writes.
7. Owning product emits canonical execution events.
8. RecordArr stores retained evidence and documents.
9. ReportArr consumes events/projections for reporting.

## Offline Rule

Offline actions remain pending until confirmed by the owning product.

Field Companion must reject or mark conflicts when the owning product rejects stale, unauthorized, or invalid offline changes.

## Event Boundary

Field Companion may emit interaction events such as:

- `fieldcompanion.task.viewed`
- `fieldcompanion.task.acknowledged`
- `fieldcompanion.task.progressUpdated`
- `fieldcompanion.task.photoCaptured`
- `fieldcompanion.task.noteCaptured`
- `fieldcompanion.task.signatureCaptured`
- `fieldcompanion.offlineChange.queued`
- `fieldcompanion.offlineChange.synced`
- `fieldcompanion.offlineChange.rejected`

These events do not replace owning product execution events.
