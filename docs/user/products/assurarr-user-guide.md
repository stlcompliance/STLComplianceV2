# AssurArr User Guide

## What This Product Is For
AssurArr is for quality assurance, nonconformances, quality holds, containment, disposition, CAPA, audits, findings, supplier quality issues, customer complaint quality cases, releases, quality status snapshots, and scorecards.

## Who Uses It
- quality managers
- quality reviewers
- quality technicians
- supplier quality managers
- customer quality managers
- quality auditors

## Main Pages
- Quality control center
- Nonconformances
- Holds
- CAPA
- Audits
- Findings
- Reviews
- Releases
- Containment
- Dispositions
- Supplier quality
- SCARs
- Complaints
- Status
- Scorecards
- History
- Settings

## Main Records
- nonconformance
- quality hold
- containment action
- disposition
- CAPA
- CAPA action
- effectiveness verification
- quality audit
- audit finding
- supplier quality issue
- supplier corrective action request
- customer complaint quality case
- quality release
- quality status snapshot
- scorecard

## Common Workflows
- create a nonconformance
- place or release a quality hold
- assign containment or disposition work
- create and close a CAPA
- run audits and verify findings
- review supplier quality issues and SCARs
- handle customer complaint quality cases
- publish quality status for other products to consume

## Permissions Usually Needed
- assurarr.nonconformances.read
- assurarr.nonconformances.create
- assurarr.nonconformances.triage
- assurarr.holds.place
- assurarr.holds.release
- assurarr.capa.create
- assurarr.capa.verify
- assurarr.audits.create
- assurarr.findings.create
- assurarr.supplier_quality.manage
- assurarr.customer_complaints.manage

## Related Products
- StaffArr owns people and internal locations.
- TrainArr owns remediation training.
- MaintainArr owns asset repair and work order execution.
- LoadArr owns inventory movement and stock ledger.
- SupplyArr owns supplier and vendor master data.
- CustomArr owns customer master data and relationship context.
- OrdArr owns order lifecycle.
- RecordArr stores retained quality evidence files.
- ReportArr reports quality trends and scorecards.

## Common Troubleshooting
- [Product not visible](../troubleshooting/product-not-visible.md)
- [Missing permission](../troubleshooting/missing-permission.md)
- If a source object is wrong, correct it in the product that owns that record.
- If evidence is missing, use RecordArr-backed capture before closing the quality decision.
- Remember: AssurArr does not own inventory balances, asset repair execution, supplier master data, customer master data, documents, training execution, or financial work.

