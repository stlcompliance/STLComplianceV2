# LedgArr Documentation Manifest

This documentation set defines LedgArr as the STL Compliance finance product and preserves product ownership boundaries.

Files:

- `ledgarr_00_scope_and_boundaries.md` - product scope, non-ownership rules, cross-product references, UI safety
- `ledgarr_01_financial_core_model.md` - tenant financial profile, FinancialLegalEntity, fiscal core, close/lock rules
- `ledgarr_02_chart_of_accounts_dimensions_periods.md` - COA, GL accounts, dimensions, and fiscal period controls
- `ledgarr_03_financial_packet_and_posting_engine.md` - packet contract, lifecycle, posting engine, journal immutability
- `ledgarr_04_accounts_payable.md` - AP bill, match, approval, posting, payment, and aging ownership
- `ledgarr_05_accounts_receivable.md` - AR invoice, issue, payment, application, statements, and aging ownership
- `ledgarr_06_inventory_valuation.md` - inventory valuation separate from LoadArr WMS execution
- `ledgarr_07_fixed_assets_project_costing_budgets.md` - fixed assets, depreciation, project/job cost, budgets, and budget checks
- `ledgarr_08_external_erp_bridge.md` - STL Ledger Master, External GL Master, Export Only modes
- `ledgarr_09_workflows_events_apis.md` - API groups, workflows, events, and worker responsibilities
- `ledgarr_10_financial_legal_entities_vs_governing_bodies.md` - LedgArr FinancialLegalEntity vs Compliance Core GoverningBody

Authority:

The ownership constitution remains authoritative. These LedgArr docs are the product-specific interpretation for finance. When older notes say external finance owns invoices, bills, payments, tax, GL, AP, AR, or close, treat those notes as stale unless they specifically refer to external system status after LedgArr export or a tenant-selected External GL Master mode.
