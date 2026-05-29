/**
 * Canonical product ownership copy — kept in sync with docs/02_PRODUCT_OWNERSHIP_BOUNDARIES.md.
 * Marketing pages import from here so public language matches the suite boundary matrix.
 */
export const OWNERSHIP_SOURCE_DOC = 'docs/02_PRODUCT_OWNERSHIP_BOUNDARIES.md'

export type ProductOwnershipCopy = {
  owns: string
  doesNotOwn: string
}

export const PRODUCT_OWNERSHIP: Record<string, ProductOwnershipCopy> = {
  nexarr: {
    owns:
      'Platform identity, login, tenants, entitlements, licensing, service clients, service tokens, platform admin, and launch.',
    doesNotOwn:
      'Operations, people records, maintenance, training, routing, purchasing, or compliance rule content.',
  },
  staffarr: {
    owns:
      'People, org, roles, permissions, certifications, readiness, incidents, and personnel history.',
    doesNotOwn:
      'Login, entitlement, training workflow evidence, maintenance, routing, procurement, or rule packs.',
  },
  trainarr: {
    owns:
      'Programs, versions, requirements, assignments, evidence, tests, evaluations, signoffs, completions, retraining, and training-derived qualifications.',
    doesNotOwn:
      'People/org truth, platform login, maintenance, routing, procurement, or compliance rule packs.',
  },
  maintainarr: {
    owns:
      'Assets, inspections, defects, work orders, PM, maintenance history, labor, part-consumption snapshots, and asset readiness.',
    doesNotOwn: 'People, training, dispatch, full procurement, vendors, or rule packs.',
  },
  routarr: {
    owns:
      'Route planning, trip execution, dispatch, driver assignment, vehicle references, DVIR, proof, exceptions, and route history.',
    doesNotOwn: 'People, training, asset maintenance, procurement, or rule packs.',
  },
  supplyarr: {
    owns:
      'Vendors, suppliers, parts, catalogs, inventory, purchase requests, purchase orders, receiving, pricing, and lead times.',
    doesNotOwn:
      'Login, people, maintenance execution, training records, dispatch records, or rule packs.',
  },
  compliancecore: {
    owns:
      'Controlled vocabulary, compliance keys, material keys, rule packs, regulatory mappings, SDS/HazCom references, source metadata, and evaluation patterns.',
    doesNotOwn:
      'Tenant operations, people, assets, work orders, routes, purchase orders, or training workflow records.',
  },
  companion: {
    owns:
      'Aggregated field inbox presentation and mobile-oriented task navigation (links into entitled products).',
    doesNotOwn:
      'Business authority, tenant data, or product workflow state (each product API remains authoritative).',
  },
}

export const COMPLIANCE_CORE_EDUCATION = {
  headline: 'Authority layer — not an operational workflow product',
  lead:
    'Compliance Core supplies rule context, controlled vocabulary, and evaluation patterns. Operational Arr products own facts, workflow actions, and permitted overrides with audit.',
  bullets: [
    'Normal tenant users consume compliance results through entitled product APIs — not unrestricted rule authoring.',
    'Products publish operational facts; Compliance Core maps keys, rule packs, and reason codes for evaluation.',
    'SDS/HazCom references and regulatory mappings live here; work orders, routes, and training assignments do not.',
  ],
} as const

export const DOCS_11_ACCEPTANCE_NOTE =
  'V1 operational labels mean each product ships a real API, PostgreSQL database, worker, and authenticated UI in render.yaml. docs/11 acceptance still requires 100% FEATURESET implementation — per-product gap canvases track remaining doc-true items.'
