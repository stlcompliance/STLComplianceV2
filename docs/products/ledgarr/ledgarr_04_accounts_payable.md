# LedgArr 04 - Accounts Payable

LedgArr owns AP financial workflow. SupplyArr remains the source of truth for vendors, suppliers, item/procurement context, vendor terms, purchase orders, and vendor-facing operational workflows.

AP entities:

- VendorFinancialProfile
- VendorBill and VendorBillLine
- VendorBillApproval
- VendorBillMatch and VendorBillVariance
- VendorCredit
- APPayment and APPaymentLine
- PaymentRun and PaymentExportBatch
- APDispute
- APAgingSnapshot

Typical flow:

1. SupplyArr emits purchase_order_commitment or vendor_invoice financial packet.
2. LoadArr emits receiving_accrual or inventory_receipt_valuation packet when goods are received.
3. LedgArr resolves vendor financial profile, FinancialLegalEntity, accounts, dimensions, tax, and match rules.
4. VendorBill is created, matched, disputed, approved, or blocked for variance.
5. Approved VendorBill posts to AP and GL.
6. PaymentRun creates APPayment records and exports to banking or external ERP/accounting when configured.
7. AP aging and payment export status update from LedgArr records.

LedgArr must reject AP bill posting when required match variance is unresolved. Payment exports must not bypass LedgArr approval and posting controls.
