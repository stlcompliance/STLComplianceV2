# LedgArr User Guide

## What This Product Is For

LedgArr is the finance ERP spine for STL Compliance. It is for finance teams that need STL Compliance to post, review, close, reconcile, and report financial activity without turning operational products into accidental accounting systems. It owns Financial Legal Entities, fiscal periods, chart of accounts, dimensions, financial packet inboxes, posting previews, journals, AP, AR, cash and bank control, inventory valuation, fixed assets, budgets, payroll financials, tax accounting, intercompany, consolidation, and external ERP/accounting bridge batches.

LedgArr is not where users create customers, vendors, orders, trips, warehouse movements, assets, quality holds, compliance governing bodies, citations, or documents. Those records stay in their owning products and flow to LedgArr through packets, references, and mapped snapshots.

## Who Uses It

- controllers
- accountants
- AP and AR specialists
- finance approvers
- finance operations managers
- auditors and authorized integration administrators

## Main Pages

- Dashboard
- Legal Entities
- General Ledger
- Payables
- Receivables
- Cash & Bank
- Budgets
- Cost Accounting
- Projects & Jobs
- Fixed Assets
- Payroll Financials
- Taxes
- Intercompany
- Consolidation
- Close
- Reports
- Settings

## Common Workflows

### Review and post a packet

1. Open Financial Packet Inbox.
2. Filter for received, needs mapping, or preview ready packets.
3. Resolve Financial Legal Entity, account, tax, and dimension mapping.
4. Review posting preview.
5. Approve when the preview is balanced and supported.
6. Post to create immutable journal/subledger entries.

### Close a period

1. Open Fiscal Periods.
2. Review unposted packets, AP/AR aging, reconciliation exceptions, open approvals, and linked RecordArr support references.
3. Close the period when normal posting should stop.
4. Lock the period after final review when all posting should stop except controlled reopening.

### Export to external ERP/accounting

1. Confirm journals are posted.
2. Create an external posting batch.
3. Review mappings and export status.
4. Send the export.
5. Resolve sync issues in LedgArr.

## Boundary Reminders

- Financial Legal Entity means a tenant-owned accounting/reporting entity.
- Compliance Core GoverningBody means a regulator or standards authority.
- LedgArr must not create or own GoverningBody records.
- RecordArr stores files and evidence; LedgArr stores document references.
- General Ledger now includes LedgArr approval matrix, segregation-of-duties rules, journal support references, and immutable journal history.
- Close includes RecordArr-backed evidence references and recent immutable finance events for signoff review.
- StaffArr owns people and canonical internal locations; LedgArr may mirror location references only as finance dimensions.
- LoadArr owns operational inventory quantity; LedgArr owns financial valuation.
- MaintainArr owns physical assets; LedgArr owns financial book value and depreciation.
- StaffArr owns worker identity and approved time; LedgArr owns payroll export, labor costing, and payroll journal settlement.
