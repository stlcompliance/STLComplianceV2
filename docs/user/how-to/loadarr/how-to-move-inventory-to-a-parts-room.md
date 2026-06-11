# How to move inventory to a parts room

## Audience
Warehouse staff and maintenance parts coordinators.

## Purpose
Transfer inventory to a parts room or service location using LoadArr stock movement.

## Before You Start
- LoadArr access.
- Transfer create or execute permission.
- Source location, destination location, item, and quantity.
- Destination location defined in StaffArr and available in LoadArr setup.

## Steps
1. Open LoadArr.
2. Open **Work** > **Transfers**.
3. Create a transfer request if needed.
4. Choose source location.
5. Choose destination location, such as a parts room.
6. Choose item and quantity.
7. Save the transfer.
8. Execute the transfer when stock is physically moved.
9. Review **Movement History** or **Stock Ledger**.

## What Happens Next
LoadArr updates inventory movement and balances. MaintainArr can consume availability but does not own the stock ledger.

## Troubleshooting
- If destination is missing, check location setup.
- If stock is unavailable, check holds, reservations, and current balances.
- If transfer execution is disabled, check transfer status and permission.

## Related Docs
- [How to transfer inventory between locations](how-to-transfer-inventory-between-locations.md)
- [Parts not available](../../troubleshooting/parts-not-available.md)
