# PO to Putaway

## Purpose
Receive purchased goods and move them into usable inventory.

## Who Participates
- Buyer
- Warehouse receiver
- Warehouse supervisor

## Starting Point
A purchase order or vendor order is expected.

## Main Steps
1. Review the SupplyArr vendor order or purchase context.
2. Open LoadArr expected receipt or purchase order receipt.
3. Receive inbound goods.
4. Create exceptions for mismatch, damage, or quarantine.
5. Complete putaway tasks.
6. Review stock ledger and balances.

## Products Involved
- SupplyArr owns procurement context.
- LoadArr owns physical receiving and stock movement.
- StaffArr owns locations.
- RecordArr may store procurement documents.

## Records Created or Updated
- vendor order
- expected receipt
- receiving session
- receiving exception
- putaway task
- stock ledger entry

## Where Users May Get Stuck
- Expected receipt missing.
- Location missing.
- Quarantine or hold unresolved.
- Quantity mismatch needs approval.

## Related How-To Docs
- [How to create a purchase order](../how-to/supplyarr/how-to-create-a-purchase-order.md)
- [How to put away inventory](../how-to/loadarr/how-to-put-away-inventory.md)
