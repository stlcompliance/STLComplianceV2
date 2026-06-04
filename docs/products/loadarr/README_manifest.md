# LoadArr Granular End-Goal Markdown Package

This package defines LoadArr at the domain-object level.

## Files

- `loadarr_00_scope_and_boundaries.md`
- `loadarr_01_item_location_balance_model.md`
- `loadarr_02_receiving_putaway_model.md`
- `loadarr_03_reservation_pick_issue_transfer_model.md`
- `loadarr_04_counts_adjustments_discrepancy_model.md`
- `loadarr_05_workflows_status_events_apis.md`
- `loadarr_all_in_one_granular_featureset.md`

## Purpose

LoadArr owns WMS and inventory execution for STL Compliance / ARR:

- Inventory execution item view
- WMS behavior attached to StaffArr locations
- Inventory balances
- Stock ledger movements
- Expected receipts
- Receiving
- Putaway
- Reservations
- Picks
- Issues
- Returns
- Transfers
- Cycle counts
- Adjustments
- Inventory discrepancies
- Quarantine/hold movement behavior
- Availability responses to other products

LoadArr does not own canonical internal location identity, supplier master, procurement approval, purchase orders, maintenance work orders, customer order lifecycle, route/trip execution, document file truth, quality hold/release decisions, regulatory meaning, reporting read models, or accounting execution.
