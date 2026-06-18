# STL Compliance Platform Constitutions Index

These constitutions extend the existing STL Compliance ownership, shared shell, dashboard, create-view, and detail-view rules.

They are platform-level rules. Product-specific constitutions may add detail, but they may not violate these platform rules unless the ownership constitution is intentionally revised.

## Recommended adoption order

1. `platform-api-integration-constitution.md`
2. `platform-events-handoffs-readmodels-constitution.md`
3. `platform-security-tenancy-authority-constitution.md`
4. `platform-reference-snapshot-mirror-constitution.md`
5. `platform-record-lifecycle-status-constitution.md`
6. `platform-product-key-naming-constitution.md`
7. `platform-audit-evidence-retention-constitution.md`
8. `platform-reference-data-ingestion-constitution.md`
9. `platform-list-board-queue-constitution.md`
10. `platform-notifications-tasks-inbox-constitution.md`
11. `platform-external-systems-integration-constitution.md`
12. `platform-reporting-metrics-provenance-constitution.md`
13. `platform-mobile-offline-capture-sync-constitution.md`
14. `platform-settings-admin-configuration-constitution.md`
15. `platform-error-degraded-state-constitution.md`
16. `platform-workflow-approval-assignment-escalation-constitution.md`
17. `platform-accessibility-time-localization-human-factors-constitution.md`
18. `platform-contract-testing-release-constitution.md`

## Non-negotiable alignment rules

- One owner per business truth.
- No cross-database foreign keys.
- NexArr is the login, tenant, entitlement, launch, and service-identity authority.
- StaffArr owns internal people and internal places.
- ReferenceDataCore owns shared public identifiers, taxonomies, units of measure, manufacturer identity, and crosswalks.
- CustomArr owns customer truth.
- OrdArr owns order and request orchestration.
- Products own execution in their lane.
- Compliance Core owns rule meaning and evidence requirements.
- RecordArr owns stored records, files, retention, and controlled document lifecycle.
- ReportArr reports and analyzes; it does not correct source records.
- Field Companion is a mobile execution surface, not a source-of-truth product.
- STL owns finance through LedgArr and integrates with external accounting/ERP systems as bridge/export targets; payroll, banking, certified hardware, and specialized systems remain external unless STL explicitly builds a replacement product.

## How to use these files

Place these files beside the existing constitution markdown files. When implementing a feature, codex/agents should first identify:

1. Which product owns the business truth.
2. Which platform constitution applies.
3. Which product constitution applies.
4. Which API, event, handoff, reference, security, lifecycle, evidence, reporting, and UI rules are affected.
5. Which tests or contract checks prove alignment.
