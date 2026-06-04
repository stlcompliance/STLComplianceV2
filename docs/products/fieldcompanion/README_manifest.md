# Field Companion Granular End-Goal Markdown Package

This package defines Field Companion at the domain-object level.

## Files

- `field_companion_00_scope_and_boundaries.md`
- `field_companion_01_mobile_task_session_model.md`
- `field_companion_02_secure_upload_capture_model.md`
- `field_companion_03_offline_sync_device_model.md`
- `field_companion_04_product_surfaces_action_model.md`
- `field_companion_05_workflows_status_events_apis.md`
- `field_companion_all_in_one_granular_featureset.md`

## Purpose

Field Companion is the worker-facing mobile execution layer for STL Compliance / ARR.

It owns:

- Mobile task presentation
- Mobile task inbox
- Product switcher UX
- Secure no-login upload sessions
- Mobile capture UX
- Offline action queue
- Device profile/session context
- Barcode/QR scan UX
- Photo/signature/document capture UX
- Field-friendly action schemas

Field Companion does not own maintenance truth, training truth, inventory truth, route truth, document file truth, incident truth, quality truth, people truth, customer/order truth, regulatory meaning, reporting read models, or accounting execution.
