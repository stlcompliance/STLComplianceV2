# LedgArr 06 - Inventory Valuation

LedgArr owns inventory financial valuation. LoadArr owns physical inventory, WMS execution, receiving, putaway, reservations, picks, issues, transfers, counts, adjustments, operational stock ledger, and inventory balance truth.

Valuation entities:

- InventoryValuationProfile
- ItemCostProfile
- InventoryCostLayer
- InventoryValuationMovement
- InventoryValuationAdjustment
- LandedCostAllocation and LandedCostAllocationLine
- InventorySubledgerBalance
- COGSPostingRun
- InventoryReconciliationRun

Supported cost methods should include:

- weighted average
- FIFO
- standard cost with variance

Typical flow:

1. LoadArr completes receiving, adjustment, transfer, scrap, correction, or shipment movement.
2. LoadArr emits inventory valuation packet with immutable movement snapshot.
3. LedgArr resolves item cost profile, FinancialLegalEntity, account mapping, dimensions, and cost method.
4. LedgArr creates cost layers or adjusts valuation.
5. Shipment or issue events post COGS when required.
6. Reconciliation compares LoadArr operational quantity to LedgArr financial subledger value.

LedgArr must not mutate LoadArr inventory balances. LoadArr must not calculate financial book value as source truth.
