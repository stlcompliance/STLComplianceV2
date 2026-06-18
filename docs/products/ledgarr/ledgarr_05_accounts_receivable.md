# LedgArr 05 - Accounts Receivable

LedgArr owns AR financial workflow. CustomArr remains the customer source of truth, and OrdArr remains the order/request source of truth.

AR entities:

- CustomerFinancialProfile
- CustomerInvoice and CustomerInvoiceLine
- CustomerInvoiceApproval
- CustomerCreditMemo
- CustomerPayment
- CustomerPaymentApplication
- CustomerStatement
- CollectionStatus
- ARAgingSnapshot

Typical flow:

1. OrdArr emits customer_order_invoice_request, shipment_revenue, customer_invoice, customer_credit, or customer_payment packet.
2. CustomArr provides customer identity and authorized customer financial profile source data.
3. LedgArr resolves customer financial profile, FinancialLegalEntity, accounts, dimensions, tax, invoice terms, and approval requirements.
4. CustomerInvoice is created, approved, issued, and posted.
5. CustomerPayment is recorded and applied to open invoices.
6. AR aging, statement, collection status, and revenue reports update from LedgArr records.

LedgArr must reject issuing or posting when required tax, account, customer, or FinancialLegalEntity mapping is missing. OrdArr and CustomArr may display LedgArr invoice/payment read models, but they do not own AR ledger truth.
