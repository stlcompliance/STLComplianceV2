# How to update asset readiness

## Audience
Maintenance supervisors and asset managers

## Product
MaintainArr

## Support Status
Supported by current UI/API with product-owned validation

## Purpose
Review or change whether an asset is ready for work, blocked, or requires maintenance follow-up.

## Before You Start
- MaintainArr owns asset readiness.
- RoutArr, LoadArr, and ReportArr may consume readiness, but they do not own it.

## Steps
1. Open MaintainArr.
2. Open Readiness or the asset detail page.
3. Find the asset.
4. Review open defects, inspections, recalls, PM due state, and work orders that affect readiness.
5. Use the readiness action available in the asset or readiness view when a manual update is permitted.
6. Enter the reason and effective context required by the page.
7. Save the readiness update.
8. Notify affected dispatch, warehouse, or operations teams when readiness changes.

## What Happens Next
MaintainArr records the readiness state and downstream products should use that state for dispatch, warehouse, or compliance decisions.

## Troubleshooting
- If readiness is blocked by an open defect or inspection, resolve the source record instead of overriding readiness.
- If another product shows stale readiness, verify the MaintainArr publication or integration state.

