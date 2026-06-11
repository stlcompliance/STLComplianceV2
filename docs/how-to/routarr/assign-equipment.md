# How to assign equipment

## Audience
Dispatchers and transportation supervisors

## Product
RoutArr

## Support Status
Supported by current UI/API with readiness checks

## Purpose
Assign a MaintainArr asset or vehicle to a trip or route.

## Before You Start
- MaintainArr owns vehicle or equipment readiness.
- RoutArr owns the transportation assignment.
- LoadArr may own load readiness for goods being moved.

## Steps
1. Open RoutArr.
2. Open Trips, Routes, Dispatch board, or Route planner.
3. Select the trip or route.
4. Open the equipment assignment area.
5. Choose the equipment or vehicle assignment action when available.
6. Select the asset.
7. Review readiness, open defects, inspection status, and validation blockers.
8. Save the assignment.
9. Confirm the route or trip reflects the assigned equipment.

## What Happens Next
RoutArr records the assignment and should use MaintainArr readiness as the source of truth.

## Troubleshooting
- If the asset is missing or readiness is wrong, correct it in MaintainArr.
- If a load is not ready, check LoadArr before dispatching.

## Related How-To Documents
- [How to update asset readiness](../maintainarr/update-asset-readiness.md)

