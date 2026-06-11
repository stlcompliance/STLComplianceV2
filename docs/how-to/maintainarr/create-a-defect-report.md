# How to create a defect report

## Audience
Technicians, operators, supervisors, and maintenance admins

## Product
MaintainArr

## Support Status
Supported by current UI/API

## Purpose
Record a defect that may affect asset condition, readiness, inspection status, or maintenance work.

## Before You Start
- MaintainArr owns defects and asset readiness impact.
- Field Companion may provide a mobile surface, but MaintainArr owns the defect record.

## Steps
1. Open MaintainArr.
2. Open Defects.
3. Choose Create.
4. Select the affected asset.
5. Describe the defect, severity, discovery source, and operating impact.
6. Add related inspection, route, incident, or field task references when applicable.
7. Set readiness or restriction impact if the page asks for it.
8. Save the defect report.
9. Create or link a work order when repair or inspection follow-up is needed.

## What Happens Next
MaintainArr updates defect history and may affect asset readiness. Related products should consume readiness rather than creating their own asset condition truth.

## Troubleshooting
- If the defect came from the field, verify the Field Companion task synced back to MaintainArr.
- If a defect should block dispatch, confirm MaintainArr readiness is visible to RoutArr.

