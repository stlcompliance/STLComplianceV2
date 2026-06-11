# How to create a route

## Audience
Dispatchers and route planners

## Product
RoutArr

## Support Status
Supported by current UI/API with route planner surface

## Purpose
Create the transportation route that organizes stops and trip execution.

## Before You Start
- RoutArr owns routes, stop sequence, and dispatch execution.
- Customer context belongs to CustomArr when available, and order/request context belongs to OrdArr when available.

## Steps
1. Open RoutArr.
2. Open Routes or Route planner.
3. Choose Create if the create action is available.
4. Enter route name, service date, origin, destination, and operating notes requested by the page.
5. Add or import stops when available.
6. Review validation blockers and readiness context.
7. Save the route.
8. Use Dispatch board or Trips to plan assignment and execution.

## What Happens Next
RoutArr owns the route and its execution state. Related products own their source records.

## Troubleshooting
- If customer or order data is missing, correct it in the owning product rather than creating a local RoutArr customer truth.
- If the route cannot dispatch, review validation blockers.

