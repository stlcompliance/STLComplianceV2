# How to create an asset

## Audience
Maintenance admins and asset managers

## Product
MaintainArr

## Support Status
Supported by current UI/API

## Purpose
Create the MaintainArr-owned asset record used for maintenance, inspections, defects, and readiness.

## Before You Start
- MaintainArr owns asset master records and readiness.
- StaffArr owns internal location references.
- SupplyArr owns parts and item context.

## Steps
1. Open MaintainArr.
2. Open Assets.
3. Choose Create.
4. Enter the asset identity, type, operating status, location reference, and any required ownership or readiness context.
5. Add hierarchy or component context when the page asks for it.
6. Save the asset.
7. Open the asset detail to confirm readiness, history, defects, work orders, inspections, documents, and related records.

## What Happens Next
The asset becomes the source record for maintenance execution and readiness checks used by RoutArr, LoadArr, ReportArr, and compliance workflows.

## Troubleshooting
- If the location is missing, create or correct it in StaffArr.
- If parts are missing, create or correct item/part context in SupplyArr.

