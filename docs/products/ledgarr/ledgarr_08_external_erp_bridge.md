# LedgArr 08 - External ERP Bridge

LedgArr owns external ERP/accounting integration control. No external integration may bypass LedgArr validation, posting preview, approval, or posting rules.

Bridge modes:

1. STL Ledger Master - LedgArr is the official GL.
2. External GL Master - LedgArr manages packets, subledgers, previews, and validations, then exports to the external GL.
3. Export Only - LedgArr creates approved posting batches for manual import into systems such as QuickBooks, NetSuite, SAP, Oracle, Odoo, or another accounting platform.

External integration entities:

- ExternalFinanceSystem
- ExternalFinanceConnection
- ExternalAccountMapping
- ExternalDimensionMapping
- ExternalCustomerMapping
- ExternalVendorMapping
- ExternalItemMapping
- ExternalPostingBatch
- ExternalPostingResult
- ExternalSyncRun
- ExternalSyncIssue

Bridge requirements:

- generic provider abstraction first; do not hardcode one accounting provider as the only path
- account, dimension, FinancialLegalEntity, customer, vendor, and item mappings
- export approval and immutable export history
- retry handling and sync issue queue
- external reference IDs stored only in LedgArr integration tables
- rejection when posting batch contains unposted or failed journal entries
- external status snapshots clearly labeled as external
