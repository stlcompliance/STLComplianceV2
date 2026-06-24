# LoadArr — Production Safety, Warehouse Execution, and UI

## Audit mandate

Replace fixture-generated records, discarded writes, and every local-success fallback before release. LoadArr must never claim receiving, putaway, transfer, issue, count, adjustment, pick, stage, or ship completion until a durable transaction succeeds.

## Durable inventory core

Persist expected receipts, receipts/lines, handling units, putaway tasks, inventory ledger entries, balances, reservations, picks, staging, transfers, issues, cycle counts, adjustments, holds, exceptions, dock appointments, and idempotency records. Ledger and balance invariants are transactional and tested.

## Ownership

Use StaffArr location IDs, SupplyArr item/vendor/PO references, OrdArr fulfillment demand, RoutArr inbound/outbound visibility, AssurArr quality decisions, MaintainArr work-order demand, and RecordArr evidence. Keep snapshots only for display/audit.

## Navigation

- Inbound: Expected Receipts, Dock Schedule, Receiving, Putaway
- Inventory: Inventory, Transfers, Reservations, Cycle Counts
- Outbound: Picking, Staging, Shipping/Loadout
- Exceptions: Exceptions, Holds, Unexplained
- Administration: Settings, Integrations, Permissions

Parent groups carry counts and priority without exposing a 15-link wall.

## Execution UX

Barcode/scan and manual workflows share server state. Forms preserve work on failure, show conflict/short/over/damaged/held states, and use owner-backed quick create only where valid. Inventory adjustment can require four-eyes approval by tenant setting.

## Tests

Prove tenant isolation, ledger balance invariants, duplicate scans, idempotent retries, concurrent allocation, hold enforcement, restart/multi-replica behavior, and honest UI failure states.
