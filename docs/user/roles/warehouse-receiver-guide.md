# Warehouse Receiver Guide

## What This Role Does
A warehouse receiver uses LoadArr to receive inbound goods, handle dock activity, stage and put away inventory, handle receiving exceptions, and keep stock movement accurate.

## What This Role Can Usually Access
- LoadArr Receiving, Dock Schedule, Putaway, Inventory, Transfers, Exceptions, Holds, and Records when permissions allow.
- Field Companion receiving tasks where enabled.

## What This Role Usually Cannot Access
- Does not own vendor or item commercial data; SupplyArr owns that.
- Does not own StaffArr location identity.
- Cannot approve purchase requests without separate SupplyArr permissions.

## Common Daily Tasks
- Start or open receiving sessions.
- Record received quantities and exceptions.
- Stage items and complete putaway tasks.
- Move inventory between locations.
- Place or release holds when permitted.

## Records This Role Works With
- expected receipt
- receiving session
- putaway task
- inventory balance
- transfer
- hold
- stock ledger entry

## Notifications This Role May Receive
- Dock schedule tasks
- receiving tasks
- exceptions
- hold or quarantine updates
- backorder or reorder signals

## Common Issues
- Receiving exception requires supervisor or quality follow-up.
- Putaway target location is missing or not allowed.
- Parts are not available for a maintenance request.

## Related How-To Documents
- [How to receive inbound goods](../how-to/loadarr/how-to-receive-inbound-goods.md)
- [How to put away inventory](../how-to/loadarr/how-to-put-away-inventory.md)
