# How to Edit Asset Details

## Before you start
You need MaintainArr asset-edit permission and authority for the asset's scope.

## Steps
1. Open the asset detail page.
2. Select **Edit asset**; detail pages remain read-first until edit is intentional.
3. Change only MaintainArr-owned fields. Use owner-backed actions for StaffArr locations, RecordArr documents, LoadArr inventory, SupplyArr procurement, or other foreign truth.
4. Review downstream impacts when changing class/type, operating status, location, meter policy, or readiness-sensitive fields.
5. Provide a reason when the change affects identity, compliance, readiness, or history.
6. Save and wait for server confirmation.

## What happens next
The new version is visible with history/audit context. Readiness, PM, inspections, reservations, and dependent workflows are reevaluated when relevant.

## Troubleshooting
- Field is read-only: it is system-managed, history-protected, or owned by another product.
- Conflict: reload/compare the newer version; your unsaved changes should remain available for review.
- Missing reference: use the owner-backed picker or Quick Create.
