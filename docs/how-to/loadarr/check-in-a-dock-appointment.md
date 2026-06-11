# How to check in a dock appointment

## Audience
Warehouse receivers and dock coordinators

## Product
LoadArr

## Support Status
Intended workflow partially supported by current routes/docs

## Purpose
Connect an inbound appointment to receiving work at the dock.

## Before You Start
- LoadArr exposes Dock Schedule and receiving views.
- RoutArr owns transportation dispatch and dock appointment notifications.
- Exact dock check-in labels should be confirmed in the current UI.

## Steps
1. Open LoadArr.
2. Open Dock Schedule.
3. Find the appointment by carrier, route, vendor order, time, or reference.
4. Confirm the dock, arrival time, trailer or vehicle, and expected receipt link.
5. Use the check-in or arrival action if it is available.
6. Move the appointment into receiving or staging according to the page workflow.
7. Record exceptions if the appointment is early, late, missing paperwork, damaged, or does not match the expected receipt.
8. Continue with receiving once goods are ready to unload.

## What Happens Next
LoadArr owns the dock receiving state. RoutArr remains the owner of transportation trip or route status.

## Troubleshooting
- If no check-in action appears, use Dock Schedule for visibility and complete the receipt in Receiving.
- If the appointment came from RoutArr but is missing, check the RoutArr dock appointment notification and LoadArr integration state.

## Related How-To Documents
- [How to notify LoadArr about an inbound delivery](../routarr/notify-loadarr-about-an-inbound-delivery.md)

