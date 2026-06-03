export type AlternativeScenario = {
  id: string
  title: string
  whenItFits: string
  limitations: string
}

export type ComparisonDimension = {
  id: string
  checklistItem: string
  spreadsheets: string
  pointTools: string
  stlSuite: string
  stlAnswer: 'Native' | 'Connected' | 'Manual outside suite'
}

export const COMPARE_DISCLAIMER =
  'Your organization may legitimately use spreadsheets, point tools, or a mix today. This page explains where STL Compliance helps when those tools start creating gaps, duplicate work, and audit scramble.'

export const COMPARE_LEAD =
  'Spreadsheets and single-purpose tools can solve real problems. STL Compliance is for teams that need people, training, assets, dispatch, supply, and proof to stay connected as the work happens.'

export const ALTERNATIVE_SCENARIOS: AlternativeScenario[] = [
  {
    id: 'spreadsheets',
    title: 'Spreadsheets and shared drives',
    whenItFits:
      'Small teams tracking a narrow checklist, one-off audits, or early pilots where only a few people update the file.',
    limitations:
      'Files drift, formulas break, approvals are hard to prove, and handoffs between training, readiness, maintenance, and dispatch become manual work.',
  },
  {
    id: 'point-tools',
    title: 'Point tools',
    whenItFits:
      'A focused need such as training-only, maintenance-only, or dispatch-only where handoffs to other teams are simple.',
    limitations:
      'Teams often re-enter the same people, assets, vendors, or evidence in multiple places, then rebuild the story during audits.',
  },
]

export const COMPARISON_DIMENSIONS: ComparisonDimension[] = [
  {
    id: 'records',
    checklistItem: 'Can the system connect people, assets, training, supply, dispatch, and proof without re-keying the same facts?',
    spreadsheets: 'Usually manual. One workbook can hold many columns, but ownership and change history are informal.',
    pointTools: 'Partial. Each tool tracks its own slice; cross-tool reconciliation becomes your team’s job.',
    stlSuite:
      'Connected. Each product owns its records while suite links keep the important people, asset, training, supply, dispatch, and proof relationships visible.',
    stlAnswer: 'Connected',
  },
  {
    id: 'access',
    checklistItem: 'Can users sign in once and reach only the products and records they are allowed to use?',
    spreadsheets: 'Manual. File share access is easy to lose track of as teams, folders, and copies multiply.',
    pointTools: 'Partial. Strong tools have roles, but each product often has its own account and permission model.',
    stlSuite: 'NexArr gives customers one secure place to sign in and reach the products they use.',
    stlAnswer: 'Native',
  },
  {
    id: 'compliance',
    checklistItem: 'Can rules, citations, evidence expectations, and product records be evaluated together?',
    spreadsheets: 'Manual. Rules live in cells, macros, side documents, or individual reviewer knowledge.',
    pointTools: 'Partial. A point tool may validate its own workflow but rarely sees the full operational chain.',
    stlSuite:
      'Compliance Core connects rules and evidence expectations to the work captured across the suite.',
    stlAnswer: 'Native',
  },
  {
    id: 'readiness',
    checklistItem: 'Can a supervisor see whether a person is qualified before assigning real work?',
    spreadsheets: 'Manual. Status columns are updated after email, paper signoff, or a separate training export.',
    pointTools: 'Partial. Training tools may prove course completion without linking that proof to assignment decisions.',
    stlSuite: 'TrainArr captures training proof, and StaffArr turns that proof into readiness.',
    stlAnswer: 'Connected',
  },
  {
    id: 'asset-readiness',
    checklistItem: 'Can asset condition, inspection status, defects, work orders, and repair proof affect operational release?',
    spreadsheets: 'Manual. Inspection and repair status often sit in separate sheets, forms, or messages.',
    pointTools: 'Partial. CMMS tools go deep on maintenance, but handoffs to dispatch, training, and compliance still need integration.',
    stlSuite:
      'MaintainArr manages assets, inspections, defects, work orders, repairs, and asset readiness while sharing context with dispatch, supply, and compliance workflows.',
    stlAnswer: 'Native',
  },
  {
    id: 'route-readiness',
    checklistItem: 'Can dispatch see whether the worker, vehicle, inspection state, documentation, and exceptions support a trip?',
    spreadsheets: 'Manual. Dispatchers often check multiple files, messages, or systems before releasing work.',
    pointTools: 'Partial. TMS and telematics tools are strong for route execution but may not own training, maintenance, or audit evidence.',
    stlSuite:
      'RoutArr manages routes, drivers, vehicles, trip status, exceptions, and dispatch proof while consuming readiness signals from StaffArr, TrainArr, MaintainArr, and Compliance Core.',
    stlAnswer: 'Connected',
  },
  {
    id: 'supply-warehouse',
    checklistItem: 'Can purchasing, supplier restrictions, receiving, inventory movement, and stock proof stay tied to operations?',
    spreadsheets: 'Manual. Purchase requests, receiving, and inventory changes are easy to split across files.',
    pointTools: 'Partial. Procurement or WMS tools are strong in their own lane but usually need integration to maintenance, route, and compliance decisions.',
    stlSuite:
      'SupplyArr manages vendors, purchasing, approvals, receiving, and supply records; LoadArr manages warehouse movement, reservations, picking, counts, and inventory history.',
    stlAnswer: 'Connected',
  },
  {
    id: 'audit',
    checklistItem: 'Can audit evidence be exported from actual workflow history instead of rebuilt later?',
    spreadsheets: 'Manual. It is hard to prove who changed what, when, and why across copied files.',
    pointTools: 'Partial. Vendor exports can help, but cross-team audit packages are often assembled by hand.',
    stlSuite:
      'Actions, approvals, changes, and evidence are captured inside the workflows instead of reconstructed later.',
    stlAnswer: 'Native',
  },
]

export const SUITE_HONESTY_NOTES: string[] = [
  'STL Compliance connects frontline identity, qualifications, maintenance readiness, route readiness, inventory status, procurement evidence, and audit packaging in one operating model.',
  'The suite is strongest where operational decisions depend on multiple teams and records, not one departmental workflow.',
  'The value is the readiness decision: who can work, what equipment can run, what supply is available, what route is safe to release, and what proof exists if anyone asks.',
]
