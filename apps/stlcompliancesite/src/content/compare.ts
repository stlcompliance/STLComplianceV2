export type AlternativeScenario = {
  id: string
  title: string
  whenItFits: string
  limitations: string
}

export type ComparisonDimension = {
  id: string
  dimension: string
  spreadsheets: string
  pointTools: string
  stlSuite: string
}

/** Public comparison narrative — not a competitive teardown or feature checklist. */
export const COMPARE_DISCLAIMER =
  'Marketing perspective only. Your organization may legitimately use spreadsheets, point tools, or a mix today. This page explains how the STL Compliance suite differs in architecture and ownership — not which tool “wins” every scenario.'

export const COMPARE_LEAD =
  'Spreadsheets and single-domain tools solve real problems. The suite targets operators who need separate products with clear boundaries, server-enforced permissions, and cross-product integration without shared database foreign keys.'

export const ALTERNATIVE_SCENARIOS: AlternativeScenario[] = [
  {
    id: 'spreadsheets',
    title: 'Spreadsheets and shared drives',
    whenItFits:
      'Small teams tracking a narrow checklist, one-off audits, or early pilots where change control and multi-user concurrency are manageable.',
    limitations:
      'Version drift, weak audit trails, formula errors, and permission models that do not map to operational roles. Cross-domain workflows (training proof → readiness → dispatch) become manual reconciliation.',
  },
  {
    id: 'point-tools',
    title: 'Point tools (single-domain SaaS or modules)',
    whenItFits:
      'A focused need — LMS-only training, CMMS-only maintenance, or TMS-only dispatch — where integration with adjacent domains is optional or handled by exports.',
    limitations:
      'Duplicate person/asset/vendor records, conflicting “sources of truth,” and integration glue that breaks when vendors change APIs. Compliance authority often becomes a spreadsheet overlay.',
  },
]

export const COMPARISON_DIMENSIONS: ComparisonDimension[] = [
  {
    id: 'boundaries',
    dimension: 'Product ownership',
    spreadsheets: 'One workbook spans everything; boundaries are informal.',
    pointTools: 'Each vendor owns its domain; your team reconciles overlaps.',
    stlSuite:
      'Each Arr product owns its PostgreSQL database and APIs. Cross-product links use APIs, events, and rebuildable mirrors — not shared foreign keys.',
  },
  {
    id: 'authority',
    dimension: 'Access and licensing',
    spreadsheets: 'File share permissions; no tenant-scoped product entitlements.',
    pointTools: 'Per-vendor accounts and roles; suite-wide launch is custom.',
    stlSuite:
      'NexArr enforces login, tenant context, entitlements, and launch. Operational products do not grant access on their own.',
  },
  {
    id: 'compliance',
    dimension: 'Compliance authority',
    spreadsheets: 'Rules live in cells, macros, or side documents.',
    pointTools: 'Often module-specific checklists; suite-wide keys are duplicated.',
    stlSuite:
      'Compliance Core supplies vocabulary, keys, mappings, and evaluation context. Operational products own facts, workflow actions, and audited overrides.',
  },
  {
    id: 'readiness',
    dimension: 'Workforce readiness',
    spreadsheets: 'Manual status columns updated after email or paper signoff.',
    pointTools: 'Training LMS may not publish proof to HR/readiness systems.',
    stlSuite:
      'TrainArr owns training proof; StaffArr owns readiness and certifications via publication APIs — not duplicated training truth in HR tables.',
  },
  {
    id: 'operations',
    dimension: 'Field and operations execution',
    spreadsheets: 'Dispatch, receiving, and work orders in separate tabs or files.',
    pointTools: 'Strong in one lane; weak handoffs to training, supply, or compliance.',
    stlSuite:
      'MaintainArr, RoutArr, SupplyArr, and Companion each own their workflows with companion deep links into entitled product surfaces.',
  },
  {
    id: 'audit',
    dimension: 'Audit and evidence',
    spreadsheets: 'Hard to prove who changed what, when, and under which rule version.',
    pointTools: 'Varies by vendor; cross-product audit packages are manual assembly.',
    stlSuite:
      'Per-product audit logging and Compliance Core audit export patterns — evaluated in entitled product UIs, not on this marketing site.',
  },
]

export const SUITE_HONESTY_NOTES: string[] = [
  'V1 ships real APIs, workers, databases, and authenticated product UIs — not mock-only marketing screens.',
  'Public maturity labels on the products hub describe what is operational today; not every backlog item is complete.',
  'This site does not store tenant data, change entitlements, or run product workflows.',
]
