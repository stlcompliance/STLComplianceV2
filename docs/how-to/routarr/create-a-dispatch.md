# How to create a dispatch

## Audience
Dispatchers and transportation supervisors

## Product
RoutArr

## Support Status
Supported by current UI/API with intended validation gates

## Purpose
Create or prepare dispatch work while respecting driver, vehicle, inventory, and compliance readiness.

## Before You Start
- RoutArr owns dispatch plans, routes, trips, assignments, stop sequence, and transportation exceptions.
- StaffArr owns drivers as people, TrainArr owns qualifications, MaintainArr owns vehicle readiness, and LoadArr owns load readiness.

## Steps
1. Open RoutArr.
2. Open Dispatch board or Dispatch plans.
3. Create or select the dispatch plan.
4. Add the route, trip, or work request context available in the page.
5. Review readiness blockers for driver, vehicle, load, and compliance.
6. Assign driver, equipment, and stops when the page allows.
7. Resolve validation blockers before release.
8. Release or dispatch only when required readiness checks pass.

## What Happens Next
RoutArr records dispatch execution. Other products continue to own their readiness facts and source records.

## Troubleshooting
- If dispatch is blocked, open Validation blockers and follow the owning product link or context.
- Do not override driver qualification, vehicle readiness, or inventory readiness in RoutArr unless an explicit approved override exists.

