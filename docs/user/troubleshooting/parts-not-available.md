# Parts are not available

## Symptoms
- Work order supply readiness shows shortage.
- LoadArr has no available stock.
- Purchase request or vendor order is still pending.

## Likely Causes
- No LoadArr stock.
- Stock is held, quarantined, reserved, or not put away.
- SupplyArr procurement is not approved or received.
- Part reference does not match SupplyArr or LoadArr.

## What to Check
1. Open MaintainArr work order parts demand.
2. Check SupplyArr part and purchase request.
3. Check LoadArr Inventory, Holds, and Receiving.
4. Review backorders.

## How to Fix
- Correct part reference.
- Approve or create purchase request if needed.
- Receive and put away goods in LoadArr.
- Release holds only after review.

## Who Can Help
Maintenance manager, buyer, or warehouse receiver.

## Related Docs
- [Part request to receiving workflow](../workflows/part-request-to-receiving.md)
