# Part Request to Receiving

## Purpose
Move from maintenance parts need to procurement and warehouse receiving.

## Who Participates
- Technician or maintenance manager
- Buyer
- Warehouse receiver

## Starting Point
A work order needs parts.

## Main Steps
1. Add parts demand to the MaintainArr work order.
2. Publish demand when ready.
3. Create or review SupplyArr purchase request or vendor order.
4. Receive goods in LoadArr.
5. Put away or stage inventory.
6. Update work order supply readiness.

## Products Involved
- MaintainArr owns work order parts demand.
- SupplyArr owns procurement context.
- LoadArr owns inventory and receiving.
- RecordArr may store documents.

## Records Created or Updated
- parts demand line
- purchase request
- vendor order
- receiving session
- putaway task
- stock ledger entry

## Where Users May Get Stuck
- Part not in SupplyArr catalog.
- Purchase request not approved.
- Receiving exception or quarantine.
- Inventory not available for the work order.

## Related How-To Docs
- [How to request parts for a work order](../how-to/maintainarr/how-to-request-parts-for-a-work-order.md)
- [How to receive inbound goods](../how-to/loadarr/how-to-receive-inbound-goods.md)
