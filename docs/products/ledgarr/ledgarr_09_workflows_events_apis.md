# LedgArr 09 - Workflows, Events, and APIs

LedgArr APIs live under `/api/v1/ledgarr`. Integration APIs live under `/api/v1/integrations/ledgarr`.

Core API groups:

- dashboard
- financial legal entities
- fiscal calendars and periods
- chart of accounts and GL accounts
- dimensions and mappings
- posting rules and posting preview
- financial packets
- journals and reversals
- AP vendor bills, matching, approvals, payment runs, export, and aging
- AR customer invoices, credit memos, payments, statements, and aging
- inventory valuation
- fixed assets and depreciation
- projects/job costing
- budgets and checks
- tax accounting
- reports
- external finance systems and posting batches

Primary workflow chain:

1. Operational product emits financial packet.
2. LedgArr ingests packet idempotently.
3. LedgArr validates source refs, FinancialLegalEntity, fiscal period, dimensions, tax, and account mappings.
4. LedgArr maps packet to posting preview.
5. Approval policy either auto-approves or requires manual approval.
6. LedgArr posts balanced journal/subledger entries.
7. LedgArr emits domain events and updates reports.
8. External bridge exports only approved posted batches when configured.

Event families are documented in `docs/platform/v2/event-catalog-and-consumer-matrix.md` and use the `ledgarr.*` prefix.

Background worker responsibilities:

- packet validation
- packet mapping
- Financial Legal Entity resolution
- posting preview generation
- auto-post eligible packet processing
- AP and AR aging snapshots
- inventory valuation reconciliation
- depreciation runs
- budget actual snapshots
- external export retries
- period close validation
