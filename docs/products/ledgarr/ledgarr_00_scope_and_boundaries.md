# LedgArr 00 - Scope and Boundaries

LedgArr is the STL Compliance financial system of record. Its machine key is `ledgarr`, and its display name is LedgArr.

LedgArr owns financial truth only:

- Financial Legal Entities for accounting and reporting
- fiscal calendars, fiscal years, fiscal periods, close, reopen, and lock state
- chart of accounts, GL accounts, dimensions, mappings, and posting rules
- financial packets, posting previews, journals, reversals, subledgers, and audit trail
- AP, AR, inventory valuation, fixed asset accounting, project/job costing, budgets, tax accounting, and external ERP/accounting bridge records
- financial statements, trial balance, aging, valuation, budget, and external export reports

LedgArr does not own operational or regulatory truth:

- NexArr owns login, tenant entitlement, launch, platform admins, and service-client authority.
- StaffArr owns people, roles, permissions assignment context, org units, departments, and internal locations.
- CustomArr owns customers and customer relationship truth.
- SupplyArr owns vendors, suppliers, items, procurement intent, purchase orders, and vendor-facing workflows.
- OrdArr owns orders, requests, orchestration, completion packets, and order lifecycle.
- LoadArr owns WMS receiving, stock ledger, inventory movement execution, balances, picking, transfers, counts, and operational inventory state.
- MaintainArr owns physical assets, components, work orders, PMs, inspections, repairs, and asset readiness.
- RoutArr owns transportation demand, dispatch, trips, routing, carriers, freight events, detention, and accessorial operational context.
- AssurArr owns quality events, holds, inspections, dispositions, exceptions, nonconformance, and CAPA.
- RecordArr owns documents, records, retention, files, evidence packages, and controlled document lifecycle.
- Compliance Core owns governing bodies, regulators, citations, rulepacks, regulatory vocabulary, compliance determinations, and rule normalization.
- ReportArr may compose financial reporting views but must not recalculate ledger truth independently.

Cross-product references into LedgArr must use `productKey`, `sourceRecordType`, `sourceRecordId`, `sourceEventId`, `sourceVersion`, `tenantId`, and immutable source snapshots. LedgArr must not introduce direct cross-product database foreign keys.

Normal LedgArr UI must not expose raw JSON or internal IDs except in authorized admin, troubleshooting, audit, and integration surfaces. Selectors and mapped read models should be used when users reference canonical records from other products.
