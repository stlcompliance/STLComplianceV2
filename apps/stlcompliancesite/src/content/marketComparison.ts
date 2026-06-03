export type UsualStackRow = {
  need: string
  product: string
  solves: string
  gap: string
}

export type FeatureChecklistRow = {
  capability: string
  wms: string
  cmms: string
  lms: string
  wfm: string
  tms: string
  grc: string
  stl: string
}

export type CategoryComparison = {
  id: string
  title: string
  pointToolsGreatAt: string
  stlAdds: string[]
  takeaway: string
}

export type ProductStackRow = {
  product: string
  complements: string
  primaryJob: string
  positioning: string
}

export type Objection = {
  title: string
  body: string
  answer: string
}

export const USUAL_STACK_ROWS: UsualStackRow[] = [
  {
    need: 'Warehouse operations',
    product: 'WMS',
    solves: 'Receiving, picking, put-away, cycle counts, inventory movement',
    gap: 'Worker qualification, equipment readiness, training evidence, regulatory proof',
  },
  {
    need: 'Maintenance',
    product: 'CMMS / EAM',
    solves: 'Assets, inspections, PMs, repairs, work orders',
    gap: 'Training-gated assignment, compliance rule mapping, route impact, parts approval',
  },
  {
    need: 'Training',
    product: 'LMS',
    solves: 'Courses, completions, learning paths, certificates',
    gap: 'Whether training actually controls work eligibility',
  },
  {
    need: 'Workforce',
    product: 'WFM / HCM',
    solves: 'Scheduling, labor, time, attendance, staffing',
    gap: 'Whether the scheduled person is operationally qualified and compliant',
  },
  {
    need: 'Dispatch',
    product: 'TMS / fleet system',
    solves: 'Routes, vehicles, drivers, ETAs, exceptions',
    gap: 'Whether people, vehicles, inspections, docs, and evidence are all ready',
  },
  {
    need: 'Compliance',
    product: 'GRC / document system',
    solves: 'Policies, controls, audits, requirements',
    gap: 'Whether compliance is enforced inside daily work',
  },
  {
    need: 'Identity',
    product: 'IAM / SSO',
    solves: 'Login, roles, access, authentication',
    gap: 'Product-specific readiness, qualifications, operational permissions',
  },
]

export const FEATURE_CHECKLIST_ROWS: FeatureChecklistRow[] = [
  {
    capability: 'One platform login across products',
    wms: 'Partial',
    cmms: 'Partial',
    lms: 'Partial',
    wfm: 'Partial',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Tenant and product entitlement control',
    wms: 'Partial',
    cmms: 'Partial',
    lms: 'Partial',
    wfm: 'Partial',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Central person record',
    wms: 'No',
    cmms: 'Partial',
    lms: 'Partial',
    wfm: 'Yes',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Training programs and signoffs',
    wms: 'No',
    cmms: 'Partial',
    lms: 'Yes',
    wfm: 'Partial',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Qualification controls work eligibility',
    wms: 'Rare',
    cmms: 'Rare',
    lms: 'Partial',
    wfm: 'Partial',
    tms: 'Rare',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Asset records and inspections',
    wms: 'Partial',
    cmms: 'Yes',
    lms: 'No',
    wfm: 'No',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'PMs, defects, work orders, repairs',
    wms: 'No',
    cmms: 'Yes',
    lms: 'No',
    wfm: 'No',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Route and dispatch execution',
    wms: 'Partial',
    cmms: 'No',
    lms: 'No',
    wfm: 'Partial',
    tms: 'Yes',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Inventory movement and WMS workflows',
    wms: 'Yes',
    cmms: 'Partial',
    lms: 'No',
    wfm: 'No',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Vendor, customer, procurement records',
    wms: 'Partial',
    cmms: 'Partial',
    lms: 'No',
    wfm: 'No',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Rule packs and evidence mapping',
    wms: 'Partial',
    cmms: 'Partial',
    lms: 'Partial',
    wfm: 'Partial',
    tms: 'Partial',
    grc: 'Yes',
    stl: 'Yes',
  },
  {
    capability: 'Audit package by person/site/asset/event',
    wms: 'Partial',
    cmms: 'Partial',
    lms: 'Partial',
    wfm: 'Partial',
    tms: 'Partial',
    grc: 'Yes',
    stl: 'Yes',
  },
  {
    capability: 'Incident-to-retraining workflow',
    wms: 'Rare',
    cmms: 'Partial',
    lms: 'Partial',
    wfm: 'Partial',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Assignment blocked by missing qualification',
    wms: 'Rare',
    cmms: 'Partial',
    lms: 'Partial',
    wfm: 'Partial',
    tms: 'Rare',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Asset readiness affects dispatch/work',
    wms: 'Partial',
    cmms: 'Partial',
    lms: 'No',
    wfm: 'No',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Parts demand tied to work orders',
    wms: 'Partial',
    cmms: 'Partial',
    lms: 'No',
    wfm: 'No',
    tms: 'No',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Evidence captured during work',
    wms: 'Partial',
    cmms: 'Partial',
    lms: 'Partial',
    wfm: 'Partial',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
  {
    capability: 'Compliance built into execution',
    wms: 'Partial',
    cmms: 'Partial',
    lms: 'Partial',
    wfm: 'Partial',
    tms: 'Partial',
    grc: 'Partial',
    stl: 'Yes',
  },
]

export const CATEGORY_COMPARISONS: CategoryComparison[] = [
  {
    id: 'wms',
    title: 'STL Compliance vs WMS',
    pointToolsGreatAt:
      'Inventory control, put-away, picking, replenishment, cycle counts, warehouse labor, slotting, automation, and throughput.',
    stlAdds: [
      'Whether the worker is qualified.',
      'Whether equipment is safe.',
      'Whether training is current.',
      'Whether the movement has a compliance reason.',
      'Whether the evidence can be produced later.',
    ],
    takeaway:
      'A WMS tells you where the inventory moved. STL tells you whether the movement was allowed, who did it, why it happened, and whether the supporting evidence exists.',
  },
  {
    id: 'cmms',
    title: 'STL Compliance vs CMMS / EAM',
    pointToolsGreatAt:
      'Assets, preventive maintenance, work orders, inspections, downtime, repairs, technician notes, and maintenance history.',
    stlAdds: [
      'Training-gated maintenance assignment.',
      'StaffArr person history.',
      'TrainArr qualification evidence.',
      'SupplyArr parts and vendor context.',
      'LoadArr inventory movement.',
      'Compliance Core rule mapping.',
      'RoutArr readiness impact.',
    ],
    takeaway:
      'A CMMS tells you the asset was repaired. STL tells you whether the right person repaired it, whether the part was controlled, whether the defect affected operations, and whether the repair satisfies the rule.',
  },
  {
    id: 'lms',
    title: 'STL Compliance vs LMS',
    pointToolsGreatAt:
      'Courses, completions, certificates, learning paths, content delivery, quizzes, and compliance training reports.',
    stlAdds: [
      'Training as operational authorization.',
      'Certificates that become work eligibility conditions.',
      'Signoffs, evaluations, remediation, retraining, and qualification history tied to operations.',
    ],
    takeaway: 'An LMS says training is complete. STL decides whether the person is cleared to work.',
  },
  {
    id: 'wfm',
    title: 'STL Compliance vs WFM / HCM',
    pointToolsGreatAt:
      'Scheduling, time, attendance, shift coverage, labor cost, overtime, availability, and staffing rules.',
    stlAdds: [
      'Operational readiness behind the schedule.',
      'Qualification and permission checks.',
      'Asset, route, evidence, and compliance dependencies around the assignment.',
    ],
    takeaway: 'WFM schedules people. STL verifies whether the scheduled work should happen.',
  },
  {
    id: 'tms',
    title: 'STL Compliance vs TMS / Fleet / Dispatch',
    pointToolsGreatAt:
      'Routes, dispatch, ETA, vehicle location, driver app workflows, optimization, telematics, HOS, and exceptions.',
    stlAdds: [
      'Pre-dispatch readiness.',
      'Driver qualification and site fit.',
      'Vehicle inspection and defect closure.',
      'Load, stock, documentation, and route evidence checks.',
    ],
    takeaway: 'A TMS moves the route. STL clears the route.',
  },
  {
    id: 'grc',
    title: 'STL Compliance vs GRC',
    pointToolsGreatAt:
      'Policies, controls, audits, risk registers, documents, regulatory references, and compliance tasks.',
    stlAdds: [
      'Execution-level enforcement.',
      'Rules applied when people are assigned, assets are inspected, parts are consumed, routes are dispatched, or incidents are reported.',
      'Evidence expectations connected to daily workflows.',
    ],
    takeaway: 'GRC documents the requirement. STL applies the requirement to the work.',
  },
]

export const CAN_WORK_START_ITEMS = [
  'Person exists',
  'Person is active',
  'Person belongs to the tenant/site',
  'Person has required permissions',
  'Person has required qualifications',
  'Training is complete and current',
  'Asset is available',
  'Asset is inspected',
  'Defects are resolved or dispositioned',
  'Required parts/materials are available',
  'Vendor/customer records are valid where applicable',
  'Route or work assignment is allowed',
  'Compliance Core requirements are satisfied or flagged',
  'Evidence is captured automatically',
  'Audit history is preserved',
] as const

export const PRODUCT_STACK_ROWS: ProductStackRow[] = [
  {
    product: 'StaffArr',
    complements: 'WFM, HR admin, org charts',
    primaryJob: 'People, sites, permissions, incidents',
    positioning: 'The people and readiness source of truth',
  },
  {
    product: 'TrainArr',
    complements: 'LMS, certification trackers',
    primaryJob: 'Training, signoffs, qualifications',
    positioning: 'Training that actually controls work',
  },
  {
    product: 'MaintainArr',
    complements: 'CMMS / EAM',
    primaryJob: 'Assets, PMs, inspections, repairs',
    positioning: 'Maintenance tied to readiness and evidence',
  },
  {
    product: 'RoutArr',
    complements: 'TMS / dispatch',
    primaryJob: 'Routes, drivers, vehicles, execution',
    positioning: 'Dispatch with qualification and asset checks',
  },
  {
    product: 'SupplyArr',
    complements: 'Procurement / vendor systems',
    primaryJob: 'Vendors, customers, parts, approvals',
    positioning: 'Supply records tied to operational work',
  },
  {
    product: 'LoadArr',
    complements: 'WMS',
    primaryJob: 'Inventory, locations, movement, picks',
    positioning: 'WMS connected to people, sites, and compliance',
  },
  {
    product: 'Compliance Core',
    complements: 'GRC / rule libraries',
    primaryJob: 'Rule packs, evidence, citations',
    positioning: 'Compliance logic that products can enforce',
  },
]

export const OBJECTIONS: Objection[] = [
  {
    title: 'We already have a WMS.',
    body:
      'Great. STL does not have to replace it on day one. But your WMS probably does not own training qualifications, incident history, maintenance readiness, regulatory rule packs, or cross-product audit packaging.',
    answer:
      'STL can either replace lightweight WMS needs through LoadArr or sit around an existing WMS as the readiness and compliance layer.',
  },
  {
    title: 'We already have a CMMS.',
    body:
      'Good. That means you already understand asset discipline. The question is whether your CMMS knows whether the technician was qualified, whether retraining was required after an incident, whether the part came from an approved source, whether the route was blocked because of the defect, and whether the evidence satisfies the rule.',
    answer:
      'MaintainArr is strongest when maintenance readiness needs to connect to the rest of operations.',
  },
  {
    title: 'We already have an LMS.',
    body: 'That is fine. But course completion is not the same as work authorization.',
    answer:
      'TrainArr is built around qualifications, signoffs, evaluations, remediation, and publishing work eligibility back into the operational record.',
  },
  {
    title: 'We already have workforce scheduling.',
    body: 'Scheduling someone is not the same as clearing them.',
    answer:
      'StaffArr and TrainArr make sure the person is not just available, but appropriate, qualified, assigned, permitted, and documented.',
  },
  {
    title: 'We already have compliance software.',
    body: 'Most compliance software stores requirements. STL Compliance applies requirements.',
    answer:
      'Compliance Core turns rules, evidence expectations, exceptions, and citations into logic the rest of the suite can actually use.',
  },
]
