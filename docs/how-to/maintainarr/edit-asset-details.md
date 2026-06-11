# How to edit asset details

## Audience
Maintenance admins and asset managers

## Product
MaintainArr

## Support Status
Supported by current UI/API

## Purpose
Update asset information while keeping maintenance history intact.

## Before You Start
- You need permission to manage MaintainArr assets.
- Do not use asset edits to change StaffArr location ownership or LoadArr inventory truth.

## Steps
1. Open MaintainArr.
2. Open Assets.
3. Find and open the asset.
4. Choose the edit action or open the asset edit route.
5. Update the fields that MaintainArr owns, such as asset attributes, condition, hierarchy, readiness context, or maintenance metadata.
6. Save the changes.
7. Review asset detail and history after saving.
8. Notify affected product owners if the change affects dispatch, warehouse, compliance, or reporting workflows.

## What Happens Next
MaintainArr records the asset change and downstream products should consume the updated asset/readiness information through approved integrations.

## Troubleshooting
- If a field belongs to StaffArr, SupplyArr, or LoadArr, update it in the owning product.
- If readiness does not change as expected, review open defects, inspections, PM state, and work orders.

