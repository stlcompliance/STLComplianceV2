# How to add stops to a route

## Audience
Dispatchers and route planners

## Product
RoutArr

## Support Status
Supported by current UI/API with route planner surface

## Purpose
Add pickup, delivery, dock, or service stops to a RoutArr route.

## Before You Start
- RoutArr owns stop sequence and transportation execution.
- Customer, order, inventory, and dock details may come from other owning products.

## Steps
1. Open RoutArr.
2. Open Routes, Stops, or Route planner.
3. Select the route.
4. Choose the stop add action when available.
5. Enter stop type, location, planned time window, contact or reference, and notes requested by the page.
6. Add pickup, delivery, dock appointment, or service details only when supported by the form.
7. Save the stop.
8. Adjust sequence in Route planner if allowed.
9. Review the route for validation blockers.

## What Happens Next
RoutArr records the stop and sequence. Source products remain the owners of customer, order, load, or location truth.

## Troubleshooting
- If the stop location is an internal site, verify StaffArr location setup.
- If a customer location is missing, use the customer-owning product when available.

