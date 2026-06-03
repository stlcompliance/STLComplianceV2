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
    dimension: 'Connected records',
    spreadsheets: 'One workbook spans everything; boundaries are informal.',
    pointTools: 'Each tool tracks its own slice; your team reconciles overlaps.',
    stlSuite:
      'Each product focuses on its part of the work, while the suite keeps the important people, asset, training, supply, and proof connections visible.',
  },
  {
    id: 'access',
    dimension: 'Access',
    spreadsheets: 'File share access is easy to lose track of.',
    pointTools: 'Separate accounts and roles in each tool.',
    stlSuite: 'NexArr gives customers one secure place to sign in and reach the products they use.',
  },
  {
    id: 'compliance',
    dimension: 'Compliance proof',
    spreadsheets: 'Rules live in cells, macros, or side documents.',
    pointTools: 'Often limited to the checklist inside one tool.',
    stlSuite:
      'Compliance Core connects rules and evidence expectations to the work captured across the suite.',
  },
  {
    id: 'readiness',
    dimension: 'Workforce readiness',
    spreadsheets: 'Manual status columns updated after email or paper signoff.',
    pointTools: 'Training tools may not tell supervisors who is ready for real work.',
    stlSuite: 'TrainArr captures training proof, and StaffArr turns that proof into readiness.',
  },
  {
    id: 'operations',
    dimension: 'Daily operations',
    spreadsheets: 'Dispatch, receiving, and work orders live in separate tabs or files.',
    pointTools: 'Strong in one lane; weak handoffs to training, supply, or compliance.',
    stlSuite:
      'MaintainArr, RoutArr, SupplyArr, LoadArr, and Companion keep daily work moving while preserving the proof behind it.',
  },
  {
    id: 'audit',
    dimension: 'Audits and evidence',
    spreadsheets: 'Hard to prove who changed what, when, and why.',
    pointTools: 'Varies by vendor; cross-team audit packages are often assembled by hand.',
    stlSuite:
      'Actions, approvals, changes, and evidence are captured inside the workflows instead of reconstructed later.',
  },
]

export const SUITE_HONESTY_NOTES: string[] = [
  'STL Compliance connects frontline identity, qualifications, maintenance readiness, route readiness, inventory status, procurement evidence, and audit packaging in one operating model.',
  'The suite is strongest where operational decisions depend on multiple teams and records, not one departmental workflow.',
  'This public site explains the platform. Real customer work happens after secure sign-in.',
]
