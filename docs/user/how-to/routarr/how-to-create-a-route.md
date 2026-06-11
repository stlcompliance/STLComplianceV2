# How to create a route

## Audience
Dispatchers and route planners.

## Purpose
Create a RoutArr route for trip execution.

## Before You Start
- RoutArr access.
- Route create access.
- Route name, stops, timing, and related trip if known.

## Steps
1. Open RoutArr.
2. Open **Routes** and select **Create**.
3. Enter route details shown by the form.
4. Add stops or link stops where available.
5. Review sequence and timing.
6. Save the route.
7. Open **Route planner** to review planning context.

## What Happens Next
RoutArr owns the route and stop sequence. Trips may use the route for dispatch execution.

## Troubleshooting
- If **Create** is missing, check routarr.routes.create access.
- If stops are missing, create or add stops first.
- If route cannot dispatch, check validation blockers and trip readiness.

## Related Docs
- [How to add stops](how-to-add-stops.md)
- [Dispatch to completion workflow](../../workflows/dispatch-to-completion.md)
