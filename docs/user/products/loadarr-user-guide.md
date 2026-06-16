# LoadArr User Guide

## What This Product Is For
LoadArr is for expected receipts, receiving workflow, dock receiving queue, putaway, inventory balances, stock ledger, warehouse tasks, reservations, picks, issues, returns, cycle counts, inventory adjustments, holds, quarantine status, lot tracking, serial tracking, bin stock, and inventory availability.

## Who Uses It
- warehouse receivers
- inventory leads
- warehouse managers
- maintenance parts coordinators

## Main Pages
- Work dashboard
- Expected Receipts
- Receiving
- Dock Schedule
- Putaway
- Inventory
- Transfers
- Reservations
- Picking
- Staging
- Shipping / Loadout
- Cycle Counts
- Exceptions
- Holds
- Unexplained
- Supply Coordination
- Purchase Order Receipts
- Vendor Returns
- Backorders
- Reorder Signals
- Warehouses & Areas
- Stock Ledger
- Receiving History
- Movement History
- Count History
- Adjustment History
- LoadArr Settings
- Integrations
- Permissions

## Main Records
- expected receipt
- receiving session
- dock schedule task
- putaway task
- inventory balance
- transfer
- reservation
- pick task
- staging assignment
- loadout
- cycle count
- hold
- stock ledger entry

## Common Workflows
- receive inbound goods
- complete putaway
- move inventory between locations
- hold or quarantine items
- release holds
- review stock ledger
- coordinate PO receipts and backorders

## Permissions Usually Needed
- loadarr.receiving.create
- loadarr.receiving.confirm
- loadarr.putaway.execute
- loadarr.inventory.read
- loadarr.inventory.hold
- loadarr.inventory.release
- loadarr.transfers.create
- loadarr.transfers.execute
- loadarr.exceptions.resolve

## Related Products
- StaffArr owns sites and locations.
- SupplyArr owns supplier/vendor and tenant commercial item/part/material/SKU context.
- ReferenceDataCore owns shared public identifiers, taxonomies, unit normalization, manufacturer identity, and crosswalks.
- MaintainArr consumes parts fulfillment status.
- RoutArr consumes load readiness.

## Common Troubleshooting
- [Product not visible](../troubleshooting/product-not-visible.md)
- [Missing permission](../troubleshooting/missing-permission.md)
- If a page is visible but an action is disabled, check the record status and your role or permission assignment.
- Remember: LoadArr does not own StaffArr location identity, vendor master data, item commercial ownership, purchase approvals, customer master data, dispatch execution, maintenance work orders, financial inventory valuation, shared public reference identity, or scanner hardware ownership.
