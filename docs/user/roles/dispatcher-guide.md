# Dispatcher Guide

## What This Role Does
A dispatcher creates and manages trips, routes, dispatch plans, driver and vehicle assignments, exceptions, proof review, dock appointments, and load visibility in RoutArr.

## What This Role Can Usually Access
- RoutArr Dispatch board, Dispatch plans, Route planner, Trips, Routes, Stops, Exceptions, Proof review, Dock appointments, Load visibility, Availability, and Calendar.
- Driver assignment actions for roles such as routarr_dispatcher, routarr_manager, routarr_admin, or tenant_admin.

## What This Role Usually Cannot Access
- Does not own driver person records or driver qualifications.
- Does not own vehicle maintenance readiness or warehouse load readiness.
- Does not own customer master records or financial freight billing.

## Common Daily Tasks
- Create trips and routes.
- Assign drivers and equipment.
- Review validation blockers.
- Update trip status and handle delays.
- Review proof of pickup or delivery.

## Records This Role Works With
- trip
- route
- stop
- dispatch plan
- driver assignment
- vehicle assignment
- proof record
- dock appointment

## Notifications This Role May Receive
- Trip blockers
- dispatch exceptions
- proof needing review
- dock appointment or load visibility updates

## Common Issues
- Driver assignment is blocked by missing driver readiness or qualification.
- Equipment assignment is blocked by maintenance readiness.
- Load visibility depends on LoadArr readiness.

## Related How-To Documents
- [How to create a dispatch](../how-to/routarr/how-to-create-a-dispatch.md)
- [How to assign a driver](../how-to/routarr/how-to-assign-a-driver.md)
- [How to handle delays or missed stops](../how-to/routarr/how-to-handle-delays-or-missed-stops.md)
