# How to transfer inventory between locations

## Audience
Warehouse users and inventory coordinators

## Product
LoadArr

## Support Status
Supported by current UI/API

## Purpose
Move inventory from one controlled location to another while preserving the stock ledger.

## Before You Start
- LoadArr owns transfers and stock movement.
- The source and destination locations must be valid for the item and tenant.

## Steps
1. Open LoadArr.
2. Open Transfers or Inventory.
3. Find the item, lot, serial, or location balance.
4. Choose the transfer action.
5. Select source location, destination location, quantity, and transfer type when requested.
6. Review reservations, holds, and lot/serial constraints.
7. Submit the transfer.
8. Review transfer audit or movement history to confirm completion.

## What Happens Next
LoadArr updates the stock ledger and inventory balances for both locations.

## Troubleshooting
- If transfer is blocked, check holds, reservations, insufficient quantity, or location policy.
- If the destination is a new physical place, verify StaffArr and LoadArr setup first.

