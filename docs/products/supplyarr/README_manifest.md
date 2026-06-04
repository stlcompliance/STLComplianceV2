# SupplyArr Granular End-Goal Markdown Package

This package defines SupplyArr at the domain-object level.

## Files

- `supplyarr_00_scope_and_boundaries.md`
- `supplyarr_01_supplier_vendor_dealer_model.md`
- `supplyarr_02_sourcing_item_catalog_model.md`
- `supplyarr_03_purchase_request_order_model.md`
- `supplyarr_04_supplier_compliance_performance_model.md`
- `supplyarr_05_workflows_status_events_apis.md`
- `supplyarr_all_in_one_granular_featureset.md`

## Purpose

SupplyArr owns supplier/vendor/dealer management, sourcing, procurement workflow, purchase requests, purchase orders, supplier compliance status, supplier quality/performance summaries, and procurement-side item/supplier relationships for STL Compliance / ARR.

SupplyArr answers:

- Who can we buy from?
- Is this supplier approved, restricted, suspended, or blocked?
- What documents are required from this supplier?
- Which supplier can provide this item?
- What is the vendor item number, price snapshot, lead time, MOQ, and package quantity?
- Who requested this purchase?
- Is the purchase request approved?
- What purchase order was created?
- What is expected to be received by LoadArr?
- How did the supplier perform?

SupplyArr does not own inventory balances, stock ledger, receiving execution, canonical internal locations, work orders, order lifecycle, route execution, document file truth, quality hold/release decisions, regulatory meaning, reporting read models, or accounting execution.
