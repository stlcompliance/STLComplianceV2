# How to notify LoadArr about an inbound delivery

## Audience
Dispatchers, dock coordinators, and warehouse supervisors

## Product
RoutArr

## Support Status
Supported by current UI/API with intended dock handoff

## Purpose
Let LoadArr know an inbound route or trip is expected at a dock.

## Before You Start
- RoutArr owns the trip, route, and dock appointment notification.
- LoadArr owns dock receiving queue, expected receipts, and receiving execution.

## Steps
1. Open RoutArr.
2. Open Dock appointments or the trip detail.
3. Select or create the inbound dock appointment when available.
4. Confirm carrier, route/trip, planned arrival, dock, load, and receiving references.
5. Save or publish the appointment.
6. Open LoadArr Dock Schedule or Expected Receipts to confirm the appointment or receiving context appears.
7. Update RoutArr if arrival timing changes.
8. Complete receiving in LoadArr when goods arrive.

## What Happens Next
RoutArr communicates transportation arrival context. LoadArr controls receiving and inventory state.

## Troubleshooting
- If LoadArr does not show the appointment, check the dock appointment handoff and location reference.
- Do not mark inventory received in RoutArr.

## Related How-To Documents
- [How to check in a dock appointment](../loadarr/check-in-a-dock-appointment.md)
- [How to receive inbound goods](../loadarr/receive-inbound-goods.md)

